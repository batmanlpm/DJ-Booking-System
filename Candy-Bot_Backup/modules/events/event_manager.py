"""
event manager for handling event scheduling and notifications.
"""
import asyncio
import datetime
import pytz
from typing import Dict, List, Optional, Tuple
import discord
from discord.ext import tasks
from modules.database.database import db

__all__ = ['EventManager']

class EventManager:
    """manages event scheduling and notifications."""
    
    def __init__(self):
        """initialize the event manager."""
        self.bot = None
        self.active_events = {}
        self.notification_task = None
        
    async def initialize(self, bot):
        """initialize the event manager with the bot instance."""
        self.bot = bot
        await self.load_events()
        self._start_notification_loop()
    
    async def load_events(self):
        """load all upcoming events from the database."""
        try:
            # clear existing events
            self.active_events = {}
            
            # try to get the events with the new schema first
            try:
                query = """
                SELECT 
                    e.*, 
                    COALESCE(e.event_channel_id, g.event_channel_id) as event_channel_id
                FROM events e
                LEFT JOIN guilds g ON e.guild_id = g.guild_id
                WHERE datetime(e.time) > datetime('now')
                """
                events = db.execute_query(query, fetch=True)
                
            except sqlite3.OperationalError as e:
                if 'no such column' in str(e).lower():
                    print("Database schema is outdated. Attempting to update...")
                    # try to add the missing column
                    try:
                        db.execute_query("""
                        ALTER TABLE events 
                        ADD COLUMN event_channel_id INTEGER 
                        REFERENCES guilds(event_channel_id) 
                        ON DELETE SET NULL
                        """, commit=True)
                        print("Successfully updated database schema.")
                        
                        # update existing events with guild's event channel
                        db.execute_query("""
                        UPDATE events 
                        SET event_channel_id = (
                            SELECT event_channel_id 
                            FROM guilds 
                            WHERE guilds.guild_id = events.guild_id
                        )
                        WHERE event_channel_id IS NULL
                        """, commit=True)
                        
                        # try the query again
                        query = """
                        SELECT 
                            e.*, 
                            COALESCE(e.event_channel_id, g.event_channel_id) as event_channel_id
                        FROM events e
                        LEFT JOIN guilds g ON e.guild_id = g.guild_id
                        WHERE datetime(e.time) > datetime('now')
                        """
                        events = db.execute_query(query, fetch=True)
                        
                    except Exception as e:
                        print(f"Error updating database schema: {e}")
                        # fall back to basic query without the new column
                        print("Falling back to basic event loading...")
                        query = """
                        SELECT * FROM events 
                        WHERE datetime(time) > datetime('now')
                        """
                        events = db.execute_query(query, fetch=True)
                else:
                    # some other randomdatabase errors
                    print(f"Database error: {e}")
                    return []
            
            if not events:
                print("No upcoming events found in the database.")
                return []
                
            loaded_count = 0
            for event in events:
                try:
                    # convert database row to dict and ensure all fields are present
                    if hasattr(event, 'keys'):  # if it's a dict-like object
                        event_dict = {}
                        event_dict['event_id'] = event['event_id']
                        event_dict['guild_id'] = event['guild_id']
                        event_dict['name'] = event['name']
                        event_dict['time'] = event['time']
                        event_dict['timezone'] = event.get('timezone', 'UTC') if hasattr(event, 'get') else (event['timezone'] if 'timezone' in event else 'UTC')
                        event_dict['description'] = event.get('description', '') if hasattr(event, 'get') else (event['description'] if 'description' in event else '')
                        event_dict['event_channel_id'] = event.get('event_channel_id') if hasattr(event, 'get') else (event['event_channel_id'] if 'event_channel_id' in event else None)
                    else:
                        # fallback for direct attribute access if needed
                        event_dict = {
                            'event_id': event['event_id'],
                            'guild_id': event['guild_id'],
                            'name': event['name'],
                            'time': event['time'],
                            'timezone': getattr(event, 'timezone', 'UTC'),
                            'description': getattr(event, 'description', ''),
                            'event_channel_id': getattr(event, 'event_channel_id', None)
                        }
                    
                    self.active_events[event_dict['event_id']] = event_dict
                    print(f"Loaded event: {event_dict['name']} (ID: {event_dict['event_id']})")
                    loaded_count += 1
                    
                except Exception as e:
                    print(f"Error loading event {event.get('event_id', 'unknown')}: {e}")
                    continue
                    
            print(f"Successfully loaded {loaded_count} upcoming events.")
            return events
            
        except Exception as e:
            print(f"Error loading events from database: {e}")
            return []
    
    def _start_notification_loop(self):
        """start the background task for checking event notifications."""
        if self.notification_task is not None and not self.notification_task.done():
            self.notification_task.cancel()
            
        self.notification_task = self.bot.loop.create_task(self._check_event_notifications())
    
    async def _check_event_notifications(self):
        """background task to check for upcoming events and send notifications."""
        while True:
            try:
                now = datetime.datetime.utcnow().replace(tzinfo=datetime.timezone.utc)
                
                # check each active event
                for event_id, event in list(self.active_events.items()):
                    try:
                        # parse the event time and ensure it's timezone-aware
                        event_time = datetime.datetime.fromisoformat(event['time'])
                        if event_time.tzinfo is None:
                            event_time = event_time.replace(tzinfo=datetime.timezone.utc)
                        time_until = event_time - now
                        
                        # check if it's time to send a reminder (1 hour before)
                        if datetime.timedelta(hours=0, minutes=59) < time_until <= datetime.timedelta(hours=1):
                            # check if we've already sent a reminder for this event
                            query = """
                            SELECT 1 FROM event_reminders 
                            WHERE event_id = ? AND reminder_type = 'dm_1h'
                            """
                            result = db.execute_query(query, (event_id,), fetch=True)
                            
                            if not result:
                                print(f"Sending 1h reminder for event {event_id}: {event['name']}")
                                # send dm reminders
                                await self._send_dm_reminders(event)
                                
                                # record that we've sent the reminder
                                # use insert or ignore to handle race conditions
                                query = """
                                INSERT OR IGNORE INTO event_reminders (event_id, reminder_type, sent_at)
                                VALUES (?, ?, ?)
                                """
                                try:
                                    db.execute_query(
                                        query,
                                        (event_id, 'dm_1h', datetime.datetime.utcnow().isoformat()),
                                        commit=True
                                    )
                                    print(f"Recorded 1h reminder for event {event_id}")
                                except sqlite3.IntegrityError:
                                    # Another instance might have inserted the record
                                    print(f"Reminder already recorded for event {event_id}")
                                    pass
                                print(f"Sent 1h reminder for event {event_id}: {event['name']}")
                        
                        # check if the event has started (within last 5 minutes to handle restarts)
                        elif datetime.timedelta(minutes=-5) <= time_until <= datetime.timedelta(seconds=0):
                            print(f"Event {event_id} has started or is about to start: {event['name']}")
                            # check if we've already sent the start notification
                            query = """
                            SELECT 1 FROM event_reminders 
                            WHERE event_id = ? AND reminder_type = 'start_notification'
                            """
                            result = db.execute_query(query, (event_id,), fetch=True)
                            
                            if not result:
                                # send event start notification
                                await self._send_event_start_notification(event)
                                
                                # record that we've sent the notification
                                query = """
                                INSERT INTO event_reminders (event_id, reminder_type, sent_at)
                                VALUES (?, ?, ?)
                                """
                                db.execute_query(
                                    query,
                                    (event_id, 'start_notification', datetime.datetime.utcnow().isoformat()),
                                    commit=True
                                )
                                print(f"Sent start notification for event {event_id}: {event['name']}")
                            
                            # remove the event from active events if it's in the past
                            if time_until < datetime.timedelta(minutes=-5):
                                self.active_events.pop(event_id, None)
                                print(f"Removed past event {event_id} from active events")
                        
                        # clean up old events (more than 1 day old)
                        elif time_until < datetime.timedelta(days=-1):
                            self.active_events.pop(event_id, None)
                            print(f"Cleaned up old event {event_id}: {event['name']}")
                            
                    except Exception as e:
                        print(f"Error processing event {event_id}: {e}")
                        import traceback
                        traceback.print_exc()
                        # remove the problematic event to prevent repeated errors
                        self.active_events.pop(event_id, None)
                
                # wait for 30 seconds before checking again
                await asyncio.sleep(30)
                
            except Exception as e:
                print(f"Error in notification loop: {e}")
                import traceback
                traceback.print_exc()
                await asyncio.sleep(60)  # wait longer on error
    
    async def _send_dm_reminders(self, event: dict):
        """send dm reminders to all non-bot members in all servers."""
        print("\n" + "="*50)
        print("STARTING DM SENDING PROCESS")
        print("="*50)
        
        try:
            # get event details
            event_id = event.get('event_id')
            event_name = event.get('name', 'Unnamed Event')
            event_time = datetime.datetime.fromisoformat(event['time']).replace(tzinfo=datetime.timezone.utc)
            
            print(f"\n🔔 Processing event: {event_name} (ID: {event_id})")
            print(f"📅 Event time: {event_time.isoformat()}")
            
            # check if we've already processed this event
            query = """
            SELECT 1 FROM event_reminders 
            WHERE event_id = ? AND reminder_type = 'dm_1h'
            """
            already_processed = db.execute_query(query, (event_id,), fetch=True)
            
            # create a set to track users who have already received a dm for this event
            sent_to_users = set()
            
            # if we've already processed this event, load the sent users
            if already_processed:
                print("ℹ️  dm reminders already processed for this event")
                return
                
            # get all guilds
            guilds = list(self.bot.guilds)
            print(f"\n🌐 Found {len(guilds)} guilds")
            
            total_members = 0
            total_sent = 0
            
            # process each guild
            for guild in guilds:
                try:
                    print(f"\n🏰 Processing guild: {guild.name} (ID: {guild.id})")
                    
                    # fetch all members with detailed debugging
                    print(f"🔄 Fetching members for {guild.name}...")
                    try:
                        # debug guild member count
                        print(f"   Guild member count: {guild.member_count}")
                        print(f"   Bot has members intent: {self.bot.intents.members}")
                        
                        # ensure we have the latest member data
                        print("   Chunking guild members...")
                        try:
                            await guild.chunk(cache=True)
                            print("   Guild chunking completed")
                        except Exception as chunk_error:
                            print(f"   Error during chunking: {type(chunk_error).__name__}: {chunk_error}")
                        
                        # get all non-bot members
                        members = [m for m in guild.members if not m.bot]
                        print(f"👥 Found {len(members)} non-bot members (out of {len(guild.members)} total members)")
                        
                        # debug: print member count by status
                        status_count = {}
                        for m in guild.members:
                            status = str(m.status)
                            status_count[status] = status_count.get(status, 0) + 1
                        print(f"   Member statuses: {status_count}")
                        
                        # debug: print the first few member names if any
                        if members:
                            member_info = []
                            for m in members[:5]:
                                perms = m.guild_permissions
                                can_dm = '✅' if perms.read_messages else '❌'
                                member_info.append(f"{m} (DM: {can_dm})")
                            print(f"   Sample members: {', '.join(member_info)}" + ('...' if len(members) > 5 else ''))
                        else:
                            print("   No non-bot members found in the guild")
                    except Exception as e:
                        print(f"❌ Error fetching members: {e}")
                        continue
                    
                    # process each member
                    for member in members:
                        try:
                            total_members += 1
                            
                            # skip if bot or already processed
                            if member.bot:
                                continue
                            
                            print(f"\n👤 Processing member: {member} (ID: {member.id})")
                            
                            # get user's timezone
                            user_timezone = None
                            try:
                                user_timezone = await self.get_user_timezone(member.id)
                                print(f"  ℹ️  Timezone for {member}: {user_timezone}")
                            except Exception as e:
                                print(f"  ❌ Error getting timezone for {member}: {e}")
                                # continue anyway, we'll use utc as fallback
                                
                            try:
                                # format time based on user's timezone
                                time_display = ""
                                if user_timezone:
                                    try:
                                        user_tz = pytz.timezone(user_timezone)
                                        user_time = event_time.astimezone(user_tz)
                                        time_display = user_time.strftime('%Y-%m-%d %H:%M %Z')
                                    except Exception as tz_error:
                                        print(f"  ❌ Error processing timezone {user_timezone} for {member}: {tz_error}")
                                        # fall back to utc if timezone is invalid
                                        time_display = event_time.strftime('%Y-%m-%d %H:%M %Z (UTC)')
                                else:
                                    # use utc if no timezone set
                                    time_display = event_time.strftime('%Y-%m-%d %H:%M %Z (UTC)')
                                
                                # create embed for dm with the requested design
                                embed = discord.Embed(
                                    title=f"Upcoming Event: {event_name} - ***in one hour***",
                                    description=f"**Discord server:** {guild.name}",
                                    color=3092790  # using the specified blue color
                                )
                                
                                # add fields
                                embed.add_field(
                                    name="Event Time",
                                    value=time_display,
                                    inline=True
                                )
                                
                                embed.add_field(
                                    name="Event Description",
                                    value=event.get('description', 'No description provided.'),
                                    inline=True
                                )
                                
                                # add bot's avatar as author
                                bot_user = self.bot.user
                                embed.set_author(
                                    name=bot_user.display_name,
                                    icon_url=bot_user.display_avatar.url
                                )
                                
                                # add footer
                                embed.set_footer(
                                    text="This is an automated reminder for an upcoming event, you can disable reminders by blocking this bot."
                                )
                                
                            except Exception as tz_error:
                                print(f"  ❌ Error creating embed for {member}: {tz_error}")
                                continue
                            
                            # skip if we've already sent to this user
                            if member.id in sent_to_users:
                                print(f"  ℹ️  Already sent DM to {member} (ID: {member.id}) for this event")
                                continue
                                
                            # try to send DM
                            try:
                                # create DM channel if needed
                                if member.dm_channel is None:
                                    print("  Creating DM channel...")
                                    try:
                                        await member.create_dm()
                                        print("  ✅ DM channel created")
                                    except Exception as e:
                                        print(f"  ❌ Failed to create DM channel: {e}")
                                        continue
                                
                                # check if we've already sent to this user for this event
                                query = """
                                SELECT 1 FROM event_dm_log 
                                WHERE event_id = ? AND user_id = ?
                                """
                                already_sent = db.execute_query(query, (event_id, member.id), fetch=True)
                                
                                if already_sent:
                                    print(f"  ℹ️  Already sent DM to {member} (ID: {member.id}) for this event (from database)")
                                    sent_to_users.add(member.id)
                                    continue
                                
                                # send the embed
                                print("  Sending DM...")
                                try:
                                    await member.dm_channel.send(embed=embed)
                                    total_sent += 1
                                    print(f"  ✅ DM sent to {member} (ID: {member.id})")
                                    
                                    # log the successful DM
                                    try:
                                        query = """
                                        INSERT INTO event_dm_log (event_id, user_id, sent_at)
                                        VALUES (?, ?, ?)
                                        """
                                        db.execute_query(
                                            query,
                                            (event_id, member.id, datetime.datetime.utcnow().isoformat()),
                                            commit=True
                                        )
                                        sent_to_users.add(member.id)
                                    except Exception as e:
                                        print(f"  ❌ Error logging DM to database: {e}")
                                        
                                except discord.Forbidden:
                                    print(f"  ❌ Cannot send DM (user has DMs disabled)")
                                except Exception as e:
                                    print(f"  ❌ Error sending DM: {type(e).__name__}: {e}")
                                
                                # small delay to avoid rate limits
                                await asyncio.sleep(0.5)
                                
                            except Exception as e:
                                print(f"  ❌ Unexpected error in DM process: {type(e).__name__}: {e}")
                                continue
                                
                        except Exception as e:
                            print(f"  ❌ Error processing member {member}: {type(e).__name__}: {e}")
                            continue
                            
                except Exception as e:
                    print(f"\n❌ Error in guild {guild.name}: {type(e).__name__}: {e}")
                    continue
            
            # log completion
            print("\n" + "="*50)
            print("DM SENDING COMPLETE")
            print("="*50)
            print(f"Total members processed: {total_members}")
            print(f"Total DMs sent: {total_sent}")
            
            # update database to mark this event as processed
            try:
                # first ensure the event is marked as processed
                query = """
                INSERT OR IGNORE INTO event_reminders (event_id, reminder_type, sent_at)
                VALUES (?, 'dm_1h', ?)
                """
                db.execute_query(
                    query, 
                    (event_id, datetime.datetime.utcnow().isoformat()),
                    commit=True
                )
                
                # create the event_dm_log table if it doesn't exist
                create_table_query = """
                CREATE TABLE IF NOT EXISTS event_dm_log (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    event_id INTEGER NOT NULL,
                    user_id INTEGER NOT NULL,
                    sent_at TEXT NOT NULL,
                    UNIQUE(event_id, user_id)
                )
                """
                db.execute_query(create_table_query, commit=True)
                
                print("\n✅ DM reminders recorded in database")
            except Exception as e:
                print(f"\n❌ Error recording DM reminders: {type(e).__name__}: {e}")
            
            print(f"\n🎉 Successfully sent {total_sent} DM reminders for event: {event_name}")
            print("="*50 + "\n")
                    
        except Exception as e:
            print("\n" + "!"*50)
            print("CRITICAL ERROR IN DM SENDING PROCESS")
            print("!"*50)
            print(f"Error: {type(e).__name__}: {e}")
            import traceback
            traceback.print_exc()
            print("!"*50 + "\n")
    
    async def _send_event_start_notification(self, event: dict):
        """send a notification to the event channel when the event starts."""
        try:
            print(f"\n🔔 Sending event start notification for: {event.get('name')} (ID: {event.get('event_id')})")
            
            # get the guild
            guild = self.bot.get_guild(int(event['guild_id']))
            if not guild:
                print(f"❌ Guild {event['guild_id']} not found")
                return
                
            # get the event channel
            channel_id = event.get('event_channel_id')
            if not channel_id:
                # try to get from guild settings
                query = """
                SELECT event_channel_id 
                FROM guilds 
                WHERE guild_id = ?
                """
                result = db.execute_query(query, (guild.id,), fetch=True)
                if result and result[0]['event_channel_id']:
                    channel_id = result[0]['event_channel_id']
                else:
                    print(f"❌ No event channel set for guild {guild.name}")
                    return
            
            channel = guild.get_channel(int(channel_id))
            if not channel:
                print(f"❌ Event channel {channel_id} not found in guild {guild.name}")
                return
            
            print(f"📢 Found event channel: #{channel.name} ({channel.id}) in {guild.name}")
            
            # get the event time in UTC
            event_time = datetime.datetime.fromisoformat(event['time']).replace(tzinfo=datetime.timezone.utc)
            
            # create embed with the specified design
            embed = discord.Embed(
                title=f"Event Started: **{event['name']}**",
                color=3092790,  # #2F3E56 - dark blue color
                timestamp=datetime.datetime.utcnow()
            )
            
            # add fields
            embed.add_field(
                name="Event Time",
                value=f"<t:{int(event_time.timestamp())}:F>\n(<t:{int(event_time.timestamp())}:R>)",
                inline=True
            )
            
            embed.add_field(
                name="Event Description",
                value=event.get('description', 'No description provided.'),
                inline=True
            )
            
            # set author with bot's info and avatar
            bot_user = self.bot.user
            embed.set_author(
                name=bot_user.display_name,
                icon_url=str(bot_user.avatar.url) if bot_user.avatar else None
            )
            
            # set footer
            embed.set_footer(
                text="Set your timezone with /timezone to get event notifications in private messages in your local time."
            )
            
            # send the notification
            try:
                print(f"📤 Sending event start notification to channel {channel.id}...")
                await channel.send(embed=embed)
                print("✅ Event start notification sent successfully!")
                
                # mark notification as sent in the database
                try:
                    query = """
                    INSERT OR IGNORE INTO event_reminders (event_id, reminder_type, sent_at)
                    VALUES (?, 'event_started', ?)
                    """
                    db.execute_query(
                        query,
                        (event['event_id'], datetime.datetime.utcnow().isoformat()),
                        commit=True
                    )
                    print("✅ Recorded event start notification in database")
                except Exception as db_error:
                    print(f"❌ Error recording notification in database: {db_error}")
                
            except Exception as send_error:
                print(f"❌ Failed to send event start notification: {send_error}")
                raise
            
        except Exception as e:
            print(f"\n❌ Error in _send_event_start_notification: {type(e).__name__}: {e}")
            import traceback
            traceback.print_exc()
            raise            
    async def create_event(self, guild_id: int, name: str, event_time: datetime.datetime, 
                          timezone: str, description: str = None) -> int:
        """create a new event."""
        try:
            # get the event channel ID from guild settings
            query = """
            SELECT event_channel_id 
            FROM guilds 
            WHERE guild_id = ?
            """
            result = db.execute_query(query, (guild_id,), fetch=True)
            event_channel_id = result[0]['event_channel_id'] if result and result[0] and 'event_channel_id' in result[0] and result[0]['event_channel_id'] else None
            
            # insert the event into the database
            query = """
            INSERT INTO events (guild_id, name, time, timezone, description, event_channel_id)
            VALUES (?, ?, ?, ?, ?, ?)
            """
            event_id = db.execute_query(
                query,
                (guild_id, name, event_time.isoformat(), timezone, description, event_channel_id),
                commit=True
            )
            
            if event_id:
                # add to active events
                self.active_events[event_id] = {
                    'event_id': event_id,
                    'guild_id': guild_id,
                    'name': name,
                    'time': event_time.isoformat(),
                    'timezone': timezone,
                    'description': description or '',
                    'event_channel_id': event_channel_id
                }
                
                print(f"Created new event: {name} (ID: {event_id}) in guild {guild_id}")
                
            return event_id
            
        except Exception as e:
            print(f"Error creating event: {e}")
            import traceback
            traceback.print_exc()
            return None

    async def get_user_timezone(self, user_id: int):
        """get a user's timezone."""
        try:
            query = "SELECT timezone FROM user_timezones WHERE user_id = ?"
            result = db.execute_query(query, (user_id,), fetch=True)
            
            if not result or not result[0]:
                print(f"No timezone found for user {user_id}")
                return None
                
            # convert to dict if it's a sqlite3.Row object
            if hasattr(result[0], 'keys'):
                row = dict(result[0])
            else:
                row = result[0]
                
            timezone = row.get('timezone') if isinstance(row, dict) else row[0]
            print(f"Retrieved timezone for user {user_id}: {timezone}")
            return timezone
            
        except Exception as e:
            print(f"Error getting timezone for user {user_id}: {e}")
            import traceback
            traceback.print_exc()
            return None

    async def delete_event(self, event_id: int) -> bool:
        """delete an event."""
        try:
            # delete from database
            query = "DELETE FROM events WHERE event_id = ?"
            db.execute_query(query, (event_id,), commit=True)
            
            # remove from active events
            self.active_events.pop(event_id, None)
            
            return True
            
        except Exception as e:
            print(f"Error deleting event: {e}")
            return False
    
    async def list_events(self, guild_id: int) -> List[dict]:
        """list all events for a guild."""
        try:
            print(f"\n🔍 Fetching events for guild {guild_id}")
            
            # first, check if guild exists in guilds table
            guild_check = db.execute_query("SELECT 1 FROM guilds WHERE guild_id = ?", (guild_id,), fetch=True)
            if not guild_check:
                print(f"⚠️ Guild {guild_id} not found in guilds table")
                # add guild to guilds table if not exists
                try:
                    db.execute_query(
                        "INSERT OR IGNORE INTO guilds (guild_id, owner_id) VALUES (?, ?)",
                        (guild_id, 0),  # using 0 as default owner_id since we don't have it
                        commit=True
                    )
                    print(f"✅ Added guild {guild_id} to guilds table")
                except Exception as e:
                    print(f"❌ Error adding guild to guilds table: {e}")
            
            # check if events table exists
            table_check = db.execute_query(
                "SELECT name FROM sqlite_master WHERE type='table' AND name='events'"
            )
            if not table_check:
                print("❌ Events table does not exist")
                return []
            
            # get all events for this guild, including past ones for debugging
            query = """
            SELECT * FROM events 
            WHERE guild_id = ? 
            ORDER BY datetime(time) ASC
            """
            events = db.execute_query(query, (guild_id,), fetch=True)
            
            if not events:
                print(f"ℹ️ No events found for guild {guild_id} in the database")
                # check if there are any events in the active_events cache
                cached_events = [e for e in self.active_events.values() if e.get('guild_id') == guild_id]
                if cached_events:
                    print(f"ℹ️ Found {len(cached_events)} events in active_events cache")
                    return cached_events
                return []
            
            print(f"✅ Found {len(events)} events in database")
            
            # convert to list of dicts and ensure all required fields exist
            result = []
            for event in events:
                try:
                    event_dict = dict(event)
                    # ensure all required fields exist
                    if 'event_id' not in event_dict:
                        print(f"⚠️ Event missing event_id: {event}")
                        continue
                    if 'time' not in event_dict:
                        print(f"⚠️ Event {event_dict.get('event_id')} missing time")
                        continue
                    
                    result.append(event_dict)
                except Exception as e:
                    print(f"❌ Error processing event {event.get('event_id', 'unknown')}: {e}")
            
            print(f"📋 Returning {len(result)} valid events")
            return result
            
        except Exception as e:
            print(f"❌ Error in list_events for guild {guild_id}: {type(e).__name__}: {e}")
            import traceback
            traceback.print_exc()
            return []
        
    async def set_user_timezone(self, user_id: int, timezone: str) -> bool:
        """set a user's timezone."""
        try:
            # first check if the timezone is valid
            try:
                # check common timezone abbreviations first
                common_timezones = {
                    'EST': 'America/New_York',
                    'EDT': 'America/New_York',
                    'CST': 'America/Chicago',
                    'CDT': 'America/Chicago',
                    'MST': 'America/Denver',
                    'MDT': 'America/Denver',
                    'PST': 'America/Los_Angeles',
                    'PDT': 'America/Los_Angeles',
                    'AEST': 'Australia/Sydney',
                    'AEDT': 'Australia/Sydney',
                    'AWST': 'Australia/Perth',
                    'GMT': 'Europe/London',
                    'BST': 'Europe/London',
                    'CET': 'Europe/Paris',
                    'CEST': 'Europe/Paris',
                    'IST': 'Asia/Kolkata',
                    'JST': 'Asia/Tokyo',
                    'HKT': 'Asia/Hong_Kong',
                    'SGT': 'Asia/Singapore'
                }
                
                # if it's a common abbreviation, use the full timezone name
                if timezone.upper() in common_timezones:
                    timezone = common_timezones[timezone.upper()]
                
                # verify the timezone is valid
                pytz.timezone(timezone)
                
            except pytz.UnknownTimeZoneError:
                print(f"Invalid timezone: {timezone}")
                return False
                
            # insert or update user's timezone
            query = """
            INSERT INTO user_timezones (user_id, timezone) 
            VALUES (?, ?)
            ON CONFLICT(user_id) DO UPDATE SET timezone = excluded.timezone
            """
            db.execute_query(query, (user_id, timezone), commit=True)
            print(f"Set timezone for user {user_id} to {timezone}")
            return True
            
        except Exception as e:
            print(f"Error setting user timezone: {e}")
            import traceback
            traceback.print_exc()
            return False
