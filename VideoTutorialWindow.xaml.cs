using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DJBookingSystem
{
    public partial class VideoTutorialWindow : Window
    {
        private List<string> _videoQueue;
        private int _currentVideoIndex = 0;
        private bool _isMandatory;
        private Action? _onComplete;

        /// <summary>
        /// Constructor for single video
        /// </summary>
        public VideoTutorialWindow(string videoPath)
        {
            InitializeComponent();
            
            _videoQueue = new List<string> { videoPath };
            _currentVideoIndex = 0;
            _isMandatory = false;
            
            CloseButton.Visibility = Visibility.Visible;
            
            LoadAndPlayCurrentVideo();
            
            System.Diagnostics.Debug.WriteLine($"?? Playing tutorial video: {videoPath}");
        }

        /// <summary>
        /// Constructor for mandatory tutorial with multiple videos
        /// </summary>
        public VideoTutorialWindow(List<string> videoPaths, bool isMandatory = true, Action? onComplete = null)
        {
            InitializeComponent();
            
            _videoQueue = videoPaths;
            _currentVideoIndex = 0;
            _isMandatory = isMandatory;
            _onComplete = onComplete;
            
            // Hide skip button if mandatory
            CloseButton.Visibility = isMandatory ? Visibility.Collapsed : Visibility.Visible;
            InfoPanel.Visibility = Visibility.Visible;
            
            // Prevent closing if mandatory
            if (isMandatory)
            {
                this.Closing += VideoTutorialWindow_Closing;
            }
            
            UpdateVideoCounter();
            LoadAndPlayCurrentVideo();
            
            System.Diagnostics.Debug.WriteLine($"?? Starting mandatory tutorial: {videoPaths.Count} videos");
        }

        private void LoadAndPlayCurrentVideo()
        {
            if (_currentVideoIndex >= _videoQueue.Count)
            {
                System.Diagnostics.Debug.WriteLine("? All tutorial videos completed");
                CompleteTutorial();
                return;
            }

            string videoPath = _videoQueue[_currentVideoIndex];
            
            try
            {
                VideoPlayer.Source = new Uri(videoPath, UriKind.Absolute);
                VideoPlayer.Play();
                
                UpdateVideoCounter();
                
                System.Diagnostics.Debug.WriteLine($"?? Playing video {_currentVideoIndex + 1}/{_videoQueue.Count}: {videoPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error loading video: {ex.Message}");
                MessageBox.Show(
                    $"Error loading tutorial video:\n{ex.Message}\n\nPath: {videoPath}",
                    "Tutorial Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                // If mandatory and fails, try next video or close
                if (_isMandatory)
                {
                    _currentVideoIndex++;
                    LoadAndPlayCurrentVideo();
                }
                else
                {
                    Close();
                }
            }
        }

        private void UpdateVideoCounter()
        {
            if (_videoQueue.Count > 1)
            {
                VideoCounter.Text = $"Video {_currentVideoIndex + 1} of {_videoQueue.Count}";
            }
            else
            {
                VideoCounter.Visibility = Visibility.Collapsed;
            }
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"? Video {_currentVideoIndex + 1} completed");
            
            // Move to next video
            _currentVideoIndex++;
            
            if (_currentVideoIndex < _videoQueue.Count)
            {
                LoadAndPlayCurrentVideo();
            }
            else
            {
                CompleteTutorial();
            }
        }

        private void CompleteTutorial()
        {
            System.Diagnostics.Debug.WriteLine("?? Tutorial sequence completed");
            
            // Call completion callback
            _onComplete?.Invoke();
            
            // Now allow closing
            if (_isMandatory)
            {
                this.Closing -= VideoTutorialWindow_Closing;
            }
            
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isMandatory)
            {
                System.Diagnostics.Debug.WriteLine("?? Tutorial skipped by user");
                VideoPlayer.Stop();
                Close();
            }
        }

        private void VideoTutorialWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Prevent closing if mandatory and not completed
            if (_isMandatory && _currentVideoIndex < _videoQueue.Count)
            {
                e.Cancel = true;
                System.Diagnostics.Debug.WriteLine("?? Cannot close mandatory tutorial before completion");
                
                MessageBox.Show(
                    "?? Tutorial Required\n\n" +
                    "This training is mandatory for new users.\n" +
                    "Please watch all tutorial videos to continue.\n\n" +
                    $"Progress: {_currentVideoIndex + 1}/{_videoQueue.Count} videos",
                    "CandyBot Training",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Prevent Alt+F4 and other close shortcuts if mandatory
            if (_isMandatory && _currentVideoIndex < _videoQueue.Count)
            {
                if (e.Key == Key.F4 && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                {
                    e.Handled = true;
                    System.Diagnostics.Debug.WriteLine("?? Alt+F4 blocked during mandatory tutorial");
                }
                else if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    System.Diagnostics.Debug.WriteLine("?? Escape blocked during mandatory tutorial");
                }
            }
        }
    }
}
