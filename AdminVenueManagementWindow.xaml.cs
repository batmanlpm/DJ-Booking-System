using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
#pragma warning disable CS0618 // Obsolete OpenTime/CloseTime usage

    public partial class AdminVenueManagementWindow : Window
    {
        private CosmosDbService _cosmosDbService;
        private User _currentUser;
        private List<Venue> _venues = new List<Venue>();
        private Venue? _selectedVenue = null;
        private bool _isNewVenue = false;

        public AdminVenueManagementWindow(CosmosDbService cosmosDbService, User currentUser)
        {
            InitializeComponent();
            _cosmosDbService = cosmosDbService;
            _currentUser = currentUser;
            
            // COMPREHENSIVE PERMISSION ENFORCEMENT
            EnforceVenuePermissions();
            
            LoadVenues();
            ClearVenueEditor();
        }

        /// <summary>
        /// Enforce all venue-related permissions
        /// </summary>
        private void EnforceVenuePermissions()
        {
            bool isAdmin = _currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager;

            // If user can't even view venues, close the window
            if (!isAdmin && !_currentUser.Permissions.CanViewVenues)
            {
                MessageBox.Show("You do not have permission to view venues.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            // If admin, allow everything
            if (isAdmin)
            {
                return;
            }

            // Apply permission-based restrictions for non-admins
            // Note: UI button enforcement requires buttons to exist in XAML
            // For now, rely on code-level checks in action methods
            
            // CanEditVenues - Make text boxes read-only if no permission
            if (!_currentUser.Permissions.CanEditVenues)
            {
                if (VenueNameTextBox != null) VenueNameTextBox.IsReadOnly = true;
                if (VenueDescriptionTextBox != null) VenueDescriptionTextBox.IsReadOnly = true;
                if (DiscordWebhookTextBox != null) DiscordWebhookTextBox.IsReadOnly = true;
            }

            // CanToggleVenueStatus
            if (!_currentUser.Permissions.CanToggleVenueStatus && VenueIsOpenCheckBox != null)
            {
                VenueIsOpenCheckBox.IsEnabled = false;
                VenueIsOpenCheckBox.ToolTip = "You don't have permission to toggle venue status";
            }

            System.Diagnostics.Debug.WriteLine($"[VenueManagement] Permissions enforced for {_currentUser.Username}:");
            System.Diagnostics.Debug.WriteLine($"  - View: {_currentUser.Permissions.CanViewVenues}");
            System.Diagnostics.Debug.WriteLine($"  - Create: {_currentUser.Permissions.CanRegisterVenues}");
            System.Diagnostics.Debug.WriteLine($"  - Edit: {_currentUser.Permissions.CanEditVenues}");
            System.Diagnostics.Debug.WriteLine($"  - Delete: {_currentUser.Permissions.CanDeleteVenues}");
            System.Diagnostics.Debug.WriteLine($"  - Toggle Status: {_currentUser.Permissions.CanToggleVenueStatus}");
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

        private async void LoadVenues()
        {
            try
            {
                _venues = await _cosmosDbService.GetAllVenuesAsync();
                VenuesListBox.ItemsSource = null;
                VenuesListBox.ItemsSource = _venues;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load venues: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VenuesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (VenuesListBox.SelectedItem is Venue venue)
            {
                _selectedVenue = venue;
                _isNewVenue = false;
                LoadVenueToEditor(venue);
            }
        }

        private void LoadVenueToEditor(Venue venue)
        {
            // Simple details only
            VenueNameTextBox.Text = venue.Name;
            
            // TODO: Add these controls to AdminVenueManagementWindow.xaml
            // DescriptionTextBox.Text = venue.Description ?? "";
            // IsOpenCheckBox.IsChecked = venue.IsActive;
            
#pragma warning disable CS0618 // Type or member is obsolete
            // Parse open/close times
            if (!string.IsNullOrEmpty(venue.OpenTime))
            {
                var timeParts = venue.OpenTime.Split(':');
                if (timeParts.Length == 2)
                {
                    // Use first text box in Monday row as placeholder
                    if (MondayOpenTimeTextBox != null)
                    {
                        MondayOpenTimeTextBox.Text = venue.OpenTime;
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(venue.CloseTime))
            {
                var timeParts = venue.CloseTime.Split(':');
                if (timeParts.Length == 2)
                {
                    if (MondayCloseTimeTextBox != null)
                    {
                        MondayCloseTimeTextBox.Text = venue.CloseTime;
                    }
                }
            }
#pragma warning restore CS0618 // Type or member is obsolete
            
            /* OLD COMPLEX SCHEDULE SYSTEM - COMMENTED OUT
            if (venue.OpeningSchedule != null)
            {
                LoadDaySchedule(DayOfWeek.Monday, venue.OpeningSchedule.Monday);
                LoadDaySchedule(DayOfWeek.Tuesday, venue.OpeningSchedule.Tuesday);
                LoadDaySchedule(DayOfWeek.Wednesday, venue.OpeningSchedule.Wednesday);
                LoadDaySchedule(DayOfWeek.Thursday, venue.OpeningSchedule.Thursday);
                LoadDaySchedule(DayOfWeek.Friday, venue.OpeningSchedule.Friday);
                LoadDaySchedule(DayOfWeek.Saturday, venue.OpeningSchedule.Saturday);
                LoadDaySchedule(DayOfWeek.Sunday, venue.OpeningSchedule.Sunday);
            }
            */
        }

        /* OLD COMPLEX SCHEDULE METHODS - COMMENTED OUT FOR NOW
        private void LoadDaySchedule(DayOfWeek day, DaySchedule schedule)
        {
            // Complex schedule code removed
        }
        
        private DaySchedule GetDaySchedule(CheckBox openCheckBox, TextBox openTimeTextBox, TextBox closeTimeTextBox)
        {
            // Complex schedule code removed  
        }
        */

        private void ClearVenueEditor()
        {
            VenueNameTextBox.Clear();
            VenueDescriptionTextBox.Clear();
            DiscordWebhookTextBox.Clear();
            VenueIsOpenCheckBox.IsChecked = true;
            
            // Clear all day schedules
            MondayOpenCheckBox.IsChecked = false;
            TuesdayOpenCheckBox.IsChecked = false;
            WednesdayOpenCheckBox.IsChecked = false;
            ThursdayOpenCheckBox.IsChecked = false;
            FridayOpenCheckBox.IsChecked = false;
            SaturdayOpenCheckBox.IsChecked = false;
            SundayOpenCheckBox.IsChecked = false;
        }

        private void CreateVenue_Click(object sender, RoutedEventArgs e)
        {
            // PERMISSION CHECK
            bool isAdmin = _currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager;
            if (!isAdmin && !_currentUser.Permissions.CanRegisterVenues)
            {
                MessageBox.Show("You don't have permission to create venues.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _selectedVenue = null;
            _isNewVenue = true;
            ClearVenueEditor();
            MessageBox.Show("Creating new venue. Fill in the details and click Save.", "New Venue", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void SaveVenue_Click(object sender, RoutedEventArgs e)
        {
            // PERMISSION CHECK
            bool isAdmin = _currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager;
            bool isNew = _isNewVenue || _selectedVenue == null;
            
            if (!isAdmin)
            {
                if (isNew && !_currentUser.Permissions.CanRegisterVenues)
                {
                    MessageBox.Show("You don't have permission to create venues.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!isNew && !_currentUser.Permissions.CanEditVenues)
                {
                    MessageBox.Show("You don't have permission to edit venues.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(VenueNameTextBox.Text))
                {
                    MessageBox.Show("Venue name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Create or update venue
                Venue venue;
                if (_isNewVenue || _selectedVenue == null)
                {
                    venue = new Venue
                    {
                        CreatedAt = DateTime.Now,
                        OwnerUsername = _currentUser.Username
                    };
                }
                else
                {
                    venue = _selectedVenue;
                }
                
                // Set basic info with simplified model
                venue.Name = VenueNameTextBox.Text.Trim();
                venue.Description = VenueDescriptionTextBox.Text?.Trim() ?? "";
                venue.OpenTime = MondayOpenTimeTextBox?.Text ?? "18:00";
                venue.CloseTime = MondayCloseTimeTextBox?.Text ?? "02:00";
                venue.IsActive = VenueIsOpenCheckBox?.IsChecked ?? true;
                venue.OwnerUsername = _currentUser.Username;
                
                // Save to database
                if (_isNewVenue || string.IsNullOrEmpty(venue.Id))
                {
                    string venueId = await _cosmosDbService.AddVenueAsync(venue);
                    MessageBox.Show($"Venue '{venue.Name}' created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _cosmosDbService.UpdateVenueAsync(venue.Id, venue);
                    MessageBox.Show($"Venue '{venue.Name}' updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                // Reload venues
                LoadVenues();
                _isNewVenue = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save venue: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /* OLD COMPLEX SCHEDULE METHODS - COMMENTED OUT FOR NOW
        private DaySchedule GetDaySchedule(System.Windows.Controls.CheckBox openCheckBox, System.Windows.Controls.TextBox openTimeTextBox, System.Windows.Controls.TextBox closeTimeTextBox)
        {
            return new DaySchedule
            {
                IsOpen = openCheckBox.IsChecked ?? false,
                OpenTime = openTimeTextBox.Text.Trim(),
                CloseTime = closeTimeTextBox.Text.Trim()
            };
        }
        */

        private async void DeleteVenue_Click(object sender, RoutedEventArgs e)
        {
            // PERMISSION CHECK
            bool isAdmin = _currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager;
            if (!isAdmin && !_currentUser.Permissions.CanDeleteVenues)
            {
                MessageBox.Show("You don't have permission to delete venues.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_selectedVenue == null)
            {
                MessageBox.Show("Please select a venue to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var result = MessageBox.Show(
                $"Are you sure you want to delete venue '{_selectedVenue.Name}'?\n\nThis action cannot be undone!",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _cosmosDbService.DeleteVenueAsync(_selectedVenue.Id ?? "");
                    MessageBox.Show($"Venue '{_selectedVenue.Name}' deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    _selectedVenue = null;
                    ClearVenueEditor();
                    LoadVenues();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete venue: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
