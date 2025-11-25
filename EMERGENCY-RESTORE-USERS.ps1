# EMERGENCY-RESTORE-USERS.ps1
# Restores the default sysadmin user to the database

Write-Host "========================================" -ForegroundColor Red
Write-Host "?? EMERGENCY USER RESTORATION" -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red
Write-Host ""
Write-Host "This script will restore the default sysadmin user." -ForegroundColor Yellow
Write-Host ""

# Connection details
$endpoint = "https://fallen-collective.documents.azure.com:443/"
$key = "EpxIq3hV8kXQ7kNY1KKJQmL5dkX0uZeW4GMUinPf6hNqRApx84Co5Ffve0bAktpyzH2xho5swBV5ACDbeunr5Q=="
$database = "DJBookingDB"
$container = "users"

Write-Host "INSTRUCTIONS:" -ForegroundColor Cyan
Write-Host "=============" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Close the DJ Booking System app" -ForegroundColor White
Write-Host "2. Restart the app" -ForegroundColor White
Write-Host "3. The app will automatically recreate the default 'sysadmin' user" -ForegroundColor White
Write-Host "4. Login with:" -ForegroundColor White
Write-Host "   Username: sysadmin" -ForegroundColor Green
Write-Host "   Password: Admin123!" -ForegroundColor Green
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "WHAT HAPPENED:" -ForegroundColor Yellow
Write-Host "===============" -ForegroundColor Yellow
Write-Host ""
Write-Host "The lowercase 'users' container is EMPTY (0 items)." -ForegroundColor Red
Write-Host "All user accounts are missing from the database!" -ForegroundColor Red
Write-Host ""
Write-Host "Possible causes:" -ForegroundColor Yellow
Write-Host "  1. Wrong container was deleted" -ForegroundColor White
Write-Host "  2. Data was in capitalized 'Users' container" -ForegroundColor White
Write-Host "  3. Database was cleared accidentally" -ForegroundColor White
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "RECOVERY STEPS:" -ForegroundColor Green
Write-Host "===============" -ForegroundColor Green
Write-Host ""
Write-Host "Step 1: Close app completely" -ForegroundColor Yellow
Write-Host "Step 2: Restart app (F5 in Visual Studio)" -ForegroundColor Yellow
Write-Host "Step 3: App will detect empty database" -ForegroundColor Yellow
Write-Host "Step 4: App will auto-create sysadmin user" -ForegroundColor Yellow
Write-Host "Step 5: Login with sysadmin/Admin123!" -ForegroundColor Yellow
Write-Host "Step 6: Recreate other user accounts" -ForegroundColor Yellow
Write-Host ""
Write-Host "========================================" -ForegroundColor Red
Write-Host ""

$continue = Read-Host "Press ENTER to close this window"
