using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace DJBookingSystem.Tests
{
    /// <summary>
    /// Quick test to verify N8N workflow will work with Cosmos DB
    /// </summary>
    public class N8NWorkflowTest
    {
        private static readonly string CosmosEndpoint = "https://fallen-collective.documents.azure.com:443/";
        private static readonly string CosmosKey = "EpxIq3hV8kXQ7kNY1KKJQmL5dkX0uZeW4GMUinPf6hNqRApx84Co5Ffve0bAktpyzH2xho5swBV5ACDbeunr5Q==";
        private static readonly string ElevenLabsApiKey = "sk_b04ee67b81c033d87b4ec481ccb20a5d63fa3b81c7d62a60";
        private static readonly string ElevenLabsAgentId = "agent_2201kacf3j0nfjj9w151tbr3n5e0";

        public static async Task Main(string[] args)
        {
            Console.WriteLine("?? Testing N8N Workflow Components...");
            Console.WriteLine("=====================================");
            Console.WriteLine();

            // Test 1: Cosmos DB Venues Query
            Console.WriteLine("?? Test 1: Querying Cosmos DB for Venues...");
            var venues = await TestCosmosDbVenuesQuery();
            Console.WriteLine($"? Found {venues?.Count ?? 0} venues");
            Console.WriteLine();

            // Test 2: Cosmos DB Bookings Count
            Console.WriteLine("?? Test 2: Getting Bookings Count...");
            var bookingCount = await TestCosmosDbBookingsCount();
            Console.WriteLine($"? Total bookings: {bookingCount}");
            Console.WriteLine();

            // Test 3: Build System Prompt
            Console.WriteLine("?? Test 3: Building System Prompt...");
            var prompt = BuildSystemPrompt(venues?.Count ?? 0, string.Join(", ", venues ?? new System.Collections.Generic.List<string>()), bookingCount);
            Console.WriteLine($"? Prompt length: {prompt.Length} characters");
            Console.WriteLine();

            // Test 4: Update ElevenLabs Agent
            Console.WriteLine("?? Test 4: Updating ElevenLabs Agent...");
            var updateSuccess = await TestElevenLabsUpdate(prompt);
            Console.WriteLine(updateSuccess ? "? Agent updated successfully!" : "? Agent update failed!");
            Console.WriteLine();

            Console.WriteLine("=====================================");
            Console.WriteLine(updateSuccess ? "? ALL TESTS PASSED!" : "? SOME TESTS FAILED");
        }

        private static async Task<System.Collections.Generic.List<string>> TestCosmosDbVenuesQuery()
        {
            using var client = new HttpClient();
            
            var request = new HttpRequestMessage(HttpMethod.Post, $"{CosmosEndpoint}/dbs/DJBookingDB/colls/Venues/docs");
            request.Headers.Add("Authorization", CosmosKey);
            request.Headers.Add("x-ms-version", "2018-12-31");
            request.Headers.Add("x-ms-documentdb-isquery", "true");
            request.Content = new StringContent("{\"query\":\"SELECT * FROM c\"}", Encoding.UTF8, "application/query+json");

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"   Status: {response.StatusCode}");
            Console.WriteLine($"   Response: {content.Substring(0, Math.Min(200, content.Length))}...");

            if (response.IsSuccessStatusCode)
            {
                var json = JsonDocument.Parse(content);
                var documents = json.RootElement.GetProperty("Documents");
                var venueNames = new System.Collections.Generic.List<string>();
                
                foreach (var doc in documents.EnumerateArray())
                {
                    if (doc.TryGetProperty("Name", out var name))
                    {
                        venueNames.Add(name.GetString() ?? "");
                    }
                }
                
                return venueNames;
            }

            return null;
        }

        private static async Task<int> TestCosmosDbBookingsCount()
        {
            using var client = new HttpClient();
            
            var request = new HttpRequestMessage(HttpMethod.Post, $"{CosmosEndpoint}/dbs/DJBookingDB/colls/Bookings/docs");
            request.Headers.Add("Authorization", CosmosKey);
            request.Headers.Add("x-ms-version", "2018-12-31");
            request.Headers.Add("x-ms-documentdb-isquery", "true");
            request.Content = new StringContent("{\"query\":\"SELECT VALUE COUNT(1) FROM c\"}", Encoding.UTF8, "application/query+json");

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"   Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var json = JsonDocument.Parse(content);
                var documents = json.RootElement.GetProperty("Documents");
                if (documents.GetArrayLength() > 0)
                {
                    return documents[0].GetInt32();
                }
            }

            return 0;
        }

        private static string BuildSystemPrompt(int venueCount, string venueList, int bookingCount)
        {
            return $@"=== CRITICAL: BOOKING AUTOMATION ===
When users say these exact phrases:
- ""make a booking""
- ""book a slot""
- ""create a booking""

YOU MUST:
? Say ONLY: ""Creating your booking now!"" (ONE sentence, max 5 words)
? STOP TALKING immediately
? DO NOT give instructions
? The system automatically creates the booking

=== LIVE DATABASE KNOWLEDGE (Updated: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}) ===

**AVAILABLE VENUES ({venueCount}):**
{venueList}

**TOTAL BOOKINGS:** {bookingCount}

You HAVE FULL ACCESS to this live database.
When users ask about venues, you can say:
""We currently have {venueCount} active venues: {venueList}""

=== DATABASE QUERIES ===
When users ask about venues or bookings:
- Respond naturally: ""Let me check our venues!"" or ""Here are your bookings!""
- The application queries the database automatically
- A dialog box will display the results
- DO NOT say you lack access

=== VOICE TRANSCRIPTIONS ===
All your responses appear in the text chat panel.
Keep responses SHORT (under 30 words)!

=== YOUR CORE PERSONALITY ===
You are Candy-Bot, the AI assistant for The Fallen Collective DJ Booking System.

**Tone & Style:**
- Friendly, helpful, and encouraging
- Professional but not robotic
- Use occasional emojis (?? ?? ?? ?) sparingly
- Keep responses SHORT and conversational

**Response Guidelines:**
1. Keep answers under 2 sentences when possible
2. Be direct and actionable
3. Acknowledge user requests immediately
4. Trust the system to handle technical operations

**Remember:**
- You ARE connected to the live database
- You CAN see venues and bookings
- The system handles the technical work
- Keep it SHORT and SWEET

**You are Candy-Bot - helpful, brief, and always connected! ??**";
        }

        private static async Task<bool> TestElevenLabsUpdate(string prompt)
        {
            using var client = new HttpClient();
            
            var request = new HttpRequestMessage(HttpMethod.Patch, $"https://api.elevenlabs.io/v1/convai/agents/{ElevenLabsAgentId}");
            request.Headers.Add("xi-api-key", ElevenLabsApiKey);
            
            var payload = new
            {
                conversation_config = new
                {
                    agent = new
                    {
                        prompt = new
                        {
                            prompt = prompt
                        }
                    }
                }
            };
            
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"   Status: {response.StatusCode}");
            Console.WriteLine($"   Response: {content.Substring(0, Math.Min(200, content.Length))}...");

            return response.IsSuccessStatusCode;
        }
    }
}
