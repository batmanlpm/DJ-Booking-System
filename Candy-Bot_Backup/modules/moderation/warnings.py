"""warning system for moderation"""
import discord
from discord.ext import commands
from discord import app_commands
from typing import List, Optional, Dict, Any
import logging
from datetime import datetime
from modules.database.database import db

logger = logging.getLogger(__name__)

class Warnings(commands.Cog):
    """handles warning functionality"""
    
    def __init__(self, bot: commands.Bot) -> None:
        self.bot = bot
        self._create_tables()
    
    def _create_tables(self) -> None:
        """create necessary database tables if they don't exist"""
        try:
            db.execute_query("""
                CREATE TABLE IF NOT EXISTS warnings (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    guild_id INTEGER NOT NULL,
                    user_id INTEGER NOT NULL,
                    moderator_id INTEGER NOT NULL,
                    reason TEXT NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    active BOOLEAN DEFAULT 1
                )
            """, commit=True)
            logger.info("ensured warnings table exists")
        except Exception as e:
            logger.error(f"failed to create warnings table: {e}")
            raise
    
    def get_user_warnings(self, guild_id: int, user_id: int) -> List[Dict[str, Any]]:
        """get all active warnings for a user in a guild"""
        try:
            rows = db.execute_query(
                """
                SELECT id, moderator_id, reason, created_at 
                FROM warnings 
                WHERE guild_id = ? AND user_id = ? AND active = 1
                ORDER BY created_at DESC
                """,
                (guild_id, user_id),
                fetch=True
            )
            return [dict(row) for row in rows]
        except Exception as e:
            logger.error(f"failed to get user warnings: {e}")
            return []
    
    def add_warning(
        self, 
        guild_id: int, 
        user_id: int, 
        moderator_id: int, 
        reason: str
    ) -> int:
        """add a warning to a user and return the warning ID"""
        try:
            warning_id = db.execute_query(
                """
                INSERT INTO warnings (guild_id, user_id, moderator_id, reason)
                VALUES (?, ?, ?, ?)
                """,
                (guild_id, user_id, moderator_id, reason),
                commit=True
            )
            return warning_id
        except Exception as e:
            logger.error(f"failed to add warning: {e}")
            raise
    
    async def log_warning_embed(
        self,
        guild: discord.Guild,
        user: discord.Member,
        moderator: discord.Member,
        reason: str,
        warning_id: int,
        warning_count: int
    ) -> Optional[discord.Message]:
        """send a warning log to the mod log channel"""
        mod_log_cog = self.bot.get_cog("ModLog")
        if not mod_log_cog:
            return None
            
        # Get the mod log channel ID
        channel_id = mod_log_cog.get_mod_log_channel(guild.id)
        if not channel_id:
            return None
            
        channel = guild.get_channel(channel_id)
        if not channel or not isinstance(channel, discord.TextChannel):
            return None
            
        # Create and send the embed
        embed = discord.Embed(
            title=f"warning issued - case #{warning_id}",
            color=discord.Color.orange(),
            timestamp=discord.utils.utcnow()
        )
        
        embed.add_field(name="user", value=f"{user.mention} (`{user.id}`)", inline=False)
        embed.add_field(name="moderator", value=moderator.mention, inline=False)
        embed.add_field(name="reason", value=reason, inline=False)
        embed.add_field(name="total warnings", value=str(warning_count), inline=False)
        
        embed.set_author(name=str(user), icon_url=user.display_avatar.url)
        
        try:
            return await channel.send(embed=embed)
        except discord.HTTPException as e:
            logger.error(f"failed to send warning log: {e}")
            return None
    
    @app_commands.command(name="warn", description="warn a member")
    @app_commands.describe(
        member="the member to warn",
        reason="reason for the warning"
    )
    @app_commands.checks.has_permissions(manage_messages=True)
    async def warn_command(
        self, 
        interaction: discord.Interaction, 
        member: discord.Member,
        reason: Optional[str] = None
    ) -> None:
        """warn a member"""
        if not interaction.guild:
            return await interaction.response.send_message("this command can only be used in a server", ephemeral=True)
            
        # Check if the target is the guild owner or a bot
        if member == interaction.guild.owner:
            return await interaction.response.send_message("you cannot warn the server owner", ephemeral=True)
            
        if member.bot:
            return await interaction.response.send_message("you cannot warn bots", ephemeral=True)
            
        # Check if the target is higher in the role hierarchy
        if interaction.user != interaction.guild.owner and member.top_role >= interaction.user.top_role:
            return await interaction.response.send_message("you cannot warn someone with an equal or higher role", ephemeral=True)
            
        reason = reason or "No reason provided"
        
        # Add warning to database
        warning_id = self.add_warning(
            guild_id=interaction.guild.id,
            user_id=member.id,
            moderator_id=interaction.user.id,
            reason=reason
        )
        
        # Get warning count for the user
        warnings = self.get_user_warnings(interaction.guild.id, member.id)
        warning_count = len(warnings)
        
        # Log the warning
        log_message = await self.log_warning_embed(
            guild=interaction.guild,
            user=member,
            moderator=interaction.user,
            reason=reason,
            warning_id=warning_id,
            warning_count=warning_count
        )
        
        # Send response to moderator
        embed = discord.Embed(
            title=f"warning issued - case #{warning_id}",
            description=f"{member.mention} has been warned.",
            color=discord.Color.green()
        )
        
        if log_message and log_message.channel:
            embed.add_field(
                name="log",
                value=f"[view in mod logs]({log_message.jump_url})",
                inline=False
            )
            
        embed.add_field(
            name="total warnings",
            value=str(warning_count),
            inline=False
        )
        
        await interaction.response.send_message(embed=embed, ephemeral=True)
        
        # Try to DM the user
        try:
            dm_embed = discord.Embed(
                title=f"you have been warned in {interaction.guild.name}",
                description=f"**reason:** {reason or 'No reason provided'}",
                color=discord.Color.orange()
            )
            dm_embed.set_footer(text=f"warning #{warning_id} • total warnings: {warning_count}")
            await member.send(embed=dm_embed)
        except discord.Forbidden:
            pass  # User has DMs disabled or blocked the bot


def setup(bot: commands.Bot) -> None:
    """load the warnings cog"""
    bot.add_cog(Warnings(bot))
