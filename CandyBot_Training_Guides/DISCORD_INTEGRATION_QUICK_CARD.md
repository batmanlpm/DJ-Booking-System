# ?? Discord Chat Integration - Quick Setup Card

## ? Quick Start (5 Minutes)

### 1?? Create Bot (2 min)
```
?? https://discord.com/developers/applications
   ? New Application ? Bot ? Add Bot
   ? Copy Token ??
   ? Enable "MESSAGE CONTENT INTENT" ?
```

### 2?? Invite Bot (1 min)
```
OAuth2 ? URL Generator
   ? Scope: bot
   ? Permissions: Send Messages, Read Messages, Read Message History
   ? Copy URL ? Open in Browser ? Authorize
```

### 3?? Get Channel ID (1 min)
```
Discord Settings ? Advanced ? Enable Developer Mode
Right-click channel ? Copy ID
```

### 4?? Connect in App (1 min)
```
DJ Booking System ? Chat View ? Connect Button
   ?? Bot Token: [paste]
   ?? Channel ID: [paste]
   ?? Channel Name: #general
   ? Test Connection ? Save ?
```

## ?? Visual Guide

### Message Types
```
???????????????????????????????????????
? ?? You · 14:30                      ?  ? Local (Green)
? Hello from the app!                 ?
???????????????????????????????????????

???????????????????????????????????????
? ?? JohnDJ (Discord) · 14:31        ?  ? Discord (Purple)
? Hey! Got the message!               ?
???????????????????????????????????????

???????????????????????????????????????
? ? Discord connected successfully!  ?  ? System (Gold)
???????????????????????????????????????
```

## ?? Features

### ? What Works
- ? Send messages to Discord from app
- ? Receive Discord messages in real-time
- ? Auto-reconnect on app restart
- ? Visual indicators for message source
- ? Connection status monitoring
- ? Settings persistence

### ?? Usage
1. **Send to Discord**: Type message ? Press Enter
2. **Receive from Discord**: Messages appear automatically
3. **Toggle Discord**: Use checkbox to enable/disable sending
4. **Disconnect**: Click Disconnect button
5. **Reconnect**: Click Connect button

## ?? Troubleshooting

| Problem | Solution |
|---------|----------|
| Bot not receiving messages | Enable "MESSAGE CONTENT INTENT" in Developer Portal |
| Connection failed | Verify token, channel ID, and bot is in server |
| Messages not sending | Check "Send Messages" permission |
| Bot shows offline | Reconnect in app |

## ?? Requirements

### Bot Permissions Needed
- ? Read Messages/View Channels
- ? Send Messages  
- ? Read Message History

### Gateway Intents (REQUIRED!)
- ? **MESSAGE CONTENT INTENT** ?? Must be enabled!
- ? Guilds
- ? Guild Messages

## ?? Security Notes
- Token stored locally: `%USERPROFILE%\Documents\DJBookingSystem\discord_settings.json`
- Keep your bot token secret
- Don't share discord_settings.json file
- Delete file to reset credentials

## ?? Pro Tips

1. **Test First**: Always use "Test Connection" before saving
2. **Multiple Channels**: Create separate bots for different channels
3. **Permissions**: Give bot only needed permissions
4. **Naming**: Use descriptive names like "DJ-Booking-Bot"
5. **Auto-connect**: Settings saved automatically, bot reconnects on restart

## ?? Help Resources

- **Discord Bot Setup**: Click "?? Setup Instructions" in connection dialog
- **Full Guide**: See `239.DISCORD_CHAT_INTEGRATION_COMPLETE.md`
- **Discord Dev Portal**: https://discord.com/developers/applications
- **Discord.Net Docs**: https://docs.discordnet.dev/

## ?? Success Indicators

You'll know it's working when:
1. ? Status shows "Connected to #channel-name" in green
2. ? Test message appears in Discord
3. ? Discord messages appear in app with purple border
4. ? Bot shows online in Discord server

---

**Integration by**: Discord.Net 3.16.0  
**Last Updated**: 2024  
**Status**: ? Production Ready
