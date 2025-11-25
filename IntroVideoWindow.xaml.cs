using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DJBookingSystem
{
    /// <summary>
    /// Window to play a random intro video before MainWindow
    /// Shows 8-second intro videos from Fallen Intro folder
    /// </summary>
    public partial class IntroVideoWindow : Window
    {
        private MediaElement? _mediaElement;
        private string _videoPath;
        private Action? _onComplete;

        public IntroVideoWindow(string videoPath, Action onComplete)
        {
            InitializeComponent();
            
            _videoPath = videoPath;
            _onComplete = onComplete;
            
            System.Diagnostics.Debug.WriteLine($"[IntroVideo] Playing: {Path.GetFileName(videoPath)}");
            
            // Window setup
            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
            this.Topmost = true;
            this.Background = new SolidColorBrush(Colors.Black);
            this.AllowsTransparency = false;
            
            // Create MediaElement
            _mediaElement = new MediaElement
            {
                Source = new Uri(videoPath, UriKind.Absolute),
                LoadedBehavior = MediaState.Manual,
                UnloadedBehavior = MediaState.Close,
                Stretch = Stretch.Uniform,
                StretchDirection = StretchDirection.Both
            };
            
            _mediaElement.MediaEnded += MediaElement_MediaEnded;
            _mediaElement.MediaFailed += MediaElement_MediaFailed;
            
            this.Content = _mediaElement;
            this.Loaded += IntroVideoWindow_Loaded;
            this.KeyDown += IntroVideoWindow_KeyDown;
        }

        private void IntroVideoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _mediaElement?.Play();
                System.Diagnostics.Debug.WriteLine("[IntroVideo] Playback started");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[IntroVideo] Error playing video: {ex.Message}");
                CloseAndContinue();
            }
        }

        private void MediaElement_MediaEnded(object? sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[IntroVideo] Video ended");
            CloseAndContinue();
        }

        private void MediaElement_MediaFailed(object? sender, ExceptionRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[IntroVideo] Media failed: {e.ErrorException?.Message}");
            CloseAndContinue();
        }

        private void IntroVideoWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Allow ESC or any key to skip
            if (e.Key == System.Windows.Input.Key.Escape || 
                e.Key == System.Windows.Input.Key.Space || 
                e.Key == System.Windows.Input.Key.Enter)
            {
                System.Diagnostics.Debug.WriteLine("[IntroVideo] Skipped by user");
                CloseAndContinue();
            }
        }

        private void CloseAndContinue()
        {
            try
            {
                _mediaElement?.Stop();
                _mediaElement?.Close();
                
                this.Close();
                
                _onComplete?.Invoke();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[IntroVideo] Error closing: {ex.Message}");
                _onComplete?.Invoke();
            }
        }

        /// <summary>
        /// Get a random intro video from the Fallen Intro folder
        /// </summary>
        public static string? GetRandomIntroVideo()
        {
            try
            {
                // Path to intro videos
                string introFolder = @"K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\Fallen Intro";
                
                if (!Directory.Exists(introFolder))
                {
                    System.Diagnostics.Debug.WriteLine($"[IntroVideo] Intro folder not found: {introFolder}");
                    return null;
                }
                
                // Get all video files
                var videoFiles = Directory.GetFiles(introFolder, "*.*")
                    .Where(f => f.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                               f.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) ||
                               f.EndsWith(".wmv", StringComparison.OrdinalIgnoreCase) ||
                               f.EndsWith(".mov", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                if (videoFiles.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("[IntroVideo] No video files found in intro folder");
                    return null;
                }
                
                // Pick random video
                Random random = new Random();
                string selectedVideo = videoFiles[random.Next(videoFiles.Count)];
                
                System.Diagnostics.Debug.WriteLine($"[IntroVideo] Selected: {Path.GetFileName(selectedVideo)} (from {videoFiles.Count} videos)");
                
                return selectedVideo;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[IntroVideo] Error selecting video: {ex.Message}");
                return null;
            }
        }
    }
}
