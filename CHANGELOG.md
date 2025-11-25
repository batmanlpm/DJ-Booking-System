# Changelog

## Version 1.2.5 (2025-01-XX)

### New Features
- ✅ **Comprehensive Permission System** - All 17 permissions now fully enforced
  - Booking permissions (View, Create, Edit, Delete)
  - Venue permissions (View, Register, Edit, Delete, Toggle Status)
  - Admin permissions (Manage Users, Customize App, Access Settings)
  - Moderation permissions (Ban, Mute, View Reports, Resolve Reports)
  - RadioBOSS permissions (View, Control)
- 🎨 **New Promote/Demote User Dialog** - Professional role selection UI
- 📻 **Radio Control Center** - Unified 3-panel layout for all radio stations
  - LivePartyMusic.fm (C40)
  - Radio Station Listener (21 stations)
  - Candy-Bot Radio Relay (C19)
- 📌 **Stay On Top** - Pin window above all others
- ⚙️ **Simplified Menu Structure** - Radio and Settings are now single-click navigation

### Permission Enforcement Details
- **Dual-Level Security**: UI controls disabled + code-level validation
- **Admin Bypass**: SysAdmin and Manager roles bypass all permission checks
- **Real-Time Enforcement**: Permission changes apply immediately
- **Debug Logging**: All permission checks logged for troubleshooting

### UI/UX Improvements
- 🎭 **Role Selection Dialog** - Clean, modern interface with visual feedback
- 🎨 **Green/Pink Theme** - Consistent color scheme matching app design
- 📋 **Admin Tools Section** Only visible to admins
- 🔒 **Read-Only RadioBOSS Mode** - View-only access for non-admin users

### Bug Fixes
- ✅ Fixed 7 compiler warnings (CS1998, CS8629)
- ✅ Fixed Settings menu initialization with user context
- ✅ Fixed RadioBOSS permission enforcement
- ✅ Fixed venue management permission checks

### Technical Changes
- 🔧 Updated all views to enforce permissions properly
- 🔧 Removed deprecated InputBox dialogs
- 🔧 Added EnforcePermissions() methods to all management views
- 🔧 Improved code organization and maintainability

### Files Modified (11 files)
1. `Views\Bookings\BookingsView.xaml.cs` - Booking permissions
2. `AdminVenueManagementWindow.xaml.cs` - Venue permissions
3. `Views\UsersView.xaml.cs` - Admin + Moderation permissions
4. `Views\Radio\RadioBossCloudView.xaml.cs` - RadioBOSS permissions
5. `Views\Radio\RadioBossStreamView.xaml.cs` - RadioBOSS permissions
6. `Views\Radio\RadioUnifiedView.xaml` - New 3-panel layout
7. `Views\Radio\RadioUnifiedView.xaml.cs` - Navigation logic
8. `Views\PromoteDemoteUserWindow.xaml` - New role dialog
9. `Views\PromoteDemoteUserWindow.xaml.cs` - Role selection logic
10. `MainWindow.xaml` - Menu structure updates
11. `Services\EnhancedFileOrganizerService.cs` - Warning fixes

### Build Status
- ✅ 0 Errors
- ✅ 0 Warnings
- ✅ All permissions tested and verified

---

## Version 1.2.0 (2025-01-15)

### New Features
- 🔗 Discord WebView2 integration with auto-login
- 🔄 Hourly automatic forced updates
- 💬 Chat mode selection dialog (fullscreen)
- ⚡ Improved UI performance and responsiveness
- 📦 Complete dependency inclusion in installer

### Bug Fixes
- 🔧 Fixed WebView2 initialization error
- 🔧 Fixed online status tracking
- 🔧 Fixed auto-login issues for Discord
- 🔧 Resolved freezing issues on bookings view
- 🔧 Fixed null reference errors in venues

### Improvements
- ⚡ Optimized startup time
- 🔒 Enhanced security with SSL certificate pinning
- 📝 Better error handling and logging
- 🎨 Improved user experience for updates

### Technical Changes
- 🔧 Migrated to .NET 8
- 📦 Self-contained deployment (no .NET installation required)
- 🔄 Hourly update checks on the hour
- ⚠️ Forced update downloads with no cancel option

---

## Version 1.0.0 (Initial Release)

### Initial Features
- 📅 Booking management system
- 🏢 Venue management
- 👥 User management with roles
- 🔐 Authentication and authorization
- ☁️ Azure Cosmos DB integration
- 📻 Radio streaming integration
- 🤖 Candy Bot assistant
- 🎨 Modern space-themed UI

---

**The Fallen Collective & Mega Byte I.T Services**
