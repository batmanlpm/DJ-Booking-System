# Build-Installer-With-Exclusions.ps1
# Creates installer that automatically adds Windows Defender exclusions

param(
    [string]$Version = "1.2.5"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building DJ Booking System Installer v$Version" -ForegroundColor Cyan
Write-Host "With Windows Defender Auto-Exclusion" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ProjectPath = "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking"
$OutputPath = "$ProjectPath\Installer\Output"
$PublishPath = "$ProjectPath\bin\Release\net8.0-windows\win-x64\publish"

# Step 1: Build application
Write-Host "Step 1: Building application..." -ForegroundColor Yellow
dotnet clean "$ProjectPath\DJBookingSystem.csproj" --configuration Release | Out-Null
dotnet publish "$ProjectPath\DJBookingSystem.csproj" `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishTrimmed=false `
    --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "? Build complete" -ForegroundColor Green
Write-Host ""

# Step 2: Create installer with auto-exclusion
Write-Host "Step 2: Creating installer with auto-exclusion..." -ForegroundColor Yellow

# Create installer directory
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
}

# Copy main executable
$sourceExe = "$PublishPath\DJBookingSystem.exe"
$destExe = "$OutputPath\DJBookingSystem-Setup.exe"
Copy-Item $sourceExe $destExe -Force

# Create installation script that includes Defender exclusion
$installerScript = @'
@echo off
echo ========================================
echo DJ Booking System - Installation
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

echo.
echo ========================================
echo Installation Complete!
echo ========================================
echo.
echo DJ Booking System has been installed to:
echo   %ProgramFiles%\DJ Booking System
echo.
echo Desktop shortcut created.
echo Windows Defender exclusions added.
echo.
pause
'@

$installerScript | Out-File "$OutputPath\Install.bat" -Encoding ASCII

Write-Host "? Installer created" -ForegroundColor Green
Write-Host ""

# Step 3: Package everything
Write-Host "Step 3: Creating installation package..." -ForegroundColor Yellow

# Create self-extracting archive (requires 7-Zip)
$sevenZipPath = "C:\Program Files\7-Zip\7z.exe"
if (Test-Path $sevenZipPath) {
    $packageName = "DJBookingSystem-Installer-v$Version.exe"
    
    & $sevenZipPath a -sfx7z.sfx "$OutputPath\$packageName" `
        "$OutputPath\DJBookingSystem-Setup.exe" `
        "$OutputPath\Install.bat" `
        "$ProjectPath\Installer\Add-WindowsDefenderExclusion.ps1"
    
    Write-Host "? Self-extracting installer created: $packageName" -ForegroundColor Green
} else {
    Write-Host "? 7-Zip not found - creating ZIP package instead" -ForegroundColor Yellow
    
    Compress-Archive -Path `
        "$OutputPath\DJBookingSystem-Setup.exe", `
        "$OutputPath\Install.bat", `
        "$ProjectPath\Installer\Add-WindowsDefenderExclusion.ps1" `
        -DestinationPath "$OutputPath\DJBookingSystem-Installer-v$Version.zip" -Force
    
    Write-Host "? ZIP package created: DJBookingSystem-Installer-v$Version.zip" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "? Installer Build Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Files created:" -ForegroundColor Cyan
Write-Host "  • DJBookingSystem-Setup.exe (main executable)" -ForegroundColor White
Write-Host "  • Install.bat (installation script)" -ForegroundColor White
Write-Host "  • Add-WindowsDefenderExclusion.ps1 (auto-exclusion)" -ForegroundColor White
Write-Host ""
Write-Host "User Instructions:" -ForegroundColor Yellow
Write-Host "  1. Right-click Install.bat" -ForegroundColor White
Write-Host "  2. Select 'Run as administrator'" -ForegroundColor White
Write-Host "  3. Click 'Yes' on UAC prompt" -ForegroundColor White
Write-Host "  4. Installation completes with auto-exclusions" -ForegroundColor White
Write-Host ""
