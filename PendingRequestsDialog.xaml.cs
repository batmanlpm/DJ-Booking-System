using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class PendingRequestsDialog : Window
    {
        private readonly FriendsService _friendsService;
        private ObservableCollection<FriendRequestViewModel> _requests = new ObservableCollection<FriendRequestViewModel>();
        public bool HasChanges { get; private set; } = false;

        public PendingRequestsDialog(FriendsService friendsService)
        {
            InitializeComponent();
            _friendsService = friendsService;

            RequestsList.ItemsSource = _requests;

            Loaded += async (s, e) => await LoadRequestsAsync();
        }

        private async System.Threading.Tasks.Task LoadRequestsAsync()
        {
            try
            {
                var requests = await _friendsService.GetPendingFriendRequestsAsync();

                _requests.Clear();

                if (requests.Any())
                {
                    foreach (var request in requests)
                    {
                        _requests.Add(new FriendRequestViewModel
                        {
                            Id = request.Id,
                            FromUsername = request.FromUsername,
                            Message = request.Message,
                            SentAt = request.SentAt,
                            MessageVisibility = string.IsNullOrEmpty(request.Message) ? Visibility.Collapsed : Visibility.Visible
                        });
                    }

                    NoRequestsText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    NoRequestsText.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading friend requests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Accept_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var requestId = button?.Tag as string;

            if (string.IsNullOrEmpty(requestId)) return;

            try
            {
                button.IsEnabled = false;
                button.Content = "ACCEPTING...";

                await _friendsService.AcceptFriendRequestAsync(requestId);

                HasChanges = true;

                // Remove from list
                var request = _requests.FirstOrDefault(r => r.Id == requestId);
                if (request != null)
                {
                    _requests.Remove(request);
                }

                // Show no requests message if list is empty
                if (!_requests.Any())
                {
                    NoRequestsText.Visibility = Visibility.Visible;
                }

                MessageBox.Show("Friend request accepted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error accepting request: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                button.IsEnabled = true;
                button.Content = "ACCEPT";
            }
        }

        private async void Decline_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var requestId = button?.Tag as string;

            if (string.IsNullOrEmpty(requestId)) return;

            var result = MessageBox.Show("Are you sure you want to decline this friend request?", 
                "Confirm Decline", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                button.IsEnabled = false;
                button.Content = "DECLINING...";

                await _friendsService.DeclineFriendRequestAsync(requestId);

                HasChanges = true;

                // Remove from list
                var request = _requests.FirstOrDefault(r => r.Id == requestId);
                if (request != null)
                {
                    _requests.Remove(request);
                }

                // Show no requests message if list is empty
                if (!_requests.Any())
                {
                    NoRequestsText.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error declining request: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                button.IsEnabled = true;
                button.Content = "DECLINE";
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = HasChanges;
            Close();
        }
    }

    // ViewModel for displaying friend requests
    public class FriendRequestViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FromUsername { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime SentAt { get; set; }
        public Visibility MessageVisibility { get; set; } = Visibility.Collapsed;
    }
}
