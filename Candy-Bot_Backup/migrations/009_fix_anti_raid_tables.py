"""
Migration to ensure both guilds and anti_raid_settings tables exist.
"""

def up():
    """Apply the migration."""
    return """
    -- Create guilds table if it doesn't exist
    CREATE TABLE IF NOT EXISTS guilds (
        id INTEGER PRIMARY KEY,
        name TEXT NOT NULL,
        prefix TEXT DEFAULT '!',
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );
    
    -- Create anti_raid_settings table if it doesn't exist
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
    
    -- Insert default settings for guilds that don't have them
    INSERT OR IGNORE INTO anti_raid_settings (guild_id)
    SELECT id FROM guilds
    WHERE id NOT IN (SELECT guild_id FROM anti_raid_settings);
    """

def down():
    """Revert the migration."""
    return """
    -- We won't drop the tables in the down migration to prevent data loss
    -- If you need to drop them, do it manually
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
        conn.commit()
        
        # Verify the tables were created
        cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='guilds'")
        if not cursor.fetchone():
            print("Error: Failed to create guilds table")
        else:
            print("guilds table verified")
            
        cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='anti_raid_settings'")
        if not cursor.fetchone():
            print("Error: Failed to create anti_raid_settings table")
        else:
            print("anti_raid_settings table verified")
            
        # Show current anti_raid_settings
        cursor.execute("SELECT * FROM anti_raid_settings")
        settings = cursor.fetchall()
        if settings:
            print("\nCurrent anti-raid settings:")
            for setting in settings:
                print(f"Guild ID: {setting[0]}, Enabled: {setting[1]}")
        else:
            print("\nNo anti-raid settings found. Make sure guilds exist in the database.")
            
    except Exception as e:
        print(f"Error applying migration: {str(e)}")
        conn.rollback()
    finally:
        conn.close()
