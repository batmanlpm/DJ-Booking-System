# ?? DJ BOOKING SYSTEM - COMPLETE DEVELOPMENT HISTORY
## The Fallen Collective - Online DJ Booking & Management System

**Last Updated:** January 23, 2025 (Evening Session)  
**Current Version:** 1.3.0 - Discord-Style Friends & DM System  
**Project Location:** `K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\`  
**Technology Stack:** .NET 8, WPF, Azure Cosmos DB, C# 12.0

---

## ?? **TABLE OF CONTENTS**

1. [Project Overview](#project-overview)
2. [Architecture & Technology](#architecture--technology)
3. [Complete Feature List](#complete-feature-list)
4. [Development Sessions](#development-sessions)
5. [Database Schema](#database-schema)
6. [Security Features](#security-features)
7. [Known Issues & Fixes](#known-issues--fixes)
8. [Future Roadmap](#future-roadmap)
9. [Deployment Guide](#deployment-guide)
10. [Troubleshooting](#troubleshooting)

---

## ?? **PROJECT OVERVIEW**

### **Purpose:**
A comprehensive online booking and management system for The Fallen Collective DJ community, featuring:
- DJ booking and scheduling
- Venue management
- Integrated chat system
- Radio station integration (LPM.fm)
- User management with role-based permissions
- Support ticket system
- Advanced moderation tools

### **Target Users:**
- **DJs** - Book gigs, manage profiles, stream shows
- **Venue Owners** - Post gigs, manage bookings
- **Admins/Managers** - Full system control, moderation
- **SysAdmin** - Ultimate control, database management

### **Primary Features:**
- ? User authentication with auto-login
- ? Role-based permission system (Guest ? DJ/Venue ? Manager ? SysAdmin)
- ? Three-strike progressive ban system with machine + IP binding
- ? Integrated chat with support tickets
- ? **Discord-style friends list with DM messaging** ? NEW
- ? Radio player integration
- ? Real-time online/offline status
- ? Theme customization
- ? Auto-updater system
- ? CandyBot AI assistant (file management, image generation, voice)

---

## ??? **ARCHITECTURE & TECHNOLOGY**

### **Technology Stack:**

| Component | Technology | Version |
|-----------|-----------|---------|
| Framework | .NET | 8.0 |
| Language | C# | 12.0 |
| UI Framework | WPF | .NET 8 |
| Database | Azure Cosmos DB | NoSQL |
| Cloud Platform | Microsoft Azure | - |
| Web Hosting | Hostinger | Production |
| Package Manager | NuGet | - |

### **Key NuGet Packages:**

```xml
<PackageReference Include="Microsoft.Azure.Cosmos" Version="3.x" />
<PackageReference Include="Newtonsoft.Json" Version="13.x" />
<PackageReference Include="System.Management" Version="10.0.0" />
<PackageReference Include="NAudio" Version="2.x" />
<PackageReference Include="Microsoft.Toolkit.Uwp.Notifications" Version="7.x" />
```

### **Project Structure:**

```
DJBookingSystem/
??? Models/                  # Data models
?   ??? User.cs             # User account model
?   ??? Booking.cs          # DJ booking model
?   ??? Venue.cs            # Venue model
?   ??? ChatMessage.cs      # Chat message model
?   ??? SupportTicket.cs    # Support ticket model
?   ??? Friendship.cs       # Friends & requests models ? NEW
?   ??? UserReport.cs       # User report for moderation ? NEW
?   ??? ...
??? Services/               # Business logic
?   ??? CosmosDbService.cs          # Database operations
?   ??? AuthenticationService.cs    # Login/auth
?   ??? OnlineUserStatusService.cs  # Real-time status
?   ??? MachineBanService.cs        # Ban enforcement
?   ??? RoleChangeValidator.cs      # Role security
?   ??? FriendsService.cs           # Friends management ? NEW
?   ??? ...
??? Views/                  # UI screens
?   ??? UsersView.xaml     # User management
?   ??? BanCountdownWindow.xaml    # Ban timer
?   ??? BanReasonDialog.xaml       # Ban reason input
?   ??? SettingsView.xaml  # Settings
?   ??? ...
??? Controls/              # Reusable UI components
?   ??? VideoCandyBotAvatar.xaml  # CandyBot mascot
?   ??? ClaudeIndicator.xaml      # Status indicators
??? Themes/                # UI themes
?   ??? GreenTheme.xaml
?   ??? SpaceTheme.xaml
?   ??? ...
??? Resources/             # Assets
    ??? Candy-Bot.mp4      # CandyBot animation
    ??? New-BG.png         # Background image
```

---

## ?? **COMPLETE FEATURE LIST**

### **1. USER MANAGEMENT**

#### **Authentication:**
- ? Username/password login
- ? "Remember Me" option
- ? Auto-login (saves to database)
- ? Registration with DJ/Venue flags
- ? Password validation (min 6 chars)
- ? IP tracking and history
- ? Machine ID binding (VPN prevention)

#### **User Roles:**

| Role | Permissions | Can Promote To |
|------|-------------|----------------|
| **Guest** | View only | DJ, Venue |
| **DJ** | Book gigs, manage profiles, stream shows | - |
| **Venue Owner** | Post gigs, manage bookings | - |
| **Manager** | Full system control, moderate, ban (Strikes 1-2), promote to Manager | DJ, Venue, Manager |
| **SysAdmin** | Ultimate control, ban (Strike 3), reset strikes, database | All roles |

#### **Role Security (RoleChangeValidator):**
- ? Prevents self-promotion
- ? Enforces role hierarchy (can't demote higher roles)
- ? Protects SysAdmin role (only SysAdmin can promote to SysAdmin)
- ? Audit trail for all role changes
- ? Confirmation dialogs with warnings

#### **User Profile Fields:**

```csharp
public class User {
    string Username
    string PasswordHash
    UserRole Role
    bool IsDJ
    bool IsVenueOwner
    bool IsActive
    bool IsBanned
    int BanStrikeCount        // 0-3 strikes
    bool IsPermanentBan       // True on Strike 3
    DateTime? BanExpiry       // When temp ban expires
    string BanReason
    string BannedBy
    string BannedIP
    string CurrentIP
    List<string> IPHistory
    bool IsGloballyMuted      // Chat mute
    AppPreferences { ... }
    Permissions { ... }
    // ... and more
}
```

---

### **2. THREE-STRIKE BAN SYSTEM** ? **MAJOR FEATURE**

#### **Progressive Escalation:**

| Strike | Duration | Who Can Issue | Notes |
|--------|----------|--------------|-------|
| **1** | 24 hours | Manager, SysAdmin | First offense |
| **2** | 48 hours | Manager, SysAdmin | Warning shown |
| **3** | **PERMANENT** | **SysAdmin ONLY** | Requires override |

#### **How It Works:**

1. **Admin Bans User:**
   ```
   Admin clicks "Ban" ? BanReasonDialog appears
   ? Admin enters reason (required, min 10 chars)
   ? System checks user.BanStrikeCount
   ? Auto-assigns duration based on strikes:
      • 0 strikes ? Strike 1 ? 24 hours
      • 1 strike ? Strike 2 ? 48 hours
      • 2 strikes ? Strike 3 ? Permanent (SysAdmin only)
   ? Confirms with admin
   ? Saves to database
   ? Kicks user if online
   ```

2. **User Gets Kicked:**
   ```
   Force logout event fires
   ? Fetches updated user data (with ban reason)
   ? Stores ban LOCALLY on user's machine
   ? Shows ban message with reason
   ? Closes app
   ```

3. **User Tries to Login:**
   ```
   Check 1: Machine ban file (local) ? BLOCKED if found
   Check 2: IP ban (database) ? BLOCKED if found
   ? Shows countdown window with timer
   ? User waits until ban expires
   ```

#### **Dual-Layer Enforcement:**

**Layer 1: Machine Ban (Hardware ID)**
- Location: `%AppData%\DJBookingSystem\machine_ban.dat`
- Tied to: CPU ID + Motherboard Serial (SHA256 hash)
- Prevents: VPN bypass, IP changes, proxy

**Layer 2: IP/Network Ban**
- Location: Cosmos DB (`user.BannedIP`)
- Tied to: WAN IP address
- Prevents: Using other computers on same network

**VPN Bypass Prevention:**
```
Scenario 1: User uses VPN
? Changes IP ? Machine ban still active ? BLOCKED ?

Scenario 2: User switches to laptop
? Same network IP ? IP ban active ? BLOCKED ?

Scenario 3: User uses proxy + different computer
? Both layers enforce ? BLOCKED ?

Only bypass: Replace CPU + Motherboard (expensive!)
```

#### **Animated Countdown Timer:**

**Features:**
- Beautiful flip-clock display (Days : Hours : Minutes : Seconds)
- Color-coded: Days (Orange), Hours (Green), Minutes (Red), Seconds (Purple)
- Updates every second
- Shows strike count (1/2/3 of 3)
- Displays ban reason
- Escalating warnings:
  - Strike 1: "Next ban will be 48 hours"
  - Strike 2: "FINAL WARNING - Next ban is PERMANENT"
  - Strike 3: "PERMANENT BAN - Contact SysAdmin"
- Contact Support button
- Auto-closes when ban expires

#### **SysAdmin Override:**

**Powers:**
- ? Unban Strike 3 (permanent bans)
- ? Reset strike count to 0 (gives user fresh start)
- ? Skip to permanent ban immediately (for severe violations)
- ? View full ban history

**Managers Can:**
- ? Ban users (Strikes 1-2 only)
- ? Unban Strikes 1-2
- ? Cannot unban Strike 3
- ? Cannot reset strike count

#### **Files Involved:**

| File | Purpose |
|------|---------|
| `Models/User.cs` | Added `BanStrikeCount`, `IsPermanentBan` |
| `Views/BanReasonDialog.xaml` | Admin enters ban reason |
| `Views/BanCountdownWindow.xaml` | Animated countdown timer |
| `Services/MachineBanService.cs` | Machine ID + local ban storage |
| `Views/UsersView.xaml.cs` | Ban/unban/reset logic |
| `LoginWindow.xaml.cs` | Dual-layer ban checks |
| `MainWindow.xaml.cs` | Force logout + local ban storage |

---

### **3. INTEGRATED CHAT SYSTEM**

#### **Channels:**

| Channel | Access | Purpose | Privacy |
|---------|--------|---------|---------|
| **?? World Chat** | All users | Public chat | Everyone sees |
| **?? Support Tickets** | User + Admins | Private help | PRIVATE |
| **?? Announcements** | Read-only | Admin posts | Everyone sees |
| **?? Admin Only** | SysAdmin + Manager | Private admin chat | Admins only |

#### **Features:**
- ? Real-time sync (3-second refresh)
- ? Online user count display
- ? Channel switching
- ? Role badges (DJ, Venue, Admin)
- ? Timestamp display (HH:mm format)
- ? Message filtering by channel
- ? Auto-scroll to latest
- ? Optimistic UI updates (instant feedback)
- ? Enter key to send
- ? CosmosDB storage

#### **Support Tickets (In Progress):**

**Design:**
```csharp
public class SupportTicket {
    string Id
    int TicketNumber           // Auto-incremented
    string CreatedBy           // Username
    DateTime CreatedAt
    string Subject
    TicketStatus Status        // Open, InProgress, Resolved, Closed
    TicketPriority Priority    // Low, Normal, High, Critical
    string AssignedTo          // Admin username
    List<TicketMessage> Messages
    bool IsPrivate = true      // Always true for tickets
    List<string> VisibleTo     // Creator + assigned admins
}
```

**Privacy Rules:**
- ? Only creator can see their own ticket
- ? All admins can see all tickets
- ? Other users CANNOT see each other's tickets
- ? Ticket list filtered per user

**Status:**
- ? Model created (`Models/SupportTicket.cs`)
- ? Admin ticket view (in progress)
- ? Ticket creation workflow (in progress)
- ? Private chat for tickets (in progress)

---

### **3.5 FRIENDS LIST & DIRECT MESSAGING** ? **NEW v1.3.0**

#### **Overview:**
Discord-style friends system with friend requests, online/offline status tracking, and private direct messaging capabilities integrated into the IntegratedChatWindow.

#### **Friend Request System:**

**Sending Friend Requests:**
- ? "Add Friend" button in chat sidebar (green theme)
- ? Enter username to send request
- ? Optional personal message (max 200 characters)
- ? Duplicate request prevention
- ? User existence validation

**Managing Friend Requests:**
- ? "Pending Requests" button (appears with badge when requests exist)
- ? Accept/Decline buttons for each request
- ? View sender's message
- ? Request timestamp display
- ? Auto-remove from list after action

**Smart Features:**
- ? Auto-accept mutual requests (if both users send to each other)
- ? Request status tracking (Pending, Accepted, Declined, Cancelled)
- ? Cannot send request to yourself
- ? Cannot request already-friended users

#### **Friends List Display:**

**Visual Design:**
- ? Sidebar section below channels
- ? Online status indicators:
  - ?? **Green dot** = Friend is online
  - ? **Gray dot** = Friend is offline
- ? Black background (#0F0F0F) matching app theme
- ? Green borders (#00FF00) on hover
- ? Consistent with existing UI design

**Sorting Priority:**
1. ? Favorites (pinned friends)
2. ?? Online friends
3. ?? Alphabetical order (by nickname or username)

**Auto-Refresh:**
- ? Friends list updates every 10 seconds
- ? Online status syncs in real-time
- ? New friends appear automatically
- ? ObservableCollection for instant UI updates

#### **Direct Messaging (DM):**

**Starting a DM:**
- ? Click any friend's name in the list
- ? Chat header updates to "DM with [Username]"
- ? Message area clears and loads DM history
- ? Send button works for private messages

**DM Features:**
- ? Private 1-on-1 conversations
- ? Messages visible only to participants
- ? Message history persistence
- ? Conversation ID format: `private:user1:user2` (alphabetically sorted)
- ? Participant tracking in message metadata
- ? Same UI as world chat (consistent experience)

**Message Filtering:**
- ? DM messages filtered by sender + recipient
- ? Bidirectional filtering (you ? them, them ? you)
- ? Separate from world chat / support tickets
- ? No cross-contamination with other channels

#### **Friendship Metadata:**
