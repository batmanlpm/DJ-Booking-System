"""
events module for handling event scheduling and timezone management.
"""

# Import the global event manager instance
from .event_instance import event_manager

async def setup(bot):
    """set up the events module."""
    # Import here to avoid circular imports
    from .timezone_commands import setup as setup_timezone_commands
    from .event_commands import setup as setup_event_commands
    
    # Initialize event manager
    await event_manager.initialize(bot)
    
    # Set up commands
    await setup_timezone_commands(bot)
    await setup_event_commands(bot)
    
    return True

__all__ = ['event_manager', 'setup']
