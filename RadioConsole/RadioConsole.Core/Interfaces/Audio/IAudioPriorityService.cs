using RadioConsole.Core.Enums;

namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Service for managing audio priority and ducking behavior.
/// When high priority audio plays, low priority audio is automatically ducked (volume reduced).
/// </summary>
public interface IAudioPriorityService
{
  /// <summary>
  /// Register an audio source with a specific priority level.
  /// </summary>
  /// <param name="sourceId">Unique identifier for the audio source.</param>
  /// <param name="priority">Priority level for this source.</param>
  Task RegisterSourceAsync(string sourceId, AudioPriority priority);

  /// <summary>
  /// Unregister an audio source.
  /// </summary>
  /// <param name="sourceId">Unique identifier for the audio source.</param>
  Task UnregisterSourceAsync(string sourceId);

  /// <summary>
  /// Notify that a high priority audio source is starting playback.
  /// This will duck (reduce volume) all low priority sources.
  /// </summary>
  /// <param name="sourceId">The high priority source that is starting.</param>
  Task OnHighPriorityStartAsync(string sourceId);

  /// <summary>
  /// Notify that a high priority audio source has finished playback.
  /// This will restore volume to low priority sources if no other high priority sources are active.
  /// </summary>
  /// <param name="sourceId">The high priority source that is finishing.</param>
  Task OnHighPriorityEndAsync(string sourceId);

  /// <summary>
  /// Get the configured duck percentage (0.0 to 1.0).
  /// Default is 0.2 (20%).
  /// </summary>
  float DuckPercentage { get; }

  /// <summary>
  /// Set the duck percentage (0.0 to 1.0).
  /// </summary>
  /// <param name="percentage">The volume percentage to duck to (0.0 to 1.0).</param>
  Task SetDuckPercentageAsync(float percentage);

  /// <summary>
  /// Check if high priority audio is currently active.
  /// </summary>
  bool IsHighPriorityActive { get; }
}
