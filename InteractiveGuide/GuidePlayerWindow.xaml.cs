using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;

namespace DJBookingSystem.InteractiveGuide
{
    public partial class GuidePlayerWindow : Window
    {
        private DispatcherTimer _timer;
        private MediaPlayer _mediaPlayer;
        private bool _isPlaying = false;

        public GuidePlayerWindow(string audioPath)
        {
            InitializeComponent();
            
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.Open(new Uri(audioPath, UriKind.Absolute));
            _mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            _mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += Timer_Tick;
        }

        private void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            if (_mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                Dispatcher.Invoke(() =>
                {
                    ProgressSlider.Maximum = _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                    TotalTimeText.Text = FormatTime(_mediaPlayer.NaturalDuration.TimeSpan);
                    PlayButton.Content = "? Play";
                });
            }
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            _isPlaying = false;
            _timer.Stop();
            Dispatcher.Invoke(() =>
            {
                PlayButton.Content = "? Play";
                ProgressSlider.Value = 0;
            });
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_isPlaying && _mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                Dispatcher.Invoke(() =>
                {
                    ProgressSlider.Value = _mediaPlayer.Position.TotalSeconds;
                    CurrentTimeText.Text = FormatTime(_mediaPlayer.Position);
                });
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isPlaying)
            {
                _mediaPlayer.Pause();
                _timer.Stop();
                PlayButton.Content = "? Play";
            }
            else
            {
                _mediaPlayer.Play();
                _timer.Start();
                PlayButton.Content = "?? Pause";
            }
            _isPlaying = !_isPlaying;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Stop();
            _timer.Stop();
            PlayButton.Content = "? Play";
            _isPlaying = false;
            ProgressSlider.Value = 0;
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Position = TimeSpan.Zero;
            _mediaPlayer.Play();
            _timer.Start();
            PlayButton.Content = "?? Pause";
            _isPlaying = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Close();
            _timer.Stop();
            this.Close();
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                _mediaPlayer.Position = TimeSpan.FromSeconds(ProgressSlider.Value);
            }
        }

        private string FormatTime(TimeSpan time)
        {
            return string.Format("{0:D2}:{1:D2}", (int)time.TotalMinutes, time.Seconds);
        }
    }
}
