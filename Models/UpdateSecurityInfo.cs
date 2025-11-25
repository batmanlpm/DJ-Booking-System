using System;

namespace DJBookingSystem.Models
{
    /// <summary>
    /// Security information for update verification
    /// </summary>
    public class UpdateSecurityInfo
    {
        /// <summary>
        /// SHA256 fingerprint of the SSL certificate
        /// </summary>
        public string CertificateFingerprint { get; set; } = "";

        /// <summary>
        /// SHA256 hash of the update file
        /// </summary>
        public string FileSignature { get; set; } = "";

        /// <summary>
        /// Timestamp when the update was signed
        /// </summary>
        public DateTime SignatureTimestamp { get; set; }

        /// <summary>
        /// Whether the connection used SSL/TLS
        /// </summary>
        public bool UsedSecureConnection { get; set; }

        /// <summary>
        /// Whether certificate pinning was successful
        /// </summary>
        public bool CertificatePinningSuccessful { get; set; }

        /// <summary>
        /// Whether file signature verification passed
        /// </summary>
        public bool FileSignatureValid { get; set; }

        /// <summary>
        /// TLS version used for the connection
        /// </summary>
        public string TlsVersion { get; set; } = "";

        /// <summary>
        /// Cipher suite used for the connection
        /// </summary>
        public string CipherSuite { get; set; } = "";

        /// <summary>
        /// Overall security status
        /// </summary>
        public SecurityStatus Status
        {
            get
            {
                if (UsedSecureConnection && CertificatePinningSuccessful && FileSignatureValid)
                {
                    return SecurityStatus.Secure;
                }
                else if (UsedSecureConnection && CertificatePinningSuccessful)
                {
                    return SecurityStatus.PartiallySecure;
                }
                else
                {
                    return SecurityStatus.Insecure;
                }
            }
        }

        /// <summary>
        /// Get security status message
        /// </summary>
        public string GetSecurityStatusMessage()
        {
            return Status switch
            {
                SecurityStatus.Secure => "? Secure connection with verified certificate and signature",
                SecurityStatus.PartiallySecure => "?? Secure connection but file signature could not be verified",
                SecurityStatus.Insecure => "? Insecure connection - update not recommended",
                _ => "? Unknown security status"
            };
        }

        /// <summary>
        /// Get detailed security information
        /// </summary>
        public string GetDetailedSecurityInfo()
        {
            return $@"Security Information:
??????????????????????????????????

?? Connection Security
   • Secure Connection: {(UsedSecureConnection ? "? Yes" : "? No")}
   • TLS Version: {TlsVersion}
   • Cipher Suite: {CipherSuite}

?? Certificate Verification
   • Certificate Pinning: {(CertificatePinningSuccessful ? "? Successful" : "? Failed")}
   • Certificate Fingerprint: {CertificateFingerprint}

?? File Integrity
   • Signature Verification: {(FileSignatureValid ? "? Valid" : "? Invalid")}
   • File Signature: {FileSignature}
   • Signed: {SignatureTimestamp:yyyy-MM-dd HH:mm:ss} UTC

??????????????????????????????????
Overall Status: {GetSecurityStatusMessage()}";
        }
    }

    /// <summary>
    /// Security status enum
    /// </summary>
    public enum SecurityStatus
    {
        Secure,
        PartiallySecure,
        Insecure
    }
}
