using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem.Views
{
    public partial class OnlineUsersWindow : Window
    {
        private readonly OnlineUserStatusService _statusService;
        private readonly CosmosDbService _cosmosService;
        private ObservableCollection<OnlineUserInfo> _allUsers;
        private ObservableCollection<OnlineUserInfo> _filteredUsers;
        private DispatcherTimer? _refreshTimer;
        private bool _isLoading = false;

        public OnlineUsersWindow(CosmosDbService cosmosService)
        {
            InitializeComponent();
            
            _cosmosService = cosmosService ?? throw new ArgumentNullException(nameof(cosmosService));
            _statusService = OnlineUserStatusService.Instance;
            _allUsers = new ObservableCollection<OnlineUserInfo>();
            _filteredUsers = new ObservableCollection<OnlineUserInfo>();

            OnlineUsersDataGrid.ItemsSource = _filteredUsers;

            // Subscribe to status changes
            _statusService.UserStatusChanged += OnUserStatusChanged;

            // Initial load
            LoadOnlineUsers();

            // Start auto-refresh
            StartAutoRefresh();
        }

        private void LoadOnlineUsers()
        {
            if (_isLoading) return;
            
            _isLoading = true;

            try
            {
                Dispatcher.Invoke(() =>
                {
                    StatusTextBlock.Text = "? Loading online users...";
                });

                var onlineUsers = _statusService.GetOnlineUsers();
                
                if (onlineUsers == null || onlineUsers.Count == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        _allUsers.Clear();
                        _filteredUsers.Clear();
                        UpdateStatistics();
                        StatusTextBlock.Text = "?? No users currently online";
                        LastUpdateTextBlock.Text = $"Last Updated: {DateTime.Now:HH:mm:ss}";
                    });
                    return;
                }

                var userInfoList = new List<OnlineUserInfo>();

                foreach (var user in onlineUsers)
                {
                    try
                    {
                        var info = _statusService.GetUserInfo(user.Username);
                        if (info != null)
                        {
                            userInfoList.Add(info);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error getting info for user {user.Username}: {ex.Message}");
                    }
                }

                // Update UI on main thread
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        _allUsers.Clear();
                        foreach (var user in userInfoList.OrderBy(u => u.Username))
                        {
                            _allUsers.Add(user);
                        }

                        ApplyFilters();
                        UpdateStatistics();

                        StatusTextBlock.Text = $"? Showing {_filteredUsers.Count} of {_allUsers.Count} online users";
                        LastUpdateTextBlock.Text = $"Last Updated: {DateTime.Now:HH:mm:ss}";
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating UI: {ex.Message}");
                        StatusTextBlock.Text = $"?? Error updating display: {ex.Message}";
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading online users: {ex.Message}");
                Dispatcher.Invoke(() =>
                {
                    StatusTextBlock.Text = $"? Error: {ex.Message}";
                });
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ApplyFilters()
        {
            try
            {
                _filteredUsers.Clear();

                if (_allUsers == null || _allUsers.Count == 0)
                {
                    return;
                }

                var searchText = SearchTextBox?.Text?.ToLower() ?? string.Empty;
                var selectedRole = (RoleFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Roles";

                var filtered = _allUsers.Where(user =>
                {
                    if (user == null) return false;

                    // Search filter
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        var username = user.Username?.ToLower() ?? string.Empty;
                        var fullName = user.FullName?.ToLower() ?? string.Empty;
                        
                        if (!username.Contains(searchText) && !fullName.Contains(searchText))
                        {
                            return false;
                        }
                    }

                    // Role filter
                    if (selectedRole != "All Roles" && !string.IsNullOrEmpty(selectedRole))
                    {
                        var userRole = user.Role ?? string.Empty;
                        
                        // Handle special case for "Venue Owner"
                        if (selectedRole == "Venue Owner" && user.IsVenueOwner)
                        {
                            return true;
                        }
                        
                        // Handle special case for "DJ"
                        if (selectedRole == "DJ" && user.IsDJ)
                        {
                            return true;
                        }
                        
                        if (userRole != selectedRole)
                        {
                            return false;
                        }
                    }

                    return true;
                });

                foreach (var user in filtered)
                {
                    _filteredUsers.Add(user);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying filters: {ex.Message}");
                MessageBox.Show($"Error applying filters: {ex.Message}", "Filter Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateStatistics()
        {
            try
            {
                if (_allUsers == null)
                {
                    TotalOnlineTextBlock.Text = "0";
                    AdminsOnlineTextBlock.Text = "0";
                    DJsOnlineTextBlock.Text = "0";
                    return;
                }

                var totalOnline = _allUsers.Count;
                var adminsOnline = _allUsers.Count(u => 
                    u?.Role == "SysAdmin" || u?.Role == "Manager");
                var djsOnline = _allUsers.Count(u => 
                    u?.IsDJ == true || u?.Role == "DJ");

                TotalOnlineTextBlock.Text = totalOnline.ToString();
                AdminsOnlineTextBlock.Text = adminsOnline.ToString();
                DJsOnlineTextBlock.Text = djsOnline.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating statistics: {ex.Message}");
            }
        }

        private void OnUserStatusChanged(object? sender, UserStatusEventArgs e)
        {
            if (e == null) return;

            try
            {
                // Refresh list when user status changes
                Dispatcher.InvokeAsync(() =>
                {
                    LoadOnlineUsers();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnUserStatusChanged: {ex.Message}");
            }
        }

        private void StartAutoRefresh()
        {
            try
            {
                StopAutoRefresh(); // Stop any existing timer

                if (AutoRefreshCheckBox?.IsChecked == true)
                {
                    _refreshTimer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(5)
                    };
                    _refreshTimer.Tick += (s, e) => LoadOnlineUsers();
                    _refreshTimer.Start();
                    
                    System.Diagnostics.Debug.WriteLine("Auto-refresh started (5 seconds)");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting auto-refresh: {ex.Message}");
            }
        }

        private void StopAutoRefresh()
        {
            try
            {
                if (_refreshTimer != null)
                {
                    _refreshTimer.Stop();
                    _refreshTimer.Tick -= (s, e) => LoadOnlineUsers();
                    _refreshTimer = null;
                    
                    System.Diagnostics.Debug.WriteLine("Auto-refresh stopped");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping auto-refresh: {ex.Message}");
            }
        }

        // Event Handlers

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount == 2)
                {
                    WindowState = WindowState == WindowState.Maximized 
                        ? WindowState.Normal 
                        : WindowState.Maximized;
                }
                else if (e.LeftButton == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TitleBar_MouseLeftButtonDown: {ex.Message}");
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error closing window: {ex.Message}");
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadOnlineUsers();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Refresh_Click: {ex.Message}");
                MessageBox.Show($"Error refreshing: {ex.Message}", "Refresh Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (_allUsers == null || _allUsers.Count == 0)
                {
                    StatusTextBlock.Text = "?? No users to search";
                    return;
                }

                ApplyFilters();
                
                var searchText = SearchTextBox?.Text ?? string.Empty;
                if (!string.IsNullOrEmpty(searchText))
                {
                    StatusTextBlock.Text = $"?? Showing {_filteredUsers.Count} of {_allUsers.Count} users matching '{searchText}'";
                }
                else
                {
                    StatusTextBlock.Text = $"? Showing {_filteredUsers.Count} of {_allUsers.Count} online users";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SearchTextBox_TextChanged: {ex.Message}");
            }
        }

        private void RoleFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_allUsers == null || _allUsers.Count == 0)
                {
                    return;
                }

                ApplyFilters();
                
                var selectedRole = (RoleFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Roles";
                if (selectedRole != "All Roles")
                {
                    StatusTextBlock.Text = $"?? Showing {_filteredUsers.Count} {selectedRole} users of {_allUsers.Count} total";
                }
                else
                {
                    StatusTextBlock.Text = $"? Showing {_filteredUsers.Count} of {_allUsers.Count} online users";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RoleFilter_SelectionChanged: {ex.Message}");
            }
        }

        private void AutoRefresh_Changed(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AutoRefreshCheckBox?.IsChecked == true)
                {
                    StartAutoRefresh();
                    StatusTextBlock.Text = "?? Auto-refresh enabled (5 seconds)";
                }
                else
                {
                    StopAutoRefresh();
                    StatusTextBlock.Text = "?? Auto-refresh disabled";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AutoRefresh_Changed: {ex.Message}");
            }
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is not Button button)
                {
                    MessageBox.Show("Invalid button reference", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var username = button.Tag?.ToString();

                if (string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("Username not found", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var userInfo = _statusService.GetUserInfo(username);
                
                if (userInfo == null)
                {
                    MessageBox.Show($"Could not find information for user: {username}", "User Not Found", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var details = $"USER DETAILS\n\n" +
                             $"???????????????????????????????????????\n" +
                             $"ACCOUNT INFORMATION\n" +
                             $"???????????????????????????????????????\n\n" +
                             $"?? Username: {userInfo.Username}\n" +
                             $"?? Full Name: {userInfo.FullName}\n" +
                             $"?? Role: {userInfo.Role}\n" +
                             $"?? Email: {userInfo.Email}\n\n" +
                             $"???????????????????????????????????????\n" +
                             $"SESSION INFORMATION\n" +
                             $"???????????????????????????????????????\n\n" +
                             $"?? Login Time: {userInfo.LoginTime:yyyy-MM-dd HH:mm:ss}\n" +
                             $"?? Online Duration: {userInfo.OnlineDuration}\n" +
                             $"?? Last Activity: {userInfo.LastActivity}\n" +
                             $"? Session Duration: {(DateTime.Now - userInfo.LoginTime).TotalMinutes:F1} minutes\n\n" +
                             $"???????????????????????????????????????\n" +
                             $"LOCATION & NETWORK\n" +
                             $"???????????????????????????????????????\n\n" +
                             $"?? Location: {userInfo.Location}\n" +
                             $"?? IP Address: {userInfo.IPAddress}\n\n" +
                             $"???????????????????????????????????????\n" +
                             $"ACCOUNT FLAGS\n" +
                             $"???????????????????????????????????????\n\n" +
                             $"?? Is DJ: {(userInfo.IsDJ ? "Yes ?" : "No ?")}\n" +
                             $"?? Is Venue Owner: {(userInfo.IsVenueOwner ? "Yes ?" : "No ?")}\n\n" +
                             $"???????????????????????????????????????";

                MessageBox.Show(details, $"User Details - {userInfo.Username}", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ViewDetails_Click: {ex.Message}");
                MessageBox.Show($"Error viewing details: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is not Button button)
                {
                    MessageBox.Show("Invalid button reference", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var username = button.Tag?.ToString();

                if (string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("Username not found", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if user is still online
                if (!_statusService.IsUserOnline(username))
                {
                    MessageBox.Show($"User {username} is no longer online", "User Offline", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadOnlineUsers(); // Refresh list
                    return;
                }

                // TODO: Implement messaging system
                var result = MessageBox.Show(
                    $"Send message to {username}?\n\n" +
                    $"The messaging system is currently under development.\n\n" +
                    $"Would you like to be notified when this feature is ready?",
                    "Send Message",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show(
                        "Thank you! You will be notified when the messaging feature is available.",
                        "Notification Registered",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SendMessage_Click: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Messaging Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                // Cleanup
                StopAutoRefresh();
                
                if (_statusService != null)
                {
                    _statusService.UserStatusChanged -= OnUserStatusChanged;
                }

                System.Diagnostics.Debug.WriteLine("OnlineUsersWindow closed and cleaned up");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnClosed: {ex.Message}");
            }
            finally
            {
                base.OnClosed(e);
            }
        }
    }
}
