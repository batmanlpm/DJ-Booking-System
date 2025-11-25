"""
configuration settings for the discord bot.

this file contains all the necessary configuration variables for the bot.
please create a .env file in the root directory with the following variables:
- DISCORD_BOT_TOKEN: your discord bot token
- BOT_PREFIX: command prefix (default: '!')
- RADIOBOSS_API_URL: url for the radioboss api
- RADIOBOSS_API_PASSWORD: password for the radioboss api
- RADIOBOSS_OWNER_DM_USER_ID: discord user id for owner dms
- RADIOBOSS_STREAM_URL: url for the radio stream
"""

import os
from dotenv import load_dotenv

# load environment variables from .env file
load_dotenv()

# bot configuration
DISCORD_BOT_TOKEN = 'MTM5NTM5MjAxNzA0MzAzNDExMg.Gi59Vi.NOb8pL8fXP7WD4e3x0yveNUAyJ8hZSVsLw90HQ'  # your discord bot token
BOT_PREFIX = '!'  # command prefix
BOT_ADMINS = [1333179341118636032,1290890068709609544,952442449756848158]  # list of user IDs with bot admin privileges

DJ_ADMIN_PIN = "2569"  # default pin for DJ admin access

# radioboss configuration
RADIOBOSS_API_URL = 'https://c4.radioboss.fm/api'  # this might not be accessible
RADIOBOSS_API_KEY = '9CT2KPOHAG9N'  # your radioboss api key
RADIOBOSS_OWNER_DM_USER_ID = 1333179341118636032  # your discord user id for dms

# station configurations
STATIONS = {
    'LPM': {
        'name': 'LivePartyMusic.fm',
        'url': 'https://c4.radioboss.fm:8560/stream',
        'website': 'https://livepartymusic.fm',
        'thumbnail': 'https://livepartymusic.fm/lpm/wp-content/uploads/2025/04/Transparent-Logo.webp'
    },
    'BSC': {
        'name': 'BatShit Crazies',
        'url': 'https://c32.radioboss.fm:8806/stream',
        'website': 'http://batshitcrazies.rocks/',
        'thumbnail': 'https://cdn.discordapp.com/attachments/1388545094709285038/1398967086872789083/The_Batshit_Crazies_logo.png?ex=68874921&is=6885f7a1&hm=daebd37c9eb747af891a4fe4bcd5503f7d3dba8710a8465dfcd43f656aa8ab45&'  # Replace with actual BSC thumbnail
    },
    'APO': {
        'name': 'Apollo11.Party',
        'url': 'https://c6.radioboss.fm:8366/stream',
        'website': '',
        'thumbnail': 'https://cdn.discordapp.com/attachments/1388545094709285038/1398967087191425104/apollo11_logo.png?ex=68874921&is=6885f7a1&hm=9e4cffa613691003515aa99e0f619c34535fc3ef22d6e1e4669e534e6489f6c9&'  # Replace with actual APO thumbnail
    },
    'MIS': {
        'name': 'Misfits Studios',
        'url': 'https://c19.radioboss.fm:8162/stream',
        'website': '',
        'thumbnail': 'https://cdn.discordapp.com/attachments/1388545094709285038/1399122799523790889/Misfits_Studios.png?ex=6887da25&is=688688a5&hm=38f8853b67733a7f47e112fdb21707d93e6eb0a2cf74bd68cfb6b3d8af575650&'  # Replace with actual MIS thumbnail
    }
}

# default station (for backward compatibility)
DEFAULT_STATION = 'LPM'
RADIOBOSS_STREAM_URL = STATIONS[DEFAULT_STATION]['url']

# validate required configuration
required_configs = {
    'DISCORD_BOT_TOKEN': DISCORD_BOT_TOKEN,
    'RADIOBOSS_API_KEY': RADIOBOSS_API_KEY,
    'RADIOBOSS_OWNER_DM_USER_ID': RADIOBOSS_OWNER_DM_USER_ID,
    'STATIONS': STATIONS,
    'RADIOBOSS_STREAM_URL': RADIOBOSS_STREAM_URL
}

for name, value in required_configs.items():
    if not value:
        raise ValueError(f'missing required configuration: {name}')
