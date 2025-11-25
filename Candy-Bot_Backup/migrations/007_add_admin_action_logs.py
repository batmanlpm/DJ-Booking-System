"""
Migration to add the admin_action_logs table for tracking admin actions.
"""

def up():
    """Apply the migration."""
    return """
    CREATE TABLE IF NOT EXISTS admin_action_logs (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        guild_id INTEGER NOT NULL,
        user_id INTEGER NOT NULL,
        action TEXT NOT NULL,
        target_id INTEGER,
        timestamp DATETIME NOT NULL,
        FOREIGN KEY (guild_id) REFERENCES guilds(id) ON DELETE CASCADE
    );
    
    CREATE INDEX IF NOT EXISTS idx_admin_action_logs_guild_user 
    ON admin_action_logs(guild_id, user_id);
    
    CREATE INDEX IF NOT EXISTS idx_admin_action_logs_timestamp 
    ON admin_action_logs(timestamp);
    """

def down():
    """Revert the migration."""
    return """
    DROP TABLE IF EXISTS admin_action_logs;
    """

if __name__ == "__main__":
    # For testing the migration directly
    import sqlite3
    import os
    
    # Get the absolute path to the database file
    db_path = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), 'data', 'bot_database.db')
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    
    # Apply the migration
    print("Applying migration...")
    cursor.executescript(up())
    
    # Verify the table was created
    cursor.execute("""
        SELECT name FROM sqlite_master 
        WHERE type='table' AND name='admin_action_logs';
    """)
    
    if cursor.fetchone():
        print("Migration applied successfully!")
    else:
        print("Failed to create admin_action_logs table")
    
    conn.close()
