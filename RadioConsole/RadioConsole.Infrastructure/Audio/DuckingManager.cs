using RadioConsole.Core.Configuration;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Manages audio ducking using smooth volume transitions.
/// Coordinates priority-based volume reduction across mixer channels.
/// </summary>
public class DuckingManager : IDuckingService
{
  private readonly ILogger<DuckingManager> _logger;
  private readonly IMixerService _mixerService;
  private readonly object _lock = new();

  private DuckingConfiguration _configuration;
  private bool _isInitialized;
  private bool _disposed;

  private readonly Dictionary<MixerChannel, DuckingStatus> _channelStatus;
  private readonly Dictionary<string, ActiveDuck> _activeDucks;
  private readonly DuckingMetrics _metrics;
  private readonly Stopwatch _stopwatch;

  // Cached index of channel pairs by target channel for performance
  private Dictionary<MixerChannel, List<ChannelPairDuckingSettings>> _channelPairsByTarget;

  /// <inheritdoc/>
  public bool IsInitialized => _isInitialized;

  /// <inheritdoc/>
  public bool IsEnabled => _configuration?.Enabled ?? false;

  /// <inheritdoc/>
  public DuckingPreset ActivePreset => _configuration?.ActivePreset ?? DuckingPreset.Custom;

  /// <inheritdoc/>
  public bool IsDuckingActive
  {
    get
    {
      lock (_lock)
      {
        return _channelStatus.Values.Any(s => s.IsDucked);
      }
    }
  }

  /// <inheritdoc/>
  public event EventHandler<DuckingEventArgs>? DuckingStateChanged;

  /// <inheritdoc/>
  public event EventHandler<DuckingConfigurationChangedEventArgs>? ConfigurationChanged;

  /// <summary>
  /// Creates a new DuckingManager instance.
  /// </summary>
  /// <param name="mixerService">The mixer service to control volumes.</param>
  /// <param name="logger">Logger instance.</param>
  public DuckingManager(IMixerService mixerService, ILogger<DuckingManager> logger)
  {
    _mixerService = mixerService ?? throw new ArgumentNullException(nameof(mixerService));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    _configuration = DuckingConfiguration.CreateDefault();
    _channelStatus = new Dictionary<MixerChannel, DuckingStatus>
    {
      [MixerChannel.Main] = new DuckingStatus { Channel = MixerChannel.Main },
      [MixerChannel.Event] = new DuckingStatus { Channel = MixerChannel.Event },
      [MixerChannel.Voice] = new DuckingStatus { Channel = MixerChannel.Voice }
    };
    _activeDucks = new Dictionary<string, ActiveDuck>();
    _metrics = new DuckingMetrics();
    _stopwatch = Stopwatch.StartNew();
    _channelPairsByTarget = new Dictionary<MixerChannel, List<ChannelPairDuckingSettings>>();

    _logger.LogInformation("DuckingManager created");
  }

  /// <inheritdoc/>
  public async Task InitializeAsync(DuckingConfiguration configuration, CancellationToken cancellationToken = default)
  {
    if (_isInitialized)
    {
      _logger.LogWarning("DuckingManager already initialized");
      return;
    }

    _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    RebuildChannelPairIndex();
    _isInitialized = true;

    _logger.LogInformation("DuckingManager initialized with preset {Preset}", configuration.ActivePreset);
    await Task.CompletedTask;
  }

  /// <summary>
  /// Rebuilds the channel pair index for faster lookups.
  /// </summary>
  private void RebuildChannelPairIndex()
  {
    _channelPairsByTarget.Clear();

    foreach (var pair in _configuration.ChannelPairSettings.Values)
    {
      if (!_channelPairsByTarget.TryGetValue(pair.TargetChannel, out var list))
      {
        list = new List<ChannelPairDuckingSettings>();
        _channelPairsByTarget[pair.TargetChannel] = list;
      }
      list.Add(pair);
    }
  }

  /// <inheritdoc/>
  public DuckingStatus GetChannelDuckingStatus(MixerChannel channel)
  {
    lock (_lock)
    {
      if (_channelStatus.TryGetValue(channel, out var status))
      {
        return CloneStatus(status);
      }
      return new DuckingStatus { Channel = channel };
    }
  }

  /// <inheritdoc/>
  public async Task StartDuckingAsync(MixerChannel triggerChannel, string sourceId, CancellationToken cancellationToken = default)
  {
    if (!_isInitialized || !_configuration.Enabled)
    {
      _logger.LogDebug("Ducking not enabled or initialized, skipping StartDuckingAsync");
      return;
    }

    var startTime = _stopwatch.ElapsedMilliseconds;
    _logger.LogInformation("Starting ducking from trigger channel {TriggerChannel} for source {SourceId}",
      triggerChannel, sourceId);

    // Find all channel pairs where this trigger channel causes ducking
    var affectedPairs = _configuration.ChannelPairSettings.Values
      .Where(cp => cp.TriggerChannel == triggerChannel && cp.Enabled)
      .OrderByDescending(cp => cp.Priority)
      .ToList();

    if (!affectedPairs.Any())
    {
      _logger.LogDebug("No ducking pairs configured for trigger channel {TriggerChannel}", triggerChannel);
      return;
    }

    var duckTasks = new List<Task>();
    foreach (var pair in affectedPairs)
    {
      duckTasks.Add(ApplyDuckAsync(pair, sourceId, cancellationToken));
    }

    await Task.WhenAll(duckTasks);

    // Record active duck
    lock (_lock)
    {
      _activeDucks[sourceId] = new ActiveDuck
      {
        SourceId = sourceId,
        TriggerChannel = triggerChannel,
        AffectedChannels = affectedPairs.Select(p => p.TargetChannel).ToList(),
        StartedAt = DateTime.UtcNow
      };

      _metrics.TotalDuckingEvents++;
      var elapsed = _stopwatch.ElapsedMilliseconds - startTime;
      UpdateAttackMetrics(elapsed);
    }
  }

  /// <inheritdoc/>
  public async Task EndDuckingAsync(string sourceId, CancellationToken cancellationToken = default)
  {
    if (!_isInitialized)
    {
      return;
    }

    _logger.LogInformation("Ending ducking for source {SourceId}", sourceId);

    ActiveDuck? duck;
    lock (_lock)
    {
      if (!_activeDucks.TryGetValue(sourceId, out duck))
      {
        _logger.LogDebug("No active duck found for source {SourceId}", sourceId);
        return;
      }
      _activeDucks.Remove(sourceId);
    }

    // Restore volumes for each affected channel
    var restoreTasks = new List<Task>();
    foreach (var channel in duck.AffectedChannels)
    {
      restoreTasks.Add(RestoreChannelVolumeAsync(channel, sourceId, cancellationToken));
    }

    await Task.WhenAll(restoreTasks);
  }

  /// <inheritdoc/>
  public async Task ApplyEmergencyDuckAsync(MixerChannel triggerChannel, string sourceId)
  {
    if (!_isInitialized)
    {
      return;
    }

    _logger.LogWarning("Applying emergency duck from {TriggerChannel} for source {SourceId}",
      triggerChannel, sourceId);

    lock (_lock)
    {
      _metrics.EmergencyDuckCount++;
    }

    // Find all affected pairs and apply instantly
    var affectedPairs = _configuration.ChannelPairSettings.Values
      .Where(cp => cp.TriggerChannel == triggerChannel && cp.Enabled)
      .ToList();

    foreach (var pair in affectedPairs)
    {
      lock (_lock)
      {
        var status = _channelStatus[pair.TargetChannel];
        status.OriginalLevel = status.CurrentLevel;
        status.CurrentLevel = pair.Timing.DuckLevel;
        status.IsDucked = true;
        status.DuckingStartedAt = DateTime.UtcNow;
        status.Phase = DuckingPhase.Hold;
        status.TriggeringSourceIds.Add(sourceId);
        status.TriggeringChannels.Add(triggerChannel);
      }

      // Apply volume instantly (no ramp)
      await _mixerService.SetChannelVolumeAsync(pair.TargetChannel, pair.Timing.DuckLevel, 0);

      RaiseDuckingStateChanged(pair.TargetChannel, triggerChannel, sourceId, true, pair.Timing.DuckLevel);
    }

    // Record active duck
    lock (_lock)
    {
      _activeDucks[sourceId] = new ActiveDuck
      {
        SourceId = sourceId,
        TriggerChannel = triggerChannel,
        AffectedChannels = affectedPairs.Select(p => p.TargetChannel).ToList(),
        StartedAt = DateTime.UtcNow,
        IsEmergency = true
      };
    }
  }

  /// <inheritdoc/>
  public async Task SetPresetAsync(DuckingPreset preset)
  {
    var oldPreset = _configuration.ActivePreset;
    var newConfig = DuckingPresets.CreatePreset(preset);

    await UpdateConfigurationAsync(newConfig);

    _logger.LogInformation("Ducking preset changed from {OldPreset} to {NewPreset}", oldPreset, preset);
  }

  /// <inheritdoc/>
  public async Task UpdateConfigurationAsync(DuckingConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    var oldPreset = _configuration.ActivePreset;
    _configuration = configuration;
    RebuildChannelPairIndex();

    ConfigurationChanged?.Invoke(this, new DuckingConfigurationChangedEventArgs
    {
      NewConfiguration = configuration,
      PreviousPreset = oldPreset,
      NewPreset = configuration.ActivePreset
    });

    _logger.LogInformation("Ducking configuration updated to preset {Preset}", configuration.ActivePreset);
    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public DuckingConfiguration GetConfiguration()
  {
    return _configuration;
  }

  /// <inheritdoc/>
  public ChannelPairDuckingSettings? GetChannelPairSettings(MixerChannel triggerChannel, MixerChannel targetChannel)
  {
    var key = $"{triggerChannel}-{targetChannel}";
    return _configuration.ChannelPairSettings.GetValueOrDefault(key);
  }

  /// <inheritdoc/>
  public async Task UpdateChannelPairSettingsAsync(ChannelPairDuckingSettings settings)
  {
    ArgumentNullException.ThrowIfNull(settings);

    var key = $"{settings.TriggerChannel}-{settings.TargetChannel}";
    _configuration.ChannelPairSettings[key] = settings;
    RebuildChannelPairIndex();

    _logger.LogInformation("Updated channel pair settings for {Key}", key);
    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public async Task SetEnabledAsync(bool enabled)
  {
    _configuration.Enabled = enabled;

    if (!enabled)
    {
      // Restore all ducked channels
      await ResetAsync();
    }

    _logger.LogInformation("Ducking {State}", enabled ? "enabled" : "disabled");
  }

  /// <inheritdoc/>
  public DuckingMetrics GetMetrics()
  {
    lock (_lock)
    {
      return new DuckingMetrics
      {
        TotalDuckingEvents = _metrics.TotalDuckingEvents,
        AverageAttackTimeMs = _metrics.AverageAttackTimeMs,
        MaxAttackTimeMs = _metrics.MaxAttackTimeMs,
        AverageReleaseTimeMs = _metrics.AverageReleaseTimeMs,
        CascadingDuckCount = _metrics.CascadingDuckCount,
        EmergencyDuckCount = _metrics.EmergencyDuckCount,
        CpuUsage = _metrics.CpuUsage,
        LatencyMs = _metrics.LatencyMs
      };
    }
  }

  /// <inheritdoc/>
  public async Task ResetAsync()
  {
    _logger.LogInformation("Resetting all ducking state");

    lock (_lock)
    {
      _activeDucks.Clear();
    }

    // Restore all channels to original volume
    var restoreTasks = new List<Task>();
    foreach (var channel in _channelStatus.Keys)
    {
      lock (_lock)
      {
        var status = _channelStatus[channel];
        status.IsDucked = false;
        status.CurrentLevel = status.OriginalLevel;
        status.Phase = DuckingPhase.None;
        status.TriggeringChannels.Clear();
        status.TriggeringSourceIds.Clear();
        status.DuckingStartedAt = null;
      }

      restoreTasks.Add(_mixerService.SetChannelVolumeAsync(channel, 1.0f, 100));
    }

    await Task.WhenAll(restoreTasks);
  }

  private async Task ApplyDuckAsync(ChannelPairDuckingSettings pair, string sourceId, CancellationToken cancellationToken)
  {
    var targetChannel = pair.TargetChannel;
    var timing = pair.Timing;

    bool isNewDuck;
    lock (_lock)
    {
      var status = _channelStatus[targetChannel];

      if (status.IsDucked)
      {
        // Already ducked - track cascading
        _metrics.CascadingDuckCount++;
        isNewDuck = false;
      }
      else
      {
        status.OriginalLevel = _mixerService.GetChannelVolume(targetChannel);
        isNewDuck = true;
      }

      status.IsDucked = true;
      status.Phase = DuckingPhase.Attack;
      status.DuckingStartedAt = DateTime.UtcNow;
      status.TriggeringSourceIds.Add(sourceId);
      if (!status.TriggeringChannels.Contains(pair.TriggerChannel))
      {
        status.TriggeringChannels.Add(pair.TriggerChannel);
      }
    }

    // Calculate target volume - use the lowest duck level if multiple ducks
    var targetVolume = CalculateTargetDuckLevel(targetChannel);

    _logger.LogDebug("Applying duck to channel {Channel} with attack {AttackMs}ms to level {Level}",
      targetChannel, timing.AttackTimeMs, targetVolume);

    // Apply volume ramp
    await _mixerService.SetChannelVolumeAsync(targetChannel, targetVolume, timing.AttackTimeMs, cancellationToken);

    lock (_lock)
    {
      var status = _channelStatus[targetChannel];
      status.CurrentLevel = targetVolume;
      status.Phase = DuckingPhase.Hold;
    }

    if (isNewDuck)
    {
      RaiseDuckingStateChanged(targetChannel, pair.TriggerChannel, sourceId, true, targetVolume);
    }
  }

  private async Task RestoreChannelVolumeAsync(MixerChannel channel, string sourceId, CancellationToken cancellationToken)
  {
    DuckingStatus status;
    lock (_lock)
    {
      status = _channelStatus[channel];
      status.TriggeringSourceIds.Remove(sourceId);

      // If other sources are still causing ducking, don't restore yet
      if (status.TriggeringSourceIds.Count > 0)
      {
        _logger.LogDebug("Channel {Channel} still has {Count} ducking sources, not restoring",
          channel, status.TriggeringSourceIds.Count);
        return;
      }
    }

    // Get release timing
    var timing = GetTimingForChannel(channel);
    var releaseTimeMs = timing?.ReleaseTimeMs ?? _configuration.DefaultSettings.ReleaseTimeMs;

    _logger.LogDebug("Restoring channel {Channel} to original level {Level} with release {ReleaseMs}ms",
      channel, status.OriginalLevel, releaseTimeMs);

    lock (_lock)
    {
      status.Phase = DuckingPhase.Release;
    }

    // Apply volume ramp back to original
    await _mixerService.SetChannelVolumeAsync(channel, status.OriginalLevel, releaseTimeMs, cancellationToken);

    lock (_lock)
    {
      status.IsDucked = false;
      status.CurrentLevel = status.OriginalLevel;
      status.Phase = DuckingPhase.None;
      status.TriggeringChannels.Clear();
      status.DuckingStartedAt = null;
    }

    RaiseDuckingStateChanged(channel, MixerChannel.Main, sourceId, false, status.OriginalLevel);
  }

  private float CalculateTargetDuckLevel(MixerChannel channel)
  {
    // Use cached index for faster lookup
    if (!_channelPairsByTarget.TryGetValue(channel, out var pairs))
    {
      return 1.0f;
    }

    // Find the lowest duck level from all active ducks affecting this channel
    var minLevel = 1.0f;
    foreach (var pair in pairs)
    {
      if (pair.Enabled)
      {
        minLevel = Math.Min(minLevel, pair.Timing.DuckLevel);
      }
    }
    return minLevel;
  }

  private DuckingTimingSettings? GetTimingForChannel(MixerChannel channel)
  {
    // Use cached index for faster lookup
    if (!_channelPairsByTarget.TryGetValue(channel, out var pairs))
    {
      return null;
    }

    return pairs.FirstOrDefault(p => p.Enabled)?.Timing;
  }

  private void UpdateAttackMetrics(long attackTimeMs)
  {
    // Simple running average calculation
    var count = _metrics.TotalDuckingEvents;
    if (count <= 0)
    {
      return; // Avoid division by zero
    }

    _metrics.AverageAttackTimeMs =
      ((_metrics.AverageAttackTimeMs * (count - 1)) + attackTimeMs) / count;
    _metrics.MaxAttackTimeMs = Math.Max(_metrics.MaxAttackTimeMs, attackTimeMs);
    _metrics.LatencyMs = attackTimeMs;
  }

  private void RaiseDuckingStateChanged(MixerChannel affected, MixerChannel trigger, string sourceId, bool started, float level)
  {
    DuckingStateChanged?.Invoke(this, new DuckingEventArgs
    {
      AffectedChannel = affected,
      TriggerChannel = trigger,
      SourceId = sourceId,
      DuckingStarted = started,
      DuckLevel = level,
      Timestamp = DateTime.UtcNow
    });
  }

  private static DuckingStatus CloneStatus(DuckingStatus status)
  {
    return new DuckingStatus
    {
      Channel = status.Channel,
      IsDucked = status.IsDucked,
      CurrentLevel = status.CurrentLevel,
      OriginalLevel = status.OriginalLevel,
      TriggeringChannels = new List<MixerChannel>(status.TriggeringChannels),
      TriggeringSourceIds = new List<string>(status.TriggeringSourceIds),
      DuckingStartedAt = status.DuckingStartedAt,
      Phase = status.Phase
    };
  }

  /// <inheritdoc/>
  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    _isInitialized = false;

    lock (_lock)
    {
      _activeDucks.Clear();
      _channelStatus.Clear();
    }

    _logger.LogInformation("DuckingManager disposed");
  }

  private class ActiveDuck
  {
    public string SourceId { get; set; } = string.Empty;
    public MixerChannel TriggerChannel { get; set; }
    public List<MixerChannel> AffectedChannels { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public bool IsEmergency { get; set; }
  }
}
