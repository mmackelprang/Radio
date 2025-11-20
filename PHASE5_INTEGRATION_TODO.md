# Phase 5 Integration TODO

This document outlines the remaining integration work needed to connect the Phase 5 Blazor UI with the backend audio services implemented in Phases 1-4.

## Overview

The Phase 5 UI is complete and functional, but currently operates with mock/placeholder data. The following integrations are needed to connect the UI to actual audio services.

## 1. AudioSetupPanel Integration

**File:** `RadioConsole.Web/Components/Shared/AudioSetupPanel.razor`

### Required Work:
- [ ] Inject `IAudioPlayer` service
- [ ] Inject `IRaddyRadioService` for radio input
- [ ] Inject `ISpotifyService` for Spotify input  
- [ ] Inject `ICastAudioOutput` for Cast device discovery
- [ ] Wire input selector dropdown to switch between audio sources
- [ ] Wire output selector to switch between Local and Cast outputs
- [ ] Implement Cast device discovery and populate dropdown
- [ ] Connect transport controls (Play/Pause/Stop/Next/Previous) to `IAudioPlayer`
- [ ] Bind volume slider to `IAudioPlayer.SetVolume()`
- [ ] Bind balance slider to `IAudioPlayer.SetBalance()`
- [ ] Load current audio settings on initialization

### TODO Comments in Code:
```csharp
// TODO: Connect to audio service (lines 70, 77, 83, 89)
// TODO: Load available cast devices from cast service (line 94)
// TODO: Load current audio settings (line 95)
```

## 2. NowPlayingPanel Integration

**File:** `RadioConsole.Web/Components/Shared/NowPlayingPanel.razor`

### Required Work:
- [ ] Subscribe to audio source change events
- [ ] Display current radio frequency from `IRaddyRadioService.GetCurrentFrequency()`
- [ ] Display radio signal strength from `IRaddyRadioService.GetSignalStrength()`
- [ ] Display Spotify track info from `ISpotifyService.GetCurrentTrack()`
- [ ] Display Spotify album art URL
- [ ] Display Spotify lyrics (if available)
- [ ] Update vinyl playback status
- [ ] Display local file playback progress
- [ ] Implement real-time updates (consider SignalR or polling)

### TODO Comments in Code:
```csharp
// TODO: Subscribe to input source changes (line 93)
// TODO: Load current playing info based on input type (line 94)
```

## 3. SystemTestPanel Integration

**File:** `RadioConsole.Web/Components/Shared/SystemTestPanel.razor`

### Required Work:
- [ ] Configure HttpClient base address from appsettings.json
- [ ] Uncomment API calls to Phase 4 TestController endpoints:
  - `POST /api/test/tts` (TTS tests)
  - `POST /api/test/tone` (tone tests)
  - `POST /api/test/doorbell` (doorbell test)
  - `POST /api/test/broadcast` (broadcast test)
  - `GET /api/test/status` (system status)
- [ ] Implement actual CPU usage monitoring
- [ ] Implement actual memory usage monitoring
- [ ] Implement actual system uptime calculation
- [ ] Add real-time status refresh (polling every 5-10 seconds)

### TODO Comments in Code:
```csharp
// TODO: Load actual system status (line 126)
// TODO: Call actual status endpoint (line 134)
// TODO: Uncomment when API is available (lines 156, 188, 214, etc.)
```

## 4. GlobalHeader Integration

**File:** `RadioConsole.Web/Components/Shared/GlobalHeader.razor`

### Required Work:
- [ ] Implement actual WiFi status check using `NetworkInterface` API
- [ ] Implement system health monitoring:
  - CPU usage threshold check
  - Memory usage threshold check
  - Disk space check
  - Service availability check
- [ ] Update status icons based on real data
- [ ] Consider adding polling mechanism for status updates

### TODO Comments in Code:
```csharp
// TODO: Integrate with actual network status check (line 41)
// TODO: Integrate with actual system health monitoring (line 42)
```

## 5. VisualizationPanel Integration

**File:** `RadioConsole.Web/Components/Shared/VisualizationPanel.razor`
**File:** `RadioConsole.Web/Hubs/VisualizerHub.cs`

### Required Work:
- [ ] Implement FFT analysis in `IAudioPlayer` or create dedicated service
- [ ] Configure audio player to generate FFT data every 50ms
- [ ] Inject `IHubContext<VisualizerHub>` into audio player service
- [ ] Push FFT data to all connected clients via `SendFFTData(float[] fftData)`
- [ ] Ensure FFT data is normalized to 0-1 range
- [ ] Consider using 64 or 128 frequency bins for optimal visualization
- [ ] Handle audio player start/stop to enable/disable FFT generation

### Implementation Example:
```csharp
// In IAudioPlayer implementation or service:
private readonly IHubContext<VisualizerHub> _hubContext;
private readonly Timer _fftTimer;

public async Task StartAsync(IAudioSource source)
{
    // Start audio playback
    // ...
    
    // Start FFT timer
    _fftTimer = new Timer(async _ => 
    {
        var fftData = GenerateFFT(); // Implement this
        await _hubContext.Clients.All.SendAsync("ReceiveFFTData", fftData);
    }, null, 0, 50); // Every 50ms
}
```

## 6. Configuration Service Integration

### Required Work:
- [ ] Add HttpClient base URL configuration to `appsettings.json`
- [ ] Add API endpoint URLs to configuration
- [ ] Add SignalR hub configuration if needed
- [ ] Add Cast device discovery settings
- [ ] Add FFT generation settings (sample rate, bin count, etc.)

### Example appsettings.json:
```json
{
  "RadioConsole": {
    "ApiBaseUrl": "http://localhost:5100",
    "SignalR": {
      "VisualizerUpdateIntervalMs": 50
    },
    "Audio": {
      "FFT": {
        "BinCount": 64,
        "SampleRate": 44100
      }
    }
  }
}
```

## 7. Error Handling and User Feedback

### Required Work:
- [ ] Add error boundaries for each major component
- [ ] Implement retry logic for failed API calls
- [ ] Add user-friendly error messages using MudBlazor Snackbar
- [ ] Handle offline scenarios gracefully
- [ ] Add loading indicators for async operations
- [ ] Implement connection status indicator for SignalR

## 8. Performance Optimization

### Recommendations:
- [ ] Use `@rendermode InteractiveServer` only where needed
- [ ] Implement debouncing for volume/balance sliders
- [ ] Cache Cast device list with periodic refresh
- [ ] Optimize FFT data transfer (consider binary format)
- [ ] Add connection throttling for SignalR

## 9. Testing

### Required Tests:
- [ ] Unit tests for component logic
- [ ] Integration tests for API calls
- [ ] SignalR hub connection tests
- [ ] FFT data visualization tests
- [ ] Cast device discovery tests
- [ ] Error handling tests

## 10. Documentation

### Required Documentation:
- [ ] API endpoint documentation
- [ ] SignalR hub protocol documentation
- [ ] Configuration options documentation
- [ ] Deployment guide for Raspberry Pi kiosk mode
- [ ] Troubleshooting guide

## Priority Order

1. **High Priority** (Core Functionality):
   - AudioSetupPanel service integration
   - NowPlayingPanel data binding
   - VisualizationPanel FFT integration

2. **Medium Priority** (Testing & Monitoring):
   - SystemTestPanel API integration
   - GlobalHeader status monitoring

3. **Low Priority** (Polish):
   - Error handling improvements
   - Performance optimization
   - Additional documentation

## Estimated Effort

- AudioSetupPanel: 4-6 hours
- NowPlayingPanel: 3-4 hours
- VisualizationPanel/FFT: 6-8 hours
- SystemTestPanel: 2-3 hours
- GlobalHeader: 2-3 hours
- Configuration: 1-2 hours
- Testing: 4-6 hours

**Total Estimate:** 22-32 hours

## Notes

- All UI components are fully functional with mock data
- No breaking changes should be needed to existing Phase 1-4 code
- Integration can be done incrementally, testing each component separately
- Consider using feature flags to enable/disable integrations during development
