namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Interface for audio output implementations.
/// Supports both local playback and casting to remote devices.
/// </summary>
public interface IAudioOutput
{
  /// <summary>
  /// Initialize the audio output.
  /// </summary>
  Task InitializeAsync();

  /// <summary>
  /// Start outputting audio from the specified audio player.
  /// </summary>
  /// <param name="audioPlayer">The audio player providing the audio stream.</param>
  Task StartAsync(IAudioPlayer audioPlayer);

  /// <summary>
  /// Stop outputting audio.
  /// </summary>
  Task StopAsync();

  /// <summary>
  /// Check if the output is currently active.
  /// </summary>
  bool IsActive { get; }

  /// <summary>
  /// Get the name/description of this output.
  /// </summary>
  string Name { get; }
}
