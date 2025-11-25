using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using DJBookingSystem.Models;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Manages CandyBot updates - checking, downloading, and installing
    /// </summary>
    public class CandyBotUpdateService
    {
        private const string UPDATE_SERVER_URL = "https://fallencollective.livepartymusic.fm/downloads";
        private const string VERSION_CHECK_URL = "https://fallencollective.livepartymusic.fm/version.json";
        
        private static readonly string LocalAppData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CandyBot");
        
        private static readonly string UpdateCachePath = Path.Combine(LocalAppData, "Updates");
        private static readonly string BackupPath = Path.Combine(LocalAppData, "Backup");
        
        public event EventHandler<UpdateEventArgs>? UpdateAvailable;
        public event EventHandler<UpdateProgressEventArgs>? UpdateProgress;
        public event EventHandler<UpdateCompletedEventArgs>? UpdateCompleted;

        /// <summary>
        /// Get current installed version
        /// </summary>
        public static Version GetCurrentVersion()
        {
            return Models.VersionInfo.CurrentVersion;
        }

        /// <summary>
        /// Get current version string
        /// </summary>
        public static string GetCurrentVersionString()
        {
            return Models.VersionInfo.VersionString;
        }

        /// <summary>
        /// Check for updates (simplified method for desktop widget)
        /// </summary>
        public async Task<UpdateCheckResult> CheckForUpdatesAsync()
        {
            try
            {
                var updateInfo = await CheckForUpdatesInternalAsync();
                
                if (updateInfo != null && updateInfo.IsUpdateAvailable)
                {
                    return new UpdateCheckResult
                    {
                        UpdateAvailable = true,
                        CurrentVersion = GetCurrentVersion().ToString(),
                        LatestVersion = updateInfo.Version,
                        ReleaseNotes = updateInfo.ReleaseNotes ?? "Bug fixes and improvements",
                        DownloadUrl = updateInfo.DownloadUrl,
                        ReleaseDate = updateInfo.ReleaseDate.ToString("s"),
                        ChangelogUrl = updateInfo.ChangelogUrl,
                        Features = updateInfo.Features,
                        BugFixes = updateInfo.BugFixes,
                        IsCritical = updateInfo.IsCritical,
                        MinimumVersion = updateInfo.MinimumVersion,
                        IsSecureConnection = updateInfo.IsSecureConnection
                    };
                }
                
                return new UpdateCheckResult
                {
                    UpdateAvailable = false,
                    CurrentVersion = GetCurrentVersion().ToString(),
                    LatestVersion = GetCurrentVersion().ToString(),
                    ReleaseNotes = ""
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update check failed: {ex.Message}");
                return new UpdateCheckResult
                {
                    UpdateAvailable = false,
                    CurrentVersion = GetCurrentVersion().ToString(),
                    LatestVersion = GetCurrentVersion().ToString(),
                    ReleaseNotes = ""
                };
            }
        }

        /// <summary>
        /// Download and install update (wrapper for UpdateInfo)
        /// </summary>
        public async Task DownloadAndInstallUpdateAsync()
        {
            try
            {
                var updateInfo = await CheckForUpdatesInternalAsync();
                if (updateInfo != null)
                {
                    await DownloadAndInstallUpdateAsync(updateInfo);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update installation failed: {ex.Message}");
                MessageBox.Show($"Failed to install update:\n{ex.Message}", "Update Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Check if updates are available (internal method)
        /// </summary>
        private async Task<UpdateInfo?> CheckForUpdatesInternalAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                
                var response = await client.GetAsync(VERSION_CHECK_URL);
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var updateInfo = JsonSerializer.Deserialize<UpdateInfo>(json);
                
                if (updateInfo != null)
                {
                    var currentVersion = GetCurrentVersion();
                    var latestVersion = Version.Parse(updateInfo.Version);
                    
                    if (latestVersion > currentVersion)
                    {
                        updateInfo.IsUpdateAvailable = true;
                        UpdateAvailable?.Invoke(this, new UpdateEventArgs(updateInfo));
                        return updateInfo;
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update check failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Download and install update
        /// </summary>
        public async Task<bool> DownloadAndInstallUpdateAsync(UpdateInfo updateInfo)
        {
            try
            {
                // Create update cache directory
                Directory.CreateDirectory(UpdateCachePath);
                Directory.CreateDirectory(BackupPath);
                
                // Download update package
                ReportProgress(0, "Downloading update...");
                var downloadPath = await DownloadUpdatePackageAsync(updateInfo.DownloadUrl);
                
                if (string.IsNullOrEmpty(downloadPath))
                {
                    ReportProgress(0, "Download failed");
                    return false;
                }
                
                // Backup current installation
                ReportProgress(30, "Backing up current version...");
                BackupCurrentInstallation();
                
                // Extract update
                ReportProgress(50, "Extracting update...");
                await ExtractUpdateAsync(downloadPath);
                
                // Apply update
                ReportProgress(70, "Installing update...");
                await ApplyUpdateAsync();
                
                // Verify installation
                ReportProgress(90, "Verifying installation...");
                bool verified = VerifyInstallation(updateInfo.Version);
                
                if (verified)
                {
                    ReportProgress(100, "Update completed successfully!");
                    
                    // Clean up
                    CleanupUpdateFiles();
                    
                    UpdateCompleted?.Invoke(this, new UpdateCompletedEventArgs(true, updateInfo));
                    return true;
                }
                else
                {
                    // Rollback on verification failure
                    ReportProgress(0, "Verification failed, rolling back...");
                    await RollbackUpdateAsync();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update installation failed: {ex.Message}");
                
                // Attempt rollback
                try
                {
                    await RollbackUpdateAsync();
                }
                catch (Exception rollbackEx)
                {
                    Debug.WriteLine($"Rollback failed: {rollbackEx.Message}");
                }
                
                UpdateCompleted?.Invoke(this, new UpdateCompletedEventArgs(false, updateInfo, ex.Message));
                return false;
            }
        }

        /// <summary>
        /// Download update package
        /// </summary>
        private async Task<string?> DownloadUpdatePackageAsync(string downloadUrl)
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(10);
                
                var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                
                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                var downloadPath = Path.Combine(UpdateCachePath, "update.zip");
                
                using var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var contentStream = await response.Content.ReadAsStreamAsync();
                
                var buffer = new byte[8192];
                long totalRead = 0;
                int bytesRead;
                
                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalRead += bytesRead;
                    
                    if (totalBytes > 0)
                    {
                        var progress = (int)((totalRead * 30) / totalBytes);
                        ReportProgress(progress, $"Downloading... {totalRead / 1024 / 1024}MB / {totalBytes / 1024 / 1024}MB");
                    }
                }
                
                return downloadPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Download failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Backup current installation
        /// </summary>
        private void BackupCurrentInstallation()
        {
            try
            {
                // Clear old backup
                if (Directory.Exists(BackupPath))
                {
                    Directory.Delete(BackupPath, true);
                }
                Directory.CreateDirectory(BackupPath);
                
                // Copy current files
                var exePath = Assembly.GetExecutingAssembly().Location;
                var installDir = Path.GetDirectoryName(exePath) ?? LocalAppData;
                
                CopyDirectory(installDir, BackupPath);
                
                Debug.WriteLine("Backup completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Backup failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Extract update package
        /// </summary>
        private async Task ExtractUpdateAsync(string zipPath)
        {
            await Task.Run(() =>
            {
                var extractPath = Path.Combine(UpdateCachePath, "extracted");
                
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
                Directory.CreateDirectory(extractPath);
                
                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);
                
                Debug.WriteLine("Extraction completed");
            });
        }

        /// <summary>
        /// Apply update by replacing files
        /// </summary>
        private async Task ApplyUpdateAsync()
        {
            await Task.Run(() =>
            {
                var extractPath = Path.Combine(UpdateCachePath, "extracted");
                var installDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? LocalAppData;
                
                // Copy new files
                CopyDirectory(extractPath, installDir, true);
                
                Debug.WriteLine("Update applied successfully");
            });
        }

        /// <summary>
        /// Verify installation is correct
        /// </summary>
        private bool VerifyInstallation(string expectedVersion)
        {
            try
            {
                var currentVersion = GetCurrentVersion();
                var expected = Version.Parse(expectedVersion);
                
                return currentVersion >= expected;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Rollback to previous version
        /// </summary>
        private async Task RollbackUpdateAsync()
        {
            await Task.Run(() =>
            {
                if (!Directory.Exists(BackupPath))
                {
                    throw new Exception("Backup not found");
                }
                
                var installDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? LocalAppData;
                
                // Restore from backup
                CopyDirectory(BackupPath, installDir, true);
                
                Debug.WriteLine("Rollback completed");
            });
        }

        /// <summary>
        /// Clean up temporary update files
        /// </summary>
        private void CleanupUpdateFiles()
        {
            try
            {
                if (Directory.Exists(UpdateCachePath))
                {
                    Directory.Delete(UpdateCachePath, true);
                }
                
                Debug.WriteLine("Cleanup completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cleanup failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Copy directory recursively
        /// </summary>
        private void CopyDirectory(string sourceDir, string destDir, bool overwrite = false)
        {
            Directory.CreateDirectory(destDir);
            
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(destDir, fileName);
                
                // Skip if file is in use
                try
                {
                    File.Copy(file, destFile, overwrite);
                }
                catch (IOException)
                {
                    Debug.WriteLine($"Skipped locked file: {fileName}");
                }
            }
            
            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var dirName = Path.GetFileName(dir);
                var destSubDir = Path.Combine(destDir, dirName);
                CopyDirectory(dir, destSubDir, overwrite);
            }
        }

        /// <summary>
        /// Report progress to UI
        /// </summary>
        private void ReportProgress(int percentage, string message)
        {
            UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(percentage, message));
        }

        /// <summary>
        /// Get scheduled update info
        /// </summary>
        public UpdateInfo? GetScheduledUpdate()
        {
            try
            {
                var schedulePath = Path.Combine(LocalAppData, "scheduled-update.json");
                if (File.Exists(schedulePath))
                {
                    var json = File.ReadAllText(schedulePath);
                    return JsonSerializer.Deserialize<UpdateInfo>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to get scheduled update: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Clear scheduled update
        /// </summary>
        public void ClearScheduledUpdate()
        {
            try
            {
                var schedulePath = Path.Combine(LocalAppData, "scheduled-update.json");
                if (File.Exists(schedulePath))
                {
                    File.Delete(schedulePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to clear scheduled update: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Update information from server
    /// </summary>
    public class UpdateInfo
    {
        public string Version { get; set; } = "1.0.0";
        public DateTime ReleaseDate { get; set; }
        public string DownloadUrl { get; set; } = "";
        public string ReleaseNotes { get; set; } = "";
        public bool IsUpdateAvailable { get; set; }
        public string ChangelogUrl { get; set; } = "";
        public string[] Features { get; set; } = Array.Empty<string>();
        public string[] BugFixes { get; set; } = Array.Empty<string>();
        public bool IsCritical { get; set; }
        public string MinimumVersion { get; set; } = "";
        public bool IsSecureConnection { get; set; } = true;
    }

    /// <summary>
    /// Event args for update available
    /// </summary>
    public class UpdateEventArgs : EventArgs
    {
        public UpdateInfo UpdateInfo { get; }

        public UpdateEventArgs(UpdateInfo updateInfo)
        {
            UpdateInfo = updateInfo;
        }
    }

    /// <summary>
    /// Event args for update progress
    /// </summary>
    public class UpdateProgressEventArgs : EventArgs
    {
        public int Percentage { get; }
        public string Message { get; }

        public UpdateProgressEventArgs(int percentage, string message)
        {
            Percentage = percentage;
            Message = message;
        }
    }

    /// <summary>
    /// Event args for update completed
    /// </summary>
    public class UpdateCompletedEventArgs : EventArgs
    {
        public bool Success { get; }
        public UpdateInfo UpdateInfo { get; }
        public string? ErrorMessage { get; }

        public UpdateCompletedEventArgs(bool success, UpdateInfo updateInfo, string? errorMessage = null)
        {
            Success = success;
            UpdateInfo = updateInfo;
            ErrorMessage = errorMessage;
        }
    }

    /// <summary>
    /// Simple update check result for desktop widget
    /// </summary>
    public class UpdateCheckResult
    {
        public bool UpdateAvailable { get; set; }
        public string CurrentVersion { get; set; } = "";
        public string LatestVersion { get; set; } = "";
        public string ReleaseNotes { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public string ReleaseDate { get; set; } = "";
        public string ChangelogUrl { get; set; } = "";
        public string[] Features { get; set; } = Array.Empty<string>();
        public string[] BugFixes { get; set; } = Array.Empty<string>();
        public bool IsCritical { get; set; }
        public string MinimumVersion { get; set; } = "";
        public bool IsSecureConnection { get; set; } = true;
    }
}
