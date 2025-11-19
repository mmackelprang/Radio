namespace RadioConsole.Core.Interfaces.Inputs;

/// <summary>
/// Interface for Spotify integration using SpotifyAPI-NET.
/// Provides methods for authentication, search, playback control, and album art fetching.
/// </summary>
public interface ISpotifyService
{
  /// <summary>
  /// Authenticate with Spotify using OAuth or client credentials.
  /// </summary>
  /// <param name="clientId">Spotify application client ID.</param>
  /// <param name="clientSecret">Spotify application client secret.</param>
  Task AuthenticateAsync(string clientId, string clientSecret);

  /// <summary>
  /// Check if the service is currently authenticated.
  /// </summary>
  bool IsAuthenticated { get; }

  /// <summary>
  /// Search for tracks on Spotify.
  /// </summary>
  /// <param name="query">Search query string.</param>
  /// <param name="limit">Maximum number of results to return.</param>
  /// <returns>List of search results.</returns>
  Task<IEnumerable<SpotifyTrack>> SearchTracksAsync(string query, int limit = 20);

  /// <summary>
  /// Search for albums on Spotify.
  /// </summary>
  /// <param name="query">Search query string.</param>
  /// <param name="limit">Maximum number of results to return.</param>
  /// <returns>List of album search results.</returns>
  Task<IEnumerable<SpotifyAlbum>> SearchAlbumsAsync(string query, int limit = 20);

  /// <summary>
  /// Play a track by its Spotify URI.
  /// </summary>
  /// <param name="trackUri">Spotify track URI (e.g., "spotify:track:...").</param>
  Task PlayAsync(string trackUri);

  /// <summary>
  /// Pause the currently playing track.
  /// </summary>
  Task PauseAsync();

  /// <summary>
  /// Resume playback of the current track.
  /// </summary>
  Task ResumeAsync();

  /// <summary>
  /// Stop playback completely.
  /// </summary>
  Task StopAsync();

  /// <summary>
  /// Get the album art URL for a track or album.
  /// </summary>
  /// <param name="spotifyUri">Spotify URI (track or album).</param>
  /// <returns>URL to the album art image, or null if not found.</returns>
  Task<string?> GetAlbumArtUrlAsync(string spotifyUri);

  /// <summary>
  /// Get the currently playing track information.
  /// </summary>
  /// <returns>Currently playing track, or null if nothing is playing.</returns>
  Task<SpotifyTrack?> GetCurrentlyPlayingAsync();

  /// <summary>
  /// Check if Spotify is currently playing.
  /// </summary>
  bool IsPlaying { get; }
}

/// <summary>
/// Represents a Spotify track with metadata.
/// </summary>
public class SpotifyTrack
{
  public string Id { get; set; } = string.Empty;
  public string Uri { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string Artist { get; set; } = string.Empty;
  public string Album { get; set; } = string.Empty;
  public string? AlbumArtUrl { get; set; }
  public int DurationMs { get; set; }
}

/// <summary>
/// Represents a Spotify album with metadata.
/// </summary>
public class SpotifyAlbum
{
  public string Id { get; set; } = string.Empty;
  public string Uri { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string Artist { get; set; } = string.Empty;
  public string? AlbumArtUrl { get; set; }
  public int TotalTracks { get; set; }
  public string ReleaseDate { get; set; } = string.Empty;
}
