using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DJBookingSystem
{
    /// <summary>
    /// Partial class for MainWindow - Menu Navigation Handlers
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Menu Navigation Handlers

        /// <summary>
        /// Hides all content panels
        /// </summary>
        private void HideAllPanels()
        {
            if (WelcomePanel != null) WelcomePanel.Visibility = Visibility.Collapsed;
            if (BookingsPanel != null) BookingsPanel.Visibility = Visibility.Collapsed;
            if (VenuesManagementPanel != null) VenuesManagementPanel.Visibility = Visibility.Collapsed;
            if (RadioPlayerPanel != null) RadioPlayerPanel.Visibility = Visibility.Collapsed;
            if (RadioUnifiedPanel != null) RadioUnifiedPanel.Visibility = Visibility.Collapsed;
            if (RadioBossCloudPanel != null) RadioBossCloudPanel.Visibility = Visibility.Collapsed;
            if (RadioBossStreamPanel != null) RadioBossStreamPanel.Visibility = Visibility.Collapsed;
            if (LiveRadioDatabasePanel != null) LiveRadioDatabasePanel.Visibility = Visibility.Collapsed;
            if (LPMLiveStationsPanel != null) LPMLiveStationsPanel.Visibility = Visibility.Collapsed;
            if (ChatPanel != null) ChatPanel.Visibility = Visibility.Collapsed;
            if (SettingsPanel != null) SettingsPanel.Visibility = Visibility.Collapsed;
            if (HelpPanel != null) HelpPanel.Visibility = Visibility.Collapsed;
            if (UsersPanel != null) UsersPanel.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Shows the specified panel and hides all others
        /// </summary>
        private void ShowPanel(Grid panel)
        {
            HideAllPanels();
            if (panel != null)
            {
                panel.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Menu Click Handlers

        private async void Menu_Bookings_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(BookingsPanel);
            
            // Validate required dependencies
            if (_cosmosDbService == null)
            {
                MessageBox.Show("Database service is not initialized. Please restart the application.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            if (_currentUser == null)
            {
                MessageBox.Show("User session is not valid. Please log in again.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Initialize BookingsView if ContentControl is empty
            if (BookingsViewContainer != null && BookingsViewContainer.Content == null)
            {
                try
                {
                    var bookingsView = new Views.BookingsView();
                    BookingsViewContainer.Content = bookingsView;
                    await bookingsView.Initialize(_cosmosDbService, _currentUser);
                    System.Diagnostics.Debug.WriteLine("✅ BookingsView initialized");
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Failed to initialize BookingsView: {ex.Message}");
                    MessageBox.Show($"Failed to load Bookings page: {ex.Message}", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (BookingsViewContainer?.Content is Views.BookingsView existingView)
            {
                // Reinitialize to refresh data
                try
                {
                    await existingView.Initialize(_cosmosDbService, _currentUser);
                    System.Diagnostics.Debug.WriteLine("✅ BookingsView refreshed");
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Failed to refresh BookingsView: {ex.Message}");
                    MessageBox.Show($"Failed to refresh Bookings page: {ex.Message}", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void Menu_Venues_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(VenuesManagementPanel);
            
            // Validate required dependencies
            if (_cosmosDbService == null)
            {
                MessageBox.Show("Database service is not initialized. Please restart the application.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine("❌ Cannot show VenuesManagementView: _cosmosDbService is null");
                return;
            }
            
            if (_currentUser == null)
            {
                MessageBox.Show("User session is not valid. Please log in again.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine("❌ Cannot show VenuesManagementView: _currentUser is null");
                return;
            }
            
            // Initialize VenuesManagementView if ContentControl is empty
            if (VenuesManagementViewContainer != null && VenuesManagementViewContainer.Content == null)
            {
                try
                {
                    var venuesView = new Views.VenuesManagementView();
                    VenuesManagementViewContainer.Content = venuesView;
                    await venuesView.Initialize(_cosmosDbService, _currentUser);
                    System.Diagnostics.Debug.WriteLine("✅ VenuesManagementView initialized");
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Failed to initialize VenuesManagementView: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                    MessageBox.Show($"Failed to load Venues page: {ex.Message}\n\nPlease check the debug output for details.", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (VenuesManagementViewContainer?.Content is Views.VenuesManagementView existingView)
            {
                // Reinitialize to refresh data
                try
                {
                    await existingView.Initialize(_cosmosDbService, _currentUser);
                    System.Diagnostics.Debug.WriteLine("✅ VenuesManagementView refreshed");
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Failed to refresh VenuesManagementView: {ex.Message}");
                    MessageBox.Show($"Failed to refresh Venues page: {ex.Message}", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Menu_Radio_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(RadioUnifiedPanel);
            
            // Initialize RadioUnifiedView with current user for permission checking
            if (RadioUnifiedPanel?.Children.Count > 0 && RadioUnifiedPanel.Children[0] is Views.Radio.RadioUnifiedView radioView)
            {
                if (_currentUser != null)
                {
                    radioView.Initialize(_currentUser);
                    System.Diagnostics.Debug.WriteLine($"[MainWindow] RadioUnifiedView initialized for user: {_currentUser.Username}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[MainWindow] ⚠ RadioUnifiedView opened but _currentUser is null");
                }
            }
        }

        private void Menu_Chat_Click(object sender, RoutedEventArgs e)
        {
            // Show chat mode selection dialog
            var chatModeDialog = new ChatModeSelectionDialog();
            chatModeDialog.Owner = this;

            if (chatModeDialog.ShowDialog() == true)
            {
                if (chatModeDialog.SelectedMode == ChatModeSelectionDialog.ChatMode.Discord)
                {
                    ShowPanel(ChatPanel);

                    // Initialize ChatView with current user for Discord WebView2 session
                    if (ChatPanel?.Children.Count > 0 && ChatPanel.Children[0] is Views.ChatView chatView)
                    {
                        if (_currentUser != null)
                        {
                            chatView.Initialize(_currentUser);
                            System.Diagnostics.Debug.WriteLine($"[MainWindow] ChatView initialized for user: {_currentUser.Username}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("[MainWindow] ⚠ ChatView opened but _currentUser is null");
                        }
                    }
                }
                else if (chatModeDialog.SelectedMode == ChatModeSelectionDialog.ChatMode.Integrated)
                {
                    // Open Integrated Chat window
                    var integratedChat = new IntegratedChatWindow(_currentUser, _cosmosDbService);
                    integratedChat.Owner = this;
                    integratedChat.Show();
                    System.Diagnostics.Debug.WriteLine($"[MainWindow] Integrated Chat opened for user: {_currentUser.Username}");
                }
            }
        }

        private void Menu_Theme_Click(object sender, RoutedEventArgs e)
        {
            // ShowPanel(ThemePanel);
            MessageBox.Show("Theme panel coming soon!", "Theme");
        }

        private void Menu_Admin_Click(object sender, RoutedEventArgs e)
        {
            // ShowPanel(AdminPanel);
            MessageBox.Show("Admin panel coming soon!", "Admin");
        }
        
        private void Menu_LiveRadioDatabase_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(LiveRadioDatabasePanel);
        }
        
        private void Menu_LPMLiveStations_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(LPMLiveStationsPanel);
        }

        /// <summary>
        /// Navigate to Live Radio Stations (legacy)
        /// </summary>
        private void Menu_LiveRadioStations_Click(object sender, RoutedEventArgs e)
        {
            // ShowPanel(LiveRadioStationsPanel);
            ShowPanel(LiveRadioDatabasePanel); // Redirect to new panel
        }

        /// <summary>
        /// Navigate to My Station Database (legacy)
        /// </summary>
        private void Menu_MyStationDatabase_Click(object sender, RoutedEventArgs e)
        {
            // ShowPanel(MyStationDatabasePanel);
            ShowPanel(LPMLiveStationsPanel); // Redirect to new panel
        }

        private void AddNewStation_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Add New Station feature coming soon!\n\nYou'll be able to add custom radio stations that will be shared with all users.",
                "Coming Soon",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Menu_Settings_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(SettingsPanel);
            
            // Initialize SettingsView with current user and database service
            if (SettingsPanel?.Children.Count > 0 && SettingsPanel.Children[0] is Views.SettingsView settingsView)
            {
                if (_currentUser != null && _cosmosDbService != null)
                {
                    settingsView.Initialize(_currentUser, _cosmosDbService);
                    System.Diagnostics.Debug.WriteLine($"[MainWindow] SettingsView initialized for user: {_currentUser.Username} with database service");
                }
                else if (_currentUser != null)
                {
                    // Fallback to old method if database service is not available
                    settingsView.Initialize(_currentUser);
                    System.Diagnostics.Debug.WriteLine($"[MainWindow] SettingsView initialized for user: {_currentUser.Username} (no database service)");
                }
            }
        }

        private async void Menu_Users_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[MainWindow] Users menu clicked");
            ShowPanel(UsersPanel);
            
            // Validate required dependencies
            if (_cosmosDbService == null)
            {
                MessageBox.Show("Database service is not initialized. Please restart the application.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine("[MainWindow] Cannot show UsersView: _cosmosDbService is null");
                return;
            }
            
            if (_currentUser == null)
            {
                MessageBox.Show("User session is not valid. Please log in again.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine("[MainWindow] Cannot show UsersView: _currentUser is null");
                return;
            }
            
            // Initialize UsersView if ContentControl is empty OR reinitialize to refresh online status
            if (UsersViewContainer != null)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[MainWindow] Initializing/Refreshing UsersView");
                    
                    // Always create a fresh instance to ensure online status is current
                    var usersView = new Views.UsersView();
                    UsersViewContainer.Content = usersView;
                    await usersView.Initialize(_cosmosDbService, _currentUser);
                    
                    System.Diagnostics.Debug.WriteLine("[MainWindow] UsersView initialized successfully");
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MainWindow] Failed to initialize UsersView: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[MainWindow] Stack Trace: {ex.StackTrace}");
                    MessageBox.Show($"Failed to load Users page: {ex.Message}\n\nPlease check the debug output for details.", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Menu_Help_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(HelpPanel);
        }

        private void Menu_LPM1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://c40.radioboss.fm/stream/98",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening LPM Station 1: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void Menu_LPM2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://c19.radioboss.fm/stream/162",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening LPM Station 2: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Menu_AddRadioStation_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "?? ADD NEW RADIO STATION ??\n\n" +
                "Coming Soon!\n\n" +
                "You'll be able to:\n" +
                "? Add custom radio stations by URL\n" +
                "? Share stations with all users\n" +
                "? Add station name, genre, and details\n" +
                "? Play stations in the built-in radio player\n\n" +
                "Stay tuned!",
                "Add Radio Station",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Menu_FavoriteStations_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "?? MY FAVORITE STATIONS ??\n\n" +
                "Coming Soon!\n\n" +
                "You'll be able to:\n" +
                "? Mark stations as favorites\n" +
                "? Quick access to your favorite stations\n" +
                "? Organize your personal station list\n" +
                "? One-click play from favorites\n\n" +
                "Stay tuned!",
                "Favorite Stations",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // ==================== RADIOBOSS MENU HANDLERS ====================

        /// <summary>
        /// Open RadioBoss Cloud Control
        /// </summary>
        private void Menu_RadioBossCloud_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(RadioBossCloudPanel);
            
            // Initialize with current user for permission checking
            if (RadioBossCloudPanel?.Children.Count > 0 && 
                RadioBossCloudPanel.Children[0] is Views.Radio.RadioBossCloudView cloudView)
            {
                cloudView.Initialize(_currentUser);
            }
        }

        /// <summary>
        /// Open RadioBoss Stream Control
        /// </summary>
        private void Menu_RadioBossStream_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(RadioBossStreamPanel);
            
            // Initialize with current user for permission checking
            if (RadioBossStreamPanel?.Children.Count > 0 && 
                RadioBossStreamPanel.Children[0] is Views.Radio.RadioBossStreamView streamView)
            {
                streamView.Initialize(_currentUser);
            }
        }

        // ==================== VENUE MENU HANDLERS ====================

        /// <summary>
        /// Open Venue Creation Window
        /// </summary>
        private void Menu_CreateVenue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if user is a venue owner or admin
                if (!_currentUser.IsVenueOwner && _currentUser.Role != Models.UserRole.Manager && _currentUser.Role != Models.UserRole.SysAdmin)
                {
                    MessageBox.Show(
                        "Only Venue Owners can create venues.\n\n" +
                        "Please contact an admin to set your account as a Venue Owner.",
                        "Venue Owner Required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var createVenueWindow = new Views.CreateVenueWindow(_cosmosDbService!, _currentUser);
                createVenueWindow.Owner = this;
                
                if (createVenueWindow.ShowDialog() == true && createVenueWindow.CreatedVenue != null)
                {
                    MessageBox.Show(
                        $"Venue '{createVenueWindow.CreatedVenue.Name}' created successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    
                    // Refresh venues list
                    LoadVenuesData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening venue creation:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Open Admin Venue Management (Admin only)
        /// </summary>
        private void Menu_ManageVenues_Click(object sender, RoutedEventArgs e)
        {
            // Check if user is admin - using correct UserRole enum values
            if (_currentUser.Role != Models.UserRole.Manager && _currentUser.Role != Models.UserRole.SysAdmin)
            {
                MessageBox.Show(
                    "?? Admin Access Required\n\n" +
                    "You must be a Manager or SysAdmin to manage venues.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                var adminVenueWindow = new AdminVenueManagementWindow(_cosmosDbService!, _currentUser);
                adminVenueWindow.Owner = this;
                adminVenueWindow.ShowDialog();
                
                // Refresh venues list
                LoadVenuesData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening admin venue management:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ==================== SETTINGS MENU HANDLERS ====================

        /// <summary>
        /// Open User Settings Window
        /// </summary>
        private void Menu_MySettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsWindow = new UserSettingsWindow(_currentUser, _cosmosDbService!);
                settingsWindow.Owner = this;
                settingsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening settings:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Open Account Settings Window
        /// </summary>
        private void Menu_AccountSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // AccountSettingsWindow has a parameterless constructor
                var accountWindow = new AccountSettingsWindow();
                accountWindow.Owner = this;
                accountWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening account settings:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Open Theme Selection
        /// </summary>
        private void Menu_Themes_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "?? THEME SELECTION\n\n" +
                "Use My Settings to change themes!\n\n" +
                "Available themes:\n" +
                "� Green (Matrix style)\n" +
                "� Cyan, Blue, Purple\n" +
                "� Red, Orange, Yellow\n" +
                "� Pink, Crimson, Teal\n" +
                "� White\n" +
                "� Rainbow (Animated!)\n\n" +
                "Open Settings > My Settings to change!",
                "Themes",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Open Notification Settings
        /// </summary>
        private void Menu_Notifications_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "🔔 NOTIFICATION SETTINGS\n\n" +
                "Use My Settings to configure notifications!\n\n" +
                "You can enable/disable:\n" +
                "📬 Booking notifications\n" +
                "💬 Chat alerts\n" +
                "📢 System messages\n" +
                "🔊 Sound effects\n\n" +
                "Open Settings > My Settings to configure!",
                "Notifications",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Open Tutorial Video
        /// </summary>
        private void Menu_Tutorial_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Build path to tutorial videos folder
                string tutorialFolder = Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                    "..", "..", "..", "CandyBot_Training_Guides"
                );

                // Normalize path
                tutorialFolder = Path.GetFullPath(tutorialFolder);

                List<string> tutorialVideos = new List<string>();

                // Add user tutorial (everyone gets this)
                string userTutorialPath = Path.Combine(tutorialFolder, "Tutorial_Users.mp4");
                if (File.Exists(userTutorialPath))
                {
                    tutorialVideos.Add(userTutorialPath);
                }

                // Add admin tutorial if user is admin
                if (_currentUser.Role == Models.UserRole.Manager || _currentUser.Role == Models.UserRole.SysAdmin)
                {
                    string adminTutorialPath = Path.Combine(tutorialFolder, "Tutorial_Admin.mp4");
                    if (File.Exists(adminTutorialPath))
                    {
                        tutorialVideos.Add(adminTutorialPath);
                    }
                }

                if (tutorialVideos.Count == 0)
                {
                    MessageBox.Show(
                        "⚠️ Tutorial videos not found!\n\n" +
                        $"Expected location:\n{tutorialFolder}\n\n" +
                        "Please make sure the tutorial videos are in the CandyBot_Training_Guides folder.",
                        "Tutorial Not Available",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Show tutorial in optional mode (can be skipped)
                var tutorialWindow = new VideoTutorialWindow(
                    tutorialVideos,
                    isMandatory: false,  // Optional mode - can be skipped
                    onComplete: null
                );
                
                tutorialWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening tutorial:\n{ex.Message}\n\n{ex.StackTrace}",
                    "Tutorial Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ==================== USER MANAGEMENT MENU HANDLERS ====================

        // Navigation handlers (All Users, Online Users, Offline Users) are kept above
        // All other user management actions are handled directly in UsersView control panel

        /// <summary>
        /// Diagnostic tool to check and fix user role issues (defined in MainWindow.RoleFix.cs)
        /// </summary>
        private void Menu_DiagnoseRole_Click(object sender, RoutedEventArgs e)
        {
            // Method implementation is in MainWindow.RoleFix.cs partial class
            DiagnoseAndFixUserRole();
        }

        #endregion

        #region Data Loading Methods

        private async void LoadBookingsData()
        {
            try
            {
                if (_cosmosDbService == null) return;

                var bookings = await _cosmosDbService.GetAllBookingsAsync();
                if (bookings != null)
                {
                    _allBookings = bookings.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading bookings: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void LoadVenuesData()
        {
            try
            {
                if (_cosmosDbService == null) return;

                var venues = await _cosmosDbService.GetAllVenuesAsync();
                if (venues != null)
                {
                    _allVenues = venues.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading venues: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion

        #region UPDATER MENU HANDLERS (ADMIN ONLY)

        /// <summary>
        /// Open Update Manager window
        /// </summary>
        private void Menu_UpdateManager_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check admin permissions
                if (CurrentUser == null || !IsAdmin(CurrentUser))
                {
                    MessageBox.Show(
                        "Access Denied\n\n" +
                        "The Update Manager is only accessible to administrators.\n\n" +
                        "Required Role: SysAdmin or Manager",
                        "Admin Access Required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var updaterWindow = new UpdaterAdminWindow();
                updaterWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening Update Manager: {ex.Message}");
                MessageBox.Show($"Error opening Update Manager: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Quick deploy new update
        /// </summary>
        private void Menu_DeployUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check admin permissions
                if (CurrentUser == null || !IsAdmin(CurrentUser))
                {
                    MessageBox.Show(
                        "Access Denied",
                        "Admin Access Required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var updaterWindow = new UpdaterAdminWindow();
                updaterWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Test SSL connection
        /// </summary>
        private async void Menu_TestSSL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check admin permissions
                if (CurrentUser == null || !IsAdmin(CurrentUser))
                {
                    MessageBox.Show("Access Denied", "Admin Access Required", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show(
                    "Testing SSL connection to update server...",
                    "SSL Test",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                var result = await Services.CertificateManager.TestSslConnectionAsync("https://c40.radioboss.fm/u/98");

                if (result.Success)
                {
                    MessageBox.Show(
                        $"SSL Connection Test Successful!\n\n" +
                        $"{result.GetSummary()}",
                        "SSL Test Result",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        $"SSL Connection Test Failed\n\n" +
                        $"{result.GetSummary()}",
                        "SSL Test Result",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error testing SSL: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// View deployment history
        /// </summary>
        private void Menu_DeploymentHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentUser == null || !IsAdmin(CurrentUser))
                {
                    MessageBox.Show("Access Denied", "Admin Access Required", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show(
                    "Deployment History\n\n" +
                    "This feature will show:\n" +
                    "• All previous deployments\n" +
                    "• Deployment dates and versions\n" +
                    "• Who deployed each update\n" +
                    "• Success/failure status\n\n" +
                    "Coming in next update!",
                    "Deployment History",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// View user update statistics
        /// </summary>
        private void Menu_UpdateStats_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentUser == null || !IsAdmin(CurrentUser))
                {
                    MessageBox.Show("Access Denied", "Admin Access Required", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show(
                    "User Update Statistics\n\n" +
                    "This feature will show:\n" +
                    "• How many users are on each version\n" +
                    "• Update adoption rate\n" +
                    "• Users who need to update\n" +
                    "• Version distribution graph",
                    "Coming Soon",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Menu_UpdateStats_Click: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Menu_Documentation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var docWindow = new DocumentationWindow();
                docWindow.Owner = this;
                docWindow.Show();
                System.Diagnostics.Debug.WriteLine("[MainWindow] Documentation Center opened");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening Documentation: {ex.Message}");
                MessageBox.Show($"Error opening Documentation Center:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
