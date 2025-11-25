<#
.SYNOPSIS
    Deletes test accounts from Cosmos DB (keeps only SysAdmin)

.DESCRIPTION
    This script deletes test1 and test2 accounts from the Cosmos DB database.
    Only SysAdmin account will remain.

.EXAMPLE
    .\Delete-TestAccounts.ps1
#>

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  DELETE TEST ACCOUNTS FROM COSMOS DB" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Get Cosmos DB connection string from environment or config
$cosmosConnectionString = $env:COSMOS_DB_CONNECTION_STRING

if ([string]::IsNullOrWhiteSpace($cosmosConnectionString)) {
    Write-Host "ERROR: Cosmos DB connection string not found!" -ForegroundColor Red
    Write-Host "Please set COSMOS_DB_CONNECTION_STRING environment variable" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Example:" -ForegroundColor Gray
    Write-Host '  $env:COSMOS_DB_CONNECTION_STRING = "AccountEndpoint=https://...;AccountKey=...;"' -ForegroundColor Gray
    exit 1
}

# Test accounts to delete
$testAccounts = @("test1", "test2")

Write-Host "Test accounts to delete:" -ForegroundColor Yellow
foreach ($account in $testAccounts) {
    Write-Host "  - $account" -ForegroundColor White
}
Write-Host ""

# Confirm deletion
$confirmation = Read-Host "Are you sure you want to delete these accounts? (yes/no)"
if ($confirmation -ne "yes") {
    Write-Host "Deletion cancelled." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Deleting test accounts..." -ForegroundColor Cyan

# Load the compiled tool
$toolPath = Join-Path $PSScriptRoot "bin\Release\net8.0\MigrationTool.dll"

if (-not (Test-Path $toolPath)) {
    Write-Host "ERROR: MigrationTool.dll not found at: $toolPath" -ForegroundColor Red
    Write-Host "Please build the MigrationTool project first" -ForegroundColor Yellow
    exit 1
}

try {
    # Use the CosmosDB service to delete accounts
    foreach ($username in $testAccounts) {
        Write-Host "Deleting account: $username..." -ForegroundColor Yellow -NoNewline
        
        # Call dotnet to execute deletion
        $result = dotnet $toolPath delete-user $username $cosmosConnectionString 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host " ? DELETED" -ForegroundColor Green
        } else {
            Write-Host " ? FAILED" -ForegroundColor Red
            Write-Host "Error: $result" -ForegroundColor Red
        }
    }
    
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Green
    Write-Host "  TEST ACCOUNTS DELETION COMPLETE" -ForegroundColor Green
    Write-Host "============================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Remaining account: SysAdmin only" -ForegroundColor Cyan
    
} catch {
    Write-Host ""
    Write-Host "ERROR: Failed to delete test accounts" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
