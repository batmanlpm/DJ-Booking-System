using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem.Views
{
    public partial class BookingsCalendarView : UserControl
    {
        private CosmosDbService? _cosmosService;
        private User? _currentUser;
        private List<Venue> _venues = new List<Venue>();
        private Venue? _selectedVenue;
        private DateTime _currentWeekStart;
        private List<Booking> _allBookings = new List<Booking>();

        public BookingsCalendarView()
        {
            InitializeComponent();
            _currentWeekStart = GetWeekStart(DateTime.Now);
        }

        public async Task Initialize(CosmosDbService cosmosService, User currentUser)
        {
            _cosmosService = cosmosService;
            _currentUser = currentUser;
            await LoadVenuesAsync();
        }

        private async Task LoadVenuesAsync()
        {
            try
            {
                if (_cosmosService == null) return;

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
                MessageBox.Show($"Failed to load venues: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void VenueComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VenueComboBox.SelectedIndex >= 0 && VenueComboBox.SelectedIndex < _venues.Count)
            {
                _selectedVenue = _venues[VenueComboBox.SelectedIndex];
                VenueNameText.Text = $"{_selectedVenue.Name} - OPEN DECKS";
                await LoadWeekSchedule();
            }
        }

        private DateTime GetWeekStart(DateTime date)
        {
            // Get Monday of the current week
            int daysFromMonday = ((int)date.DayOfWeek + 6) % 7;
            return date.Date.AddDays(-daysFromMonday);
        }

        private async void PrevWeek_Click(object sender, RoutedEventArgs e)
        {
            _currentWeekStart = _currentWeekStart.AddDays(-7);
            await LoadWeekSchedule();
        }

        private async void ThisWeek_Click(object sender, RoutedEventArgs e)
        {
            _currentWeekStart = GetWeekStart(DateTime.Now);
            await LoadWeekSchedule();
        }

        private async void NextWeek_Click(object sender, RoutedEventArgs e)
        {
            _currentWeekStart = _currentWeekStart.AddDays(7);
            await LoadWeekSchedule();
        }

        private async Task LoadWeekSchedule()
        {
            if (_selectedVenue == null || _cosmosService == null) return;

            try
            {
                // Update day headers with dates
                UpdateDayHeaders();

                // Load ALL bookings for this venue (not filtered by date since bookings are recurring)
                var allBookings = await _cosmosService.GetAllBookingsAsync();
                
                _allBookings = allBookings
                    .Where(b => b.VenueId == _selectedVenue.Id &&
                               b.Status != BookingStatus.Cancelled)
                    .ToList();

                // Build the time slots grid
                BuildTimeSlotGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load schedule: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDayHeaders()
        {
            // Get current week number
            int currentWeek = Venue.GetWeekOfMonth(_currentWeekStart);
            
            MondayHeader.Text = $"Mon\nWeek {currentWeek}";
            TuesdayHeader.Text = $"Tue\nWeek {currentWeek}";
            WednesdayHeader.Text = $"Wed\nWeek {currentWeek}";
            ThursdayHeader.Text = $"Thu\nWeek {currentWeek}";
            FridayHeader.Text = $"Fri\nWeek {currentWeek}";
            SaturdayHeader.Text = $"Sat\nWeek {currentWeek}";
            SundayHeader.Text = $"Sun\nWeek {currentWeek}";
        }

        private void BuildTimeSlotGrid()
        {
            if (_selectedVenue == null) return;

            // Clear existing content
            TimeSlotsContainer.Items.Clear();

            try
            {
                // Get current week number
                int currentWeek = Venue.GetWeekOfMonth(_currentWeekStart);

                // Collect all unique time slots across all open days
                var allTimeSlots = new HashSet<string>();
                foreach (var day in _selectedVenue.OpenDays)
                {
                    try
                    {
                        var slots = _selectedVenue.GetAvailableTimeSlots(day);
                        foreach (var slot in slots)
                        {
                            allTimeSlots.Add(slot);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error getting slots for {day}: {ex.Message}");
                        // Continue with other days
                    }
                }

                if (!allTimeSlots.Any())
                {
                    var noSlotsText = new TextBlock
                    {
                        Text = "No time slots configured for this venue",
                        Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                        FontFamily = new FontFamily("Consolas"),
                        FontSize = 16,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 50, 0, 0)
                    };
                    TimeSlotsContainer.Items.Add(noSlotsText);
                    return;
                }

                // Sort time slots with error handling
                var sortedTimeSlots = allTimeSlots.OrderBy(t => 
                {
                    if (TimeSpan.TryParse(t, out var time))
                        return time;
                    return TimeSpan.Zero;
                }).ToList();

                // Create a Grid for the time slots
                var grid = new Grid();
                
                // Add columns: Time + 7 days
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) }); // Time
                for (int i = 0; i < 7; i++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                // Add rows for each time slot
                int rowIndex = 0;
                foreach (var slot in sortedTimeSlots)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });

                    // Time label
                    var timeBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(10, 10, 10)),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(5)
                    };
                    var timeText = new TextBlock
                    {
                        Text = slot,
                        Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                        FontFamily = new FontFamily("Consolas"),
                        FontWeight = FontWeights.Bold,
                        FontSize = 13,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    timeBorder.Child = timeText;
                    Grid.SetRow(timeBorder, rowIndex);
                    Grid.SetColumn(timeBorder, 0);
                    grid.Children.Add(timeBorder);

                    // Day slots
                    for (int dayOffset = 0; dayOffset < 7; dayOffset++)
                    {
                        try
                        {
                            var dayOfWeek = (DayOfWeek)(((int)DayOfWeek.Monday + dayOffset) % 7);
                            
                            // Check if venue is open on this day/week
                            bool venueOpen = _selectedVenue.IsOpenOn(dayOfWeek, currentWeek);
                            
                            // Check if this specific time slot is available for this day
                            bool timeSlotAvailable = false;
                            if (venueOpen)
                            {
                                var daySlots = _selectedVenue.GetAvailableTimeSlots(dayOfWeek);
                                timeSlotAvailable = daySlots != null && daySlots.Contains(slot);
                            }
                            
                            // Find booking for this day/week/slot
                            var booking = timeSlotAvailable ? _allBookings.FirstOrDefault(b =>
                                b.DayOfWeek == dayOfWeek && 
                                b.WeekNumber == currentWeek && 
                                b.TimeSlot == slot) : null;

                            var slotBorder = CreateSlotBorder(booking, dayOfWeek, currentWeek, slot, timeSlotAvailable);
                            Grid.SetRow(slotBorder, rowIndex);
                            Grid.SetColumn(slotBorder, dayOffset + 1);
                            grid.Children.Add(slotBorder);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error creating slot for day {dayOffset}: {ex.Message}");
                            // Create an error slot
                            var errorBorder = new Border
                            {
                                Background = new SolidColorBrush(Color.FromRgb(50, 0, 0)),
                                BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                                BorderThickness = new Thickness(1)
                            };
                            Grid.SetRow(errorBorder, rowIndex);
                            Grid.SetColumn(errorBorder, dayOffset + 1);
                            grid.Children.Add(errorBorder);
                        }
                    }

                    rowIndex++;
                }

                TimeSlotsContainer.Items.Add(grid);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error building time slot grid: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Border CreateSlotBorder(Booking? booking, DayOfWeek dayOfWeek, int weekNumber, string timeSlot, bool venueOpen)
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5),
                Margin = new Thickness(2),
                Cursor = venueOpen ? System.Windows.Input.Cursors.Hand : System.Windows.Input.Cursors.Arrow
            };

            var stackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            if (!venueOpen)
            {
                // Venue closed on this day/week
                border.Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)); // Gray
                var closedText = new TextBlock
                {
                    Text = "CLOSED",
                    Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 9,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                stackPanel.Children.Add(closedText);
            }
            else if (booking != null)
            {
                // Booked slot
                border.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0)); // Red
                border.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Color.FromRgb(255, 0, 0),
                    BlurRadius = 10,
                    ShadowDepth = 0,
                    Opacity = 0.6
                };

                var djText = new TextBlock
                {
                    Text = booking.DJName,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontFamily = new FontFamily("Consolas"),
                    FontWeight = FontWeights.Bold,
                    FontSize = 11,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };
                stackPanel.Children.Add(djText);

                // Add click handler for viewing booking details
                border.Tag = booking;
                border.MouseLeftButtonDown += BookedSlot_Click;
            }
            else
            {
                // Available slot
                border.Background = new SolidColorBrush(Color.FromRgb(0, 17, 0)); // Dark green
                
                var availableText = new TextBlock
                {
                    Text = "AVAILABLE",
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 10,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                stackPanel.Children.Add(availableText);

                // Add click handler for booking
                border.Tag = new { DayOfWeek = dayOfWeek, WeekNumber = weekNumber, TimeSlot = timeSlot };
                border.MouseLeftButtonDown += AvailableSlot_Click;
            }

            border.Child = stackPanel;
            return border;
        }

        private void BookedSlot_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Booking booking)
            {
                var canViewLink = CanViewStreamingLink(booking);
                var linkText = canViewLink ? booking.StreamingLink : "[Hidden - Venue Owner/Admin Only]";

                MessageBox.Show(
                    $"RECURRING BOOKING\n\n" +
                    $"DJ: {booking.DJName}\n" +
                    $"Schedule: {booking.DayOfWeek} (Week {booking.WeekNumber})\n" +
                    $"Time: {booking.TimeSlot}\n" +
                    $"Streaming Link: {linkText}\n" +
                    $"Status: {booking.Status}",
                    "Booking Details",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private async void AvailableSlot_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_currentUser == null || !_currentUser.IsDJEffective)
            {
                MessageBox.Show("Only DJs can book slots.", "DJ Required", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (sender is Border border && border.Tag != null && _selectedVenue != null && _cosmosService != null)
            {
                dynamic slotData = border.Tag;
                DayOfWeek dayOfWeek = slotData.DayOfWeek;
                int weekNumber = slotData.WeekNumber;
                string timeSlot = slotData.TimeSlot;

                var result = MessageBox.Show(
                    $"Book this recurring slot?\n\n" +
                    $"Venue: {_selectedVenue.Name}\n" +
                    $"Schedule: {dayOfWeek} (Week {weekNumber}) every month\n" +
                    $"Time: {timeSlot}\n\n" +
                    "This will be a recurring monthly booking.\n" +
                    "You'll need to provide your streaming link.",
                    "Confirm Recurring Booking",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // TODO: Open booking window with recurring options
                    MessageBox.Show("Recurring booking creation coming soon!", "Feature In Progress");
                    // For now, just refresh
                    await LoadWeekSchedule();
                }
            }
        }

        private bool CanViewStreamingLink(Booking booking)
        {
            if (_currentUser == null) return false;

            // Admins (SysAdmin and Manager) can see all links
            if (_currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager) 
                return true;

            // DJ who made booking can see own link
            if (booking.DJUsername == _currentUser.Username) return true;

            // Venue owner can see links for their venue
            if (_currentUser.IsVenueOwner && booking.VenueOwnerUsername == _currentUser.Username) 
                return true;

            return false;
        }
    }
}
