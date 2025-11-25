using RadioConsole.Core.Configuration;
using RadioConsole.Core.Enums;

namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Service for managing audio ducking behavior.
/// Coordinates volume reduction of lower priority channels when higher priority audio plays.
/// </summary>
public interface IDuckingService : IDisposable
{
  /// <summary>
  /// Whether the ducking service is initialized and ready.
  /// </summary>
  bool IsInitialized { get; }

  /// <summary>
  /// Whether ducking is globally enabled.
  /// </summary>
  bool IsEnabled { get; }

  /// <summary>
  /// The currently active ducking preset.
  /// </summary>
  DuckingPreset ActivePreset { get; }

  /// <summary>
  /// Whether any channel is currently being ducked.
  /// </summary>
  bool IsDuckingActive { get; }

  /// <summary>
  /// Gets the current ducking status for a specific channel.
  /// </summary>
  /// <param name="channel">The channel to check.</param>
  /// <returns>Current ducking status.</returns>
  DuckingStatus GetChannelDuckingStatus(MixerChannel channel);

  /// <summary>
  /// Initializes the ducking service with the specified configuration.
  /// </summary>
  /// <param name="configuration">Ducking configuration to use.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  Task InitializeAsync(DuckingConfiguration configuration, CancellationToken cancellationToken = default);

  /// <summary>
  /// Starts ducking the target channel due to audio on the trigger channel.
  /// </summary>
  /// <param name="triggerChannel">The channel that triggered the ducking.</param>
  /// <param name="sourceId">The source ID that triggered the ducking.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  Task StartDuckingAsync(MixerChannel triggerChannel, string sourceId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Ends ducking that was triggered by a specific source.
  /// </summary>
  /// <param name="sourceId">The source ID that triggered the ducking.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  Task EndDuckingAsync(string sourceId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Applies ducking immediately without smooth transition (for emergencies).
  /// </summary>
  /// <param name="triggerChannel">The channel that triggered the ducking.</param>
  /// <param name="sourceId">The source ID.</param>
  Task ApplyEmergencyDuckAsync(MixerChannel triggerChannel, string sourceId);

  /// <summary>
  /// Sets the active ducking preset.
  /// </summary>
  /// <param name="preset">The preset to activate.</param>
  Task SetPresetAsync(DuckingPreset preset);

  /// <summary>
  /// Updates the ducking configuration.
  /// </summary>
  /// <param name="configuration">New configuration to apply.</param>
  Task UpdateConfigurationAsync(DuckingConfiguration configuration);

  /// <summary>
  /// Gets the current ducking configuration.
  /// </summary>
  DuckingConfiguration GetConfiguration();

  /// <summary>
  /// Gets settings for a specific channel pair.
  /// </summary>
  /// <param name="triggerChannel">The trigger channel.</param>
  /// <param name="targetChannel">The target channel.</param>
  /// <returns>Channel pair settings, or null if not configured.</returns>
  ChannelPairDuckingSettings? GetChannelPairSettings(MixerChannel triggerChannel, MixerChannel targetChannel);

  /// <summary>
  /// Updates settings for a specific channel pair.
  /// </summary>
  /// <param name="settings">New settings to apply.</param>
  Task UpdateChannelPairSettingsAsync(ChannelPairDuckingSettings settings);

  /// <summary>
  /// Enables or disables ducking globally.
  /// </summary>
  /// <param name="enabled">Whether ducking should be enabled.</param>
  Task SetEnabledAsync(bool enabled);

  /// <summary>
  /// Gets performance metrics for the ducking system.
  /// </summary>
  DuckingMetrics GetMetrics();

  /// <summary>
  /// Resets all ducking state and restores original volumes.
  /// </summary>
  Task ResetAsync();

  /// <summary>
  /// Event raised when ducking state changes for any channel.
  /// </summary>
  event EventHandler<DuckingEventArgs>? DuckingStateChanged;

  /// <summary>
  /// Event raised when ducking configuration changes.
  /// </summary>
  event EventHandler<DuckingConfigurationChangedEventArgs>? ConfigurationChanged;
}

/// <summary>
/// Current ducking status for a channel.
/// </summary>
public class DuckingStatus
{
  /// <summary>
  /// The channel this status is for.
  /// </summary>
  public MixerChannel Channel { get; set; }

  /// <summary>
  /// Whether the channel is currently being ducked.
  /// </summary>
  public bool IsDucked { get; set; }

  /// <summary>
  /// Current volume level after ducking (0.0 to 1.0).
  /// </summary>
  public float CurrentLevel { get; set; } = 1.0f;

  /// <summary>
  /// Target volume level before ducking (0.0 to 1.0).
  /// </summary>
  public float OriginalLevel { get; set; } = 1.0f;

  /// <summary>
  /// The channels that are causing this channel to duck.
  /// </summary>
  public List<MixerChannel> TriggeringChannels { get; set; } = new();

  /// <summary>
  /// The source IDs that are causing this channel to duck.
  /// </summary>
  public List<string> TriggeringSourceIds { get; set; } = new();

  /// <summary>
  /// When ducking started (if ducked).
  /// </summary>
  public DateTime? DuckingStartedAt { get; set; }

  /// <summary>
  /// Current phase of ducking transition.
  /// </summary>
  public DuckingPhase Phase { get; set; } = DuckingPhase.None;
}

/// <summary>
/// Phases of ducking transition.
/// </summary>
public enum DuckingPhase
{
  /// <summary>
  /// No ducking active.
  /// </summary>
  None = 0,

  /// <summary>
  /// Attack phase - ducking is ramping down.
  /// </summary>
  Attack = 1,

  /// <summary>
  /// Hold phase - ducking is at target level.
  /// </summary>
  Hold = 2,

  /// <summary>
  /// Release phase - ducking is ramping up.
  /// </summary>
  Release = 3
}

/// <summary>
/// Event arguments for ducking state changes.
/// </summary>
public class DuckingEventArgs : EventArgs
{
  /// <summary>
  /// The channel that was ducked or unducked.
  /// </summary>
  public MixerChannel AffectedChannel { get; set; }

  /// <summary>
  /// The channel that triggered the ducking.
  /// </summary>
  public MixerChannel TriggerChannel { get; set; }

  /// <summary>
  /// The source ID that triggered the ducking.
  /// </summary>
  public string SourceId { get; set; } = string.Empty;

  /// <summary>
  /// Whether ducking started (true) or ended (false).
  /// </summary>
  public bool DuckingStarted { get; set; }

  /// <summary>
  /// The duck level applied (0.0 to 1.0).
  /// </summary>
  public float DuckLevel { get; set; }

  /// <summary>
  /// When the event occurred.
  /// </summary>
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event arguments for ducking configuration changes.
/// </summary>
public class DuckingConfigurationChangedEventArgs : EventArgs
{
  /// <summary>
  /// The new ducking configuration.
  /// </summary>
  public DuckingConfiguration NewConfiguration { get; set; } = new();

  /// <summary>
  /// The previous preset.
  /// </summary>
  public DuckingPreset PreviousPreset { get; set; }

  /// <summary>
  /// The new preset.
  /// </summary>
  public DuckingPreset NewPreset { get; set; }
}

/// <summary>
/// Performance metrics for the ducking system.
/// </summary>
public class DuckingMetrics
{
  /// <summary>
  /// Total number of ducking events triggered.
  /// </summary>
  public long TotalDuckingEvents { get; set; }

  /// <summary>
  /// Average attack time in milliseconds.
  /// </summary>
  public double AverageAttackTimeMs { get; set; }

  /// <summary>
  /// Maximum attack time observed in milliseconds.
  /// </summary>
  public double MaxAttackTimeMs { get; set; }

  /// <summary>
  /// Average release time in milliseconds.
  /// </summary>
  public double AverageReleaseTimeMs { get; set; }

  /// <summary>
  /// Number of cascading ducks (multiple simultaneous ducks).
  /// </summary>
  public long CascadingDuckCount { get; set; }

  /// <summary>
  /// Number of emergency ducks triggered.
  /// </summary>
  public long EmergencyDuckCount { get; set; }

  /// <summary>
  /// Current CPU usage estimate for ducking (0.0 to 1.0).
  /// </summary>
  public float CpuUsage { get; set; }

  /// <summary>
  /// Estimated latency added by ducking in milliseconds.
  /// </summary>
  public double LatencyMs { get; set; }
}
