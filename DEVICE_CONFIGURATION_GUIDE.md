# Audio Device Configuration Guide

This guide explains how to configure and manage audio input and output devices in the Radio Console application.

## Overview

The Radio Console application supports dynamic configuration of audio devices, allowing you to:
- Add new audio input devices (USB audio, file playback, text-to-speech, Spotify)
- Add new audio output devices (wired soundbar, Chromecast)
- Edit existing device configurations
- Remove devices you no longer need
- Enable/disable devices without deleting them

All configured devices are persisted to local storage and automatically loaded when the application starts.

## Accessing Device Management

1. Start the Radio Console Blazor application
2. Navigate to the **Devices** page using the navigation menu
3. You'll see two sections: Audio Input Devices and Audio Output Devices

## Adding a New Device

### Adding an Audio Input Device

1. Click the **Add Input** button in the Audio Input Devices section
2. Select the device type from the dropdown:
   - **USB Audio Device**: Captures audio from a USB device (radio receiver, turntable, etc.)
   - **Audio File**: Plays audio from an MP3 or WAV file
   - **Text-to-Speech**: Converts text to speech using eSpeak TTS
   - **Spotify**: Streams audio from Spotify (requires configuration)

3. Fill in the required fields:
   - **Name**: Give your device a friendly name (e.g., "Vinyl Phonograph", "Doorbell Chime")
   - **Description**: Add an optional description

4. Configure device-specific parameters (varies by device type):

   **USB Audio Device**:
   - **Device Number**: USB device number (-1 for default device, or 0, 1, 2, etc. for specific devices)

   **Audio File**:
   - **File Path**: Full path to the audio file (e.g., `/home/user/music/doorbell.mp3`)
   - **Priority**: Priority level (Low, Medium, High, Critical) - affects event audio interruption behavior
   - **Repeat Count**: Number of times to repeat (-1 = no repeat, 0 = infinite, 1+ = specific count)

   **Text-to-Speech**:
   - No additional parameters required (TTS service must be configured separately)

   **Spotify**:
   - No additional parameters required (Spotify credentials must be configured separately)

5. Check "Enable this device" to make it available immediately
6. Click **Add** to save the configuration

### Adding an Audio Output Device

1. Click the **Add Output** button in the Audio Output Devices section
2. Select the device type from the dropdown:
   - **Wired Soundbar**: Direct wired audio connection
   - **Chromecast Device**: Network streaming to Chromecast

3. Fill in the required fields (same as inputs)
4. No device-specific parameters are currently required for outputs
5. Check "Enable this device" to make it available immediately
6. Click **Add** to save the configuration

## Editing an Existing Device

1. Find the device you want to edit in the list
2. Click the **Edit** (pencil) icon in the Actions column
3. Modify the device configuration as needed
4. Click **Update** to save your changes

**Note**: You cannot change the device type when editing. If you need a different device type, delete the device and create a new one.

## Removing a Device

1. Find the device you want to remove in the list
2. Click the **Delete** (trash) icon in the Actions column
3. Confirm the deletion when prompted

**Warning**: Deleting a device will remove all its configuration. This action cannot be undone.

## Enabling/Disabling Devices

Instead of deleting a device, you can temporarily disable it:

1. Click the **Edit** icon for the device
2. Uncheck "Enable this device"
3. Click **Update**

Disabled devices will show a "Disabled" status in the list and won't be available for playback.

## Device Status Indicators

Each device in the list shows a status indicator:

- **Available** (Green): Device is enabled and available for use
- **Unavailable** (Orange): Device is enabled but not available (hardware not connected, file not found, etc.)
- **Disabled** (Gray): Device is disabled and won't be loaded

## Using Configured Devices

Once devices are configured, they automatically appear in the Audio Control page:

1. Navigate to the **Audio Control** page (home page)
2. Select your configured input device from the "Select Input" dropdown
3. Select your configured output device from the "Select Output" dropdown
4. Click **Play** to start audio playback

Both statically registered devices (defined in code) and dynamically configured devices (added through the UI) appear in these dropdowns.

## Configuration Storage

Device configurations are stored in JSON files in the application data directory:

- **Linux/macOS**: `~/.local/share/RadioConsole/storage/`
- **Windows**: `%APPDATA%\RadioConsole\storage\`

The following files store device configurations:
- `device_registry_inputs.json`: All input device configurations
- `device_registry_outputs.json`: All output device configurations

You can back up these files to preserve your device configurations.

## Example Configurations

### Example 1: USB Turntable Input

**Device Type**: USB Audio Device  
**Name**: Vinyl Phonograph  
**Description**: Audio-Technica AT-LP120 USB turntable  
**Device Number**: 0  
**Enabled**: Yes

### Example 2: Doorbell Audio File

**Device Type**: Audio File  
**Name**: Doorbell Chime  
**Description**: Doorbell notification sound  
**File Path**: `/home/user/audio/doorbell.mp3`  
**Priority**: High  
**Repeat Count**: 1  
**Enabled**: Yes

### Example 3: Living Room Chromecast

**Device Type**: Chromecast Device  
**Name**: Living Room Chromecast  
**Description**: Chromecast Audio in living room  
**Enabled**: Yes

## Troubleshooting

### Device Shows as "Unavailable"

**USB Audio Device**:
- Verify the USB device is connected
- Check the device number is correct
- Run `arecord -l` on Linux to see available audio input devices

**Audio File**:
- Verify the file path is correct and absolute
- Ensure the file exists and is readable
- Check the file format is MP3 or WAV

**Text-to-Speech**:
- Verify eSpeak TTS is installed on your system
- On Linux: `sudo apt-get install espeak`

**Chromecast**:
- Verify the Chromecast device is on the same network
- Check network connectivity

### Device Doesn't Appear in Audio Control

- Make sure the device is **Enabled** in Device Management
- Check the device status shows as **Available**
- Try restarting the application to reload device configurations

### Configuration Changes Don't Take Effect

- After adding or editing a device, it should be available immediately
- If not, try restarting the application
- Check application logs for error messages

## Best Practices

1. **Use descriptive names**: Choose names that clearly identify the device or its purpose
2. **Test devices after configuration**: After adding a device, test it on the Audio Control page
3. **Back up configurations**: Periodically back up the storage directory
4. **Disable instead of delete**: Disable devices you might use later instead of deleting them
5. **Use appropriate priority levels**: For event audio inputs, set priority according to importance (doorbell = High, reminders = Medium)

## API Access

Devices can also be managed programmatically via the REST API:

- `GET /api/devices/inputs` - List all input devices
- `POST /api/devices/inputs` - Add a new input device
- `PUT /api/devices/inputs/{id}` - Update an input device
- `DELETE /api/devices/inputs/{id}` - Remove an input device
- `GET /api/devices/outputs` - List all output devices
- `POST /api/devices/outputs` - Add a new output device
- `PUT /api/devices/outputs/{id}` - Update an output device
- `DELETE /api/devices/outputs/{id}` - Remove an output device
- `GET /api/devices/types/inputs` - Get available input types
- `GET /api/devices/types/outputs` - Get available output types

See the Swagger UI at `http://localhost:5000/swagger` for detailed API documentation.
