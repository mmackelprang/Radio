using System.Text;
using Microsoft.Extensions.Options;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Models;
using RadioConsole.Api.Services.TtsEngines;

namespace RadioConsole.Api.Services;

/// <summary>
/// Text-to-Speech service that supports multiple TTS engines
/// </summary>
public class TtsService : ITtsService
{
  private readonly TtsConfig _config;
  private readonly IEnvironmentService _environmentService;
  private readonly ILogger<TtsService> _logger;
  private readonly ILoggerFactory _loggerFactory;
  private ITtsEngine? _engine;
  private bool _isAvailable;

  public bool IsAvailable => _isAvailable;

  public TtsService(
    IOptions<TtsConfig> config,
    IEnvironmentService environmentService,
    ILogger<TtsService> logger,
    ILoggerFactory loggerFactory)
  {
    _config = config.Value;
    _environmentService = environmentService;
    _logger = logger;
    _loggerFactory = loggerFactory;
  }

  public async Task InitializeAsync()
  {
    try
    {
      if (_environmentService.IsSimulationMode)
      {
        _logger.LogInformation("TTS service initialized in simulation mode");
        _isAvailable = true;
        return;
      }

      // Select and initialize the TTS engine based on configuration
      _engine = CreateEngine(_config.Engine);

      if (_engine == null)
      {
        _logger.LogWarning("Unknown TTS engine: {Engine}. Defaulting to EspeakNG.", _config.Engine);
        _engine = CreateEngine("EspeakNG");
      }

      if (_engine == null)
      {
        _logger.LogError("Failed to create TTS engine. Please check configuration.");
        _isAvailable = false;
        return;
      }

      // Initialize the selected engine
      _isAvailable = await _engine.InitializeAsync();

      if (_isAvailable)
      {
        _logger.LogInformation("TTS service initialized successfully using {Engine} engine", _engine.EngineName);
      }
      else
      {
        _logger.LogInformation("TTS engine {Engine} is not available. Text-to-speech features will be disabled. See TTS_SETUP.md for installation instructions.", _engine.EngineName);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error initializing TTS service");
      _isAvailable = false;
    }
  }

  private ITtsEngine? CreateEngine(string engineName)
  {
    return engineName.ToLowerInvariant() switch
    {
      "espeakng" => CreateEspeakNgEngine(),
      "piper" => CreatePiperEngine(),
      "googlecloud" => CreateGoogleCloudEngine(),
      _ => null
    };
  }

  private ITtsEngine CreateEspeakNgEngine()
  {
    var config = _config.EspeakNg ?? new EspeakNgConfig();
    var logger = _loggerFactory.CreateLogger<EspeakNgTtsEngine>();
    return new EspeakNgTtsEngine(config, logger);
  }

  private ITtsEngine CreatePiperEngine()
  {
    var config = _config.Piper;
    if (config == null)
    {
      _logger.LogWarning("Piper TTS selected but no configuration provided");
      return new PiperTtsEngine(new PiperConfig(), _loggerFactory.CreateLogger<PiperTtsEngine>());
    }

    var logger = _loggerFactory.CreateLogger<PiperTtsEngine>();
    return new PiperTtsEngine(config, logger);
  }

  private ITtsEngine CreateGoogleCloudEngine()
  {
    var config = _config.GoogleCloud;
    if (config == null)
    {
      _logger.LogWarning("Google Cloud TTS selected but no configuration provided");
      return new GoogleCloudTtsEngine(new GoogleCloudConfig(), _loggerFactory.CreateLogger<GoogleCloudTtsEngine>());
    }

    var logger = _loggerFactory.CreateLogger<GoogleCloudTtsEngine>();
    return new GoogleCloudTtsEngine(config, logger);
  }

  public async Task<Stream?> GenerateSpeechAsync(string text, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(text))
    {
      throw new ArgumentException("Text cannot be empty", nameof(text));
    }

    if (_environmentService.IsSimulationMode)
    {
      return await GenerateSimulatedAudioAsync(text, cancellationToken);
    }

    if (!_isAvailable || _engine == null)
    {
      throw new InvalidOperationException("TTS service is not available");
    }

    return await _engine.GenerateSpeechAsync(text, cancellationToken);
  }

  public double EstimateDuration(string text)
  {
    if (string.IsNullOrWhiteSpace(text))
    {
      return 0;
    }

    if (_environmentService.IsSimulationMode || !_isAvailable || _engine == null)
    {
      // Default estimate: ~3 words per second
      var wordCount = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
      return (wordCount / 3.0) + 0.5;
    }

    return _engine.EstimateDuration(text);
  }

  private async Task<Stream> GenerateSimulatedAudioAsync(string text, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Simulating TTS for: {Text}", text);

    // Generate a simple WAV header with silence
    var duration = EstimateDuration(text);
    var sampleRate = 22050; // Default sample rate
    var numSamples = (int)(duration * sampleRate);
    var numBytes = numSamples * 2; // 16-bit audio

    var stream = new MemoryStream();
    await using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

    // Write WAV header
    writer.Write(Encoding.ASCII.GetBytes("RIFF"));
    writer.Write(36 + numBytes); // File size - 8
    writer.Write(Encoding.ASCII.GetBytes("WAVE"));
    writer.Write(Encoding.ASCII.GetBytes("fmt "));
    writer.Write(16); // Format chunk size
    writer.Write((short)1); // Audio format (PCM)
    writer.Write((short)1); // Number of channels
    writer.Write(sampleRate); // Sample rate
    writer.Write(sampleRate * 2); // Byte rate
    writer.Write((short)2); // Block align
    writer.Write((short)16); // Bits per sample
    writer.Write(Encoding.ASCII.GetBytes("data"));
    writer.Write(numBytes); // Data size

    // Write silence (zeros)
    var silence = new byte[numBytes];
    await stream.WriteAsync(silence, cancellationToken);

    stream.Position = 0;
    return stream;
  }
}
