using System;
using System.Windows;
using System.Threading.Tasks;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class DiscordConnectionDialog : Window
    {
        public string? BotToken { get; private set; }
        public string? ChannelId { get; private set; }
        public string? ChannelName { get; private set; }
        public bool IsConnected { get; private set; } = false;

        public DiscordConnectionDialog()
        {
            InitializeComponent();
            LoadSavedSettings();
        }

        public DiscordConnectionDialog(string? existingBotToken, string? existingChannelId, string? existingChannelName) : this()
        {
            if (!string.IsNullOrEmpty(existingBotToken))
            {
                BotTokenTextBox.Text = existingBotToken;
            }
            if (!string.IsNullOrEmpty(existingChannelId))
            {
                ChannelIdTextBox.Text = existingChannelId;
            }
            if (!string.IsNullOrEmpty(existingChannelName))
            {
                ChannelNameTextBox.Text = existingChannelName;
            }
        }

        private void LoadSavedSettings()
        {
            try
            {
                // Load from app settings
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string settingsFile = System.IO.Path.Combine(documentsPath, "DJBookingSystem", "discord_settings.json");

                if (System.IO.File.Exists(settingsFile))
                {
                    string json = System.IO.File.ReadAllText(settingsFile);
                    var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
                    
                    if (settings != null)
                    {
                        BotTokenTextBox.Text = settings.botToken?.ToString() ?? "";
                        ChannelIdTextBox.Text = settings.channelId?.ToString() ?? "";
                        ChannelNameTextBox.Text = settings.channelName?.ToString() ?? "#general";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Discord] Load settings error: {ex.Message}");
            }
        }

        private void SaveSettings()
        {
            try
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string settingsFolder = System.IO.Path.Combine(documentsPath, "DJBookingSystem");
                System.IO.Directory.CreateDirectory(settingsFolder);

                string settingsFile = System.IO.Path.Combine(settingsFolder, "discord_settings.json");

                var settings = new
                {
                    botToken = BotTokenTextBox.Text,
                    channelId = ChannelIdTextBox.Text,
                    channelName = ChannelNameTextBox.Text,
                    lastUpdated = DateTime.Now
                };

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(settingsFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Discord] Save settings error: {ex.Message}");
            }
        }

        private async void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            string botToken = BotTokenTextBox.Text.Trim();
            string channelIdText = ChannelIdTextBox.Text.Trim();

            if (string.IsNullOrEmpty(botToken))
            {
                StatusText.Text = "Please enter a bot token first!";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            if (string.IsNullOrEmpty(channelIdText) || !ulong.TryParse(channelIdText, out ulong channelId))
            {
                StatusText.Text = "Please enter a valid channel ID!";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            StatusText.Text = "Testing connection...";
            StatusText.Foreground = System.Windows.Media.Brushes.Yellow;

            try
            {
                // Test connection
                var testService = new DiscordService();
                await testService.ConnectAsync(botToken, channelId);
                
                // Send test message
                await testService.SendChannelMessageAsync("Test message from DJ Booking System! Connection successful!");
                
                await Task.Delay(2000); // Wait a bit before disconnecting
                await testService.DisconnectAsync();
                testService.Dispose();

                StatusText.Text = "Connection successful! Test message sent to Discord.";
                StatusText.Foreground = System.Windows.Media.Brushes.LimeGreen;
                IsConnected = true;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Connection failed: {ex.Message}";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
                IsConnected = false;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string botToken = BotTokenTextBox.Text.Trim();
            string channelIdText = ChannelIdTextBox.Text.Trim();

            if (string.IsNullOrEmpty(botToken))
            {
                MessageBox.Show(
                    "Please enter a Discord bot token!\n\nYou can create a bot at: https://discord.com/developers/applications",
                    "Bot Token Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(channelIdText) || !ulong.TryParse(channelIdText, out _))
            {
                MessageBox.Show(
                    "Please enter a valid Discord channel ID!\n\nTo get a channel ID:\n1. Enable Developer Mode in Discord\n2. Right-click the channel\n3. Select 'Copy ID'",
                    "Channel ID Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            BotToken = botToken;
            ChannelId = channelIdText;
            ChannelName = ChannelNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(ChannelName))
            {
                ChannelName = "#general";
            }

            SaveSettings();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void GetHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Discord Bot Setup Instructions:\n\n" +
                "1. Go to https://discord.com/developers/applications\n" +
                "2. Click 'New Application' and give it a name\n" +
                "3. Go to the 'Bot' section and click 'Add Bot'\n" +
                "4. Copy the bot token\n" +
                "5. Enable 'Message Content Intent' under Privileged Gateway Intents\n" +
                "6. Go to OAuth2 > URL Generator\n" +
                "7. Select scopes: 'bot'\n" +
                "8. Select permissions: 'Send Messages', 'Read Messages', 'Read Message History'\n" +
                "9. Copy the generated URL and open it to invite the bot to your server\n\n" +
                "To get Channel ID:\n" +
                "1. Enable Developer Mode in Discord Settings > Advanced\n" +
                "2. Right-click the channel you want to use\n" +
                "3. Click 'Copy ID'",
                "Discord Bot Setup Help",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
