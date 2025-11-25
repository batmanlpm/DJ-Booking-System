# BUILD-AND-UPLOAD.ps1
# Automated script to build installer and upload to Hostinger

param(
    [switch]$BuildOnly = $false
)

$ErrorActionPreference = "Stop"

Write-Host "?????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?     BUILD & UPLOAD TO HOSTINGER - AUTOMATED                   ?" -ForegroundColor Cyan
Write-Host "?????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# FTP Configuration
$FtpServer = "ftp://153.92.10.234"
$FtpUsername = "u833570579.djbookupdates.com"
$FtpPassword = "Fraser1960@"
$RemotePath = "/public_html/Updates"

# Local paths
$InstallerPath = "Installer\Output"
$InstallerPattern = "DJBookingSystem-Setup-*.exe"

# Step 1: Build the installer
Write-Host "Step 1: Building installer..." -ForegroundColor Yellow

if (-not (Test-Path "QUICK-BUILD.bat")) {
    Write-Host "  Running build script..." -ForegroundColor Gray
    & ".\Build-Installer.ps1"
} else {
    Write-Host "  Running quick build..." -ForegroundColor Gray
    & ".\QUICK-BUILD.bat"
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "  ? Build failed!" -ForegroundColor Red
    exit 1
}

# Find the installer
$installer = Get-ChildItem -Path $InstallerPath -Filter $InstallerPattern | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if (-not $installer) {
    Write-Host "  ? Installer not found!" -ForegroundColor Red
    exit 1
}

$installerSize = [math]::Round($installer.Length / 1MB, 2)
Write-Host "  ? Installer found: $($installer.Name)" -ForegroundColor Green
Write-Host "  Size: $installerSize MB" -ForegroundColor Gray

if ($BuildOnly) {
    Write-Host ""
    Write-Host "Build complete! Skipping upload (BuildOnly flag set)" -ForegroundColor Yellow
    exit 0
}

# Step 2: Upload to Hostinger via FTP
Write-Host ""
Write-Host "Step 2: Uploading to Hostinger..." -ForegroundColor Yellow
Write-Host "  Server: 153.92.10.234" -ForegroundColor Gray
Write-Host "  Path: $RemotePath" -ForegroundColor Gray
Write-Host "  File: $($installer.Name)" -ForegroundColor Gray
Write-Host ""

try {
    # Create FTP request
    $ftpUri = "$FtpServer$RemotePath/$($installer.Name)"
    Write-Host "  Connecting to: $ftpUri" -ForegroundColor Gray
    
    $ftpRequest = [System.Net.FtpWebRequest]::Create($ftpUri)
    $ftpRequest.Method = [System.Net.WebRequestMethods+Ftp]::UploadFile
    $ftpRequest.Credentials = New-Object System.Net.NetworkCredential($FtpUsername, $FtpPassword)
    $ftpRequest.UseBinary = $true
    $ftpRequest.KeepAlive = $true
    $ftpRequest.UsePassive = $true
    $ftpRequest.EnableSsl = $false
    
    # Read file content
    Write-Host "  Reading file..." -ForegroundColor Gray
    $fileContent = [System.IO.File]::ReadAllBytes($installer.FullName)
    $ftpRequest.ContentLength = $fileContent.Length
    
    # Upload with progress
    Write-Host "  Uploading $($installer.Name)... (this may take 10-20 minutes)" -ForegroundColor Yellow
    Write-Host "  File size: $installerSize MB" -ForegroundColor Gray
    
    $requestStream = $ftpRequest.GetRequestStream()
    
    $bufferSize = 32768  # 32KB chunks for better performance
    $bytesUploaded = 0
    $totalBytes = $fileContent.Length
    $lastPercent = 0
    
    for ($i = 0; $i -lt $totalBytes; $i += $bufferSize) {
        $size = [Math]::Min($bufferSize, $totalBytes - $i)
        $requestStream.Write($fileContent, $i, $size)
        $bytesUploaded += $size
        
        $percentComplete = [math]::Round(($bytesUploaded / $totalBytes) * 100, 0)
        if ($percentComplete -ne $lastPercent -and $percentComplete % 5 -eq 0) {
            $mbUploaded = [math]::Round($bytesUploaded / 1MB, 2)
            Write-Host "    $percentComplete% ($mbUploaded MB / $installerSize MB)" -ForegroundColor Cyan
            $lastPercent = $percentComplete
        }
    }
    
    $requestStream.Close()
    
    # Get response
    $response = $ftpRequest.GetResponse()
    Write-Host "  ? Upload complete! Status: $($response.StatusDescription)" -ForegroundColor Green
    $response.Close()
    
    Write-Host ""
    Write-Host "?????????????????????????????????????????????????????????????????" -ForegroundColor Green
    Write-Host "?                  UPLOAD SUCCESSFUL!                           ?" -ForegroundColor Green
    Write-Host "?????????????????????????????????????????????????????????????????" -ForegroundColor Green
    Write-Host ""
    Write-Host "Installer URL:" -ForegroundColor Yellow
    Write-Host "  https://djbookupdates.com/Updates/$($installer.Name)" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "  1. Test URL in browser" -ForegroundColor White
    Write-Host "  2. Update version.json if needed" -ForegroundColor White
    Write-Host "  3. Test your application!" -ForegroundColor White
    Write-Host ""
    
} catch {
    Write-Host ""
    Write-Host "  ? Upload failed: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  1. Check internet connection" -ForegroundColor White
    Write-Host "  2. Verify FTP credentials" -ForegroundColor White
    Write-Host "  3. Try uploading with FileZilla manually" -ForegroundColor White
    Write-Host "  4. Use: filezilla-djbookupdates.xml for quick connect" -ForegroundColor White
    Write-Host ""
    exit 1
}
