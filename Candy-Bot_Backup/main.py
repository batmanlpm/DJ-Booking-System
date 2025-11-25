import asyncio
import os
import sys
import subprocess
import importlib.metadata
from typing import List

# install requirements before any other imports
def install_requirements():
    """installs required packages from requirements.txt if they're not already installed"""
    requirements_file = os.path.join(os.path.dirname(__file__), 'requirements.txt')
    
    if not os.path.exists(requirements_file):
        print('warning: requirements.txt not found, skipping package installation')
        return
    
    with open(requirements_file, 'r') as f:
        requirements = [line.strip() for line in f if line.strip() and not line.startswith('#')]
    
    installed = {}
    try:
        installed = {pkg.metadata['Name'].lower(): pkg.version 
                    for pkg in importlib.metadata.distributions()}
    except Exception as e:
        print(f'warning: could not check installed packages: {e}')
    
    to_install = []
    for req in requirements:
        # handle version specifiers
        pkg_name = req.split('>=')[0].split('==')[0].strip().lower()
        if pkg_name not in installed:
            to_install.append(req)
    
    if to_install:
        print(f'installing required packages: {", ".join(to_install)}')
        try:
            subprocess.check_call([sys.executable, '-m', 'pip', 'install', *to_install])
            print('successfully installed required packages')
            # reload modules after installation
            importlib.invalidate_caches()
        except subprocess.CalledProcessError as e:
            print(f'error: failed to install packages: {e}')
            sys.exit(1)

# install requirements before any other imports
install_requirements()

# now import other dependencies
import logging
from discord.ext import commands
from discord.ext.commands import CommandNotFound, MissingPermissions, MissingRequiredArgument, BadArgument
from discord import app_commands
import discord
import config
import json
import os
import sys
from utils.logger import get_logger
from utils.bot_admin import setup as setup_bot_admin

# set up logging
logger = get_logger('bot')

# bot configuration
intents = discord.Intents.default()
intents.message_content = True
intents.members = True  # required to access member lists
intents.presences = True  # helps with member status
intents.guilds = True  # required for guild information

class HelpCog(commands.Cog):
    def __init__(self, bot):
        self.bot = bot
        self._original_help_command = bot.help_command
        bot.help_command = None  # Remove the default help command
        
    def cog_unload(self):
        self.bot.help_command = self._original_help_command
    
    @app_commands.command(
        name="help",
        description="Show all available commands"
    )
    async def help_command(self, interaction: discord.Interaction):
        """show all available commands in a nice embed."""
        embed = discord.Embed(
            title='🎵 Candy-Bot Commands',
            description='Here are all the available commands:',
            color=0x2f3136  # Dark gray color
        )
        
        # Add commands to embed
        commands_list = [
            ('/admin', 'Manage bot administrators and edit security settings'),
            ('/candy', 'Visit Candy\'s Memorial & Listen To Her Music'),
            ('/currently-playing', 'Show information about the currently playing track'),
            ('/event_delete', 'Delete a scheduled event'),
            ('/event_list', 'List all scheduled events'),
            ('/event_schedule', 'Schedule a new event'),
            ('/event_setchannel', 'Set the channel for event notifications'),
            ('/help', 'Show this help message'),
            ('/join', 'Join your voice channel and start streaming the radio'),
            ('/leave', 'Disconnect from voice channel'),
            ('/lock-channel', 'Lock the current channel to prevent messages'),
            ('/mytimezone', 'Check your current timezone setting'),
            ('/request', 'Request a song to be played on the radio'),
            ('/timezone', 'Set your timezone for event notifications'),
            ('/timezone_list', 'List all available timezones'),
            ('/unlock-channel', 'Unlock the current channel')
        ]
        
        for cmd, desc in commands_list:
            embed.add_field(name=cmd, value=desc, inline=False)
            
        embed.set_footer(text='Use /help to see this message again')
        
        await interaction.response.send_message(embed=embed, ephemeral=True)
        
    @app_commands.command(
        name="candy",
        description="Visit Candy's Memorial & Listen To Her Music"
    )
    async def candy_command(self, interaction: discord.Interaction):
        """send a link to candy's memorial website."""
        embed = discord.Embed(
            title='🎵 In Loving Memory of DJCANDY',
            description='[Click here to visit Candy\'s Memorial & Listen To Her Music](https://djcandy.neocities.org/)',
            color=0x2f3136
        )
        await interaction.response.send_message(embed=embed, ephemeral=True)

# create bot instance
bot = commands.Bot(
    command_prefix=config.BOT_PREFIX,
    intents=intents,
    case_insensitive=True
)

async def load_extensions():
    """Load all modules from the modules directory."""
    module_logger = get_logger('extensions')
    
    # Add the project root to the python path
    project_root = os.path.dirname(os.path.abspath(__file__))
    if project_root not in sys.path:
        sys.path.insert(0, project_root)
    
    modules_dir = os.path.join(project_root, 'modules')
    
    # Define the modules to load in order
    modules_to_load = [
        'admin',      # Load admin first to ensure bot admin commands are available
        'request',
        'voice',
        'radioboss',
        'database',
        'events',
        'moderation', # Moderation commands and functionality
        'security',   # This will load all security components including admin_action_tracker
        'welcome',    # Welcome messages and member onboarding
        'dj_booking'  # DJ booking system for managing DJ schedules
    ]
    
    loaded_modules = []
    failed_modules = []
    
    # Set up bot admin error handler
    try:
        await setup_bot_admin(bot)
        module_logger.info('Successfully set up bot admin error handler')
    except Exception as e:
        module_logger.error(f'Failed to set up bot admin error handler: {e}')
    
    # Load the help cog
    try:
        await bot.add_cog(HelpCog(bot))
        module_logger.info('Successfully loaded help command')
    except Exception as e:
        module_logger.error(f'Failed to load help command: {e}')
        failed_modules.append('help')
    
    # Load other modules
    for module_name in modules_to_load:
        try:
            await bot.load_extension(f'modules.{module_name}')
            loaded_modules.append(module_name)
            module_logger.info(f'Successfully loaded module: {module_name}')
        except Exception as e:
            module_logger.error(f'Failed to load module {module_name}: {e}')
            failed_modules.append(module_name)
    
    return loaded_modules, failed_modules

@bot.event
async def on_ready():
    """Event triggered when the bot is ready."""
    logger.info(f'Logged in as {bot.user.name} (ID: {bot.user.id})')
    logger.info(f'Discord.py version: {discord.__version__}')
    
    # Set custom status
    try:
        activity = discord.Activity(
            type=discord.ActivityType.listening,
            name=f'your requests! | {config.BOT_PREFIX}help'
        )
        await bot.change_presence(activity=activity)
        logger.info('Bot presence updated successfully')
    except Exception as e:
        logger.error(f'Failed to set bot presence: {str(e)}')
    
    # Load all extensions
    logger.info('Loading extensions...')
    await load_extensions()
    
    logger.info('Bot is ready and operational')
    
    # Sync commands with detailed logging
    logger.info('Syncing commands...')
    try:
        # Get all global commands
        global_commands = await bot.tree.sync()
        logger.info(f'Synced {len(global_commands)} global command(s): {[cmd.name for cmd in global_commands]}')
        
        # Get guild-specific commands
        for guild in bot.guilds:
            try:
                guild_commands = await bot.tree.sync(guild=guild)
                logger.info(f'Guild: {guild.name} (ID: {guild.id})')
                if guild_commands:
                    for cmd in guild_commands:
                        logger.info(f'  /{cmd.name} - {cmd.description}')
                else:
                    logger.info('  No guild-specific commands found')
            except Exception as e:
                logger.error(f'Failed to sync commands for guild {guild.name} ({guild.id}): {e}')
        
        # Log all available commands in the command tree
        logger.info('All available commands in command tree:')
        for cmd in bot.tree.walk_commands():
            if isinstance(cmd, app_commands.Command):
                logger.info(f'  /{cmd.name} - {cmd.description}')
                
    except Exception as e:
        logger.error(f'Error syncing commands: {e}', exc_info=True)

async def main():
    """main function to start the bot"""
    logger.info('Starting bot...')
    
    try:
        async with bot:
            logger.info('Bot instance created, starting connection...')
            await bot.start(config.DISCORD_BOT_TOKEN)
    except discord.LoginFailure:
        logger.critical('Failed to log in. Please check your bot token.')
        sys.exit(1)
    except Exception as e:
        logger.critical(f'Unexpected error: {str(e)}', exc_info=True)
        sys.exit(1)
    finally:
        logger.info('Bot has shut down')

if __name__ == '__main__':
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        logger.info('Bot shutdown requested by user')
    except Exception as e:
        logger.critical(f'Fatal error: {str(e)}', exc_info=True)
        sys.exit(1)
