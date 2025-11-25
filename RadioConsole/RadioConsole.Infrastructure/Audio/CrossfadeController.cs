using RadioConsole.Core.Configuration;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Controller for managing smooth audio transitions between sources.
/// </summary>
public class CrossfadeController : ICrossfadeController
{
  private readonly ILogger<CrossfadeController> _logger;
  private readonly IMixerService _mixerService;
  private readonly object _lock = new();

  private CrossfadeConfiguration _configuration;
  private bool _isInitialized;
  private bool _disposed;

  private CancellationTokenSource? _transitionCts;
  private Task? _activeTransition;
  private float _currentProgress;
  private string? _currentOutgoingSource;
  private string? _currentIncomingSource;

  /// <inheritdoc/>
  public bool IsInitialized => _isInitialized;

  /// <inheritdoc/>
  public bool IsTransitionInProgress
  {
    get
    {
      lock (_lock)
      {
        return _activeTransition != null && !_activeTransition.IsCompleted;
      }
    }
  }

  /// <inheritdoc/>
  public float CurrentProgress
  {
    get
    {
      lock (_lock)
      {
        return _currentProgress;
      }
    }
  }

  /// <inheritdoc/>
  public event EventHandler<CrossfadeEventArgs>? TransitionStarted;

  /// <inheritdoc/>
  public event EventHandler<CrossfadeEventArgs>? TransitionCompleted;

  /// <inheritdoc/>
  public event EventHandler<CrossfadeProgressEventArgs>? TransitionProgress;

  /// <inheritdoc/>
  public event EventHandler<CrossfadeEventArgs>? TransitionCancelled;

  /// <summary>
  /// Creates a new CrossfadeController instance.
  /// </summary>
  /// <param name="mixerService">The mixer service to control volumes.</param>
  /// <param name="logger">Logger instance.</param>
  public CrossfadeController(IMixerService mixerService, ILogger<CrossfadeController> logger)
  {
    _mixerService = mixerService ?? throw new ArgumentNullException(nameof(mixerService));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    _configuration = new CrossfadeConfiguration();

    _logger.LogInformation("CrossfadeController created");
  }

  /// <inheritdoc/>
  public async Task InitializeAsync(CrossfadeConfiguration configuration, CancellationToken cancellationToken = default)
  {
    if (_isInitialized)
    {
      _logger.LogWarning("CrossfadeController already initialized");
      return;
    }

    _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    _isInitialized = true;

    _logger.LogInformation("CrossfadeController initialized with default crossfade duration {Duration}ms",
      configuration.DefaultCrossfadeDurationMs);
    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public async Task CrossfadeAsync(string outgoingSourceId, string incomingSourceId, int durationMs, CancellationToken cancellationToken = default)
  {
    if (!_isInitialized)
    {
      throw new InvalidOperationException("CrossfadeController not initialized");
    }

    // Clamp duration to valid range
    durationMs = Math.Clamp(durationMs, 0, 10000);

    await CancelTransitionAsync();

    _logger.LogInformation("Starting crossfade from {Outgoing} to {Incoming} over {Duration}ms",
      outgoingSourceId, incomingSourceId, durationMs);

    lock (_lock)
    {
      _currentOutgoingSource = outgoingSourceId;
      _currentIncomingSource = incomingSourceId;
      _currentProgress = 0;
      _transitionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    }

    RaiseTransitionStarted(outgoingSourceId, incomingSourceId, TransitionType.Crossfade, durationMs);

    try
    {
      _activeTransition = PerformCrossfadeAsync(outgoingSourceId, incomingSourceId, durationMs, _transitionCts.Token);
      await _activeTransition;

      RaiseTransitionCompleted(outgoingSourceId, incomingSourceId, TransitionType.Crossfade, durationMs);
    }
    catch (OperationCanceledException)
    {
      _logger.LogDebug("Crossfade cancelled");
      RaiseTransitionCancelled(outgoingSourceId, incomingSourceId, TransitionType.Crossfade, durationMs);
    }
    finally
    {
      lock (_lock)
      {
        _currentOutgoingSource = null;
        _currentIncomingSource = null;
        _activeTransition = null;
      }
    }
  }

  /// <inheritdoc/>
  public async Task FadeInAsync(string sourceId, int durationMs, CancellationToken cancellationToken = default)
  {
    if (!_isInitialized)
    {
      throw new InvalidOperationException("CrossfadeController not initialized");
    }

    await CancelTransitionAsync();

    _logger.LogInformation("Starting fade-in for {Source} over {Duration}ms", sourceId, durationMs);

    lock (_lock)
    {
      _currentIncomingSource = sourceId;
      _currentProgress = 0;
      _transitionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    }

    RaiseTransitionStarted(null, sourceId, TransitionType.FadeIn, durationMs);

    try
    {
      _activeTransition = PerformFadeAsync(sourceId, 0, 1.0f, durationMs, _transitionCts.Token);
      await _activeTransition;

      RaiseTransitionCompleted(null, sourceId, TransitionType.FadeIn, durationMs);
    }
    catch (OperationCanceledException)
    {
      RaiseTransitionCancelled(null, sourceId, TransitionType.FadeIn, durationMs);
    }
    finally
    {
      lock (_lock)
      {
        _currentIncomingSource = null;
        _activeTransition = null;
      }
    }
  }

  /// <inheritdoc/>
  public async Task FadeOutAsync(string sourceId, int durationMs, CancellationToken cancellationToken = default)
  {
    if (!_isInitialized)
    {
      throw new InvalidOperationException("CrossfadeController not initialized");
    }

    await CancelTransitionAsync();

    _logger.LogInformation("Starting fade-out for {Source} over {Duration}ms", sourceId, durationMs);

    lock (_lock)
    {
      _currentOutgoingSource = sourceId;
      _currentProgress = 0;
      _transitionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    }

    RaiseTransitionStarted(sourceId, null, TransitionType.FadeOut, durationMs);

    try
    {
      // Get current volume as starting point
      var currentVolume = _mixerService.GetSourceVolume(sourceId) ?? 1.0f;
      _activeTransition = PerformFadeAsync(sourceId, currentVolume, 0, durationMs, _transitionCts.Token);
      await _activeTransition;

      RaiseTransitionCompleted(sourceId, null, TransitionType.FadeOut, durationMs);
    }
    catch (OperationCanceledException)
    {
      RaiseTransitionCancelled(sourceId, null, TransitionType.FadeOut, durationMs);
    }
    finally
    {
      lock (_lock)
      {
        _currentOutgoingSource = null;
        _activeTransition = null;
      }
    }
  }

  /// <inheritdoc/>
  public async Task EmergencyCutAsync(string? outgoingSourceId = null, string? incomingSourceId = null)
  {
    _logger.LogWarning("Performing emergency cut from {Outgoing} to {Incoming}",
      outgoingSourceId ?? "current", incomingSourceId ?? "none");

    // Cancel any in-progress transition
    await CancelTransitionAsync();

    // Immediately cut outgoing
    if (outgoingSourceId != null)
    {
      await _mixerService.SetSourceVolumeAsync(outgoingSourceId, 0, 0);
    }

    // Immediately start incoming at full volume
    if (incomingSourceId != null)
    {
      await _mixerService.SetSourceVolumeAsync(incomingSourceId, 1.0f, 0);
    }

    RaiseTransitionCompleted(outgoingSourceId, incomingSourceId, TransitionType.Cut, 0);
  }

  /// <inheritdoc/>
  public async Task CancelTransitionAsync()
  {
    CancellationTokenSource? cts;
    Task? transition;

    lock (_lock)
    {
      cts = _transitionCts;
      transition = _activeTransition;
    }

    if (cts != null && transition != null)
    {
      cts.Cancel();
      try
      {
        await transition;
      }
      catch (OperationCanceledException)
      {
        // Expected
      }
    }

    lock (_lock)
    {
      _transitionCts?.Dispose();
      _transitionCts = null;
      _activeTransition = null;
      _currentProgress = 0;
    }
  }

  /// <inheritdoc/>
  public async Task UpdateConfigurationAsync(CrossfadeConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);
    _configuration = configuration;
    _logger.LogInformation("CrossfadeController configuration updated");
    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public CrossfadeConfiguration GetConfiguration()
  {
    return _configuration;
  }

  private async Task PerformCrossfadeAsync(string outgoing, string incoming, int durationMs, CancellationToken cancellationToken)
  {
    const int updateIntervalMs = 16; // ~60fps update rate
    var steps = Math.Max(1, durationMs / updateIntervalMs);
    var stopwatch = Stopwatch.StartNew();

    // Start incoming at 0
    await _mixerService.SetSourceVolumeAsync(incoming, 0, 0, cancellationToken);

    for (int i = 0; i <= steps; i++)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var progress = (float)i / steps;
      var outgoingVolume = 1.0f - progress;
      var incomingVolume = progress;

      // Apply an equal-power crossfade curve for smoother transitions
      outgoingVolume = (float)Math.Cos(progress * Math.PI / 2);
      incomingVolume = (float)Math.Sin(progress * Math.PI / 2);

      await _mixerService.SetSourceVolumeAsync(outgoing, outgoingVolume, 0, cancellationToken);
      await _mixerService.SetSourceVolumeAsync(incoming, incomingVolume, 0, cancellationToken);

      lock (_lock)
      {
        _currentProgress = progress;
      }

      RaiseTransitionProgress(progress, outgoingVolume, incomingVolume,
        (int)stopwatch.ElapsedMilliseconds, durationMs - (int)stopwatch.ElapsedMilliseconds);

      if (i < steps)
      {
        await Task.Delay(updateIntervalMs, cancellationToken);
      }
    }

    // Ensure final values are set
    await _mixerService.SetSourceVolumeAsync(outgoing, 0, 0, cancellationToken);
    await _mixerService.SetSourceVolumeAsync(incoming, 1.0f, 0, cancellationToken);

    lock (_lock)
    {
      _currentProgress = 1.0f;
    }
  }

  private async Task PerformFadeAsync(string sourceId, float fromVolume, float toVolume, int durationMs, CancellationToken cancellationToken)
  {
    const int updateIntervalMs = 16;
    var steps = Math.Max(1, durationMs / updateIntervalMs);
    var stopwatch = Stopwatch.StartNew();

    for (int i = 0; i <= steps; i++)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var progress = (float)i / steps;
      var volume = fromVolume + (toVolume - fromVolume) * progress;

      // Apply ease-in/ease-out curve
      volume = ApplyEasingCurve(fromVolume, toVolume, progress);

      await _mixerService.SetSourceVolumeAsync(sourceId, volume, 0, cancellationToken);

      lock (_lock)
      {
        _currentProgress = progress;
      }

      RaiseTransitionProgress(progress, toVolume > fromVolume ? volume : 1 - volume,
        toVolume > fromVolume ? volume : 1 - volume,
        (int)stopwatch.ElapsedMilliseconds, durationMs - (int)stopwatch.ElapsedMilliseconds);

      if (i < steps)
      {
        await Task.Delay(updateIntervalMs, cancellationToken);
      }
    }

    // Ensure final value is set
    await _mixerService.SetSourceVolumeAsync(sourceId, toVolume, 0, cancellationToken);

    lock (_lock)
    {
      _currentProgress = 1.0f;
    }
  }

  private static float ApplyEasingCurve(float from, float to, float t)
  {
    // Ease-in-out cubic
    t = t < 0.5f ? 4 * t * t * t : 1 - (float)Math.Pow(-2 * t + 2, 3) / 2;
    return from + (to - from) * t;
  }

  private void RaiseTransitionStarted(string? outgoing, string? incoming, TransitionType type, int durationMs)
  {
    TransitionStarted?.Invoke(this, new CrossfadeEventArgs
    {
      OutgoingSourceId = outgoing,
      IncomingSourceId = incoming,
      TransitionType = type,
      DurationMs = durationMs
    });
  }

  private void RaiseTransitionCompleted(string? outgoing, string? incoming, TransitionType type, int durationMs)
  {
    TransitionCompleted?.Invoke(this, new CrossfadeEventArgs
    {
      OutgoingSourceId = outgoing,
      IncomingSourceId = incoming,
      TransitionType = type,
      DurationMs = durationMs
    });
  }

  private void RaiseTransitionCancelled(string? outgoing, string? incoming, TransitionType type, int durationMs)
  {
    TransitionCancelled?.Invoke(this, new CrossfadeEventArgs
    {
      OutgoingSourceId = outgoing,
      IncomingSourceId = incoming,
      TransitionType = type,
      DurationMs = durationMs
    });
  }

  private void RaiseTransitionProgress(float progress, float outVol, float inVol, int elapsed, int remaining)
  {
    TransitionProgress?.Invoke(this, new CrossfadeProgressEventArgs
    {
      Progress = progress,
      OutgoingVolume = outVol,
      IncomingVolume = inVol,
      ElapsedMs = elapsed,
      RemainingMs = Math.Max(0, remaining)
    });
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

    _transitionCts?.Cancel();
    _transitionCts?.Dispose();
    _transitionCts = null;

    _logger.LogInformation("CrossfadeController disposed");
  }
}
