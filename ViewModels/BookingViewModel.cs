using System;
using System.Collections.Generic;
using System.Linq;
using DJBookingSystem.Models;

namespace DJBookingSystem.ViewModels
{
#pragma warning disable CS0618 // Obsolete BookingDate usage

    /// <summary>
    /// View model for displaying booking information with permission-aware field visibility
    /// </summary>
    public class BookingViewModel
    {
        public string? Id { get; set; }
        public string DJName { get; set; } = string.Empty;
        public string StreamingLink { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public BookingStatus Status { get; set; }

        public string ActualStreamingLink { get; set; } = string.Empty;
        public bool CanCopyStreamingLink { get; set; }

        /// <summary>
        /// Create a BookingViewModel from a Booking model with permission-based visibility
        /// </summary>
        public static BookingViewModel FromBooking(Booking booking, User currentUser, List<Venue> allVenues)
        {
            var venue = allVenues.FirstOrDefault(v => v.Name == booking.VenueName);
            bool canSeeStreamingLink = CanUserViewStreamingLink(currentUser, venue, booking);

            return new BookingViewModel
            {
                Id = booking.Id,
                DJName = booking.DJName,
                StreamingLink = canSeeStreamingLink ? booking.StreamingLink : "[Hidden - Venue Owner/Admin Only]",
                Venue = booking.VenueName,
                BookingDate = booking.GetNextOccurrence(DateTime.Now),
                CreatedAt = booking.CreatedAt,
                Status = booking.Status,
                ActualStreamingLink = booking.StreamingLink,
                CanCopyStreamingLink = canSeeStreamingLink
            };
        }

        private static bool CanUserViewStreamingLink(User currentUser, Venue? venue, Booking booking)
        {
            if (currentUser.Role == UserRole.SysAdmin || currentUser.Role == UserRole.Manager)
                return true;

            if (currentUser.IsVenueOwner && venue?.OwnerUsername == currentUser.Username)
                return true;

            if (currentUser.Username == booking.DJUsername)
                return true;

            return false;
        }
    }
}
