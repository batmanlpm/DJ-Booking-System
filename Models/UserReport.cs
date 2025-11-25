using System;
using Newtonsoft.Json;

namespace DJBookingSystem.Models
{
    /// <summary>
    /// User report for moderation purposes
    /// </summary>
    public class UserReport
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("type")]
        public string Type { get; set; } = "UserReport";

        [JsonProperty("reportedUsername")]
        public string ReportedUsername { get; set; } = string.Empty;

        [JsonProperty("reporterUsername")]
        public string ReporterUsername { get; set; } = string.Empty;

        [JsonProperty("reportedBy")]
        public string ReportedBy { get; set; } = string.Empty;

        [JsonProperty("reason")]
        public string Reason { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("details")]
        public string Details { get; set; } = string.Empty;

        [JsonProperty("reportedAt")]
        public DateTime ReportedAt { get; set; } = DateTime.Now;

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("status")]
        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        [JsonProperty("isResolved")]
        public bool IsResolved { get; set; } = false;

        [JsonProperty("resolvedBy")]
        public string? ResolvedBy { get; set; }

        [JsonProperty("resolvedAt")]
        public DateTime? ResolvedAt { get; set; }

        [JsonProperty("adminNotes")]
        public string? AdminNotes { get; set; }
    }

    public enum ReportStatus
    {
        Pending,
        UnderReview,
        Resolved,
        Dismissed
    }
}
