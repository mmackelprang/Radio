using Microsoft.AspNetCore.Mvc;
using RadioConsole.Core.Interfaces.Audio;

namespace RadioConsole.API.Controllers;

/// <summary>
/// API controller for audio file metadata extraction.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MetadataController : ControllerBase
{
  private readonly IMetadataService _metadataService;
  private readonly ILogger<MetadataController> _logger;

  public MetadataController(IMetadataService metadataService, ILogger<MetadataController> logger)
  {
    _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <summary>
  /// Extract metadata from an audio file.
  /// </summary>
  /// <param name="request">Request containing the file path.</param>
  /// <returns>Audio metadata or 404 if extraction fails.</returns>
  [HttpPost("extract")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<AudioMetadata>> ExtractMetadata([FromBody] MetadataExtractionRequest request)
  {
    if (string.IsNullOrWhiteSpace(request?.FilePath))
    {
      return BadRequest(new { error = "File path is required" });
    }

    try
    {
      if (!_metadataService.IsFormatSupported(request.FilePath))
      {
        return BadRequest(new { error = "Unsupported audio format", filePath = request.FilePath });
      }

      var metadata = await _metadataService.ExtractMetadataAsync(request.FilePath);
      if (metadata == null)
      {
        return NotFound(new { error = "Unable to extract metadata from file", filePath = request.FilePath });
      }

      _logger.LogInformation("Extracted metadata from {FilePath}", request.FilePath);
      return Ok(metadata);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error extracting metadata from {FilePath}", request.FilePath);
      return StatusCode(500, new { error = "Failed to extract metadata", details = ex.Message });
    }
  }

  /// <summary>
  /// Check if an audio file format is supported for metadata extraction.
  /// </summary>
  /// <param name="filePath">Path to the audio file.</param>
  /// <returns>Support status.</returns>
  [HttpGet("supported")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public ActionResult<FormatSupportResponse> CheckFormatSupport([FromQuery] string filePath)
  {
    if (string.IsNullOrWhiteSpace(filePath))
    {
      return BadRequest(new { error = "File path is required" });
    }

    var isSupported = _metadataService.IsFormatSupported(filePath);
    var extension = Path.GetExtension(filePath);

    return Ok(new FormatSupportResponse
    {
      FilePath = filePath,
      Extension = extension,
      IsSupported = isSupported
    });
  }

  /// <summary>
  /// Get list of supported audio formats.
  /// </summary>
  /// <returns>List of supported file extensions.</returns>
  [HttpGet("formats")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public ActionResult<SupportedFormatsResponse> GetSupportedFormats()
  {
    var formats = new[]
    {
      ".mp3", ".m4a", ".aac", ".flac", ".ogg", ".wav", ".wma",
      ".ape", ".opus", ".aiff", ".tta", ".mpc", ".wv"
    };

    return Ok(new SupportedFormatsResponse
    {
      Formats = formats,
      Description = "Audio formats supported for metadata extraction"
    });
  }
}

/// <summary>
/// Request model for metadata extraction.
/// </summary>
public record MetadataExtractionRequest
{
  /// <summary>
  /// Path to the audio file.
  /// </summary>
  public string FilePath { get; init; } = string.Empty;
}

/// <summary>
/// Response for format support check.
/// </summary>
public record FormatSupportResponse
{
  /// <summary>
  /// File path that was checked.
  /// </summary>
  public string FilePath { get; init; } = string.Empty;

  /// <summary>
  /// File extension.
  /// </summary>
  public string Extension { get; init; } = string.Empty;

  /// <summary>
  /// Whether the format is supported.
  /// </summary>
  public bool IsSupported { get; init; }
}

/// <summary>
/// Response for supported formats list.
/// </summary>
public record SupportedFormatsResponse
{
  /// <summary>
  /// List of supported file extensions.
  /// </summary>
  public string[] Formats { get; init; } = Array.Empty<string>();

  /// <summary>
  /// Description of supported formats.
  /// </summary>
  public string Description { get; init; } = string.Empty;
}
