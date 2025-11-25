"""
cog for handling song requests.
"""

import asyncio
import logging
from typing import Dict, List, Optional
import aiohttp
import discord
from discord import app_commands, ui
from discord.ext import commands

import config as config_module

logger = logging.getLogger(__name__)

class SongSelectDropdown(ui.Select):
    """dropdown for selecting a song from search results."""
    
    def __init__(self, tracks: List[Dict]):
        options = [
            discord.SelectOption(
                label=track['title'][:100],  # limit to 100 chars (Discord limit)
                value=str(idx),
                description=f"ID: {track['id']}"
            ) for idx, track in enumerate(tracks)
        ]
        super().__init__(
            placeholder="Select a song...",
            min_values=1,
            max_values=1,
            options=options
        )
    
    async def callback(self, interaction: discord.Interaction):
        await interaction.response.defer()
        self.view.selected_index = int(self.values[0])
        self.view.stop()

class ConfirmRequestView(ui.View):
    """view for confirming a song request."""
    
    def __init__(self, track: Dict):
        super().__init__(timeout=60)
        self.track = track
        self.confirmed = False
    
    @ui.button(label="Confirm Request", style=discord.ButtonStyle.green)
    async def confirm(self, interaction: discord.Interaction, button: ui.Button):
        self.confirmed = True
        self.stop()
        await interaction.response.defer()
    
    @ui.button(label="Cancel", style=discord.ButtonStyle.red)
    async def cancel(self, interaction: discord.Interaction, button: ui.Button):
        await interaction.response.send_message("Request cancelled.", ephemeral=True)
        self.stop()

class MissingSongModal(ui.Modal, title="Missing Song"):
    """modal for submitting a missing song request."""
    
    song_name = ui.TextInput(
        label="Song Name",
        placeholder="Enter the full song name and artist",
        required=True,
        max_length=200
    )
    
    def __init__(self, original_query: str):
        super().__init__()
        self.original_query = original_query
        self.song_name.default = original_query
    
    async def on_submit(self, interaction: discord.Interaction):
        await interaction.response.defer()
        self.stop()

class RequestCog(commands.Cog):
    """cog for handling song requests."""
    
    def __init__(self, bot: commands.Bot):
        self.bot = bot
        self.session = aiohttp.ClientSession()
        logger.info("Request cog initialized")
    
    async def search_songs(self, query: str) -> List[Dict]:
        """search for songs using the RadioBoss API."""
        url = "https://c4.radioboss.fm/w/songrequestsearch"
        params = {
            "u": 560,  # station ID
            "q": query,
            "_": ""  # cache buster
        }
        
        # set headers to mimic a browser request
        headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
            'Accept': 'application/json, text/plain, */*',
            'Accept-Language': 'en-US,en;q=0.9',
            'Referer': 'https://c4.radioboss.fm/',
            'Origin': 'https://c4.radioboss.fm',
            'DNT': '1',
            'Connection': 'keep-alive'
        }
        
        try:
            async with self.session.get(url, params=params, headers=headers) as response:
                # get the response text first
                response_text = await response.text()
                
                # log the response for debugging
                logger.debug(f"Search response status: {response.status}")
                logger.debug(f"Response content type: {response.headers.get('Content-Type')}")
                logger.debug(f"Response text: {response_text[:500]}...")
                
                # try to parse as JSON
                try:
                    import json
                    data = json.loads(response_text)
                    
                    # handle different response formats
                    if isinstance(data, list):
                        return data  # already a list of tracks
                    elif isinstance(data, dict):
                        if 'tracks' in data and isinstance(data['tracks'], list):
                            return data['tracks']
                        elif 'error' in data and data['error'] is False:
                            return data.get('tracks', [])
                    
                    # if we get here, the format is unexpected
                    logger.warning(f"Unexpected JSON format in response: {data}")
                    return []
                    
                except json.JSONDecodeError as e:
                    logger.error(f"Failed to parse JSON response: {e}")
                    logger.debug(f"Response text that failed to parse: {response_text[:500]}...")
                    return []
                    
        except aiohttp.ClientError as e:
            logger.error(f"Error making request to RadioBoss API: {e}")
            return []
        except Exception as e:
            logger.error(f"Error searching for songs: {e}")
            return []
    
    async def request_song(self, track_id: int) -> bool:
        """request a song using the RadioBoss API."""
        url = "https://c4.radioboss.fm/w/songrequestmake"
        params = {
            "u": 560,  # station ID
            "id": track_id,
            "_": ""  # cache buster
        }
        
        try:
            async with self.session.get(url, params=params) as response:
                data = await response.json()
                return data.get('success', False)
        except Exception as e:
            logger.error(f"Error requesting song: {e}")
            return False
    
    def cog_unload(self):
        """clean up when the cog is unloaded."""
        asyncio.create_task(self.session.close())

async def setup(bot: commands.Bot):
    """set up the request cog."""
    await bot.add_cog(RequestCog(bot))
