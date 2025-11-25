using RadioConsole.Core.Configuration;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio;
using RadioConsole.Infrastructure.Audio.Sources;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace RadioConsole.Tests.Audio;

/// <summary>
/// Unit tests for MixerService.
/// </summary>
public class MixerServiceTests
{
  private readonly Mock<ILogger<MixerService>> _mockLogger;
  private readonly MixerService _service;

  public MixerServiceTests()
  {
    _mockLogger = new Mock<ILogger<MixerService>>();
    var options = Options.Create(new SoundFlowOptions
    {
      SampleRate = 48000,
      BitDepth = 16,
      Channels = 2,
      BufferSize = 2048
    });
    _service = new MixerService(_mockLogger.Object, options);
  }

  [Fact]
  public void Constructor_ShouldInitializeWithDefaultValues()
  {
    // Assert
    Assert.False(_service.IsInitialized);
    Assert.False(_service.IsDuckingActive);
    Assert.Equal(0.2f, _service.DuckLevel);
    Assert.Equal(1.0f, _service.GetMasterVolume());
  }

  [Theory]
  [InlineData(MixerChannel.Main)]
  [InlineData(MixerChannel.Event)]
  [InlineData(MixerChannel.Voice)]
  public void GetChannelVolume_ShouldReturnDefaultVolume(MixerChannel channel)
  {
    // Act
    var volume = _service.GetChannelVolume(channel);

    // Assert
    Assert.Equal(1.0f, volume);
  }

  [Fact]
  public void SetDuckLevel_ShouldUpdateDuckLevel()
  {
    // Act
    _service.SetDuckLevel(0.3f);

    // Assert
    Assert.Equal(0.3f, _service.DuckLevel);
  }

  [Theory]
  [InlineData(-0.1f, 0.0f)]
  [InlineData(1.5f, 1.0f)]
  [InlineData(0.5f, 0.5f)]
  public void SetDuckLevel_ShouldClampValues(float input, float expected)
  {
    // Act
    _service.SetDuckLevel(input);

    // Assert
    Assert.Equal(expected, _service.DuckLevel);
  }

  [Fact]
  public void GetAllSources_WhenNoSources_ShouldReturnEmpty()
  {
    // Act
    var sources = _service.GetAllSources();

    // Assert
    Assert.Empty(sources);
  }

  [Theory]
  [InlineData(MixerChannel.Main)]
  [InlineData(MixerChannel.Event)]
  [InlineData(MixerChannel.Voice)]
  public void GetChannelSources_WhenNoSources_ShouldReturnEmpty(MixerChannel channel)
  {
    // Act
    var sources = _service.GetChannelSources(channel);

    // Assert
    Assert.Empty(sources);
  }

  [Fact]
  public void GetSourceVolume_WhenSourceNotFound_ShouldReturnNull()
  {
    // Act
    var volume = _service.GetSourceVolume("nonexistent");

    // Assert
    Assert.Null(volume);
  }

  [Fact]
  public void AddSourceAsync_WhenNotInitialized_ShouldThrowInvalidOperationException()
  {
    // Arrange
    var mockSource = new Mock<ISoundFlowAudioSource>();
    mockSource.Setup(s => s.Id).Returns("test-source");
    mockSource.Setup(s => s.Name).Returns("Test Source");

    // Act & Assert
    Assert.ThrowsAsync<InvalidOperationException>(() =>
      _service.AddSourceAsync(mockSource.Object, MixerChannel.Main));
  }

  [Fact]
  public void RemoveSourceAsync_WhenNotInitialized_ShouldThrowInvalidOperationException()
  {
    // Act & Assert
    Assert.ThrowsAsync<InvalidOperationException>(() =>
      _service.RemoveSourceAsync("test-source"));
  }

  [Fact]
  public void SetChannelVolumeAsync_WhenNotInitialized_ShouldThrowInvalidOperationException()
  {
    // Act & Assert
    Assert.ThrowsAsync<InvalidOperationException>(() =>
      _service.SetChannelVolumeAsync(MixerChannel.Main, 0.5f));
  }

  [Fact]
  public void SetMasterVolumeAsync_WhenNotInitialized_ShouldThrowInvalidOperationException()
  {
    // Act & Assert
    Assert.ThrowsAsync<InvalidOperationException>(() =>
      _service.SetMasterVolumeAsync(0.5f));
  }

  [Fact]
  public void Dispose_ShouldNotThrow()
  {
    // Act & Assert (should not throw)
    _service.Dispose();
  }

  [Fact]
  public void Dispose_WhenCalledMultipleTimes_ShouldNotThrow()
  {
    // Act & Assert (should not throw)
    _service.Dispose();
    _service.Dispose();
  }
}
