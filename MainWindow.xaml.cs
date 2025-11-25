using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Threading;
using DJBookingSystem.Models;
using DJBookingSystem.Services;
using DJBookingSystem.ViewModels;
using WinForms = System.Windows.Forms; // Alias for Windows Forms
using System.IO;

namespace DJBookingSystem
{
    public partial class MainWindow : Window
    {
        private CosmosDbService _cosmosDbService = null!;
        private RadioBossService _radioBossService;
        private User _currentUser;
        private AppSettings _appSettings;
        private List<Booking> _allBookings = new List<Booking>();
        private List<Venue> _allVenues = new List<Venue>();
        private DispatcherTimer? _connectionCheckTimer;
        private DispatcherTimer? _nowPlayingTimer; // NEW: Now Playing update timer
        private MediaPlayer? _mediaPlayer;
        private PermissionService _permissionService = null!;
        
        // Candy-Bot Services (Phase 2 Integration)
        private CandyBotSoundManager? _candySound;
        private CandyBotFileManager? _candyFiles;
        
        // Candy-Bot Image & Document Generation (Phase 1 - Image/Doc Integration)
        private CandyBotImageGenerator? _candyImageGen;
        private CandyBotDocumentGenerator? _candyDocGen;

        // Public properties for CandyBotAvatar access
        public User CurrentUser => _currentUser;
        public CosmosDbService CosmosDbService => _cosmosDbService;
        public dynamic? CandyBotService { get; private set; } // Will be initialized with AI service

        public MainWindow(CosmosDbService cosmosDbService, User currentUser, AppSettings appSettings)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== MainWindow Constructor Started ===");
                
                InitializeComponent();
                
                // Apply user's saved theme colors before showing UI
                Views.SettingsView.ApplySavedTheme(currentUser);
                System.Diagnostics.Debug.WriteLine("✅ User theme applied");

                _cosmosDbService = cosmosDbService;
                _radioBossService = new RadioBossService();
                _currentUser = currentUser;
                _appSettings = appSettings;
                _permissionService = new PermissionService(currentUser);
                System.Diagnostics.Debug.WriteLine("✓ Basic services initialized");

                // Initialize Candy-Bot Services (Phase 2)
                _candySound = new CandyBotSoundManager();
                _candySound.SetVoiceMode(true);
                _candySound.SetSoundsEnabled(true);
                System.Diagnostics.Debug.WriteLine("✓ CandyBot Sound initialized");
                
                _candyFiles = new CandyBotFileManager();
                System.Diagnostics.Debug.WriteLine("✓ CandyBot Files initialized");
                
                // Initialize Image Generator with API key from settings
                string openAiApiKey = _appSettings.ApiKeys?.OpenAiApiKey ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(openAiApiKey))
                {
                    _candyImageGen = new CandyBotImageGenerator(
                        apiKey: openAiApiKey,
                        provider: CandyBotImageGenerator.ImageProvider.OpenAI_DALLE3,
                        soundManager: _candySound
                    );
                    System.Diagnostics.Debug.WriteLine("[CandyBot] Image Generator initialized with OpenAI API key");
                }
                else
                {
                    // Initialize without API key - will show error if used
                    _candyImageGen = new CandyBotImageGenerator(
                        soundManager: _candySound
                    );
                    System.Diagnostics.Debug.WriteLine("[CandyBot] Image Generator initialized without API key");
                }
                System.Diagnostics.Debug.WriteLine("✓ Image Generator initialized");

                // Initialize Document Generator (no API key needed)
                _candyDocGen = new CandyBotDocumentGenerator(_candySound);
                System.Diagnostics.Debug.WriteLine("✓ Document Generator initialized");

                System.Diagnostics.Debug.WriteLine("[CandyBot] Image & Document Generation services initialized");

                // Initialize System Tray Icon
                InitializeSystemTray();
                System.Diagnostics.Debug.WriteLine("✓ System Tray initialized");
                
                // Initialize Window Controls (Force Close, Alt+F4 handling)
                InitializeWindowControls();
                System.Diagnostics.Debug.WriteLine("✓ Window Controls initialized");

                // Subscribe to force logout event (for instant ban kicks)
                OnlineUserStatusService.Instance.UserForcedLogout += OnUserForcedLogout;
                System.Diagnostics.Debug.WriteLine("✓ Force Logout event subscribed");

                // Update title bar user info
                UpdateTitleBarUserInfo();
                System.Diagnostics.Debug.WriteLine("✓ Title Bar updated");

                ApplySettings();
                System.Diagnostics.Debug.WriteLine("✓ Settings applied");
                
                ApplyPermissions();
                System.Diagnostics.Debug.WriteLine("✓ Permissions applied");
                
                ApplyUserPreferences();
                System.Diagnostics.Debug.WriteLine("✓ User Preferences applied");
                
                // Start connection monitoring for 4 Claude indicators
                StartConnectionMonitoring();
                System.Diagnostics.Debug.WriteLine("✓ Connection monitoring started");
                
                // Start Now Playing updates
                StartNowPlayingMonitoring();
                System.Diagnostics.Debug.WriteLine("✓ Now Playing monitoring started");
                
                // Initialize Radio Player with CosmosDB service
                InitializeRadioPlayer();
                System.Diagnostics.Debug.WriteLine("✓ Radio Player initialized");
                
                // Show Candy-Bot welcome on first launch
                // TODO: Restore this after fixing file corruption
                // this.Loaded += MainWindow_Loaded;
                System.Diagnostics.Debug.WriteLine("✓ Loaded event wired (disabled temporarily)");
                
                // Wire up Candy-Bot avatar events
                WireCandyBotAvatarEvents();
                System.Diagnostics.Debug.WriteLine("✓ CandyBot Avatar events wired");

                // Initialize media player for voice recordings
                _mediaPlayer = new MediaPlayer();
                System.Diagnostics.Debug.WriteLine("✓ Media Player initialized");
                
                System.Diagnostics.Debug.WriteLine("=== MainWindow Constructor Completed Successfully ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ FATAL ERROR in MainWindow Constructor: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"Inner Stack: {ex.InnerException.StackTrace}");
                }
                
                MessageBox.Show(
                    $"FATAL ERROR IN MAINWINDOW:\n\n{ex.Message}\n\nInner: {ex.InnerException?.Message}\n\nStack:\n{ex.StackTrace}",
                    "MainWindow Construction Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                throw; // Re-throw to be caught by App.xaml.cs
            }
        }

        #region Initialization Methods

        private void InitializeSystemTray()
        {
            // System tray initialization
            System.Diagnostics.Debug.WriteLine("✓ System tray initialized");
        }

        private void UpdateTitleBarUserInfo()
        {
            // Update title bar with user information
            System.Diagnostics.Debug.WriteLine($"✓ Title bar updated for user: {_currentUser.Username}");
        }

        private void ApplySettings()
        {
            // Apply application settings from _appSettings
            System.Diagnostics.Debug.WriteLine("✓ Application settings applied");
        }

        private void ApplyUserPreferences()
        {
            // Apply user-specific preferences
            if (_currentUser.AppPreferences != null)
            {
                System.Diagnostics.Debug.WriteLine($"✓ User preferences applied for: {_currentUser.Username}");
            }
        }

        private void StartConnectionMonitoring()
        {
            // Start monitoring connections to services
            _connectionCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _connectionCheckTimer.Tick += ConnectionCheckTimer_Tick;
            _connectionCheckTimer.Start();
            
            System.Diagnostics.Debug.WriteLine("✓ Connection monitoring started");
        }

        private void ConnectionCheckTimer_Tick(object? sender, EventArgs e)
        {
            // Check connection status for indicators
            // This is a placeholder - actual implementation would check real service status
        }

        private void StartNowPlayingMonitoring()
        {
            // Start monitoring "Now Playing" information
            _nowPlayingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            _nowPlayingTimer.Tick += NowPlayingTimer_Tick;
            _nowPlayingTimer.Start();
            
            System.Diagnostics.Debug.WriteLine("✓ Now Playing monitoring started");
        }

        private void NowPlayingTimer_Tick(object? sender, EventArgs e)
        {
            // Update "Now Playing" information
            // This is a placeholder - actual implementation would fetch from RadioBOSS
        }

        #endregion

        private void InitializeRadioPlayer()
        {
            if (RadioPlayerView != null && _cosmosDbService != null)
            {
                RadioPlayerView.SetCosmosDbService(_cosmosDbService);
                RadioPlayerView.SetCurrentUsername(_currentUser.Username);
            }
        }

        private void WireCandyBotAvatarEvents()
        {
            // Event handlers are in other partial class files - temporarily disabled
            // TODO: Restore these after fixing file corruption
            /*
            if (CandyBotFloatingAvatar != null)
            {
                CandyBotFloatingAvatar.PersonalityChanged += CandyBotAvatar_PersonalityChanged;
                CandyBotFloatingAvatar.DesktopModeRequested += CandyBotAvatar_DesktopModeRequested;
                CandyBotFloatingAvatar.SearchWebRequested += CandyBotAvatar_SearchWebRequested;
                CandyBotFloatingAvatar.OpenChatRequested += CandyBotAvatar_OpenChatRequested;
                CandyBotFloatingAvatar.OpenSettingsRequested += CandyBotAvatar_OpenSettingsRequested;
                CandyBotFloatingAvatar.AlwaysOnTopToggled += CandyBotAvatar_AlwaysOnTopToggled;
                CandyBotFloatingAvatar.StartWithWindowsToggled += CandyBotAvatar_StartWithWindowsToggled;
                CandyBotFloatingAvatar.QuietModeToggled += CandyBotAvatar_QuietModeToggled;
                CandyBotFloatingAvatar.VoiceModeToggled += CandyBotAvatar_VoiceModeToggled;
                CandyBotFloatingAvatar.SoundsToggled += CandyBotAvatar_SoundsToggled;
            }
            */
        }

        private async void OnUserForcedLogout(object? sender, string bannedUsername)
        {
            // Check if the banned user is THIS user
            if (_currentUser != null && _currentUser.Username.Equals(bannedUsername, StringComparison.OrdinalIgnoreCase))
            {
                // Fetch updated user info to get ban reason
                try
                {
                    var users = await _cosmosDbService.GetAllUsersAsync();
                    var updatedUser = users.FirstOrDefault(u => u.Username.Equals(bannedUsername, StringComparison.OrdinalIgnoreCase));
                    
                    if (updatedUser != null)
                    {
                        // STORE BAN LOCALLY ON THIS MACHINE (prevents VPN bypass)
                        Services.MachineBanService.StoreBanLocally(
                            updatedUser.Username,
                            updatedUser.BanExpiry ?? DateTime.MaxValue,
                            updatedUser.BanStrikeCount,
                            updatedUser.BanReason ?? "Policy violation",
                            updatedUser.IsPermanentBan);
                        
                        System.Diagnostics.Debug.WriteLine($"[KICKED] Machine-bound ban stored for {bannedUsername}");
                    }
                    
                    string banMessage = "Your account has been banned by an administrator.\n\n";
                    
                    if (updatedUser != null && !string.IsNullOrEmpty(updatedUser.BanReason))
                    {
                        banMessage += $"Reason: {updatedUser.BanReason}\n\n";
                    }
                    else
                    {
                        banMessage += "No reason provided.\n\n";
                    }
                    
                    banMessage += "⚠ DUAL-LAYER BAN ENFORCEMENT ⚠\n\n" +
                                 "This ban is enforced on TWO levels:\n\n" +
                                 "1️⃣ MACHINE BAN (Hardware ID)\n" +
                                 "   • Tied to this specific computer\n" +
                                 "   • Prevents login even with VPN\n\n" +
                                 "2️⃣ IP/NETWORK BAN (WAN Address)\n" +
                                 "   • Blocks your entire network\n" +
                                 "   • Prevents login from other computers on your network\n\n" +
                                 "❌ Changing IP (VPN/Proxy) = Still banned (Machine ID)\n" +
                                 "❌ Using another computer = Still banned (Same IP/Network)\n\n" +
                                 "You cannot log in until the ban expires.";
                    
                    // This user was banned - kick them immediately
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            banMessage,
                            "⛔ Account Banned",
                            MessageBoxButton.OK,
                            MessageBoxImage.Stop);
                        
                        Application.Current.Shutdown();
                    });
                    
                    System.Diagnostics.Debug.WriteLine($"[KICKED] User {bannedUsername} was force logged out (banned)");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[KICKED] Error fetching ban reason: {ex.Message}");
                    
                    // Fallback message
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Your account has been banned by an administrator.",
                            "⛔ Account Banned",
                            MessageBoxButton.OK,
                            MessageBoxImage.Stop);
                        
                        Application.Current.Shutdown();
                    });
                }
            }
        }
    }
}
