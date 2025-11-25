using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class FileOrganizerWindow : Window
    {
        private readonly FileOrganizerService _organizer;

        public FileOrganizerWindow()
        {
            InitializeComponent();
            _organizer = new FileOrganizerService();
            
            // Set default folder to Downloads
            var downloads = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            FolderPathTextBox.Text = downloads;
        }

        private void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = "Select folder to organize";
            dialog.SelectedPath = FolderPathTextBox.Text;
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderPathTextBox.Text = dialog.SelectedPath;
            }
        }

        private void Analyze_Click(object sender, RoutedEventArgs e)
        {
            var path = FolderPathTextBox.Text;
            if (string.IsNullOrWhiteSpace(path))
            {
                ResultsTextBlock.Text = "❌ Please select a folder first!";
                return;
            }

            ResultsTextBlock.Text = "📊 Analyzing folder...\n\n";
            
            try
            {
                var stats = _organizer.GetStorageStats(path);
                
                ResultsTextBlock.Text += "=== Storage Analysis ===\n\n";
                
                foreach (var category in stats.OrderByDescending(x => x.Value))
                {
                    var sizeMB = category.Value / 1024.0 / 1024.0;
                    ResultsTextBlock.Text += $"{category.Key}: {sizeMB:F2} MB\n";
                }
                
                var totalMB = stats.Values.Sum() / 1024.0 / 1024.0;
                ResultsTextBlock.Text += $"\nTotal: {totalMB:F2} MB\n";
            }
            catch (Exception ex)
            {
                ResultsTextBlock.Text = $"❌ Error: {ex.Message}";
            }
        }

        private async void Organize_Click(object sender, RoutedEventArgs e)
        {
            var path = FolderPathTextBox.Text;
            if (string.IsNullOrWhiteSpace(path))
            {
                ResultsTextBlock.Text = "❌ Please select a folder first!";
                return;
            }

            var result = System.Windows.MessageBox.Show(
                $"This will organize all files in:\n{path}\n\nContinue?",
                "Confirm Organization",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            ResultsTextBlock.Text = "🗂️ Organizing files...\n\n";

            try
            {
                var organizeResult = await _organizer.OrganizeDirectoryAsync(
                    path,
                    RemoveDuplicatesCheckBox.IsChecked == true,
                    OrganizeByDateCheckBox.IsChecked == true
                );

                ResultsTextBlock.Text = "=== Organization Complete! ===\n\n";
                ResultsTextBlock.Text += $"✅ Files Organized: {organizeResult.FilesOrganized}\n";
                ResultsTextBlock.Text += $"🗑️ Duplicates Removed: {organizeResult.DuplicatesRemoved}\n";
                ResultsTextBlock.Text += $"📁 Folders Created: {organizeResult.FoldersCreated}\n\n";

                if (organizeResult.FilesByCategory.Any())
                {
                    ResultsTextBlock.Text += "=== Files by Category ===\n\n";
                    foreach (var cat in organizeResult.FilesByCategory.OrderByDescending(x => x.Value))
                    {
                        ResultsTextBlock.Text += $"{cat.Key}: {cat.Value} files\n";
                    }
                }

                if (CleanEmptyFoldersCheckBox.IsChecked == true)
                {
                    var cleaned = _organizer.CleanEmptyFolders(path);
                    ResultsTextBlock.Text += $"\n🧹 Empty Folders Cleaned: {cleaned}\n";
                }

                if (organizeResult.Errors.Any())
                {
                    ResultsTextBlock.Text += "\n⚠️ Errors:\n";
                    foreach (var error in organizeResult.Errors)
                    {
                        ResultsTextBlock.Text += $"  • {error}\n";
                    }
                }

                System.Windows.MessageBox.Show(
                    $"Organization complete!\n\n{organizeResult.FilesOrganized} files organized",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ResultsTextBlock.Text = $"❌ Error: {ex.Message}";
            }
        }

        private async void FindDuplicates_Click(object sender, RoutedEventArgs e)
        {
            var path = FolderPathTextBox.Text;
            if (string.IsNullOrWhiteSpace(path))
            {
                ResultsTextBlock.Text = "❌ Please select a folder first!";
                return;
            }

            ResultsTextBlock.Text = "🔍 Scanning for duplicates...\n\n";

            try
            {
                var duplicates = await _organizer.FindDuplicatesAsync(path);

                if (!duplicates.Any())
                {
                    ResultsTextBlock.Text = "✅ No duplicates found!";
                    return;
                }

                ResultsTextBlock.Text = $"=== Found {duplicates.Count} Duplicate Sets ===\n\n";

                foreach (var dup in duplicates)
                {
                    ResultsTextBlock.Text += $"Duplicate Set ({dup.Value.Count} files):\n";
                    foreach (var file in dup.Value)
                    {
                        var info = new System.IO.FileInfo(file);
                        ResultsTextBlock.Text += $"  • {info.Name} ({info.Length / 1024} KB)\n";
                    }
                    ResultsTextBlock.Text += "\n";
                }
            }
            catch (Exception ex)
            {
                ResultsTextBlock.Text = $"❌ Error: {ex.Message}";
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
