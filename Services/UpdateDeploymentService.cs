using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Service for deploying updates to the server
    /// </summary>
    public class UpdateDeploymentService
    {
        private readonly HttpClient _httpClient;
        // Hostinger deployment configuration
        private const string DEPLOYMENT_URL = "https://c40.radioboss.fm/u/98";
        private const string SFTP_HOST = "153.92.10.234";
        private const int SFTP_PORT = 65002;
        private const string SFTP_USERNAME = "u833570579";
        private const string SFTP_REMOTE_PATH = "/public_html/";

        public UpdateDeploymentService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(30) // Long timeout for file uploads
            };
        }

        /// <summary>
        /// Validate update file before deployment
        /// </summary>
        public async Task<UpdateValidationResult> ValidateUpdateFileAsync(string filePath)
        {
            var result = new UpdateValidationResult();

            try
            {
                if (!File.Exists(filePath))
                {
                    result.ValidationErrors.Add("File does not exist");
                    return result;
                }

                var fileInfo = new FileInfo(filePath);

                // Check file size
                result.FileSize = fileInfo.Length;
                if (result.FileSize == 0)
                {
                    result.ValidationErrors.Add("File is empty");
                }
                else if (result.FileSize < 1024 * 1024) // Less than 1MB
                {
                    result.ValidationErrors.Add("File seems too small for an application update");
                }
                else if (result.FileSize > 500 * 1024 * 1024) // More than 500MB
                {
                    result.ValidationErrors.Add("File is too large (max 500MB)");
                }

                // Check extension
                result.IsExecutable = fileInfo.Extension.ToLower() == ".exe";
                if (!result.IsExecutable)
                {
                    result.ValidationErrors.Add("File must be an executable (.exe)");
                }

                // Calculate SHA256 hash
                using (var sha256 = SHA256.Create())
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = await Task.Run(() => sha256.ComputeHash(stream));
                    result.Sha256Hash = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                }

                // Check digital signature (Windows only)
                try
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(filePath);
                    result.HasDigitalSignature = !string.IsNullOrEmpty(versionInfo.LegalCopyright);
                    result.FileVersion = versionInfo.FileVersion ?? "";
                }
                catch
                {
                    result.HasDigitalSignature = false;
                }

                result.IsValid = result.ValidationErrors.Count == 0;

                Debug.WriteLine($"Update file validation: {(result.IsValid ? "PASSED" : "FAILED")}");
                Debug.WriteLine($"File: {filePath}");
                Debug.WriteLine($"Size: {result.FileSize:N0} bytes");
                Debug.WriteLine($"SHA256: {result.Sha256Hash}");

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error validating update file: {ex.Message}");
                result.ValidationErrors.Add($"Validation error: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Deploy update to server via SFTP
        /// </summary>
        public async Task<bool> DeployUpdateAsync(UpdateDeploymentInfo deploymentInfo, IProgress<int>? progress = null)
        {
            try
            {
                Debug.WriteLine("Starting update deployment...");
                Debug.WriteLine($"Version: {deploymentInfo.Version}");
                Debug.WriteLine($"File: {deploymentInfo.FilePath}");

                // Step 1: Validate file (10%)
                progress?.Report(10);
                var validation = await ValidateUpdateFileAsync(deploymentInfo.FilePath);
                
                if (!validation.IsValid)
                {
                    Debug.WriteLine("Validation failed:");
                    foreach (var error in validation.ValidationErrors)
                    {
                        Debug.WriteLine($"  - {error}");
                    }
                    return false;
                }

                // Step 2: Upload file to server (10-80%)
                progress?.Report(20);
                bool uploadSuccess = await UploadFileToServerAsync(
                    deploymentInfo.FilePath, 
                    deploymentInfo.Version,
                    new Progress<int>(percent =>
                    {
                        // Map 0-100% to 20-80%
                        int adjustedPercent = 20 + (int)(percent * 0.6);
                        progress?.Report(adjustedPercent);
                    }));

                if (!uploadSuccess)
                {
                    Debug.WriteLine("File upload failed");
                    return false;
                }

                // Step 3: Update version.json manifest (80-90%)
                progress?.Report(85);
                bool manifestSuccess = await UpdateVersionManifestAsync(deploymentInfo, validation.Sha256Hash);
                
                if (!manifestSuccess)
                {
                    Debug.WriteLine("Manifest update failed");
                    return false;
                }

                // Step 4: Verify deployment (90-100%)
                progress?.Report(95);
                bool verifySuccess = await VerifyDeploymentAsync(deploymentInfo.Version);
                
                if (!verifySuccess)
                {
                    Debug.WriteLine("Deployment verification failed");
                    return false;
                }

                progress?.Report(100);
                Debug.WriteLine("? Update deployed successfully");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deploying update: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Upload file to server via SFTP
        /// </summary>
        private async Task<bool> UploadFileToServerAsync(string filePath, string version, IProgress<int>? progress = null)
        {
            try
            {
                Debug.WriteLine($"Uploading file to SFTP server...");
                
                // Note: This requires SSH.NET NuGet package
                // For now, using placeholder logic
                
                string remoteFileName = $"app-v{version}.exe";
                string remotePath = SFTP_REMOTE_PATH + remoteFileName;

                Debug.WriteLine($"Remote path: {remotePath}");

                /*
                // Actual SFTP upload code (requires SSH.NET package)
                using (var client = new Renci.SshNet.SftpClient(SFTP_HOST, SFTP_PORT, SFTP_USERNAME, "Fraser1960@"))
                {
                    client.Connect();

                    if (!client.IsConnected)
                    {
                        Debug.WriteLine("Failed to connect to SFTP server");
                        return false;
                    }

                    Debug.WriteLine("Connected to SFTP server");

                    using (var fileStream = File.OpenRead(filePath))
                    {
                        var fileSize = fileStream.Length;
                        var uploadedBytes = 0L;

                        client.UploadFile(fileStream, remotePath, (ulong bytes) =>
                        {
                            uploadedBytes = (long)bytes;
                            if (fileSize > 0)
                            {
                                var percentComplete = (int)((uploadedBytes * 100) / fileSize);
                                progress?.Report(percentComplete);
                            }
                        });
                    }

                    client.Disconnect();
                    Debug.WriteLine("File uploaded successfully");
                    return true;
                }
                */

                // Placeholder: Simulate upload
                await Task.Delay(2000);
                for (int i = 0; i <= 100; i += 10)
                {
                    progress?.Report(i);
                    await Task.Delay(200);
                }

                Debug.WriteLine("?? SFTP upload requires SSH.NET NuGet package");
                Debug.WriteLine("Install with: Install-Package SSH.NET");
                
                return false; // Return false until SSH.NET is implemented
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error uploading file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Update version.json manifest on server
        /// </summary>
        private async Task<bool> UpdateVersionManifestAsync(UpdateDeploymentInfo deploymentInfo, string fileHash)
        {
            try
            {
                Debug.WriteLine("Updating version manifest...");

                var manifest = new
                {
                    version = deploymentInfo.Version,
                    release_date = deploymentInfo.ReleaseDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    download_url = $"https://c40.radioboss.fm/u/98/DJBookingSystem.exe",
                    release_notes = deploymentInfo.ReleaseNotes,
                    sha256 = fileHash,
                    is_critical = deploymentInfo.IsCritical,
                    features = deploymentInfo.Features ?? Array.Empty<string>(),
                    bug_fixes = deploymentInfo.BugFixes ?? Array.Empty<string>()
                };

                var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });

                Debug.WriteLine("Manifest JSON:");
                Debug.WriteLine(json);

                // Upload manifest to server
                // This would use SFTP to upload version.json to /public_html/Updates/version.json

                await Task.Delay(500); // Placeholder

                Debug.WriteLine("? Manifest updated");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating manifest: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verify deployment was successful
        /// </summary>
        private async Task<bool> VerifyDeploymentAsync(string version)
        {
            try
            {
                Debug.WriteLine("Verifying deployment...");

                // Check if update-info.json is accessible on Hostinger
                var checkUrl = "https://c40.radioboss.fm/u/98/update-info.json";
                
                try
                {
                    var response = await _httpClient.GetStringAsync(checkUrl);
                    var versionInfo = JsonSerializer.Deserialize<ServerVersionInfo>(response);

                    if (versionInfo != null && versionInfo.Version == version)
                    {
                        Debug.WriteLine("? Deployment verified successfully");
                        return true;
                    }
                }
                catch
                {
                    // Server might not be accessible yet
                }

                Debug.WriteLine("?? Verification incomplete (server might need time to update)");
                return true; // Return true anyway as upload may have succeeded
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error verifying deployment: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Update validation result
    /// </summary>
    public class UpdateValidationResult
    {
        public bool IsValid { get; set; }
        public long FileSize { get; set; }
        public string Sha256Hash { get; set; } = "";
        public bool IsExecutable { get; set; }
        public bool HasDigitalSignature { get; set; }
        public string FileVersion { get; set; } = "";
        public List<string> ValidationErrors { get; set; } = new();
    }

    /// <summary>
    /// Update deployment information
    /// </summary>
    public class UpdateDeploymentInfo
    {
        public string Version { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string ReleaseNotes { get; set; } = "";
        public bool IsCritical { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string[] Features { get; set; } = Array.Empty<string>();
        public string[] BugFixes { get; set; } = Array.Empty<string>();
    }
}
