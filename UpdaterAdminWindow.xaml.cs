using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DJBookingSystem.Models;
using DJBookingSystem.Services;
using Microsoft.Win32;

namespace DJBookingSystem
{
    /// <summary>
    /// Admin-only window for managing updates and deploying to server
    /// </summary>
    public partial class UpdaterAdminWindow : Window
    {
        private readonly UpdateDeploymentService _deploymentService;
        private readonly SecureUpdateClient _secureClient;
        private readonly SftpDownloadService _sftpService;

        public UpdaterAdminWindow()
        {
            InitializeComponent();
            
            _deploymentService = new UpdateDeploymentService();
            _secureClient = new SecureUpdateClient();
            _sftpService = new SftpDownloadService();

            Loaded += UpdaterAdminWindow_Loaded;
        }

        private async void UpdaterAdminWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCurrentVersionInfo();
            await LoadSecurityStatus();
        }

        /// <summary>
        /// Load current version information
        /// </summary>
        private async Task LoadCurrentVersionInfo()
        {
            try
            {
                CurrentVersionText.Text = VersionInfo.VersionString;
                ReleaseDateText.Text = VersionInfo.ReleaseDate.ToString("yyyy-MM-dd HH:mm");
                
                #if DEBUG
                BuildTypeText.Text = "Debug";
                BuildTypeText.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(255, 193, 7)); // Warning yellow
                #else
                BuildTypeText.Text = "Release";
                BuildTypeText.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(40, 167, 69)); // Success green
                #endif

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading version info: {ex.Message}");
            }
        }

        /// <summary>
        /// Load security status
        /// </summary>
        private async Task LoadSecurityStatus()
        {
            try
            {
                if (UpdateManager.LastSecurityInfo != null)
                {
                    SecurityStatusText.Text = UpdateManager.LastSecurityInfo.GetDetailedSecurityInfo();
                }
                else
                {
                    SecurityStatusText.Text = "WARNING: No security information available. Run an update check first.";
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading security status: {ex.Message}");
                SecurityStatusText.Text = $"ERROR: Error loading security status: {ex.Message}";
            }
        }

        /// <summary>
        /// Test SSL connection to update server
        /// </summary>
        private async void TestSslConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SecurityStatusText.Text = "Testing SSL connection...";
                
                var result = await CertificateManager.TestSslConnectionAsync("https://c40.radioboss.fm/u/98");
                
                if (result.Success)
                {
                    SecurityStatusText.Text = $"SSL Connection Test Successful\n\n{result.GetSummary()}";
                    
                    if (result.Certificate != null)
                    {
                        var certInfo = new CertificateInfo
                        {
                            Subject = result.Certificate.Subject,
                            Issuer = result.Certificate.Issuer,
                            NotBefore = result.Certificate.NotBefore,
                            NotAfter = result.Certificate.NotAfter,
                            Sha256Fingerprint = CertificateManager.CalculateSha256Fingerprint(result.Certificate)
                        };

                        SecurityStatusText.Text += $"\n\nCertificate Details:\n{certInfo.GetValidityStatus()}\n" +
                                                  $"Fingerprint: {certInfo.Sha256Fingerprint}";
                    }
                }
                else
                {
                    SecurityStatusText.Text = $"SSL Connection Test Failed\n\n{result.GetSummary()}";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error testing SSL connection: {ex.Message}");
                SecurityStatusText.Text = $"ERROR: {ex.Message}";
            }
        }

        /// <summary>
        /// Browse for update file
        /// </summary>
        private void BrowseUpdateFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Update File",
                Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                UpdateFilePathTextBox.Text = dialog.FileName;
                
                // Auto-populate version from file if possible
                if (string.IsNullOrWhiteSpace(NewVersionTextBox.Text))
                {
                    try
                    {
                        var versionInfo = FileVersionInfo.GetVersionInfo(dialog.FileName);
                        if (!string.IsNullOrEmpty(versionInfo.FileVersion))
                        {
                            NewVersionTextBox.Text = versionInfo.FileVersion;
                        }
                    }
                    catch
                    {
                        // Ignore version extraction errors
                    }
                }
            }
        }

        /// <summary>
        /// Validate update file before deployment
        /// </summary>
        private async void ValidateUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(UpdateFilePathTextBox.Text))
                {
                    MessageBox.Show("Please select an update file first.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!File.Exists(UpdateFilePathTextBox.Text))
                {
                    MessageBox.Show("Selected file does not exist.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                UploadStatusText.Text = "Validating update file...";
                UploadStatusText.Visibility = Visibility.Visible;

                var result = await _deploymentService.ValidateUpdateFileAsync(UpdateFilePathTextBox.Text);

                if (result.IsValid)
                {
                    MessageBox.Show(
                        $"Update File Validation Successful!\n\n" +
                        $"File Size: {result.FileSize:N0} bytes\n" +
                        $"SHA256: {result.Sha256Hash}\n" +
                        $"Is Executable: {result.IsExecutable}\n" +
                        $"Has Digital Signature: {result.HasDigitalSignature}",
                        "Validation Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        $"Update File Validation Failed!\n\n" +
                        $"Errors:\n{string.Join("\n", result.ValidationErrors)}",
                        "Validation Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

                UploadStatusText.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error validating update: {ex.Message}");
                MessageBox.Show($"Error validating update file: {ex.Message}", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                UploadStatusText.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Deploy update to server
        /// </summary>
        private async void DeployUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(NewVersionTextBox.Text))
                {
                    MessageBox.Show("Please enter a version number.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(UpdateFilePathTextBox.Text))
                {
                    MessageBox.Show("Please select an update file.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!File.Exists(UpdateFilePathTextBox.Text))
                {
                    MessageBox.Show("Selected file does not exist.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Confirm deployment
                var result = MessageBox.Show(
                    $"WARNING: Deploy Update to Server?\n\n" +
                    $"Version: {NewVersionTextBox.Text}\n" +
                    $"File: {Path.GetFileName(UpdateFilePathTextBox.Text)}\n" +
                    $"Critical: {(IsCriticalCheckBox.IsChecked == true ? "Yes" : "No")}\n\n" +
                    $"This will make the update available to all users.\n\n" +
                    $"Continue with deployment?",
                    "Confirm Deployment",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                // Show progress
                UploadProgressBar.Visibility = Visibility.Visible;
                UploadStatusText.Visibility = Visibility.Visible;
                UploadProgressBar.Value = 0;

                var progress = new Progress<int>(percent =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        UploadProgressBar.Value = percent;
                        UploadStatusText.Text = $"Uploading... {percent}%";
                    });
                });

                // Deploy update
                var deploymentInfo = new UpdateDeploymentInfo
                {
                    Version = NewVersionTextBox.Text,
                    ReleaseNotes = ReleaseNotesTextBox.Text,
                    FilePath = UpdateFilePathTextBox.Text,
                    IsCritical = IsCriticalCheckBox.IsChecked == true,
                    ReleaseDate = DateTime.UtcNow
                };

                bool success = await _deploymentService.DeployUpdateAsync(deploymentInfo, progress);

                UploadProgressBar.Visibility = Visibility.Collapsed;
                UploadStatusText.Visibility = Visibility.Collapsed;

                if (success)
                {
                    MessageBox.Show(
                        $"Update Deployed Successfully!\n\n" +
                        $"Version {NewVersionTextBox.Text} is now available to all users.",
                        "Deployment Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Clear form
                    NewVersionTextBox.Clear();
                    ReleaseNotesTextBox.Clear();
                    UpdateFilePathTextBox.Clear();
                    IsCriticalCheckBox.IsChecked = false;
                }
                else
                {
                    MessageBox.Show(
                        "Update deployment failed. Please check the logs for details.",
                        "Deployment Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deploying update: {ex.Message}");
                MessageBox.Show($"Error deploying update: {ex.Message}", "Deployment Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                
                UploadProgressBar.Visibility = Visibility.Collapsed;
                UploadStatusText.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Test connection to update server
        /// </summary>
        private async void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ServerStatusText.Text = "Testing...";
                ServerStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(255, 193, 7)); // Yellow

                // Prompt admin for password (never hardcode!)
                var passwordWindow = new Window
                {
                    Title = "Server Authentication",
                    Width = 350,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(26, 26, 26))
                };
                
                var panel = new System.Windows.Controls.StackPanel { Margin = new Thickness(20) };
                panel.Children.Add(new System.Windows.Controls.TextBlock 
                { 
                    Text = "Enter SFTP server password:", 
                    Foreground = System.Windows.Media.Brushes.White,
                    Margin = new Thickness(0, 0, 0, 10)
                });
                
                var passwordBox = new System.Windows.Controls.PasswordBox 
                { 
                    Margin = new Thickness(0, 0, 0, 15),
                    Padding = new Thickness(5)
                };
                panel.Children.Add(passwordBox);
                
                var okButton = new System.Windows.Controls.Button 
                { 
                    Content = "OK", 
                    Width = 80,
                    IsDefault = true
                };
                okButton.Click += (s, args) => { passwordWindow.DialogResult = true; passwordWindow.Close(); };
                panel.Children.Add(okButton);
                
                passwordWindow.Content = panel;
                
                if (passwordWindow.ShowDialog() != true || string.IsNullOrEmpty(passwordBox.Password))
                {
                    ServerStatusText.Text = "Cancelled";
                    ServerStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(108, 117, 125)); // Gray
                    return;
                }
                
                bool connected = await _sftpService.TestConnectionAsync(passwordBox.Password);

                if (connected)
                {
                    ServerStatusText.Text = "Connected";
                    ServerStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(40, 167, 69)); // Green
                }
                else
                {
                    ServerStatusText.Text = "Connection Failed";
                    ServerStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(220, 53, 69)); // Red
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error testing connection: {ex.Message}");
                ServerStatusText.Text = "Error";
                ServerStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(220, 53, 69)); // Red
            }
        }

        /// <summary>
        /// View server logs
        /// </summary>
        private void ViewServerLogs_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Server logs viewer coming soon!\n\n" +
                "This will display deployment history, user update statistics, and error logs.",
                "Feature Coming Soon",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Refresh server status
        /// </summary>
        private async void RefreshStatus_Click(object sender, RoutedEventArgs e)
        {
            await LoadCurrentVersionInfo();
            await LoadSecurityStatus();
            // Manually trigger connection test
            TestConnection_Click(sender, e);
        }

        /// <summary>
        /// Rollback to previous update
        /// </summary>
        private void RollbackUpdate_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "DANGER: Rollback Update\n\n" +
                "This will revert the server to the previous version.\n" +
                "All users will receive the older version on next update check.\n\n" +
                "Are you absolutely sure you want to proceed?",
                "Confirm Rollback",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show(
                    "Rollback functionality will be implemented in next update.\n\n" +
                    "For now, manually upload the previous version as a new update.",
                    "Feature In Development",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Force update for all clients
        /// </summary>
        private void ForceUpdate_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "DANGER: Force Update All Clients\n\n" +
                "This will force ALL users to update immediately on next check.\n" +
                "Users will not be able to skip this update.\n\n" +
                "Only use this for critical security fixes!\n\n" +
                "Are you absolutely sure you want to proceed?",
                "Confirm Force Update",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show(
                    "Force update flag will be set with the next critical update deployment.\n\n" +
                    "Check the 'Mark as Critical Update' checkbox when deploying.",
                    "Force Update",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Handle title bar drag
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        /// <summary>
        /// Close window
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
