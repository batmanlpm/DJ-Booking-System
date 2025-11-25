using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// File organizer service for Candy-Bot
    /// Auto-sorts files by type, date, and removes duplicates
    /// </summary>
    public class FileOrganizerService
    {
        private readonly Dictionary<string, string> _fileCategories = new()
        {
            // Documents
            [".pdf"] = "Documents",
            [".doc"] = "Documents",
            [".docx"] = "Documents",
            [".txt"] = "Documents",
            [".rtf"] = "Documents",
            [".odt"] = "Documents",
            
            // Images
            [".jpg"] = "Images",
            [".jpeg"] = "Images",
            [".png"] = "Images",
            [".gif"] = "Images",
            [".bmp"] = "Images",
            [".svg"] = "Images",
            [".webp"] = "Images",
            
            // Videos
            [".mp4"] = "Videos",
            [".avi"] = "Videos",
            [".mkv"] = "Videos",
            [".mov"] = "Videos",
            [".wmv"] = "Videos",
            [".flv"] = "Videos",
            
            // Audio
            [".mp3"] = "Audio",
            [".wav"] = "Audio",
            [".flac"] = "Audio",
            [".aac"] = "Audio",
            [".m4a"] = "Audio",
            [".ogg"] = "Audio",
            
            // Archives
            [".zip"] = "Archives",
            [".rar"] = "Archives",
            [".7z"] = "Archives",
            [".tar"] = "Archives",
            [".gz"] = "Archives",
            
            // Code
            [".cs"] = "Code",
            [".js"] = "Code",
            [".py"] = "Code",
            [".java"] = "Code",
            [".cpp"] = "Code",
            [".html"] = "Code",
            [".css"] = "Code",
            [".xaml"] = "Code",
            
            // Executables
            [".exe"] = "Programs",
            [".msi"] = "Programs",
            [".dmg"] = "Programs",
            [".app"] = "Programs"
        };

        public class OrganizeResult
        {
            public int FilesOrganized { get; set; }
            public int DuplicatesRemoved { get; set; }
            public int FoldersCreated { get; set; }
            public List<string> Errors { get; set; } = new();
            public Dictionary<string, int> FilesByCategory { get; set; } = new();
        }

        /// <summary>
        /// Organize files in a directory
        /// </summary>
        public async Task<OrganizeResult> OrganizeDirectoryAsync(string sourcePath, bool removeDuplicates = true, bool organizeByDate = false)
        {
            var result = new OrganizeResult();

            if (!Directory.Exists(sourcePath))
            {
                result.Errors.Add($"Directory not found: {sourcePath}");
                return result;
            }

            try
            {
                var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.TopDirectoryOnly);
                var duplicateHashes = new Dictionary<string, string>();

                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        var extension = fileInfo.Extension.ToLower();

                        // Check for duplicates
                        if (removeDuplicates)
                        {
                            var hash = await CalculateFileHashAsync(file);
                            if (duplicateHashes.ContainsKey(hash))
                            {
                                File.Delete(file);
                                result.DuplicatesRemoved++;
                                continue;
                            }
                            duplicateHashes[hash] = file;
                        }

                        // Determine category
                        var category = _fileCategories.ContainsKey(extension) 
                            ? _fileCategories[extension] 
                            : "Other";

                        // Create category folder
                        var categoryPath = Path.Combine(sourcePath, category);
                        if (!Directory.Exists(categoryPath))
                        {
                            Directory.CreateDirectory(categoryPath);
                            result.FoldersCreated++;
                        }

                        // Organize by date if requested
                        string targetPath;
                        if (organizeByDate)
                        {
                            var dateFolder = fileInfo.LastWriteTime.ToString("yyyy-MM");
                            var datePath = Path.Combine(categoryPath, dateFolder);
                            if (!Directory.Exists(datePath))
                            {
                                Directory.CreateDirectory(datePath);
                                result.FoldersCreated++;
                            }
                            targetPath = datePath;
                        }
                        else
                        {
                            targetPath = categoryPath;
                        }

                        // Move file
                        var destinationFile = Path.Combine(targetPath, fileInfo.Name);
                        
                        // Handle name conflicts
                        if (File.Exists(destinationFile))
                        {
                            var counter = 1;
                            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileInfo.Name);
                            while (File.Exists(destinationFile))
                            {
                                destinationFile = Path.Combine(targetPath, $"{nameWithoutExt} ({counter}){extension}");
                                counter++;
                            }
                        }

                        File.Move(file, destinationFile);
                        result.FilesOrganized++;

                        // Track by category
                        if (!result.FilesByCategory.ContainsKey(category))
                            result.FilesByCategory[category] = 0;
                        result.FilesByCategory[category]++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Error processing {Path.GetFileName(file)}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error organizing directory: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Calculate MD5 hash for duplicate detection
        /// </summary>
        private async Task<string> CalculateFileHashAsync(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = await Task.Run(() => md5.ComputeHash(stream));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Find duplicate files in directory
        /// </summary>
        public async Task<Dictionary<string, List<string>>> FindDuplicatesAsync(string sourcePath)
        {
            var duplicates = new Dictionary<string, List<string>>();
            var hashes = new Dictionary<string, List<string>>();

            if (!Directory.Exists(sourcePath))
                return duplicates;

            var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                try
                {
                    var hash = await CalculateFileHashAsync(file);
                    if (!hashes.ContainsKey(hash))
                        hashes[hash] = new List<string>();
                    hashes[hash].Add(file);
                }
                catch
                {
                    // Skip files that can't be read
                }
            }

            // Filter to only duplicates
            foreach (var kvp in hashes.Where(x => x.Value.Count > 1))
            {
                duplicates[kvp.Key] = kvp.Value;
            }

            return duplicates;
        }

        /// <summary>
        /// Clean empty folders
        /// </summary>
        public int CleanEmptyFolders(string sourcePath)
        {
            var count = 0;
            try
            {
                var directories = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)
                    .OrderByDescending(d => d.Length); // Process deepest first

                foreach (var dir in directories)
                {
                    try
                    {
                        if (!Directory.EnumerateFileSystemEntries(dir).Any())
                        {
                            Directory.Delete(dir);
                            count++;
                        }
                    }
                    catch
                    {
                        // Skip folders that can't be deleted
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

            return count;
        }

        /// <summary>
        /// Get storage statistics
        /// </summary>
        public Dictionary<string, long> GetStorageStats(string sourcePath)
        {
            var stats = new Dictionary<string, long>();

            if (!Directory.Exists(sourcePath))
                return stats;

            try
            {
                var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        var extension = fileInfo.Extension.ToLower();
                        var category = _fileCategories.ContainsKey(extension) 
                            ? _fileCategories[extension] 
                            : "Other";

                        if (!stats.ContainsKey(category))
                            stats[category] = 0;

                        stats[category] += fileInfo.Length;
                    }
                    catch
                    {
                        // Skip files that can't be accessed
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

            return stats;
        }
    }
}
