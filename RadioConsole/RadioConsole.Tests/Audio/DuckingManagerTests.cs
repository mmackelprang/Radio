using RadioConsole.Core.Configuration;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace RadioConsole.Tests.Audio;

/// <summary>
/// Unit tests for DuckingManager.
/// </summary>
public class DuckingManagerTests
{
  private readonly Mock<IMixerService> _mockMixerService;
  private readonly Mock<ILogger<DuckingManager>> _mockLogger;
  private readonly DuckingManager _manager;

  public DuckingManagerTests()
  {
    _mockMixerService = new Mock<IMixerService>();
    _mockLogger = new Mock<ILogger<DuckingManager>>();

    // Setup default channel volumes
    _mockMixerService.Setup(m => m.GetChannelVolume(It.IsAny<MixerChannel>())).Returns(1.0f);
    _mockMixerService.Setup(m => m.SetChannelVolumeAsync(
      It.IsAny<MixerChannel>(),
      It.IsAny<float>(),
      It.IsAny<int>(),
      It.IsAny<CancellationToken>()
    )).Returns(Task.CompletedTask);

    _manager = new DuckingManager(_mockMixerService.Object, _mockLogger.Object);
  }

  [Fact]
  public void Constructor_ShouldInitializeWithDefaultValues()
  {
    // Assert
    Assert.False(_manager.IsInitialized);
    Assert.True(_manager.IsEnabled); // Default config has Enabled = true
    Assert.False(_manager.IsDuckingActive);
    Assert.Equal(DuckingPreset.DJMode, _manager.ActivePreset); // Default enum value
  }

  [Fact]
  public async Task InitializeAsync_ShouldSetInitializedTrue()
  {
    // Arrange
    var config = DuckingConfiguration.CreateDefault();

    // Act
    await _manager.InitializeAsync(config);

    // Assert
    Assert.True(_manager.IsInitialized);
    Assert.True(_manager.IsEnabled);
  }

  [Fact]
  public async Task InitializeAsync_WithNullConfiguration_ShouldThrowArgumentNullException()
  {
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(
      () => _manager.InitializeAsync(null!));
  }

  [Fact]
  public async Task StartDuckingAsync_WhenNotInitialized_ShouldNotDuck()
  {
    // Act
    await _manager.StartDuckingAsync(MixerChannel.Voice, "test-source");

    // Assert
    _mockMixerService.Verify(
      m => m.SetChannelVolumeAsync(It.IsAny<MixerChannel>(), It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task StartDuckingAsync_WithVoiceChannel_ShouldDuckMainChannel()
  {
    // Arrange
    var config = DuckingConfiguration.CreateDefault();
    await _manager.InitializeAsync(config);

    // Act
    await _manager.StartDuckingAsync(MixerChannel.Voice, "tts-source");

    // Assert
    _mockMixerService.Verify(
      m => m.SetChannelVolumeAsync(MixerChannel.Main, It.Is<float>(v => v < 1.0f), It.IsAny<int>(), It.IsAny<CancellationToken>()),
      Times.Once);
    Assert.True(_manager.IsDuckingActive);
  }

  [Fact]
  public async Task StartDuckingAsync_WithEventChannel_ShouldDuckMainAndVoiceChannels()
  {
    // Arrange
    var config = DuckingConfiguration.CreateDefault();
    await _manager.InitializeAsync(config);

    // Act
    await _manager.StartDuckingAsync(MixerChannel.Event, "alert-source");

    // Assert
    // Should duck both Main and Voice channels
    _mockMixerService.Verify(
      m => m.SetChannelVolumeAsync(MixerChannel.Main, It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
      Times.Once);
    _mockMixerService.Verify(
      m => m.SetChannelVolumeAsync(MixerChannel.Voice, It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task EndDuckingAsync_ShouldRestoreVolume()
  {
    // Arrange
    var config = DuckingConfiguration.CreateDefault();
    await _manager.InitializeAsync(config);
    await _manager.StartDuckingAsync(MixerChannel.Voice, "tts-source");

    _mockMixerService.Reset();
    _mockMixerService.Setup(m => m.SetChannelVolumeAsync(
      It.IsAny<MixerChannel>(),
      It.IsAny<float>(),
      It.IsAny<int>(),
      It.IsAny<CancellationToken>()
    )).Returns(Task.CompletedTask);

    // Act
    await _manager.EndDuckingAsync("tts-source");

    // Assert
    Assert.False(_manager.IsDuckingActive);
  }

  [Fact]
  public async Task EndDuckingAsync_WithMultipleSources_ShouldNotRestoreUntilAllEnd()
  {
    // Arrange
    var config = DuckingConfiguration.CreateDefault();
    await _manager.InitializeAsync(config);

    await _manager.StartDuckingAsync(MixerChannel.Voice, "tts-1");
    await _manager.StartDuckingAsync(MixerChannel.Voice, "tts-2");

    // Act - End first source
    await _manager.EndDuckingAsync("tts-1");

    // Assert - Still ducking due to second source
    Assert.True(_manager.IsDuckingActive);

    // Act - End second source
    await _manager.EndDuckingAsync("tts-2");

    // Assert - Now not ducking
    Assert.False(_manager.IsDuckingActive);
  }

  [Fact]
  public async Task ApplyEmergencyDuckAsync_ShouldDuckImmediately()
  {
    // Arrange
    var config = DuckingConfiguration.CreateDefault();
    await _manager.InitializeAsync(config);

    // Act
    await _manager.ApplyEmergencyDuckAsync(MixerChannel.Event, "emergency-source");

    // Assert
    _mockMixerService.Verify(
      m => m.SetChannelVolumeAsync(It.IsAny<MixerChannel>(), It.IsAny<float>(), 0, It.IsAny<CancellationToken>()),
      Times.AtLeastOnce);

    var metrics = _manager.GetMetrics();
    Assert.Equal(1, metrics.EmergencyDuckCount);
  }

  [Fact]
  public async Task SetPresetAsync_ShouldChangeActivePreset()
  {
    // Arrange
    var config = DuckingConfiguration.CreateDefault();
    await _manager.InitializeAsync(config);

    // Act
    await _manager.SetPresetAsync(DuckingPreset.DJMode);

    // Assert
    Assert.Equal(DuckingPreset.DJMode, _manager.ActivePreset);
  }

  [Fact]
  public async Task SetEnabledAsync_ShouldEnableOrDisableDucking()
  {
    // Arrange
    var config = DuckingConfiguration.CreateDefault();
    await _manager.InitializeAsync(config);

    // Act
    await _manager.SetEnabledAsync(false);

    // Assert
    Assert.False(_manager.IsEnabled);

    // Act
    await _manager.SetEnabledAsync(true);

    // Assert
    Assert.True(_manager.IsEnabled);
  }

  [Fact]
  public async Task GetChannelDuckingStatus_ShouldReturnCorrectStatus()
  {
    // Arrange
    var config = DuckingConfiguration.CreateDefault();
    await _manager.InitializeAsync(config);

    // Get initial status
    var initialStatus = _manager.GetChannelDuckingStatus(MixerChannel.Main);
    Assert.False(initialStatus.IsDucked);

    // Start ducking
    await _manager.StartDuckingAsync(MixerChannel.Voice, "tts-source");

    // Act
    var status = _manager.GetChannelDuckingStatus(MixerChannel.Main);

    // Assert
    Assert.True(status.IsDucked);
    Assert.Contains("tts-source", status.TriggeringSourceIds);
    Assert.Contains(MixerChannel.Voice, status.TriggeringChannels);
  }

  [Fact]
  public async Task GetChannelPairSettings_ShouldReturnConfiguredSettings()
  {
    // Arrange
    var config = DuckingConfiguration.CreateDefault();
    await _manager.InitializeAsync(config);

    // Act
    var settings = _manager.GetChannelPairSettings(MixerChannel.Voice, MixerChannel.Main);

    // Assert
    Assert.NotNull(settings);
    Assert.Equal(MixerChannel.Voice, settings.TriggerChannel);
    Assert.Equal(MixerChannel.Main, settings.TargetChannel);
  }

  [Fact]
  public async Task UpdateChannelPairSettingsAsync_ShouldUpdateSettings()
  {
    // Arrange
    var config = DuckingConfiguration.CreateDefault();
    await _manager.InitializeAsync(config);

    var newSettings = new ChannelPairDuckingSettings
    {
      TriggerChannel = MixerChannel.Voice,
      TargetChannel = MixerChannel.Main,
      Timing = new DuckingTimingSettings
      {
        AttackTimeMs = 100,
        ReleaseTimeMs = 1000,
        DuckLevel = 0.5f
      }
    };

    // Act
    await _manager.UpdateChannelPairSettingsAsync(newSettings);

    // Assert
    var updated = _manager.GetChannelPairSettings(MixerChannel.Voice, MixerChannel.Main);
    Assert.NotNull(updated);
    Assert.Equal(100, updated.Timing.AttackTimeMs);
    Assert.Equal(0.5f, updated.Timing.DuckLevel);
  }

  [Fact]
  public async Task ResetAsync_ShouldClearAllDuckingState()
  {
    // Arrange
    var config = DuckingConfiguration.CreateDefault();
    await _manager.InitializeAsync(config);
    await _manager.StartDuckingAsync(MixerChannel.Voice, "tts-source");

    // Act
    await _manager.ResetAsync();

    // Assert
    Assert.False(_manager.IsDuckingActive);
    var status = _manager.GetChannelDuckingStatus(MixerChannel.Main);
    Assert.False(status.IsDucked);
  }

  [Fact]
  public async Task GetMetrics_ShouldReturnValidMetrics()
  {
    // Arrange
    var config = DuckingConfiguration.CreateDefault();
    await _manager.InitializeAsync(config);

    await _manager.StartDuckingAsync(MixerChannel.Voice, "source-1");
    await _manager.StartDuckingAsync(MixerChannel.Voice, "source-2");
    await _manager.ApplyEmergencyDuckAsync(MixerChannel.Event, "emergency");

    // Act
    var metrics = _manager.GetMetrics();

    // Assert
    Assert.True(metrics.TotalDuckingEvents >= 2);
    Assert.Equal(1, metrics.EmergencyDuckCount);
  }

  [Fact]
  public async Task DuckingStateChanged_ShouldRaiseEvent()
  {
    // Arrange
    var config = DuckingConfiguration.CreateDefault();
    await _manager.InitializeAsync(config);

    var eventRaised = false;
    DuckingEventArgs? receivedArgs = null;

    _manager.DuckingStateChanged += (sender, args) =>
    {
      eventRaised = true;
      receivedArgs = args;
    };

    // Act
    await _manager.StartDuckingAsync(MixerChannel.Voice, "tts-source");

    // Assert
    Assert.True(eventRaised);
    Assert.NotNull(receivedArgs);
    Assert.True(receivedArgs.DuckingStarted);
    Assert.Equal("tts-source", receivedArgs.SourceId);
  }

  [Fact]
  public async Task ConfigurationChanged_ShouldRaiseEvent()
  {
    // Arrange
    var config = DuckingConfiguration.CreateDefault();
    await _manager.InitializeAsync(config);

    var eventRaised = false;
    _manager.ConfigurationChanged += (sender, args) => eventRaised = true;

    // Act
    await _manager.SetPresetAsync(DuckingPreset.EmergencyMode);

    // Assert
    Assert.True(eventRaised);
  }

  [Fact]
  public void Dispose_ShouldCleanupResources()
  {
    // Act
    _manager.Dispose();

    // Assert - No exception thrown and can access state
    Assert.False(_manager.IsInitialized);
  }

  [Fact]
  public void Dispose_WhenCalledMultipleTimes_ShouldNotThrow()
  {
    // Act & Assert
    _manager.Dispose();
    _manager.Dispose(); // Should not throw
  }
}
