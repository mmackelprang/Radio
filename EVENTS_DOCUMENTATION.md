# Event-Driven Audio Inputs Documentation

## Overview

The RadioConsole system now supports event-driven audio inputs alongside traditional music/streaming inputs. Event-driven inputs are designed for short-duration, high-priority audio notifications such as doorbells, phone rings, timers, and reminders.

## Architecture

### Key Components

#### 1. AudioInputType Enum
Distinguishes between different types of audio inputs:
- **Music**: Traditional streaming sources (Radio, Spotify, etc.)
- **Event**: Short-duration, priority-based notifications

#### 2. IEventAudioInput Interface
Extends `IAudioInput` with event-specific functionality:
- **Priority**: Priority level (Low, Medium, High, Critical)
- **Duration**: Expected duration of the event audio
- **AudioEventTriggered**: Event that fires when the audio should play

#### 3. AudioPriorityManager Service
Manages multiple audio sources with priority-based interruption:
- Monitors registered event inputs
- Saves volume states when events occur
- Reduces/mutes background audio during events
- Restores volumes after events complete
- Uses async/await for all operations
- Provides comprehensive exception handling

### Event Priority Levels

```csharp
public enum EventPriority
{
    Low,      // Informational events
    Medium,   // Standard notifications (timers, reminders, broadcasts)
    High,     // Urgent notifications (doorbell, phone ring)
    Critical  // Emergency alerts
}
```

## Available Event Input Modules

### 1. DoorbellEventInput
- **ID**: `doorbell_event`
- **Priority**: High
- **Duration**: 3 seconds
- **Use Case**: Doorbell ring notifications (e.g., Wyze doorbell integration)

### 2. TelephoneRingingEventInput
- **ID**: `telephone_event`
- **Priority**: High
- **Duration**: 5 seconds
- **Use Case**: Phone ringing notifications

### 3. GoogleBroadcastEventInput
- **ID**: `google_broadcast_event`
- **Priority**: Medium
- **Duration**: Variable (based on message length)
- **Use Case**: Google Home broadcast messages

### 4. TimerExpiredEventInput
- **ID**: `timer_event`
- **Priority**: Medium
- **Duration**: 3 seconds
- **Use Case**: Kitchen timer or other timer expiration

### 5. ReminderEventInput
- **ID**: `reminder_event`
- **Priority**: Medium
- **Duration**: 2 seconds
- **Use Case**: Calendar reminders or scheduled notifications

### 6. TextEventInput
- **ID**: `text_event`
- **Priority**: Medium
- **Duration**: Dynamic (calculated based on text length)
- **Use Case**: Text-to-speech announcements using eSpeak TTS
- **TTS Engine**: eSpeak/eSpeak-ng (local, offline TTS)
- **Special Features**: 
  - Converts any text to speech
  - Configurable voice, speed, pitch, and volume
  - Works offline on Raspberry Pi
  - Lightweight and fast
  - Perfect for custom announcements and notifications

**Configuration:**
TextEventInput requires eSpeak TTS to be installed and configured. See [ESPEAK_TTS_SETUP.md](ESPEAK_TTS_SETUP.md) for detailed setup instructions.

Common configuration parameters (in `appsettings.json`):
```json
{
  "ESpeakTts": {
    "Voice": "en-us",
    "Speed": 175,
    "Pitch": 50,
    "Volume": 100
  }
}
```

## How It Works

### Event Flow

1. **Event Occurs**: An event input triggers its `AudioEventTriggered` event
2. **Priority Check**: AudioPriorityManager checks if event can interrupt current audio
3. **Volume Save**: Current volume levels are saved for all active outputs
4. **Volume Adjust**: Background audio is reduced or muted (configurable)
5. **Event Plays**: Event audio plays for its specified duration
6. **Volume Restore**: Original volumes are restored after completion

### Priority Interruption Rules

- Higher priority events interrupt lower priority events
- Equal or lower priority events are ignored if a higher priority event is playing
- Events automatically restore volumes after completion

## Configuration

### AudioPriorityManager Configuration

```json
{
  "VolumeReductionLevel": 0.1,  // 0.0 = mute, 1.0 = no change
  "MuteBackgroundAudio": false   // true = complete mute, false = use VolumeReductionLevel
}
```

**Default Configuration:**
- `VolumeReductionLevel`: 0.1 (10% of original volume)
- `MuteBackgroundAudio`: false

## API Endpoints

### Get All Inputs (Including Events)
```
GET /api/audio/inputs
```

Response includes `InputType` field to distinguish Music vs Event inputs.

### Get Priority Manager State
```
GET /api/audio/priority/state
```

Returns:
- Current event playing (if any)
- Registered event inputs
- Configuration settings

### Update Priority Configuration
```
POST /api/audio/priority/config
Content-Type: application/json

{
  "volumeReductionLevel": 0.2,
  "muteBackgroundAudio": false
}
```

### Trigger Event Manually
```
POST /api/audio/events/{eventId}/trigger
Content-Type: application/json

{
  "metadata": {
    "location": "Front Door",
    "additionalInfo": "value"
  }
}
```

## Example Usage

### Example 1: Doorbell Ring

```http
POST /api/eventsexample/doorbell/ring?location=Front%20Door
```

**What Happens:**
1. Currently playing music volume is saved
2. Music volume reduces to 10% (or mutes, based on config)
3. Doorbell chime plays for 3 seconds
4. Music volume restores to original level

**Response:**
```json
{
  "message": "Doorbell ring simulated successfully",
  "location": "Front Door",
  "priority": "High",
  "duration": 3.0,
  "timestamp": "2024-11-08T21:45:00Z"
}
```

### Example 2: Reminder Notification

```http
POST /api/eventsexample/reminder/trigger?message=Take%20medication
```

**What Happens:**
1. Volume states saved
2. Background audio reduced
3. Reminder plays for 2 seconds
4. Volumes restored

**Response:**
```json
{
  "message": "Reminder simulated successfully",
  "reminderMessage": "Take medication",
  "priority": "Medium",
  "duration": 2.0,
  "timestamp": "2024-11-08T21:45:30Z"
}
```

### Example 3: Text-to-Speech Announcement (Hello World)

```http
POST /api/eventsexample/text/helloworld
```

**What Happens:**
1. eSpeak TTS generates speech from "Hello World"
2. Volume states saved
3. Background audio reduced
4. Speech plays (duration based on text length)
5. Volumes restored

**Response:**
```json
{
  "message": "Text announcement triggered successfully",
  "text": "Hello World",
  "priority": "Medium",
  "estimatedDuration": 1.5,
  "timestamp": "2024-11-09T21:30:00Z"
}
```

### Example 4: Custom Text Announcement

```http
POST /api/eventsexample/text/announce?text=Welcome%20home.%20The%20temperature%20is%2072%20degrees.
```

**What Happens:**
1. eSpeak TTS converts the custom text to speech
2. Audio event system handles the announcement
3. Background music volume is reduced during announcement
4. Speech is played through speakers
5. Original volume restored after completion

**Response:**
```json
{
  "message": "Text announcement triggered successfully",
  "text": "Welcome home. The temperature is 72 degrees.",
  "priority": "Medium",
  "estimatedDuration": 3.8,
  "timestamp": "2024-11-09T21:35:00Z"
}
```

### Example 5: Priority System Test

```http
POST /api/eventsexample/test/priority
```

Tests the priority system by triggering multiple events in sequence, demonstrating how higher priority events interrupt lower priority ones.

## Integration Examples

### Integrating a New Event Source

1. **Create Event Input Class**

```csharp
public class CustomEventInput : BaseEventAudioInput
{
    public override string Id => "custom_event";
    public override string Name => "Custom Event";
    public override string Description => "My custom event";
    public override EventPriority Priority => EventPriority.Medium;
    public override TimeSpan? Duration => TimeSpan.FromSeconds(4);

    public CustomEventInput(IEnvironmentService environmentService, IStorage storage)
        : base(environmentService, storage)
    {
    }

    public override async Task InitializeAsync()
    {
        await _configuration.LoadAsync();
        IsAvailable = _environmentService.IsSimulationMode;
        _display.UpdateStatus("Custom Event Ready");
    }

    public override async Task StartAsync()
    {
        IsActive = true;
        _display.UpdateStatus("Playing custom event");
        await Task.CompletedTask;
    }

    public override Task StopAsync()
    {
        IsActive = false;
        return Task.CompletedTask;
    }

    public override Task<Stream?> GetAudioStreamAsync()
    {
        // Return audio stream for the event
        return Task.FromResult<Stream?>(new MemoryStream());
    }
}
```

2. **Register in Program.cs**

```csharp
builder.Services.AddSingleton<IAudioInput, CustomEventInput>();
```

The AudioPriorityManager will automatically register it during startup.

3. **Trigger the Event**

```csharp
// From within your event input class
await TriggerAudioEventAsync(metadata);
```

### Webhook Integration Example

For real-world integrations (e.g., Wyze doorbell), create a webhook endpoint:

```csharp
[HttpPost("webhook/doorbell")]
public async Task<IActionResult> DoorbellWebhook([FromBody] DoorbellWebhookPayload payload)
{
    var doorbellInput = _audioInputs.OfType<DoorbellEventInput>().FirstOrDefault();
    
    if (doorbellInput?.IsAvailable == true)
    {
        await doorbellInput.SimulateDoorbellRingAsync(payload.Location);
        return Ok();
    }
    
    return BadRequest("Doorbell not available");
}
```

## Testing in Simulation Mode

All event inputs work in simulation mode, allowing development without hardware:

1. **Check Environment**
   - The system automatically detects non-Raspberry Pi environments
   - Event inputs are available in simulation mode

2. **Use Example Endpoints**
   - `/api/eventsexample/doorbell/ring`
   - `/api/eventsexample/reminder/trigger`
   - `/api/eventsexample/test/priority`

3. **Monitor Logs**
   - AudioPriorityManager logs all event handling
   - Check console output for volume saves/restores

## PulseAudio Integration

The AudioPriorityManager is designed for Raspberry Pi 5 with PulseAudio:

### Current Implementation
- Mock volume retrieval/setting for cross-platform development
- Simulation mode for testing without hardware

### Production Implementation (TODO)
Replace mock methods with actual PulseAudio commands:

```csharp
private async Task<double> GetOutputVolumeAsync(IAudioOutput output)
{
    // Use pactl to get actual volume
    var process = Process.Start(new ProcessStartInfo
    {
        FileName = "pactl",
        Arguments = $"get-sink-volume {output.Id}",
        RedirectStandardOutput = true
    });
    
    var output = await process.StandardOutput.ReadToEndAsync();
    // Parse volume from output
    return parsedVolume;
}
```

## Troubleshooting

### Event Not Triggering
- Check if event input is registered: `GET /api/audio/priority/state`
- Verify event input is available: `GET /api/audio/inputs`
- Check logs for error messages

### Volume Not Restoring
- AudioPriorityManager includes exception handling
- Check logs for errors during volume restore
- Verify outputs are active when event occurs

### Events Being Ignored
- Check priority levels - lower priority events don't interrupt higher ones
- Verify timing - events may complete before you check status

## Performance Considerations

- **Async Operations**: All audio operations use async/await
- **Semaphore Protection**: Single semaphore prevents concurrent event processing
- **Exception Safety**: Volumes restore even if errors occur
- **Minimal Latency**: Volume adjustments happen immediately

## Future Enhancements

1. **Multi-room Support**: Coordinate events across multiple devices
2. **Event Queue**: Queue multiple events instead of dropping them
3. **Custom Priority Rules**: User-configurable priority overrides
4. **Volume Fade**: Smooth volume transitions instead of instant changes
5. **Event History**: Track and display recent event audio playback
6. **Smart Ducking**: Context-aware volume reduction based on audio content

## Security Considerations

- Event triggering endpoints should be secured with authentication
- Webhook endpoints should validate signatures
- Rate limiting recommended for event triggers
- Input validation for all metadata fields

## Related Documentation

- [ARCHITECTURE.md](ARCHITECTURE.md) - Overall system architecture
- [README.md](README.md) - Project overview and setup
- [DEVELOPMENT.md](DEVELOPMENT.md) - Development guidelines
