using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class MultiDriveSearchWindow : Window
    {
        private readonly MultiDriveFileSearcher _searcher;
        private readonly CandyBotTextToSpeech? _tts;
        private readonly ObservableCollection<FileSearchResult> _searchResults;
        private bool _isSearching = false;
        private string _candyBotDocumentsPath;
        private CancellationTokenSource? _cancellationTokenSource;
        
        private List<string> _selectedDrives = new List<string>();
        private string _selectedExtensions = ".mp3, .wav, .flac";
        private FileOperation? _pendingAction = null;
        private string _actionDestination = "";

        public MultiDriveSearchWindow()
        {
            InitializeComponent();

            _searcher = new MultiDriveFileSearcher();
            _tts = new CandyBotTextToSpeech();
            _searchResults = new ObservableCollection<FileSearchResult>();

            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _candyBotDocumentsPath = Path.Combine(documentsPath, "CandyBot-Documents");

            ResultsDataGrid.ItemsSource = _searchResults;

            _searcher.ProgressChanged += Searcher_ProgressChanged;
            _searcher.FileFound += Searcher_FileFound;
        }

        #region Dialog Methods

        private void SelectDrives_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DriveSelectionDialog(_searcher, _selectedDrives);
            if (dialog.ShowDialog() == true)
            {
                _selectedDrives = dialog.SelectedDrives;
                
                foreach (var drive in _selectedDrives)
                {
                    _searcher.ApproveDrive(drive);
                }
                
                DrivesLabel.Text = $"Drives: {string.Join(", ", _selectedDrives)} ({_selectedDrives.Count} selected)";
                StatusText.Text = $"Selected {_selectedDrives.Count} drive(s) for searching.";
                _ = _tts?.SpeakAsync($"Selected {_selectedDrives.Count} drives!");
            }
        }

        private void SelectExtensions_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ExtensionSelectionDialog(_selectedExtensions);
            if (dialog.ShowDialog() == true)
            {
                _selectedExtensions = dialog.Extensions;
                ExtensionsLabel.Text = $"Extensions: {_selectedExtensions}";
                StatusText.Text = "File extensions updated. Ready to search!";
            }
        }

        private async void StartSearch_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDrives.Count == 0)
            {
                MessageBox.Show("Please select at least one drive first!", "No Drives Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(_selectedExtensions))
            {
                MessageBox.Show("Please select file extensions first!", "No Extensions Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmDialog = new SearchConfirmationDialog(_selectedDrives, _selectedExtensions, _pendingAction, _actionDestination);
            if (confirmDialog.ShowDialog() != true)
            {
                return;
            }

            await PerformSearchAsync();
        }

        private async void FileActions_Click(object sender, RoutedEventArgs e)
        {
            var selectedFiles = ResultsDataGrid.SelectedItems.Cast<FileSearchResult>().ToList();
            
            if (selectedFiles.Count == 0)
            {
                MessageBox.Show("Please select files first!", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Let user choose the action first
                var actionDialog = new FileActionDialog(selectedFiles.Count, _actionDestination ?? "");
                if (actionDialog.ShowDialog() != true)
                    return;

                _pendingAction = actionDialog.SelectedOperation;
                
                // If copying or moving, ask for destination
                if (_pendingAction != FileOperation.Delete)
                {
                    var folderDialog = new System.Windows.Forms.FolderBrowserDialog
                    {
                        Description = $"Choose destination folder to {_pendingAction.ToString()?.ToLower() ?? "move"} files to:",
                        ShowNewFolderButton = true,
                        SelectedPath = _actionDestination ?? _candyBotDocumentsPath
                    };

                    if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        StatusText.Text = "Operation cancelled - no destination selected.";
                        return;
                    }

                    _actionDestination = folderDialog.SelectedPath;
                }
                
                await PerformFileOperationAsync(_pendingAction.Value, selectedFiles);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = $"Error: {ex.Message}";
            }
        }

        #endregion

        #region Search Operations

        private void SetDefaultDestinationPath(List<string> extensions)
        {
            try
            {
                string primaryExtension = extensions.FirstOrDefault() ?? ".files";
                string folderName = primaryExtension.TrimStart('.').ToUpper();
                
                folderName = folderName switch
                {
                    "MP3" or "WAV" or "FLAC" or "M4A" or "AAC" or "OGG" or "WMA" => "Music",
                    "JPG" or "JPEG" or "PNG" or "GIF" or "BMP" or "SVG" or "WEBP" => "Images",
                    "MP4" or "AVI" or "MKV" or "MOV" or "WMV" or "FLV" or "WEBM" => "Videos",
                    "PDF" or "DOCX" or "DOC" or "TXT" or "XLSX" or "XLS" or "PPTX" or "PPT" => "Documents",
                    "ZIP" or "RAR" or "7Z" or "TAR" or "GZ" or "BZ2" => "Archives",
                    "CS" or "JS" or "PY" or "JAVA" or "CPP" or "H" or "HTML" or "CSS" or "SQL" => "Code",
                    _ => folderName
                };

                string destinationPath = Path.Combine(_candyBotDocumentsPath, folderName);
                
                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                }

                _actionDestination = destinationPath;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Note: Could not set default destination - {ex.Message}";
            }
        }

        private async Task PerformSearchAsync()
        {
            if (_isSearching)
            {
                MessageBox.Show("Search already in progress!", "Busy", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _isSearching = true;
            _searchResults.Clear();
            SearchProgressBar.Visibility = Visibility.Visible;
            SearchProgressBar.IsIndeterminate = true;

            ExportButton.IsEnabled = false;
            ClearButton.IsEnabled = false;
            FileActionsButton.IsEnabled = false;

            try
            {
                var extensions = _selectedExtensions
                    .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(ext => ext.Trim())
                    .Select(ext => ext.StartsWith(".") ? ext : "." + ext)
                    .ToList();

                // DO NOT auto-set destination - user chooses after seeing results
                
                StatusText.Text = $"Searching for {extensions.Count} file type(s) across {_selectedDrives.Count} drive(s)...";
                if (_tts != null)
                {
                    await _tts.SpeakAsync($"Searching for {extensions.Count} file types!");
                }

                var results = await _searcher.SearchAllDrivesAsync(extensions);

                foreach (var result in results)
                {
                    result.Drive = Path.GetPathRoot(result.FilePath)?.TrimEnd('\\') ?? "?";
                    _searchResults.Add(result);
                }

                ResultsCountText.Text = $"Results: {_searchResults.Count} files";
                StatusText.Text = $"Search complete! Found {_searchResults.Count} files.";

                if (_tts != null)
                {
                    await _tts.SpeakAsync($"Search complete! Found {_searchResults.Count} files!");
                }

                if (_searchResults.Count == 0)
                {
                    MessageBox.Show("No files found matching your criteria.", "No Results", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ExportButton.IsEnabled = true;
                    ClearButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Search failed: {ex.Message}";
                MessageBox.Show($"Search error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (_tts != null)
                {
                    await _tts.SayErrorAsync();
                }
            }
            finally
            {
                _isSearching = false;
                SearchProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Event Handlers

        private void Searcher_ProgressChanged(object? sender, SearchProgressEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = $"{e.Message}";
            });
        }

        private void Searcher_FileFound(object? sender, FileFoundEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                e.File.Drive = Path.GetPathRoot(e.File.FilePath)?.TrimEnd('\\') ?? "?";
                _searchResults.Add(e.File);
                ResultsCountText.Text = $"Results: {_searchResults.Count} files";
            });
        }

        #endregion

        #region Results Operations

        private void ExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            if (_searchResults.Count == 0)
            {
                MessageBox.Show("No results to export!", "Empty", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = $"CandyBot_Search_Results_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (dialog.ShowDialog() == true)
                {
                    var csv = new StringBuilder();
                    csv.AppendLine("FileName,Extension,Size,Drive,LastModified,FilePath");

                    foreach (var result in _searchResults)
                    {
                        csv.AppendLine(
                            $"\"{result.FileName}\",{result.Extension},{result.DisplaySize},{result.Drive},{result.LastModified:yyyy-MM-dd HH:mm},\"{result.FilePath}\"");
                    }

                    File.WriteAllText(dialog.FileName, csv.ToString());
                    
                    StatusText.Text = $"Exported {_searchResults.Count} results to CSV!";
                    _ = _tts?.SaySuccessAsync();

                    MessageBox.Show(
                        $"Successfully exported {_searchResults.Count} results!\n\nFile saved to:\n{dialog.FileName}",
                        "Export Complete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearResults_Click(object sender, RoutedEventArgs e)
        {
            if (_searchResults.Count > 0)
            {
                var result = MessageBox.Show(
                    $"Clear {_searchResults.Count} search results?",
                    "Confirm Clear",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _searchResults.Clear();
                    ResultsCountText.Text = "Results: 0 files";
                    StatusText.Text = "Results cleared. Ready for new search!";
                    ExportButton.IsEnabled = false;
                    ClearButton.IsEnabled = false;
                    FileActionsButton.IsEnabled = false;
                }
            }
        }

        #endregion

        #region Context Menu Actions

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (ResultsDataGrid.SelectedItem is FileSearchResult result)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = result.FilePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (ResultsDataGrid.SelectedItem is FileSearchResult result)
            {
                try
                {
                    string? folder = Path.GetDirectoryName(result.FilePath);
                    if (!string.IsNullOrEmpty(folder))
                    {
                        Process.Start("explorer.exe", folder);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CopyPath_Click(object sender, RoutedEventArgs e)
        {
            if (ResultsDataGrid.SelectedItem is FileSearchResult result)
            {
                try
                {
                    Clipboard.SetText(result.FilePath);
                    StatusText.Text = "Path copied to clipboard!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error copying path: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CopyName_Click(object sender, RoutedEventArgs e)
        {
            if (ResultsDataGrid.SelectedItem is FileSearchResult result)
            {
                try
                {
                    Clipboard.SetText(result.FileName);
                    StatusText.Text = "File name copied to clipboard!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error copying name: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Selection

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            ResultsDataGrid.SelectAll();
            StatusText.Text = $"Selected all {_searchResults.Count} files!";
        }

        private void SelectNone_Click(object sender, RoutedEventArgs e)
        {
            ResultsDataGrid.SelectedItems.Clear();
            StatusText.Text = "Selection cleared.";
        }

        private void ResultsDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedCount = ResultsDataGrid.SelectedItems.Count;
            SelectionCountText.Text = $"({selectedCount} selected)";

            FileActionsButton.IsEnabled = selectedCount > 0;
        }

        #endregion

        #region File Operations

        private async Task PerformFileOperationAsync(FileOperation operation, List<FileSearchResult> selectedFiles)
        {
            string? destinationPath = _actionDestination;

            // Validate destination for copy/move
            if (operation != FileOperation.Delete)
            {
                if (string.IsNullOrEmpty(destinationPath))
                {
                    MessageBox.Show("Please set a destination folder!", "Destination Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!Directory.Exists(destinationPath))
                {
                    var createResult = MessageBox.Show(
                        $"Destination folder does not exist:\n{destinationPath}\n\nCreate it now?",
                        "Create Folder?",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (createResult == MessageBoxResult.Yes)
                    {
                        try
                        {
                            Directory.CreateDirectory(destinationPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to create destination folder:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }

            // Confirmation dialog
            string operationName = operation.ToString();
            string confirmMessage = operation == FileOperation.Delete
                ? $"Are you sure you want to DELETE {selectedFiles.Count} file(s)?\n\nThis operation cannot be undone!"
                : $"Are you sure you want to {operationName.ToUpper()} {selectedFiles.Count} file(s) to:\n{destinationPath}";

            var confirmation = MessageBox.Show(confirmMessage, $"Confirm {operationName}", MessageBoxButton.YesNo,
                operation == FileOperation.Delete ? MessageBoxImage.Warning : MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            // Setup cancellation and progress
            _cancellationTokenSource = new CancellationTokenSource();
            SearchProgressBar.Visibility = Visibility.Visible;
            SearchProgressBar.IsIndeterminate = false;
            SearchProgressBar.Maximum = selectedFiles.Count;
            SearchProgressBar.Value = 0;
            
            // Show cancel button
            CancelButton.Visibility = Visibility.Visible;
            CancelButton.IsEnabled = true;

            int successCount = 0;
            int failCount = 0;
            var errors = new List<string>();

            try
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < selectedFiles.Count; i++)
                    {
                        // Check for cancellation
                        if (_cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            Dispatcher.Invoke(() => StatusText.Text = "Operation cancelled by user.");
                            break;
                        }

                        var file = selectedFiles[i];
                        
                        Dispatcher.Invoke(() =>
                        {
                            StatusText.Text = $"{operationName}ing ({i + 1}/{selectedFiles.Count}): {file.FileName}...";
                            SearchProgressBar.Value = i + 1;
                        });

                        try
                        {
                            // Perform the operation using Windows file operations
                            if (operation == FileOperation.Delete)
                            {
                                File.Delete(file.FilePath);
                            }
                            else
                            {
                                string targetFile = Path.Combine(destinationPath!, file.FileName);

                                // Handle duplicate filenames
                                if (File.Exists(targetFile))
                                {
                                    string nameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
                                    string ext = Path.GetExtension(file.FileName);
                                    int counter = 1;

                                    do
                                    {
                                        targetFile = Path.Combine(destinationPath!, $"{nameWithoutExt} ({counter}){ext}");
                                        counter++;
                                    }
                                    while (File.Exists(targetFile));
                                }

                                if (operation == FileOperation.Copy)
                                {
                                    File.Copy(file.FilePath, targetFile, false);
                                }
                                else // Move
                                {
                                    File.Move(file.FilePath, targetFile, false);
                                }
                            }

                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            failCount++;
                            errors.Add($"{file.FileName}: {ex.Message}");
                        }
                    }
                }, _cancellationTokenSource.Token);

                // Show results
                var resultMessage = new StringBuilder();
                resultMessage.AppendLine($"{operationName} Operation Complete!");
                resultMessage.AppendLine();
                resultMessage.AppendLine($"✓ Successful: {successCount}");
                
                if (failCount > 0)
                {
                    resultMessage.AppendLine($"✗ Failed: {failCount}");
                    resultMessage.AppendLine();
                    resultMessage.AppendLine("Errors:");
                    foreach (var error in errors.Take(10))
                    {
                        resultMessage.AppendLine($"  • {error}");
                    }
                    if (errors.Count > 10)
                        resultMessage.AppendLine($"  ... and {errors.Count - 10} more");
                }

                if (operation != FileOperation.Delete && successCount > 0)
                {
                    resultMessage.AppendLine();
                    resultMessage.AppendLine($"Location: {destinationPath}");
                }

                MessageBox.Show(resultMessage.ToString(), $"{operationName} Complete", 
                    MessageBoxButton.OK, failCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);

                StatusText.Text = $"{operationName} complete! {successCount} succeeded, {failCount} failed.";
                if (_tts != null)
                {
                    await _tts.SaySuccessAsync();
                }

                // Refresh results if files were moved or deleted
                if (operation != FileOperation.Copy && successCount > 0)
                {
                    foreach (var file in selectedFiles.Where(f => !errors.Any(e => e.Contains(f.FileName))))
                    {
                        _searchResults.Remove(file);
                    }
                    ResultsCountText.Text = $"Results: {_searchResults.Count} files";
                }
            }
            catch (OperationCanceledException)
            {
                StatusText.Text = $"Operation cancelled. {successCount} files processed before cancellation.";
                MessageBox.Show($"Operation was cancelled.\n\n{successCount} files were successfully {operationName.ToLower()}ed before cancellation.",
                    "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Operation error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = $"Error during {operationName.ToLower()}: {ex.Message}";
            }
            finally
            {
                SearchProgressBar.Visibility = Visibility.Collapsed;
                CancelButton.Visibility = Visibility.Collapsed;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private string GetRelativePath(string fullPath)
        {
            // Try to extract relative path from Documents folder
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            
            if (fullPath.StartsWith(documentsPath, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(documentsPath.Length).TrimStart('\\', '/');
            }

            // Try from Users\[Username] folder
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (fullPath.StartsWith(userProfile, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(userProfile.Length).TrimStart('\\', '/');
            }

            // Fall back to just the folder name
            return Path.GetFileName(fullPath) ?? "Files";
        }

        #endregion

        #region Cancel Operation

        private void CancelOperation_Click(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to cancel the current operation?\n\nAny files already processed will remain completed.",
                    "Confirm Cancel",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _cancellationTokenSource.Cancel();
                    StatusText.Text = "Cancelling operation...";
                    CancelButton.IsEnabled = false;
                }
            }
        }

        #endregion

        #region Window Controls

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximize();
            }
            else
            {
                try
                {
                    DragMove();
                }
                catch { }
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void ToggleMaximize()
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                MaximizeButton.Content = "□";
                MaximizeButton.ToolTip = "Maximize";
            }
            else
            {
                WindowState = WindowState.Maximized;
                MaximizeButton.Content = "❐";
                MaximizeButton.ToolTip = "Restore";
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _searcher.ProgressChanged -= Searcher_ProgressChanged;
            _searcher.FileFound -= Searcher_FileFound;
            _tts?.Dispose();
            base.OnClosed(e);
        }

        #endregion
    }
}
