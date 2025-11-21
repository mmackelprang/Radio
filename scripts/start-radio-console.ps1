#!/usr/bin/env pwsh
# Radio Console Startup Script for Windows (PowerShell)
# This script starts both the API and Web UI with the correct port configuration

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Radio Console Startup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$apiPort = 5100
$swaggerPort = 5101
$webPort = 5200
$apiUrl = "http://localhost:$apiPort"

Write-Host "Starting Radio Console API on port $apiPort..." -ForegroundColor Green
Write-Host "Swagger UI will be available on port $swaggerPort" -ForegroundColor Gray
Write-Host ""

# Start the API in a new window
$apiProcess = Start-Process -FilePath "dotnet" `
  -ArgumentList "run", "--project", "RadioConsole\RadioConsole.API", "--port", $apiPort, "--swagger-port", $swaggerPort `
  -PassThru `
  -WindowStyle Normal

Write-Host "API process started (PID: $($apiProcess.Id))" -ForegroundColor Gray
Write-Host "Waiting 10 seconds for API to start up..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

Write-Host ""
Write-Host "Starting Radio Console Web UI on port $webPort..." -ForegroundColor Green
Write-Host "Web UI will connect to API at $apiUrl" -ForegroundColor Gray
Write-Host ""

# Start the Web UI in a new window
$webProcess = Start-Process -FilePath "dotnet" `
  -ArgumentList "run", "--project", "RadioConsole\RadioConsole.Web", "--port", $webPort, "--api-url", $apiUrl `
  -PassThru `
  -WindowStyle Normal

Write-Host "Web UI process started (PID: $($webProcess.Id))" -ForegroundColor Gray
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Both services are starting..." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "API:     http://localhost:$apiPort" -ForegroundColor White
Write-Host "Swagger: http://localhost:$swaggerPort/swagger" -ForegroundColor White
Write-Host "Web UI:  http://localhost:$webPort" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Process IDs:" -ForegroundColor Yellow
Write-Host "  API:    $($apiProcess.Id)" -ForegroundColor Gray
Write-Host "  Web UI: $($webProcess.Id)" -ForegroundColor Gray
Write-Host ""
Write-Host "To stop the services, use these commands:" -ForegroundColor Yellow
Write-Host "  Stop-Process -Id $($apiProcess.Id)" -ForegroundColor Gray
Write-Host "  Stop-Process -Id $($webProcess.Id)" -ForegroundColor Gray
Write-Host ""
Write-Host "Press Enter to exit this script..." -ForegroundColor Yellow
Write-Host "(Note: Services will continue running after you exit)" -ForegroundColor Gray
Read-Host
