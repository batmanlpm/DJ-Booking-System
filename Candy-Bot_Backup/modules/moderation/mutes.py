"""mute functionality for moderation"""
import discord
from discord.ext import commands, tasks
from discord import app_commands
from typing import Optional, Dict, List, Tuple
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

class Mute(commands.Cog):
    """handles muting and unmuting members"""
    
    def __init__(self, bot: commands.Bot) -> None:
        self.bot = bot
        self._create_tables()
        self.check_mutes.start()
    
    def cog_unload(self) -> None:
        """cancel the background task when the cog is unloaded"""
        self.check_mutes.cancel()
    
    def _create_tables(self) -> None:
        """create necessary database tables if they don't exist"""
        try:
            db.execute_query("""
                CREATE TABLE IF NOT EXISTS mutes (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    guild_id INTEGER NOT NULL,
                    user_id INTEGER NOT NULL,
                    moderator_id INTEGER NOT NULL,
                    reason TEXT,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    expires_at TIMESTAMP,
                    active BOOLEAN DEFAULT 1,
                    UNIQUE(guild_id, user_id, active)
                )
            """, commit=True)
            logger.info("ensured mutes table exists")
        except Exception as e:
            logger.error(f"failed to create mutes table: {e}")
            raise
    
    def parse_duration(self, duration_str: str) -> Optional[timedelta]:
        """parse a duration string into a timedelta"""
        # Check if the duration matches the pattern (e.g., 1d, 2h, 30m, etc.)
        match = re.match(r'^(\d+)([smhdwMy])$', duration_str.lower())
        if not match:
            return None
            
        amount = int(match.group(1))
        unit = match.group(2)
        
        if unit not in TIME_UNITS:
            return None
            
        return timedelta(seconds=amount * TIME_UNITS[unit])
    
    def format_duration(self, duration: timedelta) -> str:
        """format a timedelta into a human-readable string"""
        total_seconds = int(duration.total_seconds())
        
        if total_seconds < 60:
            return f"{total_seconds} second{'s' if total_seconds != 1 else ''}"
        elif total_seconds < 3600:
            minutes = total_seconds // 60
            return f"{minutes} minute{'s' if minutes != 1 else ''}"
        elif total_seconds < 86400:
            hours = total_seconds // 3600
            return f"{hours} hour{'s' if hours != 1 else ''}"
        elif total_seconds < 604800:
            days = total_seconds // 86400
            return f"{days} day{'s' if days != 1 else ''}"
        elif total_seconds < 2592000:  # 30 days
            weeks = total_seconds // 604800
            return f"{weeks} week{'s' if weeks != 1 else ''}"
        elif total_seconds < 31536000:  # 365 days
            months = total_seconds // 2592000
            return f"{months} month{'s' if months != 1 else ''}"
        else:
            years = total_seconds // 31536000
            return f"{years} year{'s' if years != 1 else ''}"
    
    async def get_mute_role(self, guild: discord.Guild) -> Optional[discord.Role]:
        """get or create the mute role for the guild"""
        # Try to find an existing mute role
        for role in guild.roles:
            if role.name.lower() == 'muted':
                return role
        
        # If no mute role exists, create one
        try:
            # Create the role
            mute_role = await guild.create_role(
                name='Muted',
                reason='Automatic mute role creation',
                color=discord.Color.dark_grey()
            )
            
            # Set up channel overrides
            for channel in guild.channels:
                try:
                    # Deny send messages and add reactions in text channels
                    if isinstance(channel, discord.TextChannel):
                        await channel.set_permissions(
                            mute_role,
                            send_messages=False,
                            add_reactions=False,
                            create_public_threads=False,
                            create_private_threads=False,
                            send_messages_in_threads=False
                        )
                    # Deny speaking in voice channels
                    elif isinstance(channel, discord.VoiceChannel):
                        await channel.set_permissions(
                            mute_role,
                            speak=False
                        )
                except (discord.Forbidden, discord.HTTPException) as e:
                    logger.warning(f"failed to set permissions for {channel.name}: {e}")
                    continue
            
            return mute_role
            
        except discord.Forbidden:
            logger.error("bot doesn't have permission to create mute role")
            return None
        except discord.HTTPException as e:
            logger.error(f"failed to create mute role: {e}")
            return None
    
    def add_mute(
        self,
        guild_id: int,
        user_id: int,
        moderator_id: int,
        expires_at: Optional[datetime] = None,
        reason: Optional[str] = None
    ) -> int:
        """add or update a mute in the database"""
        try:
            # This will automatically replace any existing active mute due to the UNIQUE constraint
            db.execute_query(
                """
                INSERT OR REPLACE INTO mutes 
                (guild_id, user_id, moderator_id, reason, expires_at, active)
                VALUES (?, ?, ?, ?, ?, 1)
                """,
                (guild_id, user_id, moderator_id, reason, expires_at),
                commit=True,
                fetch=False
            )
            
            # Get the ID of the inserted/updated row
            result = db.execute_query(
                """
                SELECT id FROM mutes 
                WHERE guild_id = ? AND user_id = ?
                """,
                (guild_id, user_id),
                fetch=True
            )
            return result[0]['id'] if result else 0
            
        except Exception as e:
            logger.error(f"error in add_mute: {e}", exc_info=True)
            return 0
    
    def remove_mute(self, guild_id: int, user_id: int) -> bool:
        """remove a mute from the database"""
        try:
            result = db.execute_query(
                """
                DELETE FROM mutes 
                WHERE guild_id = ? AND user_id = ?
                """,
                (guild_id, user_id),
                commit=True
            )
            return bool(result)
        except Exception as e:
            logger.error(f"error in remove_mute: {e}", exc_info=True)
            return False
    
    @tasks.loop(seconds=30.0)
    async def check_mutes(self) -> None:
        """check for expired mutes and unmute users"""
        try:
            # Get all expired mutes
            expired_mutes = db.execute_query(
                """
                SELECT id, guild_id, user_id 
                FROM mutes 
                WHERE active = 1 
                AND expires_at IS NOT NULL 
                AND expires_at <= datetime('now')
                """,
                fetch=True
            )
            
            for mute in expired_mutes:
                guild = self.bot.get_guild(mute['guild_id'])
                if not guild:
                    # Guild not found, remove the mute record
                    db.execute_query(
                        "DELETE FROM mutes WHERE id = ?",
                        (mute['id'],),
                        commit=True
                    )
                    continue
                    
                member = guild.get_member(mute['user_id'])
                if not member:
                    # Member not found, remove the mute record
                    db.execute_query(
                        "DELETE FROM mutes WHERE id = ?",
                        (mute['id'],),
                        commit=True
                    )
                    continue
                
                try:
                    # Remove the mute role
                    mute_role = await self.get_mute_role(guild)
                    if mute_role and mute_role in member.roles:
                        await member.remove_roles(mute_role, reason="Mute expired")
                    
                    # Log the unmute
                    mod_actions = self.bot.get_cog("ModActions")
                    if mod_actions:
                        try:
                            await mod_actions.log_action_embed(
                                action="unmute",
                                guild=guild,
                                user=member,
                                moderator=self.bot.user,
                                reason="Mute expired"
                            )
                        except Exception as e:
                            logger.error(f"failed to log unmute: {e}")
                    
                    # Remove the mute record after processing
                    db.execute_query(
                        "DELETE FROM mutes WHERE id = ?",
                        (mute['id'],),
                        commit=True
                    )
                    
                except Exception as e:
                    logger.error(f"error processing expired mute {mute['id']}: {e}", exc_info=True)
                    
        except Exception as e:
            logger.error(f"error in check_mutes: {e}", exc_info=True)
    
    @check_mutes.before_loop
    async def before_check_mutes(self) -> None:
        """wait for the bot to be ready before starting the task"""
        await self.bot.wait_until_ready()
    
    @app_commands.command(name="mute", description="mute a member")
    @app_commands.describe(
        member="the member to mute",
        duration="duration of the mute (e.g., 1d, 2h, 30m, 1w, 1M, 1y)",
        reason="reason for the mute"
    )
    @app_commands.checks.has_permissions(manage_roles=True)
    async def mute_command(
        self,
        interaction: discord.Interaction,
        member: discord.Member,
        duration: str,
        reason: Optional[str] = None
    ) -> None:
        """mute a member"""
        if not interaction.guild:
            return await interaction.response.send_message(
                "this command can only be used in a server",
                ephemeral=True
            )
            
        if member.bot:
            return await interaction.response.send_message(
                "you cannot mute bots",
                ephemeral=True
            )
            
        if member.id == interaction.user.id:
            return await interaction.response.send_message(
                "you cannot mute yourself",
                ephemeral=True
            )
            
        # Check if the bot has permission to manage roles
        if not interaction.guild.me.guild_permissions.manage_roles:
            return await interaction.response.send_message(
                "i don't have permission to manage roles",
                ephemeral=True
            )
            
        # Check if the target member is higher in the role hierarchy
        if member.top_role >= interaction.guild.me.top_role:
            return await interaction.response.send_message(
                "i can't mute someone with a role higher than or equal to mine",
                ephemeral=True
            )
            
        # Check if the target member is higher than the command invoker
        if member.top_role >= interaction.user.top_role and interaction.guild.owner_id != interaction.user.id:
            return await interaction.response.send_message(
                "you can't mute someone with a role higher than or equal to yours",
                ephemeral=True
            )
        
        # Parse the duration
        duration_delta = self.parse_duration(duration)
        if not duration_delta:
            return await interaction.response.send_message(
                "invalid duration format. use a number followed by s/m/h/d/w/M/y (e.g., 1d, 2h, 30m, 1w, 1M, 1y)",
                ephemeral=True
            )
        
        # Calculate the expiration time
        now = datetime.utcnow()
        expires_at = now + duration_delta
        
        # Get or create the mute role
        mute_role = await self.get_mute_role(interaction.guild)
        if not mute_role:
            return await interaction.response.send_message(
                "failed to get or create mute role. please make sure i have the 'manage_roles' permission.",
                ephemeral=True
            )
            
        # Check if the mute role is below the bot's highest role
        if mute_role >= interaction.guild.me.top_role:
            return await interaction.response.send_message(
                "the mute role is higher than or equal to my highest role. please move it lower in the role hierarchy.",
                ephemeral=True
            )
            
        # Defer the response since we're about to make API calls
        await interaction.response.defer(ephemeral=True)
        
        try:
            # Add the mute to the database
            mute_id = self.add_mute(
                guild_id=interaction.guild.id,
                user_id=member.id,
                moderator_id=interaction.user.id,
                expires_at=expires_at,
                reason=reason
            )
            
            if not mute_id:
                return await interaction.followup.send(
                    "failed to add mute to database. please try again.",
                    ephemeral=True
                )
            
            # Add the mute role to the member
            await member.add_roles(mute_role, reason=f"Muted by {interaction.user} for {duration}: {reason or 'No reason provided'}")
            
            # Format the expiration time for display
            expires_at_str = f"<t:{int(expires_at.timestamp())}:F>" if duration_delta else "Never"
            duration_str = self.format_duration(duration_delta) if duration_delta else "Permanent"
            
            # Log the mute
            mod_actions = self.bot.get_cog("ModActions")
            log_message = None
            if mod_actions:
                log_message = await mod_actions.log_action_embed(
                    action="mute",
                    guild=interaction.guild,
                    user=member,
                    moderator=interaction.user,
                    reason=reason,
                    duration=duration_str,
                    expires_at=expires_at_str
                )
            
            # DM the user about the mute
            try:
                dm_embed = discord.Embed(
                    title=f"you have been muted in {interaction.guild.name}",
                    description=f"**reason:** {reason or 'No reason provided'}",
                    color=discord.Color.orange()
                )
                dm_embed.add_field(name="duration", value=duration_str, inline=False)
                if duration_delta:
                    dm_embed.add_field(name="expires", value=expires_at_str, inline=False)
                if log_message and log_message.channel:
                    dm_embed.set_footer(text=f"case id: {log_message.id}")
                await member.send(embed=dm_embed)
            except (discord.Forbidden, discord.HTTPException) as e:
                logger.warning(f"failed to DM user {member.id} about mute: {e}")
            
            # Send confirmation to the moderator
            embed = discord.Embed(
                title=f"member muted - {member}",
                description=f"{member.mention} has been muted for {duration_str}.",
                color=discord.Color.green()
            )
            
            if log_message and log_message.channel:
                embed.add_field(name="log", value=f"[jump to log]({log_message.jump_url})", inline=False)
                
            await interaction.followup.send(embed=embed, ephemeral=True)
            
        except discord.Forbidden:
            await interaction.followup.send(
                "i don't have permission to mute that user. please check my permissions and role hierarchy.",
                ephemeral=True
            )
        except Exception as e:
            logger.error(f"error in mute_command: {e}", exc_info=True)
            await interaction.followup.send(
                "an error occurred while muting the user. please try again.",
                ephemeral=True
            )
    
    @app_commands.command(name="unmute", description="unmute a muted member")
    @app_commands.describe(
        member="the member to unmute",
        reason="reason for the unmute"
    )
    @app_commands.checks.has_permissions(manage_roles=True)
    async def unmute_command(
        self,
        interaction: discord.Interaction,
        member: discord.Member,
        reason: Optional[str] = None
    ) -> None:
        """unmute a muted member"""
        # Check if the member is in the guild
        if not member.guild or member.guild.id != interaction.guild.id:
            return await interaction.response.send_message("member not found in this server", ephemeral=True)
            
        # Check if the command invoker has permission to unmute the target
        if member.top_role >= interaction.user.top_role and interaction.guild.owner_id != interaction.user.id:
            return await interaction.response.send_message(
                "you can't unmute someone with a role higher than or equal to yours",
                ephemeral=True
            )
            
        # Check if the bot has permission to manage roles
        if not interaction.guild.me.guild_permissions.manage_roles:
            return await interaction.response.send_message(
                "i don't have permission to manage roles",
                ephemeral=True
            )
            
        # Get the mute role
        mute_role = await self.get_mute_role(interaction.guild)
        if not mute_role:
            return await interaction.response.send_message(
                "mute role not found. please set up a mute role first.",
                ephemeral=True
            )
            
        # Check if the member is actually muted
        if mute_role not in member.roles:
            return await interaction.response.send_message(
                "that member is not muted.",
                ephemeral=True
            )
            
        # Defer the response since we're about to make API calls
        await interaction.response.defer(ephemeral=True)
        
        try:
            # Remove the mute from the database
            removed = self.remove_mute(interaction.guild.id, member.id)
            
            # Remove the mute role from the member
            await member.remove_roles(mute_role, reason=f"Unmuted by {interaction.user}: {reason or 'No reason provided'}")
            
            # Log the unmute
            mod_actions = self.bot.get_cog("ModActions")
            log_message = None
            if mod_actions:
                log_message = await mod_actions.log_action_embed(
                    action="unmute",
                    guild=interaction.guild,
                    user=member,
                    moderator=interaction.user,
                    reason=reason or "No reason provided"
                )
            
            # DM the user about the unmute
            try:
                dm_embed = discord.Embed(
                    title=f"you have been unmuted in {interaction.guild.name}",
                    description=f"**reason:** {reason or 'No reason provided'}",
                    color=discord.Color.green()
                )
                if log_message and log_message.channel:
                    dm_embed.set_footer(text=f"case id: {log_message.id}")
                await member.send(embed=dm_embed)
            except (discord.Forbidden, discord.HTTPException) as e:
                logger.warning(f"failed to DM user {member.id} about unmute: {e}")
            
            # Send confirmation to the moderator
            embed = discord.Embed(
                title=f"member unmuted - {member}",
                description=f"{member.mention} has been unmuted.",
                color=discord.Color.green()
            )
            
            if log_message and log_message.channel:
                embed.add_field(name="log", value=f"[jump to log]({log_message.jump_url})", inline=False)
                
            await interaction.followup.send(embed=embed, ephemeral=True)
            
        except discord.Forbidden:
            await interaction.followup.send(
                "i don't have permission to unmute that user. please check my permissions and role hierarchy.",
                ephemeral=True
            )
        except Exception as e:
            logger.error(f"error in unmute_command: {e}", exc_info=True)
            await interaction.followup.send(
                "an error occurred while unmuting the user. please try again.",
                ephemeral=True
            )


def setup(bot: commands.Bot) -> None:
    """load the mutes cog"""
    bot.add_cog(Mute(bot))
