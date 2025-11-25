using System;
using System.Windows;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem.Views
{
    public partial class PromoteDemoteUserWindow : Window
    {
        private UserRole _selectedRole;
        private readonly UserRole _currentRole;
        private readonly UserRole _performingUserRole;
        private readonly string _targetUsername;
        private readonly string _currentUsername;

        public UserRole SelectedRole => _selectedRole;
        public bool RoleChanged { get; private set; }

        /// <summary>
        /// Initialize Promote/Demote dialog with role validation
        /// </summary>
        /// <param name="currentRole">The target user's current role</param>
        /// <param name="performingUserRole">The role of the user making the change</param>
        /// <param name="targetUsername">Username of the user being changed</param>
        /// <param name="currentUsername">Username of the user making the change</param>
        public PromoteDemoteUserWindow(UserRole currentRole, UserRole performingUserRole, string targetUsername, string currentUsername)
        {
            InitializeComponent();
            _currentRole = currentRole;
            _selectedRole = currentRole;
            _performingUserRole = performingUserRole;
            _targetUsername = targetUsername;
            _currentUsername = currentUsername;

            // Display current role
            CurrentRoleText.Text = currentRole.ToString();

            // Highlight the current role button
            HighlightCurrentRole(currentRole);

            // Disable buttons based on permissions
            DisableInvalidRoleOptions();
        }

        /// <summary>
        /// Disable role buttons that the current user cannot assign
        /// </summary>
        private void DisableInvalidRoleOptions()
        {
            bool isSelfChange = _targetUsername.Equals(_currentUsername, StringComparison.OrdinalIgnoreCase);

            // If trying to change own role, disable everything
            if (isSelfChange)
            {
                SelectUserButton.IsEnabled = false;
                SelectDJButton.IsEnabled = false;
                SelectVenueOwnerButton.IsEnabled = false;
                SelectManagerButton.IsEnabled = false;
                SelectSysAdminButton.IsEnabled = false;

                MessageBox.Show(
                    "You cannot change your own role.\n\nPlease ask another administrator for assistance.",
                    "Self-Modification Not Allowed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // SysAdmin can do anything (except demote other SysAdmins)
            if (_performingUserRole == UserRole.SysAdmin)
            {
                // If target is SysAdmin, cannot demote them
                if (_currentRole == UserRole.SysAdmin)
                {
                    SelectUserButton.IsEnabled = false;
                    SelectDJButton.IsEnabled = false;
                    SelectVenueOwnerButton.IsEnabled = false;
                    SelectManagerButton.IsEnabled = false;
                    // SysAdmin button stays enabled (no change)
                }
                // Otherwise, SysAdmin can assign any role
                return;
            }

            // Manager restrictions
            if (_performingUserRole == UserRole.Manager)
            {
                // Cannot touch SysAdmin accounts at all
                if (_currentRole == UserRole.SysAdmin)
                {
                    MessageBox.Show(
                        "Managers cannot modify SysAdmin accounts.",
                        "Access Denied",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    Close();
                    return;
                }

                // Cannot promote to SysAdmin
                SelectSysAdminButton.IsEnabled = false;
                SelectSysAdminButton.Opacity = 0.5;

                // Can assign User, DJ, VenueOwner, Manager
                return;
            }

            // DJ, VenueOwner, User - no permissions
            if (_performingUserRole is UserRole.DJ or UserRole.VenueOwner or UserRole.User)
            {
                MessageBox.Show(
                    $"{_performingUserRole} role does not have permission to change user roles.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                Close();
            }
        }

        private void HighlightCurrentRole(UserRole role)
        {
            // Reset all buttons
            ResetAllButtons();

            // Highlight the current role
            switch (role)
            {
                case UserRole.User:
                    SelectUserButton.BorderBrush = System.Windows.Media.Brushes.Cyan;
                    SelectUserButton.BorderThickness = new Thickness(4);
                    break;
                case UserRole.DJ:
                    SelectDJButton.BorderBrush = System.Windows.Media.Brushes.Cyan;
                    SelectDJButton.BorderThickness = new Thickness(4);
                    break;
                case UserRole.VenueOwner:
                    SelectVenueOwnerButton.BorderBrush = System.Windows.Media.Brushes.Cyan;
                    SelectVenueOwnerButton.BorderThickness = new Thickness(4);
                    break;
                case UserRole.Manager:
                    SelectManagerButton.BorderBrush = System.Windows.Media.Brushes.Cyan;
                    SelectManagerButton.BorderThickness = new Thickness(4);
                    break;
                case UserRole.SysAdmin:
                    SelectSysAdminButton.BorderBrush = System.Windows.Media.Brushes.Cyan;
                    SelectSysAdminButton.BorderThickness = new Thickness(4);
                    break;
            }
        }

        private void ResetAllButtons()
        {
            var pinkBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x14, 0x93)); // #FF1493
            
            SelectUserButton.BorderBrush = pinkBrush;
            SelectUserButton.BorderThickness = new Thickness(3);
            
            SelectDJButton.BorderBrush = pinkBrush;
            SelectDJButton.BorderThickness = new Thickness(3);
            
            SelectVenueOwnerButton.BorderBrush = pinkBrush;
            SelectVenueOwnerButton.BorderThickness = new Thickness(3);
            
            SelectManagerButton.BorderBrush = pinkBrush;
            SelectManagerButton.BorderThickness = new Thickness(3);
            
            SelectSysAdminButton.BorderBrush = pinkBrush;
            SelectSysAdminButton.BorderThickness = new Thickness(3);
        }

        private void SelectRole_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is string roleString)
            {
                _selectedRole = (UserRole)System.Enum.Parse(typeof(UserRole), roleString);
                
                // Highlight the selected button
                HighlightCurrentRole(_selectedRole);
                
                System.Diagnostics.Debug.WriteLine($"[PromoteDemote] Role selected: {_selectedRole}");
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRole != _currentRole)
            {
                // Validate the role change
                bool isSelfChange = _targetUsername.Equals(_currentUsername, StringComparison.OrdinalIgnoreCase);
                
                bool isValid = RoleChangeValidator.CanChangeRole(
                    _performingUserRole,
                    _currentRole,
                    _selectedRole,
                    isSelfChange,
                    out string errorMessage);

                if (!isValid)
                {
                    MessageBox.Show(
                        $"Role change denied:\n\n{errorMessage}",
                        "Unauthorized",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to change the role from '{_currentRole}' to '{_selectedRole}'?\n\n" +
                    $"User: {_targetUsername}",
                    "Confirm Role Change",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    RoleChanged = true;
                    DialogResult = true;
                    Close();
                }
            }
            else
            {
                MessageBox.Show(
                    "No role change selected. The current role is the same as the selected role.",
                    "No Change",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            RoleChanged = false;
            DialogResult = false;
            Close();
        }
    }
}
