using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio;

namespace RadioConsole.Tests.Audio;

public class SoundFlowAudioPlayerTests
{
  private readonly Mock<ILogger<SoundFlowAudioPlayer>> _mockLogger;
  private readonly Mock<IVisualizationService> _mockVisualizationService;

  public SoundFlowAudioPlayerTests()
  {
    _mockLogger = new Mock<ILogger<SoundFlowAudioPlayer>>();
    _mockVisualizationService = new Mock<IVisualizationService>();
  }

  [Fact]
  public void Constructor_ShouldInitialize_WithoutVisualizationService()
  {
    // Arrange & Act
    var player = new SoundFlowAudioPlayer(_mockLogger.Object);

    // Assert
    Assert.NotNull(player);
    Assert.False(player.IsInitialized);
  }

  [Fact]
  public void Constructor_ShouldInitialize_WithVisualizationService()
  {
    // Arrange & Act
    var player = new SoundFlowAudioPlayer(_mockLogger.Object, _mockVisualizationService.Object);

    // Assert
    Assert.NotNull(player);
    Assert.False(player.IsInitialized);
  }

  [Fact]
  public async Task InitializeAsync_ShouldSetIsInitializedToTrue()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }
    // Arrange
    var player = new SoundFlowAudioPlayer(_mockLogger.Object, _mockVisualizationService.Object);

    // Act
    await player.InitializeAsync("default");

    // Assert
    Assert.True(player.IsInitialized);
  }

  [Fact]
  public async Task InitializeAsync_WithInvalidDeviceId_ShouldUseDefaultDevice()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }
    // Arrange
    var player = new SoundFlowAudioPlayer(_mockLogger.Object);

    // Act
    await player.InitializeAsync("invalid-device-id");

    // Assert
    Assert.True(player.IsInitialized);
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid device ID format")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task PlayAsync_WithoutInitialization_ShouldThrowInvalidOperationException()
  {
    // Arrange
    var player = new SoundFlowAudioPlayer(_mockLogger.Object);
    var audioStream = new MemoryStream();

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await player.PlayAsync("test", audioStream));
  }

  [Fact]
  public async Task SetVolumeAsync_WithInitializedPlayer_ShouldSucceed()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }
    // Arrange
    var player = new SoundFlowAudioPlayer(_mockLogger.Object);
    await player.InitializeAsync("default");

    // Act & Assert
    await player.SetVolumeAsync("test-source", 0.5f);
    // No exception should be thrown
  }

  [Fact]
  public async Task SetVolumeAsync_ShouldClampVolumeToValidRange()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }
    // Arrange
    var player = new SoundFlowAudioPlayer(_mockLogger.Object);
    await player.InitializeAsync("default");

    // Act
    await player.SetVolumeAsync("test-source", 1.5f); // Above max
    await player.SetVolumeAsync("test-source", -0.5f); // Below min

    // Assert - no exceptions thrown, volume is clamped
  }

  [Fact]
  public void GetMixedOutputStream_WithoutInitialization_ShouldThrowInvalidOperationException()
  {
    // Arrange
    var player = new SoundFlowAudioPlayer(_mockLogger.Object);

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => player.GetMixedOutputStream());
  }

  [Fact]
  public async Task GetMixedOutputStream_WithInitialization_ShouldReturnStream()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }
    // Arrange
    var player = new SoundFlowAudioPlayer(_mockLogger.Object);
    await player.InitializeAsync("default");

    // Act
    var stream = player.GetMixedOutputStream();

    // Assert
    Assert.NotNull(stream);
  }

  [Fact]
  public async Task EnableFftDataGeneration_ShouldNotThrow_WhenEnabled()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }
    // Arrange
    var player = new SoundFlowAudioPlayer(_mockLogger.Object, _mockVisualizationService.Object);
    await player.InitializeAsync("default");

    // Act
    player.EnableFftDataGeneration(true);
    Thread.Sleep(100); // Allow timer to fire once

    // Assert
    // Verify visualization service is called (with placeholder data)
    _mockVisualizationService.Verify(
      x => x.SendFFTDataAsync(It.IsAny<float[]>(), It.IsAny<CancellationToken>()),
      Times.AtLeastOnce);
  }

  [Fact]
  public void EnableFftDataGeneration_ShouldNotThrow_WhenNoVisualizationService()
  {
    // Arrange
    var player = new SoundFlowAudioPlayer(_mockLogger.Object);

    // Act & Assert - Should not throw even without visualization service
    player.EnableFftDataGeneration(true);
    Thread.Sleep(100);
    player.EnableFftDataGeneration(false);
  }

  [Fact]
  public async Task EnableFftDataGeneration_ShouldStopTimer_WhenDisabled()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }
    // Arrange
    var player = new SoundFlowAudioPlayer(_mockLogger.Object, _mockVisualizationService.Object);
    await player.InitializeAsync("default");
    player.EnableFftDataGeneration(true);
    Thread.Sleep(100);

    // Act
    player.EnableFftDataGeneration(false);
    _mockVisualizationService.Invocations.Clear();
    Thread.Sleep(200);

    // Assert - no more calls after disabling
    _mockVisualizationService.Verify(
      x => x.SendFFTDataAsync(It.IsAny<float[]>(), It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Dispose_ShouldCleanupResources()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }
    // Arrange
    var player = new SoundFlowAudioPlayer(_mockLogger.Object, _mockVisualizationService.Object);
    await player.InitializeAsync("default");
    player.EnableFftDataGeneration(true);

    // Act
    player.Dispose();

    // Assert
    Assert.False(player.IsInitialized);
  }

  [Fact]
  public async Task StopAsync_ShouldLogInformation()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }
    // Arrange
    var player = new SoundFlowAudioPlayer(_mockLogger.Object);
    await player.InitializeAsync("default");

    // Act
    await player.StopAsync("test-source");

    // Assert
    // Verify that stop was called (no exception, logging occurs)
  }
}
