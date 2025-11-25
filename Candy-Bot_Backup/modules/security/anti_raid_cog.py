"""
anti-raid system cog.
"""
import discord
from discord.ext import commands
from typing import Optional
import asyncio
from .anti_raid import AntiRaidSystem

class AntiRaidCog(commands.Cog):
    """anti-raid cog."""
    
    def __init__(self, bot):
        self.bot = bot
        self.anti_raid = AntiRaidSystem(bot)
        self._load_settings_task = self.bot.loop.create_task(self._load_all_guild_settings())
        
    @commands.Cog.listener()
    async def on_message(self, message: discord.Message):
        """check messages for potential raid activity."""
        if not message.guild or message.author.bot:
            return
            
        # check if message is from an admin or exempt role
        if await self.is_admin(message.author):
            print(f"Message from admin/exempt user {message.author} - skipping raid check")
            return
            
        # check for raid activity
        raid_detected = await self.anti_raid.is_raid_detected(message)
        print(f"Raid detected: {raid_detected} for message in {message.channel}")
        if raid_detected:
            print(f"Locking channel {message.channel} due to raid detection")
            await self.anti_raid.lock_channel(message.channel, "Possible raid detected")
    
    async def _load_all_guild_settings(self):
        """load all guild settings from the database."""
        await self.bot.wait_until_ready()
        try:
            from modules.database.database import db
            # load guild settings from database
            results = db.execute_query(
                "SELECT guild_id, enabled, message_threshold, time_window, lock_duration, exempt_roles "
                "FROM anti_raid_settings",
                fetch=True
            )
            
            for row in results:
                guild_id = row[0]
                self.anti_raid.guild_settings[guild_id] = {
                    'enabled': bool(row[1]),
                    'message_threshold': int(row[2]) if row[2] is not None else 10,
                    'time_window': int(row[3]) if row[3] is not None else 5,
                    'lock_duration': int(row[4]) if row[4] is not None else 300,
                    'exempt_roles': [str(role_id) for role_id in (row[5] or '').split(',') if role_id]
                }
                
        except Exception as e:
            print(f"Error loading anti-raid settings: {e}")
    
    async def is_admin(self, member: discord.Member) -> bool:
        """check if a member is an admin or has an exempt role."""
        # check if member has admin permissions
        if member.guild_permissions.administrator:
            return True
            
        # check against custom admin roles in the database
        try:
            from modules.database.database import db
            # check each role individually for admin permissions
            for role in member.roles:
                result = db.execute_query(
                    "SELECT 1 FROM bot_admins WHERE role_id = ? AND guild_id = ?",
                    (str(role.id), str(member.guild.id)),
                    fetch=True
                )
                if result:
                    return True
                
            # check exempt roles from anti-raid settings
            settings = self.anti_raid.get_guild_settings(member.guild.id)
            exempt_roles = settings.get('exempt_roles', [])
            if any(str(role.id) in exempt_roles for role in member.roles):
                return True
                
            return False
            
        except Exception as e:
            print(f"Error checking admin status: {e}")
            return False
    
    async def lock_channel(self, channel: discord.TextChannel, reason: str) -> bool:
        """lock a channel."""
        return await self.anti_raid.lock_channel(channel, reason)
    
    async def unlock_channel(self, channel: discord.TextChannel, reason: str) -> bool:
        """unlock a channel."""
        return await self.anti_raid.unlock_channel(channel, reason)

async def setup(bot):
    """set up the anti-raid cog."""
    await bot.add_cog(AntiRaidCog(bot))
