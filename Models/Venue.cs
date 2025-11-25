using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DJBookingSystem.Models
{
    /// <summary>
    /// Represents the operating hours for a specific day
    /// </summary>
    public class DaySchedule
    {
        public string StartTime { get; set; } = "18:00";
        public string FinishTime { get; set; } = "02:00";
    }

    /// <summary>
    /// Represents a venue with per-day scheduling and weekly patterns
    /// </summary>
    public class Venue
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        [Obsolete("Use DaySchedules instead")]
        public string OpenTime { get; set; } = "18:00";
        
        [Obsolete("Use DaySchedules instead")]
        public string CloseTime { get; set; } = "02:00";
        
        public string OwnerUsername { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public Dictionary<DayOfWeek, DaySchedule> DaySchedules { get; set; } = new();
        public List<int> ActiveWeeks { get; set; } = new() { 1, 2, 3, 4 };
        
        [JsonIgnore]
        public List<DayOfWeek> OpenDays => DaySchedules.Keys.ToList();
        
        [JsonIgnore]
        public string OpenDaysText => string.Join(", ", OpenDays.Select(d => d.ToString()[..3]));
        
        [JsonIgnore]
        public string ActiveWeeksText => string.Join(", ", ActiveWeeks.Select(w => $"W{w}"));
        
        /// <summary>
        /// Check if venue is open on a specific day and week of month
        /// </summary>
        public bool IsOpenOn(DayOfWeek day, int weekOfMonth)
        {
            if (DaySchedules == null || ActiveWeeks == null)
                return false;
                
            return DaySchedules.ContainsKey(day) && ActiveWeeks.Contains(weekOfMonth);
        }
        
        /// <summary>
        /// Get the schedule for a specific day
        /// </summary>
        public DaySchedule? GetScheduleFor(DayOfWeek day)
        {
            if (DaySchedules == null)
                return null;
                
            return DaySchedules.TryGetValue(day, out var schedule) ? schedule : null;
        }
        
        /// <summary>
        /// Calculate which week of the month a date falls in
        /// </summary>
        public static int GetWeekOfMonth(DateTime date)
        {
            DateTime firstDayOfMonth = new(date.Year, date.Month, 1);
            int dayOffset = (int)firstDayOfMonth.DayOfWeek;
            return (date.Day + dayOffset - 1) / 7 + 1;
        }
        
        /// <summary>
        /// Get all available hourly time slots for a specific day
        /// </summary>
        public List<string> GetAvailableTimeSlots(DayOfWeek day)
        {
            var slots = new List<string>();
            
            if (DaySchedules == null)
                return slots;
            
            if (!DaySchedules.TryGetValue(day, out var schedule) || schedule == null)
                return slots;
            
            if (string.IsNullOrEmpty(schedule.StartTime) || string.IsNullOrEmpty(schedule.FinishTime))
                return slots;
            
            if (!TimeSpan.TryParse(schedule.StartTime, out var startTime) || 
                !TimeSpan.TryParse(schedule.FinishTime, out var finishTime))
            {
                return slots;
            }
            
            // Validate time ranges
            if (startTime.TotalHours < 0 || startTime.TotalHours >= 24)
            {
                System.Diagnostics.Debug.WriteLine($"Invalid start time for {day}: {schedule.StartTime}");
                return slots;
            }
            
            if (finishTime.TotalHours < 0 || finishTime.TotalHours > 24)
            {
                System.Diagnostics.Debug.WriteLine($"Invalid finish time for {day}: {schedule.FinishTime}");
                return slots;
            }
            
            var currentTime = startTime;
            
            // Handle overnight schedules (finish time is after midnight)
            if (finishTime < startTime)
            {
                // Add slots from start time until midnight
                while (currentTime.TotalHours < 24)
                {
                    slots.Add(currentTime.ToString(@"hh\:mm"));
                    currentTime = currentTime.Add(TimeSpan.FromHours(1));
                    
                    // Safety check: prevent infinite loop
                    if (slots.Count > 24)
                        break;
                }
                
                // Add slots from midnight to finish time
                currentTime = TimeSpan.Zero;
                while (currentTime < finishTime)
                {
                    slots.Add(currentTime.ToString(@"hh\:mm"));
                    currentTime = currentTime.Add(TimeSpan.FromHours(1));
                    
                    // Safety check: prevent infinite loop
                    if (slots.Count > 48)
                        break;
                }
            }
            else
            {
                while (currentTime < finishTime)
                {
                    slots.Add(currentTime.ToString(@"hh\:mm"));
                    currentTime = currentTime.Add(TimeSpan.FromHours(1));
                    
                    // Safety check: prevent infinite loop
                    if (slots.Count > 24)
                        break;
                }
            }
            
            return slots;
        }
        
        [Obsolete("Use GetAvailableTimeSlots(DayOfWeek) instead")]
        public List<string> GetAvailableTimeSlots()
        {
            var firstDay = OpenDays.FirstOrDefault();
            if (firstDay == default)
                firstDay = DayOfWeek.Saturday;
            
            return GetAvailableTimeSlots(firstDay);
        }
    }
}
