# API Endpoint Reconciliation

This document maps the required endpoints from `MISSING_ENDPOINTS.md` to the existing endpoints in `RadioConsole.API`.

**Legend:**
- ✅ **FULLY AVAILABLE** - Endpoint exists with matching functionality
- 🟡 **PARTIALLY AVAILABLE** - Similar endpoint exists but may need adaptation
- ⚠️ **NEEDS MODIFICATION** - Endpoint exists but requires changes to match requirements
- ❌ **MISSING** - Endpoint does not exist and must be implemented

---

## System Status Endpoints

### GET /api/system/stats
**Status:** ⚠️ **NEEDS MODIFICATION**

**Available:** `GET /api/SystemStatus` returns comprehensive system info
**Required Changes:**
- Current response includes: `cpuUsagePercent`, `totalMemoryBytes`, `usedMemoryBytes`, `availableMemoryBytes`, `uptimeSeconds`, etc.
- Required format wants: `cpu`, `ram`, `threads` (simpler format)
- **Solution:** Keep existing endpoint, add a new `GET /api/SystemStatus/stats` that returns simplified format, OR create a DTO mapper in the UI

**Current Response Format:**
```json
{
  "apiUrl": "http://...",
  "apiPort": 5100,
  "cpuUsagePercent": 25.5,
  "totalMemoryBytes": 8589934592,
  "usedMemoryBytes": 3221225472,
  "uptimeSeconds": 86400,
  "operatingSystem": "Linux",
  "processorCount": 4
}
```

### GET /api/system/info
**Status:** ⚠️ **NEEDS MODIFICATION**

**Available:** `GET /api/SystemStatus` includes version info in same response
**Required Changes:**
- Need to add version and buildDate to SystemStatus model
- Currently returns runtime info but not application version
- **Solution:** Add `version`, `buildDate` properties to SystemStatus response

### POST /api/system/shutdown
**Status:** ❌ **MISSING**

**Required Action:** Implement system shutdown endpoint
- Requires OS-level permissions
- Should have confirmation mechanism
- Consider security implications (authentication/authorization)

---

## Audio Control Endpoints

### GET /api/audio/status
**Status:** 🟡 **PARTIALLY AVAILABLE**

**Available:** Multiple endpoints provide parts of this info:
- `GET /api/AudioDeviceManager/inputs/current` - Current input device
- `GET /api/AudioDeviceManager/outputs/current` - Current output device
- `GET /api/NowPlaying` - Playback status info

**Required Changes:**
- Need unified endpoint that combines all audio status
- Add volume and balance to the response (currently not tracked in API)
- Add shuffle status
- **Solution:** Create `GET /api/audio/status` composite endpoint

### POST /api/audio/volume
**Status:** ❌ **MISSING**

**Required Action:** Implement volume control endpoint
- Should integrate with IAudioPlayer or IAudioDeviceManager
- Consider: Is this master volume, or source-specific?

### POST /api/audio/balance
**Status:** ❌ **MISSING**

**Required Action:** Implement audio balance control endpoint
- Pan control for stereo output
- Typical range: -100 to +100 or -1.0 to +1.0

### POST /api/audio/playback
**Status:** ❌ **MISSING** (for unified interface)

**Available:** Source-specific controls exist:
- `POST /api/RaddyRadio/start` / `POST /api/RaddyRadio/stop`
- Spotify controls would be via SpotifyService

**Required Action:** Create unified playback controller
- Route commands to active input source
- Support: play, pause, next, previous
- Handle source switching

### POST /api/audio/shuffle
**Status:** ❌ **MISSING**

**Required Action:** Implement shuffle mode toggle
- Probably Spotify/File Player specific
- May not apply to Radio/Vinyl

### GET /api/audio/inputs
**Status:** ✅ **FULLY AVAILABLE**

**Available:** `GET /api/AudioDeviceManager/inputs`
**Format Match:** Yes, returns collection of devices with id, name, and availability

### GET /api/audio/outputs
**Status:** ✅ **FULLY AVAILABLE**

**Available:** `GET /api/AudioDeviceManager/outputs`
**Format Match:** Yes, returns collection of devices with id, name, and availability

### POST /api/audio/input
**Status:** ✅ **FULLY AVAILABLE**

**Available:** `POST /api/AudioDeviceManager/inputs/current`
**Request Format:** `{ "DeviceId": "spotify" }`
**UI Request Format:** `{ "inputId": "spotify" }`
**Solution:** UI can map `inputId` → `DeviceId`, or we can add an alias endpoint

### POST /api/audio/output
**Status:** ✅ **FULLY AVAILABLE**

**Available:** `POST /api/AudioDeviceManager/outputs/current`
**Request Format:** `{ "DeviceId": "speakers" }`
**UI Request Format:** `{ "outputId": "speakers" }`
**Solution:** Same as above, minor naming difference

### GET /api/audio/input/{inputId}/config
**Status:** ⚠️ **NEEDS MODIFICATION**

**Available:** `GET /api/Configuration/component/{component}` can store input configs
**Required Changes:**
- Create convenience endpoint that fetches config for specific input
- Or document that UI should call `/api/Configuration/component/Input_{inputId}`

### POST /api/audio/input/{inputId}/config
**Status:** ⚠️ **NEEDS MODIFICATION**

**Available:** `POST /api/Configuration` can save input configs
**Required Changes:**
- Create convenience endpoint, or document usage pattern

### GET /api/audio/output/{outputId}/config
**Status:** ⚠️ **NEEDS MODIFICATION**

**Available:** Similar to input config above

### POST /api/audio/output/{outputId}/config
**Status:** ⚠️ **NEEDS MODIFICATION**

**Available:** Similar to input config above

---

## Spotify Endpoints

### GET /api/spotify/current-track
**Status:** 🟡 **PARTIALLY AVAILABLE**

**Available:** `GET /api/NowPlaying/spotify`
**Response Format:**
```json
{
  "trackName": "Blinding Lights",
  "artist": "The Weeknd",
  "album": "After Hours",
  "albumArtUrl": "https://...",
  "isPlaying": true
}
```

**Required Format:**
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

**Missing Fields:** `duration`, `currentTime`, `liked`
**Solution:** Enhance NowPlaying/Spotify endpoint or create dedicated Spotify controller

### POST /api/spotify/like
**Status:** ❌ **MISSING**

**Required Action:** Implement Spotify like/unlike functionality
- Requires Spotify API integration
- May need to extend ISpotifyService

---

## Radio Endpoints

### GET /api/radio/status
**Status:** ✅ **FULLY AVAILABLE**

**Available:** `GET /api/RaddyRadio/status`
**Response Format:**
```json
{
  "isStreaming": false,
  "isDeviceDetected": true,
  "signalStrength": 4,
  "frequency": 101.5,
  "deviceId": "USB_Radio_Device"
}
```

**Required Format:**
```json
{
  "frequency": 101.5,
  "band": "FM",
  "signalStrength": 85,
  "volume": 75,
  "equalization": "flat"
}
```

**Differences:**
- Need to add `band` (can derive from frequency, already done in NowPlaying controller)
- Need to add `volume` (per-source volume, currently missing)
- Need to add `equalization` preset
- `signalStrength` format different (0-6 vs percentage)

**Solution:** Create wrapper endpoint or enhance existing one

### POST /api/radio/frequency
**Status:** ✅ **FULLY AVAILABLE**

**Available:** `POST /api/RaddyRadio/frequency`
**Request Format:** `{ "FrequencyMHz": 101.5 }`
**UI Format:** `{ "frequency": 101.5 }`
**Solution:** Minor naming difference, UI can adapt

### POST /api/radio/band
**Status:** ❌ **MISSING**

**Required Action:** Implement band switching
- RF320 supports: FM, AM, SW, AIR, VHF
- Currently only frequency setting exists
- Need to determine if hardware supports direct band switching or if it's inferred from frequency

### POST /api/radio/tune
**Status:** ❌ **MISSING**

**Required Action:** Implement step tuning (up/down by increment)
- Could be implemented as a convenience wrapper around SetFrequency
- Get current frequency, add/subtract step, set new frequency

### POST /api/radio/scan
**Status:** ❌ **MISSING**

**Required Action:** Implement station scanning
- Requires RF320 signal strength monitoring
- Scan in direction until signal threshold met
- May need async operation (long-running)

### POST /api/radio/equalization
**Status:** ❌ **MISSING**

**Required Action:** Implement EQ preset selection
- Requires audio DSP capabilities
- May need to integrate with SoundFlow library
- Presets: flat, bass-boost, treble-boost, voice, classical, rock

### POST /api/radio/save-station
**Status:** ❌ **MISSING**

**Required Action:** Implement station favorites
- Store station presets in configuration
- Include: name, frequency, band

### GET /api/radio/stations
**Status:** ❌ **MISSING**

**Required Action:** Retrieve saved stations
- Fetch from configuration service
- Return as collection

---

## Vinyl Endpoints

### GET /api/vinyl/status
**Status:** ❌ **MISSING**

**Required Action:** Implement vinyl player status endpoint
- Track preamp enable/disable state
- Track playback status

### POST /api/vinyl/preamp
**Status:** ❌ **MISSING**

**Required Action:** Implement preamp toggle
- May require hardware control via GPIO or USB
- Store preference in configuration

---

## File Player Endpoints

### GET /api/file-player/current
**Status:** ❌ **MISSING**

**Required Action:** Implement current file info endpoint
- Track currently playing file
- Return metadata (artist, title, duration, position)
- Album art extraction

### GET /api/file-player/browse
**Status:** ❌ **MISSING**

**Required Action:** Implement file system browser
- List directories and audio files
- Filter by audio formats (mp3, flac, wav, etc.)
- Security: Restrict to configured music directories

### POST /api/file-player/select
**Status:** ❌ **MISSING**

**Required Action:** Implement file/folder selection for playback
- Load file into audio player
- Generate playlist if folder selected

---

## Playlist Endpoints

### GET /api/playlist
**Status:** ❌ **MISSING**

**Required Action:** Implement playlist retrieval
- Return current queue/playlist
- Include track metadata

### POST /api/playlist/add
**Status:** ❌ **MISSING**

**Required Action:** Add track to playlist

### DELETE /api/playlist/{itemId}
**Status:** ❌ **MISSING**

**Required Action:** Remove track from playlist

### POST /api/playlist/reorder
**Status:** ❌ **MISSING**

**Required Action:** Reorder playlist items

---

## Configuration Management Endpoints

### GET /api/config/components
**Status:** ✅ **FULLY AVAILABLE**

**Available:** `GET /api/Configuration/components`
**Format Match:** Yes

### GET /api/config/{component}
**Status:** ✅ **FULLY AVAILABLE**

**Available:** `GET /api/Configuration/component/{component}`
**Format Match:** Yes, returns configuration items for component

### POST /api/config/{component}
**Status:** ⚠️ **NEEDS MODIFICATION**

**Available:** `POST /api/Configuration` (single item) or update via `PUT /api/Configuration/{component}/{key}`
**Required Changes:**
- Current API saves one item at a time
- UI wants to save multiple items for a component at once
- **Solution:** Add batch save endpoint or use multiple calls

### POST /api/config/backup
**Status:** ✅ **FULLY AVAILABLE**

**Available:** `POST /api/Configuration/backup`
**Format Match:** Yes, creates backup file

### POST /api/config/restore
**Status:** ✅ **FULLY AVAILABLE**

**Available:** `POST /api/Configuration/restore`
**Request Format:** `{ "BackupPath": "/path/to/backup.json" }`
**Note:** UI doc says multipart/form-data, but API expects JSON with path
**Solution:** Either adapt UI to send path, or enhance API to accept file upload

---

## Prompt Management Endpoints

### GET /api/prompts
**Status:** ❌ **MISSING**

**Required Action:** Implement prompts management
- TTS and audio file prompts
- CRUD operations
- Playback triggering

### POST /api/prompts
**Status:** ❌ **MISSING**

### PUT /api/prompts/{id}
**Status:** ❌ **MISSING**

### DELETE /api/prompts/{id}
**Status:** ❌ **MISSING**

### POST /api/prompts/{id}/play
**Status:** ❌ **MISSING**

**Note:** Prompts system relates to priority audio (events like doorbell, phone, TTS announcements)
**Solution:** May leverage existing AudioPriorityController for playback

---

## Additional API Endpoints (Not in UI Requirements)

The RadioConsole.API has several endpoints that aren't listed in MISSING_ENDPOINTS.md but may be useful:

### AudioPriorityController
- `POST /api/AudioPriority/sources/register` - Register audio source with priority
- `POST /api/AudioPriority/sources/unregister` - Unregister audio source
- `POST /api/AudioPriority/events/high-priority-start` - Trigger ducking
- `POST /api/AudioPriority/events/high-priority-end` - Release ducking
- `GET /api/AudioPriority/config/duck-percentage` - Get duck percentage
- `POST /api/AudioPriority/config/duck-percentage` - Set duck percentage
- `GET /api/AudioPriority/status` - Get priority system status

**UI Integration Opportunity:** These could be used for implementing priority audio features mentioned in docs

### PreferencesController
- `GET /api/Preferences/audio` - Get audio preferences
- `POST /api/Preferences/audio` - Save audio preferences
- `GET /api/Preferences/device-visibility` - Get device visibility config
- `POST /api/Preferences/device-visibility` - Save device visibility config
- `GET /api/Preferences/cast-device` - Get ChromeCast device preference
- `POST /api/Preferences/cast-device` - Save ChromeCast device preference

**UI Integration Opportunity:** Use for persisting user preferences

### MetadataController
- `GET /api/Metadata/audio-formats` - Get supported audio formats
- `GET /api/Metadata/audio-bitrates` - Get supported bitrates
- `GET /api/Metadata/sample-rates` - Get supported sample rates

**UI Integration Opportunity:** Use for displaying capabilities

### VisualizationController
- `GET /api/Visualization/audio-levels` - Get current audio levels
- `GET /api/Visualization/spectrum` - Get frequency spectrum data
- `POST /api/Visualization/config` - Configure visualization parameters

**UI Integration Opportunity:** Add audio visualization to Now Playing displays

### StreamingController
- `GET /api/Streaming/stream.mp3` - Stream audio as MP3
- `GET /api/Streaming/stream.wav` - Stream audio as WAV

**UI Integration Opportunity:** Could be used for web-based audio preview

### TestController
- Various test endpoints for development/debugging

**UI Integration Opportunity:** Useful during development

---

## Summary Statistics

**Endpoint Categories Analyzed:** 11

**Endpoints by Status:**
- ✅ **Fully Available:** 8 endpoints
- 🟡 **Partially Available:** 2 endpoints
- ⚠️ **Needs Modification:** 8 endpoints
- ❌ **Missing:** 30+ endpoints

**Priority Implementation Order:**

### High Priority (Core Functionality)
1. Unified audio status endpoint
2. Volume and balance controls
3. Unified playback control
4. Radio band switching, tuning, scanning
5. File player (current, browse, select)
6. Spotify enhancements (duration, position, like)

### Medium Priority (Enhanced Features)
7. Playlist management (all endpoints)
8. Station presets (save/load)
9. Vinyl preamp control
10. Radio EQ presets
11. Input/output config convenience endpoints

### Lower Priority (Nice to Have)
12. Prompts management
13. Shuffle mode
14. System shutdown
15. Configuration batch save

### Integration Work (Not New Endpoints)
16. Map existing preference endpoints to UI
17. Integrate visualization data
18. Use audio priority system for ducking
