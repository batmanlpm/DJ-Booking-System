using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem.Views
{
    /// <summary>
    /// Window for creating new user accounts with role-based permissions
    /// </summary>
    public partial class CreateUserWindow : Window
    {
        private readonly CosmosDbService _cosmosService;
        private readonly User _currentUser;
        
        public User? CreatedUser { get; private set; }

        public CreateUserWindow(CosmosDbService cosmosService, User currentUser)
        {
            InitializeComponent();
            _cosmosService = cosmosService ?? throw new ArgumentNullException(nameof(cosmosService));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

            // Only SysAdmin can create Manager and SysAdmin accounts
            if (_currentUser.Role != UserRole.SysAdmin)
            {
                ManagerRoleCheckBox.Visibility = Visibility.Collapsed;
                SysAdminRoleCheckBox.Visibility = Visibility.Collapsed;
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AdminRole_Checked(object sender, RoutedEventArgs e)
        {
            if (_currentUser.Role != UserRole.SysAdmin)
            {
                if (sender == ManagerRoleCheckBox)
                    ManagerRoleCheckBox.IsChecked = false;
                else if (sender == SysAdminRoleCheckBox)
                    SysAdminRoleCheckBox.IsChecked = false;
                
                ShowError("Only SysAdmin can create Manager or SysAdmin accounts");
            }
        }

        private async void CreateUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInputs(out var validationError))
                {
                    ShowError(validationError!);
                    return;
                }

                var selectedRoles = GetSelectedRoles();
                
                if (selectedRoles.Count == 0)
                {
                    ShowError("Please select at least one role");
                    return;
                }

                if (!ValidateRolePermissions(selectedRoles))
                {
                    ShowError("Only SysAdmin can create Manager or SysAdmin accounts");
                    return;
                }

                var username = UsernameTextBox.Text.Trim();
                var existingUser = await _cosmosService.GetUserByUsernameAsync(username);
                
                if (existingUser != null)
                {
                    ShowError($"Username '{username}' is already taken");
                    UsernameTextBox.Focus();
                    return;
                }

                var newUser = CreateNewUser(username, selectedRoles);
                
                StatusText.Text = "Creating user...";
                StatusText.Foreground = Brushes.Yellow;

                string userId = await _cosmosService.AddUserAsync(newUser);
                await LogUserCreation(userId, newUser, selectedRoles);

                CreatedUser = newUser;

                ShowSuccessMessage(newUser, selectedRoles);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to create user: {ex.Message}");
            }
        }

        private bool ValidateInputs(out string? error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                error = "Please enter a username";
                UsernameTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                error = "Please enter a password";
                PasswordBox.Focus();
                return false;
            }

            if (PasswordBox.Password.Length < 6)
            {
                error = "Password must be at least 6 characters";
                PasswordBox.Focus();
                return false;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                error = "Passwords do not match";
                ConfirmPasswordBox.Focus();
                return false;
            }

            return true;
        }

        private List<UserRole> GetSelectedRoles()
        {
            var roles = new List<UserRole>();

            if (UserRoleCheckBox.IsChecked == true) roles.Add(UserRole.User);
            if (DJRoleCheckBox.IsChecked == true) roles.Add(UserRole.DJ);
            if (VenueOwnerRoleCheckBox.IsChecked == true) roles.Add(UserRole.VenueOwner);
            if (ManagerRoleCheckBox.IsChecked == true) roles.Add(UserRole.Manager);
            if (SysAdminRoleCheckBox.IsChecked == true) roles.Add(UserRole.SysAdmin);

            return roles;
        }

        private bool ValidateRolePermissions(List<UserRole> roles)
        {
            return !((roles.Contains(UserRole.Manager) || roles.Contains(UserRole.SysAdmin)) && 
                     _currentUser.Role != UserRole.SysAdmin);
        }

        private User CreateNewUser(string username, List<UserRole> selectedRoles)
        {
            UserRole primaryRole = selectedRoles.Max();

            return new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = username,
                PasswordHash = AuthenticationService.HashPassword(PasswordBox.Password),
                FullName = username,
                Email = $"{username}@djbooking.local",
                Role = primaryRole,
                IsActive = true,
                IsDJ = selectedRoles.Contains(UserRole.DJ),
                IsVenueOwner = selectedRoles.Contains(UserRole.VenueOwner),
                IsBanned = false,
                IsGloballyMuted = false,
                RegisteredIP = "127.0.0.1",
                CurrentIP = "127.0.0.1",
                IPHistory = new List<string> { "127.0.0.1" },
                CreatedAt = DateTime.Now,
                LastLogin = DateTime.MinValue,
                Permissions = CreateCombinedPermissions(selectedRoles),
                AppPreferences = new UserAppPreferences
                {
                    ThemeName = "Space",
                    ColorTheme = "Green",
                    RememberMe = false,
                    AutoLogin = false,
                    StayOnTop = false
                }
            };
        }

        private async Task LogUserCreation(string userId, User user, List<UserRole> roles)
        {
            var rolesList = string.Join(", ", roles.Select(r => r.ToString()));
            var log = new UserActionLog
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Username = user.Username,
                ActionType = UserActionType.Created,
                Timestamp = DateTime.Now,
                PerformedBy = _currentUser.Username,
                ActionDetails = $"User created with roles: {rolesList}",
                Metadata = new Dictionary<string, string>
                {
                    { "PrimaryRole", user.Role.ToString() },
                    { "AllRoles", rolesList },
                    { "IsDJ", user.IsDJ.ToString() },
                    { "IsVenueOwner", user.IsVenueOwner.ToString() }
                }
            };
            await _cosmosService.AddUserActionLogAsync(log);
        }

        private static void ShowSuccessMessage(User user, List<UserRole> roles)
        {
            var rolesList = string.Join(", ", roles.Select(r => r.ToString()));
            MessageBox.Show(
                $"User '{user.Username}' created successfully!\n\n" +
                $"Username: {user.Username}\n" +
                $"Roles: {rolesList}\n" +
                $"Primary Role: {user.Role}\n\n" +
                "The user can now login with their credentials.",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private UserPermissions CreateCombinedPermissions(List<UserRole> roles)
        {
            var permissions = new UserPermissions();

            foreach (var role in roles)
            {
                var rolePermissions = PermissionService.GetDefaultPermissionsForRole(role);
                
                // Grant permission if ANY role has it (OR operation)
                permissions.CanViewBookings |= rolePermissions.CanViewBookings;
                permissions.CanCreateBookings |= rolePermissions.CanCreateBookings;
                permissions.CanEditBookings |= rolePermissions.CanEditBookings;
                permissions.CanDeleteBookings |= rolePermissions.CanDeleteBookings;
                permissions.CanViewVenues |= rolePermissions.CanViewVenues;
                permissions.CanRegisterVenues |= rolePermissions.CanRegisterVenues;
                permissions.CanEditVenues |= rolePermissions.CanEditVenues;
                permissions.CanDeleteVenues |= rolePermissions.CanDeleteVenues;
                permissions.CanToggleVenueStatus |= rolePermissions.CanToggleVenueStatus;
                permissions.CanManageUsers |= rolePermissions.CanManageUsers;
                permissions.CanCustomizeApp |= rolePermissions.CanCustomizeApp;
                permissions.CanAccessSettings |= rolePermissions.CanAccessSettings;
                permissions.CanBanUsers |= rolePermissions.CanBanUsers;
                permissions.CanMuteUsers |= rolePermissions.CanMuteUsers;
                permissions.CanViewReports |= rolePermissions.CanViewReports;
            }

            return permissions;
        }

        private void ShowError(string message)
        {
            StatusText.Text = message;
            StatusText.Foreground = Brushes.Red;
        }
    }
}
