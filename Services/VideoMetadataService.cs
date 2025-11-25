using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Video metadata extraction using FFprobe
    /// </summary>
    public class VideoMetadataService
    {
        public class VideoMetadata
        {
            public string FilePath { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public long FileSize { get; set; }
            public TimeSpan Duration { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string Resolution => $"{Width}x{Height}";
            public double FrameRate { get; set; }
            public string VideoCodec { get; set; } = string.Empty;
            public string AudioCodec { get; set; } = string.Empty;
            public long BitRate { get; set; }
            public string Format { get; set; } = string.Empty;
        }

        private readonly string _ffprobePath;

        public VideoMetadataService(string ffprobePath = "ffprobe")
        {
            _ffprobePath = ffprobePath;
        }

        /// <summary>
        /// Extract metadata from video file
        /// </summary>
        public async Task<VideoMetadata> GetMetadataAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Video file not found: {filePath}");

            var metadata = new VideoMetadata
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                FileSize = new FileInfo(filePath).Length
            };

            var args = $"-v error -show_entries format=duration,bit_rate,format_name -show_entries stream=codec_name,width,height,r_frame_rate -of default=noprint_wrappers=1 \"{filePath}\"";

            var output = await RunFFprobeAsync(args);

            // Parse output
            ParseMetadata(output, metadata);

            return metadata;
        }

        /// <summary>
        /// Get video duration only (faster)
        /// </summary>
        public async Task<TimeSpan> GetDurationAsync(string filePath)
        {
            var args = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"";
            var output = await RunFFprobeAsync(args);
            
            if (double.TryParse(output.Trim(), out double seconds))
                return TimeSpan.FromSeconds(seconds);
            
            return TimeSpan.Zero;
        }

        /// <summary>
        /// Get video resolution only (faster)
        /// </summary>
        public async Task<(int width, int height)> GetResolutionAsync(string filePath)
        {
            var args = $"-v error -select_streams v:0 -show_entries stream=width,height -of csv=s=x:p=0 \"{filePath}\"";
            var output = await RunFFprobeAsync(args);
            
            var parts = output.Trim().Split('x');
            if (parts.Length == 2 && int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height))
                return (width, height);
            
            return (0, 0);
        }

        /// <summary>
        /// Batch process multiple videos
        /// </summary>
        public async Task<Dictionary<string, VideoMetadata>> GetBatchMetadataAsync(string[] filePaths, IProgress<string>? progress = null)
        {
            var results = new Dictionary<string, VideoMetadata>();

            foreach (var filePath in filePaths)
            {
                try
                {
                    progress?.Report($"Processing {Path.GetFileName(filePath)}...");
                    var metadata = await GetMetadataAsync(filePath);
                    results[filePath] = metadata;
                }
                catch (Exception ex)
                {
                    progress?.Report($"Error processing {Path.GetFileName(filePath)}: {ex.Message}");
                }
            }

            return results;
        }

        private async Task<string> RunFFprobeAsync(string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _ffprobePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
                throw new Exception("Failed to start FFprobe");

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0 && !string.IsNullOrEmpty(error))
                throw new Exception($"FFprobe error: {error}");

            return output;
        }

        private void ParseMetadata(string output, VideoMetadata metadata)
        {
            var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                switch (key)
                {
                    case "duration":
                        if (double.TryParse(value, out double duration))
                            metadata.Duration = TimeSpan.FromSeconds(duration);
                        break;

                    case "bit_rate":
                        if (long.TryParse(value, out long bitrate))
                            metadata.BitRate = bitrate;
                        break;

                    case "format_name":
                        metadata.Format = value;
                        break;

                    case "width":
                        if (int.TryParse(value, out int width))
                            metadata.Width = width;
                        break;

                    case "height":
                        if (int.TryParse(value, out int height))
                            metadata.Height = height;
                        break;

                    case "r_frame_rate":
                        metadata.FrameRate = ParseFrameRate(value);
                        break;

                    case "codec_name":
                        if (string.IsNullOrEmpty(metadata.VideoCodec))
                            metadata.VideoCodec = value;
                        else if (string.IsNullOrEmpty(metadata.AudioCodec))
                            metadata.AudioCodec = value;
                        break;
                }
            }
        }

        private double ParseFrameRate(string frameRate)
        {
            var match = Regex.Match(frameRate, @"(\d+)/(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int num) && int.TryParse(match.Groups[2].Value, out int den) && den != 0)
                return (double)num / den;
            
            return 0;
        }

        /// <summary>
        /// Format file size to human readable
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
