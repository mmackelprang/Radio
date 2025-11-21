using Microsoft.AspNetCore.Mvc;
using RadioConsole.Core.Interfaces;
using RadioConsole.Core.Models;

namespace RadioConsole.API.Controllers;

/// <summary>
/// API controller for user preferences management.
/// Handles audio device preferences, visibility settings, and ChromeCast configuration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PreferencesController : ControllerBase
{
  private readonly IConfigurationService _configService;
  private readonly ILogger<PreferencesController> _logger;

  private const string COMPONENT = "Preferences";
  private const string AUDIO_KEY = "Audio";
  private const string DEVICE_VISIBILITY_KEY = "DeviceVisibility";
  private const string CAST_DEVICE_KEY = "CastDevice";

  public PreferencesController(IConfigurationService configService, ILogger<PreferencesController> logger)
  {
    _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <summary>
  /// Get saved audio preferences (input/output devices, sources, etc.)
  /// </summary>
  /// <returns>Audio preferences or null if not found</returns>
  [HttpGet("audio")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<AudioPreferences>> GetAudioPreferences()
  {
    try
    {
      var item = await _configService.LoadAsync(COMPONENT, AUDIO_KEY);
      if (item == null)
      {
        return NotFound(new { message = "No audio preferences found" });
      }

      var preferences = System.Text.Json.JsonSerializer.Deserialize<AudioPreferences>(item.Value);
      return Ok(preferences);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving audio preferences");
      return StatusCode(500, new { error = "Failed to retrieve audio preferences", details = ex.Message });
    }
  }

  /// <summary>
  /// Save audio preferences
  /// </summary>
  /// <param name="preferences">Audio preferences to save</param>
  /// <returns>Success message</returns>
  [HttpPost("audio")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> SaveAudioPreferences([FromBody] AudioPreferences preferences)
  {
    if (preferences == null)
    {
      return BadRequest(new { error = "Preferences are required" });
    }

    try
    {
      var jsonValue = System.Text.Json.JsonSerializer.Serialize(preferences);
      var item = new ConfigurationItem
      {
        Id = $"{COMPONENT}_{AUDIO_KEY}",
        Component = COMPONENT,
        Key = AUDIO_KEY,
        Value = jsonValue,
        Category = "UserPreferences",
        LastUpdated = DateTime.UtcNow
      };

      await _configService.SaveAsync(item);
      _logger.LogInformation("Audio preferences saved");
      return Ok(new { message = "Audio preferences saved successfully" });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error saving audio preferences");
      return StatusCode(500, new { error = "Failed to save audio preferences", details = ex.Message });
    }
  }

  /// <summary>
  /// Get device visibility configuration
  /// </summary>
  /// <returns>Device visibility settings or null if not found</returns>
  [HttpGet("device-visibility")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<DeviceVisibilityConfig>> GetDeviceVisibilityConfig()
  {
    try
    {
      var item = await _configService.LoadAsync(COMPONENT, DEVICE_VISIBILITY_KEY);
      if (item == null)
      {
        return NotFound(new { message = "No device visibility configuration found" });
      }

      var config = System.Text.Json.JsonSerializer.Deserialize<DeviceVisibilityConfig>(item.Value);
      return Ok(config);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving device visibility configuration");
      return StatusCode(500, new { error = "Failed to retrieve device visibility configuration", details = ex.Message });
    }
  }

  /// <summary>
  /// Save device visibility configuration
  /// </summary>
  /// <param name="config">Device visibility configuration to save</param>
  /// <returns>Success message</returns>
  [HttpPost("device-visibility")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> SaveDeviceVisibilityConfig([FromBody] DeviceVisibilityConfig config)
  {
    if (config == null)
    {
      return BadRequest(new { error = "Configuration is required" });
    }

    try
    {
      var jsonValue = System.Text.Json.JsonSerializer.Serialize(config);
      var item = new ConfigurationItem
      {
        Id = $"{COMPONENT}_{DEVICE_VISIBILITY_KEY}",
        Component = COMPONENT,
        Key = DEVICE_VISIBILITY_KEY,
        Value = jsonValue,
        Category = "UserPreferences",
        LastUpdated = DateTime.UtcNow
      };

      await _configService.SaveAsync(item);
      _logger.LogInformation("Device visibility configuration saved");
      return Ok(new { message = "Device visibility configuration saved successfully" });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error saving device visibility configuration");
      return StatusCode(500, new { error = "Failed to save device visibility configuration", details = ex.Message });
    }
  }

  /// <summary>
  /// Get saved ChromeCast device preference
  /// </summary>
  /// <returns>ChromeCast device name or null if not found</returns>
  [HttpGet("cast-device")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<CastDevicePreference>> GetCastDevicePreference()
  {
    try
    {
      var item = await _configService.LoadAsync(COMPONENT, CAST_DEVICE_KEY);
      if (item == null)
      {
        return NotFound(new { message = "No ChromeCast device preference found" });
      }

      var preference = System.Text.Json.JsonSerializer.Deserialize<CastDevicePreference>(item.Value);
      return Ok(preference);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving ChromeCast device preference");
      return StatusCode(500, new { error = "Failed to retrieve ChromeCast device preference", details = ex.Message });
    }
  }

  /// <summary>
  /// Save ChromeCast device preference
  /// </summary>
  /// <param name="preference">ChromeCast device preference to save</param>
  /// <returns>Success message</returns>
  [HttpPost("cast-device")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> SaveCastDevicePreference([FromBody] CastDevicePreference preference)
  {
    if (preference == null || string.IsNullOrWhiteSpace(preference.CastDevice))
    {
      return BadRequest(new { error = "ChromeCast device name is required" });
    }

    try
    {
      var jsonValue = System.Text.Json.JsonSerializer.Serialize(preference);
      var item = new ConfigurationItem
      {
        Id = $"{COMPONENT}_{CAST_DEVICE_KEY}",
        Component = COMPONENT,
        Key = CAST_DEVICE_KEY,
        Value = jsonValue,
        Category = "UserPreferences",
        LastUpdated = DateTime.UtcNow
      };

      await _configService.SaveAsync(item);
      _logger.LogInformation("ChromeCast device preference saved: {Device}", preference.CastDevice);
      return Ok(new { message = "ChromeCast device preference saved successfully" });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error saving ChromeCast device preference");
      return StatusCode(500, new { error = "Failed to save ChromeCast device preference", details = ex.Message });
    }
  }
}

/// <summary>
/// Audio preferences model
/// </summary>
public record AudioPreferences
{
  public string? InputDevice { get; set; }
  public string? OutputDevice { get; set; }
  public string? InputSource { get; set; }
  public string? OutputDestination { get; set; }
  public string? CastDevice { get; set; }
}

/// <summary>
/// Device visibility configuration model
/// </summary>
public record DeviceVisibilityConfig
{
  public List<string>? HiddenInputDevices { get; set; }
  public List<string>? HiddenOutputDevices { get; set; }
}

/// <summary>
/// ChromeCast device preference model
/// </summary>
public record CastDevicePreference
{
  public string CastDevice { get; set; } = string.Empty;
}
