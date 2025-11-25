using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class CandyBotSettingsWindow : Window
    {
        private CandyBotTextToSpeech? _tts;
        private CandyBotSoundManager? _soundManager;
        private CandyBotSharedSettings _sharedSettings = CandyBotSharedSettings.Instance;

        public CandyBotSettingsWindow()
        {
            InitializeComponent();
            
            try
            {
                _tts = new CandyBotTextToSpeech();
                _soundManager = new CandyBotSoundManager();
                _sharedSettings = CandyBotSharedSettings.Instance;
                
                LoadSettings();
                
                // Subscribe to settings changes
                _sharedSettings.PropertyChanged += SharedSettings_PropertyChanged;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CandyBotSettingsWindow init error: {ex.Message}");
            }
        }

        private void SharedSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Update UI when settings change from another instance (Desktop Mode or Booking Mode)
            Dispatcher.Invoke(() =>
            {
                if (e.PropertyName == nameof(CandyBotSharedSettings.CurrentPersonality))
                {
                    UpdatePersonalityUI();
                }
                else if (e.PropertyName == nameof(CandyBotSharedSettings.VoiceEnabled))
                {
                    if (VoiceEnabledCheckbox != null)
                        VoiceEnabledCheckbox.IsChecked = _sharedSettings.VoiceEnabled;
                }
                else if (e.PropertyName == nameof(CandyBotSharedSettings.SpeechRate))
                {
                    if (SpeechRateSlider != null)
                        SpeechRateSlider.Value = _sharedSettings.SpeechRate;
                }
                else if (e.PropertyName == nameof(CandyBotSharedSettings.CurrentTheme))
                {
                    UpdateThemeUI();
                }
                else if (e.PropertyName == nameof(CandyBotSharedSettings.AlwaysOnTop))
                {
                    if (AlwaysOnTopCheckbox != null)
                        AlwaysOnTopCheckbox.IsChecked = _sharedSettings.AlwaysOnTop;
                }
            });
        }

        private void LoadSettings()
        {
            try
            {
                // Load personality
                UpdatePersonalityUI();

                // Load voice settings
                if (VoiceEnabledCheckbox != null)
                {
                    VoiceEnabledCheckbox.IsChecked = _sharedSettings.VoiceEnabled;
                }
                
                if (SpeechRateSlider != null)
                {
                    SpeechRateSlider.Value = _sharedSettings.SpeechRate;
                }

                // Load theme
                UpdateThemeUI();

                // Load always on top
                if (AlwaysOnTopCheckbox != null)
                {
                    AlwaysOnTopCheckbox.IsChecked = _sharedSettings.AlwaysOnTop;
                }

                // Apply TTS settings
                if (_tts != null)
                {
                    _tts.SetEnabled(_sharedSettings.VoiceEnabled);
                    _tts.SetSpeechRate(_sharedSettings.SpeechRate);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadSettings error: {ex.Message}");
            }
        }

        private void UpdatePersonalityUI()
        {
            switch (_sharedSettings.CurrentPersonality)
            {
                case "Normal":
                    if (NormalMode != null) NormalMode.IsChecked = true;
                    break;
                case "Shy":
                    if (ShyMode != null) ShyMode.IsChecked = true;
                    break;
                case "Funny":
                    if (FunnyMode != null) FunnyMode.IsChecked = true;
                    break;
                case "Raunchy":
                    if (RaunchyMode != null) RaunchyMode.IsChecked = true;
                    break;
                case "ShitStirring":
                    if (MischievousMode != null) MischievousMode.IsChecked = true;
                    break;
                case "Professional":
                    if (ProfessionalMode != null) ProfessionalMode.IsChecked = true;
                    break;
            }
        }

        private void UpdateThemeUI()
        {
            switch (_sharedSettings.CurrentTheme)
            {
                case "Rainbow":
                    if (RainbowTheme != null) RainbowTheme.IsChecked = true;
                    break;
                case "Dark":
                    if (DarkTheme != null) DarkTheme.IsChecked = true;
                    break;
                case "Light":
                    if (LightTheme != null) LightTheme.IsChecked = true;
                    break;
            }
        }

        #region Tab Navigation

        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radio)
            {
                // Hide all content panels
                if (PersonalityContent != null) PersonalityContent.Visibility = Visibility.Collapsed;
                if (DisplayContent != null) DisplayContent.Visibility = Visibility.Collapsed;
                if (AudioContent != null) AudioContent.Visibility = Visibility.Collapsed;
                if (UpdateContent != null) UpdateContent.Visibility = Visibility.Collapsed;
                if (KnowledgeContent != null) KnowledgeContent.Visibility = Visibility.Collapsed;
                if (AboutContent != null) AboutContent.Visibility = Visibility.Collapsed;

                // Show the selected content
                if (radio == PersonalityTab && PersonalityContent != null)
                    PersonalityContent.Visibility = Visibility.Visible;
                else if (radio == DisplayTab && DisplayContent != null)
                    DisplayContent.Visibility = Visibility.Visible;
                else if (radio == AudioTab && AudioContent != null)
                    AudioContent.Visibility = Visibility.Visible;
                else if (radio == UpdateTab && UpdateContent != null)
                    UpdateContent.Visibility = Visibility.Visible;
                else if (radio == KnowledgeTab && KnowledgeContent != null)
                    KnowledgeContent.Visibility = Visibility.Visible;
                else if (radio == AboutTab && AboutContent != null)
                    AboutContent.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Personality Settings

        private void Personality_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radio && radio.IsChecked == true)
            {
                var personality = radio.Tag?.ToString() ?? "Normal";
                _sharedSettings.CurrentPersonality = personality;
                
                System.Diagnostics.Debug.WriteLine($"[Settings] Personality changed to: {personality}");
                
                // Play a voice line in the new personality
                _ = _tts?.SpeakAsync($"Personality changed to {personality} mode!");
            }
        }

        #endregion

        #region Display Settings

        private void Theme_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radio && radio.IsChecked == true)
            {
                string theme = radio.Content?.ToString()?.Split(' ')[0] ?? "Dark";
                _sharedSettings.CurrentTheme = theme;
                
                System.Diagnostics.Debug.WriteLine($"[Settings] Theme changed to: {theme}");
                _ = _tts?.SpeakAsync($"Theme changed to {theme}!");
            }
        }

        private void AlwaysOnTop_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox)
            {
                _sharedSettings.AlwaysOnTop = checkbox.IsChecked == true;
                System.Diagnostics.Debug.WriteLine($"[Settings] Always on top: {_sharedSettings.AlwaysOnTop}");
            }
        }

        #endregion

        #region Audio Settings

        private void Voice_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox)
            {
                _sharedSettings.VoiceEnabled = checkbox.IsChecked == true;
                _tts?.SetEnabled(_sharedSettings.VoiceEnabled);
                
                if (_sharedSettings.VoiceEnabled)
                {
                    _ = _tts?.SpeakAsync("Voice enabled!");
                }
            }
        }

        private void SpeechRate_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider)
            {
                _sharedSettings.SpeechRate = (int)slider.Value;
                _tts?.SetSpeechRate(_sharedSettings.SpeechRate);
                
                if (SpeechRateLabel != null)
                {
                    SpeechRateLabel.Text = _sharedSettings.GetSpeechRateDisplayText();
                }
            }
        }

        private async void TestVoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_tts != null)
                {
                    await _tts.SpeakAsync($"Testing voice at speed {_sharedSettings.SpeechRate}. This is {_sharedSettings.CurrentPersonality} personality mode!");
                }
                else
                {
                    MessageBox.Show("Text-to-Speech service not initialized.", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing voice: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Update Settings

        private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UpdateStatusText != null)
                {
                    UpdateStatusText.Text = "Checking for updates...";
                }

                var updateService = new CandyBotUpdateService();
                var updateInfo = await updateService.CheckForUpdatesAsync();
                
                if (updateInfo.UpdateAvailable)
                {
                    if (UpdateStatusText != null)
                    {
                        UpdateStatusText.Text = $"Update available: v{updateInfo.LatestVersion}";
                    }

                    var result = MessageBox.Show(
                        $"Candy-Bot Update Available!\n\n" +
                        $"Current Version: {updateInfo.CurrentVersion}\n" +
                        $"Latest Version: {updateInfo.LatestVersion}\n\n" +
                        $"What's New:\n{updateInfo.ReleaseNotes}\n\n" +
                        $"Would you like to update now?",
                        "Update Available",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        await updateService.DownloadAndInstallUpdateAsync();
                    }
                }
                else
                {
                    if (UpdateStatusText != null)
                    {
                        UpdateStatusText.Text = "You are running the latest version!";
                    }
                    
                    MessageBox.Show("You are running the latest version of Candy-Bot!", 
                        "Up to Date", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                if (UpdateStatusText != null)
                {
                    UpdateStatusText.Text = $"Error checking for updates: {ex.Message}";
                }
                
                MessageBox.Show($"Error checking for updates: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Knowledge Base

        private void OpenUserGuide_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("User Guide feature coming soon!", "Knowledge Base", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenShortcuts_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Keyboard Shortcuts:\n\n" +
                           "Ctrl+Shift+C - Open Chat\n" +
                           "Ctrl+Shift+F - File Search\n" +
                           "Ctrl+Shift+V - Toggle Voice\n" +
                           "Ctrl+Shift+S - Settings",
                           "Keyboard Shortcuts",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
        }

        private void OpenFAQ_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("FAQ feature coming soon!", "Knowledge Base", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Window Controls

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount == 2)
                {
                    // Double-click to maximize/restore (disabled for this window)
                }
                else
                {
                    DragMove();
                }
            }
            catch { }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            // Unsubscribe from settings changes
            if (_sharedSettings != null)
            {
                _sharedSettings.PropertyChanged -= SharedSettings_PropertyChanged;
            }

            _tts?.Dispose();
            base.OnClosed(e);
        }

        #endregion
    }
}
