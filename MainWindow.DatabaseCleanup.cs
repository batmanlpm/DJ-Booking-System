using System;
using System.Threading.Tasks;
using System.Windows;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class MainWindow
    {
        /// <summary>
        /// Database Cleanup Menu Handler
        /// </summary>
        private async void Menu_DatabaseCleanup_Click(object sender, RoutedEventArgs e)
        {
            if (_cosmosDbService == null)
            {
                MessageBox.Show(
                    "Database service not initialized!",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            // Check if user is admin
            if (_currentUser.Role != Models.UserRole.SysAdmin && _currentUser.Role != Models.UserRole.Manager)
            {
                MessageBox.Show(
                    "Only administrators can access database cleanup tools.",
                    "Permission Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Get connection string from CosmosDbService
                string connectionString = $"AccountEndpoint=https://fallen-collective.documents.azure.com:443/;AccountKey=EpxIq3hV8kXQ7kNY1KKJQmL5dkX0uZeW4GMUinPf6hNqRApx84Co5Ffve0bAktpyzH2xho5swBV5ACDbeunr5Q==;";

                await DatabaseCleanupUtility.ShowDiagnosticWindowAsync(connectionString);

                // Refresh users view after cleanup
                MessageBox.Show(
                    "Database cleanup complete!\n\nPlease restart the application for changes to take effect.",
                    "Cleanup Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Database cleanup failed:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
