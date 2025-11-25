using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DJBookingSystem.Models
{
    /// <summary>
    /// User online/offline status
    /// </summary>
    public enum UserStatus
    {
        Offline,
        Online,
        Away,
        Busy,
        DoNotDisturb
    }

    /// <summary>
    /// User roles in the system
    /// </summary>
    public enum UserRole
    {
        User,
        DJ,
        VenueOwner,
        Manager,
        SysAdmin
    }

    /// <summary>
    /// User warning record
    /// </summary>
    public class UserWarning
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Reason { get; set; } = string.Empty;
        public string IssuedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// User error report (silent error reporting to admin)
    /// </summary>
    public class UserErrorReport
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        // Error Details
        public string ErrorMessage { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public string? ErrorType { get; set; }
        public string? SourceFile { get; set; }
        public int? LineNumber { get; set; }
        
        // Context
        public string CurrentScreen { get; set; } = string.Empty;
        public string? ActionBeingPerformed { get; set; }
        public Dictionary<string, string> ContextData { get; set; } = new Dictionary<string, string>();
        
        // System Info
        public string AppVersion { get; set; } = string.Empty;
        public string OSVersion { get; set; } = string.Empty;
        public string? MachineName { get; set; }
        
        // Status
        public bool HasBeenReviewed { get; set; } = false;
        public DateTime? ReviewedDate { get; set; }
        public string? ReviewedBy { get; set; }
        public string? AdminNotes { get; set; }
        
        // Screenshot (optional)
        public string? ScreenshotBase64 { get; set; }
    }

    /// <summary>
    /// Permission group for bulk permission management
    /// </summary>
    public class PermissionGroup
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> PermissionIds { get; set; } = new List<string>();
        public List<string> UserIds { get; set; } = new List<string>();
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// User action log for audit trail
    /// </summary>
    public class UserActionLog
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        public UserActionType ActionType { get; set; }
        public string ActionDetails { get; set; } = string.Empty;
        public string? PerformedBy { get; set; }
        public string? TargetUserId { get; set; }
        
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Types of user actions for logging
    /// </summary>
    public enum UserActionType
    {
        Login,
        Logout,
        Created,
        Updated,
        Deleted,
        RoleChanged,
        PermissionChanged,
        Muted,
        Unmuted,
        Banned,
        Unbanned,
        Warned,
        ErrorReported,
        SettingsChanged
    }
}
