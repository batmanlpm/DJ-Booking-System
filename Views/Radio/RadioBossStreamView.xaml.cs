using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;
using DJBookingSystem.Models;

namespace DJBookingSystem.Views.Radio
{
    /// <summary>
    /// RadioBoss Stream Control Page - Web Integration
    /// Uses WebView2 (Edge Chromium engine) for modern JavaScript support
    /// </summary>
    public partial class RadioBossStreamView : UserControl
    {
        private bool _isFullScreen = false;
        private Window? _parentWindow;
        private WindowStyle _originalWindowStyle;
        private WindowState _originalWindowState;
        private bool _originalTopmost;
        private bool _isInitialized = false;
        private User? _currentUser;
        private bool _isReadOnly = false;

        public RadioBossStreamView()
        {
            InitializeComponent();
            this.Loaded += RadioBossStreamView_Loaded;
            this.KeyDown += RadioBossStreamView_KeyDown;
        }

        /// <summary>
        /// Initialize with current user to determine permissions
        /// </summary>
        public void Initialize(User currentUser)
        {
            _currentUser = currentUser;
            
            // PERMISSION ENFORCEMENT
            bool isAdmin = currentUser.Role == UserRole.Manager || currentUser.Role == UserRole.SysAdmin;
            bool canView = isAdmin || currentUser.Permissions.CanViewRadioBoss;
            bool canControl = isAdmin || currentUser.Permissions.CanControlRadioBoss;
            
            // If user can't even view, show access denied message
            if (!canView)
            {
                if (RadioBossStreamBrowser != null)
                {
                    RadioBossStreamBrowser.Visibility = Visibility.Collapsed;
                }
                if (ReadOnlyOverlay != null)
                {
                    ReadOnlyOverlay.Visibility = Visibility.Visible;
                }
                System.Diagnostics.Debug.WriteLine($"[RadioBossStreamView] ACCESS DENIED - User {currentUser.Username} cannot view RadioBOSS");
                return;
            }
            
            // User can view but maybe not control
            _isReadOnly = !canControl;
            
            if (_isReadOnly)
            {
                // Disable all control buttons for view-only users
                DisableControlButtons();
                
                // Keep WebView2 enabled for viewing
                if (RadioBossStreamBrowser != null)
                {
                    RadioBossStreamBrowser.IsEnabled = true;
                }
                
                if (ReadOnlyOverlay != null)
                {
                    ReadOnlyOverlay.Visibility = Visibility.Visible;
                }
                
                System.Diagnostics.Debug.WriteLine($"[RadioBossStreamView] VIEW-ONLY mode for user: {currentUser.Username}");
                System.Diagnostics.Debug.WriteLine($"  - CanViewRadioBoss: {currentUser.Permissions.CanViewRadioBoss}");
                System.Diagnostics.Debug.WriteLine($"  - CanControlRadioBoss: {currentUser.Permissions.CanControlRadioBoss}");
            }
            else
            {
                // Full control access
                if (RadioBossStreamBrowser != null)
                {
                    RadioBossStreamBrowser.IsEnabled = true;
                }
                
                if (ReadOnlyOverlay != null)
                {
                    ReadOnlyOverlay.Visibility = Visibility.Collapsed;
                }
                
                System.Diagnostics.Debug.WriteLine($"[RadioBossStreamView] FULL CONTROL granted for user: {currentUser.Username}");
            }
        }

        /// <summary>
        /// Disable control buttons for read-only mode
        /// </summary>
        private void DisableControlButtons()
        {
            if (GoBackButton != null) GoBackButton.IsEnabled = false;
            if (GoForwardButton != null) GoForwardButton.IsEnabled = false;
            if (RefreshButton != null) RefreshButton.IsEnabled = false;
            if (ZoomOutButton != null) ZoomOutButton.IsEnabled = false;
            if (ZoomInButton != null) ZoomInButton.IsEnabled = false;
            if (ZoomResetButton != null) ZoomResetButton.IsEnabled = false;
            if (FullScreenButton != null) FullScreenButton.IsEnabled = false;
        }

        private void RadioBossStreamView_KeyDown(object sender, KeyEventArgs e)
        {
            // Press ESC to exit full screen
            if (e.Key == Key.Escape && _isFullScreen)
            {
                ExitFullScreen();
            }
        }

        private async void RadioBossStreamView_Loaded(object sender, RoutedEventArgs e)
        {
            _parentWindow = Window.GetWindow(this);
            
            if (_parentWindow != null)
            {
                // Store original window state
                _originalWindowStyle = _parentWindow.WindowStyle;
                _originalWindowState = _parentWindow.WindowState;
                _originalTopmost = _parentWindow.Topmost;
                
                // Add escape key handler to parent window
                _parentWindow.KeyDown += ParentWindow_KeyDown;
                _parentWindow.PreviewKeyDown += ParentWindow_PreviewKeyDown;
            }
            
            // Make this control focusable so it can receive keyboard input
            this.Focusable = true;
            this.Focus();
            
            // Initialize WebView2
            try
            {
                if (RadioBossStreamBrowser == null)
                {
                    System.Diagnostics.Debug.WriteLine("WebView2 control is null");
                    return;
                }

                await RadioBossStreamBrowser.EnsureCoreWebView2Async(null);
                _isInitialized = true;
                
                // Configure WebView2 settings
                RadioBossStreamBrowser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = !_isReadOnly;
                RadioBossStreamBrowser.CoreWebView2.Settings.IsZoomControlEnabled = !_isReadOnly;
                
                // Disable interaction for non-admin users
                if (_isReadOnly)
                {
                    RadioBossStreamBrowser.CoreWebView2.Settings.IsScriptEnabled = true; // Keep enabled to see content
                    RadioBossStreamBrowser.CoreWebView2.Settings.AreDevToolsEnabled = false;
                    RadioBossStreamBrowser.IsEnabled = false; // Disable the entire WebView2 control
                    
                    System.Diagnostics.Debug.WriteLine("[RadioBossStreamView] WebView2 configured for read-only mode - control disabled");
                }
                
                // Handle navigation completed to auto-login and hide scrollbars
                RadioBossStreamBrowser.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
                
                // Navigate to RadioBoss Stream Control with correct URL - changed to #main
                RadioBossStreamBrowser.CoreWebView2.Navigate("https://c40.radioboss.fm/#main");
            }
            catch (Exception ex)
            {
                // Only show error if it's a real problem, not just initialization timing
                System.Diagnostics.Debug.WriteLine($"WebView2 initialization: {ex.Message}");
                
                // Try to show a less intrusive notification
                if (!_isInitialized)
                {
                    System.Diagnostics.Debug.WriteLine($"WebView2 failed to initialize: {ex}");
                }
            }
        }

        private async void CoreWebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess) return;

            try
            {
                // Disable all interaction for read-only mode
                if (_isReadOnly)
                {
                    string disableInteractionScript = @"
                        (function() {
                            var style = document.createElement('style');
                            style.textContent = '* { pointer-events: none !important; user-select: none !important; }';
                            document.head.appendChild(style);
                            document.body.style.pointerEvents = 'none';
                            document.documentElement.style.pointerEvents = 'none';
                        })();
                    ";
                    await RadioBossStreamBrowser.CoreWebView2.ExecuteScriptAsync(disableInteractionScript);
                    System.Diagnostics.Debug.WriteLine("[RadioBossStreamView] Pointer events disabled via CSS");
                }
                
                // Hide scrollbars via CSS injection
                string hideScrollbarsScript = @"
                    (function() {
                        var style = document.createElement('style');
                        style.textContent = 'body::-webkit-scrollbar { display: none !important; } body { -ms-overflow-style: none !important; scrollbar-width: none !important; overflow: auto !important; } html { overflow: hidden !important; }';
                        document.head.appendChild(style);
                        document.body.style.overflow = 'hidden';
                        document.documentElement.style.overflow = 'hidden';
                    })();
                ";
                await RadioBossStreamBrowser.CoreWebView2.ExecuteScriptAsync(hideScrollbarsScript);

                // Set zoom to 30% for maximum content visibility
                RadioBossStreamBrowser.ZoomFactor = 0.3;

                // Auto-login script with improved button detection and clicking (only for admins)
                if (!_isReadOnly)
                {
                    string autoLoginScript = @"
                        (function() {
                            var loginAttempted = false;
                            
                            function findLoginButton() {
                                var allButtons = document.querySelectorAll('button, input[type=""submit""]');
                                for (var i = 0; i < allButtons.length; i++) {
                                    var btn = allButtons[i];
                                    var text = btn.textContent || btn.value || '';
                                    if (text.toLowerCase().includes('login')) {
                                        return btn;
                                    }
                                }
                                return document.querySelector('button[type=""submit""]') || document.querySelector('button');
                            }
                            
                            function attemptLogin() {
                                if (loginAttempted) return false;
                                
                                var usernameInput = document.querySelector('input[type=""text""]');
                                var passwordInput = document.querySelector('input[type=""password""]');
                                
                                if (usernameInput && passwordInput) {
                                    usernameInput.value = 'Remote';
                                    passwordInput.value = 'R3m0t3';
                                    
                                    usernameInput.dispatchEvent(new Event('input', { bubbles: true }));
                                    passwordInput.dispatchEvent(new Event('input', { bubbles: true }));
                                    
                                    loginAttempted = true;
                                    
                                    setTimeout(function() {
                                        var loginButton = findLoginButton();
                                        if (loginButton) {
                                            loginButton.click();
                                        }
                                    }, 800);
                                    
                                    return true;
                                }
                                return false;
                            }
                            
                            if (!attemptLogin()) {
                                setTimeout(attemptLogin, 1500);
                                setTimeout(attemptLogin, 3000);
                            }
                        })();
                    ";
                    
                    // Wait a bit before injecting the login script to ensure page is ready
                    await System.Threading.Tasks.Task.Delay(800);
                    await RadioBossStreamBrowser.CoreWebView2.ExecuteScriptAsync(autoLoginScript);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Script injection error: {ex.Message}");
            }
        }

        private void ParentWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && _isFullScreen)
            {
                ExitFullScreen();
            }
        }

        private void ParentWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && _isFullScreen)
            {
                ExitFullScreen();
                e.Handled = true;
            }
        }

        private void ToggleFullScreen_Click(object sender, RoutedEventArgs e)
        {
            if (_isFullScreen)
            {
                ExitFullScreen();
            }
            else
            {
                EnterFullScreen();
            }
        }

        private void EnterFullScreen()
        {
            if (_parentWindow == null) return;

            _isFullScreen = true;

            // Store original state
            _originalWindowStyle = _parentWindow.WindowStyle;
            _originalWindowState = _parentWindow.WindowState;
            _originalTopmost = _parentWindow.Topmost;

            // Enter full screen
            _parentWindow.WindowStyle = WindowStyle.None;
            _parentWindow.WindowState = WindowState.Maximized;
            _parentWindow.Topmost = true;
            
            FullScreenButton.Content = "EXIT FULL SCREEN (ESC)";
            
            // Ensure this control has focus
            this.Focus();
        }

        private void ExitFullScreen()
        {
            if (_parentWindow == null) return;

            _isFullScreen = false;

            // Restore original window state
            _parentWindow.WindowStyle = _originalWindowStyle;
            _parentWindow.WindowState = _originalWindowState;
            _parentWindow.Topmost = _originalTopmost;
            
            FullScreenButton.Content = "FULL SCREEN";
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isInitialized && RadioBossStreamBrowser?.CoreWebView2?.CanGoBack == true)
                {
                    RadioBossStreamBrowser.CoreWebView2.GoBack();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GoBack error: {ex.Message}");
            }
        }

        private void GoForward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isInitialized && RadioBossStreamBrowser?.CoreWebView2?.CanGoForward == true)
                {
                    RadioBossStreamBrowser.CoreWebView2.GoForward();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GoForward error: {ex.Message}");
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isInitialized)
                {
                    RadioBossStreamBrowser?.CoreWebView2?.Reload();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Refresh error: {ex.Message}");
            }
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isInitialized && RadioBossStreamBrowser != null)
                {
                    // Increase zoom by 10%
                    double newZoom = RadioBossStreamBrowser.ZoomFactor + 0.1;
                    if (newZoom <= 2.0) // Max 200%
                    {
                        RadioBossStreamBrowser.ZoomFactor = newZoom;
                        System.Diagnostics.Debug.WriteLine($"Zoom In: {newZoom * 100}%");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Zoom In error: {ex.Message}");
            }
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isInitialized && RadioBossStreamBrowser != null)
                {
                    // Decrease zoom by 10%
                    double newZoom = RadioBossStreamBrowser.ZoomFactor - 0.1;
                    if (newZoom >= 0.2) // Min 20%
                    {
                        RadioBossStreamBrowser.ZoomFactor = newZoom;
                        System.Diagnostics.Debug.WriteLine($"Zoom Out: {newZoom * 100}%");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Zoom Out error: {ex.Message}");
            }
        }

        private void ZoomReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isInitialized && RadioBossStreamBrowser != null)
                {
                    // Reset to default 30%
                    RadioBossStreamBrowser.ZoomFactor = 0.3;
                    System.Diagnostics.Debug.WriteLine("Zoom Reset: 30%");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Zoom Reset error: {ex.Message}");
            }
        }
    }
}
