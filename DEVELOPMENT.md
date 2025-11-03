# Development Guide

## Getting Started with Development

This guide will help you set up your development environment and start working on the Radio Console project.

## Prerequisites

### Required Software
1. **.NET 9.0 SDK** - Download from https://dotnet.microsoft.com/download
2. **Node.js 20.x LTS** - Download from https://nodejs.org/
3. **npm** - Comes with Node.js installation
4. **Git** - For version control
5. **IDE** - Choose one:
   - Visual Studio 2022 (Windows/Mac) - Recommended for ASP.NET Core development
   - Visual Studio Code with C# and ESLint extensions
   - JetBrains Rider

## Setting Up the Project

### Clone the Repository

```bash
git clone https://github.com/mmackelprang/Radio.git
cd Radio
```

### Restore Dependencies

**Backend:**
```bash
cd src/RadioConsole.Api
dotnet restore
```

**Frontend:**
```bash
cd src/RadioConsole.Web
npm install
```

## Running the Application

### Development Mode (Simulation)

The application requires both backend and frontend to be running.

**Terminal 1 - Start the backend API:**
```bash
cd src/RadioConsole.Api
dotnet run
```

The API will start on:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

**Terminal 2 - Start the frontend:**
```bash
cd src/RadioConsole.Web
npm start
```

The web interface will open automatically at http://localhost:3000

The application automatically detects when it's not running on a Raspberry Pi and enables simulation mode, allowing you to develop and test on any platform.

### Production Build

**Backend:**
```bash
cd src/RadioConsole.Api
dotnet publish -c Release -r linux-arm64 --self-contained
```

**Frontend:**
```bash
cd src/RadioConsole.Web
npm run build
```

The built frontend will be in `build/` directory and can be served by the backend or a web server.

## Project Structure Navigation

### Key Directories

```
Radio/
├── src/
│   ├── RadioConsole.Api/     # Backend ASP.NET Core API
│   │   ├── Controllers/      # API endpoints
│   │   ├── Interfaces/       # Core interfaces - start here
│   │   ├── Services/         # Core services (storage, environment)
│   │   ├── Modules/
│   │   │   ├── Inputs/       # Audio input implementations
│   │   │   └── Outputs/      # Audio output implementations
│   │   ├── Hubs/             # SignalR hubs for real-time
│   │   └── Models/           # Data models and DTOs
│   └── RadioConsole.Web/     # Frontend React app
│       ├── src/
│       │   ├── components/   # React components
│       │   ├── contexts/     # React contexts
│       │   ├── services/     # API client services
│       │   ├── theme/        # Material-UI theme
│       │   └── hooks/        # Custom React hooks
│       └── public/           # Static assets
```

## Development Workflow

### Making Changes

1. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**
   - Follow existing code patterns
   - Use the base classes when adding new modules
   - Ensure simulation mode works

3. **Test your changes**
   
   Backend:
   ```bash
   cd src/RadioConsole.Api
   dotnet build
   dotnet run
   ```
   
   Frontend:
   ```bash
   cd src/RadioConsole.Web
   npm run lint
   npm start
   ```

4. **Commit and push**
   ```bash
   git add .
   git commit -m "Description of changes"
   git push origin feature/your-feature-name
   ```

### Adding a New Audio Input

1. **Create a new class** in `src/RadioConsole.Api/Modules/Inputs/`
   ```csharp
   public class MyInput : BaseAudioInput
   {
       public override string Id => "my_input";
       public override string Name => "My Input";
       // ... implement abstract methods
   }
   ```

2. **Register in DI container** - Add to `Program.cs`:
   ```csharp
   builder.Services.AddSingleton<IAudioInput, MyInput>();
   ```

3. **Test in simulation mode** - The new input will automatically appear in the API response

### Adding a New Audio Output

Follow the same pattern as inputs, but inherit from `BaseAudioOutput` and register in DI.

### Adding a New UI Component

1. **Create React component** in `src/RadioConsole.Web/src/components/`
   ```tsx
   import React from 'react';
   import { Box, Typography } from '@mui/material';
   
   export const MyComponent: React.FC = () => {
     return (
       <Box>
         <Typography variant="h6">My Component</Typography>
       </Box>
     );
   };
   ```

2. **Add route** in `App.tsx`:
   ```tsx
   <Route path="/mycomponent" element={<MyComponent />} />
   ```

3. **Add navigation** to the drawer/app bar

### Adding a New API Endpoint

1. **Create or update controller** in `src/RadioConsole.Api/Controllers/`
   ```csharp
   [ApiController]
   [Route("api/[controller]")]
   public class MyController : ControllerBase
   {
       [HttpGet]
       public IActionResult Get()
       {
           return Ok(new { message = "Hello" });
       }
   }
   ```

2. **Update frontend service** to call the new endpoint

## Debugging

### Backend (ASP.NET Core)

**Visual Studio:**
1. Set RadioConsole.Api as startup project
2. Press F5 to debug

**VS Code:**
1. Open `src/RadioConsole.Api` folder
2. Press F5 (uses launch.json configuration)
3. Set breakpoints in C# code

**Console Debugging:**
```csharp
Console.WriteLine($"Input initialized: {Name}");
// or
_logger.LogInformation("Input initialized: {Name}", Name);
```

### Frontend (React)

**Browser DevTools:**
1. Open Chrome/Firefox DevTools (F12)
2. Use Console tab for logging
3. Use React DevTools extension for component inspection

**VS Code:**
1. Install Debugger for Chrome extension
2. Set breakpoints in TypeScript code
3. Press F5 to start debugging

**Console Debugging:**
```typescript
console.log('Audio started:', audioInput);
// or
console.error('Failed to start audio:', error);
```

## Working with Simulation Mode

### Understanding Simulation Mode

The `EnvironmentService` detects the platform:
- On Raspberry Pi: `IsRaspberryPi = true`
- On other platforms: `IsSimulationMode = true`

### Adding Simulation Support to Modules

```csharp
public override async Task InitializeAsync()
{
    if (_environmentService.IsSimulationMode)
    {
        // Provide mock/simulated behavior
        IsAvailable = true;
        _display.UpdateStatus("Simulated");
    }
    else
    {
        // Real hardware detection
        IsAvailable = await DetectHardware();
    }
}
```

### Testing Hardware-Specific Code

Without a Raspberry Pi, you can:
1. Use simulation mode to test API and UI logic
2. Mock hardware responses in backend
3. Test on actual hardware periodically

## Code Style Guidelines

### Backend (C#)

#### Naming Conventions
- **Classes**: PascalCase (`AudioController`)
- **Methods**: PascalCase (`InitializeAsync`)
- **Private fields**: _camelCase (`_storage`)
- **Properties**: PascalCase (`IsAvailable`)
- **Interfaces**: IPascalCase (`IAudioInput`)

#### Async Patterns
- Always use `async`/`await` for I/O operations
- Suffix async methods with `Async`
- Return `Task` or `Task<T>`

### Frontend (TypeScript/React)

#### Naming Conventions
- **Components**: PascalCase (`AudioControl.tsx`)
- **Functions**: camelCase (`handleStart`)
- **Constants**: UPPER_SNAKE_CASE (`API_BASE_URL`)
- **Interfaces/Types**: PascalCase (`AudioInput`)

#### React Patterns
- Use functional components with hooks
- Use TypeScript for type safety
- Extract custom hooks for reusable logic
- Keep components small and focused

### Documentation
- Add XML comments to public C# interfaces and classes
- Add JSDoc comments to exported TypeScript functions
- Comment complex algorithms
- Keep comments up to date

## Common Development Tasks

### Updating NuGet Packages (Backend)

```bash
cd src/RadioConsole.Api
dotnet list package --outdated
dotnet add package PackageName --version x.x.x
```

### Updating npm Packages (Frontend)

```bash
cd src/RadioConsole.Web
npm outdated
npm install package-name@latest
```

### Cleaning Build Artifacts

**Backend:**
```bash
cd src/RadioConsole.Api
dotnet clean
rm -rf bin obj
```

**Frontend:**
```bash
cd src/RadioConsole.Web
rm -rf node_modules build
npm install
```

### Viewing Build Output

**Backend:**
```bash
dotnet build -v detailed
```

**Frontend:**
```bash
npm run build -- --verbose
```

## Testing on Raspberry Pi

### Deploying to Raspberry Pi

1. **Build backend for Linux ARM64:**
   ```bash
   cd src/RadioConsole.Api
   dotnet publish -c Release -r linux-arm64 --self-contained -o ./publish
   ```

2. **Build frontend:**
   ```bash
   cd src/RadioConsole.Web
   npm run build
   ```

3. **Copy to Raspberry Pi:**
   ```bash
   scp -r src/RadioConsole.Api/publish/ pi@raspberrypi:/home/pi/RadioConsole/api/
   scp -r src/RadioConsole.Web/build/ pi@raspberrypi:/home/pi/RadioConsole/web/
   ```

4. **Run on Raspberry Pi:**
   ```bash
   ssh pi@raspberrypi
   cd /home/pi/RadioConsole/api
   ./RadioConsole.Api
   ```

5. **Access the web interface:**
   - Open Chromium browser on Raspberry Pi
   - Navigate to http://localhost:5000
   - Or serve the built frontend with nginx/Apache

### Setting Up Auto-start

Create a systemd service file `/etc/systemd/system/radioconsole.service`:

```ini
[Unit]
Description=Radio Console API
After=network.target

[Service]
Type=simple
User=pi
WorkingDirectory=/home/pi/RadioConsole/api
ExecStart=/home/pi/RadioConsole/api/RadioConsole.Api
Restart=on-failure

[Install]
WantedBy=multi-user.target
```

Enable and start:
```bash
sudo systemctl enable radioconsole
sudo systemctl start radioconsole
```

## Troubleshooting

### Backend Issues

**Build Errors:**
1. Clean and rebuild:
   ```bash
   dotnet clean
   dotnet restore
   dotnet build
   ```

2. Check .NET version: `dotnet --version`
3. Ensure all NuGet packages are restored

**Runtime Errors:**
1. Check logs in console output
2. Verify simulation mode is working
3. Check storage permissions
4. Validate module initialization

### Frontend Issues

**Build Errors:**
1. Delete node_modules and reinstall:
   ```bash
   rm -rf node_modules package-lock.json
   npm install
   ```

2. Check Node.js version: `node --version`
3. Clear npm cache: `npm cache clean --force`

**Runtime Errors:**
1. Check browser console for errors
2. Verify API is running and accessible
3. Check CORS configuration
4. Verify WebSocket connection

## Resources

### Backend Development
- ASP.NET Core Documentation: https://learn.microsoft.com/aspnet/core/
- SignalR Documentation: https://learn.microsoft.com/aspnet/core/signalr/

### Frontend Development
- React Documentation: https://react.dev/
- TypeScript Documentation: https://www.typescriptlang.org/docs/
- Material-UI Documentation: https://mui.com/
- React Router: https://reactrouter.com/

### Material Design 3
- Guidelines: https://m3.material.io/
- MUI Theming: https://mui.com/material-ui/customization/theming/

### Raspberry Pi
- Documentation: https://www.raspberrypi.org/documentation/

## Getting Help

- Open an issue on GitHub
- Review existing documentation (README.md, ARCHITECTURE.md)
- Check the PROJECT_PLAN.md for feature roadmap

Happy coding! 🎵
