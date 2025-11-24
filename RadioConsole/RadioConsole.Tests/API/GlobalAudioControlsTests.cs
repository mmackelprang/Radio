using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RadioConsole.API.Controllers;
using RadioConsole.Core.Interfaces.Audio;
using Xunit;

namespace RadioConsole.Tests.API;

/// <summary>
/// Unit tests for global audio controls in AudioDeviceManagerController.
/// </summary>
public class GlobalAudioControlsTests
{
  private readonly Mock<IAudioDeviceManager> _mockDeviceManager;
  private readonly Mock<ILogger<AudioDeviceManagerController>> _mockLogger;
  private readonly AudioDeviceManagerController _controller;

  public GlobalAudioControlsTests()
  {
    _mockDeviceManager = new Mock<IAudioDeviceManager>();
    _mockLogger = new Mock<ILogger<AudioDeviceManagerController>>();
    _controller = new AudioDeviceManagerController(_mockDeviceManager.Object, _mockLogger.Object);
  }

  // Volume Tests

  [Fact]
  public async Task GetGlobalVolume_ReturnsOk_WithVolume()
  {
    // Arrange
    _mockDeviceManager.Setup(x => x.GetGlobalVolumeAsync()).ReturnsAsync(0.75f);

    // Act
    var result = await _controller.GetGlobalVolume();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var response = Assert.IsType<GlobalVolumeResponse>(okResult.Value);
    Assert.Equal(0.75f, response.Volume);
  }

  [Fact]
  public async Task SetGlobalVolume_ReturnsOk_WithValidVolume()
  {
    // Arrange
    var request = new SetGlobalVolumeRequest { Volume = 0.5f };
    _mockDeviceManager.Setup(x => x.SetGlobalVolumeAsync(0.5f)).Returns(Task.CompletedTask);

    // Act
    var result = await _controller.SetGlobalVolume(request);

    // Assert
    Assert.IsType<OkObjectResult>(result);
    _mockDeviceManager.Verify(x => x.SetGlobalVolumeAsync(0.5f), Times.Once);
  }

  [Fact]
  public async Task SetGlobalVolume_ReturnsBadRequest_WithNullRequest()
  {
    // Act
    var result = await _controller.SetGlobalVolume(null!);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  // Balance Tests

  [Fact]
  public async Task GetGlobalBalance_ReturnsOk_WithBalance()
  {
    // Arrange
    _mockDeviceManager.Setup(x => x.GetGlobalBalanceAsync()).ReturnsAsync(-0.5f);

    // Act
    var result = await _controller.GetGlobalBalance();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var response = Assert.IsType<GlobalBalanceResponse>(okResult.Value);
    Assert.Equal(-0.5f, response.Balance);
  }

  [Fact]
  public async Task SetGlobalBalance_ReturnsOk_WithValidBalance()
  {
    // Arrange
    var request = new SetGlobalBalanceRequest { Balance = 0.25f };
    _mockDeviceManager.Setup(x => x.SetGlobalBalanceAsync(0.25f)).Returns(Task.CompletedTask);

    // Act
    var result = await _controller.SetGlobalBalance(request);

    // Assert
    Assert.IsType<OkObjectResult>(result);
    _mockDeviceManager.Verify(x => x.SetGlobalBalanceAsync(0.25f), Times.Once);
  }

  [Fact]
  public async Task SetGlobalBalance_ReturnsBadRequest_WithNullRequest()
  {
    // Act
    var result = await _controller.SetGlobalBalance(null!);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  // Equalization Tests

  [Fact]
  public async Task GetEqualization_ReturnsOk_WithSettings()
  {
    // Arrange
    var eqSettings = new EqualizationSettings { Bass = 3.0f, Midrange = -1.0f, Treble = 2.0f, Enabled = true };
    _mockDeviceManager.Setup(x => x.GetEqualizationAsync()).ReturnsAsync(eqSettings);

    // Act
    var result = await _controller.GetEqualization();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var response = Assert.IsType<EqualizationSettings>(okResult.Value);
    Assert.Equal(3.0f, response.Bass);
    Assert.Equal(-1.0f, response.Midrange);
    Assert.Equal(2.0f, response.Treble);
    Assert.True(response.Enabled);
  }

  [Fact]
  public async Task SetEqualization_ReturnsOk_WithValidSettings()
  {
    // Arrange
    var settings = new EqualizationSettings { Bass = 5.0f, Midrange = 0.0f, Treble = -3.0f, Enabled = true };
    _mockDeviceManager.Setup(x => x.SetEqualizationAsync(It.IsAny<EqualizationSettings>())).Returns(Task.CompletedTask);

    // Act
    var result = await _controller.SetEqualization(settings);

    // Assert
    Assert.IsType<OkObjectResult>(result);
    _mockDeviceManager.Verify(x => x.SetEqualizationAsync(It.IsAny<EqualizationSettings>()), Times.Once);
  }

  [Fact]
  public async Task SetEqualization_ReturnsBadRequest_WithNullSettings()
  {
    // Act
    var result = await _controller.SetEqualization(null!);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  // Playback State Tests

  [Fact]
  public async Task GetPlaybackState_ReturnsOk_WithState()
  {
    // Arrange
    _mockDeviceManager.Setup(x => x.GetPlaybackStateAsync()).ReturnsAsync(PlaybackState.Playing);

    // Act
    var result = await _controller.GetPlaybackState();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var response = Assert.IsType<PlaybackStateResponse>(okResult.Value);
    Assert.Equal("Playing", response.State);
  }

  [Fact]
  public async Task Pause_ReturnsOk()
  {
    // Arrange
    _mockDeviceManager.Setup(x => x.PauseAsync()).Returns(Task.CompletedTask);

    // Act
    var result = await _controller.Pause();

    // Assert
    Assert.IsType<OkObjectResult>(result);
    _mockDeviceManager.Verify(x => x.PauseAsync(), Times.Once);
  }

  [Fact]
  public async Task Play_ReturnsOk()
  {
    // Arrange
    _mockDeviceManager.Setup(x => x.PlayAsync()).Returns(Task.CompletedTask);

    // Act
    var result = await _controller.Play();

    // Assert
    Assert.IsType<OkObjectResult>(result);
    _mockDeviceManager.Verify(x => x.PlayAsync(), Times.Once);
  }

  [Fact]
  public async Task Stop_ReturnsOk()
  {
    // Arrange
    _mockDeviceManager.Setup(x => x.StopAsync()).Returns(Task.CompletedTask);

    // Act
    var result = await _controller.Stop();

    // Assert
    Assert.IsType<OkObjectResult>(result);
    _mockDeviceManager.Verify(x => x.StopAsync(), Times.Once);
  }
}
