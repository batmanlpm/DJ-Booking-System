@echo off
REM Quick Launcher for Complete Setup
REM Run this to start the automated setup

echo.
echo ========================================
echo   DJ BOOKING SYSTEM - AUTO SETUP
echo ========================================
echo.
echo This will automatically:
echo  - Install prerequisites
echo  - Build your application
echo  - Create fancy installer
echo  - Prepare for Hostinger upload
echo.
echo Estimated time: 15-20 minutes
echo.
pause

PowerShell.exe -ExecutionPolicy Bypass -File "COMPLETE-SETUP.ps1"

pause
