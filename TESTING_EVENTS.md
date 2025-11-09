# Event-Driven Audio Testing Guide

This guide shows how to test the event-driven audio functionality in the RadioConsole API.

## Prerequisites

- .NET 9.0 SDK installed
- Project built successfully

## Starting the API

From the project root:

```bash
cd src/RadioConsole.Api
dotnet run
```

The API will start on http://localhost:5269 (or the port shown in the console output).

## Testing Event Functionality

### 1. Check Available Inputs

List all audio inputs (both Music and Event types):

```bash
curl http://localhost:5269/api/audio/inputs
```

Response includes `inputType` field:
- `0` = Music input (Radio, Spotify)
- `1` = Event input (Doorbell, Timer, etc.)

### 2. Check Priority Manager State

Get the current state of the AudioPriorityManager:

```bash
curl http://localhost:5269/api/audio/priority/state
```

Response shows:
- Whether an event is currently playing
- List of registered event inputs
- Current configuration (volume reduction level, mute settings)

### 3. Trigger a Doorbell Event

Simulate a doorbell ring:

```bash
curl -X POST "http://localhost:5269/api/eventsexample/doorbell/ring?location=Front%20Door"
```

Expected behavior:
1. Saves current audio volume levels
2. Reduces background audio to 10% (configurable)
3. Plays doorbell chime for 3 seconds
4. Restores original volume levels

### 4. Trigger a Reminder Event

Simulate a reminder notification:

```bash
curl -X POST "http://localhost:5269/api/eventsexample/reminder/trigger?message=Test%20reminder"
```

Expected behavior:
1. Saves current audio volume levels
2. Reduces background audio
3. Plays reminder notification for 2 seconds
4. Restores original volumes

### 5. Text-to-Speech Announcement (Hello World)

Test the TextEventInput with a simple "Hello World" announcement:

```bash
curl -X POST http://localhost:5269/api/eventsexample/text/helloworld
```

Expected behavior:
1. eSpeak TTS generates speech from "Hello World"
2. Saves current audio volume levels
3. Reduces background audio
4. Plays the speech announcement
5. Restores original volumes

### 6. Custom Text Announcement

Announce custom text using TTS:

```bash
curl -X POST "http://localhost:5269/api/eventsexample/text/announce?text=Welcome%20to%20the%20Radio%20Console%20system"
```

Expected behavior:
1. Converts provided text to speech using eSpeak TTS
2. Duration automatically calculated based on text length
3. Announcement plays with priority handling
4. Volumes restored after completion

### 7. Test Priority System

Test multiple events with different priorities:

```bash
curl -X POST http://localhost:5269/api/eventsexample/test/priority
```

This triggers:
1. A medium priority reminder event
2. A high priority doorbell event (interrupts the reminder)

The response shows the execution order and final state.

### 8. Get Configuration

View current AudioPriorityManager configuration:

```bash
curl http://localhost:5269/api/eventsexample/config
```

### 9. Update Configuration

Change the volume reduction level or mute setting:

```bash
curl -X PUT http://localhost:5269/api/eventsexample/config \
  -H "Content-Type: application/json" \
  -d '{
    "volumeReductionLevel": 0.2,
    "muteBackgroundAudio": false
  }'
```

Parameters:
- `volumeReductionLevel`: 0.0 (mute) to 1.0 (no change)
- `muteBackgroundAudio`: true/false

### 10. Trigger Event via Generic Endpoint

Use the generic event trigger endpoint:

```bash
curl -X POST http://localhost:5269/api/audio/events/doorbell_event/trigger \
  -H "Content-Type: application/json" \
  -d '{
    "metadata": {
      "location": "Back Door",
      "camera": "cam-002"
    }
  }'
```

## Using Swagger UI

The API includes Swagger documentation:

1. Open http://localhost:5269/swagger in your browser
2. Browse all available endpoints
3. Test endpoints directly from the Swagger interface

## Testing with Postman or Similar Tools

Import the following endpoints into your API testing tool:

### Collection: RadioConsole Event Audio

1. **GET** List Inputs
   - URL: `http://localhost:5269/api/audio/inputs`

2. **GET** Priority State
   - URL: `http://localhost:5269/api/audio/priority/state`

3. **POST** Doorbell Ring
   - URL: `http://localhost:5269/api/eventsexample/doorbell/ring?location=Front Door`

4. **POST** Reminder
   - URL: `http://localhost:5269/api/eventsexample/reminder/trigger?message=Take medication`

5. **POST** Hello World (TTS)
   - URL: `http://localhost:5269/api/eventsexample/text/helloworld`

6. **POST** Custom Text Announcement (TTS)
   - URL: `http://localhost:5269/api/eventsexample/text/announce?text=Your custom message here`

4. **POST** Reminder
   - URL: `http://localhost:5269/api/eventsexample/reminder/trigger?message=Take medication`

5. **POST** Test Priority
   - URL: `http://localhost:5269/api/eventsexample/test/priority`

6. **GET** Get Config
   - URL: `http://localhost:5269/api/eventsexample/config`

7. **PUT** Update Config
   - URL: `http://localhost:5269/api/eventsexample/config`
   - Headers: `Content-Type: application/json`
   - Body:
     ```json
     {
       "volumeReductionLevel": 0.2,
       "muteBackgroundAudio": false
     }
     ```

## Expected Console Output

When triggering events, you should see log messages like:

```
info: RadioConsole.Api.Controllers.EventsExampleController[0]
      Simulating doorbell ring at Front Door
info: RadioConsole.Api.Services.AudioPriorityManager[0]
      Handling audio event from Doorbell Ring with priority High
info: RadioConsole.Api.Services.AudioPriorityManager[0]
      Playing event audio from Doorbell Ring
info: RadioConsole.Api.Services.AudioPriorityManager[0]
      Completed event audio from Doorbell Ring
```

## Simulation Mode

All event inputs work in simulation mode (non-Raspberry Pi environments):
- Events trigger successfully
- Volume save/restore is mocked
- Audio streams are simulated (MemoryStream)
- Full functionality can be tested without hardware

## Troubleshooting

### Issue: Event not triggering

**Check:**
```bash
curl http://localhost:5269/api/audio/inputs | jq '.[] | select(.inputType == 1)'
```

Verify event inputs show `"isAvailable": true`.

### Issue: Volume not changing

**Note:** In simulation mode, actual audio volume changes are mocked. On Raspberry Pi with PulseAudio, volumes will change for real.

### Issue: Events being ignored

**Check priority levels:**
- High priority events interrupt Medium/Low
- Medium priority events interrupt Low
- Events at same priority level don't interrupt each other

View current event:
```bash
curl http://localhost:5269/api/audio/priority/state | jq '.currentEvent'
```

## Next Steps

After testing with the API:

1. Review logs to understand event flow
2. Experiment with different priority levels
3. Adjust volume reduction configuration
4. Integrate with real event sources (webhooks, timers, etc.)
5. Test on Raspberry Pi with actual audio hardware

## Additional Resources

- [EVENTS_DOCUMENTATION.md](../EVENTS_DOCUMENTATION.md) - Complete documentation
- [ARCHITECTURE.md](../ARCHITECTURE.md) - System architecture
- [README.md](../README.md) - Project overview
