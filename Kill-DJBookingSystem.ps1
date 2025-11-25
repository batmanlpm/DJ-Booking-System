# Kill DJ Booking System Process
# Quick emergency stop script for keyboard hotkey

Write-Host "Killing DJ Booking System..." -ForegroundColor Red

# Kill by process name
$processName = "DJBookingSystem"
$processes = Get-Process -Name $processName -ErrorAction SilentlyContinue

if ($processes) {
    foreach ($process in $processes) {
        Write-Host "Stopping process: $($process.ProcessName) (PID: $($process.Id))" -ForegroundColor Yellow
        Stop-Process -Id $process.Id -Force
    }
    Write-Host "DJ Booking System terminated successfully!" -ForegroundColor Green
} else {
    Write-Host "DJ Booking System is not running." -ForegroundColor Cyan
}

# Optional: Also kill Visual Studio debugger if attached
$vsDebugger = Get-Process -Name "vshost*" -ErrorAction SilentlyContinue
if ($vsDebugger) {
    Write-Host "Stopping debugger processes..." -ForegroundColor Yellow
    $vsDebugger | Stop-Process -Force
}

# Exit immediately (important for hotkey usage)
Start-Sleep -Milliseconds 500
exit
