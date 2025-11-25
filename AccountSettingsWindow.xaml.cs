using System.Windows;

namespace DJBookingSystem
{
    public partial class AccountSettingsWindow : Window
    {
        public AccountSettingsWindow()
        {
            InitializeComponent();
        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete your account? This action cannot be undone!",
                "Delete Account",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Account deletion is not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
