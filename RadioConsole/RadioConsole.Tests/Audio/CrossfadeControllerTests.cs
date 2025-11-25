using RadioConsole.Core.Configuration;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace RadioConsole.Tests.Audio;

/// <summary>
/// Unit tests for CrossfadeController.
/// </summary>
public class CrossfadeControllerTests
{
  private readonly Mock<IMixerService> _mockMixerService;
  private readonly Mock<ILogger<CrossfadeController>> _mockLogger;
  private readonly CrossfadeController _controller;

  public CrossfadeControllerTests()
  {
    _mockMixerService = new Mock<IMixerService>();
    _mockLogger = new Mock<ILogger<CrossfadeController>>();

    // Setup default behaviors
    _mockMixerService.Setup(m => m.GetSourceVolume(It.IsAny<string>())).Returns(1.0f);
    _mockMixerService.Setup(m => m.SetSourceVolumeAsync(
      It.IsAny<string>(),
      It.IsAny<float>(),
      It.IsAny<int>(),
      It.IsAny<CancellationToken>()
    )).Returns(Task.CompletedTask);

    _controller = new CrossfadeController(_mockMixerService.Object, _mockLogger.Object);
  }

  [Fact]
  public void Constructor_ShouldInitializeWithDefaultValues()
  {
    // Assert
    Assert.False(_controller.IsInitialized);
    Assert.False(_controller.IsTransitionInProgress);
    Assert.Equal(0, _controller.CurrentProgress);
  }

  [Fact]
  public async Task InitializeAsync_ShouldSetInitializedTrue()
  {
    // Arrange
    var config = new CrossfadeConfiguration();

    // Act
    await _controller.InitializeAsync(config);

    // Assert
    Assert.True(_controller.IsInitialized);
  }

  [Fact]
  public async Task InitializeAsync_WithNullConfiguration_ShouldThrowArgumentNullException()
  {
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(
      () => _controller.InitializeAsync(null!));
  }

  [Fact]
  public async Task CrossfadeAsync_WhenNotInitialized_ShouldThrowInvalidOperationException()
  {
    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
      () => _controller.CrossfadeAsync("source1", "source2", 1000));
  }

  [Fact]
  public async Task CrossfadeAsync_ShouldAdjustSourceVolumes()
  {
    // Arrange
    var config = new CrossfadeConfiguration();
    await _controller.InitializeAsync(config);

    // Act
    await _controller.CrossfadeAsync("outgoing", "incoming", 100);

    // Assert
    _mockMixerService.Verify(
      m => m.SetSourceVolumeAsync("outgoing", It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
      Times.AtLeastOnce);
    _mockMixerService.Verify(
      m => m.SetSourceVolumeAsync("incoming", It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
      Times.AtLeastOnce);
  }

  [Fact]
  public async Task CrossfadeAsync_ShouldSetFinalVolumes()
  {
    // Arrange
    var config = new CrossfadeConfiguration();
    await _controller.InitializeAsync(config);

    // Act
    await _controller.CrossfadeAsync("outgoing", "incoming", 50);

    // Assert - Final volumes should be set at least once to 0 and 1
    _mockMixerService.Verify(
      m => m.SetSourceVolumeAsync("outgoing", 0, 0, It.IsAny<CancellationToken>()),
      Times.AtLeastOnce);
    _mockMixerService.Verify(
      m => m.SetSourceVolumeAsync("incoming", 1.0f, 0, It.IsAny<CancellationToken>()),
      Times.AtLeastOnce);
  }

  [Fact]
  public async Task FadeInAsync_ShouldFadeFromZeroToOne()
  {
    // Arrange
    var config = new CrossfadeConfiguration();
    await _controller.InitializeAsync(config);

    // Act
    await _controller.FadeInAsync("source", 50);

    // Assert
    _mockMixerService.Verify(
      m => m.SetSourceVolumeAsync("source", It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
      Times.AtLeastOnce);
  }

  [Fact]
  public async Task FadeOutAsync_ShouldFadeToZero()
  {
    // Arrange
    var config = new CrossfadeConfiguration();
    await _controller.InitializeAsync(config);

    // Act
    await _controller.FadeOutAsync("source", 50);

    // Assert
    _mockMixerService.Verify(
      m => m.SetSourceVolumeAsync("source", It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
      Times.AtLeastOnce);
  }

  [Fact]
  public async Task EmergencyCutAsync_ShouldSetVolumesImmediately()
  {
    // Arrange
    var config = new CrossfadeConfiguration();
    await _controller.InitializeAsync(config);

    // Act
    await _controller.EmergencyCutAsync("outgoing", "incoming");

    // Assert
    _mockMixerService.Verify(
      m => m.SetSourceVolumeAsync("outgoing", 0, 0, It.IsAny<CancellationToken>()),
      Times.Once);
    _mockMixerService.Verify(
      m => m.SetSourceVolumeAsync("incoming", 1.0f, 0, It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task CancelTransitionAsync_ShouldCancelActiveTransition()
  {
    // Arrange
    var config = new CrossfadeConfiguration();
    await _controller.InitializeAsync(config);

    // Start a long transition
    var crossfadeTask = _controller.CrossfadeAsync("outgoing", "incoming", 5000);

    // Act
    await _controller.CancelTransitionAsync();

    // Wait for the transition to be cancelled
    try
    {
      await crossfadeTask;
    }
    catch (OperationCanceledException)
    {
      // Expected
    }

    // Assert
    Assert.False(_controller.IsTransitionInProgress);
    Assert.Equal(0, _controller.CurrentProgress);
  }

  [Fact]
  public async Task TransitionStarted_ShouldRaiseEvent()
  {
    // Arrange
    var config = new CrossfadeConfiguration();
    await _controller.InitializeAsync(config);

    var eventRaised = false;
    CrossfadeEventArgs? receivedArgs = null;

    _controller.TransitionStarted += (sender, args) =>
    {
      eventRaised = true;
      receivedArgs = args;
    };

    // Act
    await _controller.CrossfadeAsync("outgoing", "incoming", 50);

    // Assert
    Assert.True(eventRaised);
    Assert.NotNull(receivedArgs);
    Assert.Equal("outgoing", receivedArgs.OutgoingSourceId);
    Assert.Equal("incoming", receivedArgs.IncomingSourceId);
    Assert.Equal(TransitionType.Crossfade, receivedArgs.TransitionType);
  }

  [Fact]
  public async Task TransitionCompleted_ShouldRaiseEvent()
  {
    // Arrange
    var config = new CrossfadeConfiguration();
    await _controller.InitializeAsync(config);

    var eventRaised = false;
    _controller.TransitionCompleted += (sender, args) => eventRaised = true;

    // Act
    await _controller.CrossfadeAsync("outgoing", "incoming", 50);

    // Assert
    Assert.True(eventRaised);
  }

  [Fact]
  public async Task TransitionProgress_ShouldRaiseEventsWithProgressInfo()
  {
    // Arrange
    var config = new CrossfadeConfiguration();
    await _controller.InitializeAsync(config);

    var progressEvents = new List<float>();
    _controller.TransitionProgress += (sender, args) => progressEvents.Add(args.Progress);

    // Act
    await _controller.CrossfadeAsync("outgoing", "incoming", 100);

    // Assert
    Assert.NotEmpty(progressEvents);
    Assert.Contains(progressEvents, p => p > 0 && p < 1); // Should have intermediate progress
  }

  [Fact]
  public async Task UpdateConfigurationAsync_ShouldUpdateSettings()
  {
    // Arrange
    var config = new CrossfadeConfiguration { DefaultCrossfadeDurationMs = 1000 };
    await _controller.InitializeAsync(config);

    var newConfig = new CrossfadeConfiguration { DefaultCrossfadeDurationMs = 5000 };

    // Act
    await _controller.UpdateConfigurationAsync(newConfig);

    // Assert
    var current = _controller.GetConfiguration();
    Assert.Equal(5000, current.DefaultCrossfadeDurationMs);
  }

  [Fact]
  public async Task GetConfiguration_ShouldReturnCurrentConfig()
  {
    // Arrange
    var config = new CrossfadeConfiguration
    {
      DefaultCrossfadeDurationMs = 2500,
      EnableGapless = true
    };
    await _controller.InitializeAsync(config);

    // Act
    var result = _controller.GetConfiguration();

    // Assert
    Assert.Equal(2500, result.DefaultCrossfadeDurationMs);
    Assert.True(result.EnableGapless);
  }

  [Fact]
  public void Dispose_ShouldCleanupResources()
  {
    // Act
    _controller.Dispose();

    // Assert
    Assert.False(_controller.IsInitialized);
  }

  [Fact]
  public void Dispose_WhenCalledMultipleTimes_ShouldNotThrow()
  {
    // Act & Assert
    _controller.Dispose();
    _controller.Dispose(); // Should not throw
  }
}
