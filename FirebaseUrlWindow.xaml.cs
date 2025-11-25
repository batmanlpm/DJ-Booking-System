using System.Windows;

namespace DJBookingSystem
{
    public partial class FirebaseUrlWindow : Window
    {
        public string FirebaseUrl { get; private set; } = string.Empty;

        public FirebaseUrlWindow(bool stayOnTop = false)
        {
            InitializeComponent();

            // Set default Firebase URL
            FirebaseUrlTextBox.Text = "https://new-booking-system-46908-default-rtdb.firebaseio.com/";

            // Apply Stay on Top preference
            this.Topmost = stayOnTop;
           
            // Force window to front
            this.Loaded += (s, e) =>
            {
                this.Activate();
                this.Focus();
                FirebaseUrlTextBox.Focus();
                FirebaseUrlTextBox.SelectAll();
                System.Diagnostics.Debug.WriteLine("FirebaseUrlWindow loaded and activated");
            };

            this.Closing += (s, e) =>
          {
                System.Diagnostics.Debug.WriteLine($"FirebaseUrlWindow closing - DialogResult: {DialogResult}");
            };
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            string url = FirebaseUrlTextBox.Text.Trim();

            if (string.IsNullOrEmpty(url) || !url.StartsWith("https://"))
            {
                MessageBox.Show("Please enter a valid Firebase URL (must start with https://)",
                    "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            FirebaseUrl = url;
            DialogResult = true;
            Close();
        }
    }
}
