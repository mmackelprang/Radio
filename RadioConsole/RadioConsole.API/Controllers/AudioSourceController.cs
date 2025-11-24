using Microsoft.AspNetCore.Mvc;
using RadioConsole.Core.Interfaces.Audio;

namespace RadioConsole.API.Controllers;

/// <summary>
/// API controller for managing audio sources.
/// Provides endpoints to create and manage standard and high-priority audio sources.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AudioSourceController : ControllerBase
{
  private readonly IAudioSourceManager _sourceManager;
  private readonly ILogger<AudioSourceController> _logger;

  public AudioSourceController(
    IAudioSourceManager sourceManager,
    ILogger<AudioSourceController> logger)
  {
    _sourceManager = sourceManager ?? throw new ArgumentNullException(nameof(sourceManager));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  // ========================================
  // Standard Audio Sources
  // ========================================

  /// <summary>
  /// Create a Spotify audio source.
  /// Configuration is read from: Component=AudioSource, Category=Spotify.
  /// Required config keys: ClientID, ClientSecret, RefreshToken.
  /// </summary>
  /// <returns>The created source information.</returns>
  [HttpPost("spotify")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<CreateSourceResponse>> CreateSpotifySource()
  {
    try
    {
      _logger.LogInformation("Creating Spotify audio source");
      var sourceId = await _sourceManager.CreateSpotifySourceAsync();
      var sourceInfo = await _sourceManager.GetSourceInfoAsync(sourceId);
      return Ok(new CreateSourceResponse
      {
        SourceId = sourceId,
        Message = "Spotify source created successfully",
        SourceInfo = sourceInfo
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error creating Spotify audio source");
      return StatusCode(500, new { error = "Failed to create Spotify source", details = ex.Message });
    }
  }

  /// <summary>
  /// Create a USB Radio audio source (e.g., Raddy RF320).
  /// Configuration is read from: Component=AudioSource, Category=USBRadio.
  /// Required config key: USBPort.
  /// </summary>
  /// <returns>The created source information.</returns>
  [HttpPost("radio")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<CreateSourceResponse>> CreateRadioSource()
  {
    try
    {
      _logger.LogInformation("Creating USB Radio audio source");
      var sourceId = await _sourceManager.CreateRadioSourceAsync();
      var sourceInfo = await _sourceManager.GetSourceInfoAsync(sourceId);
      return Ok(new CreateSourceResponse
      {
        SourceId = sourceId,
        Message = "Radio source created successfully",
        SourceInfo = sourceInfo
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error creating Radio audio source");
      return StatusCode(500, new { error = "Failed to create Radio source", details = ex.Message });
    }
  }

  /// <summary>
  /// Create a Vinyl Record audio source.
  /// Configuration is read from: Component=AudioSource, Category=VinylRecord.
  /// Required config key: USBPort.
  /// </summary>
  /// <returns>The created source information.</returns>
  [HttpPost("vinyl")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<CreateSourceResponse>> CreateVinylRecordSource()
  {
    try
    {
      _logger.LogInformation("Creating Vinyl Record audio source");
      var sourceId = await _sourceManager.CreateVinylRecordSourceAsync();
      var sourceInfo = await _sourceManager.GetSourceInfoAsync(sourceId);
      return Ok(new CreateSourceResponse
      {
        SourceId = sourceId,
        Message = "Vinyl Record source created successfully",
        SourceInfo = sourceInfo
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error creating Vinyl Record audio source");
      return StatusCode(500, new { error = "Failed to create Vinyl Record source", details = ex.Message });
    }
  }

  /// <summary>
  /// Create a File Player audio source.
  /// Configuration is read from: Component=AudioSource, Category=FilePlayer.
  /// Config key: Path (can be overridden via request).
  /// </summary>
  /// <param name="request">Optional file path override.</param>
  /// <returns>The created source information.</returns>
  [HttpPost("fileplayer")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<CreateSourceResponse>> CreateFilePlayerSource([FromBody] CreateFilePlayerRequest? request = null)
  {
    try
    {
      _logger.LogInformation("Creating File Player audio source");
      var sourceId = await _sourceManager.CreateFilePlayerSourceAsync(request?.FilePath);
      var sourceInfo = await _sourceManager.GetSourceInfoAsync(sourceId);
      return Ok(new CreateSourceResponse
      {
        SourceId = sourceId,
        Message = "File Player source created successfully",
        SourceInfo = sourceInfo
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error creating File Player audio source");
      return StatusCode(500, new { error = "Failed to create File Player source", details = ex.Message });
    }
  }

  // ========================================
  // High Priority Audio Sources
  // ========================================

  /// <summary>
  /// Create a TTS (Text-to-Speech) Event audio source.
  /// Configuration is read from: Component=AudioSource, Category=TTS.
  /// Config key: TTSEngine.
  /// High priority - causes ducking of standard audio sources.
  /// </summary>
  /// <param name="request">The TTS parameters.</param>
  /// <returns>The created source information.</returns>
  [HttpPost("tts")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<CreateSourceResponse>> CreateTtsEventSource([FromBody] CreateTtsEventRequest request)
  {
    if (request == null || string.IsNullOrWhiteSpace(request.Text))
    {
      return BadRequest(new { error = "Text is required for TTS" });
    }

    try
    {
      _logger.LogInformation("Creating TTS Event audio source");
      var sourceId = await _sourceManager.CreateTtsEventSourceAsync(
        request.Text,
        request.Voice ?? "default",
        request.Speed ?? 1.0f);
      var sourceInfo = await _sourceManager.GetSourceInfoAsync(sourceId);
      return Ok(new CreateSourceResponse
      {
        SourceId = sourceId,
        Message = "TTS Event source created successfully",
        SourceInfo = sourceInfo
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error creating TTS Event audio source");
      return StatusCode(500, new { error = "Failed to create TTS Event source", details = ex.Message });
    }
  }

  /// <summary>
  /// Create a File Event audio source (doorbell, notifications, etc.).
  /// High priority - causes ducking of standard audio sources.
  /// </summary>
  /// <param name="request">The file event parameters.</param>
  /// <returns>The created source information.</returns>
  [HttpPost("fileevent")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<CreateSourceResponse>> CreateFileEventSource([FromBody] CreateFileEventRequest request)
  {
    if (request == null || string.IsNullOrWhiteSpace(request.FilePath))
    {
      return BadRequest(new { error = "File path is required" });
    }

    try
    {
      _logger.LogInformation("Creating File Event audio source");
      var sourceId = await _sourceManager.CreateFileEventSourceAsync(request.FilePath);
      var sourceInfo = await _sourceManager.GetSourceInfoAsync(sourceId);
      return Ok(new CreateSourceResponse
      {
        SourceId = sourceId,
        Message = "File Event source created successfully",
        SourceInfo = sourceInfo
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error creating File Event audio source");
      return StatusCode(500, new { error = "Failed to create File Event source", details = ex.Message });
    }
  }

  // ========================================
  // Source Management
  // ========================================

  /// <summary>
  /// Get all active audio sources.
  /// </summary>
  /// <returns>Collection of active audio sources.</returns>
  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<IEnumerable<AudioSourceInfo>>> GetActiveSources()
  {
    try
    {
      _logger.LogInformation("Retrieving active audio sources");
      var sources = await _sourceManager.GetActiveSourcesAsync();
      return Ok(sources);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving active audio sources");
      return StatusCode(500, new { error = "Failed to retrieve active sources", details = ex.Message });
    }
  }

  /// <summary>
  /// Get information about a specific audio source.
  /// </summary>
  /// <param name="sourceId">The source identifier.</param>
  /// <returns>The source information.</returns>
  [HttpGet("{sourceId}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<AudioSourceInfo>> GetSourceInfo(string sourceId)
  {
    try
    {
      _logger.LogInformation("Retrieving source info: {SourceId}", sourceId);
      var sourceInfo = await _sourceManager.GetSourceInfoAsync(sourceId);
      
      if (sourceInfo == null)
      {
        return NotFound(new { error = "Source not found", sourceId });
      }

      return Ok(sourceInfo);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving source info: {SourceId}", sourceId);
      return StatusCode(500, new { error = "Failed to retrieve source info", details = ex.Message });
    }
  }

  /// <summary>
  /// Stop and remove an audio source.
  /// </summary>
  /// <param name="sourceId">The source identifier to stop.</param>
  /// <returns>200 OK if successful.</returns>
  [HttpDelete("{sourceId}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> StopSource(string sourceId)
  {
    try
    {
      _logger.LogInformation("Stopping audio source: {SourceId}", sourceId);
      await _sourceManager.StopSourceAsync(sourceId);
      return Ok(new { message = "Source stopped successfully", sourceId });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error stopping audio source: {SourceId}", sourceId);
      return StatusCode(500, new { error = "Failed to stop source", details = ex.Message });
    }
  }

  /// <summary>
  /// Stop and remove all audio sources.
  /// </summary>
  /// <returns>200 OK if successful.</returns>
  [HttpDelete]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> StopAllSources()
  {
    try
    {
      _logger.LogInformation("Stopping all audio sources");
      await _sourceManager.StopAllSourcesAsync();
      return Ok(new { message = "All sources stopped successfully" });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error stopping all audio sources");
      return StatusCode(500, new { error = "Failed to stop all sources", details = ex.Message });
    }
  }

  // ========================================
  // Playback Control Endpoints
  // ========================================

  /// <summary>
  /// Start playing an audio source.
  /// </summary>
  /// <param name="sourceId">The source identifier to play.</param>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("{sourceId}/play")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> PlaySource(string sourceId)
  {
    try
    {
      var sourceInfo = await _sourceManager.GetSourceInfoAsync(sourceId);
      if (sourceInfo == null)
      {
        return NotFound(new { error = "Source not found", sourceId });
      }

      _logger.LogInformation("Playing audio source: {SourceId}", sourceId);
      await _sourceManager.PlaySourceAsync(sourceId);
      return Ok(new { message = "Source playback started", sourceId });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error playing audio source: {SourceId}", sourceId);
      return StatusCode(500, new { error = "Failed to play source", details = ex.Message });
    }
  }

  /// <summary>
  /// Pause an audio source.
  /// </summary>
  /// <param name="sourceId">The source identifier to pause.</param>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("{sourceId}/pause")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> PauseSource(string sourceId)
  {
    try
    {
      var sourceInfo = await _sourceManager.GetSourceInfoAsync(sourceId);
      if (sourceInfo == null)
      {
        return NotFound(new { error = "Source not found", sourceId });
      }

      _logger.LogInformation("Pausing audio source: {SourceId}", sourceId);
      await _sourceManager.PauseSourceAsync(sourceId);
      return Ok(new { message = "Source paused", sourceId });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error pausing audio source: {SourceId}", sourceId);
      return StatusCode(500, new { error = "Failed to pause source", details = ex.Message });
    }
  }

  /// <summary>
  /// Resume a paused audio source.
  /// </summary>
  /// <param name="sourceId">The source identifier to resume.</param>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("{sourceId}/resume")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> ResumeSource(string sourceId)
  {
    try
    {
      var sourceInfo = await _sourceManager.GetSourceInfoAsync(sourceId);
      if (sourceInfo == null)
      {
        return NotFound(new { error = "Source not found", sourceId });
      }

      _logger.LogInformation("Resuming audio source: {SourceId}", sourceId);
      await _sourceManager.ResumeSourceAsync(sourceId);
      return Ok(new { message = "Source resumed", sourceId });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error resuming audio source: {SourceId}", sourceId);
      return StatusCode(500, new { error = "Failed to resume source", details = ex.Message });
    }
  }
}

// ========================================
// Request/Response Models
// ========================================

/// <summary>
/// Response for source creation operations.
/// </summary>
public record CreateSourceResponse
{
  /// <summary>
  /// The ID of the created source.
  /// </summary>
  public string SourceId { get; init; } = string.Empty;

  /// <summary>
  /// Status message.
  /// </summary>
  public string Message { get; init; } = string.Empty;

  /// <summary>
  /// Detailed source information.
  /// </summary>
  public AudioSourceInfo? SourceInfo { get; init; }
}

/// <summary>
/// Request for creating a file player source.
/// </summary>
public record CreateFilePlayerRequest
{
  /// <summary>
  /// Optional file path override.
  /// </summary>
  public string? FilePath { get; init; }
}

/// <summary>
/// Request for creating a TTS event source.
/// </summary>
public record CreateTtsEventRequest
{
  /// <summary>
  /// The text to speak.
  /// </summary>
  public string Text { get; init; } = string.Empty;

  /// <summary>
  /// Optional voice to use.
  /// </summary>
  public string? Voice { get; init; }

  /// <summary>
  /// Optional speech speed (1.0 is normal).
  /// </summary>
  public float? Speed { get; init; }
}

/// <summary>
/// Request for creating a file event source.
/// </summary>
public record CreateFileEventRequest
{
  /// <summary>
  /// Path to the audio file.
  /// </summary>
  public string FilePath { get; init; } = string.Empty;
}
