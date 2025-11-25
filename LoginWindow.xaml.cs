using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class LoginWindow : Window
    {
        private CosmosDbService? _cosmosService;
        private AuthenticationService _authService;
        private bool _isLoggingIn = false;

        public User? LoggedInUser { get; private set; }

        public LoginWindow(CosmosDbService cosmosService)
        {
            System.Diagnostics.Debug.WriteLine("LoginWindow constructor - START");
            
            try
            {
                InitializeComponent();
                System.Diagnostics.Debug.WriteLine("LoginWindow InitializeComponent - SUCCESS");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoginWindow InitializeComponent - FAILED: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                throw;
            }
            
            _cosmosService = cosmosService;
            _authService = AuthenticationService.Instance;
            _authService.Initialize(cosmosService);

            System.Diagnostics.Debug.WriteLine("LoginWindow constructor started");

            // Focus username on load
            Loaded += (s, e) => UsernameTextBox.Focus();

            // Enable Enter key to login
            PasswordBox.KeyDown += PasswordBox_KeyDown;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !_isLoggingIn)
            {
                Login_Click(sender, e);
            }
        }

        private void UsernameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Clear error message when user starts typing
            if (ErrorMessageTextBlock != null && ErrorMessageTextBlock.Visibility == Visibility.Visible)
            {
                ErrorMessageTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Clear error message when user starts typing
            if (ErrorMessageTextBlock != null && ErrorMessageTextBlock.Visibility == Visibility.Visible)
            {
                ErrorMessageTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoggingIn) return;

            _isLoggingIn = true;
            ErrorMessageTextBlock.Visibility = Visibility.Collapsed;
            LoginButton.IsEnabled = false;

            try
            {
                // CHECK MACHINE-BOUND BAN FIRST (prevents VPN bypass)
                var machineBan = Services.MachineBanService.CheckLocalBan();
                if (machineBan != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[Login] Machine ban detected for {machineBan.Username}");

                    if (machineBan.IsPermanent)
                    {
                        // PERMANENT BAN
                        MessageBox.Show(
                            $"This computer is permanently banned.\n\n" +
                            $"Account: {machineBan.Username}\n" +
                            $"Reason: {machineBan.BanReason}\n\n" +
                            $"Strike 3 of 3 - PERMANENT BAN\n\n" +
                            "? THIS BAN IS TIED TO: ?\n" +
                            "• Your computer hardware (Machine ID)\n" +
                            "• Your IP address / Network (WAN)\n\n" +
                            "VPN, proxy, or IP changes will NOT bypass this ban.\n" +
                            "Using another computer on the same network will NOT work.\n\n" +
                            "Contact a SysAdmin to appeal.",
                            "? DUAL-LAYER BAN ACTIVE",
                            MessageBoxButton.OK,
                            MessageBoxImage.Stop);
                        LoginButton.Content = "Login";
                        return;
                    }
                    else if (machineBan.BanExpiry.HasValue)
                    {
                        // TEMPORARY BAN - Show countdown
                        var countdownWindow = new Views.BanCountdownWindow(
                            machineBan.BanExpiry.Value,
                            machineBan.StrikeCount,
                            machineBan.BanReason);
                        
                        countdownWindow.ShowDialog();
                        LoginButton.Content = "Login";
                        return;
                    }
                }

                string username = UsernameTextBox.Text.Trim();
                string password = PasswordBox.Password;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    ShowError("Please enter username and password");
                    return;
                }

                // Get IP address
                string ipAddress = AuthenticationService.GetLocalIPAddress();

                // CHECK FOR IP BAN (SECOND LAYER - blocks other computers on same network)
                if (_cosmosService != null)
                {
                    var bannedUser = await _cosmosService.GetBannedUserByIPAsync(ipAddress);
                    if (bannedUser != null && bannedUser.IsBanned)
                    {
                        // Check if ban has expired
                        if (bannedUser.BanExpiry.HasValue && bannedUser.BanExpiry.Value < DateTime.Now)
                        {
                            // Ban expired - allow login
                            System.Diagnostics.Debug.WriteLine($"[Login] IP ban expired for {ipAddress}");
                        }
                        else if (bannedUser.IsPermanentBan || !bannedUser.BanExpiry.HasValue)
                        {
                            // PERMANENT IP BAN
                            MessageBox.Show(
                                $"This IP address / Network is permanently banned.\n\n" +
                                $"Banned User: {bannedUser.Username}\n" +
                                $"Your IP: {ipAddress}\n" +
                                $"Reason: {bannedUser.BanReason ?? "Policy violation"}\n\n" +
                                $"Strike 3 of 3 - PERMANENT BAN\n\n" +
                                "? DUAL-LAYER BAN ACTIVE ?\n" +
                                "• IP/Network banned (blocks all devices on your network)\n" +
                                "• Machine ID banned (blocks this specific computer)\n\n" +
                                "Switching computers or using VPN will NOT work.\n\n" +
                                "Contact a SysAdmin to appeal.",
                                "? IP + NETWORK BANNED",
                                MessageBoxButton.OK,
                                MessageBoxImage.Stop);
                            LoginButton.Content = "Login";
                            return;
                        }
                        else
                        {
                            // TEMPORARY IP BAN - Show countdown
                            var countdownWindow = new Views.BanCountdownWindow(
                                bannedUser.BanExpiry.Value,
                                bannedUser.BanStrikeCount,
                                bannedUser.BanReason ?? "Policy violation");
                            
                            countdownWindow.ShowDialog();
                            LoginButton.Content = "Login";
                            return;
                        }
                    }
                }

                // Show loading state
                LoginButton.Content = "Logging in...";

                // Attempt login
                var result = await _authService.LoginAsync(username, password, ipAddress);

                if (result.Success)
                {
                    LoginButton.Content = "Success!";
                    LoggedInUser = result.User;

                    // Save remember me and auto-login preferences
                    bool rememberMe = RememberMeCheckBox.IsChecked == true;
                    bool autoLogin = AutoLoginCheckBox.IsChecked == true;

                    if (rememberMe)
                    {
                        // Save to local storage
                        LocalStorage.SaveLoginInfo(username, rememberMe, autoLogin);

                        // Save auto-login preference to database
                        if (result.User != null && _cosmosService != null)
                        {
                            result.User.AppPreferences.RememberMe = rememberMe;
                            result.User.AppPreferences.AutoLogin = autoLogin;
                            await _cosmosService.UpdateUserAsync(result.User);
                            System.Diagnostics.Debug.WriteLine($"[Login] Saved preferences - RememberMe: {rememberMe}, AutoLogin: {autoLogin}");
                        }
                    }
                    else
                    {
                        // Clear saved login if remember me is unchecked
                        LocalStorage.ClearLoginInfo();
                        
                        if (result.User != null && _cosmosService != null)
                        {
                            result.User.AppPreferences.RememberMe = false;
                            result.User.AppPreferences.AutoLogin = false;
                            await _cosmosService.UpdateUserAsync(result.User);
                        }
                    }

                    // Close login window and continue to main app
                    await Task.Delay(500); // Brief pause to show success message
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowError(result.Message);
                    LoginButton.Content = "Login";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Login error: {ex.Message}");
                LoginButton.Content = "Login";
                System.Diagnostics.Debug.WriteLine($"[LoginWindow] Login error: {ex.Message}");
            }
            finally
            {
                _isLoggingIn = false;
                LoginButton.IsEnabled = true;
            }
        }

        private void ShowError(string message)
        {
            ErrorMessageTextBlock.Text = message;
            ErrorMessageTextBlock.Visibility = Visibility.Visible;
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open proper registration window
                var registrationWindow = new RegistrationWindow(_cosmosService);
                registrationWindow.Owner = this;
                
                if (registrationWindow.ShowDialog() == true && registrationWindow.RegisteredUser != null)
                {
                    // Set LoggedInUser so App.xaml.cs can auto-login
                    LoggedInUser = registrationWindow.RegisteredUser;
                    
                    System.Diagnostics.Debug.WriteLine($"? Registration complete - Auto-login user: {LoggedInUser.Username}");
                    System.Diagnostics.Debug.WriteLine($"? HasSeenTutorial = {LoggedInUser.AppPreferences.HasSeenTutorial} (should be false for new user)");
                    
                    // Close login window - App.xaml.cs will handle the rest and show tutorial!
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registration error: {ex.Message}");
                MessageBox.Show(
                    "Unable to complete registration at this time.\n\n" +
                    "Please try again later.\n\n" +
                    "If the problem persists, use the Support button after logging in.",
                    "Registration Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void ContactSupportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open anonymous support ticket form (no login required)
                var supportDialog = new Window
                {
                    Title = "Create Support Ticket",
                    Width = 650,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
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
                    Text = "?? CREATE SUPPORT TICKET",
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.LimeGreen,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 10)
                });
                header.Children.Add(new System.Windows.Controls.TextBlock
                {
                    Text = "Having trouble logging in? Get help from an administrator.\nYour ticket will be reviewed within 24 hours.",
                    FontSize = 12,
                    Foreground = System.Windows.Media.Brushes.LightGray,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                });
                System.Windows.Controls.Grid.SetRow(header, 0);
                mainGrid.Children.Add(header);

                // Form
                var formStack = new System.Windows.Controls.StackPanel();
                
                // Username field
                formStack.Children.Add(new System.Windows.Controls.TextBlock 
                { 
                    Text = "Your Username (or Computer Name if you can't remember):", 
                    Foreground = System.Windows.Media.Brushes.White, 
                    Margin = new Thickness(0, 0, 0, 5),
                    FontSize = 12
                });
                var usernameBox = new System.Windows.Controls.TextBox 
                { 
                    Height = 30, 
                    Margin = new Thickness(0, 0, 0, 15),
                    FontSize = 13,
                    Background = System.Windows.Media.Brushes.Black,
                    Foreground = System.Windows.Media.Brushes.White,
                    BorderBrush = System.Windows.Media.Brushes.LimeGreen,
                    BorderThickness = new Thickness(2),
                    Padding = new Thickness(5)
                };
                formStack.Children.Add(usernameBox);

                // Issue type dropdown
                formStack.Children.Add(new System.Windows.Controls.TextBlock 
                { 
                    Text = "Issue Type:", 
                    Foreground = System.Windows.Media.Brushes.White, 
                    Margin = new Thickness(0, 0, 0, 5),
                    FontSize = 12
                });
                var issueTypeBox = new System.Windows.Controls.ComboBox
                {
                    Height = 30,
                    Margin = new Thickness(0, 0, 0, 15),
                    FontSize = 13,
                    Background = System.Windows.Media.Brushes.Black,
                    Foreground = System.Windows.Media.Brushes.White,
                    BorderBrush = System.Windows.Media.Brushes.LimeGreen,
                    BorderThickness = new Thickness(2)
                };
                issueTypeBox.Items.Add("Can't Login / Forgot Password");
                issueTypeBox.Items.Add("Account Locked / Banned");
                issueTypeBox.Items.Add("Registration Problem");
                issueTypeBox.Items.Add("Technical Issue");
                issueTypeBox.Items.Add("Other");
                issueTypeBox.SelectedIndex = 0;
                formStack.Children.Add(issueTypeBox);

                // Issue description
                formStack.Children.Add(new System.Windows.Controls.TextBlock 
                { 
                    Text = "Describe Your Issue:", 
                    Foreground = System.Windows.Media.Brushes.White, 
                    Margin = new Thickness(0, 0, 0, 5),
                    FontSize = 12
                });
                var descriptionBox = new System.Windows.Controls.TextBox
                {
                    Height = 200,
                    TextWrapping = TextWrapping.Wrap,
                    AcceptsReturn = true,
                    VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                    Margin = new Thickness(0, 0, 0, 10),
                    FontSize = 13,
                    Background = System.Windows.Media.Brushes.Black,
                    Foreground = System.Windows.Media.Brushes.White,
                    BorderBrush = System.Windows.Media.Brushes.LimeGreen,
                    BorderThickness = new Thickness(2),
                    Padding = new Thickness(5)
                };
                formStack.Children.Add(descriptionBox);

                // Info note
                var note = new System.Windows.Controls.TextBlock
                {
                    Text = "?? Note: Your ticket will be PRIVATE. Only you and administrators can see it.",
                    Foreground = System.Windows.Media.Brushes.Cyan,
                    FontSize = 11,
                    Margin = new Thickness(0, 0, 0, 10),
                    TextWrapping = TextWrapping.Wrap,
                    FontStyle = FontStyles.Italic
                };
                formStack.Children.Add(note);

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
                    Content = "? SUBMIT TICKET",
                    Width = 160,
                    Height = 40,
                    Background = System.Windows.Media.Brushes.Green,
                    Foreground = System.Windows.Media.Brushes.White,
                    FontWeight = FontWeights.Bold,
                    FontSize = 13,
                    Margin = new Thickness(0, 0, 10, 0),
                    Cursor = Cursors.Hand
                };
                submitBtn.Click += (s, args) =>
                {
                    if (string.IsNullOrWhiteSpace(usernameBox.Text) || usernameBox.Text.Length < 3)
                    {
                        MessageBox.Show(
                            "Please enter your username or computer name (minimum 3 characters).",
                            "Username Required",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                    
                    if (string.IsNullOrWhiteSpace(descriptionBox.Text) || descriptionBox.Text.Length < 20)
                    {
                        MessageBox.Show(
                            "Please provide a detailed description of your issue (minimum 20 characters).",
                            "Description Required",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    // TODO: Save ticket to database
                    // For now, show confirmation
                    MessageBox.Show(
                        $"? Support Ticket Submitted!\n\n" +
                        $"Username: {usernameBox.Text}\n" +
                        $"Issue: {issueTypeBox.SelectedItem}\n\n" +
                        "Your ticket has been created and assigned to an administrator.\n\n" +
                        "WHAT HAPPENS NEXT:\n" +
                        "• An admin will review your ticket within 24 hours\n" +
                        "• They will investigate your issue\n" +
                        "• If resolved, you'll be able to log in\n" +
                        "• You can check ticket status in the app after logging in\n\n" +
                        "Thank you for your patience!",
                        "Ticket Submitted Successfully",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    
                    supportDialog.Close();
                };
                
                var cancelBtn = new System.Windows.Controls.Button
                {
                    Content = "? CANCEL",
                    Width = 120,
                    Height = 40,
                    Background = System.Windows.Media.Brushes.DarkRed,
                    Foreground = System.Windows.Media.Brushes.White,
                    FontWeight = FontWeights.Bold,
                    FontSize = 13,
                    Cursor = Cursors.Hand
                };
                cancelBtn.Click += (s, args) => supportDialog.Close();

                buttonStack.Children.Add(submitBtn);
                buttonStack.Children.Add(cancelBtn);
                System.Windows.Controls.Grid.SetRow(buttonStack, 2);
                mainGrid.Children.Add(buttonStack);

                supportDialog.Content = mainGrid;
                supportDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Support] Error opening support form: {ex.Message}");
                MessageBox.Show(
                    "Unable to open support form at this time.\n\n" +
                    "Please try again later.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void StayOnTopCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
        }

        private void StayOnTopCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Topmost = false;
        }

        private void RememberMeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Enable Auto Login checkbox when Remember Me is checked
            AutoLoginCheckBox.IsEnabled = true;
        }

        private void RememberMeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Disable and uncheck Auto Login when Remember Me is unchecked
            AutoLoginCheckBox.IsChecked = false;
            AutoLoginCheckBox.IsEnabled = false;
        }

        private void AutoLoginCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Auto login enabled
        }

        private void AutoLoginCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Auto login disabled
        }
    }
}
