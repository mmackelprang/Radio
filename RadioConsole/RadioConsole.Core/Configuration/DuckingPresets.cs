using RadioConsole.Core.Enums;

namespace RadioConsole.Core.Configuration;

/// <summary>
/// Factory for creating predefined ducking presets.
/// </summary>
public static class DuckingPresets
{
  /// <summary>
  /// Creates a DuckingConfiguration for the specified preset.
  /// </summary>
  /// <param name="preset">The preset to create.</param>
  /// <returns>A configured DuckingConfiguration.</returns>
  public static DuckingConfiguration CreatePreset(DuckingPreset preset)
  {
    return preset switch
    {
      DuckingPreset.DJMode => CreateDJMode(),
      DuckingPreset.BackgroundMode => CreateBackgroundMode(),
      DuckingPreset.EmergencyMode => CreateEmergencyMode(),
      DuckingPreset.MusicMode => CreateMusicMode(),
      DuckingPreset.Custom => CreateCustom(),
      _ => DuckingConfiguration.CreateDefault()
    };
  }

  /// <summary>
  /// Creates a custom preset with default settings but explicitly marked as Custom.
  /// </summary>
  public static DuckingConfiguration CreateCustom()
  {
    var config = DuckingConfiguration.CreateDefault();
    config.ActivePreset = DuckingPreset.Custom;
    return config;
  }

  /// <summary>
  /// DJ Mode: Aggressive ducking for clear voice.
  /// Fast attack, moderate release, deep ducking.
  /// </summary>
  public static DuckingConfiguration CreateDJMode()
  {
    var config = new DuckingConfiguration
    {
      Enabled = true,
      ActivePreset = DuckingPreset.DJMode,
      DefaultSettings = new DuckingTimingSettings
      {
        AttackTimeMs = 30,
        ReleaseTimeMs = 800,
        HoldTimeMs = 100,
        DuckLevel = 0.15f // Reduce to 15%
      }
    };

    // Voice over Main - aggressive duck for clear announcements
    config.ChannelPairSettings["Voice-Main"] = new ChannelPairDuckingSettings
    {
      TriggerChannel = MixerChannel.Voice,
      TargetChannel = MixerChannel.Main,
      Enabled = true,
      Priority = 10,
      Timing = new DuckingTimingSettings
      {
        AttackTimeMs = 30,
        ReleaseTimeMs = 800,
        HoldTimeMs = 150,
        DuckLevel = 0.15f
      }
    };

    // Event over Main - very aggressive for alerts
    config.ChannelPairSettings["Event-Main"] = new ChannelPairDuckingSettings
    {
      TriggerChannel = MixerChannel.Event,
      TargetChannel = MixerChannel.Main,
      Enabled = true,
      Priority = 20,
      Timing = new DuckingTimingSettings
      {
        AttackTimeMs = 5,
        ReleaseTimeMs = 500,
        HoldTimeMs = 50,
        DuckLevel = 0.05f // Almost mute
      }
    };

    // Event over Voice - alerts interrupt TTS
    config.ChannelPairSettings["Event-Voice"] = new ChannelPairDuckingSettings
    {
      TriggerChannel = MixerChannel.Event,
      TargetChannel = MixerChannel.Voice,
      Enabled = true,
      Priority = 30,
      Timing = new DuckingTimingSettings
      {
        AttackTimeMs = 5,
        ReleaseTimeMs = 300,
        HoldTimeMs = 0,
        DuckLevel = 0.2f
      }
    };

    return config;
  }

  /// <summary>
  /// Background Mode: Subtle ducking for ambiance.
  /// Slow attack, slow release, light ducking.
  /// </summary>
  public static DuckingConfiguration CreateBackgroundMode()
  {
    var config = new DuckingConfiguration
    {
      Enabled = true,
      ActivePreset = DuckingPreset.BackgroundMode,
      DefaultSettings = new DuckingTimingSettings
      {
        AttackTimeMs = 200,
        ReleaseTimeMs = 2000,
        HoldTimeMs = 200,
        DuckLevel = 0.6f // Only reduce to 60%
      }
    };

    // Voice over Main - gentle duck
    config.ChannelPairSettings["Voice-Main"] = new ChannelPairDuckingSettings
    {
      TriggerChannel = MixerChannel.Voice,
      TargetChannel = MixerChannel.Main,
      Enabled = true,
      Priority = 10,
      Timing = new DuckingTimingSettings
      {
        AttackTimeMs = 200,
        ReleaseTimeMs = 2000,
        HoldTimeMs = 200,
        DuckLevel = 0.6f
      }
    };

    // Event over Main - slightly more aggressive for notifications
    config.ChannelPairSettings["Event-Main"] = new ChannelPairDuckingSettings
    {
      TriggerChannel = MixerChannel.Event,
      TargetChannel = MixerChannel.Main,
      Enabled = true,
      Priority = 20,
      Timing = new DuckingTimingSettings
      {
        AttackTimeMs = 100,
        ReleaseTimeMs = 1500,
        HoldTimeMs = 100,
        DuckLevel = 0.4f
      }
    };

    // Event over Voice - minimal ducking
    config.ChannelPairSettings["Event-Voice"] = new ChannelPairDuckingSettings
    {
      TriggerChannel = MixerChannel.Event,
      TargetChannel = MixerChannel.Voice,
      Enabled = true,
      Priority = 30,
      Timing = new DuckingTimingSettings
      {
        AttackTimeMs = 50,
        ReleaseTimeMs = 500,
        HoldTimeMs = 50,
        DuckLevel = 0.5f
      }
    };

    return config;
  }

  /// <summary>
  /// Emergency Mode: Mute all except emergency/alert sounds.
  /// Instant attack, hold until event ends, full mute.
  /// </summary>
  public static DuckingConfiguration CreateEmergencyMode()
  {
    var config = new DuckingConfiguration
    {
      Enabled = true,
      ActivePreset = DuckingPreset.EmergencyMode,
      DefaultSettings = new DuckingTimingSettings
      {
        AttackTimeMs = 1,
        ReleaseTimeMs = 100,
        HoldTimeMs = 0,
        DuckLevel = 0.0f // Full mute
      }
    };

    // Voice over Main - full mute
    config.ChannelPairSettings["Voice-Main"] = new ChannelPairDuckingSettings
    {
      TriggerChannel = MixerChannel.Voice,
      TargetChannel = MixerChannel.Main,
      Enabled = true,
      Priority = 10,
      Timing = new DuckingTimingSettings
      {
        AttackTimeMs = 1,
        ReleaseTimeMs = 100,
        HoldTimeMs = 0,
        DuckLevel = 0.0f
      }
    };

    // Event over Main - instant full mute
    config.ChannelPairSettings["Event-Main"] = new ChannelPairDuckingSettings
    {
      TriggerChannel = MixerChannel.Event,
      TargetChannel = MixerChannel.Main,
      Enabled = true,
      Priority = 100, // Highest priority
      Timing = new DuckingTimingSettings
      {
        AttackTimeMs = 1,
        ReleaseTimeMs = 50,
        HoldTimeMs = 0,
        DuckLevel = 0.0f
      }
    };

    // Event over Voice - instant mute
    config.ChannelPairSettings["Event-Voice"] = new ChannelPairDuckingSettings
    {
      TriggerChannel = MixerChannel.Event,
      TargetChannel = MixerChannel.Voice,
      Enabled = true,
      Priority = 100,
      Timing = new DuckingTimingSettings
      {
        AttackTimeMs = 1,
        ReleaseTimeMs = 50,
        HoldTimeMs = 0,
        DuckLevel = 0.0f
      }
    };

    return config;
  }

  /// <summary>
  /// Music Mode: Minimal ducking for music focus.
  /// Very slow attack, very slow release, light ducking.
  /// </summary>
  public static DuckingConfiguration CreateMusicMode()
  {
    var config = new DuckingConfiguration
    {
      Enabled = true,
      ActivePreset = DuckingPreset.MusicMode,
      DefaultSettings = new DuckingTimingSettings
      {
        AttackTimeMs = 300,
        ReleaseTimeMs = 3000,
        HoldTimeMs = 300,
        DuckLevel = 0.7f // Only reduce to 70%
      }
    };

    // Voice over Main - very subtle duck
    config.ChannelPairSettings["Voice-Main"] = new ChannelPairDuckingSettings
    {
      TriggerChannel = MixerChannel.Voice,
      TargetChannel = MixerChannel.Main,
      Enabled = true,
      Priority = 5,
      Timing = new DuckingTimingSettings
      {
        AttackTimeMs = 300,
        ReleaseTimeMs = 3000,
        HoldTimeMs = 300,
        DuckLevel = 0.7f
      }
    };

    // Event over Main - slightly more noticeable for important alerts
    config.ChannelPairSettings["Event-Main"] = new ChannelPairDuckingSettings
    {
      TriggerChannel = MixerChannel.Event,
      TargetChannel = MixerChannel.Main,
      Enabled = true,
      Priority = 15,
      Timing = new DuckingTimingSettings
      {
        AttackTimeMs = 150,
        ReleaseTimeMs = 2000,
        HoldTimeMs = 150,
        DuckLevel = 0.5f
      }
    };

    // Event over Voice - light ducking
    config.ChannelPairSettings["Event-Voice"] = new ChannelPairDuckingSettings
    {
      TriggerChannel = MixerChannel.Event,
      TargetChannel = MixerChannel.Voice,
      Enabled = true,
      Priority = 20,
      Timing = new DuckingTimingSettings
      {
        AttackTimeMs = 100,
        ReleaseTimeMs = 1000,
        HoldTimeMs = 100,
        DuckLevel = 0.6f
      }
    };

    return config;
  }

  /// <summary>
  /// Gets the default timing settings for a given preset.
  /// </summary>
  /// <param name="preset">The preset.</param>
  /// <returns>Default timing settings.</returns>
  public static DuckingTimingSettings GetDefaultTiming(DuckingPreset preset)
  {
    return preset switch
    {
      DuckingPreset.DJMode => new DuckingTimingSettings
      {
        AttackTimeMs = 30,
        ReleaseTimeMs = 800,
        HoldTimeMs = 100,
        DuckLevel = 0.15f
      },
      DuckingPreset.BackgroundMode => new DuckingTimingSettings
      {
        AttackTimeMs = 200,
        ReleaseTimeMs = 2000,
        HoldTimeMs = 200,
        DuckLevel = 0.6f
      },
      DuckingPreset.EmergencyMode => new DuckingTimingSettings
      {
        AttackTimeMs = 1,
        ReleaseTimeMs = 100,
        HoldTimeMs = 0,
        DuckLevel = 0.0f
      },
      DuckingPreset.MusicMode => new DuckingTimingSettings
      {
        AttackTimeMs = 300,
        ReleaseTimeMs = 3000,
        HoldTimeMs = 300,
        DuckLevel = 0.7f
      },
      _ => new DuckingTimingSettings()
    };
  }
}
