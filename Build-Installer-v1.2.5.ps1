# Build and Package DJBookingSystem Version 1.2.5 Installer
# This script builds the self-contained .NET 8 application and copies it to the Installer\Output folder
# Creates BOTH a static filename (DJBookingSystem-Setup.exe) and a version-specific backup

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "DJ Booking System v1.2.5 Installer Build" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ProjectPath = "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking"
$OutputPath = "$ProjectPath\Installer\Output"
$PublishPath = "$ProjectPath\bin\Release\net8.0-windows\win-x64\publish"

# Step 1: Clean previous builds
Write-Host "Step 1: Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean "$ProjectPath\DJBookingSystem.csproj" --configuration Release
Write-Host "? Clean complete" -ForegroundColor Green
Write-Host ""

# Step 2: Build and Publish
Write-Host "Step 2: Building and publishing self-contained application..." -ForegroundColor Yellow
dotnet publish "$ProjectPath\DJBookingSystem.csproj" `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishTrimmed=false

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "? Build and publish complete" -ForegroundColor Green
Write-Host ""

# Step 3: Create installer directory if it doesn't exist
Write-Host "Step 3: Preparing installer output directory..." -ForegroundColor Yellow
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
}
Write-Host "? Output directory ready: $OutputPath" -ForegroundColor Green
Write-Host ""

# Step 4: Copy to STATIC filename (always the latest)
Write-Host "Step 4: Creating static installer (DJBookingSystem-Setup.exe)..." -ForegroundColor Yellow
$SourceExe = "$PublishPath\DJBookingSystem.exe"
$StaticExe = "$OutputPath\DJBookingSystem-Setup.exe"
$VersionedExe = "$OutputPath\DJBookingSystem-Setup-v1.2.5.exe"

if (Test-Path $SourceExe) {
    # Copy to STATIC filename (for automatic updates)
    Copy-Item -Path $SourceExe -Destination $StaticExe -Force
    $FileSize = (Get-Item $StaticExe).Length / 1MB
    Write-Host "? Static installer created!" -ForegroundColor Green
    Write-Host "  Destination: $StaticExe" -ForegroundColor Gray
    Write-Host "  Size: $([math]::Round($FileSize, 2)) MB" -ForegroundColor Gray
    
    # Also create version-specific backup
    Copy-Item -Path $SourceExe -Destination $VersionedExe -Force
    Write-Host "? Version-specific backup created!" -ForegroundColor Green
    Write-Host "  Backup: $VersionedExe" -ForegroundColor Gray
} else {
    Write-Host "? Source executable not found: $SourceExe" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 5: Verify files in Output folder
Write-Host "Step 5: Verifying Output folder contents..." -ForegroundColor Yellow
Write-Host ""
Write-Host "Files in Installer\Output:" -ForegroundColor Cyan
Get-ChildItem $OutputPath | Format-Table Name, @{Label="Size (MB)"; Expression={[math]::Round($_.Length / 1MB, 2)}}, LastWriteTime -AutoSize
Write-Host ""

# Step 6: Success summary
Write-Host "========================================" -ForegroundColor Green
Write-Host "? Version 1.2.5 Installer Ready!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Static Installer (for auto-updates):" -ForegroundColor Cyan
Write-Host "  $StaticExe" -ForegroundColor White
Write-Host ""
Write-Host "Version-Specific Backup:" -ForegroundColor Cyan
Write-Host "  $VersionedExe" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Test the installer locally" -ForegroundColor White
Write-Host "  2. Upload DJBookingSystem-Setup.exe to Hostinger" -ForegroundColor White
Write-Host "     (This overwrites the old version - always latest)" -ForegroundColor Gray
Write-Host "  3. Upload version.json to update version info" -ForegroundColor White
Write-Host "  4. Keep DJBookingSystem-Setup-v1.2.5.exe as backup" -ForegroundColor White
Write-Host ""
Write-Host "Download URL (static):" -ForegroundColor Yellow
Write-Host "  https://djbookupdates.com/downloads/DJBookingSystem-Setup.exe" -ForegroundColor White
Write-Host ""
