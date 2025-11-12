namespace RadioConsole.Api.Interfaces;

/// <summary>
/// Interface for Text-to-Speech engine implementations
/// </summary>
public interface ITtsEngine
{
  /// <summary>
  /// Initialize the TTS engine and verify availability
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>True if the engine is available and initialized successfully</returns>
  Task<bool> InitializeAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Generate speech audio from text
  /// </summary>
  /// <param name="text">Text to convert to speech</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Audio stream in WAV format</returns>
  Task<Stream> GenerateSpeechAsync(string text, CancellationToken cancellationToken = default);

  /// <summary>
  /// Get estimated duration of speech in seconds
  /// </summary>
  /// <param name="text">Text to estimate duration for</param>
  /// <returns>Estimated duration in seconds</returns>
  double EstimateDuration(string text);

  /// <summary>
  /// Check if the TTS engine is available
  /// </summary>
  bool IsAvailable { get; }

  /// <summary>
  /// Get the name of the TTS engine
  /// </summary>
  string EngineName { get; }
}
