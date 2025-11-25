using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DJBookingSystem.Models;
using DJBookingSystem.Services;
using System.Threading.Tasks;

namespace DJBookingSystem.Views
{
    public partial class SettingsView : UserControl
    {
        private readonly CandyBotUpdateService _updateService;
        private DateTime _lastCheckTime = DateTime.Now;
        private User? _currentUser;
        private CosmosDbService? _cosmosDbService;

        public SettingsView()
        {
            InitializeComponent();
            _updateService = new CandyBotUpdateService();
            LoadVersionInfo();
            UpdateLastCheckTime();
        }

        private void SettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[SettingsView] Loaded event fired");
        }

        public void Initialize(User currentUser, CosmosDbService cosmosDbService)
        {
            _currentUser = currentUser;
            _cosmosDbService = cosmosDbService;
            
            Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadUserColors();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
            
            if (_currentUser != null && (_currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager))
            {
                AdminToolsSection.Visibility = Visibility.Visible;
                DeleteAccountButton.Visibility = Visibility.Collapsed;
                AdminWarningText.Visibility = Visibility.Visible;
            }
            else
            {
                AdminToolsSection.Visibility = Visibility.Collapsed;
                DeleteAccountButton.Visibility = Visibility.Visible;
                AdminWarningText.Visibility = Visibility.Collapsed;
            }
        }

        public void Initialize(User currentUser)
        {
            Initialize(currentUser, null);
        }

        private void LoadVersionInfo()
        {
            try
            {
                ProductNameTextBlock.Text = VersionInfo.ProductName;
                VersionTextBlock.Text = $"Version {VersionInfo.VersionString}";
                DescriptionTextBlock.Text = VersionInfo.Description;
                CopyrightTextBlock.Text = VersionInfo.Copyright;
                ReleaseDateTextBlock.Text = $"Released: {VersionInfo.ReleaseDate:MMMM dd, yyyy} ({VersionInfo.DaysSinceRelease} days ago)";
                UpdateStatusTextBlock.Text = "? Automatic updates enabled (checks every 24 hours)";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading version info: {ex.Message}");
            }
        }

        private void UpdateLastCheckTime()
        {
            try
            {
                _lastCheckTime = DateTime.Now;
                LastCheckTextBlock.Text = $"Last checked: {_lastCheckTime:g}";
                NextCheckTextBlock.Text = $"Next automatic check: {_lastCheckTime.AddHours(24):g}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating check time: {ex.Message}");
            }
        }

        private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            try
            {
                button.IsEnabled = false;
                button.Content = "?? Checking for updates...";
                UpdateStatusTextBlock.Text = "?? Checking for updates...";
                UpdateStatusTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(255, 165, 0));

                var result = await _updateService.CheckForUpdatesAsync();
                UpdateLastCheckTime();
                
                if (result.UpdateAvailable)
                {
                    UpdateStatusTextBlock.Text = $"?? Update available: v{result.LatestVersion}";
                    UpdateStatusTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0));
                    var updateDialog = new UpdateNotificationDialog(result);
                    updateDialog.ShowDialog();
                }
                else
                {
                    UpdateStatusTextBlock.Text = $"? Up to date (v{VersionInfo.VersionString})";
                    UpdateStatusTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                    MessageBox.Show($"You're running the latest version!\n\nCurrent version: {VersionInfo.VersionString}", "Up to Date", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                UpdateStatusTextBlock.Text = "? Update check failed";
                UpdateStatusTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                MessageBox.Show($"Failed to check for updates.\n\nError: {ex.Message}", "Update Check Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (button != null)
                {
                    button.IsEnabled = true;
                    button.Content = "?? Check for Updates";
                }
            }
        }

        private void PickBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog
            {
                FullOpen = true,
                Color = System.Drawing.Color.Black
            };

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var color = Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                BackgroundPreview.Background = new SolidColorBrush(color);
                BackgroundHexText.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                BackgroundHexText.Foreground = (color.R * 0.299 + color.G * 0.587 + color.B * 0.114) > 128 ? Brushes.Black : Brushes.White;
                
                Application.Current.Resources["AppBackgroundColor"] = color;
                Application.Current.Resources["AppBackgroundBrush"] = new SolidColorBrush(color);
                
                System.Diagnostics.Debug.WriteLine($"? Background color changed to {BackgroundHexText.Text}");
            }
        }

        private void PickPrimaryColor_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog
            {
                FullOpen = true,
                Color = System.Drawing.Color.Lime
            };

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var color = Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                PrimaryPreview.Background = new SolidColorBrush(color);
                PrimaryHexText.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                PrimaryHexText.Foreground = (color.R * 0.299 + color.G * 0.587 + color.B * 0.114) > 128 ? Brushes.Black : Brushes.White;
                
                Application.Current.Resources["AppPrimaryColor"] = color;
                Application.Current.Resources["AppPrimaryBrush"] = new SolidColorBrush(color);
                
                System.Diagnostics.Debug.WriteLine($"? Primary color changed to {PrimaryHexText.Text}");
            }
        }

        private void ResetToDefault_Click(object sender, RoutedEventArgs e)
        {
            var bgColor = Color.FromRgb(0, 0, 0);
            var primaryColor = Color.FromRgb(0, 255, 0);
            
            BackgroundPreview.Background = new SolidColorBrush(bgColor);
            BackgroundHexText.Text = "#000000";
            BackgroundHexText.Foreground = Brushes.White;
            
            PrimaryPreview.Background = new SolidColorBrush(primaryColor);
            PrimaryHexText.Text = "#00FF00";
            PrimaryHexText.Foreground = Brushes.Black;
            
            Application.Current.Resources["AppBackgroundColor"] = bgColor;
            Application.Current.Resources["AppPrimaryColor"] = primaryColor;
            Application.Current.Resources["AppBackgroundBrush"] = new SolidColorBrush(bgColor);
            Application.Current.Resources["AppPrimaryBrush"] = new SolidColorBrush(primaryColor);
            
            MessageBox.Show("Colors reset to factory defaults!\n\nBackground: Black\nPrimary: Green", "Reset Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void SaveColors_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null || _cosmosDbService == null)
            {
                MessageBox.Show("Cannot save - not logged in!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string bgColor = BackgroundHexText.Text;
                string primaryColor = PrimaryHexText.Text;
                
                _currentUser.AppPreferences.CustomBackgroundColor = bgColor;
                _currentUser.AppPreferences.CustomTextColor = primaryColor;
                _currentUser.AppPreferences.CustomBorderColor = primaryColor;
                _currentUser.AppPreferences.CustomAccentColor = primaryColor;
                _currentUser.AppPreferences.ThemeName = "Custom";
                
                await _cosmosDbService.UpdateUserAsync(_currentUser);
                
                System.Diagnostics.Debug.WriteLine($"? SAVED: BG={bgColor}, Primary={primaryColor}");
                
                MessageBox.Show($"? Colors Saved!\n\nBackground: {bgColor}\nPrimary: {primaryColor}\n\nYour colors will load next time you log in.", "Saved!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Save failed: {ex.Message}");
                MessageBox.Show($"Save failed:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUserColors()
        {
            if (_currentUser?.AppPreferences == null) return;

            try
            {
                var bgColor = (Color)ColorConverter.ConvertFromString(_currentUser.AppPreferences.CustomBackgroundColor);
                BackgroundPreview.Background = new SolidColorBrush(bgColor);
                BackgroundHexText.Text = _currentUser.AppPreferences.CustomBackgroundColor;
                BackgroundHexText.Foreground = (bgColor.R * 0.299 + bgColor.G * 0.587 + bgColor.B * 0.114) > 128 ? Brushes.Black : Brushes.White;

                var primaryColor = (Color)ColorConverter.ConvertFromString(_currentUser.AppPreferences.CustomTextColor);
                PrimaryPreview.Background = new SolidColorBrush(primaryColor);
                PrimaryHexText.Text = _currentUser.AppPreferences.CustomTextColor;
                PrimaryHexText.Foreground = (primaryColor.R * 0.299 + primaryColor.G * 0.587 + primaryColor.B * 0.114) > 128 ? Brushes.Black : Brushes.White;

                System.Diagnostics.Debug.WriteLine($"? Loaded colors for {_currentUser.Username}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? Load error: {ex.Message}");
            }
        }

        public static void ApplySavedTheme(User user)
        {
            if (user?.AppPreferences == null) return;

            try
            {
                var bgColor = (Color)ColorConverter.ConvertFromString(user.AppPreferences.CustomBackgroundColor);
                var primaryColor = (Color)ColorConverter.ConvertFromString(user.AppPreferences.CustomTextColor);
                
                Application.Current.Resources["AppBackgroundColor"] = bgColor;
                Application.Current.Resources["AppPrimaryColor"] = primaryColor;
                Application.Current.Resources["AppBackgroundBrush"] = new SolidColorBrush(bgColor);
                Application.Current.Resources["AppPrimaryBrush"] = new SolidColorBrush(primaryColor);
                
                System.Diagnostics.Debug.WriteLine($"? Applied theme for {user.Username}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? Theme apply error: {ex.Message}");
            }
        }

        private void DiagnoseRole_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                var method = mainWindow.GetType().GetMethod("DiagnoseAndFixUserRole", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(mainWindow, null);
            }
        }

        private void DeleteConfirmTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DeleteAccountButton.IsEnabled = string.Equals(DeleteConfirmTextBox.Text.Trim(), "delete", StringComparison.OrdinalIgnoreCase);
        }

        private async void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null || _cosmosDbService == null) return;
            if (_currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager)
            {
                MessageBox.Show("Administrators cannot delete their own accounts.", "Admin Protection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show($"DELETE ACCOUNT: {_currentUser.Username}?\n\nThis CANNOT be undone!", "FINAL WARNING", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                await _cosmosDbService.DeleteUserAccountAsync(_currentUser.Username);
                MessageBox.Show("Account deleted.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Delete failed:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
