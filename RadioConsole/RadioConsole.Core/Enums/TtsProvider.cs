namespace RadioConsole.Core.Enums;

/// <summary>
/// Text-to-speech provider types.
/// </summary>
public enum TtsProvider
{
  /// <summary>
  /// Local espeak TTS engine (Linux/cross-platform).
  /// </summary>
  ESpeak = 0,

  /// <summary>
  /// Google Cloud Text-to-Speech API.
  /// </summary>
  GoogleCloud = 1,

  /// <summary>
  /// Azure Cognitive Services Text-to-Speech API.
  /// </summary>
  AzureCloud = 2
}
