using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace RadioConsole.Tests.Audio;

/// <summary>
/// Unit tests for ESpeakTextToSpeechService.
/// </summary>
public class ESpeakTextToSpeechServiceTests
{
  private readonly Mock<IAudioPlayer> _mockAudioPlayer;
  private readonly Mock<IAudioPriorityService> _mockPriorityService;
  private readonly Mock<ILogger<ESpeakTextToSpeechService>> _mockLogger;
  private readonly ESpeakTextToSpeechService _service;

  public ESpeakTextToSpeechServiceTests()
  {
    _mockAudioPlayer = new Mock<IAudioPlayer>();
    _mockPriorityService = new Mock<IAudioPriorityService>();
    _mockLogger = new Mock<ILogger<ESpeakTextToSpeechService>>();
    
    _service = new ESpeakTextToSpeechService(
      _mockAudioPlayer.Object,
      _mockPriorityService.Object,
      _mockLogger.Object);
  }

  [Fact]
  public void IsSpeaking_Initially_ShouldBeFalse()
  {
    // Assert
    Assert.False(_service.IsSpeaking);
  }

  [Fact]
  public async Task SynthesizeSpeechAsync_WithEmptyText_ShouldThrowException()
  {
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(
      () => _service.SynthesizeSpeechAsync(string.Empty));
  }

  [Fact]
  public async Task SynthesizeSpeechAsync_WithNullText_ShouldThrowException()
  {
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(
      () => _service.SynthesizeSpeechAsync(null!));
  }

  [Fact]
  public async Task StopAsync_WhenNotSpeaking_ShouldNotThrowException()
  {
    // Act
    await _service.StopAsync();

    // Assert - No exception thrown
    Assert.True(true);
  }

  // Note: Additional integration tests would require espeak to be installed
  // These are basic unit tests focusing on validation and behavior
}
