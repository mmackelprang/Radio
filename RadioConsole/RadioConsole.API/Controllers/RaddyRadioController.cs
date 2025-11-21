using Microsoft.AspNetCore.Mvc;
using RadioConsole.Core.Interfaces.Inputs;

namespace RadioConsole.API.Controllers;

/// <summary>
/// API controller for Raddy RF320 radio control.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RaddyRadioController : ControllerBase
{
  private readonly IRaddyRadioService _raddyRadioService;
  private readonly ILogger<RaddyRadioController> _logger;

  public RaddyRadioController(IRaddyRadioService raddyRadioService, ILogger<RaddyRadioController> logger)
  {
    _raddyRadioService = raddyRadioService ?? throw new ArgumentNullException(nameof(raddyRadioService));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <summary>
  /// Get the current status of the Raddy radio.
  /// </summary>
  /// <returns>Radio status including signal strength, frequency, and device detection status.</returns>
  [HttpGet("status")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<RaddyRadioStatus>> GetStatus()
  {
    try
    {
      var frequency = await _raddyRadioService.GetFrequencyAsync();
      var status = new RaddyRadioStatus
      {
        IsStreaming = _raddyRadioService.IsStreaming,
        IsDeviceDetected = _raddyRadioService.IsDeviceDetected,
        SignalStrength = _raddyRadioService.SignalStrength,
        Frequency = frequency,
        DeviceId = _raddyRadioService.GetDeviceId()
      };

      return Ok(status);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting Raddy radio status");
      return StatusCode(500, new { error = "Failed to get radio status", details = ex.Message });
    }
  }

  /// <summary>
  /// Start the Raddy radio stream.
  /// </summary>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("start")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Start()
  {
    try
    {
      if (!_raddyRadioService.IsDeviceDetected)
      {
        return BadRequest(new { error = "Raddy device not detected" });
      }

      await _raddyRadioService.StartAsync();
      _logger.LogInformation("Raddy radio stream started via API");
      return Ok(new { message = "Radio stream started successfully" });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error starting Raddy radio stream");
      return StatusCode(500, new { error = "Failed to start radio stream", details = ex.Message });
    }
  }

  /// <summary>
  /// Stop the Raddy radio stream.
  /// </summary>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("stop")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> Stop()
  {
    try
    {
      await _raddyRadioService.StopAsync();
      _logger.LogInformation("Raddy radio stream stopped via API");
      return Ok(new { message = "Radio stream stopped successfully" });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error stopping Raddy radio stream");
      return StatusCode(500, new { error = "Failed to stop radio stream", details = ex.Message });
    }
  }

  /// <summary>
  /// Set the radio frequency.
  /// </summary>
  /// <param name="request">Frequency request containing the frequency in MHz.</param>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("frequency")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> SetFrequency([FromBody] FrequencyRequest request)
  {
    if (request == null || request.FrequencyMHz <= 0)
    {
      return BadRequest(new { error = "Valid frequency is required" });
    }

    try
    {
      await _raddyRadioService.SetFrequencyAsync(request.FrequencyMHz);
      _logger.LogInformation("Raddy radio frequency set to {Frequency} MHz via API", request.FrequencyMHz);
      return Ok(new { message = "Frequency set successfully", frequency = request.FrequencyMHz });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error setting Raddy radio frequency");
      return StatusCode(500, new { error = "Failed to set frequency", details = ex.Message });
    }
  }

  /// <summary>
  /// Get the radio frequency.
  /// </summary>
  /// <returns>The current frequency in MHz, or null if not available.</returns>
  [HttpGet("frequency")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<FrequencyResponse>> GetFrequency()
  {
    try
    {
      var frequency = await _raddyRadioService.GetFrequencyAsync();
      if (frequency == null)
      {
        return NotFound(new { error = "Frequency not available" });
      }

      return Ok(new FrequencyResponse { FrequencyMHz = frequency.Value });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting Raddy radio frequency");
      return StatusCode(500, new { error = "Failed to get frequency", details = ex.Message });
    }
  }

  /// <summary>
  /// Initialize the Raddy radio service.
  /// </summary>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("initialize")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> Initialize()
  {
    try
    {
      await _raddyRadioService.InitializeAsync();
      _logger.LogInformation("Raddy radio service initialized via API");
      
      var status = new
      {
        message = "Radio service initialized successfully",
        deviceDetected = _raddyRadioService.IsDeviceDetected,
        deviceId = _raddyRadioService.GetDeviceId()
      };

      return Ok(status);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error initializing Raddy radio service");
      return StatusCode(500, new { error = "Failed to initialize radio service", details = ex.Message });
    }
  }
}

/// <summary>
/// Response model for Raddy radio status.
/// </summary>
public record RaddyRadioStatus
{
  /// <summary>
  /// Whether the radio is currently streaming.
  /// </summary>
  public bool IsStreaming { get; init; }

  /// <summary>
  /// Whether the Raddy USB device is detected.
  /// </summary>
  public bool IsDeviceDetected { get; init; }

  /// <summary>
  /// Signal strength on a 0-6 scale.
  /// 0 = No Signal, 1 = Very Weak, 2 = Weak, 3 = Fair, 4 = Good, 5 = Very Good, 6 = Excellent
  /// </summary>
  public int SignalStrength { get; init; }

  /// <summary>
  /// Current frequency in MHz.
  /// </summary>
  public double? Frequency { get; init; }

  /// <summary>
  /// USB device identifier.
  /// </summary>
  public string? DeviceId { get; init; }
}

/// <summary>
/// Request model for setting frequency.
/// </summary>
public record FrequencyRequest
{
  /// <summary>
  /// Frequency in MHz.
  /// </summary>
  public double FrequencyMHz { get; init; }
}

/// <summary>
/// Response model for frequency.
/// </summary>
public record FrequencyResponse
{
  /// <summary>
  /// Frequency in MHz.
  /// </summary>
  public double FrequencyMHz { get; init; }
}
