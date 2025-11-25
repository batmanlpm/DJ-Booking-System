using System;
using System.IO;
using Microsoft.Win32;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Manages Windows startup registry entries for Candy-Bot
    /// </summary>
    public static class WindowsStartupManager
    {
        private const string APP_NAME = "CandyBotDesktopWidget";
        private const string REGISTRY_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        /// <summary>
        /// Add Candy-Bot to Windows startup
        /// </summary>
        public static bool AddToStartup()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true);
                if (key == null)
                {
                    System.Diagnostics.Debug.WriteLine("? Could not open startup registry key");
                    return false;
                }

                // Get current executable path
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
                
                if (string.IsNullOrEmpty(exePath))
                {
                    System.Diagnostics.Debug.WriteLine("? Could not determine executable path");
                    return false;
                }

                // Add with --desktop-mode argument
                string startupCommand = $"\"{exePath}\" --desktop-mode";
                key.SetValue(APP_NAME, startupCommand);

                System.Diagnostics.Debug.WriteLine($"? Added Candy-Bot to Windows startup: {startupCommand}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error adding to startup: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Remove Candy-Bot from Windows startup
        /// </summary>
        public static bool RemoveFromStartup()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true);
                if (key == null)
                {
                    System.Diagnostics.Debug.WriteLine("? Could not open startup registry key");
                    return false;
                }

                // Remove the entry
                if (key.GetValue(APP_NAME) != null)
                {
                    key.DeleteValue(APP_NAME);
                    System.Diagnostics.Debug.WriteLine("? Removed Candy-Bot from Windows startup");
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error removing from startup: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if Candy-Bot is set to start with Windows
        /// </summary>
        public static bool IsInStartup()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, false);
                if (key == null) return false;

                var value = key.GetValue(APP_NAME);
                return value != null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error checking startup status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Toggle startup status
        /// </summary>
        public static bool ToggleStartup(bool enable)
        {
            return enable ? AddToStartup() : RemoveFromStartup();
        }
    }
}
