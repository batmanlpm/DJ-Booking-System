"""
Utilities for the Discord bot.

This package contains various utility modules used throughout the bot.
"""

# make the logger available at the package level
from .logger import get_logger, setup_logger

__all__ = ['get_logger', 'setup_logger']
