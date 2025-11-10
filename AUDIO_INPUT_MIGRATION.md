# Audio Input Migration Guide

This document describes the migration from specific event input types to generic, reusable audio input components.

## Overview

The audio system has been refactored to use generic, composable input types instead of specialized event inputs. This makes the system more flexible and easier to maintain.

## Removed Input Types and Their Replacements

### RadioInput → UsbAudioInput

**Old:**
```csharp
var radioInput = new RadioInput(environmentService, storage);
```

**New:**
```csharp
// Use UsbAudioInput configured for the Raddy RF320 radio device
// Device number should match the USB audio device for the radio
var radioInput = new UsbAudioInput(
    deviceNumber: 0,  // Adjust based on your system
    name: "Radio",
    description: "SW/AM/FM Radio (Raddy RF320)",
    environmentService: environmentService,
    storage: storage
);
```

### DoorbellEventInput → FileAudioInput

**Old:**
```csharp
var doorbellInput = new DoorbellEventInput(environmentService, storage);
```

**New:**
```csharp
// Use FileAudioInput with your doorbell sound file
var doorbellInput = new FileAudioInput(
    filePath: "/path/to/doorbell-sound.mp3",
    name: "Doorbell Ring",
    priority: EventPriority.High,
    environmentService: environmentService,
    storage: storage
);
```

### TelephoneRingingEventInput → FileAudioInput

**Old:**
```csharp
var phoneInput = new TelephoneRingingEventInput(environmentService, storage);
```

**New:**
```csharp
// Use FileAudioInput with your phone ring sound file
var phoneInput = new FileAudioInput(
    filePath: "/path/to/phone-ring.wav",
    name: "Telephone Ring",
    priority: EventPriority.High,
    environmentService: environmentService,
    storage: storage
);
// Set repeat mode for continuous ringing
phoneInput.SetRepeat(0); // 0 = repeat forever
```

### GoogleBroadcastEventInput → CompositeAudioInput

**Old:**
```csharp
var broadcastInput = new GoogleBroadcastEventInput(environmentService, storage);
```

**New:**
```csharp
// Use CompositeAudioInput with announcement chime and TTS
var broadcastInput = new CompositeAudioInput(
    id: "google_broadcast",
    name: "Google Broadcast",
    priority: EventPriority.Medium,
    playSerially: true,  // Play chime, then message
    environmentService: environmentService,
    storage: storage
);

// Add announcement chime
broadcastInput.AddFileInput("/path/to/broadcast-chime.mp3", volume: 1.0);

// Add TTS message
broadcastInput.AddTtsInput("Your broadcast message here", ttsService, volume: 0.9);
```

### ReminderEventInput → CompositeAudioInput

**Old:**
```csharp
var reminderInput = new ReminderEventInput(environmentService, storage);
```

**New:**
```csharp
// Use CompositeAudioInput with notification sound and TTS reminder
var reminderInput = new CompositeAudioInput(
    id: "reminder",
    name: "Reminder",
    priority: EventPriority.Medium,
    playSerially: true,  // Play notification, then reminder text
    environmentService: environmentService,
    storage: storage
);

// Add notification sound
reminderInput.AddFileInput("/path/to/notification.mp3", volume: 0.8);

// Add reminder text
reminderInput.AddTtsInput("Reminder: Your appointment is in 15 minutes", ttsService, volume: 1.0);
```

### TimerExpiredEventInput → CompositeAudioInput

**Old:**
```csharp
var timerInput = new TimerExpiredEventInput(environmentService, storage);
```

**New:**
```csharp
// Use CompositeAudioInput with alarm sound and TTS announcement
var timerInput = new CompositeAudioInput(
    id: "timer_expired",
    name: "Timer Expired",
    priority: EventPriority.Medium,
    playSerially: true,  // Play alarm, then announcement
    environmentService: environmentService,
    storage: storage
);

// Add alarm sound (repeat 3 times)
reminderInput.AddFileInput("/path/to/timer-beep.wav", volume: 1.0, repeatCount: 3);

// Add announcement
timerInput.AddTtsInput("Timer expired", ttsService, volume: 1.0);
```

## New Features Available

All input types now support:

- **PauseAsync()** / **ResumeAsync()** - Pause and resume playback
- **SetVolumeAsync(double volume)** - Adjust volume (0.0 to 1.0)
- **SetRepeat(int count)** - Set repeat mode (0 = infinite, -1 = no repeat)
- **AllowConcurrent** - Allow/prevent concurrent playback with other streams
- **AudioDataAvailable** event - Receive PCM audio data for mixing

## Examples

### Creating a Complex Event with Multiple Audio Sources

```csharp
// Create a morning alarm with music, announcement, and weather
var morningAlarm = new CompositeAudioInput(
    id: "morning_alarm",
    name: "Morning Alarm",
    priority: EventPriority.High,
    playSerially: true,
    environmentService: environmentService,
    storage: storage
);

// Gentle wake-up music
morningAlarm.AddFileInput("/sounds/wake-up.mp3", volume: 0.5);

// Good morning announcement
morningAlarm.AddTtsInput("Good morning! It's 7 AM.", ttsService, volume: 0.8);

// Weather update
morningAlarm.AddTtsInput("Today's forecast: Sunny, high of 75 degrees.", ttsService, volume: 0.8);
```

### Playing Background Music with Event Interruption

```csharp
// Background music - allows concurrent play
var musicInput = new UsbAudioInput(0, "Radio", "FM Radio", environmentService, storage);
musicInput.AllowConcurrent = false; // Will be interrupted by events

// Doorbell - plays on top with priority
var doorbellInput = new FileAudioInput(
    "/sounds/doorbell.mp3",
    "Doorbell",
    EventPriority.High,  // Higher priority than music
    environmentService,
    storage
);
```

## Migration Checklist

When migrating your code:

1. ✅ Replace old input types with new generic types
2. ✅ Provide appropriate file paths for audio files
3. ✅ Set correct priority levels for events
4. ✅ Configure repeat modes as needed
5. ✅ Set volume levels appropriately
6. ✅ Use CompositeAudioInput for multi-part audio events
7. ✅ Test audio playback and event interruption

## Benefits of New Architecture

- **Flexibility**: Create custom audio events without new classes
- **Reusability**: Same components for different use cases
- **Composability**: Combine multiple audio sources easily
- **Maintainability**: Fewer specialized classes to maintain
- **Extensibility**: Easy to add new features to all inputs at once
