using Microsoft.AspNetCore.Mvc;
using RadioConsole.API.Services;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;

namespace RadioConsole.API.Controllers;

/// <summary>
/// API controller for audio streaming information and endpoints.
/// Provides information about streaming capabilities and unified format handling.
/// </summary>
/// <remarks>
/// <para>
/// This controller provides:
/// <list type="bullet">
/// <item><description>Information about available streaming endpoints</description></item>
/// <item><description>Supported format details with auto-detection capabilities</description></item>
/// <item><description>Status of the streaming service</description></item>
/// </list>
/// </para>
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class StreamAudioController : ControllerBase
{
  private readonly StreamAudioService _streamService;
  private readonly IAudioFormatDetector _formatDetector;
  private readonly IAudioProcessorFactory _processorFactory;
  private readonly ILogger<StreamAudioController> _logger;

  /// <summary>
  /// Initializes a new instance of the StreamAudioController class.
  /// </summary>
  /// <param name="streamService">Audio streaming service.</param>
  /// <param name="formatDetector">Audio format detector.</param>
  /// <param name="processorFactory">Audio processor factory.</param>
  /// <param name="logger">Logger instance.</param>
  public StreamAudioController(
    StreamAudioService streamService,
    IAudioFormatDetector formatDetector,
    IAudioProcessorFactory processorFactory,
    ILogger<StreamAudioController> logger)
  {
    _streamService = streamService ?? throw new ArgumentNullException(nameof(streamService));
    _formatDetector = formatDetector ?? throw new ArgumentNullException(nameof(formatDetector));
    _processorFactory = processorFactory ?? throw new ArgumentNullException(nameof(processorFactory));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <summary>
  /// Get information about available audio streaming endpoints.
  /// </summary>
  /// <returns>Streaming endpoint information.</returns>
  [HttpGet("info")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public ActionResult<StreamingInfo> GetInfo()
  {
    try
    {
      var baseUrl = $"{Request.Scheme}://{Request.Host}";
      var supportedFormats = _processorFactory.GetSupportedFormats().ToArray();

      var info = new StreamingInfo
      {
        StreamUrl = $"{baseUrl}/api/streaming/stream",
        AutoDetectUrl = $"{baseUrl}/api/streaming/stream/auto",
        Description = "Real-time audio streaming endpoints with unified format handling. " +
                      "Use the unified endpoint with ?format=xxx parameter, or use format-specific " +
                      "endpoints (e.g., stream.mp3, stream.wav). Auto-detection is also available.",
        SupportedFormats = supportedFormats.Select(f => f.ToString().ToUpper()).ToArray(),
        FormatUrls = supportedFormats.ToDictionary(
          f => f.ToString().ToUpper(),
          f => $"{baseUrl}/api/streaming/stream.{f.ToString().ToLower()}"
        ),
        FormatDetails = supportedFormats.Select(f => new FormatDetail
        {
          Format = f.ToString().ToUpper(),
          ContentType = _formatDetector.GetContentType(f),
          FileExtension = _formatDetector.GetFileExtension(f),
          StreamUrl = $"{baseUrl}/api/streaming/stream.{f.ToString().ToLower()}"
        }).ToArray(),
        UnifiedEndpointExample = $"{baseUrl}/api/streaming/stream?format=mp3"
      };

      return Ok(info);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving stream information");
      return StatusCode(500, new { error = "Failed to retrieve stream information", details = ex.Message });
    }
  }

  /// <summary>
  /// Get the status of the audio streaming service.
  /// </summary>
  /// <returns>Streaming service status.</returns>
  [HttpGet("status")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public ActionResult<StreamingStatus> GetStatus()
  {
    try
    {
      var supportedFormats = _processorFactory.GetSupportedFormats().ToArray();
      var processors = _processorFactory.GetAllProcessors().ToArray();

      var status = new StreamingStatus
      {
        IsAvailable = true,
        Message = "Audio streaming service is available with unified format handling",
        SupportedFormats = supportedFormats.Select(f => f.ToString().ToUpper()).ToArray(),
        AutoDetectionEnabled = true,
        ProcessorCount = processors.Length,
        Capabilities = new StreamingCapabilities
        {
          SupportsAutoDetection = true,
          SupportsUnifiedEndpoint = true,
          SupportedDetectionMethods = new[]
          {
            "Magic bytes/file signatures",
            "Content-Type headers",
            "File extension analysis"
          }
        }
      };

      return Ok(status);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving stream status");
      return StatusCode(500, new { error = "Failed to retrieve stream status", details = ex.Message });
    }
  }

  /// <summary>
  /// Detect the format of audio data from header bytes.
  /// </summary>
  /// <param name="request">Request containing header bytes to analyze.</param>
  /// <returns>Format detection result.</returns>
  [HttpPost("detect")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public ActionResult<FormatDetectionResponse> DetectFormat([FromBody] FormatDetectionRequest request)
  {
    try
    {
      if (request.HeaderBytes == null || request.HeaderBytes.Length == 0)
      {
        return BadRequest(new { error = "Header bytes are required for format detection" });
      }

      var result = _formatDetector.DetectFormat(request.HeaderBytes);

      return Ok(new FormatDetectionResponse
      {
        Format = result.Format.ToString(),
        Confidence = result.Confidence,
        ContentType = result.ContentType,
        DetectionMethod = result.DetectionMethod,
        IsSuccess = result.IsSuccess
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error detecting audio format");
      return StatusCode(500, new { error = "Failed to detect format", details = ex.Message });
    }
  }
}

/// <summary>
/// Information about audio streaming endpoints.
/// </summary>
public record StreamingInfo
{
  /// <summary>
  /// Base URL for unified audio stream. Use with ?format=xxx query parameter.
  /// </summary>
  public string StreamUrl { get; init; } = string.Empty;

  /// <summary>
  /// URL for auto-detect streaming endpoint.
  /// </summary>
  public string AutoDetectUrl { get; init; } = string.Empty;

  /// <summary>
  /// Description of the streaming service.
  /// </summary>
  public string Description { get; init; } = string.Empty;

  /// <summary>
  /// Supported audio formats.
  /// </summary>
  public string[] SupportedFormats { get; init; } = Array.Empty<string>();

  /// <summary>
  /// Dictionary of format names to their streaming URLs.
  /// </summary>
  public Dictionary<string, string> FormatUrls { get; init; } = new();

  /// <summary>
  /// Detailed information about each supported format.
  /// </summary>
  public FormatDetail[] FormatDetails { get; init; } = Array.Empty<FormatDetail>();

  /// <summary>
  /// Example URL for using the unified endpoint.
  /// </summary>
  public string UnifiedEndpointExample { get; init; } = string.Empty;
}

/// <summary>
/// Detailed information about a supported audio format.
/// </summary>
public record FormatDetail
{
  /// <summary>
  /// Format name.
  /// </summary>
  public string Format { get; init; } = string.Empty;

  /// <summary>
  /// MIME content type.
  /// </summary>
  public string ContentType { get; init; } = string.Empty;

  /// <summary>
  /// File extension.
  /// </summary>
  public string FileExtension { get; init; } = string.Empty;

  /// <summary>
  /// Direct stream URL for this format.
  /// </summary>
  public string StreamUrl { get; init; } = string.Empty;
}

/// <summary>
/// Status of the audio streaming service.
/// </summary>
public record StreamingStatus
{
  /// <summary>
  /// Whether the streaming service is available.
  /// </summary>
  public bool IsAvailable { get; init; }

  /// <summary>
  /// Status message.
  /// </summary>
  public string Message { get; init; } = string.Empty;

  /// <summary>
  /// Supported audio formats.
  /// </summary>
  public string[] SupportedFormats { get; init; } = Array.Empty<string>();

  /// <summary>
  /// Whether automatic format detection is enabled.
  /// </summary>
  public bool AutoDetectionEnabled { get; init; }

  /// <summary>
  /// Number of registered audio processors.
  /// </summary>
  public int ProcessorCount { get; init; }

  /// <summary>
  /// Streaming capabilities.
  /// </summary>
  public StreamingCapabilities Capabilities { get; init; } = new();
}

/// <summary>
/// Streaming service capabilities.
/// </summary>
public record StreamingCapabilities
{
  /// <summary>
  /// Whether auto-detection is supported.
  /// </summary>
  public bool SupportsAutoDetection { get; init; }

  /// <summary>
  /// Whether unified endpoint is supported.
  /// </summary>
  public bool SupportsUnifiedEndpoint { get; init; }

  /// <summary>
  /// Supported detection methods.
  /// </summary>
  public string[] SupportedDetectionMethods { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Request for format detection.
/// </summary>
public record FormatDetectionRequest
{
  /// <summary>
  /// Header bytes to analyze for format detection.
  /// Should be at least 12 bytes for reliable detection.
  /// </summary>
  public byte[] HeaderBytes { get; init; } = Array.Empty<byte>();
}

/// <summary>
/// Response from format detection.
/// </summary>
public record FormatDetectionResponse
{
  /// <summary>
  /// Detected format name.
  /// </summary>
  public string Format { get; init; } = string.Empty;

  /// <summary>
  /// Confidence level (0.0 to 1.0).
  /// </summary>
  public double Confidence { get; init; }

  /// <summary>
  /// MIME content type for the detected format.
  /// </summary>
  public string ContentType { get; init; } = string.Empty;

  /// <summary>
  /// Method used for detection.
  /// </summary>
  public string DetectionMethod { get; init; } = string.Empty;

  /// <summary>
  /// Whether detection was successful.
  /// </summary>
  public bool IsSuccess { get; init; }
}
