using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class IntegratedChatWindow : Window
    {
        private readonly User _currentUser;
        private readonly CosmosDbService _cosmosDbService;
        private readonly FriendsService _friendsService;
        private ObservableCollection<ChatMessage> _messages = new ObservableCollection<ChatMessage>();
        private ObservableCollection<FriendListEntry> _friends = new ObservableCollection<FriendListEntry>();
        private DispatcherTimer _refreshTimer;
        private DispatcherTimer _friendsRefreshTimer;
        private ChatChannel _currentChannel = ChatChannel.World;
        private string? _currentDmUsername = null; // Track current DM conversation

        // Public property for binding
        public ObservableCollection<FriendListEntry> Friends => _friends;

        public IntegratedChatWindow(User currentUser, CosmosDbService cosmosDbService)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _cosmosDbService = cosmosDbService;
            _friendsService = new FriendsService(cosmosDbService, currentUser.Username);

            MessagesPanel.ItemsSource = _messages;
            DataContext = this; // Set DataContext for binding

            // Show admin channel if user is admin
            if (_currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager)
            {
                AdminChannelLabel.Visibility = Visibility.Visible;
                AdminOnlyButton.Visibility = Visibility.Visible;
            }

            // Auto-refresh messages every 3 seconds
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _refreshTimer.Tick += async (s, e) => await LoadMessagesAsync();
            _refreshTimer.Start();

            // Auto-refresh friends list every 10 seconds
            _friendsRefreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            _friendsRefreshTimer.Tick += async (s, e) => await LoadFriendsListAsync();
            _friendsRefreshTimer.Start();

            // Initial load
            Loaded += async (s, e) => 
            {
                await LoadMessagesAsync();
                await LoadFriendsListAsync();
                await UpdatePendingRequestsBadgeAsync();
            };
        }

        private async System.Threading.Tasks.Task LoadMessagesAsync()
        {
            try
            {
                List<ChatMessage> channelMessages;

                if (_currentChannel == ChatChannel.Private && !string.IsNullOrEmpty(_currentDmUsername))
                {
                    // Load DM messages
                    var allMessages = await _cosmosDbService.GetAllChatMessagesAsync();
                    channelMessages = allMessages
                        .Where(m => m.Channel == ChatChannel.Private &&
                                   ((m.SenderUsername == _currentUser.Username && m.RecipientUsername == _currentDmUsername) ||
                                    (m.SenderUsername == _currentDmUsername && m.RecipientUsername == _currentUser.Username)))
                        .OrderBy(m => m.Timestamp)
                        .ToList();
                }
                else
                {
                    // Get all chat messages from Cosmos DB
                    var allMessages = await _cosmosDbService.GetAllChatMessagesAsync();
                    
                    // Filter by current channel
                    channelMessages = allMessages
                        .Where(m => m.Channel == _currentChannel)
                        .OrderBy(m => m.Timestamp)
                        .ToList();
                }

                // Update UI
                _messages.Clear();
                foreach (var msg in channelMessages)
                {
                    _messages.Add(msg);
                }

                // Update online user count
                var onlineUsers = await OnlineUserStatusService.Instance.GetOnlineUsersAsync();
                OnlineUsersText.Text = $"-> {onlineUsers.Count} users online";

                string channelName = _currentChannel == ChatChannel.Private && !string.IsNullOrEmpty(_currentDmUsername)
                    ? $"DM with {_currentDmUsername}"
                    : _currentChannel.ToString();

                StatusText.Text = $"Connected to {channelName} channel - {channelMessages.Count} messages - Last updated: {DateTime.Now:HH:mm:ss}";
                
                // Auto-scroll to bottom
                MessagesScrollViewer.ScrollToEnd();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error loading messages: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[Chat] Error: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LoadFriendsListAsync()
        {
            try
            {
                var friendsList = await _friendsService.GetFriendsListAsync();

                _friends.Clear();

                if (friendsList.Any())
                {
                    foreach (var friend in friendsList)
                    {
                        _friends.Add(friend);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Friends] Error loading friends list: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task UpdatePendingRequestsBadgeAsync()
        {
            try
            {
                var pendingRequests = await _friendsService.GetPendingFriendRequestsAsync();
                var count = pendingRequests.Count;

                System.Diagnostics.Debug.WriteLine($"[Friends] {count} pending friend requests");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Friends] Error updating badge: {ex.Message}");
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            await SendMessageAsync();
        }

        private async void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                await SendMessageAsync();
            }
        }

        private async System.Threading.Tasks.Task SendMessageAsync()
        {
            var message = MessageInput.Text.Trim();
            if (string.IsNullOrEmpty(message) || message == "Type your message...") return;

            try
            {
                SendButton.IsEnabled = false;
                SendButton.Content = "Sending...";

                var chatMessage = new ChatMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    SenderUsername = _currentUser.Username,
                    SenderRole = _currentUser.Role.ToString(),
                    Message = message,
                    Timestamp = DateTime.Now,
                    Channel = _currentChannel,
                    SenderIsDJ = _currentUser.IsDJ,
                    SenderIsVenueOwner = _currentUser.IsVenueOwner,
                    Type = MessageType.Normal
                };

                // Set up DM-specific fields
                if (_currentChannel == ChatChannel.Private && !string.IsNullOrEmpty(_currentDmUsername))
                {
                    chatMessage.RecipientUsername = _currentDmUsername;
                    chatMessage.Participants = new List<string> { _currentUser.Username, _currentDmUsername };
                    
                    // Create conversation ID (alphabetically ordered for consistency)
                    var users = new List<string> { _currentUser.Username, _currentDmUsername }.OrderBy(u => u).ToList();
                    chatMessage.ConversationId = $"private:{users[0]}:{users[1]}";
                }
                else
                {
                    chatMessage.ConversationId = _currentChannel.ToString().ToLower();
                }

                // Save to Cosmos DB
                await _cosmosDbService.AddChatMessageAsync(chatMessage);

                // Add to local list immediately (optimistic update)
                _messages.Add(chatMessage);
                MessageInput.Clear();
                MessagesScrollViewer.ScrollToEnd();

                SendButton.Content = "SEND";
                SendButton.IsEnabled = true;

                System.Diagnostics.Debug.WriteLine($"[Chat] Message sent to {_currentChannel}: {message}");
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Failed to send message: {ex.Message}";
                SendButton.Content = "SEND";
                SendButton.IsEnabled = true;
                System.Diagnostics.Debug.WriteLine($"[Chat] Send error: {ex.Message}");
            }
        }

        private async void WorldChat_Click(object sender, RoutedEventArgs e)
        {
            _currentChannel = ChatChannel.World;
            HeaderText.Text = "INTEGRATED CHAT - World";
            _messages.Clear();
            await LoadMessagesAsync();
        }

        private async void SupportTicket_Click(object sender, RoutedEventArgs e)
        {
            _currentChannel = ChatChannel.ToAdmin;
            HeaderText.Text = "INTEGRATED CHAT - Support Tickets";
            _messages.Clear();
            await LoadMessagesAsync();
        }

        private async void Announcements_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Announcements are read-only. Admins can post announcements.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void AdminOnly_Click(object sender, RoutedEventArgs e)
        {
            _currentChannel = ChatChannel.AdminOnly;
            HeaderText.Text = "INTEGRATED CHAT - Admin Only";
            _messages.Clear();
            await LoadMessagesAsync();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            _refreshTimer?.Stop();
            _friendsRefreshTimer?.Stop();
            Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _refreshTimer?.Stop();
            _friendsRefreshTimer?.Stop();
            base.OnClosing(e);
        }

        #region Friends Management

        private async void AddFriend_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FriendRequestDialog(_friendsService, _cosmosDbService);
            var result = dialog.ShowDialog();

            if (result == true && dialog.RequestSent)
            {
                await UpdatePendingRequestsBadgeAsync();
            }
        }

        private async void ViewRequests_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PendingRequestsDialog(_friendsService);
            var result = dialog.ShowDialog();

            if (result == true && dialog.HasChanges)
            {
                // Refresh friends list and badge
                await LoadFriendsListAsync();
                await UpdatePendingRequestsBadgeAsync();
            }
        }

        private async void Friend_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var friendUsername = button?.Tag as string;

            if (string.IsNullOrEmpty(friendUsername)) return;

            // Open DM conversation
            _currentChannel = ChatChannel.Private;
            _currentDmUsername = friendUsername;
            HeaderText.Text = $"INTEGRATED CHAT - DM with {friendUsername}";
            _messages.Clear();
            await LoadMessagesAsync();
        }

        #endregion
    }
}
