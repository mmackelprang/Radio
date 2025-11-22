# Missing API Endpoints

This document outlines the REST API endpoints required for full functionality of the Embedded Audio Controller UI. These endpoints should be implemented in your .NET Core API.

## System Status Endpoints

### GET /api/system/stats

Get current system statistics (CPU, RAM, Threads)

**Response:**

```json
{
  "cpu": 25,
  "ram": 45,
  "threads": 18
}
```

### GET /api/system/info

Get software information

**Response:**

```json
{
  "version": "2.4.1",
  "buildDate": "2024-11-15",
  "uptime": 86400
}
```

### POST /api/system/shutdown

Shutdown the system

**Request:**

```json
{
  "confirmed": true
}
```

---

## Audio Control Endpoints

### GET /api/audio/status

Get current audio status

**Response:**

```json
{
  "volume": 50,
  "balance": 0,
  "shuffle": false,
  "isPlaying": false,
  "currentInput": "spotify",
  "currentOutput": "speakers"
}
```

### POST /api/audio/volume

Set volume level

**Request:**

```json
{
  "volume": 75
}
```

### POST /api/audio/balance

Set audio balance

**Request:**

```json
{
  "balance": -10
}
```

### POST /api/audio/playback

Control playback (play, pause, next, previous)

**Request:**

```json
{
  "action": "play" | "pause" | "next" | "previous"
}
```

### POST /api/audio/shuffle

Toggle shuffle mode

**Request:**

```json
{
  "enabled": true
}
```

### GET /api/audio/inputs

Get available input devices

**Response:**

```json
{
  "inputs": [
    {"id": "spotify", "name": "Spotify", "available": true},
    {"id": "usb-radio", "name": "USB Radio", "available": true},
    {"id": "vinyl", "name": "Vinyl Phonograph", "available": true}
  ]
}
```

### GET /api/audio/outputs

Get available output devices

**Response:**

```json
{
  "outputs": [
    {"id": "speakers", "name": "Built-in Speakers", "available": true},
    {"id": "headphones", "name": "Headphones", "available": false}
  ]
}
```

### POST /api/audio/input

Set active input device

**Request:**

```json
{
  "inputId": "spotify"
}
```

### POST /api/audio/output

Set active output device

**Request:**

```json
{
  "outputId": "speakers"
}
```

### GET /api/audio/input/{inputId}/config

Get configuration for specific input

**Response:**

```json
{
  "inputId": "usb-radio",
  "settings": {
    "defaultBand": "FM",
    "scanSensitivity": 75,
    "rdsEnabled": true
  }
}
```

### POST /api/audio/input/{inputId}/config

Save configuration for specific input

**Request:**

```json
{
  "settings": {
    "defaultBand": "AM",
    "scanSensitivity": 80
  }
}
```

### GET /api/audio/output/{outputId}/config

Get configuration for specific output

**Response:**

```json
{
  "outputId": "speakers",
  "settings": {
    "maxVolume": 100,
    "bassBoost": true
  }
}
```

### POST /api/audio/output/{outputId}/config

Save configuration for specific output

---

## Spotify Endpoints

### GET /api/spotify/current-track

Get currently playing track

**Response:**

```json
{
  "name": "Blinding Lights",
  "artist": "The Weeknd",
  "album": "After Hours",
  "duration": 200,
  "currentTime": 87,
  "albumArtUrl": "https://...",
  "liked": false
}
```

### POST /api/spotify/like

Toggle like status for current track

**Request:**

```json
{
  "liked": true
}
```

---

## Radio Endpoints

### GET /api/radio/status

Get current radio status

**Response:**

```json
{
  "frequency": 101.5,
  "band": "FM",
  "signalStrength": 85,
  "volume": 75,
  "equalization": "flat"
}
```

### POST /api/radio/frequency

Set radio frequency

**Request:**

```json
{
  "frequency": 101.5
}
```

### POST /api/radio/band

Change radio band

**Request:**

```json
{
  "band": "FM" | "AM" | "SW" | "AIR" | "VHF"
}
```

### POST /api/radio/tune

Tune up or down

**Request:**

```json
{
  "direction": "up" | "down",
  "step": 0.2
}
```

### POST /api/radio/scan

Scan for stations

**Request:**

```json
{
  "direction": "up" | "down"
}
```

### POST /api/radio/equalization

Set equalization preset

**Request:**

```json
{
  "preset": "flat" | "bass-boost" | "treble-boost" | "voice" | "classical" | "rock"
}
```

### POST /api/radio/save-station

Save current station

**Request:**

```json
{
  "name": "Classic Rock FM",
  "frequency": 101.5,
  "band": "FM"
}
```

### GET /api/radio/stations

Get saved stations

**Response:**

```json
{
  "stations": [
    {"id": "1", "name": "Classic Rock FM", "frequency": 101.5, "band": "FM"}
  ]
}
```

---

## Vinyl Endpoints

### GET /api/vinyl/status

Get vinyl player status

**Response:**

```json
{
  "preampEnabled": true,
  "isPlaying": false
}
```

### POST /api/vinyl/preamp

Toggle preamp

**Request:**

```json
{
  "enabled": true
}
```

---

## File Player Endpoints

### GET /api/file-player/current

Get currently playing file

**Response:**

```json
{
  "songName": "Summer Breeze",
  "fileName": "/music/jazz/summer-breeze.flac",
  "artist": "Jazz Ensemble",
  "duration": 245,
  "currentTime": 123,
  "albumArtUrl": "https://..."
}
```

### GET /api/file-player/browse

Browse file system

**Query Parameters:**

- `path` (string): Directory path to browse

**Response:**

```json
{
  "path": "/music",
  "items": [
    {"name": "jazz", "type": "folder", "path": "/music/jazz"},
    {"name": "song.mp3", "type": "file", "path": "/music/song.mp3"}
  ]
}
```

### POST /api/file-player/select

Select file or folder to play

**Request:**

```json
{
  "path": "/music/jazz/summer-breeze.flac"
}
```

---

## Playlist Endpoints

### GET /api/playlist

Get current playlist

**Response:**

```json
{
  "items": [
    {
      "id": "1",
      "songName": "Blinding Lights",
      "artist": "The Weeknd",
      "duration": 200
    }
  ]
}
```

### POST /api/playlist/add

Add track to playlist

**Request:**

```json
{
  "trackId": "spotify:track:123"
}
```

### DELETE /api/playlist/{itemId}

Remove track from playlist

### POST /api/playlist/reorder

Reorder playlist items

**Request:**

```json
{
  "fromIndex": 0,
  "toIndex": 2
}
```

---

## Configuration Management Endpoints

### GET /api/config/components

Get list of configurable components

**Response:**

```json
{
  "components": ["Audio", "Network", "Display", "Radio"]
}
```

### GET /api/config/{component}

Get configuration for specific component

**Response:**

```json
{
  "component": "Audio",
  "items": [
    {"category": "Playback", "key": "default_volume", "value": "50"}
  ]
}
```

### POST /api/config/{component}

Save configuration for specific component

**Request:**

```json
{
  "items": [
    {"category": "Playback", "key": "default_volume", "value": "75"}
  ]
}
```

### POST /api/config/backup

Backup all configuration

**Response:** Configuration file download

### POST /api/config/restore

Restore configuration from backup

**Request:** multipart/form-data with configuration file

---

## Prompt Management Endpoints

### GET /api/prompts

Get all prompts

**Response:**

```json
{
  "prompts": [
    {
      "id": "1",
      "name": "Welcome",
      "type": "TTS",
      "data": "Welcome to the audio controller system"
    },
    {
      "id": "2",
      "name": "Startup",
      "type": "File",
      "data": "/prompts/startup.wav"
    }
  ]
}
```

### POST /api/prompts

Create new prompt

**Request:**

```json
{
  "name": "New Prompt",
  "type": "TTS",
  "data": "Prompt text here"
}
```

**Response:**

```json
{
  "id": "8",
  "name": "New Prompt",
  "type": "TTS",
  "data": "Prompt text here"
}
```

### PUT /api/prompts/{id}

Update existing prompt

**Request:**

```json
{
  "name": "Updated Prompt",
  "type": "TTS",
  "data": "Updated text"
}
```

### DELETE /api/prompts/{id}

Delete prompt

### POST /api/prompts/{id}/play

Play a specific prompt

---

## Notes

1. **Authentication**: All endpoints should implement appropriate authentication/authorization based on your deployment environment.

2. **CORS**: Configure CORS to allow requests from the embedded device's IP address.

3. **Error Handling**: All endpoints should return meaningful error messages with appropriate HTTP status codes:
   - 200: Success
   - 201: Created
   - 400: Bad Request
   - 404: Not Found
   - 500: Internal Server Error

4. **WebSocket Support**: Consider implementing WebSocket connections for real-time updates:
   - Current playback position
   - System stats updates
   - Radio signal strength changes
   - Live configuration updates

5. **Rate Limiting**: Implement rate limiting for frequently called endpoints to prevent system overload.

6. **Data Validation**: All POST/PUT requests should validate input data and return validation errors when necessary.

7. **File Upload Limits**: For configuration restore and file uploads, set appropriate file size limits.

8. **Timeout Values**: Set reasonable timeout values for all API calls to prevent UI freezing.