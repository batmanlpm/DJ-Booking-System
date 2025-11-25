using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// AWS S3 upload service for media files
    /// </summary>
    public class S3UploadService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3UploadService(string bucketName, string accessKey, string secretKey, string region = "us-east-1")
        {
            _bucketName = bucketName;
            
            var config = new Amazon.S3.AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
            };
            
            _s3Client = new AmazonS3Client(accessKey, secretKey, config);
        }

        public class UploadProgress
        {
            public long TotalBytes { get; set; }
            public long TransferredBytes { get; set; }
            public int PercentComplete => TotalBytes > 0 ? (int)((TransferredBytes * 100) / TotalBytes) : 0;
        }

        /// <summary>
        /// Upload file to S3 with progress tracking
        /// </summary>
        public async Task<string> UploadFileAsync(string filePath, string s3Key = null, IProgress<UploadProgress> progress = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            s3Key ??= Path.GetFileName(filePath);

            var fileTransferUtility = new TransferUtility(_s3Client);
            
            var uploadRequest = new TransferUtilityUploadRequest
            {
                BucketName = _bucketName,
                FilePath = filePath,
                Key = s3Key,
                CannedACL = S3CannedACL.PublicRead // Make publicly accessible
            };

            if (progress != null)
            {
                uploadRequest.UploadProgressEvent += (sender, e) =>
                {
                    progress.Report(new UploadProgress
                    {
                        TotalBytes = e.TotalBytes,
                        TransferredBytes = e.TransferredBytes
                    });
                };
            }

            await fileTransferUtility.UploadAsync(uploadRequest);

            return $"https://{_bucketName}.s3.amazonaws.com/{s3Key}";
        }

        /// <summary>
        /// Upload multiple files
        /// </summary>
        public async Task<Dictionary<string, string>> UploadFilesAsync(
            string[] filePaths, 
            string folderPrefix = "",
            IProgress<string> statusProgress = null)
        {
            var results = new Dictionary<string, string>();

            foreach (var filePath in filePaths)
            {
                try
                {
                    var fileName = Path.GetFileName(filePath);
                    var s3Key = string.IsNullOrEmpty(folderPrefix) 
                        ? fileName 
                        : $"{folderPrefix}/{fileName}";

                    statusProgress?.Report($"Uploading {fileName}...");
                    var url = await UploadFileAsync(filePath, s3Key);
                    results[filePath] = url;
                }
                catch (Exception ex)
                {
                    results[filePath] = $"Error: {ex.Message}";
                }
            }

            return results;
        }

        /// <summary>
        /// Delete file from S3
        /// </summary>
        public async Task DeleteFileAsync(string s3Key)
        {
            await _s3Client.DeleteObjectAsync(_bucketName, s3Key);
        }

        /// <summary>
        /// Check if file exists in S3
        /// </summary>
        public async Task<bool> FileExistsAsync(string s3Key)
        {
            try
            {
                await _s3Client.GetObjectMetadataAsync(_bucketName, s3Key);
                return true;
            }
            catch (Amazon.S3.AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }
    }
}
