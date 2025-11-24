using Microsoft.AspNetCore.Mvc;
using RadioConsole.Core.Interfaces.Audio;

namespace RadioConsole.API.Controllers;

/// <summary>
/// API controller for managing audio input and output devices.
/// Provides endpoints to enumerate, query, and select audio devices.
/// Also provides global audio control endpoints for volume, balance, equalization, and playback.
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

  // ========================================
  // Global Audio Control Endpoints
  // ========================================

  /// <summary>
  /// Get the current global volume level.
  /// </summary>
  /// <returns>The current volume level (0.0 to 1.0).</returns>
  [HttpGet("global/volume")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GlobalVolumeResponse>> GetGlobalVolume()
  {
    try
    {
      _logger.LogInformation("Retrieving global volume");
      var volume = await _deviceManager.GetGlobalVolumeAsync();
      return Ok(new GlobalVolumeResponse { Volume = volume });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving global volume");
      return StatusCode(500, new { error = "Failed to retrieve global volume", details = ex.Message });
    }
  }

  /// <summary>
  /// Set the global volume level.
  /// </summary>
  /// <param name="request">The volume level to set (0.0 to 1.0).</param>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("global/volume")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> SetGlobalVolume([FromBody] SetGlobalVolumeRequest request)
  {
    if (request == null)
    {
      return BadRequest(new { error = "Request body is required" });
    }

    try
    {
      _logger.LogInformation("Setting global volume to: {Volume}", request.Volume);
      await _deviceManager.SetGlobalVolumeAsync(request.Volume);
      return Ok(new { message = "Global volume set successfully", volume = request.Volume });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error setting global volume to {Volume}", request.Volume);
      return StatusCode(500, new { error = "Failed to set global volume", details = ex.Message });
    }
  }

  /// <summary>
  /// Get the current global balance (pan) setting.
  /// </summary>
  /// <returns>The current balance (-1.0 to 1.0).</returns>
  [HttpGet("global/balance")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GlobalBalanceResponse>> GetGlobalBalance()
  {
    try
    {
      _logger.LogInformation("Retrieving global balance");
      var balance = await _deviceManager.GetGlobalBalanceAsync();
      return Ok(new GlobalBalanceResponse { Balance = balance });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving global balance");
      return StatusCode(500, new { error = "Failed to retrieve global balance", details = ex.Message });
    }
  }

  /// <summary>
  /// Set the global balance (pan) level.
  /// </summary>
  /// <param name="request">The balance to set (-1.0 for full left to 1.0 for full right).</param>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("global/balance")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> SetGlobalBalance([FromBody] SetGlobalBalanceRequest request)
  {
    if (request == null)
    {
      return BadRequest(new { error = "Request body is required" });
    }

    try
    {
      _logger.LogInformation("Setting global balance to: {Balance}", request.Balance);
      await _deviceManager.SetGlobalBalanceAsync(request.Balance);
      return Ok(new { message = "Global balance set successfully", balance = request.Balance });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error setting global balance to {Balance}", request.Balance);
      return StatusCode(500, new { error = "Failed to set global balance", details = ex.Message });
    }
  }

  /// <summary>
  /// Get the current equalization settings.
  /// </summary>
  /// <returns>The current equalization settings.</returns>
  [HttpGet("global/equalization")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<EqualizationSettings>> GetEqualization()
  {
    try
    {
      _logger.LogInformation("Retrieving equalization settings");
      var settings = await _deviceManager.GetEqualizationAsync();
      return Ok(settings);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving equalization settings");
      return StatusCode(500, new { error = "Failed to retrieve equalization settings", details = ex.Message });
    }
  }

  /// <summary>
  /// Set the equalization settings.
  /// </summary>
  /// <param name="settings">The equalization settings to apply.</param>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("global/equalization")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> SetEqualization([FromBody] EqualizationSettings settings)
  {
    if (settings == null)
    {
      return BadRequest(new { error = "Equalization settings are required" });
    }

    try
    {
      _logger.LogInformation("Setting equalization: Bass={Bass}, Midrange={Midrange}, Treble={Treble}, Enabled={Enabled}",
        settings.Bass, settings.Midrange, settings.Treble, settings.Enabled);
      await _deviceManager.SetEqualizationAsync(settings);
      return Ok(new { message = "Equalization settings applied successfully" });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error setting equalization settings");
      return StatusCode(500, new { error = "Failed to set equalization settings", details = ex.Message });
    }
  }

  /// <summary>
  /// Get the current playback state.
  /// </summary>
  /// <returns>The current playback state.</returns>
  [HttpGet("global/playback")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<PlaybackStateResponse>> GetPlaybackState()
  {
    try
    {
      _logger.LogInformation("Retrieving playback state");
      var state = await _deviceManager.GetPlaybackStateAsync();
      return Ok(new PlaybackStateResponse { State = state.ToString() });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving playback state");
      return StatusCode(500, new { error = "Failed to retrieve playback state", details = ex.Message });
    }
  }

  /// <summary>
  /// Pause all audio playback.
  /// </summary>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("global/pause")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> Pause()
  {
    try
    {
      _logger.LogInformation("Pausing global audio playback");
      await _deviceManager.PauseAsync();
      return Ok(new { message = "Audio playback paused" });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error pausing audio playback");
      return StatusCode(500, new { error = "Failed to pause audio playback", details = ex.Message });
    }
  }

  /// <summary>
  /// Resume/start all audio playback.
  /// </summary>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("global/play")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> Play()
  {
    try
    {
      _logger.LogInformation("Resuming global audio playback");
      await _deviceManager.PlayAsync();
      return Ok(new { message = "Audio playback resumed" });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error resuming audio playback");
      return StatusCode(500, new { error = "Failed to resume audio playback", details = ex.Message });
    }
  }

  /// <summary>
  /// Stop all audio playback.
  /// </summary>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("global/stop")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> Stop()
  {
    try
    {
      _logger.LogInformation("Stopping global audio playback");
      await _deviceManager.StopAsync();
      return Ok(new { message = "Audio playback stopped" });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error stopping audio playback");
      return StatusCode(500, new { error = "Failed to stop audio playback", details = ex.Message });
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

/// <summary>
/// Request model for setting global volume.
/// </summary>
public record SetGlobalVolumeRequest
{
  /// <summary>
  /// The volume level (0.0 to 1.0).
  /// </summary>
  public float Volume { get; init; }
}

/// <summary>
/// Response model for global volume.
/// </summary>
public record GlobalVolumeResponse
{
  /// <summary>
  /// The current volume level (0.0 to 1.0).
  /// </summary>
  public float Volume { get; init; }
}

/// <summary>
/// Request model for setting global balance.
/// </summary>
public record SetGlobalBalanceRequest
{
  /// <summary>
  /// The balance (-1.0 for full left to 1.0 for full right, 0.0 is center).
  /// </summary>
  public float Balance { get; init; }
}

/// <summary>
/// Response model for global balance.
/// </summary>
public record GlobalBalanceResponse
{
  /// <summary>
  /// The current balance (-1.0 to 1.0).
  /// </summary>
  public float Balance { get; init; }
}

/// <summary>
/// Response model for playback state.
/// </summary>
public record PlaybackStateResponse
{
  /// <summary>
  /// The current playback state (Stopped, Playing, Paused).
  /// </summary>
  public string State { get; init; } = string.Empty;
}
