using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    /// <summary>
    /// MainWindow partial class for Candy-Bot Image & Document Generation Tests
    /// </summary>
    public partial class MainWindow
    {
        #region Candy-Bot Image & Document Generation Tests

        /// <summary>
        /// Test image generation (requires API key)
        /// </summary>
        private async Task TestImageGenerationAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[Test] Starting image generation test...");
                
                if (_candyImageGen == null)
                {
                    MessageBox.Show(
                        "Image Generator not initialized!",
                        "Test Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                
                var result = await _candyImageGen.GenerateImageAsync(
                    "A futuristic DJ booth with neon green lights and turntables, cyberpunk style",
                    size: ImageSize.Square_1024,
                    quality: ImageQuality.Standard
                );

                if (result.Success)
                {
                    System.Diagnostics.Debug.WriteLine($"[Test] Image generated successfully: {result.LocalFilePath}");
                    MessageBox.Show(
                        $"? Image Generation Test PASSED!\n\n" +
                        $"Image saved to:\n{result.LocalFilePath}\n\n" +
                        $"Generation time: {result.Duration.TotalSeconds:F1} seconds",
                        "Image Generation Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    // Open folder containing image
                    var directory = System.IO.Path.GetDirectoryName(result.LocalFilePath);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        System.Diagnostics.Process.Start("explorer.exe", directory);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[Test] Image generation failed: {result.ErrorMessage}");
                    
                    // Parse error message for specific issues
                    string userMessage;
                    string title;
                    
                    if (result.ErrorMessage?.Contains("billing_hard_limit_reached") == true)
                    {
                        title = "OpenAI Billing Limit Reached";
                        userMessage = "? Image Generation Test FAILED\n\n" +
                                     "Error: Billing hard limit has been reached.\n\n" +
                                     "This means the OpenAI API key has exceeded its spending limit.\n\n" +
                                     "Solutions:\n" +
                                     "1. Add credits to your OpenAI account\n" +
                                     "2. Check your billing settings at:\n" +
                                     "   https://platform.openai.com/account/billing\n" +
                                     "3. Update your API key in the application\n\n" +
                                     "Note: DALL-E 3 costs approximately:\n" +
                                     "• $0.040 per 1024×1024 standard image\n" +
                                     "• $0.080 per 1024×1024 HD image";
                    }
                    else if (result.ErrorMessage?.Contains("insufficient_quota") == true)
                    {
                        title = "OpenAI Quota Exceeded";
                        userMessage = "? Image Generation Test FAILED\n\n" +
                                     "Error: Insufficient quota.\n\n" +
                                     "Your OpenAI account has run out of credits.\n\n" +
                                     "To fix this:\n" +
                                     "1. Visit: https://platform.openai.com/account/billing\n" +
                                     "2. Add payment method and credits\n" +
                                     "3. Wait a few minutes for activation\n" +
                                     "4. Try again";
                    }
                    else if (result.ErrorMessage?.Contains("invalid_api_key") == true)
                    {
                        title = "Invalid API Key";
                        userMessage = "? Image Generation Test FAILED\n\n" +
                                     "Error: Invalid API key.\n\n" +
                                     "The OpenAI API key is not valid or has expired.\n\n" +
                                     "Get a new API key at:\n" +
                                     "https://platform.openai.com/api-keys";
                    }
                    else
                    {
                        title = "Image Generation Failed";
                        userMessage = $"? Image Generation Test FAILED\n\n" +
                                     $"Error: {result.ErrorMessage}\n\n" +
                                     $"Note: This requires an OpenAI API key.\n" +
                                     $"Get one at: https://platform.openai.com/api-keys";
                    }
                    
                    MessageBox.Show(
                        userMessage,
                        title,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Test] Exception: {ex.Message}");
                MessageBox.Show(
                    $"? Unexpected Error\n\n{ex.Message}\n\n" +
                    $"Stack trace:\n{ex.StackTrace}",
                    "Test Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Test document generation (no API key needed)
        /// </summary>
        private async Task TestDocumentGenerationAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[Test] Starting document generation test...");

                if (_candyDocGen == null)
                {
                    MessageBox.Show(
                        "Document Generator not initialized!",
                        "Test Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // Test 1: Create text file
                var textResult = await _candyDocGen.CreateTextFileAsync(
                    "Hello from Candy-Bot!\n\n" +
                    "This is a test document created by the Candy-Bot Document Generator.\n\n" +
                    "Features:\n" +
                    "- Text files\n" +
                    "- Word documents\n" +
                    "- PDF files\n" +
                    "- Excel spreadsheets\n" +
                    "- PowerPoint presentations\n\n" +
                    $"Generated: {DateTime.Now}",
                    fileName: $"CandyBot_Test_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                );

                // Test 2: Create markdown file
                var mdResult = await _candyDocGen.CreateMarkdownFileAsync(
                    "Candy-Bot Test Document",
                    "## Test Successful!\n\n" +
                    "This markdown file was generated by Candy-Bot.\n\n" +
                    "### Features:\n" +
                    "- ? Text files\n" +
                    "- ? Markdown files\n" +
                    "- ? Word documents (with package)\n" +
                    "- ? PDF files (with package)\n\n" +
                    $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                    fileName: $"CandyBot_Test_{DateTime.Now:yyyyMMdd_HHmmss}.md"
                );

                if (textResult.Success && mdResult.Success)
                {
                    System.Diagnostics.Debug.WriteLine("[Test] Documents generated successfully");
                    
                    var directory = System.IO.Path.GetDirectoryName(textResult.FilePath) ?? "Unknown";
                    
                    var message = $"? Document Generation Test PASSED!\n\n" +
                                 $"Created 2 documents:\n\n" +
                                 $"1. Text File:\n{textResult.FileName}\n\n" +
                                 $"2. Markdown File:\n{mdResult.FileName}\n\n" +
                                 $"Location:\n{directory}";

                    var result = MessageBox.Show(
                        message + "\n\nOpen document folder?",
                        "Document Generation Success",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        // Open folder
                        var folderPath = System.IO.Path.GetDirectoryName(textResult.FilePath);
                        if (!string.IsNullOrEmpty(folderPath))
                        {
                            System.Diagnostics.Process.Start("explorer.exe", folderPath);
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[Test] Document generation failed");
                    MessageBox.Show("Document generation failed. Check Debug Output for details.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Test] Exception: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Test Error");
            }
        }

        /// <summary>
        /// Test DJ-specific document generation
        /// </summary>
        private async Task TestDJDocumentsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[Test] Starting DJ document test...");

                if (_candyDocGen == null)
                {
                    MessageBox.Show(
                        "Document Generator not initialized!",
                        "Test Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // Create sample tracks using the Services.Track class
                var tracks = new List<Services.Track>
                {
                    new Services.Track { Artist = "Daft Punk", Title = "One More Time", Duration = "5:20", Genre = "Electronic" },
                    new Services.Track { Artist = "The Chemical Brothers", Title = "Block Rockin' Beats", Duration = "4:58", Genre = "Big Beat" },
                    new Services.Track { Artist = "Fatboy Slim", Title = "Right Here, Right Now", Duration = "6:26", Genre = "Big Beat" },
                    new Services.Track { Artist = "The Prodigy", Title = "Firestarter", Duration = "4:40", Genre = "Electronic" },
                    new Services.Track { Artist = "Underworld", Title = "Born Slippy", Duration = "9:43", Genre = "Techno" }
                };

                // Generate DJ setlist
                var setlistResult = await _candyDocGen.CreateDJSetlistAsync(
                    djName: "DJ CANDY",
                    venueName: "The Fallen Collective",
                    eventDate: DateTime.Now.AddDays(7),
                    tracks: tracks,
                    outputFormat: DocumentType.Text // Use .txt for now
                );

                // Generate booking contract
                var contractResult = await _candyDocGen.CreateBookingContractAsync(
                    djName: "DJ CANDY",
                    venueName: "The Fallen Collective",
                    eventDate: DateTime.Now.AddDays(7),
                    fee: 500.00m,
                    additionalTerms: "- Equipment provided by venue\n- Sound check at 7:00 PM\n- Performance time: 9:00 PM - 2:00 AM"
                );

                if (setlistResult.Success && contractResult.Success)
                {
                    System.Diagnostics.Debug.WriteLine("[Test] DJ documents generated successfully");

                    var directory = System.IO.Path.GetDirectoryName(setlistResult.FilePath) ?? "Unknown";

                    var message = $"? DJ Document Test PASSED!\n\n" +
                                 $"Created:\n" +
                                 $"1. Setlist ({tracks.Count} tracks)\n" +
                                 $"2. Booking Contract ($500)\n\n" +
                                 $"Files saved to:\n{directory}";

                    var result = MessageBox.Show(
                        message + "\n\nOpen documents?",
                        "DJ Documents Success",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        _candyDocGen.OpenDocument(setlistResult.FilePath);
                        _candyDocGen.OpenDocument(contractResult.FilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Test] Exception: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Test Error");
            }
        }

        /// <summary>
        /// Run all tests
        /// </summary>
        private async Task RunAllCandyBotTestsAsync()
        {
            System.Diagnostics.Debug.WriteLine("\n========================================");
            System.Diagnostics.Debug.WriteLine("CANDY-BOT FULL FEATURE TEST");
            System.Diagnostics.Debug.WriteLine("========================================\n");

            // Test 1: Document Generation (always works)
            await TestDocumentGenerationAsync();
            
            await Task.Delay(1000); // Brief pause

            // Test 2: DJ Documents
            await TestDJDocumentsAsync();

            await Task.Delay(1000); // Brief pause

            // Test 3: Image Generation (only if API key configured)
            var testImages = MessageBox.Show(
                "Document tests complete!\n\n" +
                "Test image generation?\n" +
                "(Requires OpenAI API key and costs ~$0.04 per image)",
                "Image Generation Test",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (testImages == MessageBoxResult.Yes)
            {
                await TestImageGenerationAsync();
            }

            System.Diagnostics.Debug.WriteLine("\n========================================");
            System.Diagnostics.Debug.WriteLine("ALL TESTS COMPLETE");
            System.Diagnostics.Debug.WriteLine("========================================\n");

            MessageBox.Show(
                "? All Candy-Bot tests complete!\n\n" +
                "Check the Debug Output window for detailed results.",
                "Tests Complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        #endregion

        #region Test Menu Handlers

        private async void TestDocuments_Click(object sender, RoutedEventArgs e)
        {
            await TestDocumentGenerationAsync();
        }

        private async void TestDJDocuments_Click(object sender, RoutedEventArgs e)
        {
            await TestDJDocumentsAsync();
        }

        private async void TestImages_Click(object sender, RoutedEventArgs e)
        {
            await TestImageGenerationAsync();
        }

        private async void RunAllTests_Click(object sender, RoutedEventArgs e)
        {
            await RunAllCandyBotTestsAsync();
        }

        #endregion
    }
}
