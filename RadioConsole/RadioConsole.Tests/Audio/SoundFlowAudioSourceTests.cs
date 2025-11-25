using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio.Sources;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace RadioConsole.Tests.Audio;

/// <summary>
/// Unit tests for SoundFlow audio source implementations.
/// </summary>
public class SoundFlowAudioSourceTests : IDisposable
{
  private readonly string _testFilePath;
  private readonly Mock<ILogger<LocalFileAudioSource>> _mockFileLogger;
  private readonly Mock<ILogger<USBInputAudioSource>> _mockUsbLogger;
  private readonly Mock<ILogger<SpotifyStreamAudioSource>> _mockSpotifyLogger;
  private readonly Mock<ILogger<TextToSpeechAudioSource>> _mockTtsLogger;
  private readonly Mock<ILogger<EventSoundAudioSource>> _mockEventLogger;

  public SoundFlowAudioSourceTests()
  {
    _mockFileLogger = new Mock<ILogger<LocalFileAudioSource>>();
    _mockUsbLogger = new Mock<ILogger<USBInputAudioSource>>();
    _mockSpotifyLogger = new Mock<ILogger<SpotifyStreamAudioSource>>();
    _mockTtsLogger = new Mock<ILogger<TextToSpeechAudioSource>>();
    _mockEventLogger = new Mock<ILogger<EventSoundAudioSource>>();

    // Create a temporary test file
    _testFilePath = Path.Combine(Path.GetTempPath(), $"test-audio-{Guid.NewGuid()}.mp3");
    File.WriteAllBytes(_testFilePath, new byte[] { 0xFF, 0xFB, 0x90, 0x00 }); // Minimal MP3 header
  }

  public void Dispose()
  {
    try
    {
      if (File.Exists(_testFilePath))
      {
        File.Delete(_testFilePath);
      }
    }
    catch
    {
      // Ignore cleanup errors
    }
  }

  #region LocalFileAudioSource Tests

  [Fact]
  public void LocalFileAudioSource_Constructor_ShouldSetProperties()
  {
    // Act
    var source = new LocalFileAudioSource("file-001", _testFilePath, MixerChannel.Main, _mockFileLogger.Object);

    // Assert
    Assert.Equal("file-001", source.Id);
    Assert.Equal(Path.GetFileName(_testFilePath), source.Name);
    Assert.Equal(MixerChannel.Main, source.Channel);
    Assert.Equal(AudioSourceType.FilePlayer, source.SourceType);
    Assert.Equal(AudioSourceStatus.Initializing, source.Status);
    Assert.Equal(1.0f, source.Volume);
    Assert.False(source.IsActive);
  }

  [Fact]
  public async Task LocalFileAudioSource_InitializeAsync_WithValidFile_ShouldSucceed()
  {
    // Arrange
    var source = new LocalFileAudioSource("file-001", _testFilePath, MixerChannel.Main, _mockFileLogger.Object);

    // Act
    await source.InitializeAsync();

    // Assert
    Assert.Equal(AudioSourceStatus.Ready, source.Status);
    Assert.NotNull(source.GetAudioStream());
  }

  [Fact]
  public async Task LocalFileAudioSource_InitializeAsync_WithNonExistentFile_ShouldThrow()
  {
    // Arrange
    var source = new LocalFileAudioSource("file-001", "/nonexistent/file.mp3", MixerChannel.Main, _mockFileLogger.Object);

    // Act & Assert
    await Assert.ThrowsAsync<FileNotFoundException>(() => source.InitializeAsync());
    Assert.Equal(AudioSourceStatus.Error, source.Status);
  }

  [Fact]
  public async Task LocalFileAudioSource_StatusChanged_ShouldFireEvent()
  {
    // Arrange
    var source = new LocalFileAudioSource("file-001", _testFilePath, MixerChannel.Main, _mockFileLogger.Object);
    var statusChanges = new List<AudioSourceStatusChangedEventArgs>();
    source.StatusChanged += (sender, args) => statusChanges.Add(args);

    // Act
    await source.InitializeAsync();
    await source.StartAsync();

    // Assert
    Assert.True(statusChanges.Count >= 2);
    Assert.Contains(statusChanges, e => e.NewStatus == AudioSourceStatus.Ready);
    Assert.Contains(statusChanges, e => e.NewStatus == AudioSourceStatus.Playing);
  }

  [Fact]
  public async Task LocalFileAudioSource_Volume_ShouldClampValues()
  {
    // Arrange
    var source = new LocalFileAudioSource("file-001", _testFilePath, MixerChannel.Main, _mockFileLogger.Object);

    // Act & Assert
    source.Volume = 1.5f;
    Assert.Equal(1.0f, source.Volume);

    source.Volume = -0.5f;
    Assert.Equal(0.0f, source.Volume);

    source.Volume = 0.5f;
    Assert.Equal(0.5f, source.Volume);
  }

  #endregion

  #region USBInputAudioSource Tests

  [Fact]
  public void USBInputAudioSource_Constructor_ShouldSetProperties()
  {
    // Act
    var source = new USBInputAudioSource("usb-001", "device-123", "Raddy RF320", MixerChannel.Main, _mockUsbLogger.Object);

    // Assert
    Assert.Equal("usb-001", source.Id);
    Assert.Equal("Raddy RF320", source.Name);
    Assert.Equal("device-123", source.DeviceId);
    Assert.Equal(MixerChannel.Main, source.Channel);
    Assert.Equal(AudioSourceType.USBRadio, source.SourceType);
  }

  [Fact]
  public async Task USBInputAudioSource_InitializeAsync_ShouldSucceed()
  {
    // Arrange
    var source = new USBInputAudioSource("usb-001", "device-123", "Raddy RF320", MixerChannel.Main, _mockUsbLogger.Object);

    // Act
    await source.InitializeAsync();

    // Assert
    Assert.Equal(AudioSourceStatus.Ready, source.Status);
    Assert.Null(source.GetAudioStream()); // USB input doesn't have a direct stream
  }

  [Fact]
  public void USBInputAudioSource_Metadata_ShouldContainDeviceInfo()
  {
    // Act
    var source = new USBInputAudioSource("usb-001", "device-123", "Raddy RF320", MixerChannel.Main, _mockUsbLogger.Object);

    // Assert
    Assert.Equal("device-123", source.Metadata["DeviceId"]);
    Assert.Equal("Raddy RF320", source.Metadata["DeviceName"]);
    Assert.Equal("USBInput", source.Metadata["SourceType"]);
  }

  #endregion

  #region SpotifyStreamAudioSource Tests

  [Fact]
  public void SpotifyStreamAudioSource_Constructor_ShouldSetProperties()
  {
    // Act
    var source = new SpotifyStreamAudioSource("spotify-001", "spotify:track:123", MixerChannel.Main, _mockSpotifyLogger.Object);

    // Assert
    Assert.Equal("spotify-001", source.Id);
    Assert.Equal("spotify:track:123", source.TrackUri);
    Assert.Equal(MixerChannel.Main, source.Channel);
    Assert.Equal(AudioSourceType.Spotify, source.SourceType);
  }

  [Theory]
  [InlineData("spotify:track:123", "Spotify Track")]
  [InlineData("spotify:playlist:abc", "Spotify Playlist")]
  [InlineData("spotify:album:xyz", "Spotify Album")]
  [InlineData("spotify:something", "Spotify Stream")]
  public void SpotifyStreamAudioSource_Constructor_ShouldDetectNameFromUri(string uri, string expectedName)
  {
    // Act
    var source = new SpotifyStreamAudioSource("spotify-001", uri, MixerChannel.Main, _mockSpotifyLogger.Object);

    // Assert
    Assert.Equal(expectedName, source.Name);
  }

  [Fact]
  public async Task SpotifyStreamAudioSource_InitializeAsync_ShouldSucceed()
  {
    // Arrange
    var source = new SpotifyStreamAudioSource("spotify-001", "spotify:track:123", MixerChannel.Main, _mockSpotifyLogger.Object);

    // Act
    await source.InitializeAsync();

    // Assert
    Assert.Equal(AudioSourceStatus.Ready, source.Status);
  }

  [Fact]
  public void SpotifyStreamAudioSource_UpdateTrackInfo_ShouldUpdateMetadata()
  {
    // Arrange
    var source = new SpotifyStreamAudioSource("spotify-001", "spotify:track:123", MixerChannel.Main, _mockSpotifyLogger.Object);

    // Act
    source.UpdateTrackInfo("Test Song", "Test Artist", "Test Album", 180000);

    // Assert
    Assert.Equal("Test Song", source.Name);
    Assert.Equal("Test Song", source.Metadata["TrackName"]);
    Assert.Equal("Test Artist", source.Metadata["ArtistName"]);
    Assert.Equal("Test Album", source.Metadata["AlbumName"]);
    Assert.Equal("180000", source.Metadata["DurationMs"]);
  }

  #endregion

  #region TextToSpeechAudioSource Tests

  [Fact]
  public void TextToSpeechAudioSource_Constructor_ShouldSetProperties()
  {
    // Act
    var source = new TextToSpeechAudioSource("tts-001", "Hello world", "male", 1.0f, null, _mockTtsLogger.Object);

    // Assert
    Assert.Equal("tts-001", source.Id);
    Assert.Equal("Hello world", source.Text);
    Assert.Equal("male", source.Voice);
    Assert.Equal(1.0f, source.Speed);
    Assert.Equal(MixerChannel.Voice, source.Channel); // TTS always routes to Voice channel
    Assert.Equal(AudioSourceType.TtsEvent, source.SourceType);
  }

  [Theory]
  [InlineData(0.3f, 0.5f)]
  [InlineData(2.5f, 2.0f)]
  [InlineData(1.0f, 1.0f)]
  public void TextToSpeechAudioSource_Constructor_ShouldClampSpeed(float input, float expected)
  {
    // Act
    var source = new TextToSpeechAudioSource("tts-001", "Test", null, input, null, _mockTtsLogger.Object);

    // Assert
    Assert.Equal(expected, source.Speed);
  }

  [Fact]
  public async Task TextToSpeechAudioSource_InitializeAsync_WithoutTtsService_ShouldThrow()
  {
    // Arrange
    var source = new TextToSpeechAudioSource("tts-001", "Hello world", null, 1.0f, null, _mockTtsLogger.Object);

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(() => source.InitializeAsync());
    Assert.Equal(AudioSourceStatus.Error, source.Status);
  }

  [Fact]
  public async Task TextToSpeechAudioSource_InitializeAsync_WithValidTtsService_ShouldSucceed()
  {
    // Arrange
    var mockTtsService = new Mock<ITextToSpeechService>();
    var audioStream = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });
    mockTtsService.Setup(s => s.SynthesizeSpeechAsync("Hello world", null, 1.0f))
      .ReturnsAsync(audioStream);

    var source = new TextToSpeechAudioSource("tts-001", "Hello world", null, 1.0f, mockTtsService.Object, _mockTtsLogger.Object);

    // Act
    await source.InitializeAsync();

    // Assert
    Assert.Equal(AudioSourceStatus.Ready, source.Status);
    Assert.NotNull(source.GetAudioStream());
  }

  #endregion

  #region EventSoundAudioSource Tests

  [Fact]
  public void EventSoundAudioSource_Constructor_ShouldSetProperties()
  {
    // Act
    var source = new EventSoundAudioSource("event-001", _testFilePath, _mockEventLogger.Object);

    // Assert
    Assert.Equal("event-001", source.Id);
    Assert.Equal(Path.GetFileName(_testFilePath), source.Name);
    Assert.Equal(MixerChannel.Event, source.Channel); // Event sounds always route to Event channel
    Assert.Equal(AudioSourceType.FileEvent, source.SourceType);
  }

  [Fact]
  public async Task EventSoundAudioSource_InitializeAsync_WithValidFile_ShouldSucceed()
  {
    // Arrange
    var source = new EventSoundAudioSource("event-001", _testFilePath, _mockEventLogger.Object);

    // Act
    await source.InitializeAsync();

    // Assert
    Assert.Equal(AudioSourceStatus.Ready, source.Status);
    Assert.NotNull(source.GetAudioStream());
  }

  [Fact]
  public async Task EventSoundAudioSource_InitializeAsync_WithNonExistentFile_ShouldThrow()
  {
    // Arrange
    var source = new EventSoundAudioSource("event-001", "/nonexistent/file.mp3", _mockEventLogger.Object);

    // Act & Assert
    await Assert.ThrowsAsync<FileNotFoundException>(() => source.InitializeAsync());
    Assert.Equal(AudioSourceStatus.Error, source.Status);
  }

  #endregion

  #region Common Source Tests

  [Fact]
  public async Task AllSources_StartAsync_WhenNotReady_ShouldNotChangeState()
  {
    // Arrange - Source is in Initializing state (not Ready)
    var source = new LocalFileAudioSource("file-001", _testFilePath, MixerChannel.Main, _mockFileLogger.Object);
    var initialStatus = source.Status;

    // Act
    await source.StartAsync();

    // Assert - Status should not change because we're not in Ready state
    Assert.Equal(initialStatus, source.Status);
  }

  [Fact]
  public async Task AllSources_PauseAsync_WhenNotPlaying_ShouldNotChangeState()
  {
    // Arrange
    var source = new LocalFileAudioSource("file-001", _testFilePath, MixerChannel.Main, _mockFileLogger.Object);
    await source.InitializeAsync();
    var initialStatus = source.Status;

    // Act
    await source.PauseAsync();

    // Assert - Status should not change because we're not playing
    Assert.Equal(initialStatus, source.Status);
  }

  [Fact]
  public async Task AllSources_ResumeAsync_WhenNotPaused_ShouldNotChangeState()
  {
    // Arrange
    var source = new LocalFileAudioSource("file-001", _testFilePath, MixerChannel.Main, _mockFileLogger.Object);
    await source.InitializeAsync();
    var initialStatus = source.Status;

    // Act
    await source.ResumeAsync();

    // Assert - Status should not change because we're not paused
    Assert.Equal(initialStatus, source.Status);
  }

  [Fact]
  public async Task AllSources_Lifecycle_ShouldTransitionCorrectly()
  {
    // Arrange
    var source = new LocalFileAudioSource("file-001", _testFilePath, MixerChannel.Main, _mockFileLogger.Object);

    // Initialize
    await source.InitializeAsync();
    Assert.Equal(AudioSourceStatus.Ready, source.Status);
    Assert.False(source.IsActive);

    // Start
    await source.StartAsync();
    Assert.Equal(AudioSourceStatus.Playing, source.Status);
    Assert.True(source.IsActive);

    // Pause
    await source.PauseAsync();
    Assert.Equal(AudioSourceStatus.Paused, source.Status);
    Assert.False(source.IsActive);

    // Resume
    await source.ResumeAsync();
    Assert.Equal(AudioSourceStatus.Playing, source.Status);
    Assert.True(source.IsActive);

    // Stop
    await source.StopAsync();
    Assert.Equal(AudioSourceStatus.Stopped, source.Status);
    Assert.False(source.IsActive);
  }

  [Fact]
  public async Task AllSources_Dispose_ShouldCleanupResources()
  {
    // Arrange
    var source = new LocalFileAudioSource("file-001", _testFilePath, MixerChannel.Main, _mockFileLogger.Object);
    await source.InitializeAsync();
    var stream = source.GetAudioStream();

    // Act
    source.Dispose();

    // Assert
    Assert.Equal(AudioSourceStatus.Stopped, source.Status);
    Assert.Throws<ObjectDisposedException>(() => stream!.ReadByte());
  }

  [Fact]
  public void AllSources_Dispose_WhenCalledMultipleTimes_ShouldNotThrow()
  {
    // Arrange
    var source = new LocalFileAudioSource("file-001", _testFilePath, MixerChannel.Main, _mockFileLogger.Object);

    // Act & Assert (should not throw)
    source.Dispose();
    source.Dispose();
  }

  #endregion
}
