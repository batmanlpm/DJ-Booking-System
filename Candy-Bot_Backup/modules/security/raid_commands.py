"""
commands for managing the anti-raid system.
"""
import discord
from discord import app_commands
from discord.ext import commands
from typing import Optional

class RaidCommands(commands.Cog):
    """commands for managing the anti-raid system."""
    
    def __init__(self, bot):
        self.bot = bot
    
    @app_commands.command(name="lock-channel", description="lock the current channel to prevent messages")
    @app_commands.checks.has_permissions(manage_channels=True)
    async def lock_channel(self, interaction: discord.Interaction):
        """lock the current channel to prevent messages."""
        # defer the response to show a "Thinking..." message
        await interaction.response.defer(thinking=True, ephemeral=True)
        
        if not interaction.guild:
            embed = discord.Embed(
                title="❌ Error",
                description="This command can only be used in a server.",
                color=discord.Color.red()
            )
            await interaction.followup.send(embed=embed, ephemeral=True)
            return
            
        anti_raid = self.bot.get_cog('AntiRaidCog')
        if not anti_raid:
            embed = discord.Embed(
                title="❌ Error",
                description="Anti-raid system is not loaded.",
                color=discord.Color.red()
            )
            await interaction.followup.send(embed=embed, ephemeral=True)
            return
            
        success = await anti_raid.lock_channel(interaction.channel, f"Manually locked by {interaction.user}")
        if success:
            embed = discord.Embed(
                title="🔒 Channel Locked",
                description="This channel has been locked for all roles.",
                color=discord.Color.green()
            )
            embed.set_footer(text=f"Locked by {interaction.user}", icon_url=interaction.user.display_avatar.url)
            await interaction.followup.send(embed=embed, ephemeral=True)
        else:
            embed = discord.Embed(
                title="❌ Error",
                description="Failed to lock the channel. It might already be locked.",
                color=discord.Color.red()
            )
            await interaction.followup.send(embed=embed, ephemeral=True)
    
    @app_commands.command(name="unlock-channel", description="Unlock the current channel")
    @app_commands.checks.has_permissions(manage_channels=True)
    async def unlock_channel(self, interaction: discord.Interaction):
        """unlock the current channel."""
        # defer the response to show a "Thinking..." message
        await interaction.response.defer(thinking=True, ephemeral=True)
        
        if not interaction.guild:
            embed = discord.Embed(
                title="❌ Error",
                description="This command can only be used in a server.",
                color=discord.Color.red()
            )
            await interaction.followup.send(embed=embed, ephemeral=True)
            return
            
        anti_raid = self.bot.get_cog('AntiRaidCog')
        if not anti_raid:
            embed = discord.Embed(
                title="❌ Error",
                description="Anti-raid system is not loaded.",
                color=discord.Color.red()
            )
            await interaction.followup.send(embed=embed, ephemeral=True)
            return
            
        success = await anti_raid.unlock_channel(interaction.channel, f"Manually unlocked by {interaction.user}")
        if success:
            embed = discord.Embed(
                title="🔓 Channel Unlocked",
                description="This channel has been unlocked.",
                color=discord.Color.green()
            )
            embed.set_footer(text=f"Unlocked by {interaction.user}", icon_url=interaction.user.display_avatar.url)
            await interaction.followup.send(embed=embed, ephemeral=True)
        else:
            embed = discord.Embed(
                title="⚠️ Notice",
                description="This channel is not currently locked or an error occurred.",
                color=discord.Color.orange()
            )
            await interaction.followup.send(embed=embed, ephemeral=True)

async def setup(bot):
    """set up the raid commands."""
    await bot.add_cog(RaidCommands(bot))
