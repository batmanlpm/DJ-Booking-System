using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem.Views
{
#pragma warning disable CS0618 // Obsolete BookingDate usage

    public partial class BookingsView : UserControl
    {
        private List<Booking> _bookings;
        private CosmosDbService? _cosmosService;
        private User? _currentUser;
        private bool _isCalendarView = false;
        private BookingsCalendarView? _calendarView;

        public BookingsView()
        {
            InitializeComponent();
            _bookings = new List<Booking>();
        }

        public async Task Initialize(CosmosDbService cosmosService, User currentUser)
        {
            _cosmosService = cosmosService;
            _currentUser = currentUser;
            
            // Show loading overlay
            if (LoadingOverlay != null)
                LoadingOverlay.Visibility = Visibility.Visible;
            
            try
            {
                if (LoadingMessage != null)
                    LoadingMessage.Text = "Loading your bookings...";
                
                await LoadBookingsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize bookings: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Hide loading overlay
                if (LoadingOverlay != null)
                    LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadBookingsAsync()
        {
            try
            {
                if (_cosmosService == null || _currentUser == null)
                {
                    UpdateStatusText("Not properly initialized");
                    return;
                }

                UpdateStatusText("Loading bookings...");

                // Use ConfigureAwait(false) to not block UI thread
                var allBookings = await _cosmosService.GetAllBookingsAsync().ConfigureAwait(false);
                
                // Return to UI thread for UI updates
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // Filter bookings based on user role
                    List<Booking> filteredBookings;
                    
                    if (_currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager)
                    {
                        // Admins and Managers see all bookings
                        filteredBookings = allBookings.ToList();
                        UpdateStatusText($"Loaded {filteredBookings.Count} bookings (All)");
                    }
                    else if (_currentUser.IsVenueOwnerEffective)
                    {
                        // Venue owners see bookings at their venues
                        filteredBookings = allBookings
                            .Where(b => b.VenueOwnerUsername == _currentUser.Username)
                            .ToList();
                        UpdateStatusText($"Loaded {filteredBookings.Count} bookings (Your Venues)");
                    }
                    else if (_currentUser.IsDJEffective)
                    {
                        // DJs see only their own bookings
                        filteredBookings = allBookings
                            .Where(b => b.DJUsername == _currentUser.Username)
                            .ToList();
                        UpdateStatusText($"Loaded {filteredBookings.Count} bookings (Your Bookings)");
                    }
                    else
                    {
                        // No bookings for other roles
                        filteredBookings = new List<Booking>();
                        UpdateStatusText("No bookings available");
                    }
                    
                    _bookings = filteredBookings
                        .OrderBy(b => b.DayOfWeek)
                        .ThenBy(b => b.WeekNumber)
                        .ThenBy(b => b.TimeSlot)
                        .ToList();
                
                    // Create display objects with conditional streaming link visibility
                    var displayBookings = _bookings.Select(b => new BookingDisplay
                    {
                        DJName = b.DJName,
                        VenueName = b.VenueName,
                        Date = b.DisplaySchedule, // Use DisplaySchedule instead of obsolete BookingDate
                        TimeSlot = b.TimeSlot,
                        StreamingLink = CanViewStreamingLink(b) ? b.StreamingLink : "[Hidden]",
                        Status = b.Status.ToString(),
                        CreatedAt = b.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                        OriginalBooking = b
                    }).ToList();

                    BookingsDataGrid.ItemsSource = null;
                    BookingsDataGrid.ItemsSource = displayBookings;
                    UpdateBookingCount(_bookings.Count);
                });
            }
            catch (Exception ex)
            {
                // Show error on UI thread
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Failed to load bookings: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    UpdateStatusText("Error loading bookings");
                });
            }
        }

        private bool CanViewStreamingLink(Booking booking)
        {
            if (_currentUser == null) return false;

            // SysAdmin can see all links
            if (_currentUser.Role == UserRole.SysAdmin)
                return true;

            // DJ who made the booking can see their own link
            if (booking.DJUsername == _currentUser.Username)
                return true;

            // Venue owner can see links for bookings at their venue
            if (_currentUser.IsVenueOwner && booking.VenueOwnerUsername == _currentUser.Username)
                return true;

            return false;
        }

        private void NewBooking_Click(object sender, RoutedEventArgs e)
        {
            if (_cosmosService == null || _currentUser == null)
            {
                MessageBox.Show("Not properly initialized", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Only DJs can make bookings
            if (!_currentUser.IsDJ)
            {
                MessageBox.Show(
                    "Only DJs can make bookings.\n\nPlease contact an admin to set your account as a DJ.",
                    "DJ Account Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var createWindow = new CreateBookingWindow(_cosmosService, _currentUser);
            if (createWindow.ShowDialog() == true && createWindow.CreatedBooking != null)
            {
                _ = LoadBookingsAsync(); // Refresh
            }
        }

        private void EditBooking_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Slot editing coming soon!\n\nFor now, please cancel the booking and create a new one.",
                "Coming Soon",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private async void DeleteBooking_Click(object sender, RoutedEventArgs e)
        {
            if (_cosmosService == null || _currentUser == null)
            {
                MessageBox.Show("Not properly initialized", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (BookingsDataGrid.SelectedItem is BookingDisplay selectedDisplay)
            {
                var selectedBooking = selectedDisplay.OriginalBooking;

                // Check if user can delete this booking
                bool canDelete = false;
                
                if (_currentUser.Role == UserRole.SysAdmin)
                {
                    canDelete = true;
                }
                else if (selectedBooking.DJUsername == _currentUser.Username)
                {
                    canDelete = true; // DJ can cancel their own booking
                }
                else if (_currentUser.IsVenueOwner && selectedBooking.VenueOwnerUsername == _currentUser.Username)
                {
                    canDelete = true; // Venue owner can cancel bookings at their venue
                }

                if (!canDelete)
                {
                    MessageBox.Show(
                        "You can only cancel your own bookings or bookings at your venue.",
                        "Permission Denied",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Cancel this booking?\n\n" +
                    $"DJ: {selectedBooking.DJName}\n" +
                    $"Venue: {selectedBooking.VenueName}\n" +
                    $"Schedule: {selectedBooking.DisplaySchedule}\n" +
                    $"Time: {selectedBooking.TimeSlot}\n\n" +
                    "This will make the slot available again.", 
                    "Confirm Cancellation", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Mark as cancelled instead of deleting
                        selectedBooking.Status = BookingStatus.Cancelled;
                        await _cosmosService.UpdateBookingAsync(selectedBooking.Id ?? "", selectedBooking);
                        
                        MessageBox.Show("Booking cancelled successfully", "Success", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadBookingsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to cancel booking: {ex.Message}", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a booking to cancel", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadBookingsAsync();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterComboBox.SelectedIndex < 0 || _bookings == null || _currentUser == null)
                return;

            var filteredBookings = _bookings;
            var today = DateTime.Now.Date;

            switch (FilterComboBox.SelectedIndex)
            {
                case 0: // All Bookings
                    filteredBookings = _bookings;
                    break;
                case 1: // Upcoming
                    filteredBookings = _bookings.Where(b => b.GetNextOccurrence(DateTime.Now) >= DateTime.Now && b.Status != BookingStatus.Cancelled).ToList();
                    break;
                case 2: // Today
                    filteredBookings = _bookings.Where(b => b.GetNextOccurrence(DateTime.Now).Date == today && b.Status != BookingStatus.Cancelled).ToList();
                    break;
                case 3: // This Week
                    var weekEnd = today.AddDays(7);
                    filteredBookings = _bookings.Where(b => b.GetNextOccurrence(DateTime.Now).Date >= today && b.GetNextOccurrence(DateTime.Now).Date <= weekEnd && b.Status != BookingStatus.Cancelled).ToList();
                    break;
                case 4: // Past
                    filteredBookings = _bookings.Where(b => b.GetNextOccurrence(DateTime.Now) < DateTime.Now).ToList();
                    break;
                case 5: // My Bookings (DJ)
                    filteredBookings = _bookings.Where(b => b.DJUsername == _currentUser.Username).ToList();
                    break;
                case 6: // My Venue (Venue Owner)
                    if (_currentUser.IsVenueOwner)
                    {
                        filteredBookings = _bookings.Where(b => b.VenueOwnerUsername == _currentUser.Username).ToList();
                    }
                    break;
                case 7: // Cancelled
                    filteredBookings = _bookings.Where(b => b.Status == BookingStatus.Cancelled).ToList();
                    break;
            }

            // Create display objects
            var displayBookings = filteredBookings.Select(b => new BookingDisplay
            {
                DJName = b.DJName,
                VenueName = b.VenueName,
                Date = b.GetNextOccurrence(DateTime.Now).ToString("d"),
                TimeSlot = b.TimeSlot,
                StreamingLink = CanViewStreamingLink(b) ? b.StreamingLink : "[Hidden]",
                Status = b.Status.ToString(),
                CreatedAt = b.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                OriginalBooking = b
            }).ToList();

            BookingsDataGrid.ItemsSource = null;
            BookingsDataGrid.ItemsSource = displayBookings;
            UpdateStatusText($"Showing {filteredBookings.Count} of {_bookings.Count} bookings");
            UpdateBookingCount(filteredBookings.Count);
        }

        private void BookingsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = BookingsDataGrid.SelectedItem != null;
            EditButton.IsEnabled = hasSelection;
            DeleteButton.IsEnabled = hasSelection;
        }

        private void UpdateStatusText(string text)
        {
            if (StatusText != null)
            {
                StatusText.Text = text;
            }
        }

        private void UpdateBookingCount(int count)
        {
            if (BookingCountText != null)
            {
                BookingCountText.Text = $"{count} booking{(count != 1 ? "s" : "")}";
            }
        }

        private async void ToggleView_Click(object sender, RoutedEventArgs e)
        {
            _isCalendarView = !_isCalendarView;

            if (_isCalendarView)
            {
                // Switch to Calendar View
                ListViewBorder.Visibility = Visibility.Collapsed;
                CalendarViewContainer.Visibility = Visibility.Visible;
                ViewToggleButton.Content = "?? LIST VIEW";
                ViewToggleButton.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0, 255, 0));
                
                // Initialize calendar view if not already done
                if (_calendarView == null && _cosmosService != null && _currentUser != null)
                {
                    _calendarView = new BookingsCalendarView();
                    await _calendarView.Initialize(_cosmosService, _currentUser);
                    CalendarViewContainer.Content = _calendarView;
                }
            }
            else
            {
                // Switch to List View
                ListViewBorder.Visibility = Visibility.Visible;
                CalendarViewContainer.Visibility = Visibility.Collapsed;
                ViewToggleButton.Content = "?? CALENDAR VIEW";
                ViewToggleButton.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(255, 20, 147));
            }
        }
    }

    // Display class for DataGrid
    public class BookingDisplay
    {
        public string DJName { get; set; } = string.Empty;
        public string VenueName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string TimeSlot { get; set; } = string.Empty;
        public string StreamingLink { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public Booking OriginalBooking { get; set; } = null!;
    }
}