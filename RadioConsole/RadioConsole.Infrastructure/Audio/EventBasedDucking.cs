using RadioConsole.Core.Configuration;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Event-based ducking system that monitors audio levels and triggers ducking automatically.
/// Provides both automatic (level-based) and manual ducking triggers.
/// </summary>
public class EventBasedDucking : IDisposable
{
  private readonly ILogger<EventBasedDucking> _logger;
  private readonly IDuckingService _duckingService;
  private readonly IMixerService _mixerService;
  private readonly object _lock = new();

  private bool _isInitialized;
  private bool _disposed;
  private bool _automaticDuckingEnabled;
  private float _voiceActivityThreshold = 0.1f;
  private CancellationTokenSource? _monitoringCts;
  private Task? _monitoringTask;

  private readonly Dictionary<string, AudioLevelMonitor> _levelMonitors;
  private readonly HashSet<string> _manualDuckingSources;

  /// <summary>
  /// Whether automatic ducking based on audio levels is enabled.
  /// </summary>
  public bool IsAutomaticDuckingEnabled => _automaticDuckingEnabled;

  /// <summary>
  /// Threshold for voice activity detection (0.0 to 1.0).
  /// </summary>
  public float VoiceActivityThreshold
  {
    get => _voiceActivityThreshold;
    set => _voiceActivityThreshold = Math.Clamp(value, 0.0f, 1.0f);
  }

  /// <summary>
  /// Event raised when voice activity is detected.
  /// </summary>
  public event EventHandler<VoiceActivityEventArgs>? VoiceActivityDetected;

  /// <summary>
  /// Event raised when audio level crosses threshold.
  /// </summary>
  public event EventHandler<AudioLevelEventArgs>? LevelThresholdCrossed;

  /// <summary>
  /// Event raised when a manual ducking trigger occurs.
  /// </summary>
  public event EventHandler<ManualDuckEventArgs>? ManualDuckTriggered;

  /// <summary>
  /// Creates a new EventBasedDucking instance.
  /// </summary>
  /// <param name="duckingService">The ducking service to control.</param>
  /// <param name="mixerService">The mixer service for level monitoring.</param>
  /// <param name="logger">Logger instance.</param>
  public EventBasedDucking(
    IDuckingService duckingService,
    IMixerService mixerService,
    ILogger<EventBasedDucking> logger)
  {
    _duckingService = duckingService ?? throw new ArgumentNullException(nameof(duckingService));
    _mixerService = mixerService ?? throw new ArgumentNullException(nameof(mixerService));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    _levelMonitors = new Dictionary<string, AudioLevelMonitor>();
    _manualDuckingSources = new HashSet<string>();

    _logger.LogInformation("EventBasedDucking created");
  }

  /// <summary>
  /// Initializes the event-based ducking system.
  /// </summary>
  /// <param name="enableAutomatic">Whether to enable automatic level-based ducking.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  public async Task InitializeAsync(bool enableAutomatic = false, CancellationToken cancellationToken = default)
  {
    if (_isInitialized)
    {
      _logger.LogWarning("EventBasedDucking already initialized");
      return;
    }

    _automaticDuckingEnabled = enableAutomatic;
    _isInitialized = true;

    // Subscribe to mixer source events
    _mixerService.SourceAdded += OnSourceAdded;
    _mixerService.SourceRemoved += OnSourceRemoved;

    if (enableAutomatic)
    {
      await StartMonitoringAsync(cancellationToken);
    }

    _logger.LogInformation("EventBasedDucking initialized (automatic: {Automatic})", enableAutomatic);
  }

  /// <summary>
  /// Manually triggers ducking from a specified channel.
  /// </summary>
  /// <param name="triggerChannel">The channel that should trigger ducking.</param>
  /// <param name="sourceId">Unique identifier for this duck trigger.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  public async Task TriggerDuckAsync(MixerChannel triggerChannel, string sourceId, CancellationToken cancellationToken = default)
  {
    if (!_isInitialized)
    {
      throw new InvalidOperationException("EventBasedDucking not initialized");
    }

    _logger.LogInformation("Manual duck trigger from channel {Channel} for source {Source}",
      triggerChannel, sourceId);

    lock (_lock)
    {
      _manualDuckingSources.Add(sourceId);
    }

    ManualDuckTriggered?.Invoke(this, new ManualDuckEventArgs
    {
      SourceId = sourceId,
      TriggerChannel = triggerChannel,
      IsStart = true
    });

    await _duckingService.StartDuckingAsync(triggerChannel, sourceId, cancellationToken);
  }

  /// <summary>
  /// Releases a manual duck trigger.
  /// </summary>
  /// <param name="sourceId">The source ID that triggered the duck.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  public async Task ReleaseDuckAsync(string sourceId, CancellationToken cancellationToken = default)
  {
    if (!_isInitialized)
    {
      return;
    }

    bool wasManual;
    lock (_lock)
    {
      wasManual = _manualDuckingSources.Remove(sourceId);
    }

    if (wasManual)
    {
      _logger.LogInformation("Releasing manual duck for source {Source}", sourceId);

      ManualDuckTriggered?.Invoke(this, new ManualDuckEventArgs
      {
        SourceId = sourceId,
        TriggerChannel = MixerChannel.Main,
        IsStart = false
      });
    }

    await _duckingService.EndDuckingAsync(sourceId, cancellationToken);
  }

  /// <summary>
  /// Triggers an emergency duck (instant, full mute).
  /// </summary>
  /// <param name="sourceId">Source ID for the emergency.</param>
  public async Task TriggerEmergencyDuckAsync(string sourceId)
  {
    if (!_isInitialized)
    {
      throw new InvalidOperationException("EventBasedDucking not initialized");
    }

    _logger.LogWarning("Emergency duck triggered for source {Source}", sourceId);

    lock (_lock)
    {
      _manualDuckingSources.Add(sourceId);
    }

    await _duckingService.ApplyEmergencyDuckAsync(MixerChannel.Event, sourceId);
  }

  /// <summary>
  /// Enables or disables automatic level-based ducking.
  /// </summary>
  /// <param name="enabled">Whether to enable automatic ducking.</param>
  public async Task SetAutomaticDuckingAsync(bool enabled)
  {
    _automaticDuckingEnabled = enabled;

    if (enabled && _monitoringTask == null)
    {
      await StartMonitoringAsync(CancellationToken.None);
    }
    else if (!enabled && _monitoringTask != null)
    {
      await StopMonitoringAsync();
    }

    _logger.LogInformation("Automatic ducking {State}", enabled ? "enabled" : "disabled");
  }

  /// <summary>
  /// Registers a source for level monitoring.
  /// </summary>
  /// <param name="sourceId">Source ID to monitor.</param>
  /// <param name="channel">Channel the source is on.</param>
  /// <param name="threshold">Level threshold for triggering (0.0 to 1.0).</param>
  public void RegisterLevelMonitor(string sourceId, MixerChannel channel, float threshold = 0.1f)
  {
    lock (_lock)
    {
      _levelMonitors[sourceId] = new AudioLevelMonitor
      {
        SourceId = sourceId,
        Channel = channel,
        Threshold = Math.Clamp(threshold, 0.0f, 1.0f),
        IsAboveThreshold = false
      };
    }

    _logger.LogDebug("Registered level monitor for source {Source} on channel {Channel}",
      sourceId, channel);
  }

  /// <summary>
  /// Unregisters a source from level monitoring.
  /// </summary>
  /// <param name="sourceId">Source ID to stop monitoring.</param>
  public void UnregisterLevelMonitor(string sourceId)
  {
    lock (_lock)
    {
      _levelMonitors.Remove(sourceId);
    }
  }

  /// <summary>
  /// Gets the current monitored audio level for a source.
  /// </summary>
  /// <param name="sourceId">Source ID.</param>
  /// <returns>Current level, or null if not monitored.</returns>
  public float? GetMonitoredLevel(string sourceId)
  {
    lock (_lock)
    {
      if (_levelMonitors.TryGetValue(sourceId, out var monitor))
      {
        return monitor.CurrentLevel;
      }
    }
    return null;
  }

  private async Task StartMonitoringAsync(CancellationToken cancellationToken)
  {
    _monitoringCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    _monitoringTask = MonitorLevelsAsync(_monitoringCts.Token);
    await Task.CompletedTask;
  }

  private async Task StopMonitoringAsync()
  {
    _monitoringCts?.Cancel();
    if (_monitoringTask != null)
    {
      try
      {
        await _monitoringTask;
      }
      catch (OperationCanceledException)
      {
        // Expected
      }
    }
    _monitoringCts?.Dispose();
    _monitoringCts = null;
    _monitoringTask = null;
  }

  private async Task MonitorLevelsAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Starting audio level monitoring");

    while (!cancellationToken.IsCancellationRequested)
    {
      try
      {
        // Level monitoring integrates with SoundFlow's level metering via the IMixerService.
        // The GetSourceVolume method is used as a proxy for audio levels.
        // For production use, integrate with SoundFlow's LevelMeter or FFT analysis.
        await CheckLevelsAsync(cancellationToken);
        await Task.Delay(50, cancellationToken); // 20Hz update rate
      }
      catch (OperationCanceledException)
      {
        break;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error in level monitoring");
        await Task.Delay(1000, cancellationToken); // Back off on error
      }
    }

    _logger.LogInformation("Audio level monitoring stopped");
  }

  private async Task CheckLevelsAsync(CancellationToken cancellationToken)
  {
    List<AudioLevelMonitor> monitors;
    lock (_lock)
    {
      monitors = _levelMonitors.Values.ToList();
    }

    foreach (var monitor in monitors)
    {
      // Get the source volume as a proxy for level (real implementation would use actual levels)
      var level = _mixerService.GetSourceVolume(monitor.SourceId) ?? 0;

      var wasAbove = monitor.IsAboveThreshold;
      monitor.CurrentLevel = level;
      monitor.IsAboveThreshold = level > monitor.Threshold;

      // Detect threshold crossing
      if (monitor.IsAboveThreshold && !wasAbove)
      {
        // Rising edge - potential voice activity
        _logger.LogDebug("Level threshold crossed for source {Source} (level: {Level})",
          monitor.SourceId, level);

        LevelThresholdCrossed?.Invoke(this, new AudioLevelEventArgs
        {
          SourceId = monitor.SourceId,
          Channel = monitor.Channel,
          Level = level,
          IsRisingEdge = true
        });

        // If on voice channel, detect voice activity
        if (monitor.Channel == MixerChannel.Voice)
        {
          VoiceActivityDetected?.Invoke(this, new VoiceActivityEventArgs
          {
            SourceId = monitor.SourceId,
            IsVoiceActive = true,
            Level = level
          });

          // Auto-trigger ducking
          if (_automaticDuckingEnabled)
          {
            await _duckingService.StartDuckingAsync(MixerChannel.Voice, $"auto-{monitor.SourceId}", cancellationToken);
          }
        }
      }
      else if (!monitor.IsAboveThreshold && wasAbove)
      {
        // Falling edge - voice ended
        _logger.LogDebug("Level dropped below threshold for source {Source}", monitor.SourceId);

        LevelThresholdCrossed?.Invoke(this, new AudioLevelEventArgs
        {
          SourceId = monitor.SourceId,
          Channel = monitor.Channel,
          Level = level,
          IsRisingEdge = false
        });

        if (monitor.Channel == MixerChannel.Voice)
        {
          VoiceActivityDetected?.Invoke(this, new VoiceActivityEventArgs
          {
            SourceId = monitor.SourceId,
            IsVoiceActive = false,
            Level = level
          });

          // Auto-release ducking
          if (_automaticDuckingEnabled)
          {
            await _duckingService.EndDuckingAsync($"auto-{monitor.SourceId}", cancellationToken);
          }
        }
      }
    }
  }

  private void OnSourceAdded(object? sender, MixerSourceEventArgs e)
  {
    // Auto-register voice and event channel sources for monitoring
    if (e.Channel == MixerChannel.Voice || e.Channel == MixerChannel.Event)
    {
      RegisterLevelMonitor(e.SourceId, e.Channel);
    }
  }

  private void OnSourceRemoved(object? sender, MixerSourceEventArgs e)
  {
    UnregisterLevelMonitor(e.SourceId);

    // Clean up any active ducks for this source with proper error handling
    _ = CleanupSourceDucksAsync(e.SourceId);
  }

  private async Task CleanupSourceDucksAsync(string sourceId)
  {
    try
    {
      await ReleaseDuckAsync(sourceId);
      await ReleaseDuckAsync($"auto-{sourceId}");
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Error cleaning up ducks for removed source {SourceId}", sourceId);
    }
  }

  /// <inheritdoc/>
  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;

    // Unsubscribe from events
    _mixerService.SourceAdded -= OnSourceAdded;
    _mixerService.SourceRemoved -= OnSourceRemoved;

    // Stop monitoring
    _monitoringCts?.Cancel();
    _monitoringCts?.Dispose();

    lock (_lock)
    {
      _levelMonitors.Clear();
      _manualDuckingSources.Clear();
    }

    _logger.LogInformation("EventBasedDucking disposed");
  }

  private class AudioLevelMonitor
  {
    public string SourceId { get; set; } = string.Empty;
    public MixerChannel Channel { get; set; }
    public float Threshold { get; set; }
    public float CurrentLevel { get; set; }
    public bool IsAboveThreshold { get; set; }
  }
}

/// <summary>
/// Event arguments for voice activity detection.
/// </summary>
public class VoiceActivityEventArgs : EventArgs
{
  /// <summary>
  /// The source that detected voice activity.
  /// </summary>
  public string SourceId { get; set; } = string.Empty;

  /// <summary>
  /// Whether voice is currently active.
  /// </summary>
  public bool IsVoiceActive { get; set; }

  /// <summary>
  /// Current audio level.
  /// </summary>
  public float Level { get; set; }
}

/// <summary>
/// Event arguments for audio level threshold events.
/// </summary>
public class AudioLevelEventArgs : EventArgs
{
  /// <summary>
  /// The source ID.
  /// </summary>
  public string SourceId { get; set; } = string.Empty;

  /// <summary>
  /// The channel the source is on.
  /// </summary>
  public MixerChannel Channel { get; set; }

  /// <summary>
  /// Current audio level.
  /// </summary>
  public float Level { get; set; }

  /// <summary>
  /// True if level crossed threshold going up, false if going down.
  /// </summary>
  public bool IsRisingEdge { get; set; }
}

/// <summary>
/// Event arguments for manual duck triggers.
/// </summary>
public class ManualDuckEventArgs : EventArgs
{
  /// <summary>
  /// The source ID that triggered the duck.
  /// </summary>
  public string SourceId { get; set; } = string.Empty;

  /// <summary>
  /// The trigger channel.
  /// </summary>
  public MixerChannel TriggerChannel { get; set; }

  /// <summary>
  /// Whether this is a start (true) or end (false) event.
  /// </summary>
  public bool IsStart { get; set; }
}
