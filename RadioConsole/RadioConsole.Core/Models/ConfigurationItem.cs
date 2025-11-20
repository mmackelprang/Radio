namespace RadioConsole.Core.Models;

/// <summary>
/// Represents a configuration item with a key-value pair.
/// </summary>
public class ConfigurationItem
{
  /// <summary>
  /// Gets or sets the unique identifier for this configuration item.
  /// </summary>
  public string Id { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the component this configuration belongs to (e.g., Spotify, TTS, Audio).
  /// </summary>
  public string Component { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the configuration key.
  /// </summary>
  public string Key { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the configuration value.
  /// </summary>
  public string Value { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the category for grouping related configuration items.
  /// </summary>
  public string Category { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the timestamp when this configuration was last updated.
  /// </summary>
  public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
