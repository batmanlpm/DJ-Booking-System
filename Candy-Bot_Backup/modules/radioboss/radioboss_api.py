"""
Radioboss API client for interacting with the Radioboss remote control API.
"""

import asyncio
import logging
import aiohttp
import async_timeout
from typing import Optional, Dict, Any, List

logger = logging.getLogger(__name__)

class RadioBossAPIError(Exception):
    """custom exception for Radioboss API errors."""
    pass

class RadioBossAPIClient:
    """client for interacting with the Radioboss API."""

    def __init__(self, base_url: str, api_key: str, station_id: str, owner_id: int = None):
        """initialize the Radioboss API client."""
        self.base_url = base_url.rstrip('/')
        self.api_key = api_key
        self.station_id = station_id
        self.owner_id = owner_id
        self.session: Optional[aiohttp.ClientSession] = None
        self.timeout = 10  # seconds

    async def _ensure_session(self) -> None:
        """ensure an aiohttp client session exists."""
        if self.session is None or self.session.closed:
            self.session = aiohttp.ClientSession()

    async def _make_request(self, endpoint: str, params: Optional[dict] = None) -> dict:
        """make a request to the Radioboss API."""
        await self._ensure_session()
        
        url = f"{self.base_url}/api/{endpoint.lstrip('/')}"
        headers = {
            'X-API-Key': self.api_key,
            'Accept': 'application/json'
        }
        
        if params is None:
            params = {}
            
        params['station'] = self.station_id
        
        try:
            async with async_timeout.timeout(self.timeout):
                async with self.session.get(url, params=params, headers=headers) as response:
                    if response.status != 200:
                        error_text = await response.text()
                        raise RadioBossAPIError(
                            f"API request failed with status {response.status}: {error_text}"
                        )
                    
                    try:
                        return await response.json()
                    except Exception as e:
                        text = await response.text()
                        raise RadioBossAPIError(
                            f"Failed to parse JSON response: {e}, response: {text[:200]}"
                        ) from e
                    
        except asyncio.TimeoutError as e:
            raise RadioBossAPIError("Request to RadioBoss API timed out") from e
        except aiohttp.ClientError as e:
            raise RadioBossAPIError(f"Error making request to RadioBoss API: {e}") from e

    async def get_stream_info(self) -> Dict[str, Any]:
        """get stream information including stream URLs."""
        return await self._make_request('stream/info')

    async def search_song_in_library(self, query: str) -> tuple[bool, str]:
        """search for a song in the library."""
        try:
            # use the songrequestsearch endpoint
            url = "https://c4.radioboss.fm/w/songrequestsearch"
            params = {
                "u": self.station_id,  # use the instance station_id
                "q": query,
                "_": ""  # cache buster
            }
            
            logger.info(f"Searching for song: {url}?u={params['u']}&q={query}")
            
            # set a reasonable timeout
            timeout = aiohttp.ClientTimeout(total=10)
            
            # create a session with headers that mimic a browser
            headers = {
                'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
                'Accept': 'application/json, text/plain, */*',
                'Accept-Language': 'en-US,en;q=0.9',
                'Referer': 'https://c4.radioboss.fm/',
                'Origin': 'https://c4.radioboss.fm',
                'DNT': '1',
                'Connection': 'keep-alive',
                'Sec-Fetch-Dest': 'empty',
                'Sec-Fetch-Mode': 'cors',
                'Sec-Fetch-Site': 'same-origin',
                'Pragma': 'no-cache',
                'Cache-Control': 'no-cache'
            }
            
            # use a new session for this request
            async with aiohttp.ClientSession(headers=headers) as session:
                async with session.get(url, params=params, timeout=timeout) as response:
                    # log response status and headers for debugging
                    logger.info(f"Search response status: {response.status}")
                    
                    # get the content type
                    content_type = response.headers.get('Content-Type', '').lower()
                    
                    # get the response text first
                    response_text = await response.text(encoding='utf-8')
                    
                    # log the response for debugging
                    logger.debug(f"Response content type: {content_type}")
                    logger.debug(f"Response text: {response_text[:500]}...")
                    
                    # check if we got HTML instead of JSON
                    if 'text/html' in content_type or response_text.strip().startswith('<'):
                        logger.warning("Received HTML response instead of JSON. The API might be down or rate limiting.")
                        # Try to extract JSON from HTML if it's embedded in the page
                        import re
                        json_match = re.search(r'<script[^>]*>\s*var\s+data\s*=\s*(\[.*?\]|\{.*?\})\s*;', 
                                            response_text, re.DOTALL)
                        if json_match:
                            try:
                                import json
                                json_str = json_match.group(1)
                                data = json.loads(json_str)
                                if isinstance(data, list) and len(data) > 0:
                                    track_id = str(data[0].get('id', ''))
                                    if track_id:
                                        logger.info(f"Found track ID in HTML response: {track_id}")
                                        return True, track_id
                            except Exception as e:
                                logger.warning(f"Failed to parse JSON from HTML: {e}")
                        
                        # If we got here, we couldn't extract valid data from HTML
                        return False, "The radio server returned an unexpected response. Please try again later."
                    
                    # try to parse as JSON
                    try:
                        import json
                        data = await response.json()
                        if isinstance(data, list) and len(data) > 0:
                            track_id = str(data[0].get('id', ''))
                            if track_id:
                                logger.info(f"Found track ID in JSON response: {track_id}")
                                return True, track_id
                    except Exception as e:
                        logger.warning(f"Failed to parse JSON response: {e}")
                    
                    # try to find track ID using simple string search as fallback
                    if '"id":' in response_text and '"title":' in response_text:
                        try:
                            import re
                            match = re.search(r'"id"\s*:\s*"?(\d+)', response_text)
                            if match:
                                track_id = match.group(1)
                                logger.info(f"Found track ID using regex search: {track_id}")
                                return True, track_id
                        except Exception as e:
                            logger.error(f"Error in regex track ID search: {e}")
                    
                    # last resort: look for any numbers that might be track IDs
                    try:
                        import re
                        matches = re.findall(r'\b\d{4,}\b', response_text)
                        if matches:
                            track_id = matches[0]
                            logger.info(f"Found potential track ID in raw text: {track_id}")
                            return True, track_id
                    except Exception as e:
                        logger.error(f"Error in numeric ID search: {e}")
                    
                    # if we still don't have a track ID, log the response
                    logger.error(f"Could not find track ID in response. Response starts with: {response_text[:200]}...")
                    return False, "Could not find any matching songs. Please try a different search term."
                        
        except asyncio.TimeoutError:
            logger.error("Search request timed out")
            return False, "The search request timed out. Please try again later."
            
        except Exception as e:
            logger.error(f"Error searching for song: {str(e)}", exc_info=True)
            return False, f"An error occurred while searching for songs: {str(e)}"
            
    async def request_song(self, song_id: str) -> bool:
        """request a song to be played on the radio."""
        try:
            # use the songrequestmake endpoint with the correct format
            url = "https://c4.radioboss.fm/w/songrequestmake"
            params = {
                "u": self.station_id,  # use the instance station_id
                "id": song_id,
                "_": ""  # cache buster
            }
            
            logger.info(f"Making song request: {url}?u={params['u']}&id={params['id']}")
            
            # set a reasonable timeout
            timeout = aiohttp.ClientTimeout(total=10)
            
            # create a session with headers that might help with the response
            headers = {
                'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
                'Accept': '*/*',
                'Accept-Encoding': 'gzip, deflate, br',
                'Connection': 'keep-alive'
            }
            
            async with aiohttp.ClientSession(headers=headers) as session:
                async with session.get(url, params=params, timeout=timeout) as response:
                    # log the response status and headers for debugging
                    logger.info(f"Response status: {response.status}")
                    logger.info(f"Response headers: {dict(response.headers)}")
                    
                    # get the response text first
                    response_text = await response.text(encoding='utf-8')
                    
                    # print the raw response to console
                    print("\n" + "="*80)
                    print("RAW RESPONSE FROM SONG REQUEST:")
                    print(response_text)
                    print("="*80 + "\n")
                    
                    # log the full response
                    logger.info(f"Full response: {response_text}")
                    
                    # if status is 200, assume success
                    if response.status == 200:
                        logger.info("Request successful (status 200)")
                        return True
                        
                    # if we get here, the request likely failed
                    logger.error(f"Request failed with status {response.status}")
                    return False
                        
        except asyncio.TimeoutError:
            logger.error("Request timed out")
            return False
            
        except Exception as e:
            logger.error(f"Error in request_song: {str(e)}", exc_info=True)
            return False

    async def get_current_track(self) -> Dict[str, Any]:
        """get information about the currently playing track."""
        return await self._make_request('track/current')

    async def get_stream_urls(self) -> Dict[str, str]:
        """get available stream URLs."""
        info = await self.get_stream_info()
        return info.get('stream_urls', {})

    async def get_recent_tracks(self, limit: int = 10) -> List[Dict[str, Any]]:
        """get recently played tracks."""
        return await self._make_request(f'track/history?limit={limit}')

    async def close(self) -> None:
        """close the HTTP session."""
        if self.session and not self.session.closed:
            await self.session.close()

# configuration moved to config.py

async def example_usage():
    """example of how to use the RadioBoss API client in your Discord bot."""
    
    # initialize the client
    client = RadioBossAPIClient(
        base_url=RADIOBOSS_API_URL,
        api_key=RADIOBOSS_API_KEY,
        station_id=RADIOBOSS_STATION_ID
    )
    
    try:
        # get stream information
        stream_info = await client.get_stream_info()
        print("Stream info:", stream_info)
        
        # get current track
        current_track = await client.get_current_track()
        print(f"Now playing: {current_track['title']}")
        print(f"Artist: {current_track['artist']}")
        print(f"Listeners: {current_track['listeners']}")
        
        # get stream URLs
        stream_urls = await client.get_stream_urls()
        print("Available stream URLs:")
        for url_type, url in stream_urls.items():
            print(f"  {url_type}: {url}")
        
        # get recent tracks
        recent_tracks = await client.get_recent_tracks()
        print("Recent tracks:")
        for track in recent_tracks[:5]:  # show last 5 tracks
            print(f"  {track['title']} - {track['started']}")
            
    except Exception as e:
        print(f"Error: {e}")
    finally:
        await client.close()

# discord bot command example
import discord
from discord.ext import commands

class RadioBossCommands(commands.Cog):
    """discord bot cog for RadioBoss commands."""
    
    def __init__(self, bot):
        self.bot = bot
        self.radioboss_client = RadioBossAPIClient(
            base_url=RADIOBOSS_API_URL,
            api_key=RADIOBOSS_API_KEY,
            station_id=RADIOBOSS_STATION_ID
        )
    
    @commands.command(name='nowplaying', aliases=['np'])
    async def now_playing(self, ctx):
        """get currently playing track."""
        try:
            track_info = await self.radioboss_client.get_current_track()
            
            embed = discord.Embed(
                title="🎵 Now Playing",
                color=discord.Color.blue()
            )
            embed.add_field(name="Title", value=track_info['title'], inline=False)
            embed.add_field(name="Artist", value=track_info['artist'], inline=True)
            embed.add_field(name="Album", value=track_info['album'], inline=True)
            embed.add_field(name="Duration", value=track_info['duration'], inline=True)
            embed.add_field(name="Listeners", value=track_info['listeners'], inline=True)
            embed.add_field(name="Live", value="Yes" if track_info['live'] else "No", inline=True)
            
            await ctx.send(embed=embed)
            
        except Exception as e:
            await ctx.send(f"Error getting current track: {e}")
    
    @commands.command(name='radio')
    async def radio_info(self, ctx):
        """get radio station information."""
        try:
            stream_info = await self.radioboss_client.get_stream_info()
            
            embed = discord.Embed(
                title=f"📻 {stream_info.get('station_name', 'Radio Station')}",
                description=stream_info.get('station_description', ''),
                color=discord.Color.green()
            )
            
            # add stream URL
            embed.add_field(
                name="Stream URL",
                value=f"[Listen Here]({RADIOBOSS_STREAM_URL})",
                inline=False
            )
            
            # add current track
            embed.add_field(
                name="Now Playing",
                value=stream_info.get('nowplaying', 'Unknown'),
                inline=False
            )
            
            # add listener count
            embed.add_field(
                name="Listeners",
                value=str(stream_info.get('listeners', 0)),
                inline=True
            )
            
            # add genre if available
            if stream_info.get('station_genre'):
                embed.add_field(
                    name="Genre",
                    value=stream_info['station_genre'],
                    inline=True
                )
            
            await ctx.send(embed=embed)
            
        except Exception as e:
            await ctx.send(f"Error getting radio info: {e}")
    
    @commands.command(name='recent')
    async def recent_tracks(self, ctx, limit: int = 5):
        """get recently played tracks."""
        try:
            recent = await self.radioboss_client.get_recent_tracks()
            
            if not recent:
                await ctx.send("No recent tracks found.")
                return
            
            embed = discord.Embed(
                title="🎵 Recently Played",
                color=discord.Color.purple()
            )
            
            for i, track in enumerate(recent[:limit]):
                embed.add_field(
                    name=f"{i+1}. {track['title']}",
                    value=f"Started: {track['started']}",
                    inline=False
                )
            
            await ctx.send(embed=embed)
            
        except Exception as e:
            await ctx.send(f"Error getting recent tracks: {e}")
    
    def cog_unload(self):
        """cleanup when cog is unloaded."""
        asyncio.create_task(self.radioboss_client.close())

# in your main bot file, add this:
async def setup(bot):
    await bot.add_cog(RadioBossCommands(bot))

# alternative: simple function to get stream URL
async def get_working_stream_url():
    """get a working stream URL from the API."""
    client = RadioBossAPIClient(
        base_url=RADIOBOSS_API_URL,
        api_key=RADIOBOSS_API_KEY,
        station_id=RADIOBOSS_STATION_ID
    )
    
    try:
        urls = await client.get_stream_urls()
        # try different URL types in order of preference
        for url_type in ['ssl', 'http', 'port443', 'port80']:
            if url_type in urls and urls[url_type]:
                return urls[url_type]
        # fallback to configured stream URL
        return RADIOBOSS_STREAM_URL
    except Exception as e:
        print(f"Error getting stream URL from API: {e}")
        return RADIOBOSS_STREAM_URL
    finally:
        await client.close()

if __name__ == "__main__":
    # test the API
    asyncio.run(example_usage())