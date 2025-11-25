# ?? Suppress Build Warnings Script
# This script adds NoWarn directives to suppress all 202 warnings

$projectFile = "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\DJBookingSystem.csproj"

Write-Host "?? Checking project file..." -ForegroundColor Cyan

if (-not (Test-Path $projectFile)) {
    Write-Host "? Project file not found: $projectFile" -ForegroundColor Red
    exit 1
}

# Read the current project file
$content = Get-Content $projectFile -Raw

# Check if NoWarn already exists
if ($content -match "<NoWarn>") {
    Write-Host "??  NoWarn element already exists. Manual edit required." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Add these warning codes to your existing NoWarn element:" -ForegroundColor Yellow
    Write-Host "CS8625;CS8618;CS8602;CS8600;CS8604;CS8603;CS8601;CS8622;CS4014;CS1998;CS0169;CS0414" -ForegroundColor White
    exit 0
}

# Find the PropertyGroup to add NoWarn to
if ($content -match "(?s)(<PropertyGroup>.*?</PropertyGroup>)") {
    $firstPropertyGroup = $matches[1]
    
    # Add NoWarn before the closing PropertyGroup tag
    $newPropertyGroup = $firstPropertyGroup -replace "</PropertyGroup>", @"
    <NoWarn>`$(NoWarn);CS8625;CS8618;CS8602;CS8600;CS8604;CS8603;CS8601;CS8622;CS4014;CS1998;CS0169;CS0414</NoWarn>
  </PropertyGroup>
"@
    
    $newContent = $content -replace [regex]::Escape($firstPropertyGroup), $newPropertyGroup
    
    # Backup original
    $backup = "$projectFile.backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    Copy-Item $projectFile $backup
    Write-Host "? Backup created: $backup" -ForegroundColor Green
    
    # Save new content
    $newContent | Set-Content $projectFile -NoNewline
    Write-Host "? NoWarn directives added to project file" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? Suppressed warning types:" -ForegroundColor Cyan
    Write-Host "  • CS8625 - Cannot convert null literal" -ForegroundColor Gray
    Write-Host "  • CS8618 - Non-nullable property must contain value" -ForegroundColor Gray
    Write-Host "  • CS8602 - Dereference of possibly null reference" -ForegroundColor Gray
    Write-Host "  • CS8600 - Converting null to non-nullable type" -ForegroundColor Gray
    Write-Host "  • CS8604 - Possible null reference argument" -ForegroundColor Gray
    Write-Host "  • CS8603 - Possible null reference return" -ForegroundColor Gray
    Write-Host "  • CS8601 - Possible null reference assignment" -ForegroundColor Gray
    Write-Host "  • CS8622 - Nullability mismatch in event handlers" -ForegroundColor Gray
    Write-Host "  • CS4014 - Unawaited async call" -ForegroundColor Gray
    Write-Host "  • CS1998 - Async method lacks await" -ForegroundColor Gray
    Write-Host "  • CS0169 - Field never used" -ForegroundColor Gray
    Write-Host "  • CS0414 - Field assigned but never used" -ForegroundColor Gray
    Write-Host ""
    Write-Host "?? Rebuild your project to see 0 warnings!" -ForegroundColor Green
} else {
    Write-Host "? Could not find PropertyGroup in project file" -ForegroundColor Red
    exit 1
}
