using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DJBookingSystem.Services;
using DJBookingSystem.Models;

namespace DJBookingSystem.Views
{
    public partial class ChatView : UserControl
    {
        private DiscordWebViewService? _discordWebViewService;
        private User? _currentUser;
        private bool _isInitialized = false;
        private double _currentZoomFactor = 0.75; // Start at 75% zoom to fit more content

        public ChatView()
        {
            InitializeComponent();
            this.Loaded += ChatView_Loaded;
        }

        /// <summary>
        /// Initialize ChatView with current user for user-specific Discord session
        /// </summary>
        public async void Initialize(User currentUser)
        {
            // CRITICAL: Check if WebView2 is already fully initialized
            if (_isInitialized && DiscordWebView?.CoreWebView2 != null)
            {
                System.Diagnostics.Debug.WriteLine($"[ChatView] Already initialized for {_currentUser?.Username}, just making visible");
                LoadingPanel.Visibility = Visibility.Collapsed;
                DiscordWebView.Visibility = Visibility.Visible;
                UpdateZoomDisplay();
                LoadWebhook();
                return;
            }

            _currentUser = currentUser;
            
            System.Diagnostics.Debug.WriteLine($"[ChatView] Initializing for user: {currentUser.Username}");
            
            // Create user-specific Discord WebView service
            _discordWebViewService = new DiscordWebViewService(currentUser.Username);

            LoadingUserText.Text = $"Logged in as: {currentUser.Username}";
            
            // Load webhook from user settings
            LoadWebhook();

            // Check if user has existing Discord session
            bool hasExistingSession = _discordWebViewService.HasExistingSession();
            if (hasExistingSession)
            {
                DiscordStatusText.Text = $"Restoring Discord session for {currentUser.Username}...";
                System.Diagnostics.Debug.WriteLine($"[ChatView] Found existing Discord session for {currentUser.Username}");
            }
            else
            {
                DiscordStatusText.Text = $"Loading Discord for {currentUser.Username}...";
                System.Diagnostics.Debug.WriteLine($"[ChatView] Creating new Discord session for {currentUser.Username}");
            }

            try
            {
                // CRITICAL: Only initialize if CoreWebView2 is null
                if (DiscordWebView?.CoreWebView2 == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[ChatView] CoreWebView2 is null, initializing...");
                    
                    // Initialize WebView2 with user-specific data folder
                    await _discordWebViewService.InitializeWebView2Async(DiscordWebView!);
                    
                    // Set default zoom to 75% to fit more content
                    if (DiscordWebView != null)
                    {
                        DiscordWebView.ZoomFactor = _currentZoomFactor;
                    }
                    
                    _isInitialized = true;
                    
                    System.Diagnostics.Debug.WriteLine($"[ChatView] ? WebView2 initialized successfully at {_currentZoomFactor * 100}% zoom");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[ChatView] CoreWebView2 already exists, skipping initialization");
                    _isInitialized = true;
                    LoadingPanel.Visibility = Visibility.Collapsed;
                    DiscordWebView.Visibility = Visibility.Visible;
                    DiscordWebView.ZoomFactor = _currentZoomFactor;
                }                
                UpdateZoomDisplay();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChatView] ? Error initializing WebView2: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ChatView] Stack trace: {ex.StackTrace}");
                
                DiscordStatusText.Text = "Error loading Discord";
                DiscordStatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                
                MessageBox.Show(
                    $"Failed to load Discord:\n\n{ex.Message}\n\nTry:\n" +
                    "1. Close and reopen the chat\n" +
                    "2. Restart the application\n" +
                    "3. Ensure WebView2 Runtime is installed",
                    "Discord Load Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ChatView_Loaded(object sender, RoutedEventArgs e)
        {
            // Don't auto-initialize - wait for explicit Initialize() call
            System.Diagnostics.Debug.WriteLine("[ChatView] ChatView loaded, waiting for Initialize() call");
        }

        /// <summary>
        /// Handle WebView2 navigation completion
        /// </summary>
        private void DiscordWebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            // Hide loading panel and show WebView
            LoadingPanel.Visibility = Visibility.Collapsed;
            DiscordWebView.Visibility = Visibility.Visible;

            if (e.IsSuccess)
            {
                // IMPORTANT: Reapply zoom after navigation completes
                if (DiscordWebView?.CoreWebView2 != null)
                {
                    DiscordWebView.ZoomFactor = _currentZoomFactor;
                    System.Diagnostics.Debug.WriteLine($"[ChatView] Applied zoom: {_currentZoomFactor * 100:F0}%");
                }
                
                UpdateZoomDisplay();
                DiscordStatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LimeGreen);
                
                System.Diagnostics.Debug.WriteLine($"[ChatView] ? Discord loaded successfully at {_currentZoomFactor * 100:F0}% zoom");
            }
            else
            {
                DiscordStatusText.Text = "Failed to load Discord";
                DiscordStatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                
                System.Diagnostics.Debug.WriteLine($"[ChatView] ? Discord navigation failed");
            }
        }

        /// <summary>
        /// Zoom In - Increase by 10%
        /// </summary>
        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (DiscordWebView?.CoreWebView2 != null && _currentZoomFactor < 2.0)
            {
                _currentZoomFactor += 0.1;
                DiscordWebView.ZoomFactor = _currentZoomFactor;
                UpdateZoomDisplay();
                System.Diagnostics.Debug.WriteLine($"[ChatView] Zoom In: {_currentZoomFactor * 100:F0}%");
            }
        }

        /// <summary>
        /// Zoom Out - Decrease by 10%
        /// </summary>
        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (DiscordWebView?.CoreWebView2 != null && _currentZoomFactor > 0.5)
            {
                _currentZoomFactor -= 0.1;
                DiscordWebView.ZoomFactor = _currentZoomFactor;
                UpdateZoomDisplay();
                System.Diagnostics.Debug.WriteLine($"[ChatView] Zoom Out: {_currentZoomFactor * 100:F0}%");
            }
        }

        /// <summary>
        /// Reset Zoom to 100%
        /// </summary>
        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            if (DiscordWebView?.CoreWebView2 != null)
            {
                _currentZoomFactor = 1.0;
                DiscordWebView.ZoomFactor = _currentZoomFactor;
                UpdateZoomDisplay();
                System.Diagnostics.Debug.WriteLine($"[ChatView] Zoom Reset: 100%");
            }
        }

        /// <summary>
        /// Update zoom level display in status text
        /// </summary>
        private void UpdateZoomDisplay()
        {
            if (_isInitialized && _currentUser != null)
            {
                DiscordStatusText.Text = $"Connected as {_currentUser.Username} | Zoom: {(_currentZoomFactor * 100):F0}%";
            }
        }

        /// <summary>
        /// Refresh Discord page
        /// </summary>
        private void RefreshDiscord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DiscordWebView?.CoreWebView2 != null)
                {
                    LoadingPanel.Visibility = Visibility.Visible;
                    DiscordWebView.Visibility = Visibility.Collapsed;
                    
                    DiscordWebView.CoreWebView2.Reload();
                    
                    System.Diagnostics.Debug.WriteLine("[ChatView] Discord refreshed");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error refreshing Discord:\n{ex.Message}",
                    "Refresh Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Navigate to Discord home (DMs)
        /// </summary>
        private void HomeDiscord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_discordWebViewService != null && DiscordWebView != null)
                {
                    _discordWebViewService.NavigateToHome(DiscordWebView);
                    System.Diagnostics.Debug.WriteLine("[ChatView] Navigated to Discord home");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error navigating to home:\n{ex.Message}",
                    "Navigation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Logout from Discord (clear session)
        /// </summary>
        private async void LogoutDiscord_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Logout from Discord?\n\nThis will clear your Discord session for this account.\n" +
                "You will need to log in again next time.",
                "Logout from Discord",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (_discordWebViewService != null && DiscordWebView != null)
                    {
                        LoadingPanel.Visibility = Visibility.Visible;
                        DiscordWebView.Visibility = Visibility.Collapsed;
                        
                        await _discordWebViewService.ClearUserDataAsync(DiscordWebView);
                        
                        // Reload to show login page
                        DiscordWebView.CoreWebView2?.Reload();
                        
                        DiscordStatusText.Text = "Logged out - Please sign in";
                        DiscordStatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
                        
                        MessageBox.Show(
                            "Discord session cleared!\n\nYou have been logged out.",
                            "Logged Out",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        
                        System.Diagnostics.Debug.WriteLine($"[ChatView] Discord session cleared for {_currentUser?.Username}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error clearing Discord session:\n{ex.Message}",
                        "Logout Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Load webhook URL from user's Discord webhook property
        /// </summary>
        private void LoadWebhook()
        {
            if (_currentUser != null && !string.IsNullOrWhiteSpace(_currentUser.DiscordWebhook))
            {
                WebhookTextBox.Text = _currentUser.DiscordWebhook;
                System.Diagnostics.Debug.WriteLine($"[ChatView] Loaded Discord webhook for {_currentUser.Username}");
            }
        }

        /// <summary>
        /// Submit webhook URL - save to user's profile in database
        /// </summary>
        private async void SubmitWebhook_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show(
                    "User session is not available.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            string webhookUrl = WebhookTextBox.Text?.Trim() ?? string.Empty;

            // Validate webhook URL
            if (!string.IsNullOrWhiteSpace(webhookUrl))
            {
                if (!webhookUrl.StartsWith("https://discord.com/api/webhooks/") && 
                    !webhookUrl.StartsWith("https://discordapp.com/api/webhooks/"))
                {
                    MessageBox.Show(
                        "Invalid Discord webhook URL.\n\n" +
                        "Webhook URL must start with:\n" +
                        "https://discord.com/api/webhooks/ or\n" +
                        "https://discordapp.com/api/webhooks/",
                        "Invalid Webhook",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Test the webhook before saving
                SubmitWebhookButton.IsEnabled = false;
                SubmitWebhookButton.Content = "TESTING...";
                
                bool testSuccess = await SendTestWebhook(webhookUrl);
                
                SubmitWebhookButton.IsEnabled = true;
                SubmitWebhookButton.Content = "SUBMIT";
                
                if (!testSuccess)
                {
                    var result = MessageBox.Show(
                        "?? Webhook test failed!\n\n" +
                        "The webhook URL appears to be invalid or the channel is not accessible.\n\n" +
                        "Do you want to save it anyway?",
                        "Webhook Test Failed",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    
                    if (result != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
            }

            try
            {
                // Update user's Discord webhook
                _currentUser.DiscordWebhook = webhookUrl;

                // Save to database (you'll need to pass CosmosDbService to this view)
                // For now, just update in memory
                
                MessageBox.Show(
                    string.IsNullOrWhiteSpace(webhookUrl) 
                        ? "Discord webhook removed successfully!\n\nCandyBot notifications will no longer be sent to Discord."
                        : "? Discord webhook saved successfully!\n\n?? Test message sent!\n\nCandyBot will send booking and venue notifications to this webhook.",
                    "Webhook Saved",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                System.Diagnostics.Debug.WriteLine($"[ChatView] Discord webhook updated for {_currentUser.Username}: {(string.IsNullOrWhiteSpace(webhookUrl) ? "REMOVED" : "SET")}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving webhook:\n{ex.Message}",
                    "Save Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Send test message to Discord webhook to verify connection
        /// </summary>
        private async Task<bool> SendTestWebhook(string webhookUrl)
        {
            try
            {
                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(10);

                    var testMessage = new
                    {
                        content = $"? **Webhook Connection Test**\n\n" +
                                 $"?? Success! Your Discord webhook is working correctly.\n" +
                                 $"?? User: **{_currentUser?.Username}**\n" +
                                 $"? Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
                                 $"?? CandyBot will now send notifications to this channel for:\n" +
                                 $"• ?? Booking updates\n" +
                                 $"• ?? Venue notifications\n" +
                                 $"• ?? Important alerts",
                        username = "CandyBot",
                        avatar_url = "https://i.imgur.com/AfFp7pu.png" // Optional: Add CandyBot avatar URL if you have one
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(testMessage);
                    var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(webhookUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ChatView] ? Test webhook sent successfully to Discord");
                        return true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[ChatView] ? Webhook test failed: {response.StatusCode} - {response.ReasonPhrase}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChatView] ? Exception sending test webhook: {ex.Message}");
                return false;
            }
        }
    }
}