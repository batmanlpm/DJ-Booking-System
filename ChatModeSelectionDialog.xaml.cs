using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DJBookingSystem
{
    public partial class ChatModeSelectionDialog : Window
    {
        public enum ChatMode
        {
            None,
            Integrated,
            Discord
        }

        public ChatMode SelectedMode { get; private set; } = ChatMode.None;

        public ChatModeSelectionDialog()
        {
            InitializeComponent();
        }

        private void IntegratedChat_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                border.BorderThickness = new Thickness(3);
            }
        }

        private void Discord_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderBrush = new SolidColorBrush(Color.FromRgb(88, 101, 242));
                border.BorderThickness = new Thickness(3);
            }
        }

        private void ChatOption_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                // Restore original border
                if (border.Name == "DiscordOptionBorder")
                {
                    border.BorderBrush = new SolidColorBrush(Color.FromRgb(88, 101, 242));
                }
                else
                {
                    border.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                }
                border.BorderThickness = new Thickness(2);
            }
        }

        private void IntegratedChat_Click(object sender, MouseButtonEventArgs e)
        {
            SelectedMode = ChatMode.Integrated;
            DialogResult = true;
            Close();
        }

        private void Discord_Click(object sender, MouseButtonEventArgs e)
        {
            SelectedMode = ChatMode.Discord;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            SelectedMode = ChatMode.None;
            DialogResult = false;
            Close();
        }
    }
}
