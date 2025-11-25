using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Service to check connections to various external services
    /// </summary>
    public class ConnectionService
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        /// <summary>
        /// Check if Live Party Music website is reachable
        /// </summary>
        public static async Task<bool> CheckLivePartyMusicAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://livepartymusic.fm");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if RadioBOSS control panel is reachable
        /// </summary>
        public static async Task<bool> CheckRadioBossControlAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://c40.radioboss.fm/u/98");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check RadioBOSS stream status - Returns true ONLY if stream is LIVE and actively playing
        /// Returns FALSE if stream is paused, stopped, or down
        /// </summary>
        public static async Task<bool> CheckRadioBossStreamStatusAsync()
        {
            try
            {
                // Method 1: Try to read stream data over 2 seconds to detect if it's actively flowing
                var request = new HttpRequestMessage(HttpMethod.Get, "https://c40.radioboss.fm:8098/stream");
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                
                if (!response.IsSuccessStatusCode)
                {
                    return false; // Stream endpoint is DOWN
                }

                // Check content type
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (contentType == null || (!contentType.Contains("audio") && !contentType.Contains("mpeg")))
                {
                    return false; // Not an audio stream
                }

                // Read data over 2 seconds to detect active playback
                using var stream = await response.Content.ReadAsStreamAsync();
                byte[] buffer1 = new byte[4096];
                byte[] buffer2 = new byte[4096];
                
                // First read
                var bytes1 = await stream.ReadAsync(buffer1, 0, buffer1.Length);
                if (bytes1 == 0)
                {
                    return false; // No data = stream is PAUSED or DOWN
                }
                
                // Wait 1 second
                await Task.Delay(1000);
                
                // Second read
                var bytes2 = await stream.ReadAsync(buffer2, 0, buffer2.Length);
                if (bytes2 == 0)
                {
                    return false; // No more data = stream is PAUSED or DOWN
                }
                
                // If we got different data in both reads, stream is LIVE and PLAYING
                bool dataIsDifferent = false;
                for (int i = 0; i < Math.Min(bytes1, bytes2); i++)
                {
                    if (buffer1[i] != buffer2[i])
                    {
                        dataIsDifferent = true;
                        break;
                    }
                }
                
                if (dataIsDifferent && bytes1 > 0 && bytes2 > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"RadioBOSS Stream: LIVE (bytes1={bytes1}, bytes2={bytes2})");
                    return true; // Stream is LIVE and PLAYING ?
                }
                
                System.Diagnostics.Debug.WriteLine($"RadioBOSS Stream: PAUSED or DOWN (no data flow detected)");
                return false; // Stream is PAUSED or DOWN ?
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RadioBOSS stream check error: {ex.Message}");
                return false; // If we can't connect, stream is DOWN ?
            }
        }

        /// <summary>
        /// Check Azure Cosmos DB connection
        /// </summary>
        public static async Task<bool> CheckCosmosDbAsync(CosmosDbService cosmosService)
        {
            try
            {
                if (cosmosService == null)
                    return false;

                // Try to access database - if it works, connection is good
                await cosmosService.InitializeDatabaseAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Perform all connection checks
        /// </summary>
        public static async Task<ConnectionResults> CheckAllConnectionsAsync(CosmosDbService? cosmosService = null)
        {
            var results = new ConnectionResults();

            // Run all checks in parallel
            var cosmosTask = cosmosService != null
                ? CheckCosmosDbAsync(cosmosService)
                : Task.FromResult(false);
            
            var lpmTask = CheckLivePartyMusicAsync();
            var radioBossControlTask = CheckRadioBossControlAsync();
            var radioBossStreamTask = CheckRadioBossStreamStatusAsync();

            await Task.WhenAll(cosmosTask, lpmTask, radioBossControlTask, radioBossStreamTask);

            results.CosmosDbConnected = await cosmosTask;
            results.LivePartyMusicConnected = await lpmTask;
            results.RadioBossControlConnected = await radioBossControlTask;
            results.RadioBossStreamActive = await radioBossStreamTask;

            return results;
        }
    }

    /// <summary>
    /// Results of connection checks
    /// </summary>
    public class ConnectionResults
    {
        public bool CosmosDbConnected { get; set; }
        public bool LivePartyMusicConnected { get; set; }
        public bool RadioBossControlConnected { get; set; }
        public bool RadioBossStreamActive { get; set; }

        public bool AllConnected =>
            CosmosDbConnected &&
            LivePartyMusicConnected &&
            RadioBossControlConnected &&
            RadioBossStreamActive;
    }
}
