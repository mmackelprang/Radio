@echo off
REM Radio Console Startup Script for Windows (Batch)
REM This script starts both the API and Web UI with the correct port configuration

echo ========================================
echo Radio Console Startup Script
echo ========================================
echo.

REM Configuration
set API_PORT=5100
set SWAGGER_PORT=5101
set WEB_PORT=5200
set API_URL=http://localhost:%API_PORT%

echo Starting Radio Console API on port %API_PORT%...
echo Swagger UI will be available on port %SWAGGER_PORT%
echo.

REM Start the API in a new window
start "Radio Console API" dotnet run --project RadioConsole\RadioConsole.API --port %API_PORT% --swagger-port %SWAGGER_PORT%

echo Waiting 10 seconds for API to start up...
timeout /t 10 /nobreak >nul

echo.
echo Starting Radio Console Web UI on port %WEB_PORT%...
echo Web UI will connect to API at %API_URL%
echo.

REM Start the Web UI in a new window
start "Radio Console Web UI" dotnet run --project RadioConsole\RadioConsole.Web --port %WEB_PORT% --api-url %API_URL%

echo.
echo ========================================
echo Both services are starting...
echo ========================================
echo API:     http://localhost:%API_PORT%
echo Swagger: http://localhost:%SWAGGER_PORT%/swagger
echo Web UI:  http://localhost:%WEB_PORT%
echo ========================================
echo.
echo Press any key to exit this window.
echo Note: Closing this window will NOT stop the services.
echo Close the individual service windows to stop them.
pause >nul
