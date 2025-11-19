namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Interface for playing audio streams using the SoundFlow library.
/// Supports managing audio sources, volume control, and audio mixing.
/// </summary>
public interface IAudioPlayer
{
  /// <summary>
  /// Initialize the audio player with the specified device.
  /// </summary>
  /// <param name="deviceId">The audio device identifier (ALSA device ID on Linux).</param>
  Task InitializeAsync(string deviceId);

  /// <summary>
  /// Play audio from the specified source.
  /// </summary>
  /// <param name="sourceId">Unique identifier for the audio source.</param>
  /// <param name="audioData">Audio data stream to play.</param>
  Task PlayAsync(string sourceId, Stream audioData);

  /// <summary>
  /// Stop playing audio from the specified source.
  /// </summary>
  /// <param name="sourceId">Unique identifier for the audio source.</param>
  Task StopAsync(string sourceId);

  /// <summary>
  /// Set the volume for a specific audio source.
  /// </summary>
  /// <param name="sourceId">Unique identifier for the audio source.</param>
  /// <param name="volume">Volume level (0.0 to 1.0).</param>
  Task SetVolumeAsync(string sourceId, float volume);

  /// <summary>
  /// Get the current mixed audio output stream.
  /// This stream contains the mix of all playing audio sources.
  /// </summary>
  /// <returns>A stream containing the mixed audio output.</returns>
  Stream GetMixedOutputStream();

  /// <summary>
  /// Check if the audio player is currently initialized.
  /// </summary>
  bool IsInitialized { get; }
}
