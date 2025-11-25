-- First, create a backup of the existing data
CREATE TABLE IF NOT EXISTS bot_admins_backup AS SELECT * FROM bot_admins;

-- Drop the existing table
DROP TABLE IF EXISTS bot_admins;

-- Recreate the table with a NULL-able guild_id
CREATE TABLE bot_admins (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    role_id INTEGER NOT NULL,
    added_by INTEGER,  -- NULL-able
    added_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    guild_id INTEGER,  -- Now NULL-able
    UNIQUE(role_id, guild_id) ON CONFLICT REPLACE,
    FOREIGN KEY (added_by) REFERENCES users (user_id) ON DELETE SET NULL,
    FOREIGN KEY (guild_id) REFERENCES guilds (guild_id) ON DELETE CASCADE
);

-- Recreate the index
CREATE INDEX IF NOT EXISTS idx_bot_admins_guild ON bot_admins(guild_id);

-- Insert default guild if it doesn't exist
INSERT OR IGNORE INTO guilds (guild_id, owner_id, admin_role_id, event_channel_id)
VALUES (1363481372672393370, 0, NULL, NULL);  -- Replace 0 with the actual owner ID if needed
