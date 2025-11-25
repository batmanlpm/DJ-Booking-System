using Newtonsoft.Json;
using System;

namespace DJBookingSystem.Models
{
    /// <summary>
    /// Represents a user's online status stored in Cosmos DB
    /// </summary>
    public class OnlineUserStatus
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("Username")]
        public string Username { get; set; } = string.Empty;

        [JsonProperty("IsOnline")]
        public bool IsOnline { get; set; }

        [JsonProperty("LastActivity")]
        public DateTime LastActivity { get; set; }

        [JsonProperty("LastHeartbeat")]
        public DateTime LastHeartbeat { get; set; }

        [JsonProperty("MachineName")]
        public string? MachineName { get; set; }

        [JsonProperty("SessionId")]
        public string? SessionId { get; set; }
    }
}
