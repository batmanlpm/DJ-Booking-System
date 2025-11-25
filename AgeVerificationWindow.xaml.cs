using System.Windows;

namespace DJBookingSystem
{
    public partial class AgeVerificationWindow : Window
    {
        public bool IsVerified { get; private set; } = false;

        public AgeVerificationWindow()
        {
            InitializeComponent();
        }

        private void AgeCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateContinueButton();
        }

        private void ConsentCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateContinueButton();
        }

        private void UnderstandCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateContinueButton();
        }

        private void UpdateContinueButton()
        {
            // Enable continue button only if all checkboxes are checked
            ContinueButton.IsEnabled = 
                AgeCheckbox.IsChecked == true && 
                ConsentCheckbox.IsChecked == true && 
                UnderstandCheckbox.IsChecked == true;
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            IsVerified = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsVerified = false;
            this.Close();
        }
    }
}
