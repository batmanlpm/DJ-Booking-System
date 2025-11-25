using System;
using System.Collections.Generic;

namespace DJBookingSystem.Models
{
    /// <summary>
    /// Candy-Bot personality modes
    /// </summary>
    public enum CandyBotPersonalityMode
    {
        Normal,              // Default balanced personality
        Shy,                 // Timid, reserved, gentle
        Funny,              // Jokes, puns, comedy
        ShitStirring,       // Sarcastic, provocative, edgy
        Raunchy,            // Flirty, bold (mature 18+)
        Professional        // Business-like, formal
    }
    
    /// <summary>
    /// Stores user preferences and first-time setup status for Candy-Bot
    /// </summary>
    public class CandyBotUserPreferences
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Username { get; set; } = "";
        public string PreferredName { get; set; } = ""; // What Candy-Bot calls the user
        public bool HasCompletedFirstTimeSetup { get; set; } = false;
        public bool HasSeenWelcome { get; set; } = false;
        public bool HasCompletedTutorial { get; set; } = false;
        public DateTime FirstInteraction { get; set; } = DateTime.UtcNow;
        public DateTime LastInteraction { get; set; } = DateTime.UtcNow;
        public int TotalInteractions { get; set; } = 0;
        
        // Tutorial Progress
        public bool TutorialStep1_Welcome { get; set; } = false;
        public bool TutorialStep2_Navigation { get; set; } = false;
        public bool TutorialStep3_Booking { get; set; } = false;
        public bool TutorialStep4_Venues { get; set; } = false;
        public bool TutorialStep5_Features { get; set; } = false;
        
        // Preferences
        public bool ShowAnimations { get; set; } = true;
        public bool EnableSounds { get; set; } = true;
        public bool VoiceMode { get; set; } = false; // Text-to-speech enabled
        public CandyBotPersonalityMode PersonalityMode { get; set; } = CandyBotPersonalityMode.Normal;
        public bool MinimizeToCorner { get; set; } = true;
        
        // System-Wide Features
        public bool DesktopModeEnabled { get; set; } = false;
        public bool AlwaysOnTop { get; set; } = true; // Stay on top of other windows
        public bool StartWithWindows { get; set; } = false; // Launch at Windows startup
        public bool QuietMode { get; set; } = false; // Only speak when called upon
        
        // Position
        public double WindowX { get; set; } = double.NaN;
        public double WindowY { get; set; } = double.NaN;
        public double WindowWidth { get; set; } = 400;
        public double WindowHeight { get; set; } = 500;
    }
    
    /// <summary>
    /// Tutorial step information
    /// </summary>
    public class TutorialStep
    {
        public int StepNumber { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string HighlightElement { get; set; } = ""; // Element to highlight
        public string ActionRequired { get; set; } = ""; // What user needs to do
        public bool IsCompleted { get; set; } = false;
    }
    
    /// <summary>
    /// Helper class for personality mode descriptions
    /// </summary>
    public static class CandyBotPersonalities
    {
        public static string GetModeDescription(CandyBotPersonalityMode mode)
        {
            return mode switch
            {
                CandyBotPersonalityMode.Normal => "Balanced, friendly, and helpful",
                CandyBotPersonalityMode.Shy => "Timid, gentle, and sweet",
                CandyBotPersonalityMode.Funny => "Jokes, puns, and comedy",
                CandyBotPersonalityMode.ShitStirring => "Sarcastic and edgy",
                CandyBotPersonalityMode.Raunchy => "Flirty and bold (mature)",
                CandyBotPersonalityMode.Professional => "Business-like and formal",
                _ => "Normal"
            };
        }
    }
}
