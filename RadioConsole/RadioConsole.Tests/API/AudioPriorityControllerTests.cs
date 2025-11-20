using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RadioConsole.API.Controllers;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using Xunit;

namespace RadioConsole.Tests.API;

/// <summary>
/// Unit tests for AudioPriorityController.
/// </summary>
public class AudioPriorityControllerTests
{
  private readonly Mock<IAudioPriorityService> _mockPriorityService;
  private readonly Mock<ILogger<AudioPriorityController>> _mockLogger;
  private readonly AudioPriorityController _controller;

  public AudioPriorityControllerTests()
  {
    _mockPriorityService = new Mock<IAudioPriorityService>();
    _mockLogger = new Mock<ILogger<AudioPriorityController>>();
    _controller = new AudioPriorityController(_mockPriorityService.Object, _mockLogger.Object);
  }

  [Fact]
  public async Task RegisterSource_ReturnsOk_WithValidRequest()
  {
    // Arrange
    var request = new RegisterSourceRequest { SourceId = "radio", Priority = AudioPriority.Low };
    _mockPriorityService.Setup(x => x.RegisterSourceAsync(request.SourceId, request.Priority))
      .Returns(Task.CompletedTask);

    // Act
    var result = await _controller.RegisterSource(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    _mockPriorityService.Verify(x => x.RegisterSourceAsync(request.SourceId, request.Priority), Times.Once);
  }

  [Fact]
  public async Task RegisterSource_ReturnsBadRequest_WithEmptySourceId()
  {
    // Arrange
    var request = new RegisterSourceRequest { SourceId = "", Priority = AudioPriority.Low };

    // Act
    var result = await _controller.RegisterSource(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
    _mockPriorityService.Verify(x => x.RegisterSourceAsync(It.IsAny<string>(), It.IsAny<AudioPriority>()), Times.Never);
  }

  [Fact]
  public async Task RegisterSource_ReturnsInternalServerError_OnException()
  {
    // Arrange
    var request = new RegisterSourceRequest { SourceId = "test", Priority = AudioPriority.High };
    _mockPriorityService.Setup(x => x.RegisterSourceAsync(request.SourceId, request.Priority))
      .ThrowsAsync(new Exception("Test error"));

    // Act
    var result = await _controller.RegisterSource(request);

    // Assert
    var statusCodeResult = Assert.IsType<ObjectResult>(result);
    Assert.Equal(500, statusCodeResult.StatusCode);
  }

  [Fact]
  public async Task UnregisterSource_ReturnsOk_WithValidRequest()
  {
    // Arrange
    var request = new UnregisterSourceRequest { SourceId = "radio" };
    _mockPriorityService.Setup(x => x.UnregisterSourceAsync(request.SourceId))
      .Returns(Task.CompletedTask);

    // Act
    var result = await _controller.UnregisterSource(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    _mockPriorityService.Verify(x => x.UnregisterSourceAsync(request.SourceId), Times.Once);
  }

  [Fact]
  public async Task UnregisterSource_ReturnsBadRequest_WithEmptySourceId()
  {
    // Arrange
    var request = new UnregisterSourceRequest { SourceId = "" };

    // Act
    var result = await _controller.UnregisterSource(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  [Fact]
  public async Task NotifyHighPriorityStart_ReturnsOk_WithValidRequest()
  {
    // Arrange
    var request = new SourceEventRequest { SourceId = "doorbell" };
    _mockPriorityService.Setup(x => x.OnHighPriorityStartAsync(request.SourceId))
      .Returns(Task.CompletedTask);

    // Act
    var result = await _controller.NotifyHighPriorityStart(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    _mockPriorityService.Verify(x => x.OnHighPriorityStartAsync(request.SourceId), Times.Once);
  }

  [Fact]
  public async Task NotifyHighPriorityStart_ReturnsBadRequest_WithEmptySourceId()
  {
    // Arrange
    var request = new SourceEventRequest { SourceId = "" };

    // Act
    var result = await _controller.NotifyHighPriorityStart(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  [Fact]
  public async Task NotifyHighPriorityEnd_ReturnsOk_WithValidRequest()
  {
    // Arrange
    var request = new SourceEventRequest { SourceId = "doorbell" };
    _mockPriorityService.Setup(x => x.OnHighPriorityEndAsync(request.SourceId))
      .Returns(Task.CompletedTask);

    // Act
    var result = await _controller.NotifyHighPriorityEnd(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    _mockPriorityService.Verify(x => x.OnHighPriorityEndAsync(request.SourceId), Times.Once);
  }

  [Fact]
  public async Task NotifyHighPriorityEnd_ReturnsBadRequest_WithNullRequest()
  {
    // Arrange
    SourceEventRequest? request = null;

    // Act
    var result = await _controller.NotifyHighPriorityEnd(request!);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  [Fact]
  public void GetDuckPercentage_ReturnsOk_WithCurrentPercentage()
  {
    // Arrange
    _mockPriorityService.Setup(x => x.DuckPercentage).Returns(0.2f);

    // Act
    var result = _controller.GetDuckPercentage();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task SetDuckPercentage_ReturnsOk_WithValidPercentage()
  {
    // Arrange
    var request = new SetDuckPercentageRequest { DuckPercentage = 0.3f };
    _mockPriorityService.Setup(x => x.SetDuckPercentageAsync(request.DuckPercentage))
      .Returns(Task.CompletedTask);

    // Act
    var result = await _controller.SetDuckPercentage(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    _mockPriorityService.Verify(x => x.SetDuckPercentageAsync(request.DuckPercentage), Times.Once);
  }

  [Fact]
  public async Task SetDuckPercentage_ReturnsBadRequest_WithInvalidPercentage()
  {
    // Arrange
    var request = new SetDuckPercentageRequest { DuckPercentage = 1.5f };

    // Act
    var result = await _controller.SetDuckPercentage(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
    _mockPriorityService.Verify(x => x.SetDuckPercentageAsync(It.IsAny<float>()), Times.Never);
  }

  [Fact]
  public async Task SetDuckPercentage_ReturnsBadRequest_WithNegativePercentage()
  {
    // Arrange
    var request = new SetDuckPercentageRequest { DuckPercentage = -0.1f };

    // Act
    var result = await _controller.SetDuckPercentage(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  [Fact]
  public void GetStatus_ReturnsOk_WithStatusInformation()
  {
    // Arrange
    _mockPriorityService.Setup(x => x.IsHighPriorityActive).Returns(true);
    _mockPriorityService.Setup(x => x.DuckPercentage).Returns(0.2f);

    // Act
    var result = _controller.GetStatus();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public void GetStatus_ReturnsCorrectMessage_WhenHighPriorityIsActive()
  {
    // Arrange
    _mockPriorityService.Setup(x => x.IsHighPriorityActive).Returns(true);
    _mockPriorityService.Setup(x => x.DuckPercentage).Returns(0.2f);

    // Act
    var result = _controller.GetStatus();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public void GetStatus_ReturnsCorrectMessage_WhenHighPriorityIsNotActive()
  {
    // Arrange
    _mockPriorityService.Setup(x => x.IsHighPriorityActive).Returns(false);
    _mockPriorityService.Setup(x => x.DuckPercentage).Returns(0.2f);

    // Act
    var result = _controller.GetStatus();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }
}
