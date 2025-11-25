using RadioConsole.Core.Enums;

namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Factory for creating SoundFlow audio sources from various inputs.
/// Automatically detects source type and handles format conversion.
/// </summary>
public interface IAudioSourceFactory
{
  /// <summary>
  /// Creates an audio source from a file path.
  /// Supports MP3, WAV, FLAC, OGG formats.
  /// </summary>
  /// <param name="filePath">Path to the audio file.</param>
  /// <param name="channel">Target mixer channel for routing.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>The created audio source.</returns>
  Task<ISoundFlowAudioSource> CreateFromFileAsync(string filePath, MixerChannel channel = MixerChannel.Main, CancellationToken cancellationToken = default);

  /// <summary>
  /// Creates an audio source from a URI.
  /// Detects whether it's a Spotify URI, HTTP stream, or file path.
  /// </summary>
  /// <param name="uri">The URI or path to the audio source.</param>
  /// <param name="channel">Target mixer channel for routing.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>The created audio source.</returns>
  Task<ISoundFlowAudioSource> CreateFromUriAsync(string uri, MixerChannel channel = MixerChannel.Main, CancellationToken cancellationToken = default);

  /// <summary>
  /// Creates a USB input audio source.
  /// </summary>
  /// <param name="deviceId">The USB audio device identifier.</param>
  /// <param name="deviceName">Human-readable device name.</param>
  /// <param name="channel">Target mixer channel for routing.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>The created audio source.</returns>
  Task<ISoundFlowAudioSource> CreateUsbInputAsync(string deviceId, string deviceName, MixerChannel channel = MixerChannel.Main, CancellationToken cancellationToken = default);

  /// <summary>
  /// Creates a TTS audio source from text.
  /// </summary>
  /// <param name="text">The text to synthesize.</param>
  /// <param name="voice">Optional voice name.</param>
  /// <param name="speed">Speech speed (1.0 = normal).</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>The created audio source.</returns>
  Task<ISoundFlowAudioSource> CreateTtsAsync(string text, string? voice = null, float speed = 1.0f, CancellationToken cancellationToken = default);

  /// <summary>
  /// Creates a Spotify stream audio source.
  /// </summary>
  /// <param name="trackUri">Spotify track or playlist URI.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>The created audio source.</returns>
  Task<ISoundFlowAudioSource> CreateSpotifyStreamAsync(string trackUri, CancellationToken cancellationToken = default);

  /// <summary>
  /// Creates an audio source for sound effect playback (doorbell, notification).
  /// Routes to the Event channel by default.
  /// </summary>
  /// <param name="filePath">Path to the sound effect file.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>The created audio source.</returns>
  Task<ISoundFlowAudioSource> CreateEventSoundAsync(string filePath, CancellationToken cancellationToken = default);

  /// <summary>
  /// Detects the type of audio source from a URI or path.
  /// </summary>
  /// <param name="uriOrPath">The URI or file path.</param>
  /// <returns>The detected source type.</returns>
  AudioSourceType DetectSourceType(string uriOrPath);

  /// <summary>
  /// Gets the supported file extensions.
  /// </summary>
  IReadOnlyCollection<string> SupportedExtensions { get; }

  /// <summary>
  /// Disposes of a specific audio source and removes it from tracking.
  /// </summary>
  /// <param name="sourceId">The source identifier.</param>
  void DisposeSource(string sourceId);

  /// <summary>
  /// Disposes of all tracked audio sources.
  /// </summary>
  void DisposeAllSources();
}
