using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem.Views.Bookings
{
#pragma warning disable CS0618 // Obsolete BookingDate usage

    /// <summary>
    /// Interaction logic for BookingsView.xaml
    /// </summary>
    public partial class BookingsView : UserControl
    {
        private readonly CosmosDbService? _cosmosDbService;
        private readonly User _currentUser;
        private System.Collections.Generic.List<Booking>? _allBookings;

        public BookingsView(CosmosDbService? cosmosDbService, User currentUser)
        {
            InitializeComponent();
            _cosmosDbService = cosmosDbService;
            _currentUser = currentUser;
            
            // PERMISSION ENFORCEMENT
            EnforcePermissions();
            
            LoadBookingsAsync();
        }

        /// <summary>
        /// Enforce user permissions for bookings
        /// </summary>
        private void EnforcePermissions()
        {
            if (_currentUser == null || _currentUser.Permissions == null)
            {
                // Deny all if no permissions
                EditBookingButton.IsEnabled = false;
                DeleteBookingButton.IsEnabled = false;
                return;
            }

            // SysAdmin and Manager bypass all permission checks
            bool isAdmin = _currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager;

            if (isAdmin)
            {
                // Admins can do everything
                return;
            }

            // Apply permission-based restrictions
            // CanViewBookings is implicitly granted if they can access this view

            // CanEditBookings
            if (!_currentUser.Permissions.CanEditBookings)
            {
                EditBookingButton.IsEnabled = false;
                EditBookingButton.ToolTip = "You don't have permission to edit bookings";
            }

            // CanDeleteBookings
            if (!_currentUser.Permissions.CanDeleteBookings)
            {
                DeleteBookingButton.IsEnabled = false;
                DeleteBookingButton.ToolTip = "You don't have permission to delete bookings";
            }

            System.Diagnostics.Debug.WriteLine($"[BookingsView] Permissions enforced for {_currentUser.Username}:");
            System.Diagnostics.Debug.WriteLine($"  - Create: {_currentUser.Permissions.CanCreateBookings} (button not in UI)");
            System.Diagnostics.Debug.WriteLine($"  - Edit: {_currentUser.Permissions.CanEditBookings}");
            System.Diagnostics.Debug.WriteLine($"  - Delete: {_currentUser.Permissions.CanDeleteBookings}");
        }

        private async void LoadBookingsAsync()
        {
            try
            {
                if (_cosmosDbService == null) return;

                var bookings = await _cosmosDbService.GetAllBookingsAsync();
                if (bookings != null)
                {
                    _allBookings = bookings.ToList();
                    BookingsDataGrid.ItemsSource = _allBookings;
                    PopulateVenueFilter(_allBookings.Select(b => b.VenueName).Distinct());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bookings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateVenueFilter(System.Collections.Generic.IEnumerable<string> venues)
        {
            VenueFilterComboBox.Items.Clear();
            VenueFilterComboBox.Items.Add("All Venues");
            
            foreach (var venue in venues)
            {
                VenueFilterComboBox.Items.Add(venue);
            }
            
            VenueFilterComboBox.SelectedIndex = 0;
        }

        private void VenueFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_allBookings == null || VenueFilterComboBox.SelectedItem == null) return;

            string selectedVenue = VenueFilterComboBox.SelectedItem.ToString() ?? "All Venues";
            
            if (selectedVenue == "All Venues")
            {
                BookingsDataGrid.ItemsSource = _allBookings;
            }
            else
            {
                var filtered = _allBookings.Where(b => b.VenueName == selectedVenue).ToList();
                BookingsDataGrid.ItemsSource = filtered;
            }
        }

        private void RefreshBookings_Click(object sender, RoutedEventArgs e)
        {
            LoadBookingsAsync();
        }

        private void BookingsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = BookingsDataGrid.SelectedItem != null;
            EditBookingButton.IsEnabled = hasSelection;
            DeleteBookingButton.IsEnabled = hasSelection;
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsDataGrid.SelectedItem is Booking booking && _cosmosDbService != null)
            {
                var detailWindow = new BookingDetailWindow(_cosmosDbService, _currentUser, booking);
                detailWindow.Owner = Window.GetWindow(this);
                detailWindow.ShowDialog();
            }
        }

        private async void EditBooking_Click(object sender, RoutedEventArgs e)
        {
            // PERMISSION CHECK
            if (_currentUser?.Permissions != null && !_currentUser.Permissions.CanEditBookings)
            {
                if (_currentUser.Role != UserRole.SysAdmin && _currentUser.Role != UserRole.Manager)
                {
                    MessageBox.Show("You don't have permission to edit bookings.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (BookingsDataGrid.SelectedItem is Booking booking && _cosmosDbService != null)
            {
                try
                {
                    // Load venues from database
                    var venues = await _cosmosDbService.GetAllVenuesAsync();
                    var venuesList = venues?.ToList() ?? new System.Collections.Generic.List<Venue>();
                    
                    var editWindow = new EditBookingWindow(booking, venuesList);
                    editWindow.Owner = Window.GetWindow(this);
                    if (editWindow.ShowDialog() == true)
                    {
                        // Save the updated booking
                        var updatedBooking = editWindow.UpdatedBooking;
                        if (updatedBooking != null && !string.IsNullOrEmpty(updatedBooking.Id))
                        {
                            await _cosmosDbService.UpdateBookingAsync(updatedBooking.Id, updatedBooking);
                            MessageBox.Show("Booking updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadBookingsAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error editing booking: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DeleteBooking_Click(object sender, RoutedEventArgs e)
        {
            // PERMISSION CHECK
            if (_currentUser?.Permissions != null && !_currentUser.Permissions.CanDeleteBookings)
            {
                if (_currentUser.Role != UserRole.SysAdmin && _currentUser.Role != UserRole.Manager)
                {
                    MessageBox.Show("You don't have permission to delete bookings.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (BookingsDataGrid.SelectedItem is Booking booking)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete this booking?\n\n" +
                    $"DJ: {booking.DJName}\n" +
                    $"Venue: {booking.VenueName}\n" +
                    $"Schedule: {booking.DisplaySchedule}",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes && _cosmosDbService != null)
                {
                    try
                    {
                        await _cosmosDbService.DeleteBookingAsync(booking.Id ?? string.Empty);
                        MessageBox.Show("Booking deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadBookingsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting booking: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CopyStreamingLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string link)
            {
                try
                {
                    Clipboard.SetText(link);
                    MessageBox.Show("Streaming link copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error copying link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
