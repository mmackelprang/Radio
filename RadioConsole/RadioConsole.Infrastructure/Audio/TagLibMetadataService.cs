using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Implementation of metadata extraction service using TagLibSharp.
/// </summary>
public class TagLibMetadataService : IMetadataService
{
  private readonly ILogger<TagLibMetadataService> _logger;

  private static readonly string[] SupportedExtensions = new[]
  {
    ".mp3", ".m4a", ".aac", ".flac", ".ogg", ".wav", ".wma",
    ".ape", ".opus", ".aiff", ".tta", ".mpc", ".wv"
  };

  public TagLibMetadataService(ILogger<TagLibMetadataService> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <inheritdoc />
  public async Task<AudioMetadata?> ExtractMetadataAsync(string filePath)
  {
    if (string.IsNullOrWhiteSpace(filePath))
    {
      _logger.LogWarning("File path is null or empty");
      return null;
    }

    if (!File.Exists(filePath))
    {
      _logger.LogWarning("File not found: {FilePath}", filePath);
      return null;
    }

    return await Task.Run(() =>
    {
      try
      {
        using var file = TagLib.File.Create(filePath);
        return ExtractMetadataFromFile(file, filePath);
      }
      catch (TagLib.UnsupportedFormatException ex)
      {
        _logger.LogWarning(ex, "Unsupported audio format: {FilePath}", filePath);
        return null;
      }
      catch (TagLib.CorruptFileException ex)
      {
        _logger.LogWarning(ex, "Corrupt audio file: {FilePath}", filePath);
        return null;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error extracting metadata from file: {FilePath}", filePath);
        return null;
      }
    });
  }

  /// <inheritdoc />
  public async Task<AudioMetadata?> ExtractMetadataAsync(Stream stream, string mimeType)
  {
    if (stream == null)
    {
      _logger.LogWarning("Stream is null");
      return null;
    }

    return await Task.Run(() =>
    {
      try
      {
        // Create a file abstraction from the stream
        var abstraction = new StreamFileAbstraction("stream", stream);
        using var file = TagLib.File.Create(abstraction, mimeType, TagLib.ReadStyle.Average);
        return ExtractMetadataFromFile(file, "stream");
      }
      catch (TagLib.UnsupportedFormatException ex)
      {
        _logger.LogWarning(ex, "Unsupported audio format: {MimeType}", mimeType);
        return null;
      }
      catch (TagLib.CorruptFileException ex)
      {
        _logger.LogWarning(ex, "Corrupt audio stream");
        return null;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error extracting metadata from stream");
        return null;
      }
    });
  }

  /// <inheritdoc />
  public bool IsFormatSupported(string filePath)
  {
    if (string.IsNullOrWhiteSpace(filePath))
      return false;

    var extension = Path.GetExtension(filePath).ToLowerInvariant();
    return SupportedExtensions.Contains(extension);
  }

  private AudioMetadata ExtractMetadataFromFile(TagLib.File file, string sourceName)
  {
    var metadata = new AudioMetadata();

    try
    {
      var tag = file.Tag;
      if (tag != null)
      {
        metadata.Title = tag.Title;
        metadata.Artist = tag.FirstPerformer;
        metadata.Album = tag.Album;
        metadata.AlbumArtist = tag.FirstAlbumArtist;
        metadata.Genre = tag.FirstGenre;
        metadata.Year = (int?)tag.Year;
        metadata.TrackNumber = (int?)tag.Track;
        metadata.TrackCount = (int?)tag.TrackCount;
        metadata.DiscNumber = (int?)tag.Disc;
        metadata.Composer = tag.FirstComposer;
        metadata.Comment = tag.Comment;
        metadata.Lyrics = tag.Lyrics;

        // Extract album art
        if (tag.Pictures != null && tag.Pictures.Length > 0)
        {
          var picture = tag.Pictures[0];
          metadata.AlbumArtBase64 = Convert.ToBase64String(picture.Data.Data);
          metadata.AlbumArtMimeType = picture.MimeType;
        }
      }

      // Extract audio properties
      var properties = file.Properties;
      if (properties != null)
      {
        metadata.DurationSeconds = properties.Duration.TotalSeconds;
        metadata.BitRate = properties.AudioBitrate;
        metadata.SampleRate = properties.AudioSampleRate;
        metadata.Channels = properties.AudioChannels;
      }

      _logger.LogDebug("Extracted metadata from {Source}: Title={Title}, Artist={Artist}, Album={Album}",
        sourceName, metadata.Title, metadata.Artist, metadata.Album);
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Error extracting some metadata fields from {Source}", sourceName);
    }

    return metadata;
  }

  /// <summary>
  /// Stream file abstraction for TagLib to read from a stream.
  /// </summary>
  private class StreamFileAbstraction : TagLib.File.IFileAbstraction
  {
    private readonly Stream _stream;

    public StreamFileAbstraction(string name, Stream stream)
    {
      Name = name;
      _stream = stream;
    }

    public string Name { get; }

    public Stream ReadStream => _stream;

    public Stream WriteStream => _stream;

    public void CloseStream(Stream stream)
    {
      // Don't close the stream here - let the caller manage it
    }
  }
}
