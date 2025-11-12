# TTS Setup Guide for Raspberry Pi

This guide explains how to set up Text-to-Speech (TTS) engines on a Raspberry Pi 5 for use with the RadioConsole TextEventInput. RadioConsole supports three TTS engines:

1. **eSpeak-ng** - Lightweight, fast, offline TTS
2. **Piper** - High-quality neural TTS with offline models
3. **Google Cloud TTS** - Premium cloud-based neural TTS

All engines support real-time audio streaming for low-latency playback.

## Table of Contents

- [Choosing a TTS Engine](#choosing-a-tts-engine)
- [Configuration](#configuration)
- [eSpeak-ng Setup](#espeak-ng-setup)
- [Piper Setup](#piper-setup)
- [Google Cloud TTS Setup](#google-cloud-tts-setup)
- [Testing Your TTS Setup](#testing-your-tts-setup)
- [Troubleshooting](#troubleshooting)

## Choosing a TTS Engine

| Feature | eSpeak-ng | Piper | Google Cloud TTS |
|---------|-----------|-------|------------------|
| Quality | Good (robotic) | Excellent (natural) | Excellent (natural) |
| Speed | Very fast | Fast | Moderate (network) |
| Internet Required | No | No | Yes |
| Disk Space | ~10MB | ~50-200MB per model | Minimal |
| Setup Complexity | Easy | Moderate | Moderate |
| Cost | Free | Free | Pay per character |
| Languages | 50+ | 10+ | 100+ |
| Audio Streaming | Yes | Yes | Yes |
| Best For | Announcements, alerts | General purpose | Production apps |

## Configuration

All TTS engines are configured in `appsettings.json`. Select your engine by setting the `Engine` property:

```json
{
  "Tts": {
    "Engine": "EspeakNG",
    "EspeakNg": { ... },
    "Piper": { ... },
    "GoogleCloud": { ... }
  }
}
```

Valid engine values:
- `EspeakNG` (default)
- `Piper`
- `GoogleCloud`

---

## eSpeak-ng Setup

### What is eSpeak-ng?

eSpeak-ng is a compact, open-source text-to-speech synthesizer that runs entirely offline. It's lightweight, fast, and provides clear speech synthesis without requiring internet connectivity or large model files.

**Key Features:**
- Lightweight and fast - minimal resource usage
- Runs completely offline (no internet required)
- Available on most Linux distributions
- Multiple language support (over 50 languages)
- No model files required
- Real-time audio streaming support
- Pre-installed on many Raspberry Pi OS versions

### Prerequisites

- Raspberry Pi 5 with Raspberry Pi OS (32-bit or 64-bit)
- Audio output device configured (speakers, headphones, or soundbar)
- sudo privileges for installation (if not already installed)

### Installation Steps

#### 1. Check if eSpeak-ng is Already Installed

```bash
# Check for espeak-ng (preferred)
which espeak-ng

# Or check for classic espeak
which espeak

# Test if it works
echo "Hello World" | espeak-ng
```

If you hear speech, eSpeak-ng is already installed and working!

#### 2. Install eSpeak-ng (if needed)

**Option 1: Install eSpeak-ng (Next Generation - Recommended)**
```bash
sudo apt update
sudo apt install espeak-ng
```

**Option 2: Install classic eSpeak (fallback)**
```bash
sudo apt update
sudo apt install espeak
```

#### 3. Test eSpeak-ng Installation

```bash
# Test with a simple phrase
echo "This is a test of eSpeak text to speech" | espeak-ng

# Test with custom voice
echo "Testing the American voice" | espeak-ng -v en-us

# Test with different speed
echo "This is slower speech" | espeak-ng -s 120
echo "This is faster speech" | espeak-ng -s 250
```

#### 4. List Available Voices

```bash
# List all available voices
espeak-ng --voices
```

Common voices:
- `en` - English (default)
- `en-us` - English (American)
- `en-gb` - English (British)
- `es` - Spanish
- `fr` - French
- `de` - German
- `it` - Italian
- `pt` - Portuguese

### eSpeak-ng Configuration

Edit `appsettings.json`:

```json
{
  "Tts": {
    "Engine": "EspeakNG",
    "EspeakNg": {
      "ExecutablePath": "espeak-ng",
      "Voice": "en-us",
      "Speed": 175,
      "Pitch": 50,
      "Volume": 100,
      "WordGap": 0,
      "SampleRate": 22050
    }
  }
}
```

**Configuration Parameters:**

- **ExecutablePath**: Path to eSpeak executable
  - Default: `espeak-ng` (uses system PATH)
  - Alternative: `espeak` (for classic version)
  - Full path: `/usr/bin/espeak-ng` (if needed)
  
- **Voice**: Voice/language to use
  - Default: `en-us` (American English)
  - Examples: `en`, `en-gb`, `es`, `fr`, `de`
  - Use `espeak-ng --voices` to see all available voices
  
- **Speed**: Words per minute
  - Default: 175
  - Range: 80-450
  - Lower = slower speech (e.g., 120 for clarity)
  - Higher = faster speech (e.g., 250 for quick announcements)
  
- **Pitch**: Pitch adjustment
  - Default: 50
  - Range: 0-99
  - Lower = deeper voice
  - Higher = higher voice
  
- **Volume**: Amplitude/loudness
  - Default: 100
  - Range: 0-200
  - Adjust if TTS is too quiet or too loud
  
- **WordGap**: Gap between words (in units of 10ms)
  - Default: 0 (no extra gap)
  - Increase for more distinct word separation
  - Example: 10 = 100ms gap between words
  
- **SampleRate**: Output sample rate
  - Default: 22050 Hz
  - Standard audio quality

### eSpeak-ng Voice Tuning

**For Clear Announcements:**
```json
{
  "Voice": "en-us",
  "Speed": 150,
  "Pitch": 50,
  "Volume": 120
}
```

**For Quick Notifications:**
```json
{
  "Voice": "en-us",
  "Speed": 200,
  "Pitch": 55,
  "Volume": 100
}
```

**For Deeper Voice:**
```json
{
  "Voice": "en-us",
  "Speed": 175,
  "Pitch": 30,
  "Volume": 100
}
```

### Audio Streaming

eSpeak-ng streams audio directly from stdout in WAV format, providing near-instantaneous playback start. The RadioConsole API reads the audio stream as it's generated, enabling real-time text-to-speech with minimal latency (< 100ms).

---

## Piper Setup

### What is Piper?

Piper is a fast, local neural text-to-speech system that uses ONNX models. It provides high-quality, natural-sounding speech synthesis that runs entirely on your device without requiring internet connectivity.

**Key Features:**
- High-quality neural TTS
- Fast inference on CPU (optimized for Raspberry Pi)
- Runs completely offline
- Multiple voice models available
- ONNX-based for broad compatibility
- Natural-sounding speech
- Real-time audio streaming support

### Prerequisites

- Raspberry Pi 5 (or Pi 4 with sufficient RAM)
- Raspberry Pi OS (64-bit recommended for best performance)
- At least 1GB free disk space per voice model
- Audio output device configured

### Installation Steps

#### 1. Install Piper

**Option 1: Download Pre-built Binary (Recommended)**

```bash
# Create directory for Piper
mkdir -p ~/piper
cd ~/piper

# Download Piper for Raspberry Pi (ARM64)
wget https://github.com/rhasspy/piper/releases/download/v1.2.0/piper_arm64.tar.gz

# Extract
tar -xzf piper_arm64.tar.gz

# Make executable
chmod +x piper/piper

# Test installation
./piper/piper --version
```

#### 2. Download Voice Models

Piper requires ONNX model files for each voice. Download models from the [Piper releases page](https://github.com/rhasspy/piper/releases).

**Popular English Models:**

```bash
# Create models directory
mkdir -p ~/piper/models
cd ~/piper/models

# Download a high-quality English model (US - Lessac voice)
wget https://github.com/rhasspy/piper/releases/download/v1.2.0/en_US-lessac-medium.onnx
wget https://github.com/rhasspy/piper/releases/download/v1.2.0/en_US-lessac-medium.onnx.json

# Or download British English
wget https://github.com/rhasspy/piper/releases/download/v1.2.0/en_GB-alan-medium.onnx
wget https://github.com/rhasspy/piper/releases/download/v1.2.0/en_GB-alan-medium.onnx.json
```

**Available Voice Quality Levels:**
- `low` - Fastest, smallest (10-20MB), good for testing
- `medium` - Good balance of quality and speed (20-50MB) **Recommended**
- `high` - Best quality, slower (50-100MB)

#### 3. Test Piper Installation

```bash
# Test with a simple phrase (outputs to file)
echo "This is a test of Piper text to speech" | ~/piper/piper/piper \
  --model ~/piper/models/en_US-lessac-medium.onnx \
  --output_file test.wav

# Play the audio
aplay test.wav

# Test with raw output (for streaming)
echo "Testing streaming output" | ~/piper/piper/piper \
  --model ~/piper/models/en_US-lessac-medium.onnx \
  --output_raw | aplay -r 22050 -f S16_LE -c 1
```

### Piper Configuration

Edit `appsettings.json`:

```json
{
  "Tts": {
    "Engine": "Piper",
    "Piper": {
      "ExecutablePath": "/home/pi/piper/piper/piper",
      "ModelPath": "/home/pi/piper/models/en_US-lessac-medium.onnx",
      "ConfigPath": "/home/pi/piper/models/en_US-lessac-medium.onnx.json",
      "SpeakingRate": 1.0,
      "SentenceSilence": 0.2,
      "SampleRate": 22050
    }
  }
}
```

**Configuration Parameters:**

- **ExecutablePath**: Path to the Piper executable
  - Example: `/home/pi/piper/piper/piper`
  - Must be absolute path or in system PATH
  
- **ModelPath**: Path to the voice model file (.onnx)
  - Must match a downloaded model
  - Different models = different voices
  
- **ConfigPath**: Path to the voice config file (.onnx.json)
  - Must be in same directory as model
  - Required for proper voice synthesis
  
- **SpeakingRate**: Speed multiplier
  - Default: 1.0 (normal speed)
  - Range: 0.5-2.0
  - Lower = slower (e.g., 0.8 for clarity)
  - Higher = faster (e.g., 1.5 for quick announcements)
  
- **SentenceSilence**: Pause between sentences (seconds)
  - Default: 0.2
  - Range: 0.0-1.0
  - Increase for more dramatic pauses
  
- **SampleRate**: Output sample rate
  - Default: 22050 Hz
  - Match to your audio system requirements

### Available Piper Voices

Visit [Piper Samples](https://rhasspy.github.io/piper-samples/) to hear voice samples.

**Popular Voices:**
- `en_US-lessac-medium` - Clear American English (recommended)
- `en_GB-alan-medium` - British English
- `en_US-amy-medium` - Female American English
- `en_US-joe-medium` - Male American English
- `es_ES-davefx-medium` - Spanish (Spain)
- `fr_FR-siwis-medium` - French
- `de_DE-thorsten-medium` - German

### Audio Streaming

Piper outputs raw PCM audio to stdout, which RadioConsole converts to WAV format for streaming. The conversion is performed in-memory, maintaining low latency (200-500ms) while providing high-quality neural TTS output.

---

## Google Cloud TTS Setup

### What is Google Cloud TTS?

Google Cloud Text-to-Speech is a premium cloud-based TTS service that provides highly natural-sounding voices powered by Google's neural network models (WaveNet and Neural2).

**Key Features:**
- Highest quality neural TTS
- 100+ languages and variants
- Multiple voice types per language
- SSML support for advanced control
- Extensive voice customization
- Real-time audio streaming support
- Requires internet connection
- Pay-per-use pricing

### Prerequisites

- Google Cloud Platform account
- Active billing account (includes free tier)
- Internet connection on Raspberry Pi
- Text-to-Speech API enabled

### Setup Steps

#### 1. Create Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing
3. Enable billing for the project (required even for free tier)

#### 2. Enable Text-to-Speech API

**Using gcloud CLI:**
```bash
# Install gcloud CLI if needed
curl https://sdk.cloud.google.com | bash
exec -l $SHELL

# Initialize
gcloud init

# Enable API
gcloud services enable texttospeech.googleapis.com
```

**Or via web console:**
1. Navigate to APIs & Services > Library
2. Search for "Cloud Text-to-Speech API"
3. Click Enable

#### 3. Create Service Account

```bash
# Create service account
gcloud iam service-accounts create radioconsole-tts \
  --display-name="RadioConsole TTS"

# Get project ID
PROJECT_ID=$(gcloud config get-value project)

# Grant permissions
gcloud projects add-iam-policy-binding $PROJECT_ID \
  --member="serviceAccount:radioconsole-tts@$PROJECT_ID.iam.gserviceaccount.com" \
  --role="roles/cloudtexttospeech.user"
```

#### 4. Download Credentials

```bash
# Create credentials directory
mkdir -p ~/radioconsole/credentials

# Generate and download key
gcloud iam service-accounts keys create ~/radioconsole/credentials/google-tts.json \
  --iam-account=radioconsole-tts@$PROJECT_ID.iam.gserviceaccount.com

# Set permissions
chmod 600 ~/radioconsole/credentials/google-tts.json
```

**Or via web console:**
1. Go to IAM & Admin > Service Accounts
2. Click on your service account
3. Go to Keys tab
4. Click Add Key > Create New Key
5. Select JSON format
6. Download and save to Raspberry Pi

#### 5. Test Google Cloud TTS

```bash
# Set credentials
export GOOGLE_APPLICATION_CREDENTIALS=~/radioconsole/credentials/google-tts.json

# Test using curl (requires jq)
curl -X POST \
  -H "Authorization: Bearer $(gcloud auth application-default print-access-token)" \
  -H "Content-Type: application/json; charset=utf-8" \
  -d '{
    "input": {"text": "Hello World"},
    "voice": {"languageCode": "en-US", "name": "en-US-Neural2-A"},
    "audioConfig": {"audioEncoding": "LINEAR16"}
  }' \
  "https://texttospeech.googleapis.com/v1/text:synthesize" | \
  jq -r .audioContent | base64 -d > test.wav

# Play the audio
aplay test.wav
```

### Google Cloud TTS Configuration

Edit `appsettings.json`:

```json
{
  "Tts": {
    "Engine": "GoogleCloud",
    "GoogleCloud": {
      "CredentialsPath": "/home/pi/radioconsole/credentials/google-tts.json",
      "LanguageCode": "en-US",
      "VoiceName": "en-US-Neural2-A",
      "SpeakingRate": 1.0,
      "Pitch": 0.0,
      "VolumeGainDb": 0.0,
      "SampleRate": 22050
    }
  }
}
```

**Configuration Parameters:**

- **CredentialsPath**: Path to credentials JSON file
  - Required unless GOOGLE_APPLICATION_CREDENTIALS env var is set
  - Must have read permissions
  
- **LanguageCode**: Language for the voice
  - Format: language-REGION (e.g., en-US, en-GB, es-ES)
  - See [supported languages](https://cloud.google.com/text-to-speech/docs/voices)
  
- **VoiceName**: Specific voice to use (optional)
  - If empty, uses default voice for language
  - Examples: `en-US-Neural2-A`, `en-US-Standard-B`
  - See [voice list](https://cloud.google.com/text-to-speech/docs/voices)
  
- **SpeakingRate**: Speed multiplier
  - Default: 1.0 (normal speed)
  - Range: 0.25-4.0
  - Lower = slower
  - Higher = faster
  
- **Pitch**: Pitch adjustment in semitones
  - Default: 0.0 (no change)
  - Range: -20.0 to 20.0
  - Negative = lower pitch
  - Positive = higher pitch
  
- **VolumeGainDb**: Volume gain in decibels
  - Default: 0.0 (no change)
  - Range: -96.0 to 16.0
  - Increase for louder output
  
- **SampleRate**: Output sample rate
  - Default: 22050 Hz
  - Options: 8000, 16000, 22050, 24000, 32000, 44100, 48000 Hz

### Popular Google Cloud Voices

**English (US):**
- `en-US-Neural2-A` - Male, warm
- `en-US-Neural2-C` - Female, clear
- `en-US-Neural2-F` - Female, professional
- `en-US-Wavenet-A` - Male, natural (premium)

**English (UK):**
- `en-GB-Neural2-A` - Female
- `en-GB-Neural2-B` - Male
- `en-GB-Wavenet-B` - Male, natural (premium)

**Other Languages:**
- `es-ES-Neural2-A` - Spanish (Spain)
- `fr-FR-Neural2-A` - French
- `de-DE-Neural2-B` - German
- `ja-JP-Neural2-B` - Japanese

### Audio Streaming

Google Cloud TTS returns LINEAR16 PCM audio which RadioConsole converts to WAV format. The API call and conversion happen asynchronously, enabling efficient streaming playback. Network latency typically adds 500-1000ms depending on your connection.

### Pricing

Google Cloud TTS has a pay-per-character model:

- **Standard voices**: $4 per 1 million characters
- **WaveNet voices**: $16 per 1 million characters
- **Neural2 voices**: $16 per 1 million characters
- **Free tier**: First 4 million characters per month (Standard voices)

**Example Usage:**
- 1000 announcements of 50 characters each = 50,000 characters
- Monthly cost: $0.20-$0.80 depending on voice type
- Free tier covers ~80,000 announcements/month with Standard voices

---

## Testing Your TTS Setup

### RadioConsole API Test

1. Start the RadioConsole API:

```bash
cd /path/to/Radio/src/RadioConsole.Api
dotnet run
```

2. Check the logs to see which TTS engine initialized:

```
TTS service initialized successfully using eSpeak-ng engine
```

3. Test with curl:

```bash
# Test Hello World announcement
curl -X POST "http://localhost:5269/api/eventsexample/text/helloworld"

# Test custom text announcement
curl -X POST "http://localhost:5269/api/eventsexample/text/announce?text=Welcome%20to%20RadioConsole"

# Test with longer text
curl -X POST "http://localhost:5269/api/eventsexample/text/announce?text=The%20weather%20today%20is%20sunny%20with%20a%20high%20of%2075%20degrees"
```

4. Or use Swagger UI:

```
http://localhost:5269/swagger
```

Navigate to **EventsExample** endpoints and try the text announcement endpoints.

### Performance Comparison

Typical performance on Raspberry Pi 5:

| Engine | Latency | Quality | CPU Usage | Memory |
|--------|---------|---------|-----------|--------|
| eSpeak-ng | < 100ms | Good | < 5% | ~10MB |
| Piper | 200-500ms | Excellent | 10-20% | ~100MB |
| Google Cloud | 500-1000ms | Excellent | < 5% | ~50MB |

**Latency** = Time from API request to audio playback start
**Quality** = Subjective speech naturalness
**CPU Usage** = Average during synthesis
**Memory** = Additional RAM required

### Streaming Behavior

All TTS engines support real-time audio streaming:

**eSpeak-ng:**
- Generates audio incrementally as it processes text
- Streams directly from process stdout
- Near-instantaneous start of playback
- Best for time-sensitive announcements

**Piper:**
- Generates complete audio before streaming
- Converts PCM to WAV in-memory
- Moderate latency with excellent quality
- Best for general-purpose TTS

**Google Cloud:**
- Receives complete audio from API
- Converts LINEAR16 to WAV format
- Network latency dominates
- Best for production quality

### Example Use Cases

#### Home Automation Announcements

```bash
# Doorbell (fast notification)
curl -X POST "http://localhost:5269/api/eventsexample/text/announce?text=Someone%20is%20at%20the%20front%20door"

# Weather forecast (detailed information)
curl -X POST "http://localhost:5269/api/eventsexample/text/announce?text=Today's%20forecast:%20Sunny%20with%20a%20high%20of%2075%20degrees%20and%20low%20of%2055"

# Timer expiration
curl -X POST "http://localhost:5269/api/eventsexample/text/announce?text=Your%2010%20minute%20timer%20has%20expired"

# Calendar reminder
curl -X POST "http://localhost:5269/api/eventsexample/text/announce?text=Meeting%20starts%20in%2015%20minutes"

# Security alert
curl -X POST "http://localhost:5269/api/eventsexample/text/announce?text=Motion%20detected%20in%20the%20backyard"
```

---

## Troubleshooting

### eSpeak-ng Issues

**Error: eSpeak executable not found**

```bash
# Check if installed
which espeak-ng

# Install if missing
sudo apt update
sudo apt install espeak-ng

# Update configuration to use full path
# In appsettings.json:
"ExecutablePath": "/usr/bin/espeak-ng"
```

**No audio output**

```bash
# Test audio system
speaker-test -t wav -c 2

# Check volume levels
alsamixer

# Test eSpeak directly
echo "Testing audio" | espeak-ng

# Check ALSA configuration
aplay -l
```

**Speech quality issues**

- Reduce `Speed` to 120-150 for clearer speech
- Increase `WordGap` to 5-10 for distinct words
- Try different voices (`en-gb` is often clearer than `en-us`)
- Avoid extreme `Volume` settings (stay between 80-120)

### Piper Issues

**Error: Model file not found**

```bash
# Verify model path
ls -la ~/piper/models/en_US-lessac-medium.onnx
ls -la ~/piper/models/en_US-lessac-medium.onnx.json

# Check permissions
chmod 644 ~/piper/models/*.onnx*

# Update configuration with absolute paths
# In appsettings.json:
"ModelPath": "/home/pi/piper/models/en_US-lessac-medium.onnx",
"ConfigPath": "/home/pi/piper/models/en_US-lessac-medium.onnx.json"
```

**Error: Piper executable not found**

```bash
# Check installation
ls -la ~/piper/piper/piper

# Make executable if needed
chmod +x ~/piper/piper/piper

# Update configuration with full path
# In appsettings.json:
"ExecutablePath": "/home/pi/piper/piper/piper"
```

**Slow performance or choppy audio**

```bash
# Use lower quality model
cd ~/piper/models
wget https://github.com/rhasspy/piper/releases/download/v1.2.0/en_US-lessac-low.onnx
wget https://github.com/rhasspy/piper/releases/download/v1.2.0/en_US-lessac-low.onnx.json

# Update configuration to use low quality model
# Reduce SpeakingRate if audio is choppy
"SpeakingRate": 0.9
```

**Architecture mismatch**

```bash
# Check your architecture
uname -m
# arm64 or aarch64 = 64-bit
# armv7l = 32-bit

# Download correct binary
# For 64-bit: piper_arm64.tar.gz
# For 32-bit: piper_armv7l.tar.gz
```

### Google Cloud TTS Issues

**Error: Credentials not found**

```bash
# Verify file exists
ls -la ~/radioconsole/credentials/google-tts.json

# Check permissions
chmod 600 ~/radioconsole/credentials/google-tts.json

# Set environment variable
export GOOGLE_APPLICATION_CREDENTIALS=~/radioconsole/credentials/google-tts.json

# Or update configuration with absolute path
# In appsettings.json:
"CredentialsPath": "/home/pi/radioconsole/credentials/google-tts.json"
```

**Error: API not enabled**

```bash
# Enable API
gcloud services enable texttospeech.googleapis.com

# Verify
gcloud services list --enabled | grep texttospeech

# Check project
gcloud config get-value project
```

**Error: Insufficient permissions**

```bash
# Get project ID
PROJECT_ID=$(gcloud config get-value project)

# Grant required role
gcloud projects add-iam-policy-binding $PROJECT_ID \
  --member="serviceAccount:radioconsole-tts@$PROJECT_ID.iam.gserviceaccount.com" \
  --role="roles/cloudtexttospeech.user"

# Regenerate credentials
gcloud iam service-accounts keys create ~/radioconsole/credentials/google-tts.json \
  --iam-account=radioconsole-tts@$PROJECT_ID.iam.gserviceaccount.com
```

**High latency**

```bash
# Check internet connection
ping -c 5 texttospeech.googleapis.com

# Test API latency
time curl -X POST \
  -H "Authorization: Bearer $(gcloud auth application-default print-access-token)" \
  -H "Content-Type: application/json; charset=utf-8" \
  -d '{"input": {"text": "Test"}, "voice": {"languageCode": "en-US"}, "audioConfig": {"audioEncoding": "LINEAR16"}}' \
  "https://texttospeech.googleapis.com/v1/text:synthesize"
```

**Billing or quota errors**

```bash
# Check quotas
gcloud services list --enabled
gcloud services quota list --service=texttospeech.googleapis.com

# Verify billing is enabled
gcloud beta billing projects describe $PROJECT_ID
```

### General Issues

**TTS service not available**

```bash
# Check RadioConsole logs
cd /path/to/Radio/src/RadioConsole.Api
dotnet run

# Look for TTS initialization messages in the output
# Should see: "TTS service initialized successfully using <Engine> engine"
```

**Wrong engine being used**

```bash
# Verify configuration
cat src/RadioConsole.Api/appsettings.json | jq '.Tts.Engine'

# Check for typos (case-insensitive but must match)
# Valid values: "EspeakNG", "Piper", "GoogleCloud"
```

**Audio format issues**

All engines output WAV format (PCM 16-bit, mono, 22050 Hz by default):
- If audio is distorted, check `SampleRate` matches your system
- Common rates: 22050, 44100, 48000 Hz
- NAudio mixer in RadioConsole handles format conversion

**Memory or performance issues**

```bash
# Check system resources
free -h
top

# For Raspberry Pi 4 or lower:
# - Use eSpeak-ng for minimal resource usage
# - Use Piper with "low" quality models
# - Avoid high SampleRate settings

# For Raspberry Pi 5:
# - All engines should run smoothly
# - Piper "medium" quality recommended
# - Google Cloud works well with good internet
```

---

## System Integration

### Auto-start RadioConsole API

Create a systemd service to auto-start the API on boot:

```bash
sudo nano /etc/systemd/system/radioconsole.service
```

```ini
[Unit]
Description=RadioConsole API with TTS
After=network.target sound.target

[Service]
Type=notify
User=pi
WorkingDirectory=/home/pi/Radio/src/RadioConsole.Api
ExecStart=/usr/bin/dotnet /home/pi/Radio/src/RadioConsole.Api/bin/Release/net9.0/RadioConsole.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=GOOGLE_APPLICATION_CREDENTIALS=/home/pi/radioconsole/credentials/google-tts.json

[Install]
WantedBy=multi-user.target
```

Enable and start:

```bash
sudo systemctl enable radioconsole
sudo systemctl start radioconsole
sudo systemctl status radioconsole

# View logs
sudo journalctl -u radioconsole -f
```

### Environment Variables

For Google Cloud TTS, you can set the credentials path globally:

```bash
echo 'export GOOGLE_APPLICATION_CREDENTIALS=/home/pi/radioconsole/credentials/google-tts.json' >> ~/.bashrc
source ~/.bashrc
```

---

## Additional Resources

### eSpeak-ng
- **GitHub**: https://github.com/espeak-ng/espeak-ng
- **Documentation**: http://espeak.sourceforge.net/
- **Voice Samples**: http://espeak.sourceforge.net/voices.html

### Piper
- **GitHub**: https://github.com/rhasspy/piper
- **Voice Samples**: https://rhasspy.github.io/piper-samples/
- **Model Downloads**: https://github.com/rhasspy/piper/releases
- **Documentation**: https://github.com/rhasspy/piper/blob/master/README.md

### Google Cloud TTS
- **Documentation**: https://cloud.google.com/text-to-speech/docs
- **Voice List**: https://cloud.google.com/text-to-speech/docs/voices
- **Pricing**: https://cloud.google.com/text-to-speech/pricing
- **SSML Guide**: https://cloud.google.com/text-to-speech/docs/ssml
- **Quickstart**: https://cloud.google.com/text-to-speech/docs/quickstart-client-libraries

### RadioConsole
- **Events Documentation**: See EVENTS_DOCUMENTATION.md
- **Architecture**: See ARCHITECTURE.md
- **Repository**: https://github.com/mmackelprang/Radio

---

## Support

For issues specific to:
- **eSpeak-ng**: Open an issue on https://github.com/espeak-ng/espeak-ng
- **Piper**: Open an issue on https://github.com/rhasspy/piper
- **Google Cloud TTS**: Visit https://cloud.google.com/text-to-speech/docs/support
- **RadioConsole Integration**: Open an issue on the RadioConsole repository

---

*Last Updated: 2024-11-12*
