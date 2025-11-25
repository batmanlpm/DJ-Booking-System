"""
security event handlers for the bot.
"""
import discord
from discord.ext import commands
import logging

# import bot security handlers
from .bot_security import on_bot_join

logger = logging.getLogger('discord.security.events')

async def setup(bot):
    """set up security event handlers."""
    @bot.event
    async def on_member_join(member):
        """handle new member joins for security checks."""
        try:
            # handle bot joins
            if member.bot:
                await on_bot_join(bot, member)
                return
                
            # add any additional security checks for new human members here
            
        except Exception as e:
            logger.error(f"Error in on_member_join for {member}: {e}", exc_info=True)
    
    @bot.event
    async def on_member_update(before, after):
        """handle member updates for security checks."""
        try:
            # check for role changes that might indicate admin privileges being added
            if before.roles != after.roles:
                # get the roles that were added
                added_roles = [role for role in after.roles if role not in before.roles]
                
                # check if any of the added roles have admin permissions
                for role in added_roles:
                    if role.permissions.administrator or role.permissions.manage_guild:
                        # log this potentially sensitive action
                        logger.warning(
                            f"Admin role '{role.name}' added to {after} in {after.guild}"
                        )
                        # you could add additional checks or notifications here
                        
        except Exception as e:
            logger.error(f"Error in on_member_update: {e}", exc_info=True)
    
    @bot.event
    async def on_message(message):
        """handle all messages for security checks."""
        try:
            # skip messages from bots
            if message.author.bot:
                return
                
            # add any message-based security checks here
            
        except Exception as e:
            logger.error(f"Error in on_message: {e}", exc_info=True)
        
        # allow other event handlers to process the message
        await bot.process_commands(message)
    
    @bot.event
    async def on_guild_join(guild):
        """handle when the bot joins a new guild."""
        try:
            logger.info(f"Joined new guild: {guild.name} (ID: {guild.id}) with {guild.member_count} members")
            
            # you might want to set up default security settings here
            # or notify the server owner about security features
            
        except Exception as e:
            logger.error(f"Error in on_guild_join: {e}", exc_info=True)
    
    @bot.event
    async def on_guild_remove(guild):
        """handle when the bot is removed from a guild."""
        try:
            logger.info(f"Removed from guild: {guild.name} (ID: {guild.id})")
            
            # clean up any guild-specific data
            
        except Exception as e:
            logger.error(f"Error in on_guild_remove: {e}", exc_info=True)
    
    return True
