using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DJBookingSystem.Models;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Service to reset HasSeenTutorial flag for all users
    /// Run this to force all users to see the tutorial on next login
    /// </summary>
    public static class TutorialResetService
    {
        /// <summary>
        /// Reset tutorial status for all users
        /// </summary>
        public static async Task ResetAllUsersAsync(CosmosDbService cosmosDbService)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("========================================");
                System.Diagnostics.Debug.WriteLine("   Reset Tutorial Status for All Users");
                System.Diagnostics.Debug.WriteLine("========================================");

                // Get all users
                var allUsers = await cosmosDbService.GetAllUsersAsync();
                
                System.Diagnostics.Debug.WriteLine($"Found {allUsers.Count} users");

                int updatedCount = 0;
                int skippedCount = 0;

                foreach (var user in allUsers)
                {
                    // Check if user already has HasSeenTutorial = false
                    if (!user.HasSeenTutorial)
                    {
                        System.Diagnostics.Debug.WriteLine($"  ??  Skipping {user.Username} (already set to false)");
                        skippedCount++;
                        continue;
                    }

                    // Update user
                    user.HasSeenTutorial = false;

                    try
                    {
                        await cosmosDbService.UpdateUserAsync(user);
                        System.Diagnostics.Debug.WriteLine($"  ? Updated {user.Username}");
                        updatedCount++;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"  ? Failed to update {user.Username}: {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine("========================================");
                System.Diagnostics.Debug.WriteLine($"Summary: Total={allUsers.Count}, Updated={updatedCount}, Skipped={skippedCount}");
                System.Diagnostics.Debug.WriteLine("========================================");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Reset tutorial status with UI feedback
        /// </summary>
        public static async Task ResetAllUsersWithUIAsync(CosmosDbService cosmosDbService, Window owner)
        {
            var result = MessageBox.Show(
                "?? Reset Tutorial Status for All Users?\n\n" +
                "This will:\n" +
                "• Set HasSeenTutorial = false for ALL users\n" +
                "• Force all users to watch tutorial on next login\n" +
                "• Cannot be undone\n\n" +
                "Continue?",
                "Reset Tutorial Status",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                // Get all users
                var allUsers = await cosmosDbService.GetAllUsersAsync();
                int updatedCount = 0;
                int skippedCount = 0;

                foreach (var user in allUsers)
                {
                    if (!user.HasSeenTutorial)
                    {
                        skippedCount++;
                        continue;
                    }

                    user.HasSeenTutorial = false;
                    await cosmosDbService.UpdateUserAsync(user);
                    updatedCount++;
                }

                MessageBox.Show(
                    $"? Tutorial Status Reset Complete!\n\n" +
                    $"Total users: {allUsers.Count}\n" +
                    $"Updated: {updatedCount}\n" +
                    $"Skipped: {skippedCount}\n\n" +
                    $"All users will see the mandatory tutorial on next login!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error resetting tutorial status:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Reset tutorial status for a specific user
        /// </summary>
        public static async Task ResetUserAsync(CosmosDbService cosmosDbService, string username)
        {
            try
            {
                var user = await cosmosDbService.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine($"? User '{username}' not found");
                    return;
                }

                if (!user.HasSeenTutorial)
                {
                    System.Diagnostics.Debug.WriteLine($"??  User '{username}' already has HasSeenTutorial = false");
                    return;
                }

                user.HasSeenTutorial = false;
                await cosmosDbService.UpdateUserAsync(user);
                
                System.Diagnostics.Debug.WriteLine($"? Reset tutorial status for user: {username}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error: {ex.Message}");
                throw;
            }
        }
    }
}
