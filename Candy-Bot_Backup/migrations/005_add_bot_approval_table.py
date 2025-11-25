"""Migration to add bot approval and whitelist tables."""

def upgrade(db):
    """Run the migration."""
    # Create bot_approval_requests table
    db.execute_query("""
    CREATE TABLE IF NOT EXISTS bot_approval_requests (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        bot_id INTEGER NOT NULL,
        bot_name TEXT NOT NULL,
        inviter_id INTEGER NOT NULL,
        guild_id INTEGER NOT NULL,
        website_url TEXT,
        status TEXT NOT NULL DEFAULT 'pending',
        approved_by INTEGER,
        approved_at TIMESTAMP,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        UNIQUE(bot_id, guild_id)
    )
    """)
    
    # Create bot_whitelist table
    db.execute_query("""
    CREATE TABLE IF NOT EXISTS bot_whitelist (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        bot_id INTEGER NOT NULL,
        guild_id INTEGER NOT NULL,
        approved_by INTEGER NOT NULL,
        approved_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        notes TEXT,
        UNIQUE(bot_id, guild_id)
    )
    """)
    
    # Create indexes for faster lookups
    db.execute_query("""
    CREATE INDEX IF NOT EXISTS idx_bot_approval_guild_status 
    ON bot_approval_requests(guild_id, status)
    """)
    
    db.execute_query("""
    CREATE INDEX IF NOT EXISTS idx_bot_approval_bot 
    ON bot_approval_requests(bot_id)
    """)
    
    db.execute_query("""
    CREATE INDEX IF NOT EXISTS idx_bot_whitelist_guild 
    ON bot_whitelist(guild_id)
    """)
    
    db.execute_query("""
    CREATE INDEX IF NOT EXISTS idx_bot_whitelist_bot 
    ON bot_whitelist(bot_id)
    """)

def downgrade(db):
    """Revert the migration."""
    db.execute_query("DROP TABLE IF EXISTS bot_approval_requests")
    db.execute_query("DROP TABLE IF EXISTS bot_whitelist")
    db.execute_query("DROP INDEX IF EXISTS idx_bot_approval_guild_status")
    db.execute_query("DROP INDEX IF EXISTS idx_bot_approval_bot")
    db.execute_query("DROP INDEX IF EXISTS idx_bot_whitelist_guild")
    db.execute_query("DROP INDEX IF EXISTS idx_bot_whitelist_bot")
