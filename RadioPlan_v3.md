### Part 1: The System Context (Updated)

*Save this as `RadioPlan_v3.md`. This is your master blueprint for AI coding assistants.*

# Project: Grandpa Anderson's Digital Console (Radio Console)

## 1. Project Overview
A modern audio command center hosted on a Raspberry Pi 5, encased in a vintage console radio cabinet. The software restores the original function (Radio/Vinyl) while adding modern capabilities (Spotify, Streaming, Smart Home Events, and Whole-Home Audio).

## 2. Technical Architecture & Stack
*   **Hardware:** Raspberry Pi 5 (running Raspberry Pi OS / Linux).
*   **Framework:** .NET 8/9 (C#).
*   **Hosting:** Single ASP.NET Core Host containing:
    *   **Blazor Server:** For the UI (Direct hardware access required).
    *   **Web API:** REST endpoints for external control.
    *   **SignalR Hub:** For real-time visualization data (FFT) and status updates.
    *   **Local Stream Server:** An endpoint to expose raw audio as an HTTP stream (for Chromecast integration).
*   **Audio Engine:** **SoundFlow** (https://github.com/lsxprime/soundflow-docs/).
*   **Database:** Repository Pattern supporting hot-swapping between `SQLite` and `JSON` flat files.
*   **Logging:** Serilog (Console + File sinks).

## 3. Core Software Modules

### A. Audio Management (The "Kernel")
*   **IAudioDeviceManager:** Enumerates system ALSA/PulseAudio devices.
*   **AudioPriorityService (Ducking):**
    *   *Logic:* If `Notification` (Doorbell/TTS) plays, fade `Music` (Radio/Spotify) to 20%. Restore after event.
*   **VisualizerService:** Analyzes the active audio stream via SoundFlow, generates FFT data, and pushes to Frontend via SignalR.

### B. Audio Outputs (New)
*   **Local Hardware:** Standard output via Pi HDMI or DAC.
*   **Google Cast Output:**
    *   Implementation of `IAudioOutput` that does *not* play to local speakers.
    *   Instead, it pipes SoundFlow audio to the **Local Stream Server** and initiates a Cast session (using `GoogleCast` or `SharpCaster` libraries) to stream that URL to network Google Home devices.

### C. Smart Home Inputs & Events
*   **Google Broadcast Receiver:**
    *   Implementation of the Google Assistant SDK (or gRPC bridge) to receive "Broadcast" messages from other Google Homes and play them through the Console speakers.
*   **TTS Engine:** Factory pattern (Piper/eSpeak for local, Azure/Google for cloud) with specific voice parameters (Gender/Speed).
*   **Event Triggers:** REST endpoints for Doorbell/Phone Ring events.

### D. Music Inputs
*   **Raddy RF320:** USB Audio for sound, Serial/UART for tuning control.
*   **Vinyl Turntable:** USB ADC with software pre-amp.
*   **Spotify:** `SpotifyAPI-NET` for auth/search/playback.

## 4. Blazor User Interface (Web App)
**Design System:** Material Design 3 (MudBlazor/Radzen).
**Display Target:** 12.5" x 3.75" Touchscreen.
**Window Mode:** **Kiosk Mode / Full Screen**. The application must launch in a frameless browser window so it appears as a native embedded OS, hiding the URL bar and OS taskbar.

### Layout Regions (Grid System)
*   **Global Header:**
    *   Displays current **Date and Time** (prominent, matching vintage aesthetic).
    *   Network/System Status Indicators.
*   **Left Panel (Audio Setup):**
    *   Input Selector (Radio/Phono/Spotify).
    *   **Output Selector:** Dropdown to select Local Speakers OR specific Google Cast devices found on the network.
    *   Transport Controls (Play/Pause/Vol/Balance).
*   **Center Panel (Now Playing Context):**
    *   *Radio Mode:* Big Frequency numbers, Band switch.
    *   *Spotify Mode:* Album Art, Lyrics, Metadata.
    *   *Vinyl Mode:* Spinning record animation.
*   **Right Panel (Visualization):**
    *   Canvas element rendering real-time audio bars/waves via SignalR.
*   **System & Testing Panel (Slide-out/Hidden):**
    *   **Configuration:** CRUD operations for system settings.
    *   **System Status:** CPU/Memory/Disk/Uptime.
    *   **Event Generator (Testing Dashboard):**
        *   *Button:* Play TTS "Now is the time..." (Male Voice).
        *   *Button:* Play TTS "Hello World" (Female Voice).
        *   *Button:* Play Audio File (300Hz Tone, 2 sec).
        *   *Button:* Play Audio File (Short MP3 Song).
        *   *Button:* Simulate Doorbell Ring.
        *   *Button:* Simulate Broadcast Receipt.

## 5. Development Roadmap (Updated)

### Phase 1: Core & Config
*   Project Skeleton, Serilog, JSON/SQLite config.

### Phase 2: Audio Core & Output Casting
*   Implement SoundFlow wrapper.
*   **New:** Implement `CastService` to discover devices and `StreamAudioService` to serve audio to them.

### Phase 3: Inputs & Broadcasts
*   Implement Spotify/Radio/Vinyl inputs.
*   **New:** Implement Google Assistant SDK integration for receiving broadcasts.

### Phase 4: The Mixer & Testing Tools
*   Implement Ducking logic.
*   **New:** Create the "Event Generator" backend logic to fire test events on demand.

### Phase 5: UI & Kiosk Setup
*   Build the 12.5x3.75 layouts including the **Date/Time Header**.
*   Implement the Testing Dashboard UI.
*   Document `chromium-browser --kiosk` setup for the Pi.
