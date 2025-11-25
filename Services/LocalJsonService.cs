using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DJBookingSystem.Models;
using Newtonsoft.Json;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Local JSON file-based storage - Firebase interface compatible
    /// Perfect for offline development and testing
    /// </summary>
    public class LocalJsonService
    {
        private readonly string _dataDirectory;
        private readonly string _usersFile;
        private readonly string _bookingsFile;
        private readonly string _venuesFile;
        private readonly string _equipmentFile;
        private readonly string _settingsFile;
        private readonly string _chatFile;

        public LocalJsonService()
        {
            // Store data in AppData\Local\DJBookingSystem
            _dataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DJBookingSystem",
                "Data"
            );

            Directory.CreateDirectory(_dataDirectory);

            _usersFile = Path.Combine(_dataDirectory, "users.json");
            _bookingsFile = Path.Combine(_dataDirectory, "bookings.json");
            _venuesFile = Path.Combine(_dataDirectory, "venues.json");
            _equipmentFile = Path.Combine(_dataDirectory, "equipment.json");
            _settingsFile = Path.Combine(_dataDirectory, "settings.json");
            _chatFile = Path.Combine(_dataDirectory, "chat.json");

            InitializeFiles();
        }

        private void InitializeFiles()
        {
            if (!File.Exists(_usersFile))
                File.WriteAllText(_usersFile, "{}");
            if (!File.Exists(_bookingsFile))
                File.WriteAllText(_bookingsFile, "{}");
            if (!File.Exists(_venuesFile))
                File.WriteAllText(_venuesFile, "{}");
            if (!File.Exists(_equipmentFile))
                File.WriteAllText(_equipmentFile, "{}");
            if (!File.Exists(_settingsFile))
                File.WriteAllText(_settingsFile, "{}");
            if (!File.Exists(_chatFile))
                File.WriteAllText(_chatFile, "{}");
        }

        #region User Management

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await Task.Run(() =>
            {
                var json = File.ReadAllText(_usersFile);
                var usersDict = JsonConvert.DeserializeObject<Dictionary<string, User>>(json) ?? new Dictionary<string, User>();
                return usersDict.Select(kvp => { var u = kvp.Value; u.Id = kvp.Key; return u; }).ToList();
            });
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            var users = await GetAllUsersAsync();
            return users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<string> AddUserAsync(User user)
        {
            return await Task.Run(() =>
            {
                var json = File.ReadAllText(_usersFile);
                var usersDict = JsonConvert.DeserializeObject<Dictionary<string, User>>(json) ?? new Dictionary<string, User>();
                
                var id = Guid.NewGuid().ToString();
                usersDict[id] = user;
                
                File.WriteAllText(_usersFile, JsonConvert.SerializeObject(usersDict, Formatting.Indented));
                return id;
            });
        }

        public async Task UpdateUserAsync(string id, User user)
        {
            await Task.Run(() =>
            {
                var json = File.ReadAllText(_usersFile);
                var usersDict = JsonConvert.DeserializeObject<Dictionary<string, User>>(json) ?? new Dictionary<string, User>();
                
                usersDict[id] = user;
                
                File.WriteAllText(_usersFile, JsonConvert.SerializeObject(usersDict, Formatting.Indented));
            });
        }

        public async Task DeleteUserAsync(string id)
        {
            await Task.Run(() =>
            {
                var json = File.ReadAllText(_usersFile);
                var usersDict = JsonConvert.DeserializeObject<Dictionary<string, User>>(json) ?? new Dictionary<string, User>();
                
                usersDict.Remove(id);
                
                File.WriteAllText(_usersFile, JsonConvert.SerializeObject(usersDict, Formatting.Indented));
            });
        }

        public async Task DeleteUserAccountAsync(string username)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user != null && !string.IsNullOrEmpty(user.Id))
                await DeleteUserAsync(user.Id);
        }

        public async Task InitializeDefaultAdminAsync()
        {
            var users = await GetAllUsersAsync();
            if (!users.Any(u => u.Username.Equals("SysAdmin", StringComparison.OrdinalIgnoreCase)))
            {
                // Hash for "Fraser1960@"
                string passwordHash = HashPassword("Fraser1960@");
                
                await AddUserAsync(new User
                {
                    Username = "SysAdmin",
                    PasswordHash = passwordHash,
                    FullName = "System Administrator",
                    Email = "sysadmin@djbooking.local",
                    Role = UserRole.SysAdmin,
                    Permissions = new UserPermissions(),
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    IPHistory = new List<string>(),
                    AppPreferences = new UserAppPreferences
                    {
                        ThemeName = "Default",
                        AutoLogin = false,
                        RememberMe = false
                    }
                });
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                var builder = new System.Text.StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public async Task<User?> GetBannedUserByIPAsync(string ip)
        {
            var users = await GetAllUsersAsync();
            return users.FirstOrDefault(u => u.IsBanned && u.IPHistory.Contains(ip));
        }

        #endregion

        #region App Settings

        public async Task<AppSettings> GetAppSettingsAsync()
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(_settingsFile) || new FileInfo(_settingsFile).Length == 0)
                {
                    var defaultSettings = new AppSettings
                    {
                        AppTitle = "DJ Booking Management System",
                        UpdatedAt = DateTime.Now,
                        Theme = new ThemeSettings(),
                        Features = new FeatureSettings()
                    };
                    File.WriteAllText(_settingsFile, JsonConvert.SerializeObject(defaultSettings, Formatting.Indented));
                    return defaultSettings;
                }

                var json = File.ReadAllText(_settingsFile);
                return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            });
        }

        public async Task SaveAppSettingsAsync(AppSettings settings)
        {
            await Task.Run(() =>
            {
                File.WriteAllText(_settingsFile, JsonConvert.SerializeObject(settings, Formatting.Indented));
            });
        }

        #endregion

        #region Update Management

        public async Task<bool> IsUpdateAvailableAsync()
        {
            // Local storage - no updates available
            return await Task.FromResult(false);
        }

        public async Task<AppVersion?> GetLatestVersionAsync()
        {
            return await Task.FromResult<AppVersion?>(null);
        }

        #endregion

        #region Error Logging

        public async Task LogErrorToChatAsync(string errorMessage, string errorCode, string username)
        {
            // Log to a local error log file
            var logFile = Path.Combine(_dataDirectory, "errors.log");
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{errorCode}] [{username}]\n{errorMessage}\n\n";
            
            await Task.Run(() =>
            {
                File.AppendAllText(logFile, logEntry);
            });
        }

        #endregion

        #region Booking Management

        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            return await Task.Run(() =>
            {
                var json = File.ReadAllText(_bookingsFile);
                var bookingsDict = JsonConvert.DeserializeObject<Dictionary<string, Booking>>(json) ?? new Dictionary<string, Booking>();
                return bookingsDict.Select(kvp => { var b = kvp.Value; b.Id = kvp.Key; return b; }).ToList();
            });
        }

        public async Task<string> AddBookingAsync(Booking booking)
        {
            return await Task.Run(() =>
            {
                var json = File.ReadAllText(_bookingsFile);
                var bookingsDict = JsonConvert.DeserializeObject<Dictionary<string, Booking>>(json) ?? new Dictionary<string, Booking>();
                
                var id = Guid.NewGuid().ToString();
                bookingsDict[id] = booking;
                
                File.WriteAllText(_bookingsFile, JsonConvert.SerializeObject(bookingsDict, Formatting.Indented));
                return id;
            });
        }

        public async Task UpdateBookingAsync(string id, Booking booking)
        {
            await Task.Run(() =>
            {
                var json = File.ReadAllText(_bookingsFile);
                var bookingsDict = JsonConvert.DeserializeObject<Dictionary<string, Booking>>(json) ?? new Dictionary<string, Booking>();
                
                bookingsDict[id] = booking;
                
                File.WriteAllText(_bookingsFile, JsonConvert.SerializeObject(bookingsDict, Formatting.Indented));
            });
        }

        public async Task DeleteBookingAsync(string id)
        {
            await Task.Run(() =>
            {
                var json = File.ReadAllText(_bookingsFile);
                var bookingsDict = JsonConvert.DeserializeObject<Dictionary<string, Booking>>(json) ?? new Dictionary<string, Booking>();
                
                bookingsDict.Remove(id);
                
                File.WriteAllText(_bookingsFile, JsonConvert.SerializeObject(bookingsDict, Formatting.Indented));
            });
        }

        #endregion

        #region Venue Management

        public async Task<List<Venue>> GetAllVenuesAsync()
        {
            return await Task.Run(() =>
            {
                var json = File.ReadAllText(_venuesFile);
                var venuesDict = JsonConvert.DeserializeObject<Dictionary<string, Venue>>(json) ?? new Dictionary<string, Venue>();
                return venuesDict.Select(kvp => { var v = kvp.Value; v.Id = kvp.Key; return v; }).ToList();
            });
        }

        public async Task<string> AddVenueAsync(Venue venue)
        {
            return await Task.Run(() =>
            {
                var json = File.ReadAllText(_venuesFile);
                var venuesDict = JsonConvert.DeserializeObject<Dictionary<string, Venue>>(json) ?? new Dictionary<string, Venue>();
                
                var id = Guid.NewGuid().ToString();
                venuesDict[id] = venue;
                
                File.WriteAllText(_venuesFile, JsonConvert.SerializeObject(venuesDict, Formatting.Indented));
                return id;
            });
        }

        public async Task UpdateVenueAsync(string id, Venue venue)
        {
            await Task.Run(() =>
            {
                var json = File.ReadAllText(_venuesFile);
                var venuesDict = JsonConvert.DeserializeObject<Dictionary<string, Venue>>(json) ?? new Dictionary<string, Venue>();
                
                venuesDict[id] = venue;
                
                File.WriteAllText(_venuesFile, JsonConvert.SerializeObject(venuesDict, Formatting.Indented));
            });
        }

        public async Task DeleteVenueAsync(string id)
        {
            await Task.Run(() =>
            {
                var json = File.ReadAllText(_venuesFile);
                var venuesDict = JsonConvert.DeserializeObject<Dictionary<string, Venue>>(json) ?? new Dictionary<string, Venue>();
                
                venuesDict.Remove(id);
                
                File.WriteAllText(_venuesFile, JsonConvert.SerializeObject(venuesDict, Formatting.Indented));
            });
        }

        #endregion

        public string GetDataDirectory() => _dataDirectory;
    }
}
