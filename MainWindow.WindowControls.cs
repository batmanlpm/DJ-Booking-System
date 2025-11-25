using System;
using System.Windows;
using DJBookingSystem.Services;
using DJBookingSystem.Models;

namespace DJBookingSystem
{
    /// <summary>
    /// MainWindow - Window Control Handlers (Close, Minimize, Maximize, Force Close)
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initialize window control handlers
        /// </summary>
        private void InitializeWindowControls()
        {
            // Initialize force shutdown manager
            ForceShutdownManager.InitializeForWindow(this);
            
            System.Diagnostics.Debug.WriteLine("? Window controls initialized (Alt+F4, X button, Force Close, Minimize, Maximize)");
        }

        /// <summary>
        /// Handle title bar drag and double-click maximize
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount == 2)
                {
                    // Double-click to maximize/restore
                    Maximize_Click(sender, e);
                }
                else
                {
                    // Single click to drag
                    DragMove();
                }
            }
            catch
            {
                // Ignore errors during drag
            }
        }

        /// <summary>
        /// Handle minimize button click
        /// </summary>
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            System.Diagnostics.Debug.WriteLine("Window minimized");
        }

        /// <summary>
        /// Handle minimize to system tray
        /// </summary>
        private void MinimizeToTray_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            System.Diagnostics.Debug.WriteLine("Window minimized to tray");
        }

        /// <summary>
        /// Handle Stay On Top toggle
        /// </summary>
        private void StayOnTop_Click(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
            
            if (StayOnTopButton != null)
            {
                StayOnTopButton.Content = Topmost ? "??" : "??";
                StayOnTopButton.ToolTip = Topmost ? "Window is pinned on top (click to unpin)" : "Toggle Stay On Top";
            }
            
            System.Diagnostics.Debug.WriteLine($"Stay On Top: {Topmost}");
            
            MessageBox.Show(
                Topmost ? "Window will stay on top of other windows" : "Window will behave normally",
                "Stay On Top",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Handle maximize/restore button click
        /// </summary>
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                System.Diagnostics.Debug.WriteLine("Window restored to normal");
            }
            else
            {
                WindowState = WindowState.Maximized;
                System.Diagnostics.Debug.WriteLine("Window maximized");
            }
        }

        /// <summary>
        /// Handle logout button click
        /// </summary>
        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes && _cosmosDbService != null)
            {
                // Mark current user as offline before logout
                if (_currentUser != null)
                {
                    await OnlineUserStatusService.Instance.SetUserOfflineAsync(_currentUser.Username);
                }

                // Close current window
                this.Close();

                // Show login window as dialog
                var loginWindow = new LoginWindow(_cosmosDbService);
                var loginResult = loginWindow.ShowDialog();

                // If login successful, create new MainWindow
                if (loginResult == true && loginWindow.LoggedInUser != null)
                {
                    var appSettings = new AppSettings(); // Or load from database
                    var mainWindow = new MainWindow(_cosmosDbService, loginWindow.LoggedInUser, appSettings);
                    mainWindow.Show();
                }
                else
                {
                    // User cancelled login or login failed - close app
                    Application.Current.Shutdown();
                }
            }
        }

        /// <summary>
        /// Handle close button click
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to close the application?",
                "Close",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Mark current user as offline before shutdown
                if (_currentUser != null)
                {
                    try
                    {
                        OnlineUserStatusService.Instance.SetUserOffline(_currentUser.Username);
                        System.Diagnostics.Debug.WriteLine($"[Close] User {_currentUser.Username} marked as offline");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Close] Failed to mark user offline: {ex.Message}");
                    }
                }

                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Handle force close (emergency shutdown)
        /// </summary>
        private void ForceClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? Force close button clicked");
                
                // Show confirmation dialog
                ForceShutdownManager.ShowForceCloseConfirmation(this);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error during force close: {ex.Message}");
                
                // If showing dialog fails, just force close immediately
                ForceShutdownManager.EmergencyExit();
            }
        }

        /// <summary>
        /// Handle window closing event (triggered by Alt+F4, X button, etc.)
        /// This is now handled by ForceShutdownManager
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Mark user as offline when window is closing (handles Alt+F4, crashes, etc.)
            if (_currentUser != null)
            {
                try
                {
                    // Use synchronous version since OnClosing can't be async
                    OnlineUserStatusService.Instance.SetUserOffline(_currentUser.Username);
                    System.Diagnostics.Debug.WriteLine($"[OnClosing] User {_currentUser.Username} marked as offline");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[OnClosing] Failed to mark user offline: {ex.Message}");
                }
            }

            // Let ForceShutdownManager handle the rest
            base.OnClosing(e);
        }
    }
}
