# UI Event Handling Architecture - Clarification

## Purpose
This document clarifies the architecture of UI event handling in the Radio Console application and explains why certain behaviors are expected.

## Architecture Overview

### Web UI (RadioConsole.Web)
The Blazor Web UI is a **standalone server** that:
- Hosts the Blazor Server interactive components
- Has its own dependency injection container
- Registers and uses audio services directly (AudioPlayer, RaddyRadioService, SpotifyService, etc.)
- Logs to `./logs/web-*.log`
- Listens on port 5200 (default)

### API Server (RadioConsole.API)
The REST API is a **separate server** that:
- Provides REST endpoints for external control
- Has its own dependency injection container
- Registers and uses the same audio services
- Logs to `./logs/api-*.log`
- Listens on port 5100 (default)

### Service Registration
Both servers register the same services (from RadioConsole.Infrastructure):
```csharp
// In RadioConsole.Web/Program.cs
builder.Services.AddAudioServices(builder.Configuration);
builder.Services.AddInputServices();

// In RadioConsole.API/Program.cs (same services)
builder.Services.AddAudioServices(builder.Configuration);
builder.Services.AddInputServices();
```

## UI Event Flow

### When a User Clicks a UI Button

**Example: Play/Pause button in AudioSetupPanel**

1. User clicks the Play/Pause button in the browser
2. Blazor Server receives the click event via SignalR WebSocket
3. `AudioSetupPanel.OnPlayPause()` method executes **in the Web process**
4. The method logs to `ILogger<AudioSetupPanel>` → writes to `./logs/web-*.log`
5. The method calls `await AudioPlayer.StopAsync()` or `PlayAsync()`
6. The AudioPlayer service (injected in Web) executes **in the Web process**
7. Audio playback starts/stops via SoundFlow library

**No API calls are made!** The Web UI does not call the API for button clicks.

### When External Systems Call the API

**Example: External system POSTs to `/api/audio/play`**

1. External system makes HTTP POST to API server
2. API server receives request and routes to controller
3. Controller logs to `ILogger<AudioController>` → writes to `./logs/api-*.log`
4. Controller calls `await AudioPlayer.PlayAsync()` 
5. The AudioPlayer service (injected in API) executes **in the API process**
6. Audio playback starts via SoundFlow library

## Why "No Activity in API Logs" is Expected

When testing the UI by clicking buttons:
- ✅ **Expected:** Activity in Web UI logs (`./logs/web-*.log`)
- ✅ **Expected:** Audio playback starts/stops
- ✅ **Expected:** UI updates (IsPlaying state changes, etc.)
- ❌ **Not Expected:** Activity in API logs (`./logs/api-*.log`)

The Web UI and API are **independent servers** with their own service instances. They do not communicate with each other for routine operations.

## When Web UI DOES Call the API

The Web UI only calls the API in specific scenarios:

### 1. Health Check at Startup
```csharp
// In Program.cs
var response = await httpClient.GetAsync("/health");
```
This checks if the API is running. The Web UI will exit if the API is not healthy.

### 2. Configuration Management (Future)
Some configuration operations may call API endpoints to ensure consistency.

### 3. External Device Status (Future)
Querying status of devices that the API manages.

## Verifying UI Interactivity

### To Confirm Clicks are Working:

1. **Check Web UI Logs:**
   ```bash
   tail -f ./logs/web-*.log
   ```
   You should see log entries when clicking buttons:
   - "Play/Pause toggled. IsPlaying: True"
   - "Stop pressed"
   - "Volume changed to: 75"

2. **Open Browser Console:**
   - Press F12 in browser
   - Click UI buttons
   - Look for any JavaScript errors
   - Verify SignalR connection is established

3. **Check Blazor Connection:**
   - In browser console, look for "Blazor Server connected"
   - No red connection errors
   - WebSocket should be in "Connected" state

4. **Verify Render Mode:**
   - Home.razor has `@rendermode InteractiveServer`
   - Child components inherit this mode automatically
   - All event handlers should work

## Troubleshooting UI Click Issues

If clicks truly aren't working (buttons don't respond):

### 1. Verify Blazor JavaScript Loaded
Check that `_framework/blazor.web.js` loaded successfully:
```html
<!-- In App.razor -->
<script src="_framework/blazor.web.js"></script>
```

### 2. Check Browser Console for Errors
- JavaScript errors
- Failed script loads
- SignalR connection failures
- Anti-forgery token errors

### 3. Verify Interactive Render Mode
All components used in MainLayout should be interactive because Home.razor has `@rendermode InteractiveServer`.

### 4. Check Network Connectivity
- Browser can reach Web server (port 5200)
- WebSocket connection is established
- No firewall blocking

### 5. Check for Competing Click Handlers
- CSS `pointer-events: none` blocking clicks
- Overlapping elements capturing events
- Z-index issues

## Expected Log Output

### Web UI Logs (./logs/web-*.log)
```
[14:55:00 INF] AudioSetupPanel initialized
[14:55:05 INF] Play/Pause toggled. IsPlaying: True
[14:55:10 INF] Volume changed to: 85
[14:55:15 INF] Stop pressed
```

### API Logs (./logs/api-*.log)
```
[14:55:00 INF] API server started
[14:55:01 INF] Health check endpoint accessed
```
(No activity from UI clicks - this is correct!)

## Architecture Benefits

This dual-server architecture provides:

1. **Separation of Concerns:** UI and API are independent
2. **Scalability:** Can scale Web UI and API separately
3. **Flexibility:** External systems can control audio via API
4. **Resilience:** UI works even if API is temporarily down (after initial health check)
5. **Development:** Can develop/test UI and API independently

## Future Considerations

### Potential Consolidation
The two servers could be merged into a single ASP.NET Core app hosting both:
- Blazor Server components
- REST API endpoints
- SignalR hubs

Benefits:
- Single service instance of audio services (no duplication)
- Simpler deployment
- Shared state between UI and API

Trade-offs:
- Less separation of concerns
- Harder to scale independently
- Single point of failure

### Current Recommendation
Keep the dual-server architecture during development for flexibility. Consider consolidation for production if:
- Only one user/device accesses the system
- State sharing between UI and API becomes important
- Deployment simplicity is prioritized

## Related Documentation

- **RadioPlan_v3.md** - Overall architecture
- **UI_PANELS_PLAN.md** - UI panel architecture and development plan
- **Program.cs** (Web) - Web UI configuration and service registration
- **Program.cs** (API) - API configuration and service registration

---

**Last Updated:** 2025-11-21  
**Version:** 1.0  
