using System;
using System.Windows;

namespace DJBookingSystem.Views
{
    public partial class BanReasonDialog : Window
    {
        public string BanReason { get; private set; } = string.Empty;
        public bool Confirmed { get; private set; } = false;

        public BanReasonDialog(string username)
        {
            InitializeComponent();
            UsernameText.Text = $"User: {username}";
            ReasonTextBox.Focus();
            ReasonTextBox.SelectAll();
        }

        private void Ban_Click(object sender, RoutedEventArgs e)
        {
            string reason = ReasonTextBox.Text.Trim();
            
            if (string.IsNullOrEmpty(reason) || reason == "Reason for ban...")
            {
                MessageBox.Show(
                    "You must provide a reason for the ban.",
                    "Reason Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (reason.Length < 10)
            {
                MessageBox.Show(
                    "Ban reason must be at least 10 characters long.",
                    "Reason Too Short",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            BanReason = reason;
            Confirmed = true;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = false;
            DialogResult = false;
            Close();
        }
    }
}
