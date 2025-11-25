using System;
using System.Windows;
using System.Windows.Controls;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class MainWindow
    {
        /// <summary>
        /// Apply permissions to UI elements based on current user's role and permissions
        /// </summary>
        private void ApplyPermissions()
        {
            if (_permissionService == null || _currentUser == null)
                return;

            try
            {
                // Apply menu visibility based on permissions
                ApplyMenuPermissions();

                // Apply feature access permissions
                ApplyFeaturePermissions();

                System.Diagnostics.Debug.WriteLine($"Permissions applied for user: {_currentUser.Username} (Role: {_currentUser.Role})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying permissions: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply menu item visibility and enabled state based on permissions
        /// </summary>
        private void ApplyMenuPermissions()
        {
            // Bookings menu - always visible but create/edit/delete may be restricted
            // Handled in BookingsView

            // Venues menu
            // All users can view venues, but only certain roles can create/edit/delete

            // Radio menu
            // RadioBoss Cloud and Stream require DJ role or higher
            // Handled in menu click handlers with permission checks

            // Chat menu - always visible but moderation hidden for regular users
            // Handled in ChatView

            // Users menu - only visible for Managers and above
            if (UsersMenuItem != null)
            {
                bool canManageUsers = _currentUser.Role == UserRole.SysAdmin || 
                                     _currentUser.Role == UserRole.Manager;
                UsersMenuItem.Visibility = canManageUsers ? Visibility.Visible : Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine($"Users menu visibility: {(canManageUsers ? "Visible (Manager/SysAdmin)" : "Hidden (Regular User)")}");
            }

            // Updater menu - ADMIN ONLY (SysAdmin or Manager)
            if (UpdaterMenuItem != null)
            {
                bool isAdmin = IsAdmin(_currentUser);
                UpdaterMenuItem.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine($"Updater menu visibility: {(isAdmin ? "Visible (Admin)" : "Hidden (Non-Admin)")}");
            }
            
            // Tests menu - ADMIN ONLY (SysAdmin)
            var testsMenu = this.FindName("TestsMenu") as MenuItem;
            if (testsMenu != null)
            {
                bool isSysAdmin = _currentUser.Role == UserRole.SysAdmin;
                testsMenu.Visibility = isSysAdmin ? Visibility.Visible : Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine($"Tests menu visibility: {(isSysAdmin ? "Visible (SysAdmin)" : "Hidden (Non-Admin)")}");
            }

            // Settings menu - always visible

            System.Diagnostics.Debug.WriteLine("Menu permissions applied");
        }

        /// <summary>
        /// Apply feature-level permissions
        /// </summary>
        private void ApplyFeaturePermissions()
        {
            // Store current user's permissions for easy access
            var canManageUsers = _permissionService!.HasPermission("system.manage_users");
            var canAccessRadioBoss = _permissionService.HasPermission("radio.radioboss_cloud");
            var canModerateChat = _permissionService.HasPermission("chat.moderate");

            System.Diagnostics.Debug.WriteLine($"User Permissions: ManageUsers={canManageUsers}, RadioBoss={canAccessRadioBoss}, ModerateChat={canModerateChat}");
        }

        /// <summary>
        /// Check if user has permission before allowing action
        /// </summary>
        private bool CheckPermission(string permissionId, string actionName = "")
        {
            if (_permissionService == null)
                return false;

            if (!_permissionService.HasPermission(permissionId))
            {
                string message = string.IsNullOrEmpty(actionName)
                    ? "You do not have permission to perform this action."
                    : $"You do not have permission to {actionName}.";

                MessageBox.Show(
                    message,
                    "Permission Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Get user's role display string
        /// </summary>
        private string GetUserRoleDisplay()
        {
            if (_currentUser == null)
                return "Unknown";

            return _currentUser.Role switch
            {
                UserRole.SysAdmin => "System Administrator",
                UserRole.Manager => "Manager",
                UserRole.VenueOwner => "Venue Owner",
                UserRole.DJ => "DJ",
                UserRole.User => "User",
                _ => _currentUser.Role.ToString()
            };
        }

        /// <summary>
        /// Check if user is an administrator (SysAdmin or Manager)
        /// </summary>
        private bool IsAdmin(User? user)
        {
            if (user == null)
                return false;

            return user.Role == UserRole.SysAdmin || user.Role == UserRole.Manager;
        }
    }
}
