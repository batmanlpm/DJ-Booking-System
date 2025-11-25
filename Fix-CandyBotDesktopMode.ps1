# Quick Fix Script for Candy-Bot Desktop Mode Issues
# Run this in PowerShell from the project root directory

Write-Host "?? Candy-Bot Desktop Mode Fix Script" -ForegroundColor Magenta
Write-Host "=====================================" -ForegroundColor Magenta
Write-Host ""

# File paths
$avatarXaml = "Controls\CandyBotAvatar.xaml"
$desktopWidgetXaml = "CandyBotDesktopWidget.xaml"

# Check if files exist
if (-not (Test-Path $avatarXaml)) {
    Write-Host "? ERROR: $avatarXaml not found!" -ForegroundColor Red
    Write-Host "Make sure you're running this from the project root directory." -ForegroundColor Yellow
    exit
}

if (-not (Test-Path $desktopWidgetXaml)) {
    Write-Host "? ERROR: $desktopWidgetXaml not found!" -ForegroundColor Red
    exit
}

Write-Host "? Found both files" -ForegroundColor Green
Write-Host ""

# Backup files first
Write-Host "?? Creating backups..." -ForegroundColor Cyan
Copy-Item $avatarXaml "$avatarXaml.backup"
Copy-Item $desktopWidgetXaml "$desktopWidgetXaml.backup"
Write-Host "? Backups created" -ForegroundColor Green
Write-Host ""

# Fix 1: Desktop Widget Icons
Write-Host "?? Fixing Desktop Widget icons..." -ForegroundColor Cyan
$content = Get-Content $desktopWidgetXaml -Raw -Encoding UTF8

$content = $content -replace 'Text="[?][?] Candy-Bot Desktop Widget"', 'Text="?? Candy-Bot Desktop Widget"'
$content = $content -replace 'Content="[?][?] Search"', 'Content="?? Search"'
$content = $content -replace 'Content="[?][?] Chat"', 'Content="?? Chat"'
$content = $content -replace 'Content="[?][?] Help"', 'Content="? Help"'
$content = $content -replace 'Header="[?][?] Info"', 'Header="?? Info"'
$content = $content -replace 'Content="[?][?] Settings"', 'Content="?? Settings"'
$content = $content -replace 'Content="[?][?] Files"', 'Content="?? Files"'

Set-Content $desktopWidgetXaml $content -Encoding UTF8
Write-Host "? Fixed Desktop Widget icons" -ForegroundColor Green
Write-Host ""

# Fix 2: Context Menu (more complex - requires manual review)
Write-Host "?? Context Menu reorganization requires manual steps:" -ForegroundColor Yellow
Write-Host "   1. Open Controls\CandyBotAvatar.xaml" -ForegroundColor White
Write-Host "   2. Find the <ContextMenu> section (around line 153)" -ForegroundColor White
Write-Host "   3. Move '??? Desktop Mode' to be the FIRST menu item" -ForegroundColor White
Write-Host "   4. Make it Bold: FontWeight='Bold'" -ForegroundColor White
Write-Host "   5. Remove duplicate from Display Options submenu" -ForegroundColor White
Write-Host "" 
Write-Host "   See CONTEXT_MENU_FIX_INSTRUCTIONS.md for detailed steps" -ForegroundColor Cyan
Write-Host ""

# Summary
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "?? Desktop Widget Icons FIXED!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""
Write-Host "Changes made:" -ForegroundColor Cyan
Write-Host "  ? Title: ?? Candy-Bot Desktop Widget" -ForegroundColor Green
Write-Host "  ? Buttons: ?? Search, ?? Chat, ? Help" -ForegroundColor Green
Write-Host "  ? Tab: ?? Info" -ForegroundColor Green
Write-Host ""
Write-Host "Manual step required:" -ForegroundColor Yellow
Write-Host "  ?? Move Desktop Mode to top of context menu" -ForegroundColor Yellow
Write-Host "     (See CONTEXT_MENU_FIX_INSTRUCTIONS.md)" -ForegroundColor Yellow
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review changes in Visual Studio" -ForegroundColor White
Write-Host "  2. Apply context menu fix manually" -ForegroundColor White
Write-Host "  3. Build solution (Ctrl+Shift+B)" -ForegroundColor White
Write-Host "  4. Test desktop mode!" -ForegroundColor White
Write-Host ""
Write-Host "Backups saved with .backup extension" -ForegroundColor Gray
Write-Host "To restore: Copy-Item *.backup *.xaml" -ForegroundColor Gray
Write-Host ""
Write-Host "? Done!" -ForegroundColor Magenta
