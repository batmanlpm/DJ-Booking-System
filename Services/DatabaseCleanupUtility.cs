using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Azure.Cosmos;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Database Cleanup Utility
    /// Fixes duplicate Users/users container issue
    /// </summary>
    public class DatabaseCleanupUtility
    {
        private readonly string _connectionString;
        private readonly string _databaseName;

        public DatabaseCleanupUtility(string connectionString, string databaseName = "DJBookingDB")
        {
            _connectionString = connectionString;
            _databaseName = databaseName;
        }

        /// <summary>
        /// Diagnose and report container status
        /// </summary>
        public async Task<DiagnosticReport> DiagnoseContainersAsync()
        {
            var report = new DiagnosticReport();
            
            try
            {
                using var client = new CosmosClient(_connectionString);
                var database = client.GetDatabase(_databaseName);

                // Check lowercase 'users' container
                try
                {
                    var usersContainer = database.GetContainer("users");
                    var usersQuery = usersContainer.GetItemQueryIterator<dynamic>("SELECT VALUE COUNT(1) FROM c");
                    if (usersQuery.HasMoreResults)
                    {
                        var response = await usersQuery.ReadNextAsync();
                        report.LowercaseUsersExists = true;
                        report.LowercaseUsersCount = response.FirstOrDefault() ?? 0;
                    }
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    report.LowercaseUsersExists = false;
                }

                // Check capitalized 'Users' container
                try
                {
                    var UsersContainer = database.GetContainer("Users");
                    var UsersQuery = UsersContainer.GetItemQueryIterator<dynamic>("SELECT VALUE COUNT(1) FROM c");
                    if (UsersQuery.HasMoreResults)
                    {
                        var response = await UsersQuery.ReadNextAsync();
                        report.CapitalizedUsersExists = true;
                        report.CapitalizedUsersCount = response.FirstOrDefault() ?? 0;
                    }
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    report.CapitalizedUsersExists = false;
                }

                report.Success = true;
            }
            catch (Exception ex)
            {
                report.Success = false;
                report.ErrorMessage = ex.Message;
            }

            return report;
        }

        /// <summary>
        /// Delete the capitalized Users container if it exists
        /// </summary>
        public async Task<CleanupResult> DeleteCapitalizedUsersContainerAsync()
        {
            var result = new CleanupResult();

            try
            {
                using var client = new CosmosClient(_connectionString);
                var database = client.GetDatabase(_databaseName);

                // First, verify it exists and get count
                var UsersContainer = database.GetContainer("Users");
                var query = UsersContainer.GetItemQueryIterator<dynamic>("SELECT VALUE COUNT(1) FROM c");
                
                if (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    result.ItemsInDeletedContainer = response.FirstOrDefault() ?? 0;
                }

                // Delete the container
                await UsersContainer.DeleteContainerAsync();

                result.Success = true;
                result.Message = $"Successfully deleted capitalized 'Users' container (had {result.ItemsInDeletedContainer} items)";
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                result.Success = false;
                result.Message = "Capitalized 'Users' container does not exist - nothing to delete";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Failed to delete container: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Show diagnostic window
        /// </summary>
        public static async Task ShowDiagnosticWindowAsync(string connectionString)
        {
            var utility = new DatabaseCleanupUtility(connectionString);
            var report = await utility.DiagnoseContainersAsync();

            var message = $@"DATABASE CONTAINER DIAGNOSIS
================================

Lowercase 'users' Container:
  Exists: {(report.LowercaseUsersExists ? "? YES" : "? NO")}
  Items: {report.LowercaseUsersCount}
  Status: {(report.LowercaseUsersExists ? "This is the CORRECT container" : "MISSING - Should exist!")}

Capitalized 'Users' Container:
  Exists: {(report.CapitalizedUsersExists ? "?? YES" : "? NO")}
  Items: {report.CapitalizedUsersCount}
  Status: {(report.CapitalizedUsersExists ? "This is a DUPLICATE - Should be deleted" : "Good - Already cleaned up")}

================================
RECOMMENDATION:

{GetRecommendation(report)}

================================

Would you like to fix this now?";

            var result = MessageBox.Show(
                message,
                "Database Container Diagnosis",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                await AutoFixAsync(utility, report);
            }
        }

        private static string GetRecommendation(DiagnosticReport report)
        {
            if (!report.LowercaseUsersExists && !report.CapitalizedUsersExists)
            {
                return "?? WARNING: No Users container found!\n   The database needs initialization.";
            }

            if (report.LowercaseUsersExists && !report.CapitalizedUsersExists)
            {
                return "? PERFECT: Only the correct container exists.\n   No action needed!";
            }

            if (!report.LowercaseUsersExists && report.CapitalizedUsersExists)
            {
                return "?? CRITICAL: Using wrong container!\n   Need to create lowercase 'users' container";
            }

            if (report.LowercaseUsersExists && report.CapitalizedUsersExists)
            {
                if (report.CapitalizedUsersCount == 0)
                {
                    return "? Safe to delete capitalized 'Users' container.\n   All data is in the correct lowercase 'users' container.";
                }
                else
                {
                    return $"?? Both containers have data!\n   'Users' has {report.CapitalizedUsersCount} items\n   'users' has {report.LowercaseUsersCount} items\n   Manual review needed.";
                }
            }

            return "Unknown state";
        }

        private static async Task AutoFixAsync(DatabaseCleanupUtility utility, DiagnosticReport report)
        {
            try
            {
                // Only delete if capitalized container is empty
                if (report.CapitalizedUsersExists && report.CapitalizedUsersCount == 0)
                {
                    var deleteResult = await utility.DeleteCapitalizedUsersContainerAsync();
                    MessageBox.Show(
                        deleteResult.Message,
                        deleteResult.Success ? "Success" : "Error",
                        MessageBoxButton.OK,
                        deleteResult.Success ? MessageBoxImage.Information : MessageBoxImage.Error);
                }
                else if (report.CapitalizedUsersExists && report.CapitalizedUsersCount > 0)
                {
                    MessageBox.Show(
                        $"Cannot auto-delete: Capitalized 'Users' container has {report.CapitalizedUsersCount} items.\n\nPlease manually review and delete via Azure Portal.",
                        "Manual Action Required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show(
                        "No action needed - database is already in correct state!",
                        "Already Fixed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Auto-fix failed: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    public class DiagnosticReport
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
        public bool LowercaseUsersExists { get; set; }
        public int LowercaseUsersCount { get; set; }
        public bool CapitalizedUsersExists { get; set; }
        public int CapitalizedUsersCount { get; set; }
    }

    public class CleanupResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int ItemsInDeletedContainer { get; set; }
    }
}
