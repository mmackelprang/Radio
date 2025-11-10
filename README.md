# Radio Console - Modern Raspberry Pi 5 Audio System

A modern audio system built with ASP.NET Core and React/TypeScript for Raspberry Pi 5, designed to bring new life to vintage console radios. This project provides a flexible, extensible architecture for managing multiple audio inputs and outputs with a rich touchscreen web interface.

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
- **Material Design 3** - Modern, touch-friendly React interface with Material-UI
- **Dark/Light Mode** - Easily swappable theme for different lighting conditions
- **Audio Controls** - Source selection, playback controls, volume management
- **Event Notifications** - High-priority audio notifications with automatic volume management
- **History** - Track recently played content
- **Favorites** - Save and quick-access favorite stations and playlists
- **Metadata Display** - Rich information about currently playing audio
- **Configuration** - Per-device settings and preferences
- **Responsive Design** - Optimized for touchscreen displays

### Technical Features
- **Modular Architecture** - Clean separation between backend API and frontend
- **RESTful API** - ASP.NET Core Web API for audio control and management
- **Event-Driven Audio** - Priority-based audio event system with automatic volume ducking
- **React Frontend** - Modern TypeScript-based UI with Material-UI components
- **Real-time Updates** - WebSocket support for live metadata and status updates
- **State Management** - Persistent storage of settings, history, and favorites
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

### Phase 6: User Interface Development 🚧
- [x] Develop basic React UI with navigation
- [x] Create components for audio control, history, and favorites
- [x] Implement source selection interface
- [x] Add basic playback controls
- [x] Implement dark/light mode toggle with Material Design 3
- [ ] Enhance metadata display with real-time WebSocket updates
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
- Node.js 20.x or later with npm
- For Raspberry Pi deployment:
  - Raspberry Pi 5 with Raspberry Pi OS
  - .NET runtime installed
  - Node.js runtime installed
  - Touchscreen display with web browser
  - Audio hardware (soundbar, radio receiver, etc.)

### Development Setup (Simulation Mode)

1. **Clone the repository**
   ```bash
   git clone https://github.com/mmackelprang/Radio.git
   cd Radio
   ```

2. **Build and run the backend**
   ```bash
   cd src/RadioConsole.Api
   dotnet restore
   dotnet run
   ```
   
   The API will start on http://localhost:5000 (or https://localhost:5001)

3. **Build and run the frontend** (in a new terminal)
   ```bash
   cd src/RadioConsole.Web
   npm install
   npm start
   ```
   
   The web interface will open at http://localhost:3000

   The application will automatically detect that it's not running on a Raspberry Pi and enable simulation mode, allowing you to develop and test on any platform.

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
- **Framework**: React 18.x
- **Language**: TypeScript 5.x
- **UI Library**: Material-UI (MUI) v5 with Material Design 3
- **State Management**: React Context API / Redux Toolkit
- **Theming**: Custom Material Design 3 theme with dark/light mode
- **Build Tool**: Vite

### Development
- **Cross-platform development**: Windows, macOS, Linux
- **API Testing**: Swagger/OpenAPI
- **Hot Reload**: Backend and frontend hot reload support

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
│   │   │   ├── IConfiguration.cs
│   │   │   └── IStorage.cs
│   │   ├── Modules/
│   │   │   ├── Inputs/           # Audio input implementations
│   │   │   │   ├── BaseAudioInput.cs
│   │   │   │   ├── RadioInput.cs
│   │   │   │   └── SpotifyInput.cs
│   │   │   └── Outputs/          # Audio output implementations
│   │   │       ├── BaseAudioOutput.cs
│   │   │       ├── WiredSoundbarOutput.cs
│   │   │       └── ChromecastOutput.cs
│   │   ├── Services/             # Core services
│   │   │   ├── EnvironmentService.cs
│   │   │   └── JsonStorageService.cs
│   │   ├── Hubs/                 # SignalR hubs for real-time updates
│   │   └── Models/               # Data models and DTOs
│   └── RadioConsole.Web/         # React/TypeScript frontend
│       ├── src/
│       │   ├── components/       # React components
│       │   │   ├── AudioControl.tsx
│       │   │   ├── History.tsx
│       │   │   └── Favorites.tsx
│       │   ├── contexts/         # React contexts
│       │   ├── hooks/            # Custom React hooks
│       │   ├── services/         # API service clients
│       │   ├── theme/            # Material-UI theme configuration
│       │   └── App.tsx           # Main app component
│       ├── public/               # Static assets
│       └── package.json          # npm dependencies
├── PROJECT_PLAN.md               # Detailed project plan
├── README.md                     # This file
└── LICENSE                       # Project license
```

## 🤝 Contributing

This is a personal project, but suggestions and feedback are welcome! Please open an issue to discuss any changes you'd like to propose.

## 📄 License

See the [LICENSE](LICENSE) file for details.

## 🎵 Current Status

**Project Status**: Phase 2 Complete - ASP.NET Core + React architecture established

The project has completed the migration to modern web architecture with:
- ✅ ASP.NET Core Web API backend with modular architecture
- ✅ React/TypeScript frontend with Material-UI
- ✅ Core interfaces defined (IAudioInput, IAudioOutput, IDisplay, IConfiguration, IStorage)
- ✅ Simulation mode support for cross-platform development
- ✅ Base modules for Radio and Spotify inputs
- ✅ Base modules for Wired Soundbar and Chromecast outputs
- ✅ Material Design 3 UI with dark/light mode support
- ✅ React components for Audio Control, History, and Favorites

**Next Steps**: Phase 3 - Integrate actual hardware and implement real audio streaming capabilities.

## 📞 Contact

For questions or suggestions, please open an issue on GitHub.

---

*Building the future of vintage console radios, one commit at a time.* 🎵
