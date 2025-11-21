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
    *   Displays current **Date and Time** (prominent, matching vintage aesthetic - use HH:mm:ss - DD/MMM/YYYY time format).
    *   Network/System Status Indicators.
    *   This header willa at as the Navigation Bar.  There will be meaningful icons in the header that will show panels below.
*   **Left Panel (Audio Context):**
    *   By Default will contain `AudioSetupPanel` panel with fhe following:
        *   **Input Selector:** (Radio/Phono/Spotify (priority) as well as any other system available audio input) - Dropdown
        *   **Output Selector:** (Local Speakers/Google Cast (priority) as well as any other system available audio output) - Dropdown - Upon selecting Google Cast, a dialog appars to allow the user to select which device.
        *   Transport Controls (Play/Pause/Vol/Balance/Equalization) - Note that these adjustmenta all apply to the mixed SoundFlow audio stream.
*   **Center Panel (Now Playing Context):**
    *   Will switch panels / context depending on the following:
        *   *Spotify and MP3 File Mode:* - `RichAudioPanel` Current Song, ALbum Art, Metadata - Has an icon to select other media - i.e. search/playlist/etc on Spotify, file dialog for MP3 player.  It *may* make sense to create two panels here - one for MP3 and one for Spotify.
        *   *Radio Mode:* - `RadioPanel` Big LED Frequency, LED Band, Frequency Up/Down, Scan Up/Down, Volume Up/Down, Signal Strength Indicator, Radio Equalization, Power On/Off
        *   *Vinyl Mode:* - `PhonoPanel` Spinning record animation.
*   **Right Panel (Visualization):**
    *   Canvas element rendering real-time audio bars/waves via SignalR - `VisualizationPanel`
    *   This will allow the user to select any of the defined viaualizers.
*   **System & Testing Panels (Hidden By Default / Display on Demand):**
    *   **Configuration:** CRUD operations for all configuration settings - `ConfigurationPanel`
    *   **System Status:** CPU/Memory/Disk/Uptime - `SystemPanel`
        *   Memory / CPU / Disk Usage
        *   Allow system configuration
           *   Default TTS configuration (TTS Engine, Voice, Speed)
           *   Default Nofification setup (Ring, Notify, Alert, Alarm)
           *   Select USB Device for Phono
           *   Select USB Device for Radio
    *   **Event Generator (Testing Dashboard):** - `TestingPanel`
        *   Play system default events:
            *   Default Notify Event
            *   Default Ring Event
            *   Default Alert Event
            *   Default Alarm Event
            *   Create Alert from any Audio file and play it - create dialog for this, allow the user to stream this.
            *   TTS Testing events - Create dialog that lets user enter: TTS Text, TTS Engine, TTS Voice, TTS Speed and stream it.

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
