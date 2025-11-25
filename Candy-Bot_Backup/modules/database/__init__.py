"""
database module for handling database operations.

exposes:
    - db: instance of database manager
    - DatabaseManager: database manager class
"""

from .database import DatabaseManager, db

async def setup(bot):
    """set up the database module."""
    try:
        # Create tables if they don't exist
        db.create_tables()
        return True
    except Exception as e:
        print(f"Error setting up database: {e}")
        return False

__all__ = ['DatabaseManager', 'db', 'setup']
