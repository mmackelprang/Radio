using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio;
using RadioConsole.Infrastructure.Audio.Sources;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace RadioConsole.Tests.Audio;

/// <summary>
/// Unit tests for AudioSourceFactory.
/// </summary>
public class AudioSourceFactoryTests
{
  private readonly Mock<ILogger<AudioSourceFactory>> _mockLogger;
  private readonly Mock<ILoggerFactory> _mockLoggerFactory;
  private readonly Mock<ITextToSpeechService> _mockTtsService;
  private readonly AudioSourceFactory _factory;

  public AudioSourceFactoryTests()
  {
    _mockLogger = new Mock<ILogger<AudioSourceFactory>>();
    _mockLoggerFactory = new Mock<ILoggerFactory>();
    _mockTtsService = new Mock<ITextToSpeechService>();

    // Setup logger factory to return mock loggers
    _mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
      .Returns(new Mock<ILogger>().Object);

    _factory = new AudioSourceFactory(
      _mockLogger.Object,
      _mockLoggerFactory.Object,
      _mockTtsService.Object);
  }

  [Fact]
  public void SupportedExtensions_ShouldContainCommonFormats()
  {
    // Assert
    Assert.Contains(".mp3", _factory.SupportedExtensions);
    Assert.Contains(".wav", _factory.SupportedExtensions);
    Assert.Contains(".flac", _factory.SupportedExtensions);
    Assert.Contains(".ogg", _factory.SupportedExtensions);
    Assert.Contains(".m4a", _factory.SupportedExtensions);
  }

  [Theory]
  [InlineData("spotify:track:1234567890", AudioSourceType.Spotify)]
  [InlineData("spotify:playlist:abc123", AudioSourceType.Spotify)]
  [InlineData("https://open.spotify.com/track/123", AudioSourceType.Spotify)]
  public void DetectSourceType_ShouldDetectSpotifyUris(string uri, AudioSourceType expected)
  {
    // Act
    var result = _factory.DetectSourceType(uri);

    // Assert
    Assert.Equal(expected, result);
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("  ")]
  public void DetectSourceType_WithInvalidInput_ShouldThrowArgumentException(string? uri)
  {
    // Act & Assert
    Assert.Throws<ArgumentException>(() => _factory.DetectSourceType(uri!));
  }

  [Fact]
  public async Task CreateFromFileAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
  {
    // Arrange
    var filePath = "/nonexistent/file.mp3";

    // Act & Assert
    await Assert.ThrowsAsync<FileNotFoundException>(() =>
      _factory.CreateFromFileAsync(filePath));
  }

  [Fact]
  public async Task CreateTtsAsync_WithNullTtsService_ShouldThrowInvalidOperationException()
  {
    // Arrange
    var factoryWithoutTts = new AudioSourceFactory(
      _mockLogger.Object,
      _mockLoggerFactory.Object,
      null);

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(() =>
      factoryWithoutTts.CreateTtsAsync("Hello world"));
  }

  [Fact]
  public async Task CreateTtsAsync_WithValidInput_ShouldCreateSource()
  {
    // Arrange
    var text = "Hello world";
    var audioStream = new MemoryStream(new byte[] { 0x00, 0x01, 0x02 });
    _mockTtsService.Setup(s => s.SynthesizeSpeechAsync(text, null, 1.0f))
      .ReturnsAsync(audioStream);

    // Act
    var source = await _factory.CreateTtsAsync(text);

    // Assert
    Assert.NotNull(source);
    Assert.Equal(AudioSourceType.TtsEvent, source.SourceType);
    Assert.Equal(MixerChannel.Voice, source.Channel);
    Assert.Contains("tts-", source.Id);
  }

  [Fact]
  public async Task CreateSpotifyStreamAsync_ShouldCreateSource()
  {
    // Arrange
    var trackUri = "spotify:track:1234567890";

    // Act
    var source = await _factory.CreateSpotifyStreamAsync(trackUri);

    // Assert
    Assert.NotNull(source);
    Assert.Equal(AudioSourceType.Spotify, source.SourceType);
    Assert.Equal(MixerChannel.Main, source.Channel);
    Assert.Contains("spotify-", source.Id);
  }

  [Fact]
  public async Task CreateUsbInputAsync_ShouldCreateSource()
  {
    // Arrange
    var deviceId = "usb-device-123";
    var deviceName = "Raddy RF320";

    // Act
    var source = await _factory.CreateUsbInputAsync(deviceId, deviceName);

    // Assert
    Assert.NotNull(source);
    Assert.Equal(AudioSourceType.USBRadio, source.SourceType);
    Assert.Contains("usb-", source.Id);
    Assert.Equal(deviceName, source.Name);
  }

  [Fact]
  public void DisposeSource_WithNonExistentId_ShouldNotThrow()
  {
    // Act & Assert (should not throw)
    _factory.DisposeSource("nonexistent");
  }

  [Fact]
  public void DisposeAllSources_WhenNoSources_ShouldNotThrow()
  {
    // Act & Assert (should not throw)
    _factory.DisposeAllSources();
  }

  [Fact]
  public void Dispose_ShouldNotThrow()
  {
    // Act & Assert (should not throw)
    _factory.Dispose();
  }

  [Fact]
  public void Dispose_WhenCalledMultipleTimes_ShouldNotThrow()
  {
    // Act & Assert (should not throw)
    _factory.Dispose();
    _factory.Dispose();
  }

  [Fact]
  public async Task CreateFromFileAsync_AfterDispose_ShouldThrowObjectDisposedException()
  {
    // Arrange
    _factory.Dispose();

    // Act & Assert
    await Assert.ThrowsAsync<ObjectDisposedException>(() =>
      _factory.CreateFromFileAsync("/some/file.mp3"));
  }

  [Fact]
  public async Task CreateTtsAsync_AfterDispose_ShouldThrowObjectDisposedException()
  {
    // Arrange
    _factory.Dispose();

    // Act & Assert
    await Assert.ThrowsAsync<ObjectDisposedException>(() =>
      _factory.CreateTtsAsync("Hello world"));
  }
}
