"""
DJ Book Command Cog

provides a simple command for DJs to book slots through an external link.
"""

import discord
from discord.ext import commands
from discord import app_commands

class DJBookCog(commands.Cog):
    """handles the /djbook command for DJ bookings."""

    def __init__(self, bot):
        """initialize the DJ book cog."""
        self.bot = bot
        self.booking_url = "https://c40.radioboss.fm/u/98"

    @app_commands.command(name="djbook", description="book a DJ slot through our booking system")
    async def djbook(self, interaction: discord.Interaction):
        """send an embed with a button to book a DJ slot."""
        # create embed
        embed = discord.Embed(
            title="🎧 DJ Booking",
            description="Click the button below to book your DJ slot through our NEW & Improved official booking system.",
            color=discord.Color.blue()
        )
        
        # add some additional information
        embed.add_field(
            name="Booking Instructions",
            value=f"1. Click the 'book now' button below\n2. Fill out the booking form\n3. Make sure you don't forget about your booking!",
            inline=False
        )
        
        # create view with button
        view = discord.ui.View()
        button = discord.ui.Button(
            label="book now",
            url=self.booking_url,
            style=discord.ButtonStyle.url,
            emoji="📅"
        )
        view.add_item(button)
        
        # set the bot's avatar as the embed author if available
        if self.bot.user.avatar:
            embed.set_author(
                name=self.bot.user.display_name,
                icon_url=self.bot.user.avatar.url
            )
        
        await interaction.response.send_message(embed=embed, view=view, ephemeral=False)

async def setup(bot):
    """add the cog to the bot."""
    await bot.add_cog(DJBookCog(bot))
