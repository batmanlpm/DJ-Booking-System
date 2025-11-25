"""
radioboss module for handling radioboss api interactions.
"""

from .radioboss_cog import RadioBossCog
from .radioboss_api import RadioBossAPIClient

__all__ = ['RadioBossCog', 'RadioBossAPIClient']

async def setup(bot):
    """setup function for loading the radioboss cog."""
    await bot.add_cog(RadioBossCog(bot))
    return True
