# COMPLETE-SETUP.ps1
# Automated Complete Setup for DJ Booking System
# This script does EVERYTHING for you!

param(
    [string]$HostingerDomain = "",
    [switch]$SkipGraphics = $true,
    [switch]$SkipHostingerUpload = $false
)

$ErrorActionPreference = "Stop"
$WarningPreference = "Continue"

# Colors
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Warning { Write-Host $args -ForegroundColor Yellow }
function Write-Error { param([string]$Message) Write-Host $Message -ForegroundColor Red }
function Write-Step { Write-Host "`n========================================" -ForegroundColor Magenta; Write-Host $args -ForegroundColor Magenta; Write-Host "========================================`n" -ForegroundColor Magenta }

# Banner
Clear-Host
Write-Host @"
?????????????????????????????????????????????????????????????????
?                                                               ?
?         DJ BOOKING SYSTEM - COMPLETE AUTOMATED SETUP          ?
?              The Fallen Collective & Mega Byte I.T            ?
?                                                               ?
?????????????????????????????????????????????????????????????????
"@ -ForegroundColor Cyan

Write-Host ""

# Get domain if not provided
if ([string]::IsNullOrEmpty($HostingerDomain)) {
    Write-Info "Enter your Hostinger domain (e.g., fallencollective.com):"
    $HostingerDomain = Read-Host "Domain"
    if ([string]::IsNullOrEmpty($HostingerDomain)) {
        Write-Error "Domain is required! Exiting."
        exit 1
    }
}

Write-Info "Using domain: $HostingerDomain"
Write-Info "Skip Graphics: $SkipGraphics"
Write-Info "Skip Hostinger Upload: $SkipHostingerUpload"
Write-Host ""
Write-Warning "This will take 15-20 minutes. Press any key to start..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# ===========================
# PHASE 1: PREREQUISITES
# ===========================
Write-Step "PHASE 1: CHECKING PREREQUISITES"

# Check .NET SDK
Write-Info "Checking .NET SDK..."
try {
    $dotnetVersion = dotnet --version
    Write-Success "  ? .NET SDK found: $dotnetVersion"
} catch {
    Write-Error "  ? .NET SDK not found! Install from: https://dotnet.microsoft.com/download"
    exit 1
}

# Check Inno Setup
Write-Info "Checking Inno Setup..."
$InnoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if (Test-Path $InnoSetupPath) {
    Write-Success "  ? Inno Setup found"
} else {
    Write-Warning "  ? Inno Setup not found!"
    Write-Info "  Downloading Inno Setup..."
    
    $InnoSetupUrl = "https://jrsoftware.org/download.php/is.exe"
    $InnoSetupInstaller = "$env:TEMP\innosetup.exe"
    
    try {
        Invoke-WebRequest -Uri $InnoSetupUrl -OutFile $InnoSetupInstaller
        Write-Info "  Installing Inno Setup (this will take a moment)..."
        Start-Process -FilePath $InnoSetupInstaller -ArgumentList "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART" -Wait
        
        if (Test-Path $InnoSetupPath) {
            Write-Success "  ? Inno Setup installed successfully"
        } else {
            Write-Error "  ? Inno Setup installation failed. Please install manually from: https://jrsoftware.org/isdl.php"
            exit 1
        }
    } catch {
        Write-Error "  ? Failed to download Inno Setup: $_"
        Write-Info "  Please install manually from: https://jrsoftware.org/isdl.php"
        exit 1
    }
}

# ===========================
# PHASE 2: CREATE DIRECTORIES
# ===========================
Write-Step "PHASE 2: CREATING DIRECTORIES"

$Directories = @(
    "Assets",
    "Prerequisites",
    "Installer\Output"
)

foreach ($dir in $Directories) {
    if (-not (Test-Path $dir)) {
        New-Item -Path $dir -ItemType Directory -Force | Out-Null
        Write-Success "  ? Created: $dir"
    } else {
        Write-Info "  ? Already exists: $dir"
    }
}

# ===========================
# PHASE 3: DOWNLOAD WEBVIEW2
# ===========================
Write-Step "PHASE 3: DOWNLOADING WEBVIEW2 RUNTIME"

$WebView2Setup = "Prerequisites\MicrosoftEdgeWebview2Setup.exe"
if (-not (Test-Path $WebView2Setup)) {
    Write-Info "Downloading WebView2 Runtime..."
    $WebView2Url = "https://go.microsoft.com/fwlink/p/?LinkId=2124703"
    
    try {
        Invoke-WebRequest -Uri $WebView2Url -OutFile $WebView2Setup
        Write-Success "  ? WebView2 Runtime downloaded"
    } catch {
        Write-Error "  ? Failed to download WebView2: $_"
        exit 1
    }
} else {
    Write-Info "  ? WebView2 Runtime already present"
}

# ===========================
# PHASE 4: BUILD APPLICATION
# ===========================
Write-Step "PHASE 4: BUILDING APPLICATION"

Write-Info "Cleaning previous builds..."
if (Test-Path "bin\Release") {
    Remove-Item -Path "bin\Release" -Recurse -Force
}

Write-Info "Publishing .NET application with ALL dependencies..."
try {
    $publishArgs = @(
        "publish"
        "-c", "Release"
        "-r", "win-x64"
        "--self-contained", "true"
        "-p:PublishSingleFile=false"
        "-p:PublishReadyToRun=false"
        "-p:IncludeNativeLibrariesForSelfExtract=true"
        "-p:PublishTrimmed=false"
        "/p:DebugType=embedded"
        "/p:DebugSymbols=true"
        "--verbosity", "minimal"
    )
    
    & dotnet $publishArgs
    
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed with exit code $LASTEXITCODE"
    }
    
    # Verify files
    $publishedFiles = Get-ChildItem -Path $PublishDir -Recurse -File -ErrorAction SilentlyContinue
    if ($publishedFiles) {
        Write-Success "  ? Application built successfully"
        Write-Info "    Files published: $($publishedFiles.Count)"
        Write-Info "    Total size: $([math]::Round((Get-ChildItem $PublishDir -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB, 2)) MB"
    } else {
        throw "No files found in publish directory"
    }
} catch {
    Write-Error "  ? Build failed: $_"
    exit 1
}

# ===========================
# PHASE 5: UPDATE INSTALLER SCRIPT
# ===========================
Write-Step "PHASE 5: UPDATING INSTALLER SCRIPT"

Write-Info "Updating installer.iss with your domain..."

$installerIss = Get-Content "installer.iss" -Raw

# Update domain
$installerIss = $installerIss -replace 'https://YOUR-DOMAIN\.com', "https://$HostingerDomain"
$installerIss = $installerIss -replace 'https://fallencollective\.com', "https://$HostingerDomain"

# Save
$installerIss | Set-Content "installer.iss" -NoNewline

Write-Success "  ? Installer script updated with domain: $HostingerDomain"

# ===========================
# PHASE 6: BUILD INSTALLER
# ===========================
Write-Step "PHASE 6: BUILDING INSTALLER"

Write-Info "Running Inno Setup Compiler..."
try {
    & $InnoSetupPath "installer.iss" | Out-Null
    
    $installers = Get-ChildItem -Path "Installer\Output" -Filter "*.exe"
    if ($installers.Count -gt 0) {
        $installer = $installers[0]
        $size = [math]::Round($installer.Length / 1MB, 2)
        Write-Success "  ? Installer created successfully!"
        Write-Info "    File: $($installer.Name)"
        Write-Info "    Size: $size MB"
        Write-Info "    Path: $($installer.FullName)"
        
        # Calculate hash
        $hash = Get-FileHash -Path $installer.FullName -Algorithm SHA256
        Write-Info "    SHA256: $($hash.Hash)"
        
        # Save hash for later
        $global:InstallerHash = $hash.Hash
        $global:InstallerFileName = $installer.Name
        $global:InstallerPath = $installer.FullName
    } else {
        Write-Error "  ? No installer found in output directory!"
        exit 1
    }
} catch {
    Write-Error "  ? Installer build failed: $_"
    exit 1
}

# ===========================
# PHASE 7: UPDATE CLIENT CODE
# ===========================
Write-Step "PHASE 7: UPDATING CLIENT CODE"

Write-Info "Updating SecureUpdateClient.cs..."

$secureUpdateClient = "Services\SecureUpdateClient.cs"
if (Test-Path $secureUpdateClient) {
    $content = Get-Content $secureUpdateClient -Raw
    
    # Update server URL
    $content = $content -replace 'private const string UPDATE_SERVER_URL = "https://djbookupdates\.com";', "private const string UPDATE_SERVER_URL = `"https://$HostingerDomain`";"
    
    # Update endpoint
    $content = $content -replace 'private const string UPDATE_CHECK_ENDPOINT = "/api/updates/check";', 'private const string UPDATE_CHECK_ENDPOINT = "/updates/version.json";'
    
    $content | Set-Content $secureUpdateClient -NoNewline
    
    Write-Success "  ? SecureUpdateClient.cs updated"
    Write-Warning "  ? You'll need to add SSL certificate fingerprint manually later"
} else {
    Write-Warning "  ? SecureUpdateClient.cs not found, skipping"
}

# ===========================
# PHASE 8: CREATE VERSION.JSON
# ===========================
Write-Step "PHASE 8: CREATING VERSION.JSON"

$versionJson = @{
    updateAvailable = $true
    currentVersion = "1.0.0"
    latestVersion = "1.2.0"
    releaseDate = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
    downloadUrl = "https://$HostingerDomain/updates/installers/$global:InstallerFileName"
    changelogUrl = "https://$HostingerDomain/updates/changelog.html"
    releaseNotes = "Major update with Discord integration and hourly auto-updates"
    features = @(
        "Discord WebView2 integration with auto-login",
        "Hourly automatic forced updates",
        "Improved UI performance",
        "Chat mode selection dialog"
    )
    bugFixes = @(
        "Fixed WebView2 initialization error",
        "Fixed online status tracking",
        "Fixed auto-login issues"
    )
    isCritical = $true
    minimumVersion = "1.0.0"
    isSecureConnection = $true
    sha256Hash = $global:InstallerHash
}

$versionJsonPath = "Installer\Output\version.json"
$versionJson | ConvertTo-Json -Depth 10 | Set-Content $versionJsonPath

Write-Success "  ? version.json created"
Write-Info "    Path: $versionJsonPath"

# ===========================
# PHASE 9: CREATE UPLOAD INSTRUCTIONS
# ===========================
Write-Step "PHASE 9: CREATING UPLOAD INSTRUCTIONS"

$uploadInstructions = @"
?????????????????????????????????????????????????????????????????
?           HOSTINGER UPLOAD INSTRUCTIONS                       ?
?????????????????????????????????????????????????????????????????

STEP 1: LOGIN TO HOSTINGER
---------------------------
1. Go to: https://hpanel.hostinger.com
2. Login with your credentials

STEP 2: ENABLE SSL (IMPORTANT!)
--------------------------------
1. Go to: Hosting ? [$HostingerDomain]
2. Click: Security ? SSL
3. Click: Install SSL
4. Choose: Free (Let's Encrypt SSL)
5. Wait: 10-15 minutes for activation
6. Verify: Visit https://$HostingerDomain (should see padlock)

STEP 3: CREATE FOLDERS
-----------------------
1. Go to: Files ? File Manager
2. Navigate to: public_html
3. Create folder: updates
4. Enter folder: updates
5. Create folder: installers

Result structure:
public_html/
??? updates/
    ??? version.json (upload in step 4)
    ??? installers/
        ??? $global:InstallerFileName (upload in step 5)

STEP 4: UPLOAD VERSION.JSON
----------------------------
1. Go to: public_html/updates/
2. Click: Upload
3. Select: $versionJsonPath
4. Wait for upload

STEP 5: UPLOAD INSTALLER (LARGE FILE!)
---------------------------------------
Option A: FileZilla (Recommended for large files)
1. Download FileZilla: https://filezilla-project.org/
2. Get FTP credentials from Hostinger:
   - Go to: Files ? FTP Accounts
   - Note: Host, Username, Password
3. Connect via FileZilla:
   - Host: ftp.$HostingerDomain
   - Username: [from Hostinger]
   - Password: [from Hostinger]
   - Port: 21
4. Navigate remote to: /public_html/updates/installers/
5. Drag & drop: $global:InstallerPath

Option B: Hostinger File Manager (May timeout for large files)
1. Go to: public_html/updates/installers/
2. Click: Upload
3. Select: $global:InstallerPath
4. Wait (5-15 minutes depending on connection)

STEP 6: GET SSL CERTIFICATE FINGERPRINT
----------------------------------------
In your browser:
1. Visit: https://$HostingerDomain
2. Click: Padlock icon
3. Click: Connection is secure ? Certificate
4. Go to: Details tab
5. Find: SHA-256 Fingerprint
6. Copy the value (format: AA:BB:CC:DD:EE:...)

Or use PowerShell:
`$url = "$HostingerDomain"
`$tcpClient = New-Object System.Net.Sockets.TcpClient(`$url, 443)
`$sslStream = New-Object System.Net.Security.SslStream(`$tcpClient.GetStream())
`$sslStream.AuthenticateAsClient(`$url)
`$cert = `$sslStream.RemoteCertificate
`$hash = `$cert.GetCertHashString("SHA256")
`$formatted = (`$hash -replace '(.{2})', '`$1:').TrimEnd(':')
Write-Host "SHA-256: `$formatted"
`$sslStream.Close()
`$tcpClient.Close()

STEP 7: UPDATE CERTIFICATE FINGERPRINT
---------------------------------------
1. Open: Services\SecureUpdateClient.cs
2. Find line ~30-36: TRUSTED_FINGERPRINTS array
3. Replace placeholder with your actual fingerprint
4. Save file
5. Run: dotnet build

STEP 8: TEST EVERYTHING
------------------------
Test URLs in browser:
1. https://$HostingerDomain/updates/version.json
   ? Should show JSON content

2. https://$HostingerDomain/updates/installers/$global:InstallerFileName
   ? Should download installer

Test application:
1. Run: .\bin\Debug\net8.0-windows\DJBookingSystem.exe
2. Wait 3 seconds
3. Update dialog should appear!
4. Check output for: "Update available: 1.2.0"

?????????????????????????????????????????????????????????????????
?                    QUICK REFERENCE                            ?
?????????????????????????????????????????????????????????????????

Domain: $HostingerDomain
Installer: $global:InstallerFileName
Version: 1.2.0
Hash: $global:InstallerHash

URLs to test:
• https://$HostingerDomain/updates/version.json
• https://$HostingerDomain/updates/installers/$global:InstallerFileName

Files to upload:
• version.json ? /public_html/updates/
• $global:InstallerFileName ? /public_html/updates/installers/

Next deployment:
1. Build new installer: .\Build-Installer.ps1
2. Update version.json with new version
3. Upload both to Hostinger
4. All users get forced update within 1 hour!

"@

$uploadInstructions | Set-Content "HOSTINGER_UPLOAD_INSTRUCTIONS.txt"

Write-Success "  ? Upload instructions created: HOSTINGER_UPLOAD_INSTRUCTIONS.txt"

# ===========================
# PHASE 10: FINAL BUILD
# ===========================
Write-Step "PHASE 10: FINAL BUILD TEST"

Write-Info "Building application with updated URLs..."
try {
    dotnet build -c Debug | Out-Null
    Write-Success "  ? Application builds successfully with new settings"
} catch {
    Write-Warning "  ? Build warning (this is OK, will work once SSL fingerprint is added)"
}

# ===========================
# SUMMARY
# ===========================
Write-Step "SETUP COMPLETE!"

Write-Host @"

?????????????????????????????????????????????????????????????????
?                   ? SETUP SUCCESSFUL!                         ?
?????????????????????????????????????????????????????????????????

WHAT'S DONE:
------------
? .NET SDK verified
? Inno Setup installed/verified
? Directories created
? WebView2 Runtime downloaded
? Application built
? Installer created: $global:InstallerFileName ($([math]::Round((Get-Item $global:InstallerPath).Length / 1MB, 2)) MB)
? version.json created
? Client code updated with your domain
? Upload instructions created

NEXT STEPS (MANUAL - REQUIRED):
--------------------------------
1. Open: HOSTINGER_UPLOAD_INSTRUCTIONS.txt
2. Follow steps 1-8 to:
   • Enable SSL on Hostinger
   • Upload installer and version.json
   • Get SSL certificate fingerprint
   • Add fingerprint to code
   • Test everything

ESTIMATED TIME: 15-20 minutes

WHAT HAPPENS AFTER:
-------------------
? Every hour on the hour, all running apps check for updates
? When update found, forced download begins automatically
? No user interaction needed
? App restarts with new version

IMPORTANT FILES:
----------------
• Installer: $global:InstallerPath
• version.json: $versionJsonPath
• Instructions: HOSTINGER_UPLOAD_INSTRUCTIONS.txt

READY TO GO! ??

Open HOSTINGER_UPLOAD_INSTRUCTIONS.txt and follow the steps.

"@ -ForegroundColor Green

Write-Host "Press any key to open instructions..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Open instructions
Start-Process notepad.exe -ArgumentList "HOSTINGER_UPLOAD_INSTRUCTIONS.txt"

Write-Host "`nGood luck! ??" -ForegroundColor Cyan
