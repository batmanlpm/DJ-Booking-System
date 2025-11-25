-- First, create a backup of the existing data
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
    UNIQUE(role_id, guild_id) ON CONFLICT REPLACE
);

-- Create an index for faster lookups
CREATE INDEX IF NOT EXISTS idx_bot_admins_guild ON bot_admins(guild_id);

-- Restore data with a default guild ID (you'll need to update this with actual guild IDs)
-- For now, we'll skip this step since we want to start fresh with proper guild associations
-- You'll need to re-add admin roles using the /admin command

-- Drop the backup table
DROP TABLE IF EXISTS bot_admins_backup;
