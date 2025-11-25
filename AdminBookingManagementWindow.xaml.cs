using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class AdminBookingManagementWindow : Window
    {
        private CosmosDbService _cosmosDbService;
        private User _currentUser;
        private List<Booking> _allBookings = new List<Booking>();
        private List<Venue> _allVenues = new List<Venue>();

        public AdminBookingManagementWindow(CosmosDbService cosmosDbService, User currentUser)
        {
            InitializeComponent();
            _cosmosDbService = cosmosDbService;
            _currentUser = currentUser;
            
            // Check permissions
            if (!_currentUser.Permissions.CanCreateBookings && 
                _currentUser.Role != UserRole.SysAdmin && 
                _currentUser.Role != UserRole.Manager)
            {
                MessageBox.Show("You do not have permission to manage bookings.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }
            
            InitializeControls();
            LoadVenuesAndBookings();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount == 2)
                {
                    WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                }
                else
                {
                    DragMove();
                }
            }
            catch { }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void InitializeControls()
        {
            // Initialize hour dropdown (0-23)
            for (int i = 0; i < 24; i++)
            {
                HourComboBox.Items.Add(i.ToString("D2"));
            }
            HourComboBox.SelectedIndex = 20; // Default 8PM
            
            // Initialize minute dropdown (00)
            MinuteComboBox.Items.Add("00");
            MinuteComboBox.SelectedIndex = 0;
        }

        private async void LoadVenuesAndBookings()
        {
            try
            {
                _allVenues = await _cosmosDbService.GetAllVenuesAsync();
                _allBookings = await _cosmosDbService.GetAllBookingsAsync();
                
                // Populate venue dropdowns
                var openVenues = _allVenues.Where(v => v.IsActive).ToList();
                VenueComboBox.ItemsSource = openVenues;
                VenueComboBox.DisplayMemberPath = "Name";
                
                ClearVenueComboBox.ItemsSource = openVenues;
                ClearVenueComboBox.DisplayMemberPath = "Name";
                
                // Populate filter dropdown (all venues)
                var filterVenues = new List<string> { "All Venues" };
                filterVenues.AddRange(_allVenues.Select(v => v.Name));
                FilterVenueComboBox.ItemsSource = filterVenues;
                FilterVenueComboBox.SelectedIndex = 0;
                
                // Load bookings
                RefreshBookings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshBookings()
        {
            var filteredBookings = _allBookings
                .OrderBy(b => b.DayOfWeek)
                .ThenBy(b => b.WeekNumber)
                .ThenBy(b => b.TimeSlot)
                .ToList();
            BookingsDataGrid.ItemsSource = filteredBookings;
        }

        private void RefreshBookings_Click(object sender, RoutedEventArgs e)
        {
            LoadVenuesAndBookings();
        }

        private void FilterVenue_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (FilterVenueComboBox.SelectedItem is string selectedVenue)
            {
                if (selectedVenue == "All Venues")
                {
                    BookingsDataGrid.ItemsSource = _allBookings
                        .OrderBy(b => b.DayOfWeek)
                        .ThenBy(b => b.WeekNumber)
                        .ThenBy(b => b.TimeSlot)
                        .ToList();
                }
                else
                {
                    var filtered = _allBookings
                        .Where(b => b.VenueName == selectedVenue)
                        .OrderBy(b => b.DayOfWeek)
                        .ThenBy(b => b.WeekNumber)
                        .ThenBy(b => b.TimeSlot)
                        .ToList();
                    BookingsDataGrid.ItemsSource = filtered;
                }
            }
        }

        private async void CreateBooking_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(DJNameTextBox.Text))
                {
                    MessageBox.Show("DJ Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (VenueComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select a venue.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (BookingDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Please select a booking date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                var selectedVenue = (Venue)VenueComboBox.SelectedItem;
                int hour = int.Parse(HourComboBox.SelectedItem.ToString() ?? "0");
                int minute = int.Parse(MinuteComboBox.SelectedItem.ToString() ?? "0");
                
                DateTime bookingDateTime = BookingDatePicker.SelectedDate.Value.Date
                    .AddHours(hour)
                    .AddMinutes(minute);
                
                DayOfWeek selectedDay = bookingDateTime.DayOfWeek;
                int selectedWeek = Venue.GetWeekOfMonth(bookingDateTime);
                string selectedTimeSlot = $"{hour:D2}:{minute:D2}";
                
                // Check for conflicts using new properties
                var conflict = _allBookings.FirstOrDefault(b =>
                    b.VenueName == selectedVenue.Name &&
                    b.DayOfWeek == selectedDay &&
                    b.WeekNumber == selectedWeek &&
                    b.TimeSlot == selectedTimeSlot);
                
                if (conflict != null)
                {
                    MessageBox.Show($"CONFLICT!\n\nThis time slot is already booked by DJ {conflict.DJName}.\n\nPlease choose a different time.",
                        "Booking Conflict", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                // Create booking with slot-based model
                var booking = new Booking
                {
                    DJName = DJNameTextBox.Text.Trim(),
                    DJUsername = _currentUser.Username,
                    StreamingLink = StreamingLinkTextBox.Text.Trim(),
                    VenueName = selectedVenue.Name,
                    VenueId = selectedVenue.Id ?? "",
                    VenueOwnerUsername = selectedVenue.OwnerUsername,
                    DayOfWeek = selectedDay,
                    WeekNumber = selectedWeek,
                    TimeSlot = selectedTimeSlot,
                    Status = BookingStatus.Confirmed,
                    CreatedAt = DateTime.Now
                };
                
                string bookingId = await _cosmosDbService.AddBookingAsync(booking);
                
                MessageBox.Show($"Booking created successfully for DJ {booking.DJName}!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Discord webhook removed - not in simplified model
                
                // Clear form and refresh
                ClearForm_Click(sender, e);
                LoadVenuesAndBookings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create booking: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            DJNameTextBox.Clear();
            StreamingLinkTextBox.Clear();
            VenueComboBox.SelectedIndex = -1;
            BookingDatePicker.SelectedDate = null;
            HourComboBox.SelectedIndex = 20;
            MinuteComboBox.SelectedIndex = 0;
        }

        private async void EditBooking_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsDataGrid.SelectedItem is Booking booking)
            {
                var editWindow = new EditBookingWindow(booking, _allVenues.Where(v => v.IsActive).ToList(), false);
                if (editWindow.ShowDialog() == true && editWindow.UpdatedBooking != null)
                {
                    try
                    {
                        await _cosmosDbService.UpdateBookingAsync(booking.Id ?? "", editWindow.UpdatedBooking);
                        MessageBox.Show("Booking updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadVenuesAndBookings();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to update booking: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a booking to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void DeleteBooking_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsDataGrid.SelectedItem is Booking booking)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the booking for DJ {booking.DJName} at {booking.DisplaySchedule}?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _cosmosDbService.DeleteBookingAsync(booking.Id ?? "");
                        MessageBox.Show("Booking deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadVenuesAndBookings();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete booking: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a booking to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PreviewClear_Click(object sender, RoutedEventArgs e)
        {
            if (ClearVenueComboBox.SelectedItem == null || ClearDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select both a venue and a date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var selectedVenue = (Venue)ClearVenueComboBox.SelectedItem;
            DateTime selectedDate = ClearDatePicker.SelectedDate.Value.Date;
            
            var bookingsOnDate = _allBookings.Where(b =>
                b.VenueName == selectedVenue.Name &&
                b.OccursOn(selectedDate)).ToList();
            
            if (bookingsOnDate.Count == 0)
            {
                ClearPreviewText.Text = $"? No bookings found for {selectedVenue.Name} on {selectedDate:MM/dd/yyyy}";
                ClearPreviewText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            }
            else
            {
                ClearPreviewText.Text = $"?? Found {bookingsOnDate.Count} booking(s) on {selectedDate:MM/dd/yyyy}:\n\n" +
                    string.Join("\n", bookingsOnDate.Select(b => $"� {b.DJName} at {b.TimeSlot}"));
                ClearPreviewText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            }
        }

        private async void ClearDaySchedule_Click(object sender, RoutedEventArgs e)
        {
            if (ClearVenueComboBox.SelectedItem == null || ClearDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select both a venue and a date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var selectedVenue = (Venue)ClearVenueComboBox.SelectedItem;
            DateTime selectedDate = ClearDatePicker.SelectedDate.Value.Date;
            
            var bookingsOnDate = _allBookings.Where(b =>
                b.VenueName == selectedVenue.Name &&
                b.OccursOn(selectedDate)).ToList();
            
            if (bookingsOnDate.Count == 0)
            {
                MessageBox.Show($"No bookings found for {selectedVenue.Name} on {selectedDate:MM/dd/yyyy}.",
                    "No Bookings", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var result = MessageBox.Show(
                $"?? WARNING! This will DELETE {bookingsOnDate.Count} booking(s) for {selectedVenue.Name} on {selectedDate:MM/dd/yyyy}!\n\n" +
                "Bookings to be deleted:\n" +
                string.Join("\n", bookingsOnDate.Select(b => $"� {b.DJName} at {b.TimeSlot}")) +
                "\n\nThis action CANNOT be undone!\n\nAre you ABSOLUTELY SURE?",
                "?? CONFIRM DELETION",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    int deletedCount = 0;
                    foreach (var booking in bookingsOnDate)
                    {
                        await _cosmosDbService.DeleteBookingAsync(booking.Id ?? "");
                        deletedCount++;
                    }
                    
                    MessageBox.Show($"Successfully deleted {deletedCount} booking(s) for {selectedVenue.Name} on {selectedDate:MM/dd/yyyy}!",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Clear preview and reload
                    ClearPreviewText.Text = "No bookings found for selected date";
                    ClearPreviewText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
                    LoadVenuesAndBookings();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to clear day schedule: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
