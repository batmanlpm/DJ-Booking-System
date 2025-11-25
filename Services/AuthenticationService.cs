using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DJBookingSystem.Models;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Authentication and user session management service
    /// Handles login, registration, password management, and user sessions
    /// </summary>
    public class AuthenticationService
    {
        private static AuthenticationService? _instance;
        private static readonly object _lock = new();

        private CosmosDbService? _cosmosService;
        private User? _currentUser;
        private DateTime _sessionStartTime;

        public event EventHandler<User>? UserLoggedIn;
        public event EventHandler? UserLoggedOut;
        public event EventHandler<UserActionLog>? UserActionLogged;

        private AuthenticationService()
        {
        }

        public static AuthenticationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new AuthenticationService();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Initialize the authentication service with database connection
        /// </summary>
        public void Initialize(CosmosDbService cosmosService)
        {
            _cosmosService = cosmosService ?? throw new ArgumentNullException(nameof(cosmosService));
        }

        /// <summary>
        /// Set current user during auto-login (internal use only)
        /// </summary>
        internal async Task SetCurrentUserForAutoLoginAsync(User user, string ipAddress)
        {
            if (_cosmosService == null)
                throw new InvalidOperationException("Database service not initialized");

            // Set current user and start session
            _currentUser = user;
            _sessionStartTime = DateTime.Now;

            // Update last login and IP
            user.LastLogin = DateTime.Now;
            user.CurrentIP = ipAddress;

            if (!string.IsNullOrEmpty(ipAddress) && !user.IPHistory.Contains(ipAddress))
            {
                user.IPHistory.Add(ipAddress);
            }

            await _cosmosService.UpdateUserAsync(user);

            // Mark user as online
            OnlineUserStatusService.Instance.SetUserOnline(user);

            System.Diagnostics.Debug.WriteLine($"[AuthService] Auto-login initialized for {user.Username}");

            // Log the action
            await LogActionAsync(user.Id, UserActionType.Login, $"User auto-logged in: {user.Username} (IP: {ipAddress})");

            // Fire event
            UserLoggedIn?.Invoke(this, user);
        }

        /// <summary>
        /// Current logged-in user
        /// </summary>
        public User? CurrentUser => _currentUser;

        /// <summary>
        /// Check if a user is logged in
        /// </summary>
        public bool IsAuthenticated => _currentUser != null;

        /// <summary>
        /// Get current user's role
        /// </summary>
        public UserRole CurrentRole => _currentUser?.Role ?? UserRole.User;

        /// <summary>
        /// Check if current user is admin
        /// </summary>
        public bool IsAdmin => _currentUser?.Role == UserRole.SysAdmin;

        /// <summary>
        /// Check if current user is manager or above
        /// </summary>
        public bool IsManager => _currentUser?.Role >= UserRole.Manager;

        /// <summary>
        /// Check if current user has a specific permission
        /// </summary>
        public bool HasPermission(Func<UserPermissions, bool> permissionCheck)
        {
            return _currentUser != null && permissionCheck(_currentUser.Permissions);
        }

        /// <summary>
        /// Hash password using SHA256
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }

        /// <summary>
        /// Authenticate user with username and password
        /// </summary>
        public async Task<(bool Success, string Message, User? User)> LoginAsync(string username, string password, string ipAddress = "")
        {
            if (_cosmosService == null)
            {
                return (false, "Database service not initialized", null);
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Username and password are required", null);
            }

            try
            {
                // Get all users and find by username (case-insensitive)
                var users = await _cosmosService.GetAllUsersAsync();
                var user = users.FirstOrDefault(u => 
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

                if (user == null)
                {
                    await LogActionAsync(null, UserActionType.Login, $"Failed login attempt for username: {username}");
                    return (false, "Invalid username or password", null);
                }

                // Check if user is banned
                if (user.IsBanned)
                {
                    if (user.BanExpiry.HasValue && user.BanExpiry.Value < DateTime.Now)
                    {
                        // Ban expired - unban user
                        user.IsBanned = false;
                        user.BannedBy = null;
                        user.BannedAt = null;
                        user.BanReason = null;
                        user.BanExpiry = null;
                        user.BannedIP = null;
                        await _cosmosService.UpdateUserAsync(user);
                    }
                    else
                    {
                        string banMessage = user.BanExpiry.HasValue 
                            ? $"Your account is banned until {user.BanExpiry.Value:g}\n\nReason: {user.BanReason}"
                            : $"Your account is permanently banned\n\nReason: {user.BanReason}";
                        
                        await LogActionAsync(user.Id, UserActionType.Login, $"Banned user attempted login: {username}");
                        return (false, banMessage, null);
                    }
                }

                // Check if account is active
                if (!user.IsActive)
                {
                    await LogActionAsync(user.Id, UserActionType.Login, $"Inactive account login attempt: {username}");
                    return (false, "Your account is inactive. Please contact an administrator.", null);
                }

                // Verify password
                string passwordHash = HashPassword(password);
                if (user.PasswordHash != passwordHash)
                {
                    await LogActionAsync(user.Id, UserActionType.Login, $"Failed login attempt - wrong password: {username}");
                    return (false, "Invalid username or password", null);
                }

                // ?? FIX: Get WAN IP instead of using passed local IP
                string wanIP = await GetPublicIPAddressAsync();
                System.Diagnostics.Debug.WriteLine($"[Login] WAN IP: {wanIP} (was passed: {ipAddress})");

                // Update last login and IP tracking
                user.LastLogin = DateTime.Now;
                
                if (!string.IsNullOrEmpty(wanIP))
                {
                    user.CurrentIP = wanIP;
                    
                    if (!user.IPHistory.Contains(wanIP))
                    {
                        user.IPHistory.Add(wanIP);
                    }

                    // Lookup geolocation from IP (async but don't await to not slow login)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var location = await GeoLocationService.GetLocationFromIPAsync(wanIP);
                            if (location != null && _cosmosService != null)
                            {
                                user.City = location.City;
                                user.Country = location.Country;
                                user.Latitude = location.Latitude;
                                user.Longitude = location.Longitude;
                                user.Timezone = location.Timezone;
                                user.ISP = location.ISP;

                                // Add to location history
                                user.LocationHistory.Add(new UserLocationLog
                                {
                                    Timestamp = DateTime.Now,
                                    IPAddress = wanIP,
                                    City = location.City,
                                    Country = location.Country,
                                    Latitude = location.Latitude,
                                    Longitude = location.Longitude,
                                    ISP = location.ISP,
                                    Action = "Login"
                                });

                                // Update user in database
                                await _cosmosService.UpdateUserAsync(user);

                                System.Diagnostics.Debug.WriteLine($"Location updated for {user.Username}: {location.DisplayLocation}");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Geolocation lookup failed: {ex.Message}");
                        }
                    });
                }

                await _cosmosService.UpdateUserAsync(user);

                // Set current user and start session
                _currentUser = user;
                _sessionStartTime = DateTime.Now;

                // ========================================
                // FORCE USER ONLINE - CRITICAL!
                // ========================================
                System.Diagnostics.Debug.WriteLine("====================================");
                System.Diagnostics.Debug.WriteLine($"[AuthService] ABOUT TO SET USER ONLINE");
                System.Diagnostics.Debug.WriteLine($"[AuthService] Username: {user.Username}");
                System.Diagnostics.Debug.WriteLine($"[AuthService] Role: {user.Role}");
                System.Diagnostics.Debug.WriteLine("====================================");
                
                // Mark user as online - WAIT for cloud write to complete!
                await OnlineUserStatusService.Instance.SetUserOnlineAsync(user);
                
                System.Diagnostics.Debug.WriteLine("====================================");
                System.Diagnostics.Debug.WriteLine($"[AuthService] SetUserOnlineAsync COMPLETED!");
                System.Diagnostics.Debug.WriteLine($"[AuthService] User written to Cosmos DB");
                System.Diagnostics.Debug.WriteLine($"[AuthService] Online count: {OnlineUserStatusService.Instance.GetOnlineUserCount()}");
                System.Diagnostics.Debug.WriteLine("====================================");

                // Log successful login with location
                string locationInfo = !string.IsNullOrEmpty(user.City) ? $" from {user.City}, {user.Country}" : "";
                await LogActionAsync(user.Id, UserActionType.Login, $"User logged in: {username}{locationInfo} (IP: {wanIP})");

                // Fire event
                UserLoggedIn?.Invoke(this, user);

                return (true, "Login successful", user);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthService] Login error: {ex.Message}");
                return (false, $"Login failed: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        public async Task<(bool Success, string Message, User? User)> RegisterAsync(
            string username, 
            string password, 
            string email, 
            string fullName,
            string ipAddress = "",
            bool isDJ = false,
            bool isVenueOwner = false)
        {
            if (_cosmosService == null)
            {
                return (false, "Database service not initialized", null);
            }

            // Validate input
            if (string.IsNullOrWhiteSpace(username))
                return (false, "Username is required", null);

            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password is required", null);

            if (password.Length < 6)
                return (false, "Password must be at least 6 characters long", null);

            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email is required", null);

            if (!email.Contains("@"))
                return (false, "Invalid email format", null);

            try
            {
                // Check if username already exists
                var existingUsers = await _cosmosService.GetAllUsersAsync();
                
                if (existingUsers.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                {
                    return (false, "Username already exists", null);
                }

                if (existingUsers.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                {
                    return (false, "Email already registered", null);
                }

                // Create new user
                var newUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = username,
                    PasswordHash = HashPassword(password),
                    Email = email,
                    FullName = fullName,
                    Role = UserRole.User,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    IsDJ = isDJ,
                    IsVenueOwner = isVenueOwner,
                    RegisteredIP = ipAddress,
                    CurrentIP = ipAddress
                };

                if (!string.IsNullOrEmpty(ipAddress))
                {
                    newUser.IPHistory.Add(ipAddress);
                }

                // Set default permissions for regular users
                newUser.Permissions = new UserPermissions
                {
                    CanViewBookings = true,
                    CanCreateBookings = true,
                    CanEditBookings = true,
                    CanDeleteBookings = false,
                    CanViewVenues = true,
                    CanRegisterVenues = isDJ || isVenueOwner,
                    CanEditVenues = false,
                    CanDeleteVenues = false,
                    CanAccessSettings = true
                };

                // Add user to database
                await _cosmosService.AddUserAsync(newUser);

                // Log action
                await LogActionAsync(newUser.Id, UserActionType.Created, $"New user registered: {username}");

                return (true, "Registration successful", newUser);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthService] Registration error: {ex.Message}");
                return (false, $"Registration failed: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Logout current user
        /// </summary>
        public async Task LogoutAsync()
        {
            if (_currentUser != null)
            {
                var username = _currentUser.Username;
                
                await LogActionAsync(_currentUser.Id, UserActionType.Logout, 
                    $"User logged out: {username}, Session duration: {(DateTime.Now - _sessionStartTime).TotalMinutes:F1} minutes");

                // Mark user as offline
                OnlineUserStatusService.Instance.SetUserOffline(username);

                _currentUser = null;
                _sessionStartTime = DateTime.MinValue;

                UserLoggedOut?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        public async Task<(bool Success, string Message)> ChangePasswordAsync(
            string userId, 
            string oldPassword, 
            string newPassword)
        {
            if (_cosmosService == null)
                return (false, "Database service not initialized");

            if (string.IsNullOrWhiteSpace(newPassword))
                return (false, "New password is required");

            if (newPassword.Length < 6)
                return (false, "New password must be at least 6 characters long");

            try
            {
                var user = await _cosmosService.GetUserByIdAsync(userId);
                if (user == null)
                    return (false, "User not found");

                // Verify old password
                if (user.PasswordHash != HashPassword(oldPassword))
                    return (false, "Current password is incorrect");

                // Update password
                user.PasswordHash = HashPassword(newPassword);
                await _cosmosService.UpdateUserAsync(user);

                await LogActionAsync(userId, UserActionType.Updated, "Password changed");

                return (true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to change password: {ex.Message}");
            }
        }

        /// <summary>
        /// Reset user password (admin function)
        /// </summary>
        public async Task<(bool Success, string Message, string? NewPassword)> ResetPasswordAsync(string userId)
        {
            if (_cosmosService == null)
                return (false, "Database service not initialized", null);

            if (_currentUser == null || _currentUser.Role != UserRole.SysAdmin)
                return (false, "Insufficient permissions", null);

            try
            {
                var user = await _cosmosService.GetUserByIdAsync(userId);
                if (user == null)
                    return (false, "User not found", null);

                // Generate random password
                string newPassword = GenerateRandomPassword();
                user.PasswordHash = HashPassword(newPassword);

                await _cosmosService.UpdateUserAsync(user);

                await LogActionAsync(userId, UserActionType.Updated, 
                    $"Password reset by admin: {_currentUser.Username}");

                return (true, "Password reset successfully", newPassword);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to reset password: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Generate random password
        /// </summary>
        private string GenerateRandomPassword(int length = 10)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Log user action
        /// </summary>
        private async Task LogActionAsync(string? userId, UserActionType actionType, string details)
        {
            if (_cosmosService == null) return;

            try
            {
                var log = new UserActionLog
                {
                    UserId = userId ?? "System",
                    Username = userId != null ? (await _cosmosService.GetUserByIdAsync(userId))?.Username ?? "Unknown" : "System",
                    Timestamp = DateTime.Now,
                    ActionType = actionType,
                    ActionDetails = details,
                    PerformedBy = _currentUser?.Username
                };

                await _cosmosService.AddUserActionLogAsync(log);

                UserActionLogged?.Invoke(this, log);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthService] Failed to log action: {ex.Message}");
            }
        }

        /// <summary>
        /// Get user session duration
        /// </summary>
        public TimeSpan GetSessionDuration()
        {
            if (_currentUser == null) return TimeSpan.Zero;
            return DateTime.Now - _sessionStartTime;
        }

        /// <summary>
        /// Get user's IP address
        /// </summary>
        public static string GetLocalIPAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        /// <summary>
        /// Get user's public WAN IP address (for IP banning and tracking)
        /// </summary>
        public static async Task<string> GetPublicIPAddressAsync()
        {
            try
            {
                using var httpClient = new System.Net.Http.HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                
                // Try primary service
                try
                {
                    string ip = await httpClient.GetStringAsync("https://api.ipify.org");
                    if (!string.IsNullOrWhiteSpace(ip))
                    {
                        System.Diagnostics.Debug.WriteLine($"[WAN IP] Retrieved from ipify: {ip}");
                        return ip.Trim();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[WAN IP] ipify failed: {ex.Message}");
                }

                // Fallback to secondary service
                try
                {
                    string ip = await httpClient.GetStringAsync("https://icanhazip.com");
                    if (!string.IsNullOrWhiteSpace(ip))
                    {
                        System.Diagnostics.Debug.WriteLine($"[WAN IP] Retrieved from icanhazip: {ip}");
                        return ip.Trim();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[WAN IP] icanhazip failed: {ex.Message}");
                }

                // Last fallback - use local IP
                System.Diagnostics.Debug.WriteLine("[WAN IP] All services failed, using local IP");
                return GetLocalIPAddress();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WAN IP] Critical error: {ex.Message}");
                return GetLocalIPAddress();
            }
        }
    }
}
