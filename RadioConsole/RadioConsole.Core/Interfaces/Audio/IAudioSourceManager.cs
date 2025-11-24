namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Interface for managing audio sources.
/// Provides methods to create and manage standard and high-priority audio sources.
/// </summary>
public interface IAudioSourceManager
{
  // Standard Audio Sources

  /// <summary>
  /// Creates and returns a Spotify audio source.
  /// Configuration is read from: Component=AudioSource, Category=Spotify.
  /// </summary>
  /// <returns>The audio source identifier.</returns>
  Task<string> CreateSpotifySourceAsync();

  /// <summary>
  /// Creates and returns a USB Radio audio source (e.g., Raddy RF320).
  /// Configuration is read from: Component=AudioSource, Category=USBRadio.
  /// </summary>
  /// <returns>The audio source identifier.</returns>
  Task<string> CreateRadioSourceAsync();

  /// <summary>
  /// Creates and returns a Vinyl Record USB audio source.
  /// Configuration is read from: Component=AudioSource, Category=VinylRecord.
  /// </summary>
  /// <returns>The audio source identifier.</returns>
  Task<string> CreateVinylRecordSourceAsync();

  /// <summary>
  /// Creates and returns a File Player audio source.
  /// Configuration is read from: Component=AudioSource, Category=FilePlayer.
  /// </summary>
  /// <param name="filePath">Optional file path override. If null, uses configured path.</param>
  /// <returns>The audio source identifier.</returns>
  Task<string> CreateFilePlayerSourceAsync(string? filePath = null);

  // High Priority Audio Sources

  /// <summary>
  /// Creates and returns a TTS (Text-to-Speech) Event audio source.
  /// Configuration is read from: Component=AudioSource, Category=TTS.
  /// </summary>
  /// <param name="ttsText">The text to speak.</param>
  /// <param name="ttsVoice">The voice to use for TTS.</param>
  /// <param name="speed">The speech speed (1.0 is normal).</param>
  /// <returns>The audio source identifier.</returns>
  Task<string> CreateTtsEventSourceAsync(string ttsText, string ttsVoice, float speed = 1.0f);

  /// <summary>
  /// Creates and returns a File Event audio source (e.g., doorbell, notification sounds).
  /// </summary>
  /// <param name="filePath">The path to the audio file to play.</param>
  /// <returns>The audio source identifier.</returns>
  Task<string> CreateFileEventSourceAsync(string filePath);

  // Source Playback Control

  /// <summary>
  /// Starts playing an audio source.
  /// </summary>
  /// <param name="sourceId">The identifier of the source to play.</param>
  Task PlaySourceAsync(string sourceId);

  /// <summary>
  /// Pauses an audio source.
  /// </summary>
  /// <param name="sourceId">The identifier of the source to pause.</param>
  Task PauseSourceAsync(string sourceId);

  /// <summary>
  /// Resumes a paused audio source.
  /// </summary>
  /// <param name="sourceId">The identifier of the source to resume.</param>
  Task ResumeSourceAsync(string sourceId);

  // Source Management

  /// <summary>
  /// Gets a list of all active audio sources.
  /// </summary>
  /// <returns>Collection of active audio source information.</returns>
  Task<IEnumerable<AudioSourceInfo>> GetActiveSourcesAsync();

  /// <summary>
  /// Stops and removes an audio source.
  /// </summary>
  /// <param name="sourceId">The identifier of the source to stop.</param>
  Task StopSourceAsync(string sourceId);

  /// <summary>
  /// Stops and removes all audio sources.
  /// </summary>
  Task StopAllSourcesAsync();

  /// <summary>
  /// Gets the current status of an audio source.
  /// </summary>
  /// <param name="sourceId">The source identifier.</param>
  /// <returns>The source information, or null if not found.</returns>
  Task<AudioSourceInfo?> GetSourceInfoAsync(string sourceId);
}

/// <summary>
/// Information about an audio source.
/// </summary>
public class AudioSourceInfo
{
  /// <summary>
  /// Unique identifier for this audio source.
  /// </summary>
  public string Id { get; set; } = string.Empty;

  /// <summary>
  /// The type/category of audio source.
  /// </summary>
  public AudioSourceType Type { get; set; }

  /// <summary>
  /// Human-readable name for the source.
  /// </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// Current status of the source.
  /// </summary>
  public AudioSourceStatus Status { get; set; }

  /// <summary>
  /// Whether this is a high-priority source (causes ducking of standard sources).
  /// </summary>
  public bool IsHighPriority { get; set; }

  /// <summary>
  /// When the source was created.
  /// </summary>
  public DateTime CreatedAt { get; set; }

  /// <summary>
  /// Additional metadata about the source.
  /// </summary>
  public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Types of audio sources.
/// </summary>
public enum AudioSourceType
{
  /// <summary>Spotify streaming source.</summary>
  Spotify,

  /// <summary>USB Radio source (e.g., Raddy RF320).</summary>
  USBRadio,

  /// <summary>Vinyl record USB ADC source.</summary>
  VinylRecord,

  /// <summary>Local file player source.</summary>
  FilePlayer,

  /// <summary>Text-to-speech event source.</summary>
  TtsEvent,

  /// <summary>File-based event source (doorbell, notifications).</summary>
  FileEvent
}

/// <summary>
/// Status of an audio source.
/// </summary>
public enum AudioSourceStatus
{
  /// <summary>Source is being created/initialized.</summary>
  Initializing,

  /// <summary>Source is ready to play.</summary>
  Ready,

  /// <summary>Source is currently playing.</summary>
  Playing,

  /// <summary>Source is paused.</summary>
  Paused,

  /// <summary>Source has stopped.</summary>
  Stopped,

  /// <summary>Source encountered an error.</summary>
  Error
}
