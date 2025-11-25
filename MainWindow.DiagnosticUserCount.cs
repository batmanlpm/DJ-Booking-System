using System;
using System.Threading.Tasks;
using System.Windows;

namespace DJBookingSystem
{
    public partial class MainWindow
    {
        /// <summary>
        /// Diagnostic - Show User Count from Database
        /// </summary>
        private async void Menu_DiagnosticUserCount_Click(object sender, RoutedEventArgs e)
        {
            if (_cosmosDbService == null)
            {
                MessageBox.Show("Database service not initialized!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("========================================");
                System.Diagnostics.Debug.WriteLine("DIAGNOSTIC: USER COUNT CHECK");
                System.Diagnostics.Debug.WriteLine("========================================");

                var users = await _cosmosDbService.GetAllUsersAsync();
                
                System.Diagnostics.Debug.WriteLine($"Total users in database: {users.Count}");
                
                var report = $"DATABASE USER COUNT DIAGNOSTIC\n" +
                            $"================================\n\n" +
                            $"Total Users: {users.Count}\n\n";

                if (users.Count > 0)
                {
                    report += "Users found:\n";
                    foreach (var user in users)
                    {
                        report += $"  • {user.Username} ({user.Role}) - {(user.IsActive ? "Active" : "Inactive")}\n";
                        System.Diagnostics.Debug.WriteLine($"  - Username: {user.Username}, ID: {user.Id}, Role: {user.Role}");
                    }
                }
                else
                {
                    report += "?? NO USERS FOUND!\n\n";
                    report += "This means:\n";
                    report += "  • The lowercase 'users' container is empty\n";
                    report += "  • Default sysadmin was not created\n";
                    report += "  • App needs to be restarted to reinitialize\n";
                }

                report += "\n================================\n";
                report += $"Logged in as: {_currentUser?.Username ?? "UNKNOWN"}\n";
                report += $"Session valid: {(_currentUser != null ? "YES" : "NO")}";

                System.Diagnostics.Debug.WriteLine("========================================");

                MessageBox.Show(report, "User Count Diagnostic", MessageBoxButton.OK, 
                    users.Count > 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
                MessageBox.Show($"Error checking user count:\n\n{ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
