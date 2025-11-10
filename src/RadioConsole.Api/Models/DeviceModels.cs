namespace RadioConsole.Api.Models;

/// <summary>
/// Represents a configured audio device
/// </summary>
public class DeviceConfiguration
{
    /// <summary>
    /// Unique identifier for this device configuration
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Type of device (UsbAudioInput, FileAudioInput, WiredSoundbarOutput, etc.)
    /// </summary>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// User-defined display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User-defined description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Device-specific configuration parameters
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// When this device configuration was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this device configuration was last modified
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this device is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Request to add or update a device configuration
/// </summary>
public class DeviceConfigurationRequest
{
    /// <summary>
    /// Type of device
    /// </summary>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Device-specific configuration parameters
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Whether this device is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Response containing device configuration details
/// </summary>
public class DeviceConfigurationResponse
{
    public string Id { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public bool IsEnabled { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}

/// <summary>
/// Describes available device types and their configuration parameters
/// </summary>
public class DeviceTypeInfo
{
    public string TypeName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // "Input" or "Output"
    public List<DeviceParameterInfo> Parameters { get; set; } = new();
}

/// <summary>
/// Describes a configuration parameter for a device type
/// </summary>
public class DeviceParameterInfo
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty; // "string", "int", "bool", etc.
    public bool Required { get; set; }
    public object? DefaultValue { get; set; }
}
