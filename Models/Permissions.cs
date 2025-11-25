using System;
using System.Collections.Generic;

namespace DJBookingSystem.Models
{
    /// <summary>
    /// Comprehensive permission system for all features
    /// </summary>
    public class Permission
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Permission role with associated permissions (renamed to avoid conflict with UserRole enum)
    /// </summary>
    public class PermissionRole
    {
        public string RoleName { get; set; } = string.Empty;
        public List<string> PermissionIds { get; set; } = new List<string>();
        public int Priority { get; set; } = 0; // Higher = more powerful
    }

    /// <summary>
    /// Permission categories and definitions
    /// </summary>
    public static class Permissions
    {
        // ==================== SYSTEM PERMISSIONS ====================
        public static Permission SystemAdmin = new()
        {
            Id = "system.admin",
            Name = "System Administrator",
            Description = "Full system access - can promote users, manage all settings",
            Category = "System"
        };

        public static Permission ManageUsers = new()
        {
            Id = "system.manage_users",
            Name = "Manage Users",
            Description = "Create, edit, delete user accounts (except SysAdmin)",
            Category = "System"
        };

        public static Permission PromoteUsers = new()
        {
            Id = "system.promote_users",
            Name = "Promote Users",
            Description = "Change user roles and permissions (SysAdmin only)",
            Category = "System"
        };

        public static Permission ViewAuditLogs = new()
        {
            Id = "system.view_audit_logs",
            Name = "View Audit Logs",
            Description = "Access system audit and activity logs",
            Category = "System"
        };

        // ==================== BOOKING PERMISSIONS ====================
        public static Permission CreateBookings = new()
        {
            Id = "bookings.create",
            Name = "Create Bookings",
            Description = "Create new DJ bookings",
            Category = "Bookings"
        };

        public static Permission EditBookings = new()
        {
            Id = "bookings.edit",
            Name = "Edit Bookings",
            Description = "Modify existing bookings",
            Category = "Bookings"
        };

        public static Permission DeleteBookings = new()
        {
            Id = "bookings.delete",
            Name = "Delete Bookings",
            Description = "Remove bookings from the system",
            Category = "Bookings"
        };

        public static Permission ViewAllBookings = new()
        {
            Id = "bookings.view_all",
            Name = "View All Bookings",
            Description = "See bookings from all users",
            Category = "Bookings"
        };

        public static Permission ApproveBookings = new()
        {
            Id = "bookings.approve",
            Name = "Approve Bookings",
            Description = "Approve or reject booking requests",
            Category = "Bookings"
        };

        // ==================== VENUE PERMISSIONS ====================
        public static Permission CreateVenues = new()
        {
            Id = "venues.create",
            Name = "Create Venues",
            Description = "Add new venues to the system",
            Category = "Venues"
        };

        public static Permission EditVenues = new()
        {
            Id = "venues.edit",
            Name = "Edit Venues",
            Description = "Modify venue information",
            Category = "Venues"
        };

        public static Permission DeleteVenues = new()
        {
            Id = "venues.delete",
            Name = "Delete Venues",
            Description = "Remove venues from the system",
            Category = "Venues"
        };

        public static Permission ViewAllVenues = new()
        {
            Id = "venues.view_all",
            Name = "View All Venues",
            Description = "See all venues in the system",
            Category = "Venues"
        };

        // ==================== RADIO PERMISSIONS ====================
        public static Permission AccessRadioPlayer = new()
        {
            Id = "radio.player",
            Name = "Access Radio Player",
            Description = "Listen to radio stations",
            Category = "Radio"
        };

        public static Permission AddRadioStations = new()
        {
            Id = "radio.add_stations",
            Name = "Add Radio Stations",
            Description = "Add new radio stations to the database",
            Category = "Radio"
        };

        public static Permission DeleteRadioStations = new()
        {
            Id = "radio.delete_stations",
            Name = "Delete Radio Stations",
            Description = "Remove radio stations from the database",
            Category = "Radio"
        };

        public static Permission AccessRadioBossCloud = new()
        {
            Id = "radio.radioboss_cloud",
            Name = "Access RadioBoss Cloud",
            Description = "Access RadioBoss Cloud control panel",
            Category = "Radio"
        };

        public static Permission AccessRadioBossStream = new()
        {
            Id = "radio.radioboss_stream",
            Name = "Access RadioBoss Stream",
            Description = "Access RadioBoss Stream control panel",
            Category = "Radio"
        };

        public static Permission ControlRadioBoss = new()
        {
            Id = "radio.control_radioboss",
            Name = "Control RadioBoss",
            Description = "Control playback and settings on RadioBoss",
            Category = "Radio"
        };

        // ==================== CHAT PERMISSIONS ====================
        public static Permission SendMessages = new()
        {
            Id = "chat.send_messages",
            Name = "Send Messages",
            Description = "Send messages in the chat system",
            Category = "Chat"
        };

        public static Permission ViewChat = new()
        {
            Id = "chat.view",
            Name = "View Chat",
            Description = "View chat messages",
            Category = "Chat"
        };

        public static Permission ModerateChat = new()
        {
            Id = "chat.moderate",
            Name = "Moderate Chat",
            Description = "Delete messages, warn users, timeout/ban",
            Category = "Chat"
        };

        public static Permission ManageChatRooms = new()
        {
            Id = "chat.manage_rooms",
            Name = "Manage Chat Rooms",
            Description = "Create, edit, delete chat rooms",
            Category = "Chat"
        };

        // ==================== DISCORD PERMISSIONS ====================
        public static Permission ConnectDiscord = new()
        {
            Id = "discord.connect",
            Name = "Connect Discord",
            Description = "Link Discord account to the system",
            Category = "Discord"
        };

        public static Permission UseDiscordChat = new()
        {
            Id = "discord.use_chat",
            Name = "Use Discord Chat",
            Description = "Send and receive Discord messages through the app",
            Category = "Discord"
        };

        public static Permission ManageDiscordIntegration = new()
        {
            Id = "discord.manage",
            Name = "Manage Discord Integration",
            Description = "Configure Discord bot and integration settings",
            Category = "Discord"
        };

        // ==================== CANDYBOT PERMISSIONS ====================
        public static Permission UseCandyBot = new()
        {
            Id = "candybot.use",
            Name = "Use Candy-Bot",
            Description = "Interact with Candy-Bot AI assistant",
            Category = "Candy-Bot"
        };

        public static Permission CandyBotVoiceMode = new()
        {
            Id = "candybot.voice_mode",
            Name = "Candy-Bot Voice Mode",
            Description = "Enable voice responses from Candy-Bot",
            Category = "Candy-Bot"
        };

        public static Permission CandyBotDesktopWidget = new()
        {
            Id = "candybot.desktop_widget",
            Name = "Candy-Bot Desktop Widget",
            Description = "Use Candy-Bot as a desktop widget",
            Category = "Candy-Bot"
        };

        public static Permission CandyBotRaunchyMode = new()
        {
            Id = "candybot.raunchy_mode",
            Name = "Candy-Bot Raunchy Mode (18+)",
            Description = "Access Candy-Bot's raunchy personality mode",
            Category = "Candy-Bot"
        };

        // ==================== SETTINGS PERMISSIONS ====================
        public static Permission ChangeTheme = new()
        {
            Id = "settings.change_theme",
            Name = "Change Theme",
            Description = "Change application color theme",
            Category = "Settings"
        };

        public static Permission ConfigureNotifications = new()
        {
            Id = "settings.notifications",
            Name = "Configure Notifications",
            Description = "Manage notification preferences",
            Category = "Settings"
        };

        public static Permission ManageSystemSettings = new()
        {
            Id = "settings.system",
            Name = "Manage System Settings",
            Description = "Configure system-wide settings (Admin only)",
            Category = "Settings"
        };

        // ==================== MODERATION PERMISSIONS ====================
        public static Permission ViewReports = new()
        {
            Id = "moderation.view_reports",
            Name = "View Reports",
            Description = "View user reports and moderation queue",
            Category = "Moderation"
        };

        public static Permission WarnUsers = new()
        {
            Id = "moderation.warn_users",
            Name = "Warn Users",
            Description = "Issue warnings to users",
            Category = "Moderation"
        };

        public static Permission TimeoutUsers = new()
        {
            Id = "moderation.timeout_users",
            Name = "Timeout Users",
            Description = "Temporarily restrict user access",
            Category = "Moderation"
        };

        public static Permission BanUsers = new()
        {
            Id = "moderation.ban_users",
            Name = "Ban Users",
            Description = "Permanently ban users from the system",
            Category = "Moderation"
        };

        public static Permission DeleteUserContent = new()
        {
            Id = "moderation.delete_content",
            Name = "Delete User Content",
            Description = "Delete messages, bookings, or other user content",
            Category = "Moderation"
        };

        // ==================== ALL PERMISSIONS LIST ====================
        public static List<Permission> AllPermissions = new()
        {
            // System
            SystemAdmin, ManageUsers, PromoteUsers, ViewAuditLogs,
            
            // Bookings
            CreateBookings, EditBookings, DeleteBookings, ViewAllBookings, ApproveBookings,
            
            // Venues
            CreateVenues, EditVenues, DeleteVenues, ViewAllVenues,
            
            // Radio
            AccessRadioPlayer, AddRadioStations, DeleteRadioStations,
            AccessRadioBossCloud, AccessRadioBossStream, ControlRadioBoss,
            
            // Chat
            SendMessages, ViewChat, ModerateChat, ManageChatRooms,
            
            // Discord
            ConnectDiscord, UseDiscordChat, ManageDiscordIntegration,
            
            // Candy-Bot
            UseCandyBot, CandyBotVoiceMode, CandyBotDesktopWidget, CandyBotRaunchyMode,
            
            // Settings
            ChangeTheme, ConfigureNotifications, ManageSystemSettings,
            
            // Moderation
            ViewReports, WarnUsers, TimeoutUsers, BanUsers, DeleteUserContent
        };
    }

    /// <summary>
    /// Predefined permission roles with permissions
    /// </summary>
    public static class PermissionRoles
    {
        // ==================== SYSTEM ADMINISTRATOR ====================
        public static PermissionRole SysAdmin = new()
        {
            RoleName = "SysAdmin",
            Priority = 100,
            PermissionIds = new List<string>
            {
                // Has ALL permissions
                "system.admin", "system.manage_users", "system.promote_users", "system.view_audit_logs",
                "bookings.create", "bookings.edit", "bookings.delete", "bookings.view_all", "bookings.approve",
                "venues.create", "venues.edit", "venues.delete", "venues.view_all",
                "radio.player", "radio.add_stations", "radio.delete_stations",
                "radio.radioboss_cloud", "radio.radioboss_stream", "radio.control_radioboss",
                "chat.send_messages", "chat.view", "chat.moderate", "chat.manage_rooms",
                "discord.connect", "discord.use_chat", "discord.manage",
                "candybot.use", "candybot.voice_mode", "candybot.desktop_widget", "candybot.raunchy_mode",
                "settings.change_theme", "settings.notifications", "settings.system",
                "moderation.view_reports", "moderation.warn_users", "moderation.timeout_users",
                "moderation.ban_users", "moderation.delete_content"
            }
        };

        // ==================== ADMINISTRATOR ====================
        public static PermissionRole Admin = new()
        {
            RoleName = "Admin",
            Priority = 80,
            PermissionIds = new List<string>
            {
                // Most permissions except promotion and system settings
                "system.manage_users", "system.view_audit_logs",
                "bookings.create", "bookings.edit", "bookings.delete", "bookings.view_all", "bookings.approve",
                "venues.create", "venues.edit", "venues.delete", "venues.view_all",
                "radio.player", "radio.add_stations", "radio.delete_stations",
                "radio.radioboss_cloud", "radio.radioboss_stream", "radio.control_radioboss",
                "chat.send_messages", "chat.view", "chat.moderate", "chat.manage_rooms",
                "discord.connect", "discord.use_chat",
                "candybot.use", "candybot.voice_mode", "candybot.desktop_widget", "candybot.raunchy_mode",
                "settings.change_theme", "settings.notifications",
                "moderation.view_reports", "moderation.warn_users", "moderation.timeout_users",
                "moderation.ban_users", "moderation.delete_content"
            }
        };

        // ==================== MODERATOR ====================
        public static PermissionRole Moderator = new()
        {
            RoleName = "Moderator",
            Priority = 60,
            PermissionIds = new List<string>
            {
                // Moderation and basic features
                "bookings.create", "bookings.edit", "bookings.view_all",
                "venues.view_all",
                "radio.player", "radio.add_stations",
                "chat.send_messages", "chat.view", "chat.moderate",
                "discord.connect", "discord.use_chat",
                "candybot.use", "candybot.voice_mode", "candybot.desktop_widget",
                "settings.change_theme", "settings.notifications",
                "moderation.view_reports", "moderation.warn_users", "moderation.timeout_users",
                "moderation.delete_content"
            }
        };

        // ==================== DJ ====================
        public static PermissionRole DJ = new()
        {
            RoleName = "DJ",
            Priority = 40,
            PermissionIds = new List<string>
            {
                // DJ-specific permissions
                "bookings.create", "bookings.edit",
                "venues.view_all",
                "radio.player", "radio.add_stations",
                "radio.radioboss_cloud", "radio.radioboss_stream", "radio.control_radioboss",
                "chat.send_messages", "chat.view",
                "discord.connect", "discord.use_chat",
                "candybot.use", "candybot.voice_mode", "candybot.desktop_widget", "candybot.raunchy_mode",
                "settings.change_theme", "settings.notifications"
            }
        };

        // ==================== VENUE MANAGER ====================
        public static PermissionRole VenueManager = new()
        {
            RoleName = "VenueManager",
            Priority = 50,
            PermissionIds = new List<string>
            {
                // Venue management focus
                "bookings.create", "bookings.edit", "bookings.view_all", "bookings.approve",
                "venues.create", "venues.edit", "venues.view_all",
                "radio.player",
                "chat.send_messages", "chat.view",
                "discord.connect", "discord.use_chat",
                "candybot.use", "candybot.voice_mode",
                "settings.change_theme", "settings.notifications"
            }
        };

        // ==================== USER ====================
        public static PermissionRole User = new()
        {
            RoleName = "User",
            Priority = 20,
            PermissionIds = new List<string>
            {
                // Basic user permissions
                "bookings.create",
                "venues.view_all",
                "radio.player", "radio.add_stations",
                "chat.send_messages", "chat.view",
                "discord.connect", "discord.use_chat",
                "candybot.use", "candybot.voice_mode", "candybot.desktop_widget",
                "settings.change_theme", "settings.notifications"
            }
        };

        // ==================== GUEST ====================
        public static PermissionRole Guest = new()
        {
            RoleName = "Guest",
            Priority = 10,
            PermissionIds = new List<string>
            {
                // Very limited permissions
                "venues.view_all",
                "radio.player",
                "chat.view",
                "candybot.use",
                "settings.change_theme"
            }
        };

        // ==================== ALL ROLES LIST ====================
        public static List<PermissionRole> AllRoles = new()
        {
            SysAdmin, Admin, Moderator, DJ, VenueManager, User, Guest
        };
    }
}
