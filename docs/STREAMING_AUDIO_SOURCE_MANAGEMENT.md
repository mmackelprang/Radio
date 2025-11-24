# Radio Console - Streaming Audio & Audio Source Management

## Overview

This document describes the streaming audio support, global audio controls, and audio source management features in the Radio Console application.

## Features

### 1. Streaming Audio Formats

The Radio Console supports streaming audio in the following formats (based on SoundFlow library capabilities):

| Format | Content-Type | Description |
|--------|--------------|-------------|
| WAV | audio/wav | Waveform Audio File Format (uncompressed) |
| MP3 | audio/mpeg | MPEG Audio Layer III (compressed) |
| FLAC | audio/flac | Free Lossless Audio Codec |
| AAC | audio/aac | Advanced Audio Coding |
| OGG | audio/ogg | Ogg Vorbis audio format |
| OPUS | audio/opus | Opus Interactive Audio Codec |

#### Streaming Endpoints

All streaming endpoints are available under `/api/streaming/`:

- `GET /api/streaming/stream.mp3` - MP3 audio stream
- `GET /api/streaming/stream.wav` - WAV audio stream
- `GET /api/streaming/stream.flac` - FLAC audio stream
- `GET /api/streaming/stream.aac` - AAC audio stream
- `GET /api/streaming/stream.ogg` - OGG audio stream
- `GET /api/streaming/stream.opus` - OPUS audio stream

#### Stream Information API

- `GET /api/StreamAudio/info` - Returns streaming endpoint information
- `GET /api/StreamAudio/status` - Returns streaming service status

Example response from `/api/StreamAudio/info`:
```json
{
  "streamUrl": "http://localhost:5100/api/streaming/stream",
  "description": "Real-time audio streaming endpoints for casting to external devices. Append the format extension (e.g., .mp3, .wav, .flac) to the stream URL.",
  "supportedFormats": ["WAV", "MP3", "FLAC", "AAC", "OGG", "OPUS"],
  "formatUrls": {
    "WAV": "http://localhost:5100/api/streaming/stream.wav",
    "MP3": "http://localhost:5100/api/streaming/stream.mp3",
    "FLAC": "http://localhost:5100/api/streaming/stream.flac",
    "AAC": "http://localhost:5100/api/streaming/stream.aac",
    "OGG": "http://localhost:5100/api/streaming/stream.ogg",
    "OPUS": "http://localhost:5100/api/streaming/stream.opus"
  }
}
```

### 2. Global Audio Controls

Global audio controls affect all audio sources that are currently playing through the MiniAudioEngine.

#### Volume Control

- `GET /api/AudioDeviceManager/global/volume` - Get current volume (0.0 to 1.0)
- `POST /api/AudioDeviceManager/global/volume` - Set global volume

Example request:
```json
{
  "volume": 0.75
}
```

#### Balance (Pan) Control

- `GET /api/AudioDeviceManager/global/balance` - Get current balance (-1.0 to 1.0)
- `POST /api/AudioDeviceManager/global/balance` - Set global balance

Example request:
```json
{
  "balance": 0.0
}
```

Balance values:
- `-1.0` = Full left
- `0.0` = Center
- `1.0` = Full right

#### Equalization Control

- `GET /api/AudioDeviceManager/global/equalization` - Get current EQ settings
- `POST /api/AudioDeviceManager/global/equalization` - Set EQ settings

Example request:
```json
{
  "bass": 3.0,
  "midrange": 0.0,
  "treble": 2.0,
  "enabled": true
}
```

EQ values are in dB, range: -12 to +12

#### Playback Control

- `GET /api/AudioDeviceManager/global/playback` - Get current playback state
- `POST /api/AudioDeviceManager/global/pause` - Pause all audio
- `POST /api/AudioDeviceManager/global/play` - Resume all audio
- `POST /api/AudioDeviceManager/global/stop` - Stop all audio

Playback states: `Stopped`, `Playing`, `Paused`

### 3. Audio Source Management

Audio sources are managed through the `/api/AudioSource/` endpoints.

#### Source Types

**Standard Audio Sources** (Low Priority - can be ducked):
- **Spotify** - Spotify streaming source
- **Radio** - USB Radio source (e.g., Raddy RF320)
- **Vinyl Record** - USB ADC for vinyl turntable
- **File Player** - Local audio file playback

**High Priority Audio Sources** (cause ducking of standard sources):
- **TTS Event** - Text-to-Speech announcements
- **File Event** - Notification sounds (doorbell, phone ring, etc.)

#### Creating Sources

| Endpoint | Description |
|----------|-------------|
| `POST /api/AudioSource/spotify` | Create Spotify source |
| `POST /api/AudioSource/radio` | Create USB Radio source |
| `POST /api/AudioSource/vinyl` | Create Vinyl Record source |
| `POST /api/AudioSource/fileplayer` | Create File Player source |
| `POST /api/AudioSource/tts` | Create TTS Event source (high priority) |
| `POST /api/AudioSource/fileevent` | Create File Event source (high priority) |

Example TTS request:
```json
{
  "text": "Hello, this is a test announcement",
  "voice": "en-US",
  "speed": 1.0
}
```

Example File Player request:
```json
{
  "filePath": "/path/to/audio.mp3"
}
```

#### Playback Control

- `POST /api/AudioSource/{sourceId}/play` - Start playing a source
- `POST /api/AudioSource/{sourceId}/pause` - Pause a source
- `POST /api/AudioSource/{sourceId}/resume` - Resume a paused source
- `DELETE /api/AudioSource/{sourceId}` - Stop and remove a source
- `DELETE /api/AudioSource` - Stop and remove all sources

#### Source Information

- `GET /api/AudioSource` - List all active sources
- `GET /api/AudioSource/{sourceId}` - Get information about a specific source

Example source info response:
```json
{
  "id": "spotify-0001-abc123def456",
  "type": "Spotify",
  "name": "Spotify",
  "status": "Playing",
  "isHighPriority": false,
  "createdAt": "2024-01-15T10:30:00Z",
  "metadata": {
    "ClientID": "your-client-id",
    "HasClientSecret": "True",
    "HasRefreshToken": "True"
  }
}
```

#### Source Status Values

- `Initializing` - Source is being created
- `Ready` - Source is ready to play
- `Playing` - Source is currently playing
- `Paused` - Source is paused
- `Stopped` - Source has been stopped
- `Error` - Source encountered an error

## Configuration

Audio sources read configuration from the Configuration Service using the following structure:

### Spotify Source
- Component: `AudioSource`
- Keys: `ClientID`, `ClientSecret`, `RefreshToken`

### USB Radio Source
- Component: `AudioSource`
- Keys: `USBRadio_USBPort`

### Vinyl Record Source
- Component: `AudioSource`
- Keys: `VinylRecord_USBPort`

### File Player Source
- Component: `AudioSource`
- Keys: `FilePlayer_Path`

### TTS Source
- Component: `AudioSource`
- Keys: `TTS_TTSEngine`

### Example Configuration

Using the Configuration API:
```bash
# Set Spotify configuration
curl -X POST http://localhost:5100/api/configuration \
  -H "Content-Type: application/json" \
  -d '{"component":"AudioSource","key":"ClientID","value":"your-spotify-client-id","category":"Spotify"}'

# Set USB Radio port
curl -X POST http://localhost:5100/api/configuration \
  -H "Content-Type: application/json" \
  -d '{"component":"AudioSource","key":"USBRadio_USBPort","value":"/dev/ttyUSB0","category":"USBRadio"}'
```

## Test Console Application

A test console application is included to validate the audio functionality:

```bash
cd RadioConsole/RadioConsole.TestApp
dotnet run
```

### Menu Options

1. **Test Streaming Audio Formats** - Verifies format support and content types
2. **Test Global Audio Controls** - Tests volume, balance, EQ, and playback controls
3. **Test Audio Source Management (Create Only)** - Creates and manages sources
4. **Test Audio Source Create & Play** - Creates sources and plays them
5. **Interactive Audio Source Player** - Interactive menu for managing sources
6. **Test All Features** - Runs all tests

### Interactive Player Commands

- `1-6` - Create different source types
- `P` - Play a source
- `A` - Pause a source
- `R` - Resume a source
- `S` - Stop a source
- `X` - Stop all sources
- `0` - Return to main menu

## Priority Ducking

When a high-priority audio source starts playing:
1. All low-priority (standard) sources are automatically ducked (volume reduced)
2. The duck percentage is configurable (default: 20%)
3. When the high-priority source stops, standard sources are restored to normal volume

This enables scenarios like:
- Radio playing at normal volume
- Doorbell sound plays → Radio volume automatically lowers
- Doorbell finishes → Radio volume restored

## Swagger Documentation

All endpoints are documented in Swagger. Access the Swagger UI at:
```
http://localhost:5100/swagger
```

## API Reference Summary

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/streaming/stream.{format}` | GET | Audio stream in specified format |
| `/api/StreamAudio/info` | GET | Streaming endpoint information |
| `/api/StreamAudio/status` | GET | Streaming service status |
| `/api/AudioDeviceManager/global/volume` | GET/POST | Global volume control |
| `/api/AudioDeviceManager/global/balance` | GET/POST | Global balance control |
| `/api/AudioDeviceManager/global/equalization` | GET/POST | Global EQ control |
| `/api/AudioDeviceManager/global/playback` | GET | Get playback state |
| `/api/AudioDeviceManager/global/pause` | POST | Pause all audio |
| `/api/AudioDeviceManager/global/play` | POST | Resume all audio |
| `/api/AudioDeviceManager/global/stop` | POST | Stop all audio |
| `/api/AudioSource` | GET/DELETE | List/stop all sources |
| `/api/AudioSource/{type}` | POST | Create source of type |
| `/api/AudioSource/{id}` | GET/DELETE | Get info/stop source |
| `/api/AudioSource/{id}/play` | POST | Play source |
| `/api/AudioSource/{id}/pause` | POST | Pause source |
| `/api/AudioSource/{id}/resume` | POST | Resume source |
