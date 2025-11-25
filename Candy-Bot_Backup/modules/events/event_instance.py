"""
module to hold the global event manager instance to avoid circular imports.
"""
from .event_manager import EventManager

# global event manager instance
event_manager = EventManager()

__all__ = ['event_manager']
