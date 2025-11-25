"""
voice module for handling voice channel interactions.
"""

from .voice_cog import VoiceCog

async def setup(bot):
    """setup function for loading the voice cog."""
    await bot.add_cog(VoiceCog(bot))
    return True
