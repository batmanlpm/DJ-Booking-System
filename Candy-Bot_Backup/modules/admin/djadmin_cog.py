"""
DJ Admin Cog

provides admin access to DJ management tools via a secure pin.
"""

import discord
from discord.ext import commands
from discord import app_commands
from config import DJ_ADMIN_PIN, BOT_ADMINS

class PinModal(discord.ui.Modal, title="admin authentication"):
    """modal for entering the admin pin."""
    
    pin = discord.ui.TextInput(
        label="enter admin pin",
        placeholder="enter the 4-digit pin",
        min_length=4,
        max_length=4,
        style=discord.TextStyle.short
    )
    
    def __init__(self, view, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self.view = view
    
    async def on_submit(self, interaction: discord.Interaction):
        """handle pin submission."""
        if self.pin.value == self.view.correct_pin:
            self.view.authenticated = True
            await self.view.update_embed(interaction)
        else:
            await interaction.response.send_message("❌ Incorrect pin. Access denied.", ephemeral=True)

class DJAdminView(discord.ui.View):
    """view for the DJ admin panel."""
    
    def __init__(self, correct_pin: str):
        super().__init__(timeout=None)
        self.correct_pin = correct_pin
        self.authenticated = False
        
        # add the login button
        self.add_item(LoginButton())
        self.add_item(AdminSheetButton(disabled=True))
    
    async def update_embed(self, interaction: discord.Interaction):
        """update the embed based on authentication status."""
        if self.authenticated:
            embed = discord.Embed(
                title="🎛️ DJ Admin Panel",
                description="✅ Access Granted",
                color=discord.Color.green()
            )
            embed.add_field(
                name="Admin Controls",
                value="You now have access to the admin controls.",
                inline=False
            )
            
            # enable the admin sheet button
            for child in self.children:
                if isinstance(child, AdminSheetButton):
                    child.disabled = False
        else:
            embed = discord.Embed(
                title="🔒 DJ Admin Panel",
                description="Authentication required to access admin controls.",
                color=discord.Color.red()
            )
            embed.add_field(
                name="How to Access",
                value="Click the login button below and enter the admin pin.",
                inline=False
            )
            
            # disable the admin sheet button
            for child in self.children:
                if isinstance(child, AdminSheetButton):
                    child.disabled = True
        
        await interaction.response.edit_message(embed=embed, view=self)

class LoginButton(discord.ui.Button):
    """button for initiating the login process."""
    
    def __init__(self):
        super().__init__(
            label="Login",
            style=discord.ButtonStyle.primary,
            emoji="🔑"
        )
    
    async def callback(self, interaction: discord.Interaction):
        """handle button click."""
        if self.view.authenticated:
            await interaction.response.send_message("✅ you are already logged in.", ephemeral=True)
            return
        
        await interaction.response.send_modal(PinModal(self.view))

class AdminSheetButton(discord.ui.Button):
    """button for accessing the admin sheet."""
    
    def __init__(self, disabled: bool = False):
        super().__init__(
            label="DJ Roster",
            style=discord.ButtonStyle.url,
            url="https://livepartymusic.fm/home/admin",
            emoji="📊",
            disabled=disabled
        )

class DJAdminCog(commands.Cog):
    """handles the /djadmin command for DJ management."""
    
    def __init__(self, bot):
        """initialize the DJ admin cog."""
        self.bot = bot
        self.correct_pin = DJ_ADMIN_PIN
    
    @app_commands.command(name="djadmin", description="access the DJ admin panel")
    async def djadmin(self, interaction: discord.Interaction):
        """display the DJ admin panel with authentication."""
        # check if user is in BOT_ADMINS or has manage_guild permission
        if interaction.user.id not in BOT_ADMINS and not interaction.user.guild_permissions.manage_guild:
            await interaction.response.send_message("❌ You don't have permission to use this command.", ephemeral=True)
            return
        
        # create and send the initial embed
        embed = discord.Embed(
            title="🔒 DJ Admin Panel",
            description="Authentication required to access admin controls.",
            color=discord.Color.blue()
        )
        embed.add_field(
            name="How to Access",
            value="Click the login button below and enter the admin pin.",
            inline=False
        )
        
        # set the bot's avatar as the embed author if available
        if self.bot.user.avatar:
            embed.set_author(
                name=self.bot.user.display_name,
                icon_url=self.bot.user.avatar.url
            )
        
        # create and send the view
        view = DJAdminView(self.correct_pin)
        await interaction.response.send_message(embed=embed, view=view, ephemeral=True)

async def setup(bot):
    """add the cog to the bot."""
    await bot.add_cog(DJAdminCog(bot))
