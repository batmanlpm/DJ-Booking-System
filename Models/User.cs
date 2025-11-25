using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DJBookingSystem.Models
{
    public class User
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        
        [JsonProperty("username")]  // lowercase to match Cosmos partition key /username
        public string Username { get; set; } = string.Empty;
        
        [JsonProperty("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;
        
        [JsonProperty("fullName")]
        public string FullName { get; set; } = string.Empty;
        
        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;
        
        [JsonProperty("role")]
        public UserRole Role { get; set; } = UserRole.User;
        
        [JsonProperty("permissions")]
        public UserPermissions Permissions { get; set; } = new UserPermissions();
        
        [JsonProperty("isActive")]
        public bool IsActive { get; set; } = true;
        
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [JsonProperty("lastLogin")]
        public DateTime LastLogin { get; set; }

        // Online status tracking
        [JsonIgnore]
        public bool IsOnline { get; set; } = false; // Not stored in DB, tracked in memory
        
        [JsonIgnore]
        public string OnlineStatus => IsOnline ? "Online" : "Offline";

        // Account Types (can be multiple)
        [JsonProperty("isDJ")]
        public bool IsDJ { get; set; } = false;
        
        [JsonProperty("isVenueOwner")]
        public bool IsVenueOwner { get; set; } = false;

        // Helper properties that consider role
        [JsonIgnore]
        public bool IsDJEffective => IsDJ || Role == UserRole.SysAdmin;
        
        [JsonIgnore]
        public bool IsVenueOwnerEffective => IsVenueOwner || Role == UserRole.SysAdmin;

        // DJ-specific information (stored on profile for auto-fill during booking)
        [JsonProperty("streamingLink")]
        public string StreamingLink { get; set; } = string.Empty;
        
        [JsonProperty("djLogoUrl")]
        public string DJLogoUrl { get; set; } = string.Empty;

        // Discord integration
        [JsonProperty("discordWebhook")]
        public string DiscordWebhook { get; set; } = string.Empty;

        // User App Preferences
        [JsonProperty("appPreferences")]
        public UserAppPreferences AppPreferences { get; set; } = new UserAppPreferences();

        // Tutorial tracking
        [JsonProperty("hasSeenTutorial")]
        public bool HasSeenTutorial { get; set; } = false;

        // Moderation fields
        [JsonProperty("isBanned")]
        public bool IsBanned { get; set; } = false;
        
        [JsonProperty("bannedBy")]
        public string? BannedBy { get; set; } // Username of moderator who banned
        
        [JsonProperty("bannedAt")]
        public DateTime? BannedAt { get; set; }
        
        [JsonProperty("banReason")]
        public string? BanReason { get; set; }
        
        [JsonProperty("banExpiry")]
        public DateTime? BanExpiry { get; set; } // Null = permanent ban
        
        [JsonProperty("banStrikeCount")]
        public int BanStrikeCount { get; set; } = 0; // Tracks number of bans (0-3)
        
        [JsonProperty("isPermanentBan")]
        public bool IsPermanentBan { get; set; } = false; // True on Strike 3

        [JsonProperty("isGloballyMuted")]
        public bool IsGloballyMuted { get; set; } = false; // Moderator mute (can't send messages)
        
        [JsonProperty("mutedBy")]
        public string? MutedBy { get; set; } // Username of moderator who muted
        
        [JsonProperty("mutedAt")]
        public DateTime? MutedAt { get; set; }
        
        [JsonProperty("muteExpiry")]
        public DateTime? MuteExpiry { get; set; } // Null = permanent mute

        // IP tracking for ban enforcement
        [JsonProperty("registeredIP")]
        public string? RegisteredIP { get; set; } // IP used during registration
        
        [JsonProperty("currentIP")]
        public string? CurrentIP { get; set; } // Last login IP
        
        [JsonProperty("ipHistory")]
        public List<string> IPHistory { get; set; } = new List<string>(); // Track all IPs used
        
        [JsonProperty("bannedIP")]
        public string? BannedIP { get; set; } // IP that was banned (if applicable)

        // Physical address tracking (for admin purposes)
        [JsonProperty("physicalAddress")]
        public string? PhysicalAddress { get; set; } // User's physical address
        
        [JsonProperty("city")]
        public string? City { get; set; }
        
        [JsonProperty("state")]
        public string? State { get; set; }
        
        [JsonProperty("country")]
        public string? Country { get; set; } = "Unknown";
        
        [JsonProperty("postalCode")]
        public string? PostalCode { get; set; }

        // Geolocation tracking (from IP)
        [JsonProperty("latitude")]
        public double? Latitude { get; set; }
        
        [JsonProperty("longitude")]
        public double? Longitude { get; set; }
        
        [JsonProperty("timezone")]
        public string? Timezone { get; set; }
        
        [JsonProperty("isp")]
        public string? ISP { get; set; } // Internet Service Provider

        // Location history for tracking
        public List<UserLocationLog> LocationHistory { get; set; } = new List<UserLocationLog>();
    }

    public class UserAppPreferences
    {
        // Theme preferences
        public string ThemeName { get; set; } = "Default"; // "Default", "Night", "DarkGreen", "Sunset", "Ocean", "Custom"
        public string ColorTheme { get; set; } = "Green"; // "Green", "Purple", "Blue", "Pink", "Orange", "Cyan"

        // Comprehensive custom theme colors - Matrix/Terminal style (Black with fluorescent green)
        public string CustomBackgroundColor { get; set; } = "#0A0A0A"; // Near black
        public string CustomTextColor { get; set; } = "#00FF00"; // Fluorescent green
        public string CustomHeaderColor { get; set; } = "#000000"; // Pure black
        public string CustomMenuColor { get; set; } = "#000000"; // Pure black
        public string CustomButtonColor { get; set; } = "#001100"; // Very dark green
        public string CustomButtonTextColor { get; set; } = "#00FF00"; // Fluorescent green
        public string CustomBorderColor { get; set; } = "#00FF00"; // Fluorescent green
        public string CustomAccentColor { get; set; } = "#00FF00"; // Fluorescent green
        public string CustomSuccessColor { get; set; } = "#39FF14"; // Neon green
        public string CustomErrorColor { get; set; } = "#FF0000"; // Bright red

        // Login preferences
        public bool RememberMe { get; set; } = false;
        public bool AutoLogin { get; set; } = false;

        // Window preferences
        public bool StayOnTop { get; set; } = false;
        
        // Tutorial preferences
        public bool HasSeenTutorial { get; set; } = false;
        
        /// <summary>
        /// Admin-settable flag to force tutorial on next login
        /// When true, HasSeenTutorial will be reset to false on next login
        /// </summary>
        [JsonIgnore]
        public bool ShowTutorialOnNextLogin
        {
            get => !HasSeenTutorial;
            set
            {
                if (value)
                {
                    // Admin checked "Show Tutorial" - reset the flag
                    HasSeenTutorial = false;
                }
                else
                {
                    // Admin unchecked - mark as seen so it won't show
                    HasSeenTutorial = true;
                }
            }
        }
    }

    public class UserPermissions
    {
        // Booking permissions
        public bool CanViewBookings { get; set; } = true;
        public bool CanCreateBookings { get; set; } = true;
        public bool CanEditBookings { get; set; } = true;
        public bool CanDeleteBookings { get; set; } = false;

        // Venue permissions
        public bool CanViewVenues { get; set; } = true;
        public bool CanRegisterVenues { get; set; } = true;
        public bool CanEditVenues { get; set; } = false;
        public bool CanDeleteVenues { get; set; } = false;
        public bool CanToggleVenueStatus { get; set; } = false;

        // Admin permissions
        public bool CanManageUsers { get; set; } = false;
        public bool CanCustomizeApp { get; set; } = false;
        public bool CanAccessSettings { get; set; } = true;

        // Moderation permissions
        public bool CanBanUsers { get; set; } = false;
        public bool CanMuteUsers { get; set; } = false;
        public bool CanViewReports { get; set; } = false;
        public bool CanResolveReports { get; set; } = false;

        // RadioBOSS permissions
        public bool CanViewRadioBoss { get; set; } = false;
        public bool CanControlRadioBoss { get; set; } = false;
    }

    /// <summary>
    /// User location log entry for tracking where users login from
    /// </summary>
    public class UserLocationLog
    {
        public DateTime Timestamp { get; set; }
        public string IPAddress { get; set; } = "";
        public string? City { get; set; }
        public string? Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? ISP { get; set; }
        public string Action { get; set; } = "Login"; // Login, Logout, Activity
    }
}
