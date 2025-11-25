using System;

namespace DJBookingSystem.Models
{
    /// <summary>
    /// Configuration for an AI API provider
    /// </summary>
    public class AIProviderConfig
    {
        public string Name { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public AIProviderType Type { get; set; }
        public string DefaultModel { get; set; } = string.Empty;
        public int Priority { get; set; } = 1; // Lower = higher priority
        public bool IsEnabled { get; set; } = true;
        public int RequestCount { get; set; } = 0;
        public DateTime? LastUsed { get; set; }
        public DateTime? LastFailed { get; set; }
        public int ConsecutiveFailures { get; set; } = 0;
    }

    /// <summary>
    /// Types of AI providers
    /// </summary>
    public enum AIProviderType
    {
        Claude,           // Anthropic Claude (Sonnet 4.5, etc.)
        MyNinjaAI,        // MyNinja.ai (45+ models)
        OpenAI,           // OpenAI GPT models
        AzureOpenAI,      // Azure OpenAI Service
        LocalLLM,         // Local LLM (Ollama, LM Studio, etc.)
        DiscordBridge,    // Discord bot bridge
        Offline           // Offline mode with canned responses
    }
}
