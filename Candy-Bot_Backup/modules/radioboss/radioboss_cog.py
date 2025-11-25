"""
radioboss cog for interacting with the radioboss api.
"""

import logging
from typing import Optional, List, Dict, Any, Union
import aiohttp
import discord
from discord import app_commands, ui
from discord.ext import commands
import asyncio

# import config
from config import (
    RADIOBOSS_API_URL,
    RADIOBOSS_API_KEY,
    RADIOBOSS_OWNER_DM_USER_ID
)

# import the RadioBoss API client
from .radioboss_api import RadioBossAPIClient, RadioBossAPIError

logger = logging.getLogger(__name__)

class RadioBossCog(commands.Cog):
    """cog for handling radioboss related commands."""

    def __init__(self, bot: commands.Bot) -> None:
        """Initialize the radioboss cog."""
        self.bot = bot
        self.owner_id = int(RADIOBOSS_OWNER_DM_USER_ID) if RADIOBOSS_OWNER_DM_USER_ID else None
        self.radioboss = RadioBossAPIClient(
            base_url=RADIOBOSS_API_URL,
            api_key=RADIOBOSS_API_KEY,
            station_id='560',  # default station ID
            owner_id=self.owner_id
        )
        self.confirmed = False
        self.cancelled = False
        self.selected_track = None
        logger.info("radioboss cog initialized")

    async def cog_unload(self) -> None:
        """clean up resources when the cog is unloaded."""
        if hasattr(self, 'radioboss'):
            await self.radioboss.close()
        logger.info("radioboss cog unloaded")
        
    async def confirm(self, interaction: discord.Interaction):
        """Handle the confirm button click."""
        self.confirmed = True
        await interaction.response.defer()
        
    async def cancel(self, interaction: discord.Interaction):
        """Handle the cancel button click."""
        self.cancelled = True
        await interaction.response.defer()

    class SongSelectDropdown(discord.ui.Select):
        def __init__(self, tracks):
            self.tracks = tracks
            options = [
                discord.SelectOption(
                    label=track['title'][:100],  # limit label length
                    value=str(track['id']),
                    description=f"ID: {track['id']}"[:100]  # limit description length
                ) for track in tracks[:25]  # limit to 25 options (Discord limit)
            ]
            
            super().__init__(
                placeholder="Select a song...",
                min_values=1,
                max_values=1,
                options=options
            )
        
        async def callback(self, interaction: discord.Interaction):
            # store the selected song ID in the view
            self.view.selected_song_id = self.values[0]
            # Acknowledge the interaction with an ephemeral response
            await interaction.response.defer(ephemeral=True)
            # Stop the view to prevent further interactions
            self.view.stop()

    class ConfirmRequestView(discord.ui.View):
        def __init__(self, track_title: str, track_id: str):
            super().__init__(timeout=60)
            self.confirmed = False
            self.track_title = track_title
            self.track_id = track_id
        
        @discord.ui.button(label="Confirm Request", style=discord.ButtonStyle.green)
        async def confirm_button(self, interaction: discord.Interaction, button: discord.ui.Button):
            self.confirmed = True
            await interaction.response.defer()
            self.stop()
        
        @discord.ui.button(label="Cancel", style=discord.ButtonStyle.red)
        async def cancel_button(self, interaction: discord.Interaction, button: discord.ui.Button):
            await interaction.response.send_message("Request cancelled.", ephemeral=True)
            self.stop()
    
    class RequestSongButton(discord.ui.Button):
        def __init__(self, owner_id: int, original_query: str):
            super().__init__(
                label="Request This Song",
                style=discord.ButtonStyle.secondary,
                emoji="🎵"
            )
            self.owner_id = owner_id
            self.original_query = original_query
        
        async def callback(self, interaction: discord.Interaction):
            # create and show the modal
            modal = RadioBossCog.RequestSongModal(owner_id=self.owner_id, original_query=self.original_query)
            await interaction.response.send_modal(modal)
    
    class RequestSongModal(discord.ui.Modal):
        def __init__(self, owner_id: int, original_query: str):
            super().__init__(title="Request a Song")
            self.owner_id = owner_id
            self.original_query = original_query
            
            self.add_item(discord.ui.TextInput(
                label="Song Name & Artist",
                placeholder="Artist - Song Name",
                default=original_query,
                required=True,
                max_length=200,
                custom_id="song_input"
            ))
        
        async def on_submit(self, interaction: discord.Interaction):
            await interaction.response.defer(ephemeral=True)
            
            # get the input value from the modal
            song_info = None
            for item in self.children:
                if isinstance(item, discord.ui.TextInput):
                    song_info = item.value
                    break
                    
            if not song_info:
                await interaction.followup.send("No song information provided. Please try again.", ephemeral=True)
                return
            
            # get the bot instance from interaction
            bot = interaction.client
            
            try:
                # get the owner user
                owner = await bot.fetch_user(self.owner_id)
                
                if owner:
                    # create an embed to send to the owner
                    embed = discord.Embed(
                        title="🎵 New Song Request",
                        description="A user requested a song that's not in the library.",
                        color=0x3498db
                    )
                    embed.add_field(name="Original Query", value=self.original_query, inline=False)
                    embed.add_field(name="User Input", value=song_info, inline=False)
                    embed.add_field(
                        name="Requested By", 
                        value=f"{interaction.user.mention} (ID: {interaction.user.id})", 
                        inline=False
                    )
                    embed.add_field(
                        name="Library Link", 
                        value="[Open Library](https://c4.radioboss.fm/#main)", 
                        inline=False
                    )
                    
                    try:
                        await owner.send(embed=embed)
                        await interaction.followup.send(
                            "Thank you! The admin has been notified about your request.",
                            ephemeral=True
                        )
                    except Exception as e:
                        logger.error(f"Failed to send DM to owner: {e}")
                        await interaction.followup.send(
                            "Thank you for your request. We'll look into adding this song.",
                            ephemeral=True
                        )
                else:
                    await interaction.followup.send(
                        "Thank you for your request. We'll look into adding this song.",
                        ephemeral=True
                    )
            except Exception as e:
                logger.error(f"Error in song request: {e}", exc_info=True)
                await interaction.followup.send(
                    "An error occurred while processing your request. Please try again later.",
                    ephemeral=True
                )
    
    @app_commands.command(
        name="request",
        description="Request a song to be played on the radio",
    )
    @app_commands.describe(
        song_name="Name of the song you want to request",
    )
    async def request_song(
        self,
        interaction: discord.Interaction,
        song_name: str,
    ) -> None:
        """handle the /request slash command."""
        # defer the response as ephemeral to ensure all follow-ups are also ephemeral
        await interaction.response.defer(ephemeral=True)
        self.confirmed = False
        self.cancelled = False
        
        # define headers to mimic a browser request
        headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
            'Accept': '*/*',
            'Accept-Encoding': 'gzip, deflate, br'
        }
        
        # define URL and parameters for the request
        url = "https://c4.radioboss.fm/w/songrequestsearch"
        params = {
            "u": 560,  # station ID
            "q": song_name,
            "_": ""  # cache buster
        }
        
        try:
            # search for the song in the library
            async with aiohttp.ClientSession(headers=headers) as session:
                async with session.get(url, params=params) as response:
                    if response.status != 200:
                        await interaction.followup.send(
                            "❌ Failed to search for songs. Please try again later.",
                            ephemeral=True
                        )
                        return
                        
                    # get and parse the JSON response
                    response_text = await response.text(encoding='utf-8')
                    print("\n" + "="*80)
                    print("SEARCH RESPONSE:")
                    print(response_text)
                    print("="*80 + "\n")
                    
                    try:
                        import json
                        data = json.loads(response_text)
                        
                        if not data.get('tracks'):
                            # no songs found, show the request button
                            if not self.owner_id:
                                await interaction.followup.send("No songs found. The admin has been notified.", ephemeral=True)
                                return
                            
                            # create a view with the request button
                            view = discord.ui.View(timeout=300)  # 5 minute timeout
                            request_btn = self.RequestSongButton(
                                owner_id=self.owner_id,
                                original_query=song_name
                            )
                            view.add_item(request_btn)
                            
                            await interaction.followup.send(
                                "We couldn't find that song in our library. Would you like to request it?",
                                view=view,
                                ephemeral=True
                            )
                            return
                        
                        # process the tracks
                        tracks = data.get('tracks', [])
                        if not tracks:
                            await interaction.followup.send("No songs found. Please try a different search term.", ephemeral=True)
                            return
                            
                        # create a view with the dropdown
                        select_view = discord.ui.View(timeout=60)
                        dropdown = self.SongSelectDropdown(tracks)
                        select_view.add_item(dropdown)
                        
                        # send the dropdown as ephemeral
                        await interaction.followup.send(
                            "Select a song:",
                            view=select_view,
                            ephemeral=True
                        )
                        
                        try:
                            # wait for the user to select a song
                            await select_view.wait()
                            
                            if not hasattr(select_view, 'selected_song_id'):
                                await interaction.followup.send("No song selected. Please try again.", ephemeral=True)
                                return
                                
                            # get the selected track
                            selected_track = next((t for t in tracks if str(t['id']) == select_view.selected_song_id), None)
                            
                            if not selected_track:
                                await interaction.followup.send("❌ Invalid selection. Please try again.", ephemeral=True)
                                return
                            
                            # create confirmation view
                            confirm_view = self.ConfirmRequestView(
                                track_title=selected_track['title'],
                                track_id=selected_track['id']
                            )
                            
                            # send confirmation message
                            await interaction.followup.send(
                                f"Confirm request for: **{selected_track['title']}**",
                                view=confirm_view,
                                ephemeral=True
                            )
                            
                            # wait for confirmation
                            await confirm_view.wait()
                            
                            if confirm_view.confirmed:
                                # request the song
                                success = await self.radioboss.request_song(selected_track['id'])
                                
                                if success:
                                    await interaction.followup.send(
                                        f"Your song '{selected_track['title']}' has been requested! "
                                        "It should play in about 5 minutes. Use `/join` to listen in a voice channel!",
                                        ephemeral=True
                                    )
                                else:
                                    await interaction.followup.send(
                                        "❌ Failed to request the song. Please try again later.",
                                        ephemeral=True
                                    )
                            
                        except asyncio.TimeoutError:
                            await interaction.followup.send("⏱️ Timed out. Please try your request again.", ephemeral=True)
                        except Exception as e:
                            logger.error(f"Error in song request: {e}", exc_info=True)
                            await interaction.followup.send("❌ An error occurred while processing your request. Please try again.", ephemeral=True)
                            
                    except json.JSONDecodeError as e:
                        logger.error(f"Failed to parse JSON response: {e}")
                        await interaction.followup.send("❌ Failed to process the song list. Please try again later.", ephemeral=True)
                        
        except aiohttp.ClientError as e:
            logger.error(f"HTTP error during song search: {e}")
            await interaction.followup.send("❌ Failed to connect to the music library. Please try again later.", ephemeral=True)
            
        except Exception as e:
            logger.error(f"Unexpected error in request_song: {e}", exc_info=True)
            error_embed = discord.Embed(
                title="❌ Error",
                description=(
                    "An unexpected error occurred while processing your request. "
                    "Please try again or contact support if the issue persists."
                ),
                color=0xe74c3c  # red
            )
            await interaction.followup.send(embed=error_embed, ephemeral=True)

async def setup(bot: commands.Bot) -> None:
    """add the radioboss cog to the bot."""
    await bot.add_cog(RadioBossCog(bot))
