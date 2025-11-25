using System.Windows;
using System.Windows.Controls;
using DJBookingSystem.Models;

namespace DJBookingSystem.Views.Radio
{
    /// <summary>
    /// Unified Radio Control Center - 3 clickable sections to navigate to each radio panel
    /// Left: LivePartyMusic.fm (C40), Middle: Radio Station Listener, Right: Candy-Bot Relay (C19)
    /// </summary>
    public partial class RadioUnifiedView : UserControl
    {
        private User? _currentUser;

        public RadioUnifiedView()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("[RadioUnifiedView] Initialized");
        }

        /// <summary>
        /// Initialize with current user for permission checking
        /// </summary>
        public void Initialize(User currentUser)
        {
            _currentUser = currentUser;
            System.Diagnostics.Debug.WriteLine($"[RadioUnifiedView] Initialized with user: {currentUser?.Username}");
        }

        private void OpenLivePartyMusic_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[RadioUnifiedView] Opening LivePartyMusic (C40)");

            // Navigate to RadioBoss Stream panel
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                // Call MainWindow method to show RadioBossStreamPanel
                var method = mainWindow.GetType().GetMethod("ShowPanel",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                var streamPanel = mainWindow.GetType().GetField("RadioBossStreamPanel",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(mainWindow);

                method?.Invoke(mainWindow, new[] { streamPanel });

                // Initialize the view
                if (streamPanel is System.Windows.Controls.Grid panel && panel.Children.Count > 0)
                {
                    if (panel.Children[0] is RadioBossStreamView streamView && _currentUser != null)
                    {
                        streamView.Initialize(_currentUser);
                    }
                }
            }
        }

        private void OpenRadioStationListener_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[RadioUnifiedView] Opening Radio Station Listener");

            // Navigate to Radio Player panel
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                var method = mainWindow.GetType().GetMethod("ShowPanel",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                var playerPanel = mainWindow.GetType().GetField("RadioPlayerPanel",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(mainWindow);

                method?.Invoke(mainWindow, new[] { playerPanel });
            }
        }

        private void OpenCandyBotRelay_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[RadioUnifiedView] Opening Candy-Bot Relay (C19)");

            // Navigate to RadioBoss Cloud panel
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                var method = mainWindow.GetType().GetMethod("ShowPanel",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                var cloudPanel = mainWindow.GetType().GetField("RadioBossCloudPanel",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(mainWindow);

                method?.Invoke(mainWindow, new[] { cloudPanel });

                // Initialize the view
                if (cloudPanel is System.Windows.Controls.Grid panel && panel.Children.Count > 0)
                {
                    if (panel.Children[0] is RadioBossCloudView cloudView && _currentUser != null)
                    {
                        cloudView.Initialize(_currentUser);
                    }
                }
            }
        }

        /// <summary>
        /// Close Radio View and return to main menu
        /// </summary>
        private void CloseRadioView_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[RadioUnifiedView] Closing Radio View");
            
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                // Show Welcome panel to return to main menu
                var method = mainWindow.GetType().GetMethod("ShowPanel",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                var welcomePanel = mainWindow.GetType().GetField("WelcomePanel",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(mainWindow);

                method?.Invoke(mainWindow, new[] { welcomePanel });
                
                System.Diagnostics.Debug.WriteLine("[RadioUnifiedView] Returned to Welcome panel");
            }
        }
    }
}
