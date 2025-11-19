**How to use these:**
1.  **Start a new chat session.**
2.  **Upload/Paste** the `RadioPlan_v3.md` content first to set the context.
3.  **Paste the prompt for Phase 1.**
4.  **Iterate** until Phase 1 is complete and compiling.
5.  **Repeat** for subsequent phases.

---

### Phase 1: The Skeleton & Configuration ✅ Done
**Goal:** Establish the project structure, logging, and data persistence layer.

> "Let's start with Phase 1. Please execute the following:
>
> 1.  **Solution Structure:** Generate the dotnet CLI commands to create a Clean Architecture solution with the following projects: `RadioConsole.Core`, `RadioConsole.Infrastructure`, `RadioConsole.API`, `RadioConsole.Web` (Blazor Server), and `RadioConsole.Tests` (xUnit).
> 2.  **Dependencies:** Add references between projects (Web/API depend on Core/Infrastructure). Add `Serilog.AspNetCore` to Web and API.
> 3.  **Configuration:** In `Core`, create an interface `ISystemSettingsService`. In `Infrastructure`, implement a generic Repository pattern that can switch between `SQLite` and `JSON` flat file storage based on a boolean flag in `appsettings.json`.
> 4.  **Logging:** Configure Serilog in `Program.cs` (for both Web and API) to write to Console and a rolling file in a `/logs` directory.
> 5.  **Testing:** Create a unit test in the Test project that verifies the Repository can save a setting to a JSON file and read it back."

---

### Phase 2: Audio Engine & Google Cast ✅ Done
**Goal:** Implement the SoundFlow wrapper and the logic to cast audio to Google Home devices.

> As described in `RadioPlan_v3.md`, continue devopment on this phase:
> "Moving to Phase 2: Audio Core.
>
> 1.  **SoundFlow Wrapper:** In `Core`, define `IAudioPlayer` and `IAudioDeviceManager`. In `Infrastructure`, implement these using the **SoundFlow** library. It must support selecting specific ALSA input devices (USB Audio).
> 2.  **Audio Outputs:** Create an `IAudioOutput` interface. Implement two versions:
>     *   `LocalAudioOutput`: Plays directly to the Pi's default audio sink.
>     *   `CastAudioOutput`: This does **not** play locally. It must pipe the audio stream to a local internal HTTP endpoint.
> 3.  **Streaming Service:** Create a `StreamAudioService` (Minimal API endpoint) that exposes the current SoundFlow audio mix as a continuous WAV/MP3 stream over HTTP (e.g., `http://localhost:5000/stream.mp3`).
> 4.  **Cast Logic:** Implement the logic (using a library like `GoogleCast` or `SharpCaster`) to discover network Cast devices and tell them to play the URL from step 3.
> 5.  **Unit Test:** Write a test that mocks `IAudioPlayer` and asserts that `CastAudioOutput` attempts to discover devices."

---

### Phase 3: Inputs & Broadcasts
**Goal:** Connect the specific hardware inputs and the Google Broadcast receiver.

> As described in `RadioPlan_v3.md`, continue devopment on this phase: 
> "Phase 3: Inputs & Broadcasts.
>
> 1.  **Raddy RF320:** Create a `RaddyRadioService`. It needs to manage two things:
>     *   *Control:* In a later phase, we will create the RaddyController which connects to BLE through a specific protocol.  For now, control will be handled manually.
>     *   *Audio:* Identify the specific USB Audio device ID associated with the radio and route it to `IAudioPlayer`.
> 2.  **Spotify:** Implement `ISpotifyService` using `SpotifyAPI-NET`. It needs methods for Auth, Search, Play, Pause, and fetching Album Art.
> 3.  **Google Broadcast:** Create a `BroadcastReceiverService`. This should use the Google Assistant SDK to listen for incoming 'Broadcast' events.
> 4.  **Integration:** Ensure that when a Broadcast is received, it triggers an event in the `Core` layer that contains the audio data of the message."

---

### Phase 4: The Mixer (Ducking) & Event Generator
**Goal:** Handle volume logic (music vs. voice) and create the backend for the testing tools.

> As described in `RadioPlan_v3.md`, continue devopment on this phase: 
> "Phase 4: Audio Priority & Testing Logic.
>
> 1.  **Priority Manager:** Implement `AudioPriorityService`. It requires a priority mechanism:
>     *   *High Priority:* TTS, Doorbell, Phone Ring, Google Broadcasts.
>     *   *Low Priority:* Radio, Spotify, Vinyl.
>     *   *Logic:* When a High Priority event starts, fade Low Priority volume to configurable percentage (default to20%) (Ducking). When High Priority finishes, fade back to original volume.
> 2.  **TTS Factory:** Create an `ITextToSpeechService` that can switch between `espeak` (local process) and Google/Azure (Cloud).
> 3.  **Event Generator:** Create a `SystemTestService` in Core. This service should have methods to manually trigger:
>     *   A specific TTS phrase.
>     *   A standard test tone (300Hz sine wave).
>     *   A simulated Doorbell event.
> 4.  **API Endpoints:** Expose these test triggers via the REST API (`POST /api/test/tts`, `POST /api/test/tone`) so the UI can call them."

---

### Phase 5: Blazor UI & Kiosk Layout
**Goal:** Build the visual interface, specifically for the ultrawide screen.

> As described in `RadioPlan_v3.md`, continue devopment on this phase: 
> "Phase 5: The User Interface.
>
> 1.  **Setup:** Install `MudBlazor` (or similar Material library). Configure `MainLayout.razor` to use CSS Grid. The target resolution is strictly **12.5 inches x 3.75 inches**. The UI must be responsive to this ultra-wide aspect ratio.
> 2.  **Header:** Create a Global Header component that displays the **Current Date and Time** prominently (large font), along with WiFi and System status icons.
> 3.  **Panels:** Create the following components:
>     *   `AudioSetupPanel`: Dropdowns for Input and Output (Local vs Cast).
>     *   `NowPlayingPanel`: Dynamic layout. If Input=Radio, show Frequency. If Input=Spotify, show Art/Lyrics.
>     *   `SystemTestPanel`: A grid of buttons that call the `SystemTestService` methods from Phase 4.
> 4.  **Visualizer:** Implement a SignalR Hub `VisualizerHub`. On the server, `IAudioPlayer` should push FFT data to this hub every 50ms. On the client, use an HTML5 Canvas in the `VisualizationPanel` to render bars based on this data.
> 5.  **Kiosk Note:** Ensure the app CSS hides scrollbars (`overflow: hidden`) to look like a native app when running in full-screen browser mode."
