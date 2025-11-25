# Run-App-With-Error-Capture.ps1
# This script runs the app and captures any errors

$ErrorActionPreference = "Continue"

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "DJ BOOKING SYSTEM - ERROR CAPTURE" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

$exePath = "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\bin\Debug\net8.0-windows\DJBookingSystem.exe"

if (-not (Test-Path $exePath)) {
    Write-Host "ERROR: Executable not found at:" -ForegroundColor Red
    Write-Host "  $exePath" -ForegroundColor Yellow
    exit 1
}

Write-Host "Found executable: $exePath" -ForegroundColor Green
Write-Host ""
Write-Host "Starting app..." -ForegroundColor Yellow
Write-Host "If the app crashes, errors will appear below:" -ForegroundColor Yellow
Write-Host ""

try {
    # Start the process and wait for it to exit
    $process = Start-Process -FilePath $exePath -PassThru -Wait
    
    Write-Host ""
    Write-Host "Process exited with code: $($process.ExitCode)" -ForegroundColor $(if ($process.ExitCode -eq 0) { "Green" } else { "Red" })
    
    if ($process.ExitCode -ne 0) {
        Write-Host ""
        Write-Host "Non-zero exit code indicates an error occurred!" -ForegroundColor Red
    }
}
catch {
    Write-Host ""
    Write-Host "EXCEPTION CAUGHT:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Stack Trace:" -ForegroundColor Red
    Write-Host $_.Exception.StackTrace -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Checking Event Viewer for .NET Runtime errors..." -ForegroundColor Cyan

# Get recent .NET Runtime errors from Event Viewer
$events = Get-WinEvent -FilterHashtable @{
    LogName = 'Application'
    ProviderName = '.NET Runtime'
    Level = 2  # Error level
} -MaxEvents 5 -ErrorAction SilentlyContinue

if ($events) {
    Write-Host ""
    Write-Host "Recent .NET Runtime Errors:" -ForegroundColor Red
    Write-Host "===========================" -ForegroundColor Red
    
    foreach ($event in $events) {
        Write-Host ""
        Write-Host "Time: $($event.TimeCreated)" -ForegroundColor Yellow
        Write-Host "Message:" -ForegroundColor Yellow
        Write-Host $event.Message -ForegroundColor White
        Write-Host "---" -ForegroundColor Gray
    }
} else {
    Write-Host "No recent .NET Runtime errors found in Event Viewer" -ForegroundColor Green
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Cyan
