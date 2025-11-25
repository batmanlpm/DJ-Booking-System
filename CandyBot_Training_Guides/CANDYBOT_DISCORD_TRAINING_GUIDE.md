# ?? CandyBot Training Guide - Complete Discord Integration

## ?? Table of Contents
1. [Overview](#overview)
2. [Discord Setup](#discord-setup)
3. [Basic Operations](#basic-operations)
4. [Channel Messaging](#channel-messaging)
5. [Direct Messaging](#direct-messaging)
6. [Troubleshooting](#troubleshooting)
7. [Training Commands](#training-commands)
8. [Advanced Features](#advanced-features)

---

## Overview

### What is CandyBot?
CandyBot is your AI-powered Discord assistant integrated into the DJ Booking System. It can:
- ?? Send and receive messages in Discord channels
- ?? Send and receive direct messages to/from users
- ?? Manage DJ bookings and notify via Discord
- ?? Interact with your Discord community in real-time
- ?? Switch between channel and DM modes seamlessly

### System Requirements
- ? Windows 10/11
- ? .NET 8 Runtime
- ? Discord Bot Account
- ? Internet Connection
- ? DJ Booking System installed

---

## Discord Setup

### Part 1: Create Discord Bot

#### Step 1: Go to Discord Developer Portal
```
?? https://discord.com/developers/applications
```

#### Step 2: Create Application
1. Click **"New Application"**
2. Name it: `CandyBot` (or your preferred name)
3. Click **"Create"**

#### Step 3: Add Bot
1. Left sidebar ? Click **"Bot"**
2. Click **"Add Bot"**
3. Click **"Yes, do it!"**

#### Step 4: Get Bot Token
1. Under **TOKEN** section
2. Click **"Reset Token"**
3. Click **"Yes, do it!"**
4. Click **"Copy"** to copy the token
5. ?? **SAVE THIS TOKEN SECURELY** - You'll need it later
6. ?? **NEVER SHARE THIS TOKEN PUBLICLY**

#### Step 5: Enable Privileged Intents
Scroll down to **"Privileged Gateway Intents"**

Enable ALL THREE:
- ? **PRESENCE INTENT** (for online/offline status)
- ? **SERVER MEMBERS INTENT** (CRITICAL - for user list)
- ? **MESSAGE CONTENT INTENT** (CRITICAL - for reading messages)

Click **"Save Changes"**

### Part 2: Invite Bot to Your Server

#### Step 6: Generate Invite URL
1. Left sidebar ? Click **"OAuth2"**
2. Click **"URL Generator"**

#### Step 7: Select Scopes
Under **SCOPES**, check:
- ? `bot`

#### Step 8: Select Permissions
Under **BOT PERMISSIONS**, check:
- ? Read Messages/View Channels
- ? Send Messages
- ? Read Message History
- ? Embed Links
- ? Attach Files
- ? Mention Everyone (optional)

#### Step 9: Invite Bot
1. Copy the **Generated URL** at bottom
2. Open URL in browser
3. Select your Discord server
4. Click **"Authorize"**
5. Complete captcha if prompted

### Part 3: Get Channel ID

#### Step 10: Enable Developer Mode
1. Open Discord
2. Click **User Settings** (gear icon)
3. Go to **Advanced**
4. Enable **"Developer Mode"**
5. Close settings

#### Step 11: Copy Channel ID
1. Right-click the channel you want to use
2. Click **"Copy ID"**
3. Save this ID - you'll need it in the app

---

## Basic Operations

### Connecting CandyBot to Discord

#### In DJ Booking System:

**Step 1: Open Chat View**
```
Main Menu ? Chat
```

**Step 2: Click Connect**
```
Click "CONNECT" button in Discord Integration panel
```

**Step 3: Enter Credentials**
```
??????????????????????????????????????
? Bot Token: [paste token here]     ?
? Channel ID: [paste ID here]       ?
? Channel Name: #general             ?
??????????????????????????????????????
```

**Step 4: Test Connection**
```
Click "TEST CONNECTION" button
```

**Step 5: Save**
```
If test successful, click "SAVE"
```

### Understanding the Interface

```
???????????????????????????????????????????????????????
? DISCORD INTEGRATION              Status: Connected  ?
? Connected to #general                               ?
? [DISCONNECT]                                        ?
???????????????????????????????????????????????????????
?                                                     ?
? ???????????????????????????????????????????????   ?
? ? [Messages appear here]                      ?   ?
? ?                                             ?   ?
? ? You - 14:30                                 ?   ?
? ? Hello from CandyBot!                        ?   ?
? ?                                             ?   ?
? ? JohnDJ (Discord) - 14:31                    ?   ?
? ? Hey! Got your message!                      ?   ?
? ???????????????????????????????????????????????   ?
?                                                     ?
???????????????????????????????????????????????????????
? Mode: ? Channel  ? Direct Message                  ?
? [Type message here...] [?Discord] [SEND]           ?
???????????????????????????????????????????????????????
```

---

## Channel Messaging

### Sending Messages to Channel

**Step 1: Ensure Channel Mode**
```
Select "Channel" radio button
```

**Step 2: Type Your Message**
```
Click in message box
Type your message
```

**Step 3: Enable Discord**
```
? Check "Discord" checkbox
```

**Step 4: Send**
```
Click "SEND" or press Enter
```

### Message Types

#### Local Message (Your Message)
```
??????????????????????????????????
? You - 14:30                    ?  ? Blue header
? This is my message             ?  ? Green text
??????????????????????????????????
  Green border
```

#### Discord Message (From Discord)
```
??????????????????????????????????
? Username (Discord) - 14:31     ?  ? Purple header
? This is from Discord           ?  ? White text
??????????????????????????????????
  Purple border
```

#### System Message
```
??????????????????????????????????
? Connected to Discord           ?  ? Gold text
??????????????????????????????????
  Gold border (italic)
```

### Receiving Messages from Discord

Messages appear **automatically** in real-time!

**What happens:**
1. User types in Discord channel
2. Message appears in your app within 1 second
3. Shows: Username, "Discord" label, timestamp

**No action needed** - just watch for new messages!

---

## Direct Messaging

### Understanding DM Mode

**DM Mode allows you to:**
- ?? Send private messages to specific Discord users
- ?? Receive private messages from users
- ?? Switch between different user conversations
- ?? Keep conversation history during session

### Switching to DM Mode

**Step 1: Click Direct Message**
```
? Channel  ?  ? Direct Message
```

**Step 2: Select User**
```
Click "SELECT USER" button
```

**Step 3: Choose Recipient**
```
???????????????????????????????????
? SELECT DISCORD USER             ?
???????????????????????????????????
? Search: [type name here]        ?
???????????????????????????????????
? ? JohnDJ            Online      ?
? ? SarahBass         Offline     ?
? ? MikeMixer         Online      ?
? ? LisaDrummer       Offline     ?
???????????????????????????????????
```

**Step 4: Confirm Selection**
```
Double-click user OR
Select user ? Click "SELECT"
```

### Sending Direct Messages

**After selecting user:**

1. Type your message
2. ? Check "Discord" checkbox
3. Click "SEND" or press Enter

**Result:**
- Message sent to that user privately
- Message appears in your chat
- User receives DM in Discord

### Receiving Direct Messages

**Scenario A: Viewing That User's DM**
```
Message appears inline in conversation
No notification needed
```

**Scenario B: In Channel Mode or Different DM**
```
??????????????????????????????????????
? New DM from JohnDJ: Hey, are you...?  ? Blue notification
??????????????????????????????????????
```

**To view full DM:**
1. Switch to DM mode
2. Click "SELECT USER"
3. Choose that user
4. Full conversation appears

### User Selector Features

#### Search Functionality
```
Search: john
         ?
Results: 
? JohnDJ         Online
? Johnny123      Online
```

#### Status Indicators
```
? Green Dot  = Online
? Gray Dot   = Offline
```

#### User Information
```
JohnDJ                    ? Display Name (bold)
@johndj_official          ? Username (gray)
Online                    ? Status
```

### Managing Multiple Conversations

**Switching Between Users:**
1. Click "SELECT USER"
2. Choose different user
3. Previous conversation preserved
4. New conversation loads

**Conversation Persistence:**
- ? Stays in memory during session
- ? Switch back anytime
- ? Lost when app closes

---

## Troubleshooting

### Connection Issues

#### Problem: "Connection Failed"
**Symptoms:**
```
? Connection failed: Unable to connect
```

**Solutions:**
1. **Check bot token**
   - Go to Discord Developer Portal
   - Bot section ? Reset Token
   - Copy NEW token
   - Update in app

2. **Check intents**
   - Verify all 3 intents enabled
   - Save changes
   - Wait 30 seconds
   - Try again

3. **Check internet**
   - Test Discord.com in browser
   - Restart router if needed

#### Problem: "Invalid Channel ID"
**Symptoms:**
```
? Invalid channel ID format
```

**Solutions:**
1. **Enable Developer Mode** in Discord
2. **Right-click** the channel
3. Click **"Copy ID"**
4. Paste **entire numeric ID**

### User List Issues

#### Problem: "No users found"
**Symptoms:**
```
User selector shows empty list
```

**Solutions:**
1. **Check SERVER MEMBERS INTENT**
   - Discord Developer Portal
   - Bot section
   - Enable "SERVER MEMBERS INTENT"
   - Save

2. **Reset bot token**
   - Reset Token in portal
   - Copy NEW token
   - Reconnect in app

3. **Verify bot in server**
   - Check bot appears in member list
   - Check bot has role permissions

4. **Wait after connection**
   - Bot needs 5-10 seconds to download users
   - Try "SELECT USER" again

#### Problem: "Guild Members Intent Error"
**Symptoms:**
```
? Missing required gateway intent GuildMembers
```

**Solutions:**
1. **Enable in Discord Portal**
   - Bot section
   - Toggle "SERVER MEMBERS INTENT" ON
   - Save changes

2. **Reset token**
   - Must reset after enabling intent
   - Copy new token
   - Update in app

3. **Restart app**
   - Close completely
   - Wait 30 seconds
   - Reopen
   - Reconnect

### Messaging Issues

#### Problem: "Messages not sending"
**Symptoms:**
```
? Discord send failed
```

**Solutions:**
1. **Check Discord checkbox**
   - Must be checked to send
   - Green checkmark = enabled

2. **Check connection**
   - Status should show "Connected"
   - Green text

3. **Check permissions**
   - Bot needs "Send Messages"
   - In both channel settings and role

#### Problem: "Messages not receiving"
**Symptoms:**
```
No Discord messages appear
```

**Solutions:**
1. **Check MESSAGE CONTENT INTENT**
   - Must be enabled
   - Reset token after enabling

2. **Check channel visibility**
   - Bot must see the channel
   - Check role permissions

3. **Reconnect bot**
   - Disconnect
   - Wait 10 seconds
   - Reconnect

#### Problem: "DMs not sending"
**Symptoms:**
```
? Send DM error
```

**Solutions:**
1. **User hasn't blocked bot**
   - Ask user to unblock
   - Try different user

2. **User has DMs enabled**
   - User Settings ? Privacy & Safety
   - Enable "Allow DMs from server members"

3. **Both in same server**
   - Bot and user must share server
   - Can't DM users from different servers

### Performance Issues

#### Problem: "App freezing when loading users"
**Symptoms:**
```
App becomes unresponsive
```

**Solutions:**
1. **Large server (100+ members)**
   - Wait 30 seconds for download
   - Will complete eventually

2. **Slow internet**
   - Check connection speed
   - Try again when stable

3. **Restart app**
   - Close completely
   - Reopen
   - Reconnect

---

## Training Commands

### Essential CandyBot Commands

#### Connection Commands
```
Connect to Discord    ? Click CONNECT button
Test Connection       ? Click TEST CONNECTION
Disconnect           ? Click DISCONNECT button
Save Settings        ? Click SAVE button
```

#### Messaging Commands
```
Send to Channel      ? Channel mode + Discord ?
Send DM              ? DM mode + Select User + Discord ?
Switch to Channel    ? Click "Channel" radio
Switch to DM         ? Click "Direct Message" radio
Select User          ? Click "SELECT USER"
Search User          ? Type in search box
```

#### Keyboard Shortcuts
```
Send Message         ? Enter key
New Line            ? Shift + Enter
Clear Message       ? Escape (focus message box first)
```

### Training Scenarios

#### Scenario 1: First Connection
```
Objective: Connect CandyBot to Discord for first time

Steps:
1. Have bot token ready
2. Have channel ID ready
3. Click CONNECT
4. Enter credentials
5. Click TEST CONNECTION
6. Wait for "Connection successful" message
7. Click SAVE

Success Criteria:
? Status shows "Connected to #channel-name"
? Status text is green
? Test message appears in Discord
```

#### Scenario 2: Send Channel Message
```
Objective: Send message to Discord channel

Steps:
1. Verify "Channel" mode selected
2. Type: "Hello Discord community!"
3. Check "Discord" checkbox
4. Click SEND or press Enter

Success Criteria:
? Message appears in local chat
? Message appears in Discord channel
? No error messages
```

#### Scenario 3: Send Direct Message
```
Objective: Send private DM to Discord user

Steps:
1. Click "Direct Message" radio
2. Click "SELECT USER"
3. Search for user or scroll to find
4. Double-click user or select + SELECT
5. Verify "DM with [Username]" appears
6. Type: "Hi, checking in with you!"
7. Check "Discord" checkbox
8. Click SEND

Success Criteria:
? Message appears in local chat
? User receives DM in Discord
? "DM with [Username]" shows correct user
```

#### Scenario 4: Receive and Reply to DM
```
Objective: Respond to incoming Discord DM

Expected Behavior:
1. User sends DM to bot in Discord
2. Notification appears in app:
   "New DM from [User]: [Message preview]..."
3. Click "Direct Message" mode
4. Click "SELECT USER"
5. Choose the user who sent DM
6. See full conversation
7. Type reply
8. Check "Discord" checkbox
9. Send reply

Success Criteria:
? DM notification appeared
? Conversation loaded correctly
? Reply sent successfully
? User receives reply in Discord
```

#### Scenario 5: Switch Between Conversations
```
Objective: Manage multiple DM conversations

Steps:
1. In DM mode
2. Send DM to User A
3. Click "SELECT USER"
4. Choose User B
5. Send DM to User B
6. Click "SELECT USER"
7. Choose User A again
8. Verify User A conversation still there

Success Criteria:
? Each conversation preserved
? Messages appear in correct conversation
? No mixing of conversations
```

### Common Training Mistakes

#### ? Mistake 1: Forgetting Discord Checkbox
```
Problem:
- Type message
- Click SEND
- Message only shows locally
- Doesn't go to Discord

Solution:
- ALWAYS check "Discord" checkbox before sending
- Checkbox must be ? checked
```

#### ? Mistake 2: Wrong Mode Selected
```
Problem:
- Want to send to channel
- But DM mode selected
- Or vice versa

Solution:
- Check radio button before typing
- ? Channel = sends to channel
- ? Direct Message = sends to user
```

#### ? Mistake 3: Not Selecting User in DM Mode
```
Problem:
- Switch to DM mode
- Type message
- Click SEND
- Error: "Please select a user"

Solution:
- MUST click "SELECT USER" first
- Choose a recipient
- Then type and send
```

#### ? Mistake 4: Using Old Token
```
Problem:
- Connection fails
- All intents enabled
- Everything looks correct

Solution:
- Reset bot token in portal
- Copy NEW token
- Disconnect and reconnect with new token
```

---

## Advanced Features

### Automation Possibilities

#### Booking Notifications
```csharp
// When new booking created:
1. System creates booking
2. Automatically formats message
3. Sends to Discord channel
4. Includes: DJ name, venue, time, date

Example Message:
"?? New Booking!
DJ: John Smith
Venue: Club Nexus
Time: 9:00 PM
Date: Friday, December 15, 2024"
```

#### Status Updates
```
System monitors:
- New bookings
- Cancelled bookings
- Schedule changes

Automatically posts to Discord:
- Real-time updates
- No manual sending needed
```

### Message Formatting

#### Basic Text
```
Plain text messages work automatically
```

#### Mentions (Advanced)
```
@username - Mention user
@everyone - Mention all (if bot has permission)
#channel-name - Link to channel
```

#### Emojis
```
Standard emojis work:
?? ?? ?? ? ?

Discord custom emojis (server-specific):
:emoji_name:
```

### Security Best Practices

#### Token Security
```
DO:
? Keep token private
? Store securely
? Reset if exposed
? Use different tokens for test/production

DON'T:
? Share publicly
? Commit to Git
? Post in Discord
? Share via email/chat
```

#### Permission Management
```
Grant bot ONLY permissions needed:
? Read Messages
? Send Messages
? Read Message History

Avoid unnecessary permissions:
? Administrator
? Manage Server
? Manage Roles
? Ban Members
```

#### User Privacy
```
Remember:
- Bot can see all channel messages
- Bot can read DMs sent to it
- DMs stored in memory only
- No permanent logging
- Users can block bot anytime
```

### Performance Optimization

#### Large Servers
```
If server has 100+ members:
- User list takes longer to load
- Wait 30-60 seconds
- Consider verification if 100+ servers
```

#### Message Rate Limits
```
Discord has rate limits:
- 5 messages per 5 seconds (channel)
- Don't spam
- Spread messages out
- Bot handles automatically
```

#### Memory Management
```
DM conversations:
- Stored in RAM only
- Cleared on app restart
- Don't keep app open indefinitely
- Restart periodically
```

---

## Quick Reference Cards

### Connection Quick Card
```
????????????????????????????????????????
? CONNECT TO DISCORD                   ?
????????????????????????????????????????
? 1. Get Bot Token from Portal         ?
? 2. Get Channel ID from Discord       ?
? 3. Click CONNECT in app              ?
? 4. Enter Token + Channel ID          ?
? 5. Click TEST CONNECTION             ?
? 6. Click SAVE if successful          ?
????????????????????????????????????????
```

### Messaging Quick Card
```
????????????????????????????????????????
? SEND MESSAGE                         ?
????????????????????????????????????????
? CHANNEL:                             ?
? 1. Select "Channel" radio            ?
? 2. Type message                      ?
? 3. ? Check "Discord"                 ?
? 4. SEND                              ?
?                                      ?
? DIRECT MESSAGE:                      ?
? 1. Select "Direct Message" radio     ?
? 2. SELECT USER                       ?
? 3. Choose recipient                  ?
? 4. Type message                      ?
? 5. ? Check "Discord"                 ?
? 6. SEND                              ?
????????????????????????????????????????
```

### Troubleshooting Quick Card
```
????????????????????????????????????????
? COMMON ISSUES                        ?
????????????????????????????????????????
? Connection Failed:                   ?
? ? Reset token, reconnect             ?
?                                      ?
? No Users:                            ?
? ? Enable SERVER MEMBERS INTENT       ?
? ? Reset token                        ?
?                                      ?
? Messages Not Sending:                ?
? ? Check Discord checkbox             ?
? ? Check connection status            ?
?                                      ?
? DMs Not Working:                     ?
? ? User hasn't blocked bot            ?
? ? User has DMs enabled               ?
????????????????????????????????????????
```

---

## Training Checklist

### Day 1: Setup & Connection
- [ ] Create Discord bot in Developer Portal
- [ ] Enable all 3 privileged intents
- [ ] Get bot token
- [ ] Invite bot to server
- [ ] Get channel ID
- [ ] Connect in DJ Booking System
- [ ] Test connection successfully
- [ ] Send first test message
- [ ] Receive first Discord message

### Day 2: Channel Messaging
- [ ] Send 5 messages to channel
- [ ] Receive 5 messages from Discord
- [ ] Toggle Discord checkbox on/off
- [ ] Test with/without checkbox
- [ ] Send message with Enter key
- [ ] Send multi-line message (Shift+Enter)
- [ ] Verify all messages in correct format

### Day 3: Direct Messaging
- [ ] Switch to DM mode
- [ ] Open user selector
- [ ] Search for user by name
- [ ] Select user
- [ ] Send first DM
- [ ] Receive DM reply
- [ ] Switch to different user
- [ ] Send DM to second user
- [ ] Switch back to first user
- [ ] Verify conversation preserved

### Day 4: Advanced Features
- [ ] Test booking notification
- [ ] Switch between channel/DM modes
- [ ] Handle multiple conversations
- [ ] Test with bot offline/online
- [ ] Test with user offline/online
- [ ] Practice troubleshooting scenarios

### Day 5: Real-World Usage
- [ ] Use for actual booking notifications
- [ ] Respond to community questions
- [ ] Send announcements
- [ ] Handle DM inquiries
- [ ] Monitor for issues
- [ ] Document any problems

---

## Support & Resources

### Documentation
- **Discord.NET Docs**: https://docs.discordnet.dev/
- **Discord API Docs**: https://discord.com/developers/docs/
- **Discord Developer Portal**: https://discord.com/developers/applications

### Getting Help
1. Check this guide first
2. Review troubleshooting section
3. Check Discord Developer Portal
4. Verify intents and permissions
5. Reset token if needed

### Updates & Maintenance
- Check for app updates monthly
- Update bot permissions as needed
- Monitor Discord API changes
- Review security best practices quarterly

---

## Glossary

**Bot Token**: Secret key that authenticates your bot with Discord

**Channel ID**: Unique numeric identifier for Discord channel

**Guild**: Discord's term for "server"

**Intent**: Permission type that determines what data bot can access

**DM**: Direct Message - private 1-on-1 conversation

**Privileged Intent**: Special permission requiring manual enablement

**Gateway**: Discord's WebSocket connection for real-time data

**Rate Limit**: Maximum messages allowed in time period

---

**Document Version**: 1.0.0  
**Last Updated**: 2024  
**Status**: ? Complete Training Guide  
**Compatibility**: DJ Booking System with Discord Integration
