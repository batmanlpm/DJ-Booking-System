using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class CandyBotWelcomeWindow : Window
    {
        private readonly User _currentUser;
        private readonly CosmosDbService? _cosmosDb;
        private CandyBotUserPreferences _preferences;
        private int _currentStep = 1;
        
        public string PreferredName { get; private set; } = "";
        public bool StartTutorial { get; private set; } = false;

        public CandyBotWelcomeWindow(User currentUser, CosmosDbService? cosmosDb = null)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _cosmosDb = cosmosDb;
            _preferences = new CandyBotUserPreferences
            {
                Username = currentUser.Username
            };
            
            PreferredNameTextBox.Focus();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Start animations
            var bounceIn = (Storyboard)this.Resources["BounceInAnimation"];
            bounceIn.Begin();
            
            // Start candy bounce after main animation
            var candyBounce = (Storyboard)this.Resources["CandyBounce"];
            candyBounce.BeginTime = TimeSpan.FromSeconds(0.7);
            candyBounce.Begin();
            
            // Start glow pulse
            var glowPulse = (Storyboard)this.Resources["GlowPulse"];
            glowPulse.Begin();
        }

        private void PreferredName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Continue_Click(sender, e);
            }
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep == 1)
            {
                // Step 1: Name input
                string name = PreferredNameTextBox.Text.Trim();
                
                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show(
                        "Please enter a name so I know what to call you! ??",
                        "Name Required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    PreferredNameTextBox.Focus();
                    return;
                }
                
                PreferredName = name;
                _preferences.PreferredName = name;
                
                // Move to tutorial intro
                ShowTutorialIntro();
            }
            else if (_currentStep == 2)
            {
                // Step 2: Start tutorial
                StartTutorial = true;
                SavePreferences();
                this.Close();
            }
        }

        private void ShowTutorialIntro()
        {
            _currentStep = 2;
            
            // Hide name input
            NameInputPanel.Visibility = Visibility.Collapsed;
            
            // Show tutorial intro with correct greeting
            PersonalGreeting.Text = $"Awesome, Nice To Meet You {PreferredName}! ??";
            TutorialIntroPanel.Visibility = Visibility.Visible;
            
            // Update buttons
            ContinueButton.Content = "Let's Go! ??";
            SkipButton.Visibility = Visibility.Visible;
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to skip the tutorial, {PreferredName}?\n\n" +
                "You can always ask me for help anytime by clicking the ?? button!",
                "Skip Tutorial?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                StartTutorial = false;
                _preferences.HasSeenWelcome = true;
                _preferences.HasCompletedTutorial = true;
                SavePreferences();
                this.Close();
            }
        }

        private void SavePreferences()
        {
            try
            {
                if (_cosmosDb != null)
                {
                    // Save to Cosmos DB
                    // TODO: Add method to save CandyBot preferences
                    // _ = _cosmosDb.SaveCandyBotPreferencesAsync(_preferences);
                }
            }
            catch
            {
                // Ignore errors for now
            }
        }
    }
}
