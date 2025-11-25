import discord
from discord.ext import commands
from discord import app_commands
from typing import Optional
import logging
from modules.database.database import db

logger = logging.getLogger(__name__)

class ModLog(commands.Cog):
    def __init__(self, bot: commands.Bot) -> None:
        self.bot = bot
        self._create_tables()
        
    def _create_tables(self) -> None:
        """create necessary database tables if they don't exist"""
        try:
            db.execute_query(
                """
                CREATE TABLE IF NOT EXISTS mod_logs (
                    guild_id INTEGER PRIMARY KEY,
                    channel_id INTEGER NOT NULL
                )
                """,
                commit=True
            )
            logger.info("ensured mod_logs table exists")
        except Exception as e:
            logger.error(f"failed to create mod_logs table: {e}")
            raise
    
    def get_mod_log_channel(self, guild_id: int) -> Optional[int]:
        """get the mod log channel for a guild"""
        try:
            result = db.execute_query(
                "SELECT channel_id FROM mod_logs WHERE guild_id = ?",
                (guild_id,),
                fetch=True
            )
            return result[0]['channel_id'] if result else None
        except Exception as e:
            logger.error(f"failed to get mod log channel: {e}")
            return None
    
    def set_mod_log_channel(self, guild_id: int, channel_id: int) -> None:
        """set the mod log channel for a guild"""
        try:
            db.execute_query(
                """
                INSERT OR REPLACE INTO mod_logs (guild_id, channel_id)
                VALUES (?, ?)
                """,
                (guild_id, channel_id),
                commit=True
            )
            logger.info(f"set mod log channel to {channel_id} for guild {guild_id}")
        except Exception as e:
            logger.error(f"failed to set mod log channel: {e}")
            raise
    
    @app_commands.command(name="mod-log", description="set the channel for moderation logs")
    @app_commands.describe(channel="the channel to send moderation logs to")
    @app_commands.checks.has_permissions(administrator=True)
    async def mod_log(self, interaction: discord.Interaction, channel: discord.TextChannel) -> None:
        """set the channel for moderation logs"""
        if not interaction.guild:
            return await interaction.response.send_message("this command can only be used in a server", ephemeral=True)
            
        if not interaction.guild.owner_id == interaction.user.id:
            return await interaction.response.send_message(
                "only the server owner can use this command", 
                ephemeral=True
            )
            
        self.set_mod_log_channel(interaction.guild.id, channel.id)
        await interaction.response.send_message(
            f"moderation logs will be sent to {channel.mention}",
            ephemeral=True
        )
        
        # Send a test log
        embed = discord.Embed(
            title="moderation logging enabled",
            description=f"moderation actions will be logged in this channel",
            color=discord.Color.green()
        )
        embed.set_footer(text=f"set by {interaction.user}", icon_url=interaction.user.display_avatar.url)
        await channel.send(embed=embed)



async def setup(bot: commands.Bot) -> None:
    await bot.add_cog(ModLog(bot))
