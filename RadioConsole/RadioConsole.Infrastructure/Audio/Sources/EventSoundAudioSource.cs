using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio.Sources;

/// <summary>
/// Audio source for event sound effects (doorbell, notifications, alerts).
/// Routes to the Event channel for high-priority playback with ducking.
/// </summary>
public class EventSoundAudioSource : SoundFlowAudioSourceBase
{
  private readonly string _filePath;

  /// <inheritdoc/>
  public override AudioSourceType SourceType => AudioSourceType.FileEvent;

  /// <summary>
  /// Gets the file path for this sound effect.
  /// </summary>
  public string FilePath => _filePath;

  /// <summary>
  /// Creates a new event sound audio source.
  /// </summary>
  /// <param name="id">Unique identifier.</param>
  /// <param name="filePath">Path to the sound effect file.</param>
  /// <param name="logger">Logger instance.</param>
  public EventSoundAudioSource(
    string id,
    string filePath,
    ILogger<EventSoundAudioSource> logger)
    : base(id, Path.GetFileName(filePath), MixerChannel.Event, logger)
  {
    _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

    SetMetadata("FilePath", filePath);
    SetMetadata("FileName", Path.GetFileName(filePath));
    SetMetadata("Extension", Path.GetExtension(filePath).ToLowerInvariant());
    SetMetadata("SourceType", "EventSound");
  }

  /// <inheritdoc/>
  public override async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    if (_disposed)
    {
      throw new ObjectDisposedException(nameof(EventSoundAudioSource));
    }

    _logger.LogInformation("Initializing event sound audio source: {FilePath}", _filePath);

    try
    {
      // Validate file exists
      if (!File.Exists(_filePath))
      {
        _logger.LogError("Sound effect file not found: {FilePath}", _filePath);
        Status = AudioSourceStatus.Error;
        throw new FileNotFoundException($"Sound effect file not found: {_filePath}", _filePath);
      }

      // Get file info
      var fileInfo = new FileInfo(_filePath);
      SetMetadata("FileSize", fileInfo.Length.ToString());

      // Open the file stream
      _audioStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);

      Status = AudioSourceStatus.Ready;
      _logger.LogInformation("Event sound audio source initialized successfully: {FilePath} ({Size} bytes)", _filePath, fileInfo.Length);
    }
    catch (FileNotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to initialize event sound audio source: {FilePath}", _filePath);
      Status = AudioSourceStatus.Error;
      throw;
    }

    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public override async Task StopAsync(CancellationToken cancellationToken = default)
  {
    await base.StopAsync(cancellationToken);

    // Reset stream position if possible
    if (_audioStream?.CanSeek == true)
    {
      _audioStream.Position = 0;
    }
  }

  /// <inheritdoc/>
  public override void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _logger.LogDebug("Disposing event sound audio source: {FilePath}", _filePath);
    base.Dispose();
  }
}
