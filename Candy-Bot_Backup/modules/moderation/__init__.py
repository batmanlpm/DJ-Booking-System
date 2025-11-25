from discord.ext import commands
from .modlog import ModLog
from .warnings import Warnings
from .actions import ModActions
from .tempbans import TempBan
from .mutes import Mute
from .lockdown import Lockdown

async def setup(bot: commands.Bot) -> None:
    """load the moderation module"""
    await bot.add_cog(ModLog(bot))
    await bot.add_cog(Warnings(bot))
    await bot.add_cog(ModActions(bot))
    await bot.add_cog(TempBan(bot))
    await bot.add_cog(Mute(bot))
    await bot.add_cog(Lockdown(bot))
