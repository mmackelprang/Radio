using Microsoft.AspNetCore.Mvc;
using RadioConsole.Core.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RadioConsole.API.Controllers;

/// <summary>
/// API controller for system status and diagnostics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SystemStatusController : ControllerBase
{
  private readonly ILogger<SystemStatusController> _logger;
  private readonly IConfiguration _configuration;
  private static readonly DateTime _startTime = DateTime.UtcNow;

  public SystemStatusController(ILogger<SystemStatusController> logger, IConfiguration configuration)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
  }

  /// <summary>
  /// Get comprehensive system status information.
  /// </summary>
  /// <returns>System status details.</returns>
  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<SystemStatus>> GetStatus()
  {
    try
    {
      var status = new SystemStatus
      {
        ApiUrl = _configuration["Kestrel:Endpoints:Http:Url"] ?? "http://0.0.0.0:5100",
        ApiPort = 5100,
        WebUrl = "http://0.0.0.0:5200", // This is a placeholder; in production, this might come from config
        WebPort = 5200,
        UptimeSeconds = (long)(DateTime.UtcNow - _startTime).TotalSeconds,
        CpuUsagePercent = await GetCpuUsageAsync(),
        TotalMemoryBytes = GetTotalMemory(),
        UsedMemoryBytes = GetUsedMemory(),
        AvailableMemoryBytes = GetAvailableMemory(),
        TotalDiskBytes = GetTotalDiskSpace(),
        UsedDiskBytes = GetUsedDiskSpace(),
        AvailableDiskBytes = GetAvailableDiskSpace(),
        OperatingSystem = RuntimeInformation.OSDescription,
        RuntimeVersion = RuntimeInformation.FrameworkDescription,
        MachineName = Environment.MachineName,
        ProcessorCount = Environment.ProcessorCount
      };

      return Ok(status);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting system status");
      return StatusCode(500, new { error = "Failed to get system status", details = ex.Message });
    }
  }

  /// <summary>
  /// Get uptime in a human-readable format.
  /// </summary>
  /// <returns>Uptime string.</returns>
  [HttpGet("uptime")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public ActionResult<object> GetUptime()
  {
    try
    {
      var uptime = DateTime.UtcNow - _startTime;
      var uptimeString = $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";

      return Ok(new
      {
        uptimeSeconds = (long)uptime.TotalSeconds,
        uptimeString = uptimeString,
        startTime = _startTime,
        currentTime = DateTime.UtcNow
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting uptime");
      return StatusCode(500, new { error = "Failed to get uptime", details = ex.Message });
    }
  }

  private async Task<double> GetCpuUsageAsync()
  {
    try
    {
      using var process = Process.GetCurrentProcess();
      var startTime = DateTime.UtcNow;
      var startCpuUsage = process.TotalProcessorTime;

      // Sample CPU over a short period
      await Task.Delay(100);

      var endTime = DateTime.UtcNow;
      var endCpuUsage = process.TotalProcessorTime;

      var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
      var totalMsPassed = (endTime - startTime).TotalMilliseconds;
      var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

      return cpuUsageTotal * 100;
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Error getting CPU usage");
      return 0;
    }
  }

  private long GetTotalMemory()
  {
    try
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
      {
        var memInfo = System.IO.File.ReadAllText("/proc/meminfo");
        var totalLine = memInfo.Split('\n').FirstOrDefault(l => l.StartsWith("MemTotal:"));
        if (totalLine != null)
        {
          var parts = totalLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
          if (parts.Length >= 2 && long.TryParse(parts[1], out var totalKb))
          {
            return totalKb * 1024; // Convert KB to bytes
          }
        }
      }

      // Fallback: use GC information
      return GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Error getting total memory");
      return 0;
    }
  }

  private long GetUsedMemory()
  {
    try
    {
      using var process = Process.GetCurrentProcess();
      return process.WorkingSet64;
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Error getting used memory");
      return 0;
    }
  }

  private long GetAvailableMemory()
  {
    try
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
      {
        var memInfo = System.IO.File.ReadAllText("/proc/meminfo");
        var availableLine = memInfo.Split('\n').FirstOrDefault(l => l.StartsWith("MemAvailable:"));
        if (availableLine != null)
        {
          var parts = availableLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
          if (parts.Length >= 2 && long.TryParse(parts[1], out var availableKb))
          {
            return availableKb * 1024; // Convert KB to bytes
          }
        }
      }

      return GetTotalMemory() - GetUsedMemory();
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Error getting available memory");
      return 0;
    }
  }

  private long GetTotalDiskSpace()
  {
    try
    {
      var drive = new DriveInfo(Path.GetPathRoot(AppContext.BaseDirectory) ?? "/");
      return drive.TotalSize;
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Error getting total disk space");
      return 0;
    }
  }

  private long GetUsedDiskSpace()
  {
    try
    {
      var drive = new DriveInfo(Path.GetPathRoot(AppContext.BaseDirectory) ?? "/");
      return drive.TotalSize - drive.AvailableFreeSpace;
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Error getting used disk space");
      return 0;
    }
  }

  private long GetAvailableDiskSpace()
  {
    try
    {
      var drive = new DriveInfo(Path.GetPathRoot(AppContext.BaseDirectory) ?? "/");
      return drive.AvailableFreeSpace;
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Error getting available disk space");
      return 0;
    }
  }
}
