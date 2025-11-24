using Microsoft.Extensions.Logging;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Implementation of audio format detection using magic bytes and other heuristics.
/// </summary>
/// <remarks>
/// <para>
/// Magic bytes reference for supported formats:
/// <list type="table">
/// <listheader>
/// <term>Format</term>
/// <description>Magic Bytes (Hex)</description>
/// </listheader>
/// <item>
/// <term>MP3</term>
/// <description>FF FB, FF F3, FF FA (MPEG frame sync) or 49 44 33 (ID3 tag)</description>
/// </item>
/// <item>
/// <term>WAV</term>
/// <description>52 49 46 46 (RIFF) followed by 57 41 56 45 (WAVE) at offset 8</description>
/// </item>
/// <item>
/// <term>FLAC</term>
/// <description>66 4C 61 43 (fLaC)</description>
/// </item>
/// <item>
/// <term>OGG</term>
/// <description>4F 67 67 53 (OggS)</description>
/// </item>
/// <item>
/// <term>AAC</term>
/// <description>FF F1 or FF F9 (ADTS frame) or 66 74 79 70 (ftyp for M4A container)</description>
/// </item>
/// <item>
/// <term>OPUS</term>
/// <description>OggS container (4F 67 67 53) with OpusHead marker</description>
/// </item>
/// </list>
/// </para>
/// </remarks>
public class AudioFormatDetector : IAudioFormatDetector
{
  private readonly ILogger<AudioFormatDetector> _logger;

  /// <summary>
  /// Minimum number of bytes needed for reliable format detection.
  /// </summary>
  private const int MinHeaderSize = 12;

  /// <summary>
  /// Extended header size for formats that require deeper inspection (like OPUS in OGG).
  /// </summary>
  private const int ExtendedHeaderSize = 36;

  // Magic bytes for format detection
  private static readonly byte[] Id3Magic = { 0x49, 0x44, 0x33 }; // "ID3"
  private static readonly byte[] RiffMagic = { 0x52, 0x49, 0x46, 0x46 }; // "RIFF"
  private static readonly byte[] WaveMagic = { 0x57, 0x41, 0x56, 0x45 }; // "WAVE"
  private static readonly byte[] FlacMagic = { 0x66, 0x4C, 0x61, 0x43 }; // "fLaC"
  private static readonly byte[] OggMagic = { 0x4F, 0x67, 0x67, 0x53 }; // "OggS"
  private static readonly byte[] FtypMagic = { 0x66, 0x74, 0x79, 0x70 }; // "ftyp" for M4A/AAC container
  private static readonly byte[] OpusHeadMagic = { 0x4F, 0x70, 0x75, 0x73, 0x48, 0x65, 0x61, 0x64 }; // "OpusHead"

  /// <summary>
  /// Initializes a new instance of the AudioFormatDetector class.
  /// </summary>
  /// <param name="logger">Logger instance for diagnostic output.</param>
  public AudioFormatDetector(ILogger<AudioFormatDetector> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <inheritdoc />
  public async Task<AudioFormatDetectionResult> DetectFormatAsync(Stream audioStream, CancellationToken cancellationToken = default)
  {
    if (audioStream == null)
      throw new ArgumentNullException(nameof(audioStream));

    if (!audioStream.CanSeek)
      throw new InvalidOperationException("Stream must support seeking for format detection.");

    var originalPosition = audioStream.Position;

    try
    {
      var headerBuffer = new byte[ExtendedHeaderSize];
      var bytesRead = await audioStream.ReadAsync(headerBuffer.AsMemory(0, ExtendedHeaderSize), cancellationToken);

      if (bytesRead == 0)
      {
        _logger.LogWarning("Empty stream provided for format detection");
        return CreateUnknownResult("Empty stream");
      }

      // Resize buffer if we read fewer bytes
      if (bytesRead < ExtendedHeaderSize)
      {
        Array.Resize(ref headerBuffer, bytesRead);
      }

      var result = DetectFormat(headerBuffer);

      _logger.LogDebug(
        "Detected audio format {Format} with confidence {Confidence:P0} using {Method}",
        result.Format,
        result.Confidence,
        result.DetectionMethod);

      return result;
    }
    finally
    {
      // Reset stream position for subsequent processing
      audioStream.Position = originalPosition;
    }
  }

  /// <inheritdoc />
  public AudioFormatDetectionResult DetectFormat(byte[] headerBytes)
  {
    if (headerBytes == null)
      throw new ArgumentNullException(nameof(headerBytes));

    if (headerBytes.Length == 0)
    {
      return CreateUnknownResult("Empty header bytes");
    }

    // Check for FLAC (highest priority due to unique signature)
    if (StartsWith(headerBytes, FlacMagic))
    {
      return CreateResult(AudioFormat.Flac, 1.0, "Magic bytes: fLaC");
    }

    // Check for WAV (RIFF + WAVE)
    if (StartsWith(headerBytes, RiffMagic) && headerBytes.Length >= 12)
    {
      if (ContainsAt(headerBytes, WaveMagic, 8))
      {
        return CreateResult(AudioFormat.Wav, 1.0, "Magic bytes: RIFF...WAVE");
      }
    }

    // Check for OGG container (may contain Vorbis or Opus)
    if (StartsWith(headerBytes, OggMagic))
    {
      // Check if it's OPUS (look for OpusHead marker)
      if (ContainsOpusHeader(headerBytes))
      {
        return CreateResult(AudioFormat.Opus, 1.0, "Magic bytes: OggS with OpusHead");
      }
      // Default to OGG Vorbis
      return CreateResult(AudioFormat.Ogg, 1.0, "Magic bytes: OggS");
    }

    // Check for ID3-tagged MP3
    if (StartsWith(headerBytes, Id3Magic))
    {
      return CreateResult(AudioFormat.Mp3, 1.0, "Magic bytes: ID3 tag");
    }

    // Check for AAC ADTS frame BEFORE MP3 since AAC uses more specific patterns
    // AAC ADTS sync: FFF1 (MPEG-4 AAC) or FFF9 (MPEG-2 AAC)
    if (IsAacAdtsFrame(headerBytes))
    {
      return CreateResult(AudioFormat.Aac, 0.95, "Magic bytes: AAC ADTS frame");
    }

    // Check for MP3 frame sync (various bit patterns)
    if (IsMp3FrameSync(headerBytes))
    {
      return CreateResult(AudioFormat.Mp3, 0.95, "Magic bytes: MP3 frame sync");
    }

    // Check for M4A/AAC container (ftyp)
    if (headerBytes.Length >= 8 && ContainsAt(headerBytes, FtypMagic, 4))
    {
      return CreateResult(AudioFormat.Aac, 1.0, "Magic bytes: ftyp (M4A container)");
    }

    _logger.LogDebug(
      "Could not detect format from header bytes: {HeaderHex}",
      BitConverter.ToString(headerBytes.Take(Math.Min(12, headerBytes.Length)).ToArray()));

    return CreateUnknownResult("No matching format signature");
  }

  /// <inheritdoc />
  public AudioFormatDetectionResult DetectFormatFromContentType(string contentType)
  {
    if (string.IsNullOrWhiteSpace(contentType))
    {
      return CreateUnknownResult("Empty Content-Type");
    }

    // Extract the main MIME type (ignore parameters like charset)
    var mimeType = contentType.Split(';')[0].Trim().ToLowerInvariant();

    var format = mimeType switch
    {
      "audio/mpeg" or "audio/mp3" => AudioFormat.Mp3,
      "audio/wav" or "audio/wave" or "audio/x-wav" => AudioFormat.Wav,
      "audio/flac" or "audio/x-flac" => AudioFormat.Flac,
      "audio/ogg" or "audio/vorbis" => AudioFormat.Ogg,
      "audio/aac" or "audio/aacp" or "audio/mp4" or "audio/x-m4a" => AudioFormat.Aac,
      "audio/opus" => AudioFormat.Opus,
      _ => (AudioFormat?)null
    };

    if (format.HasValue)
    {
      return CreateResult(format.Value, 0.8, $"Content-Type: {mimeType}");
    }

    return CreateUnknownResult($"Unknown Content-Type: {mimeType}");
  }

  /// <inheritdoc />
  public AudioFormatDetectionResult DetectFormatFromExtension(string fileExtension)
  {
    if (string.IsNullOrWhiteSpace(fileExtension))
    {
      return CreateUnknownResult("Empty file extension");
    }

    // Normalize extension (ensure it starts with dot and is lowercase)
    var ext = fileExtension.StartsWith('.') ? fileExtension.ToLowerInvariant() : $".{fileExtension.ToLowerInvariant()}";

    var format = ext switch
    {
      ".mp3" => AudioFormat.Mp3,
      ".wav" or ".wave" => AudioFormat.Wav,
      ".flac" => AudioFormat.Flac,
      ".ogg" or ".oga" => AudioFormat.Ogg,
      ".aac" or ".m4a" or ".mp4" => AudioFormat.Aac,
      ".opus" => AudioFormat.Opus,
      _ => (AudioFormat?)null
    };

    if (format.HasValue)
    {
      return CreateResult(format.Value, 0.4, $"File extension: {ext}");
    }

    return CreateUnknownResult($"Unknown extension: {ext}");
  }

  /// <inheritdoc />
  public string GetContentType(AudioFormat format)
  {
    return format switch
    {
      AudioFormat.Wav => "audio/wav",
      AudioFormat.Mp3 => "audio/mpeg",
      AudioFormat.Flac => "audio/flac",
      AudioFormat.Aac => "audio/aac",
      AudioFormat.Ogg => "audio/ogg",
      AudioFormat.Opus => "audio/opus",
      _ => "application/octet-stream"
    };
  }

  /// <inheritdoc />
  public string GetFileExtension(AudioFormat format)
  {
    return format switch
    {
      AudioFormat.Wav => ".wav",
      AudioFormat.Mp3 => ".mp3",
      AudioFormat.Flac => ".flac",
      AudioFormat.Aac => ".aac",
      AudioFormat.Ogg => ".ogg",
      AudioFormat.Opus => ".opus",
      _ => ".bin"
    };
  }

  #region Private Helper Methods

  private static bool StartsWith(byte[] data, byte[] prefix)
  {
    if (data.Length < prefix.Length)
      return false;

    for (int i = 0; i < prefix.Length; i++)
    {
      if (data[i] != prefix[i])
        return false;
    }

    return true;
  }

  private static bool ContainsAt(byte[] data, byte[] pattern, int offset)
  {
    if (data.Length < offset + pattern.Length)
      return false;

    for (int i = 0; i < pattern.Length; i++)
    {
      if (data[offset + i] != pattern[i])
        return false;
    }

    return true;
  }

  private static bool ContainsOpusHeader(byte[] headerBytes)
  {
    // OpusHead typically appears after the OGG page header (around byte 28-36)
    // We need to search for it within the extended header
    if (headerBytes.Length < 36)
      return false;

    // Search for OpusHead marker within the buffer
    for (int i = 0; i <= headerBytes.Length - OpusHeadMagic.Length; i++)
    {
      if (ContainsAt(headerBytes, OpusHeadMagic, i))
        return true;
    }

    return false;
  }

  private static bool IsMp3FrameSync(byte[] headerBytes)
  {
    if (headerBytes.Length < 2)
      return false;

    // MP3 frame sync: first byte is 0xFF
    // Second byte pattern for MP3 (MPEG Audio Layer III):
    // - 0xFF FB: MPEG1 Layer 3, 320kbps
    // - 0xFF FA: MPEG1 Layer 3, VBR
    // - 0xFF F3: MPEG2 Layer 3
    // - 0xFF F2: MPEG2.5 Layer 3
    // - 0xFF E3: MPEG2 Layer 3
    // We need to exclude AAC ADTS patterns: 0xFF F1, 0xFF F9
    if (headerBytes[0] == 0xFF)
    {
      var secondByte = headerBytes[1];

      // First, exclude AAC ADTS patterns (they start with 0xF1 or 0xF9)
      if ((secondByte & 0xF6) == 0xF0)
      {
        return false; // This is AAC ADTS, not MP3
      }

      // Check for MPEG audio frame sync (11 bits set, 1111 1111 111x xxxx)
      if ((secondByte & 0xE0) == 0xE0)
      {
        return true;
      }
    }

    return false;
  }

  private static bool IsAacAdtsFrame(byte[] headerBytes)
  {
    if (headerBytes.Length < 2)
      return false;

    // AAC ADTS frame sync: FFF1 (MPEG-4) or FFF9 (MPEG-2)
    if (headerBytes[0] == 0xFF)
    {
      var secondByte = headerBytes[1];
      // Check for ADTS sync (0xF1 or 0xF9)
      if ((secondByte & 0xF6) == 0xF0)
      {
        return true;
      }
    }

    return false;
  }

  private AudioFormatDetectionResult CreateResult(AudioFormat format, double confidence, string method)
  {
    return new AudioFormatDetectionResult
    {
      Format = format,
      Confidence = confidence,
      ContentType = GetContentType(format),
      DetectionMethod = method
    };
  }

  private static AudioFormatDetectionResult CreateUnknownResult(string reason)
  {
    return new AudioFormatDetectionResult
    {
      Format = AudioFormat.Mp3, // Default fallback
      Confidence = 0.0,
      ContentType = "application/octet-stream",
      DetectionMethod = $"Detection failed: {reason}"
    };
  }

  #endregion
}
