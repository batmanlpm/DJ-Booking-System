using System;
using Newtonsoft.Json;

namespace DJBookingSystem.Models
{
    public class RadioStation
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string StreamUrl { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public DateTime AddedDate { get; set; } = DateTime.Now;
        public string AddedBy { get; set; } = string.Empty;
        public bool IsFavorite { get; set; } = false;
    }
}
