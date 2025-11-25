using System;

namespace DJBookingSystem.Models
{
    /// <summary>
    /// Online user information for display in OnlineUsersWindow
    /// </summary>
    public class OnlineUserInfo
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsDJ { get; set; }
        public bool IsVenueOwner { get; set; }
        
        public DateTime LoginTime { get; set; }
        public DateTime LastActivityTime { get; set; }
        
        public string Location { get; set; } = "Unknown";
        public string IPAddress { get; set; } = "Unknown";
        
        // Computed properties for display
        public string OnlineDuration
        {
            get
            {
                var duration = DateTime.Now - LoginTime;
                
                if (duration.TotalDays >= 1)
                    return $"{(int)duration.TotalDays}d {duration.Hours}h";
                
                if (duration.TotalHours >= 1)
                    return $"{(int)duration.TotalHours}h {duration.Minutes}m";
                
                if (duration.TotalMinutes >= 1)
                    return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";
                
                return $"{(int)duration.TotalSeconds}s";
            }
        }
        
        public string LastActivity
        {
            get
            {
                var timeSince = DateTime.Now - LastActivityTime;
                
                if (timeSince.TotalSeconds < 30)
                    return "Just now";
                
                if (timeSince.TotalMinutes < 1)
                    return $"{(int)timeSince.TotalSeconds}s ago";
                
                if (timeSince.TotalMinutes < 60)
                    return $"{(int)timeSince.TotalMinutes}m ago";
                
                if (timeSince.TotalHours < 24)
                    return $"{(int)timeSince.TotalHours}h ago";
                
                return $"{(int)timeSince.TotalDays}d ago";
            }
        }
        
        public string StatusIndicator => "?";
        
        public string RoleIcon
        {
            get
            {
                return Role switch
                {
                    "SysAdmin" => "??",
                    "Manager" => "???",
                    "VenueOwner" => "??",
                    "DJ" => "??",
                    _ => "??"
                };
            }
        }
        
        public string RoleColor
        {
            get
            {
                return Role switch
                {
                    "SysAdmin" => "#FFD700",
                    "Manager" => "#FFA500",
                    "VenueOwner" => "#1E90FF",
                    "DJ" => "#FF69B4",
                    _ => "#00FF00"
                };
            }
        }
    }
}
