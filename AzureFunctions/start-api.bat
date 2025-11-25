@echo off
echo Starting Azure Functions API...
cd /d "%~dp0"
echo.
echo API will be available at: http://localhost:7071/api
echo.
echo Press Ctrl+C to stop the server
echo.
func start
