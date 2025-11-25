"""
Migration to add admin action tracking tables.
"""

def up():
    """Apply the migration."""
    return """
    -- Create admin_action_logs table if it doesn't exist
    CREATE TABLE IF NOT EXISTS admin_action_logs (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        guild_id INTEGER NOT NULL,
        user_id INTEGER NOT NULL,
        action TEXT NOT NULL,
        target_id INTEGER,
        timestamp DATETIME NOT NULL,
        details TEXT,
        FOREIGN KEY (guild_id) REFERENCES guilds(id) ON DELETE CASCADE,
        FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
    );
    
    -- Create admin_quarantine table if it doesn't exist
    CREATE TABLE IF NOT EXISTS admin_quarantine (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        guild_id INTEGER NOT NULL,
        user_id INTEGER NOT NULL,
        reason TEXT NOT NULL,
        quarantined_by INTEGER NOT NULL,
        quarantined_at DATETIME NOT NULL,
        quarantined_until DATETIME NOT NULL,
        is_active BOOLEAN DEFAULT 1,
        roles TEXT NOT NULL,
        FOREIGN KEY (guild_id) REFERENCES guilds(id) ON DELETE CASCADE,
        FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
        FOREIGN KEY (quarantined_by) REFERENCES users(id) ON DELETE CASCADE,
        UNIQUE(guild_id, user_id) ON CONFLICT REPLACE
    );
    
    -- Create index for faster lookups
    CREATE INDEX IF NOT EXISTS idx_admin_action_logs_guild_user ON admin_action_logs(guild_id, user_id);
    CREATE INDEX IF NOT EXISTS idx_admin_action_logs_timestamp ON admin_action_logs(timestamp);
    CREATE INDEX IF NOT EXISTS idx_admin_quarantine_guild_user ON admin_quarantine(guild_id, user_id);
    """

def down():
    """Revert the migration."""
    return """
    DROP INDEX IF EXISTS idx_admin_quarantine_guild_user;
    DROP INDEX IF EXISTS idx_admin_action_logs_timestamp;
    DROP INDEX IF EXISTS idx_admin_action_logs_guild_user;
    DROP TABLE IF EXISTS admin_quarantine;
    DROP TABLE IF EXISTS admin_action_logs;
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
        print("Migration applied successfully!")
        
        # Verify the tables were created
        cursor.execute("""
            SELECT name FROM sqlite_master 
            WHERE type='table' 
            AND name IN ('admin_action_logs', 'admin_quarantine')
        """)
        print("\nCreated tables:", [row[0] for row in cursor.fetchall()])
        
    except sqlite3.Error as e:
        print(f"Error applying migration: {e}")
    finally:
        conn.close()
