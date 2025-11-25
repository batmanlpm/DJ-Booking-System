using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Utility to verify Candy-Bot Voice and File Manager integration
    /// Run this to test all features are working correctly
    /// </summary>
    public class CandyBotVerificationUtility
    {
        private readonly CandyBotSoundManager _soundManager;
        private readonly CandyBotFileManager _fileManager;
        private readonly List<string> _testResults;

        public CandyBotVerificationUtility()
        {
            _soundManager = new CandyBotSoundManager();
            _fileManager = new CandyBotFileManager();
            _testResults = new List<string>();
        }

        /// <summary>
        /// Run all verification tests
        /// </summary>
        public async Task<VerificationReport> RunAllTestsAsync()
        {
            var report = new VerificationReport();
            
            Debug.WriteLine("=== Candy-Bot Verification Starting ===");

            // Test 1: Voice Files
            Debug.WriteLine("\n[TEST 1] Checking voice files...");
            report.VoiceFilesPresent = TestVoiceFiles();

            // Test 2: Voice Mapper
            Debug.WriteLine("\n[TEST 2] Testing voice mapper...");
            report.VoiceMapperWorking = TestVoiceMapper();

            // Test 3: Sound Manager
            Debug.WriteLine("\n[TEST 3] Testing sound manager...");
            report.SoundManagerWorking = await TestSoundManagerAsync();

            // Test 4: File Manager
            Debug.WriteLine("\n[TEST 4] Testing file manager...");
            report.FileManagerWorking = TestFileManager();

            // Test 5: Categories
            Debug.WriteLine("\n[TEST 5] Verifying categories...");
            report.CategoriesValid = TestCategories();

            report.AllTestsPassed = report.VoiceFilesPresent &&
                                   report.VoiceMapperWorking &&
                                   report.SoundManagerWorking &&
                                   report.FileManagerWorking &&
                                   report.CategoriesValid;

            Debug.WriteLine($"\n=== Verification Complete: {(report.AllTestsPassed ? "PASSED ✓" : "FAILED ✗")} ===");

            return report;
        }

        /// <summary>
        /// Test 1: Verify all voice files exist
        /// </summary>
        private bool TestVoiceFiles()
        {
            try
            {
                List<string> missingFiles;
                bool allPresent = CandyBotVoiceMapper.VerifyVoiceFiles(out missingFiles);

                if (allPresent)
                {
                    Debug.WriteLine("✓ All 100 voice files present");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"✗ Missing {missingFiles.Count} voice files:");
                    foreach (var missing in missingFiles)
                    {
                        Debug.WriteLine($"  - {missing}");
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ Error checking voice files: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test 2: Verify voice mapper functionality
        /// </summary>
        private bool TestVoiceMapper()
        {
            try
            {
                // Test specific line retrieval
                var line001 = CandyBotVoiceMapper.GetLine("001");
                if (line001 == null || line001.Category != "Greeting")
                {
                    Debug.WriteLine("✗ Failed to get line 001 or wrong category");
                    return false;
                }

                // Test category retrieval
                var greetings = CandyBotVoiceMapper.GetCategoryLines("Greeting");
                if (greetings.Count != 15)
                {
                    Debug.WriteLine($"✗ Expected 15 greetings, got {greetings.Count}");
                    return false;
                }

                // Test random retrieval
                var randomGreeting = CandyBotVoiceMapper.GetRandomGreeting();
                if (randomGreeting == null)
                {
                    Debug.WriteLine("✗ Failed to get random greeting");
                    return false;
                }

                Debug.WriteLine("✓ Voice mapper working correctly");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ Voice mapper error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test 3: Verify sound manager functionality
        /// </summary>
        private async Task<bool> TestSoundManagerAsync()
        {
            try
            {
                // Enable voice mode
                _soundManager.SetVoiceMode(true);
                _soundManager.SetSoundsEnabled(true);

                // Test specific voice line (non-blocking test)
                await _soundManager.PlayVoiceLine("001");

                // Test category playback
                await _soundManager.PlayRandomVoiceFromCategory("Greeting");

                // Test context methods
                _soundManager.PlayGreeting();

                Debug.WriteLine("✓ Sound manager working correctly");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ Sound manager error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test 4: Verify file manager functionality
        /// </summary>
        private bool TestFileManager()
        {
            try
            {
                // Test folder approval
                _fileManager.AddApprovedFolder(AppDomain.CurrentDomain.BaseDirectory);

                // Test log functionality
                var log = _fileManager.GetOperationLog();
                if (log.Count == 0)
                {
                    Debug.WriteLine("✗ Operation log not working");
                    return false;
                }

                // Test logging toggle
                _fileManager.SetLogging(false);
                _fileManager.SetLogging(true);

                Debug.WriteLine("✓ File manager working correctly");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ File manager error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test 5: Verify all categories are mapped correctly
        /// </summary>
        private bool TestCategories()
        {
            try
            {
                var expectedCategories = new Dictionary<string, int>
                {
                    { "Greeting", 15 },
                    { "Help", 15 },
                    { "Booking", 10 },
                    { "Positive", 15 },
                    { "Error", 10 },
                    { "Personality", 15 },
                    { "Shy", 5 },
                    { "Funny", 5 },
                    { "ShitStirring", 3 },
                    { "Professional", 3 },
                    { "Raunchy", 4 }
                };

                var actualCounts = CandyBotVoiceMapper.GetCategoryCounts();

                foreach (var expected in expectedCategories)
                {
                    if (!actualCounts.ContainsKey(expected.Key))
                    {
                        Debug.WriteLine($"✗ Missing category: {expected.Key}");
                        return false;
                    }

                    if (actualCounts[expected.Key] != expected.Value)
                    {
                        Debug.WriteLine($"✗ Category {expected.Key}: Expected {expected.Value}, got {actualCounts[expected.Key]}");
                        return false;
                    }
                }

                Debug.WriteLine("✓ All categories mapped correctly");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ Category verification error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Show verification results in MessageBox
        /// </summary>
        public static async Task ShowVerificationDialogAsync()
        {
            var utility = new CandyBotVerificationUtility();
            var report = await utility.RunAllTestsAsync();

            var message = report.AllTestsPassed
                ? "🎉 All Candy-Bot tests PASSED!\n\n" +
                  "✓ Voice files present\n" +
                  "✓ Voice mapper working\n" +
                  "✓ Sound manager working\n" +
                  "✓ File manager working\n" +
                  "✓ Categories valid\n\n" +
                  "Candy-Bot is ready to use!"
                : "⚠️ Some Candy-Bot tests FAILED\n\n" +
                  $"Voice files: {(report.VoiceFilesPresent ? "✓" : "✗")}\n" +
                  $"Voice mapper: {(report.VoiceMapperWorking ? "✓" : "✗")}\n" +
                  $"Sound manager: {(report.SoundManagerWorking ? "✓" : "✗")}\n" +
                  $"File manager: {(report.FileManagerWorking ? "✓" : "✗")}\n" +
                  $"Categories: {(report.CategoriesValid ? "✓" : "✗")}\n\n" +
                  "Check Debug Output for details";

            MessageBox.Show(message, "Candy-Bot Verification",
                MessageBoxButton.OK,
                report.AllTestsPassed ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
    }

    /// <summary>
    /// Verification report data
    /// </summary>
    public class VerificationReport
    {
        public bool VoiceFilesPresent { get; set; }
        public bool VoiceMapperWorking { get; set; }
        public bool SoundManagerWorking { get; set; }
        public bool FileManagerWorking { get; set; }
        public bool CategoriesValid { get; set; }
        public bool AllTestsPassed { get; set; }
    }
}
