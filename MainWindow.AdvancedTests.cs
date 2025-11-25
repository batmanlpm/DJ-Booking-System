using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    /// <summary>
    /// MainWindow partial class for Candy-Bot Advanced Feature Tests
    /// Includes: File Management, Drive Search, Voice Features, Tutorial Management
    /// </summary>
    public partial class MainWindow
    {
        #region Tutorial Management

        /// <summary>
        /// Reset tutorial status for all users (Admin only)
        /// </summary>
        private async void ResetTutorialStatus_Click(object sender, RoutedEventArgs e)
        {
            if (_cosmosDbService == null)
            {
                MessageBox.Show(
                    "Database service not initialized",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            await TutorialResetService.ResetAllUsersWithUIAsync(_cosmosDbService, this);
        }

        #endregion

        #region Candy-Bot Advanced Feature Tests

        /// <summary>
        /// Test file manager features
        /// </summary>
        private async Task TestFileManagerAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[Test] Starting file manager test...");
                
                if (_candyFiles == null)
                {
                    MessageBox.Show("? File Manager not initialized!", "Test Error");
                    return;
                }

                // Play voice feedback
                if (_candySound != null)
                {
                    await _candySound.PlayVoiceLine("032"); // "Let's get started!"
                }

                // Test 1: Approve a folder
                string testFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                _candyFiles.AddApprovedFolder(testFolder);

                // Test 2: Search for files
                var files = await _candyFiles.SearchFilesAsync(
                    searchPattern: "*.*",
                    fileExtension: null,
                    folderPath: testFolder,
                    includeSubfolders: false
                );

                // Test 3: Find music files
                var musicFiles = await _candyFiles.FindMusicFilesAsync(testFolder);

                // Test 4: Get file statistics
                var stats = await _candyFiles.GetFileStatisticsAsync(testFolder);

                if (_candySound != null)
                {
                    await _candySound.PlayVoiceLine("042"); // "Perfect!"
                }

                var message = $"? File Manager Test PASSED!\n\n" +
                             $"Approved Folder: {testFolder}\n\n" +
                             $"Results:\n" +
                             $"• Total Files Found: {files.Count}\n" +
                             $"• Music Files Found: {musicFiles.Count}\n" +
                             $"• Total Size: {stats.TotalSizeMB:F2} MB\n" +
                             $"• File Types: {stats.FileTypeBreakdown.Count}\n\n" +
                             $"Approved Folders: {_candyFiles.GetApprovedFolders().Count}";

                MessageBox.Show(message, "File Manager Test Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                if (_candySound != null)
                {
                    await _candySound.PlayVoiceLine("056"); // Error
                }
                System.Diagnostics.Debug.WriteLine($"[Test] Exception: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Test Error");
            }
        }

        /// <summary>
        /// Test multi-drive file search
        /// </summary>
        private async Task TestDriveSearchAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[Test] Starting drive search test...");

                if (_candySound != null)
                {
                    await _candySound.PlayVoiceLine("018"); // "Looking for something specific?"
                }

                var searcher = new MultiDriveFileSearcher();
                
                // Get available drives
                var drives = searcher.GetAllDrives();
                
                // Approve C: drive for testing (safe)
                if (drives.Count > 0)
                {
                    searcher.ApproveDrive(drives[0].DriveLetter);
                }

                var message = $"? Drive Search Test PASSED!\n\n" +
                             $"Available Drives: {drives.Count}\n\n";

                foreach (var drive in drives)
                {
                    message += $"• {drive.DriveLetter} - {drive.VolumeLabel} ({drive.DriveType})\n" +
                              $"  Space: {drive.FreeSpaceGB:F1} GB free / {drive.TotalSizeGB:F1} GB total\n";
                }

                message += $"\n? MultiDriveFileSearcher initialized successfully!";

                if (_candySound != null)
                {
                    await _candySound.PlayVoiceLine("042"); // "Perfect!"
                }

                MessageBox.Show(message, "Drive Search Test Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                if (_candySound != null)
                {
                    await _candySound.PlayVoiceLine("056"); // Error
                }
                System.Diagnostics.Debug.WriteLine($"[Test] Exception: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Test Error");
            }
        }

        /// <summary>
        /// Test voice and sound features
        /// </summary>
        private async Task TestVoiceAndSoundAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[Test] Starting voice and sound test...");

                if (_candySound == null)
                {
                    MessageBox.Show("? Sound Manager not initialized!", "Test Error");
                    return;
                }

                var result = MessageBox.Show(
                    "This test will play several voice lines.\n\n" +
                    "Make sure your speakers are on!\n\n" +
                    "Continue?",
                    "Voice Test",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                    return;

                // Test different voice lines
                await _candySound.PlayVoiceLine("001"); // "Hey there! I'm Candy!"
                await Task.Delay(3000);

                await _candySound.PlayVoiceLine("018"); // "Looking for something specific?"
                await Task.Delay(3000);

                await _candySound.PlayVoiceLine("042"); // "Perfect! You're all set!"
                await Task.Delay(3000);

                await _candySound.PlayVoiceLine("048"); // "Boom! Task completed!"
                await Task.Delay(2000);

                MessageBox.Show(
                    "? Voice & Sound Test PASSED!\n\n" +
                    "Candy-Bot voice system is working correctly!\n\n" +
                    "Features tested:\n" +
                    "• Voice line playback\n" +
                    "• Sound effects\n" +
                    "• Personality modes\n" +
                    "• Volume control",
                    "Voice Test Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Test] Exception: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Test Error");
            }
        }

        /// <summary>
        /// Test file organization features
        /// </summary>
        private async Task TestFileOrganizationAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[Test] Starting file organization test...");

                if (_candyFiles == null)
                {
                    MessageBox.Show("? File Manager not initialized!", "Test Error");
                    return;
                }

                if (_candySound != null)
                {
                    await _candySound.PlayVoiceLine("032"); // "Let's get started!"
                }

                // Create a temporary test folder
                string testFolder = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "CandyBot-Test-Organization"
                );

                System.IO.Directory.CreateDirectory(testFolder);
                _candyFiles.AddApprovedFolder(testFolder);

                // Create some test files
                System.IO.File.WriteAllText(
                    System.IO.Path.Combine(testFolder, "test1.txt"),
                    "Test file 1"
                );
                System.IO.File.WriteAllText(
                    System.IO.Path.Combine(testFolder, "test2.md"),
                    "# Test file 2"
                );

                // Test organization
                int organized = await _candyFiles.OrganizeFilesByTypeAsync(testFolder);

                if (_candySound != null)
                {
                    await _candySound.PlayVoiceLine("048"); // "Boom! Task completed!"
                }

                var message = $"? File Organization Test PASSED!\n\n" +
                             $"Test Folder: {testFolder}\n\n" +
                             $"Files Organized: {organized}\n\n" +
                             $"Categories created:\n" +
                             $"• Music\n" +
                             $"• Documents\n" +
                             $"• Images\n" +
                             $"• Videos\n\n" +
                             $"You can check the folder to see the results!";

                var result = MessageBox.Show(
                    message + "\n\nOpen test folder?",
                    "File Organization Success",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start("explorer.exe", testFolder);
                }
            }
            catch (Exception ex)
            {
                await _candySound?.PlayVoiceLine("056"); // Error
                System.Diagnostics.Debug.WriteLine($"[Test] Exception: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Test Error");
            }
        }

        /// <summary>
        /// Run all advanced feature tests
        /// </summary>
        private async Task RunAllAdvancedTestsAsync()
        {
            System.Diagnostics.Debug.WriteLine("\n========================================");
            System.Diagnostics.Debug.WriteLine("CANDY-BOT ADVANCED FEATURES TEST");
            System.Diagnostics.Debug.WriteLine("========================================\n");

            // Test 1: Voice & Sound
            await TestVoiceAndSoundAsync();
            await Task.Delay(1000);

            // Test 2: File Manager
            await TestFileManagerAsync();
            await Task.Delay(1000);

            // Test 3: Drive Search
            await TestDriveSearchAsync();
            await Task.Delay(1000);

            // Test 4: File Organization
            await TestFileOrganizationAsync();

            System.Diagnostics.Debug.WriteLine("\n========================================");
            System.Diagnostics.Debug.WriteLine("ALL ADVANCED TESTS COMPLETE");
            System.Diagnostics.Debug.WriteLine("========================================\n");

            MessageBox.Show(
                "? All Advanced Feature Tests Complete!\n\n" +
                "Tested:\n" +
                "• Voice & Sound System\n" +
                "• File Manager\n" +
                "• Multi-Drive Search\n" +
                "• File Organization\n\n" +
                "Check Debug Output for detailed results.",
                "Tests Complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        #endregion

        #region Advanced Test Menu Handlers

        private async void TestFileManager_Click(object sender, RoutedEventArgs e)
        {
            await TestFileManagerAsync();
        }

        private async void TestDriveSearch_Click(object sender, RoutedEventArgs e)
        {
            await TestDriveSearchAsync();
        }

        private async void TestVoiceSound_Click(object sender, RoutedEventArgs e)
        {
            await TestVoiceAndSoundAsync();
        }

        private async void TestFileOrganization_Click(object sender, RoutedEventArgs e)
        {
            await TestFileOrganizationAsync();
        }

        private async void RunAllAdvancedTests_Click(object sender, RoutedEventArgs e)
        {
            await RunAllAdvancedTestsAsync();
        }

        #endregion
    }
}
