# QUICK-FIX-AND-BUILD.ps1
# Fixes missing files and builds installer quickly

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "?????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?   QUICK FIX & BUILD INSTALLER             ?" -ForegroundColor Cyan
Write-Host "?????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Step 1: Create missing files
Write-Host "Step 1: Checking required files..." -ForegroundColor Yellow

if (-not (Test-Path "Assets")) {
    New-Item -Path "Assets" -ItemType Directory -Force | Out-Null
    Write-Host "  ? Created Assets directory" -ForegroundColor Green
}

if (-not (Test-Path "Prerequisites")) {
    New-Item -Path "Prerequisites" -ItemType Directory -Force | Out-Null
    Write-Host "  ? Created Prerequisites directory" -ForegroundColor Green
}

if (-not (Test-Path "Installer\Output")) {
    New-Item -Path "Installer\Output" -ItemType Directory -Force | Out-Null
    Write-Host "  ? Created Installer\Output directory" -ForegroundColor Green
}

Write-Host "  ? All directories ready" -ForegroundColor Green

# Step 2: Check Inno Setup
Write-Host "`nStep 2: Checking Inno Setup..." -ForegroundColor Yellow
$InnoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

if (-not (Test-Path $InnoSetupPath)) {
    Write-Host "  ? Inno Setup not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install Inno Setup from:" -ForegroundColor Yellow
    Write-Host "  https://jrsoftware.org/isdl.php" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Then run this script again." -ForegroundColor Yellow
    exit 1
}
Write-Host "  ? Inno Setup found" -ForegroundColor Green

# Step 3: Check if publish exists
Write-Host "`nStep 3: Checking published files..." -ForegroundColor Yellow
$PublishDir = "bin\Release\net8.0-windows\win-x64\publish"

if (-not (Test-Path $PublishDir)) {
    Write-Host "  ! Published files not found, building now..." -ForegroundColor Yellow
    Write-Host ""
    
    dotnet publish -c Release -r win-x64 --self-contained true `
        -p:PublishSingleFile=false `
        -p:PublishTrimmed=false `
        --verbosity quiet
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ? Build failed!" -ForegroundColor Red
        exit 1
    }
    Write-Host "  ? Build complete" -ForegroundColor Green
} else {
    $fileCount = (Get-ChildItem -Path $PublishDir -Recurse -File).Count
    Write-Host "  ? Published files found ($fileCount files)" -ForegroundColor Green
}

# Step 4: Build installer
Write-Host "`nStep 4: Building installer..." -ForegroundColor Yellow

try {
    & $InnoSetupPath "installer.iss" 2>&1 | Out-Null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ? Installer created!" -ForegroundColor Green
        
        $installer = Get-ChildItem -Path "Installer\Output" -Filter "*.exe" | Select-Object -First 1
        if ($installer) {
            $size = [math]::Round($installer.Length / 1MB, 2)
            Write-Host ""
            Write-Host "?????????????????????????????????????????????" -ForegroundColor Green
            Write-Host "?          BUILD SUCCESSFUL!                ?" -ForegroundColor Green
            Write-Host "?????????????????????????????????????????????" -ForegroundColor Green
            Write-Host ""
            Write-Host "Installer: $($installer.Name)" -ForegroundColor Cyan
            Write-Host "Size: $size MB" -ForegroundColor Cyan
            Write-Host "Path: $($installer.FullName)" -ForegroundColor Gray
            Write-Host ""
            Write-Host "Next Steps:" -ForegroundColor Yellow
            Write-Host "1. Upload to Hostinger" -ForegroundColor White
            Write-Host "2. Follow HOSTINGER_UPLOAD_INSTRUCTIONS.txt" -ForegroundColor White
            Write-Host ""
        }
    } else {
        Write-Host "  ? Installer build failed" -ForegroundColor Red
        Write-Host ""
        Write-Host "Check installer.iss for errors" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  ? Error: $_" -ForegroundColor Red
    exit 1
}

Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
