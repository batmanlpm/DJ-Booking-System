using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DJBookingSystem.Models;
using DJBookingSystem.Views;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Manages online user status tracking with cloud synchronization
    /// </summary>
    public class OnlineUserStatusService
    {
        private static OnlineUserStatusService? _instance;
        private static readonly object _lock = new object();

        private readonly ConcurrentDictionary<string, UserSessionInfo> _onlineUsers;
        private readonly ConcurrentDictionary<string, DateTime> _lastActivity;
        private CosmosDbService? _cosmosService;

        // Events
        public event EventHandler<UserStatusEventArgs>? UserStatusChanged;
        public event EventHandler<string>? UserForcedLogout; // NEW: Event for forced logout

        public static OnlineUserStatusService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new OnlineUserStatusService();
                        }
                    }
                }
                return _instance;
            }
        }

        private OnlineUserStatusService()
        {
            _onlineUsers = new ConcurrentDictionary<string, UserSessionInfo>();
            _lastActivity = new ConcurrentDictionary<string, DateTime>();
            // Cosmos service will be set later via Initialize method
        }

        /// <summary>
        /// Initialize with CosmosDbService (call this after CosmosDbService is created)
        /// </summary>
        public void Initialize(CosmosDbService cosmosService)
        {
            _cosmosService = cosmosService;
            
            // Start Change Feed processor for INSTANT push notifications
            _ = Task.Run(async () => await StartChangeFeedProcessorAsync());
            
            System.Diagnostics.Debug.WriteLine("[ONLINE] OnlineUserStatusService initialized with INSTANT Change Feed notifications");
        }

        /// <summary>
        /// Start Change Feed processor for real-time push notifications
        /// </summary>
        private async Task StartChangeFeedProcessorAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[CHANGE FEED] Starting Change Feed processor...");
                
                // Poll for changes every 100ms for near-instant updates
                while (true)
                {
                    try
                    {
                        if (_cosmosService != null)
                        {
                            var cloudUsers = await _cosmosService.GetOnlineUsersAsync();
                            await UpdateLocalCacheFromCloudAsync(cloudUsers);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[CHANGE FEED] Error: {ex.Message}");
                    }
                    
                    // Check every 100ms for near-instant updates
                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CHANGE FEED] Fatal error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update local cache from cloud data
        /// </summary>
        private async Task UpdateLocalCacheFromCloudAsync(List<OnlineUserStatus> cloudUsers)
        {
            var cloudUsernames = new HashSet<string>(cloudUsers.Select(u => u.Username));
            var localUsernames = new HashSet<string>(_onlineUsers.Keys);
            
            // Find NEW users online in cloud
            foreach (var cloudUser in cloudUsers)
            {
                if (!_onlineUsers.ContainsKey(cloudUser.Username))
                {
                    System.Diagnostics.Debug.WriteLine($"[INSTANT] NEW user online: {cloudUser.Username}");
                    UserStatusChanged?.Invoke(this, new UserStatusEventArgs(cloudUser.Username, true));
                }
            }
            
            // Find users who went offline
            foreach (var localUsername in localUsernames)
            {
                if (!cloudUsernames.Contains(localUsername))
                {
                    System.Diagnostics.Debug.WriteLine($"[INSTANT] User went offline: {localUsername}");
                    _onlineUsers.TryRemove(localUsername, out _);
                    UserStatusChanged?.Invoke(this, new UserStatusEventArgs(localUsername, false));
                }
            }
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Sync online status with cloud
        /// </summary>
        private async Task SyncWithCloudAsync()
        {
            if (_cosmosService == null) return;
            
            try
            {
                // Get online users from cloud
                var cloudUsers = await _cosmosService.GetOnlineUsersAsync();
                
                // Update local cache
                var cloudUsernames = new HashSet<string>(cloudUsers.Select(u => u.Username));
                var localUsernames = new HashSet<string>(_onlineUsers.Keys);
                
                // Find users who are online in cloud but not locally
                foreach (var cloudUser in cloudUsers)
                {
                    if (!_onlineUsers.ContainsKey(cloudUser.Username))
                    {
                        System.Diagnostics.Debug.WriteLine($"[SYNC] Found new online user from cloud: {cloudUser.Username}");
                        UserStatusChanged?.Invoke(this, new UserStatusEventArgs(cloudUser.Username, true));
                    }
                }
                
                // Find users who are offline in cloud but still online locally
                foreach (var localUsername in localUsernames)
                {
                    if (!cloudUsernames.Contains(localUsername))
                    {
                        System.Diagnostics.Debug.WriteLine($"[SYNC] User went offline in cloud: {localUsername}");
                        _onlineUsers.TryRemove(localUsername, out _);
                        UserStatusChanged?.Invoke(this, new UserStatusEventArgs(localUsername, false));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SYNC] Error syncing with cloud: {ex.Message}");
            }
        }

        /// <summary>
        /// Mark user as online (local and cloud)
        /// </summary>
        public async Task SetUserOnlineAsync(User user)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[ONLINE] Setting user online: {user.Username}");
                
                var sessionInfo = new UserSessionInfo
                {
                    User = user,
                    LoginTime = DateTime.Now,
                    LastActivityTime = DateTime.Now,
                    IsOnline = true
                };

                _onlineUsers.AddOrUpdate(user.Username, sessionInfo, (key, old) => sessionInfo);
                _lastActivity[user.Username] = DateTime.Now;

                // Update in cloud if service is available
                if (_cosmosService != null)
                {
                    await _cosmosService.UpdateUserOnlineStatusAsync(user.Username, true, DateTime.UtcNow);
                }

                System.Diagnostics.Debug.WriteLine($"[ONLINE] User added locally and to cloud. Total online: {_onlineUsers.Count}");
                
                // Notify listeners
                UserStatusChanged?.Invoke(this, new UserStatusEventArgs(user.Username, true));
                
                System.Diagnostics.Debug.WriteLine($"[ONLINE] Complete! {user.Username} is now ONLINE");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ONLINE] ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Synchronous version for backwards compatibility
        /// </summary>
        public void SetUserOnline(User user)
        {
            _ = SetUserOnlineAsync(user);
        }

        /// <summary>
        /// Mark user as offline (local and cloud)
        /// </summary>
        public async Task SetUserOfflineAsync(string username)
        {
            if (_onlineUsers.TryRemove(username, out var session))
            {
                System.Diagnostics.Debug.WriteLine($"User {username} is now OFFLINE (was online for {(DateTime.Now - session.LoginTime).TotalMinutes:F1} minutes)");

                // Update in cloud if service is available
                if (_cosmosService != null)
                {
                    await _cosmosService.MarkUserOfflineAsync(username);
                }

                // Notify listeners
                UserStatusChanged?.Invoke(this, new UserStatusEventArgs(username, false));
            }
        }

        /// <summary>
        /// Synchronous version for backwards compatibility
        /// </summary>
        public void SetUserOffline(string username)
        {
            _ = SetUserOfflineAsync(username);
        }

        /// <summary>
        /// Force logout a user (used when banning)
        /// </summary>
        public void ForceLogoutUser(string username)
        {
            // Mark user offline
            SetUserOffline(username);
            
            // Fire event to notify their active session to close
            UserForcedLogout?.Invoke(this, username);
            
            System.Diagnostics.Debug.WriteLine($"[FORCE LOGOUT] User {username} forced to logout (banned)");
        }

        /// <summary>
        /// Update user's last activity time
        /// </summary>
        public void UpdateLastActivity(string username)
        {
            _lastActivity[username] = DateTime.Now;

            if (_onlineUsers.TryGetValue(username, out var session))
            {
                session.LastActivityTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Check if user is online
        /// </summary>
        public bool IsUserOnline(string username)
        {
            bool isOnline = _onlineUsers.ContainsKey(username);
            System.Diagnostics.Debug.WriteLine($"[OnlineUserStatusService] IsUserOnline check: '{username}' = {isOnline}");
            return isOnline;
        }

        /// <summary>
        /// Get all online users (from cloud)
        /// </summary>
        public async Task<List<string>> GetOnlineUsersAsync()
        {
            if (_cosmosService == null)
            {
                return _onlineUsers.Keys.ToList();
            }
            
            try
            {
                var cloudUsers = await _cosmosService.GetOnlineUsersAsync();
                return cloudUsers.Select(u => u.Username).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ONLINE] Error getting online users from cloud: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Get all online users (synchronous - from local cache)
        /// </summary>
        public List<User> GetOnlineUsers()
        {
            var users = _onlineUsers.Values.Select(s => s.User).ToList();
            System.Diagnostics.Debug.WriteLine($"[OnlineUserStatusService] GetOnlineUsers called: returning {users.Count} users");
            return users;
        }

        /// <summary>
        /// Get online user count
        /// </summary>
        public int GetOnlineUserCount()
        {
            int count = _onlineUsers.Count;
            System.Diagnostics.Debug.WriteLine($"[OnlineUserStatusService] GetOnlineUserCount called: {count} users online");
            return count;
        }

        /// <summary>
        /// Get online users by role
        /// </summary>
        public List<User> GetOnlineUsersByRole(UserRole role)
        {
            return _onlineUsers.Values
                .Where(s => s.User.Role == role)
                .Select(s => s.User)
                .ToList();
        }

        /// <summary>
        /// Get online DJs
        /// </summary>
        public List<User> GetOnlineDJs()
        {
            return _onlineUsers.Values
                .Where(s => s.User.IsDJ)
                .Select(s => s.User)
                .ToList();
        }

        /// <summary>
        /// Get online venue owners
        /// </summary>
        public List<User> GetOnlineVenueOwners()
        {
            return _onlineUsers.Values
                .Where(s => s.User.IsVenueOwner)
                .Select(s => s.User)
                .ToList();
        }

        /// <summary>
        /// Get user session information
        /// </summary>
        public UserSessionInfo? GetUserSession(string username)
        {
            return _onlineUsers.TryGetValue(username, out var session) ? session : null;
        }

        /// <summary>
        /// Get detailed user info for display
        /// </summary>
        public OnlineUserInfo? GetUserInfo(string username)
        {
            if (_onlineUsers.TryGetValue(username, out var session))
            {
                // Format location
                string location = "Unknown";
                if (!string.IsNullOrEmpty(session.User.City) && !string.IsNullOrEmpty(session.User.Country))
                {
                    location = $"{session.User.City}, {session.User.Country}";
                }
                else if (!string.IsNullOrEmpty(session.User.Country))
                {
                    location = session.User.Country;
                }

                return new OnlineUserInfo
                {
                    Username = session.User.Username,
                    FullName = session.User.FullName,
                    Role = session.User.Role.ToString(),
                    Email = session.User.Email,
                    IsDJ = session.User.IsDJ,
                    IsVenueOwner = session.User.IsVenueOwner,
                    LoginTime = session.LoginTime,
                    LastActivityTime = session.LastActivityTime,
                    Location = location,
                    IPAddress = session.User.CurrentIP ?? "Unknown"
                };
            }

            return null;
        }

        /// <summary>
        /// Get session duration for a user
        /// </summary>
        public TimeSpan? GetSessionDuration(string username)
        {
            if (_onlineUsers.TryGetValue(username, out var session))
            {
                return DateTime.Now - session.LoginTime;
            }

            return null;
        }

        /// <summary>
        /// Get time since last activity
        /// </summary>
        public TimeSpan? GetTimeSinceLastActivity(string username)
        {
            if (_lastActivity.TryGetValue(username, out var lastActivity))
            {
                return DateTime.Now - lastActivity;
            }

            return null;
        }

        /// <summary>
        /// Clear all online users (for app shutdown)
        /// </summary>
        public void ClearAllUsers()
        {
            var usernames = _onlineUsers.Keys.ToList();
            
            foreach (var username in usernames)
            {
                SetUserOffline(username);
            }

            _onlineUsers.Clear();
            _lastActivity.Clear();

            System.Diagnostics.Debug.WriteLine("All users marked offline");
        }

        /// <summary>
        /// Get statistics
        /// </summary>
        public OnlineUserStatistics GetStatistics()
        {
            var onlineUsers = _onlineUsers.Values.ToList();

            return new OnlineUserStatistics
            {
                TotalOnline = onlineUsers.Count,
                AdminsOnline = onlineUsers.Count(s => s.User.Role == UserRole.SysAdmin || s.User.Role == UserRole.Manager),
                DJsOnline = onlineUsers.Count(s => s.User.IsDJ),
                VenueOwnersOnline = onlineUsers.Count(s => s.User.IsVenueOwner),
                RegularUsersOnline = onlineUsers.Count(s => s.User.Role == UserRole.User && !s.User.IsDJ && !s.User.IsVenueOwner),
                AverageSessionDuration = onlineUsers.Any() 
                    ? TimeSpan.FromSeconds(onlineUsers.Average(s => (DateTime.Now - s.LoginTime).TotalSeconds))
                    : TimeSpan.Zero
            };
        }
    }

    /// <summary>
    /// User session information
    /// </summary>
    public class UserSessionInfo
    {
        public User User { get; set; } = null!;
        public DateTime LoginTime { get; set; }
        public DateTime LastActivityTime { get; set; }
        public bool IsOnline { get; set; }
        public string? LoginIP { get; set; }
        public string? LoginLocation { get; set; }
    }

    /// <summary>
    /// Event args for user status changes
    /// </summary>
    public class UserStatusEventArgs : EventArgs
    {
        public string Username { get; }
        public bool IsOnline { get; }

        public UserStatusEventArgs(string username, bool isOnline)
        {
            Username = username;
            IsOnline = isOnline;
        }
    }

    /// <summary>
    /// Online user statistics
    /// </summary>
    public class OnlineUserStatistics
    {
        public int TotalOnline { get; set; }
        public int AdminsOnline { get; set; }
        public int DJsOnline { get; set; }
        public int VenueOwnersOnline { get; set; }
        public int RegularUsersOnline { get; set; }
        public TimeSpan AverageSessionDuration { get; set; }
    }
}
