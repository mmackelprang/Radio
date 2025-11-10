using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Models;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Tests;

/// <summary>
/// Tests for ESpeakTtsService covering:
/// - Initialization in simulation mode
/// - Initialization when espeak is not available
/// - TTS generation in simulation mode
/// - Proper error handling and logging
/// </summary>
public class ESpeakTtsServiceTests
{
    private readonly Mock<IEnvironmentService> _mockEnvironmentService;
    private readonly Mock<ILogger<ESpeakTtsService>> _mockLogger;
    private readonly IOptions<ESpeakTtsConfig> _config;

    public ESpeakTtsServiceTests()
    {
        _mockEnvironmentService = new Mock<IEnvironmentService>();
        _mockLogger = new Mock<ILogger<ESpeakTtsService>>();

        var config = new ESpeakTtsConfig
        {
            ESpeakExecutablePath = "espeak-ng",
            Voice = "en-us",
            Speed = 175,
            Pitch = 50,
            Volume = 100,
            WordGap = 0,
            SampleRate = 22050
        };
        _config = Options.Create(config);
    }

    [Fact]
    public async Task InitializeAsync_InSimulationMode_InitializesSuccessfully()
    {
        // Arrange
        _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(true);
        var service = new ESpeakTtsService(_config, _mockEnvironmentService.Object, _mockLogger.Object);

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
    public async Task InitializeAsync_WhenESpeakNotAvailable_MarksAsUnavailable()
    {
        // Arrange
        _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(false);
        var service = new ESpeakTtsService(_config, _mockEnvironmentService.Object, _mockLogger.Object);

        // Act
        await service.InitializeAsync();

        // Assert
        service.IsAvailable.Should().BeFalse();
        // Should log INFO message about espeak not being available
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not available")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateSpeechAsync_InSimulationMode_ReturnsAudioStream()
    {
        // Arrange
        _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(true);
        var service = new ESpeakTtsService(_config, _mockEnvironmentService.Object, _mockLogger.Object);
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
        var service = new ESpeakTtsService(_config, _mockEnvironmentService.Object, _mockLogger.Object);
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
        var service = new ESpeakTtsService(_config, _mockEnvironmentService.Object, _mockLogger.Object);
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
        var service = new ESpeakTtsService(_config, _mockEnvironmentService.Object, _mockLogger.Object);

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
        var service = new ESpeakTtsService(_config, _mockEnvironmentService.Object, _mockLogger.Object);

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
        var service = new ESpeakTtsService(_config, _mockEnvironmentService.Object, _mockLogger.Object);

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
        var service = new ESpeakTtsService(_config, _mockEnvironmentService.Object, _mockLogger.Object);
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
    public async Task InitializeAsync_DoesNotLogWarningOrError_WhenESpeakNotInstalled()
    {
        // Arrange
        _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(false);
        var service = new ESpeakTtsService(_config, _mockEnvironmentService.Object, _mockLogger.Object);

        // Act
        await service.InitializeAsync();

        // Assert - Should NOT log any warnings or errors, only info
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
