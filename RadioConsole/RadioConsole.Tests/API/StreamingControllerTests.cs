using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RadioConsole.API.Controllers;
using RadioConsole.API.Services;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio;
using Xunit;

namespace RadioConsole.Tests.API;

/// <summary>
/// Unit tests for StreamingController, StreamAudioController, and StreamAudioService.
/// Tests the unified format handling and auto-detection capabilities.
/// </summary>
public class StreamingControllerTests
{
  [Fact]
  public void StreamAudioService_SupportedFormats_ContainsAllRequiredFormats()
  {
    // Arrange
    var expectedFormats = new[] { "wav", "mp3", "flac", "aac", "ogg", "opus" };

    // Act
    var supportedFormats = StreamAudioService.SupportedFormats;

    // Assert
    Assert.Equal(expectedFormats.Length, supportedFormats.Length);
    foreach (var format in expectedFormats)
    {
      Assert.Contains(format, supportedFormats);
    }
  }

  [Theory]
  [InlineData("wav", true)]
  [InlineData("mp3", true)]
  [InlineData("flac", true)]
  [InlineData("aac", true)]
  [InlineData("ogg", true)]
  [InlineData("opus", true)]
  [InlineData("WAV", true)] // Test case insensitivity
  [InlineData("MP3", true)]
  [InlineData("xyz", false)]
  [InlineData("", false)]
  [InlineData("avi", false)]
  public void StreamAudioService_IsFormatSupported_ReturnsCorrectValue(string format, bool expected)
  {
    // Act
    var isSupported = StreamAudioService.IsFormatSupported(format);

    // Assert
    Assert.Equal(expected, isSupported);
  }

  [Theory]
  [InlineData("wav", "audio/wav")]
  [InlineData("mp3", "audio/mpeg")]
  [InlineData("flac", "audio/flac")]
  [InlineData("aac", "audio/aac")]
  [InlineData("ogg", "audio/ogg")]
  [InlineData("opus", "audio/opus")]
  [InlineData("WAV", "audio/wav")] // Test case insensitivity
  [InlineData("unknown", "audio/mpeg")] // Default
  public void StreamAudioService_GetContentType_ReturnsCorrectMimeType(string format, string expectedContentType)
  {
    // Act
    var contentType = StreamAudioService.GetContentType(format);

    // Assert
    Assert.Equal(expectedContentType, contentType);
  }

  [Theory]
  [InlineData("wav", AudioFormat.Wav)]
  [InlineData("mp3", AudioFormat.Mp3)]
  [InlineData("flac", AudioFormat.Flac)]
  [InlineData("aac", AudioFormat.Aac)]
  [InlineData("ogg", AudioFormat.Ogg)]
  [InlineData("opus", AudioFormat.Opus)]
  [InlineData("WAV", AudioFormat.Wav)]
  [InlineData("unknown", AudioFormat.Mp3)] // Default
  public void StreamAudioService_ParseFormat_ReturnsCorrectAudioFormat(string format, AudioFormat expected)
  {
    // Act
    var audioFormat = StreamAudioService.ParseFormat(format);

    // Assert
    Assert.Equal(expected, audioFormat);
  }
}

/// <summary>
/// Tests for the StreamAudioController unified format handling.
/// </summary>
public class StreamAudioControllerUnifiedTests
{
  private readonly Mock<IAudioFormatDetector> _formatDetectorMock;
  private readonly Mock<IAudioProcessor> _processorMock;
  private readonly Mock<ILogger<StreamAudioController>> _loggerMock;
  private readonly StreamAudioController _controller;

  public StreamAudioControllerUnifiedTests()
  {
    var audioPlayerMock = new Mock<IAudioPlayer>();
    var detectorLogger = new Mock<ILogger<AudioFormatDetector>>();
    var detector = new AudioFormatDetector(detectorLogger.Object);

    _formatDetectorMock = new Mock<IAudioFormatDetector>();
    _processorMock = new Mock<IAudioProcessor>();
    _loggerMock = new Mock<ILogger<StreamAudioController>>();

    // Setup processor to return all formats
    _processorMock.Setup(x => x.GetSupportedFormats())
      .Returns(new[] { AudioFormat.Mp3, AudioFormat.Wav, AudioFormat.Flac, AudioFormat.Aac, AudioFormat.Ogg, AudioFormat.Opus });

    _processorMock.Setup(x => x.CanProcess(It.IsAny<AudioFormat>())).Returns(true);
    _processorMock.Setup(x => x.GetContentType(It.IsAny<AudioFormat>()))
      .Returns((AudioFormat f) => detector.GetContentType(f));

    _formatDetectorMock.Setup(x => x.GetContentType(It.IsAny<AudioFormat>()))
      .Returns((AudioFormat f) => detector.GetContentType(f));

    _formatDetectorMock.Setup(x => x.GetFileExtension(It.IsAny<AudioFormat>()))
      .Returns((AudioFormat f) => detector.GetFileExtension(f));

    // Create a real StreamAudioService for testing
    var realProcessor = new AudioProcessor(
      new Mock<ILogger<AudioProcessor>>().Object,
      detector);

    var streamService = new StreamAudioService(
      new Mock<ILogger<StreamAudioService>>().Object,
      audioPlayerMock.Object,
      detector,
      realProcessor);

    _controller = new StreamAudioController(
      streamService,
      _formatDetectorMock.Object,
      _processorMock.Object,
      _loggerMock.Object);

    // Set up HttpContext
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Scheme = "https";
    httpContext.Request.Host = new HostString("localhost", 5100);
    _controller.ControllerContext = new ControllerContext
    {
      HttpContext = httpContext
    };
  }

  [Fact]
  public void GetInfo_ReturnsStreamingInfoWithUnifiedEndpoint()
  {
    // Act
    var result = _controller.GetInfo();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var info = Assert.IsType<StreamingInfo>(okResult.Value);

    Assert.Contains("/api/streaming/stream", info.StreamUrl);
    Assert.Contains("/api/streaming/stream/auto", info.AutoDetectUrl);
    Assert.Equal(6, info.SupportedFormats.Length);
    Assert.Equal(6, info.FormatDetails.Length);
    Assert.NotEmpty(info.UnifiedEndpointExample);
  }

  [Fact]
  public void GetStatus_ReturnsStatusWithAutoDetectionEnabled()
  {
    // Act
    var result = _controller.GetStatus();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var status = Assert.IsType<StreamingStatus>(okResult.Value);

    Assert.True(status.IsAvailable);
    Assert.True(status.AutoDetectionEnabled);
    Assert.Equal(1, status.ProcessorCount); // Single unified processor
    Assert.True(status.Capabilities.SupportsAutoDetection);
    Assert.True(status.Capabilities.SupportsUnifiedEndpoint);
  }

  [Fact]
  public void DetectFormat_WithValidHeaderBytes_ReturnsDetectionResult()
  {
    // Arrange
    var flacBytes = new byte[] { 0x66, 0x4C, 0x61, 0x43, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
    var request = new FormatDetectionRequest { HeaderBytes = flacBytes };

    _formatDetectorMock.Setup(x => x.DetectFormat(It.IsAny<byte[]>()))
      .Returns(new AudioFormatDetectionResult
      {
        Format = AudioFormat.Flac,
        Confidence = 1.0,
        ContentType = "audio/flac",
        DetectionMethod = "Magic bytes: fLaC"
      });

    // Act
    var result = _controller.DetectFormat(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var detection = Assert.IsType<FormatDetectionResponse>(okResult.Value);

    Assert.Equal("Flac", detection.Format);
    Assert.Equal(1.0, detection.Confidence);
    Assert.True(detection.IsSuccess);
  }

  [Fact]
  public void DetectFormat_WithEmptyHeaderBytes_ReturnsBadRequest()
  {
    // Arrange
    var request = new FormatDetectionRequest { HeaderBytes = Array.Empty<byte>() };

    // Act
    var result = _controller.DetectFormat(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result.Result);
  }

  [Fact]
  public void GetInfo_FormatDetails_ContainsAllFormatInfo()
  {
    // Act
    var result = _controller.GetInfo();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var info = Assert.IsType<StreamingInfo>(okResult.Value);

    foreach (var detail in info.FormatDetails)
    {
      Assert.NotEmpty(detail.Format);
      Assert.NotEmpty(detail.ContentType);
      Assert.NotEmpty(detail.FileExtension);
      Assert.NotEmpty(detail.StreamUrl);
    }
  }
}

/// <summary>
/// Tests for the StreamingController unified endpoints.
/// </summary>
public class StreamingControllerUnifiedEndpointTests
{
  private readonly Mock<IAudioFormatDetector> _formatDetectorMock;
  private readonly Mock<ILogger<StreamingController>> _loggerMock;
  private readonly StreamingController _controller;

  public StreamingControllerUnifiedEndpointTests()
  {
    var audioPlayerMock = new Mock<IAudioPlayer>();
    var detectorLogger = new Mock<ILogger<AudioFormatDetector>>();
    var detector = new AudioFormatDetector(detectorLogger.Object);

    _formatDetectorMock = new Mock<IAudioFormatDetector>();
    _loggerMock = new Mock<ILogger<StreamingController>>();

    var processor = new AudioProcessor(
      new Mock<ILogger<AudioProcessor>>().Object,
      detector);

    var streamService = new StreamAudioService(
      new Mock<ILogger<StreamAudioService>>().Object,
      audioPlayerMock.Object,
      detector,
      processor);

    _formatDetectorMock.Setup(x => x.GetContentType(It.IsAny<AudioFormat>()))
      .Returns((AudioFormat f) => detector.GetContentType(f));

    _formatDetectorMock.Setup(x => x.GetFileExtension(It.IsAny<AudioFormat>()))
      .Returns((AudioFormat f) => detector.GetFileExtension(f));

    _controller = new StreamingController(
      streamService,
      _formatDetectorMock.Object,
      _loggerMock.Object);

    // Set up HttpContext
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Scheme = "https";
    httpContext.Request.Host = new HostString("localhost", 5100);
    _controller.ControllerContext = new ControllerContext
    {
      HttpContext = httpContext
    };
  }

  [Fact]
  public void GetFormatCapabilities_ReturnsAllFormatsAndEndpoints()
  {
    // Act
    var result = _controller.GetFormatCapabilities();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var capabilities = Assert.IsType<FormatCapabilitiesResponse>(okResult.Value);

    Assert.True(capabilities.AutoDetectionAvailable);
    Assert.Equal(6, capabilities.SupportedFormats.Length);
    Assert.Contains("/api/streaming/stream", capabilities.UnifiedEndpoint);
    Assert.Contains("/api/streaming/stream/auto", capabilities.AutoDetectEndpoint);
  }

  [Fact]
  public void GetFormatCapabilities_EachFormatHasCorrectInfo()
  {
    // Act
    var result = _controller.GetFormatCapabilities();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var capabilities = Assert.IsType<FormatCapabilitiesResponse>(okResult.Value);

    foreach (var format in capabilities.SupportedFormats)
    {
      Assert.NotEmpty(format.Format);
      Assert.StartsWith("audio/", format.ContentType);
      Assert.StartsWith(".", format.FileExtension);
    }
  }
}
