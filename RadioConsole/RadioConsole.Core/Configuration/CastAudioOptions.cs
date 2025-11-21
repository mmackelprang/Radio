namespace RadioConsole.Core.Configuration;

/// <summary>
/// Configuration options for Google Cast audio output.
/// </summary>
public class CastAudioOptions
{
  /// <summary>
  /// Preferred Cast device name. If specified, the system will attempt to connect
  /// to a device whose friendly name contains this string (case-insensitive).
  /// </summary>
  public string? PreferredDeviceName { get; set; }

  /// <summary>
  /// Whether to automatically select the first discovered device if the preferred
  /// device is not found or not specified. Default is true.
  /// </summary>
  public bool AutoSelectFirst { get; set; } = true;

  /// <summary>
  /// Discovery timeout in seconds. Default is 5 seconds.
  /// </summary>
  public double DiscoveryTimeoutSeconds { get; set; } = 5;

  /// <summary>
  /// Enable reconnection attempts if connection is lost. Default is true.
  /// </summary>
  public bool EnableReconnection { get; set; } = true;

  /// <summary>
  /// Maximum number of reconnection attempts. Default is 5.
  /// </summary>
  public int MaxReconnectionAttempts { get; set; } = 5;

  /// <summary>
  /// Base delay in seconds for exponential backoff between reconnection attempts.
  /// Default is 2 seconds.
  /// </summary>
  public int ReconnectionBaseDelaySeconds { get; set; } = 2;
}
