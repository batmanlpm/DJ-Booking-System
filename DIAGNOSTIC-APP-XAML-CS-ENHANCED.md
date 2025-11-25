# ?? COMPREHENSIVE APP STARTUP DIAGNOSTIC

## The app is silently shutting down - let's find out exactly where and why.

### Step 1: Add Enhanced Logging to App.xaml.cs

Replace the entire `App.xaml.cs` file with this diagnostic version:

```csharp
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using DJBookingSystem.Services;
using DJBookingSystem.Models;

namespace DJBookingSystem
{
    public partial class App : Application
    {
        private static string _currentUsername = "System";
        private static StreamWriter? _logWriter;

        public App()
        {
            // Initialize logging FIRST
            InitializeLogging();
            
            Log("=== APP CONSTRUCTOR CALLED ===");
            this.Exit += App_Exit;
            Log("? Exit event handler attached");
        }

        private void InitializeLogging()
        {
            try
            {
                string logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"DJBookingSystem_StartupLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                );
                
                _logWriter = new StreamWriter(logPath, append: true);
                _logWriter.AutoFlush = true;
                
                _logWriter.WriteLine($"=== DJ BOOKING SYSTEM STARTUP LOG ===");
                _logWriter.WriteLine($"Time: {DateTime.Now}");
                _logWriter.WriteLine($"=================================");
                _logWriter.WriteLine();
            }
            catch
            {
                // If file logging fails, continue anyway
            }
        }

        private static void Log(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logMessage = $"[{timestamp}] {message}";
            
            // Write to Debug output
            System.Diagnostics.Debug.WriteLine(logMessage);
            
            // Write to file
            try
            {
                _logWriter?.WriteLine(logMessage);
            }
            catch
            {
                // Ignore logging errors
            }
        }

        private void App_Exit(object? sender, ExitEventArgs e)
        {
            Log("=== APP EXIT EVENT FIRED ===");
            Log($"Exit code: {e.ApplicationExitCode}");
            
            _logWriter?.Close();
            
            // Ensure all windows are closed and process is terminated
            Application.Current.Shutdown();
        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            Log("=== APPLICATION_STARTUP CALLED ===");
            
            try
            {
                // Set up unhandled exception handlers FIRST
                AppDomain.CurrentDomain.UnhandledException += (s, args) =>
                {
                    Exception ex = (Exception)args.ExceptionObject;
                    Log($"FATAL UNHANDLED EXCEPTION: {ex.Message}");
                    Log($"Stack trace: {ex.StackTrace}");
                    
                    MessageBox.Show($"FATAL ERROR:\n\n{ex.Message}\n\nStack:\n{ex.StackTrace}\n\nCheck desktop for log file.", 
                        "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                };
                
                this.DispatcherUnhandledException += (s, args) =>
                {
                    Log($"DISPATCHER UNHANDLED EXCEPTION: {args.Exception.Message}");
                    Log($"Stack trace: {args.Exception.StackTrace}");
                    
                    MessageBox.Show($"UI THREAD ERROR:\n\n{args.Exception.Message}\n\nStack:\n{args.Exception.StackTrace}\n\nCheck desktop for log file.", 
                        "UI Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    
                    args.Handled = true; // Prevent app crash
                };
                
                TaskScheduler.UnobservedTaskException += (s, args) =>
                {
                    Log($"UNOBSERVED TASK EXCEPTION: {args.Exception.Message}");
                    Log($"Stack trace: {args.Exception.StackTrace}");
                    args.SetObserved(); // Prevent crash
                };
                
                Log("? Exception handlers installed");
                
                await InitializeApplicationAsync();
                
                Log("? InitializeApplicationAsync completed");
                
                // Check for updates on startup (don't await to not block startup)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        Log("Starting background update check task...");
                        await Task.Delay(3000);
                        Log("Calling CheckForUpdatesOnStartupAsync...");
                        await UpdateManager.CheckForUpdatesOnStartupAsync(showNotifications: true);
                        Log("? Update check completed successfully");
                    }
                    catch (Exception ex)
                    {
                        Log($"? Error in background update check: {ex.Message}");
                        Log($"Stack: {ex.StackTrace}");
                        // Don't crash the app - update check is not critical
                    }
                });
                
                // Enable automatic update checks every hour on the hour
                _ = Task.Run(async () =>
                {
                    try
                    {
                        Log("Starting hourly update check task...");
                        await UpdateManager.EnableHourlyUpdateChecksAsync();
                        Log("? Hourly update checks enabled");
                    }
                    catch (Exception ex)
                    {
                        Log($"? Error enabling hourly update checks: {ex.Message}");
                        Log($"Stack: {ex.StackTrace}");
                        // Don't crash the app - hourly checks are not critical
                    }
                });
                
                Log("=== APPLICATION_STARTUP COMPLETED SUCCESSFULLY ===");
            }
            catch (Exception ex)
            {
                Log($"=== FATAL ERROR in Application_Startup ===");
                Log($"Message: {ex.Message}");
                Log($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Log($"Inner Exception: {ex.InnerException.Message}");
                    Log($"Inner Stack: {ex.InnerException.StackTrace}");
                }
                
                MessageBox.Show($"Application failed to start:\n\n{ex.Message}\n\nInner: {ex.InnerException?.Message}\n\nCheck desktop for log file.", 
                    "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private async Task InitializeApplicationAsync()
        {
            try
            {
                Log("=== InitializeApplicationAsync Started ===");
                
                // Azure Cosmos DB credentials
                Log("Creating CosmosDB connection string...");
                string endpoint = "https://fallen-collective.documents.azure.com:443/";
                string accountKey = "EpxIq3hV8kXQ7kNY1KKJQmL5dkX0uZeW4GMUinPf6hNqRApx84Co5Ffve0bAktpyzH2xho5swBV5ACDbeunr5Q==";
                
                string connectionString = $"AccountEndpoint={endpoint};AccountKey={accountKey};";
                Log("? Connection string created");
                
                Log("Creating CosmosDbService...");
                var cosmosService = new CosmosDbService(connectionString);
                Log("? CosmosDbService created");
                
                // Initialize OnlineUserStatusService with CosmosDbService for cloud sync
                Log("Initializing OnlineUserStatusService...");
                OnlineUserStatusService.Instance.Initialize(cosmosService);
                Log("? OnlineUserStatusService initialized");
                
                // Show splash screen with connection checking
                SplashScreen? splashScreen = null;
                Log("Creating splash screen...");
                await Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        Log("Creating SplashScreen instance...");
                        splashScreen = new SplashScreen(cosmosService);
                        Log("Showing SplashScreen...");
                        splashScreen.Show();
                        Log("? Splash screen shown");
                    }
                    catch (Exception ex)
                    {
                        Log($"? Error showing splash screen: {ex.Message}");
                        Log($"Stack: {ex.StackTrace}");
                        throw;
                    }
                });

                Log("Initializing with AZURE COSMOS DB (cloud mode)");

                // Initialize database
                Log("Initializing Azure Cosmos DB...");
                try
                {
                    await cosmosService.InitializeDatabaseAsync();
                    Log("? Azure Cosmos DB initialized!");
                    
                    await cosmosService.InitializeDefaultAdminAsync();
                    Log("? Default admin created!");
                }
                catch (Exception ex)
                {
                    Log($"? ERROR initializing Cosmos DB: {ex.Message}");
                    Log($"Stack: {ex.StackTrace}");
                    
                    string errorDetails = ex.Message;
                    if (ex.InnerException != null)
                        errorDetails += $"\n\nInner: {ex.InnerException.Message}";
                    
                    await Dispatcher.InvokeAsync(() =>
                    {
                        splashScreen?.Close();
                        MessageBox.Show(
                            $"Failed to connect to Azure Cosmos DB:\n\n{errorDetails}\n\n" +
                            $"Check:\n" +
                            $"• Azure Cosmos DB account is active\n" +
                            $"• Firewall rules allow your IP\n" +
                            $"• Account key is correct\n\n" +
                            $"Check desktop for log file.",
                            "Connection Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    });
                    Shutdown();
                    return;
                }

                // Wait for splash screen to finish its animation and connection checks
                Log("Waiting for splash screen to complete...");
                if (splashScreen != null)
                {
                    await splashScreen.CompletionTask;
                }
                Log("? Splash screen completed!");

                // Close splash screen
                await Dispatcher.InvokeAsync(() =>
                {
                    Log("Closing splash screen...");
                    splashScreen?.Close();
                    Log("? Splash screen closed");
                });
                
                // ===== PLAY RANDOM INTRO VIDEO =====
                Log("?? Checking for intro videos...");
                
                try
                {
                    string? introVideoPath = IntroVideoWindow.GetRandomIntroVideo();
                    
                    if (!string.IsNullOrEmpty(introVideoPath))
                    {
                        Log($"?? Playing intro video: {Path.GetFileName(introVideoPath)}");
                        
                        // Show intro video - blocks until complete or skipped
                        await Dispatcher.InvokeAsync(() =>
                        {
                            try
                            {
                                var introWindow = new IntroVideoWindow(introVideoPath, () =>
                                {
                                    Log("? Intro video callback fired");
                                });
                                introWindow.ShowDialog(); // Modal - blocks until video ends or user skips
                            }
                            catch (Exception ex)
                            {
                                Log($"? Error showing intro video: {ex.Message}");
                                // Continue anyway - intro is optional
                            }
                        });
                        
                        Log("? Intro video completed");
                    }
                    else
                    {
                        Log("?? No intro videos found - skipping intro");
                    }
                }
                catch (Exception ex)
                {
                    Log($"? Error in intro video system: {ex.Message}");
                    Log($"   Stack: {ex.StackTrace}");
                    // Continue anyway - intro is optional
                }

                // Initialize AuthenticationService
                Log("Initializing AuthenticationService...");
                var authService = Services.AuthenticationService.Instance;
                authService.Initialize(cosmosService);
                Log("? AuthenticationService initialized");

                // Auto-login check
                Log("Checking for auto-login...");
                User? loggedInUser = null;
                var savedLogin = Services.LocalStorage.GetLoginInfo();

                if (savedLogin != null && savedLogin.AutoLogin && !string.IsNullOrEmpty(savedLogin.Username))
                {
                    Log($"Found saved login for: {savedLogin.Username}");
                    var user = await cosmosService.GetUserByUsernameAsync(savedLogin.Username);
                    if (user != null && user.IsActive && user.AppPreferences?.AutoLogin == true)
                    {
                        Log("Auto-login: User found and active, initializing session...");
                        
                        // Get WAN IP for proper tracking
                        string wanIP = await Services.AuthenticationService.GetPublicIPAddressAsync();
                        Log($"[Auto-Login] WAN IP: {wanIP}");
                        
                        // Properly initialize authentication session
                        await authService.SetCurrentUserForAutoLoginAsync(user, wanIP);
                        
                        loggedInUser = user;
                        Log($"? Auto-login successful for: {user.Username}");
                    }
                    else
                    {
                        Log("Auto-login: User not found or inactive");
                    }
                }
                else
                {
                    Log("No auto-login found.");
                }

                // Show login if needed
                if (loggedInUser == null)
                {
                    Log("Showing login window...");
                    
                    Log("Creating LoginWindow...");
                    var loginWindow = new LoginWindow(cosmosService);
                    Log("Showing LoginWindow dialog...");
                    var dialogResult = loginWindow.ShowDialog();
                    
                    Log($"Login dialog result: {dialogResult}");
                    if (dialogResult != true || loginWindow.LoggedInUser == null)
                    {
                        Log("Login cancelled or failed. Shutting down...");
                        Shutdown();
                        return;
                    }
                    
                    loggedInUser = loginWindow.LoggedInUser;
                    Log($"? Login successful for: {loggedInUser.Username}");
                }
                else
                {
                    Log("? Auto-login successful - proceeding to MainWindow");
                }

                _currentUsername = loggedInUser?.Username ?? "System";

                // Load settings
                Log("Loading app settings...");
                var appSettings = new AppSettings(); // Use default settings for now
                Log("? App settings loaded!");

                // Show main window
                Log("Creating MainWindow...");
                await Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        Log("Passing CosmosDbService to MainWindow...");
                        var mainWindow = new MainWindow(cosmosService, loggedInUser!, appSettings);
                        Log("Showing MainWindow...");
                        mainWindow.Show();
                        Log("? MainWindow shown successfully!");
                    }
                    catch (Exception ex)
                    {
                        Log($"? FATAL ERROR creating/showing MainWindow: {ex.Message}");
                        Log($"Stack: {ex.StackTrace}");
                        if (ex.InnerException != null)
                        {
                            Log($"Inner: {ex.InnerException.Message}");
                            Log($"Inner Stack: {ex.InnerException.StackTrace}");
                        }
                        throw;
                    }
                });
                
                Log("=== InitializeApplicationAsync COMPLETED SUCCESSFULLY ===");
            }
            catch (Exception ex)
            {
                Log($"? FATAL ERROR in InitializeApplicationAsync: {ex.Message}");
                Log($"Stack: {ex.StackTrace}");
                throw;
            }
        }
    }
}
```

### Step 2: Run the App

1. **Replace App.xaml.cs** with the diagnostic version above
2. **Press F5** to run
3. **Watch the desktop** - a log file will be created: `DJBookingSystem_StartupLog_[timestamp].txt`
4. **Wait for the crash/shutdown**
5. **Open the log file** and find the LAST line before it stopped

### Step 3: Share the Log

The log file will tell us **EXACTLY** where it's failing:

- If it stops at "Creating SplashScreen instance..." ? XAML error in SplashScreen.xaml
- If it stops at "Initializing Azure Cosmos DB..." ? Network/Cosmos issue
- If it stops at "Creating MainWindow..." ? XAML error in MainWindow.xaml
- If it stops at "Passing CosmosDbService to MainWindow..." ? Constructor crash

### Expected Log (if working):

```
[12:34:56.123] === APP CONSTRUCTOR CALLED ===
[12:34:56.124] ? Exit event handler attached
[12:34:56.125] === APPLICATION_STARTUP CALLED ===
[12:34:56.126] ? Exception handlers installed
[12:34:56.127] === InitializeApplicationAsync Started ===
[12:34:56.128] Creating CosmosDB connection string...
[12:34:56.129] ? Connection string created
[12:34:56.130] Creating CosmosDbService...
[12:34:56.131] ? CosmosDbService created
[12:34:56.132] Creating splash screen...
[12:34:56.500] ? Splash screen shown
[12:34:56.501] Initializing Azure Cosmos DB...
[12:34:57.200] ? Azure Cosmos DB initialized!
[12:35:05.000] ? MainWindow shown successfully!
```

---

**Use this diagnostic version and share the log file content with me. It will tell us the EXACT line where it's failing!**
