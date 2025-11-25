using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem.Views
{
#pragma warning disable CS0618 // Obsolete BookingDate/GetAvailableTimeSlots usage

    public partial class CreateBookingWindow : Window
    {
        private readonly CosmosDbService _cosmosService;
        private readonly User _currentUser;
        private List<Venue> _venues = new List<Venue>();
        private Venue? _selectedVenue;

        public Booking? CreatedBooking { get; private set; }

        public CreateBookingWindow(CosmosDbService cosmosService, User currentUser)
        {
            InitializeComponent();
            _cosmosService = cosmosService;
            _currentUser = currentUser;

            // Pre-fill streaming link if user has one saved
            if (!string.IsNullOrEmpty(_currentUser.StreamingLink))
            {
                StreamingLinkTextBox.Text = _currentUser.StreamingLink;
            }

            // Set default date to today
            BookingDatePicker.SelectedDate = DateTime.Today;

            LoadVenues();
        }

        private async void LoadVenues()
        {
            try
            {
                var venues = await _cosmosService.GetAllVenuesAsync();
                _venues = venues.Where(v => v.IsActive).ToList();
                
                VenueComboBox.ItemsSource = _venues.Select(v => v.Name).ToList();
                
                if (_venues.Any())
                {
                    VenueComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load venues: {ex.Message}");
            }
        }

        private void VenueComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VenueComboBox.SelectedIndex >= 0 && VenueComboBox.SelectedIndex < _venues.Count)
            {
                _selectedVenue = _venues[VenueComboBox.SelectedIndex];
                LoadAvailableSlots();
            }
        }

        private void BookingDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAvailableSlots();
        }

        private async void LoadAvailableSlots()
        {
            if (_selectedVenue == null || !BookingDatePicker.SelectedDate.HasValue)
            {
                TimeSlotsListBox.ItemsSource = null;
                return;
            }

            try
            {
                StatusText.Text = "Loading available slots...";
                StatusText.Foreground = System.Windows.Media.Brushes.Yellow;

                // Get all time slots for the venue
                var allSlots = _selectedVenue.GetAvailableTimeSlots();
                
                // Get existing bookings for this venue and date
                var selectedDate = BookingDatePicker.SelectedDate.Value;
                var allBookings = await _cosmosService.GetAllBookingsAsync();
                var existingBookings = allBookings
                    .Where(b => b.VenueId == _selectedVenue.Id &&
                               b.GetNextOccurrence(DateTime.Now).Date == selectedDate.Date &&
                               b.Status != BookingStatus.Cancelled)
                    .ToList();

                // Create display list with availability status
                var displaySlots = new List<string>();
                
                foreach (var slot in allSlots)
                {
                    var booking = existingBookings.FirstOrDefault(b => b.TimeSlot == slot);
                    
                    if (booking != null)
                    {
                        // Slot is booked - show who booked it
                        displaySlots.Add($"{slot} - BOOKED by {booking.DJName}");
                    }
                    else
                    {
                        // Slot is available
                        displaySlots.Add($"{slot} - AVAILABLE");
                    }
                }

                TimeSlotsListBox.ItemsSource = displaySlots;
                StatusText.Text = $"{displaySlots.Count(s => s.Contains("AVAILABLE"))} slots available";
                StatusText.Foreground = System.Windows.Media.Brushes.Green;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load slots: {ex.Message}");
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void BookSlot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate venue selection
                if (_selectedVenue == null)
                {
                    ShowError("Please select a venue");
                    VenueComboBox.Focus();
                    return;
                }

                // Validate date selection
                if (!BookingDatePicker.SelectedDate.HasValue)
                {
                    ShowError("Please select a date");
                    BookingDatePicker.Focus();
                    return;
                }

                // Validate time slot selection
                if (TimeSlotsListBox.SelectedItem == null)
                {
                    ShowError("Please select a time slot");
                    TimeSlotsListBox.Focus();
                    return;
                }

                // Check if slot is available
                string selectedSlot = TimeSlotsListBox.SelectedItem.ToString() ?? "";
                if (!selectedSlot.Contains("AVAILABLE"))
                {
                    ShowError("This slot is already booked. Please select an available slot.");
                    return;
                }

                // Extract time slot (e.g., "20:00 - AVAILABLE" -> "20:00")
                string timeSlot = selectedSlot.Split('-')[0].Trim();

                // Validate streaming link
                if (string.IsNullOrWhiteSpace(StreamingLinkTextBox.Text))
                {
                    ShowError("Please enter your streaming link");
                    StreamingLinkTextBox.Focus();
                    return;
                }

                // Combine date and time
                var bookingDate = BookingDatePicker.SelectedDate.Value.Date;
                var timeParts = timeSlot.Split(':');
                bookingDate = bookingDate.AddHours(int.Parse(timeParts[0])).AddMinutes(int.Parse(timeParts[1]));

                // Double-check availability (race condition prevention)
                var allBookings = await _cosmosService.GetAllBookingsAsync();
                var conflictingBooking = allBookings.FirstOrDefault(b =>
                    b.VenueId == _selectedVenue.Id &&
                    b.DayOfWeek == bookingDate.DayOfWeek &&
                    b.WeekNumber == Venue.GetWeekOfMonth(bookingDate) &&
                    b.TimeSlot == timeSlot &&
                    b.Status != BookingStatus.Cancelled);

                if (conflictingBooking != null)
                {
                    ShowError("This slot was just booked by someone else. Please select another slot.");
                    LoadAvailableSlots(); // Refresh
                    return;
                }

                // Create booking
                var booking = new Booking
                {
                    Id = Guid.NewGuid().ToString(),
                    DJName = _currentUser.FullName ?? _currentUser.Username,
                    DJUsername = _currentUser.Username,
                    StreamingLink = StreamingLinkTextBox.Text.Trim(),
                    VenueName = _selectedVenue.Name,
                    VenueId = _selectedVenue.Id ?? "",
                    VenueOwnerUsername = _selectedVenue.OwnerUsername,
#pragma warning disable CS0618 // Type or member is obsolete
                    DayOfWeek = bookingDate.DayOfWeek,
                    WeekNumber = Venue.GetWeekOfMonth(bookingDate),
                    TimeSlot = timeSlot,
                    Status = BookingStatus.Confirmed,
                    CreatedAt = DateTime.Now
                };

                StatusText.Text = "Booking slot...";
                StatusText.Foreground = System.Windows.Media.Brushes.Yellow;

                // Save to database
                await _cosmosService.AddBookingAsync(booking);

                // Save streaming link to user profile for future use
                if (_currentUser.StreamingLink != StreamingLinkTextBox.Text.Trim())
                {
                    _currentUser.StreamingLink = StreamingLinkTextBox.Text.Trim();
                    await _cosmosService.UpdateUserAsync(_currentUser);
                }

                CreatedBooking = booking;

#pragma warning disable CS0618 // Type or member is obsolete
                MessageBox.Show(
                    $"Slot booked successfully!\n\n" +
                    $"Venue: {booking.VenueName}\n" +
                    $"Schedule: {booking.DisplaySchedule}\n" +
                    $"Time: {booking.TimeSlot}\n\n" +
                    "The venue owner has your streaming link.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
#pragma warning restore CS0618 // Type or member is obsolete

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to book slot: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            StatusText.Text = message;
            StatusText.Foreground = System.Windows.Media.Brushes.Red;
        }
    }
}
