"""Migration to add admin action tracking tables."""
from modules.database import db

def apply():
    # Table to track admin actions
    db.execute('''
    CREATE TABLE IF NOT EXISTS admin_actions (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        guild_id INTEGER NOT NULL,
        user_id INTEGER NOT NULL,
        action_type TEXT NOT NULL,  -- 'ban', 'kick', 'channel_delete', etc.
        target_id INTEGER,  -- ID of the target (user or channel)
        timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
        action_count INTEGER DEFAULT 1
    )
    ''')
    
    # Table to track quarantined users
    db.execute('''
    CREATE TABLE IF NOT EXISTS admin_quarantine (
        user_id INTEGER PRIMARY KEY,
        guild_id INTEGER NOT NULL,
        quarantined_at DATETIME DEFAULT CURRENT_TIMESTAMP,
        quarantined_roles TEXT,  -- JSON array of role IDs
        quarantined_until DATETIME,  -- NULL means indefinite until manually removed
        reason TEXT
    )
    ''')
    
    # Index for faster lookups
    db.execute('CREATE INDEX IF NOT EXISTS idx_admin_actions_user ON admin_actions(guild_id, user_id, action_type, timestamp)')
    db.execute('CREATE INDEX IF NOT EXISTS idx_admin_quarantine_user ON admin_quarantine(guild_id, user_id)')
    
    print("Applied 006_add_admin_action_tracking migration")

def rollback():
    db.execute('DROP TABLE IF EXISTS admin_actions')
    db.execute('DROP TABLE IF EXISTS admin_quarantine')
    print("Rolled back 006_add_admin_action_tracking migration")
