using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DJBookingSystem.Models;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Discord-style chat service with DMs, group chats, and world chat
    /// </summary>
    public class ChatService
    {
        private readonly CosmosDbService _cosmosDb;
        private readonly string _currentUsername;

        public ChatService(CosmosDbService cosmosDb, string currentUsername)
        {
            _cosmosDb = cosmosDb;
            _currentUsername = currentUsername;
        }

        #region Conversation Management

        /// <summary>
        /// Get or create a direct message conversation with another user
        /// </summary>
        public async Task<Conversation> GetOrCreateDirectMessageAsync(string otherUsername)
        {
            // DM conversation IDs are alphabetically sorted for consistency
            var participants = new List<string> { _currentUsername, otherUsername }.OrderBy(x => x).ToList();
            var conversationId = $"dm:{participants[0]}:{participants[1]}";

            // Try to find existing conversation
            var allConversations = await GetMyConversationsAsync();
            var existing = allConversations.FirstOrDefault(c => c.Id == conversationId);

            if (existing != null)
            {
                return existing;
            }

            // Create new DM conversation
            var newConversation = new Conversation
            {
                Id = conversationId,
                ConversationType = ConversationType.DirectMessage,
                Participants = participants,
                CreatedBy = _currentUsername,
                CreatedAt = DateTime.Now
            };

            // TODO: Save to database
            // await _cosmosDb.CreateConversationAsync(newConversation);

            return newConversation;
        }

        /// <summary>
        /// Create a new group chat
        /// </summary>
        public async Task<Conversation> CreateGroupChatAsync(string groupName, List<string> members)
        {
            var groupId = Guid.NewGuid().ToString();
            var conversationId = $"group:{groupId}";

            var newGroup = new Conversation
            {
                Id = conversationId,
                ConversationType = ConversationType.Group,
                GroupName = groupName,
                Participants = members,
                CreatedBy = _currentUsername,
                CreatedAt = DateTime.Now
            };

            // TODO: Save to database
            // await _cosmosDb.CreateConversationAsync(newGroup);

            return newGroup;
        }

        /// <summary>
        /// Get all conversations the current user is part of
        /// </summary>
        public async Task<List<Conversation>> GetMyConversationsAsync()
        {
            // TODO: Query database for conversations where Participants contains currentUsername
            // For now, return empty list
            return new List<Conversation>();
        }

        #endregion

        #region Message Sending

        /// <summary>
        /// Send a message to world chat (public)
        /// </summary>
        public async Task<ChatMessage> SendWorldMessageAsync(string messageText, User sender)
        {
            var message = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = "world",
                SenderUsername = sender.Username,
                SenderRole = sender.Role.ToString(),
                Message = messageText,
                Timestamp = DateTime.Now,
                Channel = ChatChannel.World,
                Type = MessageType.Normal,
                SenderIsDJ = sender.IsDJ,
                SenderIsVenueOwner = sender.IsVenueOwner,
                Participants = new List<string>() // Empty = visible to all
            };

            await _cosmosDb.AddChatMessageAsync(message);
            return message;
        }

        /// <summary>
        /// Send a private message (DM) to another user
        /// </summary>
        public async Task<ChatMessage> SendPrivateMessageAsync(string recipientUsername, string messageText, User sender)
        {
            var conversation = await GetOrCreateDirectMessageAsync(recipientUsername);

            var message = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = conversation.Id,
                SenderUsername = sender.Username,
                SenderRole = sender.Role.ToString(),
                RecipientUsername = recipientUsername,
                Message = messageText,
                Timestamp = DateTime.Now,
                Channel = ChatChannel.Private,
                Type = MessageType.Normal,
                SenderIsDJ = sender.IsDJ,
                SenderIsVenueOwner = sender.IsVenueOwner,
                Participants = conversation.Participants
            };

            await _cosmosDb.AddChatMessageAsync(message);

            // Update conversation last message
            conversation.LastMessageAt = message.Timestamp;
            conversation.LastMessagePreview = messageText.Length > 50 
                ? messageText.Substring(0, 50) + "..." 
                : messageText;

            // Increment unread count for recipient
            if (!conversation.UnreadCount.ContainsKey(recipientUsername))
            {
                conversation.UnreadCount[recipientUsername] = 0;
            }
            conversation.UnreadCount[recipientUsername]++;

            // TODO: Update conversation in database
            // await _cosmosDb.UpdateConversationAsync(conversation);

            return message;
        }

        /// <summary>
        /// Send a message to a group chat
        /// </summary>
        public async Task<ChatMessage> SendGroupMessageAsync(string conversationId, string messageText, User sender)
        {
            var conversation = (await GetMyConversationsAsync())
                .FirstOrDefault(c => c.Id == conversationId);

            if (conversation == null || conversation.ConversationType != ConversationType.Group)
            {
                throw new Exception("Group conversation not found");
            }

            if (!conversation.Participants.Contains(sender.Username))
            {
                throw new Exception("You are not a member of this group");
            }

            var message = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = conversationId,
                SenderUsername = sender.Username,
                SenderRole = sender.Role.ToString(),
                Message = messageText,
                Timestamp = DateTime.Now,
                Channel = ChatChannel.Group,
                Type = MessageType.Normal,
                SenderIsDJ = sender.IsDJ,
                SenderIsVenueOwner = sender.IsVenueOwner,
                Participants = conversation.Participants,
                GroupId = conversationId,
                GroupName = conversation.GroupName
            };

            await _cosmosDb.AddChatMessageAsync(message);

            // Update conversation last message
            conversation.LastMessageAt = message.Timestamp;
            conversation.LastMessagePreview = $"{sender.Username}: {(messageText.Length > 40 ? messageText.Substring(0, 40) + "..." : messageText)}";

            // Increment unread count for all participants except sender
            foreach (var participant in conversation.Participants.Where(p => p != sender.Username))
            {
                if (!conversation.UnreadCount.ContainsKey(participant))
                {
                    conversation.UnreadCount[participant] = 0;
                }
                conversation.UnreadCount[participant]++;
            }

            // TODO: Update conversation in database
            // await _cosmosDb.UpdateConversationAsync(conversation);

            return message;
        }

        #endregion

        #region Message Retrieval

        /// <summary>
        /// Get messages for world chat
        /// </summary>
        public async Task<List<ChatMessage>> GetWorldMessagesAsync()
        {
            var allMessages = await _cosmosDb.GetAllChatMessagesAsync();
            return allMessages
                .Where(m => m.Channel == ChatChannel.World || m.ConversationId == "world")
                .OrderBy(m => m.Timestamp)
                .ToList();
        }

        /// <summary>
        /// Get messages for a private DM conversation
        /// </summary>
        public async Task<List<ChatMessage>> GetDirectMessagesAsync(string otherUsername)
        {
            var conversation = await GetOrCreateDirectMessageAsync(otherUsername);
            var allMessages = await _cosmosDb.GetAllChatMessagesAsync();
            
            return allMessages
                .Where(m => m.ConversationId == conversation.Id)
                .OrderBy(m => m.Timestamp)
                .ToList();
        }

        /// <summary>
        /// Get messages for a group chat
        /// </summary>
        public async Task<List<ChatMessage>> GetGroupMessagesAsync(string conversationId)
        {
            var allMessages = await _cosmosDb.GetAllChatMessagesAsync();
            
            return allMessages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.Timestamp)
                .ToList();
        }

        /// <summary>
        /// Get support/admin messages
        /// </summary>
        public async Task<List<ChatMessage>> GetSupportMessagesAsync()
        {
            var allMessages = await _cosmosDb.GetAllChatMessagesAsync();
            
            return allMessages
                .Where(m => m.Channel == ChatChannel.ToAdmin && 
                           (m.SenderUsername == _currentUsername || m.Participants.Contains(_currentUsername)))
                .OrderBy(m => m.Timestamp)
                .ToList();
        }

        #endregion

        #region Conversation Helpers

        /// <summary>
        /// Mark all messages in a conversation as read
        /// </summary>
        public async Task MarkConversationAsReadAsync(string conversationId)
        {
            var conversations = await GetMyConversationsAsync();
            var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);

            if (conversation != null && conversation.UnreadCount.ContainsKey(_currentUsername))
            {
                conversation.UnreadCount[_currentUsername] = 0;
                // TODO: Update in database
                // await _cosmosDb.UpdateConversationAsync(conversation);
            }
        }

        /// <summary>
        /// Add a member to a group chat
        /// </summary>
        public async Task AddGroupMemberAsync(string conversationId, string newMemberUsername)
        {
            var conversations = await GetMyConversationsAsync();
            var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);

            if (conversation == null || conversation.ConversationType != ConversationType.Group)
            {
                throw new Exception("Group not found");
            }

            if (!conversation.Participants.Contains(newMemberUsername))
            {
                conversation.Participants.Add(newMemberUsername);
                // TODO: Update in database
                // await _cosmosDb.UpdateConversationAsync(conversation);

                // Send system message
                var systemMessage = new ChatMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    ConversationId = conversationId,
                    SenderUsername = "System",
                    Message = $"{newMemberUsername} was added to the group",
                    Timestamp = DateTime.Now,
                    Type = MessageType.System,
                    Channel = ChatChannel.Group,
                    Participants = conversation.Participants
                };
                await _cosmosDb.AddChatMessageAsync(systemMessage);
            }
        }

        /// <summary>
        /// Leave a group chat
        /// </summary>
        public async Task LeaveGroupAsync(string conversationId)
        {
            var conversations = await GetMyConversationsAsync();
            var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);

            if (conversation != null && conversation.ConversationType == ConversationType.Group)
            {
                conversation.Participants.Remove(_currentUsername);
                // TODO: Update in database
                // await _cosmosDb.UpdateConversationAsync(conversation);

                // Send system message
                var systemMessage = new ChatMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    ConversationId = conversationId,
                    SenderUsername = "System",
                    Message = $"{_currentUsername} left the group",
                    Timestamp = DateTime.Now,
                    Type = MessageType.System,
                    Channel = ChatChannel.Group,
                    Participants = conversation.Participants
                };
                await _cosmosDb.AddChatMessageAsync(systemMessage);
            }
        }

        #endregion
    }
}
