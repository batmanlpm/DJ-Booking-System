using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DJBookingSystem.Models
{
    /// <summary>
    /// Private support ticket - only visible to creator and admins
    /// </summary>
    public class SupportTicket
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("type")]
        public string Type { get; set; } = "SupportTicket"; // For Cosmos queries

        [JsonProperty("ticketType")]
        public TicketType TicketType { get; set; } = TicketType.General;

        [JsonProperty("ticketNumber")]
        public int TicketNumber { get; set; } // Auto-incremented ticket #

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; } = string.Empty; // Username who created ticket (or "Anonymous" for ban appeals)

        [JsonProperty("contactEmail")]
        public string? ContactEmail { get; set; } // Required for anonymous ban appeals

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonProperty("status")]
        public TicketStatus Status { get; set; } = TicketStatus.Open;

        [JsonProperty("priority")]
        public TicketPriority Priority { get; set; } = TicketPriority.Normal;

        [JsonProperty("assignedTo")]
        public string? AssignedTo { get; set; } // Admin username assigned to this ticket

        [JsonProperty("messages")]
        public List<TicketMessage> Messages { get; set; } = new List<TicketMessage>();

        [JsonProperty("closedAt")]
        public DateTime? ClosedAt { get; set; }

        [JsonProperty("closedBy")]
        public string? ClosedBy { get; set; }

        // Privacy: Only visible to creator and admins
        [JsonProperty("isPrivate")]
        public bool IsPrivate { get; set; } = true; // Always true for support tickets

        [JsonProperty("visibleTo")]
        public List<string> VisibleTo { get; set; } = new List<string>(); // Creator + assigned admins

        // Ban appeal specific
        [JsonProperty("isAnonymous")]
        public bool IsAnonymous { get; set; } = false; // True for ban appeals submitted without login

        [JsonProperty("relatedBanInfo")]
        public BanAppealInfo? RelatedBanInfo { get; set; } // Ban details if this is an appeal
    }

    public class TicketMessage
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("senderUsername")]
        public string SenderUsername { get; set; } = string.Empty;

        [JsonProperty("senderRole")]
        public string SenderRole { get; set; } = string.Empty;

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [JsonProperty("isAdminResponse")]
        public bool IsAdminResponse { get; set; } = false;
    }

    public enum TicketStatus
    {
        Open,
        InProgress,
        WaitingForUser,
        Resolved,
        Closed
    }

    public enum TicketPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    public enum TicketType
    {
        General,        // General support
        BanAppeal,      // Appeal a ban (can be submitted without login)
        Technical,      // Technical issue
        Billing,        // Payment/billing issue
        FeatureRequest  // Request new feature
    }

    public class BanAppealInfo
    {
        [JsonProperty("username")]
        public string Username { get; set; } = string.Empty;

        [JsonProperty("banReason")]
        public string BanReason { get; set; } = string.Empty;

        [JsonProperty("strikeCount")]
        public int StrikeCount { get; set; }

        [JsonProperty("banExpiry")]
        public DateTime? BanExpiry { get; set; }

        [JsonProperty("isPermanent")]
        public bool IsPermanent { get; set; }

        [JsonProperty("bannedBy")]
        public string? BannedBy { get; set; }

        [JsonProperty("bannedAt")]
        public DateTime? BannedAt { get; set; }
    }
}
