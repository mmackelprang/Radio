using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Models;

namespace RadioConsole.Api.Services;

/// <summary>
/// Interface for device registry service
/// </summary>
public interface IDeviceRegistry
{
    /// <summary>
    /// Load all device configurations from storage
    /// </summary>
    Task LoadConfigurationsAsync();

    /// <summary>
    /// Add a new input device configuration
    /// </summary>
    Task<DeviceConfiguration> AddInputAsync(DeviceConfiguration config);

    /// <summary>
    /// Add a new output device configuration
    /// </summary>
    Task<DeviceConfiguration> AddOutputAsync(DeviceConfiguration config);

    /// <summary>
    /// Update an existing input device configuration
    /// </summary>
    Task<DeviceConfiguration?> UpdateInputAsync(string id, DeviceConfiguration config);

    /// <summary>
    /// Update an existing output device configuration
    /// </summary>
    Task<DeviceConfiguration?> UpdateOutputAsync(string id, DeviceConfiguration config);

    /// <summary>
    /// Remove an input device configuration
    /// </summary>
    Task<bool> RemoveInputAsync(string id);

    /// <summary>
    /// Remove an output device configuration
    /// </summary>
    Task<bool> RemoveOutputAsync(string id);

    /// <summary>
    /// Get all input configurations
    /// </summary>
    IEnumerable<DeviceConfiguration> GetAllInputConfigs();

    /// <summary>
    /// Get all output configurations
    /// </summary>
    IEnumerable<DeviceConfiguration> GetAllOutputConfigs();

    /// <summary>
    /// Get a specific input configuration
    /// </summary>
    DeviceConfiguration? GetInputConfig(string id);

    /// <summary>
    /// Get a specific output configuration
    /// </summary>
    DeviceConfiguration? GetOutputConfig(string id);

    /// <summary>
    /// Get all loaded input devices
    /// </summary>
    IEnumerable<IAudioInput> GetAllInputs();

    /// <summary>
    /// Get all loaded output devices
    /// </summary>
    IEnumerable<IAudioOutput> GetAllOutputs();

    /// <summary>
    /// Get a specific loaded input device
    /// </summary>
    IAudioInput? GetInput(string id);

    /// <summary>
    /// Get a specific loaded output device
    /// </summary>
    IAudioOutput? GetOutput(string id);
}
