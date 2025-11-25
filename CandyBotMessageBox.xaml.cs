using System.Windows;

namespace DJBookingSystem
{
    public partial class CandyBotMessageBox : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        private CandyBotMessageBox()
        {
            InitializeComponent();
        }

        public static MessageBoxResult Show(string message, string title = "Candy-Bot", 
            MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information)
        {
            var msgBox = new CandyBotMessageBox();
            msgBox.TitleText.Text = title;
            msgBox.MessageText.Text = message;

            // Set icon
            msgBox.IconText.Text = icon switch
            {
                MessageBoxImage.Information => "ℹ️",
                MessageBoxImage.Question => "❓",
                MessageBoxImage.Warning => "⚠️",
                MessageBoxImage.Error => "❌",
                _ => "ℹ️"
            };

            msgBox.IconText.Foreground = icon switch
            {
                MessageBoxImage.Information => System.Windows.Media.Brushes.CornflowerBlue,
                MessageBoxImage.Question => System.Windows.Media.Brushes.DeepSkyBlue,
                MessageBoxImage.Warning => System.Windows.Media.Brushes.Orange,
                MessageBoxImage.Error => System.Windows.Media.Brushes.Red,
                _ => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 105, 180))
            };

            // Configure buttons
            if (buttons == MessageBoxButton.YesNo)
            {
                msgBox.OKButton.Visibility = Visibility.Collapsed;
                msgBox.YesButton.Visibility = Visibility.Visible;
                msgBox.NoButton.Visibility = Visibility.Visible;
            }

            msgBox.ShowDialog();
            return msgBox.Result;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            Close();
        }
    }
}
