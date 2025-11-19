using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Text-to-speech implementation using espeak (local process).
/// Cross-platform TTS engine that runs on Linux, Windows, and macOS.
/// </summary>
public class ESpeakTextToSpeechService : ITextToSpeechService
{
  private readonly IAudioPlayer _audioPlayer;
  private readonly IAudioPriorityService _priorityService;
  private readonly ILogger<ESpeakTextToSpeechService> _logger;
  private bool _isSpeaking;
  private const string TtsSourceId = "tts-espeak";

  public ESpeakTextToSpeechService(
    IAudioPlayer audioPlayer,
    IAudioPriorityService priorityService,
    ILogger<ESpeakTextToSpeechService> logger)
  {
    _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
    _priorityService = priorityService ?? throw new ArgumentNullException(nameof(priorityService));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public bool IsSpeaking => _isSpeaking;

  public async Task InitializeAsync()
  {
    // Check if espeak is installed
    try
    {
      var result = await RunCommandAsync("espeak", "--version");
      if (result.ExitCode != 0)
      {
        _logger.LogWarning("espeak may not be installed. Exit code: {ExitCode}", result.ExitCode);
      }
      else
      {
        _logger.LogInformation("espeak initialized successfully. Version: {Output}", result.Output.Trim());
      }

      // Register as high priority source
      await _priorityService.RegisterSourceAsync(TtsSourceId, Core.Enums.AudioPriority.High);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to initialize espeak TTS");
      throw;
    }
  }

  public async Task<Stream> SynthesizeSpeechAsync(string text, string? voiceGender = null, float speed = 1.0f)
  {
    if (string.IsNullOrWhiteSpace(text))
    {
      throw new ArgumentException("Text cannot be null or empty", nameof(text));
    }

    _logger.LogInformation("Synthesizing speech with espeak: {Text}", text);

    try
    {
      // Build espeak command
      var speedWpm = (int)(175 * speed); // 175 WPM is default, scale by speed
      var args = $"-s {speedWpm} --stdout";

      // Add voice gender if specified
      if (!string.IsNullOrWhiteSpace(voiceGender))
      {
        var voice = voiceGender.ToLower() switch
        {
          "male" => "+m3", // Male voice variant 3
          "female" => "+f3", // Female voice variant 3
          _ => ""
        };
        if (!string.IsNullOrEmpty(voice))
        {
          args += $" -v en{voice}";
        }
      }

      args += $" \"{text.Replace("\"", "\\\"")}\"";

      // Run espeak and capture audio output
      var result = await RunCommandWithBinaryOutputAsync("espeak", args);
      
      if (result.ExitCode != 0)
      {
        throw new InvalidOperationException($"espeak failed with exit code {result.ExitCode}: {result.Error}");
      }

      // Return the audio data as a memory stream
      var audioStream = new MemoryStream(result.BinaryOutput);
      return audioStream;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error synthesizing speech with espeak");
      throw;
    }
  }

  public async Task SpeakAsync(string text, string? voiceGender = null, float speed = 1.0f)
  {
    if (_isSpeaking)
    {
      _logger.LogWarning("Already speaking. Stopping current speech.");
      await StopAsync();
    }

    _isSpeaking = true;

    try
    {
      // Notify priority service that high priority audio is starting
      await _priorityService.OnHighPriorityStartAsync(TtsSourceId);

      // Synthesize speech
      var audioStream = await SynthesizeSpeechAsync(text, voiceGender, speed);

      // Play through audio player
      await _audioPlayer.PlayAsync(TtsSourceId, audioStream);

      _logger.LogInformation("TTS playback started for: {Text}", text);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error speaking text");
      _isSpeaking = false;
      await _priorityService.OnHighPriorityEndAsync(TtsSourceId);
      throw;
    }
    finally
    {
      // Note: In a real implementation, we'd wait for playback to complete
      // For now, we'll mark as not speaking immediately
      // This should be improved with playback completion callbacks
      await Task.Delay(TimeSpan.FromSeconds(2)); // Rough estimate
      _isSpeaking = false;
      await _priorityService.OnHighPriorityEndAsync(TtsSourceId);
    }
  }

  public async Task StopAsync()
  {
    if (!_isSpeaking)
    {
      return;
    }

    try
    {
      await _audioPlayer.StopAsync(TtsSourceId);
      _isSpeaking = false;
      await _priorityService.OnHighPriorityEndAsync(TtsSourceId);
      _logger.LogInformation("TTS playback stopped");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error stopping TTS");
      throw;
    }
  }

  private async Task<(int ExitCode, string Output, string Error)> RunCommandAsync(string command, string args)
  {
    var process = new Process
    {
      StartInfo = new ProcessStartInfo
      {
        FileName = command,
        Arguments = args,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      }
    };

    process.Start();
    var output = await process.StandardOutput.ReadToEndAsync();
    var error = await process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();

    return (process.ExitCode, output, error);
  }

  private async Task<(int ExitCode, byte[] BinaryOutput, string Error)> RunCommandWithBinaryOutputAsync(string command, string args)
  {
    var process = new Process
    {
      StartInfo = new ProcessStartInfo
      {
        FileName = command,
        Arguments = args,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      }
    };

    process.Start();
    
    using var memoryStream = new MemoryStream();
    await process.StandardOutput.BaseStream.CopyToAsync(memoryStream);
    var error = await process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();

    return (process.ExitCode, memoryStream.ToArray(), error);
  }
}
