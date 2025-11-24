using Microsoft.Extensions.Logging;
using Moq;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio;
using Xunit;

namespace RadioConsole.Tests.Audio;

/// <summary>
/// Unit tests for the AudioFormatDetector service.
/// Tests format detection from magic bytes, Content-Type headers, and file extensions.
/// </summary>
public class AudioFormatDetectorTests
{
  private readonly AudioFormatDetector _detector;
  private readonly Mock<ILogger<AudioFormatDetector>> _loggerMock;

  public AudioFormatDetectorTests()
  {
    _loggerMock = new Mock<ILogger<AudioFormatDetector>>();
    _detector = new AudioFormatDetector(_loggerMock.Object);
  }

  #region Magic Bytes Detection Tests

  [Fact]
  public void DetectFormat_WithFlacMagicBytes_ReturnsFlacWithHighConfidence()
  {
    // Arrange - FLAC magic bytes: "fLaC" (0x66 0x4C 0x61 0x43)
    var flacHeader = new byte[] { 0x66, 0x4C, 0x61, 0x43, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

    // Act
    var result = _detector.DetectFormat(flacHeader);

    // Assert
    Assert.Equal(AudioFormat.Flac, result.Format);
    Assert.Equal(1.0, result.Confidence);
    Assert.Equal("audio/flac", result.ContentType);
    Assert.Contains("fLaC", result.DetectionMethod);
  }

  [Fact]
  public void DetectFormat_WithWavMagicBytes_ReturnsWavWithHighConfidence()
  {
    // Arrange - WAV magic bytes: "RIFF" at 0 and "WAVE" at 8
    var wavHeader = new byte[]
    {
      0x52, 0x49, 0x46, 0x46, // "RIFF"
      0x00, 0x00, 0x00, 0x00, // file size (placeholder)
      0x57, 0x41, 0x56, 0x45  // "WAVE"
    };

    // Act
    var result = _detector.DetectFormat(wavHeader);

    // Assert
    Assert.Equal(AudioFormat.Wav, result.Format);
    Assert.Equal(1.0, result.Confidence);
    Assert.Equal("audio/wav", result.ContentType);
    Assert.Contains("RIFF", result.DetectionMethod);
  }

  [Fact]
  public void DetectFormat_WithId3TaggedMp3_ReturnsMp3WithHighConfidence()
  {
    // Arrange - ID3 tag: 0x49 0x44 0x33 ("ID3")
    var mp3Header = new byte[] { 0x49, 0x44, 0x33, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

    // Act
    var result = _detector.DetectFormat(mp3Header);

    // Assert
    Assert.Equal(AudioFormat.Mp3, result.Format);
    Assert.Equal(1.0, result.Confidence);
    Assert.Equal("audio/mpeg", result.ContentType);
    Assert.Contains("ID3", result.DetectionMethod);
  }

  [Theory]
  [InlineData(new byte[] { 0xFF, 0xFB, 0x90, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })] // MPEG Audio Layer 3
  [InlineData(new byte[] { 0xFF, 0xFA, 0x90, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })] // MPEG Audio Layer 3 VBR
  [InlineData(new byte[] { 0xFF, 0xF3, 0x90, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })] // MPEG Audio Layer 3
  public void DetectFormat_WithMp3FrameSync_ReturnsMp3WithMediumConfidence(byte[] header)
  {
    // Act
    var result = _detector.DetectFormat(header);

    // Assert
    Assert.Equal(AudioFormat.Mp3, result.Format);
    Assert.True(result.Confidence >= 0.9);
    Assert.Equal("audio/mpeg", result.ContentType);
    Assert.Contains("frame sync", result.DetectionMethod);
  }

  [Fact]
  public void DetectFormat_WithOggMagicBytes_ReturnsOggWithHighConfidence()
  {
    // Arrange - OGG magic bytes: "OggS" (0x4F 0x67 0x67 0x53)
    var oggHeader = new byte[] { 0x4F, 0x67, 0x67, 0x53, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

    // Act
    var result = _detector.DetectFormat(oggHeader);

    // Assert
    Assert.Equal(AudioFormat.Ogg, result.Format);
    Assert.Equal(1.0, result.Confidence);
    Assert.Equal("audio/ogg", result.ContentType);
    Assert.Contains("OggS", result.DetectionMethod);
  }

  [Fact]
  public void DetectFormat_WithOpusInOgg_ReturnsOpusWithHighConfidence()
  {
    // Arrange - OGG container with OpusHead marker
    var opusHeader = new byte[]
    {
      0x4F, 0x67, 0x67, 0x53, // "OggS"
      0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // OGG page header
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
      0x00, 0x01, 0x13,
      0x4F, 0x70, 0x75, 0x73, 0x48, 0x65, 0x61, 0x64  // "OpusHead"
    };

    // Act
    var result = _detector.DetectFormat(opusHeader);

    // Assert
    Assert.Equal(AudioFormat.Opus, result.Format);
    Assert.Equal(1.0, result.Confidence);
    Assert.Equal("audio/opus", result.ContentType);
    Assert.Contains("OpusHead", result.DetectionMethod);
  }

  [Theory]
  [InlineData(new byte[] { 0xFF, 0xF1, 0x50, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })] // ADTS AAC
  [InlineData(new byte[] { 0xFF, 0xF9, 0x50, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })] // ADTS AAC
  public void DetectFormat_WithAacAdtsFrame_ReturnsAacWithMediumConfidence(byte[] header)
  {
    // Act
    var result = _detector.DetectFormat(header);

    // Assert
    Assert.Equal(AudioFormat.Aac, result.Format);
    Assert.True(result.Confidence >= 0.9);
    Assert.Equal("audio/aac", result.ContentType);
  }

  [Fact]
  public void DetectFormat_WithM4aContainer_ReturnsAacWithHighConfidence()
  {
    // Arrange - M4A/AAC container with ftyp at offset 4
    var m4aHeader = new byte[]
    {
      0x00, 0x00, 0x00, 0x20, // atom size
      0x66, 0x74, 0x79, 0x70, // "ftyp"
      0x4D, 0x34, 0x41, 0x20  // "M4A "
    };

    // Act
    var result = _detector.DetectFormat(m4aHeader);

    // Assert
    Assert.Equal(AudioFormat.Aac, result.Format);
    Assert.Equal(1.0, result.Confidence);
    Assert.Equal("audio/aac", result.ContentType);
    Assert.Contains("ftyp", result.DetectionMethod);
  }

  [Fact]
  public void DetectFormat_WithEmptyBytes_ReturnsUnknownResult()
  {
    // Arrange
    var emptyHeader = Array.Empty<byte>();

    // Act
    var result = _detector.DetectFormat(emptyHeader);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(0.0, result.Confidence);
  }

  [Fact]
  public void DetectFormat_WithUnknownBytes_ReturnsUnknownResult()
  {
    // Arrange - Random bytes that don't match any known format
    var unknownHeader = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C };

    // Act
    var result = _detector.DetectFormat(unknownHeader);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(0.0, result.Confidence);
    Assert.Contains("Detection failed", result.DetectionMethod);
  }

  [Fact]
  public void DetectFormat_WithNullBytes_ThrowsArgumentNullException()
  {
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => _detector.DetectFormat(null!));
  }

  #endregion

  #region Content-Type Detection Tests

  [Theory]
  [InlineData("audio/mpeg", AudioFormat.Mp3)]
  [InlineData("audio/mp3", AudioFormat.Mp3)]
  [InlineData("audio/wav", AudioFormat.Wav)]
  [InlineData("audio/wave", AudioFormat.Wav)]
  [InlineData("audio/x-wav", AudioFormat.Wav)]
  [InlineData("audio/flac", AudioFormat.Flac)]
  [InlineData("audio/x-flac", AudioFormat.Flac)]
  [InlineData("audio/ogg", AudioFormat.Ogg)]
  [InlineData("audio/vorbis", AudioFormat.Ogg)]
  [InlineData("audio/aac", AudioFormat.Aac)]
  [InlineData("audio/aacp", AudioFormat.Aac)]
  [InlineData("audio/mp4", AudioFormat.Aac)]
  [InlineData("audio/x-m4a", AudioFormat.Aac)]
  [InlineData("audio/opus", AudioFormat.Opus)]
  public void DetectFormatFromContentType_WithValidContentType_ReturnsCorrectFormat(string contentType, AudioFormat expectedFormat)
  {
    // Act
    var result = _detector.DetectFormatFromContentType(contentType);

    // Assert
    Assert.Equal(expectedFormat, result.Format);
    Assert.True(result.IsSuccess);
    Assert.True(result.Confidence >= 0.8);
    Assert.Contains("Content-Type", result.DetectionMethod);
  }

  [Theory]
  [InlineData("audio/mpeg; charset=utf-8", AudioFormat.Mp3)]
  [InlineData("audio/wav; codecs=1", AudioFormat.Wav)]
  public void DetectFormatFromContentType_WithParameters_IgnoresParametersAndReturnsCorrectFormat(string contentType, AudioFormat expectedFormat)
  {
    // Act
    var result = _detector.DetectFormatFromContentType(contentType);

    // Assert
    Assert.Equal(expectedFormat, result.Format);
    Assert.True(result.IsSuccess);
  }

  [Theory]
  [InlineData("AUDIO/MPEG", AudioFormat.Mp3)]
  [InlineData("Audio/Wav", AudioFormat.Wav)]
  public void DetectFormatFromContentType_IsCaseInsensitive(string contentType, AudioFormat expectedFormat)
  {
    // Act
    var result = _detector.DetectFormatFromContentType(contentType);

    // Assert
    Assert.Equal(expectedFormat, result.Format);
    Assert.True(result.IsSuccess);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData(null)]
  public void DetectFormatFromContentType_WithEmptyOrNull_ReturnsUnknownResult(string contentType)
  {
    // Act
    var result = _detector.DetectFormatFromContentType(contentType);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(0.0, result.Confidence);
  }

  [Fact]
  public void DetectFormatFromContentType_WithUnknownType_ReturnsUnknownResult()
  {
    // Act
    var result = _detector.DetectFormatFromContentType("video/mp4");

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(0.0, result.Confidence);
    Assert.Contains("Unknown Content-Type", result.DetectionMethod);
  }

  #endregion

  #region File Extension Detection Tests

  [Theory]
  [InlineData(".mp3", AudioFormat.Mp3)]
  [InlineData(".wav", AudioFormat.Wav)]
  [InlineData(".wave", AudioFormat.Wav)]
  [InlineData(".flac", AudioFormat.Flac)]
  [InlineData(".ogg", AudioFormat.Ogg)]
  [InlineData(".oga", AudioFormat.Ogg)]
  [InlineData(".aac", AudioFormat.Aac)]
  [InlineData(".m4a", AudioFormat.Aac)]
  [InlineData(".mp4", AudioFormat.Aac)]
  [InlineData(".opus", AudioFormat.Opus)]
  public void DetectFormatFromExtension_WithValidExtension_ReturnsCorrectFormat(string extension, AudioFormat expectedFormat)
  {
    // Act
    var result = _detector.DetectFormatFromExtension(extension);

    // Assert
    Assert.Equal(expectedFormat, result.Format);
    Assert.True(result.IsSuccess);
    Assert.Contains("extension", result.DetectionMethod);
  }

  [Theory]
  [InlineData("mp3", AudioFormat.Mp3)]  // Without leading dot
  [InlineData("wav", AudioFormat.Wav)]
  public void DetectFormatFromExtension_WithoutLeadingDot_ReturnsCorrectFormat(string extension, AudioFormat expectedFormat)
  {
    // Act
    var result = _detector.DetectFormatFromExtension(extension);

    // Assert
    Assert.Equal(expectedFormat, result.Format);
    Assert.True(result.IsSuccess);
  }

  [Theory]
  [InlineData(".MP3", AudioFormat.Mp3)]
  [InlineData(".WAV", AudioFormat.Wav)]
  [InlineData(".Flac", AudioFormat.Flac)]
  public void DetectFormatFromExtension_IsCaseInsensitive(string extension, AudioFormat expectedFormat)
  {
    // Act
    var result = _detector.DetectFormatFromExtension(extension);

    // Assert
    Assert.Equal(expectedFormat, result.Format);
    Assert.True(result.IsSuccess);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData(null)]
  public void DetectFormatFromExtension_WithEmptyOrNull_ReturnsUnknownResult(string extension)
  {
    // Act
    var result = _detector.DetectFormatFromExtension(extension);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(0.0, result.Confidence);
  }

  [Fact]
  public void DetectFormatFromExtension_WithUnknownExtension_ReturnsUnknownResult()
  {
    // Act
    var result = _detector.DetectFormatFromExtension(".xyz");

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(0.0, result.Confidence);
    Assert.Contains("Unknown extension", result.DetectionMethod);
  }

  [Fact]
  public void DetectFormatFromExtension_HasLowerConfidenceThanMagicBytes()
  {
    // Arrange
    var mp3Header = new byte[] { 0x49, 0x44, 0x33, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

    // Act
    var magicBytesResult = _detector.DetectFormat(mp3Header);
    var extensionResult = _detector.DetectFormatFromExtension(".mp3");

    // Assert
    Assert.True(magicBytesResult.Confidence > extensionResult.Confidence);
  }

  #endregion

  #region Stream Detection Tests

  [Fact]
  public async Task DetectFormatAsync_WithSeekableStream_ReturnsFormatAndResetsPosition()
  {
    // Arrange
    var flacHeader = new byte[] { 0x66, 0x4C, 0x61, 0x43, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
    using var stream = new MemoryStream(flacHeader);

    // Act
    var result = await _detector.DetectFormatAsync(stream);

    // Assert
    Assert.Equal(AudioFormat.Flac, result.Format);
    Assert.Equal(1.0, result.Confidence);
    Assert.Equal(0, stream.Position); // Stream position should be reset
  }

  [Fact]
  public async Task DetectFormatAsync_WithEmptyStream_ReturnsUnknownResult()
  {
    // Arrange
    using var stream = new MemoryStream(Array.Empty<byte>());

    // Act
    var result = await _detector.DetectFormatAsync(stream);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(0.0, result.Confidence);
    Assert.Contains("Empty stream", result.DetectionMethod);
  }

  [Fact]
  public async Task DetectFormatAsync_WithNullStream_ThrowsArgumentNullException()
  {
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(() => _detector.DetectFormatAsync(null!));
  }

  [Fact]
  public async Task DetectFormatAsync_WithNonSeekableStream_ThrowsInvalidOperationException()
  {
    // Arrange - Create a non-seekable stream wrapper
    var nonSeekableStream = new NonSeekableStreamWrapper(new MemoryStream(new byte[12]));

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(() => _detector.DetectFormatAsync(nonSeekableStream));
  }

  [Fact]
  public async Task DetectFormatAsync_PreservesOriginalStreamPosition()
  {
    // Arrange
    var data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x66, 0x4C, 0x61, 0x43, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
    using var stream = new MemoryStream(data);
    stream.Position = 4; // Set position to somewhere in the middle

    // Act
    var result = await _detector.DetectFormatAsync(stream);

    // Assert
    Assert.Equal(4, stream.Position); // Position should be preserved
  }

  #endregion

  #region GetContentType Tests

  [Theory]
  [InlineData(AudioFormat.Wav, "audio/wav")]
  [InlineData(AudioFormat.Mp3, "audio/mpeg")]
  [InlineData(AudioFormat.Flac, "audio/flac")]
  [InlineData(AudioFormat.Aac, "audio/aac")]
  [InlineData(AudioFormat.Ogg, "audio/ogg")]
  [InlineData(AudioFormat.Opus, "audio/opus")]
  public void GetContentType_ReturnsCorrectMimeType(AudioFormat format, string expectedContentType)
  {
    // Act
    var result = _detector.GetContentType(format);

    // Assert
    Assert.Equal(expectedContentType, result);
  }

  #endregion

  #region GetFileExtension Tests

  [Theory]
  [InlineData(AudioFormat.Wav, ".wav")]
  [InlineData(AudioFormat.Mp3, ".mp3")]
  [InlineData(AudioFormat.Flac, ".flac")]
  [InlineData(AudioFormat.Aac, ".aac")]
  [InlineData(AudioFormat.Ogg, ".ogg")]
  [InlineData(AudioFormat.Opus, ".opus")]
  public void GetFileExtension_ReturnsCorrectExtension(AudioFormat format, string expectedExtension)
  {
    // Act
    var result = _detector.GetFileExtension(format);

    // Assert
    Assert.Equal(expectedExtension, result);
  }

  #endregion

  #region Helper Classes

  /// <summary>
  /// Wrapper to create a non-seekable stream for testing.
  /// </summary>
  private class NonSeekableStreamWrapper : Stream
  {
    private readonly Stream _innerStream;

    public NonSeekableStreamWrapper(Stream innerStream)
    {
      _innerStream = innerStream;
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
      get => throw new NotSupportedException();
      set => throw new NotSupportedException();
    }

    public override void Flush() => _innerStream.Flush();
    public override int Read(byte[] buffer, int offset, int count) => _innerStream.Read(buffer, offset, count);
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
  }

  #endregion
}
