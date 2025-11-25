# Add 5 Test Accounts to CosmosDB
# NO EMAILS - Just username, password, role

Write-Host "?? Creating 5 Test Accounts for Tutorial Testing" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

$accounts = @"
Username: 1 | Password: asdfgh | Role: User
Username: 2 | Password: asdfgh | Role: DJ
Username: 3 | Password: asdfgh | Role: VenueOwner  
Username: 4 | Password: asdfgh | Role: Manager
Username: 5 | Password: asdfgh | Role: SysAdmin
"@

Write-Host $accounts -ForegroundColor Yellow
Write-Host ""
Write-Host "? Use the USERS panel in the app to create these accounts" -ForegroundColor Green
Write-Host "   - Log in as SysAdmin" -ForegroundColor White
Write-Host "   - Go to Users tab" -ForegroundColor White
Write-Host "   - Click Create User for each account" -ForegroundColor White
Write-Host "   - NO EMAIL NEEDED!" -ForegroundColor Red
Write-Host ""
Write-Host "?? Account Details:" -ForegroundColor Cyan
Write-Host "   1: Role=User" -ForegroundColor White
Write-Host "   2: Role=DJ, IsDJ=true" -ForegroundColor White
Write-Host "   3: Role=VenueOwner, IsVenueOwner=true" -ForegroundColor White
Write-Host "   4: Role=Manager" -ForegroundColor White
Write-Host "   5: Role=SysAdmin" -ForegroundColor White
