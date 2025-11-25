using System;
using System.Reflection;

namespace DJBookingSystem.Models
{
    /// <summary>
    /// Centralized version information for CandyBot DJ Booking System
    /// </summary>
    public static class VersionInfo
    {
        /// <summary>
        /// Current application version
        /// </summary>
        public static Version CurrentVersion => GetAssemblyVersion();

        /// <summary>
        /// Version string (e.g., "1.0.0")
        /// </summary>
        public static string VersionString => CurrentVersion.ToString(3);

        /// <summary>
        /// Full version string with build number (e.g., "1.0.0.0")
        /// </summary>
        public static string FullVersionString => CurrentVersion.ToString();

        /// <summary>
        /// Product name
        /// </summary>
        public static string ProductName => "CandyBot DJ Booking System";

        /// <summary>
        /// Company name
        /// </summary>
        public static string CompanyName => "Fallen Collective";

        /// <summary>
        /// Copyright information
        /// </summary>
        public static string Copyright => $"Copyright © {DateTime.Now.Year} Fallen Collective. All rights reserved.";

        /// <summary>
        /// Application description
        /// </summary>
        public static string Description => "AI-powered DJ booking and management system with Discord integration";

        /// <summary>
        /// Release date
        /// </summary>
        public static DateTime ReleaseDate => new DateTime(2024, 11, 19);

        /// <summary>
        /// Days since release
        /// </summary>
        public static int DaysSinceRelease => (DateTime.Now - ReleaseDate).Days;

        /// <summary>
        /// Full about text
        /// </summary>
        public static string AboutText => 
            $"{ProductName}\n" +
            $"Version {VersionString}\n\n" +
            $"{Description}\n\n" +
            $"{Copyright}\n\n" +
            $"Released: {ReleaseDate:MMMM dd, yyyy}\n" +
            $"Days active: {DaysSinceRelease}";

        /// <summary>
        /// Get assembly version
        /// </summary>
        private static Version GetAssemblyVersion()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                return assembly.GetName().Version ?? new Version(1, 0, 0, 0);
            }
            catch
            {
                return new Version(1, 0, 0, 0);
            }
        }

        /// <summary>
        /// Get file version
        /// </summary>
        public static string GetFileVersion()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                return fileVersionInfo.FileVersion ?? VersionString;
            }
            catch
            {
                return VersionString;
            }
        }

        /// <summary>
        /// Get product version
        /// </summary>
        public static string GetProductVersion()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                return fileVersionInfo.ProductVersion ?? VersionString;
            }
            catch
            {
                return VersionString;
            }
        }

        /// <summary>
        /// Check if this is a newer version than the provided version
        /// </summary>
        public static bool IsNewerThan(Version otherVersion)
        {
            return CurrentVersion > otherVersion;
        }

        /// <summary>
        /// Check if this is a newer version than the provided version string
        /// </summary>
        public static bool IsNewerThan(string versionString)
        {
            if (Version.TryParse(versionString, out var otherVersion))
            {
                return CurrentVersion > otherVersion;
            }
            return false;
        }

        /// <summary>
        /// Compare with another version
        /// </summary>
        public static int CompareTo(Version otherVersion)
        {
            return CurrentVersion.CompareTo(otherVersion);
        }

        /// <summary>
        /// Compare with another version string
        /// </summary>
        public static int CompareTo(string versionString)
        {
            if (Version.TryParse(versionString, out var otherVersion))
            {
                return CurrentVersion.CompareTo(otherVersion);
            }
            return 1; // Current version is considered newer if parse fails
        }
    }
}
