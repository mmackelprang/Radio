using System.Text.Json.Serialization;
using RadioConsole.Core.Enums;

namespace RadioConsole.Core.Configuration;

/// <summary>
/// Configuration for audio ducking behavior.
/// Controls how lower priority audio is reduced when higher priority audio plays.
/// </summary>
public class DuckingConfiguration
{
  /// <summary>
  /// Whether ducking is enabled globally.
  /// </summary>
  public bool Enabled { get; set; } = true;

  /// <summary>
  /// The active preset to use. When set to Custom, uses the custom settings.
  /// </summary>
  public DuckingPreset ActivePreset { get; set; } = DuckingPreset.DJMode;

  /// <summary>
  /// Per-channel-pair ducking settings.
  /// Key format: "TriggerChannel-TargetChannel" (e.g., "Voice-Main").
  /// </summary>
  public Dictionary<string, ChannelPairDuckingSettings> ChannelPairSettings { get; set; } = new();

  /// <summary>
  /// Default settings used when no channel-pair specific settings exist.
  /// </summary>
  public DuckingTimingSettings DefaultSettings { get; set; } = new();

  /// <summary>
  /// Whether to enable look-ahead buffer for anticipatory ducking.
  /// This requires additional latency but provides smoother transitions.
  /// </summary>
  public bool EnableLookAhead { get; set; } = false;

  /// <summary>
  /// Look-ahead buffer size in milliseconds.
  /// Only used when EnableLookAhead is true.
  /// </summary>
  public int LookAheadMs { get; set; } = 50;

  /// <summary>
  /// Creates a default configuration with standard channel pair settings.
  /// </summary>
  public static DuckingConfiguration CreateDefault()
  {
    var config = new DuckingConfiguration();

    // Voice over Main (TTS/announcements over music)
    config.ChannelPairSettings["Voice-Main"] = new ChannelPairDuckingSettings
    {
      TriggerChannel = MixerChannel.Voice,
      TargetChannel = MixerChannel.Main,
      Timing = new DuckingTimingSettings
      {
        AttackTimeMs = 50,
        ReleaseTimeMs = 500,
        HoldTimeMs = 100,
        DuckLevel = 0.2f // Reduce to 20%
      }
    };

    // Event over Main (alerts/notifications over music)
    config.ChannelPairSettings["Event-Main"] = new ChannelPairDuckingSettings
    {
      TriggerChannel = MixerChannel.Event,
      TargetChannel = MixerChannel.Main,
      Timing = new DuckingTimingSettings
      {
        AttackTimeMs = 10,
        ReleaseTimeMs = 300,
        HoldTimeMs = 50,
        DuckLevel = 0.1f // Reduce to 10% for alerts
      }
    };

    // Event over Voice (emergency alerts over TTS)
    config.ChannelPairSettings["Event-Voice"] = new ChannelPairDuckingSettings
    {
      TriggerChannel = MixerChannel.Event,
      TargetChannel = MixerChannel.Voice,
      Timing = new DuckingTimingSettings
      {
        AttackTimeMs = 5,
        ReleaseTimeMs = 200,
        HoldTimeMs = 0,
        DuckLevel = 0.3f // Reduce to 30%
      }
    };

    return config;
  }
}

/// <summary>
/// Ducking settings for a specific channel pair.
/// </summary>
public class ChannelPairDuckingSettings
{
  /// <summary>
  /// The channel that triggers ducking.
  /// </summary>
  public MixerChannel TriggerChannel { get; set; }

  /// <summary>
  /// The channel that gets ducked.
  /// </summary>
  public MixerChannel TargetChannel { get; set; }

  /// <summary>
  /// Timing settings for this channel pair.
  /// </summary>
  public DuckingTimingSettings Timing { get; set; } = new();

  /// <summary>
  /// Whether this channel pair is enabled.
  /// </summary>
  public bool Enabled { get; set; } = true;

  /// <summary>
  /// Priority level for cascading ducks.
  /// Higher values take precedence.
  /// </summary>
  public int Priority { get; set; } = 0;
}

/// <summary>
/// Timing settings for ducking transitions.
/// </summary>
public class DuckingTimingSettings
{
  /// <summary>
  /// How fast to duck in milliseconds. Default 50ms for voice, 200ms for music.
  /// Sub-10ms values may cause audio artifacts.
  /// </summary>
  public int AttackTimeMs { get; set; } = 50;

  /// <summary>
  /// How fast to restore volume in milliseconds. Default 500ms for voice, 2000ms for music.
  /// </summary>
  public int ReleaseTimeMs { get; set; } = 500;

  /// <summary>
  /// Minimum duck duration in milliseconds. Default 100ms.
  /// Prevents rapid on/off ducking.
  /// </summary>
  public int HoldTimeMs { get; set; } = 100;

  /// <summary>
  /// Target volume level when ducked (0.0 to 1.0). Default 0.2 (20%).
  /// 0.0 = full mute, 1.0 = no ducking.
  /// </summary>
  public float DuckLevel { get; set; } = 0.2f;

  /// <summary>
  /// Creates a copy of these settings.
  /// </summary>
  public DuckingTimingSettings Clone() => new()
  {
    AttackTimeMs = AttackTimeMs,
    ReleaseTimeMs = ReleaseTimeMs,
    HoldTimeMs = HoldTimeMs,
    DuckLevel = DuckLevel
  };
}

/// <summary>
/// Configuration for crossfade transitions.
/// </summary>
public class CrossfadeConfiguration
{
  /// <summary>
  /// Whether crossfading is enabled.
  /// </summary>
  public bool Enabled { get; set; } = true;

  /// <summary>
  /// Default crossfade duration in milliseconds for music tracks.
  /// Range: 0-10000ms (0-10 seconds).
  /// </summary>
  public int DefaultCrossfadeDurationMs { get; set; } = 3000;

  /// <summary>
  /// Fade in duration for announcements in milliseconds.
  /// </summary>
  public int AnnouncementFadeInMs { get; set; } = 100;

  /// <summary>
  /// Fade out duration for announcements in milliseconds.
  /// </summary>
  public int AnnouncementFadeOutMs { get; set; } = 200;

  /// <summary>
  /// Whether to enable gapless playback for continuous streams.
  /// </summary>
  public bool EnableGapless { get; set; } = true;

  /// <summary>
  /// Pre-buffer duration for gapless playback in milliseconds.
  /// </summary>
  public int GaplessPreBufferMs { get; set; } = 500;
}
