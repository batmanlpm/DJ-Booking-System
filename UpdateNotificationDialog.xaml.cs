using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class UpdateNotificationDialog : Window
    {
        private readonly UpdateCheckResult _updateInfo;
        private readonly CandyBotUpdateService _updateService;
        private readonly UpdateSecurityInfo? _securityInfo;
        private readonly bool _forceDownload;

        public UpdateNotificationDialog(UpdateCheckResult updateInfo, UpdateSecurityInfo? securityInfo = null, bool forceDownload = false)
        {
            InitializeComponent();
            _updateInfo = updateInfo;
            _updateService = new CandyBotUpdateService();
            _securityInfo = securityInfo;
            _forceDownload = forceDownload;
            
            LoadUpdateInfo();
            
            // If forced download, hide cancel buttons and start download automatically
            if (_forceDownload)
            {
                this.Loaded += (s, e) =>
                {
                    HideCancelOptions();
                    _ = AutoDownloadAndInstallAsync();
                };
            }
        }

        private void HideCancelOptions()
        {
            // Find and hide Later button by scanning visual tree
            var laterButton = FindButtonByContent("LATER");
            if (laterButton != null)
                laterButton.Visibility = Visibility.Collapsed;
            
            // Make dialog non-closable
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.Topmost = true;
            
            // Disable close button (X)
            this.Closing += (s, e) =>
            {
                if (_forceDownload)
                {
                    e.Cancel = true; // Prevent closing
                }
            };
            
            System.Diagnostics.Debug.WriteLine("[UpdateDialog] FORCED DOWNLOAD MODE - No cancel options available");
        }

        private Button? FindButtonByContent(string content)
        {
            return FindVisualChildren<Button>(this)
                .FirstOrDefault(b => b.Content?.ToString() == content);
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                
                if (child is T t)
                    yield return t;

                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }

        private async System.Threading.Tasks.Task AutoDownloadAndInstallAsync()
        {
            await System.Threading.Tasks.Task.Delay(2000); // Brief delay to show dialog
            
            System.Diagnostics.Debug.WriteLine("[UpdateDialog] Starting automatic forced download...");
            
            // Automatically trigger download
            UpdateNow_Click(this, new RoutedEventArgs());
        }

        private void LoadUpdateInfo()
        {
            CurrentVersionTextBlock.Text = _updateInfo.CurrentVersion;
            NewVersionTextBlock.Text = _updateInfo.LatestVersion;
            ReleaseNotesTextBlock.Text = _updateInfo.ReleaseNotes;
            
            // Display security information if available
            if (_securityInfo != null)
            {
                System.Diagnostics.Debug.WriteLine($"Security Info: {_securityInfo.GetSecurityStatusMessage()}");
            }
            
            // Show forced download message
            if (_forceDownload)
            {
                Title = "CRITICAL UPDATE REQUIRED - Installing Automatically";
                
                // Update title text in UI
                var titleTextBlocks = FindVisualChildren<TextBlock>(this)
                    .Where(tb => tb.Text == "UPDATE AVAILABLE");
                
                foreach (var tb in titleTextBlocks)
                {
                    tb.Text = "CRITICAL UPDATE - INSTALLING AUTOMATICALLY";
                    tb.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private async void UpdateNow_Click(object sender, RoutedEventArgs e)
        {
            // Skip confirmation if forced download
            if (!_forceDownload)
            {
                var result = MessageBox.Show(
                    "The application will restart to install the update. Continue?",
                    "Confirm Update",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[UpdateDialog] FORCED DOWNLOAD - Skipping user confirmation");
            }

            try
            {
                // Find update button and show progress
                var updateButton = FindButtonByContent("UPDATE NOW");
                if (updateButton != null)
                    updateButton.Content = "Downloading...";
                
                System.Diagnostics.Debug.WriteLine("[UpdateDialog] Starting download...");
                
                // TODO: Implement actual download and installation
                // For now, simulate
                await System.Threading.Tasks.Task.Delay(1000);
                
                System.Diagnostics.Debug.WriteLine("[UpdateDialog] Download complete (simulated)");
                
                // Close dialog if not forced (forced will close on completion)
                if (!_forceDownload)
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateDialog] Error: {ex.Message}");
                MessageBox.Show($"Error downloading update:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Later_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    /// <summary>
    /// Simple progress dialog for update installation
    /// </summary>
    public class UpdateProgressDialog : Window
    {
        private readonly System.Windows.Controls.ProgressBar _progressBar;
        private readonly System.Windows.Controls.TextBlock _statusText;

        public UpdateProgressDialog()
        {
            Title = "Installing Update";
            Width = 400;
            Height = 150;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = System.Windows.Media.Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;

            var border = new System.Windows.Controls.Border
            {
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(10, 10, 10)),
                BorderBrush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0, 191, 255)),
                BorderThickness = new Thickness(3),
                CornerRadius = new System.Windows.CornerRadius(10),
                Padding = new Thickness(20)
            };

            var stack = new System.Windows.Controls.StackPanel();

            var title = new System.Windows.Controls.TextBlock
            {
                Text = "Installing Update...",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.LightBlue,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 15)
            };

            _progressBar = new System.Windows.Controls.ProgressBar
            {
                Height = 25,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Margin = new Thickness(0, 0, 0, 10)
            };

            _statusText = new System.Windows.Controls.TextBlock
            {
                Text = "Preparing...",
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.LightGray,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            stack.Children.Add(title);
            stack.Children.Add(_progressBar);
            stack.Children.Add(_statusText);

            border.Child = stack;
            Content = border;
        }

        public void UpdateProgress(int percentage, string message)
        {
            _progressBar.Value = percentage;
            _statusText.Text = message;
        }
    }
}
