using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Enhanced Candy-Bot File Manager for Desktop Mode
    /// Provides secure local file access, organization, and management
    /// </summary>
    public class CandyBotFileManager
    {
        private readonly List<string> _approvedFolders;
        private readonly List<string> _operationLog;
        private bool _loggingEnabled;

        public CandyBotFileManager()
        {
            _approvedFolders = new List<string>();
            _operationLog = new List<string>();
            _loggingEnabled = true;

            // Default approved folders (user must approve via UI)
            // None approved by default for security
        }

        #region Permission Management

        /// <summary>
        /// Request user permission to access a folder
        /// </summary>
        public void AddApprovedFolder(string folderPath)
        {
            if (Directory.Exists(folderPath) && !_approvedFolders.Contains(folderPath))
            {
                _approvedFolders.Add(folderPath);
                LogOperation($"Approved folder access: {folderPath}");
            }
        }

        /// <summary>
        /// Remove folder from approved list
        /// </summary>
        public void RemoveApprovedFolder(string folderPath)
        {
            if (_approvedFolders.Contains(folderPath))
            {
                _approvedFolders.Remove(folderPath);
                LogOperation($"Removed folder access: {folderPath}");
            }
        }

        /// <summary>
        /// Get list of approved folders
        /// </summary>
        public List<string> GetApprovedFolders()
        {
            return new List<string>(_approvedFolders);
        }

        /// <summary>
        /// Check if folder is approved for access
        /// </summary>
        private bool IsFolderApproved(string path)
        {
            return _approvedFolders.Any(approved => 
                path.StartsWith(approved, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region File Search

        /// <summary>
        /// Search for files in approved folders
        /// </summary>
        public Task<List<FileSearchResult>> SearchFilesAsync(
            string searchPattern = "*.*",
            string? fileExtension = null,
            string? folderPath = null,
            bool includeSubfolders = true)
        {
            var results = new List<FileSearchResult>();
            string pattern = searchPattern; // Declare at method scope

            try
            {
                var foldersToSearch = string.IsNullOrEmpty(folderPath)
                    ? _approvedFolders
                    : new List<string> { folderPath };

                foreach (var folder in foldersToSearch)
                {
                    if (!IsFolderApproved(folder))
                        continue;

                    var searchOption = includeSubfolders 
                        ? SearchOption.AllDirectories 
                        : SearchOption.TopDirectoryOnly;

                    pattern = fileExtension != null 
                        ? $"*.{fileExtension.TrimStart('.')}" 
                        : searchPattern;

                    var files = Directory.GetFiles(folder, pattern, searchOption);

                    foreach (var file in files)
                    {
                        results.Add(new FileSearchResult
                        {
                            FilePath = file,
                            FileName = Path.GetFileName(file),
                            Extension = Path.GetExtension(file),
                            SizeMB = new FileInfo(file).Length / (1024.0 * 1024.0),
                            LastModified = File.GetLastWriteTime(file)
                        });
                    }
                }

                LogOperation($"File search: {pattern} - Found {results.Count} files");
            }
            catch (Exception ex)
            {
                LogOperation($"Search error: {ex.Message}");
            }

            return Task.FromResult(results);
        }

        /// <summary>
        /// Search for specific content within text files
        /// </summary>
        public async Task<List<FileSearchResult>> SearchFileContentAsync(
            string searchText,
            string? folderPath = null,
            string[]? extensions = null)
        {
            extensions = extensions ?? new[] { ".txt", ".log", ".md", ".cs", ".xaml" };
            var results = new List<FileSearchResult>();

            try
            {
                var files = await SearchFilesAsync(folderPath: folderPath);
                
                foreach (var file in files.Where(f => extensions.Contains(f.Extension.ToLower())))
                {
                    try
                    {
                        var content = await File.ReadAllTextAsync(file.FilePath);
                        if (content.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                        {
                            file.MatchedContent = GetContentPreview(content, searchText);
                            results.Add(file);
                        }
                    }
                    catch { /* Skip files that can't be read */ }
                }

                LogOperation($"Content search: '{searchText}' - Found {results.Count} matches");
            }
            catch (Exception ex)
            {
                LogOperation($"Content search error: {ex.Message}");
            }

            return results;
        }

        #endregion

        #region File Operations

        /// <summary>
        /// Open a file with default application
        /// </summary>
        public bool OpenFile(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (directory == null || !IsFolderApproved(directory))
            {
                LogOperation($"Access denied: {filePath}");
                return false;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
                LogOperation($"Opened file: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                LogOperation($"Error opening file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Copy file to new location
        /// </summary>
        public bool CopyFile(string sourcePath, string destinationPath, bool overwrite = false)
        {
            var sourceDir = Path.GetDirectoryName(sourcePath);
            var destDir = Path.GetDirectoryName(destinationPath);
            
            if (sourceDir == null || destDir == null ||
                !IsFolderApproved(sourceDir) || !IsFolderApproved(destDir))
            {
                LogOperation($"Access denied for copy operation");
                return false;
            }

            try
            {
                File.Copy(sourcePath, destinationPath, overwrite);
                LogOperation($"Copied: {sourcePath} → {destinationPath}");
                return true;
            }
            catch (Exception ex)
            {
                LogOperation($"Copy error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Move file to new location
        /// </summary>
        public bool MoveFile(string sourcePath, string destinationPath)
        {
            var sourceDir = Path.GetDirectoryName(sourcePath);
            var destDir = Path.GetDirectoryName(destinationPath);
            
            if (sourceDir == null || destDir == null ||
                !IsFolderApproved(sourceDir) || !IsFolderApproved(destDir))
            {
                LogOperation($"Access denied for move operation");
                return false;
            }

            try
            {
                File.Move(sourcePath, destinationPath);
                LogOperation($"Moved: {sourcePath} → {destinationPath}");
                return true;
            }
            catch (Exception ex)
            {
                LogOperation($"Move error: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region DJ & Media Management

        /// <summary>
        /// Find all music files in approved folders
        /// </summary>
        public async Task<List<FileSearchResult>> FindMusicFilesAsync(string? folderPath = null)
        {
            var musicExtensions = new[] { "*.mp3", "*.wav", "*.flac", "*.m4a", "*.aac", "*.ogg" };
            var allMusicFiles = new List<FileSearchResult>();

            foreach (var ext in musicExtensions)
            {
                var files = await SearchFilesAsync(searchPattern: ext, folderPath: folderPath);
                allMusicFiles.AddRange(files);
            }

            LogOperation($"Found {allMusicFiles.Count} music files");
            return allMusicFiles;
        }

        /// <summary>
        /// Find DJ setlist files
        /// </summary>
        public async Task<List<FileSearchResult>> FindDJSetlistsAsync()
        {
            var setlistExtensions = new[] { "*.m3u", "*.pls", "*.txt", "*.pdf" };
            var results = new List<FileSearchResult>();

            foreach (var ext in setlistExtensions)
            {
                var files = await SearchFilesAsync(searchPattern: ext);
                results.AddRange(files.Where(f => 
                    f.FileName.Contains("setlist", StringComparison.OrdinalIgnoreCase) ||
                    f.FileName.Contains("playlist", StringComparison.OrdinalIgnoreCase)));
            }

            return results;
        }

        /// <summary>
        /// Organize files by type into folders
        /// </summary>
        public async Task<int> OrganizeFilesByTypeAsync(string sourceFolderPath)
        {
            if (!IsFolderApproved(sourceFolderPath))
                return 0;

            int filesOrganized = 0;

            try
            {
                var categoryFolders = new Dictionary<string, string[]>
                {
                    { "Music", new[] { ".mp3", ".wav", ".flac", ".m4a" } },
                    { "Documents", new[] { ".pdf", ".docx", ".txt", ".md" } },
                    { "Images", new[] { ".jpg", ".png", ".gif", ".bmp" } },
                    { "Videos", new[] { ".mp4", ".avi", ".mkv", ".mov" } }
                };

                foreach (var category in categoryFolders)
                {
                    var categoryPath = Path.Combine(sourceFolderPath, category.Key);
                    Directory.CreateDirectory(categoryPath);

                    foreach (var ext in category.Value)
                    {
                        // Fix parameter order: searchPattern, fileExtension, folderPath, includeSubfolders
                        var files = await SearchFilesAsync(
                            searchPattern: "*.*",
                            fileExtension: ext,
                            folderPath: sourceFolderPath,
                            includeSubfolders: false);
                        
                        foreach (var file in files)
                        {
                            var newPath = Path.Combine(categoryPath, file.FileName);
                            if (MoveFile(file.FilePath, newPath))
                                filesOrganized++;
                        }
                    }
                }

                LogOperation($"Organized {filesOrganized} files");
            }
            catch (Exception ex)
            {
                LogOperation($"Organization error: {ex.Message}");
            }

            return filesOrganized;
        }

        #endregion

        #region Document Analysis

        /// <summary>
        /// Read text file contents
        /// </summary>
        public async Task<string?> ReadTextFileAsync(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (directory == null || !IsFolderApproved(directory))
                return null;

            try
            {
                return await File.ReadAllTextAsync(filePath);
            }
            catch (Exception ex)
            {
                LogOperation($"Read error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get file statistics
        /// </summary>
        public async Task<FileStatistics> GetFileStatisticsAsync(string? folderPath = null)
        {
            var stats = new FileStatistics();
            var files = await SearchFilesAsync(folderPath: folderPath);

            stats.TotalFiles = files.Count;
            stats.TotalSizeMB = files.Sum(f => f.SizeMB);
            stats.FileTypeBreakdown = files.GroupBy(f => f.Extension)
                .ToDictionary(g => g.Key, g => g.Count());

            return stats;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get content preview around search match
        /// </summary>
        private string GetContentPreview(string content, string searchText, int contextLength = 100)
        {
            int index = content.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
            if (index == -1) return string.Empty;

            int start = Math.Max(0, index - contextLength);
            int end = Math.Min(content.Length, index + searchText.Length + contextLength);

            return $"...{content.Substring(start, end - start)}...";
        }

        /// <summary>
        /// Log an operation to audit trail
        /// </summary>
        private void LogOperation(string message)
        {
            if (!_loggingEnabled) return;
            
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            _operationLog.Add(logEntry);
            System.Diagnostics.Debug.WriteLine($"CandyBot FileManager: {logEntry}");
        }

        /// <summary>
        /// Get operation log
        /// </summary>
        public List<string> GetOperationLog() => new List<string>(_operationLog);

        /// <summary>
        /// Clear operation log
        /// </summary>
        public void ClearLog() => _operationLog.Clear();

        /// <summary>
        /// Enable/disable logging
        /// </summary>
        public void SetLogging(bool enabled) => _loggingEnabled = enabled;

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// File search result data
    /// </summary>
    public class FileSearchResult
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public double SizeMB { get; set; }
        public DateTime LastModified { get; set; }
        public string MatchedContent { get; set; } = string.Empty;
        public string Drive { get; set; } = string.Empty; // Drive letter (e.g., "C:", "D:")
        
        /// <summary>
        /// Display-friendly size string
        /// </summary>
        public string DisplaySize
        {
            get
            {
                if (SizeMB < 0.001)
                    return $"{SizeMB * 1024 * 1024:F0} bytes";
                else if (SizeMB < 1)
                    return $"{SizeMB * 1024:F1} KB";
                else if (SizeMB < 1024)
                    return $"{SizeMB:F2} MB";
                else
                    return $"{SizeMB / 1024:F2} GB";
            }
        }
    }

    /// <summary>
    /// File statistics data
    /// </summary>
    public class FileStatistics
    {
        public int TotalFiles { get; set; }
        public double TotalSizeMB { get; set; }
        public Dictionary<string, int> FileTypeBreakdown { get; set; } = new();
    }

    #endregion
}
