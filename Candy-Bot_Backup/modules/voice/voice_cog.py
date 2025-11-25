"""
voice cog for handling voice channel connections and audio streaming.
"""

import asyncio
import logging
from typing import Dict, Optional, Any
import aiohttp
import json
from datetime import datetime

import discord
from discord import app_commands, ui
from discord.ext import commands

import config

logger = logging.getLogger(__name__)

# FFmpeg options for stable audio streaming
FFMPEG_OPTIONS = {
    'before_options': '-reconnect 1 -reconnect_streamed 1 -reconnect_delay_max 5',
    'options': '-vn -loglevel warning -af volume=1.0',
}

class VoiceControls(ui.View):
    """view for voice control buttons."""

    def __init__(self, voice_client: discord.VoiceClient, bot: commands.Bot):
        """initialize the voice controls view."""
        super().__init__(timeout=None)  # no timeout
        self.voice_client = voice_client
        self.bot = bot
        self._message = None
        self.volume = 0.5  # default volume (50% of original)
        self._current_source = None
        
    async def on_timeout(self) -> None:
        """handle view timeout."""
        pass  # no action needed on timeout

    async def update_embed(self, interaction: discord.Interaction, title: str, description: str, color: int = 0x2f3136) -> None:
        """update the embed message."""
        embed = discord.Embed(
            title=title,
            description=description,
            color=color
        )
        
        if interaction.client.user.avatar:
            embed.set_author(
                name=interaction.client.user.display_name,
                icon_url=interaction.client.user.avatar.url
            )
        else:
            embed.set_author(name=interaction.client.user.display_name)
            
        try:
            if interaction.response.is_done():
                await interaction.followup.edit_message(
                    message_id=self._message.id,
                    embed=embed,
                    view=self
                )
            else:
                await interaction.response.edit_message(embed=embed, view=self)
        except Exception as e:
            logger.error(f"Error updating embed: {e}")

    @ui.button(label="Pause", style=discord.ButtonStyle.gray)
    async def toggle_playback(self, interaction: discord.Interaction, button: ui.Button) -> None:
        """toggle between mute and unmute the bot."""
        try:
            if not self.voice_client or not self.voice_client.is_connected():
                await interaction.response.send_message("not connected to a voice channel!", ephemeral=True)
                return

            # get the bot's member object in the guild
            bot_member = interaction.guild.me
            
            if bot_member.voice and bot_member.voice.mute:
                # unmute the bot
                await bot_member.edit(mute=False)
                button.label = "Pause"
                # get the current embed to preserve all fields
                current_embed = interaction.message.embeds[0]
                current_embed.title = "Now Playing"
                current_embed.color = 0x2f3136  # Dark gray/black theme
                await interaction.response.edit_message(embed=current_embed, view=self)
            else:
                # Mute the bot
                await bot_member.edit(mute=True)
                button.label = "Play"
                # get the current embed to preserve all fields
                current_embed = interaction.message.embeds[0]
                current_embed.title = "Paused"
                current_embed.color = 0x2f3136  # dark gray theme
                await interaction.response.edit_message(embed=current_embed, view=self)
                
            if not interaction.response.is_done():
                await interaction.response.defer()
                
        except Exception as e:
            logger.error(f"error in toggle_playback: {e}")
            if not interaction.response.is_done():
                await interaction.response.send_message("an error occurred while toggling playback", ephemeral=True)

    def _create_audio_source(self, stream_url: str, volume: float = 0.5):
        """create a new audio source with the specified volume."""
        # create a copy of FFMPEG_OPTIONS to avoid modifying the original
        ffmpeg_options = FFMPEG_OPTIONS.copy()
        
        # update the volume in the options
        ffmpeg_options['options'] = f"-vn -loglevel warning -af volume={volume:.2f}"
        
        return discord.FFmpegPCMAudio(
            stream_url,
            **ffmpeg_options
        )

    async def _update_volume(self, interaction: discord.Interaction, new_volume: float) -> None:
        """update the volume and restart playback with new volume."""
        if not self.voice_client or not self.voice_client.is_connected():
            return

        try:
            was_playing = not self.voice_client.is_paused()
            
            # create new source with updated volume
            stream_url = getattr(self.voice_client.source, 'url', config.RADIOBOSS_STREAM_URL)
            new_source = self._create_audio_source(stream_url, new_volume)
            
            # store the source for volume adjustments
            self._current_source = new_source
            
            # stop current playback and start new one
            self.voice_client.stop()
            
            # get the after callback if it exists
            after_callback = getattr(self.voice_client, '_player', None) and self.voice_client._player.after
            
            # start new playback
            self.voice_client.play(new_source, after=after_callback)
            
            # restore play/pause state if needed
            if not was_playing:
                self.voice_client.pause()
                
            # Update the volume display
            await self._update_volume_display(interaction)
            
        except Exception as e:
            logger.error(f"error updating volume: {e}")
            if not interaction.response.is_done():
                await interaction.response.send_message("an error occurred while adjusting volume", ephemeral=True)

    @ui.button(emoji="🔉", style=discord.ButtonStyle.gray, row=1)
    async def volume_down(self, interaction: discord.Interaction, button: ui.Button) -> None:
        """decrease volume by 10%."""
        try:
            # round to nearest 0.1 to avoid floating point precision issues
            current_volume = round(self.volume * 10) / 10
            new_volume = max(0.1, current_volume - 0.1)  # decrease by 10%, min 10%
            if new_volume != current_volume:
                self.volume = new_volume
                await self._update_volume(interaction, new_volume)
        except Exception as e:
            logger.error(f"error adjusting volume down: {e}")
            if not interaction.response.is_done():
                await interaction.response.send_message("an error occurred while adjusting volume", ephemeral=True)

    @ui.button(emoji="🔊", style=discord.ButtonStyle.gray, row=1)
    async def volume_up(self, interaction: discord.Interaction, button: ui.Button) -> None:
        """increase volume by 10%."""
        try:
            # round to nearest 0.1 to avoid floating point precision issues
            current_volume = round(self.volume * 10) / 10
            new_volume = min(1.0, current_volume + 0.1)  # increase by 10%, max 100%
            if new_volume != current_volume:
                self.volume = new_volume
                await self._update_volume(interaction, new_volume)
        except Exception as e:
            logger.error(f"error adjusting volume up: {e}")
            if not interaction.response.is_done():
                await interaction.response.send_message("an error occurred while adjusting volume", ephemeral=True)


    async def _update_volume_display(self, interaction: discord.Interaction) -> None:
        """update the embed with current volume."""
        try:
            current_embed = interaction.message.embeds[0]
            volume_percent = self.bot.get_cog("VoiceCog")._get_volume_percent(self.volume)
            voice_cog = self.bot.get_cog("VoiceCog")
            volume_bar = voice_cog._get_volume_bar(self.volume) if voice_cog else ""
            volume_text = f"`{volume_bar}` {volume_percent}%"
            
            # find the volume field if it exists
            volume_field_index = next((i for i, field in enumerate(current_embed.fields) 
                                    if field.name.lower() == "volume"), -1)
            
            if volume_field_index >= 0:
                current_embed.set_field_at(
                    index=volume_field_index,
                    name="Volume",
                    value=volume_text,
                    inline=False
                )
            else:
                current_embed.add_field(
                    name="Volume",
                    value=volume_text,
                    inline=False
                )
            
            # update the message
            if interaction.response.is_done():
                await interaction.edit_original_response(embed=current_embed, view=self)
            else:
                await interaction.response.edit_message(embed=current_embed, view=self)
                
        except Exception as e:
            logger.error(f"error updating volume display: {e}")
            if not interaction.response.is_done():
                await interaction.response.defer()

    @ui.button(label="Disconnect", style=discord.ButtonStyle.red)
    async def stop_playback(self, interaction: discord.Interaction, button: ui.Button) -> None:
        """stop playback and disconnect."""
        try:
            if not self.voice_client or not self.voice_client.is_connected():
                await interaction.response.send_message("Not connected to a voice channel!", ephemeral=True)
                return
                
            # update the embed before disconnecting
            await self.update_embed(
                interaction,
                "🔌 Disconnected",
                "Disconnected from the voice channel.\nuse `/join` to reconnect.",
                color=0xe74c3c
            )
            
            # defer the response before disconnecting
            if not interaction.response.is_done():
                await interaction.response.defer()
            
            # get the voice cog and clean up
            cog = self.bot.get_cog("VoiceCog")
            if cog:
                await cog.cleanup_voice_client(interaction.guild_id)
                
        except Exception as e:
            logger.error(f"error in stop_playback: {e}")
            if not interaction.response.is_done():
                await interaction.response.send_message("an error occurred while disconnecting", ephemeral=True)

class VoiceCog(commands.Cog):
    """cog for handling voice channel interactions."""
    
    @staticmethod
    def _get_volume_bar(volume: float) -> str:
        """generate a volume bar string based on volume level (0.1 to 1.0)."""
        # calculate exact number of filled blocks (1-10)
        filled_blocks = min(10, max(1, round(volume * 10)))
        return "█" * filled_blocks + "░" * (10 - filled_blocks)
        
    @staticmethod
    def _get_volume_percent(volume: float) -> int:
        """get the volume percentage as an integer (10-100)."""
        return min(100, max(10, round(volume * 100)))


    def __init__(self, bot: commands.Bot):
        """initialize the voice cog."""
        self.bot = bot
        self.voice_clients: Dict[int, discord.VoiceClient] = {}
        logger.info("voice cog initialized")

    async def cleanup_voice_client(self, guild_id: int) -> None:
        """clean up voice client for a guild."""
        if guild_id not in self.voice_clients:
            return
            
        vc = self.voice_clients[guild_id]
        try:
            # stop any ongoing playback
            if vc.is_playing() or vc.is_paused():
                vc.stop()
                
            # clean up the voice client
            if vc.is_connected():
                await vc.disconnect(force=True)  # force disconnection
                
            # clear any existing voice client state
            if vc.guild.voice_client:
                await vc.guild.voice_client.disconnect(force=True)
                
        except Exception as e:
            logger.error(f'error cleaning up voice client: {e}')
        finally:
            # ensure we remove the client from our tracking
            if guild_id in self.voice_clients:
                del self.voice_clients[guild_id]
                
            # force cleanup of any lingering voice state
            guild = self.bot.get_guild(guild_id)
            if guild and guild.voice_client:
                try:
                    await guild.voice_client.disconnect(force=True)
                except:
                    pass  # ignore any errors during forced cleanup

    @app_commands.command(
        name="join",
        description="join your voice channel and start streaming the radio"
    )
    async def join_voice(self, interaction: discord.Interaction) -> None:
        """handle the /join slash command."""
        await interaction.response.defer(ephemeral=True)
        
        # check if user is in a voice channel
        if not interaction.user.voice or not interaction.user.voice.channel:
            embed = discord.Embed(
                title="❌ not in voice channel",
                description="please join a voice channel first!",
                color=0xe74c3c
            )
            await interaction.followup.send(embed=embed, ephemeral=True)
            return

        voice_channel = interaction.user.voice.channel
        guild_id = interaction.guild_id

        try:
            # clean up any existing voice client for this guild
            await self.cleanup_voice_client(guild_id)

            # connect to voice channel with timeout
            try:
                voice_client = await asyncio.wait_for(
                    voice_channel.connect(),
                    timeout=10.0
                )
                logger.info(f"Connected to voice channel: {voice_channel.name}")
            except asyncio.TimeoutError:
                raise Exception("timed out while trying to connect to voice channel")
                
            self.voice_clients[guild_id] = voice_client

            # test if we can access the stream URL
            stream_url = config.RADIOBOSS_STREAM_URL
            logger.info(f"attempting to stream from: {stream_url}")
            
            # create audio source with error handling
            def after_playing(error):
                if error:
                    logger.error(f"playback error: {error}")
                    # schedule cleanup on the bot's loop
                    asyncio.run_coroutine_threadsafe(
                        self.cleanup_voice_client(guild_id),
                        self.bot.loop
                    )
                else:
                    logger.info("playback ended normally")

            try:
                # try to create the audio source
                audio_source = discord.FFmpegPCMAudio(
                    stream_url,
                    **FFMPEG_OPTIONS
                )
                
                # store the source and start playing
                view = VoiceControls(voice_client, self.bot)
                view._current_source = audio_source
                voice_client.play(audio_source, after=after_playing)
                
                # check if playback actually started
                await asyncio.sleep(1)  # give it a moment to start
                
                if not voice_client.is_playing():
                    raise Exception("audio source created but playback did not start")
                
                logger.info("playback started successfully")
                
                # create success embed with dark theme
                embed = discord.Embed(
                    title="Now Playing",
                    color=0x2f3136,  # dark gray/black theme
                    description=f"**Streaming from [LivePartyMusic.fm](https://livepartymusic.fm/lpm/)**\n"
                )
                embed.add_field(
                    name="Voice Channel",
                    value=f"{voice_channel.mention}",
                    inline=False
                )
                embed.add_field(
                    name="Controls",
                    value="Use the buttons below to control playback.",
                    inline=False
                )
                # add initial volume display (50%)
                voice_cog = self.bot.get_cog("VoiceCog")
                volume_bar = voice_cog._get_volume_bar(0.5)  # 50% default
                volume_percent = voice_cog._get_volume_percent(0.5)
                embed.add_field(
                    name="Volume",
                    value=f"`{volume_bar}` {volume_percent}%",
                    inline=False
                )
                embed.set_thumbnail(url="https://livepartymusic.fm/wp-content/uploads/2023/06/cropped-lpm_logo.png")
                
                # create and send controls
                view = VoiceControls(voice_client, self.bot)
                message = await interaction.followup.send(embed=embed, view=view)
                view._message = message
                
            except Exception as audio_error:
                logger.error(f"audio creation/playback error: {audio_error}")
                
                # try alternative FFmpeg options
                try:
                    logger.info("trying alternative FFmpeg options...")
                    audio_source = discord.FFmpegPCMAudio(
                        stream_url,
                        before_options='-reconnect 1 -reconnect_streamed 1',
                        options='-vn'
                    )
                    
                    voice_client.play(audio_source, after=after_playing)
                    
                    # check if playback started
                    await asyncio.sleep(1)
                    
                    if not voice_client.is_playing():
                        raise Exception("alternative options failed to start playback")
                    
                    logger.info("playback started with alternative options")
                    
                    # create success embed for compatibility mode
                    embed = discord.Embed(
                        title="Now Playing (Compatibility Mode)",
                        color=0xf39c12,  # orange for compatibility mode
                        description=f"**Streaming from [LivePartyMusic.fm](https://livepartymusic.fm/)**\n                                    \n{voice_channel.mention}\n\n"
                                    "*Using compatibility mode - some features may be limited.*"
                    )
                    embed.add_field(
                        name="Controls",
                        value="Use the buttons below to control playback.",
                        inline=False
                    )
                    embed.set_footer(
                        text="Auto-disconnect after 5 minutes of inactivity"
                    )
                    embed.set_thumbnail(url="https://livepartymusic.fm/wp-content/uploads/2023/06/cropped-lpm_logo.png")
                    
                    # create and send controls
                    view = VoiceControls(voice_client, self.bot)
                    message = await interaction.followup.send(embed=embed, view=view)
                    view._message = message
                    
                except Exception as final_error:
                    logger.error(f"all playback attempts failed: {final_error}")
                    await self.cleanup_voice_client(guild_id)
                    
                    embed = discord.Embed(
                        title="❌ playback failed",
                        description=f"failed to start audio playback.\n\n"
                                   f"**possible causes:**\n"
                                   f"• ffmpeg not installed or not in PATH\n"
                                   f"• stream url is not accessible\n"
                                   f"• network connectivity issues\n"
                                   f"• discord voice server issues\n\n"
                                   f"**error details:**\n"
                                   f"```{str(final_error)[:200]}```",
                        color=0xe74c3c
                    )
                    await interaction.followup.send(embed=embed, ephemeral=True)
                    
        except Exception as e:
            logger.error(f"error in join_voice: {e}")
            await self.cleanup_voice_client(guild_id)
            
            embed = discord.Embed(
                title="❌ connection failed",
                description=f"failed to connect to voice channel.\n\n"
                           f"**error:** {str(e)}",
                color=0xe74c3c
            )
            await interaction.followup.send(embed=embed, ephemeral=True)

    @app_commands.command(
        name="leave",
        description="disconnect from voice channel"
    )
    async def leave_voice(self, interaction: discord.Interaction) -> None:
        """handle the /leave slash command."""
        await interaction.response.defer(ephemeral=True)
        
        guild_id = interaction.guild_id
        
        if guild_id not in self.voice_clients:
            embed = discord.Embed(
                title="❌ not connected",
                description="i'm not connected to any voice channel in this server.",
                color=0xe74c3c
            )
            await interaction.followup.send(embed=embed, ephemeral=True)
            return
        
        await self.cleanup_voice_client(guild_id)
        
        embed = discord.Embed(
            title="🔌 disconnected",
            description="successfully disconnected from voice channel.",
            color=0x2ecc71
        )
        await interaction.followup.send(embed=embed, ephemeral=True)

    @commands.Cog.listener()
    async def on_voice_state_update(self, member: discord.Member, before: discord.VoiceState, after: discord.VoiceState) -> None:
        """handle voice state updates for automatic disconnection."""
        # ignore bot's own voice state updates
        if member.id == self.bot.user.id:
            return
            
        # check if we need to clean up after being alone in a voice channel
        if (before.channel and before.channel.guild.voice_client and 
                before.channel.guild.voice_client.channel == before.channel and
                len(before.channel.members) == 1 and 
                before.channel.members[0].id == self.bot.user.id):
            
            guild_id = before.channel.guild.id
            logger.info(f"auto-disconnecting from {before.channel.name} - no listeners")
            await self.cleanup_voice_client(guild_id)
            
    async def _fetch_radioboss_data(self) -> Optional[Dict[str, Any]]:
        """fetch current track information from RadioBOSS API."""
        if not hasattr(config, 'RADIOBOSS_API_KEY') or not config.RADIOBOSS_API_KEY:
            logger.error("RADIOBOSS_API_KEY is not configured")
            return None
            
        # use the domain from RADIOBOSS_STREAM_URL or default to c4.radioboss.fm
        api_domain = "c4.radioboss.fm"  # default API domain
        station_id = "560"  # default station ID
        
        if hasattr(config, 'RADIOBOSS_STREAM_URL'):
            # extract domain from stream URL (e.g., c4.radioboss.fm from https://c4.radioboss.fm:8560/stream)
            import re
            match = re.search(r'//([^/:]+)', config.RADIOBOSS_STREAM_URL)
            if match:
                api_domain = match.group(1)
        
        # construct the API URL with the correct format
        url = f"https://{api_domain}/api/info/{station_id}"
        
        # prepare headers and params
        headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
            'Accept': 'application/json',
            'X-API-Key': config.RADIOBOSS_API_KEY
        }
        params = {"key": config.RADIOBOSS_API_KEY}
        
        try:
            # try with verify_ssl=False in case of SSL certificate issues
            connector = aiohttp.TCPConnector(ssl=False)
            timeout = aiohttp.ClientTimeout(total=10)  # 10 seconds timeout
            
            async with aiohttp.ClientSession(connector=connector, timeout=timeout, headers=headers) as session:
                # first try with headers and params
                async with session.get(url, params=params) as response:
                    response_text = await response.text()
                    
                    # try to parse as JSON regardless of content-type
                    try:
                        # first try to parse the response text as JSON
                        import json
                        data = json.loads(response_text)
                        
                        # if we got this far, the response is valid JSON
                        logger.debug(f"Successfully parsed JSON response from {url}")
                        
                        # check if the response has the expected structure
                        if isinstance(data, dict) and ('currenttrack_info' in data or 'nowplaying' in data):
                            return data
                        else:
                            logger.error(f"Unexpected API response format: {data}")
                            return None
                            
                    except json.JSONDecodeError as json_err:
                        # if we can't parse as JSON, log the error and response
                        logger.error(f"Failed to parse JSON response: {json_err}")
                        logger.error(f"Response content type: {response.content_type}")
                        logger.error(f"Response headers: {dict(response.headers)}")
                        logger.error(f"Response text: {response_text[:1000]}...")
                        return None
                        
        except asyncio.TimeoutError:
            logger.error(f"Timeout while connecting to RadioBOSS API at {url}")
            return None
        except Exception as e:
            logger.error(f"Error fetching RadioBOSS data from {url}: {str(e)}")
            return None
            
    def _format_duration(self, duration_str: str) -> str:
        """format duration string from MM:SS to a more readable format."""
        try:
            minutes, seconds = map(int, duration_str.split(':'))
            if minutes >= 60:
                hours = minutes // 60
                minutes = minutes % 60
                return f"{hours}h {minutes}m {seconds}s"
            return f"{minutes}m {seconds}s"
        except (ValueError, AttributeError):
            return duration_str
            
    def _format_timestamp(self, timestamp_str: str) -> str:
        """format a timestamp string to a more readable format."""
        try:
            dt = datetime.strptime(timestamp_str, "%Y-%m-%d %H:%M:%S")
            return dt.strftime("%I:%M %p").lstrip('0')  # 12-hour format without leading zero
        except (ValueError, AttributeError):
            return timestamp_str
    
    @app_commands.command(
        name="currently-playing",
        description="show information about the currently playing track"
    )
    async def currently_playing(self, interaction: discord.Interaction) -> None:
        """handle the /currently-playing slash command."""
        await interaction.response.defer(ephemeral=True)
        
        # create a loading embed
        embed = discord.Embed(
            title="🎵 Loading Track Info...",
            description="Fetching current track information from the radio...",
            color=0x2f3136
        )
        
        await interaction.followup.send(embed=embed, ephemeral=True)
        
        # fetch data from RadioBOSS API
        data = await self._fetch_radioboss_data()
        
        if not data:
            error_embed = discord.Embed(
                title="❌ Error",
                description="Could not fetch track information. The radio station might be offline or there was an error connecting.",
                color=0xe74c3c
            )
            await interaction.edit_original_response(embed=error_embed, view=None)
            return
            
        # create the main embed with current track info
        embed = discord.Embed(
            title="Playback Feed",
            color=0x2f3136,  # dark theme color
            timestamp=datetime.now()  # use local time instead of UTC
        )
        
        # set author with bot's name and icon
        embed.set_author(
            name=interaction.client.user.name,
            icon_url=interaction.client.user.display_avatar.url
        )
        
        # add footer
        embed.set_footer(text="livepartymusic.fm")
        
        # helper function to format track info
        def format_track_info(track_data, default_artist="Unknown Artist", default_title="Unknown Track"):
            if not track_data:
                return "N/A"
                
            artist = track_data.get('ARTIST', default_artist)
            title = track_data.get('TITLE', default_title)
            duration = self._format_duration(track_data.get('DURATION', '0:00'))
            
            return f"{artist} - {title}\n⏱️ {duration}"
        
        # get track data
        current_track = data.get('currenttrack_info', {}).get('@attributes', {})
        prev_track = data.get('prevtrack_info', {}).get('@attributes', {})
        next_track = data.get('nexttrack_info', {}).get('@attributes', {})
        
        # add track fields in a 3-column layout
        embed.add_field(
            name="Previous Track",
            value=format_track_info(prev_track, "No previous track", ""),
            inline=True
        )
        
        embed.add_field(
            name=":white_small_square: Current Track",
            value=format_track_info(current_track),
            inline=True
        )
        
        embed.add_field(
            name="Next Track",
            value=format_track_info(next_track, "No upcoming tracks", ""),
            inline=True
        )
        artwork_url = data.get('links', {}).get('artwork')
        if artwork_url:
            embed.set_thumbnail(url=artwork_url)
        
        # update the message with the final embed and remove any existing view
        await interaction.edit_original_response(embed=embed, view=None)

async def setup(bot: commands.Bot) -> None:
    """set up the voice cog."""
    await bot.add_cog(VoiceCog(bot))
    logger.info("voice cog loaded")