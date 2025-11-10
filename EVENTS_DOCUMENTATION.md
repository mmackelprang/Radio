# Event-Driven Audio Inputs Documentation

## Overview

The RadioConsole system supports event-driven audio inputs alongside traditional music/streaming inputs. Event-driven inputs are designed for short-duration, high-priority audio notifications such as doorbells, phone rings, timers, and reminders.

## Architecture

### Key Components

#### 1. AudioInputType Enum
Distinguishes between different types of audio inputs:
- **Music**: Traditional streaming sources (Radio, Spotify, etc.)
- **Event**: Short-duration, priority-based notifications

#### 2. IAudioInput Interface
The unified interface for all audio inputs now includes event-specific properties:
- **InputType**: Distinguishes between Music and Event types
- **Priority**: Priority level (Low, Medium, High, Critical)
- **Duration**: Expected duration of the audio
- **AudioDataAvailable**: Event that fires when PCM audio data is available
- **AllowConcurrent**: Whether this stream can play concurrently with others

#### 3. BaseAudioInput Class
Base implementation providing common functionality for all audio inputs:
- Event management through `TriggerAudioEventAsync()` and `SimulateTriggerAsync()`
- Playback control (Start, Stop, Pause, Resume)
- Volume management
- Repeat/loop configuration
- Display and configuration interfaces

**Note**: The `BaseEventAudioInput` class has been removed. All event functionality is now integrated into `BaseAudioInput`.

#### 4. AudioPriorityManager Service
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

### Generic Event Inputs

The system uses generic, composable input types that can be configured for any event scenario:

#### 1. FileAudioInput
Plays audio files (MP3, WAV) for event notifications.
- **Use Cases**: Doorbell sounds, phone rings, alert tones
- **Priority**: Configurable (Low to Critical)
- **Duration**: Determined by file length
- **Features**: Repeat/loop support, volume control, pause/resume

#### 2. TtsAudioInput
Text-to-speech audio using eSpeak TTS engine.
- **Use Cases**: Voice announcements, timer notifications, reminders
- **Priority**: Configurable (typically Medium)
- **Duration**: Calculated based on text length
- **Features**: Offline TTS, configurable voice parameters

#### 3. CompositeAudioInput
Combines multiple audio sources (files and TTS) into a single event.
- **Use Cases**: Complex notifications with sound + voice, multi-part announcements
- **Priority**: Configurable (Low to Critical)
- **Duration**: Sum of all components (serial) or max duration (concurrent)
- **Features**: 
  - Serial or concurrent playback
  - Per-source volume control
  - Per-source repeat configuration
  - Mixed file and TTS inputs

### Legacy Event Inputs (Deprecated)

The following specific event inputs have been deprecated in favor of the generic types above:
- **DoorbellEventInput** → Use `FileAudioInput` or `CompositeAudioInput`
- **TelephoneRingingEventInput** → Use `FileAudioInput` or `CompositeAudioInput`
- **GoogleBroadcastEventInput** → Use `TtsAudioInput` or `CompositeAudioInput`
- **TimerExpiredEventInput** → Use `FileAudioInput` or `CompositeAudioInput`
- **ReminderEventInput** → Use `TtsAudioInput` or `CompositeAudioInput`
- **TextEventInput** → Use `TtsAudioInput`

See [AUDIO_INPUT_MIGRATION.md](AUDIO_INPUT_MIGRATION.md) for migration details.

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

## Configuration Examples

### Example 1: Doorbell Event with Sound and Announcement

```csharp
var doorbellEvent = new CompositeAudioInput(
    "doorbell_event",
    "Doorbell Ring",
    EventPriority.High,
    true, // Serial playback
    environmentService,
    storage);

// Add doorbell sound
doorbellEvent.AddFileInput("/sounds/doorbell.mp3", volume: 1.0);

// Add voice announcement
doorbellEvent.AddTtsInput("Someone is at the door", ttsService, volume: 0.9);

await doorbellEvent.InitializeAsync();
```

### Example 2: Timer with Multiple Beeps

```csharp
var timerEvent = new CompositeAudioInput(
    "timer_event",
    "Timer Expired",
    EventPriority.Medium,
    false, // Concurrent playback
    environmentService,
    storage);

// Add repeating beep sound
timerEvent.AddFileInput("/sounds/beep.wav", volume: 1.0, repeatCount: 3);

await timerEvent.InitializeAsync();
```

### Example 3: Emergency Alert

```csharp
var emergencyAlert = new CompositeAudioInput(
    "emergency_alert",
    "Emergency Alert",
    EventPriority.Critical,
    true, // Serial playback
    environmentService,
    storage);

// Add alarm sound
emergencyAlert.AddFileInput("/sounds/alarm.mp3", volume: 1.0, repeatCount: 2);

// Add emergency message
emergencyAlert.AddTtsInput("Emergency alert! Please evacuate immediately!", 
    ttsService, volume: 1.0);

await emergencyAlert.InitializeAsync();
```

## TTS Configuration

TtsAudioInput requires eSpeak TTS to be installed and configured. See [ESPEAK_TTS_SETUP.md](ESPEAK_TTS_SETUP.md) for detailed setup instructions.

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

1. **Event Creation**: Create an event input using FileAudioInput, TtsAudioInput, or CompositeAudioInput
2. **Registration**: Register the event input with AudioPriorityManager
3. **Trigger**: Trigger the event via `SimulateTriggerAsync()` or automatic triggers
4. **Priority Check**: AudioPriorityManager checks if event can interrupt current audio
5. **Volume Save**: Current volume levels are saved for all active outputs
6. **Volume Adjust**: Background audio is reduced or muted (configurable)
7. **Event Plays**: Event audio plays for its specified duration
8. **Volume Restore**: Original volumes are restored after completion

### Priority Interruption Rules

- Higher priority events interrupt lower priority events
- Equal or lower priority events are ignored if a higher priority event is playing
- Events automatically restore volumes after completion
- Critical priority always interrupts

## Configuration

### AudioPriorityManager Configuration

```csharp
var config = new AudioPriorityManagerConfig
{
    VolumeReductionLevel = 0.1,  // 0.0 = mute, 1.0 = no change
    MuteBackgroundAudio = false   // true = complete mute, false = use VolumeReductionLevel
};
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

The recommended approach is to use the generic `CompositeAudioInput`, `FileAudioInput`, or `TtsAudioInput` classes. However, if you need custom behavior, you can create a new class:

1. **Using CompositeAudioInput (Recommended)**

```csharp
// Create a factory method or service to configure your event
public static CompositeAudioInput CreateCustomEvent(
    IEnvironmentService environmentService,
    IStorage storage,
    ITtsService ttsService)
{
    var customEvent = new CompositeAudioInput(
        "custom_event",
        "Custom Event",
        EventPriority.Medium,
        true, // Serial playback
        environmentService,
        storage);

    // Add sound file
    customEvent.AddFileInput("/sounds/custom.mp3", volume: 1.0);
    
    // Add voice announcement
    customEvent.AddTtsInput("Custom event triggered", ttsService, volume: 0.9);
    
    return customEvent;
}
```

2. **Create Custom Event Input Class (Advanced)**

If you need custom behavior beyond what CompositeAudioInput provides:

```csharp
public class CustomEventInput : BaseAudioInput
{
    public override string Id => "custom_event";
    public override string Name => "Custom Event";
    public override string Description => "My custom event";
    public override AudioInputType InputType => AudioInputType.Event;
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

3. **Register in Program.cs**

```csharp
builder.Services.AddSingleton<IAudioInput, CustomEventInput>();
```

The AudioPriorityManager will automatically register it during startup.

4. **Trigger the Event**

```csharp
// From within your event input class
await SimulateTriggerAsync(metadata);
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
