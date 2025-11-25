using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace DJBookingSystem.Views
{
    public partial class BanCountdownWindow : Window
    {
        private DateTime _banExpiry;
        private DispatcherTimer _timer;
        private int _strikeCount;
        private string _banReason;

        public BanCountdownWindow(DateTime banExpiry, int strikeCount, string banReason)
        {
            InitializeComponent();
            _banExpiry = banExpiry;
            _strikeCount = strikeCount;
            _banReason = banReason ?? "Policy violation";

            // Update strike display
            StrikeText.Text = $"Strike {strikeCount} of 3";
            ReasonText.Text = _banReason;

            // Update warning based on strike
            if (strikeCount == 1)
            {
                WarningText.Text = "? Warning: Next ban will be 48 hours. Third strike is permanent.";
            }
            else if (strikeCount == 2)
            {
                WarningText.Text = "? FINAL WARNING: Next ban will be PERMANENT. Contact admin if you believe this is a mistake.";
            }
            else if (strikeCount >= 3)
            {
                WarningText.Text = "?? PERMANENT BAN: You must contact an administrator to appeal.";
                WarningText.Foreground = System.Windows.Media.Brushes.Red;
            }

            // Start countdown timer
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Initial update
            UpdateCountdown();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateCountdown();
        }

        private void UpdateCountdown()
        {
            TimeSpan remaining = _banExpiry - DateTime.Now;

            if (remaining.TotalSeconds <= 0)
            {
                // Ban expired!
                _timer.Stop();
                MessageBox.Show(
                    "Your ban has expired. You may now log in.",
                    "Ban Lifted",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Close();
                return;
            }

            // Update flip-clock displays
            int days = remaining.Days;
            int hours = remaining.Hours;
            int minutes = remaining.Minutes;
            int seconds = remaining.Seconds;

            DaysText.Text = days.ToString("D2");
            HoursText.Text = hours.ToString("D2");
            MinutesText.Text = minutes.ToString("D2");
            SecondsText.Text = seconds.ToString("D2");

            // Animate flip effect on seconds change
            if (seconds != _lastSecond)
            {
                AnimateFlip(SecondsBorder);
                _lastSecond = seconds;
            }
        }

        private int _lastSecond = -1;

        private void AnimateFlip(System.Windows.Controls.Border border)
        {
            // Simple opacity animation for flip effect
            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 1.0,
                To = 0.7,
                Duration = TimeSpan.FromMilliseconds(100),
                AutoReverse = true
            };
            border.BeginAnimation(OpacityProperty, animation);
        }

        private void Support_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "To get help with this ban:\n\n" +
                "Click the '?? APPEAL BAN' button to submit an appeal.\n\n" +
                "Your appeal will be reviewed by a SysAdmin within 24-48 hours.\n" +
                "You will receive a response via the app.\n\n" +
                $"Ban expires: {_banExpiry:MMM dd, yyyy HH:mm}\n" +
                $"Strike {_strikeCount} of 3",
                "Support",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void AppealBan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create anonymous ban appeal form
                var appealDialog = new Window
                {
                    Title = "Ban Appeal Submission",
                    Width = 600,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(10, 10, 10)),
                    ResizeMode = ResizeMode.NoResize
                };

                var mainGrid = new System.Windows.Controls.Grid { Margin = new Thickness(20) };
                mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

                // Header
                var header = new System.Windows.Controls.StackPanel { Margin = new Thickness(0, 0, 0, 20) };
                header.Children.Add(new System.Windows.Controls.TextBlock
                {
                    Text = "?? SUBMIT BAN APPEAL",
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.Orange,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 10)
                });
                header.Children.Add(new System.Windows.Controls.TextBlock
                {
                    Text = "Your appeal will be reviewed by a SysAdmin.\nYou will see the response when you log in after your ban expires.",
                    FontSize = 12,
                    Foreground = System.Windows.Media.Brushes.LightGray,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                });
                System.Windows.Controls.Grid.SetRow(header, 0);
                mainGrid.Children.Add(header);

                // Form
                var formStack = new System.Windows.Controls.StackPanel();
                
                // Username display (read-only)
                formStack.Children.Add(new System.Windows.Controls.TextBlock { Text = "Your Username:", Foreground = System.Windows.Media.Brushes.White, Margin = new Thickness(0, 0, 0, 5) });
                var usernameBox = new System.Windows.Controls.TextBox 
                { 
                    Height = 30, 
                    Margin = new Thickness(0, 0, 0, 15),
                    FontSize = 13,
                    IsReadOnly = true,
                    Background = System.Windows.Media.Brushes.DarkGray,
                    Text = Environment.UserName  // Windows username as fallback
                };
                formStack.Children.Add(usernameBox);

                // Appeal reason
                formStack.Children.Add(new System.Windows.Controls.TextBlock { Text = "Why should this ban be lifted?", Foreground = System.Windows.Media.Brushes.White, Margin = new Thickness(0, 0, 0, 5) });
                var reasonBox = new System.Windows.Controls.TextBox
                {
                    Height = 150,
                    TextWrapping = TextWrapping.Wrap,
                    AcceptsReturn = true,
                    VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                    Margin = new Thickness(0, 0, 0, 15),
                    FontSize = 13
                };
                formStack.Children.Add(reasonBox);

                // Ban info display
                var banInfo = new System.Windows.Controls.TextBlock
                {
                    Text = $"Ban Reason: {_banReason}\nStrike: {_strikeCount} of 3\nExpires: {_banExpiry:MMM dd, yyyy HH:mm}",
                    Foreground = System.Windows.Media.Brushes.Gray,
                    FontSize = 11,
                    Margin = new Thickness(0, 0, 0, 15),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas")
                };
                formStack.Children.Add(banInfo);

                System.Windows.Controls.Grid.SetRow(formStack, 1);
                mainGrid.Children.Add(formStack);

                // Buttons
                var buttonStack = new System.Windows.Controls.StackPanel 
                { 
                    Orientation = Orientation.Horizontal, 
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                
                var submitBtn = new System.Windows.Controls.Button
                {
                    Content = "? SUBMIT APPEAL",
                    Width = 160,
                    Height = 40,
                    Background = System.Windows.Media.Brushes.Green,
                    Foreground = System.Windows.Media.Brushes.White,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 10, 0),
                    Cursor = Cursors.Hand
                };
                submitBtn.Click += (s, args) =>
                {
                    if (string.IsNullOrWhiteSpace(reasonBox.Text) || reasonBox.Text.Length < 20)
                    {
                        MessageBox.Show("Please provide a detailed explanation (minimum 20 characters).", "Reason Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // TODO: Save appeal to database (app-only, no email)
                    MessageBox.Show(
                        "Your ban appeal has been submitted.\n\n" +
                        "A SysAdmin will review your appeal within 24-48 hours.\n\n" +
                        "The response will be visible in the app after your ban expires.\n" +
                        "Log in after the ban expires to see the admin's decision.",
                        "Appeal Submitted",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    
                    appealDialog.Close();
                };
                
                var cancelBtn = new System.Windows.Controls.Button
                {
                    Content = "? CANCEL",
                    Width = 120,
                    Height = 40,
                    Background = System.Windows.Media.Brushes.DarkRed,
                    Foreground = System.Windows.Media.Brushes.White,
                    FontWeight = FontWeights.Bold,
                    Cursor = Cursors.Hand
                };
                cancelBtn.Click += (s, args) => appealDialog.Close();

                buttonStack.Children.Add(submitBtn);
                buttonStack.Children.Add(cancelBtn);
                System.Windows.Controls.Grid.SetRow(buttonStack, 2);
                mainGrid.Children.Add(buttonStack);

                appealDialog.Content = mainGrid;
                appealDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening appeal form: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _timer?.Stop();
            base.OnClosing(e);
        }
    }
}
