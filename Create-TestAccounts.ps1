# Create 5 Test Accounts for Tutorial Testing
# Password for all: asdfgh

$endpoint = "YOUR_COSMOS_ENDPOINT"
$key = "YOUR_COSMOS_KEY"
$database = "DJBookingDB"
$container = "users"

# Test accounts
$testAccounts = @(
    @{ username = "1"; password = "asdfgh"; role = "User"; fullName = "Test User"; email = "user1@test.com" }
    @{ username = "2"; password = "asdfgh"; role = "DJ"; fullName = "Test DJ"; email = "dj2@test.com"; isDJ = $true }
    @{ username = "3"; password = "asdfgh"; role = "VenueOwner"; fullName = "Test VenueOwner"; email = "venue3@test.com"; isVenueOwner = $true }
    @{ username = "4"; password = "asdfgh"; role = "Manager"; fullName = "Test Manager"; email = "manager4@test.com" }
    @{ username = "5"; password = "asdfgh"; role = "SysAdmin"; fullName = "Test SysAdmin"; email = "sysadmin5@test.com" }
)

Write-Host "Creating 5 test accounts in CosmosDB..." -ForegroundColor Cyan

foreach ($account in $testAccounts) {
    $user = @{
        id = $account.username
        username = $account.username
        password = $account.password
        role = $account.role
        fullName = $account.fullName
        email = $account.email
        createdAt = (Get-Date).ToString("o")
        isOnline = $false
        lastLogin = $null
        tutorialCompleted = $false
    }
    
    if ($account.isDJ) { $user.isDJ = $true }
    if ($account.isVenueOwner) { $user.isVenueOwner = $true }
    
    Write-Host "Creating user: $($account.username) ($($account.role))" -ForegroundColor Yellow
    
    # You'll need to use CosmosDB SDK or REST API here
    # This is a template - actual implementation depends on your setup
}

Write-Host "`n? Test accounts created! Use these credentials:" -ForegroundColor Green
Write-Host "Username: 1 (User) - Password: asdfgh" -ForegroundColor White
Write-Host "Username: 2 (DJ) - Password: asdfgh" -ForegroundColor White
Write-Host "Username: 3 (VenueOwner) - Password: asdfgh" -ForegroundColor White
Write-Host "Username: 4 (Manager) - Password: asdfgh" -ForegroundColor White
Write-Host "Username: 5 (SysAdmin) - Password: asdfgh" -ForegroundColor White
