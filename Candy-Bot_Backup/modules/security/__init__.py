"""
security module for handling security-related functionality.
"""
import discord
from discord.ext import commands

# import all security components
from .security_events import setup as setup_security_events
from .admin_security import setup as setup_admin_security
from .anti_raid_cog import setup as setup_anti_raid
from .raid_commands import setup as setup_raid_commands
from .bot_security import setup as setup_bot_security
from .admin_action_tracker import AdminActionCog

class Security(commands.Cog):
    """main security cog that loads all security components."""
    
    def __init__(self, bot):
        self.bot = bot
        self.admin_tracker = None

    async def setup_hook(self):
        """set up all security components when the cog is loaded."""
        # set up all security components
        await setup_security_events(self.bot)
        await setup_admin_security(self.bot)
        await setup_anti_raid(self.bot)
        await setup_raid_commands(self.bot)
        await setup_bot_security(self.bot)
        
        # set up admin action tracker
        self.admin_tracker = AdminActionCog(self.bot)
        await self.bot.add_cog(self.admin_tracker)
        print("[Security] All security components loaded successfully")

async def setup(bot):
    """set up the security cog and all its components."""
    security_cog = Security(bot)
    await bot.add_cog(security_cog)
    
    # initialize the security components
    await security_cog.setup_hook()
    
    print("[Security] Security module loaded successfully")
    return True

__all__ = ['setup']
