using RadioConsole.Core.Enums;

namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Service for managing audio mixing using SoundFlow's MixerNode.
/// Handles multiple channels with priority-based ducking and volume control.
/// </summary>
public interface IMixerService : IDisposable
{
  /// <summary>
  /// Initializes the mixer service.
  /// </summary>
  /// <param name="outputDeviceId">The output device to use.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  Task InitializeAsync(string outputDeviceId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Whether the mixer is initialized and ready.
  /// </summary>
  bool IsInitialized { get; }

  /// <summary>
  /// Adds an audio source to the specified mixer channel.
  /// </summary>
  /// <param name="source">The audio source to add.</param>
  /// <param name="channel">The channel to route the source to.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  Task AddSourceAsync(ISoundFlowAudioSource source, MixerChannel channel, CancellationToken cancellationToken = default);

  /// <summary>
  /// Removes an audio source from the mixer.
  /// </summary>
  /// <param name="sourceId">The source identifier.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  Task RemoveSourceAsync(string sourceId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets all active sources on a specific channel.
  /// </summary>
  /// <param name="channel">The channel to query.</param>
  /// <returns>Collection of active sources on the channel.</returns>
  IEnumerable<ISoundFlowAudioSource> GetChannelSources(MixerChannel channel);

  /// <summary>
  /// Gets all active sources across all channels.
  /// </summary>
  /// <returns>Collection of all active sources.</returns>
  IEnumerable<ISoundFlowAudioSource> GetAllSources();

  /// <summary>
  /// Moves a source to a different channel.
  /// </summary>
  /// <param name="sourceId">The source to move.</param>
  /// <param name="newChannel">The target channel.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  Task MoveSourceToChannelAsync(string sourceId, MixerChannel newChannel, CancellationToken cancellationToken = default);

  // Volume Control

  /// <summary>
  /// Gets the volume of a specific channel.
  /// </summary>
  /// <param name="channel">The channel.</param>
  /// <returns>Volume level (0.0 to 1.0).</returns>
  float GetChannelVolume(MixerChannel channel);

  /// <summary>
  /// Sets the volume of a specific channel.
  /// </summary>
  /// <param name="channel">The channel.</param>
  /// <param name="volume">Volume level (0.0 to 1.0).</param>
  /// <param name="rampDurationMs">Duration for volume ramping in milliseconds. 0 for immediate.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  Task SetChannelVolumeAsync(MixerChannel channel, float volume, int rampDurationMs = 0, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets the master volume.
  /// </summary>
  /// <returns>Master volume level (0.0 to 1.0).</returns>
  float GetMasterVolume();

  /// <summary>
  /// Sets the master volume.
  /// </summary>
  /// <param name="volume">Volume level (0.0 to 1.0).</param>
  /// <param name="rampDurationMs">Duration for volume ramping in milliseconds. 0 for immediate.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  Task SetMasterVolumeAsync(float volume, int rampDurationMs = 0, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets the volume of a specific source.
  /// </summary>
  /// <param name="sourceId">The source identifier.</param>
  /// <returns>Volume level (0.0 to 1.0), or null if source not found.</returns>
  float? GetSourceVolume(string sourceId);

  /// <summary>
  /// Sets the volume of a specific source.
  /// </summary>
  /// <param name="sourceId">The source identifier.</param>
  /// <param name="volume">Volume level (0.0 to 1.0).</param>
  /// <param name="rampDurationMs">Duration for volume ramping in milliseconds. 0 for immediate.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  Task SetSourceVolumeAsync(string sourceId, float volume, int rampDurationMs = 0, CancellationToken cancellationToken = default);

  // Ducking Control

  /// <summary>
  /// Gets the duck level (volume reduction) when high-priority channels are active.
  /// </summary>
  float DuckLevel { get; }

  /// <summary>
  /// Sets the duck level for main channel when high-priority channels are active.
  /// </summary>
  /// <param name="level">Duck level (0.0 to 1.0, where 0.2 means reduce to 20%).</param>
  void SetDuckLevel(float level);

  /// <summary>
  /// Gets whether ducking is currently active on the main channel.
  /// </summary>
  bool IsDuckingActive { get; }

  // Events

  /// <summary>
  /// Event raised when a source is added to the mixer.
  /// </summary>
  event EventHandler<MixerSourceEventArgs>? SourceAdded;

  /// <summary>
  /// Event raised when a source is removed from the mixer.
  /// </summary>
  event EventHandler<MixerSourceEventArgs>? SourceRemoved;

  /// <summary>
  /// Event raised when ducking state changes.
  /// </summary>
  event EventHandler<DuckingStateChangedEventArgs>? DuckingStateChanged;

  /// <summary>
  /// Event raised when channel volume changes.
  /// </summary>
  event EventHandler<ChannelVolumeChangedEventArgs>? ChannelVolumeChanged;
}

/// <summary>
/// Event arguments for mixer source events.
/// </summary>
public class MixerSourceEventArgs : EventArgs
{
  /// <summary>
  /// The source identifier.
  /// </summary>
  public string SourceId { get; set; } = string.Empty;

  /// <summary>
  /// The channel the source is on.
  /// </summary>
  public MixerChannel Channel { get; set; }

  /// <summary>
  /// The source instance.
  /// </summary>
  public ISoundFlowAudioSource? Source { get; set; }
}

/// <summary>
/// Event arguments for ducking state changes.
/// </summary>
public class DuckingStateChangedEventArgs : EventArgs
{
  /// <summary>
  /// Whether ducking is now active.
  /// </summary>
  public bool IsDucking { get; set; }

  /// <summary>
  /// The channel that triggered the ducking.
  /// </summary>
  public MixerChannel? TriggerChannel { get; set; }

  /// <summary>
  /// The current duck level applied.
  /// </summary>
  public float DuckLevel { get; set; }
}

/// <summary>
/// Event arguments for channel volume changes.
/// </summary>
public class ChannelVolumeChangedEventArgs : EventArgs
{
  /// <summary>
  /// The channel that changed.
  /// </summary>
  public MixerChannel Channel { get; set; }

  /// <summary>
  /// The old volume level.
  /// </summary>
  public float OldVolume { get; set; }

  /// <summary>
  /// The new volume level.
  /// </summary>
  public float NewVolume { get; set; }
}
