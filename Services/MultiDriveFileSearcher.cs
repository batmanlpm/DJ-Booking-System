using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Multi-Drive File Search Service
    /// Searches ALL accessible drives for files by extension
    /// User-friendly with progress updates and safety checks
    /// </summary>
    public class MultiDriveFileSearcher
    {
        private bool _isSearching;
        private int _totalFilesFound;
        private List<string> _approvedDrives;

        public event EventHandler<SearchProgressEventArgs>? ProgressChanged;
        public event EventHandler<FileFoundEventArgs>? FileFound;

        public MultiDriveFileSearcher()
        {
            _approvedDrives = new List<string>();
            GetAvailableDrives(); // Auto-detect drives on startup
        }

        /// <summary>
        /// Get all available drives on the system
        /// </summary>
        public List<DriveSearchInfo> GetAvailableDrives()
        {
            var drives = new List<DriveSearchInfo>();
            
            foreach (var drive in DriveInfo.GetDrives())
            {
                try
                {
                    if (drive.IsReady)
                    {
                        drives.Add(new DriveSearchInfo
                        {
                            DriveLetter = drive.Name,
                            DriveType = drive.DriveType.ToString(),
                            TotalSizeGB = drive.TotalSize / (1024.0 * 1024.0 * 1024.0),
                            FreeSpaceGB = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0),
                            VolumeLabel = drive.VolumeLabel,
                            IsApproved = _approvedDrives.Contains(drive.Name)
                        });
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MultiDrive] Cannot access {drive.Name}: {ex.Message}");
                }
            }

            return drives;
        }

        /// <summary>
        /// Alias for GetAvailableDrives() - for compatibility
        /// </summary>
        public List<DriveSearchInfo> GetAllDrives()
        {
            return GetAvailableDrives();
        }

        /// <summary>
        /// Approve a drive for searching
        /// </summary>
        public void ApproveDrive(string driveLetter)
        {
            if (!_approvedDrives.Contains(driveLetter))
            {
                _approvedDrives.Add(driveLetter);
                Debug.WriteLine($"[MultiDrive] Approved drive: {driveLetter}");
            }
        }

        /// <summary>
        /// Remove drive approval
        /// </summary>
        public void RemoveDriveApproval(string driveLetter)
        {
            _approvedDrives.Remove(driveLetter);
            Debug.WriteLine($"[MultiDrive] Removed approval for: {driveLetter}");
        }

        /// <summary>
        /// Approve ALL available drives (use with caution)
        /// </summary>
        public void ApproveAllDrives()
        {
            _approvedDrives.Clear();
            var drives = GetAvailableDrives();
            
            foreach (var drive in drives)
            {
                _approvedDrives.Add(drive.DriveLetter);
            }
            
            Debug.WriteLine($"[MultiDrive] Approved {_approvedDrives.Count} drives for searching");
        }

        /// <summary>
        /// MAIN SEARCH METHOD
        /// Search all approved drives for files matching extensions
        /// </summary>
        /// <param name="extensions">File extensions to search for (e.g., ".txt", ".mp3", ".docx")</param>
        /// <param name="includeSubfolders">Search subfolders (default: true)</param>
        /// <param name="excludeFolders">Folders to skip (e.g., Windows, System32)</param>
        public async Task<List<FileSearchResult>> SearchAllDrivesAsync(
            List<string> extensions,
            bool includeSubfolders = true,
            List<string>? excludeFolders = null)
        {
            if (_isSearching)
            {
                throw new InvalidOperationException("Search already in progress!");
            }

            _isSearching = true;
            _totalFilesFound = 0;
            var results = new List<FileSearchResult>();

            // Default excluded folders
            if (excludeFolders == null)
            {
                excludeFolders = new List<string>
                {
                    "Windows", "System32", "$Recycle.Bin", "ProgramData",
                    "Program Files", "Program Files (x86)", "AppData"
                };
            }

            try
            {
                RaiseProgress($"Starting search for {extensions.Count} file type(s) across {_approvedDrives.Count} drive(s)...");

                foreach (var driveLetter in _approvedDrives)
                {
                    try
                    {
                        RaiseProgress($"Searching drive {driveLetter}...");

                        foreach (var extension in extensions)
                        {
                            var driveResults = await SearchDriveAsync(
                                driveLetter,
                                extension,
                                includeSubfolders,
                                excludeFolders);

                            results.AddRange(driveResults);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[MultiDrive] Error searching {driveLetter}: {ex.Message}");
                        RaiseProgress($"Skipped {driveLetter} - {ex.Message}");
                    }
                }

                RaiseProgress($"Search complete! Found {_totalFilesFound} files total.");
            }
            finally
            {
                _isSearching = false;
            }

            return results;
        }

        /// <summary>
        /// Search a single drive for files
        /// </summary>
        private async Task<List<FileSearchResult>> SearchDriveAsync(
            string driveLetter,
            string extension,
            bool includeSubfolders,
            List<string> excludeFolders)
        {
            var results = new List<FileSearchResult>();

            await Task.Run(() =>
            {
                try
                {
                    var searchOption = includeSubfolders ? 
                        SearchOption.AllDirectories : 
                        SearchOption.TopDirectoryOnly;

                    // Ensure extension has dot prefix
                    if (!extension.StartsWith("."))
                        extension = "." + extension;

                    var files = Directory.EnumerateFiles(
                        driveLetter,
                        $"*{extension}",
                        new EnumerationOptions
                        {
                            RecurseSubdirectories = includeSubfolders,
                            IgnoreInaccessible = true,
                            AttributesToSkip = FileAttributes.System | FileAttributes.Hidden
                        });

                    foreach (var filePath in files)
                    {
                        try
                        {
                            // Skip excluded folders
                            if (IsInExcludedFolder(filePath, excludeFolders))
                                continue;

                            var fileInfo = new FileInfo(filePath);
                            
                            var result = new FileSearchResult
                            {
                                FileName = fileInfo.Name,
                                FilePath = fileInfo.FullName,
                                Extension = fileInfo.Extension,
                                SizeMB = fileInfo.Length / (1024.0 * 1024.0),
                                LastModified = fileInfo.LastWriteTime
                            };

                            results.Add(result);
                            _totalFilesFound++;

                            // Raise event for real-time updates
                            RaiseFileFound(result);

                            // Update progress every 10 files
                            if (_totalFilesFound % 10 == 0)
                            {
                                RaiseProgress($"Found {_totalFilesFound} files so far...");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[MultiDrive] Error accessing {filePath}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MultiDrive] Error in {driveLetter}: {ex.Message}");
                }
            });

            return results;
        }

        /// <summary>
        /// Check if file is in an excluded folder
        /// </summary>
        private bool IsInExcludedFolder(string filePath, List<string> excludeFolders)
        {
            foreach (var excludedFolder in excludeFolders)
            {
                if (filePath.Contains($"\\{excludedFolder}\\", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Quick search for specific file types
        /// </summary>
        public async Task<List<FileSearchResult>> QuickSearchAsync(string fileTypeCategory)
        {
            List<string> extensions = fileTypeCategory.ToLower() switch
            {
                "documents" => new List<string> { ".txt", ".docx", ".pdf", ".xlsx", ".doc" },
                "music" => new List<string> { ".mp3", ".wav", ".flac", ".m4a", ".aac" },
                "images" => new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".bmp" },
                "videos" => new List<string> { ".mp4", ".avi", ".mkv", ".mov", ".wmv" },
                "code" => new List<string> { ".cs", ".py", ".js", ".html", ".css", ".cpp" },
                _ => new List<string>()
            };

            if (extensions.Count == 0)
            {
                throw new ArgumentException($"Unknown file type category: {fileTypeCategory}");
            }

            return await SearchAllDrivesAsync(extensions, includeSubfolders: true);
        }

        /// <summary>
        /// Raise progress event
        /// </summary>
        private void RaiseProgress(string message)
        {
            ProgressChanged?.Invoke(this, new SearchProgressEventArgs
            {
                Message = message,
                TotalFilesFound = _totalFilesFound
            });
        }

        /// <summary>
        /// Raise file found event
        /// </summary>
        private void RaiseFileFound(FileSearchResult file)
        {
            FileFound?.Invoke(this, new FileFoundEventArgs { File = file });
        }
    }

    #region Supporting Classes

    /// <summary>
    /// Drive information for UI
    /// </summary>
    public class DriveSearchInfo
    {
        public string DriveLetter { get; set; } = string.Empty;
        public string DriveType { get; set; } = string.Empty;
        public double TotalSizeGB { get; set; }
        public double FreeSpaceGB { get; set; }
        public string VolumeLabel { get; set; } = string.Empty;
        public bool IsApproved { get; set; }

        public string DisplayName => 
            $"{DriveLetter} ({VolumeLabel}) - {DriveType} - {TotalSizeGB:F1} GB Total";
    }

    /// <summary>
    /// Search progress event args
    /// </summary>
    public class SearchProgressEventArgs : EventArgs
    {
        public string Message { get; set; } = string.Empty;
        public int TotalFilesFound { get; set; }
    }

    /// <summary>
    /// File found event args
    /// </summary>
    public class FileFoundEventArgs : EventArgs
    {
        public FileSearchResult File { get; set; } = new();
    }

    #endregion
}
