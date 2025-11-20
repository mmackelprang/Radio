using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RadioConsole.Core.Configuration;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces;

namespace RadioConsole.Infrastructure.Configuration;

/// <summary>
/// Extension methods for registering configuration services.
/// </summary>
public static class ConfigurationServiceExtensions
{
  /// <summary>
  /// Adds the configuration service to the service collection using settings from appsettings.json.
  /// </summary>
  /// <param name="services">The service collection.</param>
  /// <param name="configuration">The configuration instance.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection AddConfigurationService(
    this IServiceCollection services, 
    IConfiguration configuration)
  {
    // Bind configuration options
    var storageOptions = new ConfigurationStorageOptions();
    configuration.GetSection("ConfigurationStorage").Bind(storageOptions);

    // Set RootDir to base directory if not specified
    if (string.IsNullOrEmpty(storageOptions.RootDir))
    {
      storageOptions.RootDir = AppDomain.CurrentDomain.BaseDirectory;
    }
    
    // Resolve storage path relative to RootDir
    var resolvedStoragePath = storageOptions.ResolvePath(storageOptions.StoragePath);
    
    // Register options
    services.Configure<ConfigurationStorageOptions>(
      configuration.GetSection("ConfigurationStorage"));

    // Ensure storage directory exists
    if (!Directory.Exists(resolvedStoragePath))
    {
      Directory.CreateDirectory(resolvedStoragePath);
      Console.WriteLine($"Created storage directory: {resolvedStoragePath}");
    }

    // Determine storage type
    var storageType = Enum.TryParse<StorageType>(storageOptions.StorageType, true, out var type)
      ? type
      : StorageType.Json;

    // Build storage file path
    var fileName = storageType == StorageType.Json 
      ? storageOptions.JsonFileName 
      : storageOptions.SqliteFileName;
    var storagePath = Path.Combine(resolvedStoragePath, fileName);

    Console.WriteLine($"Configuration storage path: {storagePath}");
    Console.WriteLine($"Configuration storage type: {storageType}");

    // Register the configuration service as singleton
    services.AddSingleton<IConfigurationService>(sp => 
      ConfigurationServiceFactory.Create(storageType, storagePath));

    return services;
  }
}
