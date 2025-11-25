"""moderation actions like kick, ban, etc."""
import discord
from discord.ext import commands
from discord import app_commands
from typing import Optional, Dict, Any
import logging
from datetime import datetime
from modules.database.database import db

logger = logging.getLogger(__name__)

class ModActions(commands.Cog):
    """handles moderation actions like kick, ban, etc."""
    
    def __init__(self, bot: commands.Bot) -> None:
        self.bot = bot
    
    async def log_action_embed(
        self,
        action: str,
        guild: discord.Guild,
        user: discord.Member,
        moderator: discord.Member,
        reason: Optional[str] = None,
        **kwargs
    ) -> Optional[discord.Message]:
        """send a moderation action log to the mod log channel"""
        mod_log_cog = self.bot.get_cog("ModLog")
        if not mod_log_cog:
            return None
            
        channel_id = mod_log_cog.get_mod_log_channel(guild.id)
        if not channel_id:
            return None
            
        channel = guild.get_channel(channel_id)
        if not channel or not isinstance(channel, discord.TextChannel):
            return None
        
        # Create action-specific embed
        action_colors = {
            "kick": discord.Color.orange(),
            "ban": discord.Color.red(),
            "warn": discord.Color.gold(),
            "mute": discord.Color.dark_grey(),
        }
        
        embed = discord.Embed(
            title=f"member {action}ed - {user}",
            color=action_colors.get(action, discord.Color.blue()),
            timestamp=discord.utils.utcnow()
        )
        
        # Add common fields
        embed.add_field(name="user", value=f"{user.mention} ({user.id})", inline=False)
        embed.add_field(name="moderator", value=moderator.mention, inline=False)
        
        if reason:
            embed.add_field(name="reason", value=reason, inline=False)
        
        # Add any additional fields from kwargs
        for key, value in kwargs.items():
            if value:  # Only add if value is not None/empty
                embed.add_field(name=key.replace('_', ' '), value=value, inline=False)
        
        embed.set_author(name=str(moderator), icon_url=moderator.display_avatar.url)
        embed.set_footer(text=f"User ID: {user.id}")
        
        try:
            return await channel.send(embed=embed)
        except discord.HTTPException as e:
            logger.error(f"failed to send action log: {e}")
            return None
    
    async def is_owner(interaction: discord.Interaction) -> bool:
        """check if the user is the server owner"""
        if not interaction.guild:
            return False
        return interaction.user.id == interaction.guild.owner_id
        
    @app_commands.command(name="slow-mode", description="set the slow mode duration for a channel (guild owner only)")
    @app_commands.describe(
        duration="slow mode duration (e.g., 5s, 1m, 2h, 0 to disable)",
        channel="channel to set slow mode for (defaults to current channel)",
        reason="reason for the change (optional)"
    )
    @app_commands.checks.check(is_owner)
    async def slow_mode_command(
        self,
        interaction: discord.Interaction,
        duration: str,
        channel: Optional[discord.TextChannel] = None,
        reason: Optional[str] = None
    ) -> None:
        """set the slow mode duration for a channel (guild owner only)"""
        if not interaction.guild:
            return await interaction.response.send_message(
                "this command can only be used in a server",
                ephemeral=True
            )
            
        # Use the current channel if none specified
        target_channel = channel or interaction.channel
        
        # Check if the bot has permission to manage the channel
        if not target_channel.permissions_for(interaction.guild.me).manage_channels:
            return await interaction.response.send_message(
                "i don't have permission to manage that channel",
                ephemeral=True
            )
        
        # Parse the duration
        try:
            # Check if it's a number (seconds)
            if duration.isdigit():
                seconds = int(duration)
            else:
                # Try to parse duration string (e.g., 5s, 1m, 2h)
                unit = duration[-1].lower()
                if unit not in ['s', 'm', 'h']:
                    raise ValueError("invalid duration unit")
                    
                value = duration[:-1]
                if not value.isdigit():
                    raise ValueError("invalid duration value")
                    
                value = int(value)
                
                if unit == 's':
                    seconds = value
                elif unit == 'm':
                    seconds = value * 60
                elif unit == 'h':
                    seconds = value * 3600
            
            # Validate the duration (Discord's limit is 6 hours = 21600 seconds)
            if seconds < 0 or seconds > 21600:
                return await interaction.response.send_message(
                    "slow mode duration must be between 0 and 6 hours (21600 seconds)",
                    ephemeral=True
                )
                
            # Defer the response since we're about to make an API call
            await interaction.response.defer(ephemeral=True)
            
            # Set the slow mode
            await target_channel.edit(
                slowmode_delay=seconds,
                reason=f"{interaction.user} (ID: {interaction.user.id}): {reason or 'No reason provided'}"
            )
            
            # Format the duration for display
            if seconds == 0:
                duration_str = "disabled"
            else:
                hours, remainder = divmod(seconds, 3600)
                minutes, seconds = divmod(remainder, 60)
                
                parts = []
                if hours > 0:
                    parts.append(f"{hours} hour{'s' if hours != 1 else ''}")
                if minutes > 0:
                    parts.append(f"{minutes} minute{'s' if minutes != 1 else ''}")
                if seconds > 0 and hours == 0:  # Only show seconds if less than a minute
                    parts.append(f"{seconds} second{'s' if seconds != 1 else ''}")
                    
                duration_str = ", ".join(parts)
            
            # Log the action
            log_message = await self.log_action_embed(
                action="slow_mode",
                guild=interaction.guild,
                user=interaction.user,
                moderator=interaction.user,
                reason=reason,
                channel=target_channel.mention,
                duration=duration_str
            )
            
            # Send confirmation
            embed = discord.Embed(
                title="slow mode updated",
                description=f"slow mode in {target_channel.mention} has been set to **{duration_str}**",
                color=discord.Color.green()
            )
            
            if log_message and log_message.channel:
                embed.add_field(
                    name="log",
                    value=f"[view in mod logs]({log_message.jump_url})",
                    inline=False
                )
                
            if reason:
                embed.add_field(name="reason", value=reason, inline=False)
            
            await interaction.followup.send(embed=embed, ephemeral=True)
            
        except (ValueError, IndexError):
            await interaction.response.send_message(
                "invalid duration format. use a number of seconds (e.g., 30) or a duration string (e.g., 5s, 1m, 2h)",
                ephemeral=True
            )
        except discord.Forbidden:
            await interaction.followup.send(
                "i don't have permission to modify that channel",
                ephemeral=True
            )
        except discord.HTTPException as e:
            logger.error(f"failed to set slow mode: {e}")
            await interaction.followup.send(
                "an error occurred while trying to set slow mode",
                ephemeral=True
            )
        
    @app_commands.command(name="purge", description="delete multiple messages (guild owner only)")
    @app_commands.describe(
        amount="number of messages to delete (1-1000)",
        user="only delete messages from this user (optional)",
        contains="only delete messages containing this text (optional)",
        starts_with="only delete messages starting with this text (optional)",
        ends_with="only delete messages ending with this text (optional)",
        match="only delete messages that exactly match this text (optional)",
        attachments="only delete messages with attachments (true/false) (optional)",
        embeds="only delete messages with embeds (true/false) (optional)",
        before_message="only delete messages before this message ID (optional)",
        after_message="only delete messages after this message ID (optional)",
        reason="reason for the purge (optional)"
    )
    @app_commands.checks.check(is_owner)
    async def purge_command(
        self,
        interaction: discord.Interaction,
        amount: int,
        user: Optional[discord.User] = None,
        contains: Optional[str] = None,
        starts_with: Optional[str] = None,
        ends_with: Optional[str] = None,
        match: Optional[str] = None,
        attachments: Optional[bool] = None,
        embeds: Optional[bool] = None,
        before_message: Optional[str] = None,
        after_message: Optional[str] = None,
        reason: Optional[str] = None
    ) -> None:
        """delete multiple messages with optional filters (guild owner only)"""
        if not interaction.guild:
            return await interaction.response.send_message(
                "this command can only be used in a server",
                ephemeral=True
            )
            
        # Check if the bot has permission to manage messages
        if not interaction.channel.permissions_for(interaction.guild.me).manage_messages:
            return await interaction.response.send_message(
                "i don't have permission to manage messages in this channel",
                ephemeral=True
            )
        
        # Validate amount
        if amount < 1 or amount > 1000:
            return await interaction.response.send_message(
                "amount must be between 1 and 1000",
                ephemeral=True
            )
        
        # Parse message IDs if provided
        before = None
        after = None
        
        if before_message:
            try:
                before = await interaction.channel.fetch_message(int(before_message))
            except (ValueError, discord.NotFound, discord.Forbidden, discord.HTTPException):
                return await interaction.response.send_message(
                    "invalid before_message ID",
                    ephemeral=True
                )
        
        if after_message:
            try:
                after = await interaction.channel.fetch_message(int(after_message))
            except (ValueError, discord.NotFound, discord.Forbidden, discord.HTTPException):
                return await interaction.response.send_message(
                    "invalid after_message ID",
                    ephemeral=True
                )
        
        # Defer the response since this might take a while
        await interaction.response.defer(ephemeral=True)
        
        try:
            # Define the check function based on the provided filters
            def check(message: discord.Message) -> bool:
                # Check user filter
                if user and message.author.id != user.id:
                    return False
                
                # Check content filters
                content = message.content.lower()
                if contains and contains.lower() not in content:
                    return False
                if starts_with and not content.startswith(starts_with.lower()):
                    return False
                if ends_with and not content.endswith(ends_with.lower()):
                    return False
                if match and content != match.lower():
                    return False
                    
                # Check attachment and embed filters
                if attachments is not None:
                    has_attachments = bool(message.attachments)
                    if attachments and not has_attachments:
                        return False
                    if not attachments and has_attachments:
                        return False
                        
                if embeds is not None:
                    has_embeds = bool(message.embeds)
                    if embeds and not has_embeds:
                        return False
                    if not embeds and has_embeds:
                        return False
                        
                return True
            
            # Delete the messages
            deleted = await interaction.channel.purge(
                limit=amount,
                check=check,
                before=before,
                after=after,
                reason=f"{interaction.user} (ID: {interaction.user.id}): {reason or 'No reason provided'}"
            )
            
            # Get the number of deleted messages
            deleted_count = len(deleted)
            
            # Log the purge
            log_message = await self.log_action_embed(
                action="purge",
                guild=interaction.guild,
                user=interaction.user,
                moderator=interaction.user,
                reason=reason,
                channel=interaction.channel.mention,
                messages_deleted=str(deleted_count),
                filters={
                    "user": user.mention if user else "Any",
                    "contains": contains or "-",
                    "starts_with": starts_with or "-",
                    "ends_with": ends_with or "-",
                    "exact_match": match or "-",
                    "has_attachments": str(attachments) if attachments is not None else "-",
                    "has_embeds": str(embeds) if embeds is not None else "-",
                    "before_message": before_message or "-",
                    "after_message": after_message or "-"
                }
            )
            
            # Send confirmation
            embed = discord.Embed(
                title="messages purged",
                description=f"deleted {deleted_count} message{'s' if deleted_count != 1 else ''} from {interaction.channel.mention}",
                color=discord.Color.green()
            )
            
            if log_message and log_message.channel:
                embed.add_field(
                    name="log",
                    value=f"[view in mod logs]({log_message.jump_url})",
                    inline=False
                )
                
            if reason:
                embed.add_field(name="reason", value=reason, inline=False)
                
            # Add filter information if any filters were used
            filters_used = []
            if user:
                filters_used.append(f"user: {user.mention}")
            if contains:
                filters_used.append(f"contains: '{contains}'")
            if starts_with:
                filters_used.append(f"starts with: '{starts_with}'")
            if ends_with:
                filters_used.append(f"ends with: '{ends_with}'")
            if match:
                filters_used.append(f"matches: '{match}'")
            if attachments is not None:
                filters_used.append(f"has attachments: {attachments}")
            if embeds is not None:
                filters_used.append(f"has embeds: {embeds}")
            if before_message:
                filters_used.append(f"before message: {before_message}")
            if after_message:
                filters_used.append(f"after message: {after_message}")
                
            if filters_used:
                embed.add_field(
                    name="filters applied",
                    value="\n".join(f"• {f}" for f in filters_used),
                    inline=False
                )
            
            # Send the confirmation message with a 10-second auto-delete
            await interaction.followup.send(embed=embed, ephemeral=True)
            
        except discord.Forbidden:
            await interaction.followup.send(
                "i don't have permission to delete messages in this channel",
                ephemeral=True
            )
        except discord.HTTPException as e:
            logger.error(f"failed to purge messages: {e}")
            await interaction.followup.send(
                "an error occurred while trying to delete messages",
                ephemeral=True
            )

    @app_commands.command(name="ban", description="ban a user from the server (server owner only)")
    @app_commands.describe(
        user="the user to ban (ID or mention)",
        reason="reason for the ban",
        delete_message_days="number of days of messages to delete (0-7)"
    )
    @app_commands.checks.check(is_owner)
    async def ban_command(
        self,
        interaction: discord.Interaction,
        user: discord.User,
        reason: Optional[str] = None,
        delete_message_days: int = 0
    ) -> None:
        """ban a user from the server (server owner only)"""
        if not interaction.guild:
            return await interaction.response.send_message(
                "this command can only be used in a server",
                ephemeral=True
            )
            
        if user.bot:
            return await interaction.response.send_message(
                "you cannot ban bots",
                ephemeral=True
            )
            
        if user.id == interaction.user.id:
            return await interaction.response.send_message(
                "you cannot ban yourself",
                ephemeral=True
            )
            
        # Validate delete_message_days
        if not 0 <= delete_message_days <= 7:
            return await interaction.response.send_message(
                "delete_message_days must be between 0 and 7",
                ephemeral=True
            )
        
        # Defer the response since we're about to make API calls
        await interaction.response.defer(ephemeral=True)
        
        try:
            # Log the ban
            log_message = await self.log_action_embed(
                action="ban",
                guild=interaction.guild,
                user=user,
                moderator=interaction.user,
                reason=reason,
                message_deletion=f"{delete_message_days} day{'s' if delete_message_days != 1 else ''} of messages deleted"
            )
            
            # DM the user about the ban
            try:
                dm_embed = discord.Embed(
                    title=f"you have been banned from {interaction.guild.name}",
                    description=f"**reason:** {reason or 'No reason provided'}",
                    color=discord.Color.red()
                )
                if log_message and log_message.channel:
                    dm_embed.set_footer(text=f"case id: {log_message.id}")
                await user.send(embed=dm_embed)
            except (discord.Forbidden, discord.HTTPException) as e:
                logger.warning(f"failed to DM user {user.id} about ban: {e}")
            
            # Actually ban the user
            await interaction.guild.ban(
                user,
                reason=f"{interaction.user} (ID: {interaction.user.id}): {reason or 'No reason provided'}",
                delete_message_days=delete_message_days
            )
            
            # Send confirmation to the moderator
            embed = discord.Embed(
                title=f"member banned - {user}",
                description=f"{user.mention} has been banned from the server.",
                color=discord.Color.green()
            )
            
            if log_message and log_message.channel:
                embed.add_field(
                    name="log",
                    value=f"[view in mod logs]({log_message.jump_url})",
                    inline=False
                )
                
            if reason:
                embed.add_field(name="reason", value=reason, inline=False)
                
            embed.add_field(
                name="messages deleted",
                value=f"{delete_message_days} day{'s' if delete_message_days != 1 else ''} of messages",
                inline=False
            )
            
            await interaction.followup.send(embed=embed, ephemeral=True)
            
        except discord.Forbidden:
            await interaction.followup.send(
                "i don't have permission to ban that user",
                ephemeral=True
            )
        except discord.HTTPException as e:
            logger.error(f"failed to ban user {user.id}: {e}")
            await interaction.followup.send(
                f"an error occurred while trying to ban {user.mention}",
                ephemeral=True
            )
    
    @app_commands.command(name="un-ban", description="unban a user from the server (server owner only)")
    @app_commands.describe(
        user="the user to unban (ID or username#discriminator)",
        reason="reason for the unban"
    )
    @app_commands.checks.check(is_owner)
    async def unban_command(
        self,
        interaction: discord.Interaction,
        user: str,
        reason: Optional[str] = None
    ) -> None:
        """unban a user from the server (server owner only)"""
        if not interaction.guild:
            return await interaction.response.send_message(
                "this command can only be used in a server",
                ephemeral=True
            )
        
        # Try to parse the user ID from the input
        try:
            # Check if it's a mention or ID
            if user.isdigit():
                user_id = int(user)
            elif user.startswith('<@') and user.endswith('>'):
                user_id = int(user[2:-1].replace('!', ''))
            else:
                # Try to parse as username#discriminator
                banned_users = [entry async for entry in interaction.guild.bans()]
                for ban_entry in banned_users:
                    if str(ban_entry.user) == user:
                        user_id = ban_entry.user.id
                        break
                else:
                    return await interaction.response.send_message(
                        "user not found in ban list. please provide a valid user ID or username#discriminator",
                        ephemeral=True
                    )
        except ValueError:
            return await interaction.response.send_message(
                "invalid user format. please provide a user ID or username#discriminator",
                ephemeral=True
            )
        
        # Defer the response since we're about to make API calls
        await interaction.response.defer(ephemeral=True)
        
        try:
            # Get the user object
            user = await self.bot.fetch_user(user_id)
            
            # Check if the user is actually banned
            try:
                ban_entry = await interaction.guild.fetch_ban(user)
            except discord.NotFound:
                return await interaction.followup.send(
                    f"{user} is not banned from this server",
                    ephemeral=True
                )
            
            # Log the unban
            log_message = await self.log_action_embed(
                action="unban",
                guild=interaction.guild,
                user=user,
                moderator=interaction.user,
                reason=reason
            )
            
            # Actually unban the user
            await interaction.guild.unban(
                user,
                reason=f"{interaction.user} (ID: {interaction.user.id}): {reason or 'No reason provided'}"
            )
            
            # Send confirmation to the moderator
            embed = discord.Embed(
                title=f"member unbanned - {user}",
                description=f"{user.mention} has been unbanned from the server.",
                color=discord.Color.green()
            )
            
            if log_message and log_message.channel:
                embed.add_field(
                    name="log",
                    value=f"[view in mod logs]({log_message.jump_url})",
                    inline=False
                )
                
            if reason:
                embed.add_field(name="reason", value=reason, inline=False)
                
            if ban_entry.reason:
                embed.add_field(
                    name="original ban reason",
                    value=ban_entry.reason,
                    inline=False
                )
            
            await interaction.followup.send(embed=embed, ephemeral=True)
            
        except discord.Forbidden:
            await interaction.followup.send(
                "i don't have permission to unban users",
                ephemeral=True
            )
        except discord.HTTPException as e:
            logger.error(f"failed to unban user {user_id}: {e}")
            await interaction.followup.send(
                f"an error occurred while trying to unban the user",
                ephemeral=True
            )
    
    @app_commands.command(name="kick", description="kick a member from the server")
    @app_commands.describe(
        member="the member to kick",
        reason="reason for the kick"
    )
    @app_commands.checks.has_permissions(kick_members=True)
    async def kick_command(
        self, 
        interaction: discord.Interaction, 
        member: discord.Member,
        reason: Optional[str] = None
    ) -> None:
        """kick a member from the server"""
        if not interaction.guild:
            return await interaction.response.send_message(
                "this command can only be used in a server", 
                ephemeral=True
            )
            
        if member.bot:
            return await interaction.response.send_message(
                "you cannot kick bots",
                ephemeral=True
            )
            
        if member.id == interaction.user.id:
            return await interaction.response.send_message(
                "you cannot kick yourself",
                ephemeral=True
            )
            
        if interaction.guild.owner_id == member.id:
            return await interaction.response.send_message(
                "you cannot kick the server owner",
                ephemeral=True
            )
            
        # Check if the bot has permission to kick
        if not interaction.guild.me.guild_permissions.kick_members:
            return await interaction.response.send_message(
                "i don't have permission to kick members",
                ephemeral=True
            )
            
        # Check if the target member is higher in the role hierarchy
        if member.top_role >= interaction.guild.me.top_role:
            return await interaction.response.send_message(
                "i can't kick someone with a role higher than or equal to mine",
                ephemeral=True
            )
            
        # Check if the target member is higher than the command invoker
        if member.top_role >= interaction.user.top_role and interaction.guild.owner_id != interaction.user.id:
            return await interaction.response.send_message(
                "you can't kick someone with a role higher than or equal to yours",
                ephemeral=True
            )
        
        # Defer the response since we're about to make an API call
        await interaction.response.defer(ephemeral=True)
        
        try:
            # Log the kick
            log_message = await self.log_action_embed(
                action="kick",
                guild=interaction.guild,
                user=member,
                moderator=interaction.user,
                reason=reason
            )
            
            # DM the user about the kick
            try:
                dm_embed = discord.Embed(
                    title=f"you have been kicked from {interaction.guild.name}",
                    description=f"**reason:** {reason or 'No reason provided'}",
                    color=discord.Color.orange()
                )
                if log_message and log_message.channel:
                    dm_embed.set_footer(text=f"case id: {log_message.id}")
                await member.send(embed=dm_embed)
            except (discord.Forbidden, discord.HTTPException) as e:
                logger.warning(f"failed to DM user {member.id} about kick: {e}")
            
            # Actually kick the member
            await member.kick(reason=f"{interaction.user} (ID: {interaction.user.id}): {reason or 'No reason provided'}")
            
            # Send confirmation to the moderator
            embed = discord.Embed(
                title=f"member kicked - {member}",
                description=f"{member.mention} has been kicked from the server.",
                color=discord.Color.green()
            )
            
            if log_message and log_message.channel:
                embed.add_field(
                    name="log",
                    value=f"[view in mod logs]({log_message.jump_url})",
                    inline=False
                )
                
            if reason:
                embed.add_field(name="reason", value=reason, inline=False)
            
            await interaction.followup.send(embed=embed, ephemeral=True)
            
        except discord.Forbidden:
            await interaction.followup.send(
                "i don't have permission to kick that member",
                ephemeral=True
            )
        except discord.HTTPException as e:
            logger.error(f"failed to kick member {member.id}: {e}")
            await interaction.followup.send(
                f"an error occurred while trying to kick {member.mention}",
                ephemeral=True
            )


def setup(bot: commands.Bot) -> None:
    """load the mod actions cog"""
    bot.add_cog(ModActions(bot))
