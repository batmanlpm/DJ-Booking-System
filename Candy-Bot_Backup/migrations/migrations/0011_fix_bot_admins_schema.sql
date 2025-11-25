-- Create a backup of the existing data
CREATE TABLE IF NOT EXISTS bot_admins_backup AS SELECT * FROM bot_admins;

-- Drop the existing table
DROP TABLE IF EXISTS bot_admins;

-- Recreate the table with the correct schema
CREATE TABLE bot_admins (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    role_id INTEGER NOT NULL,
    added_by INTEGER NOT NULL,
    added_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    guild_id INTEGER NOT NULL,
    UNIQUE(role_id, guild_id) ON CONFLICT REPLACE,
    FOREIGN KEY (added_by) REFERENCES users (user_id) ON DELETE SET NULL,
    FOREIGN KEY (guild_id) REFERENCES guilds (guild_id) ON DELETE CASCADE
);

-- Create an index for faster lookups
CREATE INDEX IF NOT EXISTS idx_bot_admins_guild ON bot_admins(guild_id);

-- Note: You'll need to re-add admin roles after this migration
-- as we're not migrating the old data due to schema changes
