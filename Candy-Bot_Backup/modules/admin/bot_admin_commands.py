"""bot admin commands for global bot administration"""
import discord
from discord import app_commands
from discord.ext import commands
from typing import Optional, List
import logging
import config
from utils.bot_admin import is_bot_admin, add_bot_admin, remove_bot_admin

logger = logging.getLogger(__name__)

class BotAdminCog(commands.Cog):
    """bot admin commands for global bot administration"""
    
    def __init__(self, bot):
        self.bot = bot
        self.logger = logging.getLogger(f"{__name__}.BotAdminCog")
    
    @app_commands.command(name="admin-add", description="add a user as a bot admin (bot owner only)")
    @app_commands.describe(user="the user to add as a bot admin")
    @is_bot_admin()
    async def admin_add(self, interaction: discord.Interaction, user: discord.User):
        """add a user as a bot admin"""
        if user.id in config.BOT_ADMINS:
            await interaction.response.send_message(
                f"❌ {user.mention} is already a bot admin.",
                ephemeral=True
            )
            return
        
        await add_bot_admin(user.id)
        self.logger.info(f"Added {user} (ID: {user.id}) as a bot admin")
        
        await interaction.response.send_message(
            f"✅ {user.mention} has been added as a bot admin.",
            ephemeral=True
        )
    
    @app_commands.command(name="admin-remove", description="remove a user from bot admins (bot owner only)")
    @app_commands.describe(user="the user to remove from bot admins")
    @is_bot_admin()
    async def admin_remove(self, interaction: discord.Interaction, user: discord.User):
        """remove a user from bot admins"""
        if user.id not in config.BOT_ADMINS:
            await interaction.response.send_message(
                f"❌ {user.mention} is not a bot admin.",
                ephemeral=True
            )
            return
        
        # Prevent removing the last bot admin
        if len(config.BOT_ADMINS) <= 1:
            await interaction.response.send_message(
                "❌ Cannot remove the last bot admin.",
                ephemeral=True
            )
            return
        
        await remove_bot_admin(user.id)
        self.logger.info(f"Removed {user} (ID: {user.id}) from bot admins")
        
        await interaction.response.send_message(
            f"✅ {user.mention} has been removed from bot admins.",
            ephemeral=True
        )
    
    @app_commands.command(name="admin-list", description="list all bot admins")
    @is_bot_admin()
    async def admin_list(self, interaction: discord.Interaction):
        """list all bot admins"""
        if not config.BOT_ADMINS:
            await interaction.response.send_message(
                "❌ No bot admins configured.",
                ephemeral=True
            )
            return
        
        admin_list = []
        for admin_id in config.BOT_ADMINS:
            try:
                user = await self.bot.fetch_user(admin_id)
                admin_list.append(f"- {user.mention} (`{user}` | `{user.id}`)")
            except discord.NotFound:
                admin_list.append(f"- Unknown User (`{admin_id}`)")
        
        embed = discord.Embed(
            title="🤖 Bot Admins",
            description="\n".join(admin_list) or "No bot admins configured.",
            color=discord.Color.blue()
        )
        
        await interaction.response.send_message(embed=embed, ephemeral=True)

async def setup(bot):
    """set up the bot admin cog"""
    await bot.add_cog(BotAdminCog(bot))
