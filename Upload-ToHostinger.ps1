# ?? HOSTINGER AUTO-UPLOAD SCRIPT
# Uploads new version to Hostinger with automatic OLD_VERSIONS management

param(
    [string]$Version = "1.2.5",
    [string]$HostingerHost = "your-site.com",
    [string]$HostingerUser = "your-username",
    [string]$HostingerPath = "/public_html/downloads",
    [switch]$ShowReleaseNotesOnly
)

# Colors for output
function Write-Success { param($msg) Write-Host $msg -ForegroundColor Green }
function Write-Info { param($msg) Write-Host $msg -ForegroundColor Cyan }
function Write-Warning { param($msg) Write-Host $msg -ForegroundColor Yellow }
function Write-Error { param($msg) Write-Host $msg -ForegroundColor Red }

# Release notes template
$releaseNotes = @"
? What's New in v$Version

MAJOR FEATURES:
? Mandatory auto-playing tutorial with CandyBot voice narration
? Interactive UI highlighting with pink spotlight
? 100% black overlay outside MainWindow
? Users panel pop-out maximizable window
? Tutorial checkbox for admin re-training

BUG FIXES:
? Fixed logout DialogResult error
? Fixed tutorial audio playback (absolute paths)
? Fixed element highlighting (added x:Name attributes)
? Fixed panel positioning (inside MainWindow, not taskbar)
? Fixed voice file paths (InteractiveGuide/Voices/)
? Fixed Cosmos DB partition key mismatch

IMPROVEMENTS:
? Tutorial cannot be skipped (mandatory completion)
? Tutorial cannot be closed early
? Auto-advances when voice finishes
? 1 second pause between steps
? Fallback timers for missing audio files
"@

# Save release notes to temp file for editing
$tempFile = [System.IO.Path]::GetTempFileName()
$releaseNotes | Out-File -FilePath $tempFile -Encoding UTF8

# Show release notes for editing
Write-Info "`n?? RELEASE NOTES FOR v$Version"
Write-Info "?????????????????????????????????????????????????????????????"
Write-Host $releaseNotes -ForegroundColor White
Write-Info "?????????????????????????????????????????????????????????????"

if ($ShowReleaseNotesOnly) {
    Write-Info "`nOpening release notes in Notepad for editing..."
    notepad $tempFile
    
    Write-Warning "`nAfter editing, run the script again without -ShowReleaseNotesOnly to upload"
    return
}

Write-Warning "`n??  REVIEW RELEASE NOTES ABOVE"
$confirm = Read-Host "`nDo you want to edit the release notes? (Y/N)"

if ($confirm -eq "Y" -or $confirm -eq "y") {
    notepad $tempFile
    Write-Info "Waiting for Notepad to close..."
    
    # Wait for user to close notepad
    Write-Warning "`nPress ENTER after you've finished editing and closed Notepad..."
    Read-Host
    
    # Read updated release notes
    $releaseNotes = Get-Content $tempFile -Raw
    
    Write-Success "`n? Release notes updated!"
    Write-Host $releaseNotes -ForegroundColor White
}

Write-Warning "`n??  READY TO UPLOAD v$Version"
$upload = Read-Host "`nProceed with upload to Hostinger? (Y/N)"

if ($upload -ne "Y" -and $upload -ne "y") {
    Write-Warning "Upload cancelled"
    return
}

# Files to upload
$publishPath = "bin\Release\net8.0-windows\win-x64\publish"
$exeFile = "$publishPath\DJBookingSystem.exe"
$fullZipOld = "$publishPath\DJBookingSystem-Full-v$Version.zip"

# Check if files exist
if (-not (Test-Path $exeFile)) {
    Write-Error "? EXE not found: $exeFile"
    Write-Info "Run: dotnet publish -c Release --self-contained false -r win-x64"
    return
}

Write-Info "`n?? CREATING ZIP PACKAGE..."
cd $publishPath

# Create full ZIP package
if (Test-Path "DJBookingSystem-Full-v$Version.zip") {
    Remove-Item "DJBookingSystem-Full-v$Version.zip" -Force
}

Compress-Archive -Path * -DestinationPath "DJBookingSystem-Full-v$Version.zip" -Force
Write-Success "? Created: DJBookingSystem-Full-v$Version.zip"

cd ..\..\..\..\

# Update update-info.json
Write-Info "`n?? UPDATING update-info.json..."

$updateInfo = @{
    version = $Version
    releaseDate = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
    downloadUrl = "https://$HostingerHost/DJBookingSystem.exe"
    fullPackageUrl = "https://$HostingerHost/DJBookingSystem-Full-v$Version.zip"
    isCritical = $false
    releaseNotes = $releaseNotes
    minimumVersion = "1.0.0"
    fileSize = (Get-Item $exeFile).Length
    sha256Hash = (Get-FileHash $exeFile -Algorithm SHA256).Hash
} | ConvertTo-Json -Depth 10

$updateInfo | Out-File -FilePath "update-info.json" -Encoding UTF8 -Force
Write-Success "? Updated: update-info.json"

# Update download.html
Write-Info "`n?? UPDATING download.html..."

$htmlContent = Get-Content "download.html" -Raw

# Replace version number
$htmlContent = $htmlContent -replace 'Version \d+\.\d+\.\d+', "Version $Version"

# Replace release notes (find the "What's New" section and replace content)
$whatsnewPattern = '(<h3>.*What''s New in v[\d\.]+</h3>\s*<ul>)(.*?)(</ul>)'
$newWhatsNew = "<h3>? What's New in v$Version</h3>`n            <ul>`n"

# Convert release notes to HTML list items
$releaseNotes -split "`n" | Where-Object { $_ -match '?' } | ForEach-Object {
    $item = $_ -replace '?\s*', ''
    if ($item.Trim()) {
        $newWhatsNew += "                <li>? $item</li>`n"
    }
}

$newWhatsNew += "            </ul>"

$htmlContent = $htmlContent -replace $whatsnewPattern, $newWhatsNew

$htmlContent | Out-File -FilePath "download.html" -Encoding UTF8 -Force
Write-Success "? Updated: download.html"

Write-Info "`n?? UPLOADING TO HOSTINGER..."
Write-Warning "??  You'll need to configure FTP/SFTP credentials"

Write-Info @"

MANUAL UPLOAD STEPS (Until FTP is configured):
?????????????????????????????????????????????????????????????

1. LOGIN TO HOSTINGER
   - Go to: https://hpanel.hostinger.com
   - Login with your credentials

2. OPEN FILE MANAGER or FTP
   - Navigate to: $HostingerPath

3. BACKUP CURRENT VERSION TO OLD_VERSIONS
   - Move existing DJBookingSystem.exe ? OLD_VERSIONS/DJBookingSystem-v[old-version].exe
   - Move existing DJBookingSystem-Full-v[old].zip ? OLD_VERSIONS/

4. UPLOAD NEW FILES
   Files to upload:
   ? $exeFile
      ? Upload as: DJBookingSystem.exe
   
   ? $publishPath\DJBookingSystem-Full-v$Version.zip
      ? Upload as: DJBookingSystem-Full-v$Version.zip
   
   ? update-info.json
      ? Upload/overwrite existing
   
   ? download.html
      ? Upload/overwrite existing

5. VERIFY LINKS
   - https://$HostingerHost/download.html
   - https://$HostingerHost/DJBookingSystem.exe
   - https://$HostingerHost/update-info.json

?????????????????????????????????????????????????????????????

"@

Write-Success "`n? ALL FILES PREPARED FOR UPLOAD!"
Write-Info "`nFiles ready in:"
Write-Info "   EXE: $exeFile"
Write-Info "   ZIP: $publishPath\DJBookingSystem-Full-v$Version.zip"
Write-Info "   JSON: update-info.json"
Write-Info "   HTML: download.html"

# Cleanup
Remove-Item $tempFile -Force

Write-Success "`n?? UPLOAD PREPARATION COMPLETE!"
