using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RadioConsole.API.Controllers;
using RadioConsole.Core.Interfaces.Audio;
using Xunit;

namespace RadioConsole.Tests.API;

/// <summary>
/// Unit tests for AudioSourceController.
/// </summary>
public class AudioSourceControllerTests
{
  private readonly Mock<IAudioSourceManager> _mockSourceManager;
  private readonly Mock<ILogger<AudioSourceController>> _mockLogger;
  private readonly AudioSourceController _controller;

  public AudioSourceControllerTests()
  {
    _mockSourceManager = new Mock<IAudioSourceManager>();
    _mockLogger = new Mock<ILogger<AudioSourceController>>();
    _controller = new AudioSourceController(_mockSourceManager.Object, _mockLogger.Object);
  }

  // Standard Audio Source Tests

  [Fact]
  public async Task CreateSpotifySource_ReturnsOk_WithSourceId()
  {
    // Arrange
    var sourceId = "spotify-0001-abc";
    var sourceInfo = new AudioSourceInfo
    {
      Id = sourceId,
      Type = AudioSourceType.Spotify,
      Name = "Spotify",
      Status = AudioSourceStatus.Ready
    };
    _mockSourceManager.Setup(x => x.CreateSpotifySourceAsync()).ReturnsAsync(sourceId);
    _mockSourceManager.Setup(x => x.GetSourceInfoAsync(sourceId)).ReturnsAsync(sourceInfo);

    // Act
    var result = await _controller.CreateSpotifySource();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var response = Assert.IsType<CreateSourceResponse>(okResult.Value);
    Assert.Equal(sourceId, response.SourceId);
    Assert.NotNull(response.SourceInfo);
  }

  [Fact]
  public async Task CreateRadioSource_ReturnsOk_WithSourceId()
  {
    // Arrange
    var sourceId = "radio-0001-abc";
    var sourceInfo = new AudioSourceInfo
    {
      Id = sourceId,
      Type = AudioSourceType.USBRadio,
      Name = "USB Radio",
      Status = AudioSourceStatus.Ready
    };
    _mockSourceManager.Setup(x => x.CreateRadioSourceAsync()).ReturnsAsync(sourceId);
    _mockSourceManager.Setup(x => x.GetSourceInfoAsync(sourceId)).ReturnsAsync(sourceInfo);

    // Act
    var result = await _controller.CreateRadioSource();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var response = Assert.IsType<CreateSourceResponse>(okResult.Value);
    Assert.Equal(sourceId, response.SourceId);
  }

  [Fact]
  public async Task CreateVinylRecordSource_ReturnsOk_WithSourceId()
  {
    // Arrange
    var sourceId = "vinyl-0001-abc";
    _mockSourceManager.Setup(x => x.CreateVinylRecordSourceAsync()).ReturnsAsync(sourceId);
    _mockSourceManager.Setup(x => x.GetSourceInfoAsync(sourceId)).ReturnsAsync((AudioSourceInfo?)null);

    // Act
    var result = await _controller.CreateVinylRecordSource();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var response = Assert.IsType<CreateSourceResponse>(okResult.Value);
    Assert.Equal(sourceId, response.SourceId);
  }

  [Fact]
  public async Task CreateFilePlayerSource_ReturnsOk_WithSourceId()
  {
    // Arrange
    var sourceId = "fileplayer-0001-abc";
    var request = new CreateFilePlayerRequest { FilePath = "/path/to/audio.mp3" };
    _mockSourceManager.Setup(x => x.CreateFilePlayerSourceAsync(request.FilePath)).ReturnsAsync(sourceId);
    _mockSourceManager.Setup(x => x.GetSourceInfoAsync(sourceId)).ReturnsAsync((AudioSourceInfo?)null);

    // Act
    var result = await _controller.CreateFilePlayerSource(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var response = Assert.IsType<CreateSourceResponse>(okResult.Value);
    Assert.Equal(sourceId, response.SourceId);
  }

  // High Priority Audio Source Tests

  [Fact]
  public async Task CreateTtsEventSource_ReturnsOk_WithSourceId()
  {
    // Arrange
    var sourceId = "tts-0001-abc";
    var request = new CreateTtsEventRequest
    {
      Text = "Hello, world",
      Voice = "en-US",
      Speed = 1.0f
    };
    var sourceInfo = new AudioSourceInfo
    {
      Id = sourceId,
      Type = AudioSourceType.TtsEvent,
      IsHighPriority = true,
      Status = AudioSourceStatus.Ready
    };
    _mockSourceManager.Setup(x => x.CreateTtsEventSourceAsync(request.Text, "en-US", 1.0f)).ReturnsAsync(sourceId);
    _mockSourceManager.Setup(x => x.GetSourceInfoAsync(sourceId)).ReturnsAsync(sourceInfo);

    // Act
    var result = await _controller.CreateTtsEventSource(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var response = Assert.IsType<CreateSourceResponse>(okResult.Value);
    Assert.Equal(sourceId, response.SourceId);
    Assert.True(response.SourceInfo?.IsHighPriority);
  }

  [Fact]
  public async Task CreateTtsEventSource_ReturnsBadRequest_WithEmptyText()
  {
    // Arrange
    var request = new CreateTtsEventRequest { Text = "" };

    // Act
    var result = await _controller.CreateTtsEventSource(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result.Result);
  }

  [Fact]
  public async Task CreateTtsEventSource_ReturnsBadRequest_WithNullRequest()
  {
    // Act
    var result = await _controller.CreateTtsEventSource(null!);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result.Result);
  }

  [Fact]
  public async Task CreateFileEventSource_ReturnsOk_WithSourceId()
  {
    // Arrange
    var sourceId = "fileevent-0001-abc";
    var request = new CreateFileEventRequest { FilePath = "/sounds/doorbell.wav" };
    var sourceInfo = new AudioSourceInfo
    {
      Id = sourceId,
      Type = AudioSourceType.FileEvent,
      IsHighPriority = true,
      Status = AudioSourceStatus.Ready
    };
    _mockSourceManager.Setup(x => x.CreateFileEventSourceAsync(request.FilePath)).ReturnsAsync(sourceId);
    _mockSourceManager.Setup(x => x.GetSourceInfoAsync(sourceId)).ReturnsAsync(sourceInfo);

    // Act
    var result = await _controller.CreateFileEventSource(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var response = Assert.IsType<CreateSourceResponse>(okResult.Value);
    Assert.Equal(sourceId, response.SourceId);
    Assert.True(response.SourceInfo?.IsHighPriority);
  }

  [Fact]
  public async Task CreateFileEventSource_ReturnsBadRequest_WithEmptyFilePath()
  {
    // Arrange
    var request = new CreateFileEventRequest { FilePath = "" };

    // Act
    var result = await _controller.CreateFileEventSource(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result.Result);
  }

  // Source Management Tests

  [Fact]
  public async Task GetActiveSources_ReturnsOk_WithSourceList()
  {
    // Arrange
    var sources = new List<AudioSourceInfo>
    {
      new AudioSourceInfo { Id = "spotify-0001", Type = AudioSourceType.Spotify },
      new AudioSourceInfo { Id = "radio-0001", Type = AudioSourceType.USBRadio }
    };
    _mockSourceManager.Setup(x => x.GetActiveSourcesAsync()).ReturnsAsync(sources);

    // Act
    var result = await _controller.GetActiveSources();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var returnedSources = Assert.IsAssignableFrom<IEnumerable<AudioSourceInfo>>(okResult.Value);
    Assert.Equal(2, returnedSources.Count());
  }

  [Fact]
  public async Task GetSourceInfo_ReturnsOk_WhenSourceExists()
  {
    // Arrange
    var sourceId = "spotify-0001";
    var sourceInfo = new AudioSourceInfo { Id = sourceId, Type = AudioSourceType.Spotify };
    _mockSourceManager.Setup(x => x.GetSourceInfoAsync(sourceId)).ReturnsAsync(sourceInfo);

    // Act
    var result = await _controller.GetSourceInfo(sourceId);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var returnedInfo = Assert.IsType<AudioSourceInfo>(okResult.Value);
    Assert.Equal(sourceId, returnedInfo.Id);
  }

  [Fact]
  public async Task GetSourceInfo_ReturnsNotFound_WhenSourceDoesNotExist()
  {
    // Arrange
    var sourceId = "nonexistent";
    _mockSourceManager.Setup(x => x.GetSourceInfoAsync(sourceId)).ReturnsAsync((AudioSourceInfo?)null);

    // Act
    var result = await _controller.GetSourceInfo(sourceId);

    // Assert
    Assert.IsType<NotFoundObjectResult>(result.Result);
  }

  [Fact]
  public async Task StopSource_ReturnsOk()
  {
    // Arrange
    var sourceId = "spotify-0001";
    _mockSourceManager.Setup(x => x.StopSourceAsync(sourceId)).Returns(Task.CompletedTask);

    // Act
    var result = await _controller.StopSource(sourceId);

    // Assert
    Assert.IsType<OkObjectResult>(result);
    _mockSourceManager.Verify(x => x.StopSourceAsync(sourceId), Times.Once);
  }

  [Fact]
  public async Task StopAllSources_ReturnsOk()
  {
    // Arrange
    _mockSourceManager.Setup(x => x.StopAllSourcesAsync()).Returns(Task.CompletedTask);

    // Act
    var result = await _controller.StopAllSources();

    // Assert
    Assert.IsType<OkObjectResult>(result);
    _mockSourceManager.Verify(x => x.StopAllSourcesAsync(), Times.Once);
  }

  // Playback Control Tests

  [Fact]
  public async Task PlaySource_ReturnsOk_WhenSourceExists()
  {
    // Arrange
    var sourceId = "spotify-0001";
    var sourceInfo = new AudioSourceInfo { Id = sourceId, Type = AudioSourceType.Spotify };
    _mockSourceManager.Setup(x => x.GetSourceInfoAsync(sourceId)).ReturnsAsync(sourceInfo);
    _mockSourceManager.Setup(x => x.PlaySourceAsync(sourceId)).Returns(Task.CompletedTask);

    // Act
    var result = await _controller.PlaySource(sourceId);

    // Assert
    Assert.IsType<OkObjectResult>(result);
    _mockSourceManager.Verify(x => x.PlaySourceAsync(sourceId), Times.Once);
  }

  [Fact]
  public async Task PlaySource_ReturnsNotFound_WhenSourceDoesNotExist()
  {
    // Arrange
    var sourceId = "nonexistent";
    _mockSourceManager.Setup(x => x.GetSourceInfoAsync(sourceId)).ReturnsAsync((AudioSourceInfo?)null);

    // Act
    var result = await _controller.PlaySource(sourceId);

    // Assert
    Assert.IsType<NotFoundObjectResult>(result);
    _mockSourceManager.Verify(x => x.PlaySourceAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task PauseSource_ReturnsOk_WhenSourceExists()
  {
    // Arrange
    var sourceId = "spotify-0001";
    var sourceInfo = new AudioSourceInfo { Id = sourceId, Type = AudioSourceType.Spotify, Status = AudioSourceStatus.Playing };
    _mockSourceManager.Setup(x => x.GetSourceInfoAsync(sourceId)).ReturnsAsync(sourceInfo);
    _mockSourceManager.Setup(x => x.PauseSourceAsync(sourceId)).Returns(Task.CompletedTask);

    // Act
    var result = await _controller.PauseSource(sourceId);

    // Assert
    Assert.IsType<OkObjectResult>(result);
    _mockSourceManager.Verify(x => x.PauseSourceAsync(sourceId), Times.Once);
  }

  [Fact]
  public async Task PauseSource_ReturnsNotFound_WhenSourceDoesNotExist()
  {
    // Arrange
    var sourceId = "nonexistent";
    _mockSourceManager.Setup(x => x.GetSourceInfoAsync(sourceId)).ReturnsAsync((AudioSourceInfo?)null);

    // Act
    var result = await _controller.PauseSource(sourceId);

    // Assert
    Assert.IsType<NotFoundObjectResult>(result);
    _mockSourceManager.Verify(x => x.PauseSourceAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task ResumeSource_ReturnsOk_WhenSourceExists()
  {
    // Arrange
    var sourceId = "spotify-0001";
    var sourceInfo = new AudioSourceInfo { Id = sourceId, Type = AudioSourceType.Spotify, Status = AudioSourceStatus.Paused };
    _mockSourceManager.Setup(x => x.GetSourceInfoAsync(sourceId)).ReturnsAsync(sourceInfo);
    _mockSourceManager.Setup(x => x.ResumeSourceAsync(sourceId)).Returns(Task.CompletedTask);

    // Act
    var result = await _controller.ResumeSource(sourceId);

    // Assert
    Assert.IsType<OkObjectResult>(result);
    _mockSourceManager.Verify(x => x.ResumeSourceAsync(sourceId), Times.Once);
  }

  [Fact]
  public async Task ResumeSource_ReturnsNotFound_WhenSourceDoesNotExist()
  {
    // Arrange
    var sourceId = "nonexistent";
    _mockSourceManager.Setup(x => x.GetSourceInfoAsync(sourceId)).ReturnsAsync((AudioSourceInfo?)null);

    // Act
    var result = await _controller.ResumeSource(sourceId);

    // Assert
    Assert.IsType<NotFoundObjectResult>(result);
    _mockSourceManager.Verify(x => x.ResumeSourceAsync(It.IsAny<string>()), Times.Never);
  }
}
