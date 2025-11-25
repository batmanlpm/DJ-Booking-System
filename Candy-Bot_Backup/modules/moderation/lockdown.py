"""lockdown functionality for moderation"""
import discord
import logging

# Set up logging
logging.basicConfig(level=logging.DEBUG)
logger = logging.getLogger(__name__)
logger.setLevel(logging.DEBUG)

# Create console handler with a higher log level
ch = logging.StreamHandler()
ch.setLevel(logging.DEBUG)

# Create formatter and add it to the handlers
formatter = logging.Formatter('%(asctime)s - %(name)s - %(levelname)s - %(message)s')
ch.setFormatter(formatter)

# Add the handlers to the logger
logger.addHandler(ch)
logger.propagate = False  # Prevent duplicate logs in Jupyter notebooks

import discord
from discord.ext import commands, tasks
from discord import app_commands
from typing import Optional, Dict, List, Tuple, Union, Any
import logging
from datetime import datetime, timedelta
import re
import asyncio
from modules.database.database import db

logger = logging.getLogger(__name__)

# Time conversion factors (in seconds)
TIME_UNITS = {
    's': 1,
    'm': 60,
    'h': 60 * 60,
    'd': 24 * 60 * 60,
    'w': 7 * 24 * 60 * 60,
    'M': 30 * 24 * 60 * 60,  # Approximate month
    'y': 365 * 24 * 60 * 60,  # Approximate year
}

class Lockdown(commands.Cog):
    """handles channel and server lockdowns"""
    
    def __init__(self, bot: commands.Bot) -> None:
        self.bot = bot
        self._create_tables()
        self.active_lockdowns: Dict[int, asyncio.Task] = {}
        self.check_lockdowns.start()
    
    def cog_unload(self) -> None:
        """cancel the background task when the cog is unloaded"""
        self.check_lockdowns.cancel()
        for task in self.active_lockdowns.values():
            task.cancel()
    
    def _create_tables(self) -> None:
        """create necessary database tables if they don't exist"""
        try:
            # Create lockdowns table
            db.execute_query("""
                -- Create the lockdowns table if it doesn't exist
                CREATE TABLE IF NOT EXISTS lockdowns (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    guild_id INTEGER NOT NULL,
                    target_id INTEGER NOT NULL,
                    moderator_id INTEGER NOT NULL,
                    reason TEXT,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    expires_at TIMESTAMP,
                    active BOOLEAN DEFAULT 1,
                    target_type TEXT NOT NULL, -- 'channel' or 'server'
                    UNIQUE(guild_id, target_id, active)
                )
            """, commit=True)
            logger.info("ensured lockdowns table exists")
            
            # Create lockdown_permissions table
            db.execute_query("""
                CREATE TABLE IF NOT EXISTS lockdown_permissions (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    lockdown_id INTEGER NOT NULL,
                    permission_type TEXT NOT NULL,
                    allow BOOLEAN DEFAULT 0,
                    FOREIGN KEY (lockdown_id) REFERENCES lockdowns(id) ON DELETE CASCADE
                )
            """, commit=True)
            logger.info("ensured lockdown_permissions table exists")
            
        except Exception as e:
            logger.error(f"failed to create lockdown tables: {e}")
            raise

    @tasks.loop(minutes=1)
    async def check_lockdowns(self) -> None:
        """check for expired lockdowns and remove them"""
        try:
            now = datetime.utcnow()
            expired = db.execute_query(
                "SELECT id, guild_id, target_id, target_type FROM lockdowns WHERE expires_at <= ? AND active = 1",
                (now,),
                fetch=True
            )
            
            for row in expired:
                lockdown_id = row['id']
                guild = self.bot.get_guild(row['guild_id'])
                if not guild:
                    continue
                    
                if row['target_type'] == 'channel':
                    target = guild.get_channel(row['target_id'])
                else:
                    target = guild
                    
                if target:
                    try:
                        await self.remove_lockdown(guild, target, "Lockdown expired")
                        logger.info(f"removed expired lockdown for {target} in {guild}")
                    except Exception as e:
                        logger.error(f"failed to remove expired lockdown: {e}")
                
                # Mark as inactive
                db.execute_query(
                    "UPDATE lockdowns SET active = 0 WHERE id = ?",
                    (lockdown_id,),
                    commit=True
                )
                
        except Exception as e:
            logger.error(f"error checking lockdowns: {e}")

    async def _save_lockdown_to_db(self, guild_id: int, target_id: int, moderator_id: int, 
                                 reason: str, expires_at: Optional[datetime], target_type: str) -> int:
        """save lockdown details to database"""
        try:
            # First, end any existing active lockdown for this target
            db.execute_query(
                "UPDATE lockdowns SET active = 0 WHERE guild_id = ? AND target_id = ? AND active = 1",
                (guild_id, target_id),
                commit=True
            )
            
            # Insert new lockdown
            cursor = db.execute_query(
                """
                INSERT INTO lockdowns 
                (guild_id, target_id, moderator_id, reason, expires_at, target_type, active)
                VALUES (?, ?, ?, ?, ?, ?, 1)
                RETURNING id
                """,
                (guild_id, target_id, moderator_id, reason, expires_at, target_type)
            )
            
            lockdown_id = cursor.fetchone()['id']
            logger.info(f"saved lockdown to database with id {lockdown_id}")
            return lockdown_id
            
        except Exception as e:
            logger.error(f"failed to save lockdown to database: {e}")
            raise

    async def _apply_lockdown_permissions(self, target: Union[discord.TextChannel, discord.Guild]) -> None:
        """apply lockdown permissions to a channel or server"""
        try:
            # Default permissions for lockdown
            overwrites = {
                'send_messages': False,
                'add_reactions': False,
                'create_public_threads': False,
                'create_private_threads': False,
                'send_messages_in_threads': False,
                'send_tts_messages': False,
                'use_application_commands': False,
                'connect': False,  # For voice channels
                'speak': False    # For voice channels
            }
            
            if isinstance(target, discord.TextChannel):
                # For text channels, apply to @everyone
                await target.edit(overwrites={
                    target.guild.default_role: discord.PermissionOverwrite(**overwrites),
                    target.guild.me: discord.PermissionOverwrite(
                        send_messages=True,
                        manage_messages=True,
                        manage_channels=True
                    )
                })
                logger.info(f"applied lockdown to channel {target.name} ({target.id})")
                
            elif isinstance(target, discord.Guild):
                # For server-wide lockdown, apply to all text channels
                for channel in target.text_channels:
                    try:
                        await channel.edit(overwrites={
                            target.default_role: discord.PermissionOverwrite(**overwrites),
                            target.me: discord.PermissionOverwrite(
                                send_messages=True,
                                manage_messages=True,
                                manage_channels=True
                            )
                        })
                        logger.info(f"applied lockdown to channel {channel.name} ({channel.id}) in {target.name}")
                    except Exception as e:
                        logger.error(f"failed to lockdown channel {channel.name} ({channel.id}): {e}")
                        continue
                
                # Also lock voice channels
                for channel in target.voice_channels:
                    try:
                        await channel.edit(overwrites={
                            target.default_role: discord.PermissionOverwrite(
                                connect=False,
                                speak=False
                            ),
                            target.me: discord.PermissionOverwrite(
                                connect=True,
                                speak=True,
                                move_members=True
                            )
                        })
                        logger.info(f"applied voice lockdown to channel {channel.name} ({channel.id}) in {target.name}")
                    except Exception as e:
                        logger.error(f"failed to lockdown voice channel {channel.name} ({channel.id}): {e}")
                        continue
        
        except Exception as e:
            logger.error(f"failed to apply lockdown permissions: {e}")
            raise

    async def _remove_lockdown_permissions(self, target: Union[discord.TextChannel, discord.Guild]) -> None:
        """remove lockdown permissions from a channel or server"""
        try:
            if isinstance(target, discord.TextChannel):
                # Reset channel to inherit permissions
                await target.edit(overwrites={})
                logger.info(f"removed lockdown from channel {target.name} ({target.id})")
                
            elif isinstance(target, discord.Guild):
                # Reset all text channels
                for channel in target.text_channels + target.voice_channels:
                    try:
                        await channel.edit(overwrites={})
                        logger.info(f"removed lockdown from channel {channel.name} ({channel.id}) in {target.name}")
                    except Exception as e:
                        logger.error(f"failed to remove lockdown from channel {channel.name} ({channel.id}): {e}")
                        continue
                        
        except Exception as e:
            logger.error(f"failed to remove lockdown permissions: {e}")
            raise

    async def remove_lockdown(self, guild: discord.Guild, target: Union[discord.TextChannel, discord.Guild], reason: str) -> None:
        """remove a lockdown from a channel or server"""
        try:
            target_id = target.id if isinstance(target, discord.TextChannel) else guild.id
            target_type = 'channel' if isinstance(target, discord.TextChannel) else 'server'
            
            # Remove the lockdown permissions
            await self._remove_lockdown_permissions(target)
            
            # Update the database
            db.execute_query(
                "UPDATE lockdowns SET active = 0 WHERE guild_id = ? AND target_id = ? AND active = 1",
                (guild.id, target_id),
                commit=True
            )
            
            # Prepare log message
            target_mention = target.mention if hasattr(target, 'mention') else 'server'
            log_message = f"🔓 Lockdown lifted from {target_type} {target_mention}"
            if reason:
                log_message += f"\n**reason:** {reason}"
            
            # Log to console
            logger.info(log_message)
            
            # Try to send to mod-log channel
            try:
                logger.debug(f"[DEBUG] Looking for mod-log channel in guild {guild.name} ({guild.id})")
                log_channel = discord.utils.get(guild.channels, name='mod-logs')
                
                if log_channel:
                    logger.debug(f"[DEBUG] Found mod-log channel: {log_channel.name} (ID: {log_channel.id})")
                    logger.debug(f"[DEBUG] Checking permissions for bot in channel {log_channel.name}")
                    
                    perms = log_channel.permissions_for(guild.me)
                    logger.debug(f"[DEBUG] Bot permissions in {log_channel.name}: {dict(perms)}")
                    
                    if perms.send_messages:
                        logger.debug("[DEBUG] Bot has send_messages permission, preparing embed")
                        
                        try:
                            moderator = guild.get_member(interaction.user.id) if interaction.user else None
                            moderator_mention = moderator.mention if moderator else 'Unknown'
                            
                            embed = discord.Embed(
                                title=f"🔓 {target_type.capitalize()} Lockdown Lifted",
                                description=(
                                    f"**{'Channel:' if target_type == 'channel' else 'Server:'}** {target_mention}\n"
                                    f"**Moderator:** {moderator_mention}\n"
                                    f"**Reason:** {reason or 'No reason provided'}"
                                ),
                                color=discord.Color.green(),
                                timestamp=datetime.utcnow()
                            )
                            
                            logger.debug("[DEBUG] Sending embed to mod-log channel")
                            await log_channel.send(embed=embed)
                            logger.info("[DEBUG] Successfully sent embed to mod-log channel")
                            
                        except Exception as embed_error:
                            logger.error(f"[DEBUG] Failed to create/send embed: {embed_error}", exc_info=True)
                            raise
                    else:
                        logger.warning("[DEBUG] Bot lacks send_messages permission in mod-log channel")
                else:
                    logger.warning("[DEBUG] No mod-log channel found in guild")
                    
            except Exception as e:
                logger.error(f"[DEBUG] Failed to handle mod-log channel: {e}", exc_info=True)
                # Fallback to console logging
                logger.info(f"[MOD-LOG FALLBACK] {log_message}")
                raise
            
        except Exception as e:
            logger.error(f"failed to remove lockdown: {e}")
            raise

    @app_commands.command(name="lockdown", description="lock down a channel or server (prevents sending messages)")
    @app_commands.checks.has_permissions(manage_channels=True)
    @app_commands.checks.bot_has_permissions(manage_channels=True, manage_roles=True)
    @app_commands.describe(
        target="the channel to lock down (leave empty for server-wide)",
        duration="how long to lock down for (e.g., 1h, 30m, 1d)",
        reason="reason for the lockdown"
    )
    async def lockdown(
        self,
        interaction: discord.Interaction,
        target: Optional[discord.TextChannel] = None,
        duration: Optional[str] = None,
        reason: Optional[str] = None
    ) -> None:
        """lock down a channel or the entire server"""
        logger.info(f"[LOCKDOWN] Command triggered by {interaction.user} in {interaction.guild}")
        
        # Defer the response to avoid the "application did not respond" error
        try:
            await interaction.response.defer(ephemeral=True)
            logger.info("[LOCKDOWN] Deferred response")
        except Exception as e:
            logger.error(f"[LOCKDOWN] Failed to defer response: {e}")
            return
        
        guild = interaction.guild
        if not guild:
            logger.error("[LOCKDOWN] Command not used in a guild")
            try:
                await interaction.followup.send("❌ this command can only be used in a server.", ephemeral=True)
            except Exception as e:
                logger.error(f"[LOCKDOWN] Failed to send error message: {e}")
            return
        
        # Determine if this is a channel or server lockdown
        is_channel_lockdown = target is not None
        target_name = f"channel {target.mention}" if is_channel_lockdown else "server"
        logger.info(f"[LOCKDOWN] Processing {target_name} lockdown in {guild.name} (ID: {guild.id})")
        
        # Process duration if provided
        duration_seconds = None
        if duration:
            try:
                duration_seconds = self._parse_duration(duration)
                logger.info(f"[LOCKDOWN] Parsed duration: {duration} = {duration_seconds} seconds")
            except ValueError as e:
                logger.error(f"[LOCKDOWN] Invalid duration format: {e}")
                try:
                    await interaction.followup.send("❌ invalid duration format. use format like '1h', '30m', '1d', etc.", ephemeral=True)
                except Exception as e:
                    logger.error(f"[LOCKDOWN] Failed to send error message: {e}")
                return
        
        # Calculate expiration time
        expires_at = None
        if duration_seconds:
            expires_at = datetime.utcnow() + timedelta(seconds=duration_seconds)
            logger.info(f"[LOCKDOWN] Lockdown will expire at {expires_at} (UTC)")
        
        # Log the lockdown attempt
        logger.info(
            f"[LOCKDOWN] Attempting to lock down {target_name} in {guild.name} (ID: {guild.id}) for "
            f"reason: {reason or 'No reason provided'}, duration: {duration or 'indefinite'}"
        )
        
        try:
            # Send a response to the user
            response_msg = f"🔒 {'Channel' if is_channel_lockdown else 'Server'} lockdown initiated"
            if duration:
                response_msg += f" for {duration}"
            if reason:
                response_msg += f"\n**reason:** {reason}"
                
            await interaction.followup.send(response_msg, ephemeral=True)
            logger.info("[LOCKDOWN] Sent initial response to user")
            
            # Apply the lockdown permissions
            try:
                await self._apply_lockdown_permissions(target or guild)
                
                # Save to database
                target_id = target.id if target else guild.id
                target_type = 'channel' if target else 'server'
                
                await self._save_lockdown_to_db(
                    guild_id=guild.id,
                    target_id=target_id,
                    moderator_id=interaction.user.id,
                    reason=reason or 'No reason provided',
                    expires_at=expires_at,
                    target_type=target_type
                )
                
                # Prepare log message
                log_message = f"🔒 {'Channel' if is_channel_lockdown else 'Server'} lockdown initiated"
                if duration:
                    log_message += f" for {duration}"
                if reason:
                    log_message += f"\n**reason:** {reason}"
                log_message += f"\n**moderator:** {interaction.user.mention}"
                
                # Log to console
                logger.info(log_message)
                
                # Try to send to mod-log channel
                try:
                    log_channel = discord.utils.get(guild.channels, name='mod-logs')
                    if log_channel and log_channel.permissions_for(guild.me).send_messages:
                        embed = discord.Embed(
                            title=f"🔒 {'Channel' if is_channel_lockdown else 'Server'} Lockdown",
                            description=f"**{'Channel:' if is_channel_lockdown else 'Server:'}** {target.mention if target else 'All channels'}\n"
                                      f"**Moderator:** {interaction.user.mention}\n"
                                      f"**Reason:** {reason or 'No reason provided'}\n"
                                      f"**Duration:** {duration or 'Indefinite'}",
                            color=discord.Color.red(),
                            timestamp=datetime.utcnow()
                        )
                        embed.set_footer(text=f"Moderator ID: {interaction.user.id}")
                        await log_channel.send(embed=embed)
                except Exception as e:
                    logger.warning(f"failed to send to mod-log channel: {e}")
                    # Also log to console if mod-log fails
                    logger.info(f"[MOD-LOG] {log_message}")
                
                logger.info(f"[LOCKDOWN] Successfully locked down {target_name} in {guild.name}")
                
            except Exception as e:
                logger.error(f"[LOCKDOWN] Failed to apply lockdown: {e}")
                try:
                    await interaction.followup.send("❌ failed to apply lockdown. please check the logs for details.", ephemeral=True)
                except Exception as e:
                    logger.error(f"[LOCKDOWN] Failed to send error message: {e}")
                return
            
        except Exception as e:
            logger.error(f"[LOCKDOWN] Error during lockdown: {e}", exc_info=True)
            try:
                await interaction.followup.send("❌ an error occurred while processing the lockdown.", ephemeral=True)
            except Exception as e:
                logger.error(f"[LOCKDOWN] Failed to send error message: {e}")

    def _parse_duration(self, duration: str) -> int:
        """parse a duration string into seconds"""
        match = re.match(r"(\d+)([smhdwMy])", duration)
        if not match:
            raise ValueError("Invalid duration format")
        
        value = int(match.group(1))
        unit = match.group(2)
        
        return value * TIME_UNITS[unit]

async def setup(bot: commands.Bot) -> None:
    """load the Lockdown cog"""
    await bot.add_cog(Lockdown(bot))
