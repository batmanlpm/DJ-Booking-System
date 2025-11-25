using System;
using System.Windows;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem.Views
{
    public partial class ManagePermissionsWindow : Window
    {
        private readonly User _user;
        private readonly CosmosDbService _cosmosService;
        private readonly User _currentUser;

        public bool PermissionsUpdated { get; private set; }

        public ManagePermissionsWindow(User user, CosmosDbService cosmosService, User currentUser)
        {
            InitializeComponent();
            
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _cosmosService = cosmosService ?? throw new ArgumentNullException(nameof(cosmosService));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

            LoadUserPermissions();
        }

        private void LoadUserPermissions()
        {
            // Update header
            UserInfoText.Text = $"User: {_user.Username} ({_user.FullName})";
            UserRoleText.Text = $"Role: {_user.Role} | Email: {_user.Email}";

            // Ensure user has permissions object
            if (_user.Permissions == null)
            {
                _user.Permissions = new UserPermissions();
            }

            // Load Booking Permissions
            CanViewBookingsCheckBox.IsChecked = _user.Permissions.CanViewBookings;
            CanCreateBookingsCheckBox.IsChecked = _user.Permissions.CanCreateBookings;
            CanEditBookingsCheckBox.IsChecked = _user.Permissions.CanEditBookings;
            CanDeleteBookingsCheckBox.IsChecked = _user.Permissions.CanDeleteBookings;

            // Load Venue Permissions
            CanViewVenuesCheckBox.IsChecked = _user.Permissions.CanViewVenues;
            CanRegisterVenuesCheckBox.IsChecked = _user.Permissions.CanRegisterVenues;
            CanEditVenuesCheckBox.IsChecked = _user.Permissions.CanEditVenues;
            CanDeleteVenuesCheckBox.IsChecked = _user.Permissions.CanDeleteVenues;
            CanToggleVenueStatusCheckBox.IsChecked = _user.Permissions.CanToggleVenueStatus;

            // Load Admin Permissions
            CanManageUsersCheckBox.IsChecked = _user.Permissions.CanManageUsers;
            CanCustomizeAppCheckBox.IsChecked = _user.Permissions.CanCustomizeApp;
            CanAccessSettingsCheckBox.IsChecked = _user.Permissions.CanAccessSettings;

            // Load Moderation Permissions
            CanBanUsersCheckBox.IsChecked = _user.Permissions.CanBanUsers;
            CanMuteUsersCheckBox.IsChecked = _user.Permissions.CanMuteUsers;
            CanViewReportsCheckBox.IsChecked = _user.Permissions.CanViewReports;
            CanResolveReportsCheckBox.IsChecked = _user.Permissions.CanResolveReports;

            // Load RadioBOSS Permissions
            CanViewRadioBossCheckBox.IsChecked = _user.Permissions.CanViewRadioBoss;
            CanControlRadioBossCheckBox.IsChecked = _user.Permissions.CanControlRadioBoss;

            System.Diagnostics.Debug.WriteLine($"[ManagePermissions] Loaded permissions for user: {_user.Username}");
        }

        private void ApplyPermissionsFromUI()
        {
            // Apply Booking Permissions
            _user.Permissions.CanViewBookings = CanViewBookingsCheckBox.IsChecked == true;
            _user.Permissions.CanCreateBookings = CanCreateBookingsCheckBox.IsChecked == true;
            _user.Permissions.CanEditBookings = CanEditBookingsCheckBox.IsChecked == true;
            _user.Permissions.CanDeleteBookings = CanDeleteBookingsCheckBox.IsChecked == true;

            // Apply Venue Permissions
            _user.Permissions.CanViewVenues = CanViewVenuesCheckBox.IsChecked == true;
            _user.Permissions.CanRegisterVenues = CanRegisterVenuesCheckBox.IsChecked == true;
            _user.Permissions.CanEditVenues = CanEditVenuesCheckBox.IsChecked == true;
            _user.Permissions.CanDeleteVenues = CanDeleteVenuesCheckBox.IsChecked == true;
            _user.Permissions.CanToggleVenueStatus = CanToggleVenueStatusCheckBox.IsChecked == true;

            // Apply Admin Permissions
            _user.Permissions.CanManageUsers = CanManageUsersCheckBox.IsChecked == true;
            _user.Permissions.CanCustomizeApp = CanCustomizeAppCheckBox.IsChecked == true;
            _user.Permissions.CanAccessSettings = CanAccessSettingsCheckBox.IsChecked == true;

            // Apply Moderation Permissions
            _user.Permissions.CanBanUsers = CanBanUsersCheckBox.IsChecked == true;
            _user.Permissions.CanMuteUsers = CanMuteUsersCheckBox.IsChecked == true;
            _user.Permissions.CanViewReports = CanViewReportsCheckBox.IsChecked == true;
            _user.Permissions.CanResolveReports = CanResolveReportsCheckBox.IsChecked == true;

            // Apply RadioBOSS Permissions
            _user.Permissions.CanViewRadioBoss = CanViewRadioBossCheckBox.IsChecked == true;
            _user.Permissions.CanControlRadioBoss = CanControlRadioBossCheckBox.IsChecked == true;
        }

        private async void SavePermissions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Apply permissions from UI
                ApplyPermissionsFromUI();

                // Save to database
                await _cosmosService.UpdateUserAsync(_user);

                PermissionsUpdated = true;

                MessageBox.Show(
                    $"? Permissions updated successfully for '{_user.Username}'!\n\n" +
                    $"The changes have been saved to the database.",
                    "Permissions Updated",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                System.Diagnostics.Debug.WriteLine($"[ManagePermissions] ? Permissions saved for user: {_user.Username}");

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ManagePermissions] ? Error saving permissions: {ex.Message}");
                MessageBox.Show(
                    $"? Failed to save permissions:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to cancel?\n\nAny unsaved changes will be lost.",
                "Cancel Changes",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DialogResult = false;
                Close();
            }
        }

        #region Quick Presets

        private void ApplyBasicUserPreset_Click(object sender, RoutedEventArgs e)
        {
            // Basic User: Can view and create bookings/venues, basic access
            CanViewBookingsCheckBox.IsChecked = true;
            CanCreateBookingsCheckBox.IsChecked = true;
            CanEditBookingsCheckBox.IsChecked = true;
            CanDeleteBookingsCheckBox.IsChecked = false;

            CanViewVenuesCheckBox.IsChecked = true;
            CanRegisterVenuesCheckBox.IsChecked = false;
            CanEditVenuesCheckBox.IsChecked = false;
            CanDeleteVenuesCheckBox.IsChecked = false;
            CanToggleVenueStatusCheckBox.IsChecked = false;

            CanManageUsersCheckBox.IsChecked = false;
            CanCustomizeAppCheckBox.IsChecked = false;
            CanAccessSettingsCheckBox.IsChecked = true;

            CanBanUsersCheckBox.IsChecked = false;
            CanMuteUsersCheckBox.IsChecked = false;
            CanViewReportsCheckBox.IsChecked = false;
            CanResolveReportsCheckBox.IsChecked = false;

            CanViewRadioBossCheckBox.IsChecked = false;
            CanControlRadioBossCheckBox.IsChecked = false;

            MessageBox.Show("? Basic User preset applied!", "Preset Applied", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ApplyDJPreset_Click(object sender, RoutedEventArgs e)
        {
            // DJ: Can manage bookings, view venues, basic RadioBOSS access
            CanViewBookingsCheckBox.IsChecked = true;
            CanCreateBookingsCheckBox.IsChecked = true;
            CanEditBookingsCheckBox.IsChecked = true;
            CanDeleteBookingsCheckBox.IsChecked = true;

            CanViewVenuesCheckBox.IsChecked = true;
            CanRegisterVenuesCheckBox.IsChecked = false;
            CanEditVenuesCheckBox.IsChecked = false;
            CanDeleteVenuesCheckBox.IsChecked = false;
            CanToggleVenueStatusCheckBox.IsChecked = false;

            CanManageUsersCheckBox.IsChecked = false;
            CanCustomizeAppCheckBox.IsChecked = false;
            CanAccessSettingsCheckBox.IsChecked = true;

            CanBanUsersCheckBox.IsChecked = false;
            CanMuteUsersCheckBox.IsChecked = false;
            CanViewReportsCheckBox.IsChecked = false;
            CanResolveReportsCheckBox.IsChecked = false;

            CanViewRadioBossCheckBox.IsChecked = true;
            CanControlRadioBossCheckBox.IsChecked = false;

            MessageBox.Show("? DJ preset applied!", "Preset Applied", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ApplyVenueOwnerPreset_Click(object sender, RoutedEventArgs e)
        {
            // Venue Owner: Can manage venues and bookings
            CanViewBookingsCheckBox.IsChecked = true;
            CanCreateBookingsCheckBox.IsChecked = true;
            CanEditBookingsCheckBox.IsChecked = true;
            CanDeleteBookingsCheckBox.IsChecked = true;

            CanViewVenuesCheckBox.IsChecked = true;
            CanRegisterVenuesCheckBox.IsChecked = true;
            CanEditVenuesCheckBox.IsChecked = true;
            CanDeleteVenuesCheckBox.IsChecked = true;
            CanToggleVenueStatusCheckBox.IsChecked = true;

            CanManageUsersCheckBox.IsChecked = false;
            CanCustomizeAppCheckBox.IsChecked = false;
            CanAccessSettingsCheckBox.IsChecked = true;

            CanBanUsersCheckBox.IsChecked = false;
            CanMuteUsersCheckBox.IsChecked = false;
            CanViewReportsCheckBox.IsChecked = false;
            CanResolveReportsCheckBox.IsChecked = false;

            CanViewRadioBossCheckBox.IsChecked = true;
            CanControlRadioBossCheckBox.IsChecked = false;

            MessageBox.Show("? Venue Owner preset applied!", "Preset Applied", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ApplyModeratorPreset_Click(object sender, RoutedEventArgs e)
        {
            // Moderator: Can moderate users and view reports
            CanViewBookingsCheckBox.IsChecked = true;
            CanCreateBookingsCheckBox.IsChecked = true;
            CanEditBookingsCheckBox.IsChecked = true;
            CanDeleteBookingsCheckBox.IsChecked = true;

            CanViewVenuesCheckBox.IsChecked = true;
            CanRegisterVenuesCheckBox.IsChecked = true;
            CanEditVenuesCheckBox.IsChecked = true;
            CanDeleteVenuesCheckBox.IsChecked = false;
            CanToggleVenueStatusCheckBox.IsChecked = true;

            CanManageUsersCheckBox.IsChecked = true;
            CanCustomizeAppCheckBox.IsChecked = false;
            CanAccessSettingsCheckBox.IsChecked = true;

            CanBanUsersCheckBox.IsChecked = true;
            CanMuteUsersCheckBox.IsChecked = true;
            CanViewReportsCheckBox.IsChecked = true;
            CanResolveReportsCheckBox.IsChecked = true;

            CanViewRadioBossCheckBox.IsChecked = true;
            CanControlRadioBossCheckBox.IsChecked = false;

            MessageBox.Show("? Moderator preset applied!", "Preset Applied", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ApplyAdminPreset_Click(object sender, RoutedEventArgs e)
        {
            // Admin: Full access to everything
            CanViewBookingsCheckBox.IsChecked = true;
            CanCreateBookingsCheckBox.IsChecked = true;
            CanEditBookingsCheckBox.IsChecked = true;
            CanDeleteBookingsCheckBox.IsChecked = true;

            CanViewVenuesCheckBox.IsChecked = true;
            CanRegisterVenuesCheckBox.IsChecked = true;
            CanEditVenuesCheckBox.IsChecked = true;
            CanDeleteVenuesCheckBox.IsChecked = true;
            CanToggleVenueStatusCheckBox.IsChecked = true;

            CanManageUsersCheckBox.IsChecked = true;
            CanCustomizeAppCheckBox.IsChecked = true;
            CanAccessSettingsCheckBox.IsChecked = true;

            CanBanUsersCheckBox.IsChecked = true;
            CanMuteUsersCheckBox.IsChecked = true;
            CanViewReportsCheckBox.IsChecked = true;
            CanResolveReportsCheckBox.IsChecked = true;

            CanViewRadioBossCheckBox.IsChecked = true;
            CanControlRadioBossCheckBox.IsChecked = true;

            MessageBox.Show("? Admin preset applied (full access)!", "Preset Applied", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}
