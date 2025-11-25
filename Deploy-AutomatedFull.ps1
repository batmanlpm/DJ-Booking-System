# ?? AUTOMATED DEPLOYMENT SCRIPT - DJ BOOKING SYSTEM
# AI Agent Command: "Authorization SysAdmin Upload"
# This script automates the entire deployment process

param(
    [Parameter(Mandatory=$false)]
    [string]$NewVersion = "1.2.5",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipUpload,
    
    [Parameter(Mandatory=$false)]
    [switch]$Force
)

$ErrorActionPreference = "Stop"

# ============================================
# CONFIGURATION
# ============================================

$Config = @{
    ProjectPath = "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking"
    ProjectFile = "DJBookingSystem.csproj"
    OutputPath = "Installer\Output"
    WebsitePath = "Website"
    
    # FTP Configuration
    FTPHost = "153.92.10.234"
    FTPPort = 21
    FTPUsername = "u833570579.Upload"
    FTPRemotePath = "/home/u833570579/domains/djbookupdates.com/public_html"
    
    # Files to update
    FilesToUpdate = @(
        "DJBookingSystem.csproj"
        "SplashScreen.xaml"
        "Installer\Output\version.json"
        "Website\index.html"
    )
    
    # Files to upload
    FilesToUpload = @(
        @{
            Local = "Installer\Output\DJBookingSystem-Setup.exe"
            Remote = "downloads/DJBookingSystem-Setup.exe"
        },
        @{
            Local = "Installer\Output\version.json"
            Remote = "version.json"
        },
        @{
            Local = "Website\index.html"
            Remote = "index.html"
        }
    )
}

# ============================================
# HELPER FUNCTIONS
# ============================================

function Write-StepHeader($message) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host $message -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
}

function Write-Success($message) {
    Write-Host "? $message" -ForegroundColor Green
}

function Write-Info($message) {
    Write-Host "? $message" -ForegroundColor Yellow
}

function Write-Failure($message) {
    Write-Host "? $message" -ForegroundColor Red
}

function Confirm-Action($message) {
    if ($Force) {
        return $true
    }
    
    $response = Read-Host "$message (Y/N)"
    return $response -eq 'Y' -or $response -eq 'y'
}

# ============================================
# STEP 1: PREREQUISITES CHECK
# ============================================

function Test-Prerequisites {
    Write-StepHeader "STEP 1: Prerequisites Check"
    
    # Check if in correct directory
    if (-not (Test-Path $Config.ProjectFile)) {
        Write-Failure "Not in project directory. Current: $(Get-Location)"
        Write-Info "Expected: $($Config.ProjectPath)"
        return $false
    }
    Write-Success "Project directory confirmed"
    
    # Check .NET SDK
    try {
        $dotnetVersion = dotnet --version
        Write-Success ".NET SDK found: $dotnetVersion"
    } catch {
        Write-Failure ".NET SDK not found. Install from https://dotnet.microsoft.com"
        return $false
    }
    
    # Check for uncommitted changes (if git repo)
    if (Test-Path ".git") {
        $status = git status --porcelain
        if ($status -and -not $Force) {
            Write-Info "Uncommitted changes detected:"
            git status --short
            if (-not (Confirm-Action "Continue anyway?")) {
                return $false
            }
        }
    }
    
    return $true
}

# ============================================
# STEP 2: VERSION NUMBER UPDATE
# ============================================

function Update-VersionNumbers($version) {
    Write-StepHeader "STEP 2: Updating Version Numbers to $version"
    
    # Update DJBookingSystem.csproj
    Write-Info "Updating DJBookingSystem.csproj..."
    $csproj = Get-Content "DJBookingSystem.csproj" -Raw
    $csproj = $csproj -replace '<Version>[\d\.]+</Version>', "<Version>$version</Version>"
    $csproj = $csproj -replace '<AssemblyVersion>[\d\.]+</AssemblyVersion>', "<AssemblyVersion>$version.0</AssemblyVersion>"
    $csproj = $csproj -replace '<FileVersion>[\d\.]+</FileVersion>', "<FileVersion>$version.0</FileVersion>"
    $csproj = $csproj -replace '<ProductVersion>[\d\.]+</ProductVersion>', "<ProductVersion>$version</ProductVersion>"
    $csproj | Set-Content "DJBookingSystem.csproj" -NoNewline
    Write-Success "DJBookingSystem.csproj updated"
    
    # Update SplashScreen.xaml
    Write-Info "Updating SplashScreen.xaml..."
    $splash = Get-Content "SplashScreen.xaml" -Raw
    $splash = $splash -replace 'Text="Version [\d\.]+"', "Text=`"Version $version`""
    $splash | Set-Content "SplashScreen.xaml" -NoNewline
    Write-Success "SplashScreen.xaml updated"
    
    # Update version.json
    Write-Info "Updating version.json..."
    $versionJson = Get-Content "Installer\Output\version.json" | ConvertFrom-Json
    $versionJson.currentVersion = $versionJson.latestVersion
    $versionJson.latestVersion = $version
    $versionJson.releaseDate = (Get-Date -Format "yyyy-MM-dd")
    $versionJson | ConvertTo-Json -Depth 10 | Set-Content "Installer\Output\version.json"
    Write-Success "version.json updated"
    
    # Update index.html version badge
    Write-Info "Updating Website\index.html..."
    $index = Get-Content "Website\index.html" -Raw
    $index = $index -replace 'Version [\d\.]+ - Latest Release', "Version $version - Latest Release"
    $index = $index -replace 'Version [\d\.]+ \| Windows', "Version $version | Windows"
    $index | Set-Content "Website\index.html" -NoNewline
    Write-Success "index.html updated"
    
    Write-Success "All version numbers updated to $version"
}

# ============================================
# STEP 3: BUILD APPLICATION
# ============================================

function Build-Application {
    Write-StepHeader "STEP 3: Building Application"
    
    # Clean previous builds
    Write-Info "Cleaning previous builds..."
    dotnet clean DJBookingSystem.csproj --configuration Release | Out-Null
    Write-Success "Clean complete"
    
    # Build and publish
    Write-Info "Building and publishing self-contained application..."
    Write-Info "This may take 1-2 minutes..."
    
    $publishArgs = @(
        "publish"
        "DJBookingSystem.csproj"
        "--configuration", "Release"
        "--runtime", "win-x64"
        "--self-contained", "true"
        "-p:PublishSingleFile=true"
        "-p:IncludeNativeLibrariesForSelfExtract=true"
        "-p:PublishTrimmed=false"
        "--verbosity", "quiet"
    )
    
    $result = & dotnet @publishArgs 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Build failed!"
        Write-Host $result
        return $false
    }
    
    Write-Success "Build complete"
    return $true
}

# ============================================
# STEP 4: CREATE INSTALLER
# ============================================

function Create-Installer($version) {
    Write-StepHeader "STEP 4: Creating Installer"
    
    $sourcePath = "bin\Release\net8.0-windows\win-x64\publish\DJBookingSystem.exe"
    $staticPath = "Installer\Output\DJBookingSystem-Setup.exe"
    $versionedPath = "Installer\Output\DJBookingSystem-Setup-v$version.exe"
    
    # Check source exists
    if (-not (Test-Path $sourcePath)) {
        Write-Failure "Published executable not found at: $sourcePath"
        return $false
    }
    
    # Create Output directory if it doesn't exist
    $outputDir = "Installer\Output"
    if (-not (Test-Path $outputDir)) {
        New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
    }
    
    # Copy to static filename (for auto-updates)
    Write-Info "Creating static installer..."
    Copy-Item $sourcePath $staticPath -Force
    $fileSize = (Get-Item $staticPath).Length / 1MB
    Write-Success "Static installer created: $([math]::Round($fileSize, 2)) MB"
    
    # Copy to version-specific filename (backup)
    Write-Info "Creating version-specific backup..."
    Copy-Item $sourcePath $versionedPath -Force
    Write-Success "Backup created: DJBookingSystem-Setup-v$version.exe"
    
    return $true
}

# ============================================
# STEP 5: VERIFY BUILD
# ============================================

function Test-Build {
    Write-StepHeader "STEP 5: Verifying Build"
    
    $checks = @()
    
    # Check installer exists
    $installerPath = "Installer\Output\DJBookingSystem-Setup.exe"
    if (Test-Path $installerPath) {
        $size = (Get-Item $installerPath).Length / 1MB
        Write-Success "Installer exists: $([math]::Round($size, 2)) MB"
        $checks += $true
    } else {
        Write-Failure "Installer not found!"
        $checks += $false
    }
    
    # Check version.json exists
    if (Test-Path "Installer\Output\version.json") {
        Write-Success "version.json exists"
        $checks += $true
    } else {
        Write-Failure "version.json not found!"
        $checks += $false
    }
    
    # Check index.html exists
    if (Test-Path "Website\index.html") {
        Write-Success "index.html exists"
        $checks += $true
    } else {
        Write-Failure "index.html not found!"
        $checks += $false
    }
    
    # List all files
    Write-Info "Files in Installer\Output:"
    Get-ChildItem "Installer\Output" -Filter "*.exe" | Format-Table Name, @{Label="Size (MB)"; Expression={[math]::Round($_.Length / 1MB, 2)}}, LastWriteTime
    
    return ($checks -notcontains $false)
}

# ============================================
# STEP 6: UPLOAD TO HOSTINGER
# ============================================

function Upload-ToHostinger {
    Write-StepHeader "STEP 6: Uploading to Hostinger"
    
    if ($SkipUpload) {
        Write-Info "Upload skipped (SkipUpload flag set)"
        return $true
    }
    
    # Check if WinSCP is available (for automated FTP)
    $winscpPath = "C:\Program Files (x86)\WinSCP\WinSCPnet.dll"
    
    if (Test-Path $winscpPath) {
        Write-Info "Using WinSCP for automated upload..."
        # Add WinSCP upload logic here if DLL is available
        Write-Info "WinSCP automation not implemented. Use manual FTP upload."
    } else {
        Write-Info "WinSCP not found. Manual upload required."
    }
    
    Write-Info ""
    Write-Info "MANUAL UPLOAD INSTRUCTIONS:" -ForegroundColor Yellow
    Write-Info "================================" -ForegroundColor Yellow
    Write-Info "1. Open FileZilla or FTP client"
    Write-Info "2. Connect to: $($Config.FTPHost):$($Config.FTPPort)"
    Write-Info "3. Username: $($Config.FTPUsername)"
    Write-Info "4. Password: [From Hostinger panel]"
    Write-Info ""
    Write-Info "Upload these files:"
    foreach ($file in $Config.FilesToUpload) {
        Write-Host "   $($file.Local) ? $($file.Remote)" -ForegroundColor Cyan
    }
    Write-Info ""
    
    if (-not (Confirm-Action "Have you completed the upload?")) {
        Write-Failure "Upload not completed. Deployment incomplete."
        return $false
    }
    
    Write-Success "Upload confirmed"
    return $true
}

# ============================================
# STEP 7: POST-DEPLOYMENT VERIFICATION
# ============================================

function Test-Deployment($version) {
    Write-StepHeader "STEP 7: Post-Deployment Verification"
    
    Write-Info "Testing URLs..."
    
    # Test main website
    try {
        $response = Invoke-WebRequest -Uri "https://djbookupdates.com/" -TimeoutSec 10 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Success "Website accessible"
        }
    } catch {
        Write-Failure "Website not accessible"
    }
    
    # Test version.json
    try {
        $versionData = Invoke-RestMethod -Uri "https://djbookupdates.com/version.json" -TimeoutSec 10
        if ($versionData.latestVersion -eq $version) {
            Write-Success "version.json correct: $($versionData.latestVersion)"
        } else {
            Write-Failure "version.json shows: $($versionData.latestVersion), expected: $version"
        }
    } catch {
        Write-Failure "version.json not accessible"
    }
    
    # Test download link
    try {
        $downloadUrl = "https://djbookupdates.com/downloads/DJBookingSystem-Setup.exe"
        $response = Invoke-WebRequest -Uri $downloadUrl -Method Head -TimeoutSec 10 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            $size = [math]::Round($response.Headers.'Content-Length' / 1MB, 2)
            Write-Success "Installer downloadable: $size MB"
        }
    } catch {
        Write-Failure "Installer download link not working"
    }
}

# ============================================
# STEP 8: GENERATE DEPLOYMENT REPORT
# ============================================

function New-DeploymentReport($version, $startTime) {
    Write-StepHeader "STEP 8: Deployment Report"
    
    $endTime = Get-Date
    $duration = $endTime - $startTime
    
    $report = @"

?????????????????????????????????????????????????????????????
?         DEPLOYMENT REPORT - DJ BOOKING SYSTEM             ?
?????????????????????????????????????????????????????????????

Version:       $version
Date:          $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Duration:      $([math]::Round($duration.TotalMinutes, 2)) minutes

Files Updated:
  ? DJBookingSystem.csproj
  ? SplashScreen.xaml
  ? version.json
  ? index.html

Build Output:
  ? DJBookingSystem-Setup.exe (Static)
  ? DJBookingSystem-Setup-v$version.exe (Backup)

Upload Status:
  $(if ($SkipUpload) { "? Skipped" } else { "? Completed" })

Verification:
  ? Website:    https://djbookupdates.com/
  ? Version:    https://djbookupdates.com/version.json
  ? Download:   https://djbookupdates.com/downloads/DJBookingSystem-Setup.exe

Next Steps:
  1. Test auto-updater on client application
  2. Monitor user feedback
  3. Update CHANGELOG.md if not done
  4. Commit changes to version control

???????????????????????????????????????????????????????????
"@

    Write-Host $report -ForegroundColor Green
    
    # Save report to file
    $reportPath = "Deployment-Report-v$version-$(Get-Date -Format 'yyyyMMdd-HHmmss').txt"
    $report | Out-File $reportPath
    Write-Info "Report saved to: $reportPath"
}

# ============================================
# MAIN EXECUTION
# ============================================

function Start-Deployment {
    $startTime = Get-Date
    
    Write-Host @"
?????????????????????????????????????????????????????????????
?  AUTOMATED DEPLOYMENT SCRIPT - DJ BOOKING SYSTEM          ?
?  Version: $NewVersion                                         ?
?????????????????????????????????????????????????????????????
"@ -ForegroundColor Cyan
    
    # Step 1: Prerequisites
    if (-not (Test-Prerequisites)) {
        Write-Failure "Prerequisites check failed. Aborting."
        return
    }
    
    # Step 2: Update version numbers
    Update-VersionNumbers $NewVersion
    
    # Step 3: Build application
    if (-not (Build-Application)) {
        Write-Failure "Build failed. Aborting."
        return
    }
    
    # Step 4: Create installer
    if (-not (Create-Installer $NewVersion)) {
        Write-Failure "Installer creation failed. Aborting."
        return
    }
    
    # Step 5: Verify build
    if (-not (Test-Build)) {
        Write-Failure "Build verification failed. Aborting."
        return
    }
    
    # Step 6: Upload to Hostinger
    if (-not (Upload-ToHostinger)) {
        Write-Failure "Upload incomplete. Deployment not finished."
        return
    }
    
    # Step 7: Post-deployment verification
    if (-not $SkipUpload) {
        Test-Deployment $NewVersion
    }
    
    # Step 8: Generate report
    New-DeploymentReport $NewVersion $startTime
    
    Write-Host ""
    Write-Success "DEPLOYMENT COMPLETE!" -ForegroundColor Green
}

# ============================================
# RUN DEPLOYMENT
# ============================================

Start-Deployment
