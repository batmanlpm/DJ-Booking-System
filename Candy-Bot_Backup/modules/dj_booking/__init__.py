"""
dj_booking module for handling DJ bookings.

this module provides functionality for DJs to book slots through an external link.
"""

import logging
from .booking_cog import DJBookCog

# set up logging
logger = logging.getLogger(__name__)

async def setup(bot):
    """load the booking cog into the bot.
    
    args:
        bot: the discord bot instance to add the cog to.
    """
    try:
        # add the booking cog to the bot
        await bot.add_cog(DJBookCog(bot))
        logger.info("DJ Booking module loaded successfully")
    except Exception as e:
        logger.error(f"failed to load DJ Booking module: {e}")
        raise
