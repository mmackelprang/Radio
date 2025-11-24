namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Interface for managing audio devices on the system.
/// Provides enumeration and selection of ALSA/PulseAudio devices.
/// Also provides global audio controls for volume, balance, equalization, and playback.
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

  // Global Audio Control Methods

  /// <summary>
  /// Gets the current global volume level.
  /// </summary>
  /// <returns>Volume level from 0.0 to 1.0.</returns>
  Task<float> GetGlobalVolumeAsync();

  /// <summary>
  /// Sets the global volume level for all audio sources.
  /// </summary>
  /// <param name="volume">Volume level from 0.0 to 1.0.</param>
  Task SetGlobalVolumeAsync(float volume);

  /// <summary>
  /// Gets the current global balance (pan) setting.
  /// </summary>
  /// <returns>Balance value from -1.0 (full left) to 1.0 (full right), 0.0 is center.</returns>
  Task<float> GetGlobalBalanceAsync();

  /// <summary>
  /// Sets the global balance (pan) for all audio sources.
  /// </summary>
  /// <param name="balance">Balance from -1.0 (full left) to 1.0 (full right), 0.0 is center.</param>
  Task SetGlobalBalanceAsync(float balance);

  /// <summary>
  /// Gets the current equalization settings.
  /// </summary>
  /// <returns>The current equalization settings.</returns>
  Task<EqualizationSettings> GetEqualizationAsync();

  /// <summary>
  /// Sets the global equalization settings.
  /// </summary>
  /// <param name="settings">The equalization settings to apply.</param>
  Task SetEqualizationAsync(EqualizationSettings settings);

  /// <summary>
  /// Pauses all audio playback.
  /// </summary>
  Task PauseAsync();

  /// <summary>
  /// Resumes all audio playback.
  /// </summary>
  Task PlayAsync();

  /// <summary>
  /// Stops all audio playback.
  /// </summary>
  Task StopAsync();

  /// <summary>
  /// Gets the current playback state.
  /// </summary>
  /// <returns>The current playback state.</returns>
  Task<PlaybackState> GetPlaybackStateAsync();
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

/// <summary>
/// Represents global equalization settings.
/// </summary>
public class EqualizationSettings
{
  /// <summary>
  /// Bass level adjustment in dB. Range: -12 to +12.
  /// </summary>
  public float Bass { get; set; }

  /// <summary>
  /// Midrange level adjustment in dB. Range: -12 to +12.
  /// </summary>
  public float Midrange { get; set; }

  /// <summary>
  /// Treble level adjustment in dB. Range: -12 to +12.
  /// </summary>
  public float Treble { get; set; }

  /// <summary>
  /// Whether the equalizer is enabled.
  /// </summary>
  public bool Enabled { get; set; }
}

/// <summary>
/// Represents the current audio playback state.
/// </summary>
public enum PlaybackState
{
  /// <summary>
  /// Audio is stopped.
  /// </summary>
  Stopped,

  /// <summary>
  /// Audio is playing.
  /// </summary>
  Playing,

  /// <summary>
  /// Audio is paused.
  /// </summary>
  Paused
}
