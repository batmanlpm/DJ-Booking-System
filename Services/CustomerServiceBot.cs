using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Customer service chatbot - SFW queries only
    /// </summary>
    public class CustomerServiceBot
    {
        private readonly Dictionary<string, string> _faqs = new()
        {
            // Account & Access
            ["how do i sign up"] = "Visit our registration page and fill in your details. You'll receive a confirmation email within minutes.",
            ["forgot password"] = "Click 'Forgot Password' on the login page. We'll send a reset link to your email.",
            ["change email"] = "Go to Account Settings > Email to update your email address. You'll need to verify the new email.",
            
            // Billing & Payments
            ["payment methods"] = "We accept Credit/Debit cards, PayPal, and cryptocurrency. All payments are processed securely.",
            ["cancel subscription"] = "Go to Account Settings > Subscription > Cancel. Your access continues until the billing period ends.",
            ["refund policy"] = "Refunds are available within 14 days of purchase if you haven't accessed premium content.",
            
            // Technical Support
            ["video not playing"] = "Try clearing your browser cache, updating your browser, or try a different browser. Contact support if issues persist.",
            ["slow streaming"] = "Check your internet connection (minimum 5 Mbps recommended). Try lowering video quality in settings.",
            ["mobile app"] = "Download our app from the App Store or Google Play. Log in with your existing account.",
            
            // Content & Features
            ["upload content"] = "Pro users can upload via the Upload button in your dashboard. Max file size: 5GB.",
            ["download videos"] = "Premium members can download videos for offline viewing. Look for the download icon.",
            ["content schedule"] = "New content is posted Monday, Wednesday, and Friday at 8 PM EST.",
            
            // Privacy & Security
            ["privacy policy"] = "We never share your personal data. View our full privacy policy at www.yoursite.com/privacy",
            ["delete account"] = "Contact support@yoursite.com to permanently delete your account and all associated data.",
            ["secure payment"] = "All payments use 256-bit SSL encryption. We never store your full credit card details."
        };

        public class ChatResponse
        {
            public string Query { get; set; } = string.Empty;
            public string Response { get; set; } = string.Empty;
            public bool WasHandled { get; set; }
            public List<string> SuggestedQuestions { get; set; } = new();
        }

        /// <summary>
        /// Get response to customer query
        /// </summary>
        public Task<ChatResponse> GetResponseAsync(string query)
        {
            query = query.ToLower().Trim();

            // Filter out inappropriate queries
            if (IsInappropriate(query))
            {
                return Task.FromResult(new ChatResponse
                {
                    Query = query,
                    Response = "I can only help with account, billing, and technical support questions. Please contact our support team for other inquiries.",
                    WasHandled = true
                });
            }

            // Check FAQs
            foreach (var faq in _faqs)
            {
                if (query.Contains(faq.Key))
                {
                    return Task.FromResult(new ChatResponse
                    {
                        Query = query,
                        Response = faq.Value,
                        WasHandled = true,
                        SuggestedQuestions = GetRelatedQuestions(faq.Key)
                    });
                }
            }

            // No match found
            return Task.FromResult(new ChatResponse
            {
                Query = query,
                Response = "I can help with:\n• Account & Login\n• Billing & Subscriptions\n• Technical Issues\n• Content Access\n\nFor other questions, email support@yoursite.com",
                WasHandled = false,
                SuggestedQuestions = new List<string>
                {
                    "How do I sign up?",
                    "What payment methods do you accept?",
                    "Video not playing - what should I do?"
                }
            });
        }

        /// <summary>
        /// Check if query is inappropriate for SFW bot
        /// </summary>
        private bool IsInappropriate(string query)
        {
            var inappropriate = new[] 
            { 
                "nude", "naked", "sex", "porn", "xxx", "adult content",
                "explicit", "nsfw", "performers", "models"
            };

            return Array.Exists(inappropriate, word => query.Contains(word));
        }

        /// <summary>
        /// Get related questions
        /// </summary>
        private List<string> GetRelatedQuestions(string category)
        {
            if (category.Contains("sign up") || category.Contains("password"))
            {
                return new List<string>
                {
                    "How do I change my email?",
                    "How do I cancel my subscription?"
                };
            }

            if (category.Contains("payment") || category.Contains("subscription"))
            {
                return new List<string>
                {
                    "What is your refund policy?",
                    "How do I cancel my subscription?"
                };
            }

            if (category.Contains("video") || category.Contains("streaming"))
            {
                return new List<string>
                {
                    "Why is my video streaming slowly?",
                    "Is there a mobile app?"
                };
            }

            return new List<string>();
        }

        /// <summary>
        /// Get common questions by category
        /// </summary>
        public Dictionary<string, List<string>> GetCommonQuestions()
        {
            return new Dictionary<string, List<string>>
            {
                ["Account"] = new()
                {
                    "How do I sign up?",
                    "I forgot my password",
                    "How do I change my password"
                },
                ["Billing"] = new()
                {
                    "What payment methods do you accept?",
                    "How do I cancel my subscription?",
                    "What is your refund policy?"
                },
                ["Technical"] = new()
                {
                    "Video not playing",
                    "Streaming is slow",
                    "Is there a mobile app?"
                },
                ["Content"] = new()
                {
                    "How do I upload content?",
                    "Can I download videos?",
                    "When is new content posted?"
                }
            };
        }
    }
}
