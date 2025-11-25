using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Track information from RadioBOSS
    /// </summary>
    public class TrackInfo
    {
        public string Artist { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string Album { get; set; } = string.Empty;
        public int ListenerCount { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string StationUrl { get; set; } = string.Empty;
    }

    public class RadioBossService
    {
        // LPM Stations
        private readonly string _lpmStation1Url = "https://stations.radioboss.fm/r/lpm";
        private readonly string _lpmStation2Url = "https://stations.radioboss.fm/r/lpm1";
        
        private readonly HttpClient _httpClient;
        private string _currentApiUrl;
        private int _currentLpmStation = 1; // 1 or 2

        public RadioBossService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            
            // Default to LPM station 1
            _currentApiUrl = _lpmStation1Url;
        }

        /// <summary>
        /// Set which station to use for Now Playing
        /// </summary>
        public void SetStation(string stationUrl)
        {
            _currentApiUrl = stationUrl;
            
            // Detect which LPM station
            if (stationUrl.Contains("/lpm1"))
            {
                _currentLpmStation = 2;
                System.Diagnostics.Debug.WriteLine($"📻 RadioBOSS station set to: LPM Station 2");
            }
            else if (stationUrl.Contains("/lpm"))
            {
                _currentLpmStation = 1;
                System.Diagnostics.Debug.WriteLine($"📻 RadioBOSS station set to: LPM Station 1");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"📻 RadioBOSS station set to: {stationUrl}");
            }
        }

        /// <summary>
        /// Switch between LPM stations (1 and 2)
        /// </summary>
        public void SwitchLpmStation()
        {
            _currentLpmStation = _currentLpmStation == 1 ? 2 : 1;
            _currentApiUrl = _currentLpmStation == 1 ? _lpmStation1Url : _lpmStation2Url;
            System.Diagnostics.Debug.WriteLine($"🔄 Switched to LPM Station {_currentLpmStation}");
        }

        /// <summary>
        /// Get current LPM station number
        /// </summary>
        public int GetCurrentLpmStation() => _currentLpmStation;

        /// <summary>
        /// Get both LPM stations' current tracks
        /// </summary>
        public async Task<(TrackInfo? station1, TrackInfo? station2)> GetBothLpmTracksAsync()
        {
            try
            {
                var station1Task = GetTrackFromUrlAsync(_lpmStation1Url);
                var station2Task = GetTrackFromUrlAsync(_lpmStation2Url);

                await Task.WhenAll(station1Task, station2Task);

                var track1 = await station1Task;
                if (track1 != null)
                {
                    track1.StationName = "LPM Station 1";
                    track1.StationUrl = _lpmStation1Url;
                }

                var track2 = await station2Task;
                if (track2 != null)
                {
                    track2.StationName = "LPM Station 2";
                    track2.StationUrl = _lpmStation2Url;
                }

                return (track1, track2);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error getting both LPM tracks: {ex.Message}");
                return (null, null);
            }
        }

        /// <summary>
        /// Get track info from a specific URL
        /// </summary>
        private async Task<TrackInfo?> GetTrackFromUrlAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    if (url.Contains("stations.radioboss.fm"))
                    {
                        return ParseLPMStationHtml(content, url);
                    }
                    else
                    {
                        return ParseLegacyApiJson(content);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error fetching from {url}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Check if RadioBOSS is online and responding
        /// </summary>
        public async Task<bool> IsConnectedAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_currentApiUrl);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ RadioBOSS connection check failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get current playing track information from RadioBOSS
        /// </summary>
        public async Task<TrackInfo?> GetCurrentTrackAsync()
        {
            return await GetTrackFromUrlAsync(_currentApiUrl);
        }

        /// <summary>
        /// Parse track info from LPM station HTML page
        /// </summary>
        private TrackInfo ParseLPMStationHtml(string html, string sourceUrl)
        {
            try
            {
                var trackInfo = new TrackInfo
                {
                    StationUrl = sourceUrl
                };

                // Determine station name
                if (sourceUrl.Contains("/lpm1"))
                {
                    trackInfo.StationName = "LPM Station 2 (LPM1)";
                }
                else
                {
                    trackInfo.StationName = "LPM Station 1 (LPM)";
                }

                // Method 1: Try to find "Now Playing:" text and extract what follows
                var nowPlayingMatch = System.Text.RegularExpressions.Regex.Match(
                    html,
                    @"Now Playing[:\s]*<[^>]+>([^<]+)</",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );

                if (nowPlayingMatch.Success)
                {
                    var nowPlaying = System.Net.WebUtility.HtmlDecode(nowPlayingMatch.Groups[1].Value.Trim());
                    ParseTrackString(trackInfo, nowPlaying);
                }

                // Method 2: Look for common HTML patterns
                if (string.IsNullOrEmpty(trackInfo.Artist) && string.IsNullOrEmpty(trackInfo.Title))
                {
                    // Try to find artist in various class names
                    var artistMatch = System.Text.RegularExpressions.Regex.Match(
                        html,
                        @"class=""[^""]*(?:artist|performer|band)[^""]*""[^>]*>([^<]+)<",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    );

                    if (artistMatch.Success)
                    {
                        trackInfo.Artist = System.Net.WebUtility.HtmlDecode(artistMatch.Groups[1].Value.Trim());
                    }

                    // Try to find title
                    var titleMatch = System.Text.RegularExpressions.Regex.Match(
                        html,
                        @"class=""[^""]*(?:title|song|track)[^""]*""[^>]*>([^<]+)<",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    );

                    if (titleMatch.Success)
                    {
                        trackInfo.Title = System.Net.WebUtility.HtmlDecode(titleMatch.Groups[1].Value.Trim());
                    }
                }

                // Method 3: Look for <title> tag content
                if (string.IsNullOrEmpty(trackInfo.Artist) && string.IsNullOrEmpty(trackInfo.Title))
                {
                    var titleTagMatch = System.Text.RegularExpressions.Regex.Match(
                        html,
                        @"<title>([^<]+)</title>",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    );

                    if (titleTagMatch.Success)
                    {
                        var titleContent = System.Net.WebUtility.HtmlDecode(titleTagMatch.Groups[1].Value.Trim());
                        ParseTrackString(trackInfo, titleContent);
                    }
                }

                // Method 4: Look for any bold or strong text that might be track info
                if (string.IsNullOrEmpty(trackInfo.Artist) && string.IsNullOrEmpty(trackInfo.Title))
                {
                    var boldMatch = System.Text.RegularExpressions.Regex.Match(
                        html,
                        @"<(?:b|strong)>([^<]+)</(?:b|strong)>",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    );

                    if (boldMatch.Success)
                    {
                        var boldContent = System.Net.WebUtility.HtmlDecode(boldMatch.Groups[1].Value.Trim());
                        ParseTrackString(trackInfo, boldContent);
                    }
                }

                // Try to find listener count
                var listenerMatch = System.Text.RegularExpressions.Regex.Match(
                    html,
                    @"(?:listeners?|audience|online)[^\d]*(\d+)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );

                if (listenerMatch.Success && int.TryParse(listenerMatch.Groups[1].Value, out int listeners))
                {
                    trackInfo.ListenerCount = listeners;
                }

                // If we still don't have artist/title, set defaults
                if (string.IsNullOrEmpty(trackInfo.Artist) && string.IsNullOrEmpty(trackInfo.Title))
                {
                    trackInfo.Artist = "Live Party Music";
                    trackInfo.Title = $"Now Playing on {trackInfo.StationName}";
                }

                System.Diagnostics.Debug.WriteLine($"🎵 Parsed {trackInfo.StationName}: {trackInfo.Artist} - {trackInfo.Title}");
                return trackInfo;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing LPM HTML: {ex.Message}");
                return new TrackInfo
                {
                    Artist = "Live Party Music",
                    Title = "Now Playing on LPM.fm",
                    StationName = sourceUrl.Contains("/lpm1") ? "LPM Station 2" : "LPM Station 1",
                    StationUrl = sourceUrl
                };
            }
        }

        /// <summary>
        /// Parse a track string in "Artist - Title" format
        /// </summary>
        private void ParseTrackString(TrackInfo trackInfo, string trackString)
        {
            // Clean up common prefixes
            trackString = System.Text.RegularExpressions.Regex.Replace(
                trackString,
                @"^(?:Now Playing|Current Track|Playing)[:\s]*",
                "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            ).Trim();

            // Try to split on various separators
            var separators = new[] { " - ", " – ", " — ", " | " };
            foreach (var sep in separators)
            {
                var parts = trackString.Split(new[] { sep }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    trackInfo.Artist = parts[0].Trim();
                    trackInfo.Title = string.Join(sep, parts.Skip(1)).Trim();
                    return;
                }
            }

            // If no separator found, put everything in title
            if (!string.IsNullOrEmpty(trackString))
            {
                trackInfo.Title = trackString;
                trackInfo.Artist = "Live Party Music";
            }
        }

        /// <summary>
        /// Parse track info from legacy API JSON format
        /// </summary>
        private TrackInfo? ParseLegacyApiJson(string json)
        {
            try
            {
                var data = JObject.Parse(json);
                
                var trackInfo = new TrackInfo
                {
                    Artist = data["artist"]?.ToString() ?? "Unknown Artist",
                    Title = data["title"]?.ToString() ?? "Unknown Title",
                    Album = data["album"]?.ToString() ?? string.Empty,
                    ListenerCount = data["listeners"]?.Value<int>() ?? 0,
                    StationName = "RadioBOSS"
                };
                
                // Parse duration if available
                var durationStr = data["duration"]?.ToString();
                if (!string.IsNullOrEmpty(durationStr) && TimeSpan.TryParse(durationStr, out var duration))
                {
                    trackInfo.Duration = duration;
                }
                
                return trackInfo;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing legacy API JSON: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get RadioBOSS info (for future use)
        /// </summary>
        public async Task<string?> GetRadioInfoAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_currentApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to get RadioBOSS info: {ex.Message}");
                return null;
            }
        }

        // Placeholder methods for RadioBOSS control (to be implemented)
        public Task<bool> IsOnlineAsync() => IsConnectedAsync();
        public Task<string> GetListenerCountAsync() => Task.FromResult("0");
        public Task<string> GetFormattedTrackInfoAsync() => Task.FromResult("No track info available");
        public Task<bool> SetArtworkAsync(string artworkUrl) => Task.FromResult(true);
        public Task<bool> PlayAsync() => Task.FromResult(true);
        public Task<bool> PauseAsync() => Task.FromResult(true);
        public Task<bool> StopAsync() => Task.FromResult(true);
        public Task<bool> NextTrackAsync() => Task.FromResult(true);
        public Task<bool> PreviousTrackAsync() => Task.FromResult(true);
        public Task<bool> SetVolumeAsync(int volume) => Task.FromResult(true);
        public Task<Newtonsoft.Json.Linq.JObject?> GetUpcomingEventsAsync() => Task.FromResult<Newtonsoft.Json.Linq.JObject?>(null);
        public Task<Newtonsoft.Json.Linq.JObject?> GetNowPlayingAsync() => Task.FromResult<Newtonsoft.Json.Linq.JObject?>(null);
        public Task<Newtonsoft.Json.Linq.JObject?> GetPlaylistAsync() => Task.FromResult<Newtonsoft.Json.Linq.JObject?>(null);
    }
}
