using Microsoft.AspNetCore.Mvc;
using RadioConsole.Core.Interfaces.Audio;

namespace RadioConsole.API.Controllers;

/// <summary>
/// API controller for visualization settings and information.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VisualizationController : ControllerBase
{
  private readonly IAudioPlayer _audioPlayer;
  private readonly ILogger<VisualizationController> _logger;

  public VisualizationController(IAudioPlayer audioPlayer, ILogger<VisualizationController> logger)
  {
    _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <summary>
  /// Get visualization settings and status.
  /// </summary>
  /// <returns>Visualization information.</returns>
  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public ActionResult<VisualizationInfo> Get()
  {
    try
    {
      var info = new VisualizationInfo
      {
        IsEnabled = _audioPlayer.IsInitialized,
        IsPlayerInitialized = _audioPlayer.IsInitialized
      };

      return Ok(info);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving visualization information");
      return StatusCode(500, new { error = "Failed to retrieve visualization information", details = ex.Message });
    }
  }

  /// <summary>
  /// Enable or disable FFT data generation for visualization.
  /// </summary>
  /// <param name="request">Enable/disable request.</param>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("enable")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public IActionResult EnableVisualization([FromBody] VisualizationEnableRequest request)
  {
    try
    {
      if (!_audioPlayer.IsInitialized)
      {
        return BadRequest(new { error = "Audio player is not initialized" });
      }

      _audioPlayer.EnableFftDataGeneration(request.Enabled);
      _logger.LogInformation("Visualization FFT generation {Status}", request.Enabled ? "enabled" : "disabled");
      
      return Ok(new { message = $"Visualization {(request.Enabled ? "enabled" : "disabled")} successfully", enabled = request.Enabled });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error toggling visualization");
      return StatusCode(500, new { error = "Failed to toggle visualization", details = ex.Message });
    }
  }

  /// <summary>
  /// Get available visualization types.
  /// </summary>
  /// <returns>List of available visualization types.</returns>
  [HttpGet("types")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public ActionResult<IEnumerable<string>> GetVisualizationTypes()
  {
    var types = new[] { "LevelMeter", "Waveform", "Spectrum" };
    return Ok(types);
  }

  /// <summary>
  /// Set the active visualization type.
  /// </summary>
  /// <param name="request">Visualization type selection request.</param>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("type")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public IActionResult SetVisualizationType([FromBody] VisualizationTypeRequest request)
  {
    try
    {
      _logger.LogInformation("Setting visualization type to {Type}", request.Type);
      
      // Validate the type
      var validTypes = new[] { "LevelMeter", "Waveform", "Spectrum" };
      if (!validTypes.Contains(request.Type, StringComparer.OrdinalIgnoreCase))
      {
        return BadRequest(new { error = $"Invalid visualization type: {request.Type}", validTypes });
      }

      // Note: This is a placeholder. In a full implementation, this would:
      // 1. Create the appropriate visualizer instance
      // 2. Connect it to the audio pipeline
      // 3. Start sending visualization data via SignalR
      // For now, we just log the request and return success

      return Ok(new { message = $"Visualization type set to {request.Type}", type = request.Type });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error setting visualization type");
      return StatusCode(500, new { error = "Failed to set visualization type", details = ex.Message });
    }
  }
}

/// <summary>
/// Visualization information.
/// </summary>
public record VisualizationInfo
{
  /// <summary>
  /// Whether visualization is enabled.
  /// </summary>
  public bool IsEnabled { get; init; }

  /// <summary>
  /// Whether the audio player is initialized.
  /// </summary>
  public bool IsPlayerInitialized { get; init; }

  /// <summary>
  /// Available visualization types.
  /// </summary>
  public string[] AvailableTypes { get; init; } = new[] { "LevelMeter", "Waveform", "Spectrum" };
}

/// <summary>
/// Request model for enabling/disabling visualization.
/// </summary>
public record VisualizationEnableRequest
{
  /// <summary>
  /// Whether to enable visualization.
  /// </summary>
  public bool Enabled { get; init; }
}

/// <summary>
/// Request model for setting visualization type.
/// </summary>
public record VisualizationTypeRequest
{
  /// <summary>
  /// The visualization type to set (LevelMeter, Waveform, or Spectrum).
  /// </summary>
  public string Type { get; init; } = "Spectrum";
}
