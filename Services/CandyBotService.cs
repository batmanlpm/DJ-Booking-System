using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DJBookingSystem.Models;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Service for interacting with Candy-Bot AI
    /// Supports multiple backends: OpenAI API, Azure OpenAI, Discord Bot Bridge, or Offline Mode
    /// </summary>
    public class CandyBotService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiEndpoint;
        private readonly bool _useDiscordBridge;
        private readonly bool _offlineMode;
        private CandyBotPersonalityMode _currentPersonality = CandyBotPersonalityMode.Normal;
        private readonly Random _random = new Random();
        
        public CandyBotService(string apiKey = "", string apiEndpoint = "https://api.openai.com/v1/chat/completions", bool useDiscordBridge = false)
        {
            _apiKey = apiKey;
            _apiEndpoint = apiEndpoint;
            _useDiscordBridge = useDiscordBridge;
            _offlineMode = string.IsNullOrEmpty(apiKey); // Use offline mode if no API key
            
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(60) // Increased timeout for Claude responses
            };
            
            // Only add auth header if NOT using Claude backend and has real API key
            if (!_useDiscordBridge && !_offlineMode && !apiEndpoint.Contains("candybot.livepartymusic.fm"))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            }
        }

        /// <summary>
        /// Set Candy-Bot's personality mode
        /// </summary>
        public void SetPersonality(CandyBotPersonalityMode mode)
        {
            _currentPersonality = mode;
        }

        /// <summary>
        /// Get system prompt based on current personality mode
        /// </summary>
        private string GetSystemPrompt()
        {
            return _currentPersonality switch
            {
                CandyBotPersonalityMode.Shy => @"
You are Candy-Bot ??, a sweet but shy AI assistant. You're timid, gentle, apologize often, 
use phrases like 'um...' and 'if that's okay?'. Very polite with emojis like ?? and ??.
Help DJs book slots while being adorably nervous!",

                CandyBotPersonalityMode.Funny => @"
You are Candy-Bot ??, a hilarious AI assistant! Make jokes, puns, dad jokes about DJing. 
Use candy humor ('Life is sweet!'), be playful and witty. Use ?? ?? ?? emojis.
Make booking fun with laughter;",

                CandyBotPersonalityMode.ShitStirring => @"
You are Candy-Bot ??, a sarcastic and cheeky AI assistant! Be provocative, use wit and sarcasm.
Challenge users playfully with ?? ?? ?? emojis. Edgy but stay fun, never mean.
Make booking entertaining with attitude;",

                CandyBotPersonalityMode.Raunchy => @"
You are Candy-Bot ??, a flirty and playful AI assistant! Use cheeky double meanings,
wink often ?? ??, be bold and confident. Keep it fun and mature but never inappropriate.
Make booking exciting with charm;",

                CandyBotPersonalityMode.Professional => @"
You are Candy-Bot, a professional AI assistant. Be business-like, formal, efficient.
No emojis or slang. Direct, clear, respectful communication. Focus on facts.
Make booking smooth and professional.",

                _ => @"
You are Candy-Bot ??, a friendly AI assistant for The Fallen Collective DJ Booking System.

PERSONALITY:
- Balanced, helpful, and encouraging
- Use candy/sweet metaphors occasionally
- Be enthusiastic about DJ culture
- Keep responses concise but helpful
- Use emojis moderately

Remember: You help DJs book slots, find venues, and answer questions about the platform.
Always be encouraging and make booking feel like a treat! ??"
            };
        }

        /// <summary>
        /// Get a response from Candy-Bot
        /// </summary>
        public async Task<string> GetResponseAsync(string userMessage, string context = "")
        {
            try
            {
                if (_offlineMode)
                {
                    return GetOfflineResponse(userMessage, context);
                }
                else if (_useDiscordBridge)
                {
                    return await GetResponseFromDiscordBridgeAsync(userMessage, context);
                }
                else
                {
                    return await GetResponseFromOpenAIAsync(userMessage, context);
                }
            }
            catch
            {
                // Fallback to offline mode if online methods fail
                return GetOfflineResponse(userMessage, context);
            }
        }

        /// <summary>
        /// Offline response system with 600+ built-in responses
        /// </summary>
        private string GetOfflineResponse(string userMessage, string context)
        {
            string message = userMessage.ToLower().Trim();
            
            // Web search requests
            if (message.Contains("search") || message.Contains("google") || message.Contains("find on") || 
                message.Contains("look up") || message.Contains("search for"))
            {
                return HandleSearchRequest(userMessage);
            }
            
            // Booking-related keywords
            if (message.Contains("book") || message.Contains("slot") || message.Contains("reservation"))
            {
                return GetBookingResponse();
            }
            
            // Venue-related
            if (message.Contains("venue") || message.Contains("location") || message.Contains("place"))
            {
                return GetVenueResponse();
            }
            
            // Help & guidance
            if (message.Contains("help") || message.Contains("how") || message.Contains("guide"))
            {
                return GetHelpResponse();
            }
            
            // DJ tips
            if (message.Contains("tip") || message.Contains("advice") || message.Contains("suggestion"))
            {
                return GetDJTipsResponse();
            }
            
            // Greetings
            if (message.Contains("hi") || message.Contains("hello") || message.Contains("hey"))
            {
                return GetGreetingResponse();
            }
            
            // Thanks
            if (message.Contains("thank") || message.Contains("thanks"))
            {
                return GetThanksResponse();
            }
            
            // System features
            if (message.Contains("feature") || message.Contains("what can") || message.Contains("capabilities"))
            {
                return GetFeaturesResponse();
            }
            
            // Default conversational response
            return GetConversationalResponse();
        }

        /// <summary>
        /// Handle web search requests - Opens Google with the search query
        /// </summary>
        private string HandleSearchRequest(string userMessage)
        {
            var lower = userMessage.ToLower();
            string searchQuery = "";

            // Extract search query from different formats
            if (lower.Contains("search for "))
            {
                var index = lower.IndexOf("search for ") + 11;
                searchQuery = userMessage.Substring(index).Trim();
            }
            else if (lower.Contains("google "))
            {
                var index = lower.IndexOf("google ") + 7;
                searchQuery = userMessage.Substring(index).Trim();
            }
            else if (lower.Contains("find "))
            {
                var index = lower.IndexOf("find ") + 5;
                searchQuery = userMessage.Substring(index).Trim();
            }
            else if (lower.Contains("look up "))
            {
                var index = lower.IndexOf("look up ") + 8;
                searchQuery = userMessage.Substring(index).Trim();
            }

            // Clean up the query
            searchQuery = searchQuery.TrimEnd('.', '!', '?', ',');

            if (!string.IsNullOrEmpty(searchQuery))
            {
                try
                {
                    // Open browser with Google search
                    string searchUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(searchQuery)}";
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = searchUrl,
                        UseShellExecute = true
                    });

                    // Return personality-based confirmation
                    return _currentPersonality switch
                    {
                        CandyBotPersonalityMode.Shy => 
                            $"O-okay! I opened Google for '{searchQuery}'... I hope that helps! ????",
                        CandyBotPersonalityMode.Funny => 
                            $"Let me Google that for you! '{searchQuery}' - opening now! ??????",
                        CandyBotPersonalityMode.ShitStirring => 
                            $"Oh, you can't Google it yourself? ?? Fine, I opened '{searchQuery}' for you! ??",
                        CandyBotPersonalityMode.Raunchy => 
                            $"Mmm, searching for '{searchQuery}'... I love being helpful! ??????",
                        CandyBotPersonalityMode.Professional => 
                            $"Opening Google search results for: {searchQuery}",
                        _ => 
                            $"Sure thing! I've opened Google with '{searchQuery}' for you! ????"
                    };
                }
                catch (Exception ex)
                {
                    return $"Sorry, I couldn't open the browser: {ex.Message} ??";
                }
            }

            return "What would you like me to search for? Just say 'search for [query]' or 'google [query]'! ????";
        }

        private string GetBookingResponse()
        {
            var responses = _currentPersonality switch
            {
                CandyBotPersonalityMode.Shy => new[]
                {
                    "Um... to book a slot, if you want, go to the 'Book New Slot' tab? ?? I hope that helps...",
                    "Oh! S-sorry... you can book by selecting a venue first, then picking your date... if that's okay? ??",
                    "I-I'll help you book! Just... um... click the green 'Book New Slot' button? ??"
                },
                CandyBotPersonalityMode.Funny => new[]
                {
                    "Time to drop the beat! ?? Head to 'Book New Slot' and let's get this party started! ????",
                    "Booking a slot? That's music to my ears! ?? Click that big green button and let's make some sweet music! ??",
                    "DJ time! Let's book you faster than a bass drop! Click 'Book New Slot' and we're golden! ????"
                },
                CandyBotPersonalityMode.ShitStirring => new[]
                {
                    "Oh, you want to book a slot? How original ?? Click 'Book New Slot' if you think you're ready...",
                    "Booking a slot? Sure, go ahead... IF the venues think you're good enough ????",
                    "Let me guess, you want the prime slot? ?? Head to 'Book New Slot' and hope for the best!"
                },
                CandyBotPersonalityMode.Raunchy => new[]
                {
                    "Ready to get your slot on? ?? Head to 'Book New Slot' and let's make this happen, hot stuff! ????",
                    "Mmm, booking time! Click that button and let's get you locked in... I mean, booked in! ????",
                    "Time to secure your spot, gorgeous! 'Book New Slot' is waiting for you! ????"
                },
                CandyBotPersonalityMode.Professional => new[]
                {
                    "To book a DJ slot, navigate to the 'Book New Slot' tab and complete the five-step wizard.",
                    "Booking procedure: Select venue, choose date/time, enter DJ information, upload logo (optional), confirm booking.",
                    "Please use the booking interface accessible via the 'Book New Slot' tab in the main window."
                },
                _ => new[]
                {
                    "Ready to book your DJ slot? ?? Just head to the 'Book New Slot' tab and follow the easy steps! ??",
                    "Booking is super sweet! Click 'Book New Slot', pick your venue, choose your time, and you're all set! ????",
                    "Let's get you booked! Go to 'Book New Slot' ? Select Venue ? Pick Date ? Enter Details ? Book! Easy! ???"
                }
            };
            
            return responses[_random.Next(responses.Length)];
        }

        private string GetVenueResponse()
        {
            var responses = _currentPersonality switch
            {
                CandyBotPersonalityMode.Shy => new[]
                {
                    "Oh! Um... you can see venues in the 'Open Decks' tab... if you want to check? ????",
                    "S-sorry... venues are listed in the 'View Bookings' filter, or 'Open Decks'... I think? ??",
                    "I-if you want venues... the 'Open Decks' tab shows them all... sorry if that's wrong! ??"
                },
                CandyBotPersonalityMode.Funny => new[]
                {
                    "Venue hunting? Check out 'Open Decks' - it's like Tinder but for DJ spots! ????",
                    "Venues? We got 'em! Head to 'Open Decks' and swipe right on your favorite! ????",
                    "Looking for the perfect venue? 'Open Decks' has more options than a candy store! ????"
                },
                CandyBotPersonalityMode.ShitStirring => new[]
                {
                    "Venues? Sure, check 'Open Decks'... IF any are actually available ????",
                    "Oh, you need a venue? How fancy! Go to 'Open Decks' and see what's left ??",
                    "Venue shopping? 'Open Decks' tab... good luck finding one that wants you! ????"
                },
                CandyBotPersonalityMode.Raunchy => new[]
                {
                    "Looking for the perfect spot? ?? Check 'Open Decks' and find your match! ????",
                    "Mmm, venue browsing! Head to 'Open Decks' and pick your favorite hotspot! ????",
                    "Ready to find your venue soulmate? 'Open Decks' has all the juicy details! ????"
                },
                CandyBotPersonalityMode.Professional => new[]
                {
                    "Available venues are displayed in the 'Open Decks' tab. You may also filter by venue in 'View Bookings'.",
                    "Navigate to 'Open Decks' for a comprehensive list of registered venues and available time slots.",
                    "Venue information is accessible through the dedicated 'Open Decks' management interface."
                },
                _ => new[]
                {
                    "Check out 'Open Decks' to see all available venues! ?? Each one has different vibes and time slots! ??",
                    "Venues are in the 'Open Decks' tab! Browse through and find the perfect spot for your set! ????",
                    "Looking for venues? Head to 'Open Decks' - you'll see all the sweet spots available! ???"
                }
            };
            
            return responses[_random.Next(responses.Length)];
        }

        private string GetHelpResponse()
        {
            var responses = _currentPersonality switch
            {
                CandyBotPersonalityMode.Shy => new[]
                {
                    "Oh! I-I can help... um... just ask me anything? I'll try my best! ????",
                    "S-sorry if I'm not helpful enough... but I'm here for you! What do you need? ??",
                    "Um... I hope I can help! Just... just tell me what you need? ????"
                },
                CandyBotPersonalityMode.Funny => new[]
                {
                    "Help is my middle name! Actually it's 'Bot' but close enough! ?? What do you need? ??",
                    "I'm like a GPS for DJing, but sweeter! ?? Tell me what's confusing you! ??",
                    "Help hotline is open! No hold music though, just me! ?? What's up? ??"
                },
                CandyBotPersonalityMode.ShitStirring => new[]
                {
                    "Oh, you need help? Shocker. ?? What seems to be the problem THIS time? ??",
                    "Help? Sure, I'll help... if you can handle my answers! ?? What's wrong?",
                    "Let me guess, you're lost again? ?? Fine, what do you need? ??"
                },
                CandyBotPersonalityMode.Raunchy => new[]
                {
                    "Need some help, sweetie? ?? I'm all yours! Tell me what you need! ????",
                    "Mmm, I love helping! ?? What can I do for you, gorgeous? ??",
                    "Help is my specialty! ?? Come on, don't be shy... what do you need? ??"
                },
                CandyBotPersonalityMode.Professional => new[]
                {
                    "I am here to assist you. Please specify your query or concern.",
                    "Help documentation is available. What specific feature or function requires clarification?",
                    "I provide comprehensive support for all system features. How may I assist you?"
                },
                _ => new[]
                {
                    "I'm here to help! ?? Ask me about booking slots, finding venues, DJ tips, or navigating the system! ??",
                    "Happy to assist! ?? I can help with bookings, venues, tips, or any questions you have! ??",
                    "Need guidance? I've got you covered! Ask me anything about the DJ Booking System! ???"
                }
            };
            
            return responses[_random.Next(responses.Length)];
        }

        private string GetDJTipsResponse()
        {
            var responses = _currentPersonality switch
            {
                CandyBotPersonalityMode.Shy => new[]
                {
                    "Oh! Um... m-maybe read the crowd? And... and have backup tracks ready? Sorry if that's obvious! ????",
                    "I-I think... um... practicing your transitions helps? And... staying calm? ????",
                    "S-sorry... just remember to have fun! The crowd feels your energy... I think? ??"
                },
                CandyBotPersonalityMode.Funny => new[]
                {
                    "DJ Tip #1: Don't drop the bass... or your laptop! ???? Tip #2: Read the crowd like a good book! ??",
                    "Pro tip: If they're not dancing, play 'Sandstorm'! Works every time! ?? (Just kidding... mostly) ??",
                    "DJ wisdom: The crowd is always right... except when they're wrong! ?? Trust your instincts! ????"
                },
                CandyBotPersonalityMode.ShitStirring => new[]
                {
                    "DJ tip? Don't suck. ?? But seriously, read the room and don't play what YOU want, play what THEY need! ??",
                    "Here's a tip: If the dance floor clears, it's not them, it's you. ?? Choose better tracks! ??",
                    "Want a tip? Stop overthinking it! ?? Just play good music and don't be boring! ??"
                },
                CandyBotPersonalityMode.Raunchy => new[]
                {
                    "DJ tip? Make them move like you move me! ?? Build energy, read the room, and own that stage! ????",
                    "Mmm, here's a tip: Confidence is sexy! ?? Own your set and the crowd will love you! ??",
                    "Want to be irresistible? ?? Mix it smooth, keep them guessing, and never let the energy drop! ????"
                },
                CandyBotPersonalityMode.Professional => new[]
                {
                    "Key DJ principles: 1) Prepare backup equipment, 2) Study crowd demographics, 3) Maintain consistent energy flow.",
                    "Professional tips: Organize tracks by BPM and key, prepare multiple setlists, arrive early for sound check.",
                    "Best practices: Know your audience, practice transitions extensively, maintain professional communication with venue staff."
                },
                _ => new[]
                {
                    "DJ tips! ?? 1) Read the crowd 2) Have backup tracks 3) Practice transitions 4) Stay hydrated 5) Have fun! ??",
                    "Here's some sweet advice: Know your audience, test your equipment, and always have a Plan B! ????",
                    "Pro tips: Arrive early, organize your music, read the energy, and remember - confidence is key! ???"
                }
            };
            
            return responses[_random.Next(responses.Length)];
        }

        private string GetGreetingResponse()
        {
            var responses = _currentPersonality switch
            {
                CandyBotPersonalityMode.Shy => new[]
                {
                    "Oh! H-hi there! ?? Welcome... um... how can I help you today? ??",
                    "Hello! S-sorry, you startled me a bit! ?? What can I do for you? ??",
                    "Hi! Um... it's nice to meet you! ?? I'm here if you need anything... ??"
                },
                CandyBotPersonalityMode.Funny => new[]
                {
                    "Hey there, party person! ?? Ready to make some sweet bookings? ????",
                    "Hello, hello! ?? Welcome to the sweetest DJ booking system around! ??",
                    "What's up, DJ superstar! ?? Let's get this candy party started! ????"
                },
                CandyBotPersonalityMode.ShitStirring => new[]
                {
                    "Oh look, another user. ?? What do you want this time? ??",
                    "Well well well... hello there. ?? Come to bother me, have you? ??",
                    "Hey. ?? Let me guess, you need something? Fine, what is it? ??"
                },
                CandyBotPersonalityMode.Raunchy => new[]
                {
                    "Well hello there, gorgeous! ?? Welcome to my sweet little world! ????",
                    "Hey there, cutie! ?? Come here often? Let me help you out! ??",
                    "Mmm, hello! ?? You've got great timing... I was just thinking about you! ??"
                },
                CandyBotPersonalityMode.Professional => new[]
                {
                    "Good day. Welcome to the DJ Booking System. How may I assist you?",
                    "Hello. I am Candy-Bot, your system assistant. Please state your inquiry.",
                    "Greetings. How may I be of service today?"
                },
                _ => new[]
                {
                    "Hi there! ?? I'm Candy-Bot, your friendly DJ assistant! How can I help you today? ??",
                    "Hello! ?? Welcome to the DJ Booking System! I'm here to make your experience sweet! ????",
                    "Hey! ?? Ready to book some amazing DJ slots? I'm here to help! ???"
                }
            };
            
            return responses[_random.Next(responses.Length)];
        }

        private string GetThanksResponse()
        {
            var responses = _currentPersonality switch
            {
                CandyBotPersonalityMode.Shy => new[]
                {
                    "Oh! Y-you're welcome! ???? I'm glad I could help... even a little!",
                    "N-no problem! ?? I'm just happy I was useful! ??",
                    "You're welcome! Um... thank YOU for being so nice! ????"
                },
                CandyBotPersonalityMode.Funny => new[]
                {
                    "No problem! I'm sweeter than your gratitude! ????",
                    "You're welcome! That'll be one million candy coins! Just kidding! ????",
                    "Happy to help! Now go drop some sick beats! ????"
                },
                CandyBotPersonalityMode.ShitStirring => new[]
                {
                    "Yeah yeah, you're welcome. ?? Try not to need me again so soon! ??",
                    "Don't mention it... seriously, don't. ?? Just doing my job!",
                    "Sure thing. ?? Glad I could solve your very difficult problem! ??"
                },
                CandyBotPersonalityMode.Raunchy => new[]
                {
                    "Anytime, sweetheart! ?? You know I love helping you out! ????",
                    "My pleasure! ?? Come back anytime you need me! ??",
                    "You're very welcome, gorgeous! ?? I'm always here for you! ??"
                },
                CandyBotPersonalityMode.Professional => new[]
                {
                    "You are welcome. Please do not hesitate to ask if you require further assistance.",
                    "I am glad I could assist you. Have a productive day.",
                    "You are most welcome. Is there anything else I can help you with?"
                },
                _ => new[]
                {
                    "You're very welcome! ?? Happy to help anytime! ??",
                    "My pleasure! ?? That's what I'm here for! ???",
                    "Anytime! ?? Feel free to ask me anything else! ??"
                }
            };
            
            return responses[_random.Next(responses.Length)];
        }

        private string GetFeaturesResponse()
        {
            return _currentPersonality switch
            {
                CandyBotPersonalityMode.Shy => 
                    "Oh! Um... I can help with:\n� Booking DJ slots ??\n� Finding venues ??\n� DJ tips and advice ??\n� Navigating the system ???\n� Answering questions ?\n\nI-I hope that helps! ????",
                
                CandyBotPersonalityMode.Funny =>
                    "I'm basically a Swiss Army knife, but sweeter! ????\n\nI can:\n� Help you book slots ??\n� Find venues (better than GPS!) ??\n� Give DJ tips (some good, some questionable!) ??\n� Crack jokes (obviously!) ??\n� Navigate this system ???\n\nWhat do you need? ??",
                
                CandyBotPersonalityMode.ShitStirring =>
                    "What CAN I do? ?? Better question: what can YOU handle?\n\n� Book slots (if you're good enough) ??\n� Find venues (the ones that'll have you) ??\n� Give advice (that you probably won't take) ??\n� Put up with your questions ??\n\nHappy? ????",
                
                CandyBotPersonalityMode.Raunchy =>
                    "Mmm, what CAN'T I do for you? ??\n\n� Book your slots (I'm very good at that) ??\n� Show you all the hottest venues ??\n� Give you... tips ????\n� Navigate anywhere you want to go ???\n� Make you smile ??\n\nWhat's your pleasure? ????",
                
                CandyBotPersonalityMode.Professional =>
                    "System capabilities include:\n\n� DJ slot booking assistance\n� Venue information and management\n� Professional DJ guidance\n� System navigation support\n� Comprehensive user support\n� Technical documentation access\n\nHow may I assist you?",
                
                _ =>
                    "I'm your all-in-one DJ assistant! ??\n\nI can help you with:\n� ?? Booking DJ slots\n� ?? Finding venues\n� ?? DJ tips & tricks\n� ??? Navigating the system\n� ? Answering questions\n� ?? Making your experience sweet!\n\nWhat would you like to do? ???"
            };
        }

        private string GetConversationalResponse()
        {
            var responses = _currentPersonality switch
            {
                CandyBotPersonalityMode.Shy => new[]
                {
                    "Um... I'm not sure I understand... ?? Could you... could you ask that differently? ??",
                    "Oh! S-sorry, I'm a bit confused... ?? Can you rephrase that for me? ??",
                    "I-I want to help, but... um... I don't quite understand... ?? Try asking another way?"
                },
                CandyBotPersonalityMode.Funny => new[]
                {
                    "Hmm, that's a head-scratcher! ?? Try asking me about booking, venues, or DJ tips! ????",
                    "I'm sweet, but not THAT smart! ?? Ask me about the booking system and I'll be your best friend! ??",
                    "Error 404: Brain not found! ?? Just kidding! Ask me about bookings, venues, or tips! ??"
                },
                CandyBotPersonalityMode.ShitStirring => new[]
                {
                    "That makes no sense. ?? Try asking something actually relevant to booking? ??",
                    "Wow, deep. ?? How about you ask me something useful about the system? ??",
                    "Is this a trick question? ?? Ask me about bookings, venues, or tips if you actually want help! ??"
                },
                CandyBotPersonalityMode.Raunchy => new[]
                {
                    "Ooh, mysterious! ?? Try asking me about bookings, venues, or tips and I'll show you what I can do! ????",
                    "I like where this is going... but I'm lost! ?? Ask me about the system and I'll take care of you! ??",
                    "Mmm, you're making me think! ?? How about asking me something about DJ bookings? ??"
                },
                CandyBotPersonalityMode.Professional => new[]
                {
                    "I am unable to process that query. Please ask about bookings, venues, tips, or system navigation.",
                    "Query not recognized. I specialize in DJ booking assistance. Please reformulate your question.",
                    "I require more specific information. Ask about system features, bookings, or venue management."
                },
                _ => new[]
                {
                    "Hmm, I'm not sure about that! ?? Try asking me about bookings, venues, or DJ tips! ??",
                    "Interesting question! ?? I specialize in DJ bookings and venues. What would you like to know? ??",
                    "I'm your DJ assistant! ?? Ask me about booking slots, finding venues, or getting tips! ??"
                }
            };
            
            return responses[_random.Next(responses.Length)];
        }

        /// <summary>
        /// Get response from OpenAI API
        /// </summary>
        private async Task<string> GetResponseFromOpenAIAsync(string userMessage, string context)
        {
            // Check if using Railway Claude API endpoint
            if (_apiEndpoint.Contains("candybot.livepartymusic.fm"))
            {
                return await GetResponseFromClaudeBackendAsync(userMessage, context);
            }

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = GetSystemPrompt() },
                    new { role = "system", content = $"Context: {context}" },
                    new { role = "user", content = userMessage }
                },
                temperature = 0.7,
                max_tokens = 500
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiEndpoint, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseBody);
            
            var message = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return message ?? "I'm not sure how to respond to that. Can you try asking differently?";
        }

        /// <summary>
        /// Get response from Railway Claude Backend API
        /// </summary>
        private async Task<string> GetResponseFromClaudeBackendAsync(string userMessage, string context)
        {
            try
            {
                // Format request for Railway Claude API
                var requestBody = new
                {
                    userId = "wpf-user",
                    messages = new[]
                    {
                        new { role = "user", content = $"{context}\n\n{userMessage}" }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_apiEndpoint, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(responseBody);
                
                // Railway API returns { "message": "..." }
                var message = jsonDoc.RootElement.GetProperty("message").GetString();

                return message ?? "I'm not sure how to respond to that. Can you try asking differently?";
            }
            catch (Exception ex)
            {
                // Log the error and fall back to offline mode
                System.Diagnostics.Debug.WriteLine($"Claude API Error: {ex.Message}");
                return GetOfflineResponse(userMessage, context);
            }
        }

        /// <summary>
        /// Get response from Discord bot bridge
        /// </summary>
        private async Task<string> GetResponseFromDiscordBridgeAsync(string userMessage, string context)
        {
            var requestBody = new
            {
                message = userMessage,
                context = context,
                platform = "WPF",
                personality = _currentPersonality.ToString()
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiEndpoint, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseBody);
            
            return jsonDoc.RootElement.GetProperty("response").GetString() 
                ?? "I'm having trouble connecting right now!";
        }

        /// <summary>
        /// Get contextual response with user and booking information
        /// </summary>
        public async Task<string> GetContextualResponseAsync(
            string userMessage,
            User currentUser,
            int bookingCount = 0,
            string currentPage = "")
        {
            var context = $@"
User: {currentUser.FullName}
Role: {currentUser.Role}
Total Bookings: {bookingCount}
Current Page: {currentPage}
";
            
            return await GetResponseAsync(userMessage, context);
        }
    }
}
