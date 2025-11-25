using RadioConsole.Core;
using RadioConsole.Core.Configuration;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using SoundFlow.Abstracts;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Structs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SoundFlowAudioFormat = SoundFlow.Structs.AudioFormat;
using SoundFlowDeviceInfo = SoundFlow.Structs.DeviceInfo;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Implementation of IMixerService using SoundFlow's audio mixing capabilities.
/// Manages multiple audio channels with priority-based ducking and volume control.
/// </summary>
public class MixerService : IMixerService
{
  private readonly ILogger<MixerService> _logger;
  private readonly SoundFlowOptions _options;
  private readonly object _lock = new();
  private readonly Dictionary<MixerChannel, ChannelInfo> _channels;
  private readonly Dictionary<string, ChannelSourceInfo> _sourceMappings;
  private readonly PerformanceMetrics _metrics;

  private AudioEngine? _engine;
  private AudioPlaybackDevice? _playbackDevice;
  private Mixer? _masterMixer;
  private SoundFlowAudioFormat _format;
  private bool _isInitialized;
  private bool _disposed;
  private float _masterVolume = 1.0f;
  private float _duckLevel = 0.2f;
  private bool _isDuckingActive;

  /// <inheritdoc/>
  public bool IsInitialized => _isInitialized;

  /// <inheritdoc/>
  public float DuckLevel => _duckLevel;

  /// <inheritdoc/>
  public bool IsDuckingActive => _isDuckingActive;

  /// <inheritdoc/>
  public event EventHandler<MixerSourceEventArgs>? SourceAdded;

  /// <inheritdoc/>
  public event EventHandler<MixerSourceEventArgs>? SourceRemoved;

  /// <inheritdoc/>
  public event EventHandler<DuckingStateChangedEventArgs>? DuckingStateChanged;

  /// <inheritdoc/>
  public event EventHandler<ChannelVolumeChangedEventArgs>? ChannelVolumeChanged;

  /// <summary>
  /// Creates a new MixerService instance.
  /// </summary>
  /// <param name="logger">Logger instance.</param>
  /// <param name="options">SoundFlow configuration options.</param>
  public MixerService(
    ILogger<MixerService> logger,
    IOptions<SoundFlowOptions>? options = null)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _options = options?.Value ?? new SoundFlowOptions();

    _channels = new Dictionary<MixerChannel, ChannelInfo>
    {
      [MixerChannel.Main] = new ChannelInfo { Channel = MixerChannel.Main, Volume = 1.0f },
      [MixerChannel.Event] = new ChannelInfo { Channel = MixerChannel.Event, Volume = 1.0f },
      [MixerChannel.Voice] = new ChannelInfo { Channel = MixerChannel.Voice, Volume = 1.0f }
    };

    _sourceMappings = new Dictionary<string, ChannelSourceInfo>();
    _metrics = new PerformanceMetrics();

    _logger.LogInformation("MixerService created with options: SampleRate={SampleRate}, BufferSize={BufferSize}",
      _options.SampleRate, _options.BufferSize);
  }

  /// <inheritdoc/>
  public async Task InitializeAsync(string outputDeviceId, CancellationToken cancellationToken = default)
  {
    if (_isInitialized)
    {
      _logger.LogWarning("MixerService already initialized");
      return;
    }

    _logger.LogInformation("Initializing MixerService with output device: {DeviceId}", outputDeviceId);

    try
    {
      // Initialize the MiniAudioEngine
      _engine = new MiniAudioEngine();

      // Create audio format
      var sampleFormat = _options.BitDepth switch
      {
        8 => SampleFormat.U8,
        16 => SampleFormat.S16,
        24 => SampleFormat.S24,
        32 => SampleFormat.S32,
        _ => SampleFormat.S16
      };

      _format = new SoundFlowAudioFormat
      {
        SampleRate = _options.SampleRate,
        Channels = _options.Channels,
        Format = sampleFormat
      };

      // Find the device
      SoundFlowDeviceInfo? targetDevice = null;
      if (!string.IsNullOrEmpty(outputDeviceId) && outputDeviceId != Core.AudioConstants.DefaultDeviceId)
      {
        var devices = _engine.PlaybackDevices;
        if (devices != null && nint.TryParse(outputDeviceId, out var deviceIdNint))
        {
          targetDevice = devices.FirstOrDefault(d => d.Id == deviceIdNint);
        }
      }

      // Initialize playback device
      _playbackDevice = _engine.InitializePlaybackDevice(targetDevice, _format, null);
      _masterMixer = _playbackDevice.MasterMixer;

      _isInitialized = true;
      _logger.LogInformation("MixerService initialized successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to initialize MixerService");
      throw;
    }

    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public async Task AddSourceAsync(ISoundFlowAudioSource source, MixerChannel channel, CancellationToken cancellationToken = default)
  {
    ThrowIfNotInitialized();

    lock (_lock)
    {
      if (_sourceMappings.ContainsKey(source.Id))
      {
        _logger.LogWarning("Source {SourceId} already added to mixer", source.Id);
        return;
      }
    }

    _logger.LogInformation("Adding source {SourceId} ({Name}) to channel {Channel}", source.Id, source.Name, channel);
    _metrics.SourcesAdded++;

    try
    {
      var channelInfo = _channels[channel];

      // Create a decoder for the source's audio stream
      SoundComponent? component = null;
      var stream = source.GetAudioStream();
      if (stream != null && _engine != null)
      {
        var decoder = _engine.CreateDecoder(stream, _format);
        component = decoder as SoundComponent;
      }

      lock (_lock)
      {
        _sourceMappings[source.Id] = new ChannelSourceInfo
        {
          Source = source,
          Channel = channel,
          Component = component
        };

        channelInfo.Sources.Add(source);

        // Add component to master mixer
        if (component != null && _masterMixer != null)
        {
          _masterMixer.AddComponent(component);
        }
      }

      // Check if ducking should be applied
      UpdateDuckingState();

      SourceAdded?.Invoke(this, new MixerSourceEventArgs
      {
        SourceId = source.Id,
        Channel = channel,
        Source = source
      });

      _logger.LogInformation("Source {SourceId} added to channel {Channel} successfully", source.Id, channel);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to add source {SourceId} to mixer", source.Id);
      throw;
    }

    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public async Task RemoveSourceAsync(string sourceId, CancellationToken cancellationToken = default)
  {
    ThrowIfNotInitialized();

    _logger.LogInformation("Removing source {SourceId} from mixer", sourceId);
    _metrics.SourcesRemoved++;

    ChannelSourceInfo? sourceInfo;
    lock (_lock)
    {
      if (!_sourceMappings.TryGetValue(sourceId, out sourceInfo))
      {
        _logger.LogWarning("Source {SourceId} not found in mixer", sourceId);
        return;
      }

      var channelInfo = _channels[sourceInfo.Channel];
      channelInfo.Sources.Remove(sourceInfo.Source);

      // Remove component from master mixer
      if (sourceInfo.Component != null && _masterMixer != null)
      {
        _masterMixer.RemoveComponent(sourceInfo.Component);
        sourceInfo.Component.Dispose();
      }

      _sourceMappings.Remove(sourceId);
    }

    // Update ducking state
    UpdateDuckingState();

    SourceRemoved?.Invoke(this, new MixerSourceEventArgs
    {
      SourceId = sourceId,
      Channel = sourceInfo.Channel,
      Source = sourceInfo.Source
    });

    _logger.LogInformation("Source {SourceId} removed from mixer successfully", sourceId);

    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public IEnumerable<ISoundFlowAudioSource> GetChannelSources(MixerChannel channel)
  {
    lock (_lock)
    {
      return _channels[channel].Sources.ToList();
    }
  }

  /// <inheritdoc/>
  public IEnumerable<ISoundFlowAudioSource> GetAllSources()
  {
    lock (_lock)
    {
      return _sourceMappings.Values.Select(s => s.Source).ToList();
    }
  }

  /// <inheritdoc/>
  public async Task MoveSourceToChannelAsync(string sourceId, MixerChannel newChannel, CancellationToken cancellationToken = default)
  {
    ThrowIfNotInitialized();

    lock (_lock)
    {
      if (!_sourceMappings.TryGetValue(sourceId, out var sourceInfo))
      {
        _logger.LogWarning("Source {SourceId} not found in mixer", sourceId);
        return;
      }

      var oldChannel = sourceInfo.Channel;
      if (oldChannel == newChannel)
      {
        return;
      }

      _logger.LogInformation("Moving source {SourceId} from {OldChannel} to {NewChannel}", 
        sourceId, oldChannel, newChannel);

      // Remove from old channel
      var oldChannelInfo = _channels[oldChannel];
      oldChannelInfo.Sources.Remove(sourceInfo.Source);

      // Add to new channel
      var newChannelInfo = _channels[newChannel];
      newChannelInfo.Sources.Add(sourceInfo.Source);

      sourceInfo.Channel = newChannel;
    }

    UpdateDuckingState();

    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public float GetChannelVolume(MixerChannel channel)
  {
    lock (_lock)
    {
      return _channels[channel].Volume;
    }
  }

  /// <inheritdoc/>
  public async Task SetChannelVolumeAsync(MixerChannel channel, float volume, int rampDurationMs = 0, CancellationToken cancellationToken = default)
  {
    ThrowIfNotInitialized();

    var clampedVolume = Math.Clamp(volume, 0.0f, 1.0f);
    float oldVolume;

    lock (_lock)
    {
      oldVolume = _channels[channel].Volume;
      if (Math.Abs(oldVolume - clampedVolume) < 0.001f)
      {
        return;
      }

      _channels[channel].Volume = clampedVolume;

      // Apply volume to all sources in the channel
      var channelInfo = _channels[channel];
      foreach (var source in channelInfo.Sources)
      {
        source.Volume = clampedVolume;
      }
    }

    _logger.LogInformation("Channel {Channel} volume changed from {Old} to {New}", channel, oldVolume, clampedVolume);

    ChannelVolumeChanged?.Invoke(this, new ChannelVolumeChangedEventArgs
    {
      Channel = channel,
      OldVolume = oldVolume,
      NewVolume = clampedVolume
    });

    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public float GetMasterVolume() => _masterVolume;

  /// <inheritdoc/>
  public async Task SetMasterVolumeAsync(float volume, int rampDurationMs = 0, CancellationToken cancellationToken = default)
  {
    ThrowIfNotInitialized();

    var clampedVolume = Math.Clamp(volume, 0.0f, 1.0f);
    var oldVolume = _masterVolume;

    if (Math.Abs(oldVolume - clampedVolume) < 0.001f)
    {
      return;
    }

    _masterVolume = clampedVolume;

    if (_masterMixer != null)
    {
      if (rampDurationMs > 0)
      {
        await RampVolumeAsync(_masterMixer, oldVolume, clampedVolume, rampDurationMs, cancellationToken);
      }
      else
      {
        _masterMixer.Volume = clampedVolume;
      }
    }

    _logger.LogInformation("Master volume changed from {Old} to {New}", oldVolume, clampedVolume);
  }

  /// <inheritdoc/>
  public float? GetSourceVolume(string sourceId)
  {
    lock (_lock)
    {
      if (_sourceMappings.TryGetValue(sourceId, out var sourceInfo))
      {
        return sourceInfo.Source.Volume;
      }
    }
    return null;
  }

  /// <inheritdoc/>
  public async Task SetSourceVolumeAsync(string sourceId, float volume, int rampDurationMs = 0, CancellationToken cancellationToken = default)
  {
    ThrowIfNotInitialized();

    var clampedVolume = Math.Clamp(volume, 0.0f, 1.0f);

    lock (_lock)
    {
      if (_sourceMappings.TryGetValue(sourceId, out var sourceInfo))
      {
        sourceInfo.Source.Volume = clampedVolume;
        _logger.LogDebug("Source {SourceId} volume set to {Volume}", sourceId, clampedVolume);
      }
      else
      {
        _logger.LogWarning("Source {SourceId} not found for volume adjustment", sourceId);
      }
    }

    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public void SetDuckLevel(float level)
  {
    _duckLevel = Math.Clamp(level, 0.0f, 1.0f);
    _logger.LogInformation("Duck level set to {Level}", _duckLevel);

    if (_isDuckingActive)
    {
      // Re-apply ducking with new level
      ApplyDucking(true);
    }
  }

  private void UpdateDuckingState()
  {
    bool hasHighPriority;
    lock (_lock)
    {
      hasHighPriority = _channels[MixerChannel.Event].Sources.Any() ||
                        _channels[MixerChannel.Voice].Sources.Any();
    }

    var wasActive = _isDuckingActive;
    _isDuckingActive = hasHighPriority;

    if (wasActive != _isDuckingActive)
    {
      _logger.LogInformation("Ducking state changed: {IsDucking}", _isDuckingActive);
      ApplyDucking(_isDuckingActive);

      DuckingStateChanged?.Invoke(this, new DuckingStateChangedEventArgs
      {
        IsDucking = _isDuckingActive,
        TriggerChannel = hasHighPriority ? 
          (_channels[MixerChannel.Voice].Sources.Any() ? MixerChannel.Voice : MixerChannel.Event) : null,
        DuckLevel = _duckLevel
      });
    }
  }

  private void ApplyDucking(bool duck)
  {
    lock (_lock)
    {
      var mainChannel = _channels[MixerChannel.Main];
      foreach (var source in mainChannel.Sources)
      {
        var targetVolume = duck ? mainChannel.Volume * _duckLevel : mainChannel.Volume;
        source.Volume = targetVolume;
        _logger.LogDebug("Source {SourceId} volume set to {Volume} (ducking: {IsDucking})", source.Id, targetVolume, duck);
      }
    }
  }

  private async Task RampVolumeAsync(Mixer mixer, float from, float to, int durationMs, CancellationToken cancellationToken)
  {
    const int steps = 20;
    var stepDuration = durationMs / steps;
    var volumeStep = (to - from) / steps;

    for (int i = 1; i <= steps; i++)
    {
      if (cancellationToken.IsCancellationRequested)
      {
        break;
      }

      mixer.Volume = from + (volumeStep * i);
      await Task.Delay(stepDuration, cancellationToken);
    }

    mixer.Volume = to; // Ensure final value is exact
  }

  private void ThrowIfNotInitialized()
  {
    if (!_isInitialized)
    {
      throw new InvalidOperationException("MixerService is not initialized. Call InitializeAsync first.");
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
    _logger.LogInformation("Disposing MixerService");

    // Dispose all sources
    lock (_lock)
    {
      foreach (var mapping in _sourceMappings.Values)
      {
        mapping.Component?.Dispose();
      }
      _sourceMappings.Clear();

      foreach (var channel in _channels.Values)
      {
        channel.Sources.Clear();
      }
    }

    _playbackDevice?.Dispose();
    _engine?.Dispose();

    _isInitialized = false;

    // Log metrics
    _logger.LogInformation("MixerService disposed. Metrics: SourcesAdded={Added}, SourcesRemoved={Removed}",
      _metrics.SourcesAdded, _metrics.SourcesRemoved);
  }

  private class ChannelInfo
  {
    public MixerChannel Channel { get; set; }
    public float Volume { get; set; } = 1.0f;
    public List<ISoundFlowAudioSource> Sources { get; } = new();
  }

  private class ChannelSourceInfo
  {
    public ISoundFlowAudioSource Source { get; set; } = null!;
    public MixerChannel Channel { get; set; }
    public SoundComponent? Component { get; set; }
  }

  private class PerformanceMetrics
  {
    public int SourcesAdded { get; set; }
    public int SourcesRemoved { get; set; }
  }
}
