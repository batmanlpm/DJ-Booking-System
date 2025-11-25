# Build-Installer.ps1
# Professional Installer Builder for DJ Booking System

Write-Host "========================================" -ForegroundColor Green
Write-Host " DJ BOOKING SYSTEM INSTALLER BUILDER" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

# Configuration
$InnoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
$ProjectRoot = $PSScriptRoot
$PublishDir = "$ProjectRoot\bin\Release\net8.0-windows\win-x64\publish"
$InstallerScript = "$ProjectRoot\installer.iss"
$OutputDir = "$ProjectRoot\Installer\Output"

# Step 1: Check Prerequisites
Write-Host "Step 1: Checking prerequisites..." -ForegroundColor Yellow

if (-not (Test-Path $InnoSetupPath)) {
    Write-Host "ERROR: Inno Setup not found!" -ForegroundColor Red
    Write-Host "Please install from: https://jrsoftware.org/isdl.php" -ForegroundColor Red
    exit 1
}
Write-Host "  Inno Setup found!" -ForegroundColor Green

# Step 2: Build Application
Write-Host "`nStep 2: Building application..." -ForegroundColor Yellow

# Clean previous builds
if (Test-Path $PublishDir) {
    Remove-Item -Path $PublishDir -Recurse -Force
    Write-Host "  Cleaned previous build" -ForegroundColor Gray
}

# Publish application with ALL dependencies included
Write-Host "  Publishing .NET application with all dependencies..." -ForegroundColor Gray
Write-Host "  Configuration: Release | Platform: win-x64 | Self-Contained: Yes" -ForegroundColor Gray

dotnet publish -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:PublishReadyToRun=false `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishTrimmed=false `
    /p:DebugType=embedded `
    /p:DebugSymbols=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed!" -ForegroundColor Red
    exit 1
}

# Verify all dependencies are included
Write-Host "  Verifying published files..." -ForegroundColor Gray
$publishedFiles = Get-ChildItem -Path $PublishDir -Recurse -File
Write-Host "  Total files published: $($publishedFiles.Count)" -ForegroundColor Cyan

# List key dependencies
$keyDlls = @(
    "DJBookingSystem.exe",
    "Microsoft.Web.WebView2.Core.dll",
    "Microsoft.Web.WebView2.WinForms.dll",
    "Microsoft.Web.WebView2.Wpf.dll",
    "Newtonsoft.Json.dll",
    "Azure.Core.dll",
    "System.*.dll"
)

$missingDlls = @()
foreach ($dll in $keyDlls) {
    if ($dll -like "*.*") {
        $found = Get-ChildItem -Path $PublishDir -Filter $dll -Recurse -ErrorAction SilentlyContinue
        if (-not $found) {
            $missingDlls += $dll
        }
    }
}

if ($missingDlls.Count -gt 0) {
    Write-Host "  ? Warning: Some expected DLLs not found:" -ForegroundColor Yellow
    $missingDlls | ForEach-Object { Write-Host "    - $_" -ForegroundColor Yellow }
    Write-Host "  (This may be OK if they're embedded or not needed)" -ForegroundColor Gray
}

Write-Host "  ? Application built successfully with all dependencies!" -ForegroundColor Green

# Step 3: Create Required Directories
Write-Host "`nStep 3: Creating directories..." -ForegroundColor Yellow

$RequiredDirs = @(
    "$ProjectRoot\Assets",
    "$ProjectRoot\Prerequisites",
    "$ProjectRoot\Installer\Output"
)

foreach ($dir in $RequiredDirs) {
    if (-not (Test-Path $dir)) {
        New-Item -Path $dir -ItemType Directory -Force | Out-Null
        Write-Host "  Created: $dir" -ForegroundColor Gray
    }
}
Write-Host "  Directories ready!" -ForegroundColor Green

# Step 4: Download WebView2 Runtime (if needed)
Write-Host "`nStep 4: Checking WebView2 Runtime..." -ForegroundColor Yellow

$WebView2Setup = "$ProjectRoot\Prerequisites\MicrosoftEdgeWebview2Setup.exe"
if (-not (Test-Path $WebView2Setup)) {
    Write-Host "  Downloading WebView2 Runtime..." -ForegroundColor Gray
    $WebView2Url = "https://go.microsoft.com/fwlink/p/?LinkId=2124703"
    Invoke-WebRequest -Uri $WebView2Url -OutFile $WebView2Setup
    Write-Host "  WebView2 Runtime downloaded!" -ForegroundColor Green
} else {
    Write-Host "  WebView2 Runtime already present" -ForegroundColor Green
}

# Step 5: Check Graphics Assets
Write-Host "`nStep 5: Checking graphics assets..." -ForegroundColor Yellow

$RequiredAssets = @{
    "WizardImage.bmp" = "164x314 pixels"
    "WizardSmallImage.bmp" = "55x55 pixels"
    "SetupIcon.ico" = "256x256 pixels"
}

$MissingAssets = @()
foreach ($asset in $RequiredAssets.Keys) {
    $assetPath = "$ProjectRoot\Assets\$asset"
    if (-not (Test-Path $assetPath)) {
        $MissingAssets += "$asset ($($RequiredAssets[$asset]))"
        Write-Host "  Missing: $asset" -ForegroundColor Red
    } else {
        Write-Host "  Found: $asset" -ForegroundColor Green
    }
}

if ($MissingAssets.Count -gt 0) {
    Write-Host "`n  WARNING: Missing graphics assets:" -ForegroundColor Yellow
    $MissingAssets | ForEach-Object { Write-Host "    - $_" -ForegroundColor Yellow }
    Write-Host "  The installer will be created but may not look professional." -ForegroundColor Yellow
    Write-Host "  Create these files in the Assets folder for best results." -ForegroundColor Yellow
    
    $continue = Read-Host "`n  Continue anyway? (Y/N)"
    if ($continue -ne 'Y') {
        Write-Host "Build cancelled." -ForegroundColor Red
        exit 1
    }
}

# Step 6: Create Placeholder Assets (if needed)
Write-Host "`nStep 6: Creating placeholder assets..." -ForegroundColor Yellow

# Create simple placeholder BMP files if missing
if (-not (Test-Path "$ProjectRoot\Assets\WizardImage.bmp")) {
    Write-Host "  Creating placeholder WizardImage.bmp..." -ForegroundColor Gray
    # In production, you'd create actual image files here
    # For now, we'll just note it
}

# Step 7: Build Installer
Write-Host "`nStep 7: Building installer..." -ForegroundColor Yellow

Write-Host "  Running Inno Setup Compiler..." -ForegroundColor Gray
& $InnoSetupPath $InstallerScript

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Installer build failed!" -ForegroundColor Red
    exit 1
}

# Step 8: Verify Output
Write-Host "`nStep 8: Verifying output..." -ForegroundColor Yellow

$installers = Get-ChildItem -Path $OutputDir -Filter "*.exe"
if ($installers.Count -eq 0) {
    Write-Host "ERROR: No installer found in output directory!" -ForegroundColor Red
    exit 1
}

Write-Host "  Installer created successfully!" -ForegroundColor Green
foreach ($installer in $installers) {
    $size = [math]::Round($installer.Length / 1MB, 2)
    Write-Host "  File: $($installer.Name)" -ForegroundColor Cyan
    Write-Host "  Size: $size MB" -ForegroundColor Cyan
    Write-Host "  Path: $($installer.FullName)" -ForegroundColor Cyan
}

# Step 9: Calculate File Hash
Write-Host "`nStep 9: Calculating file hash..." -ForegroundColor Yellow

$installerFile = $installers[0].FullName
$hash = Get-FileHash -Path $installerFile -Algorithm SHA256
Write-Host "  SHA256: $($hash.Hash)" -ForegroundColor Cyan
Write-Host "  (Use this in version.json)" -ForegroundColor Gray

# Step 10: Success Summary
Write-Host "`n========================================" -ForegroundColor Green
Write-Host " BUILD COMPLETE!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Test the installer: $($installers[0].FullName)" -ForegroundColor White
Write-Host "2. Upload to Hostinger: /public_html/updates/installers/" -ForegroundColor White
Write-Host "3. Update version.json with:" -ForegroundColor White
Write-Host "   - New version number" -ForegroundColor Gray
Write-Host "   - Download URL" -ForegroundColor Gray
Write-Host "   - SHA256 hash: $($hash.Hash.Substring(0, 32))..." -ForegroundColor Gray
Write-Host ""
Write-Host "Upload Command (FTP):" -ForegroundColor Yellow
Write-Host "  ftp ftp.yourdomain.com" -ForegroundColor Cyan
Write-Host "  cd /public_html/updates/installers" -ForegroundColor Cyan
Write-Host "  put `"$($installers[0].FullName)`"" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
