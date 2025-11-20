namespace RadioConsole.Core.Enums;

/// <summary>
/// Audio priority levels for ducking management.
/// Higher priority audio will duck (lower volume of) lower priority audio.
/// </summary>
public enum AudioPriority
{
  /// <summary>
  /// Low priority audio (Radio, Spotify, Vinyl).
  /// This audio will be ducked when high priority audio plays.
  /// </summary>
  Low = 0,

  /// <summary>
  /// High priority audio (TTS, Doorbell, Phone Ring, Google Broadcasts).
  /// This audio will cause low priority audio to duck.
  /// </summary>
  High = 1
}
