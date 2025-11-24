using Microsoft.Extensions.Logging;
using RadioConsole.Core.Interfaces;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Core.Models;
using RadioConsole.Core.Enums;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Implementation of IAudioSourceManager.
/// Manages creation and lifecycle of audio sources using configuration.
/// </summary>
public class AudioSourceManager : IAudioSourceManager
{
  private readonly ILogger<AudioSourceManager> _logger;
  private readonly IConfigurationService _configurationService;
  private readonly IAudioPlayer _audioPlayer;
  private readonly IAudioPriorityService? _priorityService;
  private readonly ITextToSpeechService? _ttsService;
  private readonly Dictionary<string, AudioSourceInfo> _activeSources;
  private int _sourceCounter;

  /// <summary>
  /// Configuration component name for audio sources.
  /// </summary>
  public const string ConfigComponent = "AudioSource";

  public AudioSourceManager(
    ILogger<AudioSourceManager> logger,
    IConfigurationService configurationService,
    IAudioPlayer audioPlayer,
    IAudioPriorityService? priorityService = null,
    ITextToSpeechService? ttsService = null)
  {
    _logger = logger;
    _configurationService = configurationService;
    _audioPlayer = audioPlayer;
    _priorityService = priorityService;
    _ttsService = ttsService;
    _activeSources = new Dictionary<string, AudioSourceInfo>();
    _sourceCounter = 0;
  }

  /// <inheritdoc/>
  public async Task<string> CreateSpotifySourceAsync()
  {
    _logger.LogInformation("Creating Spotify audio source");

    try
    {
      // Read configuration
      var clientIdConfig = await _configurationService.LoadAsync(ConfigComponent, "ClientID");
      var clientSecretConfig = await _configurationService.LoadAsync(ConfigComponent, "ClientSecret");
      var refreshTokenConfig = await _configurationService.LoadAsync(ConfigComponent, "RefreshToken");

      var sourceId = GenerateSourceId("spotify");
      var sourceInfo = new AudioSourceInfo
      {
        Id = sourceId,
        Type = AudioSourceType.Spotify,
        Name = "Spotify",
        Status = AudioSourceStatus.Initializing,
        IsHighPriority = false,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
          ["ClientID"] = clientIdConfig?.Value ?? string.Empty,
          ["HasClientSecret"] = (!string.IsNullOrEmpty(clientSecretConfig?.Value)).ToString(),
          ["HasRefreshToken"] = (!string.IsNullOrEmpty(refreshTokenConfig?.Value)).ToString()
        }
      };

      _activeSources[sourceId] = sourceInfo;
      
      // Register with priority service if available (Low priority - music sources)
      if (_priorityService != null)
      {
        await _priorityService.RegisterSourceAsync(sourceId, AudioPriority.Low);
      }

      sourceInfo.Status = AudioSourceStatus.Ready;
      _logger.LogInformation("Spotify source created: {SourceId}", sourceId);
      
      return sourceId;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to create Spotify audio source");
      throw;
    }
  }

  /// <inheritdoc/>
  public async Task<string> CreateRadioSourceAsync()
  {
    _logger.LogInformation("Creating USB Radio audio source");

    try
    {
      // Read configuration
      var usbPortConfig = await _configurationService.LoadAsync(ConfigComponent, "USBRadio_USBPort");

      var sourceId = GenerateSourceId("radio");
      var sourceInfo = new AudioSourceInfo
      {
        Id = sourceId,
        Type = AudioSourceType.USBRadio,
        Name = "USB Radio",
        Status = AudioSourceStatus.Initializing,
        IsHighPriority = false,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
          ["USBPort"] = usbPortConfig?.Value ?? "default"
        }
      };

      _activeSources[sourceId] = sourceInfo;

      // Register with priority service if available (Low priority - music sources)
      if (_priorityService != null)
      {
        await _priorityService.RegisterSourceAsync(sourceId, AudioPriority.Low);
      }

      sourceInfo.Status = AudioSourceStatus.Ready;
      _logger.LogInformation("Radio source created: {SourceId}, USBPort: {USBPort}", 
        sourceId, usbPortConfig?.Value ?? "default");

      return sourceId;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to create USB Radio audio source");
      throw;
    }
  }

  /// <inheritdoc/>
  public async Task<string> CreateVinylRecordSourceAsync()
  {
    _logger.LogInformation("Creating Vinyl Record audio source");

    try
    {
      // Read configuration
      var usbPortConfig = await _configurationService.LoadAsync(ConfigComponent, "VinylRecord_USBPort");

      var sourceId = GenerateSourceId("vinyl");
      var sourceInfo = new AudioSourceInfo
      {
        Id = sourceId,
        Type = AudioSourceType.VinylRecord,
        Name = "Vinyl Record",
        Status = AudioSourceStatus.Initializing,
        IsHighPriority = false,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
          ["USBPort"] = usbPortConfig?.Value ?? "default"
        }
      };

      _activeSources[sourceId] = sourceInfo;

      // Register with priority service if available (Low priority - music sources)
      if (_priorityService != null)
      {
        await _priorityService.RegisterSourceAsync(sourceId, AudioPriority.Low);
      }

      sourceInfo.Status = AudioSourceStatus.Ready;
      _logger.LogInformation("Vinyl Record source created: {SourceId}, USBPort: {USBPort}", 
        sourceId, usbPortConfig?.Value ?? "default");

      return sourceId;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to create Vinyl Record audio source");
      throw;
    }
  }

  /// <inheritdoc/>
  public async Task<string> CreateFilePlayerSourceAsync(string? filePath = null)
  {
    _logger.LogInformation("Creating File Player audio source");

    try
    {
      // Read configuration if path not provided
      var path = filePath;
      if (string.IsNullOrEmpty(path))
      {
        var pathConfig = await _configurationService.LoadAsync(ConfigComponent, "FilePlayer_Path");
        path = pathConfig?.Value ?? string.Empty;
      }

      var sourceId = GenerateSourceId("fileplayer");
      var sourceInfo = new AudioSourceInfo
      {
        Id = sourceId,
        Type = AudioSourceType.FilePlayer,
        Name = "File Player",
        Status = AudioSourceStatus.Initializing,
        IsHighPriority = false,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
          ["Path"] = path
        }
      };

      _activeSources[sourceId] = sourceInfo;

      // Register with priority service if available (Low priority - music sources)
      if (_priorityService != null)
      {
        await _priorityService.RegisterSourceAsync(sourceId, AudioPriority.Low);
      }

      sourceInfo.Status = AudioSourceStatus.Ready;
      _logger.LogInformation("File Player source created: {SourceId}, Path: {Path}", sourceId, path);

      return sourceId;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to create File Player audio source");
      throw;
    }
  }

  /// <inheritdoc/>
  public async Task<string> CreateTtsEventSourceAsync(string ttsText, string ttsVoice, float speed = 1.0f)
  {
    _logger.LogInformation("Creating TTS Event audio source");

    try
    {
      // Read TTS configuration
      var ttsEngineConfig = await _configurationService.LoadAsync(ConfigComponent, "TTS_TTSEngine");

      var sourceId = GenerateSourceId("tts");
      var sourceInfo = new AudioSourceInfo
      {
        Id = sourceId,
        Type = AudioSourceType.TtsEvent,
        Name = "TTS Event",
        Status = AudioSourceStatus.Initializing,
        IsHighPriority = true, // TTS events are high priority
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
          ["Text"] = ttsText,
          ["Voice"] = ttsVoice,
          ["Speed"] = speed.ToString("F2"),
          ["TTSEngine"] = ttsEngineConfig?.Value ?? "espeak"
        }
      };

      _activeSources[sourceId] = sourceInfo;

      // Register with priority service as high priority (causes ducking)
      if (_priorityService != null)
      {
        await _priorityService.RegisterSourceAsync(sourceId, AudioPriority.High);
      }

      sourceInfo.Status = AudioSourceStatus.Ready;
      _logger.LogInformation("TTS Event source created: {SourceId}, Text: {Text}, Voice: {Voice}", 
        sourceId, ttsText, ttsVoice);

      return sourceId;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to create TTS Event audio source");
      throw;
    }
  }

  /// <inheritdoc/>
  public async Task<string> CreateFileEventSourceAsync(string filePath)
  {
    _logger.LogInformation("Creating File Event audio source for: {Path}", filePath);

    try
    {
      var sourceId = GenerateSourceId("fileevent");
      var sourceInfo = new AudioSourceInfo
      {
        Id = sourceId,
        Type = AudioSourceType.FileEvent,
        Name = "File Event",
        Status = AudioSourceStatus.Initializing,
        IsHighPriority = true, // File events are high priority
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
          ["Path"] = filePath
        }
      };

      _activeSources[sourceId] = sourceInfo;

      // Register with priority service as high priority (causes ducking)
      if (_priorityService != null)
      {
        await _priorityService.RegisterSourceAsync(sourceId, AudioPriority.High);
      }

      sourceInfo.Status = AudioSourceStatus.Ready;
      _logger.LogInformation("File Event source created: {SourceId}, Path: {Path}", sourceId, filePath);

      return sourceId;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to create File Event audio source");
      throw;
    }
  }

  /// <inheritdoc/>
  public Task<IEnumerable<AudioSourceInfo>> GetActiveSourcesAsync()
  {
    return Task.FromResult<IEnumerable<AudioSourceInfo>>(_activeSources.Values.ToList());
  }

  /// <inheritdoc/>
  public async Task StopSourceAsync(string sourceId)
  {
    _logger.LogInformation("Stopping audio source: {SourceId}", sourceId);

    if (_activeSources.TryGetValue(sourceId, out var sourceInfo))
    {
      // Unregister from priority service
      if (_priorityService != null)
      {
        await _priorityService.UnregisterSourceAsync(sourceId);
      }

      // Stop audio playback
      await _audioPlayer.StopAsync(sourceId);

      sourceInfo.Status = AudioSourceStatus.Stopped;
      _activeSources.Remove(sourceId);

      _logger.LogInformation("Audio source stopped and removed: {SourceId}", sourceId);
    }
    else
    {
      _logger.LogWarning("Attempted to stop non-existent source: {SourceId}", sourceId);
    }
  }

  /// <inheritdoc/>
  public async Task StopAllSourcesAsync()
  {
    _logger.LogInformation("Stopping all audio sources");

    var sourceIds = _activeSources.Keys.ToList();
    foreach (var sourceId in sourceIds)
    {
      await StopSourceAsync(sourceId);
    }

    _logger.LogInformation("All audio sources stopped");
  }

  /// <inheritdoc/>
  public Task<AudioSourceInfo?> GetSourceInfoAsync(string sourceId)
  {
    _activeSources.TryGetValue(sourceId, out var sourceInfo);
    return Task.FromResult(sourceInfo);
  }

  /// <inheritdoc/>
  public async Task PlaySourceAsync(string sourceId)
  {
    _logger.LogInformation("Playing audio source: {SourceId}", sourceId);

    if (_activeSources.TryGetValue(sourceId, out var sourceInfo))
    {
      try
      {
        // For high-priority sources, notify the priority service to duck other sources
        if (sourceInfo.IsHighPriority && _priorityService != null)
        {
          await _priorityService.OnHighPriorityStartAsync(sourceId);
        }

        // Start playback based on source type
        switch (sourceInfo.Type)
        {
          case AudioSourceType.FilePlayer:
          case AudioSourceType.FileEvent:
            var filePath = sourceInfo.Metadata.GetValueOrDefault("Path", string.Empty);
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
              using var fileStream = File.OpenRead(filePath);
              await _audioPlayer.PlayAsync(sourceId, fileStream);
            }
            else
            {
              _logger.LogWarning("File not found for source {SourceId}: {Path}", sourceId, filePath);
              sourceInfo.Status = AudioSourceStatus.Error;
              return;
            }
            break;

          case AudioSourceType.TtsEvent:
            // TTS requires the TTS service to generate audio
            if (_ttsService != null)
            {
              var text = sourceInfo.Metadata.GetValueOrDefault("Text", string.Empty);
              var speed = float.TryParse(sourceInfo.Metadata.GetValueOrDefault("Speed", "1.0"), out var s) ? s : 1.0f;
              if (!string.IsNullOrEmpty(text))
              {
                var audioStream = await _ttsService.SynthesizeSpeechAsync(text, null, speed);
                await _audioPlayer.PlayAsync(sourceId, audioStream);
              }
            }
            else
            {
              _logger.LogWarning("TTS service not available for source {SourceId}", sourceId);
              sourceInfo.Status = AudioSourceStatus.Error;
              return;
            }
            break;

          case AudioSourceType.Spotify:
          case AudioSourceType.USBRadio:
          case AudioSourceType.VinylRecord:
            // These sources require external input streams
            // For now, log that playback was initiated (actual implementation would connect to the source)
            _logger.LogInformation("Playback initiated for {Type} source: {SourceId}", sourceInfo.Type, sourceId);
            break;
        }

        sourceInfo.Status = AudioSourceStatus.Playing;
        _logger.LogInformation("Audio source playing: {SourceId}", sourceId);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to play audio source: {SourceId}", sourceId);
        sourceInfo.Status = AudioSourceStatus.Error;
        throw;
      }
    }
    else
    {
      _logger.LogWarning("Attempted to play non-existent source: {SourceId}", sourceId);
    }
  }

  /// <inheritdoc/>
  public async Task PauseSourceAsync(string sourceId)
  {
    _logger.LogInformation("Pausing audio source: {SourceId}", sourceId);

    if (_activeSources.TryGetValue(sourceId, out var sourceInfo))
    {
      if (sourceInfo.Status == AudioSourceStatus.Playing)
      {
        // For high-priority sources, notify the priority service
        if (sourceInfo.IsHighPriority && _priorityService != null)
        {
          await _priorityService.OnHighPriorityEndAsync(sourceId);
        }

        sourceInfo.Status = AudioSourceStatus.Paused;
        _logger.LogInformation("Audio source paused: {SourceId}", sourceId);
      }
      else
      {
        _logger.LogWarning("Source {SourceId} is not playing, cannot pause (status: {Status})", sourceId, sourceInfo.Status);
      }
    }
    else
    {
      _logger.LogWarning("Attempted to pause non-existent source: {SourceId}", sourceId);
    }

    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public async Task ResumeSourceAsync(string sourceId)
  {
    _logger.LogInformation("Resuming audio source: {SourceId}", sourceId);

    if (_activeSources.TryGetValue(sourceId, out var sourceInfo))
    {
      if (sourceInfo.Status == AudioSourceStatus.Paused)
      {
        // For high-priority sources, notify the priority service to duck other sources again
        if (sourceInfo.IsHighPriority && _priorityService != null)
        {
          await _priorityService.OnHighPriorityStartAsync(sourceId);
        }

        sourceInfo.Status = AudioSourceStatus.Playing;
        _logger.LogInformation("Audio source resumed: {SourceId}", sourceId);
      }
      else
      {
        _logger.LogWarning("Source {SourceId} is not paused, cannot resume (status: {Status})", sourceId, sourceInfo.Status);
      }
    }
    else
    {
      _logger.LogWarning("Attempted to resume non-existent source: {SourceId}", sourceId);
    }

    await Task.CompletedTask;
  }

  private string GenerateSourceId(string prefix)
  {
    return $"{prefix}-{++_sourceCounter:D4}-{Guid.NewGuid():N}".Substring(0, 24);
  }
}
