# Radio Console - Grandpa Anderson's Digital Console

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/mmackelprang/Radio)
[![Tests](https://img.shields.io/badge/tests-110%2F119-green)](https://github.com/mmackelprang/Radio)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

A modern audio command center for Raspberry Pi 5, encased in a vintage console radio cabinet. The software restores the original function (Radio/Vinyl) while adding modern capabilities (Spotify, Streaming, Smart Home Events, and Whole-Home Audio).

## Overview

Radio Console is a comprehensive audio management system built with .NET 8 and Blazor Server, designed to run on Raspberry Pi 5. It provides a vintage-inspired interface for controlling multiple audio sources, with support for casting to Google Home devices and integration with smart home systems.

## Key Features

### 🎵 Audio Sources
- **USB Radio (Raddy RF320)**: Full control with LED displays, tuning, and band selection
- **Vinyl Turntable**: USB ADC input with software pre-amp
- **Spotify**: Complete playback control with metadata display
- **Local Files**: MP3 and other format playback with metadata extraction
- **Smart Home Events**: TTS announcements, doorbell, and Google Broadcast integration

### 🎛️ Audio Management
- **Priority Ducking**: Automatic volume adjustment when notifications play
- **Multi-Output**: Local speakers or Google Cast devices
- **Real-time Visualization**: FFT-based audio visualization (multiple modes)
- **Audio Streaming**: HTTP streaming endpoints for external clients

### 🖥️ User Interface
- **Material Design 3**: Modern, touchscreen-optimized interface
- **Ultra-wide Display**: Optimized for 12.5" x 3.75" screens
- **LED-Style Displays**: Vintage aesthetic with modern functionality
- **Touch Keyboards**: Full QWERTY and numeric keypads for input
- **Radio Control Panel**: Comprehensive USB radio interface with frequency validation

### 🔌 API & Integration
- **REST API**: 20+ endpoints for external control
- **Swagger Documentation**: Interactive API documentation
- **SignalR**: Real-time visualization data streaming
- **Configuration Management**: CRUD operations with backup/restore

### 📊 Metadata & Information
- **TagLib# Integration**: Comprehensive audio file metadata extraction
- **Now Playing**: Real-time display of current track information
- **Album Art**: Display of cover artwork from files and streaming services
- **Lyrics Support**: Display lyrics when available

## Architecture

The project follows Clean Architecture principles with clear separation of concerns:

```
RadioConsole/
├── RadioConsole.Core/          # Domain interfaces and models
├── RadioConsole.Infrastructure/ # SoundFlow, TagLib#, external APIs
├── RadioConsole.API/           # REST API controllers
├── RadioConsole.Web/           # Blazor UI components
└── RadioConsole.Tests/         # Unit and integration tests
```

### Technology Stack
- **Framework**: .NET 8 (C#)
- **UI**: Blazor Server with MudBlazor
- **Audio Engine**: [SoundFlow](https://github.com/lsxprime/SoundFlow)
- **Metadata**: TagLibSharp
- **Database**: SQLite / JSON (hot-swappable)
- **Logging**: Serilog
- **Testing**: xUnit

## Getting Started

### Prerequisites
- .NET 8 SDK or later
- Raspberry Pi 5 (or development machine for testing)
- Optional: Raddy RF320 USB radio
- Optional: USB audio interface for turntable

### Building

```bash
# Clone the repository
git clone https://github.com/mmackelprang/Radio.git
cd Radio

# Build the solution
dotnet build RadioConsole.sln

# Run tests
dotnet test RadioConsole.sln
```

### Running

#### API Server
```bash
cd RadioConsole/RadioConsole.API
dotnet run
```
Access Swagger UI at: `http://localhost:5211/swagger`

#### Web Interface
```bash
cd RadioConsole/RadioConsole.Web
dotnet run
```
Access the UI at: `http://localhost:5000`

### Demo Page

Visit `/radio-demo` in the web interface to see:
- Interactive USB Radio Control Panel
- Numeric Keypad demonstration
- Touch Keyboard demonstration
- Component documentation and usage examples

## API Endpoints

### Configuration (`/api/Configuration`)
- CRUD operations for system configuration
- Backup and restore functionality

### Now Playing (`/api/NowPlaying`)
- Get current playback information
- Radio frequency and band details
- Spotify track metadata

### Metadata (`/api/Metadata`)
- Extract metadata from audio files
- Support for 13+ audio formats (MP3, FLAC, AAC, OGG, WAV, etc.)
- Format validation

### Visualization (`/api/Visualization`)
- Enable/disable FFT generation
- List available visualization types

### Stream Audio (`/api/StreamAudio`)
- Get streaming endpoint URLs (MP3/WAV)
- Streaming service status

### System Test (`/api/Test`)
- TTS testing (male/female voices)
- Test tone generation
- Event simulation (doorbell, broadcasts)

## Components

### USB Radio Control Panel
Comprehensive control interface for Raddy RF320:
- LED-style frequency display with auto-formatting per band
- Band selector: FM, AM, SW, AIR, VHF
- Tune Up/Down with band-specific steps
- Frequency entry via numeric keypad with validation
- Independent radio volume control
- Scan functionality (stubbed)

### Touch Keyboards
Material 3 design touchscreen-optimized input:
- **NumericKeypad**: For frequencies, favorites, configuration values
- **TouchKeyboard**: Full QWERTY with shift key for uppercase and special characters

### Visualization
Real-time audio visualization with multiple modes:
- Level Meter (RMS/Peak)
- Waveform display
- Frequency spectrum

## Configuration

Configuration can be managed via:
1. **JSON files**: `appsettings.json` and `config.json`
2. **SQLite database**: Hot-swappable storage backend
3. **REST API**: Configuration controller endpoints
4. **Web UI**: Future CRUD interface (planned)

### Key Configuration Options
```json
{
  "ConfigurationStorage": {
    "RootDir": "",
    "StoragePath": "./storage",
    "StorageType": "Json",
    "JsonFileName": "config.json",
    "SqliteFileName": "config.db"
  }
}
```

## Development

### Project Structure
- **Core**: Domain interfaces, entities, business logic
- **Infrastructure**: SoundFlow implementation, database, external APIs
- **API**: REST controllers and endpoints
- **Web**: Blazor components and pages
- **Tests**: xUnit tests for all layers

### Coding Standards
- Follow C# naming conventions (PascalCase, camelCase)
- Use 2 spaces for indentation
- XML documentation on public members
- Dependency injection for all services
- SOLID principles and clean architecture

### Testing
```bash
# Run all tests
dotnet test RadioConsole.sln

# Run with coverage
dotnet test RadioConsole.sln --collect:"XPlat Code Coverage"
```

Note: 9 audio device tests will fail in CI/environments without audio hardware (expected behavior).

## Deployment

### Raspberry Pi 5
1. Install .NET 8 runtime on Raspberry Pi OS
2. Copy built binaries to the Pi
3. Configure audio devices and USB radio
4. Run in kiosk mode: `chromium-browser --kiosk http://localhost:5000`

See `RadioPlan_v3.md` for detailed deployment instructions.

## Documentation

### 📘 Essential Guides
- **[RadioPlan_v3.md](RadioPlan_v3.md)** - Complete project specification and architecture blueprint
- **[Audio Priority Service](docs/AUDIO_PRIORITY_SERVICE.md)** - Developer guide for audio ducking and priority management
- **[Configuration Service](docs/CONFIGURATION_SERVICE.md)** - Configuration management and storage guide
- **[Visualization Guide](docs/UI_VISUALIZATION_GUIDE.md)** - Audio visualization implementation details

### 📋 Active Development
- **[PHASE5_INTEGRATION_TODO.md](PHASE5_INTEGRATION_TODO.md)** - Current integration tasks and status

### 📚 Additional Documentation
See the [Documentation Index](docs/README.md) for:
- Historical phase completion summaries
- Previous plan versions
- Code review guidelines
- All project documentation organized by category

## Contributing

Contributions are welcome! Please follow these guidelines:
1. Follow existing code style and architecture
2. Add tests for new features
3. Update documentation as needed
4. Ensure all tests pass before submitting PR

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [SoundFlow](https://github.com/lsxprime/SoundFlow) - Audio engine
- [TagLibSharp](https://github.com/mono/taglib-sharp) - Metadata extraction
- [MudBlazor](https://mudblazor.com/) - Material Design components
- Raspberry Pi Foundation - Hardware platform

## Status

**Current Version**: Phase 5 - UI Integration + API Expansion  
**Build Status**: ✅ Passing (110/119 tests)  
**Security**: ✅ No CodeQL alerts  
**Last Updated**: November 2025
