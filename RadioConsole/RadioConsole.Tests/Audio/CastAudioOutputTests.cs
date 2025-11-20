using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace RadioConsole.Tests.Audio;

/// <summary>
/// Unit tests for CastAudioOutput.
/// Verifies that the CastAudioOutput attempts to discover Cast devices.
/// </summary>
public class CastAudioOutputTests
{
  private readonly Mock<ILogger<CastAudioOutput>> _mockLogger;
  private readonly Mock<IAudioPlayer> _mockAudioPlayer;

  public CastAudioOutputTests()
  {
    _mockLogger = new Mock<ILogger<CastAudioOutput>>();
    _mockAudioPlayer = new Mock<IAudioPlayer>();
  }

  [Fact]
  public async Task InitializeAsync_ShouldCompleteSuccessfully()
  {
    // Arrange
    var castOutput = new CastAudioOutput(_mockLogger.Object, "http://localhost:5000/stream.mp3");

    // Act
    await castOutput.InitializeAsync();

    // Assert
    Assert.False(castOutput.IsActive);
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Initializing cast audio output")),
        null,
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task DiscoverDevicesAsync_ShouldAttemptDiscovery()
  {
    // Arrange
    var castOutput = new CastAudioOutput(_mockLogger.Object, "http://localhost:5000/stream.mp3");

    // Act
    var devices = await castOutput.DiscoverDevicesAsync(0.1); // Short timeout for test

    // Assert - Should complete without throwing, even if no devices found
    Assert.NotNull(devices);
    
    // Verify that discovery was attempted (logged)
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Discovering Cast devices")),
        null,
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task StartAsync_WithNoDevices_ShouldThrowException()
  {
    // Arrange
    var castOutput = new CastAudioOutput(_mockLogger.Object, "http://localhost:5000/stream.mp3");
    castOutput.DiscoveryTimeoutSeconds = 0.1; // Speed up test
    _mockAudioPlayer.Setup(x => x.IsInitialized).Returns(true);

    // Act & Assert
    // Since we won't find any devices in the test environment, 
    // StartAsync should throw an exception
    await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await castOutput.StartAsync(_mockAudioPlayer.Object)
    );

    // Verify that device discovery was attempted
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("discovering devices")),
        null,
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task StopAsync_WhenNotActive_ShouldLogWarning()
  {
    // Arrange
    var castOutput = new CastAudioOutput(_mockLogger.Object, "http://localhost:5000/stream.mp3");

    // Act
    await castOutput.StopAsync();

    // Assert
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not active")),
        null,
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  [Fact]
  public void Name_WhenNoDeviceSelected_ShouldReturnDefaultMessage()
  {
    // Arrange
    var castOutput = new CastAudioOutput(_mockLogger.Object, "http://localhost:5000/stream.mp3");

    // Act
    var name = castOutput.Name;

    // Assert
    Assert.Contains("No device selected", name);
  }

  [Fact]
  public void IsActive_InitialState_ShouldBeFalse()
  {
    // Arrange
    var castOutput = new CastAudioOutput(_mockLogger.Object, "http://localhost:5000/stream.mp3");

    // Act
    var isActive = castOutput.IsActive;

    // Assert
    Assert.False(isActive);
  }

  [Fact]
  public async Task StartAsync_ShouldAttemptToDiscoverDevices_WithMockedPlayer()
  {
    // Arrange
    var castOutput = new CastAudioOutput(_mockLogger.Object, "http://localhost:5000/stream.mp3");
    castOutput.DiscoveryTimeoutSeconds = 0.1; // Speed up test
    _mockAudioPlayer.Setup(x => x.IsInitialized).Returns(true);
    _mockAudioPlayer.Setup(x => x.GetMixedOutputStream()).Returns(new MemoryStream());

    // Act
    try
    {
      await castOutput.StartAsync(_mockAudioPlayer.Object);
    }
    catch (InvalidOperationException)
    {
      // Expected - no devices available in test environment
    }

    // Assert - Verify that CastAudioOutput attempts to discover devices
    // This is the core requirement: "assert that CastAudioOutput attempts to discover devices"
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Discovering Cast devices")),
        null,
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once,
      "CastAudioOutput should attempt to discover devices when StartAsync is called");
  }
}
