# AUTO-DEPLOY.ps1
# Fully Automated Build, Upload, and Deploy - NO USER INPUT

$ErrorActionPreference = "Stop"

Write-Host "???????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  FULLY AUTOMATED DEPLOYMENT - Version 1.2.1" -ForegroundColor Cyan
Write-Host "???????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Step 1: Clean old builds
Write-Host "Step 1: Cleaning old builds..." -ForegroundColor Yellow
Remove-Item "Installer\Output\*.exe" -Force -ErrorAction SilentlyContinue
Write-Host "  ? Old builds removed" -ForegroundColor Green

# Step 2: Publish application
Write-Host ""
Write-Host "Step 2: Publishing application..." -ForegroundColor Yellow
$publishPath = "bin\Release\net8.0-windows\win-x64\publish"
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false 2>&1 | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ? Application published" -ForegroundColor Green
} else {
    Write-Host "  ? Publish failed!" -ForegroundColor Red
    exit 1
}

# Step 3: Build installer
Write-Host ""
Write-Host "Step 3: Building installer..." -ForegroundColor Yellow
$isccPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
& $isccPath "installer.iss" 2>&1 | Out-Null
if ($LASTEXITCODE -eq 0) {
    $installer = Get-ChildItem "Installer\Output\*.exe" | Select-Object -First 1
    if ($installer) {
        $size = [math]::Round($installer.Length / 1MB, 2)
        Write-Host "  ? Installer created: $($installer.Name)" -ForegroundColor Green
        Write-Host "  Size: $size MB" -ForegroundColor Gray
    } else {
        Write-Host "  ? Installer file not found!" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "  ? Installer build failed!" -ForegroundColor Red
    exit 1
}

# Step 4: Upload installer
Write-Host ""
Write-Host "Step 4: Uploading installer to Hostinger..." -ForegroundColor Yellow
$ftpServer = "ftp://153.92.10.234"
$ftpUsername = "u833570579.Upload"
$ftpPassword = "Fraser1960@"

try {
    $webclient = New-Object System.Net.WebClient
    $webclient.Credentials = New-Object System.Net.NetworkCredential($ftpUsername, $ftpPassword)
    $uri = New-Object System.Uri("$ftpServer/$($installer.Name)")
    $webclient.UploadFile($uri, $installer.FullName)
    $webclient.Dispose()
    Write-Host "  ? Installer uploaded" -ForegroundColor Green
} catch {
    Write-Host "  ? Upload failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 5: Upload version.json
Write-Host ""
Write-Host "Step 5: Uploading version.json..." -ForegroundColor Yellow
try {
    $webclient = New-Object System.Net.WebClient
    $webclient.Credentials = New-Object System.Net.NetworkCredential($ftpUsername, $ftpPassword)
    $uri = New-Object System.Uri("$ftpServer/version.json")
    $webclient.UploadFile($uri, "Installer\Output\version.json")
    $webclient.Dispose()
    Write-Host "  ? version.json uploaded" -ForegroundColor Green
} catch {
    Write-Host "  ? Upload failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Done!
Write-Host ""
Write-Host "???????????????????????????????????????????????" -ForegroundColor Green
Write-Host "  DEPLOYMENT COMPLETE!" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""
Write-Host "Installer URL:" -ForegroundColor Yellow
Write-Host "  https://djbookupdates.com/$($installer.Name)" -ForegroundColor Cyan
Write-Host ""
Write-Host "Version JSON:" -ForegroundColor Yellow
Write-Host "  https://djbookupdates.com/version.json" -ForegroundColor Cyan
Write-Host ""
Write-Host "? Both PCs will auto-update in 10 seconds!" -ForegroundColor Yellow
Write-Host ""
