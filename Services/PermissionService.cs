using System;
using System.Collections.Generic;
using System.Linq;
using DJBookingSystem.Models;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Service for managing and checking user permissions
    /// </summary>
    public class PermissionService
    {
        private readonly User _currentUser;

        public PermissionService(User currentUser)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        }

        /// <summary>
        /// Check if user has a specific permission
        /// </summary>
        public bool HasPermission(string permissionId)
        {
            if (_currentUser == null) return false;

            // SysAdmin has all permissions
            if (_currentUser.Role == UserRole.SysAdmin)
                return true;

            // Check role-based permissions
            return permissionId switch
            {
                // System permissions
                "system.admin" => _currentUser.Role == UserRole.SysAdmin,
                "system.manage_users" => _currentUser.Role >= UserRole.Manager,
                "system.promote_users" => _currentUser.Role == UserRole.SysAdmin,
                "system.view_audit_logs" => _currentUser.Role >= UserRole.Manager,

                // Booking permissions
                "bookings.create" => _currentUser.Permissions.CanCreateBookings,
                "bookings.edit" => _currentUser.Permissions.CanEditBookings,
                "bookings.delete" => _currentUser.Permissions.CanDeleteBookings,
                "bookings.view_all" => _currentUser.Permissions.CanViewBookings,
                "bookings.approve" => _currentUser.Role >= UserRole.Manager,

                // Venue permissions
                "venues.create" => _currentUser.Permissions.CanRegisterVenues,
                "venues.edit" => _currentUser.Permissions.CanEditVenues || _currentUser.IsVenueOwner,
                "venues.delete" => _currentUser.Permissions.CanDeleteVenues,
                "venues.view_all" => _currentUser.Permissions.CanViewVenues,

                // Radio permissions
                "radio.player" => true, // Everyone can access radio player
                "radio.add_stations" => _currentUser.Role >= UserRole.Manager,
                "radio.delete_stations" => _currentUser.Role >= UserRole.Manager,
                "radio.radioboss_cloud" => _currentUser.Role >= UserRole.DJ,
                "radio.radioboss_stream" => _currentUser.Role >= UserRole.DJ,
                "radio.control_radioboss" => _currentUser.Role >= UserRole.Manager,

                // Chat permissions
                "chat.send_messages" => !_currentUser.IsGloballyMuted && !_currentUser.IsBanned,
                "chat.view" => !_currentUser.IsBanned,
                "chat.moderate" => _currentUser.Permissions.CanMuteUsers || _currentUser.Role >= UserRole.Manager,
                "chat.manage_rooms" => _currentUser.Role >= UserRole.Manager,

                // Discord permissions
                "discord.connect" => true,
                "discord.use_chat" => !_currentUser.IsGloballyMuted && !_currentUser.IsBanned,
                "discord.manage" => _currentUser.Role >= UserRole.Manager,

                // Candy-Bot permissions
                "candybot.use" => true,
                "candybot.voice_mode" => true,
                "candybot.desktop_widget" => true,
                "candybot.raunchy_mode" => _currentUser.Role != UserRole.User, // 18+ check done in UI

                // Settings permissions
                "settings.change_theme" => _currentUser.Permissions.CanCustomizeApp,
                "settings.notifications" => true,
                "settings.system" => _currentUser.Role >= UserRole.Manager,

                // Moderation permissions
                "moderation.ban_users" => _currentUser.Permissions.CanBanUsers,
                "moderation.mute_users" => _currentUser.Permissions.CanMuteUsers,
                "moderation.view_reports" => _currentUser.Permissions.CanViewReports,
                "moderation.warn_users" => _currentUser.Role >= UserRole.Manager,

                _ => false
            };
        }

        /// <summary>
        /// Check multiple permissions (user must have ALL)
        /// </summary>
        public bool HasAllPermissions(params string[] permissionIds)
        {
            return permissionIds.All(HasPermission);
        }

        /// <summary>
        /// Check multiple permissions (user must have ANY)
        /// </summary>
        public bool HasAnyPermission(params string[] permissionIds)
        {
            return permissionIds.Any(HasPermission);
        }

        /// <summary>
        /// Get all permissions for the current user
        /// </summary>
        public List<string> GetUserPermissions()
        {
            var permissions = new List<string>();

            // Get all permission IDs from the Permissions class
            var allPermissions = new[]
            {
                "system.admin", "system.manage_users", "system.promote_users", "system.view_audit_logs",
                "bookings.create", "bookings.edit", "bookings.delete", "bookings.view_all", "bookings.approve",
                "venues.create", "venues.edit", "venues.delete", "venues.view_all",
                "radio.player", "radio.add_stations", "radio.delete_stations", "radio.radioboss_cloud", "radio.radioboss_stream", "radio.control_radioboss",
                "chat.send_messages", "chat.view", "chat.moderate", "chat.manage_rooms",
                "discord.connect", "discord.use_chat", "discord.manage",
                "candybot.use", "candybot.voice_mode", "candybot.desktop_widget", "candybot.raunchy_mode",
                "settings.change_theme", "settings.notifications", "settings.system",
                "moderation.ban_users", "moderation.mute_users", "moderation.view_reports", "moderation.warn_users"
            };

            foreach (var permission in allPermissions)
            {
                if (HasPermission(permission))
                {
                    permissions.Add(permission);
                }
            }

            return permissions;
        }

        /// <summary>
        /// Apply role-based default permissions
        /// </summary>
        public static UserPermissions GetDefaultPermissionsForRole(UserRole role)
        {
            return role switch
            {
                UserRole.SysAdmin => new UserPermissions
                {
                    CanViewBookings = true,
                    CanCreateBookings = true,
                    CanEditBookings = true,
                    CanDeleteBookings = true,
                    CanViewVenues = true,
                    CanRegisterVenues = true,
                    CanEditVenues = true,
                    CanDeleteVenues = true,
                    CanToggleVenueStatus = true,
                    CanManageUsers = true,
                    CanCustomizeApp = true,
                    CanAccessSettings = true,
                    CanBanUsers = true,
                    CanMuteUsers = true,
                    CanViewReports = true
                },
                UserRole.Manager => new UserPermissions
                {
                    CanViewBookings = true,
                    CanCreateBookings = true,
                    CanEditBookings = true,
                    CanDeleteBookings = true,
                    CanViewVenues = true,
                    CanRegisterVenues = true,
                    CanEditVenues = true,
                    CanDeleteVenues = false,
                    CanToggleVenueStatus = true,
                    CanManageUsers = true,
                    CanCustomizeApp = true,
                    CanAccessSettings = true,
                    CanBanUsers = true,
                    CanMuteUsers = true,
                    CanViewReports = true
                },
                UserRole.DJ => new UserPermissions
                {
                    CanViewBookings = true,
                    CanCreateBookings = true,
                    CanEditBookings = true,
                    CanDeleteBookings = false,
                    CanViewVenues = true,
                    CanRegisterVenues = true,
                    CanEditVenues = false,
                    CanDeleteVenues = false,
                    CanToggleVenueStatus = false,
                    CanManageUsers = false,
                    CanCustomizeApp = true,
                    CanAccessSettings = true,
                    CanBanUsers = false,
                    CanMuteUsers = false,
                    CanViewReports = false
                },
                UserRole.VenueOwner => new UserPermissions
                {
                    CanViewBookings = true,
                    CanCreateBookings = true,
                    CanEditBookings = true,
                    CanDeleteBookings = false,
                    CanViewVenues = true,
                    CanRegisterVenues = true,
                    CanEditVenues = true,
                    CanDeleteVenues = false,
                    CanToggleVenueStatus = true,
                    CanManageUsers = false,
                    CanCustomizeApp = true,
                    CanAccessSettings = true,
                    CanBanUsers = false,
                    CanMuteUsers = false,
                    CanViewReports = false
                },
                UserRole.User => new UserPermissions
                {
                    CanViewBookings = true,
                    CanCreateBookings = true,
                    CanEditBookings = false,
                    CanDeleteBookings = false,
                    CanViewVenues = true,
                    CanRegisterVenues = false,
                    CanEditVenues = false,
                    CanDeleteVenues = false,
                    CanToggleVenueStatus = false,
                    CanManageUsers = false,
                    CanCustomizeApp = true,
                    CanAccessSettings = true,
                    CanBanUsers = false,
                    CanMuteUsers = false,
                    CanViewReports = false
                },
                _ => new UserPermissions()
            };
        }

        /// <summary>
        /// Check if user can perform an action on another user
        /// </summary>
        public bool CanModerateUser(User targetUser)
        {
            if (_currentUser.Role == UserRole.SysAdmin)
                return true;

            // Can't moderate users of equal or higher role
            if (targetUser.Role >= _currentUser.Role)
                return false;

            // Must have moderation permissions
            return _currentUser.Permissions.CanBanUsers || _currentUser.Permissions.CanMuteUsers;
        }

        /// <summary>
        /// Get permission display name
        /// </summary>
        public static string GetPermissionDisplayName(string permissionId)
        {
            return permissionId switch
            {
                "system.admin" => "System Administrator",
                "system.manage_users" => "Manage Users",
                "bookings.create" => "Create Bookings",
                "bookings.edit" => "Edit Bookings",
                "venues.create" => "Create Venues",
                "radio.radioboss_cloud" => "RadioBoss Cloud Control",
                "chat.moderate" => "Moderate Chat",
                _ => permissionId.Replace(".", " - ").Replace("_", " ")
            };
        }
    }
}
