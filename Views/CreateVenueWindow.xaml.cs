using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using DJBookingSystem.Models;
using DJBookingSystem.Services;
using System.Collections.Generic;
using System.Linq;

namespace DJBookingSystem.Views
{
    public partial class CreateVenueWindow : Window
    {
        private readonly CosmosDbService _cosmosService;
        private readonly User _currentUser;
        public Venue? CreatedVenue { get; private set; }

        public CreateVenueWindow(CosmosDbService cosmosService, User currentUser)
        {
            InitializeComponent();
            _cosmosService = cosmosService;
            _currentUser = currentUser;
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

        private void DayCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                // Show/hide time panel based on checkbox
                var dayName = checkBox.Name.Replace("CheckBox", "TimePanel");
                var timePanel = FindName(dayName) as Grid;
                
                if (timePanel != null)
                {
                    timePanel.Visibility = checkBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private async void CreateVenue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate venue name
                if (string.IsNullOrWhiteSpace(VenueNameTextBox.Text))
                {
                    ShowError("Venue name is required");
                    VenueNameTextBox.Focus();
                    return;
                }

                // Validate description
                if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
                {
                    ShowError("Description is required");
                    DescriptionTextBox.Focus();
                    return;
                }

                // Collect per-day schedules
                var daySchedules = new Dictionary<DayOfWeek, DaySchedule>();
                
                if (MondayCheckBox.IsChecked == true)
                {
                    if (!ValidateAndAddDay(DayOfWeek.Monday, MondayStartTextBox.Text, MondayFinishTextBox.Text, daySchedules, "Monday"))
                        return;
                }
                
                if (TuesdayCheckBox.IsChecked == true)
                {
                    if (!ValidateAndAddDay(DayOfWeek.Tuesday, TuesdayStartTextBox.Text, TuesdayFinishTextBox.Text, daySchedules, "Tuesday"))
                        return;
                }
                
                if (WednesdayCheckBox.IsChecked == true)
                {
                    if (!ValidateAndAddDay(DayOfWeek.Wednesday, WednesdayStartTextBox.Text, WednesdayFinishTextBox.Text, daySchedules, "Wednesday"))
                        return;
                }
                
                if (ThursdayCheckBox.IsChecked == true)
                {
                    if (!ValidateAndAddDay(DayOfWeek.Thursday, ThursdayStartTextBox.Text, ThursdayFinishTextBox.Text, daySchedules, "Thursday"))
                        return;
                }
                
                if (FridayCheckBox.IsChecked == true)
                {
                    if (!ValidateAndAddDay(DayOfWeek.Friday, FridayStartTextBox.Text, FridayFinishTextBox.Text, daySchedules, "Friday"))
                        return;
                }
                
                if (SaturdayCheckBox.IsChecked == true)
                {
                    if (!ValidateAndAddDay(DayOfWeek.Saturday, SaturdayStartTextBox.Text, SaturdayFinishTextBox.Text, daySchedules, "Saturday"))
                        return;
                }
                
                if (SundayCheckBox.IsChecked == true)
                {
                    if (!ValidateAndAddDay(DayOfWeek.Sunday, SundayStartTextBox.Text, SundayFinishTextBox.Text, daySchedules, "Sunday"))
                        return;
                }

                if (daySchedules.Count == 0)
                {
                    ShowError("Please select at least one day of the week");
                    return;
                }

                // Get selected weeks
                var activeWeeks = new List<int>();
                if (Week1CheckBox.IsChecked == true) activeWeeks.Add(1);
                if (Week2CheckBox.IsChecked == true) activeWeeks.Add(2);
                if (Week3CheckBox.IsChecked == true) activeWeeks.Add(3);
                if (Week4CheckBox.IsChecked == true) activeWeeks.Add(4);

                if (activeWeeks.Count == 0)
                {
                    ShowError("Please select at least one week of the month");
                    return;
                }

                // Create venue
                var venue = new Venue
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = VenueNameTextBox.Text.Trim(),
                    Description = DescriptionTextBox.Text.Trim(),
                    DaySchedules = daySchedules,
                    ActiveWeeks = activeWeeks,
                    OwnerUsername = _currentUser.Username,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                StatusText.Text = "Creating venue...";
                StatusText.Foreground = System.Windows.Media.Brushes.Yellow;

                // Save to database
                await _cosmosService.AddVenueAsync(venue);

                CreatedVenue = venue;

                // Build summary message
                var daysText = string.Join(", ", daySchedules.Keys.Select(d => d.ToString().Substring(0, 3)));
                var weeksText = string.Join(", ", activeWeeks.Select(w => $"Week {w}"));
                
                var scheduleDetails = string.Join("\n", daySchedules.Select(kvp => 
                    $"  {kvp.Key}: {kvp.Value.StartTime} - {kvp.Value.FinishTime}" +
                    (IsOvernightSchedule(kvp.Value) ? " (overnight)" : "")));

                MessageBox.Show(
                    $"Venue '{venue.Name}' created successfully!\n\n" +
                    $"Days: {daysText}\n" +
                    $"Weeks: {weeksText}\n\n" +
                    $"Schedule:\n{scheduleDetails}",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to create venue: {ex.Message}");
            }
        }

        private bool ValidateAndAddDay(DayOfWeek day, string startTime, string finishTime, 
            Dictionary<DayOfWeek, DaySchedule> schedules, string dayName)
        {
            // Validate time format
            if (!TimeSpan.TryParse(startTime, out var start))
            {
                ShowError($"{dayName}: Invalid start time format. Use HH:mm (e.g., 20:00)");
                return false;
            }

            if (!TimeSpan.TryParse(finishTime, out var finish))
            {
                ShowError($"{dayName}: Invalid finish time format. Use HH:mm (e.g., 02:00)");
                return false;
            }

            // Add the schedule (overnight is allowed and handled automatically)
            schedules[day] = new DaySchedule
            {
                StartTime = startTime,
                FinishTime = finishTime
            };

            return true;
        }

        private bool IsOvernightSchedule(DaySchedule schedule)
        {
            if (TimeSpan.TryParse(schedule.StartTime, out var start) && 
                TimeSpan.TryParse(schedule.FinishTime, out var finish))
            {
                return finish < start; // e.g., 20:00 start, 02:00 finish = overnight
            }
            return false;
        }

        private void ShowError(string message)
        {
            StatusText.Text = message;
            StatusText.Foreground = System.Windows.Media.Brushes.Red;
        }
    }
}