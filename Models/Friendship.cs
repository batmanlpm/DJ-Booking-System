using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DJBookingSystem.Models
{
    public class FriendRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("type")]
        public string Type { get; set; } = "FriendRequest";

        [JsonProperty("fromUsername")]
        public string FromUsername { get; set; } = string.Empty;

        [JsonProperty("toUsername")]
        public string ToUsername { get; set; } = string.Empty;

        [JsonProperty("status")]
        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;

        [JsonProperty("sentAt")]
        public DateTime SentAt { get; set; } = DateTime.Now;

        [JsonProperty("respondedAt")]
        public DateTime? RespondedAt { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }
    }

    public class Friendship
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("type")]
        public string Type { get; set; } = "Friendship";

        [JsonProperty("user1")]
        public string User1 { get; set; } = string.Empty;

        [JsonProperty("user2")]
        public string User2 { get; set; } = string.Empty;

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("dmConversationId")]
        public string? DmConversationId { get; set; }

        [JsonProperty("user1Metadata")]
        public FriendMetadata User1Metadata { get; set; } = new FriendMetadata();

        [JsonProperty("user2Metadata")]
        public FriendMetadata User2Metadata { get; set; } = new FriendMetadata();
    }

    public class FriendMetadata
    {
        [JsonProperty("nickname")]
        public string? Nickname { get; set; }

        [JsonProperty("isFavorite")]
        public bool IsFavorite { get; set; } = false;

        [JsonProperty("notes")]
        public string? Notes { get; set; }

        [JsonProperty("lastInteraction")]
        public DateTime? LastInteraction { get; set; }
    }

    public enum FriendRequestStatus
    {
        Pending,
        Accepted,
        Declined,
        Cancelled
    }

    public class FriendListEntry
    {
        public string Username { get; set; } = string.Empty;
        public string? Nickname { get; set; }
        public UserRole Role { get; set; }
        public bool IsOnline { get; set; }
        public bool IsFavorite { get; set; }
        public DateTime? LastSeen { get; set; }
        public DateTime FriendsSince { get; set; }
        public string? DmConversationId { get; set; }
        public int UnreadMessages { get; set; } = 0;
    }
}