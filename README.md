# Radio Console - Modern Raspberry Pi 5 Audio System

A modern audio system built with ASP.NET Core and Blazor for Raspberry Pi 5, designed to bring new life to vintage console radios. This project provides a flexible, extensible architecture for managing multiple audio inputs and outputs with a rich touchscreen web interface.

## 🎯 Project Goals

The Radio Console project aims to create a comprehensive audio management system with the following features:

### Audio Inputs

#### Music Inputs
- **SW/AM/FM Radio** - Integration with Raddy RF320 radio receiver
- **Vinyl Turntable** - Analog to digital conversion via USB sound card
- **Network Streaming**
  - Spotify integration with playback controls
  - MP3 files from local network shares
- **Bluetooth** - Pair and receive audio from Bluetooth devices

#### Event-Driven Inputs (High Priority Audio)
- **Doorbell Ring** - Audio notifications from smart doorbells (e.g., Wyze)
- **Telephone Ring** - Phone ringing notifications
- **Google Broadcast** - Google Home broadcast message receiver
- **Timer Expired** - Kitchen timer and other timer notifications
- **Reminders** - Calendar and scheduled reminder notifications
- **Priority Management** - Automatic volume reduction/restoration during events

### Audio Outputs
- **Wired Soundbar** - Direct wired connection
- **Chromecast** - Network streaming to Chromecast devices
- **Bluetooth Speaker** - Wireless output to Bluetooth speakers
- **Output Switching** - Seamless switching between output devices

### User Interface
- **Material Design 3** - Modern, touch-friendly Blazor interface with MudBlazor
- **Dark/Light Mode** - Easily swappable theme for different lighting conditions
- **Audio Controls** - Source selection, playback controls, volume management
- **Device Management** - Add, edit, and remove audio input/output devices through the UI
- **Event Notifications** - High-priority audio notifications with automatic volume management
- **History** - Track recently played content
- **Favorites** - Save and quick-access favorite stations and playlists
- **Metadata Display** - Rich information about currently playing audio
- **Configuration** - Per-device settings and preferences
- **Responsive Design** - Optimized for touchscreen displays

### Technical Features
- **Modular Architecture** - Clean separation between backend API and frontend
- **RESTful API** - ASP.NET Core Web API for audio control and management
- **Dynamic Device Configuration** - Add and configure audio devices at runtime without code changes
- **Event-Driven Audio** - Priority-based audio event system with automatic volume ducking
- **Blazor Server UI** - Modern C# Blazor Server interface with Material Design 3
- **Real-time Updates** - SignalR for live metadata and status updates
- **State Management** - Persistent storage of settings, history, favorites, and device configurations
- **Development Mode** - Local development with mock hardware simulation

## 🏗️ Architecture

The project follows a modular architecture with clear separation of concerns:

### Core Interfaces
- **IAudioInput** - Base interface for all audio input sources with new features:
  - PCM audio streaming via AudioDataAvailable event
  - Playback control (pause, resume, volume, repeat)
  - Priority and duration properties for event-based audio
  - Concurrent playback support
- **IAudioOutput** - Base interface for all audio output devices (ALSA-compatible on Linux/Raspberry Pi)
- **IDisplay** - Interface for metadata and status display
- **IConfiguration** - Interface for device configuration
- **IStorage** - Interface for data persistence

### Modules

#### Audio Inputs
The system now uses generic, composable input types:
- **UsbAudioInput** - Captures audio from USB devices (radios, turntables, etc.)
- **FileAudioInput** - Plays MP3/WAV files using NAudio
- **CompositeAudioInput** - Combines multiple audio sources with custom timing and volume
- **TtsAudioInput** - Text-to-speech using eSpeak TTS
- **SpotifyInput** - Spotify streaming integration

Legacy specific event inputs (DoorbellEventInput, RadioInput, etc.) have been replaced with generic types.
See [AUDIO_INPUT_MIGRATION.md](AUDIO_INPUT_MIGRATION.md) for migration details.

#### Audio Outputs
- **WiredSoundbarOutput** - Direct wired connection via ALSA
- **ChromecastOutput** - Network streaming to Chromecast devices
- All outputs handle ALSA audio streams on Linux/Raspberry Pi

#### Audio Mixer
- **AudioMixer** - Advanced PCM audio mixer that:
  - Mixes multiple audio sources in real-time
  - Respects priority levels for event-based interruptions
  - Supports concurrent playback when allowed
  - Per-source volume control
  - Sample format consistency
  - Robust error handling

### Services
- **EnvironmentService** - Detects runtime environment and enables simulation mode
- **JsonStorageService** - JSON-based persistent storage
- **ESpeakTtsService** - Text-to-speech using eSpeak
- **AudioPriorityManager** - Priority-based audio event management
- **AudioMixer** - Real-time audio mixing service
- **DeviceRegistry** - Manages configured audio devices with persistent storage
- **DeviceFactory** - Creates audio device instances from configuration

## 📋 Development Phases

### Phase 1: Requirements & Architecture Design ✅
- [x] Define detailed requirements for each input and output
- [x] Decide on overall software architecture patterns (MVVM)
- [x] Define input/output interfaces and data models
- [x] Choose datastore technology (JSON-based storage)
- [x] Setup repository structure

### Phase 2: Basic Raspberry Pi Setup and Project Initialization ✅
- [x] Initialize ASP.NET Core Web API project structure
- [x] Initialize React/TypeScript frontend project
- [x] Develop simulation mode for non-RPi environment
- [x] Define project skeleton with interfaces and modules
- [x] Create base implementations for inputs and outputs
- [x] Setup Material Design 3 UI with React and Material-UI

### Phase 3: Core Audio Input and Output Interfaces ✅
- [x] Implement abstractions for audio inputs and outputs
- [x] Create base module implementations with simulation support
- [x] Setup Radio input module (Raddy RF320) - *Simulation mode ready*
- [x] Setup Spotify input module - *Simulation mode ready*
- [x] Setup Wired Soundbar output - *Simulation mode ready*
- [x] Setup Chromecast output - *Simulation mode ready*
- [x] Implement event-driven audio input system
- [x] Create AudioPriorityManager for managing high-priority audio events
- [x] Add event input modules (doorbell, telephone, timer, reminder, broadcast)
- [ ] Integrate actual hardware for Raddy RF320 radio
- [ ] Implement vinyl turntable input interface
- [ ] Add support for MP3 playback from network shares

### Phase 4: Advanced Input Sources and Event-driven Audio ✅
- [x] Design and implement event-driven audio architecture
- [x] Create AudioPriorityManager service with volume ducking
- [x] Implement base event audio input classes
- [x] Add doorbell, telephone, timer, reminder, and broadcast event inputs
- [x] Create example API endpoints for testing event functionality
- [ ] Integrate Wyze doorbell webhook for real events
- [ ] Integrate Google broadcast receiver for real audio
- [ ] Implement full Spotify streaming integration
- [ ] Add Bluetooth audio input support and pairing UI

### Phase 5: Audio Output Devices 📝
- [ ] Complete wired soundbar integration
- [ ] Finalize Chromecast output streaming
- [ ] Add external Bluetooth speaker output
- [ ] Implement output switching mechanisms

### Phase 6: User Interface Development ✅
- [x] Develop basic Blazor UI with navigation
- [x] Create components for audio control, history, and favorites
- [x] Implement source selection interface
- [x] Add basic playback controls
- [x] Implement dark/light mode toggle with Material Design 3
- [x] Add device management UI for configuring audio inputs/outputs
- [ ] Enhance metadata display with real-time WebSocket updates
- [ ] Implement individual player controls (radio tuning, Spotify controls)
- [ ] Apply final UI styling for console radio display

### Phase 7: State Management and Persistency ✅
- [x] Setup JSON-based storage infrastructure
- [x] Implement device configuration persistence
- [ ] Implement saving/loading of favorites
- [ ] Implement history tracking
- [ ] Add migration and backup capabilities

### Phase 8: Testing, Simulation, and Deployment 📝
- [ ] Conduct unit tests for core modules
- [ ] Integration tests for input/output modules
- [ ] Test simulation mode on development machines
- [ ] Setup automated deployment for Raspberry Pi
- [ ] Performance testing and optimization

### Phase 9: Documentation and Final Adjustments 🚧
- [x] Create comprehensive README
- [x] Create device configuration guide
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
  - Touchscreen display with web browser
  - Audio hardware (soundbar, radio receiver, etc.)

### Development Setup (Simulation Mode)

1. **Clone the repository**
   ```bash
   git clone https://github.com/mmackelprang/Radio.git
   cd Radio
   ```

2. **Build and run the backend API**
   ```bash
   cd src/RadioConsole.Api
   dotnet restore
   dotnet run
   ```
   
   The API will start and log the ports it's listening on:
   - API endpoint: http://localhost:5000 (or https://localhost:5001)
   - Swagger UI: http://localhost:5000/swagger
   - OpenAPI JSON: http://localhost:5000/swagger/v1/swagger.json

3. **Build and run the Blazor UI** (in a new terminal)
   ```bash
   cd src/RadioConsole.Blazor
   dotnet restore
   dotnet run
   ```
   
   The Blazor interface will start and log the port it's listening on.
   Open your browser to http://localhost:5000 (or the displayed port)

   The application will automatically detect that it's not running on a Raspberry Pi and enable simulation mode, allowing you to develop and test on any platform.

**Note**: The React/TypeScript frontend in `src/RadioConsole.Web` is retained for reference but the primary UI is now the Blazor Server application.

### Raspberry Pi Deployment

Deployment instructions will be added in Phase 8.

## 🛠️ Technology Stack

### Backend
- **Framework**: ASP.NET Core 9.0 Web API
- **Language**: C# 12.0
- **API**: RESTful endpoints with SignalR for real-time updates
- **Storage**: JSON-based file storage
- **Target Platform**: Raspberry Pi 5 (Linux ARM64)

### Frontend
- **Framework**: Blazor Server with .NET 9.0
- **Language**: C# 12.0
- **UI Library**: MudBlazor 8.x with Material Design 3 support
- **State Management**: Component-based state with SignalR for real-time updates
- **Theming**: Material Design 3 theme with dark/light mode toggle
- **Rendering**: Interactive Server-side rendering

### Development
- **Cross-platform development**: Windows, macOS, Linux
- **API Testing**: Swagger/OpenAPI
- **Hot Reload**: Backend hot reload support

## 📂 Project Structure

```
Radio/
├── src/
│   ├── RadioConsole.Api/          # ASP.NET Core Web API
│   │   ├── Controllers/           # API controllers
│   │   ├── Interfaces/            # Core interfaces
│   │   │   ├── IAudioInput.cs
│   │   │   ├── IAudioOutput.cs
│   │   │   ├── IDisplay.cs
│   │   │   ├── IDeviceConfiguration.cs
│   │   │   └── IStorage.cs
│   │   ├── Modules/
│   │   │   ├── Inputs/           # Audio input implementations
│   │   │   │   ├── BaseAudioInput.cs
│   │   │   │   ├── SpotifyInput.cs
│   │   │   │   ├── UsbAudioInput.cs
│   │   │   │   ├── FileAudioInput.cs
│   │   │   │   └── TtsAudioInput.cs
│   │   │   └── Outputs/          # Audio output implementations
│   │   │       ├── BaseAudioOutput.cs
│   │   │       ├── WiredSoundbarOutput.cs
│   │   │       └── ChromecastOutput.cs
│   │   ├── Services/             # Core services
│   │   │   ├── EnvironmentService.cs
│   │   │   ├── JsonStorageService.cs
│   │   │   ├── AudioMixer.cs
│   │   │   ├── AudioPriorityManager.cs
│   │   │   ├── DeviceRegistry.cs
│   │   │   └── DeviceFactory.cs
│   │   ├── Hubs/                 # SignalR hubs for real-time updates
│   │   └── Models/               # Data models and DTOs
│   ├── RadioConsole.Blazor/      # Blazor Server UI (Material Design 3)
│   │   ├── Components/
│   │   │   ├── Layout/           # Layout components
│   │   │   │   ├── MainLayout.razor
│   │   │   │   └── NavMenu.razor
│   │   │   └── Pages/            # Blazor pages
│   │   │       ├── Home.razor            # Audio Control
│   │   │       ├── DeviceManagement.razor # Device Configuration
│   │   │       ├── DeviceDialog.razor    # Device Add/Edit Dialog
│   │   │       ├── History.razor         # Playback History
│   │   │       └── Favorites.razor       # Favorites
│   │   ├── wwwroot/              # Static assets
│   │   └── Program.cs            # App configuration
│   └── RadioConsole.Web/         # React/TypeScript frontend (legacy)
│       ├── src/
│       │   ├── components/       # React components
│       │   ├── contexts/         # React contexts
│       │   ├── hooks/            # Custom React hooks
│       │   ├── services/         # API service clients
│       │   ├── theme/            # Material-UI theme configuration
│       │   └── App.tsx           # Main app component
│       └── package.json          # npm dependencies
├── tests/
│   └── RadioConsole.Api.Tests/   # API unit tests
├── DEVICE_CONFIGURATION_GUIDE.md # Device configuration guide
├── PROJECT_PLAN.md               # Detailed project plan
├── README.md                     # This file
└── LICENSE                       # Project license
```

## 🤝 Contributing

This is a personal project, but suggestions and feedback are welcome! Please open an issue to discuss any changes you'd like to propose.

## 📄 License

See the [LICENSE](LICENSE) file for details.

## 🎵 Current Status

**Project Status**: Phase 6 Complete - Device Management UI

The project has completed the core infrastructure and device management features:
- ✅ ASP.NET Core Web API backend with modular architecture
- ✅ Blazor Server UI with Material Design 3 (MudBlazor)
- ✅ Core interfaces defined (IAudioInput, IAudioOutput, IDisplay, IDeviceConfiguration, IStorage)
- ✅ Simulation mode support for cross-platform development
- ✅ Base modules for audio inputs (USB, File, TTS, Spotify) and outputs (Wired, Chromecast)
- ✅ Material Design 3 UI with dark/light mode toggle
- ✅ Blazor pages for Audio Control, Device Management, History, and Favorites
- ✅ Interactive server-side rendering with real-time updates
- ✅ **Dynamic device configuration** - Add, edit, and remove audio devices through the UI
- ✅ **Persistent device storage** - Configurations saved to JSON and loaded on startup
- ✅ **Device factory pattern** - Create device instances from configurations

**Next Steps**: 
- Integrate actual hardware for real audio streaming
- Implement favorites and history tracking
- Add real-time metadata display
- Enhance Spotify integration

## 📚 Documentation

- [Device Configuration Guide](DEVICE_CONFIGURATION_GUIDE.md) - Complete guide for managing audio devices
- [Architecture Documentation](ARCHITECTURE.md) - Detailed system architecture
- [Development Guide](DEVELOPMENT.md) - Setup instructions for developers

## 📞 Contact

For questions or suggestions, please open an issue on GitHub.

---

*Building the future of vintage console radios, one commit at a time.* 🎵
