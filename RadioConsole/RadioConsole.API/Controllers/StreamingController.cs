using Microsoft.AspNetCore.Mvc;
using RadioConsole.API.Services;
using RadioConsole.Core.Interfaces.Audio;

namespace RadioConsole.API.Controllers;

/// <summary>
/// Controller for audio streaming endpoints.
/// Provides HTTP streaming of audio in various formats using unified format handling.
/// </summary>
/// <remarks>
/// <para>
/// Supported formats: WAV, MP3, FLAC, AAC, OGG, OPUS.
/// </para>
/// <para>
/// Format detection methods (in priority order):
/// <list type="number">
/// <item><description>Query parameter ?format=xxx (e.g., ?format=mp3)</description></item>
/// <item><description>Magic bytes/file signatures from stream header</description></item>
/// <item><description>Content-Type headers from HTTP requests</description></item>
/// <item><description>Default to MP3 format</description></item>
/// </list>
/// </para>
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class StreamingController : ControllerBase
{
  private readonly StreamAudioService _streamService;
  private readonly IAudioFormatDetector _formatDetector;
  private readonly ILogger<StreamingController> _logger;

  /// <summary>
  /// Initializes a new instance of the StreamingController class.
  /// </summary>
  /// <param name="streamService">Audio streaming service.</param>
  /// <param name="formatDetector">Audio format detector.</param>
  /// <param name="logger">Logger instance.</param>
  public StreamingController(
    StreamAudioService streamService,
    IAudioFormatDetector formatDetector,
    ILogger<StreamingController> logger)
  {
    _streamService = streamService ?? throw new ArgumentNullException(nameof(streamService));
    _formatDetector = formatDetector ?? throw new ArgumentNullException(nameof(formatDetector));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <summary>
  /// Unified audio streaming endpoint that handles any supported format.
  /// </summary>
  /// <param name="format">
  /// The audio format to stream. Supported values: wav, mp3, flac, aac, ogg, opus.
  /// Default is mp3 if not specified.
  /// </param>
  /// <returns>Audio stream in the requested format.</returns>
  /// <response code="200">Returns the audio stream.</response>
  /// <response code="400">If the format is not supported.</response>
  [HttpGet("stream")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task StreamAsync([FromQuery] string format = "mp3")
  {
    if (!StreamAudioService.IsFormatSupported(format))
    {
      _logger.LogWarning("Unsupported format requested: {Format}", format);
      Response.StatusCode = StatusCodes.Status400BadRequest;
      await Response.WriteAsJsonAsync(new
      {
        error = "Unsupported format",
        requestedFormat = format,
        supportedFormats = StreamAudioService.SupportedFormats
      });
      return;
    }

    _logger.LogInformation("Audio stream requested with format: {Format}", format);
    await _streamService.StreamAudioAsync(HttpContext, format);
  }

  /// <summary>
  /// Stream audio with automatic format detection from input stream.
  /// </summary>
  /// <param name="contentType">Optional Content-Type hint for format detection.</param>
  /// <returns>Audio stream with auto-detected format.</returns>
  /// <response code="200">Returns the audio stream.</response>
  [HttpGet("stream/auto")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task StreamAutoAsync([FromQuery] string? contentType = null)
  {
    _logger.LogInformation("Auto-detect stream requested");
    await _streamService.StreamWithAutoDetectAsync(HttpContext, contentTypeHint: contentType);
  }

  /// <summary>
  /// Gets information about the format detector's supported formats and capabilities.
  /// </summary>
  /// <returns>Format detection capabilities.</returns>
  [HttpGet("formats")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public ActionResult<FormatCapabilitiesResponse> GetFormatCapabilities()
  {
    var formats = _streamService.GetSupportedAudioFormats()
      .Select(f => new FormatInfo
      {
        Format = f.ToString(),
        ContentType = _formatDetector.GetContentType(f),
        FileExtension = _formatDetector.GetFileExtension(f)
      })
      .ToArray();

    return Ok(new FormatCapabilitiesResponse
    {
      SupportedFormats = formats,
      AutoDetectionAvailable = true,
      UnifiedEndpoint = $"{Request.Scheme}://{Request.Host}/api/streaming/stream",
      AutoDetectEndpoint = $"{Request.Scheme}://{Request.Host}/api/streaming/stream/auto"
    });
  }
}

/// <summary>
/// Information about a supported audio format.
/// </summary>
public record FormatInfo
{
  /// <summary>
  /// The format name.
  /// </summary>
  public string Format { get; init; } = string.Empty;

  /// <summary>
  /// The MIME content type.
  /// </summary>
  public string ContentType { get; init; } = string.Empty;

  /// <summary>
  /// The file extension.
  /// </summary>
  public string FileExtension { get; init; } = string.Empty;
}

/// <summary>
/// Response containing format detection capabilities.
/// </summary>
public record FormatCapabilitiesResponse
{
  /// <summary>
  /// Array of supported audio formats.
  /// </summary>
  public FormatInfo[] SupportedFormats { get; init; } = Array.Empty<FormatInfo>();

  /// <summary>
  /// Whether automatic format detection is available.
  /// </summary>
  public bool AutoDetectionAvailable { get; init; }

  /// <summary>
  /// URL of the unified streaming endpoint.
  /// </summary>
  public string UnifiedEndpoint { get; init; } = string.Empty;

  /// <summary>
  /// URL of the auto-detect streaming endpoint.
  /// </summary>
  public string AutoDetectEndpoint { get; init; } = string.Empty;
}
