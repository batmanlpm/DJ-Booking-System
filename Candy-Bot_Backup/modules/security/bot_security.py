"""
bot security module to handle bot approval workflow.
"""
import discord
from discord.ext import commands
from discord import ui
import logging
from typing import Optional, Dict, Any
import asyncio
from datetime import datetime

from modules.database import db

# set up logging
logger = logging.getLogger('discord.security.bot')

def log_action(message: str, level: str = 'info', **kwargs):
    """helper function for consistent logging."""
    log_message = f"[Bot Security] {message}"
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
    print(log_message)

class BotApprovalModal(ui.Modal, title="Bot Approval Request"):
    """modal for submitting bot website URL for approval."""
    website = ui.TextInput(
        label="Bot's Website/Invite URL",
        placeholder="https://example.com or https://discord.com/api/oauth2/...",
        style=discord.TextStyle.short,
        required=True,
        max_length=200
    )

    def __init__(self, bot_id: int, inviter_id: int, guild_id: int):
        super().__init__()
        self.bot_id = bot_id
        self.inviter_id = inviter_id
        self.guild_id = guild_id

    async def on_submit(self, interaction: discord.Interaction):
        await interaction.response.defer(ephemeral=True)
        
        # update the database with the website URL
        db.execute_query(
            """
            UPDATE bot_approval_requests 
            SET website_url = ?, status = 'pending_review', updated_at = CURRENT_TIMESTAMP
            WHERE bot_id = ? AND guild_id = ? AND inviter_id = ?
            """,
            (str(self.website), self.bot_id, self.guild_id, self.inviter_id)
        )
        
        # get the request details
        request = db.execute_query(
            """
            SELECT * FROM bot_approval_requests 
            WHERE bot_id = ? AND guild_id = ? AND inviter_id = ?
            """,
            (self.bot_id, self.guild_id, self.inviter_id),
            fetch=True
        )
        
        if not request:
            await interaction.followup.send(
                "❌ An error occurred while processing your request. Please try again.",
                ephemeral=True
            )
            return
            
        request = request[0]
        
        # notify the inviter
        await interaction.followup.send(
            f"✅ Your request for bot approval has been submitted! "
            f"The server owner will review your submission.",
            ephemeral=True
        )
        
        # get the guild and owner
        guild = interaction.client.get_guild(self.guild_id)
        if not guild:
            return
            
        owner = guild.owner
        if not owner:
            return
            
        # send approval request to owner
        embed = discord.Embed(
            title="🤖 Bot Approval Request",
            description=f"A bot has been invited to **{guild.name}** and requires approval.",
            color=discord.Color.orange(),
            timestamp=datetime.utcnow()
        )
        
        bot = guild.get_member(self.bot_id)
        inviter = guild.get_member(self.inviter_id)
        
        embed.add_field(name="Bot", value=f"{bot.mention} (`{bot.id}`)" if bot else f"`{self.bot_id}`", inline=True)
        embed.add_field(name="Invited By", value=inviter.mention if inviter else f"`{self.inviter_id}`", inline=True)
        
        if request['website_url']:
            embed.add_field(name="Website/Invite URL", value=request['website_url'], inline=False)
            
        embed.set_footer(text=f"Request ID: {request['id']}")
        
        view = BotApprovalView(
            request_id=request['id'],
            bot_id=self.bot_id,
            inviter_id=self.inviter_id,
            guild_id=self.guild_id
        )
        
        try:
            await owner.send(embed=embed, view=view)
        except Exception as e:
            log_action(f"Failed to send DM to server owner: {e}", level='error')


class BotApprovalView(ui.View):
    """view for handling bot approval actions."""
    def __init__(self, request_id: int, bot_id: int, inviter_id: int, guild_id: int):
        super().__init__(timeout=86400)  # 24 hour timeout
        self.request_id = request_id
        self.bot_id = bot_id
        self.inviter_id = inviter_id
        self.guild_id = guild_id

    @ui.button(label="✅ Approve", style=discord.ButtonStyle.green)
    async def approve(self, interaction: discord.Interaction, button: ui.Button):
        """approve the bot and whitelist it."""
        await interaction.response.defer(ephemeral=True)
        
        # get the request details first
        request = db.execute_query(
            "SELECT * FROM bot_approval_requests WHERE id = ?",
            (self.request_id,),
            fetch=True
        )
        
        if not request:
            await interaction.followup.send("❌ Could not find the approval request.", ephemeral=True)
            return
            
        request = request[0]
        
        # update the request status
        db.execute_query(
            """
            UPDATE bot_approval_requests 
            SET status = 'approved', 
                approved_by = ?,
                approved_at = CURRENT_TIMESTAMP,
                updated_at = CURRENT_TIMESTAMP
            WHERE id = ?
            """,
            (interaction.user.id, self.request_id)
        )
        
        # add bot to whitelist
        db.execute_query(
            """
            INSERT OR IGNORE INTO bot_whitelist (bot_id, guild_id, approved_by, approved_at)
            VALUES (?, ?, ?, CURRENT_TIMESTAMP)
            """,
            (self.bot_id, self.guild_id, interaction.user.id)
        )
        
        # get the guild
        guild = interaction.client.get_guild(self.guild_id)
        if not guild:
            await interaction.followup.send("❌ Could not find the server.", ephemeral=True)
            return
            
        # get the bot user (might not be in the server yet)
        try:
            bot_user = await interaction.client.fetch_user(self.bot_id)
        except discord.NotFound:
            bot_user = None
        
        # get the inviter to notify them
        inviter = guild.get_member(self.inviter_id)
        
        # create bot invite link
        permissions = discord.Permissions(
            read_messages=True,
            send_messages=True,
            embed_links=True,
            attach_files=True,
            read_message_history=True,
            use_external_emojis=True,
            add_reactions=True
        )
        
        # create an invite link with the bot's application ID
        invite_url = f"https://discord.com/api/oauth2/authorize?client_id={self.bot_id}&permissions={permissions.value}&scope=bot%20applications.commands"
        
        # update the embed to show approved
        embed = interaction.message.embeds[0].copy()
        embed.color = discord.Color.green()
        embed.title = "✅ Bot Approved"
        embed.description = (
            f"The bot `{request['bot_name']}` (`{self.bot_id}`) has been approved "
            f"for **{guild.name}** by {interaction.user.mention}.\n\n"
            f"**Invite Link:** [Click here to add the bot]({invite_url})"
        )
        
        # add bot info if available
        if bot_user:
            embed.set_thumbnail(url=bot_user.display_avatar.url)
            
        # add website if provided
        if request.get('website_url'):
            embed.add_field(name="Website", value=request['website_url'], inline=False)
        
        await interaction.message.edit(embed=embed, view=None)
        
        # notify the inviter with the invite link
        if inviter:
            try:
                inviter_embed = discord.Embed(
                    title="✅ Bot Approved",
                    description=(
                        f"Your bot `{request['bot_name']}` has been approved "
                        f"for **{guild.name}** by {interaction.user.mention}.\n\n"
                        f"**You can now invite the bot using this link:**\n"
                        f"[Click here to add the bot]({invite_url})"
                    ),
                    color=discord.Color.green()
                )
                
                if bot_user:
                    inviter_embed.set_thumbnail(url=bot_user.display_avatar.url)
                
                if request.get('website_url'):
                    inviter_embed.add_field(name="Website", value=request['website_url'], inline=False)
                
                await inviter.send(embed=inviter_embed)
            except Exception as e:
                log_action(f"Failed to notify inviter: {e}", level='error')
        
        await interaction.followup.send(
            f"✅ Bot has been approved and whitelisted. "
            f"The inviter has been notified with the invite link.", 
            ephemeral=True
        )
        
        log_action(
            f"Bot {request['bot_name']} ({self.bot_id}) approved by {interaction.user} ({interaction.user.id})",
            guild_id=guild.id,
            invite_link=invite_url
        )

    @ui.button(label="❌ Deny", style=discord.ButtonStyle.red)
    async def deny(self, interaction: discord.Interaction, button: ui.Button):
        """deny the bot and kick it from the server."""
        await interaction.response.defer(ephemeral=True)
        
        # get the request details
        request = db.execute_query(
            "SELECT * FROM bot_approval_requests WHERE id = ?",
            (self.request_id,),
            fetch=True
        )
        
        if not request:
            await interaction.followup.send("❌ Could not find the approval request.", ephemeral=True)
            return
            
        request = request[0]
        
        # update the request status
        db.execute_query(
            """
            UPDATE bot_approval_requests 
            SET status = 'denied', updated_at = CURRENT_TIMESTAMP
            WHERE id = ?
            """,
            (self.request_id,)
        )
        
        # get the guild and bot
        guild = interaction.client.get_guild(self.guild_id)
        if not guild:
            await interaction.followup.send("❌ Could not find the server.", ephemeral=True)
            return
            
        bot = guild.get_member(self.bot_id)
        
        # kick the bot if it's still in the server
        if bot:
            try:
                await bot.kick(reason=f"Bot denied by {interaction.user}")
                kick_success = True
            except Exception as e:
                log_action(f"Failed to kick bot: {e}", level='error')
                kick_success = False
        else:
            kick_success = False
        
        # get the inviter to notify them
        inviter = guild.get_member(self.inviter_id)
        
        # update the embed to show denied
        embed = interaction.message.embeds[0].copy()
        embed.color = discord.Color.red()
        embed.title = "❌ Bot Denied"
        
        if kick_success:
            embed.description = f"The bot has been denied and kicked from **{guild.name}** by {interaction.user.mention}."
        else:
            embed.description = f"The bot has been denied for **{guild.name}** by {interaction.user.mention}."
        
        await interaction.message.edit(embed=embed, view=None)
        
        # notify the inviter if possible
        if inviter:
            try:
                await inviter.send(
                    f"❌ Your bot has been denied for **{guild.name}** by {interaction.user.mention}.\n"
                    f"Reason: {interaction.message.content or 'No reason provided'}"
                )
            except Exception as e:
                log_action(f"Failed to notify inviter: {e}", level='error')
        
        if kick_success:
            await interaction.followup.send("✅ Bot has been denied and kicked from the server.", ephemeral=True)
        else:
            await interaction.followup.send("✅ Bot has been denied.", ephemeral=True)
        
        log_action(
            f"Bot {request['bot_name']} ({self.bot_id}) denied by {interaction.user} ({interaction.user.id})",
            guild_id=guild.id,
            kick_success=kick_success
        )


class BotApprovalButton(ui.Button):
    """button for requesting bot approval."""
    def __init__(self, bot_id: int, inviter_id: int, guild_id: int):
        super().__init__(
            label="Request Approval",
            style=discord.ButtonStyle.primary,
            emoji="📝"
        )
        self.bot_id = bot_id
        self.inviter_id = inviter_id
        self.guild_id = guild_id
    
    async def callback(self, interaction: discord.Interaction):
        """handle button click."""
        # only the inviter can click this button
        if interaction.user.id != self.inviter_id:
            await interaction.response.send_message(
                "Only the person who invited the bot can request approval.",
                ephemeral=True
            )
            return
            
        # show the modal to enter website URL
        modal = BotApprovalModal(self.bot_id, self.inviter_id, self.guild_id)
        await interaction.response.send_modal(modal)


async def setup(bot):
    """set up the bot security handlers."""
    # the actual event handlers are in the security_events.py file
    log_action("Bot security handlers loaded")


# this function will be called when a bot joins the server
async def on_bot_join(bot: commands.Bot, member: discord.Member):
    """handle when a bot joins the server."""
    if not member.bot:
        return
        
    guild = member.guild
    
    # check if bot is whitelisted
    whitelisted = db.execute_query(
        """
        SELECT * FROM bot_whitelist 
        WHERE bot_id = ? AND guild_id = ?
        """,
        (member.id, guild.id),
        fetch=True
    )
    
    if whitelisted:
        log_action(f"Whitelisted bot {member} ({member.id}) joined the server", guild_id=guild.id)
        return
        
    # check if there's a pending approval request
    existing_request = db.execute_query(
        """
        SELECT * FROM bot_approval_requests 
        WHERE bot_id = ? AND guild_id = ? AND status = 'pending_review'
        """,
        (member.id, guild.id),
        fetch=True
    )
    
    if existing_request:
        log_action(f"Bot {member} ({member.id}) has a pending approval request", guild_id=guild.id)
        # still kick the bot even if there's a pending request
        # to prevent unauthorized access during review
    
    # get the audit log to find who invited the bot
    inviter_id = None
    try:
        async for entry in guild.audit_logs(
            action=discord.AuditLogAction.bot_add,
            limit=10
        ):
            if entry.target.id == member.id:
                inviter_id = entry.user.id
                break
    except Exception as e:
        log_action(f"Failed to fetch audit log: {e}", level='error')
    
    if not inviter_id:
        # if we can't find who invited the bot, use the server owner
        inviter_id = guild.owner_id
    
    # create a new approval request
    db.execute_query(
        """
        INSERT OR IGNORE INTO bot_approval_requests 
        (bot_id, bot_name, inviter_id, guild_id, status)
        VALUES (?, ?, ?, ?, 'pending')
        """,
        (member.id, str(member), inviter_id, guild.id)
    )
    
    # kick the bot immediately
    try:
        await member.kick(reason="Bot requires approval from server owner")
        kick_success = True
    except Exception as e:
        log_action(f"Failed to kick bot: {e}", level='error')
        kick_success = False
    
    # get the inviter to notify them
    inviter = guild.get_member(inviter_id)
    
    if inviter:
        embed = discord.Embed(
            title="🤖 Bot Approval Required",
            description=(
                f"You've invited the bot **{member}** to **{guild.name}**.\n"
                "This server requires bot approvals. Please provide the bot's website or invite URL for review."
            ),
            color=discord.Color.orange()
        )
        
        embed.add_field(name="Bot ID", value=f"`{member.id}`", inline=True)
        embed.add_field(name="Server", value=guild.name, inline=True)
        
        view = ui.View(timeout=86400)  # 24 hour timeout
        view.add_item(BotApprovalButton(member.id, inviter_id, guild.id))
        
        try:
            await inviter.send(embed=embed, view=view)
        except Exception as e:
            log_action(f"Failed to send DM to inviter: {e}", level='error')
    
    log_action(
        f"Bot {member} ({member.id}) joined and requires approval. "
        f"Inviter: {inviter} ({inviter_id}), Kick success: {kick_success}",
        guild_id=guild.id
    )
    
    # notify server owner if different from inviter
    if guild.owner_id != inviter_id and guild.owner:
        try:
            owner_embed = discord.Embed(
                title="⚠️ Bot Requires Approval",
                description=(
                    f"A bot was invited to your server **{guild.name}** by {inviter.mention if inviter else 'an unknown user'}.\n"
                    f"Bot: {member.mention} (`{member.id}`)\n"
                    "The bot has been temporarily kicked until approved."
                ),
                color=discord.Color.orange()
            )
            
            await guild.owner.send(embed=owner_embed)
        except Exception as e:
            log_action(f"Failed to notify server owner: {e}", level='error')
