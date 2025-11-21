namespace RadioConsole.Core.Interfaces.Inputs;

/// <summary>
/// Interface for managing the Raddy RF320 radio.
/// Handles USB Audio routing and control (BLE control to be added in future phase).
/// </summary>
public interface IRaddyRadioService
{
  /// <summary>
  /// Initialize the Raddy radio service and identify the USB Audio device.
  /// </summary>
  Task InitializeAsync();

  /// <summary>
  /// Start streaming audio from the Raddy radio to the audio player.
  /// </summary>
  Task StartAsync();

  /// <summary>
  /// Stop streaming audio from the Raddy radio.
  /// </summary>
  Task StopAsync();

  /// <summary>
  /// Get the USB Audio device ID associated with the Raddy RF320.
  /// </summary>
  /// <returns>The device ID, or null if not found.</returns>
  string? GetDeviceId();

  /// <summary>
  /// Check if the Raddy radio is currently streaming.
  /// </summary>
  bool IsStreaming { get; }

  /// <summary>
  /// Check if the Raddy radio USB Audio device is detected.
  /// </summary>
  bool IsDeviceDetected { get; }

  /// <summary>
  /// Get the current signal strength (0-6 scale).
  /// 0 = No Signal, 1 = Very Weak, 2 = Weak, 3 = Fair, 4 = Good, 5 = Very Good, 6 = Excellent
  /// </summary>
  int SignalStrength { get; }

  /// <summary>
  /// Get the current frequency (placeholder for future BLE control).
  /// </summary>
  Task<double?> GetFrequencyAsync();

  /// <summary>
  /// Set the radio frequency (placeholder for future BLE control).
  /// </summary>
  /// <param name="frequencyMHz">Frequency in MHz.</param>
  Task SetFrequencyAsync(double frequencyMHz);
}
