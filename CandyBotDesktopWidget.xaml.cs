using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    // Suppress CS4014 for this file: TTS voice feedback is intentionally fire-and-forget
    // Voice feedback doesn't need to be awaited - it's non-blocking background audio
#pragma warning disable CS4014

    /// <summary>
    /// Enhanced Candy-Bot Desktop Widget v2.0
    /// Floating desktop companion with bobbing animation, voice feedback, and full features
    /// </summary>
    public partial class CandyBotDesktopWidget : Window
    {
        private CandyBotTextToSpeech _tts;
        private ContextMenu _contextMenu = new();
        private bool _isDragging = false;
        private Point _clickPosition;
        private DispatcherTimer _speechBubbleTimer = new();

        public CandyBotDesktopWidget()
        {
            InitializeComponent();
            
            // Initialize TTS
            _tts = new CandyBotTextToSpeech();
            _tts.Initialize();
            _tts.SetEnabled(true);
            
            // Setup context menu with icons
            SetupContextMenuWithIcons();
            
            // Setup speech bubble auto-hide timer
            _speechBubbleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _speechBubbleTimer.Tick += (s, e) =>
            {
                HideSpeechBubble();
                _speechBubbleTimer.Stop();
            };
            
            // Welcome message (voice feedback is fire-and-forget)
            _tts.SpeakAsync("Candy-Bot desktop widget ready! Right-click me for all features!");
        }

        /// <summary>
        /// Window loaded - start animations and check for updates
        /// </summary>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Start bobbing animation
            var bobbingStoryboard = (Storyboard)this.Resources["BobbingAnimation"];
            bobbingStoryboard.Begin();
            
            // Show speech bubble after short delay
            await System.Threading.Tasks.Task.Delay(800);
            ShowSpeechBubble("Hi! Right-click me for options!");
            
            // Check for updates
            await CheckForUpdatesAsync();
        }

        /// <summary>
        /// Show speech bubble with message
        /// </summary>
        private void ShowSpeechBubble(string message)
        {
            if (SpeechBubbleText != null && SpeechBubblePopup != null)
            {
                SpeechBubbleText.Text = message;
                var animation = (Storyboard)this.Resources["SpeechBubbleAnimation"];
                animation.Begin();
                _speechBubbleTimer.Start();
            }
        }

        /// <summary>
        /// Hide speech bubble
        /// </summary>
        private void HideSpeechBubble()
        {
            if (SpeechBubblePopup != null)
            {
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
                SpeechBubblePopup.BeginAnimation(OpacityProperty, fadeOut);
            }
        }

        /// <summary>
        /// Check for application updates
        /// </summary>
        private async System.Threading.Tasks.Task CheckForUpdatesAsync()
        {
            try
            {
                var updateService = new CandyBotUpdateService();
                var updateInfo = await updateService.CheckForUpdatesAsync();
                
                if (updateInfo.UpdateAvailable)
                {
                    ShowSpeechBubble($"Update v{updateInfo.LatestVersion} available!");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    _tts.SpeakAsync($"Update version {updateInfo.LatestVersion} is available!");
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    
                    // Show update prompt after speech bubble
                    await System.Threading.Tasks.Task.Delay(5000);
                    
                    var result = MessageBox.Show(
                        $"Candy-Bot Update Available!\n\n" +
                        $"Current Version: {updateInfo.CurrentVersion}\n" +
                        $"Latest Version: {updateInfo.LatestVersion}\n\n" +
                        $"What's New:\n{updateInfo.ReleaseNotes}\n\n" +
                        $"Would you like to update now?",
                        "Update Available",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        await updateService.DownloadAndInstallUpdateAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update check failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Setup context menu with proper icons matching the design
        /// Role-based menu items based on user permissions
        /// </summary>
        private void SetupContextMenuWithIcons()
        {
            // Apply the style from resources
            var menuStyle = (Style)this.FindResource("DesktopWidgetMenuStyle");
            var itemStyle = (Style)this.FindResource("DesktopWidgetMenuItemStyle");
            
            _contextMenu = new ContextMenu
            {
                Style = menuStyle,
                Background = new SolidColorBrush(Color.FromRgb(26, 26, 26)),
                Foreground = new SolidColorBrush(Color.FromRgb(255, 20, 147)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(255, 105, 180)),
                BorderThickness = new Thickness(2),
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                HasDropShadow = false
            };

            // Add drop shadow effect
            _contextMenu.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Color.FromRgb(255, 105, 180),
                BlurRadius = 15,
                ShadowDepth = 0,
                Opacity = 0.8
            };

            // Get current user for role-based menu
            var currentUser = AuthenticationService.Instance.CurrentUser;
            bool isAdmin = currentUser?.Role == Models.UserRole.SysAdmin;
            
            // === BOOKINGS (All Users) ===
            AddMenuItemWithIcon("📅 Bookings", "", DJBookings_Click, itemStyle);
            _contextMenu.Items.Add(CreateSeparator());
            
            // === VENUES (All Users) ===
            AddMenuItemWithIcon("🏢 Venues", "", ViewVenues_Click, itemStyle);
            _contextMenu.Items.Add(CreateSeparator());
            
            // === RADIO PLAYER (All Users) ===
            AddMenuItemWithIcon("📻 Radio Player", "", RadioPlayer_Click, itemStyle);
            _contextMenu.Items.Add(CreateSeparator());
            
            // === CHAT (All Users) ===
            AddMenuItemWithIcon("💬 Chat", "", OpenChat_Click, itemStyle);
            _contextMenu.Items.Add(CreateSeparator());
            
            // === SETTINGS (All Users) ===
            AddMenuItemWithIcon("⚙️ Settings", "", Settings_Click, itemStyle);
            _contextMenu.Items.Add(CreateSeparator());
            
            // === HELP (All Users) ===
            AddMenuItemWithIcon("❓ Help", "", Help_Click, itemStyle);
            
            // === ADMIN-ONLY FEATURES ===
            if (isAdmin)
            {
                _contextMenu.Items.Add(CreateSeparator());
                
                // USERS MANAGEMENT (Admin Only)
                AddMenuItemWithIcon("👥 Users", "", Users_Click, itemStyle);
                
                _contextMenu.Items.Add(CreateSeparator());
                
                // ADVANCED TESTS (Admin Only)
                AddMenuItemWithIcon("🧪 Tests", "", Tests_Click, itemStyle);
                
                _contextMenu.Items.Add(CreateSeparator());
                
                // FILE SEARCH (Admin Only)
                var fileSearchMenu = CreateSubmenuWithIcon("🔍 File Search", "", "▶", itemStyle);
                fileSearchMenu.Items.Add(CreateSubmenuItem("Quick Search: Music", () => QuickSearch("music"), itemStyle));
                fileSearchMenu.Items.Add(CreateSubmenuItem("Quick Search: Documents", () => QuickSearch("documents"), itemStyle));
                fileSearchMenu.Items.Add(CreateSubmenuItem("Quick Search: Images", () => QuickSearch("images"), itemStyle));
                fileSearchMenu.Items.Add(CreateSubmenuItem("Quick Search: Videos", () => QuickSearch("videos"), itemStyle));
                fileSearchMenu.Items.Add(CreateSeparator());
                fileSearchMenu.Items.Add(CreateSubmenuItem("Multi-Drive Search", OpenMultiDriveSearch, itemStyle));
                _contextMenu.Items.Add(fileSearchMenu);
                
                _contextMenu.Items.Add(CreateSeparator());
                
                // CODE & AI TOOLS (Admin Only)
                var codeMenu = CreateSubmenuWithIcon("💻 Code & AI Tools", "", "▶", itemStyle);
                codeMenu.Items.Add(CreateSubmenuItem("Coding Expert", OpenCodingExpert, itemStyle));
                codeMenu.Items.Add(CreateSubmenuItem("AI Assistant", OpenAIAssistant, itemStyle));
                codeMenu.Items.Add(CreateSubmenuItem("Code Generator", OpenCodeGenerator, itemStyle));
                _contextMenu.Items.Add(codeMenu);
            }
            
            _contextMenu.Items.Add(CreateSeparator());
            
            // === CLOSE (All Users) ===
            AddMenuItemWithIcon("✕ Close", "", Close_Click, itemStyle);

            this.ContextMenu = _contextMenu;
        }

        #region Menu Helpers

        private void AddMenuItemWithIcon(string text, string icon, RoutedEventHandler handler, Style style)
        {
            var item = new MenuItem
            {
                Header = string.IsNullOrEmpty(icon) ? text : $"{icon}  {text}",
                Style = style,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 105, 180)),
                Background = new SolidColorBrush(Color.FromRgb(26, 26, 26)),
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(0)
            };
            
            // Style for when item is highlighted/hovered
            item.MouseEnter += (s, e) =>
            {
                item.Foreground = Brushes.Black;
                item.Background = new SolidColorBrush(Color.FromRgb(255, 105, 180));
            };
            item.MouseLeave += (s, e) =>
            {
                item.Foreground = new SolidColorBrush(Color.FromRgb(255, 105, 180));
                item.Background = new SolidColorBrush(Color.FromRgb(26, 26, 26));
            };
            
            item.Click += handler;
            _contextMenu.Items.Add(item);
        }

        private MenuItem CreateSubmenuWithIcon(string text, string icon, string arrow, Style style)
        {
            var menuItem = new MenuItem
            {
                Header = string.IsNullOrEmpty(icon) ? text : $"{icon}  {text}",
                Style = style,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 105, 180)),
                Background = new SolidColorBrush(Color.FromRgb(26, 26, 26)),
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(0)
            };
            
            // Style for when item is highlighted/hovered
            menuItem.MouseEnter += (s, e) =>
            {
                menuItem.Foreground = Brushes.Black;
                menuItem.Background = new SolidColorBrush(Color.FromRgb(255, 105, 180));
            };
            menuItem.MouseLeave += (s, e) =>
            {
                menuItem.Foreground = new SolidColorBrush(Color.FromRgb(255, 105, 180));
                menuItem.Background = new SolidColorBrush(Color.FromRgb(26, 26, 26));
            };
            
            return menuItem;
        }

        private MenuItem CreateSubmenuItem(string header, Action action, Style style)
        {
            var item = new MenuItem
            {
                Header = header,
                Style = style,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                Background = new SolidColorBrush(Color.FromRgb(30, 0, 30)),
                FontSize = 12,
                Padding = new Thickness(10, 4, 10, 4),
                Margin = new Thickness(0)
            };
            
            // Style for when item is highlighted/hovered
            item.MouseEnter += (s, e) =>
            {
                item.Foreground = Brushes.Black;
                item.Background = new SolidColorBrush(Color.FromRgb(255, 105, 180));
            };
            item.MouseLeave += (s, e) =>
            {
                item.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                item.Background = new SolidColorBrush(Color.FromRgb(30, 0, 30));
            };
            
            item.Click += (s, e) => action();
            return item;
        }

        private Separator CreateSeparator()
        {
            return new Separator
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 105, 180)),
                Height = 2,
                Margin = new Thickness(0)
            };
        }

        #endregion

        #region Context Menu Handlers

        private void BookingMode_Click(object sender, RoutedEventArgs e)
        {
            CandyBotMessageBox.Show("Switching to Booking Mode!\n\n" +
                "In Booking Mode, you can manage your DJ bookings and schedules.\n\n" +
                "To return to Desktop Mode, close this window.", 
                "Candy-Bot", MessageBoxButton.OK, MessageBoxImage.Information);
            _tts.SpeakAsync("Switching to booking mode!");
        }
        
        private void ViewVenues_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Navigate to Venues in main window
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.Show();
                    mainWindow.WindowState = WindowState.Normal;
                    mainWindow.Activate();
                    // Navigate to venues - you'll need to add this method to MainWindow
                    _tts.SpeakAsync("Opening venues!");
                }
            }
            catch (Exception ex)
            {
                CandyBotMessageBox.Show($"Error opening venues: {ex.Message}", "Candy-Bot", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void RadioPlayer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Navigate to Radio Player in main window
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.Show();
                    mainWindow.WindowState = WindowState.Normal;
                    mainWindow.Activate();
                    _tts.SpeakAsync("Opening radio player!");
                }
            }
            catch (Exception ex)
            {
                CandyBotMessageBox.Show($"Error opening radio: {ex.Message}", "Candy-Bot", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            CandyBotMessageBox.Show(
                "Candy-Bot Desktop Widget Help\n\n" +
                "📅 BOOKINGS\n" +
                "View and manage your DJ bookings\n\n" +
                "🏢 VENUES\n" +
                "Browse available venues\n\n" +
                "📻 RADIO PLAYER\n" +
                "Listen to live radio streams\n\n" +
                "💬 CHAT\n" +
                "Chat with Candy-Bot AI assistant\n\n" +
                "⚙️ SETTINGS\n" +
                "Customize your preferences\n\n" +
                "Right-click me anytime for this menu!",
                "Candy-Bot Help",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            _tts.SpeakAsync("Here's how to use me!");
        }
        
        private void Users_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.Show();
                    mainWindow.WindowState = WindowState.Normal;
                    mainWindow.Activate();
                    _tts.SpeakAsync("Opening user management!");
                }
            }
            catch (Exception ex)
            {
                CandyBotMessageBox.Show($"Error opening users: {ex.Message}", "Candy-Bot", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void Tests_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.Show();
                    mainWindow.WindowState = WindowState.Normal;
                    mainWindow.Activate();
                    _tts.SpeakAsync("Opening test menu!");
                }
            }
            catch (Exception ex)
            {
                CandyBotMessageBox.Show($"Error opening tests: {ex.Message}", "Candy-Bot", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenChat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get current user from AuthenticationService
                var currentUser = AuthenticationService.Instance.CurrentUser;
                
                // Create CandyBotService if needed
                var candyBotService = new CandyBotService();
                
                // Open chat window with proper parameters
                var chatWindow = new CandyBotWindow(currentUser, candyBotService);
                chatWindow.Show();
                _tts.SpeakAsync("Opening chat window! Let's talk!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening chat: {ex.Message}");
                CandyBotMessageBox.Show($"Could not open chat: {ex.Message}", "Candy-Bot", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchWeb_Click(object sender, RoutedEventArgs e)
        {
            CandyBotMessageBox.Show("Web Search\n\nWeb search feature coming in next update!", 
                "Candy-Bot", MessageBoxButton.OK, MessageBoxImage.Information);
            _tts.SpeakAsync("Web search coming soon!");
        }

        private void SetPersonality(string mode)
        {
            string displayText = mode switch
            {
                "Normal" => "Normal - Sweet & Balanced",
                "Shy" => "Shy - Timid & Cute",
                "Funny" => "Funny - Jokes & Humor",
                "Raunchy" => "Raunchy - Flirty & Bold",
                "ShitStirring" => "Shit-Stirring - Mischievous",
                "Professional" => "Professional - Formal",
                _ => "Normal - Sweet & Balanced"
            };
            
            // Show speech bubble instead of updating text element
            ShowSpeechBubble($"Personality: {displayText}");
            _tts.SpeakAsync($"Personality changed to {mode} mode!");
        }

        private void SetTheme(string theme)
        {
            CandyBotMessageBox.Show($"Theme: {theme}\n\nTheme switching coming in next update!", 
                "Candy-Bot", MessageBoxButton.OK, MessageBoxImage.Information);
            _tts.SpeakAsync($"Theme changed to {theme}!");
        }

        private void ToggleAlwaysOnTop()
        {
            Topmost = !Topmost;
            var status = Topmost ? "enabled" : "disabled";
            CandyBotMessageBox.Show($"Always On Top: {status.ToUpper()}", "Candy-Bot", MessageBoxButton.OK, MessageBoxImage.Information);
            _tts.SpeakAsync($"Always on top {status}!");
        }

        private void ToggleVisibility()
        {
            WindowState = WindowState == WindowState.Minimized ? WindowState.Normal : WindowState.Minimized;
        }

        private void QuickSearch(string category)
        {
            try
            {
                var searchWindow = new MultiDriveSearchWindow();
                searchWindow.Show();
                _tts.SpeakAsync($"Opening quick search for {category}!");
            }
            catch
            {
                CandyBotMessageBox.Show($"{category.ToUpper()} Search\n\nOpening file search window...", "Candy-Bot", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenMultiDriveSearch()
        {
            try
            {
                var searchWindow = new MultiDriveSearchWindow();
                searchWindow.Show();
                _tts.SpeakAsync("Opening multi-drive file search!");
            }
            catch (Exception ex)
            {
                CandyBotMessageBox.Show($"Error opening search window:\n{ex.Message}", "Candy-Bot", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenCodingExpert()
        {
            try
            {
                var codeWindow = new CandyBotCodeAssistant(null, null, null);
                codeWindow.Show();
                _tts.SpeakAsync("Opening coding expert! Ready to code!");
            }
            catch
            {
                CandyBotMessageBox.Show("Coding Expert\n\nFeature coming soon!", "Candy-Bot", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenAIAssistant()
        {
            try
            {
                var chatWindow = new CandyBotWindow(null, null);
                chatWindow.Show();
                _tts.SpeakAsync("Opening AI assistant!");
            }
            catch
            {
                CandyBotMessageBox.Show("AI Assistant coming soon!", "Candy-Bot", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenCodeGenerator()
        {
            try
            {
                var codeWindow = new CandyBotCodeAssistant(null, null, null);
                codeWindow.Show();
                _tts.SpeakAsync("Opening code generator!");
            }
            catch
            {
                CandyBotMessageBox.Show("Code Generator coming soon!", "Candy-Bot", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DJBookings_Click(object sender, RoutedEventArgs e)
        {
            CandyBotMessageBox.Show("DJ BOOKINGS\n\nFEATURE COMING SOON\n\n" +
                "DJ Bookings integration will be available in the next update!\n\n" +
                "Stay tuned!", 
                "Candy-Bot - Coming Soon", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
            _tts.SpeakAsync("DJ Bookings feature coming soon! Stay tuned!");
        }

        private void SetVoice(bool enabled)
        {
            _tts.SetEnabled(enabled);
            var status = enabled ? "ON" : "OFF";
            CandyBotMessageBox.Show($"Voice: {status}", "Candy-Bot", MessageBoxButton.OK, MessageBoxImage.Information);
            if (enabled)
                _tts.SpeakAsync("Voice enabled!");
        }

        private void AdjustSpeed(int rate)
        {
            _tts.SetSpeechRate(rate);
            string speed = rate > 0 ? "faster" : rate < 0 ? "slower" : "normal";
            _tts.SpeakAsync($"Speech rate set to {speed}!");
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsWindow = new CandyBotSettingsWindow();
                settingsWindow.Show();
                _tts.SpeakAsync("Opening Candy-Bot settings!");
            }
            catch (Exception ex)
            {
                CandyBotMessageBox.Show($"Error opening settings:\n{ex.Message}", 
                    "Candy-Bot", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            CandyBotMessageBox.Show(
                "Candy-Bot Desktop Widget v2.0\n\n" +
                "Your AI-powered desktop assistant!\n\n" +
                "Features:\n" +
                "• Multi-drive file search & organization\n" +
                "• Coding expert with 10+ languages\n" +
                "• Voice interaction & feedback\n" +
                "• Smart file management\n" +
                "• Personality modes\n" +
                "• And much more!\n\n" +
                "Created with <3 for productivity\n\n" +
                "Right-click me for all features!",
                "About Candy-Bot",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Main Widget Button Handlers

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var searchWindow = new MultiDriveSearchWindow();
                searchWindow.Show();
                _tts.SpeakAsync("Opening advanced file search!");
            }
            catch
            {
                CandyBotMessageBox.Show("File Search coming soon!", "Candy-Bot", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var chatWindow = new CandyBotWindow(null, null);
                chatWindow.Show();
                _tts.SpeakAsync("Let's chat! I'm ready to help!");
            }
            catch
            {
                CandyBotMessageBox.Show("Chat feature coming soon!", "Candy-Bot", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            CandyBotMessageBox.Show(
                "Candy-Bot Quick Help\n\n" +
                "HOW TO USE:\n" +
                "• Right-click me for full menu\n" +
                "• Drag me anywhere on desktop\n" +
                "• I have voice feedback!\n" +
                "• Access coding tools & more!\n\n" +
                "TIP: Right-click to explore all features!",
                "Candy-Bot Help",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void InfoTab_Click(object sender, RoutedEventArgs e)
        {
            About_Click(sender, e);
        }

        private void SearchTab_Click(object sender, RoutedEventArgs e)
        {
            SearchButton_Click(sender, e);
        }

        #endregion

        #region Window Dragging

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _clickPosition = e.GetPosition(this);
            this.CaptureMouse();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = PointToScreen(e.GetPosition(this));
                this.Left = currentPosition.X - _clickPosition.X;
                this.Top = currentPosition.Y - _clickPosition.Y;
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            this.ReleaseMouseCapture();
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Show context menu on right-click
            if (_contextMenu != null)
            {
                _contextMenu.PlacementTarget = this;
                _contextMenu.IsOpen = true;
            }
        }

        #endregion

        #region Window Controls

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        #endregion
    }
}
