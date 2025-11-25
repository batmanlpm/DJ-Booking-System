using System;
using System.Diagnostics;
using System.Windows;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class BookingDetailWindow : Window
    {
        private CosmosDbService _CosmosDbService;
        private User _currentUser;
        private Booking _booking;
        private bool _canViewStreamingLink = false;

        public BookingDetailWindow(CosmosDbService CosmosDbService, User currentUser, Booking booking)
        {
            InitializeComponent();
            _CosmosDbService = CosmosDbService;
            _currentUser = currentUser;
            _booking = booking;

            LoadBookingDetails();
        }

        private async void LoadBookingDetails()
        {
            // Set basic information (visible to everyone)
            DJNameTextBlock.Text = _booking.DJName;
            VenueTextBlock.Text = _booking.VenueName;
#pragma warning disable CS0618 // Type or member is obsolete
            DateTextBlock.Text = _booking.GetNextOccurrence(DateTime.Now).ToString("d");
#pragma warning restore CS0618 // Type or member is obsolete
            CreatedTextBlock.Text = $"Booking created on {_booking.CreatedAt:MMM dd, yyyy}";

            // Determine if user can view streaming link
            bool isAdmin = _currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager;
            bool isVenueOwner = _currentUser.IsVenueOwner;

            // Check if current user owns this venue
            bool ownsThisVenue = false;
            if (isVenueOwner && !isAdmin)
            {
                try
                {
                    var venues = await _CosmosDbService.GetAllVenuesAsync();
                    ownsThisVenue = venues.Exists(v => v.Name == _booking.VenueName && v.OwnerUsername == _currentUser.Username);
                }
                catch
                {
                    ownsThisVenue = false;
                }
            }

            // Determine access level
            if (isAdmin)
            {
                _canViewStreamingLink = true;
                StreamingLinkPanel.Visibility = Visibility.Visible;
                StreamingLinkTextBox.Text = _booking.StreamingLink;
                AccessInfoTextBlock.Text = "ℹ️ Admin Access: You can view all booking details including streaming links.";
                BookingSubtitleTextBlock.Text = "Full administrative access";
            }
            else if (isVenueOwner && ownsThisVenue)
            {
                _canViewStreamingLink = true;
                StreamingLinkPanel.Visibility = Visibility.Visible;
                StreamingLinkTextBox.Text = _booking.StreamingLink;
                AccessInfoTextBlock.Text = "ℹ️ Venue Owner Access: You can view full details for bookings at your venue including DJ streaming links.";
                BookingSubtitleTextBlock.Text = $"Venue Owner: {_booking.VenueName}";
            }
            else
            {
                _canViewStreamingLink = false;
                StreamingLinkPanel.Visibility = Visibility.Collapsed;
                AccessInfoTextBlock.Text = "ℹ️ Limited Access: Streaming links are only visible to venue owners (for their venues) and administrators.";
                BookingSubtitleTextBlock.Text = "Public booking information";
            }
        }

        private void CopyStreamingLink_Click(object sender, RoutedEventArgs e)
        {
            if (!_canViewStreamingLink)
            {
                MessageBox.Show("You don't have permission to access the streaming link.", "Access Denied",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Clipboard.SetText(_booking.StreamingLink);
                MessageBox.Show("Streaming link copied to clipboard!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy link: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenStreamingLink_Click(object sender, RoutedEventArgs e)
        {
            if (!_canViewStreamingLink)
            {
                MessageBox.Show("You don't have permission to access the streaming link.", "Access Denied",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _booking.StreamingLink,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open link: {ex.Message}\n\nLink: {_booking.StreamingLink}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
