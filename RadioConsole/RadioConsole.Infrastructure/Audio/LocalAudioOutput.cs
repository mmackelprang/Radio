using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Local audio output implementation that plays directly to the configured audio sink.
/// </summary>
public class LocalAudioOutput : IAudioOutput
{
  private readonly ILogger<LocalAudioOutput> _logger;
  private readonly string _audioSinkId;
  private bool _isActive;
  private IAudioPlayer? _audioPlayer;

  public bool IsActive => _isActive;
  public string Name => $"Local Audio Output ({_audioSinkId})";

  /// <summary>
  /// Initializes a new instance of the LocalAudioOutput class.
  /// </summary>
  /// <param name="logger">The logger instance.</param>
  /// <param name="audioSinkId">The audio sink device ID to use (default = "default").</param>
  public LocalAudioOutput(ILogger<LocalAudioOutput> logger, string audioSinkId = "default")
  {
    _logger = logger;
    _audioSinkId = audioSinkId;
    _isActive = false;
  }

  public async Task InitializeAsync()
  {
    _logger.LogInformation("Initializing local audio output with sink: {AudioSinkId}", _audioSinkId);
    await Task.CompletedTask;
  }

  public async Task StartAsync(IAudioPlayer audioPlayer)
  {
    if (_isActive)
    {
      _logger.LogWarning("Local audio output is already active");
      return;
    }

    try
    {
      _logger.LogInformation("Starting local audio output");
      _audioPlayer = audioPlayer;

      // Initialize the audio player with the specified device
      if (!audioPlayer.IsInitialized)
      {
        await audioPlayer.InitializeAsync(_audioSinkId);
      }

      _isActive = true;
      _logger.LogInformation("Local audio output started successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to start local audio output");
      throw;
    }
  }

  public async Task StopAsync()
  {
    if (!_isActive)
    {
      _logger.LogWarning("Local audio output is not active");
      return;
    }

    try
    {
      _logger.LogInformation("Stopping local audio output");
      _isActive = false;
      _audioPlayer = null;
      _logger.LogInformation("Local audio output stopped successfully");
      await Task.CompletedTask;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to stop local audio output");
      throw;
    }
  }
}
