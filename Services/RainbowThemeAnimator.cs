using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Rainbow Theme Animator - Creates constantly cycling gradient colors
    /// </summary>
    public class RainbowThemeAnimator
    {
        private static DispatcherTimer? _animationTimer = null;
        private static double _hueOffset = 0;
        private static bool _isAnimating = false;

        /// <summary>
        /// Start the rainbow animation
        /// </summary>
        public static void StartAnimation()
        {
            if (_isAnimating) return;

            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50) // Update every 50ms for smooth animation
            };

            _animationTimer.Tick += AnimationTimer_Tick;
            _animationTimer.Start();
            _isAnimating = true;

            Console.WriteLine("?? Rainbow theme animation started!");
        }

        /// <summary>
        /// Stop the rainbow animation
        /// </summary>
        public static void StopAnimation()
        {
            if (_animationTimer != null)
            {
                _animationTimer.Stop();
                _animationTimer.Tick -= AnimationTimer_Tick;
                _animationTimer = null;
            }

            _isAnimating = false;
            _hueOffset = 0;

            Console.WriteLine("?? Rainbow theme animation stopped.");
        }

        private static void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            // Increment hue offset for continuous color cycling
            _hueOffset += 2; // Speed of color change
            if (_hueOffset >= 360)
                _hueOffset = 0;

            // Update the accent color in application resources
            try
            {
                var app = Application.Current;
                if (app != null)
                {
                    // Calculate current rainbow color
                    Color rainbowColor = HsvToRgb(_hueOffset, 1.0, 1.0);

                    // Update AccentColor
                    if (app.Resources.Contains("AccentColor"))
                    {
                        app.Resources["AccentColor"] = rainbowColor;
                    }

                    // Update AccentBrush
                    if (app.Resources.Contains("AccentBrush"))
                    {
                        var brush = app.Resources["AccentBrush"] as SolidColorBrush;
                        if (brush != null)
                        {
                            // Animate the color change smoothly
                            var colorAnimation = new ColorAnimation
                            {
                                To = rainbowColor,
                                Duration = TimeSpan.FromMilliseconds(100),
                                EasingFunction = new QuadraticEase()
                            };

                            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Rainbow animation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Convert HSV to RGB color
        /// </summary>
        private static Color HsvToRgb(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            return hi switch
            {
                0 => Color.FromRgb(v, t, p),
                1 => Color.FromRgb(q, v, p),
                2 => Color.FromRgb(p, v, t),
                3 => Color.FromRgb(p, q, v),
                4 => Color.FromRgb(t, p, v),
                _ => Color.FromRgb(v, p, q)
            };
        }

        /// <summary>
        /// Check if animation is running
        /// </summary>
        public static bool IsAnimating => _isAnimating;

        /// <summary>
        /// Get current rainbow color at specific position (0-360 degrees)
        /// </summary>
        public static Color GetRainbowColor(double position)
        {
            return HsvToRgb(position, 1.0, 1.0);
        }

        /// <summary>
        /// Set animation speed (1.0 = normal, 2.0 = twice as fast, 0.5 = half speed)
        /// </summary>
        public static void SetAnimationSpeed(double speedMultiplier)
        {
            if (_animationTimer != null)
            {
                _animationTimer.Interval = TimeSpan.FromMilliseconds(50 / speedMultiplier);
            }
        }
    }
}
