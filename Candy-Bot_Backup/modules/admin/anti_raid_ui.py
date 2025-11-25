"""
UI components for managing anti-raid settings in the admin pannel
"""
import discord
from discord import ui
from typing import Optional, List, Dict, Any
import json

class AntiRaidSettingsView(ui.View):
    """view for managing anti-raid settings."""
    
    def __init__(self, bot, guild_id: int, timeout: float = 180):
        super().__init__(timeout=timeout)
        self.bot = bot
        self.guild_id = guild_id
        self.message = None
        self._load_settings()
        
        # add buttons
        self.add_item(EnableAntiRaidButton(self.settings['enabled']))
        self.add_item(EditThresholdButton(self.settings['message_threshold']))
        self.add_item(EditTimeWindowButton(self.settings['time_window']))
        self.add_item(EditLockDurationButton(self.settings['lock_duration']))
        self.add_item(ManageExemptRolesButton())
        self.add_item(CloseButton())
    
    def _load_settings(self):
        """load anti-raid settings from the database."""
        from modules.database.database import db
        
        # default settings
        self.settings = {
            'enabled': True,
            'message_threshold': 10,
            'time_window': 5,
            'lock_duration': 300,
            'exempt_roles': []
        }
        
        # load from database
        try:
            result = db.execute_query(
                "SELECT * FROM anti_raid_settings WHERE guild_id = ?",
                (self.guild_id,),
                fetch=True
            )
            
            if result:
                row = result[0]
                self.settings.update({
                    'enabled': bool(row['enabled']),
                    'message_threshold': row['message_threshold'],
                    'time_window': row['time_window'],
                    'lock_duration': row['lock_duration'],
                    'exempt_roles': json.loads(row['exempt_roles'] or '[]')
                })
        except Exception as e:
            print(f"Error loading anti-raid settings: {e}")
    
    async def save_settings(self):
        """save settings to the databaase"""
        from modules.database.database import db
        
        try:
            db.execute_query(
                """
                INSERT INTO anti_raid_settings 
                (guild_id, enabled, message_threshold, time_window, lock_duration, exempt_roles)
                VALUES (?, ?, ?, ?, ?, ?)
                ON CONFLICT(guild_id) DO UPDATE SET
                    enabled = excluded.enabled,
                    message_threshold = excluded.message_threshold,
                    time_window = excluded.time_window,
                    lock_duration = excluded.lock_duration,
                    exempt_roles = excluded.exempt_roles
                """,
                (
                    self.guild_id,
                    int(self.settings['enabled']),
                    self.settings['message_threshold'],
                    self.settings['time_window'],
                    self.settings['lock_duration'],
                    json.dumps(self.settings['exempt_roles'])
                ),
                commit=True
            )
            return True
        except Exception as e:
            print(f"Error saving anti-raid settings: {e}")
            return False
    
    def get_embed(self) -> discord.Embed:
        """get the settings embed"""
        embed = discord.Embed(
            title="🛡️ Anti-Raid Settings",
            description=(
                "Configure the anti-raid system to protect your server from spam and raids.\n\n"
                f"**Status:** {'🟢 Enabled' if self.settings['enabled'] else '🔴 Disabled'}\n"
                f"**Message Threshold:** {self.settings['message_threshold']} messages\n"
                f"**Time Window:** {self.settings['time_window']} seconds\n"
                f"**Lock Duration:** {self.settings['lock_duration'] // 60} minutes\n"
                f"**Exempt Roles:** {len(self.settings['exempt_roles'])} roles"
            ),
            color=discord.Color.blue()
        )
        return embed
    
    async def update_message(self, interaction: discord.Interaction):
        """update the message with current settings"""
        try:
            # try to edit the original message
            if interaction.response.is_done():
                # if we already responded, edit the followup message
                await interaction.edit_original_response(
                    embed=self.get_embed(),
                    view=self
                )
            else:
                # if we haven't responded yet, use response.edit_message
                await interaction.response.edit_message(
                    embed=self.get_embed(),
                    view=self
                )
        except (discord.NotFound, discord.HTTPException) as e:
            # if the message was deleted or we can't edit it, send a new one
            try:
                await interaction.followup.send(
                    "Here's an updated view of your settings:",
                    embed=self.get_embed(),
                    view=self,
                    ephemeral=True
                )
            except Exception as e:
                # if all else fails, just send a simple message
                await interaction.followup.send(
                    "❌ Failed to update the settings view. Please try again.",
                    ephemeral=True
                )

class EnableAntiRaidButton(ui.Button):
    """button to enable/disable anti-raid"""
    
    def __init__(self, enabled: bool):
        super().__init__(
            style=discord.ButtonStyle.green if enabled else discord.ButtonStyle.red,
            label=f"Anti-Raid: {'ON' if enabled else 'OFF'}"
        )
    
    async def callback(self, interaction: discord.Interaction):
        """handle button click"""
        view: AntiRaidSettingsView = self.view
        view.settings['enabled'] = not view.settings['enabled']
        await view.save_settings()
        
        # update button appearance
        self.style = discord.ButtonStyle.green if view.settings['enabled'] else discord.ButtonStyle.red
        self.label = f"Anti-Raid: {'ON' if view.settings['enabled'] else 'OFF'}"
        self.emoji = "✅" if view.settings['enabled'] else "❌"
        
        await view.update_message(interaction)

class EditThresholdButton(ui.Button):
    """button to edit message threshold"""
    
    def __init__(self, threshold: int):
        self.threshold = threshold
        super().__init__(
            style=discord.ButtonStyle.primary,
            label=f"Threshold: {threshold} msgs"
        )
    
    def update_label(self, threshold: int):
        """update the button label with new threshold value"""
        self.threshold = threshold
        self.label = f"Threshold: {threshold} msgs"
    
    async def callback(self, interaction: discord.Interaction):
        """handle button click"""
        view: AntiRaidSettingsView = self.view
        modal = EditThresholdModal(view)
        await interaction.response.send_modal(modal)

class EditTimeWindowButton(ui.Button):
    """button to edit time window"""
    
    def __init__(self, time_window: int):
        self.time_window = time_window
        super().__init__(
            style=discord.ButtonStyle.primary,
            label=f"Time Window: {time_window}s"
        )
    
    def update_label(self, time_window: int):
        """update the button label with new time window value"""
        self.time_window = time_window
        self.label = f"Time Window: {time_window}s"
    
    async def callback(self, interaction: discord.Interaction):
        """handle button click"""
        view: AntiRaidSettingsView = self.view
        modal = EditTimeWindowModal(view)
        await interaction.response.send_modal(modal)

class EditLockDurationButton(ui.Button):
    """button to edit lock duration"""
    
    def __init__(self, lock_duration: int):
        self.minutes = lock_duration // 60
        super().__init__(
            style=discord.ButtonStyle.primary,
            label=f"Lock Duration: {self.minutes}m"
        )
    
    def update_label(self, lock_duration: int):
        """update the button label with new lock duration"""
        self.minutes = lock_duration // 60
        self.label = f"Lock Duration: {self.minutes}m"
    
    async def callback(self, interaction: discord.Interaction):
        """handle button click"""
        view: AntiRaidSettingsView = self.view
        modal = EditLockDurationModal(view)
        await interaction.response.send_modal(modal)

class ManageExemptRolesButton(ui.Button):
    """button to manage exempt roles"""
    
    def __init__(self):
        super().__init__(
            style=discord.ButtonStyle.secondary,
            label="Manage Exempt Roles"
        )
    
    async def callback(self, interaction: discord.Interaction):
        """handle button click"""
        view: AntiRaidSettingsView = self.view
        await interaction.response.send_message(
            "Please mention the roles you want to exempt from anti-raid measures.",
            view=ExemptRolesView(view),
            ephemeral=True
        )

class CloseButton(ui.Button):
    """button to close the settings"""
    
    def __init__(self):
        super().__init__(
            style=discord.ButtonStyle.danger,
            label="Close"
        )
    
    async def callback(self, interaction: discord.Interaction):
        """handle button click"""
        await interaction.response.defer()
        await interaction.delete_original_response()

class EditThresholdModal(ui.Modal, title="Edit Message Threshold"):
    """modal for editing message threshold"""
    
    def __init__(self, view: AntiRaidSettingsView):
        super().__init__()
        self.view = view
        self.threshold = ui.TextInput(
            label="Message Threshold",
            placeholder="Enter number of messages to trigger anti-raid",
            default=str(view.settings['message_threshold']),
            min_length=1,
            max_length=3
        )
        self.add_item(self.threshold)
    
    async def on_submit(self, interaction: discord.Interaction):
        """handle modal submission"""
        try:
            threshold = int(self.threshold.value)
            if threshold < 2 or threshold > 100:
                await interaction.response.send_message(
                    "Please enter a number between 2 and 100.",
                    ephemeral=True
                )
                return
            
            self.view.settings['message_threshold'] = threshold
            await self.view.save_settings()
            
            # update the button in the view
            for item in self.view.children:
                if isinstance(item, EditThresholdButton):
                    item.update_label(threshold)
                    break
                    
            await self.view.update_message(interaction)
        except ValueError:
            await interaction.response.send_message(
                "Please enter a valid number.",
                ephemeral=True
            )

class EditTimeWindowModal(ui.Modal, title="Edit Time Window"):
    """modal for editing time window"""
    
    def __init__(self, view: AntiRaidSettingsView):
        super().__init__()
        self.view = view
        self.time_window = ui.TextInput(
            label="Time Window (seconds)",
            placeholder="Enter time window in seconds",
            default=str(view.settings['time_window']),
            min_length=1,
            max_length=3
        )
        self.add_item(self.time_window)
    
    async def on_submit(self, interaction: discord.Interaction):
        """handle modal submission"""
        try:
            time_window = int(self.time_window.value)
            if time_window < 1 or time_window > 300:
                await interaction.response.send_message(
                    "Please enter a number between 1 and 300 seconds.",
                    ephemeral=True
                )
                return
            
            self.view.settings['time_window'] = time_window
            await self.view.save_settings()
            
            # update the button in the view
            for item in self.view.children:
                if isinstance(item, EditTimeWindowButton):
                    item.update_label(time_window)
                    break
                    
            await self.view.update_message(interaction)
        except ValueError:
            await interaction.response.send_message(
                "Please enter a valid number.",
                ephemeral=True
            )

class EditLockDurationModal(ui.Modal, title="Edit Lock Duration"):
    """modal for editing lock duration"""
    
    def __init__(self, view: AntiRaidSettingsView):
        super().__init__()
        self.view = view
        self.lock_duration = ui.TextInput(
            label="Lock Duration (minutes)",
            placeholder="Enter lock duration in minutes",
            default=str(view.settings['lock_duration'] // 60),
            min_length=1,
            max_length=4
        )
        self.add_item(self.lock_duration)
    
    async def on_submit(self, interaction: discord.Interaction):
        """handle modal submission"""
        try:
            minutes = int(self.lock_duration.value)
            if minutes < 1 or minutes > 1440:
                await interaction.response.send_message(
                    "Please enter a number between 1 and 1440 minutes (24 hours).",
                    ephemeral=True
                )
                return
            
            self.view.settings['lock_duration'] = minutes * 60
            await self.view.save_settings()
            
            # update the button in the view
            for item in self.view.children:
                if isinstance(item, EditLockDurationButton):
                    item.update_label(minutes * 60)  # covnert back to seconds for consistency
                    break
                    
            await self.view.update_message(interaction)
        except ValueError:
            await interaction.response.send_message(
                "Please enter a valid number.",
                ephemeral=True
            )

class RoleSelectDropdown(ui.Select):
    """base class for role selection dropdowns"""
    
    def __init__(self, placeholder: str, min_values: int = 1, max_values: int = 1):
        super().__init__(
            placeholder=placeholder,
            min_values=min_values,
            max_values=max_values,
            options=[]
        )
    
    async def get_available_roles(self, guild: discord.Guild) -> list[discord.Role]:
        """get roles that can be added to the exempt list"""
        return [role for role in guild.roles if not role.is_bot_managed() and role.name != "@everyone"]
    
    def get_role_options(self, roles: list[discord.Role]) -> list[discord.SelectOption]:
        """convert roles to select options"""
        return [
            discord.SelectOption(
                label=role.name,
                value=str(role.id),
                description=f"ID: {role.id}"
            )
            for role in sorted(roles, key=lambda r: r.position, reverse=True)
        ]

class AddRoleDropdown(RoleSelectDropdown):
    """dropdown to select roles to add to the exempt list"""
    
    def __init__(self, settings_view: AntiRaidSettingsView):
        super().__init__("Select roles to exempt from anti-raid")
        self.settings_view = settings_view
    
    async def callback(self, interaction: discord.Interaction):
        """handle role selection"""
        # acknowledge the interaction first
        await interaction.response.defer(ephemeral=True)
        
        guild = interaction.guild
        if not guild:
            await interaction.followup.send(
                "❌ Error: Could not get guild information.",
                ephemeral=True
            )
            return
            
        added = 0
        
        try:
            for role_id in self.values:
                role = guild.get_role(int(role_id))
                if role and str(role_id) not in self.settings_view.settings['exempt_roles']:
                    self.settings_view.settings['exempt_roles'].append(str(role_id))
                    added += 1
            
            if added > 0:
                await self.settings_view.save_settings()
                
                # send success message
                try:
                    # Create a new view first
                    new_view = ExemptRolesView(self.settings_view)
                    
                    # Try to edit the original message with the new view
                    if hasattr(interaction, 'edit_original_response'):
                        await interaction.edit_original_response(
                            content=f"✅ Successfully added {added} role(s) to the exempt list.",
                            view=new_view,
                            embed=None
                        )
                    else:
                        # If we can't edit the original, send a new message
                        await interaction.followup.send(
                            f"✅ Successfully added {added} role(s) to the exempt list.",
                            view=new_view,
                            ephemeral=True
                        )
                except Exception as e:
                    # If we can't update the view, just send a success message
                    await interaction.followup.send(
                        f"✅ Successfully added {added} role(s) to the exempt list. "
                        "Please use the command again to see the updated view.",
                        ephemeral=True
                    )
            else:
                await interaction.followup.send(
                    "ℹ️ No new roles were added to the exempt list.",
                    ephemeral=True
                )
        except Exception as e:
            logger.error(f"Error in AddRoleDropdown callback: {str(e)}", exc_info=True)
            await interaction.followup.send(
                "❌ An error occurred while processing your request. Please try again.",
                ephemeral=True
            )

class RemoveRoleDropdown(RoleSelectDropdown):
    """dropdown to select roles to remove from the exempt list"""
    
    def __init__(self, settings_view: AntiRaidSettingsView):
        super().__init__("Select roles to remove from exempt list")
        self.settings_view = settings_view
    
    async def callback(self, interaction: discord.Interaction):
        """handle role removal"""
        # acknowledge the interaction first
        await interaction.response.defer(ephemeral=True)
        
        removed = 0
        
        for role_id in self.values:
            if role_id in self.settings_view.settings['exempt_roles']:
                self.settings_view.settings['exempt_roles'].remove(role_id)
                removed += 1
        
        if removed > 0:
            await self.settings_view.save_settings()
            
            # send success message without trying to edit the original message
            await interaction.followup.send(
                f"✅ Successfully removed {removed} role(s) from the exempt list.",
                ephemeral=True
            )
            
            # create a new message with the updated view
            view = ExemptRolesView(self.settings_view)
            try:
                await interaction.followup.send(
                    "Anti-Raid Settings (updated):",
                    embed=view.get_embed(),
                    view=view,
                    ephemeral=True
                )
            except Exception as e:
                await interaction.followup.send(
                    "✅ Roles were removed, but there was an error showing the updated view. "
                    "Please use the command again to see the changes.",
                    ephemeral=True
                )
        else:
            await interaction.followup.send(
                "ℹ️ No roles were removed from the exempt list.",
                ephemeral=True
            )

class AddExemptRoleButton(ui.Button):
    """button to show the add role dropdown"""
    
    def __init__(self):
        super().__init__(
            label="Add Role",
            style=discord.ButtonStyle.primary
        )
    
    async def callback(self, interaction: discord.Interaction):
        """show the add role dropdown"""
        try:
            view: ExemptRolesView = self.view
            guild = interaction.guild
            
            # get roles that aren't already exempt
            exempt_role_ids = set(view.settings_view.settings['exempt_roles'])
            available_roles = [
                role for role in guild.roles 
                if not role.is_bot_managed() 
                and role.name != "@everyone" 
                and str(role.id) not in exempt_role_ids
            ]
            
            if not available_roles:
                await interaction.response.send_message(
                    "All available roles are already exempt.",
                    ephemeral=True
                )
                return
            
            # create a view with the dropdown
            dropdown = AddRoleDropdown(view.settings_view)
            dropdown.options = dropdown.get_role_options(available_roles)
            
            select_view = ui.View(timeout=300)
            select_view.add_item(dropdown)
            
            await interaction.response.send_message(
                "Select roles to add to the exempt list:",
                view=select_view,
                ephemeral=True
            )
        except Exception as e:
            if not interaction.response.is_done():
                await interaction.response.send_message(
                    f"❌ An error occurred: {str(e)}",
                    ephemeral=True
                )
            else:
                await interaction.followup.send(
                    f"❌ An error occurred: {str(e)}",
                    ephemeral=True
                )

class RemoveExemptRoleButton(ui.Button):
    """button to show the remove role dropdown"""
    
    def __init__(self):
        super().__init__(
            label="Remove Role",
            style=discord.ButtonStyle.danger
        )
    
    async def callback(self, interaction: discord.Interaction):
        """show the remove role dropdown"""
        try:
            view: ExemptRolesView = self.view
            guild = interaction.guild
            
            if not view.settings_view.settings['exempt_roles']:
                await interaction.response.send_message(
                    "There are no roles to remove.",
                    ephemeral=True
                )
                return
            
            # get currently exempt roles that still exist in the guild
            exempt_roles = []
            for role_id in view.settings_view.settings['exempt_roles']:
                role = guild.get_role(int(role_id))
                if role:
                    exempt_roles.append(role)
            
            if not exempt_roles:
                await interaction.response.send_message(
                    "No valid exempt roles found.",
                    ephemeral=True
                )
                return
            
            # create a dropdown with the exempt roles
            dropdown = RemoveRoleDropdown(view.settings_view)
            dropdown.options = dropdown.get_role_options(exempt_roles)
            
            select_view = ui.View(timeout=300)
            select_view.add_item(dropdown)
            
            await interaction.response.send_message(
                "Select roles to remove from the exempt list:",
                view=select_view,
                ephemeral=True
            )
        except Exception as e:
            if not interaction.response.is_done():
                await interaction.response.send_message(
                    f"❌ An error occurred: {str(e)}",
                    ephemeral=True
                )
            else:
                await interaction.followup.send(
                    f"❌ An error occurred: {str(e)}",
                    ephemeral=True
                )

class ViewExemptRolesButton(ui.Button):
    """button to view current exempt roles"""
    
    def __init__(self):
        super().__init__(
            label="View Exempt Roles",
            style=discord.ButtonStyle.secondary
        )
    
    async def callback(self, interaction: discord.Interaction):
        """handle button click"""
        view: ExemptRolesView = self.view
        
        if not view.settings_view.settings['exempt_roles']:
            await interaction.response.send_message(
                "There are no exempt roles set.",
                ephemeral=True
            )
            return
        
        guild = interaction.guild
        roles = []
        
        for role_id in view.settings_view.settings['exempt_roles']:
            role = guild.get_role(int(role_id))
            if role:
                roles.append(f"- {role.mention} (`{role.id}`)")
        
        if not roles:
            await interaction.response.send_message(
                "No valid exempt roles found.",
                ephemeral=True
            )
            return
        
        await interaction.response.send_message(
            "**Exempt Roles:**\n" + "\n".join(roles),
            ephemeral=True
        )

class CloseExemptRolesButton(ui.Button):
    """button to close the exempt roles view"""
    
    def __init__(self):
        super().__init__(
            label="Close",
            style=discord.ButtonStyle.danger
        )
    
    async def callback(self, interaction: discord.Interaction):
        """handle button click"""
        await interaction.response.defer()
        await interaction.delete_original_response()

class ExemptRolesView(ui.View):
    """view for managing exempt roles"""
    
    def __init__(self, settings_view: AntiRaidSettingsView):
        super().__init__(timeout=300)
        self.settings_view = settings_view
        
        # Add buttons
        self.add_item(AddExemptRoleButton())
        self.add_item(RemoveExemptRoleButton())
        self.add_item(ViewExemptRolesButton())
        self.add_item(CloseExemptRolesButton())

class AddExemptRoleModal(ui.Modal, title="Add Exempt Role"):
    """modal for adding an exempt role"""
    
    def __init__(self, settings_view: AntiRaidSettingsView):
        super().__init__()
        self.settings_view = settings_view
        self.role = ui.TextInput(
            label="Role",
            placeholder="Mention the role or enter role ID",
            required=True
        )
        self.add_item(self.role)
    
    async def on_submit(self, interaction: discord.Interaction):
        """handle modal submission"""
        role_input = self.role.value.strip()
        role = None
        
        # check if input is a role mention
        if role_input.startswith('<@&') and role_input.endswith('>'):
            role_id = int(role_input[3:-1])
            role = interaction.guild.get_role(role_id)
        # check if input is a role ID
        elif role_input.isdigit():
            role = interaction.guild.get_role(int(role_input))
        
        if not role:
            await interaction.response.send_message(
                "Could not find that role. Please mention the role or enter a valid role ID.",
                ephemeral=True
            )
            return
        
        # add role to exempt list if not already there
        role_id_str = str(role.id)
        if role_id_str not in self.settings_view.settings['exempt_roles']:
            self.settings_view.settings['exempt_roles'].append(role_id_str)
            await self.settings_view.save_settings()
            await interaction.response.send_message(
                f"Added {role.mention} to the exempt roles list.",
                ephemeral=True
            )
        else:
            await interaction.response.send_message(
                f"{role.mention} is already in the exempt roles list.",
                ephemeral=True
            )
