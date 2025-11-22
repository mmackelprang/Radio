# API Endpoints for Embedded Audio Controller UI

This document outlines the REST API endpoints required for full functionality of the Embedded Audio Controller UI. 

**Status Update:** This document has been reconciled with the existing `RadioConsole.API` project. See the detailed analysis in `API_ENDPOINT_RECONCILIATION.md` in the parent directory.

**Legend:**
- ✅ **AVAILABLE** - Endpoint exists in RadioConsole.API
- 🟡 **PARTIAL** - Similar endpoint exists but needs adaptation
- ⚠️ **MODIFY** - Exists but requires changes
- ❌ **MISSING** - Must be implemented

---

## System Status Endpoints

### GET /api/system/stats
**Status:** ⚠️ **MODIFY** - Available as `GET /api/SystemStatus` but needs simplified format

Get current system statistics (CPU, RAM, Threads)

**RadioConsole.API Equivalent:** `GET /api/SystemStatus` returns comprehensive system info including CPU, memory, uptime, etc. Response format differs slightly.

**Response:**

```json
{
  "cpu": 25,
  "ram": 45,
  "threads": 18
}
```

### GET /api/system/info
**Status:** ⚠️ **MODIFY** - Included in `GET /api/SystemStatus` but needs version/buildDate fields

Get software information

**RadioConsole.API Note:** Currently returns runtime info but not application version/buildDate.

**Response:**

```json
{
  "version": "2.4.1",
  "buildDate": "2024-11-15",
  "uptime": 86400
}
```

### POST /api/system/shutdown
**Status:** ❌ **MISSING** - Not yet implemented

Shutdown the system

**Implementation Note:** Requires OS-level permissions and security considerations.

**Request:**

```json
{
  "confirmed": true
}
```

---

## Audio Control Endpoints

### GET /api/audio/status
**Status:** 🟡 **PARTIAL** - Available via multiple endpoints: `GET /api/AudioDeviceManager/inputs/current`, `/outputs/current`, and `GET /api/NowPlaying`

Get current audio status

**RadioConsole.API Note:** Need unified endpoint combining device info, volume, balance, and playback status.

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
**Status:** ❌ **MISSING** - Not yet implemented

Set volume level

**Implementation Note:** Needs integration with IAudioPlayer or IAudioDeviceManager for master volume control.

**Request:**

```json
{
  "volume": 75
}
```

### POST /api/audio/balance
**Status:** ❌ **MISSING** - Not yet implemented

Set audio balance

**Implementation Note:** Stereo pan control, typically -100 to +100 or -1.0 to +1.0.

**Request:**

```json
{
  "balance": -10
}
```

### POST /api/audio/playback
**Status:** ❌ **MISSING** - Unified interface needed (source-specific controls exist)

Control playback (play, pause, next, previous)

**RadioConsole.API Note:** Source-specific controls exist (e.g., `POST /api/RaddyRadio/start`, `/stop`). Need unified controller routing to active source.

**Request:**

```json
{
  "action": "play" | "pause" | "next" | "previous"
}
```

### POST /api/audio/shuffle
**Status:** ❌ **MISSING** - Not yet implemented

Toggle shuffle mode

**Implementation Note:** Applicable to Spotify/File Player, may not apply to Radio/Vinyl.

**Request:**

```json
{
  "enabled": true
}
```

### GET /api/audio/inputs
**Status:** ✅ **AVAILABLE** - `GET /api/AudioDeviceManager/inputs`

Get available input devices

**RadioConsole.API:** Returns collection of AudioDeviceInfo with id, name, and availability.

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
**Status:** ✅ **AVAILABLE** - `GET /api/AudioDeviceManager/outputs`

Get available output devices

**RadioConsole.API:** Returns collection of AudioDeviceInfo with id, name, and availability.

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
**Status:** ✅ **AVAILABLE** - `POST /api/AudioDeviceManager/inputs/current`

Set active input device

**RadioConsole.API:** Request uses `DeviceId` instead of `inputId` (minor naming difference).

**Request:**

```json
{
  "inputId": "spotify"
}
```

### POST /api/audio/output
**Status:** ✅ **AVAILABLE** - `POST /api/AudioDeviceManager/outputs/current`

Set active output device

**RadioConsole.API:** Request uses `DeviceId` instead of `outputId` (minor naming difference).

**Request:**

```json
{
  "outputId": "speakers"
}
```

### GET /api/audio/input/{inputId}/config
**Status:** ⚠️ **MODIFY** - Can use `GET /api/Configuration/component/{component}`

Get configuration for specific input

**RadioConsole.API:** Use generic configuration endpoint with component name like `Input_{inputId}`, or create convenience wrapper.

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
**Status:** ⚠️ **MODIFY** - Can use `POST /api/Configuration`

Save configuration for specific input

**RadioConsole.API:** Use generic configuration endpoint, or create convenience wrapper.

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
**Status:** ⚠️ **MODIFY** - Can use `GET /api/Configuration/component/{component}`

Get configuration for specific output

**RadioConsole.API:** Use generic configuration endpoint with component name like `Output_{outputId}`, or create convenience wrapper.

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
**Status:** ⚠️ **MODIFY** - Can use `POST /api/Configuration`

Save configuration for specific output

**RadioConsole.API:** Use generic configuration endpoint, or create convenience wrapper.

---

## Spotify Endpoints

### GET /api/spotify/current-track
**Status:** 🟡 **PARTIAL** - Available as `GET /api/NowPlaying/spotify`

Get currently playing track

**RadioConsole.API Note:** Returns trackName, artist, album, albumArtUrl, isPlaying. Missing: duration, currentTime, liked.

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
**Status:** ❌ **MISSING** - Not yet implemented

Toggle like status for current track

**Implementation Note:** Requires Spotify API integration, may need to extend ISpotifyService.

**Request:**

```json
{
  "liked": true
}
```

---

## Radio Endpoints

### GET /api/radio/status
**Status:** ✅ **AVAILABLE** - `GET /api/RaddyRadio/status`

Get current radio status

**RadioConsole.API:** Returns isStreaming, isDeviceDetected, signalStrength (0-6), frequency, deviceId. Missing: band (can derive), volume, equalization.

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
**Status:** ✅ **AVAILABLE** - `POST /api/RaddyRadio/frequency`

Set radio frequency

**RadioConsole.API:** Request uses `FrequencyMHz` instead of `frequency` (minor naming difference).

**Request:**

```json
{
  "frequency": 101.5
}
```

### POST /api/radio/band
**Status:** ❌ **MISSING** - Not yet implemented

Change radio band

**Implementation Note:** Need to determine if RF320 supports direct band switching or if band is inferred from frequency range.

**Request:**

```json
{
  "band": "FM" | "AM" | "SW" | "AIR" | "VHF"
}
```

### POST /api/radio/tune
**Status:** ❌ **MISSING** - Not yet implemented

Tune up or down

**Implementation Note:** Can be implemented as wrapper: get current frequency, add/subtract step, set new frequency.

**Request:**

```json
{
  "direction": "up" | "down",
  "step": 0.2
}
```

### POST /api/radio/scan
**Status:** ❌ **MISSING** - Not yet implemented

Scan for stations

**Implementation Note:** Requires RF320 signal monitoring. Scan until signal threshold met. May need async/long-running operation.

**Request:**

```json
{
  "direction": "up" | "down"
}
```

### POST /api/radio/equalization
**Status:** ❌ **MISSING** - Not yet implemented

Set equalization preset

**Implementation Note:** Requires audio DSP integration with SoundFlow library. Presets: flat, bass-boost, treble-boost, voice, classical, rock.

**Request:**

```json
{
  "preset": "flat" | "bass-boost" | "treble-boost" | "voice" | "classical" | "rock"
}
```

### POST /api/radio/save-station
**Status:** ❌ **MISSING** - Not yet implemented

Save current station

**Implementation Note:** Store station presets in configuration service with name, frequency, and band.

**Request:**

```json
{
  "name": "Classic Rock FM",
  "frequency": 101.5,
  "band": "FM"
}
```

### GET /api/radio/stations
**Status:** ❌ **MISSING** - Not yet implemented

Get saved stations

**Implementation Note:** Retrieve from configuration service, return as collection.

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
**Status:** ❌ **MISSING** - Not yet implemented

Get vinyl player status

**Implementation Note:** Track preamp enable/disable state and playback status.

**Response:**

```json
{
  "preampEnabled": true,
  "isPlaying": false
}
```

### POST /api/vinyl/preamp
**Status:** ❌ **MISSING** - Not yet implemented

Toggle preamp

**Implementation Note:** May require hardware control via GPIO or USB. Store preference in configuration.

**Request:**

```json
{
  "enabled": true
}
```

---

## File Player Endpoints

### GET /api/file-player/current
**Status:** ❌ **MISSING** - Not yet implemented

Get currently playing file

**Implementation Note:** Track current file, return metadata (artist, title, duration, position), album art extraction.

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
**Status:** ❌ **MISSING** - Not yet implemented

Browse file system

**Query Parameters:**

- `path` (string): Directory path to browse

**Implementation Note:** List directories and audio files. Filter by formats (mp3, flac, wav). Security: Restrict to configured music directories.

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
**Status:** ❌ **MISSING** - Not yet implemented

Select file or folder to play

**Implementation Note:** Load file into audio player. Generate playlist if folder selected.

**Request:**

```json
{
  "path": "/music/jazz/summer-breeze.flac"
}
```

---

## Playlist Endpoints

### GET /api/playlist
**Status:** ❌ **MISSING** - Not yet implemented

Get current playlist

**Implementation Note:** Return current queue/playlist with track metadata.

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
**Status:** ❌ **MISSING** - Not yet implemented

Add track to playlist

**Request:**

```json
{
  "trackId": "spotify:track:123"
}
```

### DELETE /api/playlist/{itemId}
**Status:** ❌ **MISSING** - Not yet implemented

Remove track from playlist

### POST /api/playlist/reorder
**Status:** ❌ **MISSING** - Not yet implemented

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
**Status:** ✅ **AVAILABLE** - `GET /api/Configuration/components`

Get list of configurable components

**RadioConsole.API:** Returns collection of component names.

**Response:**

```json
{
  "components": ["Audio", "Network", "Display", "Radio"]
}
```

### GET /api/config/{component}
**Status:** ✅ **AVAILABLE** - `GET /api/Configuration/component/{component}`

Get configuration for specific component

**RadioConsole.API:** Returns configuration items for the specified component.

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
**Status:** ⚠️ **MODIFY** - Batch save needed, currently saves one item at a time

Save configuration for specific component

**RadioConsole.API:** Use `POST /api/Configuration` for single item or `PUT /api/Configuration/{component}/{key}` for updates. Consider adding batch save endpoint.

**Request:**

```json
{
  "items": [
    {"category": "Playback", "key": "default_volume", "value": "75"}
  ]
}
```

### POST /api/config/backup
**Status:** ✅ **AVAILABLE** - `POST /api/Configuration/backup`

Backup all configuration

**Response:** Configuration file download

**RadioConsole.API:** Creates backup file and returns path.

### POST /api/config/restore
**Status:** ✅ **AVAILABLE** - `POST /api/Configuration/restore`

Restore configuration from backup

**Request:** multipart/form-data with configuration file

**RadioConsole.API Note:** Current API expects JSON with `BackupPath`. May need to adapt to accept file upload or document path-based approach.

---

## Prompt Management Endpoints

### GET /api/prompts
**Status:** ❌ **MISSING** - Not yet implemented

Get all prompts

**Implementation Note:** TTS and audio file prompts for priority audio (doorbell, phone, TTS). May leverage AudioPriorityController.

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
**Status:** ❌ **MISSING** - Not yet implemented

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
**Status:** ❌ **MISSING** - Not yet implemented

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
**Status:** ❌ **MISSING** - Not yet implemented

Delete prompt

### POST /api/prompts/{id}/play
**Status:** ❌ **MISSING** - Not yet implemented

Play a specific prompt

---

## Additional Available Endpoints in RadioConsole.API

The RadioConsole.API provides several additional endpoints not listed above that may be useful for the UI:

### Audio Priority Management
- `POST /api/AudioPriority/sources/register` - Register audio source with priority
- `POST /api/AudioPriority/events/high-priority-start` - Trigger audio ducking
- `POST /api/AudioPriority/events/high-priority-end` - Release audio ducking
- `GET /api/AudioPriority/status` - Get priority system status
- `POST /api/AudioPriority/config/duck-percentage` - Configure ducking level

### User Preferences
- `GET /api/Preferences/audio` - Get audio preferences
- `POST /api/Preferences/audio` - Save audio preferences
- `GET /api/Preferences/device-visibility` - Get device visibility config
- `POST /api/Preferences/cast-device` - Save ChromeCast device preference

### Metadata
- `GET /api/Metadata/audio-formats` - Get supported audio formats
- `GET /api/Metadata/sample-rates` - Get supported sample rates

### Visualization
- `GET /api/Visualization/audio-levels` - Get current audio levels
- `GET /api/Visualization/spectrum` - Get frequency spectrum data

### Streaming
- `GET /api/Streaming/stream.mp3` - Stream audio as MP3
- `GET /api/Streaming/stream.wav` - Stream audio as WAV

### Health Check
- `GET /health` - Simple health check endpoint

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