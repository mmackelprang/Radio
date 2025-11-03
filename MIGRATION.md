# Migration from .NET MAUI to ASP.NET Core + React

## Overview

This project has been migrated from a .NET MAUI application to a modern web-based architecture using:
- **Backend**: ASP.NET Core 9.0 Web API
- **Frontend**: React 18 + TypeScript with Material-UI v5 (Material Design 3)

## Old MAUI Code

The original .NET MAUI code is preserved in `src/RadioConsole/` for reference. This includes:
- XAML views and ViewModels
- MAUI-specific platform implementations
- Original app structure

## New Architecture

The new implementation is located in:
- **Backend API**: `src/RadioConsole.Api/` - ASP.NET Core Web API with RESTful endpoints
- **Frontend Web**: `src/RadioConsole.Web/` - React + TypeScript SPA with Material-UI

## Key Improvements

1. **Web-Based**: Accessible via any modern web browser
2. **Material Design 3**: Modern, touch-friendly UI with dark/light mode support
3. **Better Separation**: Clear separation between backend API and frontend
4. **Real-time Ready**: SignalR infrastructure for live updates
5. **Cross-Platform Development**: Easier development on any platform

## Running the New Application

### Backend (API)
```bash
cd src/RadioConsole.Api
dotnet run
```
API will be available at http://localhost:5000

### Frontend (Web)
```bash
cd src/RadioConsole.Web
npm install  # First time only
npm start
```
Web interface will be available at http://localhost:3000

## Migration Notes

- All core interfaces (IAudioInput, IAudioOutput, etc.) have been preserved
- Audio modules (Radio, Spotify, Chromecast, etc.) work in both implementations
- The web-based approach is better suited for the Raspberry Pi touchscreen use case
- Dark/light mode theming is now easily switchable via UI toggle

## Future Work

The old MAUI code in `src/RadioConsole/` can be removed once the new web-based implementation is fully validated and deployed.
