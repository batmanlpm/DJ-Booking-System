using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Utility for managing SSL/TLS certificates and fingerprints
    /// </summary>
    public static class CertificateManager
    {
        /// <summary>
        /// Get certificate fingerprint from a URL
        /// </summary>
        public static async Task<CertificateInfo?> GetCertificateInfoAsync(string url)
        {
            try
            {
                X509Certificate2? certificate = null;

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (request, cert, chain, errors) =>
                    {
                        if (cert != null)
                        {
                            certificate = new X509Certificate2(cert);
                        }
                        return true; // Always return true to capture certificate
                    }
                };

                using (var client = new HttpClient(handler))
                {
                    await client.GetAsync(url);
                }

                if (certificate == null)
                {
                    return null;
                }

                return new CertificateInfo
                {
                    Subject = certificate.Subject,
                    Issuer = certificate.Issuer,
                    SerialNumber = certificate.SerialNumber,
                    NotBefore = certificate.NotBefore,
                    NotAfter = certificate.NotAfter,
                    Thumbprint = certificate.Thumbprint,
                    Sha256Fingerprint = CalculateSha256Fingerprint(certificate),
                    Sha1Fingerprint = certificate.Thumbprint,
                    PublicKey = Convert.ToBase64String(certificate.GetPublicKey()),
                    SignatureAlgorithm = certificate.SignatureAlgorithm.FriendlyName ?? "Unknown",
                    Version = certificate.Version,
                    HasPrivateKey = certificate.HasPrivateKey,
                    DnsNames = GetSubjectAlternativeNames(certificate)
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting certificate info: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Calculate SHA256 fingerprint of certificate
        /// </summary>
        public static string CalculateSha256Fingerprint(X509Certificate2 certificate)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(certificate.RawData);
                return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
            }
        }

        /// <summary>
        /// Get Subject Alternative Names from certificate
        /// </summary>
        private static string[] GetSubjectAlternativeNames(X509Certificate2 certificate)
        {
            try
            {
                var sanExtension = certificate.Extensions
                    .OfType<X509Extension>()
                    .FirstOrDefault(e => e.Oid?.Value == "2.5.29.17"); // SAN OID

                if (sanExtension != null)
                {
                    var asnData = new AsnEncodedData(sanExtension.Oid!, sanExtension.RawData);
                    var sanString = asnData.Format(true);
                    
                    return sanString
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => s.StartsWith("DNS Name="))
                        .Select(s => s.Replace("DNS Name=", ""))
                        .ToArray();
                }

                return Array.Empty<string>();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Verify certificate matches expected fingerprint
        /// </summary>
        public static bool VerifyFingerprint(X509Certificate2 certificate, string expectedFingerprint)
        {
            try
            {
                string actualFingerprint = CalculateSha256Fingerprint(certificate);
                return string.Equals(actualFingerprint, expectedFingerprint, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error verifying fingerprint: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Export certificate information to text file
        /// </summary>
        public static void ExportCertificateInfo(CertificateInfo certInfo, string outputPath)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("???????????????????????????????????????????????????????????");
                sb.AppendLine("              SSL/TLS CERTIFICATE INFORMATION              ");
                sb.AppendLine("???????????????????????????????????????????????????????????");
                sb.AppendLine();
                
                sb.AppendLine("?? BASIC INFORMATION");
                sb.AppendLine("?????????????????????????????????????????????????????????");
                sb.AppendLine($"Subject:     {certInfo.Subject}");
                sb.AppendLine($"Issuer:      {certInfo.Issuer}");
                sb.AppendLine($"Serial:      {certInfo.SerialNumber}");
                sb.AppendLine($"Version:     {certInfo.Version}");
                sb.AppendLine($"Algorithm:   {certInfo.SignatureAlgorithm}");
                sb.AppendLine();

                sb.AppendLine("?? VALIDITY PERIOD");
                sb.AppendLine("?????????????????????????????????????????????????????????");
                sb.AppendLine($"Valid From:  {certInfo.NotBefore:yyyy-MM-dd HH:mm:ss} UTC");
                sb.AppendLine($"Valid Until: {certInfo.NotAfter:yyyy-MM-dd HH:mm:ss} UTC");
                sb.AppendLine($"Status:      {(DateTime.Now >= certInfo.NotBefore && DateTime.Now <= certInfo.NotAfter ? "? Valid" : "? Expired")}");
                sb.AppendLine();

                sb.AppendLine("?? FINGERPRINTS (for Certificate Pinning)");
                sb.AppendLine("?????????????????????????????????????????????????????????");
                sb.AppendLine($"SHA-256:     {certInfo.Sha256Fingerprint}");
                sb.AppendLine($"SHA-1:       {certInfo.Sha1Fingerprint}");
                sb.AppendLine();

                if (certInfo.DnsNames.Length > 0)
                {
                    sb.AppendLine("?? SUBJECT ALTERNATIVE NAMES");
                    sb.AppendLine("?????????????????????????????????????????????????????????");
                    foreach (var dnsName in certInfo.DnsNames)
                    {
                        sb.AppendLine($"   • {dnsName}");
                    }
                    sb.AppendLine();
                }

                sb.AppendLine("?? PUBLIC KEY");
                sb.AppendLine("?????????????????????????????????????????????????????????");
                sb.AppendLine(certInfo.PublicKey);
                sb.AppendLine();

                sb.AppendLine("???????????????????????????????????????????????????????????");
                sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss} UTC");
                sb.AppendLine("???????????????????????????????????????????????????????????");

                File.WriteAllText(outputPath, sb.ToString());
                Debug.WriteLine($"Certificate information exported to: {outputPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error exporting certificate info: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate certificate pinning code snippet
        /// </summary>
        public static string GeneratePinningCodeSnippet(CertificateInfo certInfo)
        {
            return $@"
// Certificate Pinning Configuration
// Certificate: {certInfo.Subject}
// Valid Until: {certInfo.NotAfter:yyyy-MM-dd}

private static readonly string[] TRUSTED_FINGERPRINTS = new[]
{{
    // SHA-256 Fingerprint
    ""{certInfo.Sha256Fingerprint}"",
}};

private bool ValidateServerCertificate(
    HttpRequestMessage request,
    X509Certificate2 certificate,
    X509Chain chain,
    SslPolicyErrors sslPolicyErrors)
{{
    string fingerprint = CalculateSha256Fingerprint(certificate);
    return TRUSTED_FINGERPRINTS.Contains(fingerprint, StringComparer.OrdinalIgnoreCase);
}}
";
        }

        /// <summary>
        /// Test SSL connection to server
        /// </summary>
        public static async Task<SslTestResult> TestSslConnectionAsync(string url)
        {
            var result = new SslTestResult { Url = url };

            try
            {
                result.TestStartTime = DateTime.UtcNow;

                using (var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (request, cert, chain, errors) =>
                    {
                        if (cert != null)
                        {
                            result.Certificate = new X509Certificate2(cert);
                            result.CertificateReceived = true;
                        }
                        result.SslPolicyErrors = errors;
                        return true; // Always succeed to capture details
                    }
                })
                using (var client = new HttpClient(handler))
                {
                    var response = await client.GetAsync(url);
                    result.ConnectionSuccessful = response.IsSuccessStatusCode;
                    result.HttpStatusCode = (int)response.StatusCode;
                    result.ResponseMessage = response.ReasonPhrase ?? "";
                }

                result.TestEndTime = DateTime.UtcNow;
                result.ResponseTime = (result.TestEndTime - result.TestStartTime).TotalMilliseconds;
                result.Success = result.ConnectionSuccessful && result.CertificateReceived;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                Debug.WriteLine($"SSL connection test failed: {ex.Message}");
            }

            return result;
        }
    }

    /// <summary>
    /// Certificate information model
    /// </summary>
    public class CertificateInfo
    {
        public string Subject { get; set; } = "";
        public string Issuer { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public DateTime NotBefore { get; set; }
        public DateTime NotAfter { get; set; }
        public string Thumbprint { get; set; } = "";
        public string Sha256Fingerprint { get; set; } = "";
        public string Sha1Fingerprint { get; set; } = "";
        public string PublicKey { get; set; } = "";
        public string SignatureAlgorithm { get; set; } = "";
        public int Version { get; set; }
        public bool HasPrivateKey { get; set; }
        public string[] DnsNames { get; set; } = Array.Empty<string>();

        public bool IsValid()
        {
            return DateTime.Now >= NotBefore && DateTime.Now <= NotAfter;
        }

        public string GetValidityStatus()
        {
            if (!IsValid())
            {
                return "? Expired";
            }

            var daysUntilExpiry = (NotAfter - DateTime.Now).Days;
            if (daysUntilExpiry < 30)
            {
                return $"?? Expires in {daysUntilExpiry} days";
            }

            return "? Valid";
        }
    }

    /// <summary>
    /// SSL connection test result
    /// </summary>
    public class SslTestResult
    {
        public string Url { get; set; } = "";
        public bool Success { get; set; }
        public bool ConnectionSuccessful { get; set; }
        public bool CertificateReceived { get; set; }
        public X509Certificate2? Certificate { get; set; }
        public System.Net.Security.SslPolicyErrors SslPolicyErrors { get; set; }
        public int HttpStatusCode { get; set; }
        public string ResponseMessage { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
        public DateTime TestStartTime { get; set; }
        public DateTime TestEndTime { get; set; }
        public double ResponseTime { get; set; }

        public string GetSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine("SSL Connection Test Results");
            sb.AppendLine("???????????????????????????????????????");
            sb.AppendLine($"URL: {Url}");
            sb.AppendLine($"Status: {(Success ? "? Success" : "? Failed")}");
            sb.AppendLine($"Response Time: {ResponseTime:F2} ms");
            sb.AppendLine($"HTTP Status: {HttpStatusCode} {ResponseMessage}");
            
            if (Certificate != null)
            {
                sb.AppendLine($"Certificate: {Certificate.Subject}");
                sb.AppendLine($"Fingerprint: {CertificateManager.CalculateSha256Fingerprint(Certificate)}");
            }

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                sb.AppendLine($"Error: {ErrorMessage}");
            }

            return sb.ToString();
        }
    }
}
