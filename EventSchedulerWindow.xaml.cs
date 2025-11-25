using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class EventSchedulerWindow : Window
    {
        private readonly ContentSchedulerService _scheduler;
        private readonly S3UploadService _s3Service;

        public EventSchedulerWindow(S3UploadService s3Service = null)
        {
            InitializeComponent();
            _s3Service = s3Service;
            _scheduler = new ContentSchedulerService(_s3Service);
            
            _scheduler.PostPublished += OnPostPublished;
            _scheduler.PostFailed += OnPostFailed;
            
            PlatformComboBox.SelectedIndex = 0;
            DatePicker.SelectedDate = DateTime.Now;
            
            LoadScheduledItems();
            UpdateStats();
        }

        private void LoadScheduledItems()
        {
            var items = _scheduler.GetScheduledPosts();
            ScheduledItemsList.ItemsSource = items;
        }

        private void UpdateStats()
        {
            var stats = _scheduler.GetStatistics();
            StatsTextBlock.Text = $"Total: {stats["Total"]} | Scheduled: {stats["Scheduled"]} | Posted: {stats["Posted"]} | Failed: {stats["Failed"]}";
        }

        private async void ScheduleEvent_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                System.Windows.MessageBox.Show("Please enter a title", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DatePicker.SelectedDate.HasValue)
            {
                System.Windows.MessageBox.Show("Please select a date", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var scheduledTime = DatePicker.SelectedDate.Value;
            
            if (int.TryParse(HourTextBox.Text, out int hour) && int.TryParse(MinuteTextBox.Text, out int minute))
            {
                scheduledTime = scheduledTime.AddHours(hour).AddMinutes(minute);
            }

            var title = TitleTextBox.Text;
            var description = DescriptionTextBox.Text;
            var platform = (PlatformComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString() ?? "Website";
            var filePath = FilePathTextBox.Text;

            try
            {
                if (!string.IsNullOrEmpty(filePath) && UploadToS3CheckBox.IsChecked == true)
                {
                    await _scheduler.SchedulePostWithUploadAsync(title, description, scheduledTime, filePath, platform);
                }
                else
                {
                    _scheduler.SchedulePost(title, description, scheduledTime, filePath, platform);
                }

                System.Windows.MessageBox.Show($"Event scheduled for {scheduledTime:MMM dd, yyyy HH:mm}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                ClearForm();
                LoadScheduledItems();
                UpdateStats();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new OpenFileDialog();
            dialog.Filter = "All Files (*.*)|*.*|Videos (*.mp4;*.avi;*.mkv)|*.mp4;*.avi;*.mkv|Images (*.jpg;*.png)|*.jpg;*.png";
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FilePathTextBox.Text = dialog.FileName;
            }
        }

        private void Calendar_SelectedDatesChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (EventCalendar.SelectedDate.HasValue)
            {
                DatePicker.SelectedDate = EventCalendar.SelectedDate;
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var postId = button?.Tag as string;
            
            if (postId != null)
            {
                var post = _scheduler.GetScheduledPosts().FirstOrDefault(p => p.Id == postId);
                if (post != null)
                {
                    TitleTextBox.Text = post.Title;
                    DescriptionTextBox.Text = post.Description;
                    DatePicker.SelectedDate = post.ScheduledTime.Date;
                    HourTextBox.Text = post.ScheduledTime.Hour.ToString();
                    MinuteTextBox.Text = post.ScheduledTime.Minute.ToString("00");
                    FilePathTextBox.Text = post.FilePath ?? "";
                    
                    _scheduler.DeletePost(postId);
                    LoadScheduledItems();
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var postId = button?.Tag as string;
            
            if (postId != null)
            {
                var result = System.Windows.MessageBox.Show("Delete this scheduled event?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _scheduler.DeletePost(postId);
                    LoadScheduledItems();
                    UpdateStats();
                }
            }
        }

        private void OnPostPublished(object sender, ContentSchedulerService.ScheduledPost post)
        {
            Dispatcher.Invoke(() =>
            {
                LoadScheduledItems();
                UpdateStats();
                System.Windows.MessageBox.Show($"Posted: {post.Title}", "Event Published", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private void OnPostFailed(object sender, ContentSchedulerService.ScheduledPost post)
        {
            Dispatcher.Invoke(() =>
            {
                LoadScheduledItems();
                UpdateStats();
                System.Windows.MessageBox.Show($"Failed to post: {post.Title}\n{post.ErrorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        private void ClearForm()
        {
            TitleTextBox.Clear();
            DescriptionTextBox.Clear();
            FilePathTextBox.Clear();
            HourTextBox.Text = "12";
            MinuteTextBox.Text = "00";
            DatePicker.SelectedDate = DateTime.Now;
            PlatformComboBox.SelectedIndex = 0;
            UploadToS3CheckBox.IsChecked = false;
        }
    }
}
