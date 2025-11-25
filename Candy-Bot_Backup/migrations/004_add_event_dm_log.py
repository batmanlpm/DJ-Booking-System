"""
Migration to add the event_dm_log table for tracking DM notifications.
"""
import sqlite3
from pathlib import Path
import logging

# Set up logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def apply_migration(db_path: str = 'bot_database.db') -> None:
    """
    Apply the migration to add the event_dm_log table.
    
    Args:
        db_path: Path to the SQLite database file
    """
    try:
        # Connect to the database
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        
        # Enable foreign keys
        cursor.execute("PRAGMA foreign_keys = ON")
        
        # Create the event_dm_log table
        cursor.execute("""
        CREATE TABLE IF NOT EXISTS event_dm_log (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            event_id INTEGER NOT NULL,
            user_id INTEGER NOT NULL,
            sent_at TEXT NOT NULL,
            status TEXT NOT NULL,
            error_message TEXT,
            FOREIGN KEY (event_id) REFERENCES events (event_id) ON DELETE CASCADE,
            UNIQUE(event_id, user_id)
        )
        """)
        
        # Create an index for faster lookups
        cursor.execute("""
        CREATE INDEX IF NOT EXISTS idx_event_dm_log_event_user 
        ON event_dm_log(event_id, user_id)
        """)
        
        # Commit changes
        conn.commit()
        logger.info("Successfully created event_dm_log table")
        
    except sqlite3.Error as e:
        logger.error(f"Error applying migration: {e}")
        if conn:
            conn.rollback()
        raise
    finally:
        if conn:
            conn.close()

if __name__ == "__main__":
    # Get the absolute path to the database
    db_path = str(Path(__file__).parent.parent / 'bot_database.db')
    apply_migration(db_path)
