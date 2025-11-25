"""
Migration to add the anti_raid_settings table.
"""

def up():
    """Apply the migration."""
    return """
    -- Create anti_raid_settings table
    CREATE TABLE IF NOT EXISTS anti_raid_settings (
        guild_id INTEGER PRIMARY KEY,
        enabled BOOLEAN DEFAULT 1,
        message_threshold INTEGER DEFAULT 5,
        time_window INTEGER DEFAULT 10,
        lock_duration INTEGER DEFAULT 300,
        exempt_roles TEXT DEFAULT '[]',
        last_raid_trigger TIMESTAMP,
        is_locked BOOLEAN DEFAULT 0
    );
    
    -- Insert default settings for known guilds
    INSERT OR IGNORE INTO anti_raid_settings (guild_id) 
    SELECT id FROM guilds;
    """

def down():
    """Revert the migration."""
    return """
    DROP TABLE IF EXISTS anti_raid_settings;
    """

if __name__ == "__main__":
    # For testing the migration directly
    import sqlite3
    import os
    
    # Get the absolute path to the database file
    script_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    db_path = os.path.join(script_dir, 'data', 'bot_database.db')
    
    # Ensure the data directory exists
    os.makedirs(os.path.dirname(db_path), exist_ok=True)
    
    # Connect to the database
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    
    try:
        # Apply the migration
        print("Applying migration...")
        cursor.executescript(up())
        
        # Add default settings for all guilds
        cursor.execute("SELECT id FROM guilds")
        guilds = cursor.fetchall()
        
        for guild in guilds:
            cursor.execute(
                """
                INSERT OR IGNORE INTO anti_raid_settings 
                (guild_id, enabled, message_threshold, time_window, lock_duration, exempt_roles)
                VALUES (?, 1, 5, 10, 300, '[]')
                """,
                (guild[0],)
            )
        
        conn.commit()
        print("Migration applied successfully!")
        
        # Verify the table was created
        cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='anti_raid_settings'")
        if not cursor.fetchone():
            print("Error: Failed to create anti_raid_settings table")
        else:
            print("anti_raid_settings table verified")
            
    except Exception as e:
        print(f"Error applying migration: {str(e)}")
        conn.rollback()
    finally:
        conn.close()
