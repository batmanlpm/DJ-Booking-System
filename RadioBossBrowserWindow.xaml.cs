using System;
using System.Windows;
using DJBookingSystem.Models;

namespace DJBookingSystem
{
    /// <summary>
    /// RadioBOSS Browser Window
    /// Provides web browser access to RadioBOSS Cloud using standard WPF WebBrowser
    /// - All users can view the interface
    /// - Only SysAdmin and Manager users get full access indicated
    /// </summary>
    public partial class RadioBossBrowserWindow : Window
    {
        private const string Url = "https://c40.radioboss.fm/#home";
        private readonly User _currentUser;
        private bool _hasControlAccess = false;

        public RadioBossBrowserWindow(User currentUser, bool stayOnTop = false)
        {
            InitializeComponent();
            
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            
            // Determine if user has control access (SysAdmin or Manager)
            _hasControlAccess = _currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager;

            // Apply Stay on Top preference
            this.Topmost = stayOnTop;

            // Update window title based on access level
            this.Title = _hasControlAccess 
                ? "RadioBOSS Cloud Control - Full Access" 
                : "RadioBOSS Cloud Control - View Only";

            // Update footer text
            FooterTextBlock.Text = _hasControlAccess
                ? $"Full Access: {_currentUser.FullName} ({_currentUser.Role})"
                : $"View Only: {_currentUser.FullName} ({_currentUser.Role})";

            // Navigate to RadioBoss Cloud
            try
            {
                RadioBossWebView.Navigate(Url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading RadioBoss Cloud:\n{ex.Message}",
                    "Navigation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}
