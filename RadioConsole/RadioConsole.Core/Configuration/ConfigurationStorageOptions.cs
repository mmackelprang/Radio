namespace RadioConsole.Core.Configuration;

/// <summary>
/// Configuration options for the configuration service storage.
/// </summary>
public class ConfigurationStorageOptions
{
  /// <summary>
  /// Gets or sets the storage directory path. Defaults to "./storage" if not specified.
  /// </summary>
  public string StoragePath { get; set; } = "./storage";

  /// <summary>
  /// Gets or sets the storage type (Json or SQLite). Defaults to Json.
  /// </summary>
  public string StorageType { get; set; } = "Json";

  /// <summary>
  /// Gets or sets the filename for JSON storage. Defaults to "config.json".
  /// </summary>
  public string JsonFileName { get; set; } = "config.json";

  /// <summary>
  /// Gets or sets the filename for SQLite storage. Defaults to "config.db".
  /// </summary>
  public string SqliteFileName { get; set; } = "config.db";
}
