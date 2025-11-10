using RadioConsole.Api.Interfaces;

namespace RadioConsole.Api.Services;

/// <summary>
/// Interface for dynamic device factory service
/// </summary>
public interface IDeviceFactory
{
    /// <summary>
    /// Create an audio input from a device configuration
    /// </summary>
    IAudioInput CreateInput(Models.DeviceConfiguration config);

    /// <summary>
    /// Create an audio output from a device configuration
    /// </summary>
    IAudioOutput CreateOutput(Models.DeviceConfiguration config);

    /// <summary>
    /// Get information about available device types
    /// </summary>
    IEnumerable<Models.DeviceTypeInfo> GetAvailableInputTypes();

    /// <summary>
    /// Get information about available device types
    /// </summary>
    IEnumerable<Models.DeviceTypeInfo> GetAvailableOutputTypes();
}
