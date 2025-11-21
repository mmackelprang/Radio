#!/bin/bash
# Radio Console Startup Script for Linux/macOS (Bash)
# This script starts both the API and Web UI with the correct port configuration

echo "========================================"
echo "Radio Console Startup Script"
echo "========================================"
echo ""

# Configuration
API_PORT=5100
SWAGGER_PORT=5101
WEB_PORT=5200
API_URL="http://localhost:${API_PORT}"

# Color codes for terminal output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

echo -e "${GREEN}Starting Radio Console API on port ${API_PORT}...${NC}"
echo -e "${GRAY}Swagger UI will be available on port ${SWAGGER_PORT}${NC}"
echo ""

# Start the API in the background
dotnet run --project RadioConsole/RadioConsole.API --port ${API_PORT} --swagger-port ${SWAGGER_PORT} &
API_PID=$!

echo -e "${GRAY}API process started (PID: ${API_PID})${NC}"
echo -e "${YELLOW}Waiting 10 seconds for API to start up...${NC}"
sleep 10

echo ""
echo -e "${GREEN}Starting Radio Console Web UI on port ${WEB_PORT}...${NC}"
echo -e "${GRAY}Web UI will connect to API at ${API_URL}${NC}"
echo ""

# Start the Web UI in the background
dotnet run --project RadioConsole/RadioConsole.Web --port ${WEB_PORT} --api-url ${API_URL} &
WEB_PID=$!

echo -e "${GRAY}Web UI process started (PID: ${WEB_PID})${NC}"
echo ""

echo -e "${CYAN}========================================${NC}"
echo -e "${GREEN}Both services are starting...${NC}"
echo -e "${CYAN}========================================${NC}"
echo "API:     http://localhost:${API_PORT}"
echo "Swagger: http://localhost:${SWAGGER_PORT}/swagger"
echo "Web UI:  http://localhost:${WEB_PORT}"
echo -e "${CYAN}========================================${NC}"
echo ""
echo -e "${YELLOW}Process IDs:${NC}"
echo -e "${GRAY}  API:    ${API_PID}${NC}"
echo -e "${GRAY}  Web UI: ${WEB_PID}${NC}"
echo ""
echo -e "${YELLOW}To stop the services, use these commands:${NC}"
echo -e "${GRAY}  kill ${API_PID}${NC}"
echo -e "${GRAY}  kill ${WEB_PID}${NC}"
echo ""
echo -e "${YELLOW}Or press Ctrl+C to stop both services${NC}"
echo ""

# Function to handle script termination
cleanup() {
  echo ""
  echo -e "${YELLOW}Stopping services...${NC}"
  
  # Try graceful shutdown first
  kill -TERM ${API_PID} 2>/dev/null
  kill -TERM ${WEB_PID} 2>/dev/null
  
  # Wait up to 5 seconds for graceful shutdown
  for i in {1..5}; do
    sleep 1
    # Check if processes are still running
    if ! kill -0 ${API_PID} 2>/dev/null && ! kill -0 ${WEB_PID} 2>/dev/null; then
      break
    fi
  done
  
  # Force kill if still running
  kill -KILL ${API_PID} 2>/dev/null
  kill -KILL ${WEB_PID} 2>/dev/null
  
  echo -e "${GREEN}Services stopped${NC}"
  exit 0
}

# Trap Ctrl+C and call cleanup
trap cleanup INT TERM

# Wait for both processes
wait
