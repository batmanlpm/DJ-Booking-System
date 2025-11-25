"""
event scheduling commands for the bot.
"""
import discord
from discord import app_commands
from discord.ext import commands
from datetime import datetime, timedelta
from typing import Optional, List
import pytz
from ..database.database import db
from .event_instance import event_manager

def format_timedelta(delta: timedelta) -> str:
    """format a timedelta into a human-readable string."""
    total_seconds = int(delta.total_seconds())
    days, remainder = divmod(abs(total_seconds), 86400)
    hours, remainder = divmod(remainder, 3600)
    minutes, seconds = divmod(remainder, 60)
    
    parts = []
    if days > 0:
        parts.append(f"{days} day{'s' if days > 1 else ''}")
    if hours > 0:
        parts.append(f"{hours} hour{'s' if hours > 1 else ''}")
    if minutes > 0 or not parts:  # always show at least minutes
        parts.append(f"{minutes} minute{'s' if minutes != 1 else ''}")
    
    if total_seconds < 0:
        return f"{', '.join(parts)} ago"
    return f"in {', '.join(parts)}"

# define a custom view for event selection
class EventSelectView(discord.ui.View):
    def __init__(self, events: List[dict], timeout: float = 180.0):
        super().__init__(timeout=timeout)
        self.events = events
        self.selected_event_id = None
        
        # create a select menu with events
        select = discord.ui.Select(
            placeholder="Select an event to delete",
            options=[
                discord.SelectOption(
                    label=event['name'][:100],
                    description=f"{event['time'].split(' ')[0]} {event['timezone']}",
                    value=str(idx)
                )
                for idx, event in enumerate(events)
            ]
        )
        select.callback = self.on_select
        self.add_item(select)
    
    async def on_select(self, interaction: discord.Interaction):
        """handle event selection."""
        self.selected_event_id = int(interaction.data['values'][0])
        self.stop()
        await interaction.response.defer()

# timezone selection view
class TimezoneSelectView(discord.ui.Select):
    def __init__(self):
        timezones = [
            ('Eastern Time (US/Canada)', 'America/New_York'),
            ('Central Time (US/Canada)', 'America/Chicago'),
            ('Mountain Time (US/Canada)', 'America/Denver'),
            ('Pacific Time (US/Canada)', 'America/Los_Angeles'),
            ('London (UK)', 'Europe/London'),
            ('Paris (France)', 'Europe/Paris'),
            ('Berlin (Germany)', 'Europe/Berlin'),
            ('Tokyo (Japan)', 'Asia/Tokyo'),
            ('Sydney (Australia)', 'Australia/Sydney'),
            ('Other (Specify)', 'other')
        ]
        
        options = [
            discord.SelectOption(label=label, value=tz)
            for label, tz in timezones
        ]
        
        super().__init__(
            placeholder="Select your timezone...",
            min_values=1,
            max_values=1,
            options=options
        )
    
    async def callback(self, interaction: discord.Interaction):
        if self.values[0] == 'other':
            await interaction.response.send_modal(TimezoneModal())
        else:
            await interaction.response.defer()
            await interaction.followup.send(f"Timezone set to: {self.values[0]}", ephemeral=True)
            await interaction.followup.send(
                f"Your timezone has been set to **{self.values[0]}**. "
                "You'll now receive event reminders in your local time."
            )

# timezone modal for custom timezone input
class TimezoneModal(discord.ui.Modal, title="Set Your Timezone"):
    timezone = discord.ui.TextInput(
        label="Enter your timezone (e.g., America/New_York, Europe/London)",
        placeholder="Continent/City",
        max_length=50,
    )

    async def on_submit(self, interaction: discord.Interaction):
        timezone = self.timezone.value
        try:
            # validate timezone
            pytz.timezone(timezone)
            await interaction.response.send_message(
                f"Your timezone has been set to **{timezone}**. "
                "You'll now receive event reminders in your local time.",
                ephemeral=True
            )
            # save timezone
            await interaction.client.get_cog('Events').event_manager.set_user_timezone(interaction.user.id, timezone)
        except pytz.UnknownTimeZoneError:
            await interaction.response.send_message(
                "❌ Invalid timezone. Please use the format 'Continent/City' (e.g., America/New_York, Europe/London).\n"
                "You can find your timezone at https://en.wikipedia.org/wiki/List_of_tz_database_time_zones",
                ephemeral=True
            )

# define a custom view for confirmation
class ConfirmView(discord.ui.View):
    def __init__(self, timeout: float = 60.0):
        super().__init__(timeout=timeout)
        self.value = None
    
    @discord.ui.button(label="Confirm", style=discord.ButtonStyle.danger)
    async def confirm(self, interaction: discord.Interaction, button: discord.ui.Button):
        self.value = True
        self.stop()
        await interaction.response.defer()
    
    @discord.ui.button(label="Cancel", style=discord.ButtonStyle.secondary)
    async def cancel(self, interaction: discord.Interaction, button: discord.ui.Button):
        self.value = False
        self.stop()
        await interaction.response.defer()

async def setup_event_channel(interaction: discord.Interaction, channel: discord.TextChannel):
    """set up the event notification channel for this server."""
    # check if user has admin permissions
    if not (interaction.user.guild_permissions.administrator or interaction.user == interaction.guild.owner):
        await interaction.response.send_message(
            "❌ You need administrator permissions to set up the event channel.",
            ephemeral=True
        )
        return
    
    try:
        # update the guild's event channel in the database
        query = """
        INSERT INTO guilds (guild_id, event_channel_id)
        VALUES (?, ?)
        ON CONFLICT(guild_id) DO UPDATE 
        SET event_channel_id = excluded.event_channel_id
        """
        db.execute_query(
            query,
            (interaction.guild_id, channel.id),
            commit=True
        )
        
        await interaction.response.send_message(
            f"✅ Event notifications will be sent to {channel.mention}",
            ephemeral=True
        )
        
    except Exception as e:
        print(f"Error setting up event channel: {e}")
        await interaction.response.send_message(
            "❌ An error occurred while setting up the event channel.",
            ephemeral=True
        )

async def setup(bot):
    """set up the event commands."""
    # check if user has admin role
    def is_admin(interaction: discord.Interaction) -> bool:
        try:
            # first ensure the guild is registered in the database
            register_guild_query = """
            INSERT INTO guilds (guild_id, owner_id)
            VALUES (?, ?)
            ON CONFLICT(guild_id) DO UPDATE 
            SET owner_id = COALESCE(owner_id, excluded.owner_id)
            """
            db.execute_query(
                register_guild_query,
                (interaction.guild_id, interaction.guild.owner_id),
                commit=True
            )
            
            # get admin role ID from database
            query = """
            SELECT admin_role_id 
            FROM guilds 
            WHERE guild_id = ?
            """
            result = db.execute_query(query, (interaction.guild_id,), fetch=True)
            admin_role_id = result[0]['admin_role_id'] if result and result[0]['admin_role_id'] else None
            
            # check if user has admin role or is server owner
            return (
                interaction.user.guild_permissions.administrator or
                interaction.user.id == interaction.guild.owner_id or
                (admin_role_id and any(role.id == admin_role_id for role in interaction.user.roles))
            )
        except Exception as e:
            print(f"Error in is_admin check: {e}")
            # fallback to basic permission check if there's a database error
            return interaction.user.guild_permissions.administrator or interaction.user.id == interaction.guild.owner_id
    
    @bot.tree.command(name="setup", description="Set up bot features for this server.")
    @app_commands.describe(
        feature="The feature to set up",
        channel="The channel to use for this feature"
    )
    @app_commands.choices(feature=[app_commands.Choice(name="event-channel", value="event_channel")])
    async def setup_command(interaction: discord.Interaction, feature: str, channel: discord.TextChannel):
        """set up bot features for this server."""
        if feature == "event_channel":
            await setup_event_channel(interaction, channel)
    
    @bot.tree.command(name="event_schedule", description="schedule a new event.")
    @app_commands.check(is_admin)
    @app_commands.describe(
        name="Name of the event",
        time="Event time (DD-MM-YY HH:MM, e.g., 25-12-25 14:30 for Dec 25, 2025 2:30 PM)",
        timezone="Timezone (e.g., 'America/New_York', 'UTC', 'CET')",
        description="Optional description of the event"
    )
    @app_commands.check(is_admin)
    @app_commands.checks.cooldown(1, 5)  # 5 second cooldown per user
    async def event_schedule(
        interaction: discord.Interaction,
        name: str,
        time: str,
        timezone: str,
        description: Optional[str] = None
    ):
        """schedule a new event."""
        # defer the response to avoid timeout
        await interaction.response.defer(ephemeral=True)
        
        # parse the event time
        try:
            # try to parse the time string in DD-MM-YY HH:MM format
            event_time = datetime.strptime(time, '%d-%m-%y %H:%M')
            
            # set the timezone
            try:
                # handle common timezone abbreviations
                tz_abbreviations = {
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
                
                tz_name = tz_abbreviations.get(timezone.upper(), timezone)
                tz = pytz.timezone(tz_name)
                
                # localize the datetime
                event_time = tz.localize(event_time)
                
                # convert to utc for storage
                utc_time = event_time.astimezone(pytz.UTC)
                
                # check if the time is in the future
                if utc_time < datetime.now(pytz.UTC):
                    await interaction.followup.send(
                        "❌ The event time must be in the future.",
                        ephemeral=True
                    )
                    return
                
                # get the event channel ID
                query = """
                SELECT event_channel_id 
                FROM guilds 
                WHERE guild_id = ?
                """
                result = db.execute_query(query, (interaction.guild_id,), fetch=True)
                event_channel_id = result[0]['event_channel_id'] if result and result[0]['event_channel_id'] else None
                
                # create the event
                event_data = {
                    'guild_id': interaction.guild_id,
                    'name': name,
                    'time': utc_time.isoformat(),
                    'timezone': timezone,
                    'description': description,
                    'event_channel_id': event_channel_id
                }
                
                # insert the event into the database
                query = """
                INSERT INTO events (guild_id, name, time, timezone, description)
                VALUES (?, ?, ?, ?, ?)
                """
                event_id = db.execute_query(
                    query,
                    (interaction.guild_id, name, utc_time.isoformat(), timezone, description),
                    commit=True
                )
                
                # add to active events
                if event_id:
                    event_data['event_id'] = event_id
                    event_manager.active_events[event_id] = event_data
                
                if not event_id:
                    await interaction.followup.send(
                        "❌ Failed to create the event. Please try again.",
                        ephemeral=True
                    )
                    return
                
                # create embed for the event with the new design
                embed = discord.Embed(
                    title=f"New Event Upcoming: **{name}**",
                    color=3092790  # exact color as requested
                )
                
                # add event time in specified timezone
                embed.add_field(
                    name="Event Time",
                    value=f"{utc_time.strftime('%Y-%m-%d %H:%M %Z')}",
                    inline=True
                )
                
                # add event description
                embed.add_field(
                    name="Event Description",
                    value=description or "No description provided.",
                    inline=True
                )
                
                # set author with bot's info and avatar
                bot_user = interaction.client.user
                embed.set_author(
                    name=bot_user.display_name,
                    icon_url=str(bot_user.avatar.url) if bot_user.avatar else None
                )
                
                # set footer
                embed.set_footer(
                    text="Set your timezone with /timezone to get event notifications in your local time."
                )
                
                # send the event to the event channel if set
                if event_channel_id:
                    channel = interaction.guild.get_channel(event_channel_id)
                    if channel and channel.permissions_for(interaction.guild.me).send_messages:
                        # add a note that members will be notified 1 hour before
                        embed.add_field(
                            name="Notifications",
                            value="🔔 All server members will receive a DM 1 hour before the event starts.",
                            inline=False
                        )
                        await channel.send(embed=embed)
                
                await interaction.followup.send(
                    f"✅ Event '{name}' scheduled successfully!",
                    ephemeral=True
                )
                
            except pytz.UnknownTimeZoneError:
                await interaction.followup.send(
                    "❌ Invalid timezone. Please use a valid timezone like 'UTC', 'America/New_York', or 'Europe/London'.",
                    ephemeral=True
                )
                
        except ValueError:
            await interaction.followup.send(
                "❌ Invalid time format. Please use DD-MM-YY HH:MM (e.g., 25-12-25 14:30 for Dec 25, 2025 2:30 PM)",
                ephemeral=True
            )
    
    @event_schedule.error
    async def event_schedule_error(interaction: discord.Interaction, error):
        """Handle errors for the event_schedule command."""
        if isinstance(error, app_commands.CheckFailure):
            await interaction.response.send_message(
                "❌ You don't have permission to schedule events.",
                ephemeral=True
            )
        elif isinstance(error, app_commands.CommandOnCooldown):
            await interaction.response.send_message(
                f"❌ Command is on cooldown. Please try again in {error.retry_after:.1f} seconds.",
                ephemeral=True
            )
        else:
            await interaction.response.send_message(
                f"❌ An error occurred: {str(error)}",
                ephemeral=True
            )
    
    @bot.tree.command(name="event_list", description="List all scheduled events.")
    @app_commands.checks.cooldown(1, 5)  # 5 second cooldown per user
    async def event_list(interaction: discord.Interaction):
        """list all scheduled events."""
        try:
            # defer the response to avoid timeout
            await interaction.response.defer(ephemeral=True)
            
            print(f"\n📋 Received event_list command from guild {interaction.guild_id}")
            
            # get all events for this guild
            events = await event_manager.list_events(interaction.guild_id)
            
            if not events:
                print(f"ℹ️ No events found for guild {interaction.guild_id}")
                await interaction.followup.send(
                    "No scheduled events found. Use `/event_schedule` to create a new event.", 
                    ephemeral=True
                )
                return
                
            print(f"📅 Found {len(events)} events")
            
            # create an embed to display the events
            embed = discord.Embed(
                title="📅 Upcoming Events",
                color=3092790  # using the same blue as other embeds
            )
            
            current_time = datetime.now(pytz.utc)
            
            for event in events:
                try:
                    # parse the event time
                    event_time = datetime.fromisoformat(event['time']).replace(tzinfo=pytz.utc)
                    time_until = event_time - current_time
                    
                    # format the time for display
                    time_str = f"<t:{int(event_time.timestamp())}:F>"
                    
                    # add a field for each event
                    embed.add_field(
                        name=f"🔹 {event['name']}",
                        value=(
                            f"**Time:** {time_str} ({format_timedelta(time_until)} from now)\n"
                            f"**Description:** {event.get('description', 'No description')}\n"
                            f"**ID:** `{event['event_id']}`"
                        ),
                        inline=False
                    )
                except Exception as e:
                    print(f"❌ Error formatting event {event.get('event_id', 'unknown')}: {e}")
            
            # add bot's avatar as author
            bot_user = interaction.client.user
            if bot_user.avatar:
                embed.set_author(
                    name=bot_user.display_name,
                    icon_url=str(bot_user.avatar.url)
                )
            
            # add footer with timezone info
            embed.set_footer(
                text=f"Showing {len(events)} events • Use /event_delete [id] to remove an event"
            )
            
            # add a timestamp
            embed.timestamp = current_time
            
            await interaction.followup.send(embed=embed, ephemeral=True)
            print("✅ Sent event list to user")
            
        except Exception as e:
            error_msg = f"❌ Error listing events: {type(e).__name__}: {e}"
            print(error_msg)
            import traceback
            traceback.print_exc()
            
            try:
                await interaction.followup.send(
                    "An error occurred while fetching events. Please try again later.",
                    ephemeral=True
                )
            except:
                try:
                    await interaction.followup.send(error_msg[:2000], ephemeral=True)
                except:
                    pass
    
    @bot.tree.command(name="event_delete", description="Delete a scheduled event.")
    @app_commands.checks.cooldown(1, 5)  # 5 second cooldown per user
    async def event_delete(interaction: discord.Interaction, event_id: int = None):
        """delete a scheduled event."""
        # defer the response to avoid timeout
        await interaction.response.defer(ephemeral=True)
        
        try:
            # if event_id is provided, try to delete it directly
            if event_id is not None:
                success = await event_manager.delete_event(event_id)
                if success:
                    await interaction.followup.send(
                        f"✅ Event with ID `{event_id}` has been deleted.",
                        ephemeral=True
                    )
                else:
                    await interaction.followup.send(
                        f"❌ Could not find an event with ID `{event_id}`.",
                        ephemeral=True
                    )
                return
                
            # if no event_id provided, show selection menu
            events = await event_manager.list_events(interaction.guild_id)
            
            if not events:
                await interaction.followup.send(
                    "No scheduled events found to delete.",
                    ephemeral=True
                )
                return
            
            # if there's only one event, delete it directly
            if len(events) == 1:
                selected_event = events[0]
                success = await event_manager.delete_event(selected_event['event_id'])
                if success:
                    await interaction.followup.send(
                        f"✅ Event '{selected_event['name']}' has been deleted.",
                        ephemeral=True
                    )
                else:
                    await interaction.followup.send(
                        f"❌ Failed to delete event '{selected_event['name']}'.",
                        ephemeral=True
                    )
                return
            
            # if multiple events, show selection menu
            view = EventSelectView(events)
            
            # send the initial message with the select menu
            await interaction.followup.send(
                "Select an event to delete:",
                view=view,
                ephemeral=True
            )
            
            # wait for the user to select an event
            await view.wait()
            
            if view.selected_event_id is None:
                await interaction.followup.send("Event selection timed out or was cancelled.", ephemeral=True)
                return
            
            # get the selected event
            selected_event = events[view.selected_event_id]
            
            # ask for confirmation
            confirm_view = ConfirmView()
            confirm_message = await interaction.followup.send(
                f"Are you sure you want to delete the event **{selected_event['name']}**? "
                f"This action cannot be undone.",
                view=confirm_view,
                ephemeral=True
            )
            
            # wait for confirmation
            await confirm_view.wait()
            
            if confirm_view.value:
                # delete the event
                success = await event_manager.delete_event(selected_event['event_id'])
                
                if success:
                    await confirm_message.edit(
                        content=f"✅ Event '{selected_event['name']}' has been deleted.",
                        view=None
                    )
                else:
                    await confirm_message.edit(
                        content=f"❌ Failed to delete event '{selected_event['name']}'.",
                        view=None
                    )
            else:
                await confirm_message.edit(
                    content="Event deletion cancelled.",
                    view=None
                )
                
        except Exception as e:
            error_msg = f"❌ Error deleting event: {type(e).__name__}: {e}"
            print(error_msg)
            import traceback
            traceback.print_exc()
            
            try:
                await interaction.followup.send(
                    "An error occurred while deleting the event. Please try again later.",
                    ephemeral=True
                )
            except:
                pass
            else:
                await interaction.followup.send(
                    "❌ Failed to delete the event. Please try again.",
                    ephemeral=True
                )
        else:
            await interaction.followup.send("Event deletion cancelled.", ephemeral=True)
    
    @event_delete.error
    async def event_delete_error(self, interaction: discord.Interaction, error):
        """Handle errors for the event_delete command."""
        if isinstance(error, app_commands.CheckFailure):
            await interaction.response.send_message(
                "❌ You don't have permission to delete events.",
                ephemeral=True
            )
        else:
            await interaction.response.send_message(
                f"❌ An error occurred: {str(error)}",
                ephemeral=True
            )
    
    # timezone command is now handled in timezone_commands.py
    
    # define the event_setchannel command as a class method
    @app_commands.command(name="event_setchannel", description="Set the channel where event notifications will be sent.")
    @app_commands.describe(channel="The channel to send event notifications to")
    @app_commands.check(is_admin)
    async def event_setchannel(interaction: discord.Interaction, channel: discord.TextChannel):
        """set the channel for event notifications."""
        # defer the response to avoid timeout
        await interaction.response.defer(ephemeral=True)
        
        # check if the bot has permission to send messages in the channel
        if not channel.permissions_for(interaction.guild.me).send_messages:
            await interaction.followup.send(
                f"❌ I don't have permission to send messages in {channel.mention}.",
                ephemeral=True
            )
            return
        
        # first, ensure the guild exists with the correct owner_id
        register_guild_query = """
        INSERT INTO guilds (guild_id, owner_id, event_channel_id)
        VALUES (?, ?, ?)
        ON CONFLICT(guild_id) DO UPDATE 
        SET event_channel_id = excluded.event_channel_id,
            owner_id = COALESCE(owner_id, excluded.owner_id)
        """
        
        try:
            # get the guild owner ID
            owner_id = interaction.guild.owner_id
            
            # update the guild record with both owner_id and event_channel_id
            db.execute_query(
                register_guild_query, 
                (interaction.guild_id, owner_id, channel.id), 
                commit=True
            )
            
            await interaction.followup.send(
                f"✅ Event notifications will now be sent to {channel.mention}",
                ephemeral=True
            )
        except Exception as e:
            print(f"Error setting event channel: {e}")
            await interaction.followup.send(
                f"❌ Failed to set the event channel: {str(e)}",
                ephemeral=True
            )
    
    # add the command to the bot's command tree
    bot.tree.add_command(event_setchannel)
    
    @event_setchannel.error
    async def event_setchannel_error(interaction: discord.Interaction, error):
        """handle errors for the event_setchannel command."""
        if isinstance(error, app_commands.CheckFailure):
            await interaction.response.send_message(
                "❌ You don't have permission to set the event channel.",
                ephemeral=True
            )
        else:
            await interaction.response.send_message(
                f"❌ An error occurred: {str(error)}",
                ephemeral=True
            )
    
    return True
