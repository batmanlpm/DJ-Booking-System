using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Enhanced file organizer with media metadata support
    /// </summary>
    public class EnhancedFileOrganizerService
    {
        private readonly FileOrganizerService _baseOrganizer;
        private readonly VideoMetadataService _videoService;
        private readonly S3UploadService _s3Service;

        public EnhancedFileOrganizerService(
            VideoMetadataService videoService = null,
            S3UploadService s3Service = null)
        {
            _baseOrganizer = new FileOrganizerService();
            _videoService = videoService ?? new VideoMetadataService();
            _s3Service = s3Service;
        }

        public class MediaFileInfo
        {
            public string FilePath { get; set; }
            public string FileName { get; set; }
            public long FileSize { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public string Category { get; set; }
            public Dictionary<string, string> Tags { get; set; } = new();
            
            // Video metadata
            public TimeSpan? Duration { get; set; }
            public int? Width { get; set; }
            public int? Height { get; set; }
            public string Resolution => Width.HasValue && Height.HasValue ? $"{Width}x{Height}" : null;
            public double? FrameRate { get; set; }
            public string VideoCodec { get; set; }
            public string S3Url { get; set; }
        }

        /// <summary>
        /// Organize with metadata extraction
        /// </summary>
        public async Task<List<MediaFileInfo>> OrganizeWithMetadataAsync(
            string sourcePath,
            bool sortByResolution = false,
            bool sortByDuration = false,
            bool extractMetadata = true,
            bool uploadToS3 = false,
            IProgress<string> progress = null)
        {
            var mediaFiles = new List<MediaFileInfo>();
            var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.TopDirectoryOnly);
            var videoExtensions = new[] { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv" };

            foreach (var file in files)
            {
                progress?.Report($"Processing {Path.GetFileName(file)}...");
                
                var info = new FileInfo(file);
                var mediaFile = new MediaFileInfo
                {
                    FilePath = file,
                    FileName = info.Name,
                    FileSize = info.Length,
                    CreatedDate = info.CreationTime,
                    ModifiedDate = info.LastWriteTime,
                    Category = GetCategory(info.Extension)
                };

                // Extract video metadata
                if (extractMetadata && videoExtensions.Contains(info.Extension.ToLower()))
                {
                    try
                    {
                        var metadata = await _videoService.GetMetadataAsync(file);
                        mediaFile.Duration = metadata.Duration;
                        mediaFile.Width = metadata.Width;
                        mediaFile.Height = metadata.Height;
                        mediaFile.FrameRate = metadata.FrameRate;
                        mediaFile.VideoCodec = metadata.VideoCodec;
                    }
                    catch { /* Skip if metadata extraction fails */ }
                }

                // Upload to S3 if requested
                if (uploadToS3 && _s3Service != null)
                {
                    try
                    {
                        var s3Key = $"media/{DateTime.Now:yyyy-MM}/{info.Name}";
                        mediaFile.S3Url = await _s3Service.UploadFileAsync(file, s3Key);
                    }
                    catch { /* Skip if upload fails */ }
                }

                mediaFiles.Add(mediaFile);
            }

            // Sort if requested
            if (sortByResolution && mediaFiles.Any(m => m.Width.HasValue))
            {
                await SortByResolutionAsync(sourcePath, mediaFiles);
            }
            else if (sortByDuration && mediaFiles.Any(m => m.Duration.HasValue))
            {
                await SortByDurationAsync(sourcePath, mediaFiles);
            }

            return mediaFiles;
        }

        /// <summary>
        /// Sort videos by resolution into folders
        /// </summary>
        private Task SortByResolutionAsync(string basePath, List<MediaFileInfo> files)
        {
            var resolutionGroups = files
                .Where(f => f.Width.HasValue && f.Height.HasValue)
                .GroupBy(f => GetResolutionCategory(f.Width!.Value, f.Height!.Value));

            foreach (var group in resolutionGroups)
            {
                var folderPath = Path.Combine(basePath, "Videos", group.Key);
                Directory.CreateDirectory(folderPath);

                foreach (var file in group)
                {
                    var destPath = Path.Combine(folderPath, file.FileName);
                    if (File.Exists(file.FilePath))
                        File.Move(file.FilePath, destPath, true);
                }
            }
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sort videos by duration into folders
        /// </summary>
        private Task SortByDurationAsync(string basePath, List<MediaFileInfo> files)
        {
            var durationGroups = files
                .Where(f => f.Duration.HasValue)
                .GroupBy(f => GetDurationCategory(f.Duration!.Value));

            foreach (var group in durationGroups)
            {
                var folderPath = Path.Combine(basePath, "Videos", group.Key);
                Directory.CreateDirectory(folderPath);

                foreach (var file in group)
                {
                    var destPath = Path.Combine(folderPath, file.FileName);
                    if (File.Exists(file.FilePath))
                        File.Move(file.FilePath, destPath, true);
                }
            }
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Tag files with custom metadata
        /// </summary>
        public void TagFile(string filePath, Dictionary<string, string> tags)
        {
            // Store tags in extended attributes or separate JSON file
            var tagFilePath = $"{filePath}.tags.json";
            var json = System.Text.Json.JsonSerializer.Serialize(tags);
            File.WriteAllText(tagFilePath, json);
        }

        /// <summary>
        /// Get tags for a file
        /// </summary>
        public Dictionary<string, string> GetTags(string filePath)
        {
            var tagFilePath = $"{filePath}.tags.json";
            if (File.Exists(tagFilePath))
            {
                var json = File.ReadAllText(tagFilePath);
                return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Backup files to destination
        /// </summary>
        public Task<int> BackupFilesAsync(
            string sourcePath, 
            string backupPath,
            bool incrementalBackup = true,
            IProgress<string>? progress = null)
        {
            var count = 0;
            Directory.CreateDirectory(backupPath);

            var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
            
            foreach (var file in files)
            {
                try
                {
                    var relativePath = Path.GetRelativePath(sourcePath, file);
                    var destPath = Path.Combine(backupPath, relativePath);
                    var destDir = Path.GetDirectoryName(destPath);
                    
                    if (!string.IsNullOrEmpty(destDir))
                        Directory.CreateDirectory(destDir);

                    // Skip if incremental and file hasn't changed
                    if (incrementalBackup && File.Exists(destPath))
                    {
                        var sourceInfo = new FileInfo(file);
                        var destInfo = new FileInfo(destPath);
                        if (sourceInfo.LastWriteTime <= destInfo.LastWriteTime)
                            continue;
                    }

                    progress?.Report($"Backing up {relativePath}...");
                    File.Copy(file, destPath, true);
                    count++;
                }
                catch { /* Skip failed files */ }
            }

            return Task.FromResult(count);
        }

        private string GetCategory(string extension)
        {
            var categories = new Dictionary<string, string[]>
            {
                ["Videos"] = new[] { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv" },
                ["Images"] = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" },
                ["Audio"] = new[] { ".mp3", ".wav", ".flac", ".aac", ".m4a" },
                ["Documents"] = new[] { ".pdf", ".doc", ".docx", ".txt" }
            };

            foreach (var cat in categories)
            {
                if (cat.Value.Contains(extension.ToLower()))
                    return cat.Key;
            }
            return "Other";
        }

        private string GetResolutionCategory(int width, int height)
        {
            if (width >= 3840 && height >= 2160) return "4K (3840x2160+)";
            if (width >= 2560 && height >= 1440) return "2K (2560x1440)";
            if (width >= 1920 && height >= 1080) return "1080p (1920x1080)";
            if (width >= 1280 && height >= 720) return "720p (1280x720)";
            if (width >= 854 && height >= 480) return "480p (854x480)";
            return "SD (Below 480p)";
        }

        private string GetDurationCategory(TimeSpan duration)
        {
            if (duration.TotalMinutes < 1) return "Under 1 min";
            if (duration.TotalMinutes < 5) return "1-5 min";
            if (duration.TotalMinutes < 15) return "5-15 min";
            if (duration.TotalMinutes < 30) return "15-30 min";
            if (duration.TotalHours < 1) return "30-60 min";
            return "Over 1 hour";
        }
    }
}
