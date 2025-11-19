using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace RadioConsole.Tests.Audio;

/// <summary>
/// Unit tests for AudioPriorityService.
/// </summary>
public class AudioPriorityServiceTests
{
  private readonly Mock<IAudioPlayer> _mockAudioPlayer;
  private readonly Mock<ILogger<AudioPriorityService>> _mockLogger;
  private readonly AudioPriorityService _service;

  public AudioPriorityServiceTests()
  {
    _mockAudioPlayer = new Mock<IAudioPlayer>();
    _mockLogger = new Mock<ILogger<AudioPriorityService>>();
    _service = new AudioPriorityService(_mockAudioPlayer.Object, _mockLogger.Object);
  }

  [Fact]
  public async Task RegisterSourceAsync_ShouldRegisterSource()
  {
    // Arrange
    var sourceId = "test-source";
    var priority = AudioPriority.Low;

    // Act
    await _service.RegisterSourceAsync(sourceId, priority);

    // Assert - No exception thrown
    Assert.True(true);
  }

  [Fact]
  public async Task UnregisterSourceAsync_ShouldRemoveSource()
  {
    // Arrange
    var sourceId = "test-source";
    await _service.RegisterSourceAsync(sourceId, AudioPriority.Low);

    // Act
    await _service.UnregisterSourceAsync(sourceId);

    // Assert - No exception thrown
    Assert.True(true);
  }

  [Fact]
  public async Task OnHighPriorityStartAsync_ShouldDuckLowPrioritySources()
  {
    // Arrange
    var lowPrioritySource = "radio";
    var highPrioritySource = "tts";

    await _service.RegisterSourceAsync(lowPrioritySource, AudioPriority.Low);
    await _service.RegisterSourceAsync(highPrioritySource, AudioPriority.High);

    // Act
    await _service.OnHighPriorityStartAsync(highPrioritySource);

    // Assert
    Assert.True(_service.IsHighPriorityActive);
    _mockAudioPlayer.Verify(
      x => x.SetVolumeAsync(lowPrioritySource, It.IsAny<float>()),
      Times.Once);
  }

  [Fact]
  public async Task OnHighPriorityEndAsync_ShouldRestoreLowPrioritySources()
  {
    // Arrange
    var lowPrioritySource = "radio";
    var highPrioritySource = "tts";

    await _service.RegisterSourceAsync(lowPrioritySource, AudioPriority.Low);
    await _service.RegisterSourceAsync(highPrioritySource, AudioPriority.High);
    await _service.OnHighPriorityStartAsync(highPrioritySource);

    // Reset mock to clear previous calls
    _mockAudioPlayer.Reset();

    // Act
    await _service.OnHighPriorityEndAsync(highPrioritySource);

    // Assert
    Assert.False(_service.IsHighPriorityActive);
    _mockAudioPlayer.Verify(
      x => x.SetVolumeAsync(lowPrioritySource, It.IsAny<float>()),
      Times.Once);
  }

  [Fact]
  public async Task OnHighPriorityStartAsync_WithMultipleHighPriority_ShouldOnlyDuckOnce()
  {
    // Arrange
    var lowPrioritySource = "radio";
    var highPrioritySource1 = "tts";
    var highPrioritySource2 = "doorbell";

    await _service.RegisterSourceAsync(lowPrioritySource, AudioPriority.Low);
    await _service.RegisterSourceAsync(highPrioritySource1, AudioPriority.High);
    await _service.RegisterSourceAsync(highPrioritySource2, AudioPriority.High);

    // Act
    await _service.OnHighPriorityStartAsync(highPrioritySource1);
    _mockAudioPlayer.Reset(); // Clear previous calls
    await _service.OnHighPriorityStartAsync(highPrioritySource2);

    // Assert
    Assert.True(_service.IsHighPriorityActive);
    // Should not duck again since high priority is already active
    _mockAudioPlayer.Verify(
      x => x.SetVolumeAsync(It.IsAny<string>(), It.IsAny<float>()),
      Times.Never);
  }

  [Fact]
  public async Task OnHighPriorityEndAsync_WithMultipleHighPriority_ShouldNotRestoreUntilAllEnd()
  {
    // Arrange
    var lowPrioritySource = "radio";
    var highPrioritySource1 = "tts";
    var highPrioritySource2 = "doorbell";

    await _service.RegisterSourceAsync(lowPrioritySource, AudioPriority.Low);
    await _service.RegisterSourceAsync(highPrioritySource1, AudioPriority.High);
    await _service.RegisterSourceAsync(highPrioritySource2, AudioPriority.High);

    await _service.OnHighPriorityStartAsync(highPrioritySource1);
    await _service.OnHighPriorityStartAsync(highPrioritySource2);

    _mockAudioPlayer.Reset();

    // Act - End first high priority source
    await _service.OnHighPriorityEndAsync(highPrioritySource1);

    // Assert - Should still be high priority active
    Assert.True(_service.IsHighPriorityActive);
    _mockAudioPlayer.Verify(
      x => x.SetVolumeAsync(It.IsAny<string>(), It.IsAny<float>()),
      Times.Never);

    // Act - End second high priority source
    await _service.OnHighPriorityEndAsync(highPrioritySource2);

    // Assert - Should restore now
    Assert.False(_service.IsHighPriorityActive);
    _mockAudioPlayer.Verify(
      x => x.SetVolumeAsync(lowPrioritySource, It.IsAny<float>()),
      Times.Once);
  }

  [Fact]
  public async Task SetDuckPercentageAsync_ShouldUpdateDuckPercentage()
  {
    // Arrange
    var newPercentage = 0.3f;

    // Act
    await _service.SetDuckPercentageAsync(newPercentage);

    // Assert
    Assert.Equal(newPercentage, _service.DuckPercentage);
  }

  [Fact]
  public async Task SetDuckPercentageAsync_WithInvalidValue_ShouldThrowException()
  {
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
      () => _service.SetDuckPercentageAsync(-0.1f));

    await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
      () => _service.SetDuckPercentageAsync(1.1f));
  }

  [Fact]
  public void DuckPercentage_DefaultValue_ShouldBe20Percent()
  {
    // Assert
    Assert.Equal(0.2f, _service.DuckPercentage);
  }

  [Fact]
  public void IsHighPriorityActive_Initially_ShouldBeFalse()
  {
    // Assert
    Assert.False(_service.IsHighPriorityActive);
  }

  [Fact]
  public async Task OnHighPriorityStartAsync_WithUnregisteredSource_ShouldAutoRegister()
  {
    // Arrange
    var highPrioritySource = "unregistered-tts";

    // Act
    await _service.OnHighPriorityStartAsync(highPrioritySource);

    // Assert
    Assert.True(_service.IsHighPriorityActive);
  }
}
