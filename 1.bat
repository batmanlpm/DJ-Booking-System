@echo off
REM Kill DJ Booking System with confirmation
taskkill /F /IM DJBookingSystem.exe
if %errorlevel%==0 (
    echo DJ Booking System terminated successfully!
) else (
    echo DJ Booking System is not running.
)
timeout /t 2 >nul
exit
