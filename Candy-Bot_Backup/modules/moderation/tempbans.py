"""temporary ban functionality"""
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

class TempBan(commands.Cog):
    """handles temporary bans"""
    
    def __init__(self, bot: commands.Bot) -> None:
        self.bot = bot
        self._create_tables()
        self.check_temp_bans.start()
    
    def cog_unload(self) -> None:
        """cancel the background task when the cog is unloaded"""
        self.check_temp_bans.cancel()
    
    def _create_tables(self) -> None:
        """create necessary database tables if they don't exist"""
        try:
            # First drop the table if it exists to recreate it with new schema
            db.execute_query("DROP TABLE IF EXISTS temp_bans", commit=True)
            
            # Create the table with a unique constraint on (guild_id, user_id) where active = 1
            db.execute_query("""
                CREATE TABLE temp_bans (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    guild_id INTEGER NOT NULL,
                    user_id INTEGER NOT NULL,
                    moderator_id INTEGER NOT NULL,
                    reason TEXT,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    expires_at TIMESTAMP NOT NULL,
                    active BOOLEAN DEFAULT 1,
                    UNIQUE(guild_id, user_id) ON CONFLICT REPLACE
                )
            """, commit=True)
            
            # Create an index for faster lookups
            db.execute_query(
                "CREATE INDEX IF NOT EXISTS idx_temp_bans_guild_user ON temp_bans(guild_id, user_id)",
                commit=True
            )
            db.execute_query(
                "CREATE INDEX IF NOT EXISTS idx_temp_bans_expires ON temp_bans(expires_at) WHERE active = 1",
                commit=True
            )
            logger.info("ensured temp_bans table exists")
        except Exception as e:
            logger.error(f"failed to create temp_bans table: {e}")
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
    
    def add_temp_ban(
        self,
        guild_id: int,
        user_id: int,
        moderator_id: int,
        expires_at: datetime,
        reason: Optional[str] = None
    ) -> int:
        """add or update a temporary ban in the database"""
        try:
            # This will automatically replace any existing active ban due to the UNIQUE constraint
            db.execute_query(
                """
                INSERT OR REPLACE INTO temp_bans 
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
                SELECT id FROM temp_bans 
                WHERE guild_id = ? AND user_id = ?
                """,
                (guild_id, user_id),
                fetch=True
            )
            return result[0]['id'] if result else 0
            
        except Exception as e:
            logger.error(f"error in add_temp_ban: {e}", exc_info=True)
            return 0
    
    def remove_temp_ban(self, guild_id: int, user_id: int) -> bool:
        """remove a temporary ban from the database"""
        try:
            result = db.execute_query(
                """
                DELETE FROM temp_bans 
                WHERE guild_id = ? AND user_id = ?
                """,
                (guild_id, user_id),
                commit=True
            )
            return bool(result)
        except Exception as e:
            logger.error(f"error in remove_temp_ban: {e}", exc_info=True)
            return False
    
    @tasks.loop(seconds=30.0)
    async def check_temp_bans(self) -> None:
        """check for expired temp bans and unban users"""
        try:
            # Get all expired temp bans
            expired_bans = db.execute_query(
                """
                SELECT id, guild_id, user_id 
                FROM temp_bans 
                WHERE expires_at <= datetime('now')
                """,
                fetch=True
            )
            
            for ban in expired_bans:
                guild = self.bot.get_guild(ban['guild_id'])
                if not guild:
                    # Guild not found, remove the ban record
                    db.execute_query(
                        "DELETE FROM temp_bans WHERE id = ?",
                        (ban['id'],),
                        commit=True
                    )
                    continue
                
                try:
                    # Try to unban the user
                    user = await self.bot.fetch_user(ban['user_id'])
                    try:
                        await guild.unban(user, reason="Temporary ban expired")
                        logger.info(f"unbanned user {ban['user_id']} (ban expired)")
                    except discord.NotFound:
                        # User is already unbanned, just log it
                        logger.info(f"user {ban['user_id']} was already unbanned")
                    
                    # Log the unban
                    mod_actions = self.bot.get_cog("ModActions")
                    if mod_actions:
                        try:
                            await mod_actions.log_action_embed(
                                action="unban",
                                guild=guild,
                                user=user,
                                moderator=self.bot.user,
                                reason="Temporary ban expired"
                            )
                        except Exception as e:
                            logger.error(f"failed to log unban: {e}")
                    
                    # Remove the ban record after processing
                    db.execute_query(
                        "DELETE FROM temp_bans WHERE id = ?",
                        (ban['id'],),
                        commit=True
                    )
                    
                except Exception as e:
                    logger.error(f"error processing expired ban {ban['id']}: {e}", exc_info=True)
                    
        except Exception as e:
            logger.error(f"error in check_temp_bans: {e}", exc_info=True)
    
    @check_temp_bans.before_loop
    async def before_check_temp_bans(self) -> None:
        """wait for the bot to be ready before starting the task"""
        await self.bot.wait_until_ready()
    
    @app_commands.command(name="temp-ban", description="temporarily ban a user from the server")
    @app_commands.describe(
        user="the user to ban (can be an ID or mention)",
        duration="duration of the ban (e.g., 1d, 2h, 30m, 1w, 1M, 1y)",
        reason="reason for the ban"
    )
    @app_commands.checks.has_permissions(ban_members=True)
    async def temp_ban_command(
        self,
        interaction: discord.Interaction,
        user: discord.User,
        duration: str,
        reason: Optional[str] = None
    ) -> None:
        """temporarily ban a user from the server"""
        if not interaction.guild:
            return await interaction.response.send_message(
                "this command can only be used in a server",
                ephemeral=True
            )
            
        if user.bot:
            return await interaction.response.send_message(
                "you cannot ban bots",
                ephemeral=True
            )
            
        if user.id == interaction.user.id:
            return await interaction.response.send_message(
                "you cannot ban yourself",
                ephemeral=True
            )
            
        if interaction.guild.owner_id == user.id:
            return await interaction.response.send_message(
                "you cannot ban the server owner",
                ephemeral=True
            )
            
        # Check if the bot has permission to ban
        if not interaction.guild.me.guild_permissions.ban_members:
            return await interaction.response.send_message(
                "i don't have permission to ban members",
                ephemeral=True
            )
            
        # Check if the target member is higher in the role hierarchy
        member = interaction.guild.get_member(user.id)
        if member and member.top_role >= interaction.guild.me.top_role:
            return await interaction.response.send_message(
                "i can't ban someone with a role higher than or equal to mine",
                ephemeral=True
            )
            
        # Check if the target member is higher than the command invoker
        if (member and member.top_role >= interaction.user.top_role and 
                interaction.guild.owner_id != interaction.user.id):
            return await interaction.response.send_message(
                "you can't ban someone with a role higher than or equal to yours",
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
        
        # Check if the duration is too long (more than 1 year)
        if duration_delta > timedelta(days=365):
            return await interaction.response.send_message(
                "maximum ban duration is 1 year",
                ephemeral=True
            )
        
        # Defer the response since we're about to make API calls
        await interaction.response.defer(ephemeral=True)
        
        try:
            # Add the temp ban to the database
            temp_ban_id = self.add_temp_ban(
                guild_id=interaction.guild.id,
                user_id=user.id,
                moderator_id=interaction.user.id,
                expires_at=expires_at,
                reason=reason
            )
            
            # Format the expiration time for display
            expires_at_str = f"<t:{int(expires_at.timestamp())}:F>"
            duration_str = self.format_duration(duration_delta)
            
            # Log the ban
            mod_actions = self.bot.get_cog("ModActions")
            log_message = None
            if mod_actions:
                log_message = await mod_actions.log_action_embed(
                    action="tempban",
                    guild=interaction.guild,
                    user=user,
                    moderator=interaction.user,
                    reason=reason,
                    duration=duration_str,
                    expires_at=expires_at_str
                )
            
            # DM the user about the ban
            try:
                dm_embed = discord.Embed(
                    title=f"you have been temporarily banned from {interaction.guild.name}",
                    description=f"**reason:** {reason or 'No reason provided'}",
                    color=discord.Color.red()
                )
                dm_embed.add_field(name="duration", value=duration_str, inline=False)
                dm_embed.add_field(name="expires", value=expires_at_str, inline=False)
                if log_message and log_message.channel:
                    dm_embed.set_footer(text=f"case id: {log_message.id}")
                await user.send(embed=dm_embed)
            except (discord.Forbidden, discord.HTTPException) as e:
                logger.warning(f"failed to DM user {user.id} about temp ban: {e}")
            
            # Actually ban the user
            await interaction.guild.ban(
                user,
                reason=f"{interaction.user} (ID: {interaction.user.id}): {reason or 'No reason provided'}",
                delete_message_days=0
            )
            
            # Send confirmation to the moderator
            embed = discord.Embed(
                title=f"member temporarily banned - {user}",
                description=f"{user.mention} has been banned for {duration_str}.",
                color=discord.Color.green()
            )
            
            if log_message and log_message.channel:
                embed.add_field(
                    name="log",
                    value=f"[view in mod logs]({log_message.jump_url})",
                    inline=False
                )
                
            embed.add_field(name="expires", value=expires_at_str, inline=False)
            if reason:
                embed.add_field(name="reason", value=reason, inline=False)
            
            await interaction.followup.send(embed=embed, ephemeral=True)
            
        except discord.Forbidden:
            await interaction.followup.send(
                "i don't have permission to ban that user",
                ephemeral=True
            )
        except discord.HTTPException as e:
            logger.error(f"failed to ban user {user.id}: {e}")
            await interaction.followup.send(
                f"an error occurred while trying to ban {user.mention}",
                ephemeral=True
            )


def setup(bot: commands.Bot) -> None:
    """load the temp ban cog"""
    bot.add_cog(TempBan(bot))
