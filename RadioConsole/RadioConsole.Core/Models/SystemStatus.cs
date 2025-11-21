namespace RadioConsole.Core.Models;

/// <summary>
/// Represents system status information including API server details and system metrics.
/// </summary>
public class SystemStatus
{
  /// <summary>
  /// API server URL.
  /// </summary>
  public string ApiUrl { get; set; } = string.Empty;

  /// <summary>
  /// API server port.
  /// </summary>
  public int ApiPort { get; set; }

  /// <summary>
  /// Web server URL.
  /// </summary>
  public string WebUrl { get; set; } = string.Empty;

  /// <summary>
  /// Web server port.
  /// </summary>
  public int WebPort { get; set; }

  /// <summary>
  /// System uptime in seconds.
  /// </summary>
  public long UptimeSeconds { get; set; }

  /// <summary>
  /// CPU usage percentage (0-100).
  /// </summary>
  public double CpuUsagePercent { get; set; }

  /// <summary>
  /// Total memory in bytes.
  /// </summary>
  public long TotalMemoryBytes { get; set; }

  /// <summary>
  /// Used memory in bytes.
  /// </summary>
  public long UsedMemoryBytes { get; set; }

  /// <summary>
  /// Available memory in bytes.
  /// </summary>
  public long AvailableMemoryBytes { get; set; }

  /// <summary>
  /// Total disk space in bytes.
  /// </summary>
  public long TotalDiskBytes { get; set; }

  /// <summary>
  /// Used disk space in bytes.
  /// </summary>
  public long UsedDiskBytes { get; set; }

  /// <summary>
  /// Available disk space in bytes.
  /// </summary>
  public long AvailableDiskBytes { get; set; }

  /// <summary>
  /// Operating system name.
  /// </summary>
  public string OperatingSystem { get; set; } = string.Empty;

  /// <summary>
  /// .NET runtime version.
  /// </summary>
  public string RuntimeVersion { get; set; } = string.Empty;

  /// <summary>
  /// Machine name/hostname.
  /// </summary>
  public string MachineName { get; set; } = string.Empty;

  /// <summary>
  /// Number of processor cores.
  /// </summary>
  public int ProcessorCount { get; set; }
}
