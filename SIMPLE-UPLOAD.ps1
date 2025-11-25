# SIMPLE-UPLOAD.ps1
# Simple FTP upload using .NET WebClient (more reliable)

param(
    [string]$FilePath = "",
    [switch]$Help
)

if ($Help) {
    Write-Host @"
SIMPLE FTP UPLOAD TO HOSTINGER

Usage:
  .\SIMPLE-UPLOAD.ps1 -FilePath "path\to\file.exe"
  
  Or just run it and it will find the installer automatically:
  .\SIMPLE-UPLOAD.ps1

FTP Details:
  Server: 153.92.10.234
  Username: u833570579
  Password: Fraser1960@
  Path: /public_html/Updates/
"@
    exit 0
}

$ErrorActionPreference = "Stop"

Write-Host "???????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  SIMPLE FTP UPLOAD TO HOSTINGER" -ForegroundColor Cyan
Write-Host "???????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# FTP Configuration - Use the full username with .Upload suffix
$ftpServer = "ftp://153.92.10.234"
$ftpUsername = "u833570579.Upload"
$ftpPassword = "Fraser1960@"
$remotePath = ""  # Start at root to verify path

# Find installer if not specified
if ([string]::IsNullOrEmpty($FilePath)) {
    Write-Host "Looking for installer..." -ForegroundColor Yellow
    $installer = Get-ChildItem -Path "Installer\Output" -Filter "*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    
    if (-not $installer) {
        Write-Host "? No installer found in Installer\Output\" -ForegroundColor Red
        Write-Host "  Run .\QUICK-BUILD.bat first!" -ForegroundColor Yellow
        exit 1
    }
    
    $FilePath = $installer.FullName
    Write-Host "? Found: $($installer.Name)" -ForegroundColor Green
}

if (-not (Test-Path $FilePath)) {
    Write-Host "? File not found: $FilePath" -ForegroundColor Red
    exit 1
}

$file = Get-Item $FilePath
$fileSize = [math]::Round($file.Length / 1MB, 2)

Write-Host ""
Write-Host "File: $($file.Name)" -ForegroundColor Cyan
Write-Host "Size: $fileSize MB" -ForegroundColor Cyan
Write-Host "Destination: $ftpServer$remotePath/" -ForegroundColor Cyan
Write-Host ""
Write-Host "This will take approximately 10-20 minutes..." -ForegroundColor Yellow
Write-Host ""

# Create WebClient with credentials
$webclient = New-Object System.Net.WebClient
$webclient.Credentials = New-Object System.Net.NetworkCredential($ftpUsername, $ftpPassword)

# FTP URI
$uri = New-Object System.Uri("$ftpServer$remotePath/$($file.Name)")

Write-Host "Uploading..." -ForegroundColor Yellow

try {
    # Register progress event
    $progressHandler = {
        param($sender, $e)
        $percent = [math]::Round(($e.BytesSent / $file.Length) * 100, 0)
        if ($percent % 10 -eq 0 -and $percent -ne $script:lastPercent) {
            $mbSent = [math]::Round($e.BytesSent / 1MB, 2)
            Write-Host "  $percent% ($mbSent MB / $fileSize MB)" -ForegroundColor Cyan
            $script:lastPercent = $percent
        }
    }
    
    $script:lastPercent = 0
    Register-ObjectEvent -InputObject $webclient -EventName UploadProgressChanged -Action $progressHandler | Out-Null
    
    # Upload file
    $webclient.UploadFile($uri, $FilePath)
    
    # Cleanup
    Get-EventSubscriber | Where-Object { $_.SourceObject -eq $webclient } | Unregister-Event
    
    Write-Host ""
    Write-Host "???????????????????????????????????????????" -ForegroundColor Green
    Write-Host "  UPLOAD SUCCESSFUL!" -ForegroundColor Green
    Write-Host "???????????????????????????????????????????" -ForegroundColor Green
    Write-Host ""
    Write-Host "File uploaded to:" -ForegroundColor Yellow
    Write-Host "  https://djbookupdates.com/Updates/$($file.Name)" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Test it:" -ForegroundColor Yellow
    Write-Host "  Start-Process 'https://djbookupdates.com/Updates/$($file.Name)'" -ForegroundColor White
    Write-Host ""
    
} catch {
    Write-Host ""
    Write-Host "? Upload failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  1. Check internet connection" -ForegroundColor White
    Write-Host "  2. Verify FTP credentials" -ForegroundColor White
    Write-Host "  3. Try FileZilla instead:" -ForegroundColor White
    Write-Host "     - Download: https://filezilla-project.org/" -ForegroundColor Gray
    Write-Host "     - Or use: filezilla-djbookupdates.xml" -ForegroundColor Gray
    Write-Host ""
    exit 1
} finally {
    $webclient.Dispose()
}
