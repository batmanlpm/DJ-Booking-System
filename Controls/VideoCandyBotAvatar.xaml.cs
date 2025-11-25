using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DJBookingSystem.Models;

namespace DJBookingSystem.Controls
{
    public partial class VideoCandyBotAvatar : UserControl
    {
        private Storyboard? _glowPulseAnimation;
        private CandyBotState _currentState = CandyBotState.Idle;
        private bool _isVideoLoaded = false;

        public event RoutedEventHandler? AvatarClicked;
        public event EventHandler<CandyBotPersonalityMode>? PersonalityChanged;
        public event EventHandler? DesktopModeRequested;
        public event EventHandler? SearchWebRequested;
        public event EventHandler? OpenChatRequested;
        public event EventHandler? OpenSettingsRequested;

        public VideoCandyBotAvatar()
        {
            InitializeComponent();
            this.Loaded += VideoCandyBotAvatar_Loaded;
            this.MouseLeftButtonDown += VideoCandyBotAvatar_MouseLeftButtonDown;
        }

        private void VideoCandyBotAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load glow pulse animation
                _glowPulseAnimation = (Storyboard)this.Resources["GlowPulseAnimation"];
                _glowPulseAnimation?.Begin();

                // Start video playback
                VideoPlayer.Play();
                
                System.Diagnostics.Debug.WriteLine("? VideoCandyBotAvatar loaded and video playback started");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error loading VideoCandyBotAvatar: {ex.Message}");
            }
        }

        #region Video Events

        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            _isVideoLoaded = true;
            System.Diagnostics.Debug.WriteLine("? Video media opened successfully");
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Don't loop - just stop at the end
            VideoPlayer.Stop();
            System.Diagnostics.Debug.WriteLine("?? Video playback ended (no loop)");
        }

        private void VideoPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"? Video playback failed: {e.ErrorException?.Message}");
            SetState(CandyBotState.Error);
        }

        #endregion

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
                    VideoPlayer.SpeedRatio = 1.0;
                    break;
                    
                case CandyBotState.Thinking:
                    StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0)); // Yellow
                    VideoPlayer.SpeedRatio = 0.8; // Slow down slightly
                    break;
                    
                case CandyBotState.Speaking:
                    StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 255)); // Cyan
                    VideoPlayer.SpeedRatio = 1.2; // Speed up slightly
                    break;
                    
                case CandyBotState.Error:
                    StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0)); // Red
                    VideoPlayer.SpeedRatio = 0.5;
                    break;
                    
                case CandyBotState.Offline:
                    StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(128, 128, 128)); // Gray
                    VideoPlayer.Pause();
                    break;
            }
            
            System.Diagnostics.Debug.WriteLine($"?? Candy-Bot State: {state}");
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
                _ => Color.FromRgb(255, 105, 180)                                  // Hot Pink (Normal)
            };
            
            OuterGlow.Stroke = new SolidColorBrush(glowColor);
            System.Diagnostics.Debug.WriteLine($"?? Avatar updated for {mode} mode");
        }

        #endregion

        #region Mouse Events

        private void Avatar_MouseEnter(object sender, MouseEventArgs e)
        {
            // Increase glow intensity on hover
            OuterGlow.StrokeThickness = 4;
        }

        private void Avatar_MouseLeave(object sender, MouseEventArgs e)
        {
            // Reset glow thickness
            OuterGlow.StrokeThickness = 3;
        }

        private void Avatar_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Context menu will open automatically
            e.Handled = true;
        }

        private void VideoCandyBotAvatar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Play bounce animation
            var bounceAnimation = (Storyboard)this.Resources["ActiveBounceAnimation"];
            bounceAnimation?.Begin();
            
            // Raise click event
            AvatarClicked?.Invoke(this, new RoutedEventArgs());
            
            System.Diagnostics.Debug.WriteLine("?? Candy-Bot clicked!");
        }

        #endregion

        #region Context Menu Handlers

        // Personality Mode Changes
        private void SetPersonality_Normal(object sender, RoutedEventArgs e)
        {
            PersonalityChanged?.Invoke(this, CandyBotPersonalityMode.Normal);
            UpdateAvatarForPersonality(CandyBotPersonalityMode.Normal);
        }

        private void SetPersonality_Shy(object sender, RoutedEventArgs e)
        {
            PersonalityChanged?.Invoke(this, CandyBotPersonalityMode.Shy);
            UpdateAvatarForPersonality(CandyBotPersonalityMode.Shy);
        }

        private void SetPersonality_Funny(object sender, RoutedEventArgs e)
        {
            PersonalityChanged?.Invoke(this, CandyBotPersonalityMode.Funny);
            UpdateAvatarForPersonality(CandyBotPersonalityMode.Funny);
        }

        private void SetPersonality_Professional(object sender, RoutedEventArgs e)
        {
            PersonalityChanged?.Invoke(this, CandyBotPersonalityMode.Professional);
            UpdateAvatarForPersonality(CandyBotPersonalityMode.Professional);
        }

        // Quick Actions
        private void DesktopMode_Click(object sender, RoutedEventArgs e)
        {
            DesktopModeRequested?.Invoke(this, EventArgs.Empty);
        }

        private void SearchWeb_Click(object sender, RoutedEventArgs e)
        {
            SearchWebRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OpenChat_Click(object sender, RoutedEventArgs e)
        {
            OpenChatRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            OpenSettingsRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Play the video if paused
        /// </summary>
        public void Play()
        {
            if (_isVideoLoaded)
            {
                VideoPlayer.Play();
            }
        }

        /// <summary>
        /// Pause the video
        /// </summary>
        public void Pause()
        {
            VideoPlayer.Pause();
        }

        /// <summary>
        /// Set video volume (0.0 to 1.0)
        /// </summary>
        public void SetVolume(double volume)
        {
            VideoPlayer.Volume = Math.Clamp(volume, 0.0, 1.0);
        }

        /// <summary>
        /// Mute/unmute video
        /// </summary>
        public void SetMuted(bool muted)
        {
            VideoPlayer.IsMuted = muted;
        }

        #endregion
    }
}
