# CHECK-USERS-IN-DATABASE.ps1
# Diagnostic script to check which Users container has data

$endpoint = "https://fallen-collective.documents.azure.com:443/"
$key = "EpxIq3hV8kXQ7kNY1KKJQmL5dkX0uZeW4GMUinPf6hNqRApx84Co5Ffve0bAktpyzH2xho5swBV5ACDbeunr5Q=="
$database = "DJBookingDB"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CHECKING USERS IN COSMOS DB" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Create connection string
$connString = "AccountEndpoint=$endpoint;AccountKey=$key;"

Write-Host "Connecting to Cosmos DB..." -ForegroundColor Yellow
Write-Host "  Database: $database" -ForegroundColor Gray
Write-Host ""

# This would require Azure.Cosmos SDK
# For now, instructions for manual check:

Write-Host "MANUAL CHECK INSTRUCTIONS:" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Open Azure Portal" -ForegroundColor Yellow
Write-Host "2. Go to Cosmos DB ? fallen-collective" -ForegroundColor Yellow
Write-Host "3. Open Data Explorer" -ForegroundColor Yellow
Write-Host "4. Expand DJBookingDB" -ForegroundColor Yellow
Write-Host ""
Write-Host "5. Check 'users' (lowercase):" -ForegroundColor Green
Write-Host "   - Click 'users' ? Items" -ForegroundColor White
Write-Host "   - Do you see: sysadmin, me, etc?" -ForegroundColor White
Write-Host ""
Write-Host "6. Check 'Users' (capitalized):" -ForegroundColor Green
Write-Host "   - Click 'Users' ? Items" -ForegroundColor White
Write-Host "   - Is this empty?" -ForegroundColor White
Write-Host ""
Write-Host "EXPECTED RESULT:" -ForegroundColor Cyan
Write-Host "  'users' (lowercase) = HAS DATA ?" -ForegroundColor Green
Write-Host "  'Users' (capitalized) = EMPTY ?" -ForegroundColor Red
Write-Host ""
Write-Host "IF CORRECT:" -ForegroundColor Yellow
Write-Host "  ? Delete the capitalized 'Users' container" -ForegroundColor White
Write-Host "  ? Keep the lowercase 'users' container" -ForegroundColor White
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "APP CONFIGURATION:" -ForegroundColor Cyan
Write-Host "  Container used by app: 'users' (lowercase)" -ForegroundColor White
Write-Host "  Partition key: /username" -ForegroundColor White
Write-Host ""
