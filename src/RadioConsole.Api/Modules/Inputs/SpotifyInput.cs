using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Models;
using RadioConsole.Api.Services;
using SpotifyAPI.Web;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Api.Modules.Inputs;

/// <summary>
/// Spotify streaming input module with comprehensive API integration
/// Supports favorite songs, playlists, recently played, recommendations, and search
/// </summary>
public class SpotifyInput : BaseAudioInput
{
  private readonly ILogger<SpotifyInput>? _logger;
  private SpotifyConfig _spotifyConfig;
  private SpotifyClient? _spotify;
  private FullTrack? _currentTrack;
  private CancellationTokenSource? _playbackCts;
  private Task? _playbackTask;

  public override string Id => "spotify";
  public override string Name => "Spotify";
  public override string Description => "Spotify Streaming with full API integration";

  public SpotifyInput(
    IEnvironmentService environmentService, 
    IStorage storage,
    ILogger<SpotifyInput>? logger = null) 
    : base(environmentService, storage)
  {
    _logger = logger;
    _spotifyConfig = new SpotifyConfig();
  }

  public override async Task InitializeAsync()
  {
    await _configuration.LoadAsync();
    
    // Load Spotify configuration from storage or environment variables
    await LoadConfigurationAsync();
    
    if (_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation)
    {
      // Simulation mode - Spotify is available but mocked
      IsAvailable = true;
      _display.UpdateStatus("Spotify (Simulation Mode)");
      _logger?.LogInformation("Spotify initialized in simulation mode");
    }
    else
    {
      // Check for Spotify integration
      IsAvailable = await CheckSpotifyAvailabilityAsync();
      _display.UpdateStatus(IsAvailable ? "Spotify Ready" : "Spotify Not Connected");
      
      if (IsAvailable)
      {
        _logger?.LogInformation("Spotify initialized successfully");
      }
      else
      {
        _logger?.LogWarning("Spotify initialization failed - check credentials");
      }
    }
  }

  public override async Task StartAsync()
  {
    if (!IsAvailable)
      throw new InvalidOperationException("Spotify is not available");

    IsActive = true;
    _display.UpdateStatus("Playing");
    
    if (_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation)
    {
      // Simulate Spotify metadata
      _display.UpdateMetadata(new Dictionary<string, string>
      {
        ["Track"] = "Sample Track",
        ["Artist"] = "Sample Artist",
        ["Album"] = "Sample Album",
        ["Duration"] = "3:45"
      });
    }
    else if (_currentTrack != null)
    {
      // Update display with current track metadata
      UpdateDisplayMetadata(_currentTrack);
      
      // Start playback monitoring
      _playbackCts = new CancellationTokenSource();
      _playbackTask = MonitorPlaybackAsync(_playbackCts.Token);
    }
    
    await Task.CompletedTask;
  }

  public override async Task StopAsync()
  {
    IsActive = false;
    
    // Cancel playback monitoring
    _playbackCts?.Cancel();
    if (_playbackTask != null)
    {
      try
      {
        await _playbackTask;
      }
      catch (OperationCanceledException)
      {
        // Expected
      }
    }
    
    _playbackCts?.Dispose();
    _playbackCts = null;
    _playbackTask = null;
    _currentTrack = null;
    
    _display.UpdateStatus("Stopped");
  }

  public override async Task PauseAsync()
  {
    if (!IsActive)
      throw new InvalidOperationException("Spotify is not active");

    IsPaused = true;
    
    if (_spotify != null && !(_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation))
    {
      try
      {
        await _spotify.Player.PausePlayback();
      }
      catch (Exception ex)
      {
        _logger?.LogError(ex, "Failed to pause Spotify playback");
      }
    }
    
    _display.UpdateStatus("Paused");
  }

  public override async Task ResumeAsync()
  {
    if (!IsActive)
      throw new InvalidOperationException("Spotify is not active");

    IsPaused = false;
    
    if (_spotify != null && !(_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation))
    {
      try
      {
        await _spotify.Player.ResumePlayback();
      }
      catch (Exception ex)
      {
        _logger?.LogError(ex, "Failed to resume Spotify playback");
      }
    }
    
    _display.UpdateStatus("Playing");
  }

  public override Task<Stream?> GetAudioStreamAsync()
  {
    if (!IsActive)
      return Task.FromResult<Stream?>(null);

    // In simulation mode, return a mock stream
    if (_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation)
    {
      return Task.FromResult<Stream?>(new MemoryStream());
    }

    // Note: Spotify Web API doesn't provide direct audio streaming
    // Audio playback must be handled by the Spotify player (Connect API)
    // This method is kept for interface compatibility
    return Task.FromResult<Stream?>(null);
  }

  #region Spotify API Features

  /// <summary>
  /// Get user's favorite/saved tracks
  /// </summary>
  public async Task<List<SpotifyTrack>> GetFavoriteSongsAsync(int limit = 50)
  {
    if (_spotify == null && !(_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation))
    {
      throw new InvalidOperationException("Spotify client not initialized");
    }

    if (_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation)
    {
      return GetSimulatedFavorites(limit);
    }

    try
    {
      var savedTracks = await _spotify!.Library.GetTracks(new LibraryTracksRequest
      {
        Limit = Math.Min(limit, _spotifyConfig.MaxItems)
      });

      return savedTracks.Items?.Select(ConvertToSpotifyTrack).ToList() ?? new List<SpotifyTrack>();
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, "Failed to get favorite songs");
      throw;
    }
  }

  /// <summary>
  /// Get user's owned playlists
  /// </summary>
  public async Task<List<SpotifyPlaylist>> GetOwnedPlaylistsAsync(int limit = 50)
  {
    if (_spotify == null && !(_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation))
    {
      throw new InvalidOperationException("Spotify client not initialized");
    }

    if (_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation)
    {
      return GetSimulatedPlaylists(limit);
    }

    try
    {
      var currentUser = await _spotify!.UserProfile.Current();
      var playlists = await _spotify.Playlists.GetUsers(currentUser.Id);
      
      return playlists.Items?.Take(Math.Min(limit, _spotifyConfig.MaxItems))
        .Select(ConvertToSpotifyPlaylist)
        .ToList() ?? new List<SpotifyPlaylist>();
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, "Failed to get owned playlists");
      throw;
    }
  }

  /// <summary>
  /// Get recently played tracks
  /// </summary>
  public async Task<List<SpotifyTrack>> GetRecentlyPlayedAsync(int limit = 50)
  {
    if (_spotify == null && !(_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation))
    {
      throw new InvalidOperationException("Spotify client not initialized");
    }

    if (_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation)
    {
      return GetSimulatedRecentlyPlayed(limit);
    }

    try
    {
      var recentTracks = await _spotify!.Player.GetRecentlyPlayed(new PlayerRecentlyPlayedRequest
      {
        Limit = Math.Min(limit, _spotifyConfig.MaxItems)
      });

      return recentTracks.Items?.Select(item => ConvertToSpotifyTrack(item.Track)).ToList() 
        ?? new List<SpotifyTrack>();
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, "Failed to get recently played tracks");
      throw;
    }
  }

  /// <summary>
  /// Get audiobook recommendations (using show/podcast recommendations as proxy)
  /// </summary>
  public async Task<List<SpotifyTrack>> GetAudiobookRecommendationsAsync(int limit = 20)
  {
    if (_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation)
    {
      return GetSimulatedRecommendations("Audiobooks", limit);
    }

    // Note: Spotify API doesn't have direct audiobook recommendations yet
    // This returns podcast/show recommendations as a proxy
    try
    {
      var searchResults = await SearchAsync("audiobook", SearchRequest.Types.Show, limit);
      return searchResults.Tracks.Take(limit).ToList();
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, "Failed to get audiobook recommendations");
      return new List<SpotifyTrack>();
    }
  }

  /// <summary>
  /// Get recommended tracks based on user's listening history
  /// </summary>
  public async Task<List<SpotifyTrack>> GetGeneralRecommendationsAsync(int limit = 20)
  {
    if (_spotify == null && !(_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation))
    {
      throw new InvalidOperationException("Spotify client not initialized");
    }

    if (_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation)
    {
      return GetSimulatedRecommendations("General", limit);
    }

    try
    {
      // Get user's top tracks to seed recommendations
      var topTracks = await _spotify!.Personalization.GetTopTracks(new PersonalizationTopRequest
      {
        Limit = 5,
        TimeRangeParam = PersonalizationTopRequest.TimeRange.ShortTerm
      });

      if (topTracks.Items?.Any() == true)
      {
        var seedTracks = topTracks.Items.Take(5).Select(t => t.Id).ToList();
        var recommendationsRequest = new RecommendationsRequest
        {
          Limit = Math.Min(limit, _spotifyConfig.MaxItems),
          Market = _spotifyConfig.Market
        };
        
        // Add seed tracks to the request
        foreach (var trackId in seedTracks)
        {
          recommendationsRequest.SeedTracks.Add(trackId);
        }
        
        var recommendations = await _spotify.Browse.GetRecommendations(recommendationsRequest);

        return recommendations.Tracks?.Select(ConvertToSpotifyTrack).ToList() 
          ?? new List<SpotifyTrack>();
      }

      return new List<SpotifyTrack>();
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, "Failed to get general recommendations");
      throw;
    }
  }

  /// <summary>
  /// Search for tracks, artists, or albums
  /// </summary>
  /// <param name="query">Search query (can include song name, artist, lyrics)</param>
  /// <param name="type">Type of search (Track, Artist, Album)</param>
  /// <param name="limit">Maximum number of results</param>
  public async Task<SpotifySearchResults> SearchAsync(
    string query, 
    SearchRequest.Types type = SearchRequest.Types.Track,
    int limit = 20)
  {
    if (string.IsNullOrWhiteSpace(query))
    {
      throw new ArgumentException("Search query cannot be empty", nameof(query));
    }

    if (_spotify == null && !(_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation))
    {
      throw new InvalidOperationException("Spotify client not initialized");
    }

    if (_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation)
    {
      return GetSimulatedSearchResults(query, limit);
    }

    try
    {
      var searchRequest = new SearchRequest(type, query)
      {
        Limit = Math.Min(limit, _spotifyConfig.MaxItems),
        Market = _spotifyConfig.Market
      };

      var searchResponse = await _spotify!.Search.Item(searchRequest);
      var results = new SpotifySearchResults();

      if (searchResponse.Tracks?.Items != null)
      {
        results.Tracks = searchResponse.Tracks.Items.Select(ConvertToSpotifyTrack).ToList();
        results.TotalTracks = searchResponse.Tracks.Total ?? 0;
      }

      if (searchResponse.Playlists?.Items != null)
      {
        results.Playlists = searchResponse.Playlists.Items.Select(ConvertToSpotifyPlaylist).ToList();
        results.TotalPlaylists = searchResponse.Playlists.Total ?? 0;
      }

      return results;
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, "Failed to search Spotify");
      throw;
    }
  }

  /// <summary>
  /// Play a specific track by URI
  /// </summary>
  public async Task PlayTrackAsync(string trackUri)
  {
    if (string.IsNullOrWhiteSpace(trackUri))
    {
      throw new ArgumentException("Track URI cannot be empty", nameof(trackUri));
    }

    if (_spotify == null && !(_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation))
    {
      throw new InvalidOperationException("Spotify client not initialized");
    }

    if (_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation)
    {
      _logger?.LogInformation("Simulating playback of track: {Uri}", trackUri);
      IsActive = true;
      return;
    }

    try
    {
      var request = new PlayerResumePlaybackRequest
      {
        Uris = new List<string> { trackUri }
      };

      await _spotify!.Player.ResumePlayback(request);
      IsActive = true;
      _display.UpdateStatus("Playing");
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, "Failed to play track: {Uri}", trackUri);
      throw;
    }
  }

  /// <summary>
  /// Get currently playing track metadata
  /// </summary>
  public async Task<SpotifyTrack?> GetCurrentlyPlayingAsync()
  {
    if (_spotify == null && !(_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation))
    {
      throw new InvalidOperationException("Spotify client not initialized");
    }

    if (_environmentService.IsSimulationMode || _spotifyConfig.UseSimulation)
    {
      return new SpotifyTrack
      {
        Name = "Sample Track",
        Artist = "Sample Artist",
        Album = "Sample Album",
        DurationMs = 225000
      };
    }

    try
    {
      var currentlyPlaying = await _spotify!.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());
      
      if (currentlyPlaying?.Item is FullTrack track)
      {
        _currentTrack = track;
        return ConvertToSpotifyTrack(track);
      }

      return null;
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, "Failed to get currently playing track");
      return null;
    }
  }

  #endregion

  #region Private Methods

  private async Task LoadConfigurationAsync()
  {
    // Try to load from environment variables first
    var clientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
    var clientSecret = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET");
    var refreshToken = Environment.GetEnvironmentVariable("SPOTIFY_REFRESH_TOKEN");
    var redirectUri = Environment.GetEnvironmentVariable("SPOTIFY_REDIRECT_URI");

    if (!string.IsNullOrWhiteSpace(clientId))
    {
      _spotifyConfig.ClientId = clientId;
    }

    if (!string.IsNullOrWhiteSpace(clientSecret))
    {
      _spotifyConfig.ClientSecret = clientSecret;
    }

    if (!string.IsNullOrWhiteSpace(refreshToken))
    {
      _spotifyConfig.RefreshToken = refreshToken;
    }

    if (!string.IsNullOrWhiteSpace(redirectUri))
    {
      _spotifyConfig.RedirectUri = redirectUri;
    }

    // Try to load from configuration storage
    var storedClientId = _configuration.GetValue<string>("ClientId");
    var storedClientSecret = _configuration.GetValue<string>("ClientSecret");
    var storedRefreshToken = _configuration.GetValue<string>("RefreshToken");
    var storedRedirectUri = _configuration.GetValue<string>("RedirectUri");
    var useSimulation = _configuration.GetValue<bool?>("UseSimulation");

    if (!string.IsNullOrWhiteSpace(storedClientId))
    {
      _spotifyConfig.ClientId = storedClientId;
    }

    if (!string.IsNullOrWhiteSpace(storedClientSecret))
    {
      _spotifyConfig.ClientSecret = storedClientSecret;
    }

    if (!string.IsNullOrWhiteSpace(storedRefreshToken))
    {
      _spotifyConfig.RefreshToken = storedRefreshToken;
    }

    if (!string.IsNullOrWhiteSpace(storedRedirectUri))
    {
      _spotifyConfig.RedirectUri = storedRedirectUri;
    }

    if (useSimulation.HasValue)
    {
      _spotifyConfig.UseSimulation = useSimulation.Value;
    }

    await Task.CompletedTask;
  }

  private async Task<bool> CheckSpotifyAvailabilityAsync()
  {
    if (string.IsNullOrWhiteSpace(_spotifyConfig.ClientId))
    {
      _logger?.LogWarning("Spotify Client ID not configured");
      return false;
    }

    try
    {
      // Check if we have a refresh token for user authentication
      if (!string.IsNullOrWhiteSpace(_spotifyConfig.RefreshToken))
      {
        _logger?.LogInformation("Initializing Spotify with user credentials (PKCE flow)");
        
        // Create PKCETokenResponse with the refresh token
        // Note: We only have the refresh token, so we'll use a placeholder for access token
        // The PKCEAuthenticator will automatically refresh it
        var tokenResponse = new PKCETokenResponse
        {
          AccessToken = "initial", // Will be refreshed automatically
          TokenType = "Bearer",
          ExpiresIn = 0, // Expired, will trigger immediate refresh
          RefreshToken = _spotifyConfig.RefreshToken,
          Scope = string.Empty,
          CreatedAt = DateTime.UtcNow.AddDays(-1) // Force expired
        };

        var authenticator = new PKCEAuthenticator(_spotifyConfig.ClientId, tokenResponse);
        var config = SpotifyClientConfig.CreateDefault().WithAuthenticator(authenticator);
        _spotify = new SpotifyClient(config);

        // Test the connection with user profile (requires user auth)
        try
        {
          var profile = await _spotify.UserProfile.Current();
          _logger?.LogInformation("Successfully authenticated with Spotify user: {UserId}", profile.Id);
          return true;
        }
        catch (Exception ex)
        {
          _logger?.LogError(ex, "Failed to authenticate with user credentials, falling back to client credentials");
          // Fall through to client credentials
        }
      }

      // Fallback to client credentials if refresh token is not available or failed
      if (string.IsNullOrWhiteSpace(_spotifyConfig.ClientSecret))
      {
        _logger?.LogWarning("Spotify Client Secret not configured for client credentials flow");
        return false;
      }

      _logger?.LogInformation("Initializing Spotify with client credentials (limited access)");
      var clientConfig = SpotifyClientConfig
        .CreateDefault()
        .WithAuthenticator(new ClientCredentialsAuthenticator(
          _spotifyConfig.ClientId, 
          _spotifyConfig.ClientSecret));
      
      _spotify = new SpotifyClient(clientConfig);

      // Test the connection
      var search = await _spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, "test"));
      return search != null;
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, "Failed to connect to Spotify API");
      return false;
    }
  }

  private async Task MonitorPlaybackAsync(CancellationToken cancellationToken)
  {
    while (!cancellationToken.IsCancellationRequested && IsActive)
    {
      try
      {
        var currentTrack = await GetCurrentlyPlayingAsync();
        if (currentTrack != null)
        {
          // Fire audio data available event (simulated for Spotify)
          OnAudioDataAvailable(new AudioDataAvailableEventArgs
          {
            AudioData = Array.Empty<byte>(), // Spotify doesn't provide raw audio
            SampleRate = 44100,
            Channels = 2,
            BitsPerSample = 16,
            Timestamp = DateTime.UtcNow
          });
        }

        await Task.Delay(1000, cancellationToken);
      }
      catch (OperationCanceledException)
      {
        break;
      }
      catch (Exception ex)
      {
        _logger?.LogError(ex, "Error monitoring playback");
      }
    }
  }

  private void UpdateDisplayMetadata(FullTrack track)
  {
    var metadata = new Dictionary<string, string>
    {
      ["Track"] = track.Name,
      ["Artist"] = string.Join(", ", track.Artists.Select(a => a.Name)),
      ["Album"] = track.Album.Name,
      ["Duration"] = TimeSpan.FromMilliseconds(track.DurationMs).ToString(@"mm\:ss")
    };

    if (track.Album.Images.Any())
    {
      metadata["AlbumArt"] = track.Album.Images.First().Url;
    }

    _display.UpdateMetadata(metadata);
  }

  private SpotifyTrack ConvertToSpotifyTrack(SavedTrack savedTrack)
  {
    return ConvertToSpotifyTrack(savedTrack.Track);
  }

  private SpotifyTrack ConvertToSpotifyTrack(FullTrack track)
  {
    return new SpotifyTrack
    {
      Id = track.Id,
      Name = track.Name,
      Artist = string.Join(", ", track.Artists.Select(a => a.Name)),
      Album = track.Album.Name,
      AlbumArtUrl = track.Album.Images.FirstOrDefault()?.Url ?? string.Empty,
      DurationMs = track.DurationMs,
      Uri = track.Uri,
      IsPlayable = track.IsPlayable
    };
  }

  private SpotifyPlaylist ConvertToSpotifyPlaylist(FullPlaylist playlist)
  {
    return new SpotifyPlaylist
    {
      Id = playlist.Id ?? string.Empty,
      Name = playlist.Name ?? string.Empty,
      Description = playlist.Description ?? string.Empty,
      Owner = playlist.Owner?.DisplayName ?? string.Empty,
      TrackCount = playlist.Tracks?.Total ?? 0,
      ImageUrl = playlist.Images?.FirstOrDefault()?.Url ?? string.Empty,
      Uri = playlist.Uri ?? string.Empty,
      IsPublic = playlist.Public ?? false
    };
  }

  #endregion

  #region Simulation Methods

  private List<SpotifyTrack> GetSimulatedFavorites(int limit = 50)
  {
    var allFavorites = new List<SpotifyTrack>
    {
      new() { Name = "Favorite Song 1", Artist = "Artist A", Album = "Album 1", DurationMs = 210000 },
      new() { Name = "Favorite Song 2", Artist = "Artist B", Album = "Album 2", DurationMs = 195000 },
      new() { Name = "Favorite Song 3", Artist = "Artist C", Album = "Album 3", DurationMs = 225000 }
    };
    return allFavorites.Take(limit).ToList();
  }

  private List<SpotifyPlaylist> GetSimulatedPlaylists(int limit = 50)
  {
    var allPlaylists = new List<SpotifyPlaylist>
    {
      new() { Name = "My Playlist 1", TrackCount = 25, Description = "Favorite tracks", IsPublic = true },
      new() { Name = "My Playlist 2", TrackCount = 42, Description = "Workout music", IsPublic = false }
    };
    return allPlaylists.Take(limit).ToList();
  }

  private List<SpotifyTrack> GetSimulatedRecentlyPlayed(int limit = 50)
  {
    var allRecent = new List<SpotifyTrack>
    {
      new() { Name = "Recent Song 1", Artist = "Artist X", Album = "Recent Album", DurationMs = 180000 },
      new() { Name = "Recent Song 2", Artist = "Artist Y", Album = "Another Album", DurationMs = 200000 }
    };
    return allRecent.Take(limit).ToList();
  }

  private List<SpotifyTrack> GetSimulatedRecommendations(string category, int limit = 20)
  {
    var allRecommendations = new List<SpotifyTrack>
    {
      new() { Name = $"{category} Recommendation 1", Artist = "Rec Artist 1", DurationMs = 190000 },
      new() { Name = $"{category} Recommendation 2", Artist = "Rec Artist 2", DurationMs = 205000 }
    };
    return allRecommendations.Take(limit).ToList();
  }

  private SpotifySearchResults GetSimulatedSearchResults(string query, int limit = 20)
  {
    var allResults = new List<SpotifyTrack>
    {
      new() { Name = $"Search Result: {query} Track 1", Artist = "Search Artist 1", DurationMs = 195000 },
      new() { Name = $"Search Result: {query} Track 2", Artist = "Search Artist 2", DurationMs = 210000 }
    };
    
    return new SpotifySearchResults
    {
      Tracks = allResults.Take(limit).ToList(),
      TotalTracks = allResults.Count
    };
  }

  #endregion
}
