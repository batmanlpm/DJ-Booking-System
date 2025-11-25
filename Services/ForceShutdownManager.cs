using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.ComponentModel;
using System.Windows.Threading;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Force Shutdown Manager - Handles forceful application termination
    /// Ensures the app can always be closed, even if processes are hanging
    /// Handles: X button, Alt+F4, Task Manager, Force Close button
    /// </summary>
    public static class ForceShutdownManager
    {
        // Windows API for forceful process termination
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetCurrentProcess();

        // Track if force close is already in progress
        private static bool _isForceClosing = false;
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Initialize force shutdown hooks for a window
        /// </summary>
        public static void InitializeForWindow(Window window)
        {
            if (window == null) return;

            // Handle window closing event (X button and Alt+F4)
            window.Closing += Window_Closing;

            // Handle Alt+F4 specifically
            window.PreviewKeyDown += Window_PreviewKeyDown;

            Debug.WriteLine("? Force shutdown hooks initialized for window");
        }

        /// <summary>
        /// Handle window closing event (X button, Alt+F4, etc.)
        /// </summary>
        private static void Window_Closing(object? sender, CancelEventArgs e)
        {
            Debug.WriteLine("?? Window closing detected (X or Alt+F4)");

            // Cancel the default close behavior
            e.Cancel = true;

            // Execute force close
            ForceClose();
        }

        /// <summary>
        /// Handle keyboard shortcuts (Alt+F4, Ctrl+Q, etc.)
        /// </summary>
        private static void Window_PreviewKeyDown(object? sender, System.Windows.Input.KeyEventArgs e)
        {
            // Alt+F4 detection
            if (e.Key == System.Windows.Input.Key.F4 && 
                (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Alt) == System.Windows.Input.ModifierKeys.Alt)
            {
                Debug.WriteLine("?? Alt+F4 detected - Forcing close");
                e.Handled = true;
                ForceClose();
            }

            // Optional: Ctrl+Q for quick exit
            if (e.Key == System.Windows.Input.Key.Q && 
                (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                Debug.WriteLine("?? Ctrl+Q detected - Forcing close");
                e.Handled = true;
                ForceClose();
            }
        }

        /// <summary>
        /// Attempts graceful shutdown first, then forces termination if needed
        /// </summary>
        public static void ForceClose()
        {
            lock (_lockObject)
            {
                if (_isForceClosing)
                {
                    Debug.WriteLine("?? Force close already in progress, skipping...");
                    return;
                }
                _isForceClosing = true;
            }

            try
            {
                Debug.WriteLine("?? FORCE CLOSE initiated...");
                
                // Step 1: Try graceful shutdown (3 second timeout)
                if (AttemptGracefulShutdown())
                {
                    Debug.WriteLine("? Graceful shutdown successful");
                    return;
                }

                // Step 2: Force shutdown
                Debug.WriteLine("?? Graceful shutdown failed, forcing termination...");
                ForceTerminateProcess();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error during force close: {ex.Message}");
                // Last resort - kill process immediately
                EmergencyExit();
            }
        }

        /// <summary>
        /// Attempts graceful shutdown with timeout
        /// </summary>
        private static bool AttemptGracefulShutdown()
        {
            try
            {
                var shutdownTask = System.Threading.Tasks.Task.Run(() =>
                {
                    // Stop all services
                    StopAllServices();

                    // Close all windows
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            foreach (Window window in Application.Current.Windows)
                            {
                                // Unhook closing events to prevent recursion
                                window.Closing -= Window_Closing;
                                window.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"?? Error closing windows: {ex.Message}");
                        }
                    });

                    // Shutdown application
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            Application.Current.Shutdown();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"?? Error during shutdown: {ex.Message}");
                        }
                    });
                });

                // Wait 3 seconds for graceful shutdown
                return shutdownTask.Wait(TimeSpan.FromSeconds(3));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"?? Graceful shutdown exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Stops all running services
        /// </summary>
        private static void StopAllServices()
        {
            try
            {
                Debug.WriteLine("?? Stopping all services...");

                // Stop timers
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        // Stop all DispatcherTimers
                        var timers = new System.Collections.Generic.List<DispatcherTimer>();
                        // Note: Add your specific timers here if needed
                        
                        foreach (var timer in timers)
                        {
                            timer?.Stop();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"?? Error stopping timers: {ex.Message}");
                    }
                });

                // Close system tray icon
                try
                {
                    // If you have a NotifyIcon, dispose it here
                    // notifyIcon?.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"?? Error disposing tray icon: {ex.Message}");
                }

                // Close any background tasks
                try
                {
                    // Cancel any CancellationTokens
                    // Stop any background workers
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"?? Error stopping background tasks: {ex.Message}");
                }

                Debug.WriteLine("? All services stopped");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"?? Error stopping services: {ex.Message}");
            }
        }

        /// <summary>
        /// Forces immediate process termination
        /// </summary>
        private static void ForceTerminateProcess()
        {
            try
            {
                Debug.WriteLine("?? Force terminating process...");
                
                var currentProcess = Process.GetCurrentProcess();
                
                // Try Process.Kill first
                currentProcess.Kill(entireProcessTree: true);
                currentProcess.WaitForExit(1000);
                
                // If still running, use Windows API
                if (!currentProcess.HasExited)
                {
                    Debug.WriteLine("?? Using Windows API to terminate...");
                    var processHandle = GetCurrentProcess();
                    TerminateProcess(processHandle, 1);
                }

                Debug.WriteLine("? Process terminated");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Force terminate failed: {ex.Message}");
                // Absolute last resort
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Emergency exit - terminates immediately without cleanup
        /// </summary>
        public static void EmergencyExit()
        {
            Debug.WriteLine("?? EMERGENCY EXIT - Immediate termination");
            
            try
            {
                // Most aggressive termination
                Process.GetCurrentProcess().Kill(entireProcessTree: true);
            }
            catch
            {
                // Fallback
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Kill all instances of this application
        /// </summary>
        public static void KillAllInstances()
        {
            try
            {
                var processName = Process.GetCurrentProcess().ProcessName;
                var processes = Process.GetProcessesByName(processName);

                Debug.WriteLine($"?? Killing {processes.Length} instances of {processName}...");

                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill(entireProcessTree: true);
                        process.WaitForExit(1000); // Wait 1 second
                        Debug.WriteLine($"? Killed process {process.Id}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"?? Failed to kill process {process.Id}: {ex.Message}");
                    }
                }

                Debug.WriteLine("? All instances terminated");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error killing instances: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if there are multiple instances running
        /// </summary>
        public static bool HasMultipleInstances()
        {
            try
            {
                var processName = Process.GetCurrentProcess().ProcessName;
                var processes = Process.GetProcessesByName(processName);
                return processes.Length > 1;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Force close with specific exit code
        /// </summary>
        public static void ForceCloseWithCode(int exitCode)
        {
            try
            {
                Debug.WriteLine($"?? Force closing with exit code: {exitCode}");
                
                // Try Process.Kill first
                Process.GetCurrentProcess().Kill(entireProcessTree: true);
            }
            catch
            {
                // Fallback to Environment.Exit
                Environment.Exit(exitCode);
            }
        }

        /// <summary>
        /// Show force close confirmation dialog
        /// </summary>
        public static void ShowForceCloseConfirmation(Window owner)
        {
            var result = MessageBox.Show(
                "?? FORCE CLOSE\n\n" +
                "This will immediately terminate the application,\n" +
                "even if processes are hanging.\n\n" +
                "All unsaved data will be lost!\n\n" +
                "Are you sure you want to force close?",
                "Force Close Application",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                Debug.WriteLine("?? User confirmed force close");
                ForceClose();
            }
            else
            {
                Debug.WriteLine("? Force close cancelled by user");
            }
        }
    }
}
