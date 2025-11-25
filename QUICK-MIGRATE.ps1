# QUICK MIGRATION - DJ Booking System
# One-command execution

Write-Host ""
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?     DJ BOOKING SYSTEM - QUICK MIGRATION                   ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

$oldPath = "K:\Customer Data\LPM\New-Booking-claude-initial-setup-011CV2Bn45svzRU7VxANArie\Fallen-Collective-Booking"
$newPath = "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking"

Write-Host "FROM: $oldPath" -ForegroundColor Red
Write-Host "TO:   $newPath" -ForegroundColor Green
Write-Host ""

# Check if Visual Studio is running
$vsProcesses = Get-Process | Where-Object { $_.ProcessName -like "*devenv*" }
if ($vsProcesses) {
    Write-Host "??  WARNING: Visual Studio is running!" -ForegroundColor Red
    Write-Host "Please close Visual Studio before continuing." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Press any key to exit..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

Write-Host "? Visual Studio is not running" -ForegroundColor Green
Write-Host ""

# Confirm
Write-Host "This will move the entire project to a new location." -ForegroundColor Yellow
Write-Host ""
$response = Read-Host "Continue? (yes/no)"

if ($response -ne "yes") {
    Write-Host "Migration cancelled." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Starting migration..." -ForegroundColor Cyan

# Execute main migration script
$scriptPath = Join-Path $PSScriptRoot "Move-DJBookingToNewLocation.ps1"

if (Test-Path $scriptPath) {
    & $scriptPath
} else {
    Write-Host "ERROR: Migration script not found!" -ForegroundColor Red
    Write-Host "Looking for: $scriptPath" -ForegroundColor Yellow
    exit 1
}
