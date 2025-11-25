@echo off
REM Automated Build and Upload to Hostinger

echo.
echo ========================================
echo   BUILD ^& UPLOAD TO HOSTINGER
echo ========================================
echo.
echo This will:
echo  1. Build the installer
echo  2. Upload to djbookupdates.com
echo.
echo Server: 153.92.10.234
echo Path: public_html\Updates\
echo.
echo This will take 15-25 minutes total.
echo.
pause

PowerShell.exe -ExecutionPolicy Bypass -File "BUILD-AND-UPLOAD.ps1"

pause
