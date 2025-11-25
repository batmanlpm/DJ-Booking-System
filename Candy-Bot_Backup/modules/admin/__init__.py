"""
Admin module for handling administrative commands and bot administration.

This module provides commands and UI components for managing bot administrators,
security settings, and anti-raid configurations. It's designed to be used by
server owners to control bot permissions and security features.
"""
import logging
from .admin_commands import AdminCog, setup as admin_setup
from .bot_admin_commands import BotAdminCog, setup as bot_admin_setup
from .djadmin_cog import DJAdminCog

# Set up module-level logger
logger = logging.getLogger(__name__)

__all__ = ['AdminCog', 'BotAdminCog', 'DJAdminCog', 'setup']

async def setup(bot):
    """Set up all admin-related cogs."""
    await admin_setup(bot)
    await bot_admin_setup(bot)
    await bot.add_cog(DJAdminCog(bot))
    logger.info("Loaded DJAdminCog")
