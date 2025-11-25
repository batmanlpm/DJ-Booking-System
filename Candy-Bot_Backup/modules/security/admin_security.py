"""
admin security handlers for the bot.
"""
import asyncio
import datetime
import logging
import traceback
import discord
from discord.ext import commands
from discord import ui, app_commands
from typing import List, Optional, Dict, Any, Union, Tuple
import asyncio
import logging

from modules.database import db

# set up logging
logger = logging.getLogger('discord.security.admin')

def log_action(message: str, level: str = 'info', **kwargs):
    """helper function for consistent logging."""
    log_message = message
    if kwargs:
        log_message += " " + " ".join(f"{k}={v!r}" for k, v in kwargs.items())
    
    if level == 'debug':
        logger.debug(log_message)
    elif level == 'warning':
        logger.warning(log_message)
    elif level == 'error':
        logger.error(log_message)
    else:
        logger.info(log_message)
    
    # also print to console for immediate feedback
    print(f"[Security] {log_message}")

# global flag to track when we're processing an approval
_processing_approval = False

# global flag to skip the next role update check
_ignore_next_role_update = False

class AdminApprovalView(ui.View):
    """view for approving/denying admin role assignments."""
    def __init__(self, assignment_id: int, assigner_id: int, assignee_id: int, role_id: int, guild_id: int):
        super().__init__(timeout=86400)  # 24 hour timeout
        self.assignment_id = assignment_id
        self.assigner_id = assigner_id
        self.assignee_id = assignee_id
        self.role_id = role_id
        self.guild_id = guild_id
        self._processing = False
        self._processed = False

    @ui.button(label="✅ Accept", style=discord.ButtonStyle.green)
    async def confirm(self, interaction: discord.Interaction, button: ui.Button):
        """handle confirmation of admin role assignment."""
        log_action("Starting admin approval process", 
                 user_id=interaction.user.id,
                 guild_id=self.guild_id,
                 assignment_id=self.assignment_id,
                 processing=self._processing,
                 processed=self._processed)
        
        # prevent double processing
        if self._processing or self._processed:
            log_action("preventing duplicate processing", 
                     user_id=interaction.user.id,
                     guild_id=self.guild_id,
                     assignment_id=self.assignment_id,
                     processing=self._processing,
                     processed=self._processed)
            if not interaction.response.is_done():
                await interaction.response.defer(ephemeral=True)
            await interaction.followup.send("This request is already being processed or has been completed.", ephemeral=True)
            return
            
        # mark as processing
        self._processing = True
        log_action("Marked as processing", 
                 user_id=interaction.user.id,
                 guild_id=self.guild_id,
                 assignment_id=self.assignment_id)
        
        # defer the interaction first to prevent timeout
        await interaction.response.defer(ephemeral=True)
        
        log_action("Admin approval view confirm button clicked", 
                 user_id=interaction.user.id, 
                 guild_id=self.guild_id,
                 assignment_id=self.assignment_id)

        # get the guild from the bot
        bot = interaction.client
        guild = bot.get_guild(self.guild_id)
        if not guild:
            error_msg = f"Could not find guild with ID {self.guild_id}"
            log_action(error_msg, level='error', 
                     user_id=interaction.user.id,
                     guild_id=self.guild_id)
            await interaction.followup.send("❌ Could not find the server. Please try again.", ephemeral=True)
            self._processing = False
            return
            
        log_action("Found guild", 
                 guild_id=self.guild_id,
                 guild_name=guild.name,
                 owner_id=guild.owner_id,
                 member_count=len(guild.members))

        # only server owner can confirm
        if interaction.user.id != guild.owner_id:
            log_action("Non-owner attempted to approve admin role", 
                      level='warning',
                      user_id=interaction.user.id,
                      guild_id=interaction.guild.id,
                      interaction_user_id=interaction.user.id,
                      guild_owner_id=guild.owner_id)
            try:
                await interaction.followup.send(
                    "❌ Only the server owner can confirm admin role assignments.",
                    ephemeral=True
                )
            except Exception as e:
                log_action("Failed to send non-owner message",
                          level='error',
                          error=str(e))
            self._processing = False
            return

        try:
            # get the assignment details
            log_action("Fetching assignment details from database",
                     assignment_id=self.assignment_id)
                     
            # first, let's check what tables exist
            tables = db.execute_query("SELECT name FROM sqlite_master WHERE type='table'")
            log_action("Available tables", tables=[row[0] for row in tables] if tables else [])
            
            # now let's check the structure of the pending_admin_assignments table
            columns = db.execute_query("PRAGMA table_info(pending_admin_assignments)", fetch=True)
            if not columns:
                raise Exception("No columns found in pending_admin_assignments table")
                
            column_names = [col[1] for col in columns]  # column name is at index 1
            log_action("Database columns found", columns=column_names)
            
            # skip checking all assignments to avoid timestamp issues
            log_action("Skipping full table scan to avoid timestamp parsing issues")
            
            # fetch the specific assignment with explicit column selection
            # convert timestamp fields to strings to avoid parsing issues
            assignment = db.execute_query(
                """
                SELECT 
                    id, guild_id, assigner_id, assignee_id, role_id, 
                    status, assigner_roles,
                    strftime('%Y-%m-%d %H:%M:%S', created_at) as created_at,
                    strftime('%Y-%m-%d %H:%M:%S', updated_at) as updated_at,
                    strftime('%Y-%m-%d %H:%M:%S', expires_at) as expires_at,
                    strftime('%Y-%m-%d %H:%M:%S', resolved_at) as resolved_at,
                    notes
                FROM pending_admin_assignments 
                WHERE id = ? AND status = 'pending' AND guild_id = ?
                """,
                (self.assignment_id, self.guild_id),
                fetch=True
            )

            if not assignment:
                await interaction.followup.send("❌ This assignment has already been processed or doesn't exist.", ephemeral=True)
                return

            # get the first row and convert to dict if it's a Row object
            assignment = assignment[0]
            if hasattr(assignment, '_asdict'):
                assignment = assignment._asdict()
            elif hasattr(assignment, 'keys') and callable(assignment.keys):
                assignment = {k: assignment[k] for k in assignment.keys()}
                
            log_action("Processing assignment", assignment=assignment)
            
            # get members and role
            assigner_id = assignment.get('assigner_id')
            assignee_id = assignment.get('assignee_id')
            role_id = assignment.get('role_id')
            
            # get the assigner and assignee
            assigner = guild.get_member(assignment['assigner_id'])
            assignee = guild.get_member(assignment['assignee_id'])
            role = guild.get_role(assignment['role_id'])
            
            # prevent self-assignment
            if assigner and assignee and assigner.id == assignee.id:
                log_action("Self-assignment not allowed",
                         assigner_id=assigner.id,
                         assignee_id=assignee.id,
                         level='warning')
                await interaction.followup.send("❌ Self-assignment of admin roles is not allowed.", ephemeral=True)
                success = False
                return
            
            log_action("Resolved objects", 
                      assigner=f"{assigner} (ID: {assigner_id})", 
                      assignee=f"{assignee} (ID: {assignee_id})",
                      role=f"{role} (ID: {role_id})")

            if not all([assigner, assignee, role]):
                await interaction.followup.send("❌ Could not find all required members/roles.", ephemeral=True)
                return

            # disable buttons to prevent double-clicks
            for item in self.children:
                if isinstance(item, ui.Button):
                    item.disabled = True
            await interaction.message.edit(view=self)

            # update the assignment status
            current_time = datetime.datetime.utcnow()
            updated = db.execute_query(
                """
                UPDATE pending_admin_assignments 
                SET status = 'approved', 
                    resolved_at = ?,
                    updated_at = ?
                WHERE id = ? AND status = 'pending'
                """,
                (current_time, current_time, self.assignment_id),
                commit=True
            )

            if not updated:
                await interaction.followup.send("❌ Failed to update assignment status.", ephemeral=True)
                return

            # process role assignments
            success = True
            
            # 1. add role to assignee
            try:
                log_action("Attempting to add role to assignee", 
                         assignee_id=assignee.id, 
                         role_id=role.id,
                         role_name=role.name)
                
                # check if the assignee already has the role
                if role in assignee.roles:
                    log_action("Assignee already has the role",
                             assignee_id=assignee.id,
                             role_id=role.id,
                             role_name=role.name)
                else:
                    await assignee.add_roles(role, reason=f"Admin role assignment approved by {interaction.user}")
                    log_action("Successfully added role to assignee",
                             assignee_id=assignee.id,
                             role_id=role.id,
                             role_name=role.name)
                
            except discord.Forbidden:
                error_msg = f"Bot lacks permissions to add role {role.name} to {assignee}"
                log_action(error_msg, level='error', 
                         guild_id=guild.id, 
                         role_id=role.id,
                         assignee_id=assignee.id)
                await interaction.followup.send(f"❌ {error_msg}", ephemeral=True)
                success = False
                
            except Exception as e:
                error_msg = f"Failed to add role to assignee: {str(e)}"
                log_action(error_msg, level='error', 
                         error_type=type(e).__name__,
                         traceback=traceback.format_exc())
                success = False
            
            # 2. restore assigner's roles if any
            assigner_roles = assignment.get('assigner_roles', '')
            if assigner_roles:
                try:
                    log_action("Found assigner roles to restore", 
                             assigner_id=assigner.id,
                             assigner_roles=assigner_roles)
                    
                    # parse role IDs and filter out invalid ones
                    role_ids = []
                    for rid in assigner_roles.split(','):
                        rid = rid.strip()
                        if rid.isdigit():
                            role_ids.append(int(rid))
                    
                    log_action("Parsed role IDs", role_ids=role_ids)
                    
                    # get valid role objects
                    roles_to_restore = []
                    for rid in role_ids:
                        role = guild.get_role(rid)
                        if role:
                            roles_to_restore.append(role)
                    
                    log_action("Roles to restore", 
                             role_ids=[r.id for r in roles_to_restore],
                             role_names=[r.name for r in roles_to_restore])
                    
                    if roles_to_restore:
                        try:
                            await assigner.add_roles(
                                *roles_to_restore, 
                                reason=f"Restoring admin roles after approving assignment {self.assignment_id}"
                            )
                            log_action("Successfully restored assigner roles",
                                     assigner_id=assigner.id,
                                     role_ids=[r.id for r in roles_to_restore])
                        except discord.Forbidden:
                            error_msg = f"Bot lacks permissions to restore roles to {assigner}"
                            log_action(error_msg, level='error',
                                     guild_id=guild.id,
                                     assigner_id=assigner.id,
                                     role_ids=[r.id for r in roles_to_restore])
                            success = False
                        except Exception as e:
                            error_msg = f"Error restoring roles to assigner: {str(e)}"
                            log_action(error_msg, level='error',
                                     error_type=type(e).__name__,
                                     traceback=traceback.format_exc())
                            success = False
                    else:
                        log_action("No valid roles to restore for assigner",
                                 assigner_id=assigner.id)
                        
                except Exception as e:
                    error_msg = f"Failed to process assigner roles: {str(e)}"
                    log_action(error_msg, level='error',
                             error_type=type(e).__name__,
                             traceback=traceback.format_exc())
                    success = False

            # handle success/failure
            if success:
                # success message
                # create and update the approval embed
                try:
                    # create the approval embed
                    approval_embed = discord.Embed(
                        title="✅ Admin Role Assignment Approved",
                        color=discord.Color.green(),
                        timestamp=datetime.datetime.utcnow()
                    )
                    
                    # handle role display
                    if getattr(role, 'mentionable', False):
                        role_display = f"{role.mention} (`{role.id}`)"
                    else:
                        role_display = f"@{role.name} (`{role.id}`)"
                    
                    approval_embed.add_field(name="Approved By", value=interaction.user.mention, inline=False)
                    approval_embed.add_field(name="Assignee", value=f"{assignee.mention} ({assignee.id})", inline=False)
                    approval_embed.add_field(name="Role Assigned", value=role_display, inline=False)
                    approval_embed.set_footer(text=f"Assignment ID: {self.assignment_id}")
                    
                    # update the original message with the approval embed and remove buttons
                    await interaction.message.edit(
                        content=None,  # remove any existing content
                        embed=approval_embed,
                        view=None  # this removes all buttons
                    )
                    
                    # acknowledge the interaction to prevent "interaction failed"
                    await interaction.response.defer()
                    
                except Exception as e:
                    log_action("Failed to update approval message",
                             error=str(e),
                             error_type=type(e).__name__,
                             level='error')
                    try:
                        await interaction.response.send_message("Failed to update approval message. Please check logs.", ephemeral=True)
                    except:
                        pass
                
                # silently send DMs to involved parties if possible
                dm_messages = {
                    assignee: f"✅ **Admin Role Approved**\n\n"
                             f"You have been granted the `{role.name}` role in **{guild.name}** by {interaction.user.mention}. "
                             f"You now have administrative privileges in the server.\n\n"
                             f"*This is an automated message.*",
                    assigner: f"✅ **Admin Role Assignment Approved**\n\n"
                             f"Your assignment of the `{role.name}` role to {assignee.mention} in **{guild.name}** has been approved. "
                             f"Your previous admin roles have been restored.\n\n"
                             f"*This is an automated message.*"
                }
                
                # silently try to send DMs without notifying in channel
                for member, message in dm_messages.items():
                    try:
                        await member.send(message)
                        log_action(
                            message=f"Sent DM to member {member.id}",
                            member_id=member.id,
                            guild_id=guild.id,
                            level='debug'
                        )
                    except Exception:
                        # silently fail for DMs - we don't need to notify about this
                        pass
                
                log_action("Successfully completed admin role assignment",
                         assignment_id=self.assignment_id,
                         guild_id=guild.id,
                         assigner_id=assigner.id,
                         assignee_id=assignee.id,
                         role_id=role.id)
                
            else:
                error_msg = "❌ Failed to complete the admin role assignment. Please check the logs for details."
                log_action("Admin role assignment failed",
                         level='error',
                         assignment_id=self.assignment_id,
                         guild_id=guild.id,
                         assigner_id=assigner.id if assigner else None,
                         assignee_id=assignee.id if assignee else None,
                         role_id=role.id if role else None)
                
                try:
                    await interaction.followup.send(error_msg, ephemeral=True)
                except Exception as e:
                    log_action("Failed to send error message",
                             error=str(e),
                             error_type=type(e).__name__,
                             level='error')
            
            # mark as processed
            self._processed = True
            
        except Exception as e:
            log_action("Error in confirm button handler", error=str(e), level='error')
        finally:
            try:
                # disable buttons after processing
                log_action("Disabling buttons and cleaning up...")
                for item in self.children:
                    if isinstance(item, ui.Button):
                        item.disabled = True
                
                # update the message to show it's been processed
                try:
                    if not interaction.response.is_done():
                        await interaction.response.defer(ephemeral=True)
                    await interaction.edit_original_response(view=self)
                    log_action("Successfully updated message with disabled buttons")
                except Exception as e:
                    log_action("Failed to update message with disabled buttons",
                             level='error',
                             error=str(e))
                
                # log completion
                log_action("Admin approval process completed",
                         assignment_id=self.assignment_id,
                         success=not self._processing or self._processed)
                
            except Exception as e:
                log_action("Error in cleanup process",
                         level='error',
                         error=str(e),
                         traceback=traceback.format_exc())
            finally:
                # always ensure processing is marked as complete
                self._processing = False

    @ui.button(label="❌ Decline", style=discord.ButtonStyle.red)
    async def decline(self, interaction: discord.Interaction, button: ui.Button):
        """handle denial of admin role assignment."""
        try:
            # defer the response first to prevent interaction timeout
            try:
                await interaction.response.defer(ephemeral=False)
            except:
                pass

            # get the guild using the stored guild_id
            guild = interaction.client.get_guild(self.guild_id)
            if not guild:
                try:
                    await interaction.followup.send("❌ Could not find server information. Please try again in the server where this request was made.", ephemeral=True)
                except:
                    pass
                return

            # only server owner can deny
            if interaction.user.id != guild.owner_id:
                try:
                    await interaction.followup.send(
                        "❌ Only the server owner can deny admin role assignments.",
                        ephemeral=True
                    )
                except:
                    pass
                return

            # get the necessary members and role
            try:
                # fetch members and role in parallel
                tasks = []
                if self.assigner_id:
                    tasks.append(guild.fetch_member(self.assigner_id))
                if self.assignee_id:
                    tasks.append(guild.fetch_member(self.assignee_id))
                
                # get the role directly from the guild's roles with debug info
                role = None
                if self.role_id:
                    role = guild.get_role(self.role_id)
                    if not role:
                        # if role not found in cache, try fetching all roles
                        try:
                            roles = await guild.fetch_roles()
                            role = discord.utils.get(roles, id=self.role_id)
                        except Exception as e:
                            log_action("Error fetching roles", error=str(e), guild_id=guild.id)
                    
                    log_action("Role lookup result", 
                             role_id=self.role_id, 
                             found=role is not None,
                             guild_roles_count=len(guild.roles) if guild else 0,
                             guild_id=guild.id if guild else None)
                
                # await all member fetches
                members = await asyncio.gather(*tasks, return_exceptions=True)
                
                # assign members based on which tasks were added
                assigner = None
                assignee = None
                if self.assigner_id and len(members) > 0 and not isinstance(members[0], Exception):
                    assigner = members[0]
                if self.assignee_id:
                    idx = 1 if self.assigner_id else 0
                    if len(members) > idx and not isinstance(members[idx], Exception):
                        assignee = members[idx]
            except Exception as e:
                log_action("Error fetching members/role for decline", 
                         error=str(e), 
                         guild_id=guild.id,
                         assigner_id=self.assigner_id,
                         assignee_id=self.assignee_id,
                         role_id=self.role_id)
                await interaction.followup.send("❌ An error occurred while fetching member or role information.", ephemeral=True)
                return
            
            # debug log the role object
            log_action("Role object before embed", 
                     role_id=getattr(role, 'id', None), 
                     role_name=getattr(role, 'name', None),
                     role_mentionable=getattr(role, 'mentionable', None))
            
            # create a clean declined embed
            declined_embed = discord.Embed(
                title="❌ Admin Role Assignment Declined",
                color=discord.Color.red(),
                timestamp=datetime.datetime.utcnow()
            )
            
            if assignee:
                declined_embed.add_field(name="Assignee", value=f"{assignee.mention} ({assignee.id})", inline=False)
            
            # handle role display
            role_display = f"@unknown-role (`{self.role_id}`)"
            if role:
                try:
                    # if role is not mentionable, use the name instead of mention
                    if getattr(role, 'mentionable', False):
                        role_display = f"{role.mention} (`{role.id}`)"
                    else:
                        role_name = getattr(role, 'name', 'unknown-role')
                        role_display = f"@{role_name} (`{role.id}`)"
                except Exception as e:
                    log_action("Error creating role display", error=str(e))
                    role_name = getattr(role, 'name', 'unknown-role')
                    role_id = getattr(role, 'id', self.role_id)
                    role_display = f"@{role_name} (`{role_id}`)"
            else:
                # fallback to database if role object is not available
                try:
                    from database import db
                    role_info = await db.fetchrow("SELECT role_name FROM admin_roles WHERE role_id = $1 AND guild_id = $2", 
                                               self.role_id, guild.id)
                    if role_info and 'role_name' in role_info:
                        role_name = role_info['role_name']
                        role_display = f"@{role_name} (`{self.role_id}`)"
                except Exception as e:
                    log_action("Database fallback failed", error=str(e))
            declined_embed.add_field(name="Role", value=role_display, inline=False)
            if assigner:
                declined_embed.add_field(name="Requested By", value=f"{assigner.mention} ({assigner.id})", inline=False)
                
            declined_embed.add_field(name="Declined By", value=interaction.user.mention, inline=False)
            declined_embed.set_footer(text=f"Assignment ID: {self.assignment_id}")
            
            # disable all buttons
            for item in self.children:
                if hasattr(item, 'disabled'):
                    item.disabled = True
            
            # edit the original message with the new embed and disabled buttons
            try:
                await interaction.edit_original_response(embed=declined_embed, view=self)
            except:
                # if we can't edit the original message, try sending a new one
                try:
                    await interaction.followup.send(embed=declined_embed, view=self)
                except Exception as e:
                    log_action("Failed to send decline message", error=str(e), level='error')
                    return
            
            # update the database to mark the assignment as declined
            try:
                from database import db
                # update the assignment status to 'declined' and set resolved timestamp
                result = await db.fetchval(
                    """
                    UPDATE pending_admin_assignments 
                    SET status = 'declined', 
                        resolved_at = CURRENT_TIMESTAMP,
                        resolved_by = $1,
                        updated_at = CURRENT_TIMESTAMP
                    WHERE id = $2
                    RETURNING id
                    """,
                    interaction.user.id,
                    self.assignment_id
                )
                
                if not result:
                    log_action("Failed to update database for declined assignment", 
                             assignment_id=self.assignment_id, 
                             level='error')
            except Exception as e:
                log_action("Database error in decline handler", 
                         error=str(e), 
                         assignment_id=self.assignment_id,
                         level='error')
            
            # send DMs to assigner and assignee if possible
            try:
                if assigner and assigner != interaction.user:
                    try:
                        await assigner.send(f"❌ Your admin role assignment to {assignee or 'a user'} has been declined by {interaction.user}.")
                    except:
                        pass
                        
                if assignee and assignee != interaction.user and assignee != assigner:
                    try:
                        await assignee.send(f"❌ The admin role assignment for you has been declined by {interaction.user}.")
                    except:
                        pass
                        
            except Exception as e:
                log_action("Error sending DMs for declined assignment", error=str(e), level='error')
                
        except Exception as e:
            log_action("Error in decline button handler", error=str(e), level='error')
            try:
                if not interaction.response.is_done():
                    await interaction.response.send_message("An error occurred while processing your request.", ephemeral=True)
                else:
                    await interaction.followup.send("An error occurred while processing your request.", ephemeral=True)
            except:
                pass

async def get_admin_roles(member: discord.Member) -> List[discord.Role]:
    """get all admin roles for a guild from a member object."""
    try:
        if not member or not member.guild:
            log_action("Invalid member or guild object", 
                     level='error',
                     member_id=getattr(member, 'id', None),
                     guild_id=getattr(member.guild, 'id', None) if member else None)
            return []
            
        guild = member.guild
        guild_id = guild.id
        
        # get admin role IDs from database
        log_action("Fetching admin roles from database", guild_id=guild_id)
        
        # first, check what tables exist in the database
        tables = db.execute_query(
            "SELECT name FROM sqlite_master WHERE type='table' AND name='bot_admins'"
        )
        log_action("Database tables check", tables_found=bool(tables))
        
        # get the admin roles with explicit column selection
        admin_roles = db.execute_query(
            "SELECT role_id FROM bot_admins WHERE guild_id = ?",
            (guild_id,),
            fetch=True
        )
        
        log_action("Raw admin roles query result", 
                  result=admin_roles,
                  result_type=type(admin_roles).__name__)
        log_action("Raw database results", 
                  guild_id=guild_id,
                  results=admin_roles,
                  result_type=type(admin_roles).__name__ if admin_roles else 'None')
        
        if not admin_roles:
            log_action("No admin roles found in database", 
                      guild_id=guild_id)
            return []
            
        # process admin roles
        result = []
        for row in admin_roles:
            try:
                # extract role_id from row
                role_id = None
                try:
                    # try to get role_id directly by index (most reliable for SQLite Row)
                    if hasattr(row, '__getitem__'):
                        role_id = row[0]  # first column is role_id in our query
                    # if that fails, try other methods
                    elif hasattr(row, '_asdict'):  # SQLite Row object
                        role_id = row._asdict().get('role_id')
                    elif isinstance(row, dict):
                        role_id = row.get('role_id')
                    
                    # debug log the row structure
                    log_action("Row structure", 
                             guild_id=guild_id,
                             row_type=type(row).__name__,
                             row_keys=dir(row) if hasattr(row, '__dir__') else 'no_dir',
                             row_repr=str(row)[:100])
                except Exception as e:
                    log_action("Error extracting role_id",
                             level='error',
                             error=str(e),
                             row_type=type(row).__name__)
                
                if not role_id:
                    log_action("No role_id found in row",
                              level='warning',
                              row=str(row))
                    continue
                
                # get the role from the guild
                role = guild.get_role(int(role_id))
                if role:
                    result.append(role)
                    log_action("Found admin role",
                              guild_id=guild_id,
                              role_id=role.id,
                              role_name=role.name)
                else:
                    log_action("Role not found in guild",
                              level='warning',
                              guild_id=guild_id,
                              role_id=role_id)
            except Exception as e:
                log_action("Error processing admin role",
                          level='error',
                          guild_id=guild_id,
                          row=str(row),
                          error=str(e))
                
        log_action("Retrieved admin roles", 
                  guild_id=guild_id, 
                  role_ids=[r.id for r in result],
                  role_count=len(result))
        return result
        
    except Exception as e:
        log_action("Error getting admin roles", 
                  level='error',
                  guild_id=guild_id,
                  error=str(e))
        return []

_processing_approval = False

from .settings import AdminSecuritySettings

async def on_member_update(before: discord.Member, after: discord.Member):
    """handle member updates to detect admin role assignments."""
    global _processing_approval, _ignore_next_role_update
    
    # skip if we're currently processing an approval
    if _processing_approval:
        return
        
    # skip if we're ignoring the next role update
    if _ignore_next_role_update:
        _ignore_next_role_update = False
        log_action("Skipping role update check due to ignore flag",
                 guild_id=after.guild.id,
                 member_id=after.id)
        return
        
    # skip if admin security is disabled for this guild
    if not AdminSecuritySettings.is_security_enabled(after.guild.id):
        return
        
    # skip if no roles were added
    if before.roles == after.roles:
        log_action("Member update detected", 
              guild_id=after.guild.id, 
              member_id=after.id, 
              member_name=str(after))
    
    # skip if no role changes
    if before.roles == after.roles:
        log_action("No role changes detected, skipping", 
                  guild_id=after.guild.id, 
                  member_id=after.id)
        return

    # get the roles that were added
    added_roles = [role for role in after.roles if role not in before.roles]
    if not added_roles:
        log_action("No roles were added, skipping", 
                  guild_id=after.guild.id, 
                  member_id=after.id)
        return

    log_action("Added roles", 
              guild_id=after.guild.id, 
              member_id=after.id, 
              added_roles=[r.name for r in added_roles])

    # check if any of the added roles are admin roles
    admin_roles = await get_admin_roles(after)
    admin_role_ids = [r.id for r in admin_roles]
    log_action("Admin roles in guild", 
              guild_id=after.guild.id, 
              admin_role_ids=admin_role_ids)
              
    # get the intersection of added role IDs and admin role IDs
    added_admin_roles = [r for r in added_roles if r.id in admin_role_ids]
    if not added_admin_roles:
        log_action("No admin roles were added, skipping",
                  guild_id=after.guild.id,
                  member_id=after.id)
        return
              
    added_admin_roles = [role for role in added_roles if role.id in [r.id for r in admin_roles]]
    if not added_admin_roles:
        log_action("No admin roles were added, skipping", 
                  guild_id=after.guild.id, 
                  member_id=after.id)
        return
        
    log_action("Admin roles added", 
              guild_id=after.guild.id, 
              member_id=after.id, 
              admin_roles=[r.name for r in added_admin_roles])

    # get the audit log entry for this role change
    log_action("Checking audit logs for role changes", 
              guild_id=after.guild.id, 
              member_id=after.id)
    
    try:
        async for entry in after.guild.audit_logs(
            action=discord.AuditLogAction.member_role_update,
            limit=10
        ):
            log_action("Audit log entry found", 
                      guild_id=after.guild.id,
                      entry_id=entry.id,
                      action=entry.action,
                      target_id=entry.target.id,
                      user_id=entry.user.id)
            
            # skip if not the same member
            if entry.target.id != after.id:
                log_action("Skipping entry - different target", 
                          guild_id=after.guild.id,
                          entry_id=entry.id,
                          target_id=entry.target.id,
                          expected_target=after.id)
                continue

            # get the roles that were added in this audit log entry
            entry_role_ids = [r.id for r in entry.after.roles if r not in entry.before.roles]
            log_action("Roles added in audit log", 
                      guild_id=after.guild.id,
                      entry_id=entry.id,
                      added_role_ids=entry_role_ids)

            # skip if not a role we care about
            if not any(role_id in [r.id for r in added_roles] for role_id in entry_role_ids):
                log_action("No matching admin roles in audit entry", 
                          guild_id=after.guild.id,
                          entry_id=entry.id)
                continue

            actor = entry.user
            log_action("Found actor who made the change", 
                      guild_id=after.guild.id,
                      actor_id=actor.id,
                      actor_name=str(actor))

            # skip if the actor is the server owner
            if actor.id == after.guild.owner_id:
                return

            # get the admin role that was assigned
            admin_role = next((role for role in added_roles if role.id in [r.id for r in admin_roles]), None)
            if not admin_role:
                log_action("No admin role found in added roles, this shouldn't happen", 
                          level='error',
                          guild_id=after.guild.id)
                return
                    
            log_action("Processing admin role assignment requiring approval", 
                      guild_id=after.guild.id,
                      actor_id=actor.id,
                      target_id=after.id,
                      role_id=admin_role.id,
                      role_name=admin_role.name)

            # store the assigner's current admin roles that need to be restored after approval
            assigner_admin_roles = [role for role in actor.roles if role.id in [r.id for r in admin_roles]]
            assigner_admin_role_ids = [str(r.id) for r in assigner_admin_roles]
            
            # store the role that needs to be assigned to the target
            target_role_id = admin_role.id

            # check for existing pending or approved assignment first
            existing_assignment = db.execute_query(
                """
                SELECT id, status FROM pending_admin_assignments 
                WHERE guild_id = ? AND assignee_id = ? AND role_id = ?
                """,
                (after.guild.id, after.id, target_role_id),
                fetch=True
            )
            
            if existing_assignment:
                # convert sqlite row to dict if needed
                assignment = existing_assignment[0]
                if hasattr(assignment, '_asdict'):
                    assignment = assignment._asdict()
                elif hasattr(assignment, 'keys') and callable(assignment.keys):
                    assignment = {k: assignment[k] for k in assignment.keys()}
                
                assignment_id = assignment.get('id')
                
                # if already approved, ensure the role is assigned
                if assignment.get('status') == 'approved':
                    log_action("Role assignment already approved, ensuring role is assigned",
                              guild_id=after.guild.id,
                              assignment_id=assignment_id,
                              role_id=target_role_id)
                    
                    # check if the role is already assigned
                    if admin_role not in after.roles:
                        try:
                            await after.add_roles(admin_role, reason="Approved admin role assignment")
                            log_action("Assigned previously approved admin role",
                                      guild_id=after.guild.id,
                                      member_id=after.id,
                                      role_id=target_role_id)
                        except Exception as e:
                            log_action("Failed to assign approved admin role",
                                      level='error',
                                      guild_id=after.guild.id,
                                      member_id=after.id,
                                      role_id=target_role_id,
                                      error=str(e))
                    return
                    
                # if pending, update the existing record
                log_action("Updating existing pending assignment record", 
                          guild_id=after.guild.id,
                          assignment_id=assignment_id,
                          actor_id=actor.id,
                          target_id=after.id,
                          role_id=target_role_id)
                
                try:
                    db.execute_query(
                        """
                        UPDATE pending_admin_assignments 
                        SET assigner_id = ?, 
                            assigner_roles = ?, 
                            updated_at = CURRENT_TIMESTAMP,
                            status = 'pending'
                        WHERE id = ?
                        """,
                        (actor.id, ','.join(assigner_admin_role_ids), assignment_id),
                        commit=True
                    )
                    log_action("Updated existing pending assignment record",
                              guild_id=after.guild.id,
                              assignment_id=assignment_id)
                except Exception as e:
                    log_action("Error updating pending assignment record",
                              level='error',
                              guild_id=after.guild.id,
                              error=str(e))
                    return
            else:
                # create new pending assignment record
                log_action("Creating new pending admin assignment record", 
                          guild_id=after.guild.id,
                          actor_id=actor.id,
                          target_id=after.id,
                          role_id=target_role_id)
                
                try:
                    # first, check one more time to avoid race conditions
                    existing = db.execute_query(
                        """
                        SELECT id FROM pending_admin_assignments 
                        WHERE guild_id = ? AND assignee_id = ? AND role_id = ?
                        """,
                        (after.guild.id, after.id, target_role_id),
                        fetch=True
                    )
                    
                    if existing:
                        # convert sqlite row to dict if needed
                        existing_assignment = existing[0]
                        if hasattr(existing_assignment, '_asdict'):
                            existing_assignment = existing_assignment._asdict()
                        elif hasattr(existing_assignment, 'keys') and callable(existing_assignment.keys):
                            existing_assignment = {k: existing_assignment[k] for k in existing_assignment.keys()}
                            
                        assignment_id = existing_assignment.get('id')
                        log_action("Found existing assignment during creation, using it",
                                  guild_id=after.guild.id,
                                  assignment_id=assignment_id)
                    else:
                        # insert the new record
                        db.execute_query(
                            """
                            INSERT INTO pending_admin_assignments 
                            (guild_id, assigner_id, assignee_id, role_id, status, assigner_roles, created_at, updated_at)
                            VALUES (?, ?, ?, ?, 'pending', ?, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
                            """,
                            (after.guild.id, actor.id, after.id, target_role_id, ','.join(assigner_admin_role_ids)),
                            commit=True
                        )
                        
                        # get the last inserted row id
                        result = db.execute_query(
                            "SELECT last_insert_rowid() as id",
                            fetch=True
                        )
                        if result:
                            # convert sqlite row to dict if needed
                            row = result[0]
                            if hasattr(row, '_asdict'):
                                row = row._asdict()
                            elif hasattr(row, 'keys') and callable(row.keys):
                                row = {k: row[k] for k in row.keys()}
                            assignment_id = row.get('id')
                        else:
                            assignment_id = None
                        
                        if not assignment_id:
                            raise Exception("Failed to get new assignment ID")
                            
                        log_action("Created new pending assignment",
                                 guild_id=after.guild.id,
                                 assignment_id=assignment_id)
                        
                except Exception as e:
                    log_action("Error in pending assignment creation", 
                              level='error',
                              guild_id=after.guild.id,
                              error=str(e))
                    return
            
            # remove the admin role from the target (if we can)
            if admin_role in after.roles:
                try:
                    await after.remove_roles(admin_role, reason="Pending admin approval")
                    log_action("Temporarily removed admin role from assignee",
                              guild_id=after.guild.id,
                              target_id=after.id,
                              role_id=admin_role.id)
                except Exception as e:
                    log_action("Could not remove admin role from assignee, will handle after approval",
                              level='warning',
                              guild_id=after.guild.id,
                              target_id=after.id,
                              role_id=admin_role.id,
                              error=str(e))
            
            log_action("Created/updated pending assignment record", 
                      guild_id=after.guild.id,
                      assignment_id=assignment_id)
                
            # handle any errors that might have occurred during the process
            if not assignment_id:
                log_action("Failed to get valid assignment ID", 
                          level='error',
                          guild_id=after.guild.id)
                return

            view = AdminApprovalView(assignment_id, actor.id, after.id, admin_role.id, after.guild.id)
        
            # format the message with safe role display that won't ping
            role_display = f"`{admin_role.name}` (ID: {admin_role.id})"
            
            embed = discord.Embed(
                title="⚠️ Admin Role Assignment Request",
                description=(
                    f"{actor.mention} (`{actor.id}`) is requesting to assign the admin role "
                    f"**{admin_role.name}** to {after.mention} (`{after.id}`).\n\n"
                    "**Action Required:** Please review and confirm or deny this assignment."
                ),
                color=discord.Color.orange(),
                timestamp=datetime.datetime.utcnow()
            )
            
            # add more details to the embed
            embed.add_field(name="Server", value=f"{after.guild.name} (ID: {after.guild.id})", inline=False)
            embed.add_field(name="Role to be Assigned", value=role_display, inline=False)
            embed.add_field(name="Requested By", value=f"{actor.mention} (ID: {actor.id})", inline=True)
            embed.add_field(name="Role Recipient", value=f"{after.mention} (ID: {after.id})", inline=True)
            
            # add timestamp and id
            embed.set_footer(text=f"Assignment ID: {assignment_id} • {datetime.datetime.utcnow().strftime('%Y-%m-%d %H:%M:%S UTC')}")

            # send the approval request to the server owner
            owner = after.guild.owner
            if not owner:
                log_action("Could not find server owner", 
                          level='error',
                          guild_id=after.guild.id)
                return
                
            log_action("Sending approval request to server owner", 
                      guild_id=after.guild.id,
                      owner_id=owner.id)
            
            try:
                view = AdminApprovalView(assignment_id, actor.id, after.id, admin_role.id, after.guild.id)
                await owner.send(embed=embed, view=view)
                log_action("Successfully sent approval request to server owner", 
                          guild_id=after.guild.id,
                          owner_id=owner.id)
                
                # notify the channel where the role was assigned
                try:
                    channel = after.guild.system_channel or next(
                        (channel for channel in after.guild.text_channels 
                         if channel.permissions_for(after.guild.me).send_messages),
                        None
                    )
                    if channel:
                        await channel.send(
                            f"{owner.mention}, an admin role assignment requires your approval. "
                            f"Please check your DMs.",
                            delete_after=60
                        )
                        log_action("Sent notification to channel", 
                                  guild_id=after.guild.id,
                                  channel_id=channel.id)
                except Exception as e:
                    log_action("Failed to send notification to channel", 
                              level='error',
                              guild_id=after.guild.id,
                              error=str(e))
                    
            except Exception as e:
                log_action("Failed to send DM to server owner", 
                          level='error',
                          guild_id=after.guild.id,
                          owner_id=owner.id,
                          error=str(e))
                return  # exit after handling the error

            break  # only process the most recent relevant audit log entry
    except Exception as e:
        log_action("Error processing audit logs", 
                  level='error',
                  guild_id=after.guild.id,
                  error=str(e))

def set_ignore_next_role_update(ignore: bool = True):
    """set whether to ignore the next role update."""
    global _ignore_next_role_update
    _ignore_next_role_update = ignore
    log_action(f"Set ignore_next_role_update to {ignore}")

async def setup(bot):
    """set up admin security handlers."""
    # add the event listener
    bot.add_listener(on_member_update)
    # add the set_ignore_next_role_update function to the bot for easy access
    bot.set_ignore_next_role_update = set_ignore_next_role_update
    log_action("Admin security handlers have been set up")
    return True
