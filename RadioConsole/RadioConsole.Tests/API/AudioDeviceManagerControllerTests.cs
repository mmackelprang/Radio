using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RadioConsole.API.Controllers;
using RadioConsole.Core.Interfaces.Audio;
using Xunit;

namespace RadioConsole.Tests.API;

/// <summary>
/// Unit tests for AudioDeviceManagerController.
/// </summary>
public class AudioDeviceManagerControllerTests
{
  private readonly Mock<IAudioDeviceManager> _mockDeviceManager;
  private readonly Mock<ILogger<AudioDeviceManagerController>> _mockLogger;
  private readonly AudioDeviceManagerController _controller;

  public AudioDeviceManagerControllerTests()
  {
    _mockDeviceManager = new Mock<IAudioDeviceManager>();
    _mockLogger = new Mock<ILogger<AudioDeviceManagerController>>();
    _controller = new AudioDeviceManagerController(_mockDeviceManager.Object, _mockLogger.Object);
  }

  [Fact]
  public async Task GetInputDevices_ReturnsOkWithDevices()
  {
    // Arrange
    var devices = new List<AudioDeviceInfo>
    {
      new AudioDeviceInfo { Id = "1", Name = "Device 1", IsDefault = true, DeviceType = "USB Audio" },
      new AudioDeviceInfo { Id = "2", Name = "Device 2", IsDefault = false, DeviceType = "HDMI" }
    };
    _mockDeviceManager.Setup(x => x.GetInputDevicesAsync()).ReturnsAsync(devices);

    // Act
    var result = await _controller.GetInputDevices();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var returnedDevices = Assert.IsAssignableFrom<IEnumerable<AudioDeviceInfo>>(okResult.Value);
    Assert.Equal(2, returnedDevices.Count());
  }

  [Fact]
  public async Task GetInputDevices_ReturnsInternalServerError_OnException()
  {
    // Arrange
    _mockDeviceManager.Setup(x => x.GetInputDevicesAsync()).ThrowsAsync(new Exception("Test error"));

    // Act
    var result = await _controller.GetInputDevices();

    // Assert
    var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
    Assert.Equal(500, statusCodeResult.StatusCode);
  }

  [Fact]
  public async Task GetOutputDevices_ReturnsOkWithDevices()
  {
    // Arrange
    var devices = new List<AudioDeviceInfo>
    {
      new AudioDeviceInfo { Id = "1", Name = "Output 1", IsDefault = false, DeviceType = "Analog" }
    };
    _mockDeviceManager.Setup(x => x.GetOutputDevicesAsync()).ReturnsAsync(devices);

    // Act
    var result = await _controller.GetOutputDevices();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var returnedDevices = Assert.IsAssignableFrom<IEnumerable<AudioDeviceInfo>>(okResult.Value);
    Assert.Single(returnedDevices);
  }

  [Fact]
  public async Task GetCurrentInputDevice_ReturnsOk_WhenDeviceExists()
  {
    // Arrange
    var device = new AudioDeviceInfo { Id = "1", Name = "Current Input", IsDefault = true, DeviceType = "USB Audio" };
    _mockDeviceManager.Setup(x => x.GetCurrentInputDeviceAsync()).ReturnsAsync(device);

    // Act
    var result = await _controller.GetCurrentInputDevice();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var returnedDevice = Assert.IsType<AudioDeviceInfo>(okResult.Value);
    Assert.Equal("1", returnedDevice.Id);
  }

  [Fact]
  public async Task GetCurrentInputDevice_ReturnsNotFound_WhenNoDeviceSelected()
  {
    // Arrange
    _mockDeviceManager.Setup(x => x.GetCurrentInputDeviceAsync()).ReturnsAsync((AudioDeviceInfo?)null);

    // Act
    var result = await _controller.GetCurrentInputDevice();

    // Assert
    Assert.IsType<NotFoundObjectResult>(result.Result);
  }

  [Fact]
  public async Task GetCurrentOutputDevice_ReturnsOk_WhenDeviceExists()
  {
    // Arrange
    var device = new AudioDeviceInfo { Id = "2", Name = "Current Output", IsDefault = false, DeviceType = "HDMI" };
    _mockDeviceManager.Setup(x => x.GetCurrentOutputDeviceAsync()).ReturnsAsync(device);

    // Act
    var result = await _controller.GetCurrentOutputDevice();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var returnedDevice = Assert.IsType<AudioDeviceInfo>(okResult.Value);
    Assert.Equal("2", returnedDevice.Id);
  }

  [Fact]
  public async Task GetCurrentOutputDevice_ReturnsNotFound_WhenNoDeviceSelected()
  {
    // Arrange
    _mockDeviceManager.Setup(x => x.GetCurrentOutputDeviceAsync()).ReturnsAsync((AudioDeviceInfo?)null);

    // Act
    var result = await _controller.GetCurrentOutputDevice();

    // Assert
    Assert.IsType<NotFoundObjectResult>(result.Result);
  }

  [Fact]
  public async Task SetInputDevice_ReturnsOk_WithValidDeviceId()
  {
    // Arrange
    var request = new SetDeviceRequest { DeviceId = "device-1" };
    _mockDeviceManager.Setup(x => x.SetInputDeviceAsync(request.DeviceId)).Returns(Task.CompletedTask);

    // Act
    var result = await _controller.SetInputDevice(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    _mockDeviceManager.Verify(x => x.SetInputDeviceAsync(request.DeviceId), Times.Once);
  }

  [Fact]
  public async Task SetInputDevice_ReturnsBadRequest_WithEmptyDeviceId()
  {
    // Arrange
    var request = new SetDeviceRequest { DeviceId = "" };

    // Act
    var result = await _controller.SetInputDevice(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
    _mockDeviceManager.Verify(x => x.SetInputDeviceAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task SetInputDevice_ReturnsInternalServerError_OnException()
  {
    // Arrange
    var request = new SetDeviceRequest { DeviceId = "device-1" };
    _mockDeviceManager.Setup(x => x.SetInputDeviceAsync(request.DeviceId)).ThrowsAsync(new Exception("Test error"));

    // Act
    var result = await _controller.SetInputDevice(request);

    // Assert
    var statusCodeResult = Assert.IsType<ObjectResult>(result);
    Assert.Equal(500, statusCodeResult.StatusCode);
  }

  [Fact]
  public async Task SetOutputDevice_ReturnsOk_WithValidDeviceId()
  {
    // Arrange
    var request = new SetDeviceRequest { DeviceId = "device-2" };
    _mockDeviceManager.Setup(x => x.SetOutputDeviceAsync(request.DeviceId)).Returns(Task.CompletedTask);

    // Act
    var result = await _controller.SetOutputDevice(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    _mockDeviceManager.Verify(x => x.SetOutputDeviceAsync(request.DeviceId), Times.Once);
  }

  [Fact]
  public async Task SetOutputDevice_ReturnsBadRequest_WithNullRequest()
  {
    // Arrange
    SetDeviceRequest? request = null;

    // Act
    var result = await _controller.SetOutputDevice(request!);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  [Fact]
  public async Task SetOutputDevice_ReturnsInternalServerError_OnException()
  {
    // Arrange
    var request = new SetDeviceRequest { DeviceId = "device-2" };
    _mockDeviceManager.Setup(x => x.SetOutputDeviceAsync(request.DeviceId)).ThrowsAsync(new Exception("Test error"));

    // Act
    var result = await _controller.SetOutputDevice(request);

    // Assert
    var statusCodeResult = Assert.IsType<ObjectResult>(result);
    Assert.Equal(500, statusCodeResult.StatusCode);
  }
}
