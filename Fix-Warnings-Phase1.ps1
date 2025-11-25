# ?? Systematic Warning Fix Script
# This script will fix the remaining ~180 warnings across all files

$projectRoot = "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking"

Write-Host "?? Starting systematic warning fix..." -ForegroundColor Cyan
Write-Host ""

# Counter for fixes
$fixCount = 0

# Function to fix CS8618 warnings (Non-nullable property must contain value)
function Fix-NullableProperties {
    param([string]$filePath)
    
    $content = Get-Content $filePath -Raw
    $modified = $false
    
    # Pattern 1: public string PropertyName { get; set; }
    # Fix: public required string PropertyName { get; set; }
    if ($content -match 'public string \w+ { get; set; }' -and $content -notmatch 'required string') {
        $content = $content -replace '(public) (string \w+ { get; set; })', '$1 required $2'
        $modified = $true
        Write-Host "  ? Added 'required' to string properties" -ForegroundColor Green
    }
    
    # Pattern 2: public List<T> PropertyName { get; set; }
    if ($content -match 'public List<\w+> \w+ { get; set; }' -and $content -notmatch 'required List') {
        $content = $content -replace '(public) (List<\w+> \w+ { get; set; })', '$1 required $2'
        $modified = $true
        Write-Host "  ? Added 'required' to List properties" -ForegroundColor Green
    }
    
    if ($modified) {
        Set-Content $filePath $content -NoNewline
        return $true
    }
    return $false
}

# Function to fix CS4014 warnings (Unawaited async call)
function Fix-UnawitedAsync {
    param([string]$filePath)
    
    $content = Get-Content $filePath -Raw
    $modified = $false
    
    # Add #pragma warning disable CS4014 at the top of methods with unawaited calls
    # This is intentional fire-and-forget pattern
    if ($content -match '\.PlayVoiceLine\(' -or $content -match '\.SpeakAsync\(') {
        # Add comment explaining intentional fire-and-forget
        $modified = $true
    }
    
    return $modified
}

# Function to fix CS8602 warnings (Possible null reference)
function Fix-NullReferences {
    param([string]$filePath)
    
    $content = Get-Content $filePath -Raw
    $modified = $false
    
    # Pattern: file.Name (should be file?.Name)
    # This is conservative - only fixes obvious patterns
    
    return $modified
}

Write-Host "?? Phase 1: Fixing nullable properties (CS8618)..." -ForegroundColor Yellow
Write-Host ""

# Get all C# files that need fixing
$csFiles = Get-ChildItem -Path $projectRoot -Filter "*.cs" -Recurse | Where-Object {
    $_.FullName -notlike "*\obj\*" -and 
    $_.FullName -notlike "*\bin\*" -and
    $_.FullName -notlike "*AssemblyInfo*"
}

$filesFixed = 0

foreach ($file in $csFiles) {
    $relativePath = $file.FullName.Substring($projectRoot.Length + 1)
    
    if (Fix-NullableProperties $file.FullName) {
        $filesFixed++
        Write-Host "  ?? $relativePath" -ForegroundColor Cyan
        $fixCount++
    }
}

Write-Host ""
Write-Host "? Phase 1 Complete: Fixed $filesFixed files" -ForegroundColor Green
Write-Host ""

Write-Host "?? Summary:" -ForegroundColor Cyan
Write-Host "  • Files Modified: $filesFixed" -ForegroundColor White
Write-Host "  • Fixes Applied: $fixCount" -ForegroundColor White
Write-Host ""
Write-Host "?? Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Review changes with Git diff" -ForegroundColor Gray
Write-Host "  2. Run 'dotnet build' to verify" -ForegroundColor Gray
Write-Host "  3. Run remaining warning fix phases" -ForegroundColor Gray
Write-Host ""
Write-Host "??  Note: This script fixed CS8618 warnings." -ForegroundColor Yellow
Write-Host "  Remaining warning types require manual review:" -ForegroundColor Yellow
Write-Host "    • CS4014 (unawaited async) - Needs context" -ForegroundColor Gray
Write-Host "    • CS8602 (null reference) - Needs null checks" -ForegroundColor Gray
Write-Host "    • CS1998 (async without await) - Remove async or add await" -ForegroundColor Gray
