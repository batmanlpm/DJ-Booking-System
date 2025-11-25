using System.Windows;

namespace DJBookingSystem
{
    /// <summary>
    /// MainWindow partial class - Public Navigation Methods
    /// Used by CandyBot and other components to navigate between views
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Navigate to Bookings view (public accessor)
        /// </summary>
        public void ShowBookingsView()
        {
            Dispatcher.Invoke(() =>
            {
                ShowPanel(BookingsPanel);
                LoadBookingsData();
            });
        }

        /// <summary>
        /// Navigate to Venues view (public accessor)
        /// </summary>
        public async void ShowVenuesView()
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                ShowPanel(VenuesManagementPanel);
                
                // Validate required dependencies
                if (_cosmosDbService == null)
                {
                    MessageBox.Show("Database service is not initialized. Please restart the application.", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    System.Diagnostics.Debug.WriteLine("? Cannot show VenuesManagementView: _cosmosDbService is null");
                    return;
                }
                
                if (_currentUser == null)
                {
                    MessageBox.Show("User session is not valid. Please log in again.", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    System.Diagnostics.Debug.WriteLine("? Cannot show VenuesManagementView: _currentUser is null");
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
                        System.Diagnostics.Debug.WriteLine("? VenuesManagementView initialized from ShowVenuesView");
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"? Failed to initialize VenuesManagementView: {ex.Message}");
                        MessageBox.Show($"Failed to load Venues page: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else if (VenuesManagementViewContainer?.Content is Views.VenuesManagementView existingView)
                {
                    try
                    {
                        await existingView.Initialize(_cosmosDbService, _currentUser);
                        System.Diagnostics.Debug.WriteLine("? VenuesManagementView refreshed from ShowVenuesView");
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"? Failed to refresh VenuesManagementView: {ex.Message}");
                    }
                }
                
                LoadVenuesData();
            });
        }

        /// <summary>
        /// Navigate to Help view (public accessor)
        /// </summary>
        public void ShowHelpView()
        {
            Dispatcher.Invoke(() =>
            {
                ShowPanel(HelpPanel);
            });
        }

        /// <summary>
        /// Navigate to Chat view (public accessor)
        /// </summary>
        public void ShowChatView()
        {
            Dispatcher.Invoke(() =>
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
                        System.Diagnostics.Debug.WriteLine("[MainWindow] ? ChatView opened but _currentUser is null");
                    }
                }
            });
        }

        /// <summary>
        /// Navigate to Settings view (public accessor)
        /// </summary>
        public void ShowSettingsView()
        {
            Dispatcher.Invoke(() =>
            {
                ShowPanel(SettingsPanel);
            });
        }

        /// <summary>
        /// Navigate to Users view (public accessor)
        /// </summary>
        public void ShowUsersView()
        {
            Dispatcher.Invoke(() =>
            {
                ShowPanel(UsersPanel);
            });
        }

        /// <summary>
        /// Navigate to Radio Player view (public accessor)
        /// </summary>
        public void ShowRadioView()
        {
            Dispatcher.Invoke(() =>
            {
                ShowPanel(RadioPlayerPanel);
            });
        }
    }
}
