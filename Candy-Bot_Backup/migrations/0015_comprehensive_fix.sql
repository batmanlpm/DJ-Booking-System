-- First, disable foreign key checks
PRAGMA foreign_keys = OFF;

-- Create backup tables
CREATE TABLE IF NOT EXISTS backup_bot_admins AS SELECT * FROM bot_admins;
CREATE TABLE IF NOT EXISTS backup_admin_security_settings AS SELECT * FROM admin_security_settings;

-- Drop existing tables in the correct order to avoid foreign key violations
DROP TABLE IF EXISTS admin_action_logs;
DROP TABLE IF EXISTS admin_security_settings;
DROP TABLE IF EXISTS bot_admins;
DROP TABLE IF EXISTS guilds;

-- Recreate guilds table
CREATE TABLE guilds (
    guild_id INTEGER PRIMARY KEY,
    owner_id INTEGER NOT NULL,
    admin_role_id INTEGER,
    event_channel_id INTEGER,
    UNIQUE(guild_id, event_channel_id)
);

-- Recreate bot_admins table
CREATE TABLE bot_admins (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    role_id INTEGER NOT NULL,
    added_by INTEGER,
    added_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    guild_id INTEGER,
    UNIQUE(role_id, guild_id) ON CONFLICT REPLACE,
    FOREIGN KEY (guild_id) REFERENCES guilds(guild_id) ON DELETE CASCADE,
    FOREIGN KEY (added_by) REFERENCES users(user_id) ON DELETE SET NULL
);

-- Recreate admin_security_settings table
CREATE TABLE admin_security_settings (
    guild_id INTEGER PRIMARY KEY,
    security_enabled BOOLEAN DEFAULT 1,
    quarantine_enabled BOOLEAN DEFAULT 1,
    two_factor_required BOOLEAN DEFAULT 0,
    log_channel_id INTEGER,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (guild_id) REFERENCES guilds(guild_id) ON DELETE CASCADE
);

-- Recreate admin_action_logs table
CREATE TABLE IF NOT EXISTS admin_action_logs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    guild_id INTEGER NOT NULL,
    admin_id INTEGER NOT NULL,
    action_type TEXT NOT NULL,
    target_id INTEGER,
    target_type TEXT,
    reason TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (guild_id) REFERENCES guilds(guild_id) ON DELETE CASCADE
);

-- Insert default guild
INSERT OR IGNORE INTO guilds (guild_id, owner_id, admin_role_id, event_channel_id)
VALUES (1363481372672393370, 0, NULL, NULL);

-- Try to restore data from backups if they exist
INSERT OR IGNORE INTO bot_admins (id, role_id, added_by, added_at, guild_id)
SELECT id, role_id, added_by, added_at, guild_id 
FROM backup_bot_admins
WHERE guild_id IN (SELECT guild_id FROM guilds);

-- Recreate indexes
CREATE INDEX IF NOT EXISTS idx_bot_admins_guild ON bot_admins(guild_id);
CREATE INDEX IF NOT EXISTS idx_admin_logs_guild ON admin_action_logs(guild_id);
CREATE INDEX IF NOT EXISTS idx_admin_logs_admin ON admin_action_logs(admin_id);

-- Clean up backup tables
DROP TABLE IF EXISTS backup_bot_admins;
DROP TABLE IF EXISTS backup_admin_security_settings;

-- Re-enable foreign key checks
PRAGMA foreign_keys = ON;

-- Run integrity check
PRAGMA integrity_check;
