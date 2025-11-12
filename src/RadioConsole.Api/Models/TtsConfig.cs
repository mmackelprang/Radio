namespace RadioConsole.Api.Models;

/// <summary>
/// Configuration for Text-to-Speech service
/// </summary>
public class TtsConfig
{
  /// <summary>
  /// TTS engine to use: EspeakNG, Piper, or GoogleCloud
  /// Default: EspeakNG
  /// </summary>
  public string Engine { get; set; } = "EspeakNG";

  /// <summary>
  /// Configuration for eSpeak-ng TTS engine
  /// </summary>
  public EspeakNgConfig? EspeakNg { get; set; }

  /// <summary>
  /// Configuration for Piper TTS engine
  /// </summary>
  public PiperConfig? Piper { get; set; }

  /// <summary>
  /// Configuration for Google Cloud TTS engine
  /// </summary>
  public GoogleCloudConfig? GoogleCloud { get; set; }
}

/// <summary>
/// Configuration for eSpeak-ng TTS engine
/// </summary>
public class EspeakNgConfig
{
  /// <summary>
  /// Path to the eSpeak or eSpeak-ng executable
  /// Default: espeak-ng (Linux) or espeak (fallback)
  /// </summary>
  public string ExecutablePath { get; set; } = "espeak-ng";

  /// <summary>
  /// Voice to use for speech synthesis
  /// Examples: en, en-us, en-gb, es, fr, de
  /// Use 'espeak-ng --voices' to list available voices
  /// </summary>
  public string Voice { get; set; } = "en-us";

  /// <summary>
  /// Speech speed in words per minute
  /// Default: 175, Range: 80-450
  /// </summary>
  public int Speed { get; set; } = 175;

  /// <summary>
  /// Pitch adjustment
  /// Default: 50, Range: 0-99
  /// </summary>
  public int Pitch { get; set; } = 50;

  /// <summary>
  /// Volume/amplitude
  /// Default: 100, Range: 0-200
  /// </summary>
  public int Volume { get; set; } = 100;

  /// <summary>
  /// Gap between words in units of 10ms
  /// Default: 0 (no gap)
  /// </summary>
  public int WordGap { get; set; } = 0;

  /// <summary>
  /// Sample rate for output audio
  /// Default: 22050 Hz
  /// </summary>
  public int SampleRate { get; set; } = 22050;
}

/// <summary>
/// Configuration for Piper TTS engine
/// </summary>
public class PiperConfig
{
  /// <summary>
  /// Path to the Piper executable
  /// Default: piper (assumes it's in PATH)
  /// </summary>
  public string ExecutablePath { get; set; } = "piper";

  /// <summary>
  /// Path to the voice model file (.onnx)
  /// Example: /path/to/models/en_US-lessac-medium.onnx
  /// </summary>
  public string ModelPath { get; set; } = string.Empty;

  /// <summary>
  /// Path to the voice config file (.json)
  /// Example: /path/to/models/en_US-lessac-medium.onnx.json
  /// </summary>
  public string ConfigPath { get; set; } = string.Empty;

  /// <summary>
  /// Speaking rate (speed multiplier)
  /// Default: 1.0, Range: 0.5-2.0
  /// </summary>
  public double SpeakingRate { get; set; } = 1.0;

  /// <summary>
  /// Sentence silence in seconds
  /// Default: 0.2
  /// </summary>
  public double SentenceSilence { get; set; } = 0.2;

  /// <summary>
  /// Sample rate for output audio
  /// Default: 22050 Hz
  /// </summary>
  public int SampleRate { get; set; } = 22050;
}

/// <summary>
/// Configuration for Google Cloud TTS engine
/// </summary>
public class GoogleCloudConfig
{
  /// <summary>
  /// Path to Google Cloud credentials JSON file
  /// Set this or use GOOGLE_APPLICATION_CREDENTIALS environment variable
  /// </summary>
  public string? CredentialsPath { get; set; }

  /// <summary>
  /// Language code for the voice
  /// Examples: en-US, en-GB, es-ES, fr-FR
  /// </summary>
  public string LanguageCode { get; set; } = "en-US";

  /// <summary>
  /// Voice name (specific voice in the language)
  /// Examples: en-US-Neural2-A, en-US-Standard-B
  /// Leave empty to use default for language
  /// </summary>
  public string? VoiceName { get; set; }

  /// <summary>
  /// Speaking rate (speed)
  /// Default: 1.0, Range: 0.25-4.0
  /// </summary>
  public double SpeakingRate { get; set; } = 1.0;

  /// <summary>
  /// Pitch adjustment in semitones
  /// Default: 0.0, Range: -20.0 to 20.0
  /// </summary>
  public double Pitch { get; set; } = 0.0;

  /// <summary>
  /// Volume gain in dB
  /// Default: 0.0, Range: -96.0 to 16.0
  /// </summary>
  public double VolumeGainDb { get; set; } = 0.0;

  /// <summary>
  /// Sample rate for output audio
  /// Default: 22050 Hz
  /// </summary>
  public int SampleRate { get; set; } = 22050;
}
