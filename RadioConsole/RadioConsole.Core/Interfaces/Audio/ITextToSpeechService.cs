namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Service for converting text to speech using various TTS engines.
/// Supports local (espeak) and cloud-based (Google, Azure) providers.
/// </summary>
public interface ITextToSpeechService
{
  /// <summary>
  /// Initialize the TTS service.
  /// </summary>
  Task InitializeAsync();

  /// <summary>
  /// Convert text to speech and return the audio stream.
  /// </summary>
  /// <param name="text">The text to convert to speech.</param>
  /// <param name="voiceGender">Optional gender for the voice ("male" or "female"). Default is provider-specific.</param>
  /// <param name="speed">Optional speech speed (0.5 to 2.0). Default is 1.0 (normal speed).</param>
  /// <returns>A stream containing the generated audio data.</returns>
  Task<Stream> SynthesizeSpeechAsync(string text, string? voiceGender = null, float speed = 1.0f);

  /// <summary>
  /// Play text to speech directly through the audio player.
  /// This is a convenience method that combines SynthesizeSpeechAsync with audio playback.
  /// </summary>
  /// <param name="text">The text to speak.</param>
  /// <param name="voiceGender">Optional gender for the voice ("male" or "female").</param>
  /// <param name="speed">Optional speech speed (0.5 to 2.0).</param>
  /// <returns>A task that completes when the speech finishes playing.</returns>
  Task SpeakAsync(string text, string? voiceGender = null, float speed = 1.0f);

  /// <summary>
  /// Stop any currently playing TTS audio.
  /// </summary>
  Task StopAsync();

  /// <summary>
  /// Check if the TTS service is currently speaking.
  /// </summary>
  bool IsSpeaking { get; }
}
