namespace RadioConsole.Core.Enums;

/// <summary>
/// Predefined ducking presets for common use cases.
/// </summary>
public enum DuckingPreset
{
  /// <summary>
  /// Aggressive ducking for clear voice announcements.
  /// Fast attack (50ms), slow release (2s), deep ducking (80% reduction).
  /// </summary>
  DJMode = 0,

  /// <summary>
  /// Subtle ducking for ambient background audio.
  /// Slow attack (200ms), slow release (2s), light ducking (40% reduction).
  /// </summary>
  BackgroundMode = 1,

  /// <summary>
  /// Mutes all audio except emergency/alert sounds.
  /// Instant attack, hold until event ends, 100% ducking.
  /// </summary>
  EmergencyMode = 2,

  /// <summary>
  /// Minimal ducking to prioritize music listening.
  /// Slow attack (300ms), very slow release (3s), light ducking (30% reduction).
  /// </summary>
  MusicMode = 3,

  /// <summary>
  /// Custom user-defined ducking settings.
  /// Uses manually configured values.
  /// </summary>
  Custom = 4
}
