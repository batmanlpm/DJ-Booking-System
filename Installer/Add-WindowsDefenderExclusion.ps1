# Add-WindowsDefenderExclusion.ps1
# Automatically adds DJ Booking System to Windows Defender exclusions
# Must run as Administrator

#Requires -RunAsAdministrator

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "DJ Booking System - Windows Defender Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Installation path
$installPath = "${env:ProgramFiles}\DJ Booking System"
$exePath = Join-Path $installPath "DJBookingSystem.exe"

Write-Host "Adding Windows Defender exclusions..." -ForegroundColor Yellow

try {
    # Check if Windows Defender is available
    $defenderStatus = Get-MpComputerStatus -ErrorAction SilentlyContinue
    
    if ($null -eq $defenderStatus) {
        Write-Host "? Windows Defender not found - skipping exclusions" -ForegroundColor Yellow
        Write-Host "   (This is normal if you use third-party antivirus)" -ForegroundColor Gray
        exit 0
    }
    
    # Add folder exclusion (covers all files in installation folder)
    Write-Host "  Adding folder exclusion: $installPath" -ForegroundColor White
    Add-MpPreference -ExclusionPath $installPath -ErrorAction SilentlyContinue
    
    # Add executable exclusion (extra layer of protection)
    if (Test-Path $exePath) {
        Write-Host "  Adding executable exclusion: $exePath" -ForegroundColor White
        Add-MpPreference -ExclusionPath $exePath -ErrorAction SilentlyContinue
    }
    
    # Add process exclusion (protects running process)
    Write-Host "  Adding process exclusion: DJBookingSystem.exe" -ForegroundColor White
    Add-MpPreference -ExclusionProcess "DJBookingSystem.exe" -ErrorAction SilentlyContinue
    
    Write-Host ""
    Write-Host "? Windows Defender exclusions added successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Exclusions added:" -ForegroundColor Cyan
    Write-Host "  • Folder: $installPath" -ForegroundColor White
    Write-Host "  • Executable: DJBookingSystem.exe" -ForegroundColor White
    Write-Host "  • Process: DJBookingSystem.exe" -ForegroundColor White
    
    exit 0
}
catch {
    Write-Host ""
    Write-Host "? Could not add Windows Defender exclusions" -ForegroundColor Yellow
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "This is not critical - you can add exclusions manually later." -ForegroundColor Gray
    
    exit 0  # Don't fail installation
}
