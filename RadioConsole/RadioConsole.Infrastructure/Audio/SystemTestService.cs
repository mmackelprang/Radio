using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Core.Enums;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Service for triggering system test events.
/// Provides methods to test audio priority, TTS, and event handling.
/// </summary>
public class SystemTestService : ISystemTestService
{
  private readonly IAudioPlayer _audioPlayer;
  private readonly IAudioPriorityService _priorityService;
  private readonly TextToSpeechFactory _ttsFactory;
  private readonly ILogger<SystemTestService> _logger;
  private bool _isTestRunning;
  private const string TestSourceId = "system-test";
  private const string DoorbellSourceId = "doorbell-test";
  private const string TtsSourceId = "tts-test";

  public SystemTestService(
    IAudioPlayer audioPlayer,
    IAudioPriorityService priorityService,
    TextToSpeechFactory ttsFactory,
    ILogger<SystemTestService> logger)
  {
    _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
    _priorityService = priorityService ?? throw new ArgumentNullException(nameof(priorityService));
    _ttsFactory = ttsFactory ?? throw new ArgumentNullException(nameof(ttsFactory));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public bool IsTestRunning => _isTestRunning;

  public async Task TriggerTtsAsync(string phrase, string? voiceGender = null, float speed = 1.0f)
  {
    if (string.IsNullOrWhiteSpace(phrase))
    {
      throw new ArgumentException("Phrase cannot be null or empty", nameof(phrase));
    }

    if (_isTestRunning)
    {
      _logger.LogWarning("Test already running. Please wait for current test to complete.");
      return;
    }

    _isTestRunning = true;

    try
    {
      _logger.LogInformation("Triggering TTS test with phrase: {Phrase}", phrase);

      // Register TTS as high priority to trigger ducking
      await _priorityService.RegisterSourceAsync(TtsSourceId, AudioPriority.High);
      await _priorityService.OnHighPriorityStartAsync(TtsSourceId);

      // Create TTS service (default to espeak)
      var ttsService = _ttsFactory.Create(TtsProvider.ESpeak);
      await ttsService.InitializeAsync();

      // Speak the phrase
      await ttsService.SpeakAsync(phrase, voiceGender, speed);

      _logger.LogInformation("TTS test completed successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during TTS test");
      throw;
    }
    finally
    {
      await _priorityService.OnHighPriorityEndAsync(TtsSourceId);
      await _priorityService.UnregisterSourceAsync(TtsSourceId);
      _isTestRunning = false;
    }
  }

  public async Task TriggerTestToneAsync(int frequency = 300, double durationSeconds = 2)
  {
    if (frequency <= 0)
    {
      throw new ArgumentException("Frequency must be greater than 0", nameof(frequency));
    }

    if (durationSeconds <= 0)
    {
      throw new ArgumentException("Duration must be greater than 0", nameof(durationSeconds));
    }

    if (_isTestRunning)
    {
      _logger.LogWarning("Test already running. Please wait for current test to complete.");
      return;
    }

    _isTestRunning = true;

    try
    {
      _logger.LogInformation("Generating {Frequency}Hz test tone for {Duration} seconds", frequency, durationSeconds);

      // Register as high priority
      await _priorityService.RegisterSourceAsync(TestSourceId, AudioPriority.High);
      await _priorityService.OnHighPriorityStartAsync(TestSourceId);

      // Generate sine wave
      var audioData = GenerateSineWave(frequency, durationSeconds);
      var audioStream = new MemoryStream(audioData);

      // Play the tone
      await _audioPlayer.PlayAsync(TestSourceId, audioStream);

      // Wait for playback to complete
      await Task.Delay(TimeSpan.FromSeconds(durationSeconds));

      _logger.LogInformation("Test tone completed successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during test tone generation");
      throw;
    }
    finally
    {
      await _priorityService.OnHighPriorityEndAsync(TestSourceId);
      await _priorityService.UnregisterSourceAsync(TestSourceId);
      _isTestRunning = false;
    }
  }

  public async Task TriggerDoorbellAsync()
  {
    if (_isTestRunning)
    {
      _logger.LogWarning("Test already running. Please wait for current test to complete.");
      return;
    }

    _isTestRunning = true;

    try
    {
      _logger.LogInformation("Simulating doorbell event");

      // Register doorbell as high priority
      await _priorityService.RegisterSourceAsync(DoorbellSourceId, AudioPriority.High);
      await _priorityService.OnHighPriorityStartAsync(DoorbellSourceId);

      // Generate a two-tone doorbell sound (E and C notes)
      var dingFrequency = 659; // E note (ding)
      var dongFrequency = 523; // C note (dong)
      var toneDuration = 0.5; // seconds

      // Generate ding-dong pattern
      var dingData = GenerateSineWave(dingFrequency, toneDuration);
      var dongData = GenerateSineWave(dongFrequency, toneDuration);

      // Combine ding and dong with a small gap
      var silenceData = new byte[(int)(44100 * 0.1 * 2)]; // 0.1 second silence (stereo)
      var doorbellData = dingData.Concat(silenceData).Concat(dongData).ToArray();

      var audioStream = new MemoryStream(doorbellData);
      await _audioPlayer.PlayAsync(DoorbellSourceId, audioStream);

      // Wait for playback to complete
      await Task.Delay(TimeSpan.FromSeconds(toneDuration * 2 + 0.1));

      _logger.LogInformation("Doorbell simulation completed successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during doorbell simulation");
      throw;
    }
    finally
    {
      await _priorityService.OnHighPriorityEndAsync(DoorbellSourceId);
      await _priorityService.UnregisterSourceAsync(DoorbellSourceId);
      _isTestRunning = false;
    }
  }

  /// <summary>
  /// Generate a sine wave audio sample.
  /// Returns raw PCM audio data (16-bit, 44.1kHz, stereo).
  /// </summary>
  private byte[] GenerateSineWave(int frequency, double durationSeconds)
  {
    const int sampleRate = 44100;
    const int bitsPerSample = 16;
    const int channels = 2; // Stereo

    var sampleCount = (int)(sampleRate * durationSeconds);
    var bytesPerSample = bitsPerSample / 8;
    var dataSize = sampleCount * bytesPerSample * channels;

    var data = new byte[dataSize];
    var amplitude = short.MaxValue * 0.5; // 50% volume to avoid clipping

    for (int i = 0; i < sampleCount; i++)
    {
      // Generate sine wave sample
      var time = (double)i / sampleRate;
      var angle = 2.0 * Math.PI * frequency * time;
      var sample = (short)(amplitude * Math.Sin(angle));

      // Convert to bytes (little-endian)
      var bytes = BitConverter.GetBytes(sample);

      // Write to both left and right channels (stereo)
      var offset = i * bytesPerSample * channels;
      
      // Left channel
      data[offset] = bytes[0];
      data[offset + 1] = bytes[1];
      
      // Right channel
      data[offset + 2] = bytes[0];
      data[offset + 3] = bytes[1];
    }

    return data;
  }
}
