namespace RadioConsole.Api.Interfaces;

/// <summary>
/// Interface for device configuration
/// </summary>
public interface IDeviceConfiguration
{
    /// <summary>
    /// Get all configuration keys
    /// </summary>
    IEnumerable<string> GetConfigurationKeys();

    /// <summary>
    /// Get a configuration value
    /// </summary>
    T? GetValue<T>(string key);

    /// <summary>
    /// Set a configuration value
    /// </summary>
    void SetValue<T>(string key, T value);

    /// <summary>
    /// Save configuration changes
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// Load configuration
    /// </summary>
    Task LoadAsync();
}
