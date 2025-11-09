namespace RadioConsole.Api.Models;

/// <summary>
/// Configuration for eSpeak TTS service
/// </summary>
public class ESpeakTtsConfig
{
    /// <summary>
    /// Path to the eSpeak or eSpeak-ng executable
    /// Default: espeak-ng (Linux) or espeak (fallback)
    /// </summary>
    public string ESpeakExecutablePath { get; set; } = "espeak-ng";

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
