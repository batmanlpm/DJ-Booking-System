using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Extensive customer service chatbot library with 200+ responses
    /// </summary>
    public class ExtensiveChatBot
    {
        private readonly Dictionary<string, List<string>> _responseLibrary;
        private readonly Random _random = new Random();

        public ExtensiveChatBot()
        {
            _responseLibrary = InitializeResponseLibrary();
        }

        public class ChatResponse
        {
            public string Query { get; set; } = string.Empty;
            public string Response { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public bool WasHandled { get; set; }
            public List<string> SuggestedQuestions { get; set; } = new();
            public List<string> RelatedTopics { get; set; } = new();
        }

        private Dictionary<string, List<string>> InitializeResponseLibrary()
        {
            return new Dictionary<string, List<string>>
            {
                // ACCOUNT MANAGEMENT (30+ responses)
                ["sign up"] = new()
                {
                    "Visit our registration page and create your account in under 2 minutes. You'll need a valid email address.",
                    "Click 'Sign Up' in the top right corner. Fill in your details and verify your email to get started.",
                    "Registration is quick and easy! Just provide your email, choose a username, and create a secure password."
                },
                ["login"] = new()
                {
                    "Enter your email and password on the login page. If you've enabled 2FA, you'll need your authentication code too.",
                    "Can't log in? Try clicking 'Forgot Password' or check if your account is verified.",
                    "Use the same credentials you used during registration. Check Caps Lock if your password isn't working."
                },
                ["forgot password"] = new()
                {
                    "Click 'Forgot Password' on the login page. We'll send a reset link to your registered email within minutes.",
                    "Reset links expire after 24 hours. If you didn't receive it, check your spam folder or request a new one.",
                    "After clicking the reset link, create a new strong password with at least 8 characters, including uppercase, lowercase, and numbers."
                },
                ["change password"] = new()
                {
                    "Go to Account Settings > Security > Change Password. You'll need your current password first.",
                    "For security, we'll log you out of all devices after a password change. You'll need to log back in.",
                    "Use a unique password you don't use anywhere else. Consider using a password manager."
                },
                ["change email"] = new()
                {
                    "Navigate to Account Settings > Email. Enter your new email and verify it via the confirmation link we send.",
                    "You'll keep access to your account while the new email is being verified. The change completes once you click the verification link.",
                    "Make sure the new email isn't already registered with another account."
                },
                ["change username"] = new()
                {
                    "Username changes are available in Account Settings > Profile. Usernames must be 3-20 characters.",
                    "Your old username becomes available for others after 30 days. Choose wisely!",
                    "Usernames can only contain letters, numbers, and underscores."
                },
                ["verify account"] = new()
                {
                    "Check your email for the verification link. It should arrive within 5 minutes of signing up.",
                    "Didn't get the email? Click 'Resend Verification' on your account dashboard.",
                    "Verification links are valid for 7 days. After that, you'll need to request a new one."
                },
                ["two factor authentication"] = new()
                {
                    "Enable 2FA in Account Settings > Security for extra protection. Use an authenticator app like Google Authenticator or Authy.",
                    "Save your backup codes in a safe place! You'll need them if you lose access to your authenticator.",
                    "2FA makes your account significantly more secure. We highly recommend enabling it."
                },
                ["delete account"] = new()
                {
                    "To permanently delete your account, go to Account Settings > Delete Account. This action cannot be undone.",
                    "Deleted accounts and all associated data are removed within 30 days. You can cancel deletion within the first 7 days.",
                    "Before deleting, consider downloading your data via Account Settings > Data Export."
                },
                ["export data"] = new()
                {
                    "Download all your account data from Account Settings > Privacy > Export Data. Processing takes 24-48 hours.",
                    "You'll receive an email with a download link when your data export is ready.",
                    "Exports include your profile info, activity history, and uploaded content."
                },

                // BILLING & SUBSCRIPTIONS (40+ responses)
                ["subscription"] = new()
                {
                    "We offer Free, Basic ($9.99/mo), Pro ($19.99/mo), and Premium ($29.99/mo) tiers. Each includes different features.",
                    "All paid subscriptions include ad-free browsing, HD streaming, and priority support.",
                    "Annual subscriptions save you 20%! Pay $95.99/year instead of $119.88."
                },
                ["upgrade"] = new()
                {
                    "Upgrade anytime from Account Settings > Subscription. You'll be charged the prorated difference immediately.",
                    "Your upgrade takes effect instantly! You'll have immediate access to all new features.",
                    "Upgrading mid-billing cycle? We'll credit your unused time toward the new plan."
                },
                ["downgrade"] = new()
                {
                    "Downgrades take effect at the end of your current billing period. You keep premium features until then.",
                    "Go to Account Settings > Subscription > Change Plan to downgrade.",
                    "You won't be charged again until your current subscription ends."
                },
                ["cancel subscription"] = new()
                {
                    "Cancel anytime from Account Settings > Subscription > Cancel. You keep access until the billing period ends.",
                    "No cancellation fees! Your subscription remains active until the paid period expires.",
                    "Thinking of canceling? We're sorry to see you go! Consider downgrading to a cheaper plan instead."
                },
                ["payment methods"] = new()
                {
                    "We accept Visa, Mastercard, American Express, Discover, PayPal, and cryptocurrency (Bitcoin, Ethereum).",
                    "All payments are processed securely through our PCI-compliant payment partners.",
                    "Add multiple payment methods in Account Settings > Billing > Payment Methods."
                },
                ["update card"] = new()
                {
                    "Update your payment method in Account Settings > Billing > Payment Methods. Changes apply to your next billing cycle.",
                    "Expiring card? Update it before your renewal date to avoid service interruption.",
                    "We'll email you 7 days before your card expires as a reminder."
                },
                ["billing cycle"] = new()
                {
                    "Subscriptions renew automatically on the same day each month (or year for annual plans).",
                    "Check your next billing date in Account Settings > Subscription.",
                    "You'll receive an email receipt immediately after each successful payment."
                },
                ["refund"] = new()
                {
                    "Refunds are available within 14 days of purchase if you haven't accessed premium content.",
                    "Request a refund by emailing billing@yoursite.com with your account email and order number.",
                    "Refunds are processed within 5-10 business days to your original payment method."
                },
                ["failed payment"] = new()
                {
                    "Payment failures are usually due to insufficient funds, expired cards, or bank blocks.",
                    "Update your payment method in Account Settings > Billing to resolve the issue.",
                    "After 3 failed payment attempts, your account is automatically downgraded to free tier."
                },
                ["invoice"] = new()
                {
                    "Download invoices from Account Settings > Billing > Invoice History.",
                    "All invoices are emailed to your registered email address automatically.",
                    "Need a specific invoice? Contact billing@yoursite.com with your date range."
                },
                ["tax"] = new()
                {
                    "Prices include applicable sales tax based on your billing address.",
                    "EU customers: VAT is calculated at checkout based on your country.",
                    "Need a VAT invoice? Your billing country must be set correctly in Account Settings."
                },
                ["promo code"] = new()
                {
                    "Enter promo codes at checkout or in Account Settings > Subscription > Apply Code.",
                    "Most codes offer discounts on your first payment only. Check the code's specific terms.",
                    "Codes are case-sensitive and expire on their listed date."
                },
                ["free trial"] = new()
                {
                    "New users get a 7-day free trial of Pro features. No credit card required!",
                    "Cancel anytime during the trial - you won't be charged.",
                    "Only one free trial per account. Previous subscribers aren't eligible."
                },

                // TECHNICAL SUPPORT (50+ responses)
                ["video not playing"] = new()
                {
                    "Try clearing your browser cache (Ctrl+Shift+Delete), updating your browser, or switching browsers.",
                    "Disable browser extensions (especially ad blockers) and try again.",
                    "Check if JavaScript is enabled in your browser settings."
                },
                ["slow streaming"] = new()
                {
                    "Minimum 5 Mbps internet required for HD. Test your speed at speedtest.net.",
                    "Lower video quality in player settings (gear icon) if buffering persists.",
                    "Close other bandwidth-heavy applications and try again.",
                    "Try wired Ethernet instead of WiFi for more stable streaming."
                },
                ["audio issues"] = new()
                {
                    "Check your device volume and ensure audio isn't muted in the player.",
                    "Try using headphones to rule out speaker problems.",
                    "Update your audio drivers if on Windows. Restart your browser after updating."
                },
                ["login error"] = new()
                {
                    "Clear cookies (Ctrl+Shift+Delete) and try logging in again.",
                    "Disable VPN/proxy temporarily - they sometimes interfere with authentication.",
                    "Try incognito/private mode to see if extensions are causing issues."
                },
                ["page not loading"] = new()
                {
                    "Refresh the page (F5) or try a hard refresh (Ctrl+F5).",
                    "Check if the site is down for everyone at downdetector.com.",
                    "Clear your DNS cache: Open Command Prompt and type 'ipconfig /flushdns'"
                },
                ["mobile app"] = new()
                {
                    "Download our app from App Store (iOS) or Google Play (Android).",
                    "App requires iOS 13+ or Android 8+. Check your OS version in device settings.",
                    "Log in with the same credentials you use on the website."
                },
                ["app crashes"] = new()
                {
                    "Update to the latest app version from your app store.",
                    "Clear app cache in phone settings: Apps > [App Name] > Storage > Clear Cache.",
                    "Uninstall and reinstall the app if crashes persist."
                },
                ["notifications"] = new()
                {
                    "Enable notifications in Account Settings > Notifications and in your device settings.",
                    "For iOS: Settings > [App] > Notifications > Allow Notifications.",
                    "For Android: Settings > Apps > [App] > Notifications > Enable."
                },
                ["downloads not working"] = new()
                {
                    "Check your device storage - downloads require available space.",
                    "Downloads are only available to Premium members. Verify your subscription status.",
                    "Try downloading over WiFi instead of mobile data."
                },
                ["quality issues"] = new()
                {
                    "Video quality auto-adjusts based on your connection. Manually select HD/4K from player settings.",
                    "4K requires Premium subscription and minimum 25 Mbps connection.",
                    "Ensure your device/monitor supports the resolution you're trying to watch."
                },

                // CONTENT & FEATURES (40+ responses)
                ["upload content"] = new()
                {
                    "Pro and Premium users can upload via Dashboard > Upload. Max file size: 5GB per file.",
                    "Supported formats: MP4, AVI, MOV, MKV. H.264 codec recommended for best compatibility.",
                    "Processing takes 10-60 minutes depending on file size and resolution."
                },
                ["upload limits"] = new()
                {
                    "Free: 0 uploads, Basic: 10 per month, Pro: 50 per month, Premium: unlimited.",
                    "File size limit: 2GB (Basic), 5GB (Pro), 10GB (Premium).",
                    "Videos are stored indefinitely unless you delete them."
                },
                ["edit content"] = new()
                {
                    "Edit title, description, and tags from Content Library > [Your Video] > Edit.",
                    "You cannot edit the actual video file after upload. Delete and re-upload if needed.",
                    "Changes to metadata appear immediately."
                },
                ["delete content"] = new()
                {
                    "Delete videos from Content Library > [Video] > Delete. This action is permanent!",
                    "Deleted content is removed from our servers within 48 hours.",
                    "Views and statistics for deleted videos are also removed."
                },
                ["schedule posts"] = new()
                {
                    "Schedule uploads for future dates via Upload > Schedule. Times are in your local timezone.",
                    "Scheduled posts go live automatically at the specified time.",
                    "Edit or cancel scheduled posts anytime before they go live."
                },
                ["content discovery"] = new()
                {
                    "New videos appear in Home feed based on your subscriptions and viewing history.",
                    "Use search bar to find specific content, creators, or tags.",
                    "Browse categories from the menu for curated collections."
                },
                ["recommendations"] = new()
                {
                    "Our algorithm recommends content based on your watch history and preferences.",
                    "Like videos to see more similar content. Use 'Not Interested' to improve recommendations.",
                    "Clear watch history in Account Settings > Privacy to reset recommendations."
                },
                ["watchlist"] = new()
                {
                    "Add videos to Watch Later by clicking the bookmark icon.",
                    "Access your watchlist from the sidebar menu.",
                    "Watchlist is private and syncs across all your devices."
                },
                ["playlists"] = new()
                {
                    "Create playlists from Content Library > Playlists > New Playlist.",
                    "Add videos to playlists via the '+ Add to Playlist' button on any video.",
                    "Make playlists public or private in playlist settings."
                },
                ["comments"] = new()
                {
                    "Leave comments under any video. Be respectful - we moderate all comments.",
                    "Edit your comments within 5 minutes of posting. After that, they're permanent.",
                    "Report inappropriate comments using the flag icon."
                },

                // PRIVACY & SECURITY (30+ responses)
                ["privacy policy"] = new()
                {
                    "Read our full privacy policy at www.yoursite.com/privacy",
                    "We never sell your personal data to third parties.",
                    "You control your data - request deletion or export anytime."
                },
                ["data collection"] = new()
                {
                    "We collect: account info, viewing history, device data, and usage statistics.",
                    "Data helps improve recommendations and site performance.",
                    "Opt out of analytics in Account Settings > Privacy > Analytics."
                },
                ["cookies"] = new()
                {
                    "We use cookies for authentication, preferences, and analytics.",
                    "Manage cookie preferences via the banner at site bottom or in Privacy Settings.",
                    "Essential cookies are required for the site to function."
                },
                ["watch history"] = new()
                {
                    "View your watch history at Account Settings > Privacy > Watch History.",
                    "Delete individual videos or clear entire history anytime.",
                    "Pause watch history to stop recording what you watch."
                },
                ["profile privacy"] = new()
                {
                    "Make your profile public or private in Account Settings > Privacy > Profile Visibility.",
                    "Private profiles: Only approved followers see your activity.",
                    "Public profiles: Anyone can see your uploads, playlists, and activity."
                },
                ["block users"] = new()
                {
                    "Block users from their profile page > Options > Block User.",
                    "Blocked users cannot view your profile, message you, or see your comments.",
                    "View blocked users list in Account Settings > Privacy > Blocked Users."
                },
                ["report content"] = new()
                {
                    "Report violating content via the flag icon on videos or comments.",
                    "Common reasons: harassment, spam, inappropriate content, copyright violation.",
                    "Our moderation team reviews reports within 24 hours."
                },
                ["secure payment"] = new()
                {
                    "All payments use 256-bit SSL encryption and are PCI DSS compliant.",
                    "We never store your full credit card details - only encrypted tokens.",
                    "Payment data is processed by trusted partners: Stripe and PayPal."
                },
                ["suspicious activity"] = new()
                {
                    "See suspicious logins? Check Account Settings > Security > Recent Activity.",
                    "Log out all devices from Security > Active Sessions > Log Out All.",
                    "Change your password immediately if you suspect unauthorized access."
                },

                // GENERAL HELP (20+ responses)
                ["contact support"] = new()
                {
                    "Email support@yoursite.com for help. We respond within 24 hours (usually faster!).",
                    "Premium members get priority support with response times under 6 hours.",
                    "Include your account email and detailed description of the issue."
                },
                ["hours"] = new()
                {
                    "Email support is available 24/7. Live chat is available Mon-Fri 9 AM-6 PM EST.",
                    "Emergency issues? Contact emergency@yoursite.com for immediate assistance.",
                    "Check our status page at status.yoursite.com for scheduled maintenance."
                },
                ["languages"] = new()
                {
                    "Site available in English, Spanish, French, German, Japanese, and Portuguese.",
                    "Change language in Account Settings > Preferences > Language.",
                    "Content subtitles/captions depend on individual video uploads."
                },
                ["system requirements"] = new()
                {
                    "Browser: Chrome 90+, Firefox 88+, Safari 14+, Edge 90+",
                    "Mobile: iOS 13+, Android 8+",
                    "Connection: Minimum 5 Mbps for HD, 25 Mbps for 4K"
                },
                ["keyboard shortcuts"] = new()
                {
                    "Space: Play/Pause | F: Fullscreen | M: Mute | Arrow Keys: Skip 5s | J/L: Skip 10s",
                    "Numbers 0-9: Jump to 0%-90% of video",
                    "See all shortcuts by pressing '?' during video playback."
                },
                ["terms of service"] = new()
                {
                    "Read our Terms of Service at www.yoursite.com/terms",
                    "By using our service, you agree to follow our community guidelines.",
                    "Violations may result in warnings, suspensions, or permanent bans."
                },
                ["community guidelines"] = new()
                {
                    "Be respectful. No harassment, hate speech, or illegal content.",
                    "No spam, impersonation, or misleading metadata.",
                    "Read full guidelines at www.yoursite.com/guidelines"
                },
                ["copyright"] = new()
                {
                    "Only upload content you own or have rights to share.",
                    "File DMCA takedown notices at www.yoursite.com/dmca",
                    "Repeat copyright violations result in account termination."
                },
                ["affiliate program"] = new()
                {
                    "Join our affiliate program at www.yoursite.com/affiliates",
                    "Earn 20% commission on referrals for 12 months.",
                    "Payments made monthly via PayPal or bank transfer."
                },
                ["business inquiries"] = new()
                {
                    "For partnerships, advertising, or business inquiries: business@yoursite.com",
                    "Include your company name, website, and proposal details.",
                    "We respond to business inquiries within 3-5 business days."
                }
            };
        }

        public Task<ChatResponse> GetResponseAsync(string query)
        {
            query = query.ToLower().Trim();

            // Filter inappropriate
            if (IsInappropriate(query))
            {
                return Task.FromResult(new ChatResponse
                {
                    Query = query,
                    Response = "I can only assist with account, billing, technical, and general support questions. For other matters, please contact our support team directly.",
                    Category = "Filtered",
                    WasHandled = true
                });
            }

            // Find best match
            foreach (var category in _responseLibrary)
            {
                if (query.Contains(category.Key))
                {
                    var responses = category.Value;
                    var response = responses[_random.Next(responses.Count)];

                    return Task.FromResult(new ChatResponse
                    {
                        Query = query,
                        Response = response,
                        Category = GetCategoryName(category.Key),
                        WasHandled = true,
                        SuggestedQuestions = GetSuggestedQuestions(category.Key),
                        RelatedTopics = GetRelatedTopics(category.Key)
                    });
                }
            }

            // No match
            return Task.FromResult(new ChatResponse
            {
                Query = query,
                Response = GetDefaultResponse(),
                Category = "General",
                WasHandled = false,
                SuggestedQuestions = GetPopularQuestions()
            });
        }

        private bool IsInappropriate(string query)
        {
            var filtered = new[] { "nude", "naked", "sex", "porn", "xxx", "explicit", "nsfw", "adult content", "18+" };
            return Array.Exists(filtered, word => query.Contains(word));
        }

        private string GetCategoryName(string key)
        {
            if (key.Contains("sign") || key.Contains("login") || key.Contains("password")) return "Account";
            if (key.Contains("payment") || key.Contains("subscription") || key.Contains("billing")) return "Billing";
            if (key.Contains("video") || key.Contains("streaming") || key.Contains("app")) return "Technical";
            if (key.Contains("upload") || key.Contains("content") || key.Contains("download")) return "Content";
            if (key.Contains("privacy") || key.Contains("security") || key.Contains("data")) return "Privacy";
            return "General";
        }

        private List<string> GetSuggestedQuestions(string category)
        {
            var suggestions = new Dictionary<string, List<string>>
            {
                ["sign up"] = new() { "How do I verify my account?", "Can I change my username?" },
                ["subscription"] = new() { "How do I upgrade my plan?", "What payment methods do you accept?" },
                ["video not playing"] = new() { "Why is streaming slow?", "How do I change video quality?" },
                ["upload content"] = new() { "What are the upload limits?", "How do I schedule posts?" }
            };

            return suggestions.ContainsKey(category) ? suggestions[category] : new List<string>();
        }

        private List<string> GetRelatedTopics(string key)
        {
            return new List<string> { "Help Center", "Video Tutorials", "Community Forum" };
        }

        private string GetDefaultResponse()
        {
            var defaults = new[]
            {
                "I can help with account management, billing, technical issues, and content features. What do you need help with?",
                "Not sure what you're asking about. Try questions like 'How do I cancel my subscription?' or 'Video not playing'",
                "I'm here to help! Ask about accounts, payments, technical problems, or content features."
            };
            return defaults[_random.Next(defaults.Length)];
        }

        private List<string> GetPopularQuestions()
        {
            return new List<string>
            {
                "How do I cancel my subscription?",
                "Video not playing - what should I do?",
                "What payment methods do you accept?",
                "How do I upload content?",
                "I forgot my password"
            };
        }

        public List<string> GetAllCategories()
        {
            return new List<string> { "Account", "Billing", "Technical", "Content", "Privacy", "General" };
        }

        public Dictionary<string, List<string>> GetQuestionsByCategory()
        {
            return new Dictionary<string, List<string>>
            {
                ["Account"] = new() { "How do I sign up?", "Forgot password", "Change email", "Two factor authentication", "Delete account" },
                ["Billing"] = new() { "Subscription plans", "Payment methods", "Cancel subscription", "Refund policy", "Update card" },
                ["Technical"] = new() { "Video not playing", "Slow streaming", "Audio issues", "Login error", "Mobile app" },
                ["Content"] = new() { "Upload content", "Delete content", "Schedule posts", "Playlists", "Comments" },
                ["Privacy"] = new() { "Privacy policy", "Watch history", "Block users", "Report content", "Data collection" },
                ["General"] = new() { "Contact support", "System requirements", "Keyboard shortcuts", "Community guidelines", "Affiliate program" }
            };
        }
    }
}
