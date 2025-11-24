using Microsoft.AspNetCore.Mvc;
using RadioConsole.API.Services;

namespace RadioConsole.API.Controllers;

/// <summary>
/// API controller for audio streaming information and endpoints.
/// The actual streaming endpoints are registered in Program.cs as minimal API endpoints.
/// This controller provides information about streaming capabilities.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StreamAudioController : ControllerBase
{
  private readonly StreamAudioService _streamService;
  private readonly ILogger<StreamAudioController> _logger;

  public StreamAudioController(StreamAudioService streamService, ILogger<StreamAudioController> logger)
  {
    _streamService = streamService ?? throw new ArgumentNullException(nameof(streamService));
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
      var info = new StreamingInfo
      {
        StreamUrl = $"{baseUrl}/api/streaming/stream",
        Description = "Real-time audio streaming endpoints for casting to external devices. Append the format extension (e.g., .mp3, .wav, .flac) to the stream URL.",
        SupportedFormats = StreamAudioService.SupportedFormats.Select(f => f.ToUpper()).ToArray(),
        FormatUrls = StreamAudioService.SupportedFormats.ToDictionary(
          f => f.ToUpper(),
          f => $"{baseUrl}/api/streaming/stream.{f}"
        )
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
      var status = new StreamingStatus
      {
        IsAvailable = true,
        Message = "Audio streaming service is available",
        SupportedFormats = StreamAudioService.SupportedFormats.Select(f => f.ToUpper()).ToArray()
      };

      return Ok(status);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving stream status");
      return StatusCode(500, new { error = "Failed to retrieve stream status", details = ex.Message });
    }
  }
}

/// <summary>
/// Information about audio streaming endpoints.
/// </summary>
public record StreamingInfo
{
  /// <summary>
  /// Base URL for audio stream. Append format extension (e.g., .mp3, .wav) to access specific format.
  /// </summary>
  public string StreamUrl { get; init; } = string.Empty;

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
}
