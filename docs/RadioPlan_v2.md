# Project: Grandpa Anderson's Digital Console (Radio Console)
## Project Overview
A modern audio command center hosted on a Raspberry Pi 5, encased in a vintage console radio cabinet. The software restores the original function (Radio/Vinyl) while adding modern capabilities (Spotify, Streaming, Smart Home Events).
## Technical Architecture & Stack
* Hardware: Raspberry Pi 5 (running Raspberry Pi OS / Linux).
* Framework: .NET 8/9 (C#).
* Hosting: Single ASP.NET Core Host containing:
  - Blazor Server: For the UI (Direct hardware access required).
  - Web API: REST endpoints for external control.
  - SignalR Hub: For real-time visualization data (FFT) and status updates.
* Audio Engine: SoundFlow (https://github.com/lsxprime/soundflow-docs/). Strict constraint: All audio processing must utilize SoundFlow.
* Database: Repository Pattern supporting hot-swapping between SQLite and JSON flat files based on appsettings.json.
* Logging: Serilog (Console + File sinks).
## Core Software Modules
### Audio Management (The "Kernel")
* IAudioDeviceManager: Enumerates and selects Linux ALSA/PulseAudio devices (USB Audio Inputs).
* AudioPriorityService: Handles "Ducking" logic.
  - Logic: If Notification (Doorbell/TTS) plays, fade Music (Radio/Spotify) to a configurable percentage (default 20%). Restore after event.
* VisualizerService: Analyzes the active audio stream via SoundFlow, generates FFT data, and pushes to Frontend via SignalR (target 30fps).
### Input Implementations
* Radio (Raddy RF320):
  - Audio: Captured via USB Audio Interface.
  - Control: Tuning/Band selection via USB Serial/UART command sending.
* Vinyl Turntable:
  - Audio: Captured via USB ADC.
  - Logic: "Passthrough" mode with optional software pre-amp gain via SoundFlow.
* Spotify:
  - Integration: SpotifyAPI-NET. Functions: Auth, Search, Transport Control, Album Art retrieval.
* Local Media:
  - Source: Local file system or SMB mounts.
  - Formats: MP3, WAV, FLAC, OGG.
### Smart Home Events & TTS
* TTS Engine: Factory pattern to switch between:
  - Offline: Piper (runs locally on Pi) or eSpeak.
  - Cloud: Google Cloud TTS or Azure Speech.
* Event Triggers:
  - REST Endpoint (POST /api/event/doorbell) triggers specific sound files.
  - Bluetooth/SIP integration for Phone Ring events.
## Blazor User Interface (Web App)
* Design System: Material Design 3 (using MudBlazor or Radzen libraries).
* Target Resolution: 12.5" x 3.75" (Ultrawide Aspect Ratio).
### Layout Regions (Grid System)
* Left Panel (Audio Setup):
  - Input Selector (Radio/Phono/Spotify/Aux).
  - Output Selector (Local Speakers/Chromecast).
  - Master Volume & EQ (Bass/Treble).
* Center Panel (Now Playing Context):
  - Dynamic View: Changes based on Input.
    - Radio Mode: Big Frequency numbers, Band switch, Signal strength.
    - Spotify Mode: Album Art (large), Lyrics, Song Title.
    - Vinyl Mode: Spinning record animation.
* Right Panel (Visualization):
  - Canvas element rendering real-time audio bars/waves (driven by SignalR).
* Hidden Overlay (System/Config):
  - Slide-out drawer for WiFi setup, API keys, and System Logs.
## Development Roadmap & Testing
### Phase 1: Core Skeleton
* Setup Clean Architecture (Core, Infrastructure, API, UI).
* Implement Serilog.
* Implement JSON/SQLite generic repository.
### Phase 2: Audio Foundation (SoundFlow)
* Wrap SoundFlow in IAudioService.
* Create Console Test App to verify playing an MP3 on the Pi.
* Implement VisualizerService and verify data stream.
### Phase 3: The Inputs
* Implement Spotify Auth and Playback.
* Implement USB Audio Passthrough (for Radio/Vinyl).
### Phase 4: The Mixer
* Implement AudioPriorityService (Ducking logic).
* Test: Play music, inject TTS, verify volume dip and restore.
### Phase 5: The UI
* Build Main Layout (CSS Grid for 12.5x3.75 screen).
* Implement SignalR client for Visualizer.
### 6. Documentation Requirements
* Swagger/OpenAPI: Auto-generated from Controllers.
* Developer Guide: Markdown file explaining how to mock USB devices for development on non-Pi machines.