using System;
using System.Windows;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class FriendRequestDialog : Window
    {
        private readonly FriendsService _friendsService;
        private readonly CosmosDbService _cosmosDbService;
        public bool RequestSent { get; private set; } = false;

        public FriendRequestDialog(FriendsService friendsService, CosmosDbService cosmosDbService)
        {
            InitializeComponent();
            _friendsService = friendsService;
            _cosmosDbService = cosmosDbService;
            
            UsernameTextBox.Focus();
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text.Trim();
            var message = MessageTextBox.Text.Trim();

            // Validation
            if (string.IsNullOrEmpty(username))
            {
                StatusText.Text = "ERROR: Username is required";
                StatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                return;
            }

            // Check if user exists
            var targetUser = await _cosmosDbService.GetUserByUsernameAsync(username);
            if (targetUser == null)
            {
                StatusText.Text = $"ERROR: User '{username}' not found";
                StatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                return;
            }

            try
            {
                StatusText.Text = "Sending friend request...";
                StatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Yellow);

                await _friendsService.SendFriendRequestAsync(username, string.IsNullOrEmpty(message) ? null : message);

                StatusText.Text = "Friend request sent successfully!";
                StatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Lime);

                RequestSent = true;

                // Close after 1 second
                await System.Threading.Tasks.Task.Delay(1000);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"ERROR: {ex.Message}";
                StatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
