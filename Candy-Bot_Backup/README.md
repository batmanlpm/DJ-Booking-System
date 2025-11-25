# CandyBot - Discord Radio Bot

A comprehensive Discord bot for managing radio streams, server security, and community engagement. This bot integrates with RadioBOSS API to provide radio control and moderation features for Discord communities.

## Key Features

### Radio Control
- Streaming radio directly in voice channels
- Song requests system via `/request` command
- Now playing information `/currently-playing`
- Volume control and playback management `/join`

### Security System
- Anti-raid protection
- Admin action tracking
- Automated security monitoring
- Customizable security settings
- Member verification
- Settings in `/admin`

### Event System
- Schedule and manage events
- Timezone support
- Event reminders
- Simple event setup

### Welcome System
- Custom welcome message with 18+ warning

## Quick Start

### Prerequisites
- Python 3.8 or higher (recommended 3.13.2)
- pip (python package manager)
- Discord bot token (from [Discord Developer Portal](https://discord.com/developers/applications))
- RadioBOSS API credentials (if using RadioBOSS features)

### Installation

1. **Create and activate virtual environment**
   ```bash
   python -m venv venv
   .\venv\Scripts\activate  # Windows
   source venv/bin/activate  # Linux/Mac
   ```

2. **Install dependencies**
   ```bash
   pip install -r requirements.txt
   ```

3. **Configure environment variables**
   Go to `config.py` file in the root directory and fill in the following variables:
   ```python
   DISCORD_BOT_TOKEN = 'YOUR_TOKEN'  # your discord bot token
   BOT_PREFIX = '!'  # command prefix

   # radioboss configuration
   # use the direct stream URL since the API endpoint is not accessible
   RADIOBOSS_API_URL = 'https://c4.radioboss.fm/api'  # this might not be accessible
   RADIOBOSS_API_KEY = '9CT2KPOHAG9N'  # your radioboss api key
   RADIOBOSS_OWNER_DM_USER_ID = 1333179341118636032  # your discord user id for dms
   RADIOBOSS_STREAM_URL = 'https://c4.radioboss.fm:8560/stream'  # direct stream url (tested and working)
   ```

4. **Initialize the database (skip if bot_database.db exists)**
   ```bash
   python setup_tables.py
   ```

5. **Run the bot**
   ```bash
   python main.py
   ```

## Bot Invitation

To invite the bot to your server:
1. Create a bot application at [Discord Developer Portal](https://discord.com/developers/applications)
2. Copy the bot token to the `config.py` file
3. Use the following URL to invite the bot (replace `<client_id>` with your bot's client ID):
   ```
   https://discord.com/oauth2/authorize?client_id=<client_id>&permissions=8&scope=bot%20applications.commands
   ```

## Database Schema

The bot uses PostgreSQL database for data persistence with the following schema:

### Tables

#### `guilds`
- `guild_id` (BIGINT, PRIMARY KEY): Discord guild ID
- `owner_id` (BIGINT): Discord ID of the guild owner
- `admin_role_id` (BIGINT, NULLABLE): Role ID with admin privileges
- `event_channel_id` (BIGINT, NULLABLE): Channel ID for event notifications
- `welcome_channel_id` (BIGINT, NULLABLE): Channel ID for welcome messages
- `log_channel_id` (BIGINT, NULLABLE): Channel ID for bot logs
- `created_at` (TIMESTAMP): When the guild record was created
- `updated_at` (TIMESTAMP): When the guild record was last updated

#### `users`
- `user_id` (BIGINT, PRIMARY KEY): Discord user ID
- `timezone` (TEXT, NULLABLE): User's timezone (e.g., 'America/New_York')
- `is_banned` (BOOLEAN, DEFAULT FALSE): If the user is banned from using the bot
- `created_at` (TIMESTAMP): When the user record was created
- `last_seen` (TIMESTAMP): When the user was last active

#### `events`
- `event_id` (SERIAL, PRIMARY KEY): Unique event identifier
- `guild_id` (BIGINT, FOREIGN KEY): Reference to guilds.guild_id
- `creator_id` (BIGINT): Discord ID of the event creator
- `name` (TEXT): Name of the event
- `description` (TEXT, NULLABLE): Detailed description of the event
- `start_time` (TIMESTAMP): When the event starts
- `end_time` (TIMESTAMP, NULLABLE): When the event ends
- `timezone` (TEXT): Timezone of the event
- `max_participants` (INTEGER, NULLABLE): Maximum number of participants
- `is_recurring` (BOOLEAN, DEFAULT FALSE): If the event repeats
- `recurrence_pattern` (TEXT, NULLABLE): Recurrence rule (e.g., 'DAILY', 'WEEKLY')
- `created_at` (TIMESTAMP): When the event was created
- `updated_at` (TIMESTAMP): When the event was last updated

#### `event_participants`
- `event_id` (INTEGER, FOREIGN KEY): Reference to events.event_id
- `user_id` (BIGINT, FOREIGN KEY): Reference to users.user_id
- `status` (TEXT): RSVP status ('GOING', 'INTERESTED', 'NOT_GOING')
- `joined_at` (TIMESTAMP, NULLABLE): When the user joined the event
- `left_at` (TIMESTAMP, NULLABLE): When the user left the event
- PRIMARY KEY (`event_id`, `user_id`)

#### `security_settings`
- `guild_id` (BIGINT, PRIMARY KEY, FOREIGN KEY): Reference to guilds.guild_id
- `anti_raid_enabled` (BOOLEAN, DEFAULT TRUE): If anti-raid protection is enabled
- `anti_raid_threshold` (INTEGER, DEFAULT 5): Max joins before action is taken
- `anti_raid_timeframe` (INTEGER, DEFAULT 10): Timeframe in seconds to monitor for raids
- `anti_nuke_enabled` (BOOLEAN, DEFAULT TRUE): If anti-nuke protection is enabled
- `anti_nuke_threshold` (INTEGER, DEFAULT 5): Max actions before nuke protection triggers
- `anti_nuke_timeframe` (INTEGER, DEFAULT 10): Timeframe in seconds to monitor for nukes
- `anti_spam_enabled` (BOOLEAN, DEFAULT TRUE): If anti-spam protection is enabled
- `blocked_keywords` (TEXT[], NULLABLE): Array of blocked keywords
- `block_links` (BOOLEAN, DEFAULT FALSE): If links should be blocked
- `mute_role_id` (BIGINT, NULLABLE): Role ID to assign for mutes
- `log_security_events` (BOOLEAN, DEFAULT TRUE): If security events should be logged
- `updated_at` (TIMESTAMP): When the settings were last updated

#### `admin_actions`
- `action_id` (SERIAL, PRIMARY KEY): Unique action identifier
- `guild_id` (BIGINT, FOREIGN KEY): Reference to guilds.guild_id
- `admin_id` (BIGINT, FOREIGN KEY): Reference to users.user_id
- `action_type` (TEXT): Type of action (e.g., 'BAN', 'KICK', 'MUTE')
- `target_id` (BIGINT): Discord ID of the target user
- `reason` (TEXT, NULLABLE): Reason for the action
- `duration` (INTERVAL, NULLABLE): Duration of the action (for temporary actions)
- `created_at` (TIMESTAMP): When the action was performed

## Voice Features

The bot provides comprehensive voice channel management and radio streaming capabilities.

### Voice Commands

#### `/join`
Make the bot join your voice channel and start streaming the radio.

**Requirements:**
- You must be in a voice channel
- Bot must have permission to join and speak in the channel

**Features:**
- Streams audio from the configured radio URL
- Includes pause/play controls
- Automatically leaves when the voice channel is empty
- Displays currently playing track information
- Volume control

#### `/leave`
Make the bot leave the voice channel.

## RadioBOSS Integration

The bot integrates with RadioBOSS radio automation software using its remote control API.

### Available Commands

#### `/currently-playing`
Display information about the currently playing track.

#### `/request <song_name>`
Request a song to be played on the radio.

**Examples:**
```
/request song_name:"Bohemian Rhapsody"
```

**Responses:**
- **Success**: Confirmation with song name and position in queue
- **Not Found**: Notification to the owner of RadioBoss that the song wasn't found
- **Error**: Appropriate error message with details

## Security Features

The bot includes a comprehensive security system to protect your server.

### Automatic Protection
- **Anti-Raid**: Detects and prevents mass joins
- **Anti-Nuke**: Prevents mass channel/message deletion
- **Anti-Spam**: Blocks rapid message sending

## Event System

Manage and schedule events with the built-in event system.

### Event Commands

#### `/event-schedule [name] [datetime] [timezone] [description]`
Create a new event.

#### `/event-list`
List upcoming events.

#### `/event-delete [event_id]`
Cancel an event (creator/admins only).

### Event Features
- Timezone support
- Recurring events
- Automatic reminders
- Event role management

## Project Structure

```
DJCandyJo-Latest/
├── .env                      # Environment variables (not in version control)
├── .env.example              # Example environment variables
├── .gitignore                # Git ignore file
├── main.py                   # Main bot entry point
├── config.py                 # Configuration settings
├── requirements.txt          # Python dependencies
├── setup_tables.py           # Database initialization script
├── reset_database.py         # Database reset utility
└── modules/                  # Bot modules
    ├── __init__.py
    ├── admin/                # Admin commands and management
    │   ├── __init__.py
    │   ├── admin_commands.py
    │   └── anti_raid_ui.py
    ├── database/             # Database operations
    │   ├── __init__.py
    │   └── database.py
    ├── events/               # Event management
    │   ├── __init__.py
    │   ├── event_commands.py
    │   ├── event_instance.py
    │   ├── event_manager.py
    │   └── timezone_commands.py
    ├── radioboss/            # RadioBOSS integration
    │   ├── __init__.py
    │   ├── radioboss_api.py
    │   └── radioboss_cog.py
    ├── request/              # Song request system
    │   ├── __init__.py
    │   └── request_cog.py
    ├── security/             # Security features
    │   ├── __init__.py
    │   ├── admin_action_tracker.py
    │   ├── admin_security.py
    │   ├── anti_raid.py
    │   ├── anti_raid_cog.py
    │   ├── bot_security.py
    │   ├── raid_commands.py
    │   ├── security_events.py
    │   └── settings.py
    ├── voice/                # Voice channel management
    │   ├── __init__.py
    │   └── voice_cog.py
    └── welcome/              # Welcome system
        ├── __init__.py
        └── welcome_cog.py
# This file tree was generated automatically
```

## Troubleshooting

### Common Issues

**Bot won't start**
- Verify your bot token is correct in `config.py`
- Check if all required environment variables are set
- Ensure the bot has proper permissions in your Discord server
- Check the logs for any error messages

**RadioBOSS commands not working**
- Verify your RadioBOSS API key is correct
- Ensure the RadioBOSS server is running and accessible
- Check if the stream URL is correct and accessible
- Verify network connectivity between the bot and RadioBOSS server

**Database connection issues**
- Ensure `bot_database.db` exists
- If it doesnt, run `setup_tables.py` to initialize the database schema

## Contributing

1. Fork the repository
2. Create a new branch for your feature/fix
3. Commit your changes
4. Push to the branch
5. Open a pull request

## Developer
- This bot was created by nextcodeworks, if you have any issues, questions or suggestions, please contact us trough our website: nextcodeworks.github.io