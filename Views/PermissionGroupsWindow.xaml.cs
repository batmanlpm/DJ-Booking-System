using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DJBookingSystem.Views
{
    /// <summary>
    /// Permission Groups Window for managing predefined permission templates
    /// </summary>
    public partial class PermissionGroupsWindow : Window
    {
        private string _selectedGroup = string.Empty;

        public PermissionGroupsWindow()
        {
            InitializeComponent();
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

        private void StandardUserGroup_Click(object sender, RoutedEventArgs e)
        {
            _selectedGroup = "Standard User";
            ShowGroupDetails(
                "Standard User Group",
                "Default permissions for regular users.\n\n" +
                "Permissions:\n" +
                "? Can view bookings and venues\n" +
                "? Basic app access\n" +
                "? Cannot create bookings (unless DJ)\n" +
                "? Cannot manage venues\n" +
                "? No admin access"
            );
        }

        private void DJGroup_Click(object sender, RoutedEventArgs e)
        {
            _selectedGroup = "DJ";
            ShowGroupDetails(
                "DJ Group",
                "Permissions for DJs who book performance slots.\n\n" +
                "Permissions:\n" +
                "? Can view bookings and venues\n" +
                "? Can create and edit own bookings\n" +
                "? Can update streaming links\n" +
                "? Cannot delete bookings\n" +
                "? Cannot manage venues\n" +
                "? No admin access"
            );
        }

        private void VenueOwnerGroup_Click(object sender, RoutedEventArgs e)
        {
            _selectedGroup = "Venue Owner";
            ShowGroupDetails(
                "Venue Owner Group",
                "Permissions for venue owners and managers.\n\n" +
                "Permissions:\n" +
                "? Can view all bookings\n" +
                "? Can create, edit, and delete venues\n" +
                "? Can manage venue schedules\n" +
                "? Can toggle venue status\n" +
                "? Cannot ban users\n" +
                "? Limited admin access"
            );
        }

        private void ModeratorGroup_Click(object sender, RoutedEventArgs e)
        {
            _selectedGroup = "Moderator";
            ShowGroupDetails(
                "Moderator Group",
                "Permissions for chat and community moderators.\n\n" +
                "Permissions:\n" +
                "? Can view all content\n" +
                "? Can mute users in chat\n" +
                "? Can ban users (temporary)\n" +
                "? Can view reports\n" +
                "? Can resolve reports\n" +
                "? Cannot manage system settings\n" +
                "? Cannot delete users"
            );
        }

        private void AdminGroup_Click(object sender, RoutedEventArgs e)
        {
            _selectedGroup = "Admin";
            ShowGroupDetails(
                "Admin Group",
                "Full system access for administrators.\n\n" +
                "Permissions:\n" +
                "? Can manage all users\n" +
                "? Can create/edit/delete anything\n" +
                "? Can ban users (permanent)\n" +
                "? Can access all settings\n" +
                "? Can view audit logs\n" +
                "? Can control RadioBOSS\n" +
                "? Full system access"
            );
        }

        private void ShowGroupDetails(string groupName, string description)
        {
            DetailsPanel.Children.Clear();

            var titleBlock = new TextBlock
            {
                Text = groupName.ToUpper(),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.LimeGreen,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var descBlock = new TextBlock
            {
                Text = description,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11,
                Foreground = Brushes.LimeGreen,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var noteBlock = new TextBlock
            {
                Text = "?? Note: This is a preview of the permission group. " +
                       "Full editing capabilities will be available in a future update.",
                FontFamily = new FontFamily("Consolas"),
                FontSize = 10,
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.Gold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 20, 0, 0)
            };

            DetailsPanel.Children.Add(titleBlock);
            DetailsPanel.Children.Add(descBlock);
            DetailsPanel.Children.Add(noteBlock);

            PermissionsGrid.Visibility = Visibility.Collapsed;
        }

        private void SaveGroup_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedGroup))
            {
                MessageBox.Show(
                    "Please select a permission group first.",
                    "No Group Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show(
                $"Permission Group: {_selectedGroup}\n\n" +
                "This feature is currently in preview mode.\n" +
                "Full save functionality will be available soon!",
                "Coming Soon",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
