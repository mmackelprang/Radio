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
  private static bool? _isAudioHardwareAvailable;

  /// <summary>
  /// Determines whether audio playback hardware appears to be available.
  /// Attempts SoundFlow device enumeration and applies lightweight OS heuristics.
  /// Set environment variable RADIO_FORCE_HW_AVAILABLE=1 to force true.
  /// </summary>
  /// <returns>True if audio hardware is likely available; false otherwise.</returns>
  public static bool AudioHardwareAvailable()
  {
    if (_isAudioHardwareAvailable.HasValue)
    {
      return _isAudioHardwareAvailable.Value;
    }

    var force = Environment.GetEnvironmentVariable("RADIO_FORCE_HW_AVAILABLE");
    if (!string.IsNullOrEmpty(force) && (force == "1" || force.Equals("true", StringComparison.OrdinalIgnoreCase)))
    {
      _isAudioHardwareAvailable = true;
      return true;
    }

    // Try SoundFlow initialization test - not just enumeration
    try
    {
      using var player = new SoundFlowAudioPlayer(new NullLogger<SoundFlowAudioPlayer>());
      player.InitializeAsync("default").GetAwaiter().GetResult();
      if (player.IsInitialized)
      {
        player.Dispose();
        _isAudioHardwareAvailable = true;
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
            _isAudioHardwareAvailable = true;
            return true;
          }
        }
      }
      else if (OperatingSystem.IsWindows())
      {
        // On Windows assume hardware unless running in certain CI containers where enumeration failed.
        // Provide minimal sentinel: absence of common system directory would be unusual.
        _isAudioHardwareAvailable = Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
        return _isAudioHardwareAvailable.Value;
      }
      else if (OperatingSystem.IsMacOS())
      {
        // macOS generally exposes CoreAudio; assume available if /System exists.
        _isAudioHardwareAvailable = Directory.Exists("/System");
        return _isAudioHardwareAvailable.Value;
      }
    }
    catch
    {
      // Ignore failures and treat as unavailable
    }

    _isAudioHardwareAvailable = false;
    return false;
  }
}
