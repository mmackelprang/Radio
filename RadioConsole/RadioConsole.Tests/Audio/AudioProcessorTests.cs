using Microsoft.Extensions.Logging;
using Moq;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio;
using Xunit;

namespace RadioConsole.Tests.Audio;

/// <summary>
/// Unit tests for audio processors and the AudioProcessorFactory.
/// </summary>
public class AudioProcessorTests
{
  #region Mp3AudioProcessor Tests

  [Fact]
  public void Mp3AudioProcessor_SupportedFormat_ReturnsMp3()
  {
    // Arrange
    var logger = new Mock<ILogger<Mp3AudioProcessor>>();
    var processor = new Mp3AudioProcessor(logger.Object);

    // Assert
    Assert.Equal(AudioFormat.Mp3, processor.SupportedFormat);
    Assert.Equal("audio/mpeg", processor.ContentType);
  }

  [Fact]
  public void Mp3AudioProcessor_CanProcess_ReturnsTrueForMp3()
  {
    // Arrange
    var logger = new Mock<ILogger<Mp3AudioProcessor>>();
    var processor = new Mp3AudioProcessor(logger.Object);

    // Act & Assert
    Assert.True(processor.CanProcess(AudioFormat.Mp3));
    Assert.False(processor.CanProcess(AudioFormat.Wav));
  }

  #endregion

  #region WavAudioProcessor Tests

  [Fact]
  public void WavAudioProcessor_SupportedFormat_ReturnsWav()
  {
    // Arrange
    var logger = new Mock<ILogger<WavAudioProcessor>>();
    var processor = new WavAudioProcessor(logger.Object);

    // Assert
    Assert.Equal(AudioFormat.Wav, processor.SupportedFormat);
    Assert.Equal("audio/wav", processor.ContentType);
  }

  [Fact]
  public void WavAudioProcessor_CanProcess_ReturnsTrueForWav()
  {
    // Arrange
    var logger = new Mock<ILogger<WavAudioProcessor>>();
    var processor = new WavAudioProcessor(logger.Object);

    // Act & Assert
    Assert.True(processor.CanProcess(AudioFormat.Wav));
    Assert.False(processor.CanProcess(AudioFormat.Mp3));
  }

  #endregion

  #region FlacAudioProcessor Tests

  [Fact]
  public void FlacAudioProcessor_SupportedFormat_ReturnsFlac()
  {
    // Arrange
    var logger = new Mock<ILogger<FlacAudioProcessor>>();
    var processor = new FlacAudioProcessor(logger.Object);

    // Assert
    Assert.Equal(AudioFormat.Flac, processor.SupportedFormat);
    Assert.Equal("audio/flac", processor.ContentType);
  }

  #endregion

  #region AacAudioProcessor Tests

  [Fact]
  public void AacAudioProcessor_SupportedFormat_ReturnsAac()
  {
    // Arrange
    var logger = new Mock<ILogger<AacAudioProcessor>>();
    var processor = new AacAudioProcessor(logger.Object);

    // Assert
    Assert.Equal(AudioFormat.Aac, processor.SupportedFormat);
    Assert.Equal("audio/aac", processor.ContentType);
  }

  #endregion

  #region OggAudioProcessor Tests

  [Fact]
  public void OggAudioProcessor_SupportedFormat_ReturnsOgg()
  {
    // Arrange
    var logger = new Mock<ILogger<OggAudioProcessor>>();
    var processor = new OggAudioProcessor(logger.Object);

    // Assert
    Assert.Equal(AudioFormat.Ogg, processor.SupportedFormat);
    Assert.Equal("audio/ogg", processor.ContentType);
  }

  #endregion

  #region OpusAudioProcessor Tests

  [Fact]
  public void OpusAudioProcessor_SupportedFormat_ReturnsOpus()
  {
    // Arrange
    var logger = new Mock<ILogger<OpusAudioProcessor>>();
    var processor = new OpusAudioProcessor(logger.Object);

    // Assert
    Assert.Equal(AudioFormat.Opus, processor.SupportedFormat);
    Assert.Equal("audio/opus", processor.ContentType);
  }

  #endregion

  #region ProcessAsync Tests

  [Fact]
  public async Task ProcessAsync_WithCancellationToken_StopsProcessing()
  {
    // Arrange
    var logger = new Mock<ILogger<Mp3AudioProcessor>>();
    var processor = new Mp3AudioProcessor(logger.Object);
    var inputData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
    using var inputStream = new MemoryStream(inputData);
    using var outputStream = new MemoryStream();
    using var cts = new CancellationTokenSource();
    
    // Cancel immediately
    cts.Cancel();

    // Act & Assert - should not throw, just stop
    await processor.ProcessAsync(inputStream, outputStream, cancellationToken: cts.Token);
  }

  [Fact]
  public async Task ProcessAsync_WithOptions_UsesProvidedBufferSize()
  {
    // Arrange
    var logger = new Mock<ILogger<Mp3AudioProcessor>>();
    var processor = new Mp3AudioProcessor(logger.Object);
    var inputData = new byte[1024];
    Array.Fill<byte>(inputData, 0xAB);
    using var inputStream = new MemoryStream(inputData);
    using var outputStream = new MemoryStream();
    using var cts = new CancellationTokenSource(100); // Short timeout

    var options = new AudioProcessingOptions
    {
      BufferSize = 512
    };

    // Act
    try
    {
      await processor.ProcessAsync(inputStream, outputStream, options, cts.Token);
    }
    catch (OperationCanceledException)
    {
      // Expected
    }

    // Assert - data should have been written
    Assert.True(outputStream.Length > 0);
  }

  #endregion
}

/// <summary>
/// Unit tests for the AudioProcessorFactory.
/// </summary>
public class AudioProcessorFactoryTests
{
  private readonly Mock<ILogger<AudioProcessorFactory>> _factoryLoggerMock;
  private readonly List<IAudioProcessor> _processors;
  private readonly AudioProcessorFactory _factory;

  public AudioProcessorFactoryTests()
  {
    _factoryLoggerMock = new Mock<ILogger<AudioProcessorFactory>>();

    _processors = new List<IAudioProcessor>
    {
      new Mp3AudioProcessor(new Mock<ILogger<Mp3AudioProcessor>>().Object),
      new WavAudioProcessor(new Mock<ILogger<WavAudioProcessor>>().Object),
      new FlacAudioProcessor(new Mock<ILogger<FlacAudioProcessor>>().Object),
      new AacAudioProcessor(new Mock<ILogger<AacAudioProcessor>>().Object),
      new OggAudioProcessor(new Mock<ILogger<OggAudioProcessor>>().Object),
      new OpusAudioProcessor(new Mock<ILogger<OpusAudioProcessor>>().Object)
    };

    _factory = new AudioProcessorFactory(_processors, _factoryLoggerMock.Object);
  }

  [Fact]
  public void Constructor_RegistersAllProcessors()
  {
    // Assert
    var supportedFormats = _factory.GetSupportedFormats().ToList();
    Assert.Equal(6, supportedFormats.Count);
    Assert.Contains(AudioFormat.Mp3, supportedFormats);
    Assert.Contains(AudioFormat.Wav, supportedFormats);
    Assert.Contains(AudioFormat.Flac, supportedFormats);
    Assert.Contains(AudioFormat.Aac, supportedFormats);
    Assert.Contains(AudioFormat.Ogg, supportedFormats);
    Assert.Contains(AudioFormat.Opus, supportedFormats);
  }

  [Theory]
  [InlineData(AudioFormat.Mp3)]
  [InlineData(AudioFormat.Wav)]
  [InlineData(AudioFormat.Flac)]
  [InlineData(AudioFormat.Aac)]
  [InlineData(AudioFormat.Ogg)]
  [InlineData(AudioFormat.Opus)]
  public void GetProcessor_WithSupportedFormat_ReturnsProcessor(AudioFormat format)
  {
    // Act
    var processor = _factory.GetProcessor(format);

    // Assert
    Assert.NotNull(processor);
    Assert.Equal(format, processor.SupportedFormat);
  }

  [Fact]
  public void GetAllProcessors_ReturnsAllRegisteredProcessors()
  {
    // Act
    var allProcessors = _factory.GetAllProcessors().ToList();

    // Assert
    Assert.Equal(6, allProcessors.Count);
  }

  [Fact]
  public void GetSupportedFormats_ReturnsAllSupportedFormats()
  {
    // Act
    var formats = _factory.GetSupportedFormats().ToList();

    // Assert
    Assert.Equal(6, formats.Count);
  }

  [Fact]
  public void Constructor_WithDuplicateProcessors_KeepsFirst()
  {
    // Arrange
    var duplicateProcessors = new List<IAudioProcessor>
    {
      new Mp3AudioProcessor(new Mock<ILogger<Mp3AudioProcessor>>().Object),
      new Mp3AudioProcessor(new Mock<ILogger<Mp3AudioProcessor>>().Object) // Duplicate
    };

    // Act
    var factory = new AudioProcessorFactory(duplicateProcessors, _factoryLoggerMock.Object);
    var formats = factory.GetSupportedFormats().ToList();

    // Assert - should only have one Mp3 processor
    Assert.Single(formats);
    Assert.Contains(AudioFormat.Mp3, formats);
  }

  [Fact]
  public void Constructor_WithEmptyProcessors_CreatesEmptyFactory()
  {
    // Arrange
    var emptyProcessors = new List<IAudioProcessor>();

    // Act
    var factory = new AudioProcessorFactory(emptyProcessors, _factoryLoggerMock.Object);
    var formats = factory.GetSupportedFormats().ToList();

    // Assert
    Assert.Empty(formats);
  }

  [Fact]
  public void Constructor_WithNullLogger_ThrowsArgumentNullException()
  {
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => new AudioProcessorFactory(_processors, null!));
  }
}
