using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Service to lookup geolocation from IP address
    /// Uses free IP-API service (no key required for non-commercial use)
    /// </summary>
    public class GeoLocationService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string IP_API_URL = "http://ip-api.com/json/";

        /// <summary>
        /// Lookup location information from IP address
        /// </summary>
        public static async Task<GeoLocationInfo?> GetLocationFromIPAsync(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return null;

            try
            {
                // Skip localhost/private IPs
                if (ipAddress == "127.0.0.1" || ipAddress == "::1" || ipAddress.StartsWith("192.168.") || 
                    ipAddress.StartsWith("10.") || ipAddress.StartsWith("172."))
                {
                    return new GeoLocationInfo
                    {
                        IP = ipAddress,
                        City = "Local Network",
                        Country = "Local",
                        CountryCode = "LOCAL",
                        Status = "success"
                    };
                }

                var response = await _httpClient.GetStringAsync($"{IP_API_URL}{ipAddress}");
                var locationData = JsonConvert.DeserializeObject<IpApiResponse>(response);

                if (locationData != null && locationData.Status == "success")
                {
                    return new GeoLocationInfo
                    {
                        IP = ipAddress,
                        City = locationData.City ?? "Unknown",
                        Region = locationData.RegionName ?? "",
                        Country = locationData.Country ?? "Unknown",
                        CountryCode = locationData.CountryCode ?? "",
                        Latitude = locationData.Lat,
                        Longitude = locationData.Lon,
                        Timezone = locationData.Timezone ?? "",
                        ISP = locationData.Isp ?? "Unknown",
                        Status = "success"
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GeoLocation lookup failed for {ipAddress}: {ex.Message}");
                return new GeoLocationInfo
                {
                    IP = ipAddress,
                    City = "Unknown",
                    Country = "Unknown",
                    Status = "failed"
                };
            }
        }

        /// <summary>
        /// Get formatted location string
        /// </summary>
        public static string FormatLocation(GeoLocationInfo? location)
        {
            if (location == null)
                return "Unknown Location";

            if (!string.IsNullOrEmpty(location.City) && !string.IsNullOrEmpty(location.Country))
            {
                if (location.Country == "Local")
                    return "Local Network";
                
                return $"{location.City}, {location.Country}";
            }

            if (!string.IsNullOrEmpty(location.Country))
                return location.Country;

            return "Unknown Location";
        }
    }

    /// <summary>
    /// Geolocation information
    /// </summary>
    public class GeoLocationInfo
    {
        public string IP { get; set; } = "";
        public string City { get; set; } = "";
        public string Region { get; set; } = "";
        public string Country { get; set; } = "";
        public string CountryCode { get; set; } = "";
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Timezone { get; set; } = "";
        public string ISP { get; set; } = "";
        public string Status { get; set; } = "";

        public string DisplayLocation => string.IsNullOrEmpty(City) ? Country : $"{City}, {Country}";
    }

    /// <summary>
    /// Response model from IP-API service
    /// </summary>
    internal class IpApiResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; } = "";

        [JsonProperty("country")]
        public string? Country { get; set; }

        [JsonProperty("countryCode")]
        public string? CountryCode { get; set; }

        [JsonProperty("region")]
        public string? Region { get; set; }

        [JsonProperty("regionName")]
        public string? RegionName { get; set; }

        [JsonProperty("city")]
        public string? City { get; set; }

        [JsonProperty("zip")]
        public string? Zip { get; set; }

        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lon")]
        public double Lon { get; set; }

        [JsonProperty("timezone")]
        public string? Timezone { get; set; }

        [JsonProperty("isp")]
        public string? Isp { get; set; }

        [JsonProperty("org")]
        public string? Org { get; set; }

        [JsonProperty("as")]
        public string? As { get; set; }

        [JsonProperty("query")]
        public string? Query { get; set; }
    }
}
