using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using DJBookingSystem.Models;
using UserModel = DJBookingSystem.Models.User;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Service for interacting with Azure Cosmos DB
    /// Manages all database operations for users, bookings, venues, chat, settings, and reports
    /// </summary>
    public class CosmosDbService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Database _database;
        private readonly Container _usersContainer;
        private readonly Container _bookingsContainer;
        private readonly Container _venuesContainer;
        private readonly Container _chatContainer;
        private readonly Container _settingsContainer;
        private readonly Container _radioStationsContainer;
        private readonly Container _reportsContainer;
        private readonly Container _onlineStatusContainer;

        public CosmosDbService(string connectionString, string databaseName = "DJBookingDB")
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));

            _cosmosClient = new CosmosClient(connectionString);
            _database = _cosmosClient.GetDatabase(databaseName);
            
            _usersContainer = _database.GetContainer("users");  // lowercase for correct partition key!
            _bookingsContainer = _database.GetContainer("Bookings");
            _venuesContainer = _database.GetContainer("Venues");
            _chatContainer = _database.GetContainer("Chat");
            _settingsContainer = _database.GetContainer("Settings");
            _radioStationsContainer = _database.GetContainer("RadioStations");
            _reportsContainer = _database.GetContainer("Reports");
            _onlineStatusContainer = _database.GetContainer("OnlineStatus");
        }

        /// <summary>
        /// Initialize database and containers with optimized partition keys
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            var databaseResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(
                id: "DJBookingDB",
                throughput: 400
            );
            
            Database database = databaseResponse.Database;
            
            await database.CreateContainerIfNotExistsAsync("users", "/username");  // FIXED: lowercase to match JSON property
            await database.CreateContainerIfNotExistsAsync("Bookings", "/Venue");
            await database.CreateContainerIfNotExistsAsync("Venues", "/OwnerUsername");
            await database.CreateContainerIfNotExistsAsync("Chat", "/Channel");
            await database.CreateContainerIfNotExistsAsync("Settings", "/Type");
            await database.CreateContainerIfNotExistsAsync("RadioStations", "/id");
            await database.CreateContainerIfNotExistsAsync("Reports", "/ReportedUsername");
            await database.CreateContainerIfNotExistsAsync("OnlineStatus", "/Username");
            await database.CreateContainerIfNotExistsAsync("FriendRequests", "/toUsername");
            await database.CreateContainerIfNotExistsAsync("Friendships", "/user1");
        }

        #region User Management

        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            var query = _usersContainer.GetItemQueryIterator<UserModel>();
            var users = new List<UserModel>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                users.AddRange(response);
            }
            return users;
        }

        public async Task<UserModel?> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                System.Diagnostics.Debug.WriteLine("[CosmosDB] GetUserByUsernameAsync called with null/empty username");
                return null;
            }

            var query = new QueryDefinition("SELECT * FROM c WHERE c.Username = @username")
                .WithParameter("@username", username);
            var iterator = _usersContainer.GetItemQueryIterator<UserModel>(query);
            
            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                return response.FirstOrDefault();
            }
            return null;
        }

        public async Task<UserModel?> GetUserByIdAsync(string id)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                    .WithParameter("@id", id);
                var iterator = _usersContainer.GetItemQueryIterator<UserModel>(query);
                
                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    return response.FirstOrDefault();
                }
                return null;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<string> AddUserAsync(UserModel user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            
            if (string.IsNullOrWhiteSpace(user.Username))
                throw new ArgumentException("Username cannot be null or empty", nameof(user));

            user.Id = Guid.NewGuid().ToString();
            var response = await _usersContainer.CreateItemAsync(user, new PartitionKey(user.Username));
            System.Diagnostics.Debug.WriteLine($"[CosmosDB] User created: {user.Username} (ID: {user.Id})");
            return response.Resource.Id!;
        }

        public async Task UpdateUserAsync(UserModel user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            
            if (string.IsNullOrEmpty(user.Id))
                throw new ArgumentException("User ID cannot be null or empty", nameof(user));
            
            if (string.IsNullOrWhiteSpace(user.Username))
                throw new ArgumentException("Username cannot be null or empty", nameof(user));

            await _usersContainer.ReplaceItemAsync(user, user.Id, new PartitionKey(user.Username));
            System.Diagnostics.Debug.WriteLine($"[CosmosDB] User updated: {user.Username} (ID: {user.Id})");
        }

        public async Task DeleteUserAsync(string id, string username)
        {
            try
            {
                await _usersContainer.DeleteItemAsync<UserModel>(id, new PartitionKey(username));
                System.Diagnostics.Debug.WriteLine($"[CosmosDB] User deleted: {username} (ID: {id})");
            }
            catch (CosmosException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CosmosDB] Failed to delete user {username}: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteUserAccountAsync(string username)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[CosmosDB] DeleteUserAccountAsync called with username: '{username}'");
                System.Diagnostics.Debug.WriteLine($"[CosmosDB] Username type: {username?.GetType().Name ?? "null"}");
                System.Diagnostics.Debug.WriteLine($"[CosmosDB] Username length: {username?.Length ?? 0}");
                
                var user = await GetUserByUsernameAsync(username);
                
                if (user?.Id != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[CosmosDB] Found user - ID: '{user.Id}', Username: '{user.Username}'");
                    await DeleteUserAsync(user.Id, user.Username);
                    System.Diagnostics.Debug.WriteLine($"[CosmosDB] User account deleted successfully: {username}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[CosmosDB] User not found for deletion: '{username}'");
                    System.Diagnostics.Debug.WriteLine($"[CosmosDB] GetUserByUsernameAsync returned: {(user == null ? "null" : "user with null ID")}");
                    throw new InvalidOperationException($"User '{username}' not found");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CosmosDB] Error deleting user account '{username}': {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[CosmosDB] Exception type: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"[CosmosDB] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task InitializeDefaultAdminAsync()
        {
            var users = await GetAllUsersAsync();
            if (!users.Any(u => u.Username.Equals("SysAdmin", StringComparison.OrdinalIgnoreCase)))
            {
                string passwordHash = HashPassword("Fraser1960@");
                await AddUserAsync(new UserModel
                {
                    Username = "SysAdmin",
                    PasswordHash = passwordHash,
                    FullName = "System Administrator",
                    Email = "sysadmin@djbooking.com",
                    Role = UserRole.SysAdmin,
                    Permissions = new UserPermissions(),
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    IPHistory = new List<string>(),
                    AppPreferences = new UserAppPreferences
                    {
                        ThemeName = "Default",
                        AutoLogin = false,
                        RememberMe = false
                    }
                });
            }
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }

        public async Task<UserModel?> GetBannedUserByIPAsync(string ip)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.IsBanned = true AND ARRAY_CONTAINS(c.IPHistory, @ip)")
                .WithParameter("@ip", ip);
            var iterator = _usersContainer.GetItemQueryIterator<UserModel>(query);
            
            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                return response.FirstOrDefault();
            }
            return null;
        }

        public async Task<List<UserModel>> GetBannedUsersAsync()
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.IsBanned = true");
            return await ExecuteQueryAsync<UserModel>(_usersContainer, query);
        }

        public async Task<List<UserModel>> GetMutedUsersAsync()
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.IsGloballyMuted = true");
            return await ExecuteQueryAsync<UserModel>(_usersContainer, query);
        }

        public async Task AddUserActionLogAsync(UserActionLog log)
        {
            await _reportsContainer.CreateItemAsync(log, new PartitionKey(log.UserId));
        }

        private static async Task<List<T>> ExecuteQueryAsync<T>(Container container, QueryDefinition query)
        {
            var iterator = container.GetItemQueryIterator<T>(query);
            var results = new List<T>();
            
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }
            
            return results;
        }

        #endregion

        #region Friends and Friend Requests

        // Friend Requests Container
        private Container _friendRequestsContainer => _database.GetContainer("FriendRequests");
        private Container _friendshipsContainer => _database.GetContainer("Friendships");

        /// <summary>
        /// Create a friend request
        /// </summary>
        public async Task CreateFriendRequestAsync(FriendRequest request)
        {
            await _friendRequestsContainer.CreateItemAsync(request, new PartitionKey(request.ToUsername));
        }

        /// <summary>
        /// Get friend request by ID
        /// </summary>
        public async Task<FriendRequest?> GetFriendRequestByIdAsync(string requestId)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id AND c.type = 'FriendRequest'")
                    .WithParameter("@id", requestId);
                var iterator = _friendRequestsContainer.GetItemQueryIterator<FriendRequest>(query);

                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    return response.FirstOrDefault();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Update a friend request status
        /// </summary>
        public async Task UpdateFriendRequestAsync(FriendRequest request)
        {
            await _friendRequestsContainer.ReplaceItemAsync(request, request.Id, new PartitionKey(request.ToUsername));
        }

        /// <summary>
        /// Get friend requests sent to a user
        /// </summary>
        public async Task<List<FriendRequest>> GetFriendRequestsByRecipientAsync(string username, FriendRequestStatus status)
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.type = 'FriendRequest' AND c.toUsername = @username AND c.status = @status")
                .WithParameter("@username", username)
                .WithParameter("@status", status.ToString());

            return await ExecuteQueryAsync<FriendRequest>(_friendRequestsContainer, query);
        }

        /// <summary>
        /// Get friend requests sent by a user
        /// </summary>
        public async Task<List<FriendRequest>> GetFriendRequestsBySenderAsync(string username, FriendRequestStatus status)
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.type = 'FriendRequest' AND c.fromUsername = @username AND c.status = @status")
                .WithParameter("@username", username)
                .WithParameter("@status", status.ToString());

            return await ExecuteQueryAsync<FriendRequest>(_friendRequestsContainer, query);
        }

        /// <summary>
        /// Create a friendship
        /// </summary>
        public async Task CreateFriendshipAsync(Friendship friendship)
        {
            await _friendshipsContainer.CreateItemAsync(friendship, new PartitionKey(friendship.User1));
        }

        /// <summary>
        /// Get friendship between two users
        /// </summary>
        public async Task<Friendship?> GetFriendshipAsync(string user1, string user2)
        {
            // Normalize order (alphabetically)
            var users = new List<string> { user1, user2 }.OrderBy(u => u).ToList();

            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.type = 'Friendship' AND c.user1 = @user1 AND c.user2 = @user2")
                .WithParameter("@user1", users[0])
                .WithParameter("@user2", users[1]);

            var iterator = _friendshipsContainer.GetItemQueryIterator<Friendship>(query);

            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                return response.FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// Get all friendships for a user
        /// </summary>
        public async Task<List<Friendship>> GetUserFriendshipsAsync(string username)
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.type = 'Friendship' AND (c.user1 = @username OR c.user2 = @username)")
                .WithParameter("@username", username);

            return await ExecuteQueryAsync<Friendship>(_friendshipsContainer, query);
        }

        /// <summary>
        /// Update a friendship (for metadata changes like nickname, favorite)
        /// </summary>
        public async Task UpdateFriendshipAsync(Friendship friendship)
        {
            await _friendshipsContainer.ReplaceItemAsync(friendship, friendship.Id, new PartitionKey(friendship.User1));
        }

        /// <summary>
        /// Delete a friendship (unfriend)
        /// </summary>
        public async Task DeleteFriendshipAsync(string friendshipId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                .WithParameter("@id", friendshipId);
            var iterator = _friendshipsContainer.GetItemQueryIterator<Friendship>(query);

            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                var friendship = response.FirstOrDefault();
                if (friendship != null)
                {
                    await _friendshipsContainer.DeleteItemAsync<Friendship>(friendshipId, new PartitionKey(friendship.User1));
                }
            }
        }

        #endregion

        #region Booking Management

        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            var query = _bookingsContainer.GetItemQueryIterator<Booking>();
            var bookings = new List<Booking>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                bookings.AddRange(response);
            }
            return bookings;
        }

        public async Task<string> AddBookingAsync(Booking booking)
        {
            if (booking == null)
                throw new ArgumentNullException(nameof(booking));
            
            if (string.IsNullOrWhiteSpace(booking.VenueName))
                throw new ArgumentException("Venue name cannot be null or empty", nameof(booking));

            booking.Id = Guid.NewGuid().ToString();
            var response = await _bookingsContainer.CreateItemAsync(booking, new PartitionKey(booking.VenueName));
            System.Diagnostics.Debug.WriteLine($"[CosmosDB] Booking created: {booking.Id} (Venue: {booking.VenueName}, DJ: {booking.DJName})");
            return response.Resource.Id!;
        }

        public async Task UpdateBookingAsync(string id, Booking booking)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Booking ID cannot be null or empty", nameof(id));
            
            if (booking == null)
                throw new ArgumentNullException(nameof(booking));
            
            if (string.IsNullOrWhiteSpace(booking.VenueName))
                throw new ArgumentException("Venue name cannot be null or empty", nameof(booking));

            booking.Id = id;
            await _bookingsContainer.ReplaceItemAsync(booking, id, new PartitionKey(booking.VenueName));
            System.Diagnostics.Debug.WriteLine($"[CosmosDB] Booking updated: {id} (Venue: {booking.VenueName})");
        }

        public async Task DeleteBookingAsync(string id)
        {
            try
            {
                // Query to find the booking and get its partition key (VenueName)
                var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                    .WithParameter("@id", id);
                var iterator = _bookingsContainer.GetItemQueryIterator<Booking>(query);
                
                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    var booking = response.FirstOrDefault();
                    
                    if (booking != null)
                    {
                        await _bookingsContainer.DeleteItemAsync<Booking>(id, new PartitionKey(booking.VenueName));
                        System.Diagnostics.Debug.WriteLine($"[CosmosDB] Booking deleted: {id} (Venue: {booking.VenueName})");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[CosmosDB] Booking not found for deletion: {id}");
                        throw new InvalidOperationException($"Booking '{id}' not found");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[CosmosDB] Booking not found: {id}");
                    throw new InvalidOperationException($"Booking '{id}' not found");
                }
            }
            catch (CosmosException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CosmosDB] Failed to delete booking {id}: {ex.Message}");
                throw;
            }
        }

        public async Task<Booking?> GetBookingByIdAsync(string id)
        {
            try
            {
                // Need to query since we don't know the partition key (VenueName)
                var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                    .WithParameter("@id", id);
                var iterator = _bookingsContainer.GetItemQueryIterator<Booking>(query);
                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    return response.FirstOrDefault();
                }
                return null;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<List<Booking>> GetBookingsByVenueAsync(string venue)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.Venue = @venue")
                .WithParameter("@venue", venue);
            var iterator = _bookingsContainer.GetItemQueryIterator<Booking>(query);
            var bookings = new List<Booking>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                bookings.AddRange(response);
            }
            return bookings;
        }

        #endregion

        #region Venue Management

        public async Task<List<Venue>> GetAllVenuesAsync()
        {
            var query = _venuesContainer.GetItemQueryIterator<Venue>();
            var venues = new List<Venue>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                venues.AddRange(response);
            }
            return venues;
        }

        public async Task<string> AddVenueAsync(Venue venue)
        {
            if (venue == null)
                throw new ArgumentNullException(nameof(venue));
            
            if (string.IsNullOrWhiteSpace(venue.OwnerUsername))
                throw new ArgumentException("Owner username cannot be null or empty", nameof(venue));

            venue.Id = Guid.NewGuid().ToString();
            var response = await _venuesContainer.CreateItemAsync(venue, new PartitionKey(venue.OwnerUsername));
            System.Diagnostics.Debug.WriteLine($"[CosmosDB] Venue created: {venue.Id} (Name: {venue.Name}, Owner: {venue.OwnerUsername})");
            return response.Resource.Id!;
        }

        public async Task UpdateVenueAsync(string id, Venue venue)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Venue ID cannot be null or empty", nameof(id));
            
            if (venue == null)
                throw new ArgumentNullException(nameof(venue));
            
            if (string.IsNullOrWhiteSpace(venue.OwnerUsername))
                throw new ArgumentException("Owner username cannot be null or empty", nameof(venue));

            venue.Id = id;
            await _venuesContainer.ReplaceItemAsync(venue, id, new PartitionKey(venue.OwnerUsername));
            System.Diagnostics.Debug.WriteLine($"[CosmosDB] Venue updated: {id} (Owner: {venue.OwnerUsername})");
        }

        public async Task DeleteVenueAsync(string id)
        {
            try
            {
                // Query to find the venue and get its partition key (OwnerUsername)
                var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                    .WithParameter("@id", id);
                var iterator = _venuesContainer.GetItemQueryIterator<Venue>(query);
                
                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    var venue = response.FirstOrDefault();
                    
                    if (venue != null)
                    {
                        await _venuesContainer.DeleteItemAsync<Venue>(id, new PartitionKey(venue.OwnerUsername));
                        System.Diagnostics.Debug.WriteLine($"[CosmosDB] Venue deleted: {id} (Owner: {venue.OwnerUsername})");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[CosmosDB] Venue not found for deletion: {id}");
                        throw new InvalidOperationException($"Venue '{id}' not found");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[CosmosDB] Venue not found: {id}");
                    throw new InvalidOperationException($"Venue '{id}' not found");
                }
            }
            catch (CosmosException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CosmosDB] Failed to delete venue {id}: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Chat Management

        public async Task<List<ChatMessage>> GetAllChatMessagesAsync()
        {
            var query = _chatContainer.GetItemQueryIterator<ChatMessage>();
            var messages = new List<ChatMessage>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                messages.AddRange(response);
            }
            return messages.OrderBy(m => m.Timestamp).ToList();
        }

        public async Task<string> AddChatMessageAsync(ChatMessage message)
        {
            message.Id = Guid.NewGuid().ToString();
            var response = await _chatContainer.CreateItemAsync(message, new PartitionKey(message.Channel.ToString()));
            return response.Resource.Id!;
        }

        public async Task<UserChatSettings?> GetUserChatSettingsAsync(string username)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.Username = @username AND c.Type = 'ChatSettings'")
                .WithParameter("@username", username);
            var iterator = _chatContainer.GetItemQueryIterator<UserChatSettings>(query);
            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                return response.FirstOrDefault();
            }
            return null;
        }

        public async Task UpdateUserChatSettingsAsync(UserChatSettings settings)
        {
            if (string.IsNullOrEmpty(settings.Id))
            {
                settings.Id = Guid.NewGuid().ToString();
                // Chat settings stored in Chat container - use a generic partition key
                await _chatContainer.CreateItemAsync(settings, new PartitionKey("Settings"));
            }
            else
            {
                await _chatContainer.ReplaceItemAsync(settings, settings.Id, new PartitionKey("Settings"));
            }
        }

        public async Task LogErrorToChatAsync(string errorMessage, string errorCode, string username)
        {
            var message = new ChatMessage
            {
                SenderUsername = "System",
                Message = $"[ERROR {errorCode}] User: {username} - {errorMessage}",
                Timestamp = DateTime.Now,
                Channel = ChatChannel.ToAdmin,
                IsErrorMessage = true,
                ErrorCode = errorCode
            };
            await AddChatMessageAsync(message);
        }

        #endregion

        #region Moderation

        public async Task BanUserAsync(string username, string moderatorUsername, string reason, DateTime? expiryDate = null)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user != null)
            {
                user.IsBanned = true;
                user.BannedBy = moderatorUsername;
                user.BannedAt = DateTime.Now;
                user.BanReason = reason;
                user.BanExpiry = expiryDate;
                if (!string.IsNullOrEmpty(user.Id))
                {
                    await UpdateUserAsync(user);
                }
            }
        }

        public async Task UnbanUserAsync(string username, string moderatorUsername)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user != null)
            {
                user.IsBanned = false;
                user.BannedBy = null;
                user.BannedAt = null;
                user.BanReason = null;
                user.BanExpiry = null;
                if (!string.IsNullOrEmpty(user.Id))
                {
                    await UpdateUserAsync(user);
                }
            }
        }

        public async Task MuteUserAsync(string username, string moderatorUsername, string reason, DateTime? expiryDate = null)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user != null)
            {
                user.IsGloballyMuted = true;
                user.MutedBy = moderatorUsername;
                user.MutedAt = DateTime.Now;
                user.MuteExpiry = expiryDate;
                if (!string.IsNullOrEmpty(user.Id))
                {
                    await UpdateUserAsync(user);
                }
            }
        }

        public async Task UnmuteUserAsync(string username, string moderatorUsername)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user != null)
            {
                user.IsGloballyMuted = false;
                user.MutedBy = null;
                user.MutedAt = null;
                user.MuteExpiry = null;
                if (!string.IsNullOrEmpty(user.Id))
                {
                    await UpdateUserAsync(user);
                }
            }
        }

        public async Task<List<UserReport>> GetAllUserReportsAsync()
        {
            var query = _reportsContainer.GetItemQueryIterator<UserReport>();
            var reports = new List<UserReport>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                reports.AddRange(response);
            }
            return reports;
        }

        public async Task<string> AddUserReportAsync(UserReport report)
        {
            report.Id = Guid.NewGuid().ToString();
            var response = await _reportsContainer.CreateItemAsync(report, new PartitionKey(report.ReportedUsername));
            return response.Resource.Id!;
        }

        public async Task UpdateUserReportAsync(string id, UserReport report)
        {
            report.Id = id;
            await _reportsContainer.ReplaceItemAsync(report, id, new PartitionKey(report.ReportedUsername));
        }

        #endregion

        #region Radio Stations

        public async Task<List<RadioStation>> GetAllRadioStationsAsync()
        {
            var query = _radioStationsContainer.GetItemQueryIterator<RadioStation>();
            var stations = new List<RadioStation>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                stations.AddRange(response);
            }
            return stations;
        }

        public async Task<string> AddRadioStationAsync(RadioStation station)
        {
            station.Id = Guid.NewGuid().ToString();
            var response = await _radioStationsContainer.CreateItemAsync(station, new PartitionKey(station.Id));
            return response.Resource.Id!;
        }

        public async Task DeleteRadioStationAsync(string id)
        {
            await _radioStationsContainer.DeleteItemAsync<RadioStation>(id, new PartitionKey(id));
        }

        #endregion

        #region Online Status Tracking

        /// <summary>
        /// Update user's online status in the cloud
        /// </summary>
        public async Task UpdateUserOnlineStatusAsync(string username, bool isOnline, DateTime lastActivity)
        {
            try
            {
                var status = new OnlineUserStatus
                {
                    Id = username,
                    Username = username,
                    IsOnline = isOnline,
                    LastActivity = lastActivity,
                    LastHeartbeat = DateTime.UtcNow
                };

                await _onlineStatusContainer.UpsertItemAsync(status, new PartitionKey(username));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to update online status: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all currently online users (active within last 5 minutes)
        /// </summary>
        public async Task<List<OnlineUserStatus>> GetOnlineUsersAsync()
        {
            try
            {
                var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);
                var query = new QueryDefinition(
                    "SELECT * FROM c WHERE c.IsOnline = true AND c.LastHeartbeat >= @cutoff")
                    .WithParameter("@cutoff", fiveMinutesAgo);

                var iterator = _onlineStatusContainer.GetItemQueryIterator<OnlineUserStatus>(query);
                var onlineUsers = new List<OnlineUserStatus>();

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    onlineUsers.AddRange(response);
                }

                return onlineUsers;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get online users: {ex.Message}");
                return new List<OnlineUserStatus>();
            }
        }

        /// <summary>
        /// Mark user as offline
        /// </summary>
        public async Task MarkUserOfflineAsync(string username)
        {
            try
            {
                await UpdateUserOnlineStatusAsync(username, false, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to mark user offline: {ex.Message}");
            }
        }

        /// <summary>
        /// Cleanup stale online status records (older than 1 hour)
        /// </summary>
        public async Task CleanupStaleOnlineStatusAsync()
        {
            try
            {
                var oneHourAgo = DateTime.UtcNow.AddHours(-1);
                var query = new QueryDefinition(
                    "SELECT * FROM c WHERE c.LastHeartbeat < @cutoff")
                    .WithParameter("@cutoff", oneHourAgo);

                var iterator = _onlineStatusContainer.GetItemQueryIterator<OnlineUserStatus>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    foreach (var status in response)
                    {
                        await _onlineStatusContainer.DeleteItemAsync<OnlineUserStatus>(
                            status.Id, 
                            new PartitionKey(status.Username));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to cleanup stale online status: {ex.Message}");
            }
        }

        #endregion

        #region Reports and Audit Logs

        /// <summary>
        /// Get all error reports from the system
        /// </summary>
        public async Task<List<UserErrorReport>> GetAllReportsAsync()
        {
            try
            {
                var query = _reportsContainer.GetItemQueryIterator<UserErrorReport>();
                var reports = new List<UserErrorReport>();
                
                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    reports.AddRange(response);
                }
                
                return reports.OrderByDescending(r => r.Timestamp).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CosmosDB] Failed to get reports: {ex.Message}");
                return new List<UserErrorReport>();
            }
        }

        /// <summary>
        /// Get all user action logs (audit trail)
        /// </summary>
        public async Task<List<UserActionLog>> GetUserActionLogsAsync()
        {
            try
            {
                var query = _reportsContainer.GetItemQueryIterator<UserActionLog>();
                var logs = new List<UserActionLog>();
                
                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    logs.AddRange(response);
                }
                
                return logs.OrderByDescending(l => l.Timestamp).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CosmosDB] Failed to get action logs: {ex.Message}");
                return new List<UserActionLog>();
            }
        }

        #endregion
    }
}
