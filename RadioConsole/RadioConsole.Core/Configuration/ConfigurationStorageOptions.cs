namespace RadioConsole.Core.Configuration;

/// <summary>
/// Configuration options for the configuration service storage.
/// </summary>
public class ConfigurationStorageOptions
{
  /// <summary>
  /// Gets or sets the root directory for the application.
  /// All relative paths will be resolved relative to this directory.
  /// Defaults to the application's base directory if not specified.
  /// </summary>
  public string RootDir { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the storage directory path relative to RootDir.
  /// Defaults to "./storage" if not specified.
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

  /// <summary>
  /// Resolves a path relative to RootDir.
  /// If the path is already absolute, returns it as-is.
  /// </summary>
  public string ResolvePath(string relativePath)
  {
    if (Path.IsPathRooted(relativePath))
    {
      return relativePath;
    }

    string baseDir = string.IsNullOrEmpty(RootDir) ? AppDomain.CurrentDomain.BaseDirectory : RootDir;
    return Path.GetFullPath(Path.Combine(baseDir, relativePath));
  }
}
