"""bot admin decorators and utilities"""
import discord
from discord import app_commands
from discord.ext import commands
from typing import Optional, List, Any, Callable, Coroutine
import functools
import logging
import config

logger = logging.getLogger(__name__)

class NotBotAdmin(commands.CheckFailure):
    """raised when a user is not a bot admin"""
    pass

def is_bot_admin() -> Callable[[discord.Interaction], Coroutine[Any, Any, bool]]:
    """check if the user is a bot admin"""
    async def predicate(interaction: discord.Interaction) -> bool:
        if not hasattr(interaction, 'user') or not interaction.user:
            logger.warning('interaction.user is missing')
            raise NotBotAdmin()
            
        user_id = getattr(interaction.user, 'id', None)
        if not user_id:
            logger.warning('interaction.user.id is missing')
            raise NotBotAdmin()
            
        if user_id in config.BOT_ADMINS:
            logger.debug(f'user {user_id} is a global bot admin')
            return True
            
        logger.debug(f'user {user_id} is not a bot admin')
        raise NotBotAdmin()
    return app_commands.check(predicate)

def bot_admin_command(**perms):
    """decorator for bot admin commands"""
    def decorator(func):
        @functools.wraps(func)
        @app_commands.checks.has_permissions(**perms)
        @is_bot_admin()
        async def wrapper(interaction: discord.Interaction, *args, **kwargs):
            return await func(interaction, *args, **kwargs)
        return wrapper
    return decorator

async def is_bot_admin_check(user_id: int) -> bool:
    """check if a user is a bot admin"""
    if not isinstance(user_id, int):
        logger.warning(f'invalid user_id type: {type(user_id)}')
        return False
    is_admin = user_id in config.BOT_ADMINS
    logger.debug(f'is_bot_admin_check({user_id}): {is_admin}')
    return is_admin

async def add_bot_admin(user_id: int) -> bool:
    """add a user to the bot admins list"""
    if user_id not in config.BOT_ADMINS:
        config.BOT_ADMINS.append(user_id)
        return True
    return False

async def remove_bot_admin(user_id: int) -> bool:
    """remove a user from the bot admins list"""
    if user_id in config.BOT_ADMINS:
        config.BOT_ADMINS.remove(user_id)
        return True
    return False

async def setup(bot):
    """setup error handlers for bot admin commands"""
    @bot.tree.error
    async def on_app_command_error(interaction: discord.Interaction, error: app_commands.AppCommandError, /) -> None:
        if isinstance(error, NotBotAdmin):
            if interaction.response.is_done():
                await interaction.followup.send(
                    "❌ you do not have permission to use this command.",
                    ephemeral=True
                )
            else:
                await interaction.response.send_message(
                    "❌ you do not have permission to use this command.",
                    ephemeral=True
                )
            return
            
        if isinstance(error, app_commands.MissingPermissions):
            if interaction.response.is_done():
                await interaction.followup.send(
                    f"❌ you are missing the following permissions: {', '.join(error.missing_permissions)}",
                    ephemeral=True
                )
            else:
                await interaction.response.send_message(
                    f"❌ you are missing the following permissions: {', '.join(error.missing_permissions)}",
                    ephemeral=True
                )
            return
            
        # Log other errors
        logger.error(f'Unhandled error in command {interaction.command.name if interaction.command else "unknown"}:', exc_info=error)
        
        if not interaction.response.is_done():
            try:
                await interaction.response.send_message(
                    "❌ an error occurred while processing your command.",
                    ephemeral=True
                )
            except discord.NotFound:
                logger.warning('Interaction not found when trying to send error response')
            except Exception as e:
                logger.error(f'Error sending error response: {e}')
