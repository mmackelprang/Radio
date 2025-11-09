# eSpeak TTS Setup Guide for Raspberry Pi

This guide explains how to set up eSpeak TTS (Text-to-Speech) engine on a Raspberry Pi 5 for use with the RadioConsole TextEventInput.

## What is eSpeak TTS?

eSpeak is a compact, open-source text-to-speech synthesizer that runs entirely offline on your Raspberry Pi. It's lightweight, fast, and provides clear speech synthesis without requiring internet connectivity or large model files.

**Key Features:**
- Lightweight and fast - minimal resource usage
- Runs completely offline (no internet required)
- Available on most Linux distributions
- Multiple language support (over 50 languages)
- No model files required
- Pre-installed on many Raspberry Pi OS versions

## Prerequisites

- Raspberry Pi 5 with Raspberry Pi OS (32-bit or 64-bit)
- Audio output device configured (speakers, headphones, or soundbar)
- sudo privileges for installation (if not already installed)

## Installation Steps

### 1. Check if eSpeak is Already Installed

Many Raspberry Pi OS installations include eSpeak by default:

```bash
# Check for espeak-ng (preferred)
which espeak-ng

# Or check for classic espeak
which espeak

# Test if it works
echo "Hello World" | espeak-ng
```

If you hear speech, eSpeak is already installed and working!

### 2. Install eSpeak (if needed)

If eSpeak is not installed, you can easily install it:

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

### 3. Test eSpeak Installation

Verify that eSpeak is working correctly:

```bash
# Test with a simple phrase
echo "This is a test of eSpeak text to speech" | espeak-ng

# Test with custom voice
echo "Testing the American voice" | espeak-ng -v en-us

# Test with different speed
echo "This is slower speech" | espeak-ng -s 120
echo "This is faster speech" | espeak-ng -s 250
```

### 4. List Available Voices

eSpeak supports many languages and accents:

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

### 5. Configure RadioConsole API

The RadioConsole API uses eSpeak configuration from `appsettings.json`:

```json
{
  "ESpeakTts": {
    "ESpeakExecutablePath": "espeak-ng",
    "Voice": "en-us",
    "Speed": 175,
    "Pitch": 50,
    "Volume": 100,
    "WordGap": 0,
    "SampleRate": 22050
  }
}
```

**Configuration Parameters:**

- **ESpeakExecutablePath**: Path to eSpeak executable
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

### 6. Verify RadioConsole Integration

Start the RadioConsole API and test the TextEventInput:

```bash
cd /path/to/Radio/src/RadioConsole.Api
dotnet run
```

Test with curl:
```bash
# Test Hello World
curl -X POST "http://localhost:5269/api/eventsexample/text/helloworld"

# Test custom text
curl -X POST "http://localhost:5269/api/eventsexample/text/announce?text=Welcome%20to%20RadioConsole"
```

Or use Swagger UI:
```
http://localhost:5269/swagger
```

## Voice and Quality Tuning

### For Clear Announcements
```json
{
  "ESpeakTts": {
    "Voice": "en-us",
    "Speed": 150,
    "Pitch": 50,
    "Volume": 120
  }
}
```

### For Quick Notifications
```json
{
  "ESpeakTts": {
    "Voice": "en-us",
    "Speed": 200,
    "Pitch": 55,
    "Volume": 100
  }
}
```

### For Deeper Voice
```json
{
  "ESpeakTts": {
    "Voice": "en-us",
    "Speed": 175,
    "Pitch": 30,
    "Volume": 100
  }
}
```

### For British Accent
```json
{
  "ESpeakTts": {
    "Voice": "en-gb",
    "Speed": 175,
    "Pitch": 50,
    "Volume": 100
  }
}
```

## Troubleshooting

### eSpeak Not Found

**Error**: `eSpeak executable not found`

**Solution**:
```bash
# Check if eSpeak is installed
which espeak-ng
which espeak

# If not found, install it
sudo apt update
sudo apt install espeak-ng

# Or install classic espeak
sudo apt install espeak
```

### No Audio Output

**Problem**: eSpeak runs but no sound is heard

**Solution**:
```bash
# Test audio system
speaker-test -t wav -c 2

# Check volume levels
alsamixer

# Test eSpeak directly
echo "Testing audio" | espeak-ng

# Check audio output device selection
aplay -l
```

### Speech Quality Issues

**Problem**: Speech sounds unclear, too fast, or robotic

**Solution**: Adjust parameters in `appsettings.json`

For clearer speech:
- Reduce `Speed` to 120-150
- Increase `WordGap` to 5-10
- Try different voices (`en-gb` is often clearer than `en-us`)

For more natural speech:
- Use moderate `Speed` (160-180)
- Set `Pitch` to 45-55
- Avoid extremely high or low `Volume` settings

### Language/Voice Not Available

**Problem**: Specific voice or language not working

**Solution**:
```bash
# List all installed voices
espeak-ng --voices

# Install additional language support
sudo apt install espeak-ng-data

# Or install specific language packs
sudo apt search espeak
```

### Permission Issues

**Error**: Permission denied when running eSpeak

**Solution**:
```bash
# Ensure espeak-ng is executable
sudo chmod +x /usr/bin/espeak-ng

# Check audio group membership
groups
# If 'audio' is not listed, add user to audio group
sudo usermod -a -G audio $USER
# Log out and back in for changes to take effect
```

## Performance Tips

### For Best Speed
- Use `Speed` of 200-250 for quick announcements
- Keep text short and concise
- eSpeak has very low latency already

### For Best Clarity
- Use `Speed` of 140-160 for important messages
- Add `WordGap` of 5-10 for distinct words
- Use British English (`en-gb`) which tends to be clearer

### Resource Usage
- eSpeak uses minimal CPU (< 5% on Raspberry Pi 5)
- No disk space needed for models
- Near-instant startup time

## Advanced Configuration

### Using Different Voices for Different Purposes

You can create different configuration profiles or dynamically adjust settings:

```csharp
// In your code, you could potentially override settings per announcement
// For now, adjust appsettings.json for your primary use case
```

### Testing Voice Parameters

Test different settings directly from command line:

```bash
# Very slow, clear speech
echo "Test message" | espeak-ng -v en-us -s 100 -p 50 -a 150

# Fast announcement
echo "Alert!" | espeak-ng -v en-us -s 250 -p 60 -a 120

# Deep voice
echo "Welcome" | espeak-ng -v en-us -s 175 -p 20 -a 100

# High-pitched voice
echo "Reminder" | espeak-ng -v en-us -s 175 -p 80 -a 100
```

## Integration Examples

### Home Automation Use Cases

Example announcements using TextEventInput with eSpeak:

```bash
# Doorbell
curl -X POST "http://localhost:5269/api/eventsexample/text/announce?text=Someone%20is%20at%20the%20front%20door"

# Weather
curl -X POST "http://localhost:5269/api/eventsexample/text/announce?text=Today's%20forecast:%20Sunny,%20high%20of%2075%20degrees"

# Timer
curl -X POST "http://localhost:5269/api/eventsexample/text/announce?text=Your%2010%20minute%20timer%20has%20expired"

# Security
curl -X POST "http://localhost:5269/api/eventsexample/text/announce?text=Motion%20detected%20in%20the%20backyard"

# Calendar
curl -X POST "http://localhost:5269/api/eventsexample/text/announce?text=Meeting%20starts%20in%2015%20minutes"
```

## System Integration

### Auto-start RadioConsole API

Create a systemd service to auto-start the API on boot:

```bash
sudo nano /etc/systemd/system/radioconsole.service
```

```ini
[Unit]
Description=RadioConsole API with eSpeak TTS
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

[Install]
WantedBy=multi-user.target
```

Enable and start the service:
```bash
sudo systemctl enable radioconsole
sudo systemctl start radioconsole
sudo systemctl status radioconsole
```

## Comparison: eSpeak vs Other TTS Engines

### eSpeak Advantages
- ✅ No installation of large model files
- ✅ Minimal disk space (< 10MB)
- ✅ Very fast and responsive
- ✅ Pre-installed on many systems
- ✅ Works offline
- ✅ Low resource usage
- ✅ Good for announcements and alerts

### eSpeak Considerations
- More "robotic" than neural TTS (Piper, Azure)
- Best for short announcements rather than long text
- English voices are most developed

### When to Use eSpeak
- Quick system announcements
- Home automation notifications
- Alerts and reminders
- Resource-constrained systems
- When simplicity is preferred

## Additional Resources

- **eSpeak-ng GitHub**: https://github.com/espeak-ng/espeak-ng
- **eSpeak Documentation**: http://espeak.sourceforge.net/
- **Voice Samples**: Try voices at http://espeak.sourceforge.net/voices.html
- **RadioConsole Documentation**: See EVENTS_DOCUMENTATION.md

## Support

For issues specific to:
- **eSpeak TTS**: Open an issue on https://github.com/espeak-ng/espeak-ng
- **RadioConsole Integration**: Open an issue on the RadioConsole repository
- **Voice Quality**: Adjust configuration parameters or try different voices

---

*Last Updated: 2024-11-09*
