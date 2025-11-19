# REST Controllers

## Audio Streaming Endpoints

### GET /stream.mp3
Streams the current audio mix as an MP3 stream over HTTP. This endpoint provides a continuous audio stream that can be consumed by Cast devices or other streaming clients.

**Content-Type:** `audio/mpeg`

**Usage:**
- Used by CastAudioOutput to stream audio to Google Cast devices
- Can be consumed by any HTTP audio streaming client
- Stream continues until client disconnects

**Example:**
```
curl http://localhost:5000/stream.mp3 --output audio.mp3
```

### GET /stream.wav
Streams the current audio mix as a WAV stream over HTTP. Provides uncompressed audio output.

**Content-Type:** `audio/wav`

**Usage:**
- Similar to `/stream.mp3` but provides uncompressed audio
- Useful for higher quality audio streaming where bandwidth is not a concern

**Example:**
```
curl http://localhost:5000/stream.wav --output audio.wav
```

## Configuration

Audio services are registered in the DI container via `AudioServiceExtensions.AddAudioServices()`.

The stream URL for Cast devices can be configured and defaults to `http://localhost:5000/stream.mp3`.

