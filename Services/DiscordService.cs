using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Discord;
using Discord.WebSocket;
using System.Linq;

namespace DJBookingSystem.Services
{
    public class DiscordService : IDisposable
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private DiscordSocketClient? _discordClient;
        private ulong _channelId;
        private bool _isConnected = false;
        private Dictionary<ulong, IDMChannel> _dmChannels = new Dictionary<ulong, IDMChannel>();

        // Events for incoming messages
        public event EventHandler<DiscordMessageReceivedEventArgs>? MessageReceived;
        public event EventHandler<string>? ConnectionStatusChanged;
        public event EventHandler<string>? ErrorOccurred;

        public bool IsConnected => _isConnected && _discordClient?.ConnectionState == ConnectionState.Connected;

        /// <summary>
        /// Connect to Discord using bot token
        /// </summary>
        public async Task ConnectAsync(string botToken, ulong channelId)
        {
            try
            {
                _channelId = channelId;

                // Initialize Discord client
                var config = new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.Guilds | 
                                   GatewayIntents.GuildMessages | 
                                   GatewayIntents.MessageContent | 
                                   GatewayIntents.DirectMessages |
                                   GatewayIntents.GuildMembers,
                    AlwaysDownloadUsers = true,
                    LogLevel = LogSeverity.Debug // Add more verbose logging
                };

                System.Diagnostics.Debug.WriteLine($"[Discord] Connecting with intents: {config.GatewayIntents}");

                _discordClient = new DiscordSocketClient(config);

                // Setup event handlers
                _discordClient.Log += LogAsync;
                _discordClient.Ready += ReadyAsync;
                _discordClient.MessageReceived += MessageReceivedAsync;
                _discordClient.Disconnected += DisconnectedAsync;

                // Connect
                await _discordClient.LoginAsync(TokenType.Bot, botToken);
                await _discordClient.StartAsync();

                ConnectionStatusChanged?.Invoke(this, "Connecting to Discord...");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Discord] Connection error: {ex}");
                ErrorOccurred?.Invoke(this, $"Connection error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Disconnect from Discord
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (_discordClient != null)
            {
                await _discordClient.StopAsync();
                await _discordClient.LogoutAsync();
                _isConnected = false;
                _dmChannels.Clear();
                ConnectionStatusChanged?.Invoke(this, "Disconnected from Discord");
            }
        }

        /// <summary>
        /// Send a message to the configured Discord channel
        /// </summary>
        public async Task SendChannelMessageAsync(string message)
        {
            if (_discordClient == null || !IsConnected)
                throw new InvalidOperationException("Not connected to Discord");

            try
            {
                var channel = await _discordClient.GetChannelAsync(_channelId) as IMessageChannel;
                if (channel != null)
                {
                    await channel.SendMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Send message error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Send a direct message to a specific Discord user
        /// </summary>
        public async Task SendDirectMessageAsync(ulong userId, string message)
        {
            if (_discordClient == null || !IsConnected)
                throw new InvalidOperationException("Not connected to Discord");

            try
            {
                // Get or create DM channel
                IDMChannel? dmChannel;
                if (!_dmChannels.TryGetValue(userId, out dmChannel))
                {
                    var user = await _discordClient.GetUserAsync(userId);
                    if (user == null)
                        throw new InvalidOperationException($"User with ID {userId} not found");

                    dmChannel = await user.CreateDMChannelAsync();
                    _dmChannels[userId] = dmChannel;
                }

                await dmChannel.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Send DM error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get a list of all users in the guild (server)
        /// </summary>
        public async Task<List<DiscordUserInfo>> GetGuildUsersAsync()
        {
            if (_discordClient == null || !IsConnected)
                throw new InvalidOperationException("Not connected to Discord");

            try
            {
                System.Diagnostics.Debug.WriteLine("[Discord] Getting guild users...");
                var users = new List<DiscordUserInfo>();
                
                // Get the guild from the channel
                var channel = await _discordClient.GetChannelAsync(_channelId) as SocketGuildChannel;
                if (channel == null)
                {
                    System.Diagnostics.Debug.WriteLine("[Discord] Channel is null or not a guild channel");
                    throw new InvalidOperationException("Channel must be a guild channel to retrieve users");
                }

                var guild = channel.Guild;
                System.Diagnostics.Debug.WriteLine($"[Discord] Found guild: {guild.Name} (ID: {guild.Id})");
                System.Diagnostics.Debug.WriteLine($"[Discord] Guild has {guild.MemberCount} members (before download)");

                // Download users - this is where GuildMembers intent is required
                await guild.DownloadUsersAsync();
                System.Diagnostics.Debug.WriteLine($"[Discord] Downloaded users. Guild.Users count: {guild.Users.Count}");

                foreach (var user in guild.Users)
                {
                    // Skip bots
                    if (user.IsBot)
                        continue;

                    users.Add(new DiscordUserInfo
                    {
                        Id = user.Id,
                        Username = user.Username,
                        DisplayName = user.DisplayName ?? user.Username,
                        AvatarUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl(),
                        IsOnline = user.Status == UserStatus.Online
                    });
                }

                System.Diagnostics.Debug.WriteLine($"[Discord] Returning {users.Count} non-bot users");
                return users.OrderBy(u => u.DisplayName).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Discord] Get users error: {ex}");
                ErrorOccurred?.Invoke(this, $"Get users error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get or create a DM channel with a user
        /// </summary>
        public async Task<ulong> GetDMChannelIdAsync(ulong userId)
        {
            if (_discordClient == null || !IsConnected)
                throw new InvalidOperationException("Not connected to Discord");

            try
            {
                IDMChannel? dmChannel;
                if (!_dmChannels.TryGetValue(userId, out dmChannel))
                {
                    var user = await _discordClient.GetUserAsync(userId);
                    if (user == null)
                        throw new InvalidOperationException($"User with ID {userId} not found");

                    dmChannel = await user.CreateDMChannelAsync();
                    _dmChannels[userId] = dmChannel;
                }

                return dmChannel.Id;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Get DM channel error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Send an embed message to the configured Discord channel
        /// </summary>
        public async Task SendEmbedMessageAsync(string title, string description, Dictionary<string, string>? fields = null, Color? color = null)
        {
            if (_discordClient == null || !IsConnected)
                throw new InvalidOperationException("Not connected to Discord");

            try
            {
                var channel = await _discordClient.GetChannelAsync(_channelId) as IMessageChannel;
                if (channel != null)
                {
                    var embedBuilder = new EmbedBuilder()
                        .WithTitle(title)
                        .WithDescription(description)
                        .WithColor(color ?? Color.Blue)
                        .WithCurrentTimestamp();

                    if (fields != null)
                    {
                        foreach (var field in fields)
                        {
                            embedBuilder.AddField(field.Key, field.Value, inline: true);
                        }
                    }

                    await channel.SendMessageAsync(embed: embedBuilder.Build());
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Send embed error: {ex.Message}");
                throw;
            }
        }

        #region Event Handlers

        private Task LogAsync(LogMessage log)
        {
            System.Diagnostics.Debug.WriteLine($"[Discord] {log}");
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            _isConnected = true;
            ConnectionStatusChanged?.Invoke(this, $"Connected as {_discordClient?.CurrentUser.Username}");
            System.Diagnostics.Debug.WriteLine($"[Discord] Bot is ready! Connected as {_discordClient?.CurrentUser.Username}");
            return Task.CompletedTask;
        }

        private Task MessageReceivedAsync(SocketMessage message)
        {
            // Ignore messages from bots (including ourselves)
            if (message.Author.IsBot)
                return Task.CompletedTask;

            bool isDirectMessage = message.Channel is IDMChannel;
            bool isTargetChannel = message.Channel.Id == _channelId;

            // Process messages from configured channel OR direct messages
            if (!isTargetChannel && !isDirectMessage)
                return Task.CompletedTask;

            try
            {
                MessageReceived?.Invoke(this, new DiscordMessageReceivedEventArgs
                {
                    Author = message.Author.Username,
                    Content = message.Content,
                    Timestamp = message.Timestamp.DateTime,
                    AuthorAvatarUrl = message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl(),
                    MessageId = message.Id.ToString(),
                    IsDirectMessage = isDirectMessage,
                    AuthorId = message.Author.Id,
                    ChannelId = message.Channel.Id
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Discord] Message processing error: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        private Task DisconnectedAsync(Exception exception)
        {
            _isConnected = false;
            string errorMsg = exception != null ? $"Disconnected: {exception.Message}" : "Disconnected from Discord";
            ConnectionStatusChanged?.Invoke(this, errorMsg);
            System.Diagnostics.Debug.WriteLine($"[Discord] {errorMsg}");
            return Task.CompletedTask;
        }

        #endregion

        #region Legacy Webhook Methods (for backward compatibility)

        /// <summary>
        /// Send a booking notification to Discord via webhook
        /// </summary>
        public static async Task SendBookingNotificationAsync(string webhookUrl, string djName, string venueName, DateTime bookingTime, int availableSlots)
        {
            if (string.IsNullOrEmpty(webhookUrl))
                return;

            try
            {
                var embed = new
                {
                    embeds = new[]
                    {
                        new
                        {
                            title = "🎉 New Booking!",
                            description = $"A new DJ booking has been made at **{venueName}**",
                            color = 3447003, // Blue color
                            fields = new[]
                            {
                                new { name = "DJ Name", value = djName, inline = true },
                                new { name = "Venue", value = venueName, inline = true },
                                new { name = "Time", value = bookingTime.ToString("HH:mm"), inline = true },
                                new { name = "Date", value = bookingTime.ToString("dddd, MMMM dd, yyyy"), inline = false },
                                new { name = "Available Slots Left Today", value = availableSlots.ToString(), inline = true }
                            },
                            footer = new
                            {
                                text = "DJ Booking System"
                            },
                            timestamp = DateTime.UtcNow.ToString("o")
                        }
                    }
                };

                string json = JsonConvert.SerializeObject(embed);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(webhookUrl, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Discord] Send notification error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Send a simple text message to Discord via webhook
        /// </summary>
        public static async Task SendMessageAsync(string webhookUrl, string message, string? username = null)
        {
            if (string.IsNullOrEmpty(webhookUrl))
                return;

            try
            {
                var payload = new
                {
                    content = message,
                    username = username ?? "DJ Booking System"
                };

                string json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(webhookUrl, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Discord] Send message error: {ex.Message}");
                throw;
            }
        }

        #endregion

        public void Dispose()
        {
            if (_discordClient != null)
            {
                DisconnectAsync().GetAwaiter().GetResult();
                _discordClient.Dispose();
            }
        }
    }

    /// <summary>
    /// Event args for Discord message received
    /// </summary>
    public class DiscordMessageReceivedEventArgs : EventArgs
    {
        public string Author { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? AuthorAvatarUrl { get; set; }
        public string? MessageId { get; set; }
        public bool IsDirectMessage { get; set; }
        public ulong AuthorId { get; set; }
        public ulong ChannelId { get; set; }
    }

    /// <summary>
    /// Discord user information
    /// </summary>
    public class DiscordUserInfo
    {
        public ulong Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public bool IsOnline { get; set; }
    }
}
