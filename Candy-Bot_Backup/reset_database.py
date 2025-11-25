"""
script to reset the database with the correct schema.
run this script to fix foreign key constraint issues.
"""
import os
import sqlite3
from pathlib import Path

# get the database path
db_path = 'bot_database.db'
backup_path = 'bot_database_backup.db'

# create a backup of the existing database
if os.path.exists(db_path):
    if os.path.exists(backup_path):
        os.remove(backup_path)
    os.rename(db_path, backup_path)
    print(f"created backup at {backup_path}")

# connect to the database (this will create a new one)
conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# enable foreign key constraints
cursor.execute('PRAGMA foreign_keys = ON')

# create the guilds table with proper constraints
cursor.execute('''
CREATE TABLE IF NOT EXISTS guilds (
    guild_id INTEGER PRIMARY KEY,
    owner_id INTEGER NOT NULL,
    admin_role_id INTEGER,
    event_channel_id INTEGER,
    UNIQUE(guild_id, event_channel_id)
)
''')

# create the users table
cursor.execute('''
CREATE TABLE IF NOT EXISTS users (
    user_id INTEGER PRIMARY KEY,
    timezone TEXT
)
''')

# create the events table with proper foreign key constraints
cursor.execute('''
CREATE TABLE IF NOT EXISTS events (
    event_id INTEGER PRIMARY KEY AUTOINCREMENT,
    guild_id INTEGER NOT NULL,
    name TEXT NOT NULL,
    time TEXT NOT NULL,
    timezone TEXT NOT NULL,
    description TEXT,
    event_channel_id INTEGER,
    FOREIGN KEY (guild_id) REFERENCES guilds (guild_id) ON DELETE CASCADE,
    FOREIGN KEY (guild_id, event_channel_id) REFERENCES guilds (guild_id, event_channel_id) ON DELETE SET NULL
)
''')

# create the admins table
cursor.execute('''
CREATE TABLE IF NOT EXISTS admins (
    guild_id INTEGER NOT NULL,
    user_id INTEGER NOT NULL,
    is_trusted BOOLEAN DEFAULT 0,
    PRIMARY KEY (guild_id, user_id),
    FOREIGN KEY (guild_id) REFERENCES guilds (guild_id) ON DELETE CASCADE
)
''')

# create the security_settings table
cursor.execute('''
CREATE TABLE IF NOT EXISTS security_settings (
    guild_id INTEGER PRIMARY KEY,
    anti_raid_enabled BOOLEAN DEFAULT 0,
    anti_raid_threshold INTEGER DEFAULT 5,
    anti_raid_timeframe INTEGER DEFAULT 10,
    anti_nuke_enabled BOOLEAN DEFAULT 0,
    anti_nuke_threshold INTEGER DEFAULT 10,
    anti_nuke_timeframe INTEGER DEFAULT 5,
    anti_spam_enabled BOOLEAN DEFAULT 0,
    blocked_keywords TEXT,
    block_links BOOLEAN DEFAULT 0,
    FOREIGN KEY (guild_id) REFERENCES guilds (guild_id) ON DELETE CASCADE
)
''')

# create the event_reminders table
cursor.execute('''
CREATE TABLE IF NOT EXISTS event_reminders (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    event_id INTEGER NOT NULL,
    reminder_type TEXT NOT NULL,
    sent_at TEXT NOT NULL,
    FOREIGN KEY (event_id) REFERENCES events (event_id) ON DELETE CASCADE,
    UNIQUE(event_id, reminder_type)
)
''')

# create the admin_security_settings table
cursor.execute('''
CREATE TABLE IF NOT EXISTS admin_security_settings (
    guild_id INTEGER PRIMARY KEY,
    security_enabled BOOLEAN DEFAULT 1,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (guild_id) REFERENCES guilds (guild_id) ON DELETE CASCADE
)
''')

# create indexes for better performance
cursor.execute('''
CREATE INDEX IF NOT EXISTS idx_events_guild_id ON events(guild_id)
''')

cursor.execute('''
CREATE INDEX IF NOT EXISTS idx_events_time ON events(time)
''')

# commit changes and close the connection
conn.commit()
conn.close()

print(f"Database has been reset with the correct schema at {db_path}")
print("You can now restart your bot.")
