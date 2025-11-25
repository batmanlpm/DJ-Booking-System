using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Scheduled content posting system
    /// </summary>
    public class ContentSchedulerService
    {
        public class ScheduledPost
        {
            public string Id { get; set; } = Guid.NewGuid().ToString();
            public DateTime ScheduledTime { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string FilePath { get; set; }
            public string S3Url { get; set; }
            public PostStatus Status { get; set; } = PostStatus.Pending;
            public string Platform { get; set; } // "Website", "Twitter", "OnlyFans", etc.
            public Dictionary<string, string> Metadata { get; set; } = new();
            public DateTime? PostedAt { get; set; }
            public string ErrorMessage { get; set; }
        }

        public enum PostStatus
        {
            Pending,
            Scheduled,
            Posted,
            Failed,
            Cancelled
        }

        private readonly List<ScheduledPost> _scheduledPosts = new();
        private readonly System.Timers.Timer _checkTimer;
        private readonly S3UploadService _s3Service;

        public event EventHandler<ScheduledPost> PostPublished;
        public event EventHandler<ScheduledPost> PostFailed;

        public ContentSchedulerService(S3UploadService s3Service = null)
        {
            _s3Service = s3Service;
            
            // Check every minute for posts to publish
            _checkTimer = new System.Timers.Timer(60000);
            _checkTimer.Elapsed += CheckScheduledPosts;
            _checkTimer.Start();
        }

        /// <summary>
        /// Schedule a new post
        /// </summary>
        public ScheduledPost SchedulePost(string title, string description, DateTime scheduledTime, string filePath = null, string platform = "Website")
        {
            var post = new ScheduledPost
            {
                Title = title,
                Description = description,
                ScheduledTime = scheduledTime,
                FilePath = filePath,
                Platform = platform,
                Status = PostStatus.Scheduled
            };

            _scheduledPosts.Add(post);
            return post;
        }

        /// <summary>
        /// Schedule post with file upload to S3
        /// </summary>
        public async Task<ScheduledPost> SchedulePostWithUploadAsync(
            string title, 
            string description, 
            DateTime scheduledTime, 
            string filePath, 
            string platform = "Website")
        {
            var post = new ScheduledPost
            {
                Title = title,
                Description = description,
                ScheduledTime = scheduledTime,
                FilePath = filePath,
                Platform = platform,
                Status = PostStatus.Pending
            };

            // Upload to S3 first
            if (_s3Service != null && !string.IsNullOrEmpty(filePath))
            {
                try
                {
                    var s3Key = $"scheduled/{DateTime.Now:yyyy-MM-dd}/{Guid.NewGuid()}/{System.IO.Path.GetFileName(filePath)}";
                    post.S3Url = await _s3Service.UploadFileAsync(filePath, s3Key);
                    post.Status = PostStatus.Scheduled;
                }
                catch (Exception ex)
                {
                    post.Status = PostStatus.Failed;
                    post.ErrorMessage = $"Upload failed: {ex.Message}";
                }
            }
            else
            {
                post.Status = PostStatus.Scheduled;
            }

            _scheduledPosts.Add(post);
            return post;
        }

        /// <summary>
        /// Get all scheduled posts
        /// </summary>
        public List<ScheduledPost> GetScheduledPosts(PostStatus? status = null)
        {
            if (status.HasValue)
                return _scheduledPosts.Where(p => p.Status == status.Value).OrderBy(p => p.ScheduledTime).ToList();
            
            return _scheduledPosts.OrderBy(p => p.ScheduledTime).ToList();
        }

        /// <summary>
        /// Get upcoming posts (next 24 hours)
        /// </summary>
        public List<ScheduledPost> GetUpcomingPosts()
        {
            var now = DateTime.Now;
            var tomorrow = now.AddDays(1);
            
            return _scheduledPosts
                .Where(p => p.Status == PostStatus.Scheduled && p.ScheduledTime >= now && p.ScheduledTime <= tomorrow)
                .OrderBy(p => p.ScheduledTime)
                .ToList();
        }

        /// <summary>
        /// Cancel scheduled post
        /// </summary>
        public bool CancelPost(string postId)
        {
            var post = _scheduledPosts.FirstOrDefault(p => p.Id == postId);
            if (post != null && post.Status == PostStatus.Scheduled)
            {
                post.Status = PostStatus.Cancelled;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reschedule a post
        /// </summary>
        public bool ReschedulePost(string postId, DateTime newScheduledTime)
        {
            var post = _scheduledPosts.FirstOrDefault(p => p.Id == postId);
            if (post != null && (post.Status == PostStatus.Scheduled || post.Status == PostStatus.Failed))
            {
                post.ScheduledTime = newScheduledTime;
                post.Status = PostStatus.Scheduled;
                post.ErrorMessage = null;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Delete post from schedule
        /// </summary>
        public bool DeletePost(string postId)
        {
            var post = _scheduledPosts.FirstOrDefault(p => p.Id == postId);
            if (post != null)
            {
                _scheduledPosts.Remove(post);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Manually publish a scheduled post now
        /// </summary>
        public async Task<bool> PublishNowAsync(string postId)
        {
            var post = _scheduledPosts.FirstOrDefault(p => p.Id == postId);
            if (post == null) return false;

            return await PublishPostAsync(post);
        }

        private async void CheckScheduledPosts(object sender, ElapsedEventArgs e)
        {
            var now = DateTime.Now;
            var postsToPublish = _scheduledPosts
                .Where(p => p.Status == PostStatus.Scheduled && p.ScheduledTime <= now)
                .ToList();

            foreach (var post in postsToPublish)
            {
                await PublishPostAsync(post);
            }
        }

        private async Task<bool> PublishPostAsync(ScheduledPost post)
        {
            try
            {
                // Here you would integrate with actual posting APIs
                // For now, we'll just mark as posted
                await Task.Delay(100); // Simulate API call

                post.Status = PostStatus.Posted;
                post.PostedAt = DateTime.Now;
                
                PostPublished?.Invoke(this, post);
                return true;
            }
            catch (Exception ex)
            {
                post.Status = PostStatus.Failed;
                post.ErrorMessage = ex.Message;
                
                PostFailed?.Invoke(this, post);
                return false;
            }
        }

        /// <summary>
        /// Get posting statistics
        /// </summary>
        public Dictionary<string, int> GetStatistics()
        {
            return new Dictionary<string, int>
            {
                ["Total"] = _scheduledPosts.Count,
                ["Scheduled"] = _scheduledPosts.Count(p => p.Status == PostStatus.Scheduled),
                ["Posted"] = _scheduledPosts.Count(p => p.Status == PostStatus.Posted),
                ["Failed"] = _scheduledPosts.Count(p => p.Status == PostStatus.Failed),
                ["Cancelled"] = _scheduledPosts.Count(p => p.Status == PostStatus.Cancelled)
            };
        }

        public void Dispose()
        {
            _checkTimer?.Stop();
            _checkTimer?.Dispose();
        }
    }
}
