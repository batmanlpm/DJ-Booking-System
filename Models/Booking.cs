using System;
using Newtonsoft.Json;

namespace DJBookingSystem.Models
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }

    /// <summary>
    /// Represents a recurring weekly booking for a DJ at a venue
    /// </summary>
    public class Booking
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        
        public string DJName { get; set; } = string.Empty;
        public string DJUsername { get; set; } = string.Empty;
        public string StreamingLink { get; set; } = string.Empty;
        
        public string VenueName { get; set; } = string.Empty;
        public string VenueId { get; set; } = string.Empty;
        public string VenueOwnerUsername { get; set; } = string.Empty;
        
        public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Saturday;
        public int WeekNumber { get; set; } = 1;
        public string TimeSlot { get; set; } = string.Empty;
        
        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [JsonIgnore]
        public string DisplaySchedule => $"{DayOfWeek} (Week {WeekNumber}) at {TimeSlot}";
        
        /// <summary>
        /// Get the next occurrence date of this booking from a given date
        /// </summary>
        public DateTime GetNextOccurrence(DateTime fromDate)
        {
            DateTime current = fromDate.Date;
            
            while (current.DayOfWeek != DayOfWeek)
            {
                current = current.AddDays(1);
            }
            
            int weekOfMonth = Venue.GetWeekOfMonth(current);
            while (weekOfMonth != WeekNumber)
            {
                current = current.AddDays(7);
                weekOfMonth = Venue.GetWeekOfMonth(current);
                
                if (current.Month != fromDate.Month)
                {
                    current = new DateTime(current.Year, current.Month, 1);
                    while (current.DayOfWeek != DayOfWeek)
                    {
                        current = current.AddDays(1);
                    }
                    weekOfMonth = Venue.GetWeekOfMonth(current);
                }
            }
            
            if (TimeSpan.TryParse(TimeSlot, out var timeSpan))
            {
                current = current.Add(timeSpan);
            }
            
            return current;
        }
        
        /// <summary>
        /// Check if this booking occurs on a specific date
        /// </summary>
        public bool OccursOn(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek && 
                   Venue.GetWeekOfMonth(date) == WeekNumber;
        }
        
        /// <summary>
        /// Check if a user has permission to view the streaming link
        /// </summary>
        public bool CanViewStreamingLink(User user)
        {
            if (user.Role == UserRole.SysAdmin || user.Role == UserRole.Manager)
                return true;
            
            if (user.IsVenueOwner && user.Username == VenueOwnerUsername)
                return true;
            
            if (user.Username == DJUsername)
                return true;
            
            return false;
        }
    }
}
