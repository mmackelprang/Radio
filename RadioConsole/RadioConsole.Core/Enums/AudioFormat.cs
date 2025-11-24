namespace RadioConsole.Core.Enums;

/// <summary>
/// Supported audio formats for streaming.
/// Based on the formats supported by the SoundFlow library.
/// </summary>
public enum AudioFormat
{
  /// <summary>
  /// Waveform Audio File Format (uncompressed).
  /// </summary>
  Wav,

  /// <summary>
  /// MPEG Audio Layer III (compressed).
  /// </summary>
  Mp3,

  /// <summary>
  /// Free Lossless Audio Codec.
  /// </summary>
  Flac,

  /// <summary>
  /// Advanced Audio Coding.
  /// </summary>
  Aac,

  /// <summary>
  /// Ogg Vorbis audio format.
  /// </summary>
  Ogg,

  /// <summary>
  /// Opus Interactive Audio Codec.
  /// </summary>
  Opus
}
