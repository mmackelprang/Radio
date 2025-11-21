# Radio Console Startup Scripts

This directory contains scripts to start both the Radio Console API and Web UI with proper port configuration.

## Available Scripts

### Windows Batch File (`start-radio-console.bat`)
For Windows Command Prompt users.

**Usage:**
```cmd
cd path\to\Radio
scripts\start-radio-console.bat
```

This will:
- Start the API on port 5100 with Swagger on port 5101 in a new window
- Wait 10 seconds for the API to initialize
- Start the Web UI on port 5200 in a new window

### PowerShell Script (`start-radio-console.ps1`)
For Windows PowerShell users.

**Usage:**
```powershell
cd path\to\Radio
.\scripts\start-radio-console.ps1
```

This will:
- Start the API on port 5100 with Swagger on port 5101
- Wait 10 seconds for the API to initialize
- Start the Web UI on port 5200
- Display process IDs for easy management
- Provide commands to stop the services

### Bash Script (`start-radio-console.sh`)
For Linux and macOS users.

**Usage:**
```bash
cd /path/to/Radio
./scripts/start-radio-console.sh
```

This will:
- Start the API on port 5100 with Swagger on port 5101 in the background
- Wait 10 seconds for the API to initialize
- Start the Web UI on port 5200 in the background
- Display process IDs for easy management
- Trap Ctrl+C to stop both services gracefully

## Default Ports

- **API**: 5100
- **Swagger UI**: 5101 (http://localhost:5101/swagger)
- **Web UI**: 5200

## Customizing Ports

If you need to use different ports, you can modify the configuration variables at the top of each script:

### Batch File:
```cmd
set API_PORT=5100
set SWAGGER_PORT=5101
set WEB_PORT=5200
```

### PowerShell:
```powershell
$apiPort = 5100
$swaggerPort = 5101
$webPort = 5200
```

### Bash:
```bash
API_PORT=5100
SWAGGER_PORT=5101
WEB_PORT=5200
```

## Command Line Arguments

Both the API and Web UI support command line arguments for flexible configuration:

### API Arguments:
- `--port <port>` or `--listening-port <port>`: Set the API listening port
- `--swagger-port <port>`: Set the Swagger UI port (defaults to API port + 1)

### Web UI Arguments:
- `--port <port>` or `--listening-port <port>`: Set the Web UI listening port
- `--api-url <url>` or `--api-server <url>`: Set the API base URL

### Configuration Priority:
1. **Command line arguments** (highest priority)
2. **Configuration file** (appsettings.json)
3. **Hardcoded defaults** (lowest priority)

## Manual Startup

If you prefer to start the services manually:

### Start API:
```bash
dotnet run --project RadioConsole/RadioConsole.API --port 5100 --swagger-port 5101
```

### Start Web UI:
```bash
dotnet run --project RadioConsole/RadioConsole.Web --port 5200 --api-url http://localhost:5100
```

## Troubleshooting

### Port Already in Use
If you see an error about a port already being in use:
- Check what's using the port: `lsof -i :5100` (Linux/macOS) or `netstat -ano | findstr :5100` (Windows)
- Kill the process or use different ports

### API Health Check Failed
If the Web UI fails to start with "API Health Check Failed":
1. Ensure the API is running first
2. Check that the API is accessible at the configured URL
3. Verify firewall settings aren't blocking the connection
4. Use the startup scripts which handle the timing automatically

### Services Won't Stop
- **Windows**: Close the individual command prompt windows or use Task Manager
- **PowerShell**: Use the `Stop-Process -Id <PID>` commands shown when starting
- **Linux/macOS**: Press Ctrl+C in the terminal or use `kill <PID>`
