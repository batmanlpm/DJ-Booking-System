using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DJBookingSystem.Views
{
    /// <summary>
    /// Live Radio Database - Web browser for radio station databases
    /// Uses standard WPF WebBrowser control (IE11 engine)
    /// Default: RadioBoss Cloud Control for LPM Stations
    /// </summary>
    public partial class LiveRadioDatabaseView : UserControl
    {
        private bool _isFullScreen = false;
        private Window? _parentWindow;
        private WindowStyle _originalWindowStyle;
        private WindowState _originalWindowState;
        private bool _originalTopmost;
        
        // Default URLs for quick access
        private string _homeUrl = "https://radio.garden";
        private string _lpmStation1Url = "https://c40.radioboss.fm/#main";  // LPM Station 1
        private string _lpmStation2Url = "https://c19.radioboss.fm/#main";  // LPM Station 2
        private string _radioGardenUrl = "https://radio.garden";  // Radio Garden

        public LiveRadioDatabaseView()
        {
            InitializeComponent();
            this.Loaded += LiveRadioDatabaseView_Loaded;
            this.KeyDown += LiveRadioDatabaseView_KeyDown;
            
            // Handle Enter key in URL textbox
            UrlTextBox.KeyDown += UrlTextBox_KeyDown;
            
            // Set default URL in textbox
            UrlTextBox.Text = _homeUrl;
        }

        private void UrlTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Go_Click(sender, e);
            }
        }

        private void LiveRadioDatabaseView_KeyDown(object sender, KeyEventArgs e)
        {
            // Press ESC to exit full screen
            if (e.Key == Key.Escape && _isFullScreen)
            {
                ExitFullScreen();
            }
        }

        private void LiveRadioDatabaseView_Loaded(object sender, RoutedEventArgs e)
        {
            _parentWindow = Window.GetWindow(this);
            
            if (_parentWindow != null)
            {
                // Store original window state
                _originalWindowStyle = _parentWindow.WindowStyle;
                _originalWindowState = _parentWindow.WindowState;
                _originalTopmost = _parentWindow.Topmost;
                
                // Add escape key handlers
                _parentWindow.KeyDown += ParentWindow_KeyDown;
                _parentWindow.PreviewKeyDown += ParentWindow_PreviewKeyDown;
            }
            
            // Make this control focusable
            this.Focusable = true;
            this.Focus();
            
            // Navigate to home URL - Simple and reliable!
            try
            {
                LiveRadioWebBrowser.Navigate(_homeUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading page: {ex.Message}\n\nPlease check your internet connection.",
                    "Navigation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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

        #region Navigation Methods

        private void NavigateTo(string url)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(url))
                {
                    // Ensure URL has protocol
                    if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                    {
                        url = "https://" + url;
                    }
                    
                    LiveRadioWebBrowser.Navigate(url);
                    UrlTextBox.Text = url;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Navigation error: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Button Event Handlers

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LiveRadioWebBrowser?.CanGoBack == true)
                {
                    LiveRadioWebBrowser.GoBack();
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
                if (LiveRadioWebBrowser?.CanGoForward == true)
                {
                    LiveRadioWebBrowser.GoForward();
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
                LiveRadioWebBrowser?.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Refresh error: {ex.Message}");
            }
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(_homeUrl);
        }

        private void Go_Click(object sender, RoutedEventArgs e)
        {
            string url = UrlTextBox.Text.Trim();
            NavigateTo(url);
        }

        private void LPM1_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(_lpmStation1Url);
        }

        private void LPM2_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(_lpmStation2Url);
        }

        private void RadioGarden_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(_radioGardenUrl);
        }

        #endregion

        #region Full Screen Mode

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
            
            // Ensure focus
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

        #endregion
    }
}