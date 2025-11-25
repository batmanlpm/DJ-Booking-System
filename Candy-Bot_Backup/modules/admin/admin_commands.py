"""
admin commands for the bot.
"""
import discord
from discord import app_commands, ui
from discord.ext import commands
from typing import Optional, List
import datetime
import logging
from modules.database import db
from modules.security.settings import AdminSecuritySettings
from modules.admin.anti_raid_ui import AntiRaidSettingsView

# set up logging
logger = logging.getLogger(__name__)

class AddRoleSelect(ui.RoleSelect):
    """dropdown for selecting a role to add as admin."""
    def __init__(self, bot):
        logger.debug(f"Initializing AddRoleSelect with bot: {bot}")
        self.bot = bot
        super().__init__(
            placeholder="Select a role to add as admin...",
            min_values=1,
            max_values=1
        )
    
    async def callback(self, interaction: discord.Interaction):
        logger.debug("AddRoleSelect callback triggered")
        try:
            if not interaction.response.is_done():
                await interaction.response.defer(ephemeral=True)
            
            if not self.values:
                logger.warning("No role selected in values")
                await interaction.followup.send("❌ No role selected. Please try again.", ephemeral=True)
                return
                
            role = self.values[0]
            logger.debug(f"Selected role: {role.name} (ID: {role.id})")
            
            try:
                # check if role is already an admin
                logger.debug("Checking if role is already an admin")
                existing = db.execute_query(
                    "SELECT 1 FROM bot_admins WHERE role_id = ? AND guild_id = ?",
                    (role.id, interaction.guild.id),
                    fetch=True
                )
                logger.debug(f"Existing admin check result: {existing}")
                
                if existing:
                    logger.info(f"Role {role.name} (ID: {role.id}) is already an admin")
                    await interaction.followup.send(
                        f"❌ {role.mention} is already an admin role.",
                        ephemeral=True
                    )
                    return
                
                try:
                    logger.debug(f"Verifying guild {interaction.guild.id} and user {interaction.user.id} exist in database")
                    
                    # first, ensure the guild exists in the guilds table
                    logger.debug("Ensuring guild exists in guilds table")
                    guild_result = db.execute_query(
                        """
                        INSERT OR IGNORE INTO guilds (guild_id, owner_id)
                        VALUES (?, ?)
                        """,
                        (interaction.guild.id, interaction.guild.owner_id),
                        commit=True
                    )
                    logger.debug(f"Guild insert result: {guild_result}")
                    
                    # ensure the user exists in the users table
                    logger.debug("Ensuring user exists in users table")
                    user_result = db.execute_query(
                        """
                        INSERT OR IGNORE INTO users (user_id, timezone)
                        VALUES (?, ?)
                        """,
                        (interaction.user.id, 'UTC'),  # Default timezone to UTC
                        commit=True
                    )
                    logger.debug(f"User insert result: {user_result}")
                    
                    # verify the guild exists
                    guild_check = db.execute_query(
                        "SELECT 1 FROM guilds WHERE guild_id = ?",
                        (interaction.guild.id,),
                        fetch=True
                    )
                    logger.debug(f"Guild check result: {guild_check}")
                    
                    # verify the user exists
                    user_check = db.execute_query(
                        "SELECT 1 FROM users WHERE user_id = ?",
                        (interaction.user.id,),
                        fetch=True
                    )
                    logger.debug(f"User check result: {user_check}")
                    
                    logger.info(f"Adding role {role.name} (ID: {role.id}) as admin in guild {interaction.guild.name} (ID: {interaction.guild.id})")
                    # add to database
                    result = db.execute_query(
                        """
                        INSERT INTO bot_admins (role_id, added_by, added_at, guild_id)
                        VALUES (?, ?, ?, ?)
                        """,
                        (role.id, interaction.user.id, datetime.datetime.utcnow().isoformat(), interaction.guild.id),
                        commit=True
                    )
                    logger.debug(f"Role insert result: {result}")
                    
                except Exception as e:
                    logger.error(f"Error adding admin role: {str(e)}", exc_info=True)
                    await interaction.followup.send(
                        f"❌ An error occurred while adding the admin role: {e}",
                        ephemeral=True
                    )
                    return
                
                await interaction.followup.send(
                    f"✅ Added {role.mention} as an admin role.",
                    ephemeral=True
                )
                logger.info(f"Successfully added role {role.name} (ID: {role.id}) as admin in guild {interaction.guild.name} (ID: {interaction.guild.id})")
                
            except Exception as e:
                logger.error(f"Error in AddRoleSelect callback (database/response): {str(e)}", exc_info=True)
                if not interaction.response.is_done():
                    await interaction.response.send_message(
                        "❌ An error occurred while adding the admin role. Please try again.",
                        ephemeral=True
                    )
                else:
                    await interaction.followup.send(
                        "❌ An error occurred while adding the admin role. Please try again.",
                        ephemeral=True
                    )
                    
        except Exception as e:
            logger.critical(f"Unhandled exception in AddRoleSelect callback: {str(e)}", exc_info=True)
            if not interaction.response.is_done():
                await interaction.response.send_message(
                    "❌ A critical error occurred. Please check the logs.",
                    ephemeral=True
                )
            else:
                await interaction.followup.send(
                    "❌ A critical error occurred. Please check the logs.",
                    ephemeral=True
                )


class RemoveRoleSelect(ui.RoleSelect):
    """dropdown for selecting a role to remove from admins."""
    def __init__(self, bot):
        logger.debug(f"Initializing RemoveRoleSelect with bot: {bot}")
        self.bot = bot
        super().__init__(
            placeholder="Select a role to remove from admins...",
            min_values=1,
            max_values=1
        )
    
    async def callback(self, interaction: discord.Interaction):
        logger.debug("RemoveRoleSelect callback triggered")
        try:
            if not interaction.response.is_done():
                await interaction.response.defer(ephemeral=True)
            
            if not self.values:
                logger.warning("No role selected in values")
                await interaction.followup.send("❌ No role selected. Please try again.", ephemeral=True)
                return
                
            role = self.values[0]
            logger.debug(f"Selected role to remove: {role.name} (ID: {role.id})")
            
            # remove from database
            try:
                result = db.execute_query(
                    "DELETE FROM bot_admins WHERE role_id = ? AND guild_id = ?",
                    (role.id, interaction.guild.id),
                    commit=True
                )
                logger.debug(f"Database delete result: {result}")
                
                await interaction.followup.send(
                    f"✅ Removed {role.mention} from admin roles.",
                    ephemeral=True
                )
                logger.info(f"Removed role {role.name} (ID: {role.id}) from admin roles in guild {interaction.guild.name} (ID: {interaction.guild.id})")
                
            except Exception as e:
                logger.error(f"Error removing admin role: {str(e)}", exc_info=True)
                await interaction.followup.send(
                    "❌ An error occurred while removing the admin role. Please try again.",
                    ephemeral=True
                )
                    
        except Exception as e:
            logger.critical(f"Unhandled exception in RemoveRoleSelect callback: {str(e)}", exc_info=True)
            if not interaction.response.is_done():
                await interaction.response.send_message(
                    "❌ A critical error occurred. Please check the logs.",
                    ephemeral=True
                )
            else:
                await interaction.followup.send(
                    "❌ A critical error occurred. Please check the logs.",
                    ephemeral=True
                )


class AddRoleButton(ui.Button):
    """button to show the add role dropdown."""
    def __init__(self, bot=None):
        logger.debug(f"Initializing AddRoleButton with bot: {bot}")
        super().__init__(
            label="Add Admin Role",
            style=discord.ButtonStyle.gray
        )
        self.bot = bot
    
    async def callback(self, interaction: discord.Interaction):
        logger.debug("AddRoleButton callback triggered")
        try:
            # check if user is server owner
            logger.debug("Checking if user is server owner")
            is_owner = await AdminManagementView.is_server_owner(interaction)
            logger.debug(f"Is server owner: {is_owner}")
            
            if not is_owner:
                logger.warning("User is not server owner")
                if not interaction.response.is_done():
                    await interaction.response.send_message(
                        "❌ Only the server owner can manage admins.",
                        ephemeral=True
                    )
                else:
                    await interaction.followup.send(
                        "❌ Only the server owner can manage admins.",
                        ephemeral=True
                    )
                return
            
            logger.debug("Creating role select view")
            view = ui.View(timeout=300)  # 5 minute timeout
            
            # get the bot instance from the view or interaction
            bot = getattr(self, 'bot', None) or getattr(self.view, 'bot', None) or interaction.client
            logger.debug(f"Using bot instance: {bot}")
            
            if not bot:
                logger.error("No bot instance available")
                if not interaction.response.is_done():
                    await interaction.response.send_message(
                        "❌ Bot instance not available. Please try again.",
                        ephemeral=True
                    )
                else:
                    await interaction.followup.send(
                        "❌ Bot instance not available. Please try again.",
                        ephemeral=True
                    )
                return
            
            view.add_item(AddRoleSelect(bot))
            
            logger.debug("Sending role select message")
            if not interaction.response.is_done():
                await interaction.response.send_message(
                    "Select a role to add as admin:",
                    view=view,
                    ephemeral=True
                )
            else:
                await interaction.followup.send(
                    "Select a role to add as admin:",
                    view=view,
                    ephemeral=True
                )
                
            logger.debug("Role select message sent")
                
        except Exception as e:
            logger.error(f"Error in AddRoleButton callback: {str(e)}", exc_info=True)
            if not interaction.response.is_done():
                await interaction.response.send_message(
                    "❌ An error occurred. Please try again.",
                    ephemeral=True
                )
            else:
                await interaction.followup.send(
                    "❌ An error occurred. Please try again.",
                    ephemeral=True
                )


class RemoveRoleButton(ui.Button):
    """button to show the remove role dropdown."""
    def __init__(self, bot=None):
        logger.debug(f"Initializing RemoveRoleButton with bot: {bot}")
        super().__init__(
            label="Remove Admin Role",
            style=discord.ButtonStyle.gray
        )
        self.bot = bot
    
    async def callback(self, interaction: discord.Interaction):
        logger.debug("RemoveRoleButton callback triggered")
        try:
            # check if user is server owner
            logger.debug("Checking if user is server owner")
            is_owner = await AdminManagementView.is_server_owner(interaction)
            logger.debug(f"Is server owner: {is_owner}")
            
            if not is_owner:
                logger.warning("User is not server owner")
                if not interaction.response.is_done():
                    await interaction.response.send_message(
                        "❌ Only the server owner can manage admins.",
                        ephemeral=True
                    )
                else:
                    await interaction.followup.send(
                        "❌ Only the server owner can manage admins.",
                        ephemeral=True
                    )
                return
            
            logger.debug("Fetching admin roles from database")
            admin_roles = db.execute_query(
                "SELECT role_id FROM bot_admins WHERE role_id IS NOT NULL AND guild_id = ?",
                (interaction.guild.id,),
                fetch=True
            )
            logger.debug(f"Found {len(admin_roles)} admin roles in database")
            
            if not admin_roles:
                logger.info("No admin roles to remove")
                if not interaction.response.is_done():
                    await interaction.response.send_message(
                        "❌ There are no admin roles to remove.",
                        ephemeral=True
                    )
                else:
                    await interaction.followup.send(
                        "❌ There are no admin roles to remove.",
                        ephemeral=True
                    )
                return
            
            # get role objects that still exist in the guild
            guild_roles = interaction.guild.roles
            valid_roles = [role for role in guild_roles if str(role.id) in [str(r['role_id']) for r in admin_roles]]
            
            if not valid_roles:
                logger.info("No valid admin roles found in the server")
                if not interaction.response.is_done():
                    await interaction.response.send_message(
                        "❌ No valid admin roles found in this server.",
                        ephemeral=True
                    )
                else:
                    await interaction.followup.send(
                        "❌ No valid admin roles found in this server.",
                        ephemeral=True
                    )
                return
            
            logger.debug(f"Found {len(valid_roles)} valid admin roles to show in dropdown")
            
            # create a view for the select menu
            view = ui.View(timeout=300)
            
            # create a select menu with the admin roles
            class AdminRoleSelect(ui.Select):
                def __init__(self, roles):
                    options = [
                        discord.SelectOption(
                            label=role.name,
                            value=str(role.id),
                            description=f"Remove {role.name} from admin roles"
                        ) for role in roles
                    ]
                    
                    super().__init__(
                        placeholder="Select a role to remove from admins...",
                        min_values=1,
                        max_values=1,
                        options=options,
                        custom_id="admin_role_select"
                    )
                    self.role_map = {str(role.id): role for role in roles}
                
                async def callback(self, select_interaction: discord.Interaction):
                    if not select_interaction.response.is_done():
                        await select_interaction.response.defer(ephemeral=True)
                    
                    selected_role_id = self.values[0]
                    selected_role = self.role_map.get(selected_role_id)
                    
                    if not selected_role:
                        await select_interaction.followup.send(
                            "❌ Invalid role selected. Please try again.",
                            ephemeral=True
                        )
                        return
                    
                    # remove from database
                    try:
                        result = db.execute_query(
                            "DELETE FROM bot_admins WHERE role_id = ? AND guild_id = ?",
                            (selected_role.id, select_interaction.guild.id),
                            commit=True
                        )
                        logger.debug(f"Role {selected_role.name} removed from admins")
                        
                        await select_interaction.followup.send(
                            f"✅ Removed {selected_role.mention} from admin roles.",
                            ephemeral=True
                        )
                        
                        # disable the select after successful removal
                        self.disabled = True
                        try:
                            await select_interaction.message.edit(view=self.view)
                        except discord.NotFound:
                            # message was already deleted, which is fine
                            pass
                        
                    except Exception as e:
                        logger.error(f"Error removing role from admins: {str(e)}", exc_info=True)
                        await select_interaction.followup.send(
                            "❌ An error occurred while removing the admin role. Please try again.",
                            ephemeral=True
                        )
            
            # add the select menu to the view
            view.add_item(AdminRoleSelect(valid_roles))
            
            # send the message with the select menu
            if not interaction.response.is_done():
                await interaction.response.send_message(
                    "Select a role to remove from admins:",
                    view=view,
                    ephemeral=True
                )
            else:
                await interaction.followup.send(
                    "Select a role to remove from admins:",
                    view=view,
                    ephemeral=True
                )
            
            logger.debug("Remove role select menu sent")
                
        except Exception as e:
            logger.error(f"Error in RemoveRoleButton callback: {str(e)}", exc_info=True)
            if not interaction.response.is_done():
                await interaction.response.send_message(
                    "❌ An error occurred. Please try again.",
                    ephemeral=True
                )
            else:
                await interaction.followup.send(
                    "❌ An error occurred. Please try again.",
                    ephemeral=True
                )


class ListAdminsButton(ui.Button):
    """button to list all admin roles."""
    def __init__(self, bot=None):
        super().__init__(
            label="List Admin Roles",
            style=discord.ButtonStyle.gray
        )
        self.bot = bot
    
    async def callback(self, interaction: discord.Interaction):
        logger.debug("ListAdminsButton callback triggered")
        try:
            if not interaction.response.is_done():
                await interaction.response.defer(ephemeral=True)
                
            logger.debug("Fetching admin roles from database")
            # get role-based admins with properly formatted timestamps
            try:
                # get role-based admins with properly formatted timestamps
                role_admins = db.execute_query(
                    """
                    SELECT 
                        role_id, 
                        added_by, 
                        strftime('%Y-%m-%d %H:%M:%S', added_at) as added_at
                    FROM bot_admins 
                    WHERE role_id IS NOT NULL AND guild_id = ?
                    """,
                    (interaction.guild.id,),
                    fetch=True
                )
                logger.debug(f"Found {len(role_admins)} admin roles")
                
                if not role_admins:
                    logger.info("No admin roles found")
                    await interaction.followup.send(
                        "No admin roles have been set up yet.",
                        ephemeral=True
                    )
                    return
            except Exception as e:
                logger.error(f"Error fetching admin roles: {e}", exc_info=True)
                await interaction.followup.send(
                    "❌ An error occurred while fetching admin roles. Please try again.",
                    ephemeral=True
                )
                return
                
            embed = discord.Embed(
                title="🛡️ Admin Roles",
                description="List of roles with admin privileges:",
                color=discord.Color.blue()
            )
            
            for admin in role_admins:
                role = interaction.guild.get_role(admin[0])
                added_by = interaction.guild.get_member(admin[1])
                added_at = admin[2]
                
                if role:
                    embed.add_field(
                        name=role.name,
                        value=f"Added by: {added_by.mention if added_by else 'Unknown'}\n"
                              f"Added at: {added_at}",
                        inline=False
                    )
            
            if not embed.fields:
                await interaction.followup.send(
                    "No valid admin roles found. They might have been deleted.",
                    ephemeral=True
                )
                return
                
            logger.debug("Sending admin roles list")
            await interaction.followup.send(embed=embed, ephemeral=True)
            
        except Exception as e:
            logger.error(f"Error in ListAdminsButton callback: {str(e)}", exc_info=True)
            if not interaction.response.is_done():
                await interaction.response.send_message(
                    "❌ An error occurred while listing admin roles. Please try again.",
                    ephemeral=True
                )
            else:
                await interaction.followup.send(
                    "❌ An error occurred while listing admin roles. Please try again.",
                    ephemeral=True
                )


class SecurityToggleButton(ui.Button):
    """button to toggle admin security settings."""
    def __init__(self, bot=None):
        self.bot = bot
        super().__init__(
            label="Security: Loading...",
            style=discord.ButtonStyle.secondary
        )
    
    async def update_label(self, interaction: discord.Interaction):
        """update the button label based on current security setting."""
        is_enabled = AdminSecuritySettings.is_security_enabled(interaction.guild_id)
        self.style = discord.ButtonStyle.green if is_enabled else discord.ButtonStyle.red
        self.label = f"Security: {'Enabled' if is_enabled else 'Disabled'}"
        
        # update the message if it exists
        if hasattr(self.view, 'message') and self.view.message:
            await interaction.response.edit_message(view=self.view)
    
    async def callback(self, interaction: discord.Interaction):
        """handle the button click to toggle security settings."""
        # check if user is server owner
        is_owner = await AdminManagementView.is_server_owner(interaction)
        if not is_owner:
            logger.warning(f"Non-owner {interaction.user} attempted to modify security settings")
            if not interaction.response.is_done():
                await interaction.response.send_message(
                    "❌ Only the server owner can manage security settings.",
                    ephemeral=True
                )
            else:
                await interaction.followup.send(
                    "❌ Only the server owner can manage security settings.",
                    ephemeral=True
                )
            return
        
        # toggle the security setting
        current_setting = AdminSecuritySettings.is_security_enabled(interaction.guild_id)
        new_setting = not current_setting
        AdminSecuritySettings.set_security_enabled(interaction.guild_id, new_setting)
        
        # update the button appearance
        await self.update_label(interaction)
        
        # send confirmation
        if not interaction.response.is_done():
            await interaction.response.defer(ephemeral=True)
        
        await interaction.followup.send(
            f"✅ Admin security is now **{'Enabled' if new_setting else 'Disabled'}**."
            f"\n\nWhen disabled, admin roles can be assigned without approval.",
            ephemeral=True
        )
        logger.info(f"Security settings toggled to {new_setting} in guild {interaction.guild.name} (ID: {interaction.guild.id})")


class ActionsSecurityToggleButton(ui.Button):
    """button to toggle actions security settings."""
    def __init__(self, bot=None):
        self.bot = bot
        super().__init__(
            label="Actions Security: Loading...",
            style=discord.ButtonStyle.secondary
        )
    
    async def update_label(self, interaction: discord.Interaction):
        """update the button label based on current security setting."""
        is_enabled = AdminSecuritySettings.is_actions_security_enabled(interaction.guild_id)
        self.style = discord.ButtonStyle.green if is_enabled else discord.ButtonStyle.red
        self.label = f"Actions Security: {'Enabled' if is_enabled else 'Disabled'}"
        
        # update the message if it exists
        if hasattr(self.view, 'message') and self.view.message:
            await interaction.response.edit_message(view=self.view)
    
    async def callback(self, interaction: discord.Interaction):
        """ handle the button click to toggle actions security settings."""
        # check if user is server owner
        is_owner = await AdminManagementView.is_server_owner(interaction)
        if not is_owner:
            logger.warning(f"Non-owner {interaction.user} attempted to modify actions security settings")
            if not interaction.response.is_done():
                await interaction.response.send_message(
                    "❌ Only the server owner can manage security settings.",
                    ephemeral=True
                )
            else:
                await interaction.followup.send(
                    "❌ Only the server owner can manage security settings.",
                    ephemeral=True
                )
            return
        
        # toggle the security setting
        current_setting = AdminSecuritySettings.is_actions_security_enabled(interaction.guild_id)
        new_setting = not current_setting
        AdminSecuritySettings.set_actions_security_enabled(interaction.guild_id, new_setting)
        
        # update the button appearance
        await self.update_label(interaction)
        
        # send confirmation
        if not interaction.response.is_done():
            await interaction.response.defer(ephemeral=True)
        
        await interaction.followup.send(
            f"✅ Actions security is now **{'Enabled' if new_setting else 'Disabled'}**."
            f"\n\nWhen disabled, admin actions (like channel deletion) won't trigger security measures.",
            ephemeral=True
        )
        logger.info(f"Actions security settings toggled to {new_setting} in guild {interaction.guild.name} (ID: {interaction.guild.id})")


class AntiRaidSettingsButton(ui.Button):
    """button to open anti-raid settings."""
    def __init__(self, bot=None):
        super().__init__(
            label="Anti-Raid Settings",
            style=discord.ButtonStyle.gray
        )
        self.bot = bot
    
    async def callback(self, interaction: discord.Interaction):
        """handle button click."""
        try:
            logger.debug("AntiRaidSettingsButton callback triggered")
            
            # check if user is server owner
            is_owner = await AdminManagementView.is_server_owner(interaction)
            if not is_owner:
                logger.warning(f"Non-owner {interaction.user} attempted to access anti-raid settings")
                if not interaction.response.is_done():
                    await interaction.response.send_message(
                        "❌ Only the server owner can access anti-raid settings.",
                        ephemeral=True
                    )
                else:
                    await interaction.followup.send(
                        "❌ Only the server owner can access anti-raid settings.",
                        ephemeral=True
                    )
                return
                
            # create the anti-raid settings view
            view = AntiRaidSettingsView(self.bot, interaction.guild.id)
            
            # send the initial message with the view
            message_content = (
                "⚙️ **Anti-Raid Settings**\n\n"
                "Configure the bot's anti-raid protection settings below."
            )
            
            if not interaction.response.is_done():
                await interaction.response.send_message(
                    message_content,
                    view=view,
                    ephemeral=True
                )
                view.message = await interaction.original_response()
            else:
                await interaction.followup.send(
                    message_content,
                    view=view,
                    ephemeral=True
                )
                view.message = await interaction.original_response()
                
            logger.info(f"Opened anti-raid settings for guild {interaction.guild.name} (ID: {interaction.guild.id})")
                
        except Exception as e:
            logger.error(f"Error in AntiRaidSettingsButton callback: {str(e)}", exc_info=True)
            if not interaction.response.is_done():
                await interaction.response.send_message(
                    "❌ An error occurred while opening anti-raid settings. Please try again.",
                    ephemeral=True
                )
            else:
                await interaction.followup.send(
                    "❌ An error occurred while opening anti-raid settings. Please try again.",
                    ephemeral=True
                )


class AdminManagementView(ui.View):
    """view for managing bot admins."""
    
    def __init__(self, bot, timeout: Optional[float] = 600):
        super().__init__(timeout=timeout)
        self.bot = bot
        self.message = None
        self.logger = logging.getLogger(f"{__name__}.AdminManagementView")
        
        try:
            # Row 1: Role management buttons
            self.add_item(AddRoleButton(bot=bot))
            self.add_item(RemoveRoleButton(bot=bot))
            self.add_item(ListAdminsButton(bot=bot))
            
            # Row 2: Security toggle buttons
            self.security_toggle = SecurityToggleButton(bot=bot)
            self.actions_security_toggle = ActionsSecurityToggleButton(bot=bot)
            self.security_toggle.row = 1
            self.actions_security_toggle.row = 1
            self.add_item(self.security_toggle)
            self.add_item(self.actions_security_toggle)
            
            # Row 3: Anti-raid settings button
            self.anti_raid_button = AntiRaidSettingsButton(bot=bot)
            self.anti_raid_button.row = 2
            self.add_item(self.anti_raid_button)
            
            self.logger.debug("Initialized AdminManagementView with all buttons")
            
        except Exception as e:
            self.logger.error(f"Error initializing AdminManagementView: {str(e)}", exc_info=True)
            raise

    @staticmethod
    async def is_global_admin(user_id: int) -> bool:
        """check if a user is a global admin."""
        logger = logging.getLogger(f"{__name__}.AdminManagementView.is_global_admin")
        try:
            from utils.bot_admin import is_bot_admin_check
            is_admin = await is_bot_admin_check(user_id)
            logger.debug(f"User {user_id} is global admin: {is_admin}")
            return is_admin
        except Exception as e:
            logger.error(f"Error checking global admin status: {str(e)}", exc_info=True)
            return False

    @classmethod
    async def is_server_owner(cls, interaction: discord.Interaction) -> bool:
        """check if the user is the server owner or a global admin."""
        logger = logging.getLogger(f"{__name__}.AdminManagementView.is_server_owner")
        
        try:
            # First check if user is a global admin
            if await cls.is_global_admin(interaction.user.id):
                logger.debug(f"User {interaction.user} (ID: {interaction.user.id}) is a global admin")
                return True
                
            if not interaction.guild:
                logger.warning("Interaction is not in a guild")
                return False
                
            # get fresh guild data to ensure we have the latest owner info
            try:
                guild = await interaction.client.fetch_guild(interaction.guild.id)
                is_owner = interaction.user.id == guild.owner_id
                logger.debug(f"User {interaction.user} (ID: {interaction.user.id}) is server owner: {is_owner}")
                return is_owner
            except Exception as e:
                logger.error(f"Error fetching guild data: {str(e)}", exc_info=True)
                # fallback to the cached guild data if available
                is_owner = interaction.user.id == interaction.guild.owner_id
                logger.debug(f"Using cached guild data - is owner: {is_owner}")
                return is_owner
                
        except Exception as e:
            logger.error(f"Error in is_server_owner: {str(e)}", exc_info=True)
            return False


class AdminCog(commands.Cog):
    """admin commands for the bot."""
    
    def __init__(self, bot):
        self.bot = bot
        self.logger = logging.getLogger(f"{__name__}.AdminCog")

    @app_commands.command(name="admin", description="Manage bot administrators")
    @app_commands.guild_only()
    async def admin(self, interaction: discord.Interaction):
        """display the admin management interface."""
        self.logger.info(f"Admin command invoked by {interaction.user} (ID: {interaction.user.id}) in guild {interaction.guild.name} (ID: {interaction.guild.id})")
        
        try:
            await interaction.response.defer(ephemeral=True)
            
            # check if user is server owner or global admin
            is_allowed = await AdminManagementView.is_server_owner(interaction)
            
            if not is_allowed:
                self.logger.warning(f"Unauthorized user {interaction.user} (ID: {interaction.user.id}) attempted to access admin panel")
                await interaction.followup.send(
                    "❌ You must be the server owner or a global admin to manage admins.",
                    ephemeral=True
                )
                return
            
            # create the view with the bot instance
            self.logger.debug("Creating AdminManagementView")
            view = AdminManagementView(bot=self.bot)
            
            # update the button labels with current settings
            self.logger.debug("Updating security toggle buttons")
            await view.security_toggle.update_label(interaction)
            await view.actions_security_toggle.update_label(interaction)
            
            # create the embed with dark gray color
            is_security_enabled = AdminSecuritySettings.is_security_enabled(interaction.guild_id)
            is_actions_security_enabled = AdminSecuritySettings.is_actions_security_enabled(interaction.guild_id)
            
            embed = discord.Embed(
                title="Admin Management",
                description=(
                    "### Role Management\n"
                    "• **Add Admin Role**: Grant admin permissions to a role\n"
                    "• **Remove Admin Role**: Revoke admin permissions from a role\n"
                    "• **List Admin Roles**: View all roles with admin permissions\n\n"
                    "### Security Settings\n"
                    f"• **Security**: {'Enabled' if is_security_enabled else 'Disabled'}\n"
                    "  _Requires approval for admin role assignments_\n"
                    f"• **Actions Security**: {'Enabled' if is_actions_security_enabled else 'Disabled'}\n"
                    "  _Monitors and restricts sensitive admin actions_"
                ),
                color=0x2f3136  # Dark gray color
            )
            
            self.logger.debug("Sending admin management interface")
            message = await interaction.followup.send(embed=embed, view=view, ephemeral=True)
            view.message = message
            self.logger.info("Successfully displayed admin management interface")
            
        except Exception as e:
            self.logger.error(f"Error in admin command: {str(e)}", exc_info=True)
            try:
                error_msg = "❌ An error occurred while processing your request. Please try again."
                if interaction.response.is_done():
                    await interaction.followup.send(error_msg, ephemeral=True)
                else:
                    await interaction.response.send_message(error_msg, ephemeral=True)
            except Exception as send_error:
                self.logger.error(f"Failed to send error message to user: {send_error}")
    
    @admin.error
    async def admin_error(self, interaction: discord.Interaction, error: Exception):
        """handle errors for the admin command."""
        self.logger.error(f"Error in admin command: {str(error)}", exc_info=True)
        
        if isinstance(error, app_commands.MissingPermissions):
            error_message = "❌ You don't have permission to use this command."
        else:
            error_message = "❌ An error occurred while processing your request. Please try again."
        
        try:
            if not interaction.response.is_done():
                await interaction.response.send_message(
                    error_message,
                    ephemeral=True
                )
            else:
                await interaction.followup.send(
                    error_message,
                    ephemeral=True
                )
        except Exception as e:
            self.logger.error(f"Failed to send error message to user: {e}")


async def setup(bot):
    """
    Set up the admin cog.   
    """
    try:
        await bot.add_cog(AdminCog(bot))
        logger = logging.getLogger(__name__)
        logger.info("Admin cog loaded successfully")
        return True
    except Exception as e:
        logger = logging.getLogger(__name__)
        logger.error(f"Failed to load admin cog: {str(e)}", exc_info=True)
        raise
