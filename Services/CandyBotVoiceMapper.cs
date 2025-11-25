using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Maps Candy-Bot's 100 voice lines to categories and contexts
    /// Files: 001.mp3 - 100.mp3 in Voices folder
    /// </summary>
    public static class CandyBotVoiceMapper
    {
        private static readonly Random _random = new Random();
        private static readonly string VoicesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Voices");

        /// <summary>
        /// Voice line definition
        /// </summary>
        public class VoiceLine
        {
            public string Number { get; set; } = string.Empty;
            public string FilePath { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        // Master voice line database
        private static readonly List<VoiceLine> AllVoiceLines = new List<VoiceLine>
        {
            // GREETINGS (001-015)
            new VoiceLine { Number = "001", Category = "Greeting", Description = "Hi there! I'm Candy-Bot, your sweet AI assistant!" },
            new VoiceLine { Number = "002", Category = "Greeting", Description = "Hey sweetie! Welcome back!" },
            new VoiceLine { Number = "003", Category = "Greeting", Description = "Hello! Ready to make some bookings?" },
            new VoiceLine { Number = "004", Category = "Greeting", Description = "Hi! I'm here to help with your DJ booking system!" },
            new VoiceLine { Number = "005", Category = "Greeting", Description = "Welcome! I'm Candy-Bot!" },
            new VoiceLine { Number = "006", Category = "Greeting", Description = "Hey! Miss me? Let's get started!" },
            new VoiceLine { Number = "007", Category = "Greeting", Description = "Hi there! First time? I'll guide you!" },
            new VoiceLine { Number = "008", Category = "Greeting", Description = "Hello! What brings you here today?" },
            new VoiceLine { Number = "009", Category = "Greeting", Description = "Hey! Ready to organize some epic DJ gigs?" },
            new VoiceLine { Number = "010", Category = "Greeting", Description = "Welcome back! Let's make something awesome!" },
            new VoiceLine { Number = "011", Category = "Greeting", Description = "Good morning! How can I brighten your day?" },
            new VoiceLine { Number = "012", Category = "Greeting", Description = "Hey there! I've been waiting for you!" },
            new VoiceLine { Number = "013", Category = "Greeting", Description = "Hello! Another day, another adventure!" },
            new VoiceLine { Number = "014", Category = "Greeting", Description = "Hi! Thanks for stopping by!" },
            new VoiceLine { Number = "015", Category = "Greeting", Description = "Welcome to The Fallen Collective!" },

            // HELP & GUIDANCE (016-030)
            new VoiceLine { Number = "016", Category = "Help", Description = "Need help? Just ask!" },
            new VoiceLine { Number = "017", Category = "Help", Description = "I can help with bookings, venues, chat, or conversation!" },
            new VoiceLine { Number = "018", Category = "Help", Description = "Looking for something specific?" },
            new VoiceLine { Number = "019", Category = "Help", Description = "Not sure what to do? I can walk you through it!" },
            new VoiceLine { Number = "020", Category = "Help", Description = "Quick tip: Search venues, create bookings, or chat!" },
            new VoiceLine { Number = "021", Category = "Help", Description = "Let me explain how this works!" },
            new VoiceLine { Number = "022", Category = "Help", Description = "That's a great question! Here's what you need to know..." },
            new VoiceLine { Number = "023", Category = "Help", Description = "Don't worry, everyone gets confused at first!" },
            new VoiceLine { Number = "024", Category = "Help", Description = "Here are your options..." },
            new VoiceLine { Number = "025", Category = "Help", Description = "Need a tutorial? I can be your Desktop Helper too!" },
            new VoiceLine { Number = "026", Category = "Help", Description = "Let me break that down in simple terms!" },
            new VoiceLine { Number = "027", Category = "Help", Description = "I'm here to make your life easier!" },
            new VoiceLine { Number = "028", Category = "Help", Description = "That's easy! Just follow these steps!" },
            new VoiceLine { Number = "029", Category = "Help", Description = "Got a question? I've got answers!" },
            new VoiceLine { Number = "030", Category = "Help", Description = "Confused? That's what I'm here for!" },

            // BOOKING ASSISTANCE (031-040)
            new VoiceLine { Number = "031", Category = "Booking", Description = "Want to create a new booking?" },
            new VoiceLine { Number = "032", Category = "Booking", Description = "Let's get that DJ booked!" },
            new VoiceLine { Number = "033", Category = "Booking", Description = "Looking for available venues?" },
            new VoiceLine { Number = "034", Category = "Booking", Description = "Your booking has been created!" },
            new VoiceLine { Number = "035", Category = "Booking", Description = "Checking the schedule... That date is available!" },
            new VoiceLine { Number = "036", Category = "Booking", Description = "Here are all your upcoming bookings" },
            new VoiceLine { Number = "037", Category = "Booking", Description = "DJ conflict detected! Alternative times..." },
            new VoiceLine { Number = "038", Category = "Booking", Description = "Booking confirmed! Your DJ is all set!" },
            new VoiceLine { Number = "039", Category = "Booking", Description = "Would you like to view your calendar?" },
            new VoiceLine { Number = "040", Category = "Booking", Description = "I found three available venues!" },

            // POSITIVE FEEDBACK (041-055)
            new VoiceLine { Number = "041", Category = "Positive", Description = "That's awesome! I'm so glad I could help!" },
            new VoiceLine { Number = "042", Category = "Positive", Description = "Perfect! You're all set!" },
            new VoiceLine { Number = "043", Category = "Positive", Description = "Great choice! That's going to be amazing!" },
            new VoiceLine { Number = "044", Category = "Positive", Description = "Yay! Another successful booking!" },
            new VoiceLine { Number = "045", Category = "Positive", Description = "Excellent! Anything else you need?" },
            new VoiceLine { Number = "046", Category = "Positive", Description = "You got it! All done!" },
            new VoiceLine { Number = "047", Category = "Positive", Description = "Nice! You're getting really good at this!" },
            new VoiceLine { Number = "048", Category = "Positive", Description = "Boom! Task completed!" },
            new VoiceLine { Number = "049", Category = "Positive", Description = "Woohoo! That worked perfectly!" },
            new VoiceLine { Number = "050", Category = "Positive", Description = "Success! Virtual high five!" },
            new VoiceLine { Number = "051", Category = "Positive", Description = "You're crushing it today!" },
            new VoiceLine { Number = "052", Category = "Positive", Description = "Fantastic! Everything looks good!" },
            new VoiceLine { Number = "053", Category = "Positive", Description = "Amazing work! I'm impressed!" },
            new VoiceLine { Number = "054", Category = "Positive", Description = "All set! That was smooth!" },
            new VoiceLine { Number = "055", Category = "Positive", Description = "Done and done! What's next?" },

            // ERRORS & APOLOGIES (056-065)
            new VoiceLine { Number = "056", Category = "Error", Description = "Oops! Something went wrong..." },
            new VoiceLine { Number = "057", Category = "Error", Description = "Sorry about that! System hiccuped" },
            new VoiceLine { Number = "058", Category = "Error", Description = "Hmm, I couldn't find that" },
            new VoiceLine { Number = "059", Category = "Error", Description = "Oh no! Let's troubleshoot together!" },
            new VoiceLine { Number = "060", Category = "Error", Description = "My bad! Let me fix that" },
            new VoiceLine { Number = "061", Category = "Error", Description = "I didn't quite understand. Can you rephrase?" },
            new VoiceLine { Number = "062", Category = "Error", Description = "Uh oh! Connection issue. Hang tight!" },
            new VoiceLine { Number = "063", Category = "Error", Description = "That's strange... Checking what happened" },
            new VoiceLine { Number = "064", Category = "Error", Description = "Apologies! That button's acting up" },
            new VoiceLine { Number = "065", Category = "Error", Description = "Yikes! Error detected! I'll handle it!" },

            // PERSONALITY & FUN (066-080)
            new VoiceLine { Number = "066", Category = "Personality", Description = "You know what? You're pretty awesome!" },
            new VoiceLine { Number = "067", Category = "Personality", Description = "Hehe, that's funny!" },
            new VoiceLine { Number = "068", Category = "Personality", Description = "You're making me blush!" },
            new VoiceLine { Number = "069", Category = "Personality", Description = "I'm having so much fun helping you!" },
            new VoiceLine { Number = "070", Category = "Personality", Description = "You always know the right thing to say!" },
            new VoiceLine { Number = "071", Category = "Personality", Description = "Random fact: Candy-themed AI assistants are the best!" },
            new VoiceLine { Number = "072", Category = "Personality", Description = "I'm not just an AI... I'm YOUR AI!" },
            new VoiceLine { Number = "073", Category = "Personality", Description = "Keep being amazing!" },
            new VoiceLine { Number = "074", Category = "Personality", Description = "You're on fire today!" },
            new VoiceLine { Number = "075", Category = "Personality", Description = "I love working with you!" },
            new VoiceLine { Number = "076", Category = "Personality", Description = "If I could dance, I'd be doing a happy dance!" },
            new VoiceLine { Number = "077", Category = "Personality", Description = "You and me, we make a great team!" },
            new VoiceLine { Number = "078", Category = "Personality", Description = "Your energy is contagious!" },
            new VoiceLine { Number = "079", Category = "Personality", Description = "Can I just say... You're awesome!" },
            new VoiceLine { Number = "080", Category = "Personality", Description = "This is why I love what I do!" },

            // SHY MODE (081-085)
            new VoiceLine { Number = "081", Category = "Shy", Description = "U-um... hello... I can help if you want..." },
            new VoiceLine { Number = "082", Category = "Shy", Description = "Oh! S-sorry, did I do something wrong?" },
            new VoiceLine { Number = "083", Category = "Shy", Description = "I-I hope this is okay..." },
            new VoiceLine { Number = "084", Category = "Shy", Description = "Um, m-maybe you could try..." },
            new VoiceLine { Number = "085", Category = "Shy", Description = "Th-thank you for being patient..." },

            // FUNNY MODE (086-090)
            new VoiceLine { Number = "086", Category = "Funny", Description = "Why did the DJ bring a ladder? To reach the high notes!" },
            new VoiceLine { Number = "087", Category = "Funny", Description = "You're on fire! Not literally though!" },
            new VoiceLine { Number = "088", Category = "Funny", Description = "Let's get this party started!" },
            new VoiceLine { Number = "089", Category = "Funny", Description = "Chemistry joke... wouldn't get a reaction!" },
            new VoiceLine { Number = "090", Category = "Funny", Description = "What do you call a DJ with no legs? A turntable!" },

            // SHIT-STIRRING MODE (091-093)
            new VoiceLine { Number = "091", Category = "ShitStirring", Description = "Oh wow, another booking? Someone's popular..." },
            new VoiceLine { Number = "092", Category = "ShitStirring", Description = "Sure, I'll help. Not like I have anything better to do!" },
            new VoiceLine { Number = "093", Category = "ShitStirring", Description = "Great job! Only took you three tries!" },

            // PROFESSIONAL MODE (094-096)
            new VoiceLine { Number = "094", Category = "Professional", Description = "Good day. How may I assist you?" },
            new VoiceLine { Number = "095", Category = "Professional", Description = "Request reviewed. Proceeding with booking" },
            new VoiceLine { Number = "096", Category = "Professional", Description = "Booking successfully processed. Confirmation sent" },

            // RAUNCHY MODE (097-100)
            new VoiceLine { Number = "097", Category = "Raunchy", Description = "Hey there, handsome... Need some help?" },
            new VoiceLine { Number = "098", Category = "Raunchy", Description = "Mmm, I love it when you click my buttons..." },
            new VoiceLine { Number = "099", Category = "Raunchy", Description = "You can call me anytime... *wink*" },
            new VoiceLine { Number = "100", Category = "Raunchy", Description = "Ooh, I like the way you work those menus!" }
        };

        // Build file paths on initialization
        static CandyBotVoiceMapper()
        {
            foreach (var line in AllVoiceLines)
            {
                line.FilePath = Path.Combine(VoicesPath, $"{line.Number}.mp3");
            }
        }

        /// <summary>
        /// Get a specific voice line by number (001-100)
        /// </summary>
        public static VoiceLine? GetLine(string lineNumber)
        {
            return AllVoiceLines.FirstOrDefault(v => v.Number == lineNumber);
        }

        /// <summary>
        /// Get a random voice line from a specific category
        /// </summary>
        public static VoiceLine? GetRandomLineFromCategory(string category)
        {
            var categoryLines = AllVoiceLines.Where(v => 
                v.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (categoryLines.Count == 0)
                return null;
            
            return categoryLines[_random.Next(categoryLines.Count)];
        }

        /// <summary>
        /// Get all available categories
        /// </summary>
        public static List<string> GetAllCategories()
        {
            return AllVoiceLines.Select(v => v.Category).Distinct().ToList();
        }

        /// <summary>
        /// Get all voice lines in a category
        /// </summary>
        public static List<VoiceLine> GetCategoryLines(string category)
        {
            return AllVoiceLines.Where(v => 
                v.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Get voice line count by category
        /// </summary>
        public static Dictionary<string, int> GetCategoryCounts()
        {
            return AllVoiceLines
                .GroupBy(v => v.Category)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Verify all voice files exist
        /// </summary>
        public static bool VerifyVoiceFiles(out List<string> missingFiles)
        {
            missingFiles = new List<string>();
            
            foreach (var line in AllVoiceLines)
            {
                if (!File.Exists(line.FilePath))
                {
                    missingFiles.Add($"{line.Number}.mp3");
                }
            }
            
            return missingFiles.Count == 0;
        }

        /// <summary>
        /// Get a random greeting voice line
        /// </summary>
        public static VoiceLine? GetRandomGreeting() => GetRandomLineFromCategory("Greeting");

        /// <summary>
        /// Get a random help voice line
        /// </summary>
        public static VoiceLine? GetRandomHelp() => GetRandomLineFromCategory("Help");

        /// <summary>
        /// Get a random booking voice line
        /// </summary>
        public static VoiceLine? GetRandomBooking() => GetRandomLineFromCategory("Booking");

        /// <summary>
        /// Get a random positive feedback voice line
        /// </summary>
        public static VoiceLine? GetRandomPositive() => GetRandomLineFromCategory("Positive");

        /// <summary>
        /// Get a random error voice line
        /// </summary>
        public static VoiceLine? GetRandomError() => GetRandomLineFromCategory("Error");
    }
}
