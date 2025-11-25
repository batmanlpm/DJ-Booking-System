@echo off
echo Installing Azure Functions dependencies...
cd /d "%~dp0"
npm install
if %errorlevel% neq 0 (
    echo Failed to install dependencies
    pause
    exit /b %errorlevel%
)
echo.
echo Dependencies installed successfully!
echo.
echo To start the API server, run: start-api.bat
pause
