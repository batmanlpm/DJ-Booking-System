# Script to convert all FirebaseService references to CosmosDbService

$projectPath = "K:\Customer Data\LPM\New-Booking-claude-initial-setup-011CV2Bn45svzRU7VxANArie\Fallen-Collective-Booking"

# Get all .cs files except the FirebaseService.cs itself
$files = Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse | Where-Object { $_.Name -ne "FirebaseService.cs" -and $_.Name -ne "IDataService.cs" }

foreach ($file in $files) {
    Write-Host "Processing: $($file.FullName)"
    
    $content = Get-Content $file.FullName -Raw
    
    # Replace FirebaseService with CosmosDbService
    $content = $content -replace 'FirebaseService', 'CosmosDbService'
    
    # Replace _firebaseService with _cosmosDbService
    $content = $content -replace '_firebaseService', '_cosmosDbService'
    
    # Replace firebaseService (parameter names) with cosmosDbService
    $content = $content -replace '\bfirebaseService\b', 'cosmosDbService'
    
    # Write back
    Set-Content -Path $file.FullName -Value $content -NoNewline
}

Write-Host "Conversion complete!"
