using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Management;
using Newtonsoft.Json;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Manages machine-specific ban enforcement to prevent VPN bypass
    /// </summary>
    public class MachineBanService
    {
        private static readonly string BAN_FILE_PATH = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DJBookingSystem",
            "machine_ban.dat");

        /// <summary>
        /// Get unique machine identifier based on hardware
        /// </summary>
        public static string GetMachineId()
        {
            try
            {
                string cpuId = GetCpuId();
                string motherboardId = GetMotherboardId();
                string combined = $"{cpuId}-{motherboardId}";
                
                // Hash the combined ID for security
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                    return Convert.ToBase64String(hashBytes);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MachineBan] Error getting machine ID: {ex.Message}");
                // Fallback to computer name if hardware detection fails
                return Environment.MachineName;
            }
        }

        private static string GetCpuId()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    return obj["ProcessorId"]?.ToString() ?? "UNKNOWN_CPU";
                }
            }
            catch { }
            return "UNKNOWN_CPU";
        }

        private static string GetMotherboardId()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                foreach (ManagementObject obj in searcher.Get())
                {
                    return obj["SerialNumber"]?.ToString() ?? "UNKNOWN_MB";
                }
            }
            catch { }
            return "UNKNOWN_MB";
        }

        /// <summary>
        /// Store ban information on local machine
        /// </summary>
        public static void StoreBanLocally(string username, DateTime banExpiry, int strikeCount, string reason, bool isPermanent)
        {
            try
            {
                var banData = new MachineBanData
                {
                    MachineId = GetMachineId(),
                    Username = username,
                    BanExpiry = banExpiry,
                    StrikeCount = strikeCount,
                    BanReason = reason,
                    IsPermanent = isPermanent,
                    BannedAt = DateTime.Now
                };

                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(BAN_FILE_PATH));

                // Write encrypted ban data
                string json = JsonConvert.SerializeObject(banData, Formatting.Indented);
                File.WriteAllText(BAN_FILE_PATH, json);

                System.Diagnostics.Debug.WriteLine($"[MachineBan] Ban stored locally for {username} on machine {banData.MachineId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MachineBan] Error storing ban: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if this machine has an active ban
        /// </summary>
        public static MachineBanData? CheckLocalBan()
        {
            try
            {
                if (!File.Exists(BAN_FILE_PATH))
                {
                    return null; // No ban file
                }

                string json = File.ReadAllText(BAN_FILE_PATH);
                var banData = JsonConvert.DeserializeObject<MachineBanData>(json);

                if (banData == null)
                {
                    return null;
                }

                // Check if ban has expired
                if (!banData.IsPermanent && banData.BanExpiry.HasValue && banData.BanExpiry.Value < DateTime.Now)
                {
                    // Ban expired - remove file
                    RemoveLocalBan();
                    System.Diagnostics.Debug.WriteLine($"[MachineBan] Ban expired and removed for {banData.Username}");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"[MachineBan] Active ban found for {banData.Username} - Expires: {banData.BanExpiry}");
                return banData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MachineBan] Error checking ban: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Remove local ban file (when ban expires or SysAdmin override)
        /// </summary>
        public static void RemoveLocalBan()
        {
            try
            {
                if (File.Exists(BAN_FILE_PATH))
                {
                    File.Delete(BAN_FILE_PATH);
                    System.Diagnostics.Debug.WriteLine("[MachineBan] Local ban file removed");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MachineBan] Error removing ban: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Data structure for machine-bound ban
    /// </summary>
    public class MachineBanData
    {
        [JsonProperty("machineId")]
        public string MachineId { get; set; } = string.Empty;

        [JsonProperty("username")]
        public string Username { get; set; } = string.Empty;

        [JsonProperty("banExpiry")]
        public DateTime? BanExpiry { get; set; }

        [JsonProperty("strikeCount")]
        public int StrikeCount { get; set; }

        [JsonProperty("banReason")]
        public string BanReason { get; set; } = string.Empty;

        [JsonProperty("isPermanent")]
        public bool IsPermanent { get; set; }

        [JsonProperty("bannedAt")]
        public DateTime BannedAt { get; set; }
    }
}
