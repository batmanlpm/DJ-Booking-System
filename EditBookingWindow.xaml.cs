using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DJBookingSystem.Models;

namespace DJBookingSystem
{
    public partial class EditBookingWindow : Window
    {
        private Booking _originalBooking;
        private List<Venue> _venues;
        public Booking UpdatedBooking { get; private set; }

        public EditBookingWindow(Booking booking, List<Venue> venues, bool stayOnTop = false)
        {
            InitializeComponent();
            _originalBooking = booking;
            _venues = venues;
            UpdatedBooking = new Booking();

            this.Topmost = stayOnTop;

            InitializeTimeControls();
            LoadBookingData();
        }

        private void InitializeTimeControls()
        {
            for (int i = 0; i < 24; i++)
            {
                HourComboBox.Items.Add(i.ToString("D2"));
            }

            MinuteComboBox.Items.Add("00");
        }

        private void LoadBookingData()
        {
            DJNameTextBox.Text = _originalBooking.DJName;
            StreamingLinkTextBox.Text = _originalBooking.StreamingLink;
            BookingDatePicker.SelectedDate = _originalBooking.GetNextOccurrence(DateTime.Now).Date;

            // Parse time from TimeSlot (format: "HH:mm")
            if (TimeSpan.TryParse(_originalBooking.TimeSlot, out var timeSpan))
            {
                HourComboBox.SelectedItem = timeSpan.Hours.ToString("D2");
                int minuteIndex = timeSpan.Minutes / 15;
                MinuteComboBox.SelectedIndex = minuteIndex;
            }

            VenueComboBox.ItemsSource = _venues;
            VenueComboBox.DisplayMemberPath = "Name";

            var selectedVenue = _venues.FirstOrDefault(v => v.Name == _originalBooking.VenueName);
            if (selectedVenue != null)
                VenueComboBox.SelectedItem = selectedVenue;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DJNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(StreamingLinkTextBox.Text) ||
                VenueComboBox.SelectedItem == null ||
                BookingDatePicker.SelectedDate == null ||
                HourComboBox.SelectedItem == null ||
                MinuteComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int hour = int.Parse(HourComboBox.SelectedItem.ToString() ?? "0");
            int minute = int.Parse(MinuteComboBox.SelectedItem.ToString() ?? "0");
            DateTime bookingDateTime = BookingDatePicker.SelectedDate.Value.Date
                .AddHours(hour)
                .AddMinutes(minute);

            var selectedVenue = (Venue)VenueComboBox.SelectedItem;

            UpdatedBooking = new Booking
            {
                Id = _originalBooking.Id,
                DJName = DJNameTextBox.Text.Trim(),
                DJUsername = _originalBooking.DJUsername,
                StreamingLink = StreamingLinkTextBox.Text.Trim(),
                VenueName = selectedVenue.Name,
                VenueId = selectedVenue.Id ?? "",
                VenueOwnerUsername = selectedVenue.OwnerUsername,
                DayOfWeek = bookingDateTime.DayOfWeek,
                WeekNumber = Venue.GetWeekOfMonth(bookingDateTime),
                TimeSlot = $"{hour:D2}:{minute:D2}",
                Status = _originalBooking.Status,
                CreatedAt = _originalBooking.CreatedAt
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