using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DJBookingSystem.Services;
using DJBookingSystem.Models;

namespace DJBookingSystem
{
    public partial class SplashScreen : Window
    {
        private Random _random = new Random();
        private double _currentProgress = 0;
        private DateTime _startTime;
        private TaskCompletionSource<bool> _completionSource = new TaskCompletionSource<bool>();
        private CosmosDbService? _cosmosService;
        
        // Connection status tracking - once green, stays green
        private bool _cosmosConnected = false;
        private bool _lpmConnected = false;
        private bool _radioBossConnected = false;
        private bool _streamConnected = false;
        
        private System.Windows.Threading.DispatcherTimer? _progressTimer;
        private System.Windows.Threading.DispatcherTimer? _messageTimer;

        // Public property to await splash screen completion
        public Task CompletionTask => _completionSource.Task;

        private readonly List<string> _humorousMessages = new List<string>
        {
            "Polishing the knob...",
            "Massaging the backend...",
            "Teaching hamsters to run faster...",
            "Bribing the loading bar...",
            "Convincing pixels to cooperate...",
            "Almost there... don't fake it now...",
            "Loading your DJ experience...",
            "Warming up the servers...",
            "Calibrating the awesome meter...",
            "Downloading more RAM...",
            "Tuning the beats...",
            "Sprinkling magic dust...",
            "Waking up the DJs...",
            "Preparing the party...",
            "Charging the lasers...",
            "Mixing the perfect playlist..."
        };

        public SplashScreen(CosmosDbService? cosmosService = null)
        {
            InitializeComponent();
            _cosmosService = cosmosService;
    
            HumorousMessageTextBlock.Text = "Connecting to services...";
            HumorousMessageTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            HumorousMessageTextBlock.FontSize = 16;
    
            // Try to load image programmatically as fallback
            TryLoadLogoImage();

            // Set all Claude indicators to connecting (yellow)
            CosmosIndicator.SetConnecting();
            LPMIndicator.SetConnecting();
            RadioBossIndicator.SetConnecting();
            StreamIndicator.SetConnecting();

            Loaded += SplashScreen_Loaded;
            
            // Safety timeout - ensure splash completes even if something goes wrong
            var safetyTimer = new System.Windows.Threading.DispatcherTimer();
            safetyTimer.Interval = TimeSpan.FromSeconds(15); // 15 second max
            safetyTimer.Tick += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("Splash screen safety timeout triggered");
                safetyTimer.Stop();
                CompleteLoading();
            };
            safetyTimer.Start();

            // Set version from VersionInfo
            VersionTextBlock.Text = $"Version {VersionInfo.VersionString}";
        }

        private void TryLoadLogoImage()
        {
            try
            {
                // Try multiple URI formats
                string[] uriFormats = new[]
                {
                    "pack://application:,,,/PNG_Version.png",
                    "/PNG_Version.png",
                    "PNG_Version.png",
                    "pack://application:,,,/DJBookingSystem;component/PNG_Version.png"
                };

                foreach (var uriFormat in uriFormats)
                {
                    try
                    {
                        var uri = new Uri(uriFormat, UriKind.RelativeOrAbsolute);
                        var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = uri;
                        bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        
                        LogoImage.Source = bitmap;
                        System.Diagnostics.Debug.WriteLine($"Successfully loaded logo with URI: {uriFormat}");
                        return;
                    }
                    catch
                    {
                        // Try next format
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("Could not load logo image with any URI format");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading logo: {ex.Message}");
            }
        }

        private void Image_Failed(object sender, ExceptionRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Image loading failed: {e.ErrorException?.Message}");
            TryLoadLogoImage();
        }

        private async void SplashScreen_Loaded(object sender, RoutedEventArgs e)
        {
            _startTime = DateTime.Now;
            
            // Start progress monitoring
            StartProgressMonitoring();
            
            // Start connection checking
            await Task.Run(async () => await CheckAllConnectionsAsync());
            
            // Start message rotation
            StartMessageRotation();
        }

        private async Task CheckAllConnectionsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting connection checks...");
                
                // Check all 4 services in parallel
                var results = await ConnectionService.CheckAllConnectionsAsync(_cosmosService);
                
                // Update indicators and connection status
                await Dispatcher.InvokeAsync(() =>
                {
                    // Update Cosmos DB
                    _cosmosConnected = results.CosmosDbConnected;
                    CosmosIndicator.IsConnected = _cosmosConnected;
                    
                    // Update LPM
                    _lpmConnected = results.LivePartyMusicConnected;
                    LPMIndicator.IsConnected = _lpmConnected;
                    
                    // Update RadioBOSS Control
                    _radioBossConnected = results.RadioBossControlConnected;
                    RadioBossIndicator.IsConnected = _radioBossConnected;
                    
                    // Update Stream Status
                    _streamConnected = results.RadioBossStreamActive;
                    StreamIndicator.IsConnected = _streamConnected;
                    
                    System.Diagnostics.Debug.WriteLine($"Connection check complete:");
                    System.Diagnostics.Debug.WriteLine($"  Cosmos: {_cosmosConnected}");
                    System.Diagnostics.Debug.WriteLine($"  LPM.fm: {_lpmConnected}");
                    System.Diagnostics.Debug.WriteLine($"  RadioBOSS Control: {_radioBossConnected}");
                    System.Diagnostics.Debug.WriteLine($"  Stream Status: {_streamConnected}");
                    
                    // Update progress based on connections
                    UpdateProgressFromConnections();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking connections: {ex.Message}");
            }
        }

        private void StartProgressMonitoring()
        {
            _progressTimer = new System.Windows.Threading.DispatcherTimer 
            { 
                Interval = TimeSpan.FromMilliseconds(100) 
            };
            
            _progressTimer.Tick += (s, e) =>
            {
                // Calculate target progress based on connected services
                double targetProgress = CalculateTargetProgress();
                
                // Smoothly animate towards target
                if (_currentProgress < targetProgress)
                {
                    _currentProgress += 2.0; // Smooth animation speed
                    if (_currentProgress > targetProgress)
                    {
                        _currentProgress = targetProgress;
                    }
                }
                
                // Update UI
                UpdateProgress(_currentProgress);
                
                // Complete at 100%
                if (_currentProgress >= 100)
                {
                    _progressTimer.Stop();
                    
                    // Small delay before closing
                    var closeTimer = new System.Windows.Threading.DispatcherTimer 
                    { 
                        Interval = TimeSpan.FromMilliseconds(500) 
                    };
                    closeTimer.Tick += (s2, e2) =>
                    {
                        closeTimer.Stop();
                        _completionSource.TrySetResult(true);
                        
                        // Fade out animation
                        var fadeOut = (Storyboard)Resources["FadeOutAnimation"];
                        fadeOut.Completed += (s3, e3) => Close();
                        fadeOut.Begin(this);
                    };
                    closeTimer.Start();
                }
                // Safety timeout after 12 seconds
                else if (HasTimedOut())
                {
                    System.Diagnostics.Debug.WriteLine("Safety timeout reached - forcing completion");
                    _currentProgress = 100;
                    UpdateProgress(100);
                }
            };
            
            _progressTimer.Start();
        }

        private double CalculateTargetProgress()
        {
            // Each service contributes 25% to the total progress
            double progress = 0;
            
            if (_cosmosConnected) progress += 25;
            if (_lpmConnected) progress += 25;
            if (_radioBossConnected) progress += 25;
            if (_streamConnected) progress += 25;
            
            return progress;
        }

        private void UpdateProgressFromConnections()
        {
            double targetProgress = CalculateTargetProgress();
            System.Diagnostics.Debug.WriteLine($"Target progress: {targetProgress}% (Cosmos:{_cosmosConnected}, LPM:{_lpmConnected}, RadioBOSS:{_radioBossConnected}, Stream:{_streamConnected})");
        }

        private bool HasTimedOut()
        {
            // Allow 12 seconds for connections before forcing completion
            return (DateTime.Now - _startTime).TotalSeconds > 12;
        }

        private void UpdateProgress(double progress)
        {
            if (progress > 100) progress = 100;
            
            Dispatcher.Invoke(() =>
            {
                // Update percentage text
                PercentageTextBlock.Text = $"{(int)progress}%";
                
                // Get actual container width dynamically
                var container = ProgressBarFill.Parent as Border;
                if (container != null && container.ActualWidth > 0)
                {
                    // Calculate progress bar width based on actual container size
                    // Subtract 4 pixels for the 2px border on each side
                    double maxWidth = container.ActualWidth - 4;
                    double targetWidth = (progress / 100.0) * maxWidth;
                    
                    // Animate progress bar
                    var widthAnimation = new DoubleAnimation
                    {
                        To = targetWidth,
                        Duration = TimeSpan.FromMilliseconds(100),
                        EasingFunction = new QuadraticEase()
                    };
                    
                    ProgressBarFill.BeginAnimation(WidthProperty, widthAnimation);
                }
            });
        }

        private void StartMessageRotation()
        {
            _messageTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(2000)
            };
            
            _messageTimer.Tick += (s, e) =>
            {
                // Pick a random message
                var message = _humorousMessages[_random.Next(_humorousMessages.Count)];
                
                Dispatcher.Invoke(() =>
                {
                    HumorousMessageTextBlock.Text = message;
                });
            };
            
            _messageTimer.Start();
        }

        private void CompleteLoading()
        {
            _currentProgress = 100;
            UpdateProgress(100);
            _completionSource.TrySetResult(true);
            
            // Fade out animation
            var fadeOut = (Storyboard)Resources["FadeOutAnimation"];
            fadeOut.Completed += (s3, e3) => Close();
            fadeOut.Begin(this);
        }

        private void RadioBossIndicator_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
