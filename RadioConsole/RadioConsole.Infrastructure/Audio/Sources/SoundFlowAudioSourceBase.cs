using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio.Sources;

/// <summary>
/// Base class for SoundFlow audio sources.
/// Provides common functionality for all audio source types.
/// </summary>
public abstract class SoundFlowAudioSourceBase : ISoundFlowAudioSource
{
  protected readonly ILogger _logger;
  protected readonly object _stateLock = new();
  protected AudioSourceStatus _status = AudioSourceStatus.Initializing;
  protected float _volume = 1.0f;
  protected bool _disposed;
  protected Stream? _audioStream;

  /// <inheritdoc/>
  public string Id { get; }

  /// <inheritdoc/>
  public string Name { get; protected set; }

  /// <inheritdoc/>
  public abstract AudioSourceType SourceType { get; }

  /// <inheritdoc/>
  public MixerChannel Channel { get; set; }

  /// <inheritdoc/>
  public AudioSourceStatus Status
  {
    get
    {
      lock (_stateLock)
      {
        return _status;
      }
    }
    protected set
    {
      AudioSourceStatus oldStatus;
      bool statusChanged = false;
      lock (_stateLock)
      {
        oldStatus = _status;
        if (_status != value)
        {
          _status = value;
          statusChanged = true;
        }
      }
      if (statusChanged)
      {
        OnStatusChanged(oldStatus, value);
      }
    }
  }

  /// <inheritdoc/>
  public bool IsActive => Status == AudioSourceStatus.Playing;

  /// <inheritdoc/>
  public float Volume
  {
    get => _volume;
    set => _volume = Math.Clamp(value, 0.0f, 1.0f);
  }

  /// <inheritdoc/>
  public IReadOnlyDictionary<string, string> Metadata => _metadata;
  protected readonly Dictionary<string, string> _metadata = new();

  /// <inheritdoc/>
  public event EventHandler<AudioSourceStatusChangedEventArgs>? StatusChanged;

  protected SoundFlowAudioSourceBase(string id, string name, MixerChannel channel, ILogger logger)
  {
    Id = id ?? throw new ArgumentNullException(nameof(id));
    Name = name ?? throw new ArgumentNullException(nameof(name));
    Channel = channel;
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    _metadata["CreatedAt"] = DateTime.UtcNow.ToString("O");
    _metadata["Channel"] = channel.ToString();
  }

  /// <inheritdoc/>
  public abstract Task InitializeAsync(CancellationToken cancellationToken = default);

  /// <inheritdoc/>
  public virtual async Task StartAsync(CancellationToken cancellationToken = default)
  {
    if (_disposed)
    {
      throw new ObjectDisposedException(GetType().Name);
    }

    if (Status != AudioSourceStatus.Ready && Status != AudioSourceStatus.Paused)
    {
      _logger.LogWarning("Cannot start source {SourceId} in status {Status}", Id, Status);
      return;
    }

    _logger.LogInformation("Starting audio source {SourceId} ({Name})", Id, Name);
    Status = AudioSourceStatus.Playing;
    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public virtual async Task StopAsync(CancellationToken cancellationToken = default)
  {
    if (_disposed)
    {
      return;
    }

    _logger.LogInformation("Stopping audio source {SourceId} ({Name})", Id, Name);
    Status = AudioSourceStatus.Stopped;
    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public virtual async Task PauseAsync(CancellationToken cancellationToken = default)
  {
    if (_disposed)
    {
      throw new ObjectDisposedException(GetType().Name);
    }

    if (Status != AudioSourceStatus.Playing)
    {
      _logger.LogWarning("Cannot pause source {SourceId} in status {Status}", Id, Status);
      return;
    }

    _logger.LogInformation("Pausing audio source {SourceId} ({Name})", Id, Name);
    Status = AudioSourceStatus.Paused;
    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public virtual async Task ResumeAsync(CancellationToken cancellationToken = default)
  {
    if (_disposed)
    {
      throw new ObjectDisposedException(GetType().Name);
    }

    if (Status != AudioSourceStatus.Paused)
    {
      _logger.LogWarning("Cannot resume source {SourceId} in status {Status}", Id, Status);
      return;
    }

    _logger.LogInformation("Resuming audio source {SourceId} ({Name})", Id, Name);
    Status = AudioSourceStatus.Playing;
    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public virtual Stream? GetAudioStream() => _audioStream;

  protected virtual void OnStatusChanged(AudioSourceStatus oldStatus, AudioSourceStatus newStatus)
  {
    _logger.LogDebug("Source {SourceId} status changed: {OldStatus} -> {NewStatus}", Id, oldStatus, newStatus);

    StatusChanged?.Invoke(this, new AudioSourceStatusChangedEventArgs
    {
      SourceId = Id,
      OldStatus = oldStatus,
      NewStatus = newStatus
    });
  }

  protected void SetMetadata(string key, string value)
  {
    _metadata[key] = value;
  }

  public virtual void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    _logger.LogDebug("Disposing audio source {SourceId} ({Name})", Id, Name);

    try
    {
      _audioStream?.Dispose();
      _audioStream = null;
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Error disposing audio stream for source {SourceId}", Id);
    }

    Status = AudioSourceStatus.Stopped;
  }
}
