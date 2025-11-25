using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using DJBookingSystem.Models;

namespace DJBookingSystem
{
    /// <summary>
    /// RadioBOSS Cloud Control Panel Window
    /// Uses standard WPF WebBrowser control (IE11 engine)
    /// - All users can view the control panel
    /// - Only SysAdmin and Manager users can operate controls
    /// </summary>
    public partial class RadioBossControlWindow : Window
    {
        #region Fields

        private readonly User _currentUser;
        private bool _isAdminMode = false;
        private const string RadioBossUrl = "https://c40.radioboss.fm/#home";

        #endregion

        #region Constructor

        public RadioBossControlWindow(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            
            // Configure access mode based on user role
            ConfigureAccessMode();
            
            // Navigate to RadioBoss Cloud - Simple and reliable!
            NavigateToRadioBoss();
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Navigate to RadioBoss Cloud using standard WebBrowser
        /// </summary>
        private void NavigateToRadioBoss()
        {
            try
            {
                RadioBossWebView.Navigate(RadioBossUrl);
                CurrentUrlText.Text = RadioBossUrl;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load RadioBoss Cloud: {ex.Message}\n\n" +
                    "Please check your internet connection.",
                    "Navigation Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        #endregion

        #region Access Control

        /// <summary>
        /// Configures access mode based on user role
        /// SysAdmin and Manager users get full control
        /// Other users get view-only access
        /// </summary>
        private void ConfigureAccessMode()
        {
            // Determine if user has admin privileges
            _isAdminMode = _currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager;
            
            if (_isAdminMode)
            {
                // ADMIN MODE - Full control enabled
                AccessModeText.Text = "ADMIN CONTROL MODE";
                AccessModeText.Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#00FF00"));
                
                // Remove access control overlay
                AccessControlOverlay.Visibility = Visibility.Collapsed;
                AccessControlOverlay.IsHitTestVisible = false;
            }
            else
            {
                // VIEW-ONLY MODE - Controls blocked
                AccessModeText.Text = "VIEW-ONLY MODE";
                AccessModeText.Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FFD700"));
                
                // Enable access control overlay to block interactions
                AccessControlOverlay.Visibility = Visibility.Visible;
                AccessControlOverlay.IsHitTestVisible = true;
                
                MessageBox.Show(
                    "You are in VIEW-ONLY mode.\n\n" +
                    "You can see the RadioBOSS control panel, but you cannot interact with controls.\n\n" +
                    "Only SysAdmin and Manager users can operate the controls.",
                    "View-Only Access", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
            
            // Update user role display
            UserRoleText.Text = $"Logged in as: {_currentUser.FullName} ({_currentUser.Role})";
        }

        #endregion

        #region Button Event Handlers

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioBossWebView.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Refresh error: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToRadioBoss();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region WebBrowser Events

        private void WebView_Navigated(object sender, NavigationEventArgs e)
        {
            // Hide loading panel when navigation completes
            LoadingPanel.Visibility = Visibility.Collapsed;
            
            // Update current URL display
            if (e.Uri != null)
            {
                CurrentUrlText.Text = e.Uri.ToString();
            }
        }

        #endregion
    }
}
