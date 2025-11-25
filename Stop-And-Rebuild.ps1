# Stop Running DJ Booking System Process and Rebuild

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "STOP PROCESS AND REBUILD" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Find and stop the DJBookingSystem process
$processName = "DJBookingSystem"
$processes = Get-Process -Name $processName -ErrorAction SilentlyContinue

if ($processes) {
    Write-Host "Found running process(es):" -ForegroundColor Yellow
    foreach ($proc in $processes) {
        Write-Host "  - PID: $($proc.Id) | Name: $($proc.ProcessName)" -ForegroundColor Yellow
    }
    Write-Host ""
    
    Write-Host "Stopping process(es)..." -ForegroundColor Yellow
    try {
        Stop-Process -Name $processName -Force -ErrorAction Stop
        Write-Host "? Process stopped successfully" -ForegroundColor Green
        Start-Sleep -Seconds 1
    }
    catch {
        Write-Host "? Error stopping process: $_" -ForegroundColor Red
        Write-Host ""
        Write-Host "Try manually closing the application or restart your computer." -ForegroundColor Yellow
        exit 1
    }
}
else {
    Write-Host "? No running DJBookingSystem process found" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CLEANING BUILD ARTIFACTS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Clean bin and obj folders
Write-Host "Removing bin folders..." -ForegroundColor Yellow
Get-ChildItem -Path . -Recurse -Directory -Filter "bin" -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host "  Removing: $($_.FullName)" -ForegroundColor Gray
    Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "Removing obj folders..." -ForegroundColor Yellow
Get-ChildItem -Path . -Recurse -Directory -Filter "obj" -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host "  Removing: $($_.FullName)" -ForegroundColor Gray
    Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "? Build artifacts cleaned" -ForegroundColor Green
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "REBUILDING PROJECT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Clean
Write-Host "Running dotnet clean..." -ForegroundColor Yellow
dotnet clean
Write-Host ""

# Restore
Write-Host "Running dotnet restore..." -ForegroundColor Yellow
dotnet restore
Write-Host ""

# Build
Write-Host "Running dotnet build..." -ForegroundColor Yellow
dotnet build --no-restore

Write-Host ""
if ($LASTEXITCODE -eq 0) {
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "? BUILD SUCCESSFUL!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
}
else {
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "? BUILD FAILED!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
