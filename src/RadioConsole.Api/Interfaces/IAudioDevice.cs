namespace RadioConsole.Api.Interfaces;

/// <summary>
/// Base interface for all audio devices (inputs and outputs)
/// Contains common properties shared by both audio inputs and outputs
/// </summary>
public interface IAudioDevice
{
  /// <summary>
  /// Unique identifier for this device
  /// </summary>
  string Id { get; }

  /// <summary>
  /// Display name for this device
  /// </summary>
  string Name { get; }

  /// <summary>
  /// Description of this device
  /// </summary>
  string Description { get; }

  /// <summary>
  /// Whether this device is currently available
  /// </summary>
  bool IsAvailable { get; }

  /// <summary>
  /// Whether this device is currently active
  /// </summary>
  bool IsActive { get; }

  /// <summary>
  /// Initialize the device
  /// </summary>
  Task InitializeAsync();

  /// <summary>
  /// Start the device
  /// </summary>
  Task StartAsync();

  /// <summary>
  /// Stop the device
  /// </summary>
  Task StopAsync();

  /// <summary>
  /// Get the configuration interface for this device
  /// </summary>
  IDeviceConfiguration GetConfiguration();

  /// <summary>
  /// Get the display interface for this device
  /// </summary>
  IDisplay GetDisplay();
}
