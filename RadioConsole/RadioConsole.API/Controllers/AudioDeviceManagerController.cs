using Microsoft.AspNetCore.Mvc;
using RadioConsole.Core.Interfaces.Audio;

namespace RadioConsole.API.Controllers;

/// <summary>
/// API controller for managing audio input and output devices.
/// Provides endpoints to enumerate, query, and select audio devices.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AudioDeviceManagerController : ControllerBase
{
  private readonly IAudioDeviceManager _deviceManager;
  private readonly ILogger<AudioDeviceManagerController> _logger;

  public AudioDeviceManagerController(
    IAudioDeviceManager deviceManager,
    ILogger<AudioDeviceManagerController> logger)
  {
    _deviceManager = deviceManager ?? throw new ArgumentNullException(nameof(deviceManager));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <summary>
  /// Get all available audio input devices.
  /// </summary>
  /// <returns>Collection of available input devices.</returns>
  [HttpGet("inputs")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<IEnumerable<AudioDeviceInfo>>> GetInputDevices()
  {
    try
    {
      _logger.LogInformation("Retrieving available input devices");
      var devices = await _deviceManager.GetInputDevicesAsync();
      return Ok(devices);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving input devices");
      return StatusCode(500, new { error = "Failed to retrieve input devices", details = ex.Message });
    }
  }

  /// <summary>
  /// Get all available audio output devices.
  /// </summary>
  /// <returns>Collection of available output devices.</returns>
  [HttpGet("outputs")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<IEnumerable<AudioDeviceInfo>>> GetOutputDevices()
  {
    try
    {
      _logger.LogInformation("Retrieving available output devices");
      var devices = await _deviceManager.GetOutputDevicesAsync();
      return Ok(devices);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving output devices");
      return StatusCode(500, new { error = "Failed to retrieve output devices", details = ex.Message });
    }
  }

  /// <summary>
  /// Get the currently selected input device.
  /// </summary>
  /// <returns>The current input device, or 404 if none is selected.</returns>
  [HttpGet("inputs/current")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<AudioDeviceInfo>> GetCurrentInputDevice()
  {
    try
    {
      _logger.LogInformation("Retrieving current input device");
      var device = await _deviceManager.GetCurrentInputDeviceAsync();
      
      if (device == null)
      {
        return NotFound(new { error = "No input device currently selected" });
      }

      return Ok(device);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving current input device");
      return StatusCode(500, new { error = "Failed to retrieve current input device", details = ex.Message });
    }
  }

  /// <summary>
  /// Get the currently selected output device.
  /// </summary>
  /// <returns>The current output device, or 404 if none is selected.</returns>
  [HttpGet("outputs/current")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<AudioDeviceInfo>> GetCurrentOutputDevice()
  {
    try
    {
      _logger.LogInformation("Retrieving current output device");
      var device = await _deviceManager.GetCurrentOutputDeviceAsync();
      
      if (device == null)
      {
        return NotFound(new { error = "No output device currently selected" });
      }

      return Ok(device);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving current output device");
      return StatusCode(500, new { error = "Failed to retrieve current output device", details = ex.Message });
    }
  }

  /// <summary>
  /// Set the audio input device.
  /// </summary>
  /// <param name="request">Request containing the device ID to set.</param>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("inputs/current")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> SetInputDevice([FromBody] SetDeviceRequest request)
  {
    if (string.IsNullOrWhiteSpace(request?.DeviceId))
    {
      return BadRequest(new { error = "Device ID is required" });
    }

    try
    {
      _logger.LogInformation("Setting input device to: {DeviceId}", request.DeviceId);
      await _deviceManager.SetInputDeviceAsync(request.DeviceId);
      return Ok(new { message = "Input device set successfully", deviceId = request.DeviceId });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error setting input device to {DeviceId}", request.DeviceId);
      return StatusCode(500, new { error = "Failed to set input device", details = ex.Message });
    }
  }

  /// <summary>
  /// Set the audio output device.
  /// </summary>
  /// <param name="request">Request containing the device ID to set.</param>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("outputs/current")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> SetOutputDevice([FromBody] SetDeviceRequest request)
  {
    if (string.IsNullOrWhiteSpace(request?.DeviceId))
    {
      return BadRequest(new { error = "Device ID is required" });
    }

    try
    {
      _logger.LogInformation("Setting output device to: {DeviceId}", request.DeviceId);
      await _deviceManager.SetOutputDeviceAsync(request.DeviceId);
      return Ok(new { message = "Output device set successfully", deviceId = request.DeviceId });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error setting output device to {DeviceId}", request.DeviceId);
      return StatusCode(500, new { error = "Failed to set output device", details = ex.Message });
    }
  }
}

/// <summary>
/// Request model for setting audio device.
/// </summary>
public record SetDeviceRequest
{
  /// <summary>
  /// The device ID to set as active.
  /// </summary>
  public string DeviceId { get; init; } = string.Empty;
}
