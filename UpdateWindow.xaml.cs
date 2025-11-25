using System;
using System.Diagnostics;
using System.Windows;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class UpdateWindow : Window
    {
        private ServerVersionInfo? _versionInfo;

        public UpdateWindow(ServerVersionInfo versionInfo)
        {
            InitializeComponent();
            _versionInfo = versionInfo;
            LoadUpdateInfo();
        }

        private void LoadUpdateInfo()
        {
            if (_versionInfo == null) return;

            // Set version information
            CurrentVersionText.Text = UpdateManager.CurrentVersion;
            NewVersionText.Text = _versionInfo.Version;
            ReleaseDateText.Text = _versionInfo.ReleaseDate;

            // Load features
            FeaturesListBox.ItemsSource = _versionInfo.Features;

            // Load bug fixes
            BugFixesListBox.ItemsSource = _versionInfo.BugFixes;

            // Show critical warning if needed
            if (_versionInfo.IsCritical ?? false)
            {
                TitleText.Text = "? Critical Update Required";
                TitleText.Foreground = System.Windows.Media.Brushes.Red;
                LaterButton.IsEnabled = false;
                LaterButton.Opacity = 0.5;
            }
        }

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            if (_versionInfo == null) return;

            // Show progress UI
            ButtonPanel.Visibility = Visibility.Collapsed;
            ProgressPanel.Visibility = Visibility.Visible;

            var progress = new Progress<int>(percent =>
            {
                Dispatcher.Invoke(() =>
                {
                    UpdateProgressBar.Value = percent;
                    ProgressPercentText.Text = $"{percent}%";

                    if (percent < 100)
                    {
                        ProgressText.Text = $"Downloading update... {percent}%";
                    }
                    else
                    {
                        ProgressText.Text = "Installing update...";
                    }
                });
            });

            try
            {
                // Convert ServerVersionInfo to UpdateCheckResult
                var updateCheckResult = new UpdateCheckResult
                {
                    UpdateAvailable = true,
                    LatestVersion = _versionInfo.LatestVersion ?? _versionInfo.Version ?? "",
                    ReleaseDate = _versionInfo.ReleaseDate,
                    DownloadUrl = _versionInfo.DownloadUrl,
                    ChangelogUrl = _versionInfo.ChangelogUrl,
                    Features = _versionInfo.Features ?? Array.Empty<string>(),
                    BugFixes = _versionInfo.BugFixes ?? Array.Empty<string>(),
                    IsCritical = _versionInfo.IsCritical ?? false,
                    MinimumVersion = _versionInfo.MinimumVersion
                };

                bool success = await UpdateManager.DownloadAndInstallUpdateAsync(updateCheckResult, progress);

                if (!success)
                {
                    MessageBox.Show(
                        "Update installation failed. Please try again later.",
                        "Update Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    // Reset UI
                    ButtonPanel.Visibility = Visibility.Visible;
                    ProgressPanel.Visibility = Visibility.Collapsed;
                }
                // If successful, application will restart automatically
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Update failed: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                // Reset UI
                ButtonPanel.Visibility = Visibility.Visible;
                ProgressPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void Later_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Changelog_Click(object sender, RoutedEventArgs e)
        {
            if (_versionInfo != null && !string.IsNullOrEmpty(_versionInfo.ChangelogUrl))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _versionInfo.ChangelogUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Could not open changelog: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}
