"""
anti-raid system implementation.
"""
import discord
from discord.ext import commands
from datetime import datetime, timedelta
from typing import Dict, List, Set, Optional
import asyncio
from dataclasses import dataclass

@dataclass
class MessageRecord:
    """track message history for rate limiting."""
    timestamp: datetime
    user_id: int
    channel_id: int

class AntiRaidSystem:
    """handles anti-raid detection and mitigation."""
    
    def __init__(self, bot):
        self.bot = bot
        self.message_history: List[MessageRecord] = []
        self.locked_channels: Set[int] = set()
        self.original_permissions: Dict[int, Dict[int, discord.PermissionOverwrite]] = {}
        self.default_settings = {
            'enabled': True,
            'message_threshold': 5,    # Number of messages to trigger
            'time_window': 10,         # Time window in seconds
            'lock_duration': 300,      # 5 minutes lock duration
            'exempt_roles': []         # Role IDs that are exempt from locking
        }
        self.guild_settings: Dict[int, dict] = {}
        self.cleanup_task = self.bot.loop.create_task(self._cleanup_old_messages())
        print(f"[AntiRaid] System initialized with default settings: {self.default_settings}")
    
    async def _cleanup_old_messages(self):
        """clean up old message records to prevent memory leaks."""
        while not self.bot.is_closed():
            try:
                now = datetime.utcnow()
                self.message_history = [
                    msg for msg in self.message_history
                    if now - msg.timestamp < timedelta(minutes=5)
                ]
            except Exception as e:
                print(f"Error in message cleanup: {e}")
            await asyncio.sleep(60)  # Run cleanup every minute
    
    def get_guild_settings(self, guild_id: int) -> dict:
        """get settings for a guild, with defaults where not set."""
        settings = self.guild_settings.get(guild_id, self.default_settings.copy())
        # ensure all settings exist
        for key, value in self.default_settings.items():
            if key not in settings:
                settings[key] = value
        return settings
    
    async def is_raid_detected(self, message: discord.Message) -> bool:
        """check if current message activity indicates a raid."""
        if message.guild is None or message.author.bot:
            return False
            
        settings = self.get_guild_settings(message.guild.id)
        if not settings['enabled']:
            print(f"[AntiRaid] Anti-raid is disabled for guild {message.guild.id}")
            return False
        
        print(f"[AntiRaid] Checking message in {message.channel} from {message.author}")
        
        # add current message to history
        record = MessageRecord(
            timestamp=datetime.utcnow(),
            user_id=message.author.id,
            channel_id=message.channel.id
        )
        self.message_history.append(record)
        
        # get recent messages in this channel
        time_threshold = datetime.utcnow() - timedelta(seconds=settings['time_window'])
        recent_messages = [
            msg for msg in self.message_history
            if msg.timestamp > time_threshold 
            and msg.channel_id == message.channel.id
        ]
        
        # get unique users
        unique_users = {msg.user_id for msg in recent_messages}
        
        print(f"[AntiRaid] Recent messages: {len(recent_messages)} (threshold: {settings['message_threshold']})")
        print(f"[AntiRaid] Unique users: {len(unique_users)}")
        
        # check if we have enough messages (testing with single user)
        if len(recent_messages) >= settings['message_threshold']:
            print(f"[AntiRaid] RAID DETECTED in {message.channel}!")
            print(f"[AntiRaid] Messages: {len(recent_messages)} from {len(unique_users)} users")
            return True
            
        return False
    
    async def lock_channel(self, channel: discord.TextChannel, reason: str = "Raid detected") -> bool:
        """lock a channel to prevent further messages for all roles."""
        if channel.id in self.locked_channels:
            print(f"[AntiRaid] Channel {channel} is already locked")
            return False
            
        guild = channel.guild
        settings = self.get_guild_settings(guild.id)
        
        print(f"[AntiRaid] attempting to lock channel {channel} in guild {guild}")
        
        try:
            # get all roles in the guild
            roles = guild.roles
            exempt_roles = [int(rid) for rid in settings.get('exempt_roles', []) if rid.isdigit()]
            
            # save current state for later restoration
            self.locked_channels.add(channel.id)
            self.original_permissions[channel.id] = {}
            
            # store current permissions for all roles
            for role in roles:
                try:
                    current_perms = channel.overwrites_for(role)
                    # Only store if there are any permission overrides
                    if current_perms:
                        self.original_permissions[channel.id][role.id] = current_perms
                except Exception as e:
                    print(f"[AntiRaid] Error storing permissions for role {role.name}: {e}")
            
            # update permissions for each role to prevent sending messages
            for role in roles:
                # skip exempt roles
                if role.id in exempt_roles:
                    print(f"[AntiRaid] Skipping exempt role: {role.name}")
                    continue
                    
                try:
                    # get current permissions for the role
                    current_perms = channel.overwrites_for(role)
                    
                    # create a new PermissionOverwrite with send_messages set to False
                    # and preserve all other permissions
                    new_perms = discord.PermissionOverwrite(
                        send_messages=False,
                        # preserve all other permissions from the original
                        read_messages=current_perms.read_messages,
                        view_channel=current_perms.view_channel,
                        send_messages_in_threads=current_perms.send_messages_in_threads,
                        create_public_threads=current_perms.create_public_threads,
                        create_private_threads=current_perms.create_private_threads,
                        embed_links=current_perms.embed_links,
                        attach_files=current_perms.attach_files,
                        add_reactions=current_perms.add_reactions,
                        use_external_emojis=current_perms.use_external_emojis,
                        use_external_stickers=current_perms.use_external_stickers,
                        mention_everyone=current_perms.mention_everyone,
                        manage_messages=current_perms.manage_messages,
                        manage_channels=current_perms.manage_channels,
                        manage_roles=current_perms.manage_roles,
                        manage_webhooks=current_perms.manage_webhooks,
                        read_message_history=current_perms.read_message_history
                    )
                    
                    # only update if the role doesn't already have send_messages=False
                    if current_perms.send_messages is not False:
                        await channel.set_permissions(
                            role,
                            overwrite=new_perms,
                            reason=f"Anti-raid: {reason}"
                        )
                        print(f"[AntiRaid] Locked channel for role: {role.name}")
                    
                except discord.Forbidden:
                    print(f"[AntiRaid] Missing permissions to modify permissions for role: {role.name}")
                except Exception as e:
                    print(f"[AntiRaid] Error updating permissions for role {role.name}: {e}")
            
            # also lock for @everyone if not already locked
            try:
                everyone = guild.default_role
                everyone_perms = channel.overwrites_for(everyone)
                
                # only update if @everyone can send messages
                if everyone_perms.send_messages is not False:
                    # create a new PermissionOverwrite for @everyone
                    new_everyone_perms = discord.PermissionOverwrite(
                        send_messages=False,
                        # preserve all other permissions from the original
                        read_messages=everyone_perms.read_messages,
                        view_channel=everyone_perms.view_channel,
                        send_messages_in_threads=everyone_perms.send_messages_in_threads,
                        create_public_threads=everyone_perms.create_public_threads,
                        create_private_threads=everyone_perms.create_private_threads,
                        embed_links=everyone_perms.embed_links,
                        attach_files=everyone_perms.attach_files,
                        add_reactions=everyone_perms.add_reactions,
                        use_external_emojis=everyone_perms.use_external_emojis,
                        use_external_stickers=everyone_perms.use_external_stickers,
                        mention_everyone=everyone_perms.mention_everyone,
                        manage_messages=everyone_perms.manage_messages,
                        manage_channels=everyone_perms.manage_channels,
                        manage_roles=everyone_perms.manage_roles,
                        manage_webhooks=everyone_perms.manage_webhooks,
                        read_message_history=everyone_perms.read_message_history
                    )
                    
                    await channel.set_permissions(
                        everyone,
                        overwrite=new_everyone_perms,
                        reason=f"Anti-raid: {reason}"
                    )
                    print("[AntiRaid] Locked channel for @everyone")
            except Exception as e:
                print(f"[AntiRaid] Error updating @everyone permissions: {e}")
                
            print(f"[AntiRaid] Successfully locked channel {channel}")
            
            # send notification
            try:
                await channel.send(
                    f"🔒 **Channel Locked**\n"
                    f"This channel has been locked due to possible raid activity. "
                    f"Please wait for a moderator to unlock it.\n"
                    f"*Reason: {reason}*"
                )
            except Exception as e:
                print(f"[AntiRaid] Could not send lock message: {e}")
            
            # schedule unlock after duration
            unlock_seconds = settings['lock_duration']
            print(f"[AntiRaid] Scheduling unlock for {channel} in {unlock_seconds} seconds")
            asyncio.create_task(self._schedule_unlock(channel, unlock_seconds))
            return True
            
        except Exception as e:
            print(f"[AntiRaid] Error locking channel {channel.id}: {e}")
            return False
    
    async def unlock_channel(self, channel: discord.TextChannel, reason: str = "Manually unlocked") -> bool:
        """unlock a previously locked channel and restore original permissions."""
        if channel.id not in self.locked_channels:
            print(f"[AntiRaid] Channel {channel} is not locked")
            return False
            
        try:
            print(f"[AntiRaid] Attempting to unlock channel {channel}")
            guild = channel.guild
            
            # restore original permissions for each role
            if channel.id in self.original_permissions:
                for role_id, perms in self.original_permissions[channel.id].items():
                    try:
                        role = guild.get_role(role_id)
                        if role:  # check if role still exists
                            # if the role had no permissions before, remove the override
                            if not any(perm for perm in perms if perm[1] is not None):
                                await channel.set_permissions(role, overwrite=None, reason=f"Restoring original permissions: {reason}")
                            else:
                                await channel.set_permissions(role, overwrite=perms, reason=f"Restoring original permissions: {reason}")
                            print(f"[AntiRaid] Restored permissions for role ID {role_id}")
                    except Exception as e:
                        print(f"[AntiRaid] Error restoring permissions for role ID {role_id}: {e}")
                
                # remove the stored permissions
                del self.original_permissions[channel.id]
            else:
                # if we don't have original permissions stored, just reset @everyone
                print(f"[AntiRaid] No original permissions found for channel {channel}, resetting @everyone")
                everyone = guild.default_role
                await channel.set_permissions(everyone, overwrite=None, reason=f"Resetting @everyone permissions: {reason}")
            
            # remove from locked channels
            self.locked_channels.discard(channel.id)
            
            # send notification
            try:
                await channel.send(
                    f"🔓 **Channel Unlocked**\n"
                    f"This channel has been unlocked.\n"
                    f"*{reason}*"
                )
            except Exception as e:
                print(f"[AntiRaid] Could not send unlock message: {e}")
            
            print(f"[AntiRaid] Successfully unlocked channel {channel}")
            return True
            
        except Exception as e:
            print(f"[AntiRaid] Error unlocking channel {channel.id}: {e}")
            return False
    
    async def _schedule_unlock(self, channel: discord.TextChannel, delay: int):
        """schedule automatic unlocking of a channel."""
        await asyncio.sleep(delay)
        await self.unlock_channel(channel, "Automatic unlock after anti-raid cooldown")
