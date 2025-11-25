using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio.Sources;

/// <summary>
/// Audio source for text-to-speech output.
/// Routes to the Voice channel for priority playback with ducking.
/// </summary>
public class TextToSpeechAudioSource : SoundFlowAudioSourceBase
{
  private readonly string _text;
  private readonly string? _voice;
  private readonly float _speed;
  private readonly ITextToSpeechService? _ttsService;

  /// <inheritdoc/>
  public override AudioSourceType SourceType => AudioSourceType.TtsEvent;

  /// <summary>
  /// Gets the text to be spoken.
  /// </summary>
  public string Text => _text;

  /// <summary>
  /// Gets the voice name.
  /// </summary>
  public string? Voice => _voice;

  /// <summary>
  /// Gets the speech speed.
  /// </summary>
  public float Speed => _speed;

  /// <summary>
  /// Creates a new text-to-speech audio source.
  /// </summary>
  /// <param name="id">Unique identifier.</param>
  /// <param name="text">Text to be spoken.</param>
  /// <param name="voice">Optional voice name.</param>
  /// <param name="speed">Speech speed (1.0 = normal).</param>
  /// <param name="ttsService">TTS service for synthesis.</param>
  /// <param name="logger">Logger instance.</param>
  public TextToSpeechAudioSource(
    string id,
    string text,
    string? voice,
    float speed,
    ITextToSpeechService? ttsService,
    ILogger<TextToSpeechAudioSource> logger)
    : base(id, "TTS Announcement", MixerChannel.Voice, logger)
  {
    _text = text ?? throw new ArgumentNullException(nameof(text));
    _voice = voice;
    _speed = Math.Clamp(speed, 0.5f, 2.0f);
    _ttsService = ttsService;

    SetMetadata("Text", TruncateText(text, 100));
    SetMetadata("Voice", voice ?? "default");
    SetMetadata("Speed", speed.ToString("F2"));
    SetMetadata("SourceType", "TTS");
  }

  private static string TruncateText(string text, int maxLength)
  {
    if (text.Length <= maxLength)
    {
      return text;
    }
    return text.Substring(0, maxLength - 3) + "...";
  }

  /// <inheritdoc/>
  public override async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    if (_disposed)
    {
      throw new ObjectDisposedException(nameof(TextToSpeechAudioSource));
    }

    _logger.LogInformation("Initializing TTS audio source: \"{Text}\" (Voice: {Voice}, Speed: {Speed})", 
      TruncateText(_text, 50), _voice ?? "default", _speed);

    try
    {
      if (_ttsService == null)
      {
        _logger.LogWarning("TTS service not available, source cannot produce audio");
        Status = AudioSourceStatus.Error;
        throw new InvalidOperationException("TTS service not available");
      }

      // Synthesize speech
      _audioStream = await _ttsService.SynthesizeSpeechAsync(_text, _voice, _speed);

      if (_audioStream == null || _audioStream.Length == 0)
      {
        _logger.LogWarning("TTS synthesis returned empty audio");
        Status = AudioSourceStatus.Error;
        throw new InvalidOperationException("TTS synthesis returned empty audio");
      }

      SetMetadata("AudioLength", _audioStream.Length.ToString());
      Status = AudioSourceStatus.Ready;
      _logger.LogInformation("TTS audio source initialized successfully ({AudioLength} bytes)", _audioStream.Length);
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("TTS initialization cancelled");
      throw;
    }
    catch (InvalidOperationException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to initialize TTS audio source");
      Status = AudioSourceStatus.Error;
      throw;
    }
  }

  /// <inheritdoc/>
  public override async Task StopAsync(CancellationToken cancellationToken = default)
  {
    await base.StopAsync(cancellationToken);

    // Reset stream position if possible
    if (_audioStream?.CanSeek == true)
    {
      _audioStream.Position = 0;
    }
  }

  /// <inheritdoc/>
  public override void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _logger.LogDebug("Disposing TTS audio source: \"{Text}\"", TruncateText(_text, 30));
    base.Dispose();
  }
}
