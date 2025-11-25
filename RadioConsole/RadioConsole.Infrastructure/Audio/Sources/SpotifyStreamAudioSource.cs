using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio.Sources;

/// <summary>
/// Audio source for Spotify streaming.
/// Provides audio from Spotify through the Spotify Web API.
/// </summary>
public class SpotifyStreamAudioSource : SoundFlowAudioSourceBase
{
  private readonly string _trackUri;

  /// <inheritdoc/>
  public override AudioSourceType SourceType => AudioSourceType.Spotify;

  /// <summary>
  /// Gets the Spotify track or playlist URI.
  /// </summary>
  public string TrackUri => _trackUri;

  /// <summary>
  /// Creates a new Spotify stream audio source.
  /// </summary>
  /// <param name="id">Unique identifier.</param>
  /// <param name="trackUri">Spotify track or playlist URI.</param>
  /// <param name="channel">Target mixer channel.</param>
  /// <param name="logger">Logger instance.</param>
  public SpotifyStreamAudioSource(
    string id,
    string trackUri,
    MixerChannel channel,
    ILogger<SpotifyStreamAudioSource> logger)
    : base(id, ExtractNameFromUri(trackUri), channel, logger)
  {
    _trackUri = trackUri ?? throw new ArgumentNullException(nameof(trackUri));

    SetMetadata("TrackUri", trackUri);
    SetMetadata("SourceType", "Spotify");
  }

  private static string ExtractNameFromUri(string uri)
  {
    // Extract a reasonable name from the Spotify URI
    // e.g., "spotify:track:1234567890" -> "Spotify Track"
    if (string.IsNullOrEmpty(uri))
    {
      return "Spotify Stream";
    }

    if (uri.Contains("playlist", StringComparison.OrdinalIgnoreCase))
    {
      return "Spotify Playlist";
    }
    if (uri.Contains("album", StringComparison.OrdinalIgnoreCase))
    {
      return "Spotify Album";
    }
    if (uri.Contains("track", StringComparison.OrdinalIgnoreCase))
    {
      return "Spotify Track";
    }

    return "Spotify Stream";
  }

  /// <inheritdoc/>
  public override async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    if (_disposed)
    {
      throw new ObjectDisposedException(nameof(SpotifyStreamAudioSource));
    }

    _logger.LogInformation("Initializing Spotify stream audio source: {TrackUri}", _trackUri);

    try
    {
      // Spotify streaming integration would be handled here
      // The actual audio stream comes from the Spotify service

      Status = AudioSourceStatus.Ready;
      _logger.LogInformation("Spotify stream audio source initialized: {TrackUri}", _trackUri);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to initialize Spotify stream audio source: {TrackUri}", _trackUri);
      Status = AudioSourceStatus.Error;
      throw;
    }

    await Task.CompletedTask;
  }

  /// <summary>
  /// Updates the track information from Spotify metadata.
  /// </summary>
  /// <param name="trackName">Track name.</param>
  /// <param name="artistName">Artist name.</param>
  /// <param name="albumName">Album name.</param>
  /// <param name="durationMs">Track duration in milliseconds.</param>
  public void UpdateTrackInfo(string? trackName, string? artistName, string? albumName, int? durationMs)
  {
    if (!string.IsNullOrEmpty(trackName))
    {
      Name = trackName;
      SetMetadata("TrackName", trackName);
    }

    if (!string.IsNullOrEmpty(artistName))
    {
      SetMetadata("ArtistName", artistName);
    }

    if (!string.IsNullOrEmpty(albumName))
    {
      SetMetadata("AlbumName", albumName);
    }

    if (durationMs.HasValue)
    {
      SetMetadata("DurationMs", durationMs.Value.ToString());
    }

    _logger.LogDebug("Spotify track info updated: {TrackName} by {ArtistName}", trackName, artistName);
  }

  /// <inheritdoc/>
  public override void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _logger.LogDebug("Disposing Spotify stream audio source: {TrackUri}", _trackUri);
    base.Dispose();
  }
}
