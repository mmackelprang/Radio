# Radio Console - Modern Raspberry Pi 5 Audio System

A modern audio system built with C# .NET MAUI for Raspberry Pi 5, designed to bring new life to vintage console radios. This project provides a flexible, extensible architecture for managing multiple audio inputs and outputs with a rich touchscreen interface.

## 🎯 Project Goals

The Radio Console project aims to create a comprehensive audio management system with the following features:

### Audio Inputs
- **SW/AM/FM Radio** - Integration with Raddy RF320 radio receiver
- **Vinyl Turntable** - Analog to digital conversion via USB sound card
- **Network Streaming**
  - Spotify integration with playback controls
  - MP3 files from local network shares
- **Smart Home Integration**
  - Wyze doorbell audio notifications
  - Google broadcast receiver
- **Bluetooth** - Pair and receive audio from Bluetooth devices

### Audio Outputs
- **Wired Soundbar** - Direct wired connection
- **Chromecast** - Network streaming to Chromecast devices
- **Bluetooth Speaker** - Wireless output to Bluetooth speakers
- **Output Switching** - Seamless switching between output devices

### User Interface
- **Material Design 3** - Modern, touch-friendly interface
- **Audio Controls** - Source selection, playback controls, volume management
- **History** - Track recently played content
- **Favorites** - Save and quick-access favorite stations and playlists
- **Metadata Display** - Rich information about currently playing audio
- **Configuration** - Per-device settings and preferences

### Technical Features
- **Modular Architecture** - Clean interfaces for extensibility
- **Simulation Mode** - Develop and test on non-Raspberry Pi environments
- **State Management** - Persistent storage of settings, history, and favorites
- **Cross-Platform** - Built with .NET MAUI for future platform support

## 🏗️ Architecture

The project follows a modular architecture with clear separation of concerns:

### Core Interfaces
- **IAudioInput** - Base interface for all audio input sources
- **IAudioOutput** - Base interface for all audio output devices
- **IDisplay** - Interface for metadata and status display
- **IConfiguration** - Interface for device configuration
- **IStorage** - Interface for data persistence

### Modules
Each input and output is implemented as a separate module that inherits from the base interfaces:
- **Inputs**: `RadioInput`, `SpotifyInput`, etc.
- **Outputs**: `WiredSoundbarOutput`, `ChromecastOutput`, etc.

### Services
- **EnvironmentService** - Detects runtime environment and enables simulation mode
- **JsonStorageService** - JSON-based persistent storage

## 📋 Development Phases

### Phase 1: Requirements & Architecture Design ✅
- [x] Define detailed requirements for each input and output
- [x] Decide on overall software architecture patterns (MVVM)
- [x] Define input/output interfaces and data models
- [x] Choose datastore technology (JSON-based storage)
- [x] Setup repository structure

### Phase 2: Basic Raspberry Pi Setup and Project Initialization ✅
- [x] Initialize C# .NET MAUI project structure
- [x] Develop simulation mode for non-RPi environment
- [x] Define project skeleton with interfaces and modules
- [x] Create base implementations for inputs and outputs
- [x] Setup Material Design 3 UI shell with navigation

### Phase 3: Core Audio Input and Output Interfaces 🚧
- [x] Implement abstractions for audio inputs and outputs
- [x] Create base module implementations with simulation support
- [x] Setup Radio input module (Raddy RF320) - *Simulation mode ready*
- [x] Setup Spotify input module - *Simulation mode ready*
- [x] Setup Wired Soundbar output - *Simulation mode ready*
- [x] Setup Chromecast output - *Simulation mode ready*
- [ ] Integrate actual hardware for Raddy RF320 radio
- [ ] Implement vinyl turntable input interface
- [ ] Add support for MP3 playback from network shares

### Phase 4: Advanced Input Sources and Event-driven Audio 📝
- [ ] Integrate Wyze doorbell event-driven input
- [ ] Integrate Google broadcast receiver for audio
- [ ] Implement full Spotify streaming integration
- [ ] Add Bluetooth audio input support and pairing UI

### Phase 5: Audio Output Devices 📝
- [ ] Complete wired soundbar integration
- [ ] Finalize Chromecast output streaming
- [ ] Add external Bluetooth speaker output
- [ ] Implement output switching mechanisms

### Phase 6: User Interface Development 🚧
- [x] Develop basic touchscreen UI shell with navigation
- [x] Create placeholder views for audio control, history, and favorites
- [x] Implement source selection interface
- [x] Add basic playback controls
- [ ] Enhance metadata display with real-time updates
- [ ] Add configuration sections per input/output
- [ ] Implement individual player controls (radio tuning, Spotify controls)
- [ ] Apply final UI styling for console radio display

### Phase 7: State Management and Persistency 📝
- [x] Setup JSON-based storage infrastructure
- [ ] Implement saving/loading of favorites
- [ ] Implement history tracking
- [ ] Add migration and backup capabilities

### Phase 8: Testing, Simulation, and Deployment 📝
- [ ] Conduct unit tests for core modules
- [ ] Integration tests for input/output modules
- [ ] Test simulation mode on development machines
- [ ] Setup automated deployment for Raspberry Pi
- [ ] Performance testing and optimization

### Phase 9: Documentation and Final Adjustments 📝
- [x] Create comprehensive README
- [ ] Write developer documentation
- [ ] Record setup and usage instructions
- [ ] Create troubleshooting guide
- [ ] Perform final polishing and bug fixes

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK or later
- For Raspberry Pi deployment:
  - Raspberry Pi 5 with Raspberry Pi OS
  - .NET runtime installed
  - Touchscreen display
  - Audio hardware (soundbar, radio receiver, etc.)

### Development Setup (Simulation Mode)

1. **Clone the repository**
   ```bash
   git clone https://github.com/mmackelprang/Radio.git
   cd Radio
   ```

2. **Build the project**
   ```bash
   cd src/RadioConsole
   dotnet build
   ```

3. **Run in simulation mode**
   ```bash
   dotnet run
   ```

   The application will automatically detect that it's not running on a Raspberry Pi and enable simulation mode, allowing you to develop and test on any platform.

### Raspberry Pi Deployment

Deployment instructions will be added in Phase 8.

## 🛠️ Technology Stack

- **Framework**: .NET 9.0 / .NET MAUI
- **Language**: C# 12.0
- **UI Framework**: .NET MAUI with Material Design 3
- **MVVM**: CommunityToolkit.Mvvm
- **Storage**: JSON-based file storage
- **Target Platform**: Raspberry Pi 5 (Linux ARM64)
- **Development**: Cross-platform (Windows, macOS, Linux)

## 📂 Project Structure

```
Radio/
├── src/
│   └── RadioConsole/              # Main MAUI application
│       ├── Interfaces/            # Core interfaces
│       │   ├── IAudioInput.cs
│       │   ├── IAudioOutput.cs
│       │   ├── IDisplay.cs
│       │   ├── IConfiguration.cs
│       │   └── IStorage.cs
│       ├── Modules/
│       │   ├── Inputs/           # Audio input implementations
│       │   │   ├── BaseAudioInput.cs
│       │   │   ├── RadioInput.cs
│       │   │   └── SpotifyInput.cs
│       │   └── Outputs/          # Audio output implementations
│       │       ├── BaseAudioOutput.cs
│       │       ├── WiredSoundbarOutput.cs
│       │       └── ChromecastOutput.cs
│       ├── Services/             # Core services
│       │   ├── EnvironmentService.cs
│       │   └── JsonStorageService.cs
│       ├── ViewModels/           # MVVM view models
│       ├── Views/                # XAML views
│       ├── Converters/           # Value converters
│       └── Resources/            # Images, styles, fonts
├── PROJECT_PLAN.md               # Detailed project plan
├── README.md                     # This file
└── LICENSE                       # Project license
```

## 🤝 Contributing

This is a personal project, but suggestions and feedback are welcome! Please open an issue to discuss any changes you'd like to propose.

## 📄 License

See the [LICENSE](LICENSE) file for details.

## 🎵 Current Status

**Project Status**: Phase 2 Complete - Initial project structure established

The project has completed the initial setup with:
- ✅ Full project structure with modular architecture
- ✅ Core interfaces defined (IAudioInput, IAudioOutput, IDisplay, IConfiguration, IStorage)
- ✅ Simulation mode support for cross-platform development
- ✅ Base modules for Radio and Spotify inputs
- ✅ Base modules for Wired Soundbar and Chromecast outputs
- ✅ Material Design 3 UI shell with navigation
- ✅ Placeholder views for Audio Control, History, and Favorites

**Next Steps**: Phase 3 - Integrate actual hardware and implement real audio streaming capabilities.

## 📞 Contact

For questions or suggestions, please open an issue on GitHub.

---

*Building the future of vintage console radios, one commit at a time.* 🎵
