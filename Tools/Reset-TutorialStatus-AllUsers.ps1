# Reset Tutorial Status for All Users
# This script updates all users in Cosmos DB to set HasSeenTutorial = false
# So they will see the mandatory tutorial on next login

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Reset Tutorial Status for All Users" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Cosmos DB Configuration
$CosmosEndpoint = "https://djbookingcosmos.documents.azure.com:443/"
$DatabaseName = "DJBookingDB"
$ContainerName = "Users"

Write-Host "This script will:" -ForegroundColor Yellow
Write-Host "  1. Connect to Cosmos DB" -ForegroundColor White
Write-Host "  2. Update ALL users to set HasSeenTutorial = false" -ForegroundColor White
Write-Host "  3. Users will see mandatory tutorial on next login" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "Do you want to continue? (yes/no)"
if ($confirm -ne "yes") {
    Write-Host "Operation cancelled." -ForegroundColor Red
    exit
}

Write-Host ""
Write-Host "Enter your Cosmos DB Primary Key:" -ForegroundColor Cyan
$CosmosKey = Read-Host -AsSecureString

# Convert secure string to plain text
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($CosmosKey)
$PlainKey = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

Write-Host ""
Write-Host "Connecting to Cosmos DB..." -ForegroundColor Green

# Create authorization header
$date = Get-Date -Format "r"
$verb = "GET"
$resourceType = "docs"
$resourceLink = "dbs/$DatabaseName/colls/$ContainerName"

# Build the authorization signature
$keyBytes = [System.Convert]::FromBase64String($PlainKey)
$stringToSign = "$verb`n$resourceType`n$resourceLink`n$($date.ToLower())`n`n"
$bytesToSign = [System.Text.Encoding]::UTF8.GetBytes($stringToSign)
$hmac = New-Object System.Security.Cryptography.HMACSHA256(,$keyBytes)
$hash = $hmac.ComputeHash($bytesToSign)
$signature = [System.Convert]::ToBase64String($hash)
$authToken = [System.Web.HttpUtility]::UrlEncode("type=master&ver=1.0&sig=$signature")

# Query all users
$queryUrl = "$CosmosEndpoint$resourceLink/docs"
$headers = @{
    "Authorization" = $authToken
    "x-ms-date" = $date
    "x-ms-version" = "2018-12-31"
    "Content-Type" = "application/query+json"
}

$queryBody = @{
    "query" = "SELECT * FROM c"
    "parameters" = @()
} | ConvertTo-Json

try {
    Write-Host "Fetching all users..." -ForegroundColor Green
    $response = Invoke-RestMethod -Uri $queryUrl -Method Post -Headers $headers -Body $queryBody
    
    $users = $response.Documents
    Write-Host "Found $($users.Count) users" -ForegroundColor Cyan
    Write-Host ""
    
    $updatedCount = 0
    $skippedCount = 0
    
    foreach ($user in $users) {
        $username = $user.username
        
        # Check if user already has HasSeenTutorial = false
        if ($user.hasSeenTutorial -eq $false) {
            Write-Host "  ??  Skipping $username (already set to false)" -ForegroundColor Gray
            $skippedCount++
            continue
        }
        
        # Update user document
        $user.hasSeenTutorial = $false
        
        # Update in Cosmos DB
        $updateUrl = "$CosmosEndpoint$resourceLink/docs/$($user.id)"
        $updateHeaders = @{
            "Authorization" = $authToken
            "x-ms-date" = (Get-Date -Format "r")
            "x-ms-version" = "2018-12-31"
            "Content-Type" = "application/json"
            "x-ms-documentdb-partitionkey" = "[`"$username`"]"
        }
        
        $updateBody = $user | ConvertTo-Json -Depth 10
        
        try {
            Invoke-RestMethod -Uri $updateUrl -Method Put -Headers $updateHeaders -Body $updateBody | Out-Null
            Write-Host "  ? Updated $username" -ForegroundColor Green
            $updatedCount++
        }
        catch {
            Write-Host "  ? Failed to update $username : $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Summary:" -ForegroundColor Cyan
    Write-Host "  Total users: $($users.Count)" -ForegroundColor White
    Write-Host "  Updated: $updatedCount" -ForegroundColor Green
    Write-Host "  Skipped: $skippedCount" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "? All users will now see the mandatory tutorial on next login!" -ForegroundColor Green
}
catch {
    Write-Host "? Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Stack Trace:" -ForegroundColor Yellow
    Write-Host $_.Exception.StackTrace -ForegroundColor Gray
}

# Clear the key from memory
$PlainKey = $null
[System.GC]::Collect()

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
