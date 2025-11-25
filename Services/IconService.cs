using System;
using System.Collections.Generic;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Provides theme-aware icons for the application
    /// Icons change based on the active theme
    /// </summary>
    public static class IconService
    {
        /// <summary>
        /// Get an icon based on the icon name and current theme
        /// </summary>
        public static string GetIcon(string iconName)
        {
            string currentTheme = ThemeManager.CurrentTheme ?? "Green";
            
            // Map icon names to actual icon characters/emoji
            return iconName switch
            {
                // Title Bar Icons
                "AppIcon" => "??",
                "MinimizeToTray" => "?",
                "Logout" => "??",
                "Close" => "?",
                
                // Booking Management Icons
                "NewBooking" => "?",
                "Edit" => "??",
                "Delete" => "???",
                "Refresh" => "??",
                "BookingsTitle" => "??",
                
                // Venue Management Icons
                "NewVenue" => "??",
                "VenuesTitle" => "???",
                
                // User Management Icons
                "AllUsers" => "??",
                "OnlineUsers" => "??",
                "OfflineUsers" => "?",
                "CreateUser" => "?",
                "Mute" => "??",
                "Ban" => "??",
                "Promote" => "??",
                "Permissions" => "??",
                "PermissionGroups" => "??",
                "ErrorReports" => "??",
                "AuditLog" => "??",
                "UsersTitle" => "?????",
                
                // Help Icons
                "Help" => "?",
                "HelpTitle" => "??",
                
                // Default fallback
                _ => GetDefaultIcon(iconName)
            };
        }

        /// <summary>
        /// Get default icon if no specific icon is defined
        /// </summary>
        private static string GetDefaultIcon(string iconName)
        {
            // Return a neutral icon based on context
            if (iconName.Contains("New") || iconName.Contains("Create") || iconName.Contains("Add"))
                return "?";
            if (iconName.Contains("Edit") || iconName.Contains("Update"))
                return "??";
            if (iconName.Contains("Delete") || iconName.Contains("Remove"))
                return "???";
            if (iconName.Contains("Refresh") || iconName.Contains("Reload"))
                return "??";
            if (iconName.Contains("Search") || iconName.Contains("Find"))
                return "??";
            if (iconName.Contains("Settings") || iconName.Contains("Config"))
                return "??";
            
            return "?"; // Default bullet point
        }

        /// <summary>
        /// Get all available icon names for documentation
        /// </summary>
        public static Dictionary<string, string> GetAllIcons()
        {
            return new Dictionary<string, string>
            {
                { "AppIcon", GetIcon("AppIcon") },
                { "MinimizeToTray", GetIcon("MinimizeToTray") },
                { "Logout", GetIcon("Logout") },
                { "Close", GetIcon("Close") },
                { "NewBooking", GetIcon("NewBooking") },
                { "Edit", GetIcon("Edit") },
                { "Delete", GetIcon("Delete") },
                { "Refresh", GetIcon("Refresh") },
                { "BookingsTitle", GetIcon("BookingsTitle") },
                { "NewVenue", GetIcon("NewVenue") },
                { "VenuesTitle", GetIcon("VenuesTitle") },
                { "AllUsers", GetIcon("AllUsers") },
                { "OnlineUsers", GetIcon("OnlineUsers") },
                { "OfflineUsers", GetIcon("OfflineUsers") },
                { "CreateUser", GetIcon("CreateUser") },
                { "Mute", GetIcon("Mute") },
                { "Ban", GetIcon("Ban") },
                { "Promote", GetIcon("Promote") },
                { "Permissions", GetIcon("Permissions") },
                { "PermissionGroups", GetIcon("PermissionGroups") },
                { "ErrorReports", GetIcon("ErrorReports") },
                { "AuditLog", GetIcon("AuditLog") },
                { "UsersTitle", GetIcon("UsersTitle") },
                { "Help", GetIcon("Help") },
                { "HelpTitle", GetIcon("HelpTitle") }
            };
        }
    }
}
