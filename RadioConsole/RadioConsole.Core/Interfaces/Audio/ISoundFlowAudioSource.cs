using RadioConsole.Core.Enums;

namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Interface for SoundFlow-compatible audio sources.
/// Provides a unified abstraction for various audio input types (files, streams, USB input, TTS).
/// </summary>
public interface ISoundFlowAudioSource : IDisposable
{
  /// <summary>
  /// Unique identifier for this audio source.
  /// </summary>
  string Id { get; }

  /// <summary>
  /// Human-readable name for the source.
  /// </summary>
  string Name { get; }

  /// <summary>
  /// The type of audio source.
  /// </summary>
  AudioSourceType SourceType { get; }

  /// <summary>
  /// The mixer channel this source should be routed to.
  /// </summary>
  MixerChannel Channel { get; }

  /// <summary>
  /// Current status of the audio source.
  /// </summary>
  AudioSourceStatus Status { get; }

  /// <summary>
  /// Whether this source is currently producing audio.
  /// </summary>
  bool IsActive { get; }

  /// <summary>
  /// Current volume level (0.0 to 1.0).
  /// </summary>
  float Volume { get; set; }

  /// <summary>
  /// Additional metadata about the source.
  /// </summary>
  IReadOnlyDictionary<string, string> Metadata { get; }

  /// <summary>
  /// Initializes the audio source and prepares it for playback.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  Task InitializeAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Starts playback of the audio source.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  Task StartAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Stops playback of the audio source.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  Task StopAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Pauses playback of the audio source.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  Task PauseAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Resumes playback of a paused audio source.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  Task ResumeAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets the audio data stream for this source.
  /// Used by the mixer to read audio samples.
  /// </summary>
  /// <returns>Stream containing audio data.</returns>
  Stream? GetAudioStream();

  /// <summary>
  /// Event raised when the source status changes.
  /// </summary>
  event EventHandler<AudioSourceStatusChangedEventArgs>? StatusChanged;
}

/// <summary>
/// Event arguments for audio source status changes.
/// </summary>
public class AudioSourceStatusChangedEventArgs : EventArgs
{
  /// <summary>
  /// The previous status.
  /// </summary>
  public AudioSourceStatus OldStatus { get; set; }

  /// <summary>
  /// The new status.
  /// </summary>
  public AudioSourceStatus NewStatus { get; set; }

  /// <summary>
  /// The source that changed status.
  /// </summary>
  public string SourceId { get; set; } = string.Empty;
}
