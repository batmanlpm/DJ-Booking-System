using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DJBookingSystem.Models
{
    public class ChatMessage
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        
        // Partition key: Use conversation ID for efficient querying
        // Format: "world" | "private:{user1}:{user2}" | "group:{groupId}" | "support:{ticketId}"
        [JsonProperty("conversationId")]
        public string ConversationId { get; set; } = "world";
        
        public string SenderUsername { get; set; } = string.Empty;
        public string SenderRole { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool IsErrorMessage { get; set; } = false;
        public string? ErrorCode { get; set; }
        public MessageType Type { get; set; } = MessageType.Normal;

        // Sender identity flags for color coding
        public bool SenderIsDJ { get; set; } = false;
        public bool SenderIsVenueOwner { get; set; } = false;

        // Channel and targeting
        public ChatChannel Channel { get; set; } = ChatChannel.World;
        public string? RecipientUsername { get; set; } // For private 1-on-1 messages
        public List<string> GroupMembers { get; set; } = new List<string>(); // For group chats
        public string? GroupName { get; set; } // For group chat identification
        public string? GroupId { get; set; } // Unique group identifier

        // Message status (for private/group chats)
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        
        // Participants (for easy querying)
        public List<string> Participants { get; set; } = new List<string>(); // All users who can see this message
    }

    public enum MessageType
    {
        Normal,
        Error,
        System,
        Announcement
    }

    public enum ChatChannel
    {
        World,          // Public to all users
        ToAdmin,        // Support tickets (visible to sender + all admins)
        AdminOnly,      // Admin-only channel (SysAdmin + Managers)
        Private,        // 1-on-1 private message
        Group           // Group chat
    }

    /// <summary>
    /// Represents a conversation (DM or Group)
    /// </summary>
    public class Conversation
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [JsonProperty("type")]
        public string Type { get; set; } = "Conversation";
        
        [JsonProperty("conversationType")]
        public ConversationType ConversationType { get; set; }
        
        [JsonProperty("participants")]
        public List<string> Participants { get; set; } = new List<string>();
        
        [JsonProperty("groupName")]
        public string? GroupName { get; set; } // For group chats
        
        [JsonProperty("groupDescription")]
        public string? GroupDescription { get; set; }
        
        [JsonProperty("createdBy")]
        public string? CreatedBy { get; set; }
        
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [JsonProperty("lastMessageAt")]
        public DateTime? LastMessageAt { get; set; }
        
        [JsonProperty("lastMessagePreview")]
        public string? LastMessagePreview { get; set; }
        
        [JsonProperty("unreadCount")]
        public Dictionary<string, int> UnreadCount { get; set; } = new Dictionary<string, int>(); // Username -> count
    }

    public enum ConversationType
    {
        DirectMessage,  // 1-on-1 private chat
        Group          // Group chat
    }

    public class UserChatSettings
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        public string Type { get; set; } = "ChatSettings"; // Used for queries
        public string Username { get; set; } = string.Empty;
        public List<string> BlockedUsers { get; set; } = new List<string>(); // Users this user has blocked
        public List<string> MutedUsers { get; set; } = new List<string>();   // Users this user has muted
        public List<string> PinnedConversations { get; set; } = new List<string>(); // Conversation IDs
        public bool EnableNotifications { get; set; } = true;
        public bool EnableSoundNotifications { get; set; } = true;
        public bool MinimizeToTray { get; set; } = true;
    }

    public class ChatNotification
    {
        public string MessageId { get; set; } = string.Empty;
        public string SenderUsername { get; set; } = string.Empty;
        public string MessagePreview { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public ChatChannel Channel { get; set; } = ChatChannel.World;
        public string? ConversationId { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
