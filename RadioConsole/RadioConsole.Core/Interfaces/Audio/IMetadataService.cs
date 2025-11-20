namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Service for extracting metadata from audio files.
/// Supports common audio formats (MP3, FLAC, WAV, AAC, etc.).
/// </summary>
public interface IMetadataService
{
  /// <summary>
  /// Extract metadata from an audio file.
  /// </summary>
  /// <param name="filePath">Path to the audio file.</param>
  /// <returns>Audio metadata, or null if extraction fails.</returns>
  Task<AudioMetadata?> ExtractMetadataAsync(string filePath);

  /// <summary>
  /// Extract metadata from an audio stream.
  /// </summary>
  /// <param name="stream">Audio stream.</param>
  /// <param name="mimeType">MIME type of the audio (e.g., "audio/mpeg", "audio/flac").</param>
  /// <returns>Audio metadata, or null if extraction fails.</returns>
  Task<AudioMetadata?> ExtractMetadataAsync(Stream stream, string mimeType);

  /// <summary>
  /// Check if a file format is supported for metadata extraction.
  /// </summary>
  /// <param name="filePath">Path to the audio file.</param>
  /// <returns>True if the format is supported, false otherwise.</returns>
  bool IsFormatSupported(string filePath);
}

/// <summary>
/// Audio file metadata extracted from tags.
/// </summary>
public class AudioMetadata
{
  /// <summary>
  /// Track title.
  /// </summary>
  public string? Title { get; set; }

  /// <summary>
  /// Artist name(s).
  /// </summary>
  public string? Artist { get; set; }

  /// <summary>
  /// Album name.
  /// </summary>
  public string? Album { get; set; }

  /// <summary>
  /// Album artist.
  /// </summary>
  public string? AlbumArtist { get; set; }

  /// <summary>
  /// Genre.
  /// </summary>
  public string? Genre { get; set; }

  /// <summary>
  /// Year of release.
  /// </summary>
  public int? Year { get; set; }

  /// <summary>
  /// Track number on the album.
  /// </summary>
  public int? TrackNumber { get; set; }

  /// <summary>
  /// Total number of tracks on the album.
  /// </summary>
  public int? TrackCount { get; set; }

  /// <summary>
  /// Disc number for multi-disc albums.
  /// </summary>
  public int? DiscNumber { get; set; }

  /// <summary>
  /// Duration of the track in seconds.
  /// </summary>
  public double? DurationSeconds { get; set; }

  /// <summary>
  /// Bitrate in kbps.
  /// </summary>
  public int? BitRate { get; set; }

  /// <summary>
  /// Sample rate in Hz.
  /// </summary>
  public int? SampleRate { get; set; }

  /// <summary>
  /// Number of audio channels.
  /// </summary>
  public int? Channels { get; set; }

  /// <summary>
  /// Composer name(s).
  /// </summary>
  public string? Composer { get; set; }

  /// <summary>
  /// Comment/description.
  /// </summary>
  public string? Comment { get; set; }

  /// <summary>
  /// Album art/cover image (base64 encoded).
  /// </summary>
  public string? AlbumArtBase64 { get; set; }

  /// <summary>
  /// MIME type of the album art.
  /// </summary>
  public string? AlbumArtMimeType { get; set; }

  /// <summary>
  /// Lyrics.
  /// </summary>
  public string? Lyrics { get; set; }
}
