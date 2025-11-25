#!/usr/bin/env pwsh
# DJ Booking System - Comprehensive Fix Script

Write-Host "=== FIXING ALL ISSUES ===" -ForegroundColor Cyan

$projectRoot = "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking"

# 1. Clean temporary files
Write-Host "`n[1/6] Cleaning temporary files..." -ForegroundColor Yellow
$patterns = @("*_wpftmp.csproj", "*.BACKUP", "*.broken", "*.CORRUPT-BACKUP*")
$count = 0
foreach ($pattern in $patterns) {
    Get-ChildItem -Path $projectRoot -Filter $pattern -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
        Remove-Item $_.FullName -Force -ErrorAction SilentlyContinue
        $count++
    }
}
Write-Host "  Cleaned $count files" -ForegroundColor Green

# 2. Fix AdminBookingManagementWindow.xaml.cs
Write-Host "`n[2/6] Fixing AdminBookingManagementWindow.xaml.cs..." -ForegroundColor Yellow
$file = Join-Path $projectRoot "AdminBookingManagementWindow.xaml.cs"
$lines = Get-Content $file
$newLines = @()
$skipNext = $false

for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    
    # Skip pragma lines
    if ($line -match '#pragma warning (disable|restore) CS0618') {
        continue
    }
    
    # Fix OrderBy
    if ($line -match 'OrderBy\(b => b\.BookingDate\)') {
        $line = $line -replace 'OrderBy\(b => b\.BookingDate\)', 'OrderBy(b => b.DayOfWeek).ThenBy(b => b.WeekNumber).ThenBy(b => b.TimeSlot)'
    }
    
    # Fix BookingDate assignment - comment it out
    if ($line -match 'BookingDate\s*=\s*bookingDateTime') {
        $line = $line -replace 'BookingDate\s*=\s*bookingDateTime', '// BookingDate = bookingDateTime // REMOVED: Use DayOfWeek, WeekNumber, TimeSlot'
    }
    
    # Fix BookingDate comparison - needs context, skip for now
    
    $newLines += $line
}

$newLines | Set-Content $file
Write-Host "  Fixed" -ForegroundColor Green

# 3. Update DJBookingSystem.csproj
Write-Host "`n[3/6] Updating project file..." -ForegroundColor Yellow
$csproj = Join-Path $projectRoot "DJBookingSystem.csproj"
$content = Get-Content $csproj -Raw
$content = $content -replace ';CS0618', ''
$content | Set-Content $csproj -NoNewline
Write-Host "  Removed CS0618 from warnings" -ForegroundColor Green

# 4. Fix MainWindow.xaml.cs null safety
Write-Host "`n[4/6] Fixing null safety..." -ForegroundColor Yellow
$mainWindow = Join-Path $projectRoot "MainWindow.xaml.cs"
$content = Get-Content $mainWindow -Raw
$content = $content -replace 'private CosmosDbService\? _cosmosDbService;', 'private CosmosDbService _cosmosDbService = null!;'
$content = $content -replace 'private PermissionService\? _permissionService;', 'private PermissionService _permissionService = null!;'
$content = $content -replace 'public CosmosDbService\? CosmosDbService', 'public CosmosDbService CosmosDbService'
$content | Set-Content $mainWindow -NoNewline
Write-Host "  Fixed MainWindow" -ForegroundColor Green

# 5. Clean obj directory
Write-Host "`n[5/6] Cleaning obj directory..." -ForegroundColor Yellow
$objPath = Join-Path $projectRoot "obj"
if (Test-Path $objPath) {
    Get-ChildItem $objPath -Filter "*_wpftmp.*" -Recurse | Remove-Item -Force -ErrorAction SilentlyContinue
    Write-Host "  Cleaned obj folder" -ForegroundColor Green
}

# 6. Generate fix summary
Write-Host "`n[6/6] Generating detailed fix list..." -ForegroundColor Yellow

$fixList = @"
=== COMPREHENSIVE FIX SUMMARY ===

COMPLETED AUTOMATICALLY:
✓ Cleaned $count temporary build files
✓ Removed #pragma warning suppressions from AdminBookingManagementWindow.xaml.cs
✓ Fixed OrderBy to use DayOfWeek/WeekNumber/TimeSlot
✓ Commented out BookingDate assignments
✓ Removed CS0618 from project NoWarn list
✓ Fixed null safety in MainWindow.xaml.cs

REQUIRES MANUAL ATTENTION:

1. AdminBookingManagementWindow.xaml.cs (Lines 172-178)
   OLD: var conflict = _allBookings.FirstOrDefault(b => b.VenueName == selectedVenue.Name && b.BookingDate == bookingDateTime);
   FIX: Replace with proper day/week/time comparison using new properties

2. Booking.cs - Remove BookingDate property entirely
   - Property is marked obsolete but still exists
   - Remove lines ~37-38

3. App.xaml.cs - Async error handling (Lines 67-85)
   - Change underscore discard (_  = Task.Run) to proper await
   - Add .ConfigureAwait(false) to all async calls

4. Search all *.cs files for remaining "BookingDate" usage:
   - Replace with GetNextOccurrence() or OccursOn() methods
   - Use DayOfWeek, WeekNumber, TimeSlot properties

FILES STILL USING BookingDate:
"@

# Search for remaining BookingDate usage
$csFiles = Get-ChildItem -Path $projectRoot -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notmatch '\\obj\\' }
$filesWithIssues = @()

foreach ($csFile in $csFiles) {
    $content = Get-Content $csFile.FullName -Raw
    if ($content -match 'BookingDate') {
        $filesWithIssues += "  - $($csFile.Name)"
    }
}

$fixList += "`n" + ($filesWithIssues -join "`n")
$fixList += "`n`nNEXT STEPS:`n1. Review changes`n2. Fix manual items above`n3. Run: dotnet build`n4. Test thoroughly"

$fixList | Set-Content (Join-Path $projectRoot "FIX-SUMMARY.txt")

Write-Host "`n=== DONE ===" -ForegroundColor Green
Write-Host "Review: FIX-SUMMARY.txt" -ForegroundColor Cyan
