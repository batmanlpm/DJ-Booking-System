using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Shared settings service for Candy-Bot that synchronizes between Desktop Mode and Booking Mode
    /// Any change in one mode is automatically reflected in the other
    /// </summary>
    public class CandyBotSharedSettings : INotifyPropertyChanged
    {
        private static CandyBotSharedSettings? _instance;
        private static readonly object _lock = new object();
        private readonly string _settingsFilePath;

        // Settings properties
        private string _currentPersonality = "Normal";
        private bool _voiceEnabled = true;
        private int _speechRate = 0;
        private string _currentTheme = "Dark";
        private bool _alwaysOnTop = true;
        private bool _autoCheckUpdates = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        private CandyBotSharedSettings()
        {
            // Store settings in AppData for persistence
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string candyBotFolder = Path.Combine(appDataPath, "CandyBot");
            Directory.CreateDirectory(candyBotFolder);
            _settingsFilePath = Path.Combine(candyBotFolder, "candy bot-settings.json");

            LoadSettings();
        }

        /// <summary>
        /// Singleton instance - ensures both Desktop and Booking modes share the same settings
        /// </summary>
        public static CandyBotSharedSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new CandyBotSharedSettings();
                        }
                    }
                }
                return _instance;
            }
        }

        #region Properties

        /// <summary>
        /// Current personality mode (Normal, Shy, Funny, Raunchy, ShitStirring, Professional)
        /// </summary>
        public string CurrentPersonality
        {
            get => _currentPersonality;
            set
            {
                if (_currentPersonality != value)
                {
                    _currentPersonality = value;
                    OnPropertyChanged();
                    SaveSettings();
                }
            }
        }

        /// <summary>
        /// Voice enabled/disabled state
        /// </summary>
        public bool VoiceEnabled
        {
            get => _voiceEnabled;
            set
            {
                if (_voiceEnabled != value)
                {
                    _voiceEnabled = value;
                    OnPropertyChanged();
                    SaveSettings();
                }
            }
        }

        /// <summary>
        /// Speech rate (-2 to +2)
        /// </summary>
        public int SpeechRate
        {
            get => _speechRate;
            set
            {
                if (_speechRate != value)
                {
                    _speechRate = value;
                    OnPropertyChanged();
                    SaveSettings();
                }
            }
        }

        /// <summary>
        /// Current theme (Rainbow, Dark, Light)
        /// </summary>
        public string CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    OnPropertyChanged();
                    SaveSettings();
                }
            }
        }

        /// <summary>
        /// Always on top setting
        /// </summary>
        public bool AlwaysOnTop
        {
            get => _alwaysOnTop;
            set
            {
                if (_alwaysOnTop != value)
                {
                    _alwaysOnTop = value;
                    OnPropertyChanged();
                    SaveSettings();
                }
            }
        }

        /// <summary>
        /// Auto-check for updates setting
        /// </summary>
        public bool AutoCheckUpdates
        {
            get => _autoCheckUpdates;
            set
            {
                if (_autoCheckUpdates != value)
                {
                    _autoCheckUpdates = value;
                    OnPropertyChanged();
                    SaveSettings();
                }
            }
        }

        #endregion

        #region Save/Load

        private void SaveSettings()
        {
            try
            {
                var settings = new
                {
                    CurrentPersonality,
                    VoiceEnabled,
                    SpeechRate,
                    CurrentTheme,
                    AlwaysOnTop,
                    AutoCheckUpdates,
                    LastModified = DateTime.Now
                };

                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);

                System.Diagnostics.Debug.WriteLine($"[CandyBot Settings] Saved: {_settingsFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CandyBot Settings] Save error: {ex.Message}");
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<JsonElement>(json);

                    if (settings.TryGetProperty("CurrentPersonality", out var personality))
                        _currentPersonality = personality.GetString() ?? "Normal";

                    if (settings.TryGetProperty("VoiceEnabled", out var voice))
                        _voiceEnabled = voice.GetBoolean();

                    if (settings.TryGetProperty("SpeechRate", out var rate))
                        _speechRate = rate.GetInt32();

                    if (settings.TryGetProperty("CurrentTheme", out var theme))
                        _currentTheme = theme.GetString() ?? "Dark";

                    if (settings.TryGetProperty("AlwaysOnTop", out var onTop))
                        _alwaysOnTop = onTop.GetBoolean();

                    if (settings.TryGetProperty("AutoCheckUpdates", out var autoUpdate))
                        _autoCheckUpdates = autoUpdate.GetBoolean();

                    System.Diagnostics.Debug.WriteLine($"[CandyBot Settings] Loaded from: {_settingsFilePath}");
                }
                else
                {
                    // First time - save defaults
                    SaveSettings();
                    System.Diagnostics.Debug.WriteLine($"[CandyBot Settings] Created new settings file");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CandyBot Settings] Load error: {ex.Message}");
            }
        }

        #endregion

        #region INotifyPropertyChanged

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get display name for personality mode
        /// </summary>
        public string GetPersonalityDisplayName()
        {
            return CurrentPersonality switch
            {
                "Normal" => "Normal - Sweet & Balanced",
                "Shy" => "Shy - Timid & Cute",
                "Funny" => "Funny - Jokes & Humor",
                "Raunchy" => "Raunchy - Flirty & Bold",
                "ShitStirring" => "Mischievous - Playful Troublemaker",
                "Professional" => "Professional - Formal & Business",
                _ => "Normal - Sweet & Balanced"
            };
        }

        /// <summary>
        /// Get speech rate display text
        /// </summary>
        public string GetSpeechRateDisplayText()
        {
            return SpeechRate switch
            {
                -2 => "Much Slower",
                -1 => "Slower",
                0 => "Normal Speed",
                1 => "Faster",
                2 => "Much Faster",
                _ => "Normal Speed"
            };
        }

        /// <summary>
        /// Reset all settings to defaults
        /// </summary>
        public void ResetToDefaults()
        {
            CurrentPersonality = "Normal";
            VoiceEnabled = true;
            SpeechRate = 0;
            CurrentTheme = "Dark";
            AlwaysOnTop = true;
            AutoCheckUpdates = true;
        }

        #endregion
    }
}
