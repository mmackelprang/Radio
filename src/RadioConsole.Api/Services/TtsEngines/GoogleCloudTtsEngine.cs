using Google.Cloud.TextToSpeech.V1;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Models;

namespace RadioConsole.Api.Services.TtsEngines;

/// <summary>
/// Google Cloud Text-to-Speech engine implementation
/// High-quality cloud-based neural TTS
/// </summary>
public class GoogleCloudTtsEngine : ITtsEngine
{
  private readonly GoogleCloudConfig _config;
  private readonly ILogger<GoogleCloudTtsEngine> _logger;
  private bool _isAvailable;
  private TextToSpeechClient? _client;

  public bool IsAvailable => _isAvailable;
  public string EngineName => "Google Cloud TTS";

  public GoogleCloudTtsEngine(
    GoogleCloudConfig config,
    ILogger<GoogleCloudTtsEngine> logger)
  {
    _config = config;
    _logger = logger;
  }

  public async Task<bool> InitializeAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      // Set credentials path if specified
      if (!string.IsNullOrWhiteSpace(_config.CredentialsPath))
      {
        if (!File.Exists(_config.CredentialsPath))
        {
          _logger.LogWarning("Google Cloud credentials file not found at: {Path}", _config.CredentialsPath);
          _isAvailable = false;
          return false;
        }

        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", _config.CredentialsPath);
      }

      // Check if credentials are available (from env var or default credentials)
      var credentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
      if (string.IsNullOrWhiteSpace(credentialsPath))
      {
        _logger.LogInformation("Google Cloud TTS credentials not configured. Set GOOGLE_APPLICATION_CREDENTIALS or provide CredentialsPath in config.");
        _isAvailable = false;
        return false;
      }

      // Create the client
      _client = await TextToSpeechClient.CreateAsync(cancellationToken);

      // Test the connection with a simple voices list call
      try
      {
        await _client.ListVoicesAsync(new ListVoicesRequest
        {
          LanguageCode = _config.LanguageCode
        }, cancellationToken);

        _isAvailable = true;
        _logger.LogInformation("Google Cloud TTS initialized successfully with language: {Language}", _config.LanguageCode);
        return true;
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Failed to connect to Google Cloud TTS API");
        _isAvailable = false;
        return false;
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error initializing Google Cloud TTS");
      _isAvailable = false;
      return false;
    }
  }

  public async Task<Stream> GenerateSpeechAsync(string text, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(text))
    {
      throw new ArgumentException("Text cannot be empty", nameof(text));
    }

    if (!_isAvailable || _client == null)
    {
      throw new InvalidOperationException("Google Cloud TTS is not available");
    }

    try
    {
      // Build the synthesis input
      var input = new SynthesisInput
      {
        Text = text
      };

      // Build the voice selection
      var voice = new VoiceSelectionParams
      {
        LanguageCode = _config.LanguageCode
      };

      if (!string.IsNullOrWhiteSpace(_config.VoiceName))
      {
        voice.Name = _config.VoiceName;
      }

      // Build the audio config
      var audioConfig = new AudioConfig
      {
        AudioEncoding = AudioEncoding.Linear16,
        SampleRateHertz = _config.SampleRate,
        SpeakingRate = _config.SpeakingRate,
        Pitch = _config.Pitch,
        VolumeGainDb = _config.VolumeGainDb
      };

      _logger.LogDebug("Calling Google Cloud TTS API for text: {Text}", text);

      // Perform the text-to-speech request
      var response = await _client.SynthesizeSpeechAsync(input, voice, audioConfig, cancellationToken);

      // Convert the audio content to a stream
      var audioStream = new MemoryStream(response.AudioContent.ToByteArray());

      // Convert LINEAR16 to WAV format
      var wavStream = ConvertLinear16ToWav(audioStream, _config.SampleRate);

      _logger.LogDebug("Generated {Bytes} bytes of audio for text: {Text}", wavStream.Length, text);

      return wavStream;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error generating speech for text: {Text}", text);
      throw;
    }
  }

  public double EstimateDuration(string text)
  {
    if (string.IsNullOrWhiteSpace(text))
    {
      return 0;
    }

    // Rough estimate: ~3 words per second at normal speed
    var wordCount = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    var baseSeconds = wordCount / 3.0;

    // Adjust for speaking rate
    var seconds = baseSeconds / _config.SpeakingRate;

    // Add small buffer for processing
    return seconds + 0.5;
  }

  private Stream ConvertLinear16ToWav(MemoryStream pcmStream, int sampleRate)
  {
    pcmStream.Position = 0;
    var pcmData = pcmStream.ToArray();
    var numBytes = pcmData.Length;

    var wavStream = new MemoryStream();
    using var writer = new BinaryWriter(wavStream, System.Text.Encoding.UTF8, leaveOpen: true);

    // Write WAV header
    writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
    writer.Write(36 + numBytes); // File size - 8
    writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
    writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
    writer.Write(16); // Format chunk size
    writer.Write((short)1); // Audio format (PCM)
    writer.Write((short)1); // Number of channels (mono)
    writer.Write(sampleRate); // Sample rate
    writer.Write(sampleRate * 2); // Byte rate (sample rate * channels * bytes per sample)
    writer.Write((short)2); // Block align (channels * bytes per sample)
    writer.Write((short)16); // Bits per sample
    writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
    writer.Write(numBytes); // Data size

    // Write PCM data
    writer.Write(pcmData);

    wavStream.Position = 0;
    return wavStream;
  }
}
