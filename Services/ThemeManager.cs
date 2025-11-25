using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DJBookingSystem.Models;

namespace DJBookingSystem.Services
{
    public static class ThemeManager
    {
        // 🎨 Available Color Themes - Matrix Style + Animated Rainbow
        public static readonly Dictionary<string, string> ColorThemes = new Dictionary<string, string>
        {
            { "Green", "💚 Matrix Green (Default)" },
            { "Blue", "💙 Cyber Blue" },
            { "Red", "❤️ Neon Red" },
            { "Purple", "💜 Electric Purple" },
            { "Pink", "🩷 Hot Pink" },
            { "Orange", "🧡 Sunset Orange" },
            { "Yellow", "💛 Cyber Yellow" },
            { "Cyan", "🩵 Ice Cyan" },
            { "White", "🤍 Ghost White" },
            { "Crimson", "🔥 Blood Crimson" },
            { "Teal", "💎 Aqua Teal" },
            { "Rainbow", "🌈 Rainbow (Animated)" }
        };

        private static string _currentTheme = "Green";

        /// <summary>
        /// Apply theme to the entire application
        /// </summary>
        public static void ApplyColorTheme(string themeName)
        {
            if (string.IsNullOrEmpty(themeName))
                themeName = "Green";

            var app = Application.Current;
            if (app == null) return;

            try
            {
                // Stop rainbow animation if switching away from Rainbow theme
                if (_currentTheme == "Rainbow" && themeName != "Rainbow")
                {
                    RainbowThemeAnimator.StopAnimation();
                }

                // Remove old color theme
                var oldTheme = app.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Source != null && 
                                       (d.Source.OriginalString.Contains("Theme.xaml") ||
                                        d.Source.OriginalString.Contains("ColorThemes/")) &&
                                       !d.Source.OriginalString.Contains("SpaceTheme") &&
                                       !d.Source.OriginalString.Contains("CustomTabStyles"));

                if (oldTheme != null)
                    app.Resources.MergedDictionaries.Remove(oldTheme);

                // Add new color theme from ColorThemes folder
                var themeUri = new Uri($"/Themes/ColorThemes/{themeName}Theme.xaml", UriKind.Relative);
                var newTheme = new ResourceDictionary { Source = themeUri };
                app.Resources.MergedDictionaries.Add(newTheme);

                // Start rainbow animation if Rainbow theme selected
                if (themeName == "Rainbow")
                {
                    RainbowThemeAnimator.StartAnimation();
                }

                _currentTheme = themeName;
                System.Diagnostics.Debug.WriteLine($"✅ Applied {themeName} theme!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error applying theme: {ex.Message}");
                
                // Fallback to Green theme
                if (themeName != "Green")
                {
                    ApplyColorTheme("Green");
                }
            }
        }

        /// <summary>
        /// Apply theme preferences to a specific window
        /// </summary>
        public static void ApplyTheme(Window window, UserAppPreferences prefs)
        {
            if (prefs == null) return;

            try
            {
                // Apply color theme globally
                if (!string.IsNullOrEmpty(prefs.ColorTheme))
                    ApplyColorTheme(prefs.ColorTheme);

                // Apply StayOnTop preference
                window.Topmost = prefs.StayOnTop;

                System.Diagnostics.Debug.WriteLine($"✅ Applied theme preferences to {window.GetType().Name}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error applying theme preferences: {ex.Message}");
            }
        }

        /// <summary>
        /// Get theme display name
        /// </summary>
        public static string GetThemeDisplayName(string themeName)
        {
            if (string.IsNullOrEmpty(themeName))
                themeName = "Green";

            return ColorThemes.ContainsKey(themeName) 
                ? ColorThemes[themeName] 
                : ColorThemes["Green"];
        }

        /// <summary>
        /// Get theme preview color (for UI display)
        /// </summary>
        public static Color GetThemePreviewColor(string themeName)
        {
            return themeName switch
            {
                "Green" => Color.FromRgb(0, 255, 65),      // Matrix green
                "Blue" => Color.FromRgb(0, 212, 255),
                "Red" => Color.FromRgb(255, 0, 64),
                "Purple" => Color.FromRgb(180, 0, 255),
                "Pink" => Color.FromRgb(255, 0, 212),
                "Orange" => Color.FromRgb(255, 149, 0),
                "Yellow" => Color.FromRgb(255, 255, 0),
                "Cyan" => Color.FromRgb(0, 255, 255),
                "White" => Color.FromRgb(255, 255, 255),
                "Crimson" => Color.FromRgb(220, 20, 60),
                "Teal" => Color.FromRgb(0, 255, 200),
                "Rainbow" => Color.FromRgb(255, 0, 255),   // Purple/magenta for rainbow
                _ => Color.FromRgb(0, 255, 65) // Default Matrix green
            };
        }

        /// <summary>
        /// Get current active theme
        /// </summary>
        public static string CurrentTheme => _currentTheme;

        /// <summary>
        /// Check if Rainbow theme is currently active and animating
        /// </summary>
        public static bool IsRainbowActive => _currentTheme == "Rainbow" && RainbowThemeAnimator.IsAnimating;

        // Legacy theme support (kept for backwards compatibility)
        public static Dictionary<string, ThemeColors> AvailableThemes = new Dictionary<string, ThemeColors>
        {
            {
                "Default", new ThemeColors
                {
                    Name = "Default (Day Mode)",
                    Background = "#ECF0F1",
                    Text = "#2C3E50",
                    Header = "#34495E",
                    Menu = "#BDC3C7",
                    Button = "#3498DB",
                    ButtonText = "#FFFFFF",
                    Border = "#7F8C8D",
                    Accent = "#1ABC9C",
                    Success = "#27AE60",
                    Error = "#E74C3C",
                    PrimaryColor = Color.FromRgb(52, 152, 219),
                    SecondaryColor = Color.FromRgb(26, 188, 156),
                    BackgroundColor = Color.FromRgb(236, 240, 241)
                }
            },
            {
                "Night", new ThemeColors
                {
                    Name = "Night Mode (Dark)",
                    Background = "#1A1A1A",
                    Text = "#FFFFFF",
                    Header = "#2C2C2C",
                    Menu = "#333333",
                    Button = "#3498DB",
                    ButtonText = "#FFFFFF",
                    Border = "#555555",
                    Accent = "#5DADE2",
                    Success = "#58D68D",
                    Error = "#EC7063",
                    PrimaryColor = Color.FromRgb(93, 173, 226),
                    SecondaryColor = Color.FromRgb(88, 214, 141),
                    BackgroundColor = Color.FromRgb(26, 26, 26)
                }
            },
            {
                "DarkGreen", new ThemeColors
                {
                    Name = "Dark Green (Matrix)",
                    Background = "#000000",
                    Text = "#00FF00",
                    Header = "#001100",
                    Menu = "#001100",
                    Button = "#00FF00",
                    ButtonText = "#000000",
                    Border = "#00FF00",
                    Accent = "#00FF00",
                    Success = "#39FF14",
                    Error = "#FF0000",
                    PrimaryColor = Color.FromRgb(0, 255, 0),
                    SecondaryColor = Color.FromRgb(57, 255, 20),
                    BackgroundColor = Color.FromRgb(0, 0, 0)
                }
            },
            {
                "Sunset", new ThemeColors
                {
                    Name = "Sunset (Warm)",
                    Background = "#2C1810",
                    Text = "#FFD700",
                    Header = "#401C10",
                    Menu = "#502010",
                    Button = "#FF6B35",
                    ButtonText = "#FFF8E1",
                    Border = "#FF8C42",
                    Accent = "#FFA500",
                    Success = "#FFD700",
                    Error = "#FF4500",
                    PrimaryColor = Color.FromRgb(255, 107, 53),
                    SecondaryColor = Color.FromRgb(255, 165, 0),
                    BackgroundColor = Color.FromRgb(44, 24, 16)
                }
            },
            {
                "Ocean", new ThemeColors
                {
                    Name = "Ocean (Cool Blue)",
                    Background = "#0A1929",
                    Text = "#00D4FF",
                    Header = "#0D2642",
                    Menu = "#102A43",
                    Button = "#00B4D8",
                    ButtonText = "#FFFFFF",
                    Border = "#00D4FF",
                    Accent = "#0096C7",
                    Success = "#00D4FF",
                    Error = "#FF6B6B",
                    PrimaryColor = Color.FromRgb(0, 180, 216),
                    SecondaryColor = Color.FromRgb(0, 212, 255),
                    BackgroundColor = Color.FromRgb(10, 25, 41)
                }
            },
            {
                "Purple", new ThemeColors
                {
                    Name = "Purple Haze",
                    Background = "#1A0A2E",
                    Text = "#B565D8",
                    Header = "#2E1A47",
                    Menu = "#3D1E6D",
                    Button = "#9B59B6",
                    ButtonText = "#FFFFFF",
                    Border = "#B565D8",
                    Accent = "#8E44AD",
                    Success = "#A29BFE",
                    Error = "#FF6B9D",
                    PrimaryColor = Color.FromRgb(155, 89, 182),
                    SecondaryColor = Color.FromRgb(181, 101, 216),
                    BackgroundColor = Color.FromRgb(26, 10, 46)
                }
            },
            {
                "MidnightBlue", new ThemeColors
                {
                    Name = "Midnight Blue",
                    Background = "#0C1445",
                    Text = "#5DADE2",
                    Header = "#1A237E",
                    Menu = "#283593",
                    Button = "#3F51B5",
                    ButtonText = "#FFFFFF",
                    Border = "#5C6BC0",
                    Accent = "#5DADE2",
                    Success = "#4FC3F7",
                    Error = "#EF5350",
                    PrimaryColor = Color.FromRgb(63, 81, 181),
                    SecondaryColor = Color.FromRgb(93, 173, 226),
                    BackgroundColor = Color.FromRgb(12, 20, 69)
                }
            },
            {
                "Custom", new ThemeColors
                {
                    Name = "Custom Theme",
                    Background = "#0A0A0A",
                    Text = "#00FF00",
                    Header = "#000000",
                    Menu = "#000000",
                    Button = "#001100",
                    ButtonText = "#00FF00",
                    Border = "#00FF00",
                    Accent = "#00FF00",
                    Success = "#39FF14",
                    Error = "#FF0000",
                    PrimaryColor = Color.FromRgb(0, 255, 0),
                    SecondaryColor = Color.FromRgb(57, 255, 20),
                    BackgroundColor = Color.FromRgb(10, 10, 10)
                }
            }
        };
    }

    // Legacy ThemeColors class for backwards compatibility
    public class ThemeColors
    {
        public string Name { get; set; } = string.Empty;
        public Color PrimaryColor { get; set; }
        public Color SecondaryColor { get; set; }
        public Color BackgroundColor { get; set; }
        
        // Additional properties for UserSettingsWindow compatibility
        public string Background { get; set; } = "#000000";
        public string Text { get; set; } = "#00FF00";
        public string Header { get; set; } = "#00FF00";
        public string Menu { get; set; } = "#001100";
        public string Button { get; set; } = "#00FF00";
        public string ButtonText { get; set; } = "#000000";
        public string Border { get; set; } = "#00FF00";
        public string Accent { get; set; } = "#00FF00";
        public string Success { get; set; } = "#00FF00";
        public string Error { get; set; } = "#FF0000";
    }
}
