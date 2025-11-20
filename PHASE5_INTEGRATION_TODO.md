# Phase 5 Integration TODO - Status Update

**Last Updated:** November 20, 2025

This document outlines the remaining integration work needed to connect the Phase 5 Blazor UI with the backend audio services implemented in Phases 1-4.

## Current Status Summary

### ✅ Completed (Build Infrastructure)
- Architecture fixed: Created `IVisualizationService` abstraction to prevent circular dependencies
- All service injections working in components
- Build successful with 80/80 tests passing
- API endpoints functional via SystemTestPanel

### ⚠️ Partially Complete (Functional but Limited)
- AudioSetupPanel: UI works, limited backend integration
- NowPlayingPanel: UI works, using available service methods  
- SystemTestPanel: Full API integration complete
- GlobalHeader: Basic functionality, needs real system monitoring
- VisualizationPanel: Architecture ready, needs FFT implementation

### ❌ Not Started
- FFT data generation from audio stream
- Advanced service methods (seek, skip, etc.)
- Real-time system monitoring
- Performance optimizations

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

## 5. VisualizationPanel Integration ⚠️ ARCHITECTURE READY

**File:** `RadioConsole.Web/Components/Shared/VisualizationPanel.razor`  
**File:** `RadioConsole.Web/Hubs/VisualizerHub.cs`  
**File:** `RadioConsole.Core/Interfaces/Audio/IVisualizationService.cs` ✨ **NEW**  
**File:** `RadioConsole.Web/Services/SignalRVisualizationService.cs` ✨ **NEW**

### Completed:
- [x] Created `IVisualizationService` interface in Core layer
- [x] Implemented `SignalRVisualizationService` in Web layer  
- [x] Updated `SoundFlowAudioPlayer` to accept `IVisualizationService` dependency
- [x] SignalR hub configured and mapped
- [x] Canvas visualization rendering working
- [x] FFT timer mechanism in place (placeholder data)
- [x] Fixed circular dependency issue

### Remaining Work:
- [ ] Implement actual FFT analysis from audio stream (SoundFlow limitation)
  - SoundFlow/MiniAudio doesn't provide built-in FFT
  - Need to integrate FFT library (e.g., Kiss FFT, MathNet.Numerics)
  - Capture audio samples from playback stream
  - Process samples through FFT algorithm
- [ ] Configure audio player to generate FFT data every 50ms
- [ ] Enable/disable FFT generation based on audio player state
- [ ] Ensure FFT data is normalized to 0-1 range
- [ ] Optimize FFT bin count for visualization (currently 256, could use 64/128)

### Current Limitations:
- FFT data is placeholder random values
- No actual audio analysis happening
- FFT generation requires integration with audio processing library

### Implementation Notes:
The architecture is now correct with proper separation of concerns:
```
Core (IVisualizationService) 
  ↑ used by
Infrastructure (SoundFlowAudioPlayer)
  ↓ implemented in  
Web (SignalRVisualizationService → VisualizerHub)

## 6. Configuration Service Integration ⚠️ PARTIALLY COMPLETE

### Completed:
- [x] Add HttpClient base URL configuration to `appsettings.json`
- [x] Configure API client in Program.cs with default fallback

### Remaining Work:
- [ ] Add SignalR hub configuration if needed
- [ ] Add Cast device discovery settings
- [ ] Add FFT generation settings (sample rate, bin count, etc.)
- [ ] Document all configuration options

### Current Configuration:
The `appsettings.json` should include:
```json
{
  "RadioConsole": {
    "ApiBaseUrl": "http://localhost:5100"
  }
}
```

### Recommended Additions:
```json
{
  "RadioConsole": {
    "ApiBaseUrl": "http://localhost:5100",
    "SignalR": {
      "VisualizerUpdateIntervalMs": 50,
      "ReconnectDelayMs": 5000
    },
    "Audio": {
      "FFT": {
        "BinCount": 64,
        "SampleRate": 44100,
        "UpdateIntervalMs": 50
      }
    },
    "Cast": {
      "DiscoveryTimeoutMs": 5000,
      "RefreshIntervalMs": 30000
    }
  }
}
```

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

## Notes

- All UI components are fully functional with mock data
- No breaking changes should be needed to existing Phase 1-4 code
- Integration can be done incrementally, testing each component separately
- Consider using feature flags to enable/disable integrations during development
