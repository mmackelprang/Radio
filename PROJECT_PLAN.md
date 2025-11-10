# Modern Raspberry Pi 5 Console Radio Audio System Project Plan

## Project Overview
This project involves building a modern audio system inside an old console radio cabinet using a Raspberry Pi 5 with ASP.NET Core backend and React/TypeScript frontend. The system will support multiple audio input sources and output devices with a flexible, extensible architecture and a rich touchscreen web interface.

## Project Goals
- Support audio inputs: SW/AM/FM radio (Raddy RF320), vinyl, Wyze doorbell, Google broadcast, Spotify, Bluetooth, MP3 files on local network
- Support audio outputs: wired soundbar, Chromecast, Bluetooth speaker
- Modern web-based UI with Material Design 3, dark/light mode toggle
- Touchscreen interface with history, favorites, audio controls, configuration, and metadata display
- RESTful API with real-time WebSocket updates
- Extensible interfaces for inputs/outputs and state management with datastore
- Simulation mode for non-Raspberry Pi development

## Phases

### Phase 1: Requirements & Architecture Design
- Define detailed requirements for each input and output
- Decide on overall software architecture patterns (RESTful API with React frontend)
- Define input/output interfaces and data models
- Choose datastore technology (JSON-based storage for settings and history)
- Design API endpoints and WebSocket integration for real-time updates
- Setup repository and CI/CD pipeline with GitHub

### Phase 2: Basic Raspberry Pi Setup and Project Initialization
- Setup Raspberry Pi 5 with required OS, .NET 9.0 runtime, and Node.js
- Initialize ASP.NET Core Web API project structure
- Initialize React/TypeScript frontend with Vite
- Develop simulation mode for non-RPi environment
- Define project skeleton with interfaces and modules
- Setup Material-UI with Material Design 3 theme

### Phase 3: Core Audio Input and Output Interfaces
- Implement abstractions for audio inputs and outputs
- Create dummy implementations for testing
- Integrate Raddy RF320 radio audio input
- Implement vinyl turntable input interface (e.g., analog to digital via USB sound card)
- Add support for MP3 playback from network shares

### Phase 4: Advanced Input Sources and Event-driven Audio
- Integrate Wyze doorbell event-driven input
- Integrate Google broadcast receiver for audio
- Implement Spotify streaming integration with basic controls
- Add Bluetooth audio input support and pairing UI

### Phase 5: Audio Output Devices
- Support wired soundbar output connection
- Integrate Chromecast output streaming
- Add external Bluetooth speaker output
- Implement output switching and configuration mechanisms

### Phase 6: User Interface Development
- Develop React-based web UI with routing and navigation
- Create Material-UI components for audio control, history, and favorites
- Implement dark/light mode toggle with persistent preferences
- Integrate real-time updates via WebSocket/SignalR
- Add configuration sections per input/output
- Implement individual player controls (radio tuning, Spotify playback, MP3/Bluetooth controls)
- Apply responsive design suitable for console radio touchscreen display
- Ensure accessibility and touch-friendly interactions

### Phase 7: State Management and Persistency
- Implement saving/loading of favorites, history, and configurations using chosen datastore
- Add migration and backup capabilities

### Phase 8: Testing, Simulation, and Deployment
- Conduct unit tests and integration tests
- Test simulation mode on development machines
- Setup automated deployment/update process for Raspberry Pi

### Phase 9: Documentation and Final Adjustments
- Write comprehensive user and developer documentation
- Record setup and usage instructions
- Perform final polishing and bug fixes

## Recent Major Refactoring (November 2024)

### Audio System Modernization ✅

The audio input/output system has undergone a major refactoring to improve flexibility, maintainability, and functionality:

#### Key Changes:
1. **Unified IAudioInput Interface**
   - Merged IEventAudioInput functionality into IAudioInput
   - Added PCM audio streaming via AudioDataAvailable event
   - Added playback control: PauseAsync, ResumeAsync, SetVolumeAsync, SetRepeat
   - Added AllowConcurrent property for concurrent playback rules
   - Added Priority and Duration properties for all inputs

2. **Generic, Composable Input Types**
   - **UsbAudioInput** - Captures from USB audio devices (radios, turntables)
   - **FileAudioInput** - Plays MP3/WAV files using NAudio
   - **CompositeAudioInput** - Combines multiple sources with timing/volume control
   - **TtsAudioInput** - Text-to-speech (renamed from TextEventInput)
   - Replaced 6 specialized event inputs with generic types

3. **AudioMixer Service**
   - Real-time PCM audio mixing
   - Priority-based playback management
   - Per-source volume control
   - Concurrent playback rules
   - Sample format consistency handling

4. **ALSA Compatibility**
   - All outputs documented for ALSA support on Linux/Raspberry Pi
   - NAudio automatically uses ALSA on Linux
   - Seamless integration with system audio

5. **Documentation**
   - Created AUDIO_INPUT_MIGRATION.md guide
   - Updated README.md with new architecture
   - Comprehensive code comments

#### Migration Status:
- ✅ Core refactoring complete
- ✅ Build succeeds with 0 errors, 0 warnings
- ✅ All new input types implemented
- ✅ AudioMixer integrated and functional
- ⏳ Testing phase pending
- ⏳ Hardware integration pending
