# ?? CandyBot DJ Booking System - Complete Information Manual

## ?? Table of Contents
1. [System Overview](#system-overview)
2. [Venues Management](#venues-management)
3. [Bookings Management](#bookings-management)
4. [User Accounts & Roles](#user-accounts--roles)
5. [Required Information](#required-information)
6. [Workflows & Procedures](#workflows--procedures)
7. [Discord Notifications](#discord-notifications)
8. [Quick Reference](#quick-reference)

---

## System Overview

### What is the DJ Booking System?
The DJ Booking System is a comprehensive platform for managing:
- ?? **Venues** - Nightclubs, bars, and entertainment spaces
- ?? **DJs** - Performers and talent
- ?? **Bookings** - Recurring weekly performance schedules
- ?? **Users** - Account management with roles and permissions
- ?? **Discord Integration** - Real-time notifications and communication

### Core Features
- ? Per-day venue scheduling (different hours for each day)
- ? Weekly recurring bookings (Week 1, 2, 3, or 4 of month)
- ? Automatic booking calendar generation
- ? User role-based permissions
- ? Streaming link management for DJs
- ? Discord notifications for new bookings
- ? Multi-user support with online status tracking

---

## Venues Management

### What is a Venue?

A **Venue** represents a physical location where DJ performances occur (nightclub, bar, lounge, etc.).

### Venue Information Required

#### Basic Information
```
????????????????????????????????????????????
? REQUIRED FIELDS                          ?
????????????????????????????????????????????
? Name:            The venue's name        ?
? Description:     What type of venue      ?
? Owner Username:  Who manages it          ?
? Status:          Active or Inactive      ?
????????????????????????????????????????????
```

#### Operating Schedule

**Per-Day Hours** (different for each day):
```
????????????????????????????????????????????
? DAY          START TIME    FINISH TIME   ?
????????????????????????????????????????????
? Monday       18:00         02:00         ?
? Tuesday      (Closed)                    ?
? Wednesday    20:00         03:00         ?
? Thursday     (Closed)                    ?
? Friday       18:00         04:00         ?
? Saturday     18:00         04:00         ?
? Sunday       (Closed)                    ?
????????????????????????????????????????????
```

**Active Weeks** (which weeks of month venue is open):
```
Week 1: First occurrence of each selected day
Week 2: Second occurrence
Week 3: Third occurrence
Week 4: Fourth occurrence (and fifth if exists)

Example:
Active Weeks: 1, 3
Open Days: Friday, Saturday

Venue opens on:
- 1st Friday of month
- 3rd Friday of month
- 1st Saturday of month
- 3rd Saturday of month
```

### Overnight Schedules

**Important**: Venues can have overnight hours!

```
Example:
Start Time: 22:00 (10:00 PM)
Finish Time: 04:00 (4:00 AM next day)

System creates slots:
22:00, 23:00, 00:00, 01:00, 02:00, 03:00

Note: Finish time is the NEXT day
```

### Venue Data Structure

```json
{
  "id": "venue-123",
  "name": "Club Nexus",
  "description": "High-energy nightclub with state-of-the-art sound",
  "ownerUsername": "john_owner",
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "daySchedules": {
    "Friday": {
      "startTime": "18:00",
      "finishTime": "02:00"
    },
    "Saturday": {
      "startTime": "18:00",
      "finishTime": "04:00"
    }
  },
  "activeWeeks": [1, 2, 3, 4]
}
```

### Creating a Venue

**Step-by-Step Process**:

1. **Navigate to Venues**
   ```
   Main Menu ? Venues
   ```

2. **Click "Register New Venue"**

3. **Enter Basic Info**
   - Venue Name
   - Description
   - Owner (auto-filled if you're venue owner)

4. **Configure Days**
   For each day you want venue open:
   - Check the day
   - Set Start Time (HH:mm format, 24-hour)
   - Set Finish Time (can be next day)
   - Click "Add Day"

5. **Select Active Weeks**
   - Check Week 1, 2, 3, or 4
   - Multiple weeks can be selected

6. **Save**

### Example Venues

#### Example 1: Weekend Nightclub
```
Name: Pulse Nightclub
Description: Premier weekend destination
Open Days: Friday, Saturday
Friday: 22:00 - 05:00
Saturday: 22:00 - 06:00
Active Weeks: 1, 2, 3, 4 (every week)
```

#### Example 2: Monthly Special Event
```
Name: The Loft
Description: Monthly house music event
Open Days: Saturday
Saturday: 21:00 - 03:00
Active Weeks: 1 (first Saturday only)
```

#### Example 3: Bi-Weekly Rotation
```
Name: Underground
Description: Alternating Friday events
Open Days: Friday
Friday: 20:00 - 04:00
Active Weeks: 1, 3 (1st and 3rd Friday only)
```

### Venue Permissions

**Who can do what:**

| Action | SysAdmin | Manager | Venue Owner | DJ | User |
|--------|----------|---------|-------------|----|----|
| View all venues | ? | ? | Own only | ? | ? |
| Create venue | ? | ? | ? | ? | ? |
| Edit any venue | ? | ? | ? | ? | ? |
| Edit own venue | ? | ? | ? | ? | ? |
| Delete venue | ? | ? | ? | ? | ? |
| Deactivate venue | ? | ? | ? | ? | ? |

---

## Bookings Management

### What is a Booking?

A **Booking** is a recurring weekly performance by a DJ at a specific venue on a specific time slot.

### Booking Information Required

#### Core Booking Details
```
????????????????????????????????????????????????????
? REQUIRED FIELDS                                  ?
????????????????????????????????????????????????????
? DJ Name:         Performer's name                ?
? DJ Username:     System username                 ?
? Venue:           Where performance occurs        ?
? Day of Week:     Monday - Sunday                 ?
? Week Number:     1, 2, 3, or 4                   ?
? Time Slot:       HH:mm (from venue's schedule)   ?
? Streaming Link:  DJ's performance stream URL     ?
? Status:          Pending/Confirmed/Cancelled     ?
????????????????????????????????????????????????????
```

### Booking Data Structure

```json
{
  "id": "booking-456",
  "djName": "DJ Shadow",
  "djUsername": "dj_shadow",
  "streamingLink": "https://twitch.tv/djshadow",
  "venueName": "Club Nexus",
  "venueId": "venue-123",
  "venueOwnerUsername": "john_owner",
  "dayOfWeek": "Saturday",
  "weekNumber": 1,
  "timeSlot": "22:00",
  "status": "Confirmed",
  "createdAt": "2024-01-20T14:00:00Z"
}
```

### Understanding Recurring Bookings

**Bookings repeat EVERY MONTH on the same schedule**:

```
Example Booking:
DJ: John Smith
Venue: Club Nexus
Day: Saturday
Week: 1
Time: 22:00

This DJ performs at 10 PM on the FIRST SATURDAY
of EVERY MONTH indefinitely.

January:   Saturday 6th at 22:00
February:  Saturday 3rd at 22:00
March:     Saturday 1st at 22:00
April:     Saturday 5th at 22:00
... and so on
```

### Booking Status Types

| Status | Meaning | Who Can Set | Color |
|--------|---------|-------------|-------|
| **Pending** | Awaiting confirmation | System | Yellow |
| **Confirmed** | Approved and scheduled | Admin/Venue Owner | Green |
| **Cancelled** | No longer active | Admin/Venue Owner/DJ | Red |
| **Completed** | Past performance | System | Blue |

### Creating a Booking

**Step-by-Step Process**:

1. **Navigate to Bookings**
   ```
   Main Menu ? Bookings ? Create Booking
   ```

2. **Select DJ**
   - Choose from DJ list
   - Or enter DJ name manually
   - Streaming link auto-fills if DJ has profile

3. **Select Venue**
   - Pick from available venues
   - Only shows venues you have access to

4. **Choose Schedule**
   - Select Day of Week
   - Select Week Number (1-4)
   - Pick Time Slot (from venue's available slots)

5. **Enter/Verify Streaming Link**
   - URL where DJ will stream
   - Twitch, YouTube, Mixcloud, etc.

6. **Set Status**
   - Usually starts as "Confirmed"
   - Or "Pending" if awaiting approval

7. **Save**

### Time Slot Availability

**System automatically generates available slots**:

```
Venue Schedule:
Friday 18:00 - 02:00

Available Time Slots:
18:00, 19:00, 20:00, 21:00, 22:00, 23:00, 
00:00, 01:00

Each slot = 1 hour block
DJ can book ONE slot per day/week combination
Multiple DJs can book different slots same night
```

### Booking Examples

#### Example 1: Resident DJ
```
DJ: Mike Bass
Venue: Pulse Nightclub
Schedule: Every Friday (Week 1, 2, 3, 4)
Time: 22:00 - 23:00
Status: Confirmed

Result: Mike plays EVERY Friday at 10 PM
(This requires 4 separate bookings, one for each week)
```

#### Example 2: Monthly Feature
```
DJ: Sarah House
Venue: The Loft
Schedule: First Saturday only (Week 1)
Time: 21:00 - 22:00
Status: Confirmed

Result: Sarah plays 1st Saturday of every month at 9 PM
```

#### Example 3: Bi-Weekly Rotation
```
DJ: Alex Trance
Venue: Underground
Schedule: 1st & 3rd Friday (Week 1, 3)
Time: 20:00 - 21:00
Status: Confirmed

Result: Alex plays 1st and 3rd Friday at 8 PM each month
(Requires 2 bookings: one for Week 1, one for Week 3)
```

### Booking Conflicts

**System prevents double-booking**:

```
? CONFLICT:
DJ A: Club Nexus, Saturday Week 1, 22:00
DJ B: Club Nexus, Saturday Week 1, 22:00
? Same venue, day, week, time = NOT ALLOWED

? ALLOWED:
DJ A: Club Nexus, Saturday Week 1, 22:00
DJ B: Club Nexus, Saturday Week 1, 23:00
? Different time slots = ALLOWED

? ALLOWED:
DJ A: Club Nexus, Saturday Week 1, 22:00
DJ B: Club Nexus, Saturday Week 2, 22:00
? Different weeks = ALLOWED
```

### Booking Permissions

| Action | SysAdmin | Manager | Venue Owner | DJ | User |
|--------|----------|---------|-------------|----|----|
| View all bookings | ? | ? | Own venues | Own bookings | ? |
| Create booking | ? | ? | Own venues | ? | ? |
| Edit any booking | ? | ? | ? | ? | ? |
| Edit own booking | ? | ? | ? | ? | ? |
| Cancel booking | ? | ? | ? | ? | ? |
| View streaming link | ? | ? | ? | Own only | ? |

---

## User Accounts & Roles

### User Types

The system has **5 distinct roles**:

#### 1. SysAdmin (System Administrator)
```
Highest access level
Can do ANYTHING in system
- Manage all users
- All venue operations
- All booking operations
- System configuration
- Permission management

Default Count: 1 (you!)
```

#### 2. Manager
```
High-level operations access
Similar to SysAdmin but limited
- Create/edit users
- Manage all venues
- Manage all bookings
- View all data

Use Case: Trusted staff, assistant managers
```

#### 3. Venue Owner
```
Manages their own venue(s)
- Register venues
- Edit own venues
- Create bookings for own venues
- View bookings at own venues
- Approve/decline bookings

Must have: IsVenueOwner = true
Use Case: Nightclub owners, venue managers
```

#### 4. DJ
```
Performance talent
- Create booking requests
- View own bookings
- Edit own streaming link
- View venues

Must have: IsDJ = true
Use Case: Performers, artists
```

#### 5. User (Regular)
```
Basic access
- View venues
- View public calendar
- Basic system access

Use Case: General members, browsing
```

### Account Flags

**Users can have multiple account types**:

```
Example 1: DJ who also owns venue
Username: mike_bass
Role: User
IsDJ: true
IsVenueOwner: true

Can:
- Create bookings as DJ
- Register and manage venues
- Book own venue
```

```
Example 2: Pure DJ
Username: sarah_house
Role: User
IsDJ: true
IsVenueOwner: false

Can:
- Create booking requests
- View venues
- Manage own profile
```

### User Data Structure

```json
{
  "id": "user-789",
  "username": "dj_shadow",
  "fullName": "John Shadow",
  "email": "john@example.com",
  "role": "User",
  "isDJ": true,
  "isVenueOwner": false,
  "streamingLink": "https://twitch.tv/djshadow",
  "djLogoUrl": "https://example.com/logo.png",
  "isActive": true,
  "createdAt": "2024-01-10T12:00:00Z",
  "lastLogin": "2024-01-20T15:30:00Z",
  "permissions": {
    "canViewBookings": true,
    "canCreateBookings": true,
    "canEditBookings": true,
    "canDeleteBookings": false,
    "canViewVenues": true,
    "canRegisterVenues": false
  }
}
```

### Required User Information

#### For All Users
```
- Username (unique, 3-20 characters)
- Password (minimum 6 characters)
- Full Name
- Email Address
- Role (SysAdmin, Manager, User)
- Active Status (true/false)
```

#### Additional for DJs
```
- DJ Flag (IsDJ = true)
- Streaming Link (optional but recommended)
- DJ Logo URL (optional)
```

#### Additional for Venue Owners
```
- Venue Owner Flag (IsVenueOwner = true)
- Associated Venues (assigned during venue creation)
```

### User Permissions Matrix

```
??????????????????????????????????????????????????????????????
?                    PERMISSION MATRIX                       ?
????????????????????????????????????????????????????????????
? ACTION         ? Sys  ? Manager ? Venue ? DJ  ? User     ?
?                ?Admin ?         ? Owner ?     ?          ?
????????????????????????????????????????????????????????????
? Create Users   ?  ?  ?   ?    ?  ?   ? ?  ?   ?     ?
? Edit Any User  ?  ?  ?   ?    ?  ?   ? ?  ?   ?     ?
? Delete Users   ?  ?  ?   ?    ?  ?   ? ?  ?   ?     ?
? Assign Roles   ?  ?  ?   ?    ?  ?   ? ?  ?   ?     ?
?                ?      ?         ?       ?     ?          ?
? Create Venue   ?  ?  ?   ?    ?  ?   ? ?  ?   ?     ?
? Edit Any Venue ?  ?  ?   ?    ?  ?   ? ?  ?   ?     ?
? Edit Own Venue ?  ?  ?   ?    ?  ?   ? ?  ?   ?     ?
? Delete Venue   ?  ?  ?   ?    ?  ?   ? ?  ?   ?     ?
? View Venues    ?  ?  ?   ?    ?  ?   ? ?  ?   ?     ?
?                ?      ?         ?       ?     ?          ?
? Create Booking ?  ?  ?   ?    ?  ?   ? ?  ?   ?     ?
? Edit Any Book  ?  ?  ?   ?    ?  ?   ? ?  ?   ?     ?
? Edit Own Book  ?  ?  ?   ?    ?  ?   ? ?  ?   ?     ?
? Cancel Booking ?  ?  ?   ?    ?  ?   ? ?  ?   ?     ?
? View All Books ?  ?  ?   ?    ? Own   ?Own  ?   ?     ?
?                ?      ?         ?       ?     ?          ?
? View Stream URL?  ?  ?   ?    ?  ?   ?Own  ?   ?     ?
? Discord Admin  ?  ?  ?   ?    ?  ?   ? ?  ?   ?     ?
????????????????????????????????????????????????????????????
```

---

## Required Information

### Data Collection Checklist

#### When Creating a Venue
```
? MUST HAVE:
- Venue name
- At least one open day with hours
- At least one active week selected
- Owner username

? RECOMMENDED:
- Description
- Multiple days configured
- Realistic hours (most venues: 18:00-04:00)

? OPTIONAL:
- Logo/image
- Website
- Phone number
```

#### When Creating a Booking
```
? MUST HAVE:
- DJ name
- DJ username
- Venue selection
- Day of week
- Week number (1-4)
- Time slot

? RECOMMENDED:
- Streaming link
- Status (Confirmed vs Pending)

? OPTIONAL:
- DJ logo
- Additional notes
```

#### When Creating a User
```
? MUST HAVE:
- Username (unique)
- Password
- Full name
- Email
- Role

? IF DJ:
- IsDJ flag = true
- Streaming link (recommended)

? IF VENUE OWNER:
- IsVenueOwner flag = true
- Will be able to create venues
```

### Data Validation Rules

#### Venue Rules
```
- Name: 1-100 characters
- Start Time: 00:00-23:59 format
- Finish Time: 00:00-24:00 format (can be next day)
- Active Weeks: At least one (1, 2, 3, or 4)
- Open Days: At least one day must be configured
```

#### Booking Rules
```
- DJ Name: 1-50 characters
- Time Slot: Must be from venue's available slots
- Week Number: 1-4 only
- Day of Week: Monday-Sunday
- Streaming Link: Valid URL format (if provided)
- No double-booking same slot
```

#### User Rules
```
- Username: 3-20 characters, alphanumeric + underscore
- Password: Minimum 6 characters
- Email: Valid email format
- Role: Must be valid enum value
- Unique username constraint
- Unique email constraint
```

---

## Workflows & Procedures

### Complete Booking Workflow

```
STEP 1: Venue Owner Registers Venue
??????????????????????????????????????
1. Log in as venue owner
2. Navigate to Venues
3. Click "Register New Venue"
4. Enter venue details:
   - Name: "Club Nexus"
   - Description: "Premier nightclub"
5. Configure days:
   - Friday: 18:00 - 02:00
   - Saturday: 18:00 - 04:00
6. Select active weeks: 1, 2, 3, 4
7. Save venue
? Venue is now available for bookings


STEP 2: DJ Creates Account or Gets Added
?????????????????????????????????????????
Option A - DJ Self-Registration:
1. DJ creates account
2. Sets IsDJ flag
3. Enters streaming link

Option B - Admin Creates:
1. Admin creates user account
2. Sets Role: User
3. Checks IsDJ box
4. Enters DJ's streaming link
? DJ can now create bookings


STEP 3: DJ Creates Booking
???????????????????????????
1. DJ logs in
2. Navigate to Bookings
3. Click "Create Booking"
4. Select venue: "Club Nexus"
5. Choose day: Friday
6. Choose week: 1 (first Friday)
7. Select time slot: 22:00
8. Streaming link auto-fills
9. Submit booking
? Booking created as "Pending" or "Confirmed"


STEP 4: Venue Owner Reviews (if needed)
????????????????????????????????????????
1. Venue owner logs in
2. Views bookings for their venue
3. Reviews DJ's request
4. Approves or declines
? Booking status updated


STEP 5: Discord Notification (Automatic)
?????????????????????????????????????????
When booking confirmed:
- System sends notification to Discord
- Message includes:
  - DJ name
  - Venue name
  - Day and time
  - Week number
? Community notified


STEP 6: Booking Appears in Calendar
????????????????????????????????????
- Shows on monthly calendar
- Appears on correct dates
- Displays DJ name and time
- Color-coded by status
? Schedule visible to authorized users
```

### Common Scenarios

#### Scenario 1: Resident DJ (Every Friday)
```
Requirement: DJ plays every Friday at 10 PM

Process:
1. Create 4 separate bookings:
   - Booking 1: Friday, Week 1, 22:00
   - Booking 2: Friday, Week 2, 22:00
   - Booking 3: Friday, Week 3, 22:00
   - Booking 4: Friday, Week 4, 22:00

Result:
DJ appears on calendar EVERY Friday of month
at 10 PM indefinitely
```

#### Scenario 2: Monthly Feature DJ
```
Requirement: DJ plays first Saturday only

Process:
1. Create 1 booking:
   - Day: Saturday
   - Week: 1
   - Time: 21:00

Result:
DJ appears on 1st Saturday of every month
at 9 PM
```

#### Scenario 3: Multiple DJs Same Night
```
Requirement: 3 DJs at same venue, same night

Process:
1. Booking 1: DJ A, Friday Week 1, 20:00
2. Booking 2: DJ B, Friday Week 1, 21:00
3. Booking 3: DJ C, Friday Week 1, 22:00

Result:
Three 1-hour sets on same night:
20:00-21:00: DJ A
21:00-22:00: DJ B
22:00-23:00: DJ C
```

#### Scenario 4: Venue Closed Specific Week
```
Problem: Venue closed 2nd week for renovations

Solution:
Set Active Weeks to: 1, 3, 4 (exclude 2)

Result:
- Week 1: Venue open, bookings happen
- Week 2: Venue closed, no bookings
- Week 3: Venue open, bookings happen
- Week 4: Venue open, bookings happen
```

### Editing & Cancelling

#### To Edit a Booking
```
1. Navigate to Bookings
2. Find the booking
3. Click Edit (if you have permission)
4. Modify:
   - Time slot (to different available slot)
   - Streaming link
   - Status
5. Cannot change:
   - Venue (create new booking instead)
   - DJ (create new booking instead)
   - Day/Week (create new booking instead)
6. Save changes
```

#### To Cancel a Booking
```
1. Find the booking
2. Click Cancel (or Edit ? Set Status to Cancelled)
3. Reason (optional but recommended)
4. Confirm cancellation

Result:
- Booking marked as Cancelled
- No longer appears in active calendar
- Historical record preserved
- Time slot becomes available
```

---

## Discord Notifications

### Automatic Notifications

**System automatically sends Discord messages for**:

#### New Booking Created
```
Message Format:
"?? New Booking!
DJ: John Shadow
Venue: Club Nexus
Time: 22:00
Date: Friday (Week 1)"
```

#### Booking Confirmed
```
Message Format:
"? Booking Confirmed!
DJ: Sarah House
Venue: The Loft
Schedule: Saturday Week 1 at 21:00"
```

#### Booking Cancelled
```
Message Format:
"? Booking Cancelled
DJ: Mike Bass
Venue: Underground
Was scheduled: Friday Week 2 at 20:00"
```

### Discord Integration Features

**What CandyBot can do via Discord**:

1. **Announcements**
   - New booking notifications
   - Schedule changes
   - Venue updates

2. **Queries** (if implemented)
   - "Show bookings this week"
   - "Who's playing Friday?"
   - "When does DJ X perform?"

3. **Direct Messages**
   - Contact specific DJs
   - Message venue owners
   - Private communications

### Discord Configuration

**Required Setup**:
```
1. Discord Bot Token
2. Discord Channel ID
3. Privileged intents enabled:
   - Message Content Intent
   - Server Members Intent
   - Presence Intent

4. Bot permissions:
   - Send Messages
   - Read Messages
   - Read Message History
```

---

## Quick Reference

### Time Format Guide

```
24-Hour Format (HH:mm)
?????????????????????????
00:00 = Midnight
06:00 = 6 AM
12:00 = Noon
18:00 = 6 PM
22:00 = 10 PM
23:59 = 11:59 PM

Overnight Examples:
Start: 22:00 (10 PM)
Finish: 04:00 (4 AM next day)
```

### Week Numbers Guide

```
Week of Month Calculation
?????????????????????????????????
Week 1: Days 1-7 of month
Week 2: Days 8-14 of month
Week 3: Days 15-21 of month
Week 4: Days 22-31 of month

Examples for January 2024:
- Jan 6 (Saturday) = Week 1
- Jan 13 (Saturday) = Week 2
- Jan 20 (Saturday) = Week 3
- Jan 27 (Saturday) = Week 4
```

### Status Color Codes

```
Venues:
?? Green = Active
?? Red = Inactive

Bookings:
?? Yellow = Pending
?? Green = Confirmed
?? Red = Cancelled
?? Blue = Completed

Users:
?? Green Dot = Online
? Gray Dot = Offline
```

### Quick Commands Cheat Sheet

```
VENUES
??????????????????????????????
View Venues       ? Main Menu ? Venues
Create Venue      ? Venues ? Register New Venue
Edit Venue        ? Venues ? Click venue ? Edit
Deactivate Venue  ? Venues ? Click venue ? Deactivate

BOOKINGS
??????????????????????????????
View Bookings     ? Main Menu ? Bookings
Create Booking    ? Bookings ? Create Booking
View Calendar     ? Bookings ? Calendar View
Edit Booking      ? Bookings ? Click booking ? Edit
Cancel Booking    ? Bookings ? Click booking ? Cancel

USERS
??????????????????????????????
View Users        ? Main Menu ? Users
Create User       ? Users ? Create New User
Edit User         ? Users ? Click user ? Edit
Set DJ Flag       ? Edit User ? Check "Is DJ"
Set Venue Owner   ? Edit User ? Check "Is Venue Owner"

DISCORD
??????????????????????????????
Open Chat         ? Main Menu ? Chat
Connect Discord   ? Chat ? CONNECT button
Send Message      ? Type ? Check Discord ? SEND
Switch to DM      ? Click "Direct Message" radio
```

### Common Data Patterns

```
USERNAME FORMATS
????????????????
john_doe
dj_shadow
mike.bass
sarah2024
venue_owner

STREAMING LINK FORMATS
??????????????????????
https://twitch.tv/username
https://youtube.com/@channel
https://mixcloud.com/username
https://soundcloud.com/username

VENUE NAME EXAMPLES
???????????????????
Club Nexus
The Underground
Pulse Nightclub
Jazz Lounge
Skybar Rooftop

DJ NAME EXAMPLES
????????????????
DJ Shadow
Mike Bass
Sarah House
The Mixer
Alex Trance
```

### Troubleshooting Quick Fixes

```
PROBLEM: Can't create booking
SOLUTION: 
- Check if DJ account exists
- Verify DJ flag is set
- Ensure venue is active
- Confirm time slot available

PROBLEM: Venue not showing days
SOLUTION:
- Edit venue
- Add days with start/finish times
- Select at least one active week
- Save changes

PROBLEM: Booking conflicts
SOLUTION:
- Choose different time slot
- Or different week number
- Or different day

PROBLEM: Can't see streaming link
SOLUTION:
- Must be SysAdmin, Manager, Venue Owner, or DJ (own booking)
- Regular users cannot see streaming links
```

---

## System Limits & Constraints

### Technical Limits
```
Maximum Venues per Owner: Unlimited
Maximum Bookings per DJ: Unlimited
Maximum Time Slots per Day: 24 (one per hour)
Maximum Active Weeks: 4 (1, 2, 3, 4)
Booking Duration: 1 hour blocks
Calendar Range: Current month + future months
User Roles: 5 types (SysAdmin, Manager, Venue Owner, DJ, User)
```

### Business Rules
```
- One booking per DJ per time slot
- No double-booking same venue/time/week
- Venues must have at least one day configured
- Bookings inherit venue's schedule constraints
- Cancelled bookings free up the time slot
- Historical bookings preserved for records
```

---

## Data Storage & Structure

### Database Collections

```
USERS Collection
????????????????
Stores: User accounts, credentials, roles
Key Fields: username, role, isDJ, isVenueOwner

VENUES Collection
?????????????????
Stores: Venue details, schedules
Key Fields: name, ownerUsername, daySchedules, activeWeeks

BOOKINGS Collection
???????????????????
Stores: DJ performances schedule
Key Fields: djUsername, venueId, dayOfWeek, weekNumber, timeSlot

SETTINGS Collection
???????????????????
Stores: System configuration
Key Fields: Discord settings, theme preferences
```

### Data Relationships

```
USER ??owns??> VENUE
USER (DJ) ??books??> BOOKING
VENUE ??contains??> BOOKING
USER (Venue Owner) ??approves??> BOOKING
```

---

## Best Practices

### For Venue Owners
```
? Set realistic operating hours
? Configure all days you're open
? Use active weeks to match your schedule
? Keep venue active when operating
? Review booking requests promptly
? Update hours if schedule changes
```

### For DJs
```
? Keep streaming link updated
? Book slots well in advance
? Respect venue time slots
? Communicate changes to venue owner
? Keep profile information current
```

### For Administrators
```
? Assign roles correctly
? Monitor booking conflicts
? Keep Discord integration active
? Regular user permission audits
? Back up data regularly
? Train new users on system
```

---

**Manual Version**: 1.0.0  
**Last Updated**: 2024  
**System**: DJ Booking System with Discord Integration  
**Status**: ? Complete Information Guide
