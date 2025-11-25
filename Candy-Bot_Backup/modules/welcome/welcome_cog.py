"""
Welcome cog for sending DMs to new members with 18+ notice and timezone setup.
"""
import discord
from discord.ext import commands
from discord import ui
from typing import Optional
import logging

logger = logging.getLogger('discord.welcome')

class TimezoneButton(ui.Button):
    """Button to guide users on setting their timezone."""
    
    def __init__(self):
        super().__init__(
            label="Set Timezone",
            style=discord.ButtonStyle.gray
        )
    
    async def callback(self, interaction: discord.Interaction):
        """Handle button click by sending instructions."""
        await interaction.response.send_message(
            "To set your timezone for event notifications, please use the `/timezone` command in the server.",
            ephemeral=True
        )

class WelcomeView(ui.View):
    """View containing the timezone setup button."""
    
    def __init__(self):
        super().__init__(timeout=None)
        self.add_item(TimezoneButton())

class WelcomeCog(commands.Cog):
    """Cog for handling welcome messages and member join events."""
    
    def __init__(self, bot):
        self.bot = bot
        self.view = WelcomeView()
    
    @commands.Cog.listener()
    async def on_member_join(self, member: discord.Member):
        """Send a welcome DM to new members."""
        try:
            # Create the embed
            embed = discord.Embed(
                title="Welcome to Our Community",
                description=(
                    "### :underage: **18+ Community**\n"
                    "By joining this server, you confirm you are 18 years of age or older. "
                    "All content here is strictly for adults.\n\n"
                    "### Set Your Timezone\n"
                    "To get the most out of our community, please set your timezone using the button below. "
                    "This ensures you receive event notifications at the correct time."
                ),
                color=0x2f3136  # Dark gray theme
            )
            
            # Send the DM
            try:
                await member.send(embed=embed, view=self.view)
                logger.info(f"Sent welcome DM to {member} ({member.id})")
            except discord.Forbidden:
                logger.warning(f"Could not send welcome DM to {member} (DMs disabled)")
            except Exception as e:
                logger.error(f"Error sending welcome DM to {member}: {e}")
                
        except Exception as e:
            logger.error(f"Error in on_member_join for {member}: {e}")

async def setup(bot):
    """Set up the welcome cog."""
    cog = WelcomeCog(bot)
    await bot.add_cog(cog)
    logger.info("Welcome cog loaded")
    return True
