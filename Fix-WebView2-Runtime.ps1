# WebView2 Runtime - Automatic Installation & Fix
# Fixes: "CoreWebView2 is null after initialization" errors

Write-Host ""
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?     WEBVIEW2 RUNTIME - INSTALLATION & FIX               ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Check if WebView2 Runtime is installed
$webView2Path = "${env:ProgramFiles(x86)}\Microsoft\EdgeWebView\Application"
$isInstalled = Test-Path $webView2Path

Write-Host "Checking WebView2 Runtime installation..." -ForegroundColor Yellow
Write-Host ""

if ($isInstalled) {
    Write-Host "? WebView2 Runtime is INSTALLED" -ForegroundColor Green
    
    # Get version
    $versions = Get-ChildItem $webView2Path -Directory -ErrorAction SilentlyContinue
    if ($versions) {
        $latestVersion = $versions | Sort-Object Name -Descending | Select-Object -First 1
        Write-Host "  Version: $($latestVersion.Name)" -ForegroundColor Cyan
        Write-Host "  Path: $webView2Path" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "WebView2 Runtime is installed but errors persist?" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Possible solutions:" -ForegroundColor White
    Write-Host "  1. Repair/Reinstall WebView2 Runtime" -ForegroundColor White
    Write-Host "  2. Update Windows to latest version" -ForegroundColor White
    Write-Host "  3. Restart your computer" -ForegroundColor White
    Write-Host "  4. Check application code for initialization issues" -ForegroundColor White
    Write-Host ""
    
    $response = Read-Host "Would you like to reinstall WebView2 Runtime? (yes/no)"
    
    if ($response -eq "yes") {
        Write-Host ""
        Write-Host "Opening WebView2 Runtime installer..." -ForegroundColor Yellow
        Start-Process "https://go.microsoft.com/fwlink/p/?LinkId=2124703"
        Write-Host "? Installer opened in browser" -ForegroundColor Green
        Write-Host ""
        Write-Host "After installation:" -ForegroundColor Yellow
        Write-Host "  1. Restart this application" -ForegroundColor White
        Write-Host "  2. Run your DJ Booking System again" -ForegroundColor White
    }
}
else {
    Write-Host "? WebView2 Runtime is NOT INSTALLED" -ForegroundColor Red
    Write-Host ""
    Write-Host "This is required for your DJ Booking System!" -ForegroundColor Yellow
    Write-Host ""
    
    Write-Host "Solution:" -ForegroundColor Green
    Write-Host "  Install Microsoft Edge WebView2 Runtime" -ForegroundColor White
    Write-Host ""
    
    $response = Read-Host "Would you like to download and install it now? (yes/no)"
    
    if ($response -eq "yes") {
        Write-Host ""
        Write-Host "Opening WebView2 Runtime installer..." -ForegroundColor Yellow
        Write-Host ""
        
        try {
            Start-Process "https://go.microsoft.com/fwlink/p/?LinkId=2124703"
            Write-Host "? Installer opened in browser" -ForegroundColor Green
            Write-Host ""
            Write-Host "?? Installation Steps:" -ForegroundColor Cyan
            Write-Host "  1. Download MicrosoftEdgeWebview2Setup.exe" -ForegroundColor White
            Write-Host "  2. Run the installer" -ForegroundColor White
            Write-Host "  3. Follow the installation prompts" -ForegroundColor White
            Write-Host "  4. Restart your DJ Booking System" -ForegroundColor White
            Write-Host ""
            Write-Host "??  Installation takes about 1-2 minutes" -ForegroundColor Gray
        }
        catch {
            Write-Host "? Error opening installer: $_" -ForegroundColor Red
            Write-Host ""
            Write-Host "Manual Installation:" -ForegroundColor Yellow
            Write-Host "  1. Open your browser" -ForegroundColor White
            Write-Host "  2. Go to: https://go.microsoft.com/fwlink/p/?LinkId=2124703" -ForegroundColor White
            Write-Host "  3. Download and run the installer" -ForegroundColor White
        }
    }
    else {
        Write-Host ""
        Write-Host "??  WebView2 Runtime is REQUIRED!" -ForegroundColor Red
        Write-Host ""
        Write-Host "Your application will NOT work without it." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Download manually from:" -ForegroundColor White
        Write-Host "  https://go.microsoft.com/fwlink/p/?LinkId=2124703" -ForegroundColor Cyan
    }
}

Write-Host ""
Write-Host "????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Additional troubleshooting
Write-Host "?? TROUBLESHOOTING CHECKLIST:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  [ ] WebView2 Runtime installed" -ForegroundColor White
Write-Host "  [ ] Windows 10 version 1803 or higher" -ForegroundColor White
Write-Host "  [ ] Windows is up to date" -ForegroundColor White
Write-Host "  [ ] Application restarted after installation" -ForegroundColor White
Write-Host "  [ ] Computer restarted if errors persist" -ForegroundColor White
Write-Host ""

# Check Windows version
$osVersion = [System.Environment]::OSVersion.Version
Write-Host "Current Windows Version: $($osVersion.Major).$($osVersion.Minor).$($osVersion.Build)" -ForegroundColor Gray
Write-Host ""

if ($osVersion.Build -lt 17134) {
    Write-Host "??  WARNING: Your Windows version may be too old" -ForegroundColor Red
    Write-Host "   WebView2 requires Windows 10 version 1803 (Build 17134) or higher" -ForegroundColor Yellow
    Write-Host "   Please update Windows" -ForegroundColor Yellow
}
else {
    Write-Host "? Windows version is compatible with WebView2" -ForegroundColor Green
}

Write-Host ""
Write-Host "????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Offer to create a fix code file
Write-Host "Would you like to add WebView2 error handling to your code? (yes/no)" -ForegroundColor Yellow
$addCode = Read-Host

if ($addCode -eq "yes") {
    Write-Host ""
    Write-Host "Creating WebView2 helper class..." -ForegroundColor Yellow
    
    $helperCode = @"
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Helper service for WebView2 initialization and error handling
    /// </summary>
    public class WebView2Helper
    {
        /// <summary>
        /// Check if WebView2 Runtime is installed
        /// </summary>
        public static bool IsWebView2RuntimeInstalled()
        {
            try
            {
                string runtimePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    @"Microsoft\EdgeWebView\Application");
                
                return Directory.Exists(runtimePath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get installed WebView2 Runtime version
        /// </summary>
        public static string? GetWebView2Version()
        {
            try
            {
                string runtimePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    @"Microsoft\EdgeWebView\Application");
                
                if (Directory.Exists(runtimePath))
                {
                    var versions = Directory.GetDirectories(runtimePath);
                    if (versions.Length > 0)
                    {
                        return Path.GetFileName(versions[0]);
                    }
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Show user-friendly error dialog if WebView2 is not installed
        /// </summary>
        public static void ShowInstallationDialog()
        {
            var result = MessageBox.Show(
                "?? WebView2 Runtime Required\n\n" +
                "This application requires Microsoft Edge WebView2 Runtime.\n\n" +
                "WebView2 Runtime is not installed on your computer.\n\n" +
                "Would you like to download it now?\n\n" +
                "Click Yes to open the download page.",
                "WebView2 Runtime Required",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://go.microsoft.com/fwlink/p/?LinkId=2124703",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Failed to open download page.\n\n" +
                        "Please manually download from:\n" +
                        "https://go.microsoft.com/fwlink/p/?LinkId=2124703\n\n" +
                        "Error: " + ex.Message,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Initialize WebView2 with error handling
        /// </summary>
        public static async Task<bool> InitializeWebView2Async(Microsoft.Web.WebView2.Wpf.WebView2 webView)
        {
            try
            {
                // Check if runtime is installed
                if (!IsWebView2RuntimeInstalled())
                {
                    ShowInstallationDialog();
                    return false;
                }

                // Initialize WebView2
                await webView.EnsureCoreWebView2Async(null);
                
                if (webView.CoreWebView2 == null)
                {
                    MessageBox.Show(
                        "WebView2 initialization failed.\n\n" +
                        "CoreWebView2 is null after initialization.\n\n" +
                        "Please try:\n" +
                        "1. Restart this application\n" +
                        "2. Restart your computer\n" +
                        "3. Reinstall WebView2 Runtime",
                        "WebView2 Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to initialize WebView2:\n\n" +
                    ex.Message + "\n\n" +
                    "Please ensure WebView2 Runtime is installed from:\n" +
                    "https://go.microsoft.com/fwlink/p/?LinkId=2124703",
                    "WebView2 Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }
    }
}
"@

    $helperPath = "Services\WebView2Helper.cs"
    
    try {
        Set-Content -Path $helperPath -Value $helperCode -Encoding UTF8
        Write-Host "? Created: $helperPath" -ForegroundColor Green
        Write-Host ""
        Write-Host "Usage in your code:" -ForegroundColor Cyan
        Write-Host @"
  
  // In your window's Loaded event or initialization:
  if (!WebView2Helper.IsWebView2RuntimeInstalled())
  {
      WebView2Helper.ShowInstallationDialog();
      return;
  }
  
  // Initialize WebView2 with error handling:
  bool success = await WebView2Helper.InitializeWebView2Async(YourWebView2Control);
  if (success)
  {
      // WebView2 is ready to use
      YourWebView2Control.CoreWebView2.Navigate("https://yoururl.com");
  }

"@ -ForegroundColor White
    }
    catch {
        Write-Host "? Error creating helper file: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
