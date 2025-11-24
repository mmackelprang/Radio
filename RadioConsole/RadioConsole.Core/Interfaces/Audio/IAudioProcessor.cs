using RadioConsole.Core.Enums;

namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Audio processing options for stream handling.
/// </summary>
public record AudioProcessingOptions
{
  /// <summary>
  /// Buffer size for streaming operations.
  /// </summary>
  public int BufferSize { get; init; } = 4096;

  /// <summary>
  /// Sample rate for audio conversion if needed.
  /// </summary>
  public int SampleRate { get; init; } = 44100;

  /// <summary>
  /// Number of audio channels.
  /// </summary>
  public int Channels { get; init; } = 2;

  /// <summary>
  /// Bit depth for audio processing.
  /// </summary>
  public int BitDepth { get; init; } = 16;
}

/// <summary>
/// Unified interface for audio stream processing.
/// </summary>
/// <remarks>
/// <para>
/// The audio processor handles all audio formats through a single implementation,
/// using the IAudioFormatDetector to determine content types and format-specific behavior.
/// </para>
/// <para>
/// Implementations should be stateless and thread-safe to allow concurrent usage.
/// </para>
/// </remarks>
public interface IAudioProcessor
{
  /// <summary>
  /// Gets all supported audio formats.
  /// </summary>
  /// <returns>Collection of supported audio formats.</returns>
  IEnumerable<AudioFormat> GetSupportedFormats();

  /// <summary>
  /// Gets the MIME content type for the specified format.
  /// </summary>
  /// <param name="format">The audio format.</param>
  /// <returns>The MIME content type string.</returns>
  string GetContentType(AudioFormat format);

  /// <summary>
  /// Determines if the specified format is supported.
  /// </summary>
  /// <param name="format">The audio format to check.</param>
  /// <returns>True if the format is supported; otherwise, false.</returns>
  bool CanProcess(AudioFormat format);

  /// <summary>
  /// Processes the audio stream and writes to the output stream.
  /// </summary>
  /// <param name="inputStream">The input audio stream.</param>
  /// <param name="outputStream">The output stream to write processed audio.</param>
  /// <param name="format">The audio format being processed.</param>
  /// <param name="options">Processing options.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>A task representing the processing operation.</returns>
  Task ProcessAsync(
    Stream inputStream,
    Stream outputStream,
    AudioFormat format,
    AudioProcessingOptions? options = null,
    CancellationToken cancellationToken = default);
}
