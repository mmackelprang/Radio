namespace RadioConsole.Core;

/// <summary>
/// Constants used throughout the RadioConsole application.
/// </summary>
public static class AudioConstants
{
  /// <summary>
  /// Default audio device identifier.
  /// </summary>
  public const string DefaultDeviceId = "default";

  /// <summary>
  /// Cast audio output type identifier.
  /// </summary>
  public const string CastDeviceType = "cast";

  /// <summary>
  /// Local audio output type identifier.
  /// </summary>
  public const string LocalDeviceType = "local";

  /// <summary>
  /// Default sample rate for audio (CD quality).
  /// </summary>
  public const int DefaultSampleRate = 44100;

  /// <summary>
  /// Default number of audio channels (stereo).
  /// </summary>
  public const int DefaultChannels = 2;

  /// <summary>
  /// Default audio buffer size.
  /// </summary>
  public const int DefaultBufferSize = 4096;
}
