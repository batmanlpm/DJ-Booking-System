@echo off
REM Simple Upload to Hostinger

echo.
echo ========================================
echo   SIMPLE UPLOAD TO HOSTINGER
echo ========================================
echo.
echo This will upload the installer to:
echo   Server: 153.92.10.234
echo   Path: /public_html/Updates/
echo.
echo Time: 10-20 minutes
echo.
pause

PowerShell.exe -ExecutionPolicy Bypass -File "SIMPLE-UPLOAD.ps1"

pause
