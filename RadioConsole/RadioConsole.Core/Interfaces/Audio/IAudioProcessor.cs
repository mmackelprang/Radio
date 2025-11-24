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
/// Interface for format-specific audio processors using the strategy pattern.
/// </summary>
/// <remarks>
/// <para>
/// Each implementation handles a specific audio format and provides:
/// <list type="bullet">
/// <item><description>Stream reading and processing</description></item>
/// <item><description>Format-specific decoding if necessary</description></item>
/// <item><description>Metadata extraction when available</description></item>
/// </list>
/// </para>
/// <para>
/// Implementations should be stateless and thread-safe to allow concurrent usage.
/// </para>
/// </remarks>
public interface IAudioProcessor
{
  /// <summary>
  /// Gets the audio format this processor handles.
  /// </summary>
  AudioFormat SupportedFormat { get; }

  /// <summary>
  /// Gets the MIME content type for this format.
  /// </summary>
  string ContentType { get; }

  /// <summary>
  /// Determines if this processor can handle the specified format.
  /// </summary>
  /// <param name="format">The audio format to check.</param>
  /// <returns>True if this processor can handle the format; otherwise, false.</returns>
  bool CanProcess(AudioFormat format);

  /// <summary>
  /// Processes the audio stream and writes to the output stream.
  /// </summary>
  /// <param name="inputStream">The input audio stream.</param>
  /// <param name="outputStream">The output stream to write processed audio.</param>
  /// <param name="options">Processing options.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>A task representing the processing operation.</returns>
  Task ProcessAsync(
    Stream inputStream,
    Stream outputStream,
    AudioProcessingOptions? options = null,
    CancellationToken cancellationToken = default);
}

/// <summary>
/// Factory interface for creating audio processors based on detected format.
/// </summary>
/// <remarks>
/// <para>
/// The factory uses dependency injection to resolve the appropriate processor
/// based on the detected audio format. This enables easy extension for new formats.
/// </para>
/// </remarks>
public interface IAudioProcessorFactory
{
  /// <summary>
  /// Gets an audio processor for the specified format.
  /// </summary>
  /// <param name="format">The audio format to get a processor for.</param>
  /// <returns>The audio processor for the format, or null if no processor is available.</returns>
  IAudioProcessor? GetProcessor(AudioFormat format);

  /// <summary>
  /// Gets all available audio processors.
  /// </summary>
  /// <returns>Collection of all registered audio processors.</returns>
  IEnumerable<IAudioProcessor> GetAllProcessors();

  /// <summary>
  /// Gets all supported audio formats.
  /// </summary>
  /// <returns>Collection of supported audio formats.</returns>
  IEnumerable<AudioFormat> GetSupportedFormats();
}
