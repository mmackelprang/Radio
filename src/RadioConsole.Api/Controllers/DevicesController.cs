using Microsoft.AspNetCore.Mvc;
using RadioConsole.Api.Models;
using RadioConsole.Api.Services;
using RadioConsole.Api.Interfaces;

namespace RadioConsole.Api.Controllers;

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
        try
        {
            var configs = _deviceRegistry.GetAllInputConfigs();
            var loadedDevices = _deviceRegistry.GetAllInputs().ToDictionary(d => d.Id);

            var response = configs.Select(config => new DeviceConfigurationResponse
            {
                Id = config.Id,
                DeviceType = config.DeviceType,
                Name = config.Name,
                Description = config.Description,
                Parameters = config.Parameters,
                IsEnabled = config.IsEnabled,
                IsAvailable = loadedDevices.TryGetValue(config.Id, out var device) && device.IsAvailable,
                CreatedAt = config.CreatedAt,
                ModifiedAt = config.ModifiedAt
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting input devices");
            return StatusCode(500, "Error getting input devices");
        }
    }

    /// <summary>
    /// Get all configured output devices
    /// </summary>
    [HttpGet("outputs")]
    public IActionResult GetOutputs()
    {
        try
        {
            var configs = _deviceRegistry.GetAllOutputConfigs();
            var loadedDevices = _deviceRegistry.GetAllOutputs().ToDictionary(d => d.Id);

            var response = configs.Select(config => new DeviceConfigurationResponse
            {
                Id = config.Id,
                DeviceType = config.DeviceType,
                Name = config.Name,
                Description = config.Description,
                Parameters = config.Parameters,
                IsEnabled = config.IsEnabled,
                IsAvailable = loadedDevices.TryGetValue(config.Id, out var device) && device.IsAvailable,
                CreatedAt = config.CreatedAt,
                ModifiedAt = config.ModifiedAt
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting output devices");
            return StatusCode(500, "Error getting output devices");
        }
    }

    /// <summary>
    /// Get a specific input device configuration
    /// </summary>
    [HttpGet("inputs/{id}")]
    public IActionResult GetInput(string id)
    {
        try
        {
            var config = _deviceRegistry.GetInputConfig(id);
            if (config == null)
                return NotFound($"Input device with ID '{id}' not found");

            var device = _deviceRegistry.GetInput(id);

            return Ok(new DeviceConfigurationResponse
            {
                Id = config.Id,
                DeviceType = config.DeviceType,
                Name = config.Name,
                Description = config.Description,
                Parameters = config.Parameters,
                IsEnabled = config.IsEnabled,
                IsAvailable = device?.IsAvailable ?? false,
                CreatedAt = config.CreatedAt,
                ModifiedAt = config.ModifiedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting input device {Id}", id);
            return StatusCode(500, "Error getting input device");
        }
    }

    /// <summary>
    /// Get a specific output device configuration
    /// </summary>
    [HttpGet("outputs/{id}")]
    public IActionResult GetOutput(string id)
    {
        try
        {
            var config = _deviceRegistry.GetOutputConfig(id);
            if (config == null)
                return NotFound($"Output device with ID '{id}' not found");

            var device = _deviceRegistry.GetOutput(id);

            return Ok(new DeviceConfigurationResponse
            {
                Id = config.Id,
                DeviceType = config.DeviceType,
                Name = config.Name,
                Description = config.Description,
                Parameters = config.Parameters,
                IsEnabled = config.IsEnabled,
                IsAvailable = device?.IsAvailable ?? false,
                CreatedAt = config.CreatedAt,
                ModifiedAt = config.ModifiedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting output device {Id}", id);
            return StatusCode(500, "Error getting output device");
        }
    }

    /// <summary>
    /// Add a new input device
    /// </summary>
    [HttpPost("inputs")]
    public async Task<IActionResult> AddInput([FromBody] DeviceConfigurationRequest request)
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
                IsEnabled = request.IsEnabled
            };

            var added = await _deviceRegistry.AddInputAsync(config);
            var device = _deviceRegistry.GetInput(added.Id);

            _logger.LogInformation("Added input device: {Name} ({Type})", added.Name, added.DeviceType);

            return CreatedAtAction(nameof(GetInput), new { id = added.Id }, new DeviceConfigurationResponse
            {
                Id = added.Id,
                DeviceType = added.DeviceType,
                Name = added.Name,
                Description = added.Description,
                Parameters = added.Parameters,
                IsEnabled = added.IsEnabled,
                IsAvailable = device?.IsAvailable ?? false,
                CreatedAt = added.CreatedAt,
                ModifiedAt = added.ModifiedAt
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid device type: {Type}", request.DeviceType);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding input device");
            return StatusCode(500, "Error adding input device");
        }
    }

    /// <summary>
    /// Add a new output device
    /// </summary>
    [HttpPost("outputs")]
    public async Task<IActionResult> AddOutput([FromBody] DeviceConfigurationRequest request)
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
                IsEnabled = request.IsEnabled
            };

            var added = await _deviceRegistry.AddOutputAsync(config);
            var device = _deviceRegistry.GetOutput(added.Id);

            _logger.LogInformation("Added output device: {Name} ({Type})", added.Name, added.DeviceType);

            return CreatedAtAction(nameof(GetOutput), new { id = added.Id }, new DeviceConfigurationResponse
            {
                Id = added.Id,
                DeviceType = added.DeviceType,
                Name = added.Name,
                Description = added.Description,
                Parameters = added.Parameters,
                IsEnabled = added.IsEnabled,
                IsAvailable = device?.IsAvailable ?? false,
                CreatedAt = added.CreatedAt,
                ModifiedAt = added.ModifiedAt
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid device type: {Type}", request.DeviceType);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding output device");
            return StatusCode(500, "Error adding output device");
        }
    }

    /// <summary>
    /// Update an existing input device
    /// </summary>
    [HttpPut("inputs/{id}")]
    public async Task<IActionResult> UpdateInput(string id, [FromBody] DeviceConfigurationRequest request)
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
                IsEnabled = request.IsEnabled
            };

            var updated = await _deviceRegistry.UpdateInputAsync(id, config);
            if (updated == null)
                return NotFound($"Input device with ID '{id}' not found");

            var device = _deviceRegistry.GetInput(id);

            _logger.LogInformation("Updated input device: {Name} ({Type})", updated.Name, updated.DeviceType);

            return Ok(new DeviceConfigurationResponse
            {
                Id = updated.Id,
                DeviceType = updated.DeviceType,
                Name = updated.Name,
                Description = updated.Description,
                Parameters = updated.Parameters,
                IsEnabled = updated.IsEnabled,
                IsAvailable = device?.IsAvailable ?? false,
                CreatedAt = updated.CreatedAt,
                ModifiedAt = updated.ModifiedAt
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid device type: {Type}", request.DeviceType);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating input device {Id}", id);
            return StatusCode(500, "Error updating input device");
        }
    }

    /// <summary>
    /// Update an existing output device
    /// </summary>
    [HttpPut("outputs/{id}")]
    public async Task<IActionResult> UpdateOutput(string id, [FromBody] DeviceConfigurationRequest request)
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
                IsEnabled = request.IsEnabled
            };

            var updated = await _deviceRegistry.UpdateOutputAsync(id, config);
            if (updated == null)
                return NotFound($"Output device with ID '{id}' not found");

            var device = _deviceRegistry.GetOutput(id);

            _logger.LogInformation("Updated output device: {Name} ({Type})", updated.Name, updated.DeviceType);

            return Ok(new DeviceConfigurationResponse
            {
                Id = updated.Id,
                DeviceType = updated.DeviceType,
                Name = updated.Name,
                Description = updated.Description,
                Parameters = updated.Parameters,
                IsEnabled = updated.IsEnabled,
                IsAvailable = device?.IsAvailable ?? false,
                CreatedAt = updated.CreatedAt,
                ModifiedAt = updated.ModifiedAt
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid device type: {Type}", request.DeviceType);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating output device {Id}", id);
            return StatusCode(500, "Error updating output device");
        }
    }

    /// <summary>
    /// Remove an input device
    /// </summary>
    [HttpDelete("inputs/{id}")]
    public async Task<IActionResult> RemoveInput(string id)
    {
        try
        {
            var success = await _deviceRegistry.RemoveInputAsync(id);
            if (!success)
                return NotFound($"Input device with ID '{id}' not found");

            _logger.LogInformation("Removed input device: {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing input device {Id}", id);
            return StatusCode(500, "Error removing input device");
        }
    }

    /// <summary>
    /// Remove an output device
    /// </summary>
    [HttpDelete("outputs/{id}")]
    public async Task<IActionResult> RemoveOutput(string id)
    {
        try
        {
            var success = await _deviceRegistry.RemoveOutputAsync(id);
            if (!success)
                return NotFound($"Output device with ID '{id}' not found");

            _logger.LogInformation("Removed output device: {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing output device {Id}", id);
            return StatusCode(500, "Error removing output device");
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
