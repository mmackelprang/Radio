using Microsoft.AspNetCore.Mvc;
using RadioConsole.Core.Interfaces;
using RadioConsole.Core.Models;

namespace RadioConsole.API.Controllers;

/// <summary>
/// API controller for configuration management (CRUD operations).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConfigurationController : ControllerBase
{
  private readonly IConfigurationService _configService;
  private readonly ILogger<ConfigurationController> _logger;

  public ConfigurationController(IConfigurationService configService, ILogger<ConfigurationController> logger)
  {
    _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <summary>
  /// Get all configuration items.
  /// </summary>
  /// <returns>Collection of all configuration items.</returns>
  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<ConfigurationItem>>> GetAll()
  {
    try
    {
      var items = await _configService.LoadAllAsync();
      return Ok(items);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving all configuration items");
      return StatusCode(500, new { error = "Failed to retrieve configuration items", details = ex.Message });
    }
  }

  /// <summary>
  /// Get a specific configuration item by component and key.
  /// </summary>
  /// <param name="component">Component name.</param>
  /// <param name="key">Configuration key.</param>
  /// <returns>The configuration item, or 404 if not found.</returns>
  [HttpGet("{component}/{key}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<ConfigurationItem>> Get(string component, string key)
  {
    try
    {
      var item = await _configService.LoadAsync(component, key);
      if (item == null)
      {
        return NotFound(new { error = "Configuration item not found", component, key });
      }
      return Ok(item);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving configuration item {Component}/{Key}", component, key);
      return StatusCode(500, new { error = "Failed to retrieve configuration item", details = ex.Message });
    }
  }

  /// <summary>
  /// Get all configuration items by component.
  /// </summary>
  /// <param name="component">Component name.</param>
  /// <returns>Collection of configuration items for the specified component.</returns>
  [HttpGet("component/{component}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<ConfigurationItem>>> GetByComponent(string component)
  {
    try
    {
      var items = await _configService.LoadByComponentAsync(component);
      return Ok(items);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving configuration items for component {Component}", component);
      return StatusCode(500, new { error = "Failed to retrieve configuration items", details = ex.Message });
    }
  }

  /// <summary>
  /// Get all configuration items by category.
  /// </summary>
  /// <param name="category">Category name.</param>
  /// <returns>Collection of configuration items for the specified category.</returns>
  [HttpGet("category/{category}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<ConfigurationItem>>> GetByCategory(string category)
  {
    try
    {
      var items = await _configService.LoadByCategoryAsync(category);
      return Ok(items);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving configuration items for category {Category}", category);
      return StatusCode(500, new { error = "Failed to retrieve configuration items", details = ex.Message });
    }
  }

  /// <summary>
  /// Get all components defined in the configuration.
  /// </summary>
  /// <returns>Collection of component names.</returns>
  [HttpGet("components")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<string>>> GetComponents()
  {
    try
    {
      var components = await _configService.GetComponentsAsync();
      return Ok(components);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving component list");
      return StatusCode(500, new { error = "Failed to retrieve components", details = ex.Message });
    }
  }

  /// <summary>
  /// Create or update a configuration item.
  /// </summary>
  /// <param name="item">The configuration item to save.</param>
  /// <returns>200 OK if successful.</returns>
  [HttpPost]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Create([FromBody] ConfigurationItem item)
  {
    if (item == null || string.IsNullOrWhiteSpace(item.Component) || string.IsNullOrWhiteSpace(item.Key))
    {
      return BadRequest(new { error = "Component and Key are required" });
    }

    try
    {
      // Generate ID if not provided
      if (string.IsNullOrWhiteSpace(item.Id))
      {
        item.Id = $"{item.Component}_{item.Key}";
      }

      item.LastUpdated = DateTime.UtcNow;
      await _configService.SaveAsync(item);
      _logger.LogInformation("Configuration item saved: {Component}/{Key}", item.Component, item.Key);
      return Ok(new { message = "Configuration item saved successfully", item });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error saving configuration item {Component}/{Key}", item.Component, item.Key);
      return StatusCode(500, new { error = "Failed to save configuration item", details = ex.Message });
    }
  }

  /// <summary>
  /// Update an existing configuration item.
  /// </summary>
  /// <param name="component">Component name.</param>
  /// <param name="key">Configuration key.</param>
  /// <param name="item">The updated configuration item.</param>
  /// <returns>200 OK if successful, 404 if not found.</returns>
  [HttpPut("{component}/{key}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Update(string component, string key, [FromBody] ConfigurationItem item)
  {
    if (item == null)
    {
      return BadRequest(new { error = "Configuration item is required" });
    }

    try
    {
      var existing = await _configService.LoadAsync(component, key);
      if (existing == null)
      {
        return NotFound(new { error = "Configuration item not found", component, key });
      }

      // Preserve the component and key from the URL
      item.Component = component;
      item.Key = key;
      item.Id = existing.Id;
      item.LastUpdated = DateTime.UtcNow;

      await _configService.SaveAsync(item);
      _logger.LogInformation("Configuration item updated: {Component}/{Key}", component, key);
      return Ok(new { message = "Configuration item updated successfully", item });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error updating configuration item {Component}/{Key}", component, key);
      return StatusCode(500, new { error = "Failed to update configuration item", details = ex.Message });
    }
  }

  /// <summary>
  /// Delete a configuration item.
  /// </summary>
  /// <param name="component">Component name.</param>
  /// <param name="key">Configuration key.</param>
  /// <returns>200 OK if successful, 404 if not found.</returns>
  [HttpDelete("{component}/{key}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> Delete(string component, string key)
  {
    try
    {
      var exists = await _configService.ExistsAsync(component, key);
      if (!exists)
      {
        return NotFound(new { error = "Configuration item not found", component, key });
      }

      await _configService.DeleteAsync(component, key);
      _logger.LogInformation("Configuration item deleted: {Component}/{Key}", component, key);
      return Ok(new { message = "Configuration item deleted successfully", component, key });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error deleting configuration item {Component}/{Key}", component, key);
      return StatusCode(500, new { error = "Failed to delete configuration item", details = ex.Message });
    }
  }

  /// <summary>
  /// Create a backup of all configuration data.
  /// </summary>
  /// <param name="backupDirectory">Optional backup directory path.</param>
  /// <returns>The path to the created backup file.</returns>
  [HttpPost("backup")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> Backup([FromQuery] string? backupDirectory = null)
  {
    try
    {
      var backupPath = await _configService.BackupAsync(backupDirectory);
      _logger.LogInformation("Configuration backup created at {BackupPath}", backupPath);
      return Ok(new { message = "Backup created successfully", backupPath });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error creating configuration backup");
      return StatusCode(500, new { error = "Failed to create backup", details = ex.Message });
    }
  }

  /// <summary>
  /// Restore configuration data from a backup file.
  /// </summary>
  /// <param name="request">Restore request containing the backup file path.</param>
  /// <returns>200 OK if successful.</returns>
  [HttpPost("restore")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Restore([FromBody] RestoreRequest request)
  {
    if (string.IsNullOrWhiteSpace(request?.BackupPath))
    {
      return BadRequest(new { error = "Backup path is required" });
    }

    try
    {
      await _configService.RestoreAsync(request.BackupPath);
      _logger.LogInformation("Configuration restored from {BackupPath}", request.BackupPath);
      return Ok(new { message = "Configuration restored successfully", backupPath = request.BackupPath });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error restoring configuration from {BackupPath}", request.BackupPath);
      return StatusCode(500, new { error = "Failed to restore configuration", details = ex.Message });
    }
  }
}

/// <summary>
/// Request model for restore endpoint.
/// </summary>
public record RestoreRequest
{
  /// <summary>
  /// Path to the backup file to restore from.
  /// </summary>
  public string BackupPath { get; init; } = string.Empty;
}
