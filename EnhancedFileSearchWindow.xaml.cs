using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DJBookingSystem.Services;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace DJBookingSystem
{
    /// <summary>
    /// Advanced File Search Window
    /// Search across all approved drives for any file type
    /// User can specify extensions and choose actions for found files
    /// </summary>
    public partial class EnhancedFileSearchWindow : Window
    {
        private CandyBotFileManager _fileManager;
        private CandyBotTextToSpeech? _tts;
        private CandyBotSoundManager? _soundManager;
        private ObservableCollection<FileSearchResult> _searchResults;
        private bool _isSearching;

        public EnhancedFileSearchWindow(
            CandyBotFileManager fileManager,
            CandyBotTextToSpeech? tts = null,
            CandyBotSoundManager? soundManager = null)
        {
            InitializeComponent();
            
            _fileManager = fileManager;
            _tts = tts;
            _soundManager = soundManager;
            _searchResults = new ObservableCollection<FileSearchResult>();
            
            ResultsDataGrid.ItemsSource = _searchResults;
            
            // Populate common file types
            PopulateFileTypePresets();
            
            // Say hello
            _tts?.SpeakAsync("File search ready! What would you like to find?");
        }

        /// <summary>
        /// Populate common file type checkboxes
        /// </summary>
        private void PopulateFileTypePresets()
        {
            // Add common types to the checkbox list
            var commonTypes = new[]
            {
                new FileTypePreset { Extension = ".txt", Description = "Text Files", Category = "Documents" },
                new FileTypePreset { Extension = ".docx", Description = "Word Documents", Category = "Documents" },
                new FileTypePreset { Extension = ".pdf", Description = "PDF Files", Category = "Documents" },
                new FileTypePreset { Extension = ".xlsx", Description = "Excel Spreadsheets", Category = "Documents" },
                
                new FileTypePreset { Extension = ".mp3", Description = "MP3 Audio", Category = "Music" },
                new FileTypePreset { Extension = ".wav", Description = "WAV Audio", Category = "Music" },
                new FileTypePreset { Extension = ".flac", Description = "FLAC Audio", Category = "Music" },
                new FileTypePreset { Extension = ".m4a", Description = "M4A Audio", Category = "Music" },
                
                new FileTypePreset { Extension = ".jpg", Description = "JPEG Images", Category = "Images" },
                new FileTypePreset { Extension = ".png", Description = "PNG Images", Category = "Images" },
                new FileTypePreset { Extension = ".gif", Description = "GIF Images", Category = "Images" },
                
                new FileTypePreset { Extension = ".mp4", Description = "MP4 Videos", Category = "Videos" },
                new FileTypePreset { Extension = ".avi", Description = "AVI Videos", Category = "Videos" },
                new FileTypePreset { Extension = ".mkv", Description = "MKV Videos", Category = "Videos" },
                
                new FileTypePreset { Extension = ".zip", Description = "ZIP Archives", Category = "Archives" },
                new FileTypePreset { Extension = ".rar", Description = "RAR Archives", Category = "Archives" },
                new FileTypePreset { Extension = ".7z", Description = "7-Zip Archives", Category = "Archives" }
            };

            FileTypeListBox.ItemsSource = commonTypes;
        }

        /// <summary>
        /// Start search button click
        /// </summary>
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isSearching)
            {
                MessageBox.Show("Search already in progress!");
                return;
            }

            await PerformSearchAsync();
        }

        /// <summary>
        /// Perform the file search
        /// </summary>
        private async Task PerformSearchAsync()
        {
            try
            {
                _isSearching = true;
                SearchButton.IsEnabled = false;
                StatusTextBlock.Text = "Searching...";
                _searchResults.Clear();

                // Get selected file types
                var selectedTypes = GetSelectedFileTypes();
                
                if (selectedTypes.Count == 0 && string.IsNullOrWhiteSpace(CustomExtensionTextBox.Text))
                {
                    MessageBox.Show("Please select at least one file type or enter a custom extension!");
                    return;
                }

                // Add custom extensions
                if (!string.IsNullOrWhiteSpace(CustomExtensionTextBox.Text))
                {
                    var customExts = CustomExtensionTextBox.Text
                        .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => e.Trim().StartsWith(".") ? e.Trim() : "." + e.Trim());
                    
                    selectedTypes.AddRange(customExts);
                }

                _tts?.SpeakAsync($"Searching for {selectedTypes.Count} file types. This may take a moment...");

                // Get search scope
                string? searchPath = GetSearchPath();
                bool includeSubfolders = IncludeSubfoldersCheckBox.IsChecked ?? true;

                // Perform search for each file type
                int totalFound = 0;
                foreach (var extension in selectedTypes)
                {
                    var results = await _fileManager.SearchFilesAsync(
                        searchPattern: $"*{extension}",
                        folderPath: searchPath,
                        includeSubfolders: includeSubfolders
                    );

                    foreach (var result in results)
                    {
                        _searchResults.Add(result);
                        totalFound++;
                    }

                    StatusTextBlock.Text = $"Found {totalFound} files so far...";
                }

                // Update UI
                StatusTextBlock.Text = $"Search complete! Found {totalFound} files.";
                ResultCountTextBlock.Text = $"Results: {totalFound} files";

                // Voice feedback
                if (totalFound > 0)
                {
                    _soundManager?.PlayPositiveFeedback();
                    _tts?.SpeakAsync($"Found {totalFound} files! They're displayed in the results grid.");
                }
                else
                {
                    _soundManager?.PlayErrorResponse();
                    _tts?.SpeakAsync("No files found matching your search criteria.");
                }
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error: {ex.Message}";
                _tts?.SpeakAsync("Oops! An error occurred during the search.");
                MessageBox.Show($"Search error: {ex.Message}", "Error");
            }
            finally
            {
                _isSearching = false;
                SearchButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Get selected file type extensions
        /// </summary>
        private List<string> GetSelectedFileTypes()
        {
            var selected = new List<string>();
            
            foreach (var item in FileTypeListBox.Items)
            {
                var container = FileTypeListBox.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                if (container != null)
                {
                    var checkbox = FindVisualChild<CheckBox>(container);
                    if (checkbox?.IsChecked == true)
                    {
                        var preset = item as FileTypePreset;
                        if (preset != null)
                        {
                            selected.Add(preset.Extension);
                        }
                    }
                }
            }

            return selected;
        }

        /// <summary>
        /// Get search path based on selection
        /// </summary>
        private string? GetSearchPath()
        {
            if (SearchSpecificPathCheckBox.IsChecked == true && !string.IsNullOrWhiteSpace(SpecificPathTextBox.Text))
            {
                return SpecificPathTextBox.Text;
            }

            // Search all approved folders
            return null;
        }

        /// <summary>
        /// Select All file types
        /// </summary>
        private void SelectAllTypes_Click(object sender, RoutedEventArgs e)
        {
            SetAllCheckboxes(true);
        }

        /// <summary>
        /// Deselect All file types
        /// </summary>
        private void DeselectAllTypes_Click(object sender, RoutedEventArgs e)
        {
            SetAllCheckboxes(false);
        }

        /// <summary>
        /// Set all checkboxes to a state
        /// </summary>
        private void SetAllCheckboxes(bool isChecked)
        {
            foreach (var item in FileTypeListBox.Items)
            {
                var container = FileTypeListBox.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                if (container != null)
                {
                    var checkbox = FindVisualChild<CheckBox>(container);
                    if (checkbox != null)
                    {
                        checkbox.IsChecked = isChecked;
                    }
                }
            }
        }

        /// <summary>
        /// Quick select by category
        /// </summary>
        private void QuickSelect_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var category = button?.Tag as string;

            if (string.IsNullOrEmpty(category))
                return;

            foreach (var item in FileTypeListBox.Items)
            {
                var preset = item as FileTypePreset;
                var container = FileTypeListBox.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                
                if (container != null && preset != null && preset.Category == category)
                {
                    var checkbox = FindVisualChild<CheckBox>(container);
                    if (checkbox != null)
                    {
                        checkbox.IsChecked = true;
                    }
                }
            }

            _tts?.SpeakAsync($"Selected all {category} file types");
        }

        #region Actions for Selected Files

        /// <summary>
        /// Open selected file
        /// </summary>
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var selected = ResultsDataGrid.SelectedItem as FileSearchResult;
            if (selected == null)
            {
                MessageBox.Show("Please select a file first!");
                return;
            }

            try
            {
                _fileManager.OpenFile(selected.FilePath);
                _tts?.SpeakAsync($"Opening {selected.FileName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}");
            }
        }

        /// <summary>
        /// Open folder containing selected file
        /// </summary>
        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            var selected = ResultsDataGrid.SelectedItem as FileSearchResult;
            if (selected == null)
            {
                MessageBox.Show("Please select a file first!");
                return;
            }

            try
            {
                Process.Start("explorer.exe", $"/select,\"{selected.FilePath}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Copy selected files to folder
        /// </summary>
        private void CopyFiles_Click(object sender, RoutedEventArgs e)
        {
            var selected = ResultsDataGrid.SelectedItems.Cast<FileSearchResult>().ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("Please select files to copy!");
                return;
            }

            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _tts?.SpeakAsync($"Copying {selected.Count} files...");
                
                int copied = 0;
                foreach (var file in selected)
                {
                    try
                    {
                        var destPath = Path.Combine(dialog.SelectedPath, file.FileName);
                        _fileManager.CopyFile(file.FilePath, destPath, overwrite: false);
                        copied++;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error copying {file.FileName}: {ex.Message}");
                    }
                }

                MessageBox.Show($"Copied {copied} of {selected.Count} files to:\n{dialog.SelectedPath}");
                _tts?.SpeakAsync($"Copy complete! {copied} files copied successfully.");
            }
        }

        /// <summary>
        /// Create playlist from selected music files
        /// </summary>
        private void CreatePlaylist_Click(object sender, RoutedEventArgs e)
        {
            var selected = ResultsDataGrid.SelectedItems.Cast<FileSearchResult>().ToList();
            
            // Filter to music files only
            var musicExtensions = new[] { ".mp3", ".wav", ".flac", ".m4a", ".aac", ".ogg" };
            var musicFiles = selected.Where(f => 
                musicExtensions.Contains(f.Extension.ToLower())).ToList();

            if (musicFiles.Count == 0)
            {
                MessageBox.Show("Please select music files to create a playlist!");
                return;
            }

            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "M3U Playlist|*.m3u|Text Playlist|*.txt",
                    DefaultExt = ".m3u",
                    FileName = $"Playlist_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var playlistContent = string.Join("\n", musicFiles.Select(f => f.FilePath));
                    File.WriteAllText(saveDialog.FileName, playlistContent);
                    
                    MessageBox.Show($"Playlist created with {musicFiles.Count} tracks!\n{saveDialog.FileName}");
                    _tts?.SpeakAsync($"Playlist created with {musicFiles.Count} tracks!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating playlist: {ex.Message}");
            }
        }

        /// <summary>
        /// Export results to file
        /// </summary>
        private void ExportResults_Click(object sender, RoutedEventArgs e)
        {
            if (_searchResults.Count == 0)
            {
                MessageBox.Show("No results to export!");
                return;
            }

            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV File|*.csv|Text File|*.txt",
                    DefaultExt = ".csv",
                    FileName = $"SearchResults_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var csv = "File Name,Extension,Size (MB),Path,Last Modified\n";
                    csv += string.Join("\n", _searchResults.Select(r =>
                        $"\"{r.FileName}\",{r.Extension},{r.SizeMB:F2},\"{r.FilePath}\",{r.LastModified:yyyy-MM-dd HH:mm}"));
                    
                    File.WriteAllText(saveDialog.FileName, csv);
                    MessageBox.Show($"Results exported!\n{saveDialog.FileName}");
                    _tts?.SpeakAsync("Results exported successfully!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Find visual child of type T
        /// </summary>
        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                
                if (child is T typedChild)
                    return typedChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Browse for specific folder
        /// </summary>
        private void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SpecificPathTextBox.Text = dialog.SelectedPath;
                SearchSpecificPathCheckBox.IsChecked = true;
                
                // Request approval for this folder
                _fileManager.AddApprovedFolder(dialog.SelectedPath);
                _tts?.SpeakAsync($"Folder approved for searching: {Path.GetFileName(dialog.SelectedPath)}");
            }
        }

        #endregion
    }

    /// <summary>
    /// File type preset for UI
    /// </summary>
    public class FileTypePreset
    {
        public string Extension { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
