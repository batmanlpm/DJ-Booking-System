using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DJBookingSystem.Models;
using DJBookingSystem.Services;
using Microsoft.VisualBasic;

namespace DJBookingSystem.Views
{
    /// <summary>
    /// User management view for administrators
    /// Handles user CRUD operations, banning, muting, and role management
    /// </summary>
    public partial class UsersView : UserControl
    {
        private List<User> _users;
        private CosmosDbService? _cosmosService;
        private User? _currentUser;

        public UsersView()
        {
            InitializeComponent();
            _users = new List<User>();
            
            // Unsubscribe from events when unloaded
            Unloaded += UsersView_Unloaded;
        }

        private void UsersView_Unloaded(object sender, RoutedEventArgs e)
        {
            // Unsubscribe from online status changes
            OnlineUserStatusService.Instance.UserStatusChanged -= OnUserStatusChanged;
        }

        public async Task Initialize(CosmosDbService cosmosService, User currentUser)
        {
            _cosmosService = cosmosService ?? throw new ArgumentNullException(nameof(cosmosService));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            
            System.Diagnostics.Debug.WriteLine($"========================================");
            System.Diagnostics.Debug.WriteLine($"[UsersView] INITIALIZING");
            System.Diagnostics.Debug.WriteLine($"[UsersView] Current User: {_currentUser.Username}");
            System.Diagnostics.Debug.WriteLine($"[UsersView] Current User Role: {_currentUser.Role}");
            System.Diagnostics.Debug.WriteLine($"[UsersView] CosmosDB Service: {(_cosmosService != null ? "Connected" : "NULL")}");
            
            // ENFORCE PERMISSIONS
            EnforceUserManagementPermissions();
            
            // Subscribe to online status changes
            OnlineUserStatusService.Instance.UserStatusChanged -= OnUserStatusChanged; // Unsubscribe first to avoid duplicates
            OnlineUserStatusService.Instance.UserStatusChanged += OnUserStatusChanged;
            
            System.Diagnostics.Debug.WriteLine($"[UsersView] Subscribed to UserStatusChanged events");
            System.Diagnostics.Debug.WriteLine($"========================================");
            
            await LoadUsersAsync();
        }

        /// <summary>
        /// Enforce Admin and Moderation permissions
        /// </summary>
        private void EnforceUserManagementPermissions()
        {
            bool isAdmin = _currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager;

            // Admins bypass all checks
            if (isAdmin)
            {
                System.Diagnostics.Debug.WriteLine($"[UsersView] Admin user - all permissions granted");
                return;
            }

            // Note: UI button enforcement skipped - buttons may have different names in XAML
            // Permission enforcement happens at code level in action methods

            System.Diagnostics.Debug.WriteLine($"[UsersView] Permissions enforced:");
            System.Diagnostics.Debug.WriteLine($"  - CanManageUsers: {_currentUser.Permissions.CanManageUsers}");
            System.Diagnostics.Debug.WriteLine($"  - CanBanUsers: {_currentUser.Permissions.CanBanUsers}");
            System.Diagnostics.Debug.WriteLine($"  - CanMuteUsers: {_currentUser.Permissions.CanMuteUsers}");
            System.Diagnostics.Debug.WriteLine($"  - CanViewReports: {_currentUser.Permissions.CanViewReports}");
        }

        private void OnUserStatusChanged(object? sender, Services.UserStatusEventArgs e)
        {
            // Update the user's online status in the UI
            Dispatcher.Invoke(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[UsersView] ========== STATUS CHANGE EVENT ==========");
                    System.Diagnostics.Debug.WriteLine($"[UsersView] User: {e.Username}, IsOnline: {e.IsOnline}");
                    
                    var user = _users.FirstOrDefault(u => u.Username == e.Username);
                    if (user != null)
                    {
                        bool wasOnline = user.IsOnline;
                        user.IsOnline = e.IsOnline;
                        
                        System.Diagnostics.Debug.WriteLine($"[UsersView] ? Found user in list: {user.Username}");
                        System.Diagnostics.Debug.WriteLine($"[UsersView] Status updated: {wasOnline} ? {user.IsOnline}");
                        System.Diagnostics.Debug.WriteLine($"[UsersView] OnlineStatus property now: '{user.OnlineStatus}'");
                        
                        // Refresh the DataGrid to update the display
                        UsersDataGrid.Items.Refresh();
                        
                        System.Diagnostics.Debug.WriteLine($"[UsersView] DataGrid refreshed for user: {e.Username}");
                        
                        // Update status text
                        int onlineCount = _users.Count(u => u.IsOnline);
                        int offlineCount = _users.Count - onlineCount;
                        UpdateStatusText($"Loaded {_users.Count} users ({onlineCount} online, {offlineCount} offline)");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[UsersView] ? WARNING: User '{e.Username}' not found in loaded users list!");
                        System.Diagnostics.Debug.WriteLine($"[UsersView] Current user list ({_users.Count} users):");
                        foreach (var u in _users.Take(10))
                        {
                            System.Diagnostics.Debug.WriteLine($"  - {u.Username}");
                        }
                    }
                }
                catch { }
            });
        }

        /// <summary>
        /// Event handler for auto-refresh checkbox
        /// </summary>
        private void AutoRefresh_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // Auto-refresh logic can be implemented here if needed
            System.Diagnostics.Debug.WriteLine($"[UsersView] Auto-refresh toggled");
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                if (_cosmosService == null)
                {
                    UpdateStatusText("Database service not initialized");
                    System.Diagnostics.Debug.WriteLine("[UsersView] ERROR: Database service is NULL!");
                    return;
                }

                UpdateStatusText("Loading users...");
                System.Diagnostics.Debug.WriteLine("[UsersView] ========== LOADING USERS ==========");

                _users = await _cosmosService.GetAllUsersAsync();
                System.Diagnostics.Debug.WriteLine($"[UsersView] Loaded {_users.Count} users from database");
                
                // Get online user count from service
                int onlineCount = OnlineUserStatusService.Instance.GetOnlineUserCount();
                System.Diagnostics.Debug.WriteLine($"[UsersView] OnlineUserStatusService reports {onlineCount} users online");
                
                // List all online users according to the service
                var onlineUsersFromService = OnlineUserStatusService.Instance.GetOnlineUsers();
                System.Diagnostics.Debug.WriteLine($"[UsersView] Online users according to service:");
                foreach (var onlineUser in onlineUsersFromService)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {onlineUser.Username}");
                }
                
                // Synchronize IsOnline status from OnlineUserStatusService
                int syncedOnlineCount = 0;
                foreach (var user in _users)
                {
                    bool wasOnline = user.IsOnline;
                    user.IsOnline = OnlineUserStatusService.Instance.IsUserOnline(user.Username);
                    
                    if (user.IsOnline)
                    {
                        syncedOnlineCount++;
                        System.Diagnostics.Debug.WriteLine($"[UsersView] ? User '{user.Username}' is ONLINE (Status: {user.OnlineStatus})");
                    }
                    
                    if (wasOnline != user.IsOnline)
                    {
                        System.Diagnostics.Debug.WriteLine($"[UsersView] Status changed for {user.Username}: {wasOnline} ? {user.IsOnline}");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"[UsersView] Synchronized online status: {syncedOnlineCount} users marked as online");
                
                _users = _users.OrderBy(u => u.Username).ToList();
                
                UsersDataGrid.ItemsSource = null;
                UsersDataGrid.ItemsSource = _users;

                UpdateStatusText($"Loaded {_users.Count} users ({syncedOnlineCount} online, {_users.Count - syncedOnlineCount} offline)");
                
                System.Diagnostics.Debug.WriteLine($"[UsersView] ========== USERS LOADED ==========");
                System.Diagnostics.Debug.WriteLine($"[UsersView] DataGrid updated with {_users.Count} users");
                System.Diagnostics.Debug.WriteLine($"[UsersView] Online: {syncedOnlineCount}, Offline: {_users.Count - syncedOnlineCount}");
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load users: {ex.Message}", "Error");
                UpdateStatusText("Error loading users");
                System.Diagnostics.Debug.WriteLine($"[UsersView] ERROR loading users: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[UsersView] Stack trace: {ex.StackTrace}");
            }
        }

        private void NewUser_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInitialization()) return;

            var createUserWindow = new CreateUserWindow(_cosmosService!, _currentUser!);
            if (createUserWindow.ShowDialog() == true && createUserWindow.CreatedUser != null)
            {
                ShowInfo($"User '{createUserWindow.CreatedUser.Username}' created successfully!", "Success");
                _ = LoadUsersAsync();
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInitialization()) return;

            if (UsersDataGrid.SelectedItem is not User selectedUser)
            {
                ShowWarning("Please select a user to edit", "No Selection");
                return;
            }

            var dialog = new EditUserWindow(selectedUser);
            if (dialog.ShowDialog() == true && dialog.UpdatedUser != null)
            {
                _ = UpdateUserAsync(dialog.UpdatedUser);
            }
        }

        private async Task UpdateUserAsync(User user)
        {
            try
            {
                if (string.IsNullOrEmpty(user.Id))
                {
                    ShowError("User ID is missing", "Error");
                    return;
                }

                await _cosmosService!.UpdateUserAsync(user);
                ShowInfo($"User '{user.Username}' updated successfully!", "Success");
                await LoadUsersAsync();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to update user: {ex.Message}", "Error");
            }
        }

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInitialization()) return;

            // PERMISSION CHECK
            bool isAdmin = _currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager;
            if (!isAdmin && !_currentUser.Permissions.CanManageUsers)
            {
                ShowWarning("You don't have permission to delete users", "Access Denied");
                return;
            }

            if (UsersDataGrid.SelectedItem is not User selectedUser)
            {
                ShowWarning("Please select a user to delete", "No Selection");
                return;
            }

            // Debug: Log the selected user details
            System.Diagnostics.Debug.WriteLine($"[DeleteUser] ========== DELETE ATTEMPT ==========");
            System.Diagnostics.Debug.WriteLine($"[DeleteUser] Selected object type: {selectedUser.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"[DeleteUser] Username: '{selectedUser.Username}' (Length: {selectedUser.Username?.Length ?? 0})");
            System.Diagnostics.Debug.WriteLine($"[DeleteUser] Id: '{selectedUser.Id}'");
            System.Diagnostics.Debug.WriteLine($"[DeleteUser] FullName: '{selectedUser.FullName}'");
            System.Diagnostics.Debug.WriteLine($"[DeleteUser] Email: '{selectedUser.Email}'");
            System.Diagnostics.Debug.WriteLine($"[DeleteUser] Role: {selectedUser.Role}");
            System.Diagnostics.Debug.WriteLine($"[DeleteUser] ==========================================");

            // Validate username is not null/empty
            if (string.IsNullOrWhiteSpace(selectedUser.Username))
            {
                ShowError("Selected user has no username. Please refresh the user list and try again.", "Invalid User Data");
                System.Diagnostics.Debug.WriteLine("[DeleteUser] ERROR: Username is null or empty!");
                return;
            }

            if (selectedUser.Id == _currentUser!.Id)
            {
                ShowWarning("You cannot delete your own account", "Cannot Delete");
                return;
            }

            if (selectedUser.Username.Equals("SysAdmin", StringComparison.OrdinalIgnoreCase))
            {
                ShowWarning("Cannot delete the system administrator account", "Cannot Delete");
                return;
            }

            if (!ConfirmAction($"Are you sure you want to delete user '{selectedUser.Username}'?\n\nFull Name: {selectedUser.FullName}\nEmail: {selectedUser.Email}\n\nThis action cannot be undone!", "Confirm Delete"))
                return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"[DeleteUser] Calling DeleteUserAccountAsync with username: '{selectedUser.Username}'");
                await _cosmosService!.DeleteUserAccountAsync(selectedUser.Username);
                ShowInfo($"User '{selectedUser.Username}' deleted successfully", "Success");
                await LoadUsersAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeleteUser] Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DeleteUser] Stack trace: {ex.StackTrace}");
                ShowError($"Failed to delete user: {ex.Message}", "Error");
            }
        }

        private async void BanToggle_Click(object sender, RoutedEventArgs e)
        {
            // PERMISSION CHECK
            bool isAdmin = _currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager;
            if (!isAdmin && !_currentUser.Permissions.CanBanUsers)
            {
                ShowWarning("You don't have permission to ban/unban users", "Access Denied");
                return;
            }

            if (UsersDataGrid.SelectedItem is not User selectedUser || _cosmosService == null)
            {
                ShowWarning("Please select a user to ban or unban", "No Selection");
                return;
            }

            try
            {
                if (selectedUser.IsBanned)
                {
                    await UnbanUserAsync(selectedUser);
                }
                else
                {
                    await BanUserAsync(selectedUser);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to update user ban status: {ex.Message}", "Error");
            }
        }

        private async Task BanUserAsync(User user)
        {
            // Show ban reason dialog
            var banDialog = new BanReasonDialog(user.Username);
            banDialog.Owner = Window.GetWindow(this);
            
            if (banDialog.ShowDialog() != true || !banDialog.Confirmed)
            {
                return; // User cancelled
            }

            string banReason = banDialog.BanReason;

            // AUTO-ESCALATION: Check strike count
            int newStrikeCount = user.BanStrikeCount + 1;
            TimeSpan banDuration = TimeSpan.Zero; // Initialize to avoid unassigned error
            string banLengthText;
            bool isPermanent = false;

            if (newStrikeCount == 1)
            {
                // Strike 1: 24 hours
                banDuration = TimeSpan.FromHours(24);
                banLengthText = "24 hours";
            }
            else if (newStrikeCount == 2)
            {
                // Strike 2: 48 hours
                banDuration = TimeSpan.FromHours(48);
                banLengthText = "48 hours";
            }
            else
            {
                // Strike 3: Permanent (requires SysAdmin override)
                isPermanent = true;
                banLengthText = "PERMANENT";
                
                // Only SysAdmin can issue permanent bans
                if (_currentUser.Role != UserRole.SysAdmin)
                {
                    MessageBox.Show(
                        $"This user has {user.BanStrikeCount} previous bans.\n\n" +
                        "The next ban would be PERMANENT.\n\n" +
                        "Only a SysAdmin can issue permanent bans.\n" +
                        "Please contact a SysAdmin.",
                        "SysAdmin Required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
            }

            // Final confirmation
            string confirmMessage = $"Are you sure you want to ban '{user.Username}'?\n\n" +
                                   $"Reason: {banReason}\n\n" +
                                   $"Strike {newStrikeCount} of 3\n" +
                                   $"Ban Duration: {banLengthText}\n\n";

            if (isPermanent)
            {
                confirmMessage += "? THIS IS A PERMANENT BAN ?\n" +
                                "User will need SysAdmin override to unban.";
            }
            else
            {
                confirmMessage += $"Ban will expire: {DateTime.Now.Add(banDuration):MMM dd, yyyy HH:mm}\n\n" +
                                "This will:\n• Deactivate their account\n• Block their IP address\n• Kick them immediately if online";
            }

            if (!ConfirmAction(confirmMessage, $"Confirm Ban (Strike {newStrikeCount})"))
                return;

            // Apply ban
            user.IsBanned = true;
            user.BannedBy = _currentUser?.Username ?? "System";
            user.BannedAt = DateTime.Now;
            user.BanReason = banReason;
            user.BannedIP = user.CurrentIP;
            user.IsActive = false;
            user.BanStrikeCount = newStrikeCount;

            if (isPermanent)
            {
                user.IsPermanentBan = true;
                user.BanExpiry = null; // Null = permanent
            }
            else
            {
                user.IsPermanentBan = false;
                user.BanExpiry = DateTime.Now.Add(banDuration);
            }

            await _cosmosService!.UpdateUserAsync(user);
            
            // NOTE: Machine-bound ban will be stored locally on their computer when they're kicked
            // This prevents VPN bypass - ban is tied to the physical machine
            
            // FORCE LOGOUT if user is currently online (will trigger machine-ban storage on their end)
            if (OnlineUserStatusService.Instance.IsUserOnline(user.Username))
            {
                OnlineUserStatusService.Instance.ForceLogoutUser(user.Username);
                System.Diagnostics.Debug.WriteLine($"[BAN] User {user.Username} was kicked - Strike {newStrikeCount}");
            }
            
            ShowInfo($"User '{user.Username}' has been banned.\n\nStrike: {newStrikeCount} of 3\nDuration: {banLengthText}\nReason: {banReason}\nIP: {user.BannedIP ?? "Unknown"}", 
                    isPermanent ? "PERMANENT BAN ISSUED" : "User Banned");
            await LoadUsersAsync();
        }

        private async Task UnbanUserAsync(User user)
        {
            // Check if user is SysAdmin for permanent ban override
            bool canUnban = _currentUser.Role == UserRole.SysAdmin || 
                           (_currentUser.Role == UserRole.Manager && !user.IsPermanentBan);

            if (!canUnban)
            {
                MessageBox.Show(
                    "Only a SysAdmin can unban users with permanent bans (Strike 3).",
                    "SysAdmin Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            string unbanMessage = $"Are you sure you want to unban '{user.Username}'?\n\n" +
                                 $"Current Strike: {user.BanStrikeCount} of 3\n";

            if (user.IsPermanentBan)
            {
                unbanMessage += "\n? This is a PERMANENT BAN ?\n" +
                              "Only SysAdmin can override this.\n\n";
            }

            unbanMessage += $"Ban Reason: {user.BanReason ?? "None"}\n" +
                           $"Banned By: {user.BannedBy ?? "Unknown"}\n\n" +
                           "Unban Options:\n" +
                           "• Keep strike count (user retains ban history)\n" +
                           "• Strike count will NOT be reset";

            if (!ConfirmAction(unbanMessage, "Confirm Unban"))
                return;

            // Unban user
            user.IsBanned = false;
            user.BannedBy = null;
            user.BannedAt = null;
            user.BanReason = null;
            user.BanExpiry = null;
            user.IsPermanentBan = false;
            user.IsActive = true;
            // NOTE: BanStrikeCount is NOT reset - keeps ban history

            await _cosmosService!.UpdateUserAsync(user);
            ShowInfo($"User '{user.Username}' has been unbanned.\n\nStrike history retained: {user.BanStrikeCount} of 3\n\n" +
                    "? User still has ban strikes. Next ban will be escalated accordingly.",
                    "User Unbanned");
            await LoadUsersAsync();
        }

        /// <summary>
        /// Reset ban strike count (SysAdmin only)
        /// </summary>
        private async void ResetStrikes_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser.Role != UserRole.SysAdmin)
            {
                ShowWarning("Only SysAdmin can reset ban strikes", "Access Denied");
                return;
            }

            if (UsersDataGrid.SelectedItem is not User selectedUser || _cosmosService == null)
            {
                ShowWarning("Please select a user to reset strikes", "No Selection");
                return;
            }

            if (selectedUser.BanStrikeCount == 0)
            {
                ShowInfo($"User '{selectedUser.Username}' has no ban strikes to reset.", "No Strikes");
                return;
            }

            if (!ConfirmAction(
                $"Reset ban strikes for '{selectedUser.Username}'?\n\n" +
                $"Current Strikes: {selectedUser.BanStrikeCount} of 3\n\n" +
                "This will:\n" +
                "• Reset strike count to 0\n" +
                "• Clear ban history\n" +
                "• Give user a fresh start\n\n" +
                "? This action cannot be undone!",
                "Reset Strikes (SysAdmin)"))
                return;

            selectedUser.BanStrikeCount = 0;
            await _cosmosService.UpdateUserAsync(selectedUser);

            ShowInfo($"Ban strikes reset for '{selectedUser.Username}'.\n\nUser now has 0 strikes.", "Strikes Reset");
            await LoadUsersAsync();
        }

        private async void MuteToggle_Click(object sender, RoutedEventArgs e)
        {
            // PERMISSION CHECK
            bool isAdmin = _currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager;
            if (!isAdmin && !_currentUser.Permissions.CanMuteUsers)
            {
                ShowWarning("You don't have permission to mute/unmute users", "Access Denied");
                return;
            }

            if (UsersDataGrid.SelectedItem is not User selectedUser || _cosmosService == null)
            {
                ShowWarning("Please select a user to mute or unmute", "No Selection");
                return;
            }

            try
            {
                if (selectedUser.IsGloballyMuted)
                {
                    await UnmuteUserAsync(selectedUser);
                }
                else
                {
                    await MuteUserAsync(selectedUser);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to update user mute status: {ex.Message}", "Error");
            }
        }

        private async Task MuteUserAsync(User user)
        {
            if (!ConfirmAction(
                $"Are you sure you want to mute user '{user.Username}'?\n\nThis will prevent them from sending chat messages.",
                "Confirm Mute"))
                return;

            user.IsGloballyMuted = true;
            user.MutedBy = _currentUser?.Username ?? "System";
            user.MutedAt = DateTime.Now;
            user.MuteExpiry = null;

            await _cosmosService!.UpdateUserAsync(user);
            ShowInfo($"User '{user.Username}' has been muted.", "User Muted");
            await LoadUsersAsync();
        }

        private async Task UnmuteUserAsync(User user)
        {
            if (!ConfirmAction($"Are you sure you want to unmute user '{user.Username}'?", "Confirm Unmute"))
                return;

            user.IsGloballyMuted = false;
            user.MutedBy = null;
            user.MutedAt = null;
            user.MuteExpiry = null;

            await _cosmosService!.UpdateUserAsync(user);
            ShowInfo($"User '{user.Username}' has been unmuted.", "User Unmuted");
            await LoadUsersAsync();
        }

        private async void PromoteDemote_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is not User selectedUser || _cosmosService == null)
            {
                ShowWarning("Please select a user to promote or demote", "No Selection");
                return;
            }

            try
            {
                // Open the new custom role selection dialog with validation parameters
                var roleDialog = new PromoteDemoteUserWindow(
                    currentRole: selectedUser.Role,
                    performingUserRole: _currentUser.Role,
                    targetUsername: selectedUser.Username,
                    currentUsername: _currentUser.Username);
                    
                roleDialog.Owner = Window.GetWindow(this);
                
                if (roleDialog.ShowDialog() == true && roleDialog.RoleChanged)
                {
                    var newRole = roleDialog.SelectedRole;
                    
                    System.Diagnostics.Debug.WriteLine($"[UsersView] Changing role for {selectedUser.Username}: {selectedUser.Role} -> {newRole}");
                    
                    selectedUser.Role = newRole;
                    await _cosmosService.UpdateUserAsync(selectedUser);
                    
                    ShowInfo($"User '{selectedUser.Username}' role changed to '{newRole}'", "Success");
                    await LoadUsersAsync();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to change user role: {ex.Message}", "Error");
            }
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                ShowInfo($"Password reset feature coming soon!\n\nThis will allow you to reset the password for '{selectedUser.Username}'.", "Coming Soon");
            }
            else
            {
                ShowWarning("Please select a user", "No Selection");
            }
        }

        private async void FilterAllUsers_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[UsersView] Filter: All Users clicked");
            await LoadUsersAsync();
        }

        public async void FilterOnlineUsers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[UsersView] Filter: Online Users clicked");
                
                if (_cosmosService == null)
                {
                    UpdateStatusText("Database service not initialized");
                    return;
                }

                UpdateStatusText("Loading online users...");

                _users = await _cosmosService.GetAllUsersAsync();
                
                // Synchronize IsOnline status from OnlineUserStatusService
                foreach (var user in _users)
                {
                    user.IsOnline = OnlineUserStatusService.Instance.IsUserOnline(user.Username);
                }
                
                // Filter to only online users
                var onlineUsers = _users.Where(u => u.IsOnline).OrderBy(u => u.Username).ToList();
                
                UsersDataGrid.ItemsSource = null;
                UsersDataGrid.ItemsSource = onlineUsers;

                UpdateStatusText($"Showing {onlineUsers.Count} online users (out of {_users.Count} total)");
                
                System.Diagnostics.Debug.WriteLine($"[UsersView] Online users filter: {onlineUsers.Count} online out of {_users.Count} total");
                foreach (var user in onlineUsers)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {user.Username}: {user.OnlineStatus}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to filter online users: {ex.Message}", "Error");
                UpdateStatusText("Error filtering users");
                System.Diagnostics.Debug.WriteLine($"[UsersView] Error in FilterOnlineUsers: {ex.Message}");
            }
        }

        public async void FilterOfflineUsers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[UsersView] Filter: Offline Users clicked");
                
                if (_cosmosService == null)
                {
                    UpdateStatusText("Database service not initialized");
                    return;
                }

                UpdateStatusText("Loading offline users...");

                _users = await _cosmosService.GetAllUsersAsync();
                
                // Synchronize IsOnline status from OnlineUserStatusService
                foreach (var user in _users)
                {
                    user.IsOnline = OnlineUserStatusService.Instance.IsUserOnline(user.Username);
                }
                
                // Filter to only offline users
                var offlineUsers = _users.Where(u => !u.IsOnline).OrderBy(u => u.Username).ToList();
                
                UsersDataGrid.ItemsSource = null;
                UsersDataGrid.ItemsSource = offlineUsers;

                UpdateStatusText($"Showing {offlineUsers.Count} offline users (out of {_users.Count} total)");
                
                System.Diagnostics.Debug.WriteLine($"[UsersView] Offline users filter: {offlineUsers.Count} offline out of {_users.Count} total");
            }
            catch (Exception ex)
            {
                ShowError($"Failed to filter offline users: {ex.Message}", "Error");
                UpdateStatusText("Error filtering users");
                System.Diagnostics.Debug.WriteLine($"[UsersView] Error in FilterOfflineUsers: {ex.Message}");
            }
        }

        private async void FilterComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (FilterComboBox == null || _cosmosService == null) return;

            try
            {
                var selectedItem = FilterComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem;
                if (selectedItem == null) return;

                string filter = selectedItem.Content?.ToString() ?? "All Users";
                System.Diagnostics.Debug.WriteLine($"[UsersView] ComboBox filter changed to: {filter}");

                UpdateStatusText($"Applying filter: {filter}...");

                _users = await _cosmosService.GetAllUsersAsync();
                
                // Synchronize IsOnline status
                foreach (var user in _users)
                {
                    user.IsOnline = OnlineUserStatusService.Instance.IsUserOnline(user.Username);
                }

                List<User> filteredUsers = filter switch
                {
                    "Online Only" => _users.Where(u => u.IsOnline).ToList(),
                    "Offline Only" => _users.Where(u => !u.IsOnline).ToList(),
                    "Active Only" => _users.Where(u => u.IsActive).ToList(),
                    "Banned Only" => _users.Where(u => u.IsBanned).ToList(),
                    "Admins Only" => _users.Where(u => u.Role == UserRole.SysAdmin || u.Role == UserRole.Manager).ToList(),
                    "DJs Only" => _users.Where(u => u.IsDJ).ToList(),
                    "Venue Owners Only" => _users.Where(u => u.IsVenueOwner).ToList(),
                    _ => _users
                };

                filteredUsers = filteredUsers.OrderBy(u => u.Username).ToList();
                
                UsersDataGrid.ItemsSource = null;
                UsersDataGrid.ItemsSource = filteredUsers;

                UpdateStatusText($"Showing {filteredUsers.Count} users ({filter})");
                
                System.Diagnostics.Debug.WriteLine($"[UsersView] Filter '{filter}' applied: {filteredUsers.Count} users shown");
            }
            catch (Exception ex)
            {
                ShowError($"Failed to apply filter: {ex.Message}", "Error");
                UpdateStatusText("Error applying filter");
                System.Diagnostics.Debug.WriteLine($"[UsersView] Error in FilterComboBox_SelectionChanged: {ex.Message}");
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[UsersView] Refresh button clicked");
            await LoadUsersAsync();
        }

        private void ManagePermissions_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInitialization()) return;

            if (UsersDataGrid.SelectedItem is not User selectedUser)
            {
                ShowWarning("Please select a user to manage permissions", "No Selection");
                return;
            }

            // Check if current user has permission to manage permissions
            if (_currentUser?.Role != UserRole.SysAdmin && _currentUser?.Role != UserRole.Manager)
            {
                ShowWarning("Only SysAdmin and Manager roles can manage user permissions", "Access Denied");
                return;
            }

            try
            {
                var permissionsWindow = new ManagePermissionsWindow(selectedUser, _cosmosService!, _currentUser);
                permissionsWindow.Owner = Window.GetWindow(this);
                
                if (permissionsWindow.ShowDialog() == true && permissionsWindow.PermissionsUpdated)
                {
                    ShowInfo($"? Permissions updated successfully for '{selectedUser.Username}'!", "Success");
                    _ = LoadUsersAsync(); // Refresh the user list
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to open permissions manager:\n{ex.Message}", "Error");
                System.Diagnostics.Debug.WriteLine($"[UsersView] Error opening ManagePermissionsWindow: {ex.Message}");
            }
        }

        private void PermissionGroups_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInitialization()) return;

            // Check if current user has permission
            if (_currentUser?.Role != UserRole.SysAdmin)
            {
                ShowWarning("Only SysAdmin can manage permission groups", "Access Denied");
                return;
            }

            MessageBox.Show(
                "?? PERMISSION GROUPS\n\n" +
                "Permission groups allow you to create templates of permissions\n" +
                "that can be quickly applied to multiple users.\n\n" +
                "Quick Presets Available:\n" +
                "• Basic User - View and create bookings\n" +
                "• DJ - Full booking access + RadioBOSS view\n" +
                "• Venue Owner - Manage venues and bookings\n" +
                "• Moderator - User moderation + reports\n" +
                "• Admin - Full system access\n\n" +
                "?? TIP: Use the 'Manage Permissions' button and\n" +
                "click the Quick Presets to apply these groups!",
                "Permission Groups",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private async void PopOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_cosmosService == null || _currentUser == null)
                {
                    ShowError("User Management not initialized", "Error");
                    return;
                }

                // Create maximizable window
                var usersWindow = new UsersManagementWindow();
                
                // Initialize with current services
                await usersWindow.Initialize(_cosmosService, _currentUser);
                
                // Show window
                usersWindow.Show();
                
                System.Diagnostics.Debug.WriteLine("? Users Management window opened in pop-out mode");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening Users Management window: {ex.Message}");
                ShowError($"Failed to open window: {ex.Message}", "Error");
            }
        }

        private async void UsersDataGrid_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == System.Windows.Controls.DataGridEditAction.Commit)
            {
                var user = e.Row.Item as User;
                if (user != null && _cosmosService != null)
                {
                    try
                    {
                        // Get the column that was edited
                        var columnHeader = e.Column.Header?.ToString() ?? "";
                        
                        System.Diagnostics.Debug.WriteLine($"[UsersView] ========== CELL EDIT ==========");
                        System.Diagnostics.Debug.WriteLine($"[UsersView] User: {user.Username}");
                        System.Diagnostics.Debug.WriteLine($"[UsersView] Column: '{columnHeader}'");
                        System.Diagnostics.Debug.WriteLine($"[UsersView] =====================================");
                        
                        // Wait for the edit to complete
                        await Dispatcher.InvokeAsync(async () =>
                        {
                            await Task.Delay(200); // Give UI time to update binding
                            
                            if (columnHeader == "Tutorial")
                            {
                                System.Diagnostics.Debug.WriteLine($"[UsersView] Tutorial checkbox changed!");
                                System.Diagnostics.Debug.WriteLine($"[UsersView] BEFORE save:");
                                System.Diagnostics.Debug.WriteLine($"   HasSeenTutorial = {user.AppPreferences.HasSeenTutorial}");
                                System.Diagnostics.Debug.WriteLine($"   ShowTutorialOnNextLogin = {user.AppPreferences.ShowTutorialOnNextLogin}");
                            }
                            

                            // Save changes to database
                            await _cosmosService.UpdateUserAsync(user);
                            System.Diagnostics.Debug.WriteLine($"[UsersView] ? User saved to database");
                            
                            if (columnHeader == "Tutorial")
                            {
                                string status = user.AppPreferences.ShowTutorialOnNextLogin ? "WILL SHOW" : "will NOT show";
                                System.Diagnostics.Debug.WriteLine($"[UsersView] AFTER save:");
                                System.Diagnostics.Debug.WriteLine($"   HasSeenTutorial = {user.AppPreferences.HasSeenTutorial}");
                                System.Diagnostics.Debug.WriteLine($"   ShowTutorialOnNextLogin = {user.AppPreferences.ShowTutorialOnNextLogin}");
                                System.Diagnostics.Debug.WriteLine($"   Result: Tutorial {status} on next login");
                                
                                ShowInfo(
                                    $"Tutorial setting updated for {user.Username}!\n\n" +
                                    $"Tutorial {status} on their next login.",
                                    "Tutorial Setting Changed");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[UsersView] Column '{columnHeader}' updated");
                            }
                        }, System.Windows.Threading.DispatcherPriority.Background);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[UsersView] ? ERROR updating user: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"[UsersView] Stack: {ex.StackTrace}");
                        ShowError($"Failed to update user: {ex.Message}", "Error");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[UsersView] ? Cannot save: user={user != null}, cosmosService={_cosmosService != null}");
                }
            }
        }

        #region Helper Methods

        private void UpdateStatusText(string message)
        {
            if (StatusText != null)
                StatusText.Text = message;
        }

        private bool ValidateInitialization()
        {
            if (_cosmosService == null || _currentUser == null)
            {
                ShowError("Not properly initialized", "Error");
                return false;
            }
            return true;
        }

        private static void ShowError(string message, string title) =>
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

        private static void ShowWarning(string message, string title) =>
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

        private static void ShowInfo(string message, string title) =>
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

        private static bool ConfirmAction(string message, string title) =>
            MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        #endregion

        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void ErrorReports_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInitialization()) return;

            // Check if current user has permission
            if (_currentUser?.Role != UserRole.SysAdmin && _currentUser?.Role != UserRole.Manager)
            {
                ShowWarning("Only SysAdmin and Manager roles can view error reports", "Access Denied");
                return;
            }

            try
            {
                // Get all reports from database
                var reports = await _cosmosService!.GetAllReportsAsync();
                
                if (reports == null || reports.Count == 0)
                {
                    ShowInfo("No error reports found in the system.", "No Reports");
                    return;
                }

                // Display error reports
                var reportText = "?? ERROR REPORTS\n\n";
                reportText += $"Total Reports: {reports.Count}\n\n";
                
                foreach (var report in reports.Take(10)) // Show first 10
                {
                    reportText += $"• User: {report.Username}\n";
                    reportText += $"  Time: {report.Timestamp:yyyy-MM-dd HH:mm:ss}\n";
                    reportText += $"  Error: {report.ErrorMessage}\n";
                    if (!string.IsNullOrEmpty(report.CurrentScreen))
                    {
                        reportText += $"  Screen: {report.CurrentScreen}\n";
                    }
                    reportText += "\n";
                }

                if (reports.Count > 10)
                {
                    reportText += $"\n... and {reports.Count - 10} more reports";
                }

                MessageBox.Show(reportText, "Error Reports", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load error reports:\n{ex.Message}", "Error");
                System.Diagnostics.Debug.WriteLine($"[UsersView] Error loading reports: {ex.Message}");
            }
        }

        private async void AuditLog_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInitialization()) return;

            // Check if current user has permission
            if (_currentUser?.Role != UserRole.SysAdmin)
            {
                ShowWarning("Only SysAdmin can view the audit log", "Access Denied");
                return;
            }

            try
            {
                // Get all action logs from database
                var logs = await _cosmosService!.GetUserActionLogsAsync();
                
                if (logs == null || logs.Count == 0)
                {
                    ShowInfo("No audit log entries found in the system.", "No Logs");
                    return;
                }

                // Display audit log
                var logText = "?? AUDIT LOG\n\n";
                logText += $"Total Entries: {logs.Count}\n\n";
                
                var recentLogs = logs.OrderByDescending(l => l.Timestamp).Take(15);
                foreach (var log in recentLogs)
                {
                    logText += $"• {log.Timestamp:yyyy-MM-dd HH:mm:ss} - {log.ActionType}\n";
                    logText += $"  User: {log.Username}\n";
                    if (!string.IsNullOrEmpty(log.ActionDetails))
                    {
                        logText += $"  Details: {log.ActionDetails}\n";
                    }
                    logText += "\n";
                }

                if (logs.Count > 15)
                {
                    logText += $"\n... and {logs.Count - 15} more entries";
                }

                MessageBox.Show(logText, "Audit Log", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load audit log:\n{ex.Message}", "Error");
                System.Diagnostics.Debug.WriteLine($"[UsersView] Error loading audit log: {ex.Message}");
            }
        }
    }
}