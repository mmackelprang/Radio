# Audio Priority Service Developer Guide

## Overview

The Audio Priority Service (`IAudioPriorityService`) provides automatic audio ducking functionality for the Radio Console. This service ensures that high-priority audio events (such as TTS notifications, doorbell alerts, or phone rings) automatically reduce the volume of low-priority audio (such as radio, Spotify, or vinyl playback).

## Core Concepts

### Audio Priority Levels

The system supports two priority levels defined in `AudioPriority` enum:

- **`AudioPriority.Low`** - Background music sources (Radio, Spotify, Vinyl, Local Files)
- **`AudioPriority.High`** - Notification sources (TTS, Doorbell, Phone Ring, Google Broadcasts)

### Ducking Behavior

When a high-priority audio source starts:
1. All registered low-priority sources have their volume reduced to the configured duck percentage (default: 20%)
2. The high-priority audio plays at full volume
3. When all high-priority sources stop, low-priority sources are restored to their original volume

### Volume Fade

The service implements smooth volume transitions (300ms fade duration by default) to avoid jarring audio changes.

## Using the Priority Service

### 1. Register Audio Sources

Before using the priority service, register each audio source with its priority level:

```csharp
// Register a low-priority music source
await _priorityService.RegisterSourceAsync("radio", AudioPriority.Low);
await _priorityService.RegisterSourceAsync("spotify", AudioPriority.Low);

// Register high-priority event sources
await _priorityService.RegisterSourceAsync("doorbell", AudioPriority.High);
await _priorityService.RegisterSourceAsync("tts-notification", AudioPriority.High);
```

### 2. Trigger High Priority Events

When a high-priority audio event starts, notify the service:

```csharp
// Doorbell rings - duck the music
await _priorityService.OnHighPriorityStartAsync("doorbell");

// Play your high-priority audio here...
await _audioPlayer.PlayAsync("doorbell", doorbellAudioStream);

// When finished, restore music volume
await _priorityService.OnHighPriorityEndAsync("doorbell");
```

### 3. Unregister Sources

When an audio source is no longer needed, unregister it:

```csharp
await _priorityService.UnregisterSourceAsync("radio");
```

## API Integration

The Audio Priority Service is exposed through REST API endpoints for external control.

### Register a Source

```http
POST /api/audiopriority/sources/register
Content-Type: application/json

{
  "sourceId": "radio",
  "priority": 0  // 0 = Low, 1 = High
}
```

### Trigger High Priority Event

```http
POST /api/audiopriority/events/high-priority-start
Content-Type: application/json

{
  "sourceId": "doorbell"
}
```

### End High Priority Event

```http
POST /api/audiopriority/events/high-priority-end
Content-Type: application/json

{
  "sourceId": "doorbell"
}
```

### Configure Duck Percentage

```http
POST /api/audiopriority/config/duck-percentage
Content-Type: application/json

{
  "duckPercentage": 0.3  // 30% volume when ducked
}
```

### Get Current Status

```http
GET /api/audiopriority/status
```

Response:
```json
{
  "isHighPriorityActive": true,
  "duckPercentage": 0.2,
  "message": "High priority audio is active, low priority sources are ducked"
}
```

## Best Practices

### 1. Always Use Try-Finally for High Priority Events

Ensure volume is always restored even if an error occurs:

```csharp
try
{
  await _priorityService.OnHighPriorityStartAsync("notification");
  await PlayNotificationAudio();
}
finally
{
  await _priorityService.OnHighPriorityEndAsync("notification");
}
```

### 2. Register Sources Early

Register all audio sources during application initialization to avoid runtime registration errors.

### 3. Use Consistent Source IDs

Define source IDs as constants to avoid typos:

```csharp
public static class AudioSourceIds
{
  public const string Radio = "radio";
  public const string Spotify = "spotify";
  public const string Doorbell = "doorbell";
  public const string TtsNotification = "tts-notification";
}
```

### 4. Multiple High Priority Sources

The service handles multiple concurrent high-priority sources correctly:
- Volume is only ducked when the first high-priority source starts
- Volume is only restored when the last high-priority source ends

```csharp
// Both can be active simultaneously
await _priorityService.OnHighPriorityStartAsync("doorbell");
await _priorityService.OnHighPriorityStartAsync("tts");

// Volume only restores after both end
await _priorityService.OnHighPriorityEndAsync("doorbell");
// Music still ducked because TTS is active
await _priorityService.OnHighPriorityEndAsync("tts");
// Now music volume is restored
```

## Configuration

### Duck Percentage

The duck percentage determines what volume level low-priority sources are reduced to:

- Default: `0.2` (20% of original volume)
- Range: `0.0` to `1.0`
- Configure via API or directly:

```csharp
await _priorityService.SetDuckPercentageAsync(0.3f); // 30%
```

### Fade Duration

The fade duration is currently hardcoded to 300ms. To modify:

```csharp
// In AudioPriorityService.cs
private const int FadeDurationMs = 300;
```

## Testing

The `SystemTestService` provides built-in testing for the priority service:

### Via API

```http
POST /api/test/tts
Content-Type: application/json

{
  "phrase": "This is a test notification",
  "voiceGender": "female",
  "speed": 1.0
}
```

```http
POST /api/test/doorbell
```

### Via Code

```csharp
// Inject ISystemTestService
await _systemTestService.TriggerTtsAsync("Test notification");
await _systemTestService.TriggerDoorbellAsync();
```

## Architecture Integration

### Service Registration

The priority service is registered in `AudioServiceExtensions.cs`:

```csharp
services.AddSingleton<IAudioPriorityService, AudioPriorityService>();
```

### Dependencies

The service requires:
- `IAudioPlayer` - For controlling source volumes
- `ILogger<AudioPriorityService>` - For logging

### Thread Safety

The service uses `SemaphoreSlim` to ensure thread-safe operations when managing priority states.

## Troubleshooting

### Problem: Volume Not Ducking

**Check:**
1. Source is registered: `await _priorityService.RegisterSourceAsync(...)`
2. Priority is set correctly (High for events, Low for music)
3. `OnHighPriorityStartAsync` is called before playing audio

### Problem: Volume Not Restoring

**Check:**
1. `OnHighPriorityEndAsync` is called after audio completes
2. No other high-priority sources are still active
3. Check logs for errors during volume restoration

### Problem: Multiple Sources Interfering

**Solution:** Use unique source IDs for each audio source. Reusing source IDs can cause unexpected behavior.

## Example: Complete Implementation

```csharp
public class NotificationService
{
  private readonly IAudioPriorityService _priorityService;
  private readonly IAudioPlayer _audioPlayer;
  private const string SourceId = "notification-service";

  public NotificationService(
    IAudioPriorityService priorityService,
    IAudioPlayer audioPlayer)
  {
    _priorityService = priorityService;
    _audioPlayer = audioPlayer;
  }

  public async Task InitializeAsync()
  {
    // Register as high priority
    await _priorityService.RegisterSourceAsync(SourceId, AudioPriority.High);
  }

  public async Task PlayNotificationAsync(Stream audioData)
  {
    try
    {
      // Duck low-priority audio
      await _priorityService.OnHighPriorityStartAsync(SourceId);
      
      // Play notification
      await _audioPlayer.PlayAsync(SourceId, audioData);
      
      // Wait for completion...
      await Task.Delay(/* audio duration */);
    }
    finally
    {
      // Always restore volume
      await _priorityService.OnHighPriorityEndAsync(SourceId);
    }
  }

  public async Task DisposeAsync()
  {
    await _priorityService.UnregisterSourceAsync(SourceId);
  }
}
```

## API Endpoints Summary

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/audiopriority/sources/register` | POST | Register an audio source |
| `/api/audiopriority/sources/unregister` | POST | Unregister an audio source |
| `/api/audiopriority/events/high-priority-start` | POST | Start a high-priority event |
| `/api/audiopriority/events/high-priority-end` | POST | End a high-priority event |
| `/api/audiopriority/config/duck-percentage` | GET | Get current duck percentage |
| `/api/audiopriority/config/duck-percentage` | POST | Set duck percentage |
| `/api/audiopriority/status` | GET | Get current priority status |

## See Also

- `IAudioPriorityService.cs` - Service interface
- `AudioPriorityService.cs` - Implementation
- `AudioPriorityController.cs` - REST API controller
- `SystemTestService.cs` - Testing service with examples
- `RadioPlan_v3.md` - Project architecture overview
