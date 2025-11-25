namespace RadioConsole.Core.Configuration;

/// <summary>
/// Configuration options for SoundFlow audio library.
/// Controls buffer size, sample rate, and device settings.
/// </summary>
public class SoundFlowOptions
{
  /// <summary>
  /// Audio sample rate in Hz. Default is 48000 (48kHz) for low latency.
  /// </summary>
  public int SampleRate { get; set; } = 48000;

  /// <summary>
  /// Audio bit depth. Default is 16-bit.
  /// </summary>
  public int BitDepth { get; set; } = 16;

  /// <summary>
  /// Number of audio channels. Default is 2 (stereo).
  /// </summary>
  public int Channels { get; set; } = 2;

  /// <summary>
  /// Audio buffer size in samples. Smaller values reduce latency but increase CPU usage.
  /// Default is 2048 samples for a good balance.
  /// </summary>
  public int BufferSize { get; set; } = 2048;

  /// <summary>
  /// Enable exclusive mode for lower latency audio.
  /// When true, the application takes exclusive control of the audio device.
  /// </summary>
  public bool ExclusiveMode { get; set; } = true;

  /// <summary>
  /// Preferred audio backend. Default is "alsa" for Linux.
  /// Options: "alsa", "pulseaudio", "pipewire", "wasapi", "coreaudio"
  /// </summary>
  public string PreferredBackend { get; set; } = "alsa";

  /// <summary>
  /// Enable hot-plug detection for USB audio devices.
  /// When true, the system will detect when devices are connected or disconnected.
  /// </summary>
  public bool EnableHotPlug { get; set; } = true;

  /// <summary>
  /// Polling interval in milliseconds for device change detection.
  /// Only used when EnableHotPlug is true.
  /// </summary>
  public int HotPlugPollingIntervalMs { get; set; } = 2000;

  /// <summary>
  /// Preferred USB audio device name pattern for automatic selection.
  /// Used to identify specific devices like "Raddy" or "SH5".
  /// </summary>
  public string? PreferredUsbDevicePattern { get; set; } = "Raddy";

  /// <summary>
  /// Latency hint in milliseconds. Used to configure audio buffer timing.
  /// Lower values provide less latency but require more CPU.
  /// </summary>
  public int LatencyHintMs { get; set; } = 20;
}
