using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces;

namespace RadioConsole.Infrastructure.Configuration;

/// <summary>
/// Factory for creating configuration service instances based on storage type.
/// </summary>
public static class ConfigurationServiceFactory
{
  /// <summary>
  /// Creates a configuration service instance based on the specified storage type.
  /// </summary>
  /// <param name="storageType">The type of storage to use.</param>
  /// <param name="storagePath">The path to the storage file (JSON file or SQLite database).</param>
  /// <returns>An instance of IConfigurationService.</returns>
  public static IConfigurationService Create(StorageType storageType, string storagePath)
  {
    return storageType switch
    {
      StorageType.Json => new JsonConfigurationService(storagePath),
      StorageType.SQLite => new SqliteConfigurationService(storagePath),
      _ => throw new ArgumentException($"Unsupported storage type: {storageType}", nameof(storageType))
    };
  }
}
