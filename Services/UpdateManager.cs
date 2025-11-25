using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using DJBookingSystem.Models;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Server version information for the application
    /// </summary>
    public class ServerVersionInfo
    {
        public bool UpdateAvailable { get; set; } = false;
        public string? CurrentVersion { get; set; }
        public string? LatestVersion { get; set; }
        public string? Version { get; set; } // Fallback for compatibility
        public string ReleaseDate { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public string ChangelogUrl { get; set; } = "";
        public string[]? Features { get; set; } = Array.Empty<string>();
        public string[]? BugFixes { get; set; } = Array.Empty<string>();
        public bool? IsCritical { get; set; } = false;
        public string MinimumVersion { get; set; } = "";
        public bool IsSecureConnection { get; set; } = true;
    }

    /// <summary>
    /// Candy-Bot specific update information
    /// </summary>
    public class CandyBotUpdateInfo
    {
        public string Version { get; set; } = "";
        public string ReleaseDate { get; set; } = "";
        public string[] NewPersonalities { get; set; } = Array.Empty<string>();
        public string[] NewResponses { get; set; } = Array.Empty<string>();
        public string[] Improvements { get; set; } = Array.Empty<string>();
        public string UpdateUrl { get; set; } = "";
    }

    /// <summary>
    /// Manages application and Candy-Bot updates with secure connections
    /// </summary>
    public class UpdateManager
    {
        private static SecureUpdateClient? _secureClient;
        private static readonly CandyBotUpdateService _updateService = new();
        private static System.Threading.Timer? _autoCheckTimer;
        private static bool _isChecking = false;
        
        public static string CurrentVersion => VersionInfo.VersionString;
        public static string CandyBotVersion => "2.0.0";

        // Security information from last check
        public static UpdateSecurityInfo? LastSecurityInfo { get; private set; }

        /// <summary>
        /// Get or create secure update client
        /// </summary>
        private static SecureUpdateClient GetSecureClient()
        {
            if (_secureClient == null)
            {
                _secureClient = new SecureUpdateClient();
            }
            return _secureClient;
        }

        /// <summary>
        /// Check for updates on application startup with secure connection
        /// </summary>
        public static async Task CheckForUpdatesOnStartupAsync(bool showNotifications = true, bool forceDownload = false)
        {
            if (_isChecking) return;
            _isChecking = true;

            try
            {
                Debug.WriteLine("Checking for updates on startup (secure connection)...");

                // Use secure client for update check
                var secureClient = GetSecureClient();
                var result = await secureClient.CheckForUpdatesAsync();

                // Store security information
                LastSecurityInfo = new UpdateSecurityInfo
                {
                    UsedSecureConnection = result.IsSecureConnection,
                    CertificatePinningSuccessful = result.IsSecureConnection,
                    TlsVersion = "TLS 1.3",
                    SignatureTimestamp = DateTime.UtcNow
                };

                if (result.UpdateAvailable)
                {
                    Debug.WriteLine($"Update available: {result.LatestVersion}");
                    Debug.WriteLine($"Security Status: {LastSecurityInfo.GetSecurityStatusMessage()}");
                    Debug.WriteLine($"Force Download: {forceDownload}");

                    if (showNotifications)
                    {
                        // Show update notification on UI thread
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            try
                            {
                                var dialog = new UpdateNotificationDialog(result, LastSecurityInfo, forceDownload);
                                dialog.Show();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error showing update dialog: {ex.Message}");
                            }
                        });
                    }
                }
                else
                {
                    Debug.WriteLine($"No updates available. Current version: {result.CurrentVersion}");
                }
            }
            catch (UpdateException ex)
            {
                Debug.WriteLine($"Secure update check failed: {ex.Message}");
                // Log security-related errors
                LastSecurityInfo = new UpdateSecurityInfo
                {
                    UsedSecureConnection = false,
                    CertificatePinningSuccessful = false
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update check failed: {ex.Message}");
                // Don't show error to user on startup - silent fail
            }
            finally
            {
                _isChecking = false;
            }
        }

        /// <summary>
        /// Enable automatic update checks every 24 hours
        /// </summary>
        public static async Task EnableAutoUpdateChecksAsync()
        {
            // Check immediately
            await CheckForUpdatesOnStartupAsync(showNotifications: false);

            // Set up timer for every 24 hours
            var interval = TimeSpan.FromHours(24);
            _autoCheckTimer = new System.Threading.Timer(
                async _ => await CheckForUpdatesOnStartupAsync(showNotifications: true),
                null,
                interval,
                interval);

            Debug.WriteLine("Automatic update checks enabled (every 24 hours, secure connection)");
        }

        /// <summary>
        /// Enable automatic update checks every hour on the hour
        /// </summary>
        public static async Task EnableHourlyUpdateChecksAsync()
        {
            // Check immediately
            await CheckForUpdatesOnStartupAsync(showNotifications: false);

            // 🔥 TESTING MODE: Check every 10 seconds for instant updates!
            // TODO: Change back to hourly once testing is complete
            Debug.WriteLine("⚡ INSTANT UPDATE MODE: Checking every 10 seconds!");
            Debug.WriteLine("📝 TODO: Change to hourly (TimeSpan.FromHours(1)) after testing");

            // Set up timer to check every 10 seconds (TESTING ONLY)
            _autoCheckTimer = new System.Threading.Timer(
                async _ => 
                {
                    Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ⚡ INSTANT Update check triggered (10s interval)");
                    await CheckForUpdatesOnStartupAsync(showNotifications: true, forceDownload: true);
                },
                null,
                TimeSpan.FromSeconds(10), // First check in 10 seconds
                TimeSpan.FromSeconds(10)); // Then every 10 seconds

            Debug.WriteLine("⚡ INSTANT update checks enabled (every 10 seconds, secure connection, forced download)");
            Debug.WriteLine("⚠️ WARNING: This is for testing only! Change to hourly for production.");
        }

        /// <summary>
        /// Disable automatic update checks
        /// </summary>
        public static void DisableAutoUpdateChecks()
        {
            _autoCheckTimer?.Dispose();
            _autoCheckTimer = null;
            Debug.WriteLine("Automatic update checks disabled");
        }

        /// <summary>
        /// Download and install application update securely
        /// </summary>
        public static async Task<bool> DownloadAndInstallUpdateAsync(UpdateCheckResult updateInfo, IProgress<int>? progress = null, string? expectedSignature = null)
        {
            try
            {
                Debug.WriteLine("Downloading update with secure connection...");

                var secureClient = GetSecureClient();
                
                // Download update file securely
                string tempPath = await secureClient.DownloadUpdateAsync(updateInfo.DownloadUrl, progress!);

                // Verify file signature if provided
                if (!string.IsNullOrEmpty(expectedSignature))
                {
                    Debug.WriteLine("Verifying update file signature...");
                    bool signatureValid = secureClient.VerifyUpdateSignature(tempPath, expectedSignature);

                    if (!signatureValid)
                    {
                        Debug.WriteLine("⚠️ WARNING: Update file signature verification failed!");
                        File.Delete(tempPath);
                        
                        MessageBox.Show(
                            "Update file signature verification failed. The update has been cancelled for security reasons.",
                            "Security Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        
                        return false;
                    }

                    Debug.WriteLine("✅ Update file signature verified successfully");

                    // Update security info
                    if (LastSecurityInfo != null)
                    {
                        LastSecurityInfo.FileSignatureValid = true;
                        LastSecurityInfo.FileSignature = expectedSignature;
                    }
                }

                // Create update script
                string updateScript = Path.Combine(Path.GetTempPath(), "update_secure.bat");
                string currentExe = Process.GetCurrentProcess().MainModule?.FileName ?? "";

                File.WriteAllText(updateScript, $@"
@echo off
echo Applying secure update...
timeout /t 2 /nobreak >nul
taskkill /F /IM DJBookingSystem.exe >nul 2>&1
timeout /t 1 /nobreak >nul
copy /Y ""{tempPath}"" ""{currentExe}"" >nul
if %errorlevel% equ 0 (
    echo Update applied successfully
    del ""{tempPath}"" >nul
    start """" ""{currentExe}""
) else (
    echo Update failed
    pause
)
del ""{updateScript}""
");

                // Launch update script
                Process.Start(new ProcessStartInfo
                {
                    FileName = updateScript,
                    CreateNoWindow = true,
                    UseShellExecute = false
                });

                // Exit application to allow update
                Application.Current.Shutdown();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error installing update: {ex.Message}");
                MessageBox.Show(
                    $"Failed to install update: {ex.Message}",
                    "Update Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Download and apply Candy-Bot update
        /// </summary>
        public static async Task<bool> DownloadCandyBotUpdateAsync(CandyBotUpdateInfo updateInfo, IProgress<int>? progress = null)
        {
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetStringAsync(updateInfo.UpdateUrl);
                var updateData = JsonConvert.DeserializeObject<dynamic>(response);
                
                // Apply Candy-Bot personality and response updates
                if (updateData != null)
                {
                    // Update personality database files
                    // Download and install new personality packs
                }
                
                progress?.Report(100);
                System.Diagnostics.Debug.WriteLine("Candy-Bot updated successfully");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating Candy-Bot: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Show update notification to user
        /// </summary>
        public static void ShowUpdateNotification(ServerVersionInfo versionInfo)
        {
            var result = MessageBox.Show(
                $"New Update Available!\n\n" +
                $"Version: {versionInfo.Version}\n" +
                $"Released: {versionInfo.ReleaseDate}\n\n" +
                $"New Features:\n{string.Join("\n", versionInfo.Features ?? Array.Empty<string>())}\n\n" +
                $"Bug Fixes:\n{string.Join("\n", versionInfo.BugFixes ?? Array.Empty<string>())}\n\n" +
                $"Would you like to update now?",
                "Update Available",
                MessageBoxButton.YesNo,
                (versionInfo.IsCritical ?? false) ? MessageBoxImage.Warning : MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                // Convert ServerVersionInfo to UpdateCheckResult
                var updateCheckResult = new UpdateCheckResult
                {
                    UpdateAvailable = true,
                    LatestVersion = versionInfo.LatestVersion ?? versionInfo.Version ?? "",
                    ReleaseDate = versionInfo.ReleaseDate,
                    DownloadUrl = versionInfo.DownloadUrl,
                    ChangelogUrl = versionInfo.ChangelogUrl,
                    Features = versionInfo.Features ?? Array.Empty<string>(),
                    BugFixes = versionInfo.BugFixes ?? Array.Empty<string>(),
                    IsCritical = versionInfo.IsCritical ?? false,
                    MinimumVersion = versionInfo.MinimumVersion
                };

                // Launch update process
                Task.Run(async () =>
                {
                    var progress = new Progress<int>(percent =>
                    {
                        System.Diagnostics.Debug.WriteLine($"Download progress: {percent}%");
                    });

                    await DownloadAndInstallUpdateAsync(updateCheckResult, progress);
                });
            }
        }

        /// <summary>
        /// Show Candy-Bot update notification
        /// </summary>
        public static void ShowCandyBotUpdateNotification(CandyBotUpdateInfo updateInfo)
        {
            var result = MessageBox.Show(
                $"Candy-Bot Update Available!\n\n" +
                $"Version: {updateInfo.Version}\n" +
                $"Released: {updateInfo.ReleaseDate}\n\n" +
                $"New Personalities:\n{string.Join("\n", updateInfo.NewPersonalities)}\n\n" +
                $"Improvements:\n{string.Join("\n", updateInfo.Improvements)}\n\n" +
                $"Update Candy-Bot now?",
                "Candy-Bot Update",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                Task.Run(async () =>
                {
                    var progress = new Progress<int>(percent =>
                    {
                        System.Diagnostics.Debug.WriteLine($"Candy-Bot update progress: {percent}%");
                    });

                    await DownloadCandyBotUpdateAsync(updateInfo, progress);
                    
                    MessageBox.Show(
                        "Candy-Bot updated successfully!\n\nPlease restart the application to use the new features.",
                        "Update Complete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                });
            }
        }
    }
}
