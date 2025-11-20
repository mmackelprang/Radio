using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace RadioConsole.Tests.Audio;

/// <summary>
/// Unit tests for SystemTestService.
/// </summary>
public class SystemTestServiceTests
{
  private readonly Mock<IAudioPlayer> _mockAudioPlayer;
  private readonly Mock<IAudioPriorityService> _mockPriorityService;
  private readonly Mock<ILogger<SystemTestService>> _mockLogger;
  private readonly Mock<ILogger<TextToSpeechFactory>> _mockTtsFactoryLogger;
  private readonly Mock<IServiceProvider> _mockServiceProvider;
  private readonly TextToSpeechFactory _ttsFactory;
  private readonly SystemTestService _service;

  public SystemTestServiceTests()
  {
    _mockAudioPlayer = new Mock<IAudioPlayer>();
    _mockPriorityService = new Mock<IAudioPriorityService>();
    _mockLogger = new Mock<ILogger<SystemTestService>>();
    _mockTtsFactoryLogger = new Mock<ILogger<TextToSpeechFactory>>();
    _mockServiceProvider = new Mock<IServiceProvider>();

    // Setup service provider to return mocked dependencies
    _mockServiceProvider.Setup(x => x.GetService(typeof(IAudioPlayer)))
      .Returns(_mockAudioPlayer.Object);
    _mockServiceProvider.Setup(x => x.GetService(typeof(IAudioPriorityService)))
      .Returns(_mockPriorityService.Object);
    _mockServiceProvider.Setup(x => x.GetService(typeof(ILogger<ESpeakTextToSpeechService>)))
      .Returns(new Mock<ILogger<ESpeakTextToSpeechService>>().Object);

    _ttsFactory = new TextToSpeechFactory(_mockServiceProvider.Object, _mockTtsFactoryLogger.Object);
    
    _service = new SystemTestService(
      _mockAudioPlayer.Object,
      _mockPriorityService.Object,
      _ttsFactory,
      _mockLogger.Object);
  }

  [Fact]
  public void IsTestRunning_Initially_ShouldBeFalse()
  {
    // Assert
    Assert.False(_service.IsTestRunning);
  }

  [Fact]
  public async Task TriggerTtsAsync_WithEmptyPhrase_ShouldThrowException()
  {
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(
      () => _service.TriggerTtsAsync(string.Empty));
  }

  [Fact]
  public async Task TriggerTestToneAsync_WithValidParameters_ShouldGenerateTone()
  {
    // Arrange
    var frequency = 440;
    var duration = 0.1;

    // Act
    await _service.TriggerTestToneAsync(frequency, duration);

    // Assert
    _mockPriorityService.Verify(
      x => x.RegisterSourceAsync(It.IsAny<string>(), It.IsAny<Core.Enums.AudioPriority>()),
      Times.Once);
    _mockPriorityService.Verify(
      x => x.OnHighPriorityStartAsync(It.IsAny<string>()),
      Times.Once);
    _mockAudioPlayer.Verify(
      x => x.PlayAsync(It.IsAny<string>(), It.IsAny<Stream>()),
      Times.Once);
  }

  [Fact]
  public async Task TriggerTestToneAsync_WithInvalidFrequency_ShouldThrowException()
  {
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(
      () => _service.TriggerTestToneAsync(0, 2));
  }

  [Fact]
  public async Task TriggerTestToneAsync_WithInvalidDuration_ShouldThrowException()
  {
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(
      () => _service.TriggerTestToneAsync(300, 0));
  }

  [Fact]
  public async Task TriggerDoorbellAsync_ShouldSimulateDoorbell()
  {
    // Act
    await _service.TriggerDoorbellAsync();

    // Assert
    _mockPriorityService.Verify(
      x => x.RegisterSourceAsync(It.IsAny<string>(), It.IsAny<Core.Enums.AudioPriority>()),
      Times.Once);
    _mockPriorityService.Verify(
      x => x.OnHighPriorityStartAsync(It.IsAny<string>()),
      Times.Once);
    _mockAudioPlayer.Verify(
      x => x.PlayAsync(It.IsAny<string>(), It.IsAny<Stream>()),
      Times.Once);
    _mockPriorityService.Verify(
      x => x.OnHighPriorityEndAsync(It.IsAny<string>()),
      Times.Once);
  }
}
