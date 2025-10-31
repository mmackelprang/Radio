# Modern Raspberry Pi 5 Console Radio Audio System Project Plan

## Project Overview
This project involves building a modern audio system inside an old console radio cabinet using a Raspberry Pi 5 and C# modern development. The system will support multiple audio input sources and output devices with a flexible, extensible architecture and a rich touchscreen user interface.

## Project Goals
- Support audio inputs: SW/AM/FM radio (Raddy RF320), vinyl, Wyze doorbell, Google broadcast, Spotify, Bluetooth, MP3 files on local network
- Support audio outputs: wired soundbar, Chromecast, Bluetooth speaker
- Touchscreen UI with history, favorites, audio controls, configuration, and metadata display
- Extensible interfaces for inputs/outputs and state management with datastore
- Simulation mode for non-Raspberry Pi development

## Phases

### Phase 1: Requirements & Architecture Design
- Define detailed requirements for each input and output
- Decide on overall software architecture patterns (e.g., MVVM)
- Define input/output interfaces and data models
- Choose datastore technology (e.g., SQLite, LiteDB, or JSON with filesystem)
- Setup repository and CI/CD pipeline with GitHub

### Phase 2: Basic Raspberry Pi Setup and Project Initialization
- Setup Raspberry Pi 5 with required OS and .NET 8.0/9.0 runtime
- Initialize C# project and solution structure (e.g., using .NET MAUI for UI)
- Develop simulation mode for non-RPi environment
- Define project skeleton with interfaces and modules

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
- Develop touchscreen UI for source selection and controls
- Implement history and favorites views
- Integrate metadata and real-time clock display
- Add configuration sections per input/output
- Implement individual player controls (radio tuning, Spotify playback, MP3/Bluetooth controls)
- Apply UI styling and layout suitable for console radio display

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
