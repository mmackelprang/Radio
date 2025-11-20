# Phase 5 Integration TODO - Status Update

**Last Updated:** November 20, 2025 (Updated with API Controllers, Metadata, and UI Components)

This document outlines the remaining integration work needed to connect the Phase 5 Blazor UI with the backend audio services implemented in Phases 1-4.

## Current Status Summary

### ✅ Completed (Build Infrastructure & New Features)
- Architecture fixed: Created `IVisualizationService` abstraction to prevent circular dependencies
- All service injections working in components
- Build successful with 110/119 tests passing (9 audio device tests fail in CI as expected)
- API endpoints functional via SystemTestPanel
- **NEW: 5 API Controllers added with 20+ endpoints (Configuration, NowPlaying, Visualization, StreamAudio, Metadata)**
- **NEW: TagLib# metadata extraction service integrated**
- **NEW: Material 3 touchscreen keyboards (NumericKeypad, TouchKeyboard)**
- **NEW: USB Radio Control Panel with LED displays and frequency validation**
- **NEW: Radio Demo page with component documentation at /radio-demo**

### ⚠️ Partially Complete (Functional but Limited)
- AudioSetupPanel: UI works, limited backend integration
- NowPlayingPanel: UI works, using available service methods (can now use MetadataController for local files)
- SystemTestPanel: Full API integration complete
- GlobalHeader: Basic functionality, needs real system monitoring
- VisualizationPanel: Architecture ready, needs FFT implementation

### ❌ Not Started
- FFT data generation from audio stream
- Advanced service methods (seek, skip, etc.)
- Real-time system monitoring
- Performance optimizations
- Integration of metadata extraction into NowPlayingPanel for local files

## Overview

The Phase 5 UI is complete and functional. The initial integration commit connected components to services, but several advanced features still need implementation due to missing methods in service interfaces.

## 1. AudioSetupPanel Integration ⚠️ PARTIALLY COMPLETE

**File:** `RadioConsole.Web/Components/Shared/AudioSetupPanel.razor`

### Completed:
- [x] Inject `IAudioPlayer` service  
- [x] Inject `IRaddyRadioService` for radio input
- [x] Inject `ISpotifyService` for Spotify input  
- [x] Inject `CastAudioOutput` for Cast device discovery
- [x] Wire input selector dropdown to switch between audio sources
- [x] Wire output selector to switch between Local and Cast outputs (UI only)
- [x] Implement Cast device discovery and populate dropdown
- [x] Connect Play/Pause/Stop transport controls
- [x] Bind volume slider to `IAudioPlayer.SetVolumeAsync()`
- [x] Load current audio settings on initialization

### Remaining Work:
- [ ] Add Previous/Next methods to service interfaces:
  - `ISpotifyService.SkipToPreviousAsync()`
  - `ISpotifyService.SkipToNextAsync()`
  - `IRaddyRadioService.SeekDownAsync()` (frequency seek)
  - `IRaddyRadioService.SeekUpAsync()` (frequency seek)
- [ ] Implement balance control (not yet in IAudioPlayer interface)
- [ ] Wire output switching to actually change audio output device
- [ ] Implement local file playback support

### Current Limitations:
- Previous/Next buttons show info messages (methods don't exist in interfaces)
- Balance slider is UI-only (no backend implementation)
- Radio/Spotify start via service methods, but no stream integration
- Vinyl and Local modes are placeholder only

## 2. NowPlayingPanel Integration ⚠️ PARTIALLY COMPLETE

**File:** `RadioConsole.Web/Components/Shared/NowPlayingPanel.razor`

### Completed:
- [x] Subscribe to audio source change events (via timer polling)
- [x] Display current radio frequency from `IRaddyRadioService.GetFrequencyAsync()`
- [x] Display Spotify track info from `ISpotifyService.GetCurrentlyPlayingAsync()`
- [x] Display Spotify album art URL
- [x] Implement real-time updates (1-second timer polling)
- [x] Update vinyl playback status (placeholder)
- [x] Display local file playback progress (placeholder)

### Remaining Work:
- [ ] Add signal strength method to `IRaddyRadioService` interface (currently hardcoded)
- [ ] Implement Spotify lyrics display (API may not support)
- [ ] Implement actual vinyl playback status
- [ ] Implement actual local file playback and progress
- [ ] Consider switching from timer polling to SignalR for real-time updates

### Current Limitations:
- Radio signal strength is hardcoded (method doesn't exist in interface)
- Spotify lyrics feature not implemented (may require additional API)
- Vinyl and Local modes are UI placeholders only

## 3. SystemTestPanel Integration ✅ COMPLETE

**File:** `RadioConsole.Web/Components/Shared/SystemTestPanel.razor`

### Completed:
- [x] Configure HttpClient base address from appsettings.json
- [x] API calls to Phase 4 TestController endpoints:
  - `POST /api/test/tts` (TTS tests)
  - `POST /api/test/tone` (tone tests)
  - `POST /api/test/doorbell` (doorbell test)
  - `POST /api/test/broadcast` (broadcast test)
  - `GET /api/test/status` (system status)
- [x] Add real-time status refresh (polling every 5 seconds)

### Remaining Work:
- [ ] Implement actual CPU usage monitoring on server side
- [ ] Implement actual memory usage monitoring on server side
- [ ] Implement actual system uptime calculation on server side

**Note:** The panel is fully functional and makes real API calls. System metrics (CPU, memory, uptime) need to be implemented in the TestController on the API side.

## 4. GlobalHeader Integration ⚠️ PARTIALLY COMPLETE

**File:** `RadioConsole.Web/Components/Shared/GlobalHeader.razor`

### Completed:
- [x] Real-time date/time display (updates every second)
- [x] Status icons (WiFi, System Health)
- [x] Polling mechanism for status updates (every 5 seconds)
- [x] HttpClient configured for API calls

### Remaining Work:
- [ ] Implement actual WiFi status check using `NetworkInterface` API
- [ ] Implement system health monitoring:
  - CPU usage threshold check
  - Memory usage threshold check
  - Disk space check
  - Service availability check
- [ ] Update status icons based on real data

### Current Limitations:
- WiFi status is hardcoded to `true`
- System health is hardcoded to `true`
- Needs backend API endpoint or direct system monitoring

## 5. VisualizationPanel Integration ✅ ENHANCED WITH MULTIPLE VISUALIZATIONS

**File:** `RadioConsole.Web/Components/Shared/VisualizationPanel.razor`  
**File:** `RadioConsole.Web/Hubs/VisualizerHub.cs`  
**File:** `RadioConsole.Core/Interfaces/Audio/IVisualizationService.cs`  
**File:** `RadioConsole.Web/Services/SignalRVisualizationService.cs`  
**File:** `RadioConsole.Core/Interfaces/Audio/IVisualizationContext.cs` ✨ **NEW**  
**File:** `RadioConsole.Core/Interfaces/Audio/IVisualizer.cs` ✨ **NEW**  
**File:** `RadioConsole.Infrastructure/Audio/Visualization/LevelMeterVisualizer.cs` ✨ **NEW**  
**File:** `RadioConsole.Infrastructure/Audio/Visualization/WaveformVisualizer.cs` ✨ **NEW**  
**File:** `RadioConsole.Infrastructure/Audio/Visualization/SpectrumVisualizer.cs` ✨ **NEW**  
**File:** `RadioConsole.Web/Services/BlazorVisualizationContext.cs` ✨ **NEW**

### Completed:
- [x] Created `IVisualizationService` interface in Core layer
- [x] Implemented `SignalRVisualizationService` in Web layer  
- [x] Updated `SoundFlowAudioPlayer` to accept `IVisualizationService` dependency
- [x] SignalR hub configured and mapped
- [x] Canvas visualization rendering working
- [x] FFT timer mechanism in place (placeholder data)
- [x] Fixed circular dependency issue
- [x] **Created `IVisualizationContext` interface adapted from WPF examples**
- [x] **Created `IVisualizer` interface with `VisualizationType` enum**
- [x] **Implemented `LevelMeterVisualizer` for RMS/Peak level display**
- [x] **Implemented `WaveformVisualizer` for audio waveform display**
- [x] **Implemented `SpectrumVisualizer` for frequency spectrum display**
- [x] **Created `BlazorVisualizationContext` for JavaScript interop rendering**
- [x] **Added visualization type selector dropdown to UI**
- [x] **Updated visualizer.js to support multiple visualization modes**
- [x] **Added rendering commands system for custom visualizers**

### Remaining Work:
- [ ] Integrate visualizers with audio player to process real audio data
- [ ] Implement actual FFT analysis from audio stream (SoundFlow limitation)
  - SoundFlow/MiniAudio doesn't provide built-in FFT
  - Need to integrate FFT library (e.g., Kiss FFT, MathNet.Numerics)
  - Capture audio samples from playback stream
  - Process samples through FFT algorithm
- [ ] Configure audio player to send audio samples to selected visualizer
- [ ] Enable/disable visualization based on audio player state
- [ ] Ensure visualization data is normalized to 0-1 range
- [ ] Optimize performance for real-time rendering

### Current Limitations:
- FFT data is placeholder random values (no real audio analysis)
- Visualizers are implemented but not yet connected to audio player
- Real FFT requires integration with audio processing library
- SpectrumVisualizer uses simplified frequency approximation instead of true FFT

### Implementation Notes:
The architecture follows the example code from `/examples` folder:
- `IVisualizationContext` provides drawing primitives (adapted from WPF examples)
- `IVisualizer` interface defines the contract for all visualizers
- Three visualizer types implemented: LevelMeter, Waveform, Spectrum
- Blazor uses JavaScript interop for canvas rendering (instead of WPF's DrawingContext)
- Visualization type can be selected from dropdown in UI

Architecture:
```
Core (IVisualizationService, IVisualizationContext, IVisualizer) 
  ↑ used by
Infrastructure (SoundFlowAudioPlayer, LevelMeterVisualizer, WaveformVisualizer, SpectrumVisualizer)
  ↓ implemented in  
Web (SignalRVisualizationService → VisualizerHub, BlazorVisualizationContext)
```

## 6. Configuration Service Integration ✅ COMPLETE WITH ROOTDIR SUPPORT

### Completed:
- [x] Add HttpClient base URL configuration to `appsettings.json`
- [x] Configure API client in Program.cs with default fallback
- [x] **Added `RootDir` configuration option to `ConfigurationStorageOptions`**
- [x] **Implemented `ResolvePath()` method to resolve paths relative to RootDir**
- [x] **Updated `ConfigurationServiceExtensions` to use RootDir**
- [x] **Added diagnostic logging for configuration paths**
- [x] **Added appsettings.json detection and warnings in Program.cs**
- [x] **Updated all appsettings.json files with RootDir field**

### Configuration Changes:
The `appsettings.json` now includes `RootDir` in the `ConfigurationStorage` section:
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

### Implementation Details:
- **RootDir**: Base directory for all configuration paths. Defaults to application base directory if empty.
- **Path Resolution**: All relative paths (StoragePath, log paths, etc.) are resolved relative to RootDir
- **Diagnostics**: Application logs configuration file location at startup
- **Warnings**: Clear warnings printed if appsettings.json is not found, showing:
  - Expected location
  - Application base directory
  - Current working directory

### Remaining Work:
- [ ] Add SignalR hub configuration if needed
- [ ] Add Cast device discovery settings
- [ ] Add visualization settings (update interval, smoothing, etc.)
- [ ] Document all configuration options in README

## 7. Error Handling and User Feedback ⚠️ PARTIALLY COMPLETE

### Completed:
- [x] MudBlazor Snackbar configured and used in components
- [x] Try-catch blocks in all async operations
- [x] User-friendly error messages in AudioSetupPanel
- [x] User-friendly error messages in SystemTestPanel
- [x] User-friendly error messages in NowPlayingPanel
- [x] Logging of errors via ILogger

### Remaining Work:
- [ ] Add error boundaries for each major component
- [ ] Implement retry logic for failed API calls
- [ ] Handle offline scenarios gracefully
- [ ] Add loading indicators for async operations (e.g., Cast device discovery)
- [ ] Implement connection status indicator for SignalR
- [ ] Add global error handler

### Current Implementation:
Basic error handling is in place with Snackbar notifications, but advanced error handling features are missing.

## 8. Performance Optimization ❌ NOT STARTED

### Recommendations:
- [ ] Use `@rendermode InteractiveServer` only where needed (currently used globally)
- [ ] Implement debouncing for volume/balance sliders
- [ ] Cache Cast device list with periodic refresh
- [ ] Optimize FFT data transfer (consider binary format instead of float[])
- [ ] Add connection throttling for SignalR
- [ ] Implement virtual scrolling for large lists (if applicable)
- [ ] Optimize timer intervals (currently 1s for header, 5s for test panel, 1s for now playing)

### Notes:
Performance optimizations should be done after core functionality is complete and tested.

## 9. Testing ⚠️ BASIC TESTS PASSING

### Current Status:
- [x] All 80 existing tests passing
- [x] Infrastructure layer tests complete
- [x] Core interfaces tested

### Remaining Work:
- [ ] Unit tests for component logic (Razor components)
- [ ] Integration tests for API calls
- [ ] SignalR hub connection tests  
- [ ] FFT data visualization tests
- [ ] Cast device discovery tests
- [ ] Error handling tests
- [ ] End-to-end UI tests

### Test Coverage Gaps:
- No tests for Blazor components (AudioSetupPanel, NowPlayingPanel, etc.)
- No tests for SignalRVisualizationService
- No integration tests for Web → API communication
- No tests for VisualizerHub

### Priority:
- **High:** Component unit tests (service injection, state management)
- **Medium:** Integration tests (API communication)
- **Low:** UI/E2E tests

## 10. Documentation ❌ NOT STARTED

### Required Documentation:
- [ ] API endpoint documentation (Swagger is available but needs enhancement)
- [ ] SignalR hub protocol documentation
- [ ] Configuration options documentation  
- [ ] Deployment guide for Raspberry Pi kiosk mode
- [ ] Troubleshooting guide
- [ ] Architecture diagram showing layer separation
- [ ] Component usage guide

### Notes:
- Swagger UI is available at `/swagger` when API is running
- In-code XML comments exist but need review
- README files exist in each project but need updates

## Priority Order (Updated)

### 1. **High Priority** (Missing Core Functionality):
   - [ ] FFT implementation for real audio visualization
   - [ ] Missing service methods (Skip, Seek, SignalStrength, Balance)
   - [ ] Real system monitoring (WiFi, CPU, Memory)
   - [ ] Component unit tests

### 2. **Medium Priority** (Enhanced Functionality):
   - [ ] Output device switching implementation
   - [ ] Vinyl and Local file playback
   - [ ] Performance optimizations (debouncing, caching)
   - [ ] Error handling improvements

### 3. **Low Priority** (Polish & Documentation):
   - [ ] Advanced error boundaries and retry logic
   - [ ] Configuration documentation
   - [ ] Deployment guides
   - [ ] E2E tests

## Work Completed Since Initial Commit

### ✅ Architecture Fixes (Critical)
1. Created `IVisualizationService` interface to break circular dependency
2. Implemented `SignalRVisualizationService` in Web layer
3. Updated `SoundFlowAudioPlayer` to use abstraction
4. Fixed all build errors and nullable warnings

### ✅ Component Integration (Partial)
1. AudioSetupPanel: Connected to services, transport controls work
2. NowPlayingPanel: Uses actual service methods for Radio/Spotify
3. SystemTestPanel: Full API integration complete
4. GlobalHeader: Basic status display working
5. VisualizationPanel: Architecture ready, needs FFT implementation

### ✅ Testing
1. All 80 existing tests passing
2. Build successful with no errors
3. Components compile and run without crashes

## Estimated Effort (Remaining Work)

- FFT Implementation: 8-12 hours (complex, requires new library integration)
- Missing Service Methods: 4-6 hours (interface changes, implementations, tests)
- Real System Monitoring: 3-4 hours (platform-specific code)
- Component Unit Tests: 6-8 hours  
- Output Device Switching: 2-3 hours
- Vinyl/Local Playback: 6-8 hours (needs implementation from scratch)
- Performance Optimization: 3-4 hours
- Documentation: 4-6 hours

**Total Remaining Estimate:** 36-51 hours

**Completed Work:** ~10-12 hours (architecture fixes, integration, testing)

## Breaking Changes

### None Required
All changes are additive and maintain backward compatibility with existing code.

## Known Issues

1. **FFT Visualization:** Placeholder data only (SoundFlow/MiniAudio limitation)
2. **Service Methods:** Several UI features disabled due to missing interface methods
3. **Balance Control:** Not implemented in IAudioPlayer
4. **Signal Strength:** Not exposed by IRaddyRadioService  
5. **System Monitoring:** Hardcoded values (needs platform-specific implementation)

## Next Steps (Recommended Order)

1. **Add Missing Service Methods** (Priority: HIGH)
   - Add to interfaces in Core
   - Implement in Infrastructure
   - Update tests
   - Enable in UI components

2. **Implement Real System Monitoring** (Priority: HIGH)
   - Add system status endpoint to API
   - Implement CPU/Memory/WiFi monitoring
   - Update GlobalHeader to use real data

3. **FFT Implementation** (Priority: HIGH)
   - Research FFT library options (MathNet.Numerics, Kiss FFT)
   - Implement audio sample capture
   - Integrate FFT processing
   - Test visualization

4. **Component Testing** (Priority: MEDIUM)
   - Add bUnit tests for Razor components
   - Add integration tests for API calls
   - Add SignalR hub tests

5. **Vinyl & Local Playback** (Priority: MEDIUM)
   - Design and implement vinyl input service
   - Implement local file browser and player
   - Add to UI

## 11. Recently Completed Features (November 2025)

### API Controllers ✅ COMPLETE
Added 5 new REST API controllers with Swagger documentation:

1. **ConfigurationController** (`/api/Configuration`)
   - Full CRUD operations for configuration management
   - Backup and restore functionality
   - 8 endpoints total

2. **NowPlayingController** (`/api/NowPlaying`)
   - Real-time playback metadata from all sources
   - Radio, Spotify, and general playback info
   - 3 endpoints total

3. **VisualizationController** (`/api/Visualization`)
   - FFT data generation control
   - Visualization type enumeration
   - 3 endpoints total

4. **StreamAudioController** (`/api/StreamAudio`)
   - Audio streaming endpoint discovery
   - MP3/WAV streaming URLs
   - 2 endpoints total

5. **MetadataController** (`/api/Metadata`)
   - Audio file metadata extraction using TagLib#
   - Format validation and support checking
   - 3 endpoints total

**Files Added:**
- `RadioConsole.API/Controllers/ConfigurationController.cs`
- `RadioConsole.API/Controllers/NowPlayingController.cs`
- `RadioConsole.API/Controllers/VisualizationController.cs`
- `RadioConsole.API/Controllers/StreamAudioController.cs`
- `RadioConsole.API/Controllers/MetadataController.cs`

### TagLib# Metadata Integration ✅ COMPLETE
Implemented comprehensive audio file metadata extraction:

**Features:**
- Extracts title, artist, album, artwork, lyrics, duration, bitrate, channels
- Supports 13+ audio formats (MP3, FLAC, AAC, OGG, WAV, WMA, etc.)
- Stream-based extraction for non-file sources
- Format validation and support checking

**Files Added:**
- `RadioConsole.Core/Interfaces/Audio/IMetadataService.cs`
- `RadioConsole.Infrastructure/Audio/TagLibMetadataService.cs`

**Files Modified:**
- `RadioConsole.Infrastructure/Audio/AudioServiceExtensions.cs` (registered service)
- `RadioConsole.Infrastructure/RadioConsole.Infrastructure.csproj` (added TagLibSharp package)

### Material 3 Keyboard Components ✅ COMPLETE
Created two touchscreen-optimized input components:

1. **NumericKeypad** (`NumericKeypad.razor`)
   - Large touch-friendly buttons (70px height)
   - LED-style display with validation
   - Configurable decimal support
   - Event callbacks for value changes and submission
   - Used for radio frequency entry

2. **TouchKeyboard** (`TouchKeyboard.razor`)
   - Full QWERTY layout with shift key
   - Auto-deactivates shift after character
   - LED-style display with blinking cursor
   - Support for special characters
   - Large touch targets (50px height)

**Files Added:**
- `RadioConsole.Web/Components/Shared/NumericKeypad.razor`
- `RadioConsole.Web/Components/Shared/TouchKeyboard.razor`

### USB Radio Control Panel ✅ COMPLETE
Comprehensive radio control interface:

**Features:**
- LED-style frequency display (green) with auto-formatting per band
- LED-style band display (orange)
- Tune Up/Down with band-specific steps (FM: 100kHz, AM: 10kHz, etc.)
- Scan Up/Down buttons (stubbed for future implementation)
- Independent radio volume control (0-100%)
- Frequency entry via NumericKeypad with band-based validation
- Band selector: FM (87.5-108.0 MHz), AM (0.53-1.71), SW (1.71-30.0), AIR (108.0-137.0), VHF (30.0-300.0)
- Status indicators for streaming and USB device detection

**Files Added:**
- `RadioConsole.Web/Components/Shared/RaddyRadioControlPanel.razor`

### Demo Page & Documentation ✅ COMPLETE
Created comprehensive demonstration page:

**Features:**
- Interactive component demonstrations
- Live event logging
- Complete parameter documentation
- Usage examples and feature lists
- 4 tabs: Radio Control, Numeric Keypad, Touch Keyboard, Documentation

**Files Added:**
- `RadioConsole.Web/Components/Pages/RadioDemo.razor`

**Files Modified:**
- `RadioConsole.Web/Components/Layout/NavMenu.razor` (added navigation link)

### Build & Test Status
- ✅ All projects build successfully (0 warnings, 0 errors)
- ✅ 110/119 tests passing (9 audio device tests fail in CI as expected)
- ✅ 0 CodeQL security alerts
- ✅ Code review completed and issues addressed

### Documentation
- ✅ UPDATES_COMPLETED.md created with comprehensive summary
- ✅ This file updated with completed features
- ✅ README.md needs update (see next section)

## Notes

- All UI components are fully functional with mock data
- No breaking changes should be needed to existing Phase 1-4 code
- Integration can be done incrementally, testing each component separately
- Consider using feature flags to enable/disable integrations during development
