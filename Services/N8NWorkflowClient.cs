using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// N8N Workflow Client - Calls N8N workflows to update ElevenLabs agent
    /// This is more reliable than direct API calls because N8N handles:
    /// - Error handling
    /// - Retries
    /// - Logging
    /// - Alternative payload structures
    /// </summary>
    public class N8NWorkflowClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _n8nUrl;

        // N8N webhook URL (update this after creating your workflow)
        private const string DEFAULT_N8N_URL = "http://localhost:5678/webhook/update-elevenlabs-agent";

        public N8NWorkflowClient(string? n8nUrl = null)
        {
            _httpClient = new HttpClient();
            _n8nUrl = n8nUrl ?? DEFAULT_N8N_URL;
            
            System.Diagnostics.Debug.WriteLine($"[N8N Client] Initialized with URL: {_n8nUrl}");
        }

        /// <summary>
        /// Update ElevenLabs agent via N8N workflow
        /// </summary>
        public async Task<bool> UpdateAgentSystemPromptAsync(string systemPrompt)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[N8N Client] ?? Sending update request to N8N...");
                System.Diagnostics.Debug.WriteLine($"[N8N Client] Prompt length: {systemPrompt.Length} characters");

                // Prepare payload
                var payload = new
                {
                    prompt = systemPrompt,
                    timestamp = DateTime.UtcNow.ToString("o"),
                    source = "DJBookingSystem"
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine($"[N8N Client] Sending POST to: {_n8nUrl}");

                // Send to N8N webhook
                var response = await _httpClient.PostAsync(_n8nUrl, content);

                System.Diagnostics.Debug.WriteLine($"[N8N Client] Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[N8N Client] Response: {responseBody}");

                    // Parse response
                    var doc = JsonDocument.Parse(responseBody);
                    if (doc.RootElement.TryGetProperty("success", out var successElement))
                    {
                        var success = successElement.GetBoolean();
                        
                        if (success)
                        {
                            System.Diagnostics.Debug.WriteLine("[N8N Client] ? Agent updated successfully via N8N!");
                            return true;
                        }
                        else
                        {
                            var message = doc.RootElement.TryGetProperty("message", out var msgElement) 
                                ? msgElement.GetString() 
                                : "Unknown error";
                            System.Diagnostics.Debug.WriteLine($"[N8N Client] ? N8N workflow failed: {message}");
                            return false;
                        }
                    }

                    System.Diagnostics.Debug.WriteLine("[N8N Client] ? Request accepted by N8N");
                    return true;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[N8N Client] ? HTTP error: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"[N8N Client] Error details: {error}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[N8N Client] ? Network error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine("[N8N Client] ?? Is N8N running? Check: http://localhost:5678");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[N8N Client] ? Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[N8N Client] Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Test N8N connection
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[N8N Client] ?? Testing N8N connection...");

                // Send test payload
                var payload = new
                {
                    test = true,
                    message = "Connection test from DJ Booking System"
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_n8nUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("[N8N Client] ? N8N is reachable!");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[N8N Client] ? N8N returned: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[N8N Client] ? Cannot reach N8N: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get the optimal system prompt (same as ElevenLabsApiClient)
        /// </summary>
        public static string GetOptimalSystemPrompt()
        {
            return ElevenLabsApiClient.GetOptimalSystemPrompt();
        }
    }
}
