namespace RadioConsole.Core.Enums;

/// <summary>
/// Mixer channels for audio routing.
/// Each channel has different priority and ducking behavior.
/// </summary>
public enum MixerChannel
{
  /// <summary>
  /// Main audio channel for background music (Radio, Spotify, Vinyl, File Player).
  /// Lowest priority - will be ducked when higher priority channels are active.
  /// </summary>
  Main = 0,

  /// <summary>
  /// Event channel for sound effects (doorbell, notifications, alerts).
  /// High priority - will duck the Main channel.
  /// </summary>
  Event = 1,

  /// <summary>
  /// Voice channel for TTS and announcements.
  /// High priority - will duck the Main channel.
  /// </summary>
  Voice = 2
}
