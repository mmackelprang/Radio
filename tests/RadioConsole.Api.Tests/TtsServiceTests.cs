using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Models;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Tests;

/// <summary>
/// Tests for TtsService covering:
/// - Initialization in simulation mode
/// - Initialization with different engines
/// - TTS generation in simulation mode
/// - Proper error handling and logging
/// </summary>
public class TtsServiceTests
{
  private readonly Mock<IEnvironmentService> _mockEnvironmentService;
  private readonly Mock<ILogger<TtsService>> _mockLogger;
  private readonly Mock<ILoggerFactory> _mockLoggerFactory;

  public TtsServiceTests()
  {
    _mockEnvironmentService = new Mock<IEnvironmentService>();
    _mockLogger = new Mock<ILogger<TtsService>>();
    _mockLoggerFactory = new Mock<ILoggerFactory>();

    // Setup mock logger factory to return mock loggers
    _mockLoggerFactory
      .Setup(x => x.CreateLogger(It.IsAny<string>()))
      .Returns(new Mock<ILogger>().Object);
  }

  private IOptions<TtsConfig> CreateDefaultConfig()
  {
    var config = new TtsConfig
    {
      Engine = "EspeakNG",
      EspeakNg = new EspeakNgConfig
      {
        ExecutablePath = "espeak-ng",
        Voice = "en-us",
        Speed = 175,
        Pitch = 50,
        Volume = 100,
        WordGap = 0,
        SampleRate = 22050
      }
    };
    return Options.Create(config);
  }

  [Fact]
  public async Task InitializeAsync_InSimulationMode_InitializesSuccessfully()
  {
    // Arrange
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(true);
    var service = new TtsService(CreateDefaultConfig(), _mockEnvironmentService.Object, _mockLogger.Object, _mockLoggerFactory.Object);

    // Act
    await service.InitializeAsync();

    // Assert
    service.IsAvailable.Should().BeTrue();
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("simulation mode")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task InitializeAsync_WithEspeakNgEngine_WhenNotAvailable_MarksAsUnavailable()
  {
    // Arrange
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(false);
    
    // Use a non-existent executable path to ensure espeak-ng is not available
    var config = new TtsConfig
    {
      Engine = "EspeakNG",
      EspeakNg = new EspeakNgConfig
      {
        ExecutablePath = "/nonexistent/espeak-ng",
        Voice = "en-us",
        Speed = 175,
        Pitch = 50,
        Volume = 100,
        WordGap = 0,
        SampleRate = 22050
      }
    };
    var service = new TtsService(Options.Create(config), _mockEnvironmentService.Object, _mockLogger.Object, _mockLoggerFactory.Object);

    // Act
    await service.InitializeAsync();

    // Assert
    service.IsAvailable.Should().BeFalse();
  }

  [Fact]
  public async Task GenerateSpeechAsync_InSimulationMode_ReturnsAudioStream()
  {
    // Arrange
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(true);
    var service = new TtsService(CreateDefaultConfig(), _mockEnvironmentService.Object, _mockLogger.Object, _mockLoggerFactory.Object);
    await service.InitializeAsync();

    // Act
    var stream = await service.GenerateSpeechAsync("Hello World");

    // Assert
    stream.Should().NotBeNull();
    stream!.Length.Should().BeGreaterThan(0);
  }

  [Fact]
  public async Task GenerateSpeechAsync_WithEmptyText_ThrowsArgumentException()
  {
    // Arrange
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(true);
    var service = new TtsService(CreateDefaultConfig(), _mockEnvironmentService.Object, _mockLogger.Object, _mockLoggerFactory.Object);
    await service.InitializeAsync();

    // Act
    Func<Task> act = async () => await service.GenerateSpeechAsync("");

    // Assert
    await act.Should().ThrowAsync<ArgumentException>()
      .WithMessage("*Text cannot be empty*");
  }

  [Fact]
  public async Task GenerateSpeechAsync_WhenNotAvailable_ThrowsInvalidOperationException()
  {
    // Arrange
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(false);
    
    // Use a non-existent executable path to ensure espeak-ng is not available
    var config = new TtsConfig
    {
      Engine = "EspeakNG",
      EspeakNg = new EspeakNgConfig
      {
        ExecutablePath = "/nonexistent/espeak-ng",
        Voice = "en-us",
        Speed = 175,
        Pitch = 50,
        Volume = 100,
        WordGap = 0,
        SampleRate = 22050
      }
    };
    var service = new TtsService(Options.Create(config), _mockEnvironmentService.Object, _mockLogger.Object, _mockLoggerFactory.Object);
    await service.InitializeAsync();

    // Act
    Func<Task> act = async () => await service.GenerateSpeechAsync("Hello World");

    // Assert
    await act.Should().ThrowAsync<InvalidOperationException>()
      .WithMessage("*not available*");
  }

  [Fact]
  public void EstimateDuration_WithValidText_ReturnsPositiveDuration()
  {
    // Arrange
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(true);
    var service = new TtsService(CreateDefaultConfig(), _mockEnvironmentService.Object, _mockLogger.Object, _mockLoggerFactory.Object);

    // Act
    var duration = service.EstimateDuration("This is a test sentence with ten words");

    // Assert
    duration.Should().BeGreaterThan(0);
  }

  [Fact]
  public void EstimateDuration_WithEmptyText_ReturnsZero()
  {
    // Arrange
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(true);
    var service = new TtsService(CreateDefaultConfig(), _mockEnvironmentService.Object, _mockLogger.Object, _mockLoggerFactory.Object);

    // Act
    var duration = service.EstimateDuration("");

    // Assert
    duration.Should().Be(0);
  }

  [Fact]
  public void EstimateDuration_WithLongerText_ReturnsLongerDuration()
  {
    // Arrange
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(true);
    var service = new TtsService(CreateDefaultConfig(), _mockEnvironmentService.Object, _mockLogger.Object, _mockLoggerFactory.Object);

    var shortText = "Hello";
    var longText = "Hello World this is a much longer sentence with many more words";

    // Act
    var shortDuration = service.EstimateDuration(shortText);
    var longDuration = service.EstimateDuration(longText);

    // Assert
    longDuration.Should().BeGreaterThan(shortDuration);
  }

  [Fact]
  public async Task GenerateSpeechAsync_InSimulationMode_GeneratesWAVFormat()
  {
    // Arrange
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(true);
    var service = new TtsService(CreateDefaultConfig(), _mockEnvironmentService.Object, _mockLogger.Object, _mockLoggerFactory.Object);
    await service.InitializeAsync();

    // Act
    var stream = await service.GenerateSpeechAsync("Test");

    // Assert
    stream.Should().NotBeNull();

    // Check WAV header
    var buffer = new byte[12];
    stream!.Position = 0;
    var bytesRead = await stream.ReadAsync(buffer.AsMemory());
    bytesRead.Should().Be(12);

    // RIFF header
    System.Text.Encoding.ASCII.GetString(buffer, 0, 4).Should().Be("RIFF");
    // WAVE format
    System.Text.Encoding.ASCII.GetString(buffer, 8, 4).Should().Be("WAVE");
  }

  [Fact]
  public async Task InitializeAsync_WithPiperEngine_WhenConfigMissing_MarksAsUnavailable()
  {
    // Arrange
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(false);
    var config = new TtsConfig
    {
      Engine = "Piper",
      Piper = new PiperConfig
      {
        ExecutablePath = "piper",
        ModelPath = "/nonexistent/model.onnx",
        ConfigPath = "/nonexistent/config.json"
      }
    };
    var service = new TtsService(Options.Create(config), _mockEnvironmentService.Object, _mockLogger.Object, _mockLoggerFactory.Object);

    // Act
    await service.InitializeAsync();

    // Assert
    service.IsAvailable.Should().BeFalse();
  }

  [Fact]
  public async Task InitializeAsync_WithGoogleCloudEngine_WhenCredentialsMissing_MarksAsUnavailable()
  {
    // Arrange
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(false);
    var config = new TtsConfig
    {
      Engine = "GoogleCloud",
      GoogleCloud = new GoogleCloudConfig
      {
        CredentialsPath = "/nonexistent/credentials.json",
        LanguageCode = "en-US"
      }
    };
    var service = new TtsService(Options.Create(config), _mockEnvironmentService.Object, _mockLogger.Object, _mockLoggerFactory.Object);

    // Act
    await service.InitializeAsync();

    // Assert
    service.IsAvailable.Should().BeFalse();
  }

  [Fact]
  public async Task InitializeAsync_WithUnknownEngine_FallsBackToEspeakNg()
  {
    // Arrange
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(false);
    var config = new TtsConfig
    {
      Engine = "UnknownEngine",
      EspeakNg = new EspeakNgConfig()
    };
    var service = new TtsService(Options.Create(config), _mockEnvironmentService.Object, _mockLogger.Object, _mockLoggerFactory.Object);

    // Act
    await service.InitializeAsync();

    // Assert - Should fall back to EspeakNG and log warning
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unknown TTS engine")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }
}
