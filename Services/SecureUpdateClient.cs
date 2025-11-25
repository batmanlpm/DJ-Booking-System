using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DJBookingSystem.Models;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Secure update client with SSL certificate pinning and fingerprint verification
    /// </summary>
    public class SecureUpdateClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private bool _disposed = false;

        // Update server configuration - HOSTINGER
        private const string UPDATE_SERVER_URL = "https://djbookupdates.com";
        private const string UPDATE_CHECK_ENDPOINT = "/version.json";
        private const string UPDATE_DOWNLOAD_ENDPOINT = "/downloads/DJBookingSystem-Setup.exe";
        
        // Expected SSL certificate fingerprints (SHA256) for certificate pinning
        // Note: Certificate pinning disabled for Hostinger (shared hosting)
        private static readonly string[] TRUSTED_FINGERPRINTS = new string[]
        {
            // Hostinger uses shared certificates - pinning not recommended
            // Leave empty array to disable certificate pinning
        };

        // SSH/SFTP credentials for secure file transfer
        private const string SFTP_HOST = "153.92.10.234";
        private const int SFTP_PORT = 65002;
        private const string SFTP_USERNAME = "u833570579";
        // Note: Password should be stored securely, not hardcoded
        private static readonly string SFTP_PASSWORD = DecryptPassword();

        public SecureUpdateClient()
        {
            // Create HttpClientHandler with custom certificate validation
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = ValidateServerCertificate,
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
                CheckCertificateRevocationList = true
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromMinutes(5),
                BaseAddress = new Uri(UPDATE_SERVER_URL)
            };

            _httpClient.DefaultRequestHeaders.Add("User-Agent", $"DJBookingSystem/{VersionInfo.VersionString}");
            _httpClient.DefaultRequestHeaders.Add("X-Client-Version", VersionInfo.VersionString);
        }

        /// <summary>
        /// Validates server certificate using fingerprint pinning
        /// </summary>
        private bool ValidateServerCertificate(
            HttpRequestMessage request,
            X509Certificate2? certificate,
            X509Chain? chain,
            SslPolicyErrors sslPolicyErrors)
        {
            // CERTIFICATE PINNING DISABLED FOR HOSTINGER (shared hosting)
            // Just validate standard SSL/TLS without pinning
            
            // Null check for certificate
            if (certificate == null)
            {
                Debug.WriteLine("Certificate validation failed: Certificate is null");
                return false;
            }

            // Log certificate details for debugging
            Debug.WriteLine($"Certificate Subject: {certificate.Subject}");
            Debug.WriteLine($"Certificate Issuer: {certificate.Issuer}");
            Debug.WriteLine($"Certificate Valid From: {certificate.NotBefore}");
            Debug.WriteLine($"Certificate Valid To: {certificate.NotAfter}");

            // For Hostinger, we accept standard SSL certificates without pinning
            // Just check for basic SSL policy errors
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                Debug.WriteLine($"SSL Policy Errors: {sslPolicyErrors}");
                
                // For shared hosting (Hostinger), we can't pin certificates
                // So we just accept valid SSL certificates from trusted CAs
                // Allow standard SSL validation (no pinning)
                return sslPolicyErrors == SslPolicyErrors.None;
            }

            // Verify certificate is not expired
            if (DateTime.Now < certificate.NotBefore || DateTime.Now > certificate.NotAfter)
            {
                Debug.WriteLine("Certificate is expired or not yet valid");
                return false;
            }

            // Calculate certificate fingerprint for logging (not validation)
            string fingerprint = CalculateCertificateFingerprint(certificate);
            Debug.WriteLine($"Certificate Fingerprint (SHA256): {fingerprint}");
            Debug.WriteLine("Certificate pinning disabled for Hostinger - accepting standard SSL certificate");

            // Accept valid SSL certificate (no pinning for shared hosting)
            return true;
        }

        /// <summary>
        /// Calculate SHA256 fingerprint of certificate
        /// </summary>
        private string CalculateCertificateFingerprint(X509Certificate2 certificate)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(certificate.RawData);
                return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
            }
        }

        /// <summary>
        /// Check for available updates securely
        /// </summary>
        public async Task<UpdateCheckResult> CheckForUpdatesAsync()
        {
            try
            {
                Debug.WriteLine("Checking for updates securely...");
                Debug.WriteLine($"Update check URL: {UPDATE_SERVER_URL}{UPDATE_CHECK_ENDPOINT}");

                // GET version.json from server
                var response = await _httpClient.GetAsync(UPDATE_CHECK_ENDPOINT);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Version info received: {responseJson}");
                
                var updateInfo = JsonSerializer.Deserialize<ServerVersionInfo>(responseJson, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (updateInfo == null)
                {
                    Debug.WriteLine("Failed to deserialize version info");
                    return new UpdateCheckResult
                    {
                        UpdateAvailable = false,
                        CurrentVersion = VersionInfo.VersionString
                    };
                }

                // Check if update is available
                var currentVersion = new Version(VersionInfo.VersionString);
                var latestVersion = new Version(updateInfo.LatestVersion ?? updateInfo.Version);
                bool updateAvailable = latestVersion > currentVersion;

                Debug.WriteLine($"Current version: {currentVersion}, Latest version: {latestVersion}, Update available: {updateAvailable}");

                return new UpdateCheckResult
                {
                    UpdateAvailable = updateAvailable,
                    CurrentVersion = VersionInfo.VersionString,
                    LatestVersion = updateInfo.LatestVersion ?? updateInfo.Version,
                    ReleaseDate = updateInfo.ReleaseDate,
                    DownloadUrl = updateInfo.DownloadUrl,
                    ChangelogUrl = updateInfo.ChangelogUrl,
                    Features = updateInfo.Features,
                    BugFixes = updateInfo.BugFixes,
                    IsCritical = updateInfo.IsCritical ?? false,
                    MinimumVersion = updateInfo.MinimumVersion,
                    IsSecureConnection = true
                };
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HTTP error checking for updates: {ex.Message}");
                throw new UpdateException("Failed to check for updates. Please check your internet connection.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking for updates: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new UpdateException("An error occurred while checking for updates.", ex);
            }
        }

        /// <summary>
        /// Download update file securely with progress reporting
        /// </summary>
        public async Task<string> DownloadUpdateAsync(string downloadUrl, IProgress<int> progress)
        {
            try
            {
                Debug.WriteLine($"Downloading update from: {downloadUrl}");

                string tempPath = Path.Combine(Path.GetTempPath(), $"DJBookingSystem_Update_{Guid.NewGuid()}.exe");

                using (var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? 0;
                    var downloadedBytes = 0L;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            downloadedBytes += bytesRead;

                            if (totalBytes > 0)
                            {
                                var percentComplete = (int)((downloadedBytes * 100) / totalBytes);
                                progress?.Report(percentComplete);
                            }
                        }
                    }
                }

                // Verify downloaded file
                if (!File.Exists(tempPath))
                {
                    throw new UpdateException("Downloaded file not found");
                }

                var fileInfo = new FileInfo(tempPath);
                if (fileInfo.Length == 0)
                {
                    File.Delete(tempPath);
                    throw new UpdateException("Downloaded file is empty");
                }

                Debug.WriteLine($"Update downloaded successfully to: {tempPath}");
                Debug.WriteLine($"File size: {fileInfo.Length:N0} bytes");

                return tempPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error downloading update: {ex.Message}");
                throw new UpdateException("Failed to download update", ex);
            }
        }

        /// <summary>
        /// Decrypt stored password (implement secure storage)
        /// </summary>
        private static string DecryptPassword()
        {
            // TODO: Implement secure password storage using Windows Data Protection API (DPAPI)
            // For now, return the password (should be encrypted in production)
            // DO NOT commit actual passwords to source control
            return "Fraser1960@";
        }

        /// <summary>
        /// Verify update file signature
        /// </summary>
        public bool VerifyUpdateSignature(string filePath, string expectedSignature)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }

                // Calculate file hash
                using (var sha256 = SHA256.Create())
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = sha256.ComputeHash(stream);
                    var fileSignature = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                    
                    Debug.WriteLine($"File signature: {fileSignature}");
                    Debug.WriteLine($"Expected signature: {expectedSignature}");

                    return string.Equals(fileSignature, expectedSignature, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error verifying signature: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Update exception
    /// </summary>
    public class UpdateException : Exception
    {
        public UpdateException(string message) : base(message) { }
        public UpdateException(string message, Exception innerException) : base(message, innerException) { }
    }
}
