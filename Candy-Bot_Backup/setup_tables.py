"""
script to set up required database tables.
"""
import sqlite3
import os
from pathlib import Path

def setup_database():
    # get the absolute path to the database
    script_dir = Path(__file__).parent
    db_path = script_dir / 'data' / 'bot_database.db'
    
    # ensure the data directory exists
    db_path.parent.mkdir(parents=True, exist_ok=True)
    
    print(f"Setting up database at: {db_path}")
    
    # connect to the database
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    
    try:
        # enable foreign keys
        cursor.execute("PRAGMA foreign_keys = ON")
        
        # create guilds table
        cursor.execute("""
        CREATE TABLE IF NOT EXISTS guilds (
            id INTEGER PRIMARY KEY,
            name TEXT NOT NULL,
            prefix TEXT DEFAULT '!',
            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        )""")
        print("Created 'guilds' table")
        
        # create anti_raid_settings table
        cursor.execute("""
        CREATE TABLE IF NOT EXISTS anti_raid_settings (
            guild_id INTEGER PRIMARY KEY,
            enabled BOOLEAN DEFAULT 1,
            message_threshold INTEGER DEFAULT 5,
            time_window INTEGER DEFAULT 10,
            lock_duration INTEGER DEFAULT 300,
            exempt_roles TEXT DEFAULT '[]',
            last_raid_trigger TIMESTAMP,
            is_locked BOOLEAN DEFAULT 0,
            FOREIGN KEY (guild_id) REFERENCES guilds(id) ON DELETE CASCADE
        )""")
        print("Created 'anti_raid_settings' table")
        
        # create admin_action_logs table if it doesn't exist
        cursor.execute("""
        CREATE TABLE IF NOT EXISTS admin_action_logs (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            guild_id INTEGER NOT NULL,
            user_id INTEGER NOT NULL,
            action TEXT NOT NULL,
            target_id INTEGER,
            timestamp DATETIME NOT NULL,
            FOREIGN KEY (guild_id) REFERENCES guilds(id) ON DELETE CASCADE
        )""")
        print("Verified 'admin_action_logs' table")
        
        # create indexes
        cursor.execute("""
        CREATE INDEX IF NOT EXISTS idx_admin_action_logs_guild_user 
        ON admin_action_logs(guild_id, user_id)
        """)
        
        cursor.execute("""
        CREATE INDEX IF NOT EXISTS idx_admin_action_logs_timestamp 
        ON admin_action_logs(timestamp)
        """)
        
        print("Created indexes")
        
        # commit changes
        conn.commit()
        print("\nDatabase setup completed successfully!")
        
    except Exception as e:
        print(f"Error setting up database: {str(e)}")
        conn.rollback()
    finally:
        # close the connection
        conn.close()

if __name__ == "__main__":
    setup_database()
