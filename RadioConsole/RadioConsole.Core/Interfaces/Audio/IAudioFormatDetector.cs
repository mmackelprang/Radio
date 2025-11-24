using RadioConsole.Core.Enums;

namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Result of audio format detection containing the detected format and confidence level.
/// </summary>
/// <remarks>
/// <para>
/// The confidence level indicates how certain the detection is:
/// <list type="bullet">
/// <item><description>1.0: High confidence (magic bytes match exactly)</description></item>
/// <item><description>0.5-0.9: Medium confidence (partial match or Content-Type based)</description></item>
/// <item><description>&lt;0.5: Low confidence (file extension fallback)</description></item>
/// </list>
/// </para>
/// </remarks>
public record AudioFormatDetectionResult
{
  /// <summary>
  /// The detected audio format.
  /// </summary>
  public AudioFormat Format { get; init; }

  /// <summary>
  /// Confidence level of the detection (0.0 to 1.0).
  /// </summary>
  public double Confidence { get; init; }

  /// <summary>
  /// The MIME content type for the detected format.
  /// </summary>
  public string ContentType { get; init; } = string.Empty;

  /// <summary>
  /// Human-readable description of how the format was detected.
  /// </summary>
  public string DetectionMethod { get; init; } = string.Empty;

  /// <summary>
  /// Indicates whether the format detection was successful.
  /// </summary>
  public bool IsSuccess => Confidence > 0;
}

/// <summary>
/// Service interface for detecting audio formats from streams and byte arrays.
/// </summary>
/// <remarks>
/// <para>
/// The audio format detector uses multiple approaches to identify audio formats:
/// <list type="number">
/// <item><description>Magic bytes/file signatures in the stream header</description></item>
/// <item><description>Content-Type headers from HTTP responses</description></item>
/// <item><description>File extension analysis as a fallback</description></item>
/// </list>
/// </para>
/// <para>
/// Supported format signatures:
/// <list type="bullet">
/// <item><description>MP3: starts with FF FB, FF F3, FF FA, or ID3 (0x494433)</description></item>
/// <item><description>WAV: starts with RIFF (0x52494646) followed by WAVE</description></item>
/// <item><description>FLAC: starts with fLaC (0x664C6143)</description></item>
/// <item><description>OGG: starts with OggS (0x4F676753)</description></item>
/// <item><description>AAC/M4A: various signatures including FFF1, FFF9, or ftyp</description></item>
/// <item><description>OPUS: OGG container with OpusHead signature</description></item>
/// </list>
/// </para>
/// <para>
/// To extend support for new formats:
/// <list type="number">
/// <item><description>Add the new format to the AudioFormat enum</description></item>
/// <item><description>Add the magic bytes signature to the detector implementation</description></item>
/// <item><description>Register any new audio processor in the factory</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IAudioFormatDetector
{
  /// <summary>
  /// Detects the audio format from a stream by reading the header bytes.
  /// The stream position is reset after detection so the audio can still be processed.
  /// </summary>
  /// <param name="audioStream">The audio stream to analyze. Must support seeking.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>Detection result containing the format and confidence level.</returns>
  /// <exception cref="ArgumentNullException">Thrown when audioStream is null.</exception>
  /// <exception cref="InvalidOperationException">Thrown when stream does not support seeking.</exception>
  Task<AudioFormatDetectionResult> DetectFormatAsync(Stream audioStream, CancellationToken cancellationToken = default);

  /// <summary>
  /// Detects the audio format from a byte array header.
  /// </summary>
  /// <param name="headerBytes">The header bytes to analyze. Should be at least 12 bytes for reliable detection.</param>
  /// <returns>Detection result containing the format and confidence level.</returns>
  /// <exception cref="ArgumentNullException">Thrown when headerBytes is null.</exception>
  AudioFormatDetectionResult DetectFormat(byte[] headerBytes);

  /// <summary>
  /// Detects the audio format from a Content-Type HTTP header value.
  /// </summary>
  /// <param name="contentType">The Content-Type header value (e.g., "audio/mpeg", "audio/wav").</param>
  /// <returns>Detection result containing the format and confidence level.</returns>
  AudioFormatDetectionResult DetectFormatFromContentType(string contentType);

  /// <summary>
  /// Detects the audio format from a file extension.
  /// </summary>
  /// <param name="fileExtension">The file extension including the leading dot (e.g., ".mp3", ".wav").</param>
  /// <returns>Detection result containing the format and confidence level.</returns>
  AudioFormatDetectionResult DetectFormatFromExtension(string fileExtension);

  /// <summary>
  /// Gets the MIME content type for the specified audio format.
  /// </summary>
  /// <param name="format">The audio format.</param>
  /// <returns>The MIME content type string.</returns>
  string GetContentType(AudioFormat format);

  /// <summary>
  /// Gets the file extension for the specified audio format.
  /// </summary>
  /// <param name="format">The audio format.</param>
  /// <returns>The file extension including the leading dot.</returns>
  string GetFileExtension(AudioFormat format);
}
