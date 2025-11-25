"""
admin action tracking and quarantine system.
tracks admin actions and applies quarantine if suspicious activity is detected.
"""
import discord
from discord.ext import commands
import os
from datetime import datetime, timedelta
import json
import logging
from typing import List, Dict, Optional, Tuple, Set
import asyncio

from modules.database import db
from modules.security.settings import AdminSecuritySettings

logger = logging.getLogger('discord.security.admin_tracker')

class AdminActionTracker:
    """tracks admin actions and manages quarantine status."""
    
    def __init__(self, bot):
        self.bot = bot
        # (guild_id, user_id, action_type) -> [timestamps]
        self.action_windows: Dict[Tuple[int, int, str], List[datetime]] = {}
        self.quarantine_roles: Dict[int, List[int]] = {}  # guild_id -> [role_ids]
        self.logger = logging.getLogger('discord.security.admin_tracker')
        # track quarantined users to prevent repeated quarantine attempts
        self.quarantined_users: Set[Tuple[int, int]] = set()  # (guild_id, user_id)
        # track temporary admin assignments: (guild_id, user_id) -> expiry_time
        self.temporary_admins: Dict[Tuple[int, int], datetime] = {}
        # start background tasks
        self.bot.loop.create_task(self._check_temporary_admins())
        self.bot.loop.create_task(self._check_expired_quarantines())
        
    async def _ensure_guild_exists(self, guild_id: int):
        """ensure the guild exists in the guilds table."""
        try:
            # check if guild exists
            guild = self.bot.get_guild(guild_id)
            if not guild:
                self.logger.warning(f"Guild {guild_id} not found in bot's cache")
                return False
                
            # check if guild exists in database
            existing_guild = db.execute_query(
                "SELECT 1 FROM guilds WHERE guild_id = ?",
                (guild_id,),
                fetch=True
            )
            
            if not existing_guild:
                # insert guild if it doesn't exist
                db.execute_query(
                    """INSERT OR IGNORE INTO guilds 
                       (guild_id, owner_id, admin_role_id, event_channel_id)
                       VALUES (?, ?, NULL, NULL)""",
                    (guild_id, guild.owner_id),
                    commit=True
                )
                self.logger.info(f"Added guild {guild_id} to database")
                
            return True
            
        except Exception as e:
            self.logger.error(f"Error ensuring guild exists: {str(e)}")
            return False
            
    async def _ensure_user_exists(self, user_id: int):
        """ensure the user exists in the users table."""
        try:
            # check if user exists
            existing_user = db.execute_query(
                "SELECT 1 FROM users WHERE user_id = ?",
                (user_id,),
                fetch=True
            )
            
            if not existing_user:
                # insert user if they don't exist
                db.execute_query(
                    """INSERT OR IGNORE INTO users 
                       (user_id, timezone)
                       VALUES (?, 'UTC')""",
                    (user_id,),
                    commit=True
                )
                self.logger.info(f"Added user {user_id} to database")
                
            return True
            
        except Exception as e:
            self.logger.error(f"Error ensuring user exists: {str(e)}")
            return False
    
    async def _log_action(self, guild_id: int, user_id: int, action_type: str, target_id: int = None, **kwargs) -> None:
        """log an admin action to the database."""
        guild = self.bot.get_guild(guild_id)
        user = await self.bot.fetch_user(user_id)
        target = f" (target: {target_id})" if target_id else ""
        
        log_msg = f"[Admin Action] {user} (ID: {user_id}) performed {action_type}{target} in {guild.name if guild else 'Unknown Guild'}"
        self.logger.info(log_msg)
        
        # log to console for debugging
        print(f"\n{log_msg}")
        
        try:
            # ensure guild and user exist in their respective tables
            if not await self._ensure_guild_exists(guild_id):
                self.logger.error(f"Failed to ensure guild {guild_id} exists in database")
                return
                
            if not await self._ensure_user_exists(user_id):
                self.logger.error(f"Failed to ensure user {user_id} exists in database")
                return
            
            # log to database with error handling for foreign key constraints
            db.execute_query(
                """
                INSERT INTO admin_action_logs 
                (guild_id, user_id, action, target_id, timestamp, details)
                VALUES (?, ?, ?, ?, datetime('now'), ?)
                """,
                (guild_id, user_id, action_type, target_id, 
                 json.dumps(kwargs.get('details', {})) if kwargs.get('details') else None),
                commit=True
            )
        except Exception as e:
            self.logger.error(f"Error logging admin action: {str(e)}", exc_info=True)
            print(f"[ERROR] Failed to log admin action: {str(e)}")
            
            # print database path for debugging
            try:
                db_path = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))), 'data', 'bot_database.db')
                print(f"[DEBUG] Database path: {db_path}")
                print(f"[DEBUG] Database exists: {os.path.exists(db_path)}")
            except Exception as dbg_err:
                print(f"[DEBUG] Could not determine database path: {str(dbg_err)}")
            
    async def _notify_owner(self, guild: discord.Guild, member: discord.Member, reason: str, quarantined_until: str) -> None:
        """notify the guild owner when a user is quarantined."""
        try:
            if not guild.owner:
                self.logger.warning(f"Could not notify owner: Guild owner not found for guild {guild.id}")
                return
                
            # format the timestamp for display
            try:
                dt = datetime.fromisoformat(quarantined_until.replace('Z', '+00:00'))
                timestamp = f"<t:{int(dt.timestamp())}:F>"
            except (ValueError, AttributeError):
                timestamp = "Unknown time"
                
            # get the quarantine record to get the removed roles
            record = db.execute_query(
                """
                SELECT quarantined_roles, reason 
                FROM admin_quarantine 
                WHERE guild_id = ? AND user_id = ? AND is_active = 1
                ORDER BY quarantined_at DESC
                LIMIT 1
                """,
                (guild.id, member.id),
                fetch=True
            )
            
            # parse the removed roles
            removed_roles = []
            if record and record[0][0]:  # check if record exists and has quarantined_roles
                try:
                    role_ids = json.loads(record[0][0])
                    removed_roles = [guild.get_role(rid) for rid in role_ids if guild.get_role(rid)]
                    removed_roles = [r for r in removed_roles if r is not None]  # Filter out None values
                except (json.JSONDecodeError, TypeError):
                    self.logger.warning("Failed to parse quarantined_roles from database")
            
            # create the embed
            embed = discord.Embed(
                title="🚨 User Quarantined",
                description=f"**{member}** (`{member.id}`) has been automatically quarantined.",
                color=discord.Color.red()
            )
            
            # add fields for reason and duration
            embed.add_field(name="Reason", value=reason or "No reason provided", inline=False)
            embed.add_field(name="Quarantine Until", value=timestamp, inline=False)
            
            # add removed roles if any
            if removed_roles:
                roles_text = ", ".join([r.mention for r in removed_roles])
                embed.add_field(name="Removed Roles", value=roles_text if roles_text else "No roles removed", inline=False)
            else:
                embed.add_field(name="Removed Roles", value="No roles were removed", inline=False)
                
            embed.set_footer(text="This is an automated notification")
            
            # try to send DM to the guild owner
            try:
                await guild.owner.send(embed=embed)
                self.logger.info(f"Sent quarantine notification to guild owner {guild.owner} for user {member}")
            except discord.Forbidden:
                self.logger.warning(f"Could not DM guild owner {guild.owner} (DMs disabled)")
                # try to send to system channel as fallback
                if guild.system_channel and guild.me.guild_permissions.send_messages:
                    try:
                        await guild.system_channel.send(
                            content=f"{guild.owner.mention}, I tried to DM you about a security action but couldn't. Please enable DMs from server members.",
                            embed=embed
                        )
                    except Exception as e:
                        self.logger.error(f"Failed to send quarantine notification to system channel: {str(e)}")
            
        except Exception as e:
            self.logger.error(f"Error in _notify_owner: {str(e)}", exc_info=True)
        
    async def _check_expired_quarantines(self):
        """background task to check for and automatically unquarantine users when their quarantine expires."""
        while not self.bot.is_closed():
            try:
                now = datetime.utcnow()
                
                # find users with expired quarantines
                expired_quarantines = db.execute_query(
                    """
                    SELECT guild_id, user_id 
                    FROM admin_quarantine 
                    WHERE is_active = 1 
                    AND quarantined_until < ?
                    """,
                    (now.isoformat(),),
                    fetch=True
                )
                
                for record in expired_quarantines:
                    guild_id, user_id = record[0], record[1]
                    try:
                        # unquarantine the user
                        success = await self.unquarantine_user(guild_id, user_id)
                        if success:
                            self.logger.info(f"Auto-unquarantined user {user_id} in guild {guild_id} (quarantine expired)")
                        else:
                            self.logger.warning(f"Failed to auto-unquarantine user {user_id} in guild {guild_id}")
                    except Exception as e:
                        self.logger.error(f"Error auto-unquarantining user {user_id} in guild {guild_id}: {e}")
                
            except Exception as e:
                self.logger.error(f"Error in _check_expired_quarantines: {e}")
            
            # check every 5 minutes
            await asyncio.sleep(300)

    async def _check_temporary_admins(self):
        """background task to check for expired temporary admin assignments."""
        await self.bot.wait_until_ready()
        while not self.bot.is_closed():
            try:
                now = datetime.utcnow()
                to_remove = []
                
                # check for expired temporary admins
                for (guild_id, user_id), expiry in list(self.temporary_admins.items()):
                    if now >= expiry:
                        guild = self.bot.get_guild(guild_id)
                        if guild:
                            member = guild.get_member(user_id)
                            if member:
                                # check for suspicious activity after temp admin expired
                                suspicious = await self._check_suspicious_activity(guild_id, user_id)
                                if suspicious:
                                    await self.quarantine_user(
                                        guild_id, 
                                        user_id, 
                                        "Suspicious activity after temporary admin period ended",
                                        duration_hours=24
                                    )
                        to_remove.append((guild_id, user_id))
                
                # clean up expired entries
                for key in to_remove:
                    self.temporary_admins.pop(key, None)
                
            except Exception as e:
                self.logger.error(f"Error in _check_temporary_admins: {e}")
            
            # check every 5 minutes
            await asyncio.sleep(300)

    async def _check_suspicious_activity(self, guild_id: int, user_id: int) -> bool:
        """check if a user has performed suspicious actions recently."""
        now = datetime.utcnow()
        one_hour_ago = now - timedelta(hours=1)
        
        # get all actions by this user in the last hour
        actions = db.execute_query(
            """
            SELECT action, COUNT(*) as count 
            FROM admin_action_logs 
            WHERE guild_id = ? AND user_id = ? AND timestamp > ?
            GROUP BY action
            """,
            (guild_id, user_id, one_hour_ago.isoformat()),
            fetch=True
        )
        
        # define suspicious patterns (action: threshold)
        suspicious_patterns = {
            'ban': 3,        # 3+ bans in an hour
            'kick': 5,       # 5+ kicks in an hour
            'channel_delete': 2,  # 2+ channel deletions in an hour
            'role_update': 5,     # 5+ role updates in an hour
        }
        
        for action in actions:
            action_name = action['action']
            count = action['count']
            if action_name in suspicious_patterns and count >= suspicious_patterns[action_name]:
                return True
                
        return False

    async def record_action(self, guild_id: int, user_id: int, action_type: str, target_id: int = None) -> bool:
        """
        record an admin action and check if it triggers quarantine.
        returns true if action should be allowed, false if blocked.
        """
        # skip if actions security is disabled for this guild
        if not AdminSecuritySettings.is_actions_security_enabled(guild_id):
            print(f"[SECURITY] Actions security is disabled for guild {guild_id}, allowing action")
            return True
            
        # check if user is a temporary admin
        if (guild_id, user_id) in self.temporary_admins:
            if datetime.utcnow() < self.temporary_admins[(guild_id, user_id)]:
                # still within temporary admin period, allow the action
                return True
            else:
                # temporary admin period has expired, remove from temp admins
                self.temporary_admins.pop((guild_id, user_id), None)
        
        # check if user is already quarantined
        if (guild_id, user_id) in self.quarantined_users:
            print(f"[SECURITY] User {user_id} is already quarantined, blocking action")
            return False
            
        print(f"\n[SECURITY] Processing {action_type} by user {user_id} in guild {guild_id}")
        
        # skip if user is the bot owner or the bot itself
        if user_id in (self.bot.owner_id, self.bot.user.id):
            print("[SECURITY] Skipping action check for bot owner or self")
            return True
            
        now = datetime.utcnow()
        
        # check if user is a temporary admin
        is_temp_admin = (guild_id, user_id) in self.temporary_admins
        
        # if user is a temporary admin and the action is within their admin period, allow it
        if is_temp_admin and datetime.utcnow() < self.temporary_admins[(guild_id, user_id)]:
            return True
            
        try:
            # 1. Track this action in memory
            action_key = (guild_id, user_id, action_type)
            
            # initialize the action window if it doesn't exist
            if action_key not in self.action_windows:
                self.action_windows[action_key] = []
                
            # add the current action timestamp
            self.action_windows[action_key].append(now)
            
            # 2. Define action-specific time windows (in minutes)
            action_windows = {
                'channel_delete': 1,  # 1 minute window for channel deletions
                'ban': 1,             # 1 minute window for bans
                'kick': 1,            # 1 minute window for kicks
                'default': 1          # default 1 minute window for other actions
            }
            
            # 3. Clean up old actions based on action-specific time window
            window_minutes = action_windows.get(action_type, action_windows['default'])
            time_window = now - timedelta(minutes=window_minutes)
            self.action_windows[action_key] = [
                t for t in self.action_windows[action_key] 
                if t > time_window
            ]
            
            # 4. Count recent actions of the same type by this user
            action_count = len(self.action_windows[action_key])
            print(f"[SECURITY] Found {action_count} {action_type} actions in the last {window_minutes} minute(s)")
            
            # 4. Log the action to the database
            await self._log_action(guild_id, user_id, action_type, target_id)
            
            # 5. Define action-specific thresholds and time windows
            action_thresholds = {
                'channel_delete': {'count': 2, 'minutes': 1},
                'ban': {'count': 2, 'minutes': 1},
                'kick': {'count': 2, 'minutes': 1}
            }
            
            # 6. Check if this action type has a threshold defined
            threshold = action_thresholds.get(action_type)
            if threshold and action_count >= threshold['count']:
                print(f"[SECURITY] ALERT: User {user_id} performed {action_count} {action_type} actions in {threshold['minutes']} minute(s)!")
                quarantine_reason = f"Multiple {action_type} actions within {threshold['minutes']} minute(s)"
                
                # try to quarantine the user
                try:
                    print("[SECURITY] Attempting to quarantine user...")
                    quarantine_success = await self.quarantine_user(guild_id, user_id, quarantine_reason)
                    
                    if quarantine_success:
                        print(f"[SECURITY] Successfully quarantined user {user_id}")
                        self.quarantined_users.add((guild_id, user_id))
                        return False  # block the action
                    else:
                        print(f"[SECURITY] Failed to quarantine user {user_id}")
                        return True  # allow the action if quarantine fails
                        
                except Exception as e:
                    print(f"[SECURITY ERROR] Exception during quarantine: {str(e)}")
                    import traceback
                    traceback.print_exc()
                    return True  # allow the action if there's an error
            
            return True  # allow the action if threshold not reached
            
        except Exception as e:
            print(f"[SECURITY ERROR] Error in record_action: {str(e)}")
            return True  # always allow the action if there's an error
        
    async def is_quarantined(self, guild_id: int, user_id: int) -> bool:
        """check if a user is currently quarantined."""
        result = db.execute_query(
            """SELECT 1 FROM admin_quarantine 
               WHERE guild_id = ? AND user_id = ? 
               AND (quarantined_until IS NULL OR quarantined_until > datetime('now'))""",
            (guild_id, user_id),
            fetch=True
        )
        return bool(result)
        
    async def grant_temporary_admin(self, guild_id: int, user_id: int, duration_hours: int = 24):
        """
        grant temporary admin privileges to a user.
        """
        expiry_time = datetime.utcnow() + timedelta(hours=duration_hours)
        self.temporary_admins[(guild_id, user_id)] = expiry_time
        self.logger.info(f"Granted temporary admin to user {user_id} in guild {guild_id} until {expiry_time}")
        
        # log the temporary admin grant
        await self._log_action(
            guild_id=guild_id,
            user_id=user_id,
            action_type='temporary_admin_granted',
            details=f"Temporary admin granted for {duration_hours} hours until {expiry_time}"
        )
    
    async def _send_quarantine_dm(self, member: discord.Member, reason: str, duration_hours: int):
        """send a DM to a user when they are quarantined."""
        try:
            embed = discord.Embed(
                title="🔒 You've Been Quarantined",
                description=(
                    "You've been placed in quarantine in the server due to suspicious activity.\n\n"
                    f"**Reason:** {reason}\n"
                    f"**Duration:** {duration_hours} hours\n\n"
                    "During this time, your admin privileges have been temporarily revoked. "
                    "The server owner has been notified and will review the situation."
                ),
                color=discord.Color.red()
            )
            embed.set_footer(text="This is an automated message")
            
            await member.send(embed=embed)
            return True
        except discord.Forbidden:
            self.logger.warning(f"Could not send DM to user {member.id}: User has DMs disabled")
            return False
        except Exception as e:
            self.logger.error(f"Error sending quarantine DM to user {member.id}: {e}")
            return False

    async def _send_unquarantine_dm(self, member: discord.Member, duration_hours: int):
        """send a DM to a user when they are unquarantined."""
        try:
            embed = discord.Embed(
                title="✅ Your Admin Access Has Been Restored",
                description=(
                    "You've been unquarantined in the server.\n\n"
                    f"**Temporary Admin Status:** Active for the next {duration_hours} hours\n\n"
                    "You now have a temporary admin status that will allow you to perform admin actions "
                    "without triggering the security system. This status will expire automatically."
                ),
                color=discord.Color.green()
            )
            embed.set_footer(text="This is an automated message")
            
            await member.send(embed=embed)
            return True
        except discord.Forbidden:
            self.logger.warning(f"Could not send unquarantine DM to user {member.id}: User has DMs disabled")
            return False
        except Exception as e:
            self.logger.error(f"Error sending unquarantine DM to user {member.id}: {e}")
            return False
            
    async def _get_admin_roles(self, guild: discord.Guild) -> List[discord.Role]:
        """get all admin roles for a guild."""
        admin_roles = []
        
        self.logger.info(f"Checking roles in guild: {guild.name} (ID: {guild.id})")
        print(f"[ROLE CHECK] Checking roles in guild: {guild.name} (ID: {guild.id})")
        
        for role in guild.roles:
            if role == guild.default_role:
                continue
                
            # check for various admin/manage permissions
            is_admin = role.permissions.administrator
            can_manage_guild = role.permissions.manage_guild
            can_manage_channels = role.permissions.manage_channels
            can_manage_roles = role.permissions.manage_roles
            
            # log detailed role info for debugging
            role_perms = []
            if is_admin: role_perms.append("administrator")
            if can_manage_guild: role_perms.append("manage_guild")
            if can_manage_channels: role_perms.append("manage_channels")
            if can_manage_roles: role_perms.append("manage_roles")
            
            if any([is_admin, can_manage_guild, can_manage_channels, can_manage_roles]):
                admin_roles.append(role)
                self.logger.info(f"- Found admin role: {role.name} (ID: {role.id}) - Permissions: {', '.join(role_perms) or 'None'}")
                print(f"[ROLE CHECK] Found admin role: {role.name} (ID: {role.id}) - Permissions: {', '.join(role_perms) or 'None'}")
            
        self.logger.info(f"Total admin roles found in {guild.name}: {len(admin_roles)}")
        print(f"[ROLE CHECK] Total admin roles found in {guild.name}: {len(admin_roles)}")
        
        if not admin_roles:
            self.logger.warning("No admin roles found in the server! This might affect quarantine functionality.")
            print("[ROLE CHECK WARNING] No admin roles found in the server! This might affect quarantine functionality.")
            
        return admin_roles

    async def quarantine_user(self, guild_id: int, user_id: int, reason: str, duration_hours: int = 24) -> bool:
        """place a user in quarantine, removing their admin roles."""
        try:
            guild = self.bot.get_guild(guild_id)
            if not guild:
                error_msg = f"Guild {guild_id} not found"
                self.logger.error(error_msg)
                print(f"[QUARANTINE ERROR] {error_msg}")
                return False
                
            member = guild.get_member(user_id)
            if not member:
                error_msg = f"Member {user_id} not found in guild {guild_id}"
                self.logger.error(error_msg)
                print(f"[QUARANTINE ERROR] {error_msg}")
                return False
                
            self.logger.info(f"Found member: {member} ( guild {guild.name}")
            print(f"[QUARANTINE] Found member: {member} in guild {guild.name}")
            
            # get the bot's highest role position
            bot_member = guild.get_member(self.bot.user.id)
            if not bot_member:
                error_msg = "Bot member not found in guild"
                self.logger.error(error_msg)
                print(f"[QUARANTINE ERROR] {error_msg}")
                return False
                
            bot_top_role = bot_member.top_role
            if not bot_top_role:
                error_msg = "Bot has no roles in the guild"
                self.logger.error(error_msg)
                print(f"[QUARANTINE ERROR] {error_msg}")
                return False
                
            print(f"[QUARANTINE] Bot's highest role: {bot_top_role.name} (Position: {bot_top_role.position})")
            
            # get current admin roles and filter out those above bot's top role
            admin_roles = await self._get_admin_roles(guild)
            admin_roles = [r for r in admin_roles if r.position < bot_top_role.position]
            
            if not admin_roles:
                error_msg = "No removable admin roles found (bot's role position is too low)"
                self.logger.error(error_msg)
                print(f"[QUARANTINE ERROR] {error_msg}")
                return False
                
            # log member's current roles for debugging
            current_roles = [r for r in member.roles if r != guild.default_role]  # Exclude @everyone
            print(f"[QUARANTINE] {member}'s current roles: {[r.name for r in current_roles]}")
            
            # get member's current admin roles that we can remove
            removable_roles = [r for r in member.roles if r in admin_roles]
            
            if not removable_roles:
                warning_msg = f"User {member} has no removable admin roles (bot's role position is too low)"
                self.logger.warning(warning_msg)
                print(f"[QUARANTINE WARNING] {warning_msg}")
                print(f"[QUARANTINE] Available admin roles: {[r.name for r in admin_roles]}")
                return False
                
            # log which admin roles will be removed
            print(f"[QUARANTINE] Will attempt to remove these roles: {[r.name for r in removable_roles]}")
            
            # store roles in quarantine record
            removed_role_ids = [r.id for r in removable_roles]
            removed_roles_json = json.dumps(removed_role_ids)
            
            # first, try to update existing record if it exists
            updated = db.execute_query(
                """
                UPDATE admin_quarantine 
                SET reason = ?, 
                    quarantined_roles = ?, 
                    quarantined_at = ?,
                    is_active = ?
                WHERE guild_id = ? AND user_id = ?
                """,
                (reason, removed_roles_json, datetime.utcnow().isoformat(), True, guild_id, user_id),
                commit=True
            )
            
            # if no rows were updated, insert a new record
            if updated == 0:
                db.execute_query(
                    """
                    INSERT INTO admin_quarantine 
                    (guild_id, user_id, reason, quarantined_roles, quarantined_at, is_active)
                    VALUES (?, ?, ?, ?, ?, ?)
                    """,
                    (guild_id, user_id, reason, removed_roles_json, datetime.utcnow().isoformat(), True),
                    commit=True
                )
            
            # actually remove the roles
            for role in removable_roles:
                try:
                    await member.remove_roles(role, reason=f"Quarantine: {reason}")
                    print(f"[QUARANTINE] Successfully removed role: {role.name}")
                except discord.Forbidden:
                    print(f"[QUARANTINE WARNING] Missing permissions to remove role: {role.name}")
                except Exception as e:
                    print(f"[QUARANTINE ERROR] Failed to remove role {role.name}: {str(e)}")
            
            # calculate quarantine end time and format as ISO 8601
            quarantine_end = datetime.utcnow() + timedelta(hours=duration_hours)
            quarantined_until = quarantine_end.isoformat()  # store in ISO format
            
            # send DM to the user
            await self._send_quarantine_dm(member, reason, duration_hours)
            
            # notify guild owner
            try:
                await self._notify_owner(guild, member, reason, quarantined_until)
            except Exception as e:
                self.logger.error(f"Error notifying guild owner: {e}")
            
            # add to in-memory set to prevent duplicate quarantines
            self.quarantined_users.add((guild_id, user_id))
            
            # log the action
            await self._log_action(
                guild_id,
                user_id,
                "user_quarantined",
                target_id=user_id,
                details={
                    "reason": reason,
                    "duration_hours": duration_hours,
                    "removed_roles": [r.name for r in removable_roles]
                }
            )
            
            print(f"[QUARANTINE] Successfully quarantined user {member}")
            return True
            
        except Exception as e:
            self.logger.error(f"Error in quarantine_user: {e}", exc_info=True)
            print(f"[QUARANTINE ERROR] An error occurred: {str(e)}")
            return False
    
    async def unquarantine_user(self, guild_id: int, user_id: int) -> bool:
        """remove a user from quarantine, restoring their admin roles."""
        self.logger.info(f"Unquarantining user {user_id} in guild {guild_id}")
        print(f"\n[UNQUARANTINE] Starting unquarantine for user {user_id} in guild {guild_id}")
        
        try:
            # first, ensure the user is removed from the in-memory set
            was_in_memory = (guild_id, user_id) in self.quarantined_users
            self.quarantined_users.discard((guild_id, user_id))
            
            # get the guild and member
            guild = self.bot.get_guild(guild_id)
            if not guild:
                error_msg = f"Guild {guild_id} not found when unquarantining user {user_id}"
                self.logger.error(error_msg)
                print(f"[UNQUARANTINE ERROR] {error_msg}")
                return False
                
            member = await guild.fetch_member(user_id)
            if not member:
                error_msg = f"User {user_id} not found in guild {guild_id} when unquarantining"
                self.logger.error(error_msg)
                print(f"[UNQUARANTINE ERROR] {error_msg}")
                return False
            
            # get the quarantine record
            record = db.execute_query(
                """
                SELECT * FROM admin_quarantine 
                WHERE guild_id = ? AND user_id = ? AND is_active = 1
                """,
                (guild_id, user_id),
                fetch=True
            )
            
            if not record:
                warning_msg = f"No active quarantine record found for user {user_id} in guild {guild_id}"
                self.logger.warning(warning_msg)
                print(f"[UNQUARANTINE WARNING] {warning_msg}")
                return False
                
            record = record[0]
            quarantined_roles = json.loads(record['quarantined_roles']) if record['quarantined_roles'] else []
            
            # restore roles if any
            if quarantined_roles:
                role_objects = []
                valid_roles = []
                
                # filter out any roles that no longer exist in the guild
                for role_id in quarantined_roles:
                    role = guild.get_role(role_id)
                    if role:
                        role_objects.append(role)
                        valid_roles.append(str(role_id))
                
                if role_objects:
                    try:
                        await member.add_roles(*role_objects, reason="Quarantine lifted")
                        self.logger.info(f"Restored {len(role_objects)} roles to user {user_id}")
                        print(f"[UNQUARANTINE] Restored {len(role_objects)} roles to user {user_id}")
                    except discord.Forbidden:
                        error_msg = f"Missing permissions to restore roles to user {user_id}"
                        self.logger.error(error_msg)
                        print(f"[UNQUARANTINE ERROR] {error_msg}")
                    except Exception as e:
                        error_msg = f"Error restoring roles to user {user_id}: {str(e)}"
                        self.logger.error(error_msg)
                        print(f"[UNQUARANTINE ERROR] {error_msg}")
            
            # update the database
            current_time = datetime.utcnow().isoformat()
            updated = db.execute_query(
                """
                UPDATE admin_quarantine 
                SET is_active = 0, restored_at = ? 
                WHERE guild_id = ? AND user_id = ? AND is_active = 1
                """,
                (current_time, guild_id, user_id),
                commit=True
            )
            
            if updated == 0:
                warning_msg = f"No rows were updated when unquarantining user {user_id}"
                self.logger.warning(warning_msg)
                print(f"[UNQUARANTINE WARNING] {warning_msg}")
            else:
                self.logger.info(f"Successfully updated database for unquarantine of user {user_id}")
                print(f"[UNQUARANTINE] Successfully updated database for user {user_id}")
            
            # grant temporary admin status for 24 hours
            expiry_time = datetime.utcnow() + timedelta(hours=24)
            self.temporary_admins[(guild_id, user_id)] = expiry_time
            
            # log the unquarantine action with temporary admin details
            await self._log_action(
                guild_id, 
                user_id, 
                "user_unquarantined", 
                target_id=user_id,
                details={
                    "was_in_memory": was_in_memory, 
                    "roles_restored": len(quarantined_roles),
                    "temporary_admin_granted": True,
                    "temporary_admin_until": expiry_time.isoformat()
                }
            )
            
            # send DM to the user about being unquarantined
            await self._send_unquarantine_dm(member, 24)
            
            success_msg = f"Successfully unquarantined user {user_id} in guild {guild_id} and granted temporary admin for 24 hours"
            self.logger.info(success_msg)
            print(f"[UNQUARANTINE] {success_msg}")
            print(f"[UNQUARANTINE] Temporary admin expires at: {expiry_time}")
            return True
            
        except Exception as e:
            error_msg = f"Error in unquarantine_user: {str(e)}"
            self.logger.error(error_msg, exc_info=True)
            print(f"[UNQUARANTINE ERROR] {error_msg}")
            return False
    
    class QuarantineView(discord.ui.View):
        """view for handling quarantine actions."""
        def __init__(self, bot, guild_id, user_id):
            super().__init__(timeout=86400)  # 24 hour timeout
            self.bot = bot
            self.guild_id = guild_id
            self.user_id = user_id
            self.message = None

        @discord.ui.button(label="Unquarantine User", style=discord.ButtonStyle.green, emoji="✅")
        async def unquarantine(self, interaction: discord.Interaction, button: discord.ui.Button):
            try:
                # defer the response to prevent interaction timeout
                await interaction.response.defer(ephemeral=True)
                
                # get the guild and member
                guild = self.bot.get_guild(self.guild_id)
                if not guild:
                    await interaction.followup.send("Error: Could not find the server.", ephemeral=True)
                    return
                    
                member = guild.get_member(self.user_id)
                if not member:
                    await interaction.followup.send("User not found in this server.", ephemeral=True)
                    return
                
                # get the quarantine record directly using the db module
                try:
                    record = db.execute_query(
                        "SELECT * FROM admin_quarantine WHERE guild_id = ? AND user_id = ? AND is_active = 1",
                        (self.guild_id, self.user_id),
                        fetch=True
                    )
                except Exception as e:
                    print(f"[QUARANTINE ERROR] Database error: {e}")
                    await interaction.followup.send("Error: Could not access the database. Please try again later.", ephemeral=True)
                    return
                
                if not record:
                    await interaction.followup.send("No active quarantine record found for this user.", ephemeral=True)
                    return
                    
                record = record[0]
                
                # get the quarantined roles
                quarantined_roles = json.loads(record['quarantined_roles']) if record['quarantined_roles'] else []
                
                try:
                    # set the flag to ignore the next role update
                    if hasattr(self.bot, 'set_ignore_next_role_update'):
                        self.bot.set_ignore_next_role_update(True)
                    
                    # restore roles if any
                    if quarantined_roles:
                        role_objects = [discord.Object(role_id) for role_id in quarantined_roles]
                        await member.add_roles(*role_objects, reason="Quarantine lifted by server owner")
                    
                    # update quarantine record with current UTC timestamp
                    current_time = datetime.utcnow().isoformat()
                    db.execute_query(
                        """
                        UPDATE admin_quarantine 
                        SET is_active = 0, restored_at = ? 
                        WHERE guild_id = ? AND user_id = ? AND is_active = 1
                        """,
                        (current_time, self.guild_id, self.user_id),
                        commit=True
                    )
                    
                    # update the message
                    embed = interaction.message.embeds[0]
                    embed.color = discord.Color.green()
                    embed.title = "✅ User Unquarantined"
                    embed.description = f"**{member}** has been unquarantined and their roles have been restored."
                    
                    # reset the flag after a short delay
                    if hasattr(self.bot, 'set_ignore_next_role_update'):
                        await asyncio.sleep(5)
                        self.bot.set_ignore_next_role_update(False)
                    
                    # update database first
                    current_time = datetime.utcnow().isoformat()
                    db.execute_query(
                        """
                        UPDATE admin_quarantine 
                        SET is_active = 0, restored_at = ? 
                        WHERE guild_id = ? AND user_id = ? AND is_active = 1
                        """,
                        (current_time, self.guild_id, self.user_id),
                        commit=True
                    )
                    
                    # update in-memory state and grant temporary admin
                    if hasattr(self.bot, 'get_cog'):
                        admin_cog = self.bot.get_cog('AdminActionCog')
                        if admin_cog and hasattr(admin_cog, 'action_tracker'):
                            # remove from quarantined users in memory
                            admin_cog.action_tracker.quarantined_users.discard((self.guild_id, self.user_id))
                            # clear any existing quarantine state
                            await admin_cog.action_tracker.unquarantine_user(self.guild_id, self.user_id)
                            # grant temporary admin status
                            temp_admin_hours = 1  # using 1 hour for testing, change back to 24 for production
                            await admin_cog.action_tracker.grant_temporary_admin(
                                self.guild_id, 
                                self.user_id,
                                duration_hours=temp_admin_hours
                            )
                            
                            try:
                                # get guild and member objects
                                guild = self.bot.get_guild(self.guild_id)
                                if not guild:
                                    print(f"[UNQUARANTINE ERROR] Guild {self.guild_id} not found")
                                    return
                                    
                                member = guild.get_member(self.user_id)
                                if not member:
                                    print(f"[UNQUARANTINE ERROR] Member {self.user_id} not found in guild {guild.id}")
                                    return
                                
                                # 1. Send DM to the unquarantined user
                                print(f"[UNQUARANTINE] Sending unquarantine DM to {member}")
                                dm_sent = await admin_cog.action_tracker._send_unquarantine_dm(member, temp_admin_hours)
                                if dm_sent:
                                    print(f"[UNQUARANTINE] Successfully sent unquarantine DM to {member}")
                                else:
                                    print(f"[UNQUARANTINE WARNING] Failed to send DM to {member} (DMs may be disabled)")
                                
                                # 2. Send notification to the admin who unquarantined the user
                                try:
                                    admin = interaction.user
                                    admin_dm_channel = await admin.create_dm()
                                    admin_embed = discord.Embed(
                                        title="✅ User Unquarantined",
                                        description=f"You have unquarantined {member.mention} in {guild.name}.",
                                        color=discord.Color.green()
                                    )
                                    admin_embed.add_field(
                                        name="Temporary Admin Status",
                                        value=f"{member.display_name} has been granted temporary admin status for {temp_admin_hours} hours.",
                                        inline=False
                                    )
                                    admin_embed.set_footer(text=f"Action performed by {admin}")
                                    
                                    await admin_dm_channel.send(embed=admin_embed)
                                    print(f"[UNQUARANTINE] Sent unquarantine confirmation to admin {admin}")
                                except Exception as admin_dm_error:
                                    print(f"[UNQUARANTINE WARNING] Could not send DM to admin {admin}: {admin_dm_error}")
                                
                                # 3. Notify the guild owner
                                if guild.owner_id != admin.id:  # don't notify owner if they're the one who unquarantined
                                    try:
                                        owner = guild.owner
                                        if owner:
                                            owner_dm_channel = await owner.create_dm()
                                            owner_embed = discord.Embed(
                                                title="ℹ️ User Unquarantined",
                                                description=f"{member.mention} has been unquarantined in {guild.name} by {admin.mention}.",
                                                color=discord.Color.blue()
                                            )
                                            owner_embed.add_field(
                                                name="Temporary Admin Status",
                                                value=f"{member.display_name} has been granted temporary admin status for {temp_admin_hours} hours.",
                                                inline=False
                                            )
                                            await owner_dm_channel.send(embed=owner_embed)
                                            print(f"[UNQUARANTINE] Notified guild owner {owner} about unquarantine")
                                    except Exception as owner_error:
                                        print(f"[UNQUARANTINE WARNING] Could not notify guild owner: {owner_error}")
                                
                            except Exception as e:
                                print(f"[UNQUARANTINE ERROR] Failed to process unquarantine notifications: {str(e)}")
                                import traceback
                                traceback.print_exc()
                    
                    # remove the action field if it exists
                    if len(embed.fields) > 2:
                        embed.remove_field(2)
                    
                    embed.add_field(
                        name="Action", 
                        value="The user's admin permissions have been restored for 24 hours.", 
                        inline=False
                    )
                    
                    # disable the buttons
                    for item in self.children:
                        item.disabled = True
                    
                    await interaction.message.edit(embed=embed, view=self)
                    
                    # get the guild and member objects
                    guild = self.bot.get_guild(self.guild_id)
                    if not guild:
                        await interaction.followup.send("Error: Guild not found.", ephemeral=True)
                        return
                        
                    member = guild.get_member(self.user_id)
                    if not member:
                        await interaction.followup.send("Error: Member not found in guild.", ephemeral=True)
                        return
                    
                    # 1. Send DM to the unquarantined user
                    try:
                        dm_embed = discord.Embed(
                            title="🔓 You've Been Unquarantined",
                            description=(
                                f"You have been unquarantined in **{guild.name}**.\n\n"
                                f"**Temporary Admin Status:** Active for 1 hour\n\n"
                                "You now have temporary admin privileges to continue your work. "
                                "This will expire automatically."
                            ),
                            color=discord.Color.green()
                        )
                        await member.send(embed=dm_embed)
                        print(f"[UNQUARANTINE] Sent unquarantine DM to {member}")
                    except Exception as e:
                        print(f"[UNQUARANTINE WARNING] Could not send DM to {member}: {e}")
                    
                    # 2. Send confirmation to the admin who unquarantined
                    try:
                        admin = interaction.user
                        admin_embed = discord.Embed(
                            title="✅ User Unquarantined",
                            description=f"You have unquarantined {member.mention} in {guild.name}.",
                            color=discord.Color.green()
                        )
                        admin_embed.add_field(
                            name="Temporary Admin Status",
                            value=f"{member.display_name} has been granted temporary admin status for 1 hour.",
                            inline=False
                        )
                        
                        # try to send DM to admin
                        try:
                            await admin.send(embed=admin_embed)
                        except:
                            # fallback to ephemeral message if DM fails
                            await interaction.followup.send(embed=admin_embed, ephemeral=True)
                        
                        print(f"[UNQUARANTINE] Sent confirmation to admin {admin}")
                        
                        # 3. Notify guild owner if they're not the one who unquarantined
                        if guild.owner and guild.owner.id != admin.id:
                            try:
                                owner_embed = discord.Embed(
                                    title="ℹ️ User Unquarantined",
                                    description=f"{member.mention} has been unquarantined in {guild.name} by {admin.mention}.",
                                    color=discord.Color.blue()
                                )
                                owner_embed.add_field(
                                    name="Temporary Admin Status",
                                    value=f"{member.display_name} has been granted temporary admin status for 1 hour.",
                                    inline=False
                                )
                                await guild.owner.send(embed=owner_embed)
                                print(f"[UNQUARANTINE] Notified guild owner {guild.owner}")
                            except Exception as owner_error:
                                print(f"[UNQUARANTINE WARNING] Could not notify guild owner: {owner_error}")
                        
                    except Exception as e:
                        print(f"[UNQUARANTINE ERROR] Failed to send notifications: {e}")
                        await interaction.followup.send("✅ User has been unquarantined successfully.", ephemeral=True)
                    
                except discord.Forbidden:
                    await interaction.followup.send("Error: I don't have permission to manage roles in this server.", ephemeral=True)
                except discord.HTTPException as e:
                    await interaction.followup.send(f"Error: Failed to update roles. {str(e)}", ephemeral=True)
                except Exception as e:
                    await interaction.followup.send(f"An unexpected error occurred: {str(e)}", ephemeral=True)
                    print(f"[QUARANTINE ERROR] Failed to unquarantine user: {e}")
                    
            except Exception as e:
                print(f"[QUARANTINE ERROR] Unhandled exception in unquarantine: {e}")
                try:
                    await interaction.followup.send("An error occurred while processing your request. Please try again.", ephemeral=True)
                except:
                    pass

        @discord.ui.button(label="Keep Quarantined", style=discord.ButtonStyle.red, emoji="🔒")
        async def keep_quarantined(self, interaction: discord.Interaction, button: discord.ui.Button):
            try:
                # defer the response to prevent interaction timeout
                await interaction.response.defer(ephemeral=True)
                
                # update the message
                embed = interaction.message.embeds[0]
                embed.color = discord.Color.orange()
                embed.title = "🔒 User Kept in Quarantine"
                
                # update the footer with who took the action
                embed.set_footer(text=f"Action taken by {interaction.user} • {datetime.utcnow().strftime('%Y-%m-%d %H:%M:%S')} UTC")
                
                # disable all buttons
                for item in self.children:
                    item.disabled = True
                
                # update the message
                await interaction.message.edit(embed=embed, view=self)
                
                # send confirmation
                await interaction.followup.send("The user will remain quarantined.", ephemeral=True)
                
            except Exception as e:
                print(f"[QUARANTINE ERROR] Failed to update quarantine status: {e}")
                try:
                    await interaction.followup.send("An error occurred while updating the quarantine status. Please try again.", ephemeral=True)
                except:
                    pass
    
    async def _notify_owner(self, guild: discord.Guild, member: discord.Member, reason: str, quarantined_until: str = None):
        """notify the guild owner about a quarantine with action buttons."""
        try:
            owner = guild.owner
            if not owner:
                print(f"[QUARANTINE] Could not find owner for guild {guild.name}")
                return

            embed = discord.Embed(
                title="🚨 Admin Action Quarantine",
                description=f"**{member}** has been quarantined due to suspicious activity.",
                color=discord.Color.red()
            )
            
            embed.add_field(name="User", value=f"{member.mention} ({member.id})", inline=False)
            embed.add_field(name="Reason", value=reason, inline=False)
            
            # add status field based on whether quarantined_until is provided
            status_text = "Please review this action and decide whether to unquarantine the user or keep them quarantined."
            if quarantined_until:
                try:
                    timestamp = int(datetime.fromisoformat(quarantined_until).timestamp())
                    status_text += f"\n\n**Auto-Release**: <t:{timestamp}:R>"
                except (ValueError, TypeError) as e:
                    print(f"[QUARANTINE WARNING] Invalid quarantined_until format: {e}")
                    status_text += "\n\n**Status**: Indefinite (requires manual unquarantine)"
            else:
                status_text += "\n\n**Status**: Indefinite (requires manual unquarantine)"
                
            embed.add_field(
                name="Action Required", 
                value=status_text,
                inline=False
            )
                
            # create and send the view with buttons
            view = self.QuarantineView(self.bot, guild.id, member.id)
            
            try:
                message = await owner.send(embed=embed, view=view)
                view.message = message
                print(f"[QUARANTINE] Sent quarantine notification to guild owner {owner} for {member}")
            except discord.Forbidden:
                print(f"[QUARANTINE] Could not DM guild owner {owner} (DMs disabled)")
                
        except Exception as e:
            print(f"[QUARANTINE ERROR] Failed to notify guild owner: {e}")
            import traceback
            print(f"[QUARANTINE ERROR] Traceback: {traceback.format_exc()}")

class AdminActionCog(commands.Cog):
    """commands for managing admin action tracking and quarantine."""
    
    def __init__(self, bot):
        self.bot = bot
        self.tracker = AdminActionTracker(bot)
        self._initialized = False
        self.logger = logging.getLogger('discord.security.admin_tracker')
        self.bot.loop.create_task(self.initialize())
    
    async def initialize(self):
        """initialize the cog after the bot is ready."""
        await self.bot.wait_until_ready()
        self._initialized = True
        self.logger.info("AdminActionCog initialized")
        print("\n[AdminActionCog] Initialized and ready to track admin actions")
        
    @commands.Cog.listener()
    async def on_member_ban(self, guild: discord.Guild, user: discord.User):
        """track ban actions."""
        if not self._initialized:
            return
            
        # skip if actions security is disabled for this guild
        if not AdminSecuritySettings.is_actions_security_enabled(guild.id):
            return
            
        # skip if the bot performed the ban
        if user.id == self.bot.user.id:
            return
            
        # get the moderator who performed the ban
        try:
            async for entry in guild.audit_logs(limit=5, action=discord.AuditLogAction.ban):
                if entry.target.id == user.id:
                    # skip if the action was performed by the bot itself
                    if entry.user.id == self.bot.user.id:
                        return
                        
                    # log the ban action
                    self.logger.info(f"Ban detected: {entry.user} (ID: {entry.user.id}) banned {user} (ID: {user.id}) in {guild.name}")
                    print(f"\n[BAN] {entry.user} (ID: {entry.user.id}) banned {user} (ID: {user.id}) in {guild.name}")
                        
                    # record the ban action
                    allowed = await self.tracker.record_action(
                        guild.id, 
                        entry.user.id, 
                        'ban', 
                        user.id
                    )
                    
                    # if the action was blocked (too many bans in short time)
                    if not allowed and entry.user.id != guild.owner_id:
                        try:
                            # try to unban the user
                            await guild.unban(user, reason="Automatic unban: Too many bans in short time")
                            self.logger.warning(f"Automatically unbanned user {user} (ID: {user.id}) due to rate limiting")
                            print(f"\n[UNBAN] Automatically unbanned {user} (ID: {user.id}) due to rate limiting")
                        except Exception as e:
                            self.logger.error(f"Failed to unban user {user.id}: {str(e)}")
                    break
        except Exception as e:
            self.logger.error(f"Failed to track ban action: {str(e)}")
            print(f"\n[ERROR] Failed to track ban action: {str(e)}")
    
    @commands.Cog.listener()
    async def on_member_remove(self, member: discord.Member):
        """track kick actions (approximation - could be kick or leave)."""
        if not self._initialized:
            return
            
        # skip if actions security is disabled for this guild
        if not AdminSecuritySettings.is_actions_security_enabled(member.guild.id):
            return
            
        # skip if the bot was the one who left
        if member.id == self.bot.user.id:
            return
            
        try:
            async for entry in member.guild.audit_logs(limit=5, action=discord.AuditLogAction.kick):
                if entry.target.id == member.id:
                    # skip if the action was performed by the bot itself
                    if entry.user.id == self.bot.user.id:
                        return
                        
                    # log the kick action
                    self.logger.info(f"Kick detected: {entry.user} (ID: {entry.user.id}) kicked {member} (ID: {member.id}) in {member.guild.name}")
                    print(f"\n[KICK] {entry.user} (ID: {entry.user.id}) kicked {member} (ID: {member.id}) in {member.guild.name}")
                        
                    await self.tracker.record_action(
                        member.guild.id, 
                        entry.user.id, 
                        'kick', 
                        member.id
                    )
                    break
        except Exception as e:
            self.logger.error(f"Failed to track kick action: {str(e)}")
            print(f"\n[ERROR] Failed to track kick action: {str(e)}")
    
    @commands.Cog.listener()
    async def on_guild_channel_delete(self, channel: discord.abc.GuildChannel):
        """track channel deletion actions."""
        if not self._initialized:
            self.logger.warning("Cog not initialized, skipping channel delete tracking")
            return
            
        # skip if actions security is disabled for this guild
        if not AdminSecuritySettings.is_actions_security_enabled(channel.guild.id):
            return
            
        # check if bot has permission to view audit logs
        if not channel.guild.me.guild_permissions.view_audit_log:
            self.logger.error("Bot doesn't have 'View Audit Log' permission")
            print("\n[ERROR] Bot needs 'View Audit Log' permission to track channel deletions")
            return
            
        self.logger.info(f"Channel deleted: #{channel.name} (ID: {channel.id}) in {channel.guild.name}")
        print(f"\n[CHANNEL DELETE] Detected deletion of #{channel.name} (ID: {channel.id}) in {channel.guild.name}")
        print(f"[DEBUG] Bot permissions: {', '.join([perm for perm, value in channel.guild.me.guild_permissions if value])}")
            
        try:
            # get the most recent audit log entry for channel deletion
            async for entry in channel.guild.audit_logs(limit=5, action=discord.AuditLogAction.channel_delete):
                self.logger.info(f"Found audit log entry: {entry.action} by {entry.user} (ID: {entry.user.id})")
                
                # check if this entry is for the channel we're interested in
                if hasattr(entry, 'target') and entry.target and entry.target.id == channel.id:
                    # skip if the action was performed by the bot itself
                    if entry.user.id == self.bot.user.id:
                        self.logger.info("Skipping bot's own action")
                        return
                        
                    self.logger.info(f"Channel delete detected: {entry.user} (ID: {entry.user.id}) deleted #{channel.name} (ID: {channel.id}) in {channel.guild.name}")
                    print(f"\n[CHANNEL DELETE] {entry.user} (ID: {entry.user.id}) deleted #{channel.name} (ID: {channel.id}) in {channel.guild.name}")
                    
                    # record the action
                    result = await self.tracker.record_action(
                        channel.guild.id, 
                        entry.user.id, 
                        'channel_delete', 
                        channel.id
                    )
                    
                    self.logger.info(f"Recorded channel delete action. Result: {result}")
                    return  # exit after processing the correct audit log entry
                    
            # if we get here, no matching audit log entry was found
            self.logger.warning(f"No matching audit log entry found for channel deletion: #{channel.name} (ID: {channel.id})")
            
        except discord.Forbidden:
            self.logger.error("Bot doesn't have permission to view audit logs")
            print("\n[ERROR] Bot needs 'View Audit Log' permission to track channel deletions")
        except Exception as e:
            self.logger.error(f"Failed to track channel deletion: {str(e)}", exc_info=True)
            print(f"\n[ERROR] Failed to track channel deletion: {str(e)}")
            
    @commands.Cog.listener()
    async def on_member_update(self, before: discord.Member, after: discord.Member):
        """track role changes to detect admin role assignments."""
        # skip if the member is the bot itself
        if after.id == self.bot.user.id:
            return
            
        # skip if actions security is disabled for this guild
        if not AdminSecuritySettings.is_actions_security_enabled(after.guild.id):
            return
            
        # check if roles were added
        added_roles = set(after.roles) - set(before.roles)
        if not added_roles:
            return
            
        # get the admin role (you may need to adjust this based on your role setup)
        admin_roles = [role for role in added_roles if role.permissions.administrator]
        if not admin_roles:
            return
            
        # log the admin role assignment
        self.logger.info(f"Admin role assigned: {after} (ID: {after.id}) was given admin role(s) in {after.guild.name}")
        print(f"\n[ADMIN ROLE ASSIGNED] {after} (ID: {after.id}) was given admin role(s) in {after.guild.name}")
        
        # record this action
        for role in admin_roles:
            await self.tracker.record_action(
                after.guild.id,
                after.id,
                'admin_role_added',
                role.id
            )
    
    @commands.command(name="unquarantine")
    @commands.has_permissions(administrator=True)
    @commands.guild_only()
    async def unquarantine_cmd(self, ctx, member: discord.Member):
        """remove a user from quarantine and restore their admin roles."""
        if not await self.tracker.is_quarantined(ctx.guild.id, member.id):
            await ctx.send(f"{member.mention} is not currently quarantined.")
            return
            
        if await self.tracker.unquarantine_user(ctx.guild.id, member.id):
            await ctx.send(f"✅ Successfully unquarantined {member.mention} and restored their admin roles.")
        else:
            await ctx.send("❌ Failed to unquarantine user. Please check the logs for details.")
    
    @commands.command(name="quarantine_status")
    @commands.has_permissions(administrator=True)
    @commands.guild_only()
    async def quarantine_status_cmd(self, ctx, member: discord.Member = None):
        """check quarantine status for a user or all users."""
        if member:
            # check status for a specific user
            records = db.execute_query(
                "SELECT * FROM admin_quarantine WHERE guild_id = ? AND user_id = ?",
                (ctx.guild.id, member.id),
                fetch=True
            )
            
            if not records:
                await ctx.send(f"{member.mention} is not quarantined.")
                return
                
            record = records[0]
            embed = discord.Embed(
                title=f"🔒 Quarantine Status for {member}",
                color=discord.Color.orange(),
                timestamp=datetime.utcnow()
            )
            
            embed.add_field(name="User", value=f"{member.mention} (`{member.id}`)", inline=False)
            embed.add_field(name="Reason", value=record['reason'] or "No reason provided", inline=False)
            
            if record['quarantined_until']:
                release_time = datetime.fromisoformat(record['quarantined_until'])
                embed.add_field(
                    name="Auto-Release", 
                    value=f"<t:{int(release_time.timestamp())}:R>"
                )
            else:
                embed.add_field(name="Status", value="Indefinite (until manually released)")
            
            embed.set_footer(text=f"Quarantined by {self.bot.user.name}")
            await ctx.send(embed=embed)
            
        else:
            # show all quarantined users in the guild
            records = db.execute_query(
                "SELECT * FROM admin_quarantine WHERE guild_id = ?",
                (ctx.guild.id,),
                fetch=True
            )
            
            if not records:
                await ctx.send("No users are currently quarantined in this server.")
                return
                
            embed = discord.Embed(
                title=f"🔒 Quarantined Users in {ctx.guild.name}",
                color=discord.Color.orange(),
                timestamp=datetime.utcnow()
            )
            
            for record in records:
                user = ctx.guild.get_member(record['user_id'])
                user_str = f"<@{record['user_id']}>" if not user else user.mention
                
                # format the status
                if record['quarantined_until']:
                    release_time = datetime.fromisoformat(record['quarantined_until'])
                    status = f"Until <t:{int(release_time.timestamp())}:R>"
                else:
                    status = "Indefinite"
                
                # add a field for each quarantined user
                embed.add_field(
                    name=str(user) if user else f"User ID: {record['user_id']}",
                    value=(
                        f"{user_str} • {status}\n"
                        f"**Reason**: {record['reason'] or 'No reason provided'}"
                    ),
                    inline=False
                )
            
            # add a footer with instructions
            embed.set_footer(text=f"Use {ctx.prefix}quarantine_status @user for more details")
            
            # send the embed
            await ctx.send(embed=embed)

# setup is now handled in security/__init__.py
