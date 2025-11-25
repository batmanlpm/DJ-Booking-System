using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DJBookingSystem.Controls
{
    public partial class ConnectionIndicator : UserControl
    {
        private Storyboard? _connectingAnimation;
        private Storyboard? _blinkAnimation;
        private Storyboard? _pulseAnimation;
        private System.Threading.Timer? _statusCheckTimer;

        public bool IsConnectedToDatabase { get; private set; }
        public bool IsConnectedToRadio { get; private set; }

        public ConnectionIndicator()
        {
            InitializeComponent();
            StartConnectingAnimation();
        }

        /// <summary>
        /// 🎪 START BOUNCING & SPINNING - Called while connecting
        /// </summary>
        private void StartConnectingAnimation()
        {
            _connectingAnimation = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

            // Bouncing animation (up and down)
            var bounceY = new DoubleAnimation
            {
                From = 0,
                To = -15,
                Duration = TimeSpan.FromMilliseconds(500),
                AutoReverse = true,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            var translateTransform = new TranslateTransform();
            ClaudeCanvas.RenderTransform = translateTransform;
            Storyboard.SetTarget(bounceY, ClaudeCanvas);
            Storyboard.SetTargetProperty(bounceY, new PropertyPath("RenderTransform.Y"));
            _connectingAnimation.Children.Add(bounceY);

            // Spinning animation (rotate)
            var spin = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromMilliseconds(2000),
                RepeatBehavior = RepeatBehavior.Forever
            };

            var rotateTransform = new RotateTransform();
            ClaudeBody.RenderTransform = rotateTransform;
            ClaudeBody.RenderTransformOrigin = new Point(0.5, 0.5);
            Storyboard.SetTarget(spin, ClaudeBody);
            Storyboard.SetTargetProperty(spin, new PropertyPath("RenderTransform.Angle"));
            _connectingAnimation.Children.Add(spin);

            // Wave animations
            var wave1Anim = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(600),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTarget(wave1Anim, Wave1);
            Storyboard.SetTargetProperty(wave1Anim, new PropertyPath("Opacity"));
            _connectingAnimation.Children.Add(wave1Anim);

            var wave2Anim = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(600),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = TimeSpan.FromMilliseconds(300)
            };
            Storyboard.SetTarget(wave2Anim, Wave2);
            Storyboard.SetTargetProperty(wave2Anim, new PropertyPath("Opacity"));
            _connectingAnimation.Children.Add(wave2Anim);

            _connectingAnimation.Begin();
        }

        /// <summary>
        /// 💚 STOP BOUNCING - Called when connected, sits still
        /// </summary>
        public void SetConnected(bool databaseConnected, bool radioConnected = false)
        {
            Dispatcher.Invoke(() =>
            {
                IsConnectedToDatabase = databaseConnected;
                IsConnectedToRadio = radioConnected;

                // Stop bouncing and spinning
                _connectingAnimation?.Stop();
                
                // Reset transforms
                ClaudeCanvas.RenderTransform = new TranslateTransform(0, 0);
                ClaudeBody.RenderTransform = new RotateTransform(0);

                // Hide waves
                Wave1.Opacity = 0;
                Wave2.Opacity = 0;

                // Update colors based on connection
                Color statusColor = databaseConnected ? 
                    (Color)FindResource("AccentColor") : 
                    Color.FromRgb(255, 0, 85); // Red/Pink for disconnected

                BodyStroke.Color = statusColor;
                BodyGlow.Color = statusColor;
                LeftEyeFill.Color = statusColor;
                RightEyeFill.Color = statusColor;
                MouthStroke.Color = statusColor;
                AntennaStroke.Color = statusColor;
                SignalDotFill.Color = statusColor;
                SignalGlow.Color = statusColor;

                // Update mouth (smile vs frown)
                if (databaseConnected)
                {
                    // Happy smile
                    MouthPath.Data = Geometry.Parse("M 18 28 Q 25 32 32 28");
                }
                else
                {
                    // Sad frown
                    MouthPath.Data = Geometry.Parse("M 18 32 Q 25 28 32 32");
                }

                // Update tooltip
                DatabaseStatus.Text = databaseConnected ? 
                    "📊 Database: ✅ Connected" : 
                    "📊 Database: ❌ Disconnected";
                DatabaseStatus.Foreground = new SolidColorBrush(statusColor);

                RadioStatus.Text = radioConnected ?
                    "📻 Radio: ✅ Connected" :
                    "📻 Radio: ⚠️ Not Connected";
                RadioStatus.Foreground = new SolidColorBrush(
                    radioConnected ? statusColor : Color.FromRgb(136, 136, 136));

                // Start gentle animations when connected
                if (databaseConnected)
                {
                    StartConnectedAnimations();
                }
            });
        }

        /// <summary>
        /// 💫 Gentle animations when sitting still and connected
        /// </summary>
        private void StartConnectedAnimations()
        {
            // Gentle eye blink
            _blinkAnimation = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };
            
            var blink1 = new DoubleAnimation
            {
                From = 1.0,
                To = 0.2,
                Duration = TimeSpan.FromMilliseconds(100),
                AutoReverse = true,
                BeginTime = TimeSpan.FromSeconds(3)
            };
            Storyboard.SetTarget(blink1, LeftEye);
            Storyboard.SetTargetProperty(blink1, new PropertyPath("Opacity"));
            _blinkAnimation.Children.Add(blink1);

            var blink2 = new DoubleAnimation
            {
                From = 1.0,
                To = 0.2,
                Duration = TimeSpan.FromMilliseconds(100),
                AutoReverse = true,
                BeginTime = TimeSpan.FromSeconds(3)
            };
            Storyboard.SetTarget(blink2, RightEye);
            Storyboard.SetTargetProperty(blink2, new PropertyPath("Opacity"));
            _blinkAnimation.Children.Add(blink2);

            _blinkAnimation.Begin();

            // Gentle pulse on signal dot
            _pulseAnimation = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };
            
            var pulse = new DoubleAnimation
            {
                From = 0.5,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(1500),
                AutoReverse = true
            };
            Storyboard.SetTarget(pulse, SignalGlow);
            Storyboard.SetTargetProperty(pulse, new PropertyPath("Opacity"));
            _pulseAnimation.Children.Add(pulse);

            _pulseAnimation.Begin();
        }

        /// <summary>
        /// 🔄 Start monitoring connection status
        /// </summary>
        public void StartMonitoring(Func<Task<bool>> checkDatabaseConnection, 
                                   Func<Task<bool>>? checkRadioConnection = null)
        {
            _statusCheckTimer = new System.Threading.Timer(async _ =>
            {
                try
                {
                    bool dbConnected = await checkDatabaseConnection();
                    bool radioConnected = false;
                    
                    if (checkRadioConnection != null)
                    {
                        radioConnected = await checkRadioConnection();
                    }

                    SetConnected(dbConnected, radioConnected);
                }
                catch
                {
                    SetConnected(false, false);
                }
            }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30)); // Check every 30 seconds
        }

        /// <summary>
        /// 🛑 Stop monitoring
        /// </summary>
        public void StopMonitoring()
        {
            _statusCheckTimer?.Dispose();
            _connectingAnimation?.Stop();
            _blinkAnimation?.Stop();
            _pulseAnimation?.Stop();
        }
    }
}
