# ?? SELF-SIGNED CODE SIGNING - COMPLETE SETUP SYSTEM
# This script creates everything needed for trusted app distribution

param(
    [string]$CompanyName = "The Fallen Collective",
    [string]$AppName = "DJ Booking System",
    [int]$ValidYears = 5
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SELF-SIGNED CODE SIGNING - COMPLETE SETUP" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Company: $CompanyName" -ForegroundColor White
Write-Host "App: $AppName" -ForegroundColor White
Write-Host "Valid: $ValidYears years" -ForegroundColor White
Write-Host ""

# ===== STEP 1: CREATE CERTIFICATE =====
Write-Host "Step 1: Creating Self-Signed Certificate..." -ForegroundColor Yellow

$certName = "$CompanyName - $AppName"
$cert = New-SelfSignedCertificate `
    -Type CodeSigningCert `
    -Subject "CN=$certName" `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -NotAfter (Get-Date).AddYears($ValidYears) `
    -KeyUsage DigitalSignature `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3")

Write-Host "? Certificate created!" -ForegroundColor Green
Write-Host "  Subject: $certName" -ForegroundColor Gray
Write-Host "  Thumbprint: $($cert.Thumbprint)" -ForegroundColor Gray
Write-Host "  Valid Until: $($cert.NotAfter.ToString('yyyy-MM-dd'))" -ForegroundColor Gray
Write-Host ""

# ===== STEP 2: EXPORT CERTIFICATE =====
Write-Host "Step 2: Exporting Certificate..." -ForegroundColor Yellow

$certOutputPath = "FallenCollective-CodeSigning.cer"
Export-Certificate -Cert $cert -FilePath $certOutputPath | Out-Null

Write-Host "? Certificate exported!" -ForegroundColor Green
Write-Host "  File: $certOutputPath" -ForegroundColor Gray
Write-Host "  Users will install this file (one-time)" -ForegroundColor Gray
Write-Host ""

# ===== STEP 3: CREATE SIGNING SCRIPT =====
Write-Host "Step 3: Creating App Signing Script..." -ForegroundColor Yellow

$signScript = @"
# Sign-DJBookingSystem.ps1
# Automatically signs the DJ Booking System with self-signed certificate

param(
    [string]`$AppPath = "Installer\Output\DJBookingSystem-Setup.exe"
)

`$ErrorActionPreference = "Stop"

Write-Host "Signing DJ Booking System..." -ForegroundColor Cyan

# Find certificate
`$cert = Get-ChildItem -Path Cert:\CurrentUser\My -CodeSigningCert | 
    Where-Object { `$_.Subject -like "*$certName*" } | 
    Select-Object -First 1

if (-not `$cert) {
    Write-Host "? Certificate not found!" -ForegroundColor Red
    Write-Host "  Run Setup-CodeSigning.ps1 first" -ForegroundColor Yellow
    exit 1
}

if (-not (Test-Path `$AppPath)) {
    Write-Host "? App not found: `$AppPath" -ForegroundColor Red
    exit 1
}

# Sign the app
Write-Host "Signing: `$AppPath" -ForegroundColor White
Set-AuthenticodeSignature -FilePath `$AppPath -Certificate `$cert -TimestampServer "http://timestamp.digicert.com" | Out-Null

# Verify signature
`$signature = Get-AuthenticodeSignature -FilePath `$AppPath
if (`$signature.Status -eq 'Valid') {
    Write-Host "? App signed successfully!" -ForegroundColor Green
    Write-Host "  Signer: `$(`$signature.SignerCertificate.Subject)" -ForegroundColor Gray
    Write-Host "  Timestamp: `$(`$signature.TimeStamperCertificate.Subject)" -ForegroundColor Gray
} else {
    Write-Host "? Signing failed: `$(`$signature.Status)" -ForegroundColor Red
    exit 1
}
"@

$signScript | Out-File "Sign-DJBookingSystem.ps1" -Encoding UTF8

Write-Host "? Signing script created: Sign-DJBookingSystem.ps1" -ForegroundColor Green
Write-Host ""

# ===== STEP 4: CREATE USER INSTALLATION GUIDE =====
Write-Host "Step 4: Creating User Installation Guide..." -ForegroundColor Yellow

$userGuide = @"
# ?? DJ BOOKING SYSTEM - FIRST TIME INSTALLATION

**One-time setup (takes 30 seconds)**

## Step 1: Install Security Certificate

This allows Windows to trust all DJ Booking System updates automatically.

**Instructions:**

1. Download **$certOutputPath**
2. **Right-click** the file ? **Install Certificate**
3. Choose **"Local Machine"** ? Click **Next**
   - If asked, click **Yes** to allow changes
4. Select **"Place all certificates in the following store"**
5. Click **Browse**
6. Select **"Trusted Root Certification Authorities"**
7. Click **OK** ? **Next** ? **Finish**
8. Click **Yes** when asked "Do you want to install this certificate?"

? **Done!** You'll never see security warnings for DJ Booking System again.

## Step 2: Install DJ Booking System

1. Download **DJBookingSystem-Setup.exe**
2. Double-click to install
3. **No security warnings!** ?

---

## ? Why Do I Need This?

Windows doesn't recognize indie apps by default. Installing our certificate tells Windows:
**"I trust The Fallen Collective"**

This is safe! It only trusts apps signed by us, nothing else.

---

## ?? Troubleshooting

**"Windows protected your PC" still appears:**
- Make sure you installed certificate to **Local Machine**, not Current User
- Make sure you chose **Trusted Root Certification Authorities** store

**Need Help?**
Contact support in Discord

---

**Created:** $(Get-Date -Format 'yyyy-MM-dd')
**Certificate Valid Until:** $($cert.NotAfter.ToString('yyyy-MM-dd'))
"@

$userGuide | Out-File "USER-INSTALLATION-GUIDE.md" -Encoding UTF8

Write-Host "? User guide created: USER-INSTALLATION-GUIDE.md" -ForegroundColor Green
Write-Host ""

# ===== STEP 5: CREATE AUTO-INSTALLER =====
Write-Host "Step 5: Creating Automated Installer..." -ForegroundColor Yellow

$installerScript = @'
@echo off
echo ========================================
echo DJ BOOKING SYSTEM - INSTALLATION
echo ========================================
echo.

REM Check for admin rights
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This installer requires administrator privileges.
    echo.
    echo Please right-click and select "Run as administrator"
    pause
    exit /b 1
)

echo Installing to C:\Program Files\DJ Booking System...

REM Create installation directory
if not exist "%ProgramFiles%\DJ Booking System" (
    mkdir "%ProgramFiles%\DJ Booking System"
)

REM Copy application files
echo Copying files...
xcopy /Y /Q "%~dp0DJBookingSystem-Setup.exe" "%ProgramFiles%\DJ Booking System\DJBookingSystem.exe"

REM Add Windows Defender exclusions
echo.
echo Adding Windows Defender exclusions...
powershell.exe -ExecutionPolicy Bypass -File "%~dp0Add-WindowsDefenderExclusion.ps1"

REM Create desktop shortcut
echo Creating desktop shortcut...
powershell.exe -Command "$WshShell = New-Object -ComObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%USERPROFILE%\Desktop\DJ Booking System.lnk'); $Shortcut.TargetPath = '%ProgramFiles%\DJ Booking System\DJBookingSystem.exe'; $Shortcut.Save()"

REM Create start menu shortcut
echo Creating start menu shortcut...
if not exist "%ProgramData%\Microsoft\Windows\Start Menu\Programs\DJ Booking System" (
    mkdir "%ProgramData%\Microsoft\Windows\Start Menu\Programs\DJ Booking System"
)
powershell.exe -Command "$WshShell = New-Object -ComObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%ProgramData%\Microsoft\Windows\Start Menu\Programs\DJ Booking System\DJ Booking System.lnk'); $Shortcut.TargetPath = '%ProgramFiles%\DJ Booking System\DJBookingSystem.exe'; $Shortcut.Save()"

echo.
echo ========================================
echo Installation Complete!
echo ========================================
echo.
echo DJ Booking System has been installed to:
echo   %ProgramFiles%\DJ Booking System
echo.
echo Shortcuts created:
echo   • Desktop
echo   • Start Menu
echo.
echo Windows Defender exclusions added.
echo.
echo You can now launch DJ Booking System from:
echo   - Desktop shortcut
echo   - Start Menu
echo   - Search "DJ Booking System"
echo.
pause
'@

$installerScript | Out-File "Install.bat" -Encoding ASCII

Write-Host "? Installer script created: Install.bat" -ForegroundColor Green
Write-Host ""

# ===== STEP 6: CREATE COMPLETE BUILD SCRIPT =====
Write-Host "Step 6: Creating Complete Build & Sign Script..." -ForegroundColor Yellow

$buildScript = @"
# Build-And-Sign-Installer.ps1
# Complete automated build, sign, and package script

param(
    [string]`$Version = "1.2.5"
)

`$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "DJ BOOKING SYSTEM - BUILD & SIGN v`$Version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

`$ProjectPath = "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking"
`$OutputPath = "`$ProjectPath\Installer\Output"
`$PublishPath = "`$ProjectPath\bin\Release\net8.0-windows\win-x64\publish"

# Step 1: Build application
Write-Host "Step 1: Building application..." -ForegroundColor Yellow
dotnet clean "`$ProjectPath\DJBookingSystem.csproj" --configuration Release | Out-Null
dotnet publish "`$ProjectPath\DJBookingSystem.csproj" ``
    --configuration Release ``
    --runtime win-x64 ``
    --self-contained true ``
    -p:PublishSingleFile=true ``
    -p:IncludeNativeLibrariesForSelfExtract=true ``
    -p:PublishTrimmed=false ``
    --verbosity quiet

if (`$LASTEXITCODE -ne 0) {
    Write-Host "? Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "? Build complete" -ForegroundColor Green
Write-Host ""

# Step 2: Create installer
Write-Host "Step 2: Creating installer files..." -ForegroundColor Yellow

if (-not (Test-Path `$OutputPath)) {
    New-Item -ItemType Directory -Path `$OutputPath -Force | Out-Null
}

Copy-Item "`$PublishPath\DJBookingSystem.exe" "`$OutputPath\DJBookingSystem-Setup.exe" -Force
Copy-Item "`$OutputPath\DJBookingSystem-Setup.exe" "`$OutputPath\DJBookingSystem-Setup-v`$Version.exe" -Force

Write-Host "? Installer files created" -ForegroundColor Green
Write-Host ""

# Step 3: Sign the installer
Write-Host "Step 3: Signing installer..." -ForegroundColor Yellow

& "`$ProjectPath\Sign-DJBookingSystem.ps1" -AppPath "`$OutputPath\DJBookingSystem-Setup.exe"

if (`$LASTEXITCODE -ne 0) {
    Write-Host "? Signing failed!" -ForegroundColor Red
    exit 1
}

# Also sign versioned copy
& "`$ProjectPath\Sign-DJBookingSystem.ps1" -AppPath "`$OutputPath\DJBookingSystem-Setup-v`$Version.exe"

Write-Host ""

# Step 4: Create distribution package
Write-Host "Step 4: Creating distribution package..." -ForegroundColor Yellow

`$packageFolder = "`$OutputPath\DJBookingSystem-v`$Version-Package"
if (Test-Path `$packageFolder) {
    Remove-Item `$packageFolder -Recurse -Force
}
New-Item -ItemType Directory -Path `$packageFolder -Force | Out-Null

# Copy files to package
Copy-Item "`$OutputPath\DJBookingSystem-Setup.exe" `$packageFolder
Copy-Item "`$ProjectPath\FallenCollective-CodeSigning.cer" `$packageFolder
Copy-Item "`$ProjectPath\Install.bat" `$packageFolder
Copy-Item "`$ProjectPath\Installer\Add-WindowsDefenderExclusion.ps1" `$packageFolder
Copy-Item "`$ProjectPath\USER-INSTALLATION-GUIDE.md" "`$packageFolder\README.md"

Write-Host "? Package created: `$packageFolder" -ForegroundColor Green
Write-Host ""

# Step 5: Create ZIP archive
Write-Host "Step 5: Creating ZIP archive..." -ForegroundColor Yellow

`$zipPath = "`$OutputPath\DJBookingSystem-v`$Version-Installer.zip"
if (Test-Path `$zipPath) {
    Remove-Item `$zipPath -Force
}

Compress-Archive -Path "`$packageFolder\*" -DestinationPath `$zipPath -Force

Write-Host "? ZIP created: DJBookingSystem-v`$Version-Installer.zip" -ForegroundColor Green
Write-Host ""

# Step 6: Summary
Write-Host "========================================" -ForegroundColor Green
Write-Host "? BUILD & SIGN COMPLETE!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Files created:" -ForegroundColor Cyan
Write-Host "  • DJBookingSystem-Setup.exe (signed)" -ForegroundColor White
Write-Host "  • DJBookingSystem-Setup-v`$Version.exe (signed backup)" -ForegroundColor White
Write-Host "  • DJBookingSystem-v`$Version-Installer.zip (distribution package)" -ForegroundColor White
Write-Host ""
Write-Host "Distribution package contains:" -ForegroundColor Cyan
Write-Host "  • DJBookingSystem-Setup.exe (signed installer)" -ForegroundColor White
Write-Host "  • FallenCollective-CodeSigning.cer (certificate for users)" -ForegroundColor White
Write-Host "  • Install.bat (auto-installer)" -ForegroundColor White
Write-Host "  • Add-WindowsDefenderExclusion.ps1 (auto-exclusion)" -ForegroundColor White
Write-Host "  • README.md (user instructions)" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Upload ZIP to djbookupdates.com" -ForegroundColor White
Write-Host "  2. Users extract ZIP" -ForegroundColor White
Write-Host "  3. Users install certificate (one-time)" -ForegroundColor White
Write-Host "  4. Users run Install.bat as admin" -ForegroundColor White
Write-Host "  5. No more security warnings!" -ForegroundColor White
Write-Host ""
"@

$buildScript | Out-File "Build-And-Sign-Installer.ps1" -Encoding UTF8

Write-Host "? Build script created: Build-And-Sign-Installer.ps1" -ForegroundColor Green
Write-Host ""

# ===== STEP 7: CREATE QUICK START GUIDE =====
Write-Host "Step 7: Creating Quick Start Guide..." -ForegroundColor Yellow

$quickStart = @"
# ?? SELF-SIGNED CODE SIGNING - QUICK START

## For You (Developer)

### ? ONE-TIME SETUP (Already Done!)

This script already completed:
1. ? Created self-signed certificate
2. ? Exported certificate for users
3. ? Created signing script
4. ? Created installer scripts
5. ? Created user guide

**Certificate Location:**
- Installed: Cert:\CurrentUser\My
- Exported: $certOutputPath
- Valid Until: $($cert.NotAfter.ToString('yyyy-MM-dd'))

---

### ?? EVERY TIME YOU BUILD

Run this command:

``````powershell
.\Build-And-Sign-Installer.ps1 -Version "1.2.5"
``````

**What it does:**
1. Builds app (dotnet publish)
2. Creates installer (DJBookingSystem-Setup.exe)
3. **Signs installer** with your certificate
4. Creates distribution package (ZIP)
5. Includes certificate + user guide

**Output:**
- ``DJBookingSystem-v1.2.5-Installer.zip`` ? Upload this!

---

## For Users (Your Community)

### ? ONE-TIME SETUP (Per User)

**Step 1: Install Certificate**
``````
1. Extract DJBookingSystem-v1.2.5-Installer.zip
2. Right-click FallenCollective-CodeSigning.cer ? Install Certificate
3. Choose "Local Machine"
4. Place in "Trusted Root Certification Authorities"
5. Done!
``````

**Step 2: Install App**
``````
1. Right-click Install.bat ? Run as administrator
2. Click Yes on UAC
3. Installation completes automatically
4. Desktop + Start Menu shortcuts created
5. Windows Defender exclusions added
``````

**Total Time:** < 1 minute

---

## ?? FILES EXPLAINED

### For Distribution (Upload to Website)
- **DJBookingSystem-v1.2.5-Installer.zip** ? Main download

### Inside ZIP
- **DJBookingSystem-Setup.exe** - Signed installer
- **FallenCollective-CodeSigning.cer** - Certificate (users install once)
- **Install.bat** - Auto-installer script
- **Add-WindowsDefenderExclusion.ps1** - Auto-exclusion script
- **README.md** - User instructions

### For You (Keep Local)
- **Sign-DJBookingSystem.ps1** - Sign any file
- **Build-And-Sign-Installer.ps1** - Complete build process
- **Setup-CodeSigning.ps1** - This setup script (run once)

---

## ?? WORKFLOW

### Every Release:

1. **Build & Sign**
   ``````powershell
   .\Build-And-Sign-Installer.ps1 -Version "1.2.5"
   ``````

2. **Upload ZIP**
   ``````
   Upload: DJBookingSystem-v1.2.5-Installer.zip
   To: https://djbookupdates.com/downloads/
   ``````

3. **Users Download & Install**
   ``````
   1. Download ZIP
   2. Extract
   3. Install certificate (first time only)
   4. Run Install.bat as admin
   5. Done!
   ``````

---

## ? BENEFITS

**For You:**
- ? FREE (no annual fees)
- ? Automated signing
- ? Professional appearance
- ? One-command build

**For Users:**
- ? No security warnings (after certificate install)
- ? One-click installation
- ? Automatic Windows Defender exclusions
- ? Desktop + Start Menu shortcuts

**For Community:**
- ? Perfect for closed community
- ? Certificate installs once, works forever
- ? Simple distribution

---

## ?? TROUBLESHOOTING

### "Certificate not found" when signing
``````powershell
# Re-run setup
.\Setup-CodeSigning.ps1
``````

### Users still see warnings
``````
- Make sure they installed certificate to "Local Machine"
- Make sure they chose "Trusted Root Certification Authorities"
- Certificate must be installed BEFORE running installer
``````

### Need to renew certificate
``````powershell
# Re-run setup with new expiry
.\Setup-CodeSigning.ps1 -ValidYears 5
``````

---

## ?? SUPPORT

**For Users:**
- See: USER-INSTALLATION-GUIDE.md
- Contact: Discord support channel

**For You:**
- Certificate Location: Cert:\CurrentUser\My
- Thumbprint: $($cert.Thumbprint)
- Valid Until: $($cert.NotAfter.ToString('yyyy-MM-dd'))

---

**Created:** $(Get-Date -Format 'yyyy-MM-dd')
**System Version:** 1.0
**Certificate Valid Until:** $($cert.NotAfter.ToString('yyyy-MM-dd'))
"@

$quickStart | Out-File "QUICK-START-CODE-SIGNING.md" -Encoding UTF8

Write-Host "? Quick start guide created: QUICK-START-CODE-SIGNING.md" -ForegroundColor Green
Write-Host ""

# ===== COMPLETION SUMMARY =====
Write-Host "========================================" -ForegroundColor Green
Write-Host "? SETUP COMPLETE!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

Write-Host "Files created:" -ForegroundColor Cyan
Write-Host "  1. $certOutputPath (certificate for users)" -ForegroundColor White
Write-Host "  2. Sign-DJBookingSystem.ps1 (sign any file)" -ForegroundColor White
Write-Host "  3. Build-And-Sign-Installer.ps1 (complete build process)" -ForegroundColor White
Write-Host "  4. Install.bat (auto-installer)" -ForegroundColor White
Write-Host "  5. USER-INSTALLATION-GUIDE.md (user instructions)" -ForegroundColor White
Write-Host "  6. QUICK-START-CODE-SIGNING.md (this guide)" -ForegroundColor White
Write-Host ""

Write-Host "Certificate Info:" -ForegroundColor Cyan
Write-Host "  Subject: $certName" -ForegroundColor White
Write-Host "  Thumbprint: $($cert.Thumbprint)" -ForegroundColor White
Write-Host "  Valid Until: $($cert.NotAfter.ToString('yyyy-MM-dd'))" -ForegroundColor White
Write-Host ""

Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Read QUICK-START-CODE-SIGNING.md" -ForegroundColor White
Write-Host "  2. Run: .\Build-And-Sign-Installer.ps1 -Version `"1.2.5`"" -ForegroundColor White
Write-Host "  3. Upload generated ZIP to website" -ForegroundColor White
Write-Host "  4. Share USER-INSTALLATION-GUIDE.md with community" -ForegroundColor White
Write-Host ""

Write-Host "? Everything is ready!" -ForegroundColor Green
Write-Host ""
