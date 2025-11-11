using Microsoft.AspNetCore.Mvc;
using RadioConsole.Api.Models;
using RadioConsole.Api.Services;
using RadioConsole.Api.Interfaces;

namespace RadioConsole.Api.Controllers;

/// <summary>
/// Category of audio device for generic operations
/// </summary>
internal enum AudioDeviceCategory
{
  Input,
  Output
}

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
  private readonly IDeviceRegistry _deviceRegistry;
  private readonly IDeviceFactory _deviceFactory;
  private readonly ILogger<DevicesController> _logger;

  public DevicesController(
    IDeviceRegistry deviceRegistry,
    IDeviceFactory deviceFactory,
    ILogger<DevicesController> logger)
  {
    _deviceRegistry = deviceRegistry;
    _deviceFactory = deviceFactory;
    _logger = logger;
  }

  /// <summary>
  /// Get all configured input devices
  /// </summary>
  [HttpGet("inputs")]
  public IActionResult GetInputs()
  {
    return GetDevices(AudioDeviceCategory.Input);
  }

  /// <summary>
  /// Get all configured output devices
  /// </summary>
  [HttpGet("outputs")]
  public IActionResult GetOutputs()
  {
    return GetDevices(AudioDeviceCategory.Output);
  }

  /// <summary>
  /// Helper method to get all devices of a specific category
  /// </summary>
  private IActionResult GetDevices(AudioDeviceCategory category)
  {
    try
    {
      var configs = category == AudioDeviceCategory.Input
        ? _deviceRegistry.GetAllInputConfigs()
        : _deviceRegistry.GetAllOutputConfigs();

      Dictionary<string, IAudioDevice> loadedDevices;
      if (category == AudioDeviceCategory.Input)
      {
        loadedDevices = _deviceRegistry.GetAllInputs()
          .Cast<IAudioDevice>()
          .ToDictionary(d => d.Id);
      }
      else
      {
        loadedDevices = _deviceRegistry.GetAllOutputs()
          .Cast<IAudioDevice>()
          .ToDictionary(d => d.Id);
      }

      var response = configs.Select(config => new DeviceConfigurationResponse
      {
        Id = config.Id,
        DeviceType = config.DeviceType,
        Name = config.Name,
        Description = config.Description,
        Parameters = config.Parameters,
        AudioInputType = config.AudioInputType,
        IsEnabled = config.IsEnabled,
        IsAvailable = loadedDevices.TryGetValue(config.Id, out var device) && device.IsAvailable,
        CreatedAt = config.CreatedAt,
        ModifiedAt = config.ModifiedAt
      });

      return Ok(response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting {Category} devices", category.ToString().ToLower());
      return StatusCode(500, $"Error getting {category.ToString().ToLower()} devices");
    }
  }

  /// <summary>
  /// Get a specific input device configuration
  /// </summary>
  [HttpGet("inputs/{id}")]
  public IActionResult GetInput(string id)
  {
    return GetDevice(id, AudioDeviceCategory.Input);
  }

  /// <summary>
  /// Get a specific output device configuration
  /// </summary>
  [HttpGet("outputs/{id}")]
  public IActionResult GetOutput(string id)
  {
    return GetDevice(id, AudioDeviceCategory.Output);
  }

  /// <summary>
  /// Helper method to get a single device of a specific category
  /// </summary>
  private IActionResult GetDevice(string id, AudioDeviceCategory category)
  {
    try
    {
      var config = category == AudioDeviceCategory.Input
        ? _deviceRegistry.GetInputConfig(id)
        : _deviceRegistry.GetOutputConfig(id);

      if (config == null)
        return NotFound($"{category} device with ID '{id}' not found");

      IAudioDevice? device = category == AudioDeviceCategory.Input
        ? _deviceRegistry.GetInput(id)
        : _deviceRegistry.GetOutput(id);

      return Ok(new DeviceConfigurationResponse
      {
        Id = config.Id,
        DeviceType = config.DeviceType,
        Name = config.Name,
        Description = config.Description,
        Parameters = config.Parameters,
        AudioInputType = config.AudioInputType,
        IsEnabled = config.IsEnabled,
        IsAvailable = device?.IsAvailable ?? false,
        CreatedAt = config.CreatedAt,
        ModifiedAt = config.ModifiedAt
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting {Category} device {Id}", category.ToString().ToLower(), id);
      return StatusCode(500, $"Error getting {category.ToString().ToLower()} device");
    }
  }

  /// <summary>
  /// Add a new input device
  /// </summary>
  [HttpPost("inputs")]
  public async Task<IActionResult> AddInput([FromBody] DeviceConfigurationRequest request)
  {
    return await AddDevice(request, AudioDeviceCategory.Input);
  }

  /// <summary>
  /// Add a new output device
  /// </summary>
  [HttpPost("outputs")]
  public async Task<IActionResult> AddOutput([FromBody] DeviceConfigurationRequest request)
  {
    return await AddDevice(request, AudioDeviceCategory.Output);
  }

  /// <summary>
  /// Helper method to add a device of a specific category
  /// </summary>
  private async Task<IActionResult> AddDevice(DeviceConfigurationRequest request, AudioDeviceCategory category)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(request.Name))
        return BadRequest("Device name is required");

      if (string.IsNullOrWhiteSpace(request.DeviceType))
        return BadRequest("Device type is required");

      var config = new DeviceConfiguration
      {
        DeviceType = request.DeviceType,
        Name = request.Name,
        Description = request.Description,
        Parameters = request.Parameters,
        AudioInputType = request.AudioInputType,
        IsEnabled = request.IsEnabled
      };

      var added = category == AudioDeviceCategory.Input
        ? await _deviceRegistry.AddInputAsync(config)
        : await _deviceRegistry.AddOutputAsync(config);

      IAudioDevice? device = category == AudioDeviceCategory.Input
        ? _deviceRegistry.GetInput(added.Id)
        : _deviceRegistry.GetOutput(added.Id);

      _logger.LogInformation("Added {Category} device: {Name} ({Type})",
        category.ToString().ToLower(), added.Name, added.DeviceType);

      var actionName = category == AudioDeviceCategory.Input ? nameof(GetInput) : nameof(GetOutput);

      return CreatedAtAction(actionName, new { id = added.Id }, new DeviceConfigurationResponse
      {
        Id = added.Id,
        DeviceType = added.DeviceType,
        Name = added.Name,
        Description = added.Description,
        Parameters = added.Parameters,
        AudioInputType = added.AudioInputType,
        IsEnabled = added.IsEnabled,
        IsAvailable = device?.IsAvailable ?? false,
        CreatedAt = added.CreatedAt,
        ModifiedAt = added.ModifiedAt
      });
    }
    catch (InvalidOperationException ex)
    {
      _logger.LogWarning(ex, "Duplicate device name: {Name}", request.Name);
      return BadRequest(ex.Message);
    }
    catch (ArgumentException ex)
    {
      _logger.LogWarning(ex, "Invalid device type: {Type}", request.DeviceType);
      return BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error adding {Category} device", category.ToString().ToLower());
      return StatusCode(500, $"Error adding {category.ToString().ToLower()} device");
    }
  }

  /// <summary>
  /// Update an existing input device
  /// </summary>
  [HttpPut("inputs/{id}")]
  public async Task<IActionResult> UpdateInput(string id, [FromBody] DeviceConfigurationRequest request)
  {
    return await UpdateDevice(id, request, AudioDeviceCategory.Input);
  }

  /// <summary>
  /// Update an existing output device
  /// </summary>
  [HttpPut("outputs/{id}")]
  public async Task<IActionResult> UpdateOutput(string id, [FromBody] DeviceConfigurationRequest request)
  {
    return await UpdateDevice(id, request, AudioDeviceCategory.Output);
  }

  /// <summary>
  /// Helper method to update a device of a specific category
  /// </summary>
  private async Task<IActionResult> UpdateDevice(string id, DeviceConfigurationRequest request, AudioDeviceCategory category)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(request.Name))
        return BadRequest("Device name is required");

      if (string.IsNullOrWhiteSpace(request.DeviceType))
        return BadRequest("Device type is required");

      var config = new DeviceConfiguration
      {
        DeviceType = request.DeviceType,
        Name = request.Name,
        Description = request.Description,
        Parameters = request.Parameters,
        AudioInputType = request.AudioInputType,
        IsEnabled = request.IsEnabled
      };

      var updated = category == AudioDeviceCategory.Input
        ? await _deviceRegistry.UpdateInputAsync(id, config)
        : await _deviceRegistry.UpdateOutputAsync(id, config);

      if (updated == null)
        return NotFound($"{category} device with ID '{id}' not found");

      IAudioDevice? device = category == AudioDeviceCategory.Input
        ? _deviceRegistry.GetInput(id)
        : _deviceRegistry.GetOutput(id);

      _logger.LogInformation("Updated {Category} device: {Name} ({Type})",
        category.ToString().ToLower(), updated.Name, updated.DeviceType);

      return Ok(new DeviceConfigurationResponse
      {
        Id = updated.Id,
        DeviceType = updated.DeviceType,
        Name = updated.Name,
        Description = updated.Description,
        Parameters = updated.Parameters,
        AudioInputType = updated.AudioInputType,
        IsEnabled = updated.IsEnabled,
        IsAvailable = device?.IsAvailable ?? false,
        CreatedAt = updated.CreatedAt,
        ModifiedAt = updated.ModifiedAt
      });
    }
    catch (InvalidOperationException ex)
    {
      _logger.LogWarning(ex, "Duplicate device name: {Name}", request.Name);
      return BadRequest(ex.Message);
    }
    catch (ArgumentException ex)
    {
      _logger.LogWarning(ex, "Invalid device type: {Type}", request.DeviceType);
      return BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error updating {Category} device {Id}", category.ToString().ToLower(), id);
      return StatusCode(500, $"Error updating {category.ToString().ToLower()} device");
    }
  }

  /// <summary>
  /// Remove an input device
  /// </summary>
  [HttpDelete("inputs/{id}")]
  public async Task<IActionResult> RemoveInput(string id)
  {
    return await RemoveDevice(id, AudioDeviceCategory.Input);
  }

  /// <summary>
  /// Remove an output device
  /// </summary>
  [HttpDelete("outputs/{id}")]
  public async Task<IActionResult> RemoveOutput(string id)
  {
    return await RemoveDevice(id, AudioDeviceCategory.Output);
  }

  /// <summary>
  /// Helper method to remove a device of a specific category
  /// </summary>
  private async Task<IActionResult> RemoveDevice(string id, AudioDeviceCategory category)
  {
    try
    {
      var success = category == AudioDeviceCategory.Input
        ? await _deviceRegistry.RemoveInputAsync(id)
        : await _deviceRegistry.RemoveOutputAsync(id);

      if (!success)
        return NotFound($"{category} device with ID '{id}' not found");

      _logger.LogInformation("Removed {Category} device: {Id}", category.ToString().ToLower(), id);
      return NoContent();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error removing {Category} device {Id}", category.ToString().ToLower(), id);
      return StatusCode(500, $"Error removing {category.ToString().ToLower()} device");
    }
  }

    /// <summary>
    /// Get available input device types
    /// </summary>
    [HttpGet("types/inputs")]
    public IActionResult GetInputTypes()
    {
        try
        {
            var types = _deviceFactory.GetAvailableInputTypes();
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting input device types");
            return StatusCode(500, "Error getting input device types");
        }
    }

    /// <summary>
    /// Get available output device types
    /// </summary>
    [HttpGet("types/outputs")]
    public IActionResult GetOutputTypes()
    {
        try
        {
            var types = _deviceFactory.GetAvailableOutputTypes();
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting output device types");
            return StatusCode(500, "Error getting output device types");
        }
    }
}
