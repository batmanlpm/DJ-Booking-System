using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem.Views.Radio
{
    public partial class RadioPlayerView : UserControl
    {
        private bool _isPlaying = false;
        private string _currentStreamUrl = string.Empty;
        private CosmosDbService? _cosmosDbService;
        private ObservableCollection<RadioStation> _stations;
        private string _currentUsername = "System";

        public RadioPlayerView()
        {
            InitializeComponent();
            _stations = new ObservableCollection<RadioStation>();
            StationsItemsControl.ItemsSource = _stations;
            
            // Initialize with default stations
            InitializeDefaultStations();
            UpdatePlaybackUI();
        }

        public void SetCosmosDbService(CosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
            LoadStationsFromDatabase();
        }

        public void SetCurrentUsername(string username)
        {
            _currentUsername = username;
        }

        private void InitializeDefaultStations()
        {
            // ==================== LPM STATIONS (Priority) ====================
            _stations.Add(new RadioStation
            {
                Name = "?? LPM Station 1 (c40)",
                StreamUrl = "https://c40.radioboss.fm/stream/98",
                Genre = "Live Party Music",
                AddedBy = "System",
                AddedDate = DateTime.Now,
                IsFavorite = true
            });

            _stations.Add(new RadioStation
            {
                Name = "?? LPM Station 2 (c19)",
                StreamUrl = "https://c19.radioboss.fm/stream/162",
                Genre = "Live Party Music",
                AddedBy = "System",
                AddedDate = DateTime.Now,
                IsFavorite = true
            });

            // ==================== ELECTRONIC / DANCE ====================
            _stations.Add(new RadioStation
            {
                Name = "181.FM - The Box (Urban)",
                StreamUrl = "http://relay.181.fm:8070",
                Genre = "Hip-Hop / R&B",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            _stations.Add(new RadioStation
            {
                Name = "181.FM - Energy 98",
                StreamUrl = "http://relay.181.fm:8004",
                Genre = "Dance / Electronic",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            _stations.Add(new RadioStation
            {
                Name = "DI.FM - Trance",
                StreamUrl = "http://prem2.di.fm:80/trance",
                Genre = "Trance",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            _stations.Add(new RadioStation
            {
                Name = "DI.FM - House",
                StreamUrl = "http://prem2.di.fm:80/house",
                Genre = "House",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            // ==================== ROCK / ALTERNATIVE ====================
            _stations.Add(new RadioStation
            {
                Name = "181.FM - Rock 40",
                StreamUrl = "http://relay.181.fm:8044",
                Genre = "Rock",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            _stations.Add(new RadioStation
            {
                Name = "181.FM - 90s Alternative",
                StreamUrl = "http://relay.181.fm:8066",
                Genre = "Alternative Rock",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            _stations.Add(new RadioStation
            {
                Name = "181.FM - Highway 181",
                StreamUrl = "http://relay.181.fm:8016",
                Genre = "Classic Rock",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            // ==================== POP / TOP 40 ====================
            _stations.Add(new RadioStation
            {
                Name = "181.FM - Star 90s",
                StreamUrl = "http://relay.181.fm:8042",
                Genre = "90s Pop",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            _stations.Add(new RadioStation
            {
                Name = "181.FM - Awesome 80s",
                StreamUrl = "http://relay.181.fm:8022",
                Genre = "80s Pop",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            _stations.Add(new RadioStation
            {
                Name = "181.FM - Super 70s",
                StreamUrl = "http://relay.181.fm:8040",
                Genre = "70s Pop",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            // ==================== EASY LISTENING / CHILL ====================
            _stations.Add(new RadioStation
            {
                Name = "181.FM - Mellow Gold",
                StreamUrl = "http://relay.181.fm:8060",
                Genre = "Easy Listening",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            _stations.Add(new RadioStation
            {
                Name = "181.FM - Chilled Out",
                StreamUrl = "http://relay.181.fm:8700",
                Genre = "Chill / Lounge",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            _stations.Add(new RadioStation
            {
                Name = "Smooth Jazz Florida",
                StreamUrl = "http://us4.internet-radio.com:8266/live",
                Genre = "Smooth Jazz",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            // ==================== COUNTRY / FOLK ====================
            _stations.Add(new RadioStation
            {
                Name = "181.FM - Highway 181",
                StreamUrl = "http://relay.181.fm:8082",
                Genre = "Country",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            // ==================== REGGAE / WORLD ====================
            _stations.Add(new RadioStation
            {
                Name = "181.FM - Reggae Roots",
                StreamUrl = "http://relay.181.fm:8096",
                Genre = "Reggae",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            // ==================== METAL / HARDCORE ====================
            _stations.Add(new RadioStation
            {
                Name = "181.FM - Metal",
                StreamUrl = "http://relay.181.fm:8036",
                Genre = "Metal",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            // ==================== CHRISTMAS / SEASONAL ====================
            _stations.Add(new RadioStation
            {
                Name = "181.FM - Christmas Classics",
                StreamUrl = "http://relay.181.fm:8024",
                Genre = "Christmas",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            // ==================== INDIE / ALTERNATIVE ====================
            _stations.Add(new RadioStation
            {
                Name = "181.FM - Indie Rock",
                StreamUrl = "http://relay.181.fm:8058",
                Genre = "Indie",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            // ==================== AMBIENT / INSTRUMENTAL ====================
            _stations.Add(new RadioStation
            {
                Name = "SomaFM - Groove Salad",
                StreamUrl = "http://ice1.somafm.com/groovesalad-128-mp3",
                Genre = "Ambient / Downtempo",
                AddedBy = "System",
                AddedDate = DateTime.Now
            });

            System.Diagnostics.Debug.WriteLine($"? Initialized {_stations.Count} default radio stations");
        }

        private async void LoadStationsFromDatabase()
        {
            if (_cosmosDbService == null) return;

            try
            {
                var stationsFromDb = await _cosmosDbService.GetAllRadioStationsAsync();
                
                // Clear existing and add from database
                _stations.Clear();
                
                foreach (var station in stationsFromDb)
                {
                    _stations.Add(station);
                }

                // If no stations in database, add defaults
                if (_stations.Count == 0)
                {
                    InitializeDefaultStations();
                }

                System.Diagnostics.Debug.WriteLine($"Loaded {_stations.Count} stations from database");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading stations: {ex.Message}");
                // Fall back to default stations
                if (_stations.Count == 0)
                {
                    InitializeDefaultStations();
                }
            }
        }

        private void StationPlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is RadioStation station)
            {
                PlayStation(station.StreamUrl, station.Name);
            }
        }

        private void PlayStation(string streamUrl, string stationName)
        {
            try
            {
                // Update current station info
                _currentStreamUrl = streamUrl;
                CurrentStationName.Text = stationName;

                // Stop current playback if any
                if (_isPlaying)
                {
                    RadioPlayer.Stop();
                    _isPlaying = false;
                }

                // Load and play new station
                RadioPlayer.Source = new Uri(streamUrl);
                RadioPlayer.Play();
                _isPlaying = true;
                UpdatePlaybackUI();

                System.Diagnostics.Debug.WriteLine($"Playing station: {stationName} - {streamUrl}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error playing station:\n{ex.Message}",
                    "Playback Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                _isPlaying = false;
                UpdatePlaybackUI();
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentStreamUrl))
            {
                MessageBox.Show(
                    "Please select a radio station first!",
                    "No Station Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            try
            {
                if (_isPlaying)
                {
                    // Pause
                    RadioPlayer.Pause();
                    _isPlaying = false;
                }
                else
                {
                    // Resume or start playing
                    if (RadioPlayer.Source == null)
                    {
                        RadioPlayer.Source = new Uri(_currentStreamUrl);
                    }
                    RadioPlayer.Play();
                    _isPlaying = true;
                }

                UpdatePlaybackUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error controlling playback:\n{ex.Message}",
                    "Playback Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                _isPlaying = false;
                UpdatePlaybackUI();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioPlayer.Stop();
                RadioPlayer.Source = null;
                _isPlaying = false;
                CurrentStationName.Text = "No Station Selected";
                _currentStreamUrl = string.Empty;
                UpdatePlaybackUI();

                System.Diagnostics.Debug.WriteLine("Playback stopped");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping playback: {ex.Message}");
            }
        }

        private async void UploadStation_Click(object sender, RoutedEventArgs e)
        {
            string stationName = NewStationName.Text.Trim();
            string stationUrl = NewStationUrl.Text.Trim();

            // Validation
            if (string.IsNullOrEmpty(stationName))
            {
                MessageBox.Show(
                    "Please enter a station name!",
                    "Missing Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(stationUrl))
            {
                MessageBox.Show(
                    "Please enter a station URL!",
                    "Missing Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Validate URL format
            if (!Uri.TryCreate(stationUrl, UriKind.Absolute, out Uri? uri) || 
                (uri.Scheme != "http" && uri.Scheme != "https"))
            {
                MessageBox.Show(
                    "Please enter a valid HTTP or HTTPS URL!",
                    "Invalid URL",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                var newStation = new RadioStation
                {
                    Name = stationName,
                    StreamUrl = stationUrl,
                    Genre = "User Added",
                    AddedBy = _currentUsername,
                    AddedDate = DateTime.Now,
                    IsFavorite = false
                };

                // Add to database if available
                if (_cosmosDbService != null)
                {
                    string id = await _cosmosDbService.AddRadioStationAsync(newStation);
                    newStation.Id = id;
                    System.Diagnostics.Debug.WriteLine($"Station saved to database with ID: {id}");
                }

                // Add to local collection
                _stations.Add(newStation);

                // Clear input fields
                NewStationName.Clear();
                NewStationUrl.Clear();

                MessageBox.Show(
                    $"Station '{stationName}' has been added successfully!",
                    "Station Added",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                System.Diagnostics.Debug.WriteLine($"Added new station: {stationName} - {stationUrl}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error adding station:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                System.Diagnostics.Debug.WriteLine($"Error adding station: {ex.Message}");
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (RadioPlayer != null)
            {
                RadioPlayer.Volume = e.NewValue;
                
                if (VolumePercentage != null)
                {
                    VolumePercentage.Text = $"{(int)(e.NewValue * 100)}%";
                }
            }
        }

        private void RadioPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Media opened successfully");
            _isPlaying = true;
            UpdatePlaybackUI();
        }

        private void RadioPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Media failed: {e.ErrorException?.Message}");
            
            MessageBox.Show(
                $"Failed to load radio stream:\n{e.ErrorException?.Message}\n\nPlease check your internet connection and try again.",
                "Stream Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            
            _isPlaying = false;
            UpdatePlaybackUI();
        }

        private void UpdatePlaybackUI()
        {
            if (PlayButton == null) return;

            if (_isPlaying)
            {
                PlayButton.Content = "? PAUSE";
            }
            else
            {
                PlayButton.Content = "? PLAY";
            }

            // Enable/disable buttons based on state
            if (StopButton != null)
            {
                StopButton.IsEnabled = !string.IsNullOrEmpty(_currentStreamUrl);
            }
        }

        /// <summary>
        /// Public method to stop playback when view is unloaded
        /// </summary>
        public void StopPlayback()
        {
            try
            {
                if (_isPlaying)
                {
                    RadioPlayer.Stop();
                    RadioPlayer.Source = null;
                    _isPlaying = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping playback on unload: {ex.Message}");
            }
        }
    }
}
