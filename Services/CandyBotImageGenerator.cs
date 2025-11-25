using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Candy-Bot Image Generation Service
    /// Supports multiple AI image providers: OpenAI DALL-E, Stable Diffusion, etc.
    /// </summary>
    public class CandyBotImageGenerator
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiEndpoint;
        private readonly ImageProvider _provider;
        private readonly CandyBotSoundManager? _soundManager;

        public enum ImageProvider
        {
            OpenAI_DALLE3,
            OpenAI_DALLE2,
            StableDiffusion,
            LocalGeneration
        }

        public CandyBotImageGenerator(
            string apiKey = "",
            ImageProvider provider = ImageProvider.OpenAI_DALLE3,
            CandyBotSoundManager? soundManager = null)
        {
            // Use provided API key, or fall back to hardcoded key if empty
            _apiKey = !string.IsNullOrEmpty(apiKey) 
                ? apiKey 
                : "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";
            
            _provider = provider;
            _soundManager = soundManager;
            
            _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
            
            _apiEndpoint = provider switch
            {
                ImageProvider.OpenAI_DALLE3 => "https://api.openai.com/v1/images/generations",
                ImageProvider.OpenAI_DALLE2 => "https://api.openai.com/v1/images/generations",
                ImageProvider.StableDiffusion => "https://api.stability.ai/v1/generation/stable-diffusion-xl-1024-v1-0/text-to-image",
                _ => ""
            };

            if (!string.IsNullOrEmpty(_apiKey) && provider != ImageProvider.LocalGeneration)
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }
        }

        /// <summary>
        /// Generate an image from text prompt
        /// </summary>
        public async Task<ImageGenerationResult> GenerateImageAsync(
            string prompt,
            string? outputFolder = null,
            ImageSize size = ImageSize.Square_1024,
            ImageQuality quality = ImageQuality.Standard)
        {
            try
            {
                // Voice feedback - starting
                _soundManager?.PlayVoiceLine("032"); // "Let's get started!"

                var result = new ImageGenerationResult
                {
                    Prompt = prompt,
                    Provider = _provider.ToString(),
                    StartTime = DateTime.Now
                };

                Debug.WriteLine($"[CandyBot ImageGen] Generating: {prompt}");

                if (_provider == ImageProvider.LocalGeneration)
                {
                    result.Success = false;
                    result.ErrorMessage = "Local generation not yet implemented. Please use API provider.";
                    _soundManager?.PlayVoiceLine("056"); // Error
                    return result;
                }

                // Generate based on provider
                switch (_provider)
                {
                    case ImageProvider.OpenAI_DALLE3:
                    case ImageProvider.OpenAI_DALLE2:
                        result = await GenerateOpenAIImageAsync(prompt, size, quality, result);
                        break;
                    
                    case ImageProvider.StableDiffusion:
                        result = await GenerateStableDiffusionImageAsync(prompt, size, result);
                        break;
                }

                // Save image if generation succeeded
                if (result.Success && !string.IsNullOrEmpty(result.ImageUrl))
                {
                    outputFolder = outputFolder ?? Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                        "CandyBot-Generated"
                    );

                    Directory.CreateDirectory(outputFolder);
                    
                    var fileName = $"CandyBot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    var filePath = Path.Combine(outputFolder, fileName);

                    await DownloadImageAsync(result.ImageUrl, filePath);
                    result.LocalFilePath = filePath;

                    Debug.WriteLine($"[CandyBot ImageGen] Saved to: {filePath}");
                    
                    // Voice feedback - success
                    _soundManager?.PlayVoiceLine("048"); // "Boom! Task completed!"
                }
                else
                {
                    // Voice feedback - error
                    _soundManager?.PlayVoiceLine("056"); // "Oops! Something went wrong..."
                }

                result.EndTime = DateTime.Now;
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CandyBot ImageGen] Error: {ex.Message}");
                _soundManager?.PlayVoiceLine("056"); // Error voice
                
                return new ImageGenerationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Prompt = prompt,
                    EndTime = DateTime.Now
                };
            }
        }

        /// <summary>
        /// Generate image using OpenAI DALL-E
        /// </summary>
        private async Task<ImageGenerationResult> GenerateOpenAIImageAsync(
            string prompt, 
            ImageSize size, 
            ImageQuality quality,
            ImageGenerationResult result)
        {
            var sizeStr = size switch
            {
                ImageSize.Square_1024 => "1024x1024",
                ImageSize.Portrait_1024x1792 => "1024x1792",
                ImageSize.Landscape_1792x1024 => "1792x1024",
                _ => "1024x1024"
            };

            var requestBody = new
            {
                model = _provider == ImageProvider.OpenAI_DALLE3 ? "dall-e-3" : "dall-e-2",
                prompt = prompt,
                n = 1,
                size = sizeStr,
                quality = quality == ImageQuality.HD ? "hd" : "standard"
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiEndpoint, content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var jsonDoc = JsonDocument.Parse(responseText);
                var imageUrl = jsonDoc.RootElement
                    .GetProperty("data")[0]
                    .GetProperty("url")
                    .GetString();

                result.ImageUrl = imageUrl ?? string.Empty;
                result.Success = true;
            }
            else
            {
                result.Success = false;
                
                // Try to parse error message from JSON
                try
                {
                    var errorDoc = JsonDocument.Parse(responseText);
                    if (errorDoc.RootElement.TryGetProperty("error", out var errorElement))
                    {
                        var errorMessage = errorElement.TryGetProperty("message", out var msgElement) 
                            ? msgElement.GetString() 
                            : "Unknown error";
                        
                        var errorType = errorElement.TryGetProperty("type", out var typeElement) 
                            ? typeElement.GetString() 
                            : "unknown";
                        
                        var errorCode = errorElement.TryGetProperty("code", out var codeElement) 
                            ? codeElement.GetString() 
                            : "unknown";
                        
                        // Build user-friendly error message
                        result.ErrorMessage = $"{{\n" +
                            $"  \"error\": {{\n" +
                            $"    \"message\": \"{errorMessage}\",\n" +
                            $"    \"type\": \"{errorType}\",\n" +
                            $"    \"code\": \"{errorCode}\"\n" +
                            $"  }}\n" +
                            $"}}";
                        
                        Debug.WriteLine($"[CandyBot ImageGen] API Error - Code: {errorCode}, Type: {errorType}, Message: {errorMessage}");
                    }
                    else
                    {
                        result.ErrorMessage = responseText;
                    }
                }
                catch
                {
                    // If parsing fails, use raw response
                    result.ErrorMessage = responseText;
                }
            }

            return result;
        }

        /// <summary>
        /// Generate image using Stable Diffusion
        /// </summary>
        private Task<ImageGenerationResult> GenerateStableDiffusionImageAsync(
            string prompt,
            ImageSize size,
            ImageGenerationResult result)
        {
            // Stable Diffusion implementation
            result.Success = false;
            result.ErrorMessage = "Stable Diffusion implementation coming soon. Use OpenAI DALL-E for now.";
            return Task.FromResult(result);
        }

        /// <summary>
        /// Download generated image from URL
        /// </summary>
        private async Task DownloadImageAsync(string imageUrl, string savePath)
        {
            var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
            await File.WriteAllBytesAsync(savePath, imageBytes);
        }

        /// <summary>
        /// Generate multiple images from a prompt
        /// </summary>
        public async Task<ImageGenerationBatchResult> GenerateImageBatchAsync(
            string[] prompts,
            string? outputFolder = null,
            ImageSize size = ImageSize.Square_1024,
            ImageQuality quality = ImageQuality.Standard)
        {
            var batch = new ImageGenerationBatchResult
            {
                TotalImages = prompts.Length,
                StartTime = DateTime.Now
            };

            _soundManager?.PlayVoiceLine("032"); // "Let's get started!"

            foreach (var prompt in prompts)
            {
                var result = await GenerateImageAsync(prompt, outputFolder, size, quality);
                batch.Results.Add(result);
                
                if (result.Success)
                    batch.SuccessfulImages++;
                else
                    batch.FailedImages++;
            }

            batch.EndTime = DateTime.Now;
            
            if (batch.SuccessfulImages == batch.TotalImages)
            {
                _soundManager?.PlayVoiceLine("053"); // "Amazing work! I'm impressed!"
            }
            else if (batch.SuccessfulImages > 0)
            {
                _soundManager?.PlayVoiceLine("042"); // "Perfect!"
            }
            else
            {
                _soundManager?.PlayVoiceLine("056"); // Error
            }

            return batch;
        }

        /// <summary>
        /// Generate DJ-themed image presets
        /// </summary>
        public async Task<ImageGenerationResult> GenerateDJImageAsync(DJImagePreset preset, string? outputFolder = null)
        {
            var prompt = preset switch
            {
                DJImagePreset.NeonClubScene => "Cyberpunk DJ booth with neon green lights, turntables glowing, dark atmospheric club background, futuristic space theme",
                DJImagePreset.VinylCollection => "Collection of colorful vinyl records with neon green accents, DJ culture, artistic arrangement",
                DJImagePreset.LivePerformance => "DJ performing live at a packed club, crowd silhouettes, neon lighting, energetic atmosphere",
                DJImagePreset.StudioSetup => "Professional DJ home studio setup, equipment with neon green LED lights, space-themed decor",
                DJImagePreset.EventPoster => "Electronic music event poster, cyberpunk style, neon green and black color scheme, DJ silhouette",
                _ => "DJ-related artwork with neon green accents on black background"
            };

            return await GenerateImageAsync(prompt, outputFolder);
        }
    }

    #region Supporting Classes

    /// <summary>
    /// Image generation result
    /// </summary>
    public class ImageGenerationResult
    {
        public bool Success { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string LocalFilePath { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
    }

    /// <summary>
    /// Batch generation result
    /// </summary>
    public class ImageGenerationBatchResult
    {
        public System.Collections.Generic.List<ImageGenerationResult> Results { get; set; } = new();
        public int TotalImages { get; set; }
        public int SuccessfulImages { get; set; }
        public int FailedImages { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TotalDuration => EndTime - StartTime;
    }

    /// <summary>
    /// Image size options
    /// </summary>
    public enum ImageSize
    {
        Square_1024,           // 1024x1024
        Portrait_1024x1792,    // 1024x1792 (DALL-E 3 only)
        Landscape_1792x1024    // 1792x1024 (DALL-E 3 only)
    }

    /// <summary>
    /// Image quality options
    /// </summary>
    public enum ImageQuality
    {
        Standard,  // Faster, cheaper
        HD         // Higher quality (DALL-E 3 only)
    }

    /// <summary>
    /// DJ-themed image presets
    /// </summary>
    public enum DJImagePreset
    {
        NeonClubScene,
        VinylCollection,
        LivePerformance,
        StudioSetup,
        EventPoster
    }

    #endregion
}
