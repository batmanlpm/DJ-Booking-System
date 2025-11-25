using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DJBookingSystem.Controls
{
    /// <summary>
    /// Claude Contributor Indicator - Shows connection status for various services
    /// Hangs from top with blinking eyes and amber polling flash
    /// </summary>
    public partial class ClaudeIndicator : UserControl
    {
        public static readonly DependencyProperty ServiceNameProperty =
            DependencyProperty.Register("ServiceName", typeof(string), typeof(ClaudeIndicator),
                new PropertyMetadata("Service"));

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(ClaudeIndicator),
                new PropertyMetadata("Service"));

        public static readonly DependencyProperty IsConnectedProperty =
            DependencyProperty.Register("IsConnected", typeof(bool), typeof(ClaudeIndicator),
                new PropertyMetadata(false, OnIsConnectedChanged));

        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register("StatusText", typeof(string), typeof(ClaudeIndicator),
                new PropertyMetadata("Connecting..."));

        public static readonly DependencyProperty StatusColorProperty =
            DependencyProperty.Register("StatusColor", typeof(Brush), typeof(ClaudeIndicator),
                new PropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public static readonly DependencyProperty StatusColorValueProperty =
            DependencyProperty.Register("StatusColorValue", typeof(Color), typeof(ClaudeIndicator),
                new PropertyMetadata(Colors.Yellow));

        private DispatcherTimer? _pollingTimer;
        private readonly Random _random = new Random();
        private Color _currentStatusColor;

        public string ServiceName
        {
            get => (string)GetValue(ServiceNameProperty);
            set => SetValue(ServiceNameProperty, value);
        }

        public string LabelText
        {
            get => (string)GetValue(LabelTextProperty);
            set => SetValue(LabelTextProperty, value);
        }

        public bool IsConnected
        {
            get => (bool)GetValue(IsConnectedProperty);
            set => SetValue(IsConnectedProperty, value);
        }

        public string StatusText
        {
            get => (string)GetValue(StatusTextProperty);
            set => SetValue(StatusTextProperty, value);
        }

        public Brush StatusColor
        {
            get => (Brush)GetValue(StatusColorProperty);
            set => SetValue(StatusColorProperty, value);
        }

        public Color StatusColorValue
        {
            get => (Color)GetValue(StatusColorValueProperty);
            set => SetValue(StatusColorValueProperty, value);
        }

        public ClaudeIndicator()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Loaded += ClaudeIndicator_Loaded;
        }

        private void ClaudeIndicator_Loaded(object sender, RoutedEventArgs e)
        {
            // Start polling timer with random offset (0-10 seconds)
            StartPollingTimer();
        }

        private void StartPollingTimer()
        {
            // Random initial delay (0-10 seconds) to stagger the flashes
            int randomDelay = _random.Next(0, 10000);
            
            var initialTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(randomDelay)
            };
            
            initialTimer.Tick += (s, e) =>
            {
                initialTimer.Stop();
                
                // Now start the regular 10-second polling
                _pollingTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(10)
                };
                
                _pollingTimer.Tick += (sender, args) =>
                {
                    // Flash amber to indicate polling check
                    FlashAmber();
                };
                
                _pollingTimer.Start();
                
                // Do initial flash
                FlashAmber();
            };
            
            initialTimer.Start();
        }

        /// <summary>
        /// Flashes amber to indicate a polling check is happening
        /// </summary>
        public void FlashAmber()
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    var storyboard = this.Resources["AmberFlashAnimation"] as Storyboard;
                    if (storyboard != null)
                    {
                        // Store current color
                        _currentStatusColor = StatusColorValue;
                        
                        // Set target colors back to current color after flash
                        foreach (var animation in storyboard.Children)
                        {
                            if (animation is ColorAnimation colorAnim && colorAnim.BeginTime == TimeSpan.FromSeconds(0.3))
                            {
                                colorAnim.To = _currentStatusColor;
                            }
                        }
                        
                        storyboard.Begin();
                        
                        System.Diagnostics.Debug.WriteLine($"?? {ServiceName}: Amber flash - checking connection...");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error flashing amber for {ServiceName}: {ex.Message}");
                }
            });
        }

        private static void OnIsConnectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ClaudeIndicator indicator)
            {
                bool isConnected = (bool)e.NewValue;
                indicator.UpdateConnectionStatus(isConnected);
            }
        }

        private void UpdateConnectionStatus(bool isConnected)
        {
            Dispatcher.Invoke(() =>
            {
                if (isConnected)
                {
                    // Green for connected
                    var greenColor = Color.FromRgb(0, 255, 0);
                    StatusColor = new SolidColorBrush(greenColor);
                    StatusColorValue = greenColor;
                    _currentStatusColor = greenColor;
                    StatusText = "Connected ?";
                    
                    // Update smile to be happy
                    if (SmilePath != null)
                    {
                        SmilePath.Data = Geometry.Parse("M 13,28 Q 23,35 33,28"); // Smile
                    }
                    
                    // Normal glow speed
                    var storyboard = (Storyboard)this.Resources["GlowAnimation"];
                    storyboard.SpeedRatio = 1.0;
                }
                else
                {
                    // Red for disconnected
                    var redColor = Color.FromRgb(255, 0, 0);
                    StatusColor = new SolidColorBrush(redColor);
                    StatusColorValue = redColor;
                    _currentStatusColor = redColor;
                    StatusText = "Disconnected ?";
                    
                    // Update smile to be sad (frown)
                    if (SmilePath != null)
                    {
                        SmilePath.Data = Geometry.Parse("M 13,32 Q 23,26 33,32"); // Frown
                    }
                    
                    // Slower glow speed
                    var storyboard = (Storyboard)this.Resources["GlowAnimation"];
                    storyboard.SpeedRatio = 0.5;
                }
                
                System.Diagnostics.Debug.WriteLine($"?? {ServiceName}: Status updated - {(isConnected ? "Connected (Green)" : "Disconnected (Red)")}");
            });
        }

        public void SetConnecting()
        {
            Dispatcher.Invoke(() =>
            {
                var yellowColor = Color.FromRgb(255, 255, 0);
                StatusColor = new SolidColorBrush(yellowColor);
                StatusColorValue = yellowColor;
                _currentStatusColor = yellowColor;
                StatusText = "Connecting...";
                
                // Faster glow speed while connecting
                var storyboard = (Storyboard)this.Resources["GlowAnimation"];
                storyboard.SpeedRatio = 2.0;
            });
        }

        public void Cleanup()
        {
            _pollingTimer?.Stop();
            _pollingTimer = null;
        }
    }
}
