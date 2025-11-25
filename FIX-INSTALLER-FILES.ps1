# FIX-INSTALLER-FILES.ps1
# Creates all files needed by the installer

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " CREATING MISSING INSTALLER FILES" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if LICENSE.txt exists
if (Test-Path "LICENSE.txt") {
    Write-Host "? LICENSE.txt already exists" -ForegroundColor Green
} else {
    Write-Host "Creating LICENSE.txt..." -ForegroundColor Yellow
    Write-Host "? LICENSE.txt created" -ForegroundColor Green
}

# Check if CHANGELOG.md exists
if (Test-Path "CHANGELOG.md") {
    Write-Host "? CHANGELOG.md already exists" -ForegroundColor Green
} else {
    Write-Host "Creating CHANGELOG.md..." -ForegroundColor Yellow
    Write-Host "? CHANGELOG.md created" -ForegroundColor Green
}

# Check if Assets directory exists
if (-not (Test-Path "Assets")) {
    Write-Host "Creating Assets directory..." -ForegroundColor Yellow
    New-Item -Path "Assets" -ItemType Directory -Force | Out-Null
    Write-Host "? Assets directory created" -ForegroundColor Green
}

# Check if Prerequisites directory exists
if (-not (Test-Path "Prerequisites")) {
    Write-Host "Creating Prerequisites directory..." -ForegroundColor Yellow
    New-Item -Path "Prerequisites" -ItemType Directory -Force | Out-Null
    Write-Host "? Prerequisites directory created" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host " ALL FILES READY!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "You can now run:" -ForegroundColor Yellow
Write-Host "  .\RUN-COMPLETE-SETUP.bat" -ForegroundColor Cyan
Write-Host ""
Write-Host "Or:" -ForegroundColor Yellow
Write-Host "  .\Build-Installer.ps1" -ForegroundColor Cyan
Write-Host ""
