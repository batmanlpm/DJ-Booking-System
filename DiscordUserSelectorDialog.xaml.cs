using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class DiscordUserSelectorDialog : Window
    {
        private List<DiscordUserInfo> _allUsers = new List<DiscordUserInfo>();
        public DiscordUserInfo? SelectedUser { get; private set; }

        public DiscordUserSelectorDialog(List<DiscordUserInfo> users)
        {
            InitializeComponent();
            _allUsers = users;
            UsersListBox.ItemsSource = _allUsers;
            
            // Add converters to resources
            Resources.Add("StatusColorConverter", new StatusColorConverter());
            Resources.Add("StatusTextConverter", new StatusTextConverter());
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                UsersListBox.ItemsSource = _allUsers;
            }
            else
            {
                var filtered = _allUsers.Where(u => 
                    u.DisplayName.ToLower().Contains(searchText) ||
                    u.Username.ToLower().Contains(searchText)
                ).ToList();
                
                UsersListBox.ItemsSource = filtered;
            }
        }

        private void UsersListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (UsersListBox.SelectedItem is DiscordUserInfo user)
            {
                SelectedUser = user;
                DialogResult = true;
                Close();
            }
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (UsersListBox.SelectedItem is DiscordUserInfo user)
            {
                SelectedUser = user;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(
                    "Please select a user to send a direct message.",
                    "No User Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    // Converter for online status color
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isOnline)
            {
                return isOnline ? new SolidColorBrush(Colors.LimeGreen) : new SolidColorBrush(Colors.Gray);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter for online status text
    public class StatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isOnline)
            {
                return isOnline ? "Online" : "Offline";
            }
            return "Offline";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
