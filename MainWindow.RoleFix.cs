using System;
using System.Windows;
using DJBookingSystem.Models;

namespace DJBookingSystem
{
    /// <summary>
    /// Helper methods to diagnose and fix user role issues
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Show current user's role information and offer to upgrade to SysAdmin
        /// Call this from a menu item or button to diagnose permission issues
        /// </summary>
        private async void DiagnoseAndFixUserRole()
        {
            try
            {
                if (_currentUser == null)
                {
                    MessageBox.Show(
                        "No user is currently logged in.",
                        "No User",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Show current role info
                var roleInfo = $"?? USER ROLE DIAGNOSTIC\n\n" +
                              $"Username: {_currentUser.Username}\n" +
                              $"Full Name: {_currentUser.FullName}\n" +
                              $"Current Role: {_currentUser.Role}\n" +
                              $"Role Value: {(int)_currentUser.Role}\n\n" +
                              $"ROLE MEANINGS:\n" +
                              $"User = 0 (Basic user)\n" +
                              $"DJ = 1 (DJ privileges)\n" +
                              $"VenueOwner = 2 (Venue management)\n" +
                              $"Manager = 3 (Moderation + user management)\n" +
                              $"SysAdmin = 4 (Full system access)\n\n";

                // Check if user needs upgrade
                if (_currentUser.Role != UserRole.SysAdmin)
                {
                    roleInfo += $"? Your role is currently: {_currentUser.Role}\n";
                    roleInfo += $"? This is why you're getting access denied errors!\n\n";
                    roleInfo += $"Would you like to upgrade to SysAdmin?";

                    var result = MessageBox.Show(
                        roleInfo,
                        "Fix User Role?",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        await UpgradeToSysAdmin();
                    }
                }
                else
                {
                    roleInfo += $"? You already have SysAdmin role!\n";
                    roleInfo += $"? You should have full access to everything.";

                    MessageBox.Show(
                        roleInfo,
                        "Role Diagnostic",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error during role diagnostic:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Upgrade current user to SysAdmin role and save to database
        /// </summary>
        private async System.Threading.Tasks.Task UpgradeToSysAdmin()
        {
            try
            {
                if (_currentUser == null || _cosmosDbService == null)
                {
                    MessageBox.Show(
                        "Cannot upgrade: User or database service is null.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                var oldRole = _currentUser.Role;

                // Upgrade to SysAdmin
                _currentUser.Role = UserRole.SysAdmin;

                // Update permissions to SysAdmin defaults
                _currentUser.Permissions = Services.PermissionService.GetDefaultPermissionsForRole(UserRole.SysAdmin);

                // Save to database
                if (!string.IsNullOrEmpty(_currentUser.Id))
                {
                    await _cosmosDbService.UpdateUserAsync(_currentUser);
                }

                // Refresh permission service
                _permissionService = new Services.PermissionService(_currentUser);

                MessageBox.Show(
                    $"? SUCCESS!\n\n" +
                    $"Your role has been upgraded:\n\n" +
                    $"Old role: {oldRole}\n" +
                    $"New role: {_currentUser.Role}\n\n" +
                    $"You now have full SysAdmin access!\n" +
                    $"All features are now available.",
                    "Role Upgraded",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                System.Diagnostics.Debug.WriteLine($"User {_currentUser.Username} upgraded from {oldRole} to {_currentUser.Role}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to upgrade role:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "Upgrade Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
