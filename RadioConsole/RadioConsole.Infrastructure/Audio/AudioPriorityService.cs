using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Implementation of audio priority and ducking management.
/// Manages volume levels based on audio source priorities.
/// </summary>
public class AudioPriorityService : IAudioPriorityService
{
  private readonly IAudioPlayer _audioPlayer;
  private readonly ILogger<AudioPriorityService> _logger;
  private readonly Dictionary<string, AudioPriority> _registeredSources;
  private readonly Dictionary<string, float> _originalVolumes;
  private readonly HashSet<string> _activeHighPrioritySources;
  private readonly SemaphoreSlim _lock;
  private float _duckPercentage;
  private const int FadeDurationMs = 300;

  public AudioPriorityService(IAudioPlayer audioPlayer, ILogger<AudioPriorityService> logger)
  {
    _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _registeredSources = new Dictionary<string, AudioPriority>();
    _originalVolumes = new Dictionary<string, float>();
    _activeHighPrioritySources = new HashSet<string>();
    _lock = new SemaphoreSlim(1, 1);
    _duckPercentage = 0.2f; // Default to 20%
  }

  public float DuckPercentage => _duckPercentage;

  public bool IsHighPriorityActive => _activeHighPrioritySources.Count > 0;

  public async Task RegisterSourceAsync(string sourceId, AudioPriority priority)
  {
    await _lock.WaitAsync();
    try
    {
      _registeredSources[sourceId] = priority;
      _logger.LogInformation("Registered audio source {SourceId} with priority {Priority}", sourceId, priority);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task UnregisterSourceAsync(string sourceId)
  {
    await _lock.WaitAsync();
    try
    {
      _registeredSources.Remove(sourceId);
      _originalVolumes.Remove(sourceId);
      _activeHighPrioritySources.Remove(sourceId);
      _logger.LogInformation("Unregistered audio source {SourceId}", sourceId);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task OnHighPriorityStartAsync(string sourceId)
  {
    await _lock.WaitAsync();
    try
    {
      if (!_registeredSources.TryGetValue(sourceId, out var priority))
      {
        _logger.LogWarning("Source {SourceId} not registered. Registering as High priority.", sourceId);
        _registeredSources[sourceId] = AudioPriority.High;
        priority = AudioPriority.High;
      }

      if (priority != AudioPriority.High)
      {
        _logger.LogWarning("OnHighPriorityStartAsync called for non-high priority source {SourceId}", sourceId);
        return;
      }

      var wasHighPriorityActive = _activeHighPrioritySources.Count > 0;
      _activeHighPrioritySources.Add(sourceId);

      // Only duck if this is the first high priority source
      if (!wasHighPriorityActive)
      {
        _logger.LogInformation("High priority audio started. Ducking low priority sources to {Percentage}%", _duckPercentage * 100);
        await DuckLowPrioritySourcesAsync();
      }
      else
      {
        _logger.LogInformation("High priority audio {SourceId} started, but other high priority sources already active", sourceId);
      }
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task OnHighPriorityEndAsync(string sourceId)
  {
    await _lock.WaitAsync();
    try
    {
      _activeHighPrioritySources.Remove(sourceId);

      // Only restore if no more high priority sources are active
      if (_activeHighPrioritySources.Count == 0)
      {
        _logger.LogInformation("All high priority audio finished. Restoring low priority sources to original volume");
        await RestoreLowPrioritySourcesAsync();
      }
      else
      {
        _logger.LogInformation("High priority audio {SourceId} ended, but other high priority sources still active", sourceId);
      }
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task SetDuckPercentageAsync(float percentage)
  {
    if (percentage < 0.0f || percentage > 1.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(percentage), "Duck percentage must be between 0.0 and 1.0");
    }

    await _lock.WaitAsync();
    try
    {
      _duckPercentage = percentage;
      _logger.LogInformation("Duck percentage set to {Percentage}%", percentage * 100);

      // If high priority is active, update the ducked volume
      if (IsHighPriorityActive)
      {
        await DuckLowPrioritySourcesAsync();
      }
    }
    finally
    {
      _lock.Release();
    }
  }

  private async Task DuckLowPrioritySourcesAsync()
  {
    var lowPrioritySources = _registeredSources
      .Where(kvp => kvp.Value == AudioPriority.Low)
      .Select(kvp => kvp.Key);

    var fadeTasks = new List<Task>();
    foreach (var sourceId in lowPrioritySources)
    {
      // Store original volume if not already stored
      if (!_originalVolumes.ContainsKey(sourceId))
      {
        // Assume volume is at 1.0 if not tracked
        _originalVolumes[sourceId] = 1.0f;
      }

      var originalVolume = _originalVolumes[sourceId];
      var targetVolume = originalVolume * _duckPercentage;

      _logger.LogDebug("Ducking source {SourceId} from {Original} to {Target}", 
        sourceId, originalVolume, targetVolume);

      fadeTasks.Add(FadeVolumeAsync(sourceId, targetVolume));
    }

    await Task.WhenAll(fadeTasks);
  }

  private async Task RestoreLowPrioritySourcesAsync()
  {
    var fadeTasks = new List<Task>();
    foreach (var kvp in _originalVolumes)
    {
      var sourceId = kvp.Key;
      var originalVolume = kvp.Value;

      _logger.LogDebug("Restoring source {SourceId} to original volume {Volume}", 
        sourceId, originalVolume);

      fadeTasks.Add(FadeVolumeAsync(sourceId, originalVolume));
    }

    await Task.WhenAll(fadeTasks);
  }

  private async Task FadeVolumeAsync(string sourceId, float targetVolume)
  {
    try
    {
      // Simple fade: set the volume directly
      // In a real implementation, you might want to gradually fade over FadeDurationMs
      await _audioPlayer.SetVolumeAsync(sourceId, targetVolume);

      // For a smooth fade, uncomment and implement:
      // await Task.Delay(FadeDurationMs);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error fading volume for source {SourceId} to {TargetVolume}", 
        sourceId, targetVolume);
    }
  }
}
