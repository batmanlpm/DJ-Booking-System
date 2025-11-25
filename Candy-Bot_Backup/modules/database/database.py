"""
database module for handling all database operations.

this module provides a database manager class to interact with the sqlite database.
"""

import sqlite3
from typing import Any, List, Optional, Tuple, Union
import logging
from pathlib import Path

# set up logging
logger = logging.getLogger(__name__)

class DatabaseManager:
    """manages database connections and operations."""

    def __init__(self, db_path: str = None):
        """initialize the database manager."""
        if db_path is None:
            # default to data/bot_database.db in the project root
            import os
            script_dir = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
            self.db_path = os.path.join(script_dir, 'data', 'bot_database.db')
        else:
            self.db_path = db_path
            
        self.connection: Optional[sqlite3.Connection] = None
        self._ensure_db_directory()
        
        # ensure the database directory exists
        os.makedirs(os.path.dirname(os.path.abspath(self.db_path)), exist_ok=True)

    def _ensure_db_directory(self) -> None:
        """ensure the database directory exists."""
        db_path = Path(self.db_path)
        if db_path.parent != Path('.'):
            db_path.parent.mkdir(parents=True, exist_ok=True)

    def connect(self) -> None:
        """establish a connection to the database."""
        if self.connection is None:
            self.connection = sqlite3.connect(
                self.db_path,
                detect_types=sqlite3.PARSE_DECLTYPES | sqlite3.PARSE_COLNAMES
            )
            self.connection.row_factory = sqlite3.Row
            logger.info('database connection established')

    def close(self) -> None:
        """close the database connection if it's open."""
        if self.connection is not None:
            self.connection.close()
            self.connection = None
            logger.info('database connection closed')

    def execute_query(
        self,
        query: str,
        params: Union[tuple, dict] = (),
        fetch: bool = False,
        commit: bool = False
    ) -> Union[sqlite3.Cursor, List[sqlite3.Row], int]:
        """execute a sql query."""
        self.connect()
        cursor = self.connection.cursor()
        
        try:
            cursor.execute(query, params)
            
            if commit:
                self.connection.commit()
                return cursor.lastrowid
                
            if fetch:
                return cursor.fetchall()
                
            return cursor
            
        except sqlite3.Error as e:
            logger.error(f'database error: {e}')
            if self.connection:
                self.connection.rollback()
            raise

    def _add_column_if_not_exists(self, table: str, column: str, column_def: str):
        """add a column to a table if it doesn't already exist."""
        try:
            # check if column exists
            cursor = self.connection.execute(f"PRAGMA table_info({table})")
            columns = [col[1] for col in cursor.fetchall()]  # column names are at index 1
            
            if column.lower() not in [col.lower() for col in columns]:
                # special handling for event_channel_id to ensure proper foreign key constraint
                if column.lower() == 'event_channel_id' and table.lower() == 'events':
                    # First add the column without the foreign key
                    self.connection.execute(f"""
                        ALTER TABLE {table} 
                        ADD COLUMN {column} {column_def.split('REFERENCES')[0].strip()}
                    """)
                    # then update existing events with guild's event channel
                    self.connection.execute("""
                        UPDATE events 
                        SET event_channel_id = (
                            SELECT event_channel_id 
                            FROM guilds 
                            WHERE guilds.guild_id = events.guild_id
                        )
                        WHERE event_channel_id IS NULL
                    """)
                    # finally, add the foreign key constraint
                    self.connection.execute("""
                        CREATE INDEX IF NOT EXISTS idx_events_guild_channel 
                        ON events(guild_id, event_channel_id)
                    """)
                    print(f"Added column '{column}' to table '{table}' with proper constraints")
                else:
                    # normal column addition for other cases
                    self.connection.execute(f"ALTER TABLE {table} ADD COLUMN {column} {column_def}")
                    print(f"Added column '{column}' to table '{table}'")
        except Exception as e:
            print(f"Error adding column '{column}' to table '{table}': {e}")
            if self.connection:
                self.connection.rollback()
    
    def create_tables(self) -> None:
        """create all required database tables if they don't exist."""
        tables = {
            'guilds': '''
            CREATE TABLE IF NOT EXISTS guilds (
                guild_id INTEGER PRIMARY KEY,
                owner_id INTEGER NOT NULL,
                admin_role_id INTEGER,
                event_channel_id INTEGER,
                UNIQUE(guild_id, event_channel_id)
            )
            ''',
            
            # this is now handled in the create_tables function to avoid duplicate column errors
            
            'users': '''
            CREATE TABLE IF NOT EXISTS users (
                user_id INTEGER PRIMARY KEY,
                timezone TEXT
            )
            ''',
            
            'events': '''
            CREATE TABLE IF NOT EXISTS events (
                event_id INTEGER PRIMARY KEY AUTOINCREMENT,
                guild_id INTEGER NOT NULL,
                name TEXT NOT NULL,
                time TEXT NOT NULL,
                timezone TEXT NOT NULL,
                description TEXT,
                event_channel_id INTEGER,
                FOREIGN KEY (guild_id) REFERENCES guilds (guild_id) ON DELETE CASCADE,
                FOREIGN KEY (guild_id, event_channel_id) REFERENCES guilds (guild_id, event_channel_id) ON DELETE SET NULL
            )
            ''',
            
            'bot_admins': '''
            CREATE TABLE IF NOT EXISTS bot_admins (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER,
                role_id INTEGER,
                added_by INTEGER NOT NULL,
                added_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                permissions TEXT DEFAULT 'all',
                FOREIGN KEY (added_by) REFERENCES users (user_id) ON DELETE SET NULL,
                CHECK (user_id IS NOT NULL OR role_id IS NOT NULL)
            )
            ''',
            
            'admins': '''
            CREATE TABLE IF NOT EXISTS admins (
                guild_id INTEGER NOT NULL,
                user_id INTEGER NOT NULL,
                is_trusted BOOLEAN DEFAULT 0,
                PRIMARY KEY (guild_id, user_id),
                FOREIGN KEY (guild_id) REFERENCES guilds (guild_id) ON DELETE CASCADE,
                FOREIGN KEY (user_id) REFERENCES users (user_id) ON DELETE CASCADE
            )
            ''',
            
            'security_settings': '''
            CREATE TABLE IF NOT EXISTS security_settings (
                guild_id INTEGER PRIMARY KEY,
                anti_raid_enabled BOOLEAN DEFAULT 0,
                anti_raid_threshold INTEGER DEFAULT 5,
                anti_raid_timeframe INTEGER DEFAULT 10,
                anti_nuke_enabled BOOLEAN DEFAULT 0,
                anti_nuke_threshold INTEGER DEFAULT 10,
                anti_nuke_timeframe INTEGER DEFAULT 5,
                anti_spam_enabled BOOLEAN DEFAULT 0,
                blocked_keywords TEXT,
                block_links BOOLEAN DEFAULT 0,
                FOREIGN KEY (guild_id) REFERENCES guilds (guild_id) ON DELETE CASCADE
            )
            ''',
            
            'event_reminders': '''
            CREATE TABLE IF NOT EXISTS event_reminders (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                event_id INTEGER NOT NULL,
                reminder_type TEXT NOT NULL,
                sent_at TEXT NOT NULL,
                FOREIGN KEY (event_id) REFERENCES events (event_id) ON DELETE CASCADE,
                UNIQUE(event_id, reminder_type)
            )
            '''
        }
        
        try:
            # enable foreign key constraints
            self.execute_query('PRAGMA foreign_keys = ON')
            
            # create tables
            for table_name, table_sql in tables.items():
                try:
                    self.execute_query(table_sql, commit=True)
                    logger.info(f'table {table_name} created successfully')
                    
                    # add any missing columns for existing tables
                    if table_name == 'events':
                        self._add_column_if_not_exists('events', 'event_channel_id', 'INTEGER REFERENCES guilds(event_channel_id) ON DELETE SET NULL')
                    
                except sqlite3.Error as e:
                    logger.error(f'error creating table {table_name}: {e}')
                    # if it's not a duplicate table error, re-raise
                    if 'duplicate' not in str(e).lower():
                        raise
        
            # add any missing columns to existing tables
            try:
                self._add_column_if_not_exists('guilds', 'event_channel_id', 'INTEGER')
            except Exception as e:
                logger.error(f'error adding columns: {e}')
                raise
            
            # create indexes for better query performance
            self._create_indexes()
            
        except sqlite3.Error as e:
            logger.error(f'error creating tables: {e}')
            raise

    def _create_indexes(self) -> None:
        """create database indexes for better query performance."""
        indexes = [
            'CREATE INDEX IF NOT EXISTS idx_events_guild_id ON events(guild_id)',
            'CREATE INDEX IF NOT EXISTS idx_admins_guild_id ON admins(guild_id)',
            'CREATE INDEX IF NOT EXISTS idx_admins_user_id ON admins(user_id)'
        ]
        
        for index_query in indexes:
            self.execute_query(index_query)

    def __enter__(self):
        """context manager entry."""
        self.connect()
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        """context manager exit."""
        self.close()

# create a global instance for easy import
db = DatabaseManager()
