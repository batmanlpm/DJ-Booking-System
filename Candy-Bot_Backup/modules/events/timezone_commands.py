"""
timezone commands for the bot.
"""
import discord
from discord import app_commands
from discord.ext import commands
import pytz
from typing import Optional
from .event_instance import event_manager

async def setup(bot):
    """set up the timezone commands."""
    @bot.tree.command(name="timezone", description="set your timezone for event notifications.")
    @app_commands.describe(timezone="your timezone (e.g., 'America/New_York', 'UTC', 'CET')")
    async def timezone_set(interaction: discord.Interaction, timezone: str):
        """set your timezone for event notifications."""
        # check if the timezone is valid
        if timezone.upper() in pytz.all_timezones_set or timezone.upper() in [
            'EST', 'EDT', 'CST', 'CDT', 'MST', 'MDT', 'PST', 'PDT',
            'AEST', 'AEDT', 'AWST', 'GMT', 'BST', 'CET', 'CEST', 'IST',
            'JST', 'HKT', 'SGT'
        ]:
            success = await event_manager.set_user_timezone(interaction.user.id, timezone)
            if success:
                await interaction.response.send_message(
                    f"✅ Your timezone has been set to: {timezone}",
                    ephemeral=True
                )
            else:
                await interaction.response.send_message(
                    "❌ Failed to set timezone. Please try again.",
                    ephemeral=True
                )
        else:
            # provide a list of common timezones as a suggestion
            common_timezones = [
                "UTC", "EST", "PST", "CET", "GMT", "AEST", "JST"
            ]
            
            await interaction.response.send_message(
                f"❌ Invalid timezone. Please use a valid timezone like: {', '.join(common_timezones)}\n"
                "For a full list of timezones, use: `/timezone list`",
                ephemeral=True
            )
    
    @timezone_set.autocomplete('timezone')
    async def timezone_autocomplete(
        interaction: discord.Interaction,
        current: str,
    ) -> list[app_commands.Choice[str]]:
        """provide autocomplete suggestions for timezones."""
        try:
            common_timezones = [
                "UTC", "EST/EDT", "CST/CDT", "MST/MDT", "PST/PDT",
                "GMT/BST", "CET/CEST", "AEST/AEDT", "AWST", "JST", "IST", "HKT", "SGT"
            ]
            
            # filter based on user input
            filtered = [
                tz for tz in common_timezones
                if current.lower() in tz.lower()
            ]
            
            # if no matches in common timezones, search all timezones
            if not filtered and len(current) > 2:
                filtered_tz = [
                    tz for tz in pytz.all_timezones
                    if current.lower() in tz.lower()
                ][:10]  # limit to 10 suggestions
                filtered.extend(filtered_tz)
            
            return [
                app_commands.Choice(name=tz, value=tz.split('/')[-1])
                for tz in filtered[:25]  # Discord limit
            ]
        except Exception as e:
            print(f"Error in timezone autocomplete: {e}")
            return []
    
    @bot.tree.command(name="timezone_list", description="List all available timezones.")
    async def timezone_list(interaction: discord.Interaction):
        """list all available timezones."""
        common_timezones = [
            "UTC", "EST/EDT (US Eastern)", "CST/CDT (US Central)", 
            "MST/MDT (US Mountain)", "PST/PDT (US Pacific)",
            "GMT/BST (UK)", "CET/CEST (Central Europe)", 
            "AEST/AEDT (Australia East)", "AWST (Australia West)", 
            "JST (Japan)", "IST (India)", "HKT (Hong Kong)", "SGT (Singapore)"
        ]
        
        embed = discord.Embed(
            title="🌍 Common Timezones",
            description="Here are some common timezones you can use:",
            color=discord.Color.blue()
        )
        
        # add common timezones
        timezone_list = "\n".join(f"• {tz}" for tz in common_timezones)
        embed.add_field(
            name="Common Timezones",
            value=timezone_list,
            inline=False
        )
        
        # add note about full list
        embed.add_field(
            name="More Timezones",
            value=(
                "For a full list of timezones, visit: "
                "[Time Zone Database](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones)\n"
                "Use the format: `Continent/City` (e.g., `America/New_York`)"
            ),
            inline=False
        )
        
        # add current time in UTC for reference
        utc_now = discord.utils.utcnow()
        embed.set_footer(text=f"Current UTC time: {utc_now.strftime('%Y-%m-%d %H:%M:%S')} UTC")
        
        await interaction.response.send_message(embed=embed, ephemeral=True)
    
    @bot.tree.command(name="mytimezone", description="Check your current timezone setting.")
    async def my_timezone(interaction: discord.Interaction):
        """check your current timezone setting."""
        timezone = await event_manager.get_user_timezone(interaction.user.id)
        if timezone:
            await interaction.response.send_message(
                f"⏰ Your current timezone is set to: **{timezone}**",
                ephemeral=True
            )
        else:
            await interaction.response.send_message(
                "⏰ You haven't set your timezone yet. Use `/timezone` to set it.",
                ephemeral=True
            )
    
    return True
