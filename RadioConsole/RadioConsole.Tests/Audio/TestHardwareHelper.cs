using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using RadioConsole.Infrastructure.Audio;

namespace RadioConsole.Tests.Audio;

/// <summary>
/// Provides hardware detection helpers for conditionally executing audio integration tests.
/// </summary>
public static class TestHardwareHelper
{
  /// <summary>
  /// Determines whether audio playback hardware appears to be available.
  /// Attempts SoundFlow device enumeration and applies lightweight OS heuristics.
  /// Set environment variable RADIO_FORCE_HW_AVAILABLE=1 to force true.
  /// </summary>
  /// <returns>True if audio hardware is likely available; false otherwise.</returns>
  public static bool AudioHardwareAvailable()
  {
    var force = Environment.GetEnvironmentVariable("RADIO_FORCE_HW_AVAILABLE");
    if (!string.IsNullOrEmpty(force) && (force == "1" || force.Equals("true", StringComparison.OrdinalIgnoreCase)))
    {
      return true;
    }

    // Try SoundFlow enumeration first
    try
    {
      using var manager = new SoundFlowAudioDeviceManager(new NullLogger<SoundFlowAudioDeviceManager>());
      var outputs = manager.GetOutputDevicesAsync().GetAwaiter().GetResult();
      if (outputs != null && outputs.Any())
      {
        return true;
      }
    }
    catch
    {
      // Ignore and fall through to heuristics
    }

    // OS-specific heuristic fallbacks
    try
    {
      if (OperatingSystem.IsLinux())
      {
        // Presence of ALSA devices
        if (Directory.Exists("/proc/asound"))
        {
          var cardFiles = Directory.GetFiles("/proc/asound", "card*", SearchOption.TopDirectoryOnly);
          if (cardFiles.Length > 0)
          {
            return true;
          }
        }
      }
      else if (OperatingSystem.IsWindows())
      {
        // On Windows assume hardware unless running in certain CI containers where enumeration failed.
        // Provide minimal sentinel: absence of common system directory would be unusual.
        return Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
      }
      else if (OperatingSystem.IsMacOS())
      {
        // macOS generally exposes CoreAudio; assume available if /System exists.
        return Directory.Exists("/System");
      }
    }
    catch
    {
      // Ignore failures and treat as unavailable
    }

    return false;
  }
}
