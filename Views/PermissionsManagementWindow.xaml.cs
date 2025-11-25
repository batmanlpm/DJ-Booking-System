using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem.Views
{
    public partial class PermissionsManagementWindow : Window
    {
        private List<User> _users;
        private User? _selectedUser;
        private CosmosDbService? _cosmosDbService;

        public PermissionsManagementWindow(List<User> users, CosmosDbService? cosmosDbService = null)
        {
            InitializeComponent();
            _users = users;
            _cosmosDbService = cosmosDbService;
            LoadUsers();
        }

        private void LoadUsers()
        {
            UserComboBox.ItemsSource = _users;
            UserComboBox.DisplayMemberPath = "Username";
            
            if (_users.Any())
            {
                UserComboBox.SelectedIndex = 0;
            }
        }

        private void UserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserComboBox.SelectedItem is User user)
            {
                _selectedUser = user;
                LoadUserPermissions(user);
            }
        }

        private void LoadUserPermissions(User user)
        {
            // System Permissions
            CanManageUsersCheck.IsChecked = user.Permissions.CanManageUsers;
            CanAccessSettingsCheck.IsChecked = user.Permissions.CanAccessSettings;
            CanViewReportsCheck.IsChecked = user.Permissions.CanViewReports;

            // Booking Permissions
            CanViewBookingsCheck.IsChecked = user.Permissions.CanViewBookings;
            CanCreateBookingsCheck.IsChecked = user.Permissions.CanCreateBookings;
            CanEditBookingsCheck.IsChecked = user.Permissions.CanEditBookings;
            CanDeleteBookingsCheck.IsChecked = user.Permissions.CanDeleteBookings;

            // Venue Permissions
            CanViewVenuesCheck.IsChecked = user.Permissions.CanViewVenues;
            CanRegisterVenuesCheck.IsChecked = user.Permissions.CanRegisterVenues;
            CanEditVenuesCheck.IsChecked = user.Permissions.CanEditVenues;
            CanDeleteVenuesCheck.IsChecked = user.Permissions.CanDeleteVenues;
            CanToggleVenueStatusCheck.IsChecked = user.Permissions.CanToggleVenueStatus;

            // Moderation Permissions
            CanBanUsersCheck.IsChecked = user.Permissions.CanBanUsers;
            CanMuteUsersCheck.IsChecked = user.Permissions.CanMuteUsers;

            // Customization Permissions
            CanCustomizeAppCheck.IsChecked = user.Permissions.CanCustomizeApp;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Please select a user.", "No User Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Update user permissions
            _selectedUser.Permissions.CanManageUsers = CanManageUsersCheck.IsChecked ?? false;
            _selectedUser.Permissions.CanAccessSettings = CanAccessSettingsCheck.IsChecked ?? false;
            _selectedUser.Permissions.CanViewReports = CanViewReportsCheck.IsChecked ?? false;

            _selectedUser.Permissions.CanViewBookings = CanViewBookingsCheck.IsChecked ?? false;
            _selectedUser.Permissions.CanCreateBookings = CanCreateBookingsCheck.IsChecked ?? false;
            _selectedUser.Permissions.CanEditBookings = CanEditBookingsCheck.IsChecked ?? false;
            _selectedUser.Permissions.CanDeleteBookings = CanDeleteBookingsCheck.IsChecked ?? false;

            _selectedUser.Permissions.CanViewVenues = CanViewVenuesCheck.IsChecked ?? false;
            _selectedUser.Permissions.CanRegisterVenues = CanRegisterVenuesCheck.IsChecked ?? false;
            _selectedUser.Permissions.CanEditVenues = CanEditVenuesCheck.IsChecked ?? false;
            _selectedUser.Permissions.CanDeleteVenues = CanDeleteVenuesCheck.IsChecked ?? false;
            _selectedUser.Permissions.CanToggleVenueStatus = CanToggleVenueStatusCheck.IsChecked ?? false;

            _selectedUser.Permissions.CanBanUsers = CanBanUsersCheck.IsChecked ?? false;
            _selectedUser.Permissions.CanMuteUsers = CanMuteUsersCheck.IsChecked ?? false;

            _selectedUser.Permissions.CanCustomizeApp = CanCustomizeAppCheck.IsChecked ?? false;

            // TODO: Save to database if CosmosDbService is available
            if (_cosmosDbService != null)
            {
                // await _cosmosDbService.UpdateUserAsync(_selectedUser);
            }

            MessageBox.Show(
                $"Permissions saved for {_selectedUser.Username}!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Please select a user.", "No User Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Reset {_selectedUser.Username}'s permissions to default for their role ({_selectedUser.Role})?\n\nThis will overwrite all custom permissions.",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _selectedUser.Permissions = PermissionService.GetDefaultPermissionsForRole(_selectedUser.Role);
                LoadUserPermissions(_selectedUser);

                MessageBox.Show(
                    $"Permissions reset to default for {_selectedUser.Role} role.",
                    "Permissions Reset",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
