namespace RadioConsole.Core.Enums;

/// <summary>
/// Transition types for crossfade operations.
/// </summary>
public enum TransitionType
{
  /// <summary>
  /// Standard crossfade where outgoing fades out while incoming fades in.
  /// </summary>
  Crossfade = 0,

  /// <summary>
  /// Gradual fade in from silence.
  /// </summary>
  FadeIn = 1,

  /// <summary>
  /// Gradual fade out to silence.
  /// </summary>
  FadeOut = 2,

  /// <summary>
  /// Instant cut with no transition - used for emergency alerts.
  /// </summary>
  Cut = 3,

  /// <summary>
  /// Gapless transition for continuous streams.
  /// </summary>
  Gapless = 4
}
