using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;

namespace RadioConsole.API.Services;

/// <summary>
/// Service that exposes the current audio mix as a continuous HTTP stream.
/// This enables casting audio to Google Cast devices and other streaming clients.
/// </summary>
/// <remarks>
/// <para>
/// The service provides two modes of operation:
/// <list type="bullet">
/// <item><description>Format-specific streaming: Use StreamAudioAsync with a specific format</description></item>
/// <item><description>Auto-detection: Use StreamWithAutoDetectAsync to automatically detect format from input</description></item>
/// </list>
/// </para>
/// <para>
/// Supported formats: WAV, MP3, FLAC, AAC, OGG, OPUS
/// </para>
/// </remarks>
public class StreamAudioService
{
  private readonly ILogger<StreamAudioService> _logger;
  private readonly IAudioPlayer _audioPlayer;
  private readonly IAudioFormatDetector _formatDetector;
  private readonly IAudioProcessorFactory _processorFactory;

  /// <summary>
  /// Supported audio formats based on SoundFlow library capabilities.
  /// </summary>
  public static readonly string[] SupportedFormats = { "wav", "mp3", "flac", "aac", "ogg", "opus" };

  /// <summary>
  /// Initializes a new instance of the StreamAudioService class.
  /// </summary>
  /// <param name="logger">Logger instance.</param>
  /// <param name="audioPlayer">Audio player for getting mixed output stream.</param>
  /// <param name="formatDetector">Audio format detector for auto-detection.</param>
  /// <param name="processorFactory">Factory for creating format-specific processors.</param>
  public StreamAudioService(
    ILogger<StreamAudioService> logger,
    IAudioPlayer audioPlayer,
    IAudioFormatDetector formatDetector,
    IAudioProcessorFactory processorFactory)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
    _formatDetector = formatDetector ?? throw new ArgumentNullException(nameof(formatDetector));
    _processorFactory = processorFactory ?? throw new ArgumentNullException(nameof(processorFactory));
  }

  /// <summary>
  /// Determines if the specified format is supported.
  /// </summary>
  /// <param name="format">The audio format to check.</param>
  /// <returns>True if the format is supported, false otherwise.</returns>
  public static bool IsFormatSupported(string format)
  {
    return SupportedFormats.Contains(format.ToLower());
  }

  /// <summary>
  /// Gets the content type for the specified audio format string.
  /// </summary>
  /// <param name="format">The audio format.</param>
  /// <returns>The MIME content type for the format.</returns>
  public static string GetContentType(string format)
  {
    return format.ToLower() switch
    {
      "wav" => "audio/wav",
      "mp3" => "audio/mpeg",
      "flac" => "audio/flac",
      "aac" => "audio/aac",
      "ogg" => "audio/ogg",
      "opus" => "audio/opus",
      _ => "audio/mpeg"
    };
  }

  /// <summary>
  /// Converts a string format to the AudioFormat enum.
  /// </summary>
  /// <param name="format">The format string.</param>
  /// <returns>The corresponding AudioFormat enum value.</returns>
  public static AudioFormat ParseFormat(string format)
  {
    return format.ToLower() switch
    {
      "wav" => AudioFormat.Wav,
      "mp3" => AudioFormat.Mp3,
      "flac" => AudioFormat.Flac,
      "aac" => AudioFormat.Aac,
      "ogg" => AudioFormat.Ogg,
      "opus" => AudioFormat.Opus,
      _ => AudioFormat.Mp3
    };
  }

  /// <summary>
  /// Streams the mixed audio output to the HTTP response using format auto-detection.
  /// </summary>
  /// <param name="context">The HTTP context.</param>
  /// <param name="inputStream">Optional input stream to detect format from. If null, uses mixed output stream.</param>
  /// <param name="contentTypeHint">Optional Content-Type header hint for format detection.</param>
  /// <param name="fileExtensionHint">Optional file extension hint for format detection.</param>
  /// <returns>A task representing the streaming operation.</returns>
  /// <remarks>
  /// <para>
  /// Format detection priority:
  /// <list type="number">
  /// <item><description>Magic bytes from stream header (highest confidence)</description></item>
  /// <item><description>Content-Type header if provided</description></item>
  /// <item><description>File extension if provided</description></item>
  /// <item><description>Default to MP3 format</description></item>
  /// </list>
  /// </para>
  /// </remarks>
  public async Task StreamWithAutoDetectAsync(
    HttpContext context,
    Stream? inputStream = null,
    string? contentTypeHint = null,
    string? fileExtensionHint = null)
  {
    var stream = inputStream ?? _audioPlayer.GetMixedOutputStream();
    AudioFormatDetectionResult? detectionResult = null;

    try
    {
      // Try to detect format from stream if it supports seeking
      if (stream.CanSeek)
      {
        detectionResult = await _formatDetector.DetectFormatAsync(stream, context.RequestAborted);
        _logger.LogInformation(
          "Auto-detected format {Format} with confidence {Confidence:P0} using {Method}",
          detectionResult.Format,
          detectionResult.Confidence,
          detectionResult.DetectionMethod);
      }

      // If stream detection failed or wasn't possible, try hints
      if (detectionResult == null || !detectionResult.IsSuccess)
      {
        if (!string.IsNullOrEmpty(contentTypeHint))
        {
          detectionResult = _formatDetector.DetectFormatFromContentType(contentTypeHint);
          _logger.LogDebug("Detected format from Content-Type hint: {Format}", detectionResult.Format);
        }
        else if (!string.IsNullOrEmpty(fileExtensionHint))
        {
          detectionResult = _formatDetector.DetectFormatFromExtension(fileExtensionHint);
          _logger.LogDebug("Detected format from extension hint: {Format}", detectionResult.Format);
        }
      }

      // Default to MP3 if detection failed
      var format = detectionResult?.IsSuccess == true
        ? detectionResult.Format
        : AudioFormat.Mp3;

      var contentType = _formatDetector.GetContentType(format);

      _logger.LogInformation("Starting auto-detected audio stream in {Format} format", format);

      // Get the appropriate processor
      var processor = _processorFactory.GetProcessor(format);
      if (processor == null)
      {
        _logger.LogWarning("No processor available for format {Format}, using pass-through", format);
        await StreamAudioAsync(context, format.ToString().ToLower());
        return;
      }

      // Set up response headers
      SetStreamingHeaders(context, contentType);

      // Process and stream the audio
      await processor.ProcessAsync(
        stream,
        context.Response.Body,
        new AudioProcessingOptions(),
        context.RequestAborted);

      _logger.LogInformation("Audio streaming ended (client disconnected)");
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Audio streaming cancelled by client");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during auto-detected audio streaming");
      throw;
    }
  }

  /// <summary>
  /// Unified audio streaming endpoint that handles any supported format.
  /// </summary>
  /// <param name="context">The HTTP context.</param>
  /// <param name="format">The target audio format enum.</param>
  /// <returns>A task representing the streaming operation.</returns>
  public async Task StreamAudioAsync(HttpContext context, AudioFormat format)
  {
    await StreamAudioAsync(context, format.ToString().ToLower());
  }

  /// <summary>
  /// Streams the mixed audio output to the HTTP response.
  /// </summary>
  /// <param name="context">The HTTP context.</param>
  /// <param name="format">The audio format (wav, mp3, flac, aac, ogg, or opus).</param>
  public async Task StreamAudioAsync(HttpContext context, string format = "mp3")
  {
    _logger.LogInformation("Starting audio stream in {Format} format", format);

    try
    {
      // Set appropriate content type
      var contentType = GetContentType(format);
      SetStreamingHeaders(context, contentType);

      // Get the mixed audio output stream from the audio player
      var audioStream = _audioPlayer.GetMixedOutputStream();

      // Get the appropriate processor
      var audioFormat = ParseFormat(format);
      var processor = _processorFactory.GetProcessor(audioFormat);

      if (processor != null)
      {
        // Use the processor for format-specific handling
        await processor.ProcessAsync(
          audioStream,
          context.Response.Body,
          new AudioProcessingOptions(),
          context.RequestAborted);
      }
      else
      {
        // Fallback to simple pass-through
        await StreamPassThroughAsync(audioStream, context);
      }

      _logger.LogInformation("Audio streaming ended (client disconnected)");
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Audio streaming cancelled by client");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during audio streaming");
      throw;
    }
  }

  /// <summary>
  /// Gets format detection information for the given input.
  /// </summary>
  /// <param name="inputStream">The stream to analyze.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>The detection result containing format and confidence.</returns>
  public async Task<AudioFormatDetectionResult> DetectFormatAsync(
    Stream inputStream,
    CancellationToken cancellationToken = default)
  {
    return await _formatDetector.DetectFormatAsync(inputStream, cancellationToken);
  }

  /// <summary>
  /// Gets all supported audio formats.
  /// </summary>
  /// <returns>Collection of supported audio format enum values.</returns>
  public IEnumerable<AudioFormat> GetSupportedAudioFormats()
  {
    return _processorFactory.GetSupportedFormats();
  }

  #region Legacy format-specific methods (maintained for backward compatibility)

  /// <summary>
  /// Streams audio as WAV format (uncompressed).
  /// </summary>
  [Obsolete("Use StreamAudioAsync(context, AudioFormat.Wav) instead")]
  public async Task StreamWavAsync(HttpContext context)
  {
    await StreamAudioAsync(context, "wav");
  }

  /// <summary>
  /// Streams audio as MP3 format (compressed).
  /// </summary>
  [Obsolete("Use StreamAudioAsync(context, AudioFormat.Mp3) instead")]
  public async Task StreamMp3Async(HttpContext context)
  {
    await StreamAudioAsync(context, "mp3");
  }

  /// <summary>
  /// Streams audio as FLAC format (lossless compressed).
  /// </summary>
  [Obsolete("Use StreamAudioAsync(context, AudioFormat.Flac) instead")]
  public async Task StreamFlacAsync(HttpContext context)
  {
    await StreamAudioAsync(context, "flac");
  }

  /// <summary>
  /// Streams audio as AAC format.
  /// </summary>
  [Obsolete("Use StreamAudioAsync(context, AudioFormat.Aac) instead")]
  public async Task StreamAacAsync(HttpContext context)
  {
    await StreamAudioAsync(context, "aac");
  }

  /// <summary>
  /// Streams audio as OGG format.
  /// </summary>
  [Obsolete("Use StreamAudioAsync(context, AudioFormat.Ogg) instead")]
  public async Task StreamOggAsync(HttpContext context)
  {
    await StreamAudioAsync(context, "ogg");
  }

  /// <summary>
  /// Streams audio as OPUS format.
  /// </summary>
  [Obsolete("Use StreamAudioAsync(context, AudioFormat.Opus) instead")]
  public async Task StreamOpusAsync(HttpContext context)
  {
    await StreamAudioAsync(context, "opus");
  }

  #endregion

  #region Private Helper Methods

  private static void SetStreamingHeaders(HttpContext context, string contentType)
  {
    context.Response.ContentType = contentType;
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    context.Response.Headers["Connection"] = "keep-alive";
  }

  private async Task StreamPassThroughAsync(Stream audioStream, HttpContext context)
  {
    var buffer = new byte[4096];
    int bytesRead;

    _logger.LogInformation("Streaming audio to client (pass-through mode)...");

    while (!context.RequestAborted.IsCancellationRequested)
    {
      bytesRead = await audioStream.ReadAsync(buffer, 0, buffer.Length, context.RequestAborted);

      if (bytesRead == 0)
      {
        // No more data available, wait a bit and try again
        await Task.Delay(10, context.RequestAborted);
        continue;
      }

      await context.Response.Body.WriteAsync(buffer, 0, bytesRead, context.RequestAborted);
      await context.Response.Body.FlushAsync(context.RequestAborted);
    }
  }

  #endregion
}
