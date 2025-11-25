# ?? ONLINE STATUS NOT SYNCING - Quick Fix

## Problem
Online status is stored **locally in memory** on each PC, not in the cloud database.

**Result:**
- PC 1 (sysadmin) only knows about itself
- PC 2 (test) only knows about itself
- They can't see each other online

## Solution
Store online status in **Cosmos DB** so all PCs can see who's online.

## Quick Fix Options

### Option 1: Enable Existing Cloud Sync (If Available)
Check if `CosmosDbService` has online user tracking:

```csharp
// In your CosmosDbService, add methods to:
1. UpdateUserOnlineStatus(username, isOnline, lastActivity)
2. GetOnlineUsers()
3. Remove inactive users (older than 5 minutes)
```

### Option 2: Modify OnlineUserStatusService
Update it to use Cosmos DB instead of in-memory storage:

**Current (In-Memory - Local Only):**
```csharp
private readonly ConcurrentDictionary<string, UserSessionInfo> _onlineUsers;
// Only visible on this PC
```

**Needed (Cloud - Shared):**
```csharp
// Store in Cosmos DB
await _cosmosService.UpdateUserOnlineStatus(username, DateTime.UtcNow);
var onlineUsers = await _cosmosService.GetOnlineUsers();
```

## Quick Test

**On PC 2 (test user):**
1. Check if the app is actually calling `MarkUserOnline()`
2. Check if Cosmos DB connection is working
3. Check if firewall is blocking Cosmos DB

**Debug Steps:**
```csharp
// Add logging in LoginWindow.xaml.cs after successful login:
Debug.WriteLine($"Marking user {username} as online");
OnlineUserStatusService.Instance.MarkUserOnline(username, /* session info */);
Debug.WriteLine($"Current online users: {OnlineUserStatusService.Instance.GetOnlineUsers().Count}");
```

## Likely Issue
The `test` user on PC 2 is:
- ? Not calling `MarkUserOnline()` on login
- ? Not connected to Cosmos DB
- ? Status stored locally but not synced to cloud

## Immediate Fix

Add this to your login process (both PCs):

```csharp
// After successful login
try
{
    // Mark user online locally
    OnlineUserStatusService.Instance.MarkUserOnline(
        CurrentUser.Username,
        sessionInfo
    );
    
    // TODO: Sync to Cosmos DB
    await CosmosDbService.Instance.UpdateUserOnlineStatusAsync(
        CurrentUser.Username,
        true,
        DateTime.UtcNow
    );
    
    Debug.WriteLine($"User {CurrentUser.Username} marked online");
}
catch (Exception ex)
{
    Debug.WriteLine($"Failed to update online status: {ex.Message}");
}
```

## Need Cloud Sync?
The current implementation is **PC-local only**. For multi-PC support, you need:

1. Cosmos DB container for online users
2. Heartbeat every 30 seconds
3. Cleanup of stale entries (>5 minutes old)

**Do you want me to implement full cloud-based online status tracking?**
