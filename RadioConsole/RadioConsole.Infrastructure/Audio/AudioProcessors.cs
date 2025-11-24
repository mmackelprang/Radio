using Microsoft.Extensions.Logging;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Unified audio processor that handles all supported audio formats.
/// </summary>
/// <remarks>
/// <para>
/// This processor uses the IAudioFormatDetector for format-specific operations like
/// determining MIME content types, while providing a single implementation for stream processing.
/// </para>
/// <para>
/// Supported formats: MP3, WAV, FLAC, AAC, OGG, OPUS
/// </para>
/// <para>
/// To add support for new formats:
/// <list type="number">
/// <item><description>Add the format to the AudioFormat enum</description></item>
/// <item><description>Update the IAudioFormatDetector with magic bytes and content type mappings</description></item>
/// <item><description>The AudioProcessor will automatically support the new format</description></item>
/// </list>
/// </para>
/// </remarks>
public class AudioProcessor : IAudioProcessor
{
  private readonly ILogger<AudioProcessor> _logger;
  private readonly IAudioFormatDetector _formatDetector;

  /// <summary>
  /// All audio formats supported by this processor.
  /// </summary>
  private static readonly AudioFormat[] SupportedAudioFormats =
  {
    AudioFormat.Mp3,
    AudioFormat.Wav,
    AudioFormat.Flac,
    AudioFormat.Aac,
    AudioFormat.Ogg,
    AudioFormat.Opus
  };

  /// <summary>
  /// HashSet for O(1) format lookup.
  /// </summary>
  private static readonly HashSet<AudioFormat> SupportedFormatsSet = new(SupportedAudioFormats);

  /// <summary>
  /// Initializes a new instance of the AudioProcessor class.
  /// </summary>
  /// <param name="logger">Logger for diagnostic output.</param>
  /// <param name="formatDetector">Format detector for content type resolution.</param>
  public AudioProcessor(ILogger<AudioProcessor> logger, IAudioFormatDetector formatDetector)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _formatDetector = formatDetector ?? throw new ArgumentNullException(nameof(formatDetector));

    _logger.LogInformation(
      "AudioProcessor initialized with support for {Count} formats: {Formats}",
      SupportedAudioFormats.Length,
      string.Join(", ", SupportedAudioFormats));
  }

  /// <inheritdoc />
  public IEnumerable<AudioFormat> GetSupportedFormats() => SupportedAudioFormats;

  /// <inheritdoc />
  public string GetContentType(AudioFormat format) => _formatDetector.GetContentType(format);

  /// <inheritdoc />
  public bool CanProcess(AudioFormat format) => SupportedFormatsSet.Contains(format);

  /// <inheritdoc />
  public async Task ProcessAsync(
    Stream inputStream,
    Stream outputStream,
    AudioFormat format,
    AudioProcessingOptions? options = null,
    CancellationToken cancellationToken = default)
  {
    options ??= new AudioProcessingOptions();

    var buffer = new byte[options.BufferSize];
    int bytesRead;

    _logger.LogDebug(
      "Processing {Format} audio stream with buffer size {BufferSize}",
      format,
      options.BufferSize);

    while (!cancellationToken.IsCancellationRequested)
    {
      bytesRead = await inputStream.ReadAsync(buffer, cancellationToken);

      if (bytesRead == 0)
      {
        // No more data, wait briefly and continue (for live streams)
        await Task.Delay(10, cancellationToken);
        continue;
      }

      await outputStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
      await outputStream.FlushAsync(cancellationToken);
    }
  }
}
