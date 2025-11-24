using Microsoft.Extensions.Logging;
using Moq;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio;
using Xunit;

namespace RadioConsole.Tests.Audio;

/// <summary>
/// Unit tests for the unified AudioProcessor.
/// </summary>
public class AudioProcessorTests
{
  private readonly Mock<ILogger<AudioProcessor>> _loggerMock;
  private readonly Mock<IAudioFormatDetector> _formatDetectorMock;
  private readonly AudioProcessor _processor;

  public AudioProcessorTests()
  {
    _loggerMock = new Mock<ILogger<AudioProcessor>>();
    _formatDetectorMock = new Mock<IAudioFormatDetector>();

    // Setup format detector to return content types
    _formatDetectorMock.Setup(x => x.GetContentType(AudioFormat.Mp3)).Returns("audio/mpeg");
    _formatDetectorMock.Setup(x => x.GetContentType(AudioFormat.Wav)).Returns("audio/wav");
    _formatDetectorMock.Setup(x => x.GetContentType(AudioFormat.Flac)).Returns("audio/flac");
    _formatDetectorMock.Setup(x => x.GetContentType(AudioFormat.Aac)).Returns("audio/aac");
    _formatDetectorMock.Setup(x => x.GetContentType(AudioFormat.Ogg)).Returns("audio/ogg");
    _formatDetectorMock.Setup(x => x.GetContentType(AudioFormat.Opus)).Returns("audio/opus");

    _processor = new AudioProcessor(_loggerMock.Object, _formatDetectorMock.Object);
  }

  [Fact]
  public void GetSupportedFormats_ReturnsAllFormats()
  {
    // Act
    var formats = _processor.GetSupportedFormats().ToList();

    // Assert
    Assert.Equal(6, formats.Count);
    Assert.Contains(AudioFormat.Mp3, formats);
    Assert.Contains(AudioFormat.Wav, formats);
    Assert.Contains(AudioFormat.Flac, formats);
    Assert.Contains(AudioFormat.Aac, formats);
    Assert.Contains(AudioFormat.Ogg, formats);
    Assert.Contains(AudioFormat.Opus, formats);
  }

  [Theory]
  [InlineData(AudioFormat.Mp3, "audio/mpeg")]
  [InlineData(AudioFormat.Wav, "audio/wav")]
  [InlineData(AudioFormat.Flac, "audio/flac")]
  [InlineData(AudioFormat.Aac, "audio/aac")]
  [InlineData(AudioFormat.Ogg, "audio/ogg")]
  [InlineData(AudioFormat.Opus, "audio/opus")]
  public void GetContentType_ReturnsCorrectContentType(AudioFormat format, string expectedContentType)
  {
    // Act
    var contentType = _processor.GetContentType(format);

    // Assert
    Assert.Equal(expectedContentType, contentType);
  }

  [Theory]
  [InlineData(AudioFormat.Mp3)]
  [InlineData(AudioFormat.Wav)]
  [InlineData(AudioFormat.Flac)]
  [InlineData(AudioFormat.Aac)]
  [InlineData(AudioFormat.Ogg)]
  [InlineData(AudioFormat.Opus)]
  public void CanProcess_WithSupportedFormat_ReturnsTrue(AudioFormat format)
  {
    // Act
    var canProcess = _processor.CanProcess(format);

    // Assert
    Assert.True(canProcess);
  }

  [Fact]
  public async Task ProcessAsync_WithCancellationToken_StopsProcessing()
  {
    // Arrange
    var inputData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
    using var inputStream = new MemoryStream(inputData);
    using var outputStream = new MemoryStream();
    using var cts = new CancellationTokenSource();

    // Cancel immediately
    cts.Cancel();

    // Act & Assert - should not throw, just stop
    await _processor.ProcessAsync(inputStream, outputStream, AudioFormat.Mp3, cancellationToken: cts.Token);
  }

  [Fact]
  public async Task ProcessAsync_WithOptions_UsesProvidedBufferSize()
  {
    // Arrange
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
      await _processor.ProcessAsync(inputStream, outputStream, AudioFormat.Mp3, options, cts.Token);
    }
    catch (OperationCanceledException)
    {
      // Expected
    }

    // Assert - data should have been written
    Assert.True(outputStream.Length > 0);
  }

  [Fact]
  public void Constructor_WithNullLogger_ThrowsArgumentNullException()
  {
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => new AudioProcessor(null!, _formatDetectorMock.Object));
  }

  [Fact]
  public void Constructor_WithNullFormatDetector_ThrowsArgumentNullException()
  {
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => new AudioProcessor(_loggerMock.Object, null!));
  }
}
