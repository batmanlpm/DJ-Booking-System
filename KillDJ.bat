@echo off
REM Kill DJ Booking System - Simple Batch File
REM Can be run from Windows Run dialog (Win+R)

taskkill /F /IM DJBookingSystem.exe 2>nul

REM Exit silently
exit
