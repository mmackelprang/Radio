namespace RadioConsole.Api.Interfaces;

/// <summary>
/// Interface for Text-to-Speech services
/// </summary>
public interface ITtsService
{
    /// <summary>
    /// Initialize the TTS service
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Generate audio stream from text using TTS
    /// </summary>
    /// <param name="text">Text to convert to speech</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Audio stream in WAV format</returns>
    Task<Stream?> GenerateSpeechAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if TTS service is available
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Get estimated duration of speech in seconds
    /// </summary>
    /// <param name="text">Text to estimate duration for</param>
    /// <returns>Estimated duration in seconds</returns>
    double EstimateDuration(string text);
}
