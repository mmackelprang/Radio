# Interfaces, Models, Enums (no SoundFlow Dependencies)

## Audio Interfaces

### IAudioPlayer
Core interface for audio playback management. Supports:
- Initialization with specific ALSA device IDs (for USB Audio on Linux/Raspberry Pi)
- Playing audio from multiple sources simultaneously
- Volume control per audio source
- Getting the mixed audio output stream

### IAudioDeviceManager
Interface for enumerating and managing audio devices on the system.
- Get available input/output devices
- Select specific audio devices by ID
- Support for ALSA device enumeration

### IAudioOutput
Interface for audio output implementations:
- **LocalAudioOutput**: Plays directly to a configurable audio sink (default device)
- **CastAudioOutput**: Streams audio to Google Cast devices via HTTP endpoint (does NOT play locally)

## Usage

Audio services are implemented in the Infrastructure layer using:
- **SoundFlow** library for cross-platform audio engine
- **SharpCaster** library for Google Cast device discovery and control

See Infrastructure layer for concrete implementations.

