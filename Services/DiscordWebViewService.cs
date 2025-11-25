using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Service for managing user-specific Discord WebView2 sessions
    /// Each user gets their own isolated data folder to maintain separate Discord logins
    /// </summary>
    public class DiscordWebViewService
    {
        private readonly string _username;
        private readonly string _userDataFolder;
        private const string DISCORD_WEB_URL = "https://discord.com/channels/@me";

        public DiscordWebViewService(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));

            _username = username;
            
            // Create user-specific data folder in AppData
            string appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DJBookingSystem",
                "Discord",
                SanitizeUsername(username)
            );

            _userDataFolder = appDataFolder;

            System.Diagnostics.Debug.WriteLine($"[DiscordWebView] User data folder for {username}: {_userDataFolder}");
        }

        /// <summary>
        /// Sanitize username to be filesystem-safe
        /// </summary>
        private string SanitizeUsername(string username)
        {
            // Remove invalid path characters
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                username = username.Replace(c, '_');
            }
            return username;
        }

        /// <summary>
        /// Initialize WebView2 with user-specific data folder
        /// </summary>
        public async Task InitializeWebView2Async(WebView2 webView)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Initializing WebView2 for user: {_username}");
                
                // Check if already initialized
                if (webView.CoreWebView2 != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DiscordWebView] ?? WebView2 already has CoreWebView2, skipping initialization");
                    return;
                }

                // Ensure data folder exists
                Directory.CreateDirectory(_userDataFolder);

                System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Data folder: {_userDataFolder}");

                // Try to copy Discord desktop session data if it exists (for auto-login)
                await TryCopyDiscordDesktopSession();

                // Create environment with user-specific data folder
                var env = await CoreWebView2Environment.CreateAsync(
                    browserExecutableFolder: null,
                    userDataFolder: _userDataFolder,
                    options: new CoreWebView2EnvironmentOptions
                    {
                        AdditionalBrowserArguments = "--disable-web-security --disable-features=msWebOOUI,msPdfOOUI"
                    }
                );

                // Initialize WebView2 with the environment
                await webView.EnsureCoreWebView2Async(env);

                System.Diagnostics.Debug.WriteLine($"[DiscordWebView] ? WebView2 initialized successfully for {_username}");

                // Configure settings
                ConfigureWebView2Settings(webView);

                // Navigate to Discord
                if (webView.CoreWebView2 != null)
                {
                    webView.CoreWebView2.Navigate(DISCORD_WEB_URL);
                    System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Navigating to Discord web app...");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DiscordWebView] ? Error initializing WebView2: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Try to copy Discord desktop session data for auto-login
        /// </summary>
        private async Task TryCopyDiscordDesktopSession()
        {
            try
            {
                // Check for Discord desktop app data
                string discordAppData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Discord"
                );

                if (Directory.Exists(discordAppData))
                {
                    System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Found Discord desktop data at: {discordAppData}");
                    
                    // Copy Local Storage (contains auth tokens)
                    string discordLocalStorage = Path.Combine(discordAppData, "Local Storage", "leveldb");
                    if (Directory.Exists(discordLocalStorage))
                    {
                        string targetLocalStorage = Path.Combine(_userDataFolder, "Local Storage", "leveldb");
                        
                        // Always try to copy/update for auto-login
                        System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Attempting to copy Discord session data...");
                        
                        await Task.Run(() =>
                        {
                            try
                            {
                                CopyDirectory(discordLocalStorage, targetLocalStorage, overwrite: true);
                                System.Diagnostics.Debug.WriteLine($"[DiscordWebView] ? Discord session data copied for auto-login");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[DiscordWebView] ?? Could not copy Discord session: {ex.Message}");
                            }
                        });
                    }
                    
                    // Also copy Session Storage
                    string discordSessionStorage = Path.Combine(discordAppData, "Session Storage");
                    if (Directory.Exists(discordSessionStorage))
                    {
                        string targetSessionStorage = Path.Combine(_userDataFolder, "Session Storage");
                        
                        await Task.Run(() =>
                        {
                            try
                            {
                                CopyDirectory(discordSessionStorage, targetSessionStorage, overwrite: true);
                                System.Diagnostics.Debug.WriteLine($"[DiscordWebView] ? Discord Session Storage copied");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[DiscordWebView] ?? Could not copy Session Storage: {ex.Message}");
                            }
                        });
                    }
                    
                    // Copy Cookies
                    string discordCookies = Path.Combine(discordAppData, "Cookies");
                    if (File.Exists(discordCookies))
                    {
                        string targetCookies = Path.Combine(_userDataFolder, "Cookies");
                        
                        await Task.Run(() =>
                        {
                            try
                            {
                                File.Copy(discordCookies, targetCookies, overwrite: true);
                                System.Diagnostics.Debug.WriteLine($"[DiscordWebView] ? Discord Cookies copied");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[DiscordWebView] ?? Could not copy Cookies: {ex.Message}");
                            }
                        });
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Discord desktop app not found. User will need to log in manually.");
                }
            }
            catch (Exception ex)
            {
                // Not critical - user will just need to log in manually
                System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Info: Could not access Discord desktop data: {ex.Message}");
            }
        }

        /// <summary>
        /// Copy directory recursively
        /// </summary>
        private void CopyDirectory(string sourceDir, string targetDir, bool overwrite = false)
        {
            Directory.CreateDirectory(targetDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string targetFile = Path.Combine(targetDir, Path.GetFileName(file));
                try
                {
                    File.Copy(file, targetFile, overwrite: overwrite);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Warning: Could not copy {Path.GetFileName(file)}: {ex.Message}");
                }
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string targetSubDir = Path.Combine(targetDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, targetSubDir, overwrite);
            }
        }

        /// <summary>
        /// Configure WebView2 settings for optimal Discord experience
        /// </summary>
        private void ConfigureWebView2Settings(WebView2 webView)
        {
            var settings = webView.CoreWebView2.Settings;

            // Enable JavaScript (required for Discord)
            settings.IsScriptEnabled = true;

            // Enable WebMessage (for communication between host and web content)
            settings.IsWebMessageEnabled = true;

            // Enable DevTools for debugging (can be disabled in production)
            settings.AreDevToolsEnabled = true;

            // Disable context menu (optional - for cleaner UX)
            settings.AreDefaultContextMenusEnabled = true;

            // Enable zoom control
            settings.IsZoomControlEnabled = true;

            // Enable status bar
            settings.IsStatusBarEnabled = false;

            // Allow downloads
            settings.IsGeneralAutofillEnabled = true;

            System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Settings configured for {_username}");
        }

        /// <summary>
        /// Get the user-specific data folder path
        /// </summary>
        public string GetUserDataFolder() => _userDataFolder;

        /// <summary>
        /// Clear all user data (logout)
        /// </summary>
        public async Task ClearUserDataAsync(WebView2 webView)
        {
            try
            {
                if (webView?.CoreWebView2 != null)
                {
                    // Clear cookies
                    var cookieManager = webView.CoreWebView2.CookieManager;
                    var cookies = await cookieManager.GetCookiesAsync(DISCORD_WEB_URL);
                    foreach (var cookie in cookies)
                    {
                        cookieManager.DeleteCookie(cookie);
                    }

                    System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Cleared cookies for {_username}");
                }

                // Delete data folder
                if (Directory.Exists(_userDataFolder))
                {
                    try
                    {
                        Directory.Delete(_userDataFolder, recursive: true);
                        System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Deleted data folder for {_username}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Warning: Could not delete data folder: {ex.Message}");
                        // Folder might be in use, will be cleaned up next time
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Error clearing user data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if user has existing Discord session data
        /// </summary>
        public bool HasExistingSession()
        {
            try
            {
                if (!Directory.Exists(_userDataFolder))
                    return false;

                // Check if there are any files in the data folder (indicating a session)
                var files = Directory.GetFiles(_userDataFolder, "*", SearchOption.AllDirectories);
                bool hasSession = files.Length > 0;

                System.Diagnostics.Debug.WriteLine($"[DiscordWebView] {_username} has existing session: {hasSession}");

                return hasSession;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Error checking session: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Navigate to a specific Discord server/channel
        /// </summary>
        public void NavigateToChannel(WebView2 webView, string serverId, string channelId)
        {
            if (webView?.CoreWebView2 != null)
            {
                string url = $"https://discord.com/channels/{serverId}/{channelId}";
                webView.CoreWebView2.Navigate(url);
                System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Navigating to channel: {url}");
            }
        }

        /// <summary>
        /// Navigate to Discord home (DMs)
        /// </summary>
        public void NavigateToHome(WebView2 webView)
        {
            if (webView?.CoreWebView2 != null)
            {
                webView.CoreWebView2.Navigate(DISCORD_WEB_URL);
                System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Navigating to Discord home");
            }
        }

        /// <summary>
        /// Inject custom CSS to customize Discord appearance (optional)
        /// </summary>
        public async Task InjectCustomStylesAsync(WebView2 webView, string css)
        {
            if (webView?.CoreWebView2 != null)
            {
                try
                {
                    string script = $@"
                        (function() {{
                            var style = document.createElement('style');
                            style.textContent = `{css}`;
                            document.head.appendChild(style);
                        }})();
                    ";

                    await webView.CoreWebView2.ExecuteScriptAsync(script);
                    System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Custom styles injected");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DiscordWebView] Error injecting styles: {ex.Message}");
                }
            }
        }
    }
}
