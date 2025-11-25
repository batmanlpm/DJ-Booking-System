# Move DJ Booking System to New Location
# From: K:\Customer Data\LPM\New-Booking-claude-initial-setup-011CV2Bn45svzRU7VxANArie\Fallen-Collective-Booking
# To: K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "DJ BOOKING SYSTEM - DIRECTORY MIGRATION" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$oldPath = "K:\Customer Data\LPM\New-Booking-claude-initial-setup-011CV2Bn45svzRU7VxANArie\Fallen-Collective-Booking"
$newPath = "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking"
$parentPath = "K:\Customer Data\LPM\DJ_Booking"

Write-Host "Source Path: $oldPath" -ForegroundColor Yellow
Write-Host "Target Path: $newPath" -ForegroundColor Green
Write-Host ""

# Check if source exists
if (!(Test-Path $oldPath)) {
    Write-Host "ERROR: Source path does not exist!" -ForegroundColor Red
    exit 1
}

# Check if we're currently in the source directory
$currentPath = Get-Location
if ($currentPath.Path -eq $oldPath) {
    Write-Host "Currently in source directory. Changing to parent..." -ForegroundColor Yellow
    Set-Location "K:\Customer Data\LPM"
    Write-Host "Changed to: $(Get-Location)" -ForegroundColor Green
    Write-Host ""
}

# Create parent directory if it doesn't exist
if (!(Test-Path $parentPath)) {
    Write-Host "Creating parent directory: $parentPath" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $parentPath -Force | Out-Null
    Write-Host "? Parent directory created" -ForegroundColor Green
} else {
    Write-Host "? Parent directory already exists" -ForegroundColor Green
}

Write-Host ""

# Check if target already exists
if (Test-Path $newPath) {
    Write-Host "WARNING: Target path already exists!" -ForegroundColor Red
    $response = Read-Host "Do you want to delete it and continue? (yes/no)"
    if ($response -ne "yes") {
        Write-Host "Operation cancelled." -ForegroundColor Yellow
        exit 0
    }
    Write-Host "Removing existing target directory..." -ForegroundColor Yellow
    Remove-Item -Path $newPath -Recurse -Force
    Write-Host "? Existing directory removed" -ForegroundColor Green
    Write-Host ""
}

# Move the directory
Write-Host "Moving project files..." -ForegroundColor Yellow
try {
    Move-Item -Path $oldPath -Destination $newPath -Force
    Write-Host "? Files moved successfully!" -ForegroundColor Green
} catch {
    Write-Host "ERROR moving files: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "VERIFYING MIGRATION" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verify the move
if (Test-Path $newPath) {
    Write-Host "? New location verified: $newPath" -ForegroundColor Green
    
    # Count files
    $fileCount = (Get-ChildItem -Path $newPath -Recurse -File).Count
    Write-Host "? Total files: $fileCount" -ForegroundColor Green
    
    # Check for key files
    $keyFiles = @(
        "DJBookingSystem.csproj",
        "App.xaml",
        "MainWindow.xaml",
        "CandyBotDesktopWidget.xaml"
    )
    
    Write-Host ""
    Write-Host "Checking key files:" -ForegroundColor Yellow
    foreach ($file in $keyFiles) {
        $filePath = Join-Path $newPath $file
        if (Test-Path $filePath) {
            Write-Host "  ? $file" -ForegroundColor Green
        } else {
            Write-Host "  ? $file (NOT FOUND)" -ForegroundColor Red
        }
    }
} else {
    Write-Host "? ERROR: New location not found!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Checking if old location still exists..." -ForegroundColor Yellow
if (Test-Path $oldPath) {
    Write-Host "  ? WARNING: Old location still exists (move may have failed)" -ForegroundColor Red
} else {
    Write-Host "  ? Old location removed successfully" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "MIGRATION COMPLETE!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Close Visual Studio if it's open" -ForegroundColor White
Write-Host "2. Navigate to: $newPath" -ForegroundColor White
Write-Host "3. Open the solution file in Visual Studio" -ForegroundColor White
Write-Host "4. Clean and rebuild the solution" -ForegroundColor White
Write-Host ""
Write-Host "Commands to run:" -ForegroundColor Yellow
Write-Host "  cd '$newPath'" -ForegroundColor Cyan
Write-Host "  dotnet clean" -ForegroundColor Cyan
Write-Host "  dotnet build" -ForegroundColor Cyan
Write-Host ""

# Change to new directory
Write-Host "Changing to new directory..." -ForegroundColor Yellow
Set-Location $newPath
Write-Host "? Current directory: $(Get-Location)" -ForegroundColor Green
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
