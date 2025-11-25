using RadioConsole.Core.Configuration;
using RadioConsole.Core.Enums;
using Xunit;

namespace RadioConsole.Tests.Audio;

/// <summary>
/// Unit tests for DuckingConfiguration and DuckingPresets.
/// </summary>
public class DuckingConfigurationTests
{
  [Fact]
  public void CreateDefault_ShouldCreateValidConfiguration()
  {
    // Act
    var config = DuckingConfiguration.CreateDefault();

    // Assert
    Assert.True(config.Enabled);
    Assert.NotEmpty(config.ChannelPairSettings);
    Assert.Contains("Voice-Main", config.ChannelPairSettings.Keys);
    Assert.Contains("Event-Main", config.ChannelPairSettings.Keys);
    Assert.Contains("Event-Voice", config.ChannelPairSettings.Keys);
  }

  [Fact]
  public void DefaultSettings_ShouldHaveReasonableValues()
  {
    // Arrange
    var config = DuckingConfiguration.CreateDefault();

    // Assert
    Assert.InRange(config.DefaultSettings.AttackTimeMs, 1, 500);
    Assert.InRange(config.DefaultSettings.ReleaseTimeMs, 100, 5000);
    Assert.InRange(config.DefaultSettings.DuckLevel, 0.0f, 1.0f);
    Assert.InRange(config.DefaultSettings.HoldTimeMs, 0, 1000);
  }

  [Fact]
  public void VoiceMainPair_ShouldHaveVoiceOverMusicSettings()
  {
    // Arrange
    var config = DuckingConfiguration.CreateDefault();
    var pair = config.ChannelPairSettings["Voice-Main"];

    // Assert
    Assert.Equal(MixerChannel.Voice, pair.TriggerChannel);
    Assert.Equal(MixerChannel.Main, pair.TargetChannel);
    Assert.InRange(pair.Timing.AttackTimeMs, 10, 100); // Fast attack for voice
    Assert.InRange(pair.Timing.ReleaseTimeMs, 200, 1000);
  }

  [Fact]
  public void EventMainPair_ShouldHaveAlertSettings()
  {
    // Arrange
    var config = DuckingConfiguration.CreateDefault();
    var pair = config.ChannelPairSettings["Event-Main"];

    // Assert
    Assert.Equal(MixerChannel.Event, pair.TriggerChannel);
    Assert.Equal(MixerChannel.Main, pair.TargetChannel);
    Assert.True(pair.Timing.AttackTimeMs <= 50); // Very fast for alerts
    Assert.True(pair.Timing.DuckLevel <= 0.2f); // Aggressive duck
  }

  [Theory]
  [InlineData(DuckingPreset.DJMode)]
  [InlineData(DuckingPreset.BackgroundMode)]
  [InlineData(DuckingPreset.EmergencyMode)]
  [InlineData(DuckingPreset.MusicMode)]
  [InlineData(DuckingPreset.Custom)]
  public void CreatePreset_ShouldCreateValidConfiguration(DuckingPreset preset)
  {
    // Act
    var config = DuckingPresets.CreatePreset(preset);

    // Assert
    Assert.NotNull(config);
    Assert.True(config.Enabled);
    Assert.Equal(preset, config.ActivePreset);
    Assert.NotEmpty(config.ChannelPairSettings);
  }

  [Fact]
  public void DJMode_ShouldHaveAggressiveDucking()
  {
    // Act
    var config = DuckingPresets.CreateDJMode();

    // Assert
    Assert.Equal(DuckingPreset.DJMode, config.ActivePreset);

    var voiceMain = config.ChannelPairSettings["Voice-Main"];
    Assert.True(voiceMain.Timing.AttackTimeMs <= 50);
    Assert.True(voiceMain.Timing.DuckLevel <= 0.2f);
  }

  [Fact]
  public void BackgroundMode_ShouldHaveSubtleDucking()
  {
    // Act
    var config = DuckingPresets.CreateBackgroundMode();

    // Assert
    Assert.Equal(DuckingPreset.BackgroundMode, config.ActivePreset);

    var voiceMain = config.ChannelPairSettings["Voice-Main"];
    Assert.True(voiceMain.Timing.AttackTimeMs >= 100); // Slow attack
    Assert.True(voiceMain.Timing.ReleaseTimeMs >= 1000); // Slow release
    Assert.True(voiceMain.Timing.DuckLevel >= 0.4f); // Light ducking
  }

  [Fact]
  public void EmergencyMode_ShouldHaveInstantFullMute()
  {
    // Act
    var config = DuckingPresets.CreateEmergencyMode();

    // Assert
    Assert.Equal(DuckingPreset.EmergencyMode, config.ActivePreset);

    var eventMain = config.ChannelPairSettings["Event-Main"];
    Assert.Equal(1, eventMain.Timing.AttackTimeMs); // Instant
    Assert.Equal(0.0f, eventMain.Timing.DuckLevel); // Full mute
    Assert.Equal(100, eventMain.Priority); // Highest priority
  }

  [Fact]
  public void MusicMode_ShouldHaveMinimalDucking()
  {
    // Act
    var config = DuckingPresets.CreateMusicMode();

    // Assert
    Assert.Equal(DuckingPreset.MusicMode, config.ActivePreset);

    var voiceMain = config.ChannelPairSettings["Voice-Main"];
    Assert.True(voiceMain.Timing.AttackTimeMs >= 200); // Very slow
    Assert.True(voiceMain.Timing.ReleaseTimeMs >= 2000);
    Assert.True(voiceMain.Timing.DuckLevel >= 0.6f); // Very light
  }

  [Theory]
  [InlineData(DuckingPreset.DJMode)]
  [InlineData(DuckingPreset.BackgroundMode)]
  [InlineData(DuckingPreset.EmergencyMode)]
  [InlineData(DuckingPreset.MusicMode)]
  public void GetDefaultTiming_ShouldReturnValidSettings(DuckingPreset preset)
  {
    // Act
    var timing = DuckingPresets.GetDefaultTiming(preset);

    // Assert
    Assert.NotNull(timing);
    Assert.InRange(timing.AttackTimeMs, 1, 500);
    Assert.InRange(timing.ReleaseTimeMs, 50, 5000);
    Assert.InRange(timing.DuckLevel, 0.0f, 1.0f);
  }

  [Fact]
  public void DuckingTimingSettings_Clone_ShouldCreateIndependentCopy()
  {
    // Arrange
    var original = new DuckingTimingSettings
    {
      AttackTimeMs = 100,
      ReleaseTimeMs = 500,
      HoldTimeMs = 50,
      DuckLevel = 0.3f
    };

    // Act
    var clone = original.Clone();
    clone.AttackTimeMs = 200;

    // Assert
    Assert.Equal(100, original.AttackTimeMs); // Original unchanged
    Assert.Equal(200, clone.AttackTimeMs);
  }

  [Fact]
  public void ChannelPairDuckingSettings_DefaultValues()
  {
    // Arrange & Act
    var settings = new ChannelPairDuckingSettings();

    // Assert
    Assert.True(settings.Enabled);
    Assert.Equal(0, settings.Priority);
    Assert.NotNull(settings.Timing);
  }

  [Fact]
  public void CrossfadeConfiguration_DefaultValues()
  {
    // Arrange & Act
    var config = new CrossfadeConfiguration();

    // Assert
    Assert.True(config.Enabled);
    Assert.InRange(config.DefaultCrossfadeDurationMs, 0, 10000);
    Assert.InRange(config.AnnouncementFadeInMs, 0, 1000);
    Assert.InRange(config.AnnouncementFadeOutMs, 0, 1000);
    Assert.True(config.EnableGapless);
    Assert.True(config.GaplessPreBufferMs > 0);
  }
}
