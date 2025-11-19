using Microsoft.Extensions.Logging;
using Moq;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Inputs;
using Xunit;

namespace RadioConsole.Tests.Inputs;

/// <summary>
/// Unit tests for RaddyRadioService.
/// </summary>
public class RaddyRadioServiceTests
{
  private readonly Mock<IAudioDeviceManager> _mockAudioDeviceManager;
  private readonly Mock<IAudioPlayer> _mockAudioPlayer;
  private readonly Mock<ILogger<RaddyRadioService>> _mockLogger;
  private readonly RaddyRadioService _service;

  public RaddyRadioServiceTests()
  {
    _mockAudioDeviceManager = new Mock<IAudioDeviceManager>();
    _mockAudioPlayer = new Mock<IAudioPlayer>();
    _mockLogger = new Mock<ILogger<RaddyRadioService>>();

    _service = new RaddyRadioService(
      _mockAudioDeviceManager.Object,
      _mockAudioPlayer.Object,
      _mockLogger.Object);
  }

  [Fact]
  public async Task InitializeAsync_ShouldDetectRaddyDevice_WhenDeviceIsPresent()
  {
    // Arrange
    var devices = new List<AudioDeviceInfo>
    {
      new AudioDeviceInfo
      {
        Id = "hw:0,0",
        Name = "Raddy RF320 USB Audio",
        IsDefault = false,
        DeviceType = "USB Audio"
      }
    };

    _mockAudioDeviceManager
      .Setup(m => m.GetInputDevicesAsync())
      .ReturnsAsync(devices);

    // Act
    await _service.InitializeAsync();

    // Assert
    Assert.True(_service.IsDeviceDetected);
    Assert.Equal("hw:0,0", _service.GetDeviceId());
    Assert.False(_service.IsStreaming);
  }

  [Fact]
  public async Task InitializeAsync_ShouldNotDetectDevice_WhenDeviceIsNotPresent()
  {
    // Arrange
    var devices = new List<AudioDeviceInfo>
    {
      new AudioDeviceInfo
      {
        Id = "hw:1,0",
        Name = "Built-in Audio",
        IsDefault = true,
        DeviceType = "Internal"
      }
    };

    _mockAudioDeviceManager
      .Setup(m => m.GetInputDevicesAsync())
      .ReturnsAsync(devices);

    // Act
    await _service.InitializeAsync();

    // Assert
    Assert.False(_service.IsDeviceDetected);
    Assert.Null(_service.GetDeviceId());
  }

  [Fact]
  public async Task StartAsync_ShouldThrowException_WhenDeviceNotDetected()
  {
    // Arrange
    _mockAudioDeviceManager
      .Setup(m => m.GetInputDevicesAsync())
      .ReturnsAsync(new List<AudioDeviceInfo>());

    await _service.InitializeAsync();

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(() => _service.StartAsync());
  }

  [Fact]
  public async Task StartAsync_ShouldSetInputDevice_WhenDeviceIsDetected()
  {
    // Arrange
    var devices = new List<AudioDeviceInfo>
    {
      new AudioDeviceInfo
      {
        Id = "hw:0,0",
        Name = "Raddy RF320",
        DeviceType = "USB"
      }
    };

    _mockAudioDeviceManager
      .Setup(m => m.GetInputDevicesAsync())
      .ReturnsAsync(devices);

    _mockAudioDeviceManager
      .Setup(m => m.GetCurrentOutputDeviceAsync())
      .ReturnsAsync(new AudioDeviceInfo { Id = "hw:1,0", Name = "Output Device" });

    _mockAudioPlayer
      .Setup(m => m.IsInitialized)
      .Returns(false);

    await _service.InitializeAsync();

    // Act
    await _service.StartAsync();

    // Assert
    Assert.True(_service.IsStreaming);
    _mockAudioDeviceManager.Verify(m => m.SetInputDeviceAsync("hw:0,0"), Times.Once);
    _mockAudioPlayer.Verify(m => m.InitializeAsync(It.IsAny<string>()), Times.Once);
  }

  [Fact]
  public async Task StartAsync_ShouldNotInitializePlayer_WhenAlreadyInitialized()
  {
    // Arrange
    var devices = new List<AudioDeviceInfo>
    {
      new AudioDeviceInfo { Id = "hw:0,0", Name = "Raddy RF320", DeviceType = "USB" }
    };

    _mockAudioDeviceManager
      .Setup(m => m.GetInputDevicesAsync())
      .ReturnsAsync(devices);

    _mockAudioPlayer
      .Setup(m => m.IsInitialized)
      .Returns(true);

    await _service.InitializeAsync();

    // Act
    await _service.StartAsync();

    // Assert
    Assert.True(_service.IsStreaming);
    _mockAudioPlayer.Verify(m => m.InitializeAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task StopAsync_ShouldStopStreaming()
  {
    // Arrange
    var devices = new List<AudioDeviceInfo>
    {
      new AudioDeviceInfo { Id = "hw:0,0", Name = "Raddy RF320", DeviceType = "USB" }
    };

    _mockAudioDeviceManager
      .Setup(m => m.GetInputDevicesAsync())
      .ReturnsAsync(devices);

    _mockAudioPlayer.Setup(m => m.IsInitialized).Returns(true);

    await _service.InitializeAsync();
    await _service.StartAsync();

    // Act
    await _service.StopAsync();

    // Assert
    Assert.False(_service.IsStreaming);
    _mockAudioPlayer.Verify(m => m.StopAsync("raddy_radio"), Times.Once);
  }

  [Fact]
  public async Task GetFrequencyAsync_ShouldReturnNull_AsPlaceholder()
  {
    // Act
    var frequency = await _service.GetFrequencyAsync();

    // Assert
    Assert.Null(frequency);
  }

  [Fact]
  public async Task SetFrequencyAsync_ShouldCompleteSuccessfully_AsPlaceholder()
  {
    // Act & Assert
    await _service.SetFrequencyAsync(101.5);
    // No exception should be thrown
  }
}
