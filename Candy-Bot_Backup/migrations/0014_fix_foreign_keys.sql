-- First, create a backup of the existing data
CREATE TABLE IF NOT EXISTS bot_admins_backup AS SELECT * FROM bot_admins;

-- Create a new table with the correct schema
CREATE TABLE bot_admins_new (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    role_id INTEGER NOT NULL,
    added_by INTEGER,
    added_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    guild_id INTEGER,
    UNIQUE(role_id, guild_id) ON CONFLICT REPLACE
);

-- Copy data from the old table to the new one
INSERT INTO bot_admins_new (id, role_id, added_by, added_at, guild_id)
SELECT id, role_id, added_by, added_at, guild_id FROM bot_admins;

-- Drop the old table
DROP TABLE bot_admins;

-- Rename the new table to the original name
ALTER TABLE bot_admins_new RENAME TO bot_admins;

-- Recreate the index
CREATE INDEX IF NOT EXISTS idx_bot_admins_guild ON bot_admins(guild_id);

-- Add the guilds table if it doesn't exist
CREATE TABLE IF NOT EXISTS guilds (
    guild_id INTEGER PRIMARY KEY,
    owner_id INTEGER NOT NULL,
    admin_role_id INTEGER,
    event_channel_id INTEGER,
    UNIQUE(guild_id, event_channel_id)
);

-- Insert a default guild if it doesn't exist
INSERT OR IGNORE INTO guilds (guild_id, owner_id, admin_role_id, event_channel_id)
VALUES (1363481372672393370, 0, NULL, NULL);

-- Add the foreign key constraints with proper ON DELETE behaviors
PRAGMA foreign_keys = OFF;

-- Add the foreign key for guild_id
CREATE TABLE bot_admins_new (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    role_id INTEGER NOT NULL,
    added_by INTEGER,
    added_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    guild_id INTEGER,
    UNIQUE(role_id, guild_id) ON CONFLICT REPLACE,
    FOREIGN KEY (guild_id) REFERENCES guilds(guild_id) ON DELETE CASCADE
);

-- Copy data to the new table with constraints
INSERT INTO bot_admins_new (id, role_id, added_by, added_at, guild_id)
SELECT id, role_id, added_by, added_at, guild_id FROM bot_admins;

-- Drop the old table and rename the new one
DROP TABLE bot_admins;
ALTER TABLE bot_admins_new RENAME TO bot_admins;

-- Recreate the index
CREATE INDEX IF NOT EXISTS idx_bot_admins_guild ON bot_admins(guild_id);

-- Re-enable foreign keys
PRAGMA foreign_keys = ON;
