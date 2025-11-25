-- Add guild_id column to bot_admins table
ALTER TABLE bot_admins ADD COLUMN guild_id INTEGER NOT NULL DEFAULT 0;

-- Update existing records with a default guild ID (you'll need to replace 0 with your actual guild ID)
-- Note: This is a temporary fix - we'll update it with the actual guild ID

-- Create a new index on guild_id for faster lookups
CREATE INDEX IF NOT EXISTS idx_bot_admins_guild ON bot_admins(guild_id);

-- Drop and recreate the table with the correct schema (if the above ALTER doesn't work)
-- First, create a backup of the existing data
CREATE TABLE IF NOT EXISTS bot_admins_backup AS SELECT * FROM bot_admins;

-- Drop the existing table
DROP TABLE IF EXISTS bot_admins;

-- Recreate the table with the correct schema
CREATE TABLE bot_admins (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    role_id INTEGER NOT NULL,
    added_by INTEGER NOT NULL,
    added_at DATETIME NOT NULL,
    guild_id INTEGER NOT NULL,
    UNIQUE(role_id, guild_id) ON CONFLICT REPLACE
);

-- Restore the data with a default guild ID
-- Replace 0 with your actual guild ID
INSERT INTO bot_admins (role_id, added_by, added_at, guild_id)
SELECT role_id, added_by, added_at, 0 FROM bot_admins_backup;

-- Drop the backup table
DROP TABLE IF EXISTS bot_admins_backup;
