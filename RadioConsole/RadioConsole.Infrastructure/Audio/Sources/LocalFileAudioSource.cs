using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio.Sources;

/// <summary>
/// Audio source for local audio files (MP3, WAV, FLAC, OGG).
/// </summary>
public class LocalFileAudioSource : SoundFlowAudioSourceBase
{
  private readonly string _filePath;

  /// <inheritdoc/>
  public override AudioSourceType SourceType => AudioSourceType.FilePlayer;

  /// <summary>
  /// Gets the file path for this audio source.
  /// </summary>
  public string FilePath => _filePath;

  /// <summary>
  /// Creates a new local file audio source.
  /// </summary>
  /// <param name="id">Unique identifier.</param>
  /// <param name="filePath">Path to the audio file.</param>
  /// <param name="channel">Target mixer channel.</param>
  /// <param name="logger">Logger instance.</param>
  public LocalFileAudioSource(
    string id,
    string filePath,
    MixerChannel channel,
    ILogger<LocalFileAudioSource> logger)
    : base(id, Path.GetFileName(filePath), channel, logger)
  {
    _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

    SetMetadata("FilePath", filePath);
    SetMetadata("FileName", Path.GetFileName(filePath));
    SetMetadata("Extension", Path.GetExtension(filePath).ToLowerInvariant());
  }

  /// <inheritdoc/>
  public override async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    if (_disposed)
    {
      throw new ObjectDisposedException(nameof(LocalFileAudioSource));
    }

    _logger.LogInformation("Initializing local file audio source: {FilePath}", _filePath);

    try
    {
      // Validate file exists
      if (!File.Exists(_filePath))
      {
        _logger.LogError("Audio file not found: {FilePath}", _filePath);
        Status = AudioSourceStatus.Error;
        throw new FileNotFoundException($"Audio file not found: {_filePath}", _filePath);
      }

      // Get file info
      var fileInfo = new FileInfo(_filePath);
      SetMetadata("FileSize", fileInfo.Length.ToString());
      SetMetadata("LastModified", fileInfo.LastWriteTimeUtc.ToString("O"));

      // Open the file stream
      _audioStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);

      Status = AudioSourceStatus.Ready;
      _logger.LogInformation("Local file audio source initialized successfully: {FilePath} ({Size} bytes)", _filePath, fileInfo.Length);
    }
    catch (FileNotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to initialize local file audio source: {FilePath}", _filePath);
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

    _logger.LogDebug("Disposing local file audio source: {FilePath}", _filePath);
    base.Dispose();
  }
}
