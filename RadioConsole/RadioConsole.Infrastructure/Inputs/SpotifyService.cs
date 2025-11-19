using Microsoft.Extensions.Logging;
using RadioConsole.Core.Interfaces.Inputs;
using SpotifyAPI.Web;

namespace RadioConsole.Infrastructure.Inputs;

/// <summary>
/// Implementation of ISpotifyService using SpotifyAPI-NET.
/// Provides Spotify authentication, search, playback control, and album art fetching.
/// </summary>
public class SpotifyService : ISpotifyService
{
  private readonly ILogger<SpotifyService> _logger;
  private SpotifyClient? _spotifyClient;
  private bool _isAuthenticated;
  private bool _isPlaying;

  public bool IsAuthenticated => _isAuthenticated;
  public bool IsPlaying => _isPlaying;

  public SpotifyService(ILogger<SpotifyService> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <inheritdoc/>
  public async Task AuthenticateAsync(string clientId, string clientSecret)
  {
    if (string.IsNullOrWhiteSpace(clientId))
      throw new ArgumentException("Client ID cannot be null or empty.", nameof(clientId));
    
    if (string.IsNullOrWhiteSpace(clientSecret))
      throw new ArgumentException("Client secret cannot be null or empty.", nameof(clientSecret));

    _logger.LogInformation("Authenticating with Spotify using Client Credentials flow...");

    try
    {
      var config = SpotifyClientConfig.CreateDefault();
      var request = new ClientCredentialsRequest(clientId, clientSecret);
      var response = await new OAuthClient(config).RequestToken(request);
      
      _spotifyClient = new SpotifyClient(config.WithToken(response.AccessToken));
      _isAuthenticated = true;

      _logger.LogInformation("Successfully authenticated with Spotify.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to authenticate with Spotify");
      _isAuthenticated = false;
      throw;
    }
  }

  /// <inheritdoc/>
  public async Task<IEnumerable<SpotifyTrack>> SearchTracksAsync(string query, int limit = 20)
  {
    EnsureAuthenticated();

    _logger.LogInformation("Searching for tracks: {Query} (limit: {Limit})", query, limit);

    try
    {
      var searchRequest = new SearchRequest(SearchRequest.Types.Track, query)
      {
        Limit = limit
      };

      var searchResponse = await _spotifyClient!.Search.Item(searchRequest);

      var tracks = searchResponse.Tracks.Items?.Select(track => new SpotifyTrack
      {
        Id = track.Id,
        Uri = track.Uri,
        Name = track.Name,
        Artist = string.Join(", ", track.Artists.Select(a => a.Name)),
        Album = track.Album.Name,
        AlbumArtUrl = track.Album.Images?.FirstOrDefault()?.Url,
        DurationMs = track.DurationMs
      }).ToList() ?? new List<SpotifyTrack>();

      _logger.LogInformation("Found {Count} tracks for query: {Query}", tracks.Count, query);

      return tracks;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error searching for tracks: {Query}", query);
      throw;
    }
  }

  /// <inheritdoc/>
  public async Task<IEnumerable<SpotifyAlbum>> SearchAlbumsAsync(string query, int limit = 20)
  {
    EnsureAuthenticated();

    _logger.LogInformation("Searching for albums: {Query} (limit: {Limit})", query, limit);

    try
    {
      var searchRequest = new SearchRequest(SearchRequest.Types.Album, query)
      {
        Limit = limit
      };

      var searchResponse = await _spotifyClient!.Search.Item(searchRequest);

      var albums = searchResponse.Albums.Items?.Select(album => new SpotifyAlbum
      {
        Id = album.Id,
        Uri = album.Uri,
        Name = album.Name,
        Artist = string.Join(", ", album.Artists.Select(a => a.Name)),
        AlbumArtUrl = album.Images?.FirstOrDefault()?.Url,
        TotalTracks = album.TotalTracks,
        ReleaseDate = album.ReleaseDate
      }).ToList() ?? new List<SpotifyAlbum>();

      _logger.LogInformation("Found {Count} albums for query: {Query}", albums.Count, query);

      return albums;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error searching for albums: {Query}", query);
      throw;
    }
  }

  /// <inheritdoc/>
  public async Task PlayAsync(string trackUri)
  {
    EnsureAuthenticated();

    if (string.IsNullOrWhiteSpace(trackUri))
      throw new ArgumentException("Track URI cannot be null or empty.", nameof(trackUri));

    _logger.LogInformation("Playing track: {TrackUri}", trackUri);

    try
    {
      // Note: This requires user authentication with proper scopes.
      // For now, we'll log a warning as Client Credentials flow doesn't support playback control.
      _logger.LogWarning("Playback control requires user authentication with proper scopes. " +
                        "Client Credentials flow does not support playback operations.");
      
      // In a full implementation, you would use:
      // var playbackRequest = new PlayerResumePlaybackRequest
      // {
      //   Uris = new List<string> { trackUri }
      // };
      // await _spotifyClient!.Player.ResumePlayback(playbackRequest);
      
      _isPlaying = true;
      
      await Task.CompletedTask;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error playing track: {TrackUri}", trackUri);
      throw;
    }
  }

  /// <inheritdoc/>
  public async Task PauseAsync()
  {
    EnsureAuthenticated();

    _logger.LogInformation("Pausing playback...");

    try
    {
      // Note: This requires user authentication with proper scopes.
      _logger.LogWarning("Playback control requires user authentication with proper scopes.");
      
      // In a full implementation:
      // await _spotifyClient!.Player.PausePlayback();
      
      _isPlaying = false;
      
      await Task.CompletedTask;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error pausing playback");
      throw;
    }
  }

  /// <inheritdoc/>
  public async Task ResumeAsync()
  {
    EnsureAuthenticated();

    _logger.LogInformation("Resuming playback...");

    try
    {
      // Note: This requires user authentication with proper scopes.
      _logger.LogWarning("Playback control requires user authentication with proper scopes.");
      
      // In a full implementation:
      // await _spotifyClient!.Player.ResumePlayback();
      
      _isPlaying = true;
      
      await Task.CompletedTask;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error resuming playback");
      throw;
    }
  }

  /// <inheritdoc/>
  public async Task StopAsync()
  {
    EnsureAuthenticated();

    _logger.LogInformation("Stopping playback...");

    try
    {
      // Pause and reset
      await PauseAsync();
      
      _logger.LogInformation("Playback stopped.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error stopping playback");
      throw;
    }
  }

  /// <inheritdoc/>
  public async Task<string?> GetAlbumArtUrlAsync(string spotifyUri)
  {
    EnsureAuthenticated();

    if (string.IsNullOrWhiteSpace(spotifyUri))
      throw new ArgumentException("Spotify URI cannot be null or empty.", nameof(spotifyUri));

    _logger.LogInformation("Fetching album art for URI: {Uri}", spotifyUri);

    try
    {
      // Extract ID from URI (format: spotify:track:id or spotify:album:id)
      var parts = spotifyUri.Split(':');
      if (parts.Length < 3)
      {
        _logger.LogWarning("Invalid Spotify URI format: {Uri}", spotifyUri);
        return null;
      }

      var type = parts[1];
      var id = parts[2];

      switch (type.ToLowerInvariant())
      {
        case "track":
          var track = await _spotifyClient!.Tracks.Get(id);
          return track.Album.Images?.FirstOrDefault()?.Url;
        
        case "album":
          var album = await _spotifyClient!.Albums.Get(id);
          return album.Images?.FirstOrDefault()?.Url;
        
        default:
          _logger.LogWarning("Unsupported URI type for album art: {Type}", type);
          return null;
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error fetching album art for URI: {Uri}", spotifyUri);
      return null;
    }
  }

  /// <inheritdoc/>
  public async Task<SpotifyTrack?> GetCurrentlyPlayingAsync()
  {
    EnsureAuthenticated();

    _logger.LogInformation("Fetching currently playing track...");

    try
    {
      // Note: This requires user authentication with proper scopes.
      _logger.LogWarning("Fetching currently playing track requires user authentication with proper scopes.");
      
      // In a full implementation:
      // var currentlyPlaying = await _spotifyClient!.Player.GetCurrentPlayback();
      // if (currentlyPlaying?.Item is FullTrack track)
      // {
      //   return new SpotifyTrack { ... };
      // }
      
      return await Task.FromResult<SpotifyTrack?>(null);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error fetching currently playing track");
      return null;
    }
  }

  private void EnsureAuthenticated()
  {
    if (!_isAuthenticated || _spotifyClient == null)
    {
      throw new InvalidOperationException("Spotify service is not authenticated. Call AuthenticateAsync first.");
    }
  }
}
