using Microsoft.AspNetCore.Mvc;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;

namespace RadioConsole.API.Controllers;

/// <summary>
/// API controller for managing audio priority and ducking behavior.
/// Provides endpoints to control audio source priorities and ducking configuration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AudioPriorityController : ControllerBase
{
  private readonly IAudioPriorityService _priorityService;
  private readonly ILogger<AudioPriorityController> _logger;

  public AudioPriorityController(
    IAudioPriorityService priorityService,
    ILogger<AudioPriorityController> logger)
  {
    _priorityService = priorityService ?? throw new ArgumentNullException(nameof(priorityService));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <summary>
  /// Register an audio source with a specific priority level.
  /// </summary>
  /// <param name="request">Request containing source ID and priority.</param>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("sources/register")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> RegisterSource([FromBody] RegisterSourceRequest request)
  {
    if (string.IsNullOrWhiteSpace(request?.SourceId))
    {
      return BadRequest(new { error = "Source ID is required" });
    }

    try
    {
      _logger.LogInformation("Registering audio source {SourceId} with priority {Priority}", 
        request.SourceId, request.Priority);
      await _priorityService.RegisterSourceAsync(request.SourceId, request.Priority);
      return Ok(new { message = "Audio source registered successfully", sourceId = request.SourceId, priority = request.Priority });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error registering audio source {SourceId}", request.SourceId);
      return StatusCode(500, new { error = "Failed to register audio source", details = ex.Message });
    }
  }

  /// <summary>
  /// Unregister an audio source.
  /// </summary>
  /// <param name="request">Request containing source ID to unregister.</param>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("sources/unregister")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> UnregisterSource([FromBody] UnregisterSourceRequest request)
  {
    if (string.IsNullOrWhiteSpace(request?.SourceId))
    {
      return BadRequest(new { error = "Source ID is required" });
    }

    try
    {
      _logger.LogInformation("Unregistering audio source {SourceId}", request.SourceId);
      await _priorityService.UnregisterSourceAsync(request.SourceId);
      return Ok(new { message = "Audio source unregistered successfully", sourceId = request.SourceId });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error unregistering audio source {SourceId}", request.SourceId);
      return StatusCode(500, new { error = "Failed to unregister audio source", details = ex.Message });
    }
  }

  /// <summary>
  /// Notify that a high priority audio source is starting.
  /// This will duck (reduce volume of) low priority sources.
  /// </summary>
  /// <param name="request">Request containing source ID.</param>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("events/high-priority-start")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> NotifyHighPriorityStart([FromBody] SourceEventRequest request)
  {
    if (string.IsNullOrWhiteSpace(request?.SourceId))
    {
      return BadRequest(new { error = "Source ID is required" });
    }

    try
    {
      _logger.LogInformation("High priority audio starting: {SourceId}", request.SourceId);
      await _priorityService.OnHighPriorityStartAsync(request.SourceId);
      return Ok(new { message = "High priority audio started, low priority sources ducked", sourceId = request.SourceId });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error notifying high priority start for {SourceId}", request.SourceId);
      return StatusCode(500, new { error = "Failed to notify high priority start", details = ex.Message });
    }
  }

  /// <summary>
  /// Notify that a high priority audio source has finished.
  /// This will restore volume to low priority sources if no other high priority sources are active.
  /// </summary>
  /// <param name="request">Request containing source ID.</param>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("events/high-priority-end")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> NotifyHighPriorityEnd([FromBody] SourceEventRequest request)
  {
    if (string.IsNullOrWhiteSpace(request?.SourceId))
    {
      return BadRequest(new { error = "Source ID is required" });
    }

    try
    {
      _logger.LogInformation("High priority audio ending: {SourceId}", request.SourceId);
      await _priorityService.OnHighPriorityEndAsync(request.SourceId);
      return Ok(new { message = "High priority audio ended, low priority sources may be restored", sourceId = request.SourceId });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error notifying high priority end for {SourceId}", request.SourceId);
      return StatusCode(500, new { error = "Failed to notify high priority end", details = ex.Message });
    }
  }

  /// <summary>
  /// Get the current duck percentage.
  /// </summary>
  /// <returns>The current duck percentage (0.0 to 1.0).</returns>
  [HttpGet("config/duck-percentage")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public IActionResult GetDuckPercentage()
  {
    _logger.LogInformation("Retrieving duck percentage");
    var percentage = _priorityService.DuckPercentage;
    return Ok(new { duckPercentage = percentage, percentDisplay = $"{percentage * 100}%" });
  }

  /// <summary>
  /// Set the duck percentage (volume level for low priority audio when ducked).
  /// </summary>
  /// <param name="request">Request containing the new duck percentage.</param>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("config/duck-percentage")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> SetDuckPercentage([FromBody] SetDuckPercentageRequest request)
  {
    if (request.DuckPercentage < 0.0f || request.DuckPercentage > 1.0f)
    {
      return BadRequest(new { error = "Duck percentage must be between 0.0 and 1.0" });
    }

    try
    {
      _logger.LogInformation("Setting duck percentage to {Percentage}", request.DuckPercentage);
      await _priorityService.SetDuckPercentageAsync(request.DuckPercentage);
      return Ok(new { message = "Duck percentage updated successfully", duckPercentage = request.DuckPercentage, percentDisplay = $"{request.DuckPercentage * 100}%" });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error setting duck percentage");
      return StatusCode(500, new { error = "Failed to set duck percentage", details = ex.Message });
    }
  }

  /// <summary>
  /// Get the current priority status.
  /// </summary>
  /// <returns>Status information including whether high priority audio is active.</returns>
  [HttpGet("status")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public IActionResult GetStatus()
  {
    _logger.LogInformation("Retrieving priority status");
    var isHighPriorityActive = _priorityService.IsHighPriorityActive;
    var duckPercentage = _priorityService.DuckPercentage;
    
    return Ok(new
    {
      isHighPriorityActive,
      duckPercentage,
      message = isHighPriorityActive 
        ? "High priority audio is active, low priority sources are ducked" 
        : "No high priority audio active"
    });
  }
}

/// <summary>
/// Request model for registering an audio source.
/// </summary>
public record RegisterSourceRequest
{
  /// <summary>
  /// Unique identifier for the audio source.
  /// </summary>
  public string SourceId { get; init; } = string.Empty;

  /// <summary>
  /// Priority level for this source.
  /// </summary>
  public AudioPriority Priority { get; init; } = AudioPriority.Low;
}

/// <summary>
/// Request model for unregistering an audio source.
/// </summary>
public record UnregisterSourceRequest
{
  /// <summary>
  /// Unique identifier for the audio source to unregister.
  /// </summary>
  public string SourceId { get; init; } = string.Empty;
}

/// <summary>
/// Request model for source event notifications.
/// </summary>
public record SourceEventRequest
{
  /// <summary>
  /// Unique identifier for the audio source.
  /// </summary>
  public string SourceId { get; init; } = string.Empty;
}

/// <summary>
/// Request model for setting duck percentage.
/// </summary>
public record SetDuckPercentageRequest
{
  /// <summary>
  /// Duck percentage (0.0 to 1.0). Default is 0.2 (20%).
  /// </summary>
  public float DuckPercentage { get; init; } = 0.2f;
}
