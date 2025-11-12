namespace RadioConsole.Api.Models;

/// <summary>
/// Configuration for Spotify integration
/// </summary>
public class SpotifyConfig
{
  /// <summary>
  /// Spotify Client ID (required for authentication)
  /// Can be set via environment variable SPOTIFY_CLIENT_ID
  /// </summary>
  public string ClientId { get; set; } = string.Empty;

  /// <summary>
  /// Spotify Client Secret (required for authentication)
  /// Can be set via environment variable SPOTIFY_CLIENT_SECRET
  /// </summary>
  public string ClientSecret { get; set; } = string.Empty;

  /// <summary>
  /// Spotify Refresh Token (for user authentication)
  /// Can be set via environment variable SPOTIFY_REFRESH_TOKEN
  /// </summary>
  public string? RefreshToken { get; set; }

  /// <summary>
  /// Redirect URI for OAuth flow (default: http://localhost:5000/callback)
  /// Can be set via environment variable SPOTIFY_REDIRECT_URI
  /// </summary>
  public string RedirectUri { get; set; } = "http://localhost:5000/callback";

  /// <summary>
  /// Whether to use simulation mode (for testing without credentials)
  /// </summary>
  public bool UseSimulation { get; set; }

  /// <summary>
  /// Maximum number of items to retrieve in list operations
  /// </summary>
  public int MaxItems { get; set; } = 50;

  /// <summary>
  /// Default market/country code for recommendations (e.g., "US", "GB")
  /// </summary>
  public string Market { get; set; } = "US";
}

/// <summary>
/// Spotify track information
/// </summary>
public class SpotifyTrack
{
  public string Id { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string Artist { get; set; } = string.Empty;
  public string Album { get; set; } = string.Empty;
  public string AlbumArtUrl { get; set; } = string.Empty;
  public int DurationMs { get; set; }
  public string Uri { get; set; } = string.Empty;
  public bool IsPlayable { get; set; } = true;
}

/// <summary>
/// Spotify playlist information
/// </summary>
public class SpotifyPlaylist
{
  public string Id { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Owner { get; set; } = string.Empty;
  public int TrackCount { get; set; }
  public string ImageUrl { get; set; } = string.Empty;
  public string Uri { get; set; } = string.Empty;
  public bool IsPublic { get; set; }
}

/// <summary>
/// Spotify search results
/// </summary>
public class SpotifySearchResults
{
  public List<SpotifyTrack> Tracks { get; set; } = new();
  public List<SpotifyPlaylist> Playlists { get; set; } = new();
  public int TotalTracks { get; set; }
  public int TotalPlaylists { get; set; }
}
