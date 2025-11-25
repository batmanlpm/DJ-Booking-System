-- Add anti-raid settings table
CREATE TABLE IF NOT EXISTS anti_raid_settings (
    guild_id INTEGER PRIMARY KEY,
    enabled BOOLEAN DEFAULT 1,
    message_threshold INTEGER DEFAULT 10,
    time_window INTEGER DEFAULT 5,
    lock_duration INTEGER DEFAULT 300,
    exempt_roles TEXT DEFAULT '[]',
    FOREIGN KEY (guild_id) REFERENCES guilds(guild_id) ON DELETE CASCADE
);
