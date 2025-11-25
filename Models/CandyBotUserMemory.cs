using System;
using System.Collections.Generic;

namespace DJBookingSystem.Models
{
    /// <summary>
    /// Candy-Bot User Memory - Stores conversation context and preferences
    /// SECURITY: Never stores passwords or sensitive authentication data
    /// </summary>
    public class CandyBotUserMemory
    {
        /// <summary>
        /// User's login username (e.g., "SysAdmin", "DJ_Mike")
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// User's full name from profile (e.g., "System Administrator")
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// User's preferred name (set by user, e.g., "Mike", "DJ Flash")
        /// Takes priority over Username/FullName in conversations
        /// </summary>
        public string? PreferredName { get; set; }

        /// <summary>
        /// When user first interacted with Candy-Bot
        /// </summary>
        public DateTime FirstMet { get; set; }

        /// <summary>
        /// Last interaction timestamp
        /// </summary>
        public DateTime LastSeen { get; set; }

        /// <summary>
        /// Conversation history (last 100 messages)
        /// Format: "User: message" or "Candy-Bot: response"
        /// </summary>
        public List<string> ConversationHistory { get; set; } = new List<string>();

        /// <summary>
        /// User preferences as key-value pairs
        /// Examples: "theme" => "dark", "notifications" => "enabled"
        /// </summary>
        public Dictionary<string, string> UserPreferences { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// User's favorite Candy-Bot personality mode
        /// </summary>
        public CandyBotPersonalityMode FavoritePersonality { get; set; } = CandyBotPersonalityMode.Normal;

        /// <summary>
        /// Total number of DJ bookings made by user
        /// </summary>
        public int TotalBookings { get; set; }

        /// <summary>
        /// List of user's most booked venues (top 3)
        /// </summary>
        public List<string> FavoriteVenues { get; set; } = new List<string>();

        /// <summary>
        /// Topics discussed in recent conversations
        /// Helps maintain context across sessions
        /// </summary>
        public List<string> RecentTopics { get; set; } = new List<string>();

        /// <summary>
        /// User's timezone (for booking reminders)
        /// </summary>
        public string? TimeZone { get; set; }

        /// <summary>
        /// Custom notes Candy-Bot remembers about the user
        /// Set through natural conversation
        /// </summary>
        public Dictionary<string, string> PersonalNotes { get; set; } = new Dictionary<string, string>();
    }
}
