using RadioConsole.Core.Configuration;
using RadioConsole.Core.Enums;

namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Controller for managing audio crossfades and transitions.
/// </summary>
public interface ICrossfadeController : IDisposable
{
  /// <summary>
  /// Whether the crossfade controller is initialized.
  /// </summary>
  bool IsInitialized { get; }

  /// <summary>
  /// Whether a crossfade is currently in progress.
  /// </summary>
  bool IsTransitionInProgress { get; }

  /// <summary>
  /// Gets the current crossfade progress (0.0 to 1.0).
  /// Returns 0 if no transition is in progress.
  /// </summary>
  float CurrentProgress { get; }

  /// <summary>
  /// Initializes the crossfade controller with configuration.
  /// </summary>
  /// <param name="configuration">Crossfade configuration.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  Task InitializeAsync(CrossfadeConfiguration configuration, CancellationToken cancellationToken = default);

  /// <summary>
  /// Starts a crossfade between two sources.
  /// </summary>
  /// <param name="outgoingSourceId">Source to fade out.</param>
  /// <param name="incomingSourceId">Source to fade in.</param>
  /// <param name="durationMs">Duration in milliseconds.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  Task CrossfadeAsync(string outgoingSourceId, string incomingSourceId, int durationMs, CancellationToken cancellationToken = default);

  /// <summary>
  /// Starts a fade-in for a source.
  /// </summary>
  /// <param name="sourceId">Source to fade in.</param>
  /// <param name="durationMs">Duration in milliseconds.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  Task FadeInAsync(string sourceId, int durationMs, CancellationToken cancellationToken = default);

  /// <summary>
  /// Starts a fade-out for a source.
  /// </summary>
  /// <param name="sourceId">Source to fade out.</param>
  /// <param name="durationMs">Duration in milliseconds.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  Task FadeOutAsync(string sourceId, int durationMs, CancellationToken cancellationToken = default);

  /// <summary>
  /// Performs an emergency cut - instant switch with no fade.
  /// </summary>
  /// <param name="outgoingSourceId">Source to cut (or null to cut current).</param>
  /// <param name="incomingSourceId">Source to start (or null for silence).</param>
  Task EmergencyCutAsync(string? outgoingSourceId = null, string? incomingSourceId = null);

  /// <summary>
  /// Cancels any in-progress transition.
  /// </summary>
  Task CancelTransitionAsync();

  /// <summary>
  /// Updates the crossfade configuration.
  /// </summary>
  /// <param name="configuration">New configuration.</param>
  Task UpdateConfigurationAsync(CrossfadeConfiguration configuration);

  /// <summary>
  /// Gets the current configuration.
  /// </summary>
  CrossfadeConfiguration GetConfiguration();

  /// <summary>
  /// Event raised when a transition starts.
  /// </summary>
  event EventHandler<CrossfadeEventArgs>? TransitionStarted;

  /// <summary>
  /// Event raised when a transition completes.
  /// </summary>
  event EventHandler<CrossfadeEventArgs>? TransitionCompleted;

  /// <summary>
  /// Event raised when transition progress updates.
  /// </summary>
  event EventHandler<CrossfadeProgressEventArgs>? TransitionProgress;

  /// <summary>
  /// Event raised when a transition is cancelled.
  /// </summary>
  event EventHandler<CrossfadeEventArgs>? TransitionCancelled;
}

/// <summary>
/// Event arguments for crossfade events.
/// </summary>
public class CrossfadeEventArgs : EventArgs
{
  /// <summary>
  /// The outgoing source ID.
  /// </summary>
  public string? OutgoingSourceId { get; set; }

  /// <summary>
  /// The incoming source ID.
  /// </summary>
  public string? IncomingSourceId { get; set; }

  /// <summary>
  /// The type of transition.
  /// </summary>
  public TransitionType TransitionType { get; set; }

  /// <summary>
  /// Duration of the transition in milliseconds.
  /// </summary>
  public int DurationMs { get; set; }

  /// <summary>
  /// When the event occurred.
  /// </summary>
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event arguments for crossfade progress updates.
/// </summary>
public class CrossfadeProgressEventArgs : EventArgs
{
  /// <summary>
  /// Current progress (0.0 to 1.0).
  /// </summary>
  public float Progress { get; set; }

  /// <summary>
  /// Current volume of outgoing source.
  /// </summary>
  public float OutgoingVolume { get; set; }

  /// <summary>
  /// Current volume of incoming source.
  /// </summary>
  public float IncomingVolume { get; set; }

  /// <summary>
  /// Elapsed time in milliseconds.
  /// </summary>
  public int ElapsedMs { get; set; }

  /// <summary>
  /// Remaining time in milliseconds.
  /// </summary>
  public int RemainingMs { get; set; }
}
