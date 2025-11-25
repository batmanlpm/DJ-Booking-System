using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DJBookingSystem.Models;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Discord-style friends system with requests, accept/decline, and friends list
    /// </summary>
    public class FriendsService
    {
        private readonly CosmosDbService _cosmosDb;
        private readonly OnlineUserStatusService _statusService;
        private readonly string _currentUsername;

        public FriendsService(CosmosDbService cosmosDb, string currentUsername)
        {
            _cosmosDb = cosmosDb;
            _statusService = OnlineUserStatusService.Instance;
            _currentUsername = currentUsername;
        }

        #region Friend Requests

        /// <summary>
        /// Send a friend request to another user
        /// </summary>
        public async Task<FriendRequest> SendFriendRequestAsync(string toUsername, string? message = null)
        {
            // Check if already friends
            if (await AreFriendsAsync(toUsername))
            {
                throw new Exception("You are already friends with this user");
            }

            // Check if request already exists
            var existingRequest = await GetPendingRequestAsync(_currentUsername, toUsername);
            if (existingRequest != null)
            {
                throw new Exception("Friend request already sent");
            }

            // Check if they sent you a request (auto-accept)
            var reverseRequest = await GetPendingRequestAsync(toUsername, _currentUsername);
            if (reverseRequest != null)
            {
                await AcceptFriendRequestAsync(reverseRequest.Id);
                return reverseRequest;
            }

            var request = new FriendRequest
            {
                Id = Guid.NewGuid().ToString(),
                FromUsername = _currentUsername,
                ToUsername = toUsername,
                Status = FriendRequestStatus.Pending,
                SentAt = DateTime.Now,
                Message = message
            };

            await _cosmosDb.CreateFriendRequestAsync(request);

            return request;
        }

        /// <summary>
        /// Accept a friend request
        /// </summary>
        public async Task<Friendship> AcceptFriendRequestAsync(string requestId)
        {
            var request = await _cosmosDb.GetFriendRequestByIdAsync(requestId);

            if (request == null)
            {
                throw new Exception("Friend request not found");
            }

            if (request.ToUsername != _currentUsername)
            {
                throw new Exception("This request is not for you");
            }

            if (request.Status != FriendRequestStatus.Pending)
            {
                throw new Exception("Request is no longer pending");
            }

            // Update request status
            request.Status = FriendRequestStatus.Accepted;
            request.RespondedAt = DateTime.Now;
            await _cosmosDb.UpdateFriendRequestAsync(request);

            // Create friendship
            var friendship = await CreateFriendshipAsync(request.FromUsername, request.ToUsername);

            return friendship;
        }

        /// <summary>
        /// Decline a friend request
        /// </summary>
        public async Task DeclineFriendRequestAsync(string requestId)
        {
            var request = await _cosmosDb.GetFriendRequestByIdAsync(requestId);

            if (request == null)
            {
                throw new Exception("Friend request not found");
            }

            if (request.ToUsername != _currentUsername)
            {
                throw new Exception("This request is not for you");
            }

            request.Status = FriendRequestStatus.Declined;
            request.RespondedAt = DateTime.Now;
            await _cosmosDb.UpdateFriendRequestAsync(request);
        }

        /// <summary>
        /// Cancel a friend request you sent
        /// </summary>
        public async Task CancelFriendRequestAsync(string requestId)
        {
            var request = await _cosmosDb.GetFriendRequestByIdAsync(requestId);

            if (request == null)
            {
                throw new Exception("Friend request not found");
            }

            if (request.FromUsername != _currentUsername)
            {
                throw new Exception("You didn't send this request");
            }

            request.Status = FriendRequestStatus.Cancelled;
            await _cosmosDb.UpdateFriendRequestAsync(request);
        }

        /// <summary>
        /// Get all pending friend requests TO you
        /// </summary>
        public async Task<List<FriendRequest>> GetIncomingRequestsAsync()
        {
            return await _cosmosDb.GetFriendRequestsByRecipientAsync(_currentUsername, FriendRequestStatus.Pending);
        }

        /// <summary>
        /// Get all pending friend requests FROM you
        /// </summary>
        public async Task<List<FriendRequest>> GetOutgoingRequestsAsync()
        {
            return await _cosmosDb.GetFriendRequestsBySenderAsync(_currentUsername, FriendRequestStatus.Pending);
        }

        /// <summary>
        /// Get ALL pending friend requests (incoming + outgoing) for badge count
        /// </summary>
        public async Task<List<FriendRequest>> GetPendingFriendRequestsAsync()
        {
            return await GetIncomingRequestsAsync(); // For now, only show incoming
        }

        #endregion

        #region Friends Management

        /// <summary>
        /// Get your complete friends list with online status
        /// </summary>
        public async Task<List<FriendListEntry>> GetFriendsListAsync()
        {
            var friendships = await _cosmosDb.GetUserFriendshipsAsync(_currentUsername);

            var friendsList = new List<FriendListEntry>();

            foreach (var friendship in friendships)
            {
                var friendUsername = friendship.User1 == _currentUsername ? friendship.User2 : friendship.User1;
                var metadata = friendship.User1 == _currentUsername ? friendship.User1Metadata : friendship.User2Metadata;

                // Get friend's user data
                var friendUser = await _cosmosDb.GetUserByUsernameAsync(friendUsername);
                if (friendUser == null) continue;

                // Check online status
                var isOnline = _statusService.IsUserOnline(friendUsername);
                var lastSeen = isOnline ? null : await GetLastSeenAsync(friendUsername);

                var entry = new FriendListEntry
                {
                    Username = friendUsername,
                    Nickname = metadata.Nickname,
                    Role = friendUser.Role,
                    IsOnline = isOnline,
                    IsFavorite = metadata.IsFavorite,
                    LastSeen = lastSeen,
                    FriendsSince = friendship.CreatedAt,
                    DmConversationId = friendship.DmConversationId
                };

                friendsList.Add(entry);
            }

            // Sort: Favorites first, then online, then alphabetically
            return friendsList
                .OrderByDescending(f => f.IsFavorite)
                .ThenByDescending(f => f.IsOnline)
                .ThenBy(f => f.Nickname ?? f.Username)
                .ToList();
        }

        /// <summary>
        /// Check if two users are friends
        /// </summary>
        public async Task<bool> AreFriendsAsync(string otherUsername)
        {
            var friendship = await _cosmosDb.GetFriendshipAsync(_currentUsername, otherUsername);
            return friendship != null;
        }

        /// <summary>
        /// Remove a friend (unfriend)
        /// </summary>
        public async Task UnfriendAsync(string friendUsername)
        {
            var friendship = await _cosmosDb.GetFriendshipAsync(_currentUsername, friendUsername);
            if (friendship != null)
            {
                await _cosmosDb.DeleteFriendshipAsync(friendship.Id);
            }
        }

        /// <summary>
        /// Set a custom nickname for a friend
        /// </summary>
        public async Task SetFriendNicknameAsync(string friendUsername, string? nickname)
        {
            var friendship = await _cosmosDb.GetFriendshipAsync(_currentUsername, friendUsername);
            if (friendship != null)
            {
                var myMetadata = friendship.User1 == _currentUsername ? friendship.User1Metadata : friendship.User2Metadata;
                myMetadata.Nickname = nickname;
                await _cosmosDb.UpdateFriendshipAsync(friendship);
            }
        }

        /// <summary>
        /// Toggle favorite status for a friend (pins to top)
        /// </summary>
        public async Task ToggleFavoriteFriendAsync(string friendUsername)
        {
            var friendship = await _cosmosDb.GetFriendshipAsync(_currentUsername, friendUsername);
            if (friendship != null)
            {
                var myMetadata = friendship.User1 == _currentUsername ? friendship.User1Metadata : friendship.User2Metadata;
                myMetadata.IsFavorite = !myMetadata.IsFavorite;
                await _cosmosDb.UpdateFriendshipAsync(friendship);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Create a friendship between two users
        /// </summary>
        private async Task<Friendship> CreateFriendshipAsync(string user1, string user2)
        {
            // Store alphabetically for consistent querying
            var users = new List<string> { user1, user2 }.OrderBy(u => u).ToList();

            var friendship = new Friendship
            {
                Id = Guid.NewGuid().ToString(),
                User1 = users[0],
                User2 = users[1],
                CreatedAt = DateTime.Now,
                DmConversationId = $"dm:{users[0]}:{users[1]}" // Link to their DM
            };

            await _cosmosDb.CreateFriendshipAsync(friendship);

            return friendship;
        }

        /// <summary>
        /// Get pending request between two users
        /// </summary>
        private async Task<FriendRequest?> GetPendingRequestAsync(string from, string to)
        {
            var requests = await _cosmosDb.GetFriendRequestsBySenderAsync(from, FriendRequestStatus.Pending);
            return requests.FirstOrDefault(r => r.ToUsername == to);
        }

        /// <summary>
        /// Get user's last seen time
        /// </summary>
        private async Task<DateTime?> GetLastSeenAsync(string username)
        {
            var user = await _cosmosDb.GetUserByUsernameAsync(username);
            return user?.LastLogin;
        }

        #endregion

        #region Search & Discovery

        /// <summary>
        /// Search for users to add as friends
        /// </summary>
        public async Task<List<User>> SearchUsersAsync(string searchTerm)
        {
            var allUsers = await _cosmosDb.GetAllUsersAsync();
            var myFriends = await GetFriendsListAsync();
            var friendUsernames = myFriends.Select(f => f.Username).ToHashSet();

            return allUsers
                .Where(u => u.Username != _currentUsername) // Not yourself
                .Where(u => !friendUsernames.Contains(u.Username)) // Not already friends
                .Where(u => u.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .Take(20)
                .ToList();
        }

        /// <summary>
        /// Get suggested friends (mutual friends, same role, etc.)
        /// </summary>
        public async Task<List<User>> GetFriendSuggestionsAsync()
        {
            // TODO: Implement friend suggestions based on:
            // - Mutual friends
            // - Same role (DJ, Venue Owner)
            // - Recent interactions
            // - Common groups
            return new List<User>();
        }

        #endregion
    }
}
