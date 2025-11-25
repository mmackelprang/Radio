using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio.Sources;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Factory for creating SoundFlow audio sources from various inputs.
/// Handles source type detection, creation, and lifecycle management.
/// </summary>
public class AudioSourceFactory : IAudioSourceFactory, IDisposable
{
  private readonly ILogger<AudioSourceFactory> _logger;
  private readonly ILoggerFactory _loggerFactory;
  private readonly ITextToSpeechService? _ttsService;
  private readonly Dictionary<string, ISoundFlowAudioSource> _sources;
  private readonly object _sourcesLock = new();
  private int _sourceCounter;
  private bool _disposed;

  private static readonly HashSet<string> _supportedExtensions = new(StringComparer.OrdinalIgnoreCase)
  {
    ".mp3", ".wav", ".flac", ".ogg", ".m4a", ".aac", ".wma"
  };

  /// <inheritdoc/>
  public IReadOnlyCollection<string> SupportedExtensions => _supportedExtensions;

  /// <summary>
  /// Creates a new audio source factory.
  /// </summary>
  /// <param name="logger">Logger instance.</param>
  /// <param name="loggerFactory">Logger factory for creating source loggers.</param>
  /// <param name="ttsService">Optional TTS service for voice sources.</param>
  public AudioSourceFactory(
    ILogger<AudioSourceFactory> logger,
    ILoggerFactory loggerFactory,
    ITextToSpeechService? ttsService = null)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    _ttsService = ttsService;
    _sources = new Dictionary<string, ISoundFlowAudioSource>();
    _sourceCounter = 0;
  }

  /// <inheritdoc/>
  public async Task<ISoundFlowAudioSource> CreateFromFileAsync(
    string filePath,
    MixerChannel channel = MixerChannel.Main,
    CancellationToken cancellationToken = default)
  {
    ThrowIfDisposed();

    _logger.LogInformation("Creating audio source from file: {FilePath}, Channel: {Channel}", filePath, channel);

    var id = GenerateSourceId("file");
    var source = new LocalFileAudioSource(
      id,
      filePath,
      channel,
      _loggerFactory.CreateLogger<LocalFileAudioSource>());

    await source.InitializeAsync(cancellationToken);
    TrackSource(source);

    _logger.LogInformation("Created file audio source: {SourceId}", id);
    return source;
  }

  /// <inheritdoc/>
  public async Task<ISoundFlowAudioSource> CreateFromUriAsync(
    string uri,
    MixerChannel channel = MixerChannel.Main,
    CancellationToken cancellationToken = default)
  {
    ThrowIfDisposed();

    _logger.LogInformation("Creating audio source from URI: {Uri}, Channel: {Channel}", uri, channel);

    var sourceType = DetectSourceType(uri);

    return sourceType switch
    {
      AudioSourceType.Spotify => await CreateSpotifyStreamAsync(uri, cancellationToken),
      AudioSourceType.FilePlayer => await CreateFromFileAsync(uri, channel, cancellationToken),
      AudioSourceType.FileEvent => await CreateEventSoundAsync(uri, cancellationToken),
      _ => throw new ArgumentException($"Unable to determine source type for URI: {uri}", nameof(uri))
    };
  }

  /// <inheritdoc/>
  public async Task<ISoundFlowAudioSource> CreateUsbInputAsync(
    string deviceId,
    string deviceName,
    MixerChannel channel = MixerChannel.Main,
    CancellationToken cancellationToken = default)
  {
    ThrowIfDisposed();

    _logger.LogInformation("Creating USB input audio source: {DeviceName} (ID: {DeviceId}), Channel: {Channel}", 
      deviceName, deviceId, channel);

    var id = GenerateSourceId("usb");
    var source = new USBInputAudioSource(
      id,
      deviceId,
      deviceName,
      channel,
      _loggerFactory.CreateLogger<USBInputAudioSource>());

    await source.InitializeAsync(cancellationToken);
    TrackSource(source);

    _logger.LogInformation("Created USB input audio source: {SourceId}", id);
    return source;
  }

  /// <inheritdoc/>
  public async Task<ISoundFlowAudioSource> CreateTtsAsync(
    string text,
    string? voice = null,
    float speed = 1.0f,
    CancellationToken cancellationToken = default)
  {
    ThrowIfDisposed();

    _logger.LogInformation("Creating TTS audio source: \"{Text}\" (Voice: {Voice}, Speed: {Speed})", 
      TruncateText(text, 50), voice ?? "default", speed);

    var id = GenerateSourceId("tts");
    var source = new TextToSpeechAudioSource(
      id,
      text,
      voice,
      speed,
      _ttsService,
      _loggerFactory.CreateLogger<TextToSpeechAudioSource>());

    await source.InitializeAsync(cancellationToken);
    TrackSource(source);

    _logger.LogInformation("Created TTS audio source: {SourceId}", id);
    return source;
  }

  /// <inheritdoc/>
  public async Task<ISoundFlowAudioSource> CreateSpotifyStreamAsync(
    string trackUri,
    CancellationToken cancellationToken = default)
  {
    ThrowIfDisposed();

    _logger.LogInformation("Creating Spotify stream audio source: {TrackUri}", trackUri);

    var id = GenerateSourceId("spotify");
    var source = new SpotifyStreamAudioSource(
      id,
      trackUri,
      MixerChannel.Main,
      _loggerFactory.CreateLogger<SpotifyStreamAudioSource>());

    await source.InitializeAsync(cancellationToken);
    TrackSource(source);

    _logger.LogInformation("Created Spotify stream audio source: {SourceId}", id);
    return source;
  }

  /// <inheritdoc/>
  public async Task<ISoundFlowAudioSource> CreateEventSoundAsync(
    string filePath,
    CancellationToken cancellationToken = default)
  {
    ThrowIfDisposed();

    _logger.LogInformation("Creating event sound audio source: {FilePath}", filePath);

    var id = GenerateSourceId("event");
    var source = new EventSoundAudioSource(
      id,
      filePath,
      _loggerFactory.CreateLogger<EventSoundAudioSource>());

    await source.InitializeAsync(cancellationToken);
    TrackSource(source);

    _logger.LogInformation("Created event sound audio source: {SourceId}", id);
    return source;
  }

  /// <inheritdoc/>
  public AudioSourceType DetectSourceType(string uriOrPath)
  {
    if (string.IsNullOrWhiteSpace(uriOrPath))
    {
      throw new ArgumentException("URI or path cannot be null or empty", nameof(uriOrPath));
    }

    var lower = uriOrPath.ToLowerInvariant();

    // Spotify URIs
    if (lower.StartsWith("spotify:") || lower.Contains("open.spotify.com"))
    {
      return AudioSourceType.Spotify;
    }

    // File paths
    if (File.Exists(uriOrPath))
    {
      var extension = Path.GetExtension(uriOrPath).ToLowerInvariant();
      if (_supportedExtensions.Contains(extension))
      {
        return AudioSourceType.FilePlayer;
      }
    }

    // HTTP/HTTPS streams could be various types
    if (lower.StartsWith("http://") || lower.StartsWith("https://"))
    {
      // Assume streaming audio
      return AudioSourceType.FilePlayer;
    }

    throw new ArgumentException($"Unable to detect source type for: {uriOrPath}", nameof(uriOrPath));
  }

  /// <inheritdoc/>
  public void DisposeSource(string sourceId)
  {
    lock (_sourcesLock)
    {
      if (_sources.TryGetValue(sourceId, out var source))
      {
        _logger.LogDebug("Disposing source: {SourceId}", sourceId);
        source.Dispose();
        _sources.Remove(sourceId);
      }
    }
  }

  /// <inheritdoc/>
  public void DisposeAllSources()
  {
    lock (_sourcesLock)
    {
      _logger.LogInformation("Disposing all audio sources ({Count} sources)", _sources.Count);
      foreach (var source in _sources.Values)
      {
        try
        {
          source.Dispose();
        }
        catch (Exception ex)
        {
          _logger.LogWarning(ex, "Error disposing source {SourceId}", source.Id);
        }
      }
      _sources.Clear();
    }
  }

  private string GenerateSourceId(string prefix)
  {
    var counter = Interlocked.Increment(ref _sourceCounter);
    var timestamp = DateTime.UtcNow.Ticks.ToString("X");
    return $"{prefix}-{counter:D4}-{timestamp[^8..]}";
  }

  private void TrackSource(ISoundFlowAudioSource source)
  {
    lock (_sourcesLock)
    {
      _sources[source.Id] = source;
    }
  }

  private static string TruncateText(string text, int maxLength)
  {
    if (text.Length <= maxLength)
    {
      return text;
    }
    return text.Substring(0, maxLength - 3) + "...";
  }

  private void ThrowIfDisposed()
  {
    if (_disposed)
    {
      throw new ObjectDisposedException(nameof(AudioSourceFactory));
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
    _logger.LogInformation("Disposing AudioSourceFactory");
    DisposeAllSources();
  }
}
