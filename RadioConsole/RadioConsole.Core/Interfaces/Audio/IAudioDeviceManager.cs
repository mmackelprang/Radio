namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Interface for managing audio devices on the system.
/// Provides enumeration and selection of ALSA/PulseAudio devices.
/// </summary>
public interface IAudioDeviceManager
{
  /// <summary>
  /// Get all available audio input devices.
  /// </summary>
  /// <returns>A list of audio device information.</returns>
  Task<IEnumerable<AudioDeviceInfo>> GetInputDevicesAsync();

  /// <summary>
  /// Get all available audio output devices.
  /// </summary>
  /// <returns>A list of audio device information.</returns>
  Task<IEnumerable<AudioDeviceInfo>> GetOutputDevicesAsync();

  /// <summary>
  /// Get the currently selected audio input device.
  /// </summary>
  /// <returns>The currently selected input device, or null if none is selected.</returns>
  Task<AudioDeviceInfo?> GetCurrentInputDeviceAsync();

  /// <summary>
  /// Get the currently selected audio output device.
  /// </summary>
  /// <returns>The currently selected output device, or null if none is selected.</returns>
  Task<AudioDeviceInfo?> GetCurrentOutputDeviceAsync();

  /// <summary>
  /// Set the audio input device by its ID.
  /// </summary>
  /// <param name="deviceId">The device identifier.</param>
  Task SetInputDeviceAsync(string deviceId);

  /// <summary>
  /// Set the audio output device by its ID.
  /// </summary>
  /// <param name="deviceId">The device identifier.</param>
  Task SetOutputDeviceAsync(string deviceId);
}

/// <summary>
/// Represents information about an audio device.
/// </summary>
public class AudioDeviceInfo
{
  /// <summary>
  /// Unique identifier for the device (e.g., ALSA device ID).
  /// </summary>
  public string Id { get; set; } = string.Empty;

  /// <summary>
  /// Human-readable name of the device.
  /// </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// Indicates if this is the default device.
  /// </summary>
  public bool IsDefault { get; set; }

  /// <summary>
  /// Device type (e.g., "USB Audio", "HDMI", "Analog").
  /// </summary>
  public string DeviceType { get; set; } = string.Empty;
}
