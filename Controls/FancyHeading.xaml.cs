using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DJBookingSystem.Controls
{
    public partial class FancyHeading : UserControl
    {
        public FancyHeading()
        {
            InitializeComponent();
            HeadingText.MouseLeftButtonDown += HeadingText_MouseLeftButtonDown;
        }

        public string Text
        {
            get => HeadingText.Text;
            set => HeadingText.Text = value;
        }

        private void HeadingText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var player = new SoundPlayer("Resources/phasor_zap.wav");
                player.Play();
            }
            catch { }
        }
    }
}
