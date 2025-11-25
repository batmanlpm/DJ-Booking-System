using System;
using System.Windows;
using System.Windows.Input;
using DJBookingSystem.Models;

namespace DJBookingSystem
{
#pragma warning disable CS0618 // Obsolete OpenTime/CloseTime usage

    public partial class VenueRegistrationWindow : Window
    {
        public Venue? RegisteredVenue { get; private set; }
        private string _ownerUsername;

        public VenueRegistrationWindow(string ownerUsername, bool stayOnTop = false)
        {
            InitializeComponent();
            _ownerUsername = ownerUsername;

            // Apply Stay on Top preference
            this.Topmost = stayOnTop;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void RegisterVenue_Click(object sender, RoutedEventArgs e)
        {
            // Validate all required fields
            if (string.IsNullOrWhiteSpace(RoomNameTextBox.Text))
            {
                MessageBox.Show("Please enter the room name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                RoomNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(RoomDescriptionTextBox.Text))
            {
                MessageBox.Show("Please enter the room description.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                RoomDescriptionTextBox.Focus();
                return;
            }

            // Validate Discord webhook if provided
            string discordWebhook = DiscordWebhookTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(discordWebhook))
            {
                if (!discordWebhook.StartsWith("https://discord.com/api/webhooks/") &&
                    !discordWebhook.StartsWith("https://discordapp.com/api/webhooks/"))
                {
                    MessageBox.Show("Invalid Discord webhook URL. It should start with https://discord.com/api/webhooks/",
                        "Invalid Webhook", MessageBoxButton.OK, MessageBoxImage.Warning);
                    DiscordWebhookTextBox.Focus();
                    return;
                }
            }

            // Create the venue object with simplified model
            RegisteredVenue = new Venue
            {
                Name = RoomNameTextBox.Text.Trim(),
                Description = RoomDescriptionTextBox.Text.Trim(),
                OpenTime = "18:00", // Default opening time
                CloseTime = "02:00", // Default closing time
                IsActive = true,
                CreatedAt = DateTime.Now,
                OwnerUsername = _ownerUsername
            };

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
