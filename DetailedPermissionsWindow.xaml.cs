using System.Windows;
using DJBookingSystem.Models;

namespace DJBookingSystem
{
    public partial class DetailedPermissionsWindow : Window
    {
        private User _user;
        public UserPermissions? UpdatedPermissions { get; private set; }

        public DetailedPermissionsWindow(User user, bool stayOnTop = false)
        {
            InitializeComponent();
            _user = user;
            UserInfoTextBlock.Text = $"Editing permissions for: {user.Username} ({user.FullName}) - Role: {user.Role}";

            // Apply Stay on Top preference
            this.Topmost = stayOnTop;

            LoadPermissions();
            UpdateSummary();

            // Update summary when checkboxes change
            var checkBoxEvents = new[]
            {
                CanViewBookingsCheckBox,
                CanCreateBookingsCheckBox,
                CanEditBookingsCheckBox,
                CanDeleteBookingsCheckBox,
                CanViewVenuesCheckBox,
                CanRegisterVenuesCheckBox,
                CanEditVenuesCheckBox,
                CanDeleteVenuesCheckBox,
                CanToggleVenueStatusCheckBox,
                CanManageUsersCheckBox,
                CanCustomizeAppCheckBox,
                CanAccessSettingsCheckBox,
                CanViewRadioBossCheckBox,
                CanControlRadioBossCheckBox
            };
            foreach (var cb in checkBoxEvents)
            {
                cb.Checked += (s, e) => UpdateSummary();
                cb.Unchecked += (s, e) => UpdateSummary();
            }
        }

        private void LoadPermissions()
        {
            var perms = _user.Permissions ?? new UserPermissions();

            // Booking permissions
            CanViewBookingsCheckBox.IsChecked = perms.CanViewBookings;
            CanCreateBookingsCheckBox.IsChecked = perms.CanCreateBookings;
            CanEditBookingsCheckBox.IsChecked = perms.CanEditBookings;
            CanDeleteBookingsCheckBox.IsChecked = perms.CanDeleteBookings;

            // Venue permissions
            CanViewVenuesCheckBox.IsChecked = perms.CanViewVenues;
            CanRegisterVenuesCheckBox.IsChecked = perms.CanRegisterVenues;
            CanEditVenuesCheckBox.IsChecked = perms.CanEditVenues;
            CanDeleteVenuesCheckBox.IsChecked = perms.CanDeleteVenues;
            CanToggleVenueStatusCheckBox.IsChecked = perms.CanToggleVenueStatus;

            // Admin permissions
            CanManageUsersCheckBox.IsChecked = perms.CanManageUsers;
            CanCustomizeAppCheckBox.IsChecked = perms.CanCustomizeApp;
            CanAccessSettingsCheckBox.IsChecked = perms.CanAccessSettings;

            // RadioBOSS permissions
            CanViewRadioBossCheckBox.IsChecked = perms.CanViewRadioBoss;
            CanControlRadioBossCheckBox.IsChecked = perms.CanControlRadioBoss;
        }

        private void UpdateSummary()
        {
            int enabledCount = 0;
            int totalCount = 14;

            if (CanViewBookingsCheckBox.IsChecked == true) enabledCount++;
            if (CanCreateBookingsCheckBox.IsChecked == true) enabledCount++;
            if (CanEditBookingsCheckBox.IsChecked == true) enabledCount++;
            if (CanDeleteBookingsCheckBox.IsChecked == true) enabledCount++;
            if (CanViewVenuesCheckBox.IsChecked == true) enabledCount++;
            if (CanRegisterVenuesCheckBox.IsChecked == true) enabledCount++;
            if (CanEditVenuesCheckBox.IsChecked == true) enabledCount++;
            if (CanDeleteVenuesCheckBox.IsChecked == true) enabledCount++;
            if (CanToggleVenueStatusCheckBox.IsChecked == true) enabledCount++;
            if (CanManageUsersCheckBox.IsChecked == true) enabledCount++;
            if (CanCustomizeAppCheckBox.IsChecked == true) enabledCount++;
            if (CanAccessSettingsCheckBox.IsChecked == true) enabledCount++;
            if (CanViewRadioBossCheckBox.IsChecked == true) enabledCount++;
            if (CanControlRadioBossCheckBox.IsChecked == true) enabledCount++;

            double percentage = (enabledCount / (double)totalCount) * 100;

            PermissionSummaryTextBlock.Text = $"Enabled Permissions: {enabledCount} of {totalCount} ({percentage:F0}%)\n\n" +
                $"This user will have access to {enabledCount} features across the application.";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            UpdatedPermissions = new UserPermissions
            {
                // Booking permissions
                CanViewBookings = CanViewBookingsCheckBox.IsChecked ?? false,
                CanCreateBookings = CanCreateBookingsCheckBox.IsChecked ?? false,
                CanEditBookings = CanEditBookingsCheckBox.IsChecked ?? false,
                CanDeleteBookings = CanDeleteBookingsCheckBox.IsChecked ?? false,

                // Venue permissions
                CanViewVenues = CanViewVenuesCheckBox.IsChecked ?? false,
                CanRegisterVenues = CanRegisterVenuesCheckBox.IsChecked ?? false,
                CanEditVenues = CanEditVenuesCheckBox.IsChecked ?? false,
                CanDeleteVenues = CanDeleteVenuesCheckBox.IsChecked ?? false,
                CanToggleVenueStatus = CanToggleVenueStatusCheckBox.IsChecked ?? false,

                // Admin permissions
                CanManageUsers = CanManageUsersCheckBox.IsChecked ?? false,
                CanCustomizeApp = CanCustomizeAppCheckBox.IsChecked ?? false,
                CanAccessSettings = CanAccessSettingsCheckBox.IsChecked ?? false,

                // RadioBOSS permissions
                CanViewRadioBoss = CanViewRadioBossCheckBox.IsChecked ?? false,
                CanControlRadioBoss = CanControlRadioBossCheckBox.IsChecked ?? false
            };

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void StayOnTopCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
        }
        private void StayOnTopCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Topmost = false;
        }
    }
}
