@echo off
REM Quick Fix and Build Installer

echo.
echo ========================================
echo   QUICK FIX ^& BUILD INSTALLER
echo ========================================
echo.
echo This will:
echo  1. Create missing files/folders
echo  2. Check Inno Setup
echo  3. Build application (if needed)
echo  4. Create installer
echo.
pause

PowerShell.exe -ExecutionPolicy Bypass -File "QUICK-FIX-AND-BUILD.ps1"

pause
