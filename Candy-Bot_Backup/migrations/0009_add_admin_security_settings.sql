-- Add admin security settings table
CREATE TABLE IF NOT EXISTS admin_security_settings (
    guild_id INTEGER PRIMARY KEY,
    security_enabled BOOLEAN DEFAULT 1,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
