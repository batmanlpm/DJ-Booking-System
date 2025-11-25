using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem.Controls
{
    public enum CandyBotState
    {
        Idle,
        Thinking,
        Speaking,
        Error,
        Offline
    }

    public partial class CandyBotAvatar : UserControl
    {
        private Storyboard? _idleBobAnimation;
        private Storyboard? _activeBounceAnimation;
        private Storyboard? _sparkleAnimation;
        private CandyBotState _currentState = CandyBotState.Idle;

        public event RoutedEventHandler? AvatarClicked;
        public event EventHandler<CandyBotPersonalityMode>? PersonalityChanged;
        public event EventHandler? DesktopModeRequested;
        public event EventHandler? SearchWebRequested;
        public event EventHandler? OpenChatRequested;
        public event EventHandler? OpenSettingsRequested;
        public event EventHandler<bool>? AlwaysOnTopToggled;
        public event EventHandler<bool>? StartWithWindowsToggled;
        public event EventHandler<bool>? QuietModeToggled;
        public event EventHandler<bool>? VoiceModeToggled;
        public event EventHandler<bool>? SoundsToggled;


        public CandyBotAvatar()
        {
            InitializeComponent();
            this.Loaded += CandyBotAvatar_Loaded;
            this.MouseLeftButtonDown += CandyBotAvatar_MouseLeftButtonDown;
        }

        private void CandyBotAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            // Load animations
            _idleBobAnimation = (Storyboard)this.Resources["IdleBobAnimation"];
            _activeBounceAnimation = (Storyboard)this.Resources["ActiveBounceAnimation"];
            _sparkleAnimation = (Storyboard)this.Resources["SparkleAnimation"];
            
            // Start idle animation
            StartIdleAnimation();
            
            // Set initial state
            SetState(CandyBotState.Idle);
        }

        #region Avatar States

        /// <summary>
        /// Set the avatar's current state with visual feedback
        /// </summary>
        public void SetState(CandyBotState state)
        {
            _currentState = state;
            
            switch (state)
            {
                case CandyBotState.Idle:
                    StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0)); // Green
                    break;
                    
                case CandyBotState.Thinking:
                    StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0)); // Yellow
                    break;
                    
                case CandyBotState.Speaking:
                    StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 255)); // Cyan
                    break;
                    
                case CandyBotState.Error:
                    StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0)); // Red
                    break;
                    
                case CandyBotState.Offline:
                    StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(128, 128, 128)); // Gray
                    break;
            }
            
            System.Diagnostics.Debug.WriteLine($"Candy-Bot State: {state}");
        }

        /// <summary>
        /// Get the current avatar state
        /// </summary>
        public CandyBotState GetCurrentState() => _currentState;

        /// <summary>
        /// Update avatar glow based on personality mode
        /// </summary>
        public void UpdateAvatarForPersonality(CandyBotPersonalityMode mode)
        {
            var glowColor = mode switch
            {
                CandyBotPersonalityMode.Shy => Color.FromRgb(255, 192, 203),      // Pink
                CandyBotPersonalityMode.Funny => Color.FromRgb(255, 255, 0),      // Yellow
                CandyBotPersonalityMode.ShitStirring => Color.FromRgb(255, 69, 0), // Orange-Red
                CandyBotPersonalityMode.Raunchy => Color.FromRgb(255, 20, 147),   // Deep Pink
                CandyBotPersonalityMode.Professional => Color.FromRgb(0, 191, 255), // Blue
                _ => Color.FromRgb(0, 255, 255)                                    // Cyan (Normal)
            };
            
            GlowEffect.Color = glowColor;
            System.Diagnostics.Debug.WriteLine($"?? Avatar updated for {mode} mode");
        }

        #endregion

        #region Animations

        private void StartIdleAnimation()
        {
            _idleBobAnimation?.Begin();
        }

        private void StopIdleAnimation()
        {
            _idleBobAnimation?.Stop();
        }

        private void Avatar_MouseEnter(object sender, MouseEventArgs e)
        {
            // Start sparkle effect on hover
            _sparkleAnimation?.Begin();
        }

        private void Avatar_MouseLeave(object sender, MouseEventArgs e)
        {
            // Stop sparkle effect
            _sparkleAnimation?.Stop();
            
            // Reset sparkle opacity
            Sparkle1.Opacity = 0;
            Sparkle2.Opacity = 0;
            Sparkle3.Opacity = 0;
        }

        private void Avatar_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Context menu will open automatically
            e.Handled = true;
        }

        private void CandyBotAvatar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Bounce when clicked
            StopIdleAnimation();
            _activeBounceAnimation?.Begin();
            
            // Restart idle after bounce
            if (_activeBounceAnimation != null)
            {
                _activeBounceAnimation.Completed += (s, ev) =>
                {
                    StartIdleAnimation();
                };
            }
            
            // Raise click event
            AvatarClicked?.Invoke(this, new RoutedEventArgs());
        }

        #endregion

        #region Context Menu Handlers

        // Personality Mode Changes
        private void SetPersonality_Normal(object sender, RoutedEventArgs e)
        {
            PersonalityChanged?.Invoke(this, CandyBotPersonalityMode.Normal);
            UpdateAvatarForPersonality(CandyBotPersonalityMode.Normal);
            ShowPersonalityChangeNotification("Normal", "Normal");
        }

        private void SetPersonality_Shy(object sender, RoutedEventArgs e)
        {
            PersonalityChanged?.Invoke(this, CandyBotPersonalityMode.Shy);
            UpdateAvatarForPersonality(CandyBotPersonalityMode.Shy);
            ShowPersonalityChangeNotification("Shy", "Shy");
        }

        private void SetPersonality_Funny(object sender, RoutedEventArgs e)
        {
            PersonalityChanged?.Invoke(this, CandyBotPersonalityMode.Funny);
            UpdateAvatarForPersonality(CandyBotPersonalityMode.Funny);
            ShowPersonalityChangeNotification("Funny", "Funny");
        }

        private void SetPersonality_ShitStirring(object sender, RoutedEventArgs e)
        {
            PersonalityChanged?.Invoke(this, CandyBotPersonalityMode.ShitStirring);
            UpdateAvatarForPersonality(CandyBotPersonalityMode.ShitStirring);
            ShowPersonalityChangeNotification("Shit-Stirring", "Stirring");
        }

        private void SetPersonality_Raunchy(object sender, RoutedEventArgs e)
        {
            // This will trigger age verification in parent
            PersonalityChanged?.Invoke(this, CandyBotPersonalityMode.Raunchy);
            UpdateAvatarForPersonality(CandyBotPersonalityMode.Raunchy);
        }

        private void SetPersonality_Professional(object sender, RoutedEventArgs e)
        {
            PersonalityChanged?.Invoke(this, CandyBotPersonalityMode.Professional);
            UpdateAvatarForPersonality(CandyBotPersonalityMode.Professional);
            ShowPersonalityChangeNotification("Professional", "Pro");
        }

        private void ShowPersonalityChangeNotification(string modeName, string label)
        {
            // Visual feedback
            StopIdleAnimation();
            _activeBounceAnimation?.Begin();
            
            if (_activeBounceAnimation != null)
            {
                _activeBounceAnimation.Completed += (s, ev) =>
                {
                    StartIdleAnimation();
                };
            }
        }

        // Display Options
        private void DesktopMode_Click(object sender, RoutedEventArgs e)
        {
            DesktopModeRequested?.Invoke(this, EventArgs.Empty);
        }

        private void AlwaysOnTop_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                // Event will be handled by parent window
                AlwaysOnTopToggled?.Invoke(this, menuItem.IsChecked);
            }
        }

        private void StartWithWindows_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                StartWithWindowsToggled?.Invoke(this, menuItem.IsChecked);
            }
        }

        private void QuietMode_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                QuietModeToggled?.Invoke(this, menuItem.IsChecked);
            }
        }

        private void VoiceMode_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                VoiceModeToggled?.Invoke(this, menuItem.IsChecked);
            }
        }

        private void Sounds_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                SoundsToggled?.Invoke(this, menuItem.IsChecked);
            }
        }

        // Quick Actions
        private void SearchWeb_Click(object sender, RoutedEventArgs e)
        {
            SearchWebRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OpenKnowledge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open knowledge base markdown file
                var knowledgeBasePath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "CANDYBOT_KNOWLEDGE_BASE.md");
                
                if (System.IO.File.Exists(knowledgeBasePath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = knowledgeBasePath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show(
                        "Knowledge base file not found!",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening knowledge base: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OpenChat_Click(object sender, RoutedEventArgs e)
        {
            OpenChatRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            OpenSettingsRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OpenBookings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    // Close desktop widget if open
                    var widget = Application.Current.Windows.OfType<CandyBotDesktopWidget>().FirstOrDefault();
                    widget?.Close();
                    
                    // Navigate to bookings
                    mainWindow.ShowBookingsView();
                    mainWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening Bookings:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Candy-Bot AI Assistant\n\n" +
                "Version 2.0 - Rainbow Edition\n\n" +
                "Your personal AI assistant with 6 unique personalities!\n\n" +
                "Features:\n" +
                "• 600+ unique responses\n" +
                "• Web search integration\n" +
                "• Desktop widget mode\n" +
                "• Multi-drive file search\n" +
                "• AI code generation\n" +
                "• Image & document generation\n" +
                "• Knowledge base access\n" +
                "• Multiple personality modes\n" +
                "• Beautiful rainbow candy avatar\n\n" +
                "Created for The Fallen Collective DJ Booking System\n\n" +
                "Right-click me anytime for options!",
                "About Candy-Bot",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // File Search Features
        private void OpenMultiDriveSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var searchWindow = new MultiDriveSearchWindow();
                searchWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening Multi-Drive Search:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void QuickSearch_Music_Click(object sender, RoutedEventArgs e)
        {
            PerformQuickSearch(
                "Music Files",
                new[] { ".mp3", ".wav", ".flac", ".m4a", ".aac", ".ogg", ".wma" },
                "Music");
        }

        private void QuickSearch_Documents_Click(object sender, RoutedEventArgs e)
        {
            PerformQuickSearch(
                "Documents",
                new[] { ".pdf", ".docx", ".doc", ".txt", ".xlsx", ".xls", ".pptx", ".ppt" },
                "Documents");
        }

        private void QuickSearch_Images_Click(object sender, RoutedEventArgs e)
        {
            PerformQuickSearch(
                "Images",
                new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp" },
                "Images");
        }

        private void QuickSearch_Videos_Click(object sender, RoutedEventArgs e)
        {
            PerformQuickSearch(
                "Videos",
                new[] { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm" },
                "Videos");
        }

        private void QuickSearch_Archives_Click(object sender, RoutedEventArgs e)
        {
            PerformQuickSearch(
                "Archives",
                new[] { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2" },
                "Archives");
        }

        private void QuickSearch_Code_Click(object sender, RoutedEventArgs e)
        {
            PerformQuickSearch(
                "Code Files",
                new[] { ".cs", ".js", ".py", ".java", ".cpp", ".h", ".html", ".css", ".sql" },
                "Code");
        }

        private void QuickSearch_World_Click(object sender, RoutedEventArgs e)
        {
            PerformQuickSearch(
                ".world Files",
                new[] { ".world" },
                "World");
        }

        private void PerformQuickSearch(string typeName, string[] extensions, string label)
        {
            try
            {
                SetState(CandyBotState.Thinking);

                var result = MessageBox.Show(
                    $"Quick Search for {typeName}\n\n" +
                    $"Extensions: {string.Join(", ", extensions)}\n\n" +
                    "This will:\n" +
                    "• Search ALL approved drives\n" +
                    "• Find matching files\n" +
                    "• Display results\n\n" +
                    "Open Multi-Drive Search window now?",
                    $"Quick Search - {typeName}",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var searchWindow = new MultiDriveSearchWindow();
                    searchWindow.Show();
                    
                    // Set extensions and trigger search would go here
                    // You'd need to add a public method to MultiDriveSearchWindow
                }

                SetState(CandyBotState.Idle);
            }
            catch (Exception ex)
            {
                SetState(CandyBotState.Error);
                MessageBox.Show(
                    $"Error performing quick search:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                SetState(CandyBotState.Idle);
            }
        }

        private void ManageApprovedFolders_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "?? MANAGE APPROVED FOLDERS\n\n" +
                "Control which folders Candy-Bot can access:\n\n" +
                "• Add trusted folders\n" +
                "• Remove folder access\n" +
                "• View approved list\n" +
                "• Security settings\n\n" +
                "Open Multi-Drive Search window to manage folders.",
                "Approved Folders",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ApproveAllDrives_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "?? APPROVE ALL DRIVES\n\n" +
                "This will allow Candy-Bot to search:\n" +
                "• All local drives (C:, D:, E:, etc.)\n" +
                "• External drives\n" +
                "• Network drives\n\n" +
                "System folders will still be excluded for safety.\n\n" +
                "Approve all drives for searching?",
                "Approve All Drives",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var searcher = new Services.MultiDriveFileSearcher();
                    searcher.ApproveAllDrives();
                    
                    MessageBox.Show(
                        "? All drives approved!\n\n" +
                        "Candy-Bot can now search all your drives.\n" +
                        "Use the Multi-Drive Search window to find files.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error approving drives:\n{ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        // Code & AI Tools
        private void OpenCodeAssistant_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get current user from main window
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    var codeAssistant = new CandyBotCodeAssistant(
                        mainWindow.CurrentUser,
                        mainWindow.CandyBotService,
                        mainWindow.CosmosDbService);
                    codeAssistant.Show();
                }
                else
                {
                    MessageBox.Show(
                        "Main window not found. Please try again.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening Code Assistant:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OpenImageGenerator_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "?? AI IMAGE GENERATOR\n\n" +
                "Generate custom images with AI:\n\n" +
                "• Album covers\n" +
                "• DJ logos\n" +
                "• Event posters\n" +
                "• Custom artwork\n\n" +
                "Use the ?? Tests menu > Test Image Generation\n" +
                "to try this feature!",
                "Image Generator",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void OpenDocumentGenerator_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "?? DOCUMENT GENERATOR\n\n" +
                "Generate professional documents:\n\n" +
                "• DJ contracts\n" +
                "• Set lists\n" +
                "• Event schedules\n" +
                "• Reports\n\n" +
                "Use the ?? Tests menu > Test Documents\n" +
                "to try this feature!",
                "Document Generator",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            // Close will be handled by parent window
            var window = Window.GetWindow(this);
            window?.Close();
        }

        #endregion

        #region Notification Badge

        public void ShowNotification(int count)
        {
            NotificationBadge.Visibility = Visibility.Visible;
            NotificationCount.Text = count.ToString();
        }

        public void HideNotification()
        {
            NotificationBadge.Visibility = Visibility.Collapsed;
        }

        #endregion
    }
}
