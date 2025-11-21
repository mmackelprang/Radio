using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    // Create a logger factory for configuration-time logging
    using var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
    {
      builder.AddConsole();
    });
    var logger = loggerFactory.CreateLogger("ConfigurationService");

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
      logger.LogInformation("Created storage directory: {Path}", resolvedStoragePath);
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

    logger.LogInformation("Configuration storage path: {Path}", storagePath);
    logger.LogInformation("Configuration storage type: {Type}", storageType);

    // Register the configuration service as singleton
    services.AddSingleton<IConfigurationService>(sp => 
      ConfigurationServiceFactory.Create(storageType, storagePath));

    return services;
  }
}
