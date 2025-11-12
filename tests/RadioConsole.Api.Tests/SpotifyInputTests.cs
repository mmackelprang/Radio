using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Modules.Inputs;
using RadioConsole.Api.Services;
using SpotifyAPI.Web;

namespace RadioConsole.Api.Tests;

/// <summary>
/// Tests for SpotifyInput covering:
/// - Initialization with and without credentials
/// - Favorite songs retrieval
/// - Owned playlists retrieval
/// - Recently played tracks
/// - Recommendations (general and audiobooks)
/// - Search functionality
/// - Playback control (Start/Stop/Pause/Resume)
/// - Metadata retrieval
/// - Configuration loading
/// </summary>
public class SpotifyInputTests
{
  private readonly Mock<IEnvironmentService> _mockEnvironmentService;
  private readonly Mock<IStorage> _mockStorage;
  private readonly Mock<ILogger<SpotifyInput>> _mockLogger;

  public SpotifyInputTests()
  {
    _mockEnvironmentService = new Mock<IEnvironmentService>();
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(true);

    _mockStorage = new Mock<IStorage>();
    _mockStorage.Setup(s => s.LoadAsync<Dictionary<string, object>>(It.IsAny<string>()))
      .ReturnsAsync((Dictionary<string, object>?)null);

    _mockLogger = new Mock<ILogger<SpotifyInput>>();
  }

  #region Initialization Tests

  [Fact]
  public async Task Initialize_InSimulationMode_InitializesSuccessfully()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    // Act
    await spotifyInput.InitializeAsync();

    // Assert
    spotifyInput.IsAvailable.Should().BeTrue();
    spotifyInput.Name.Should().Be("Spotify");
    spotifyInput.Id.Should().Be("spotify");
    spotifyInput.Description.Should().Contain("Spotify");
  }

  [Fact]
  public async Task Initialize_WithoutCredentials_IsNotAvailable()
  {
    // Arrange
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(false);
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    // Act
    await spotifyInput.InitializeAsync();

    // Assert
    spotifyInput.IsAvailable.Should().BeFalse();
  }

  [Fact]
  public async Task Initialize_LoadsConfigurationFromEnvironment()
  {
    // Arrange
    Environment.SetEnvironmentVariable("SPOTIFY_CLIENT_ID", "test_client_id");
    Environment.SetEnvironmentVariable("SPOTIFY_CLIENT_SECRET", "test_client_secret");
    
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    // Act
    await spotifyInput.InitializeAsync();

    // Assert - should be available in simulation mode even with env vars
    spotifyInput.IsAvailable.Should().BeTrue();

    // Cleanup
    Environment.SetEnvironmentVariable("SPOTIFY_CLIENT_ID", null);
    Environment.SetEnvironmentVariable("SPOTIFY_CLIENT_SECRET", null);
  }

  [Fact]
  public async Task Initialize_LoadsConfigurationFromStorage()
  {
    // Arrange
    var config = new Dictionary<string, object>
    {
      ["ClientId"] = "stored_client_id",
      ["ClientSecret"] = "stored_client_secret"
    };
    
    _mockStorage.Setup(s => s.LoadAsync<Dictionary<string, object>>(It.IsAny<string>()))
      .ReturnsAsync(config);

    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    // Act
    await spotifyInput.InitializeAsync();

    // Assert
    spotifyInput.IsAvailable.Should().BeTrue();
  }

  #endregion

  #region Playback Control Tests

  [Fact]
  public async Task Start_WhenAvailable_StartsPlayback()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act
    await spotifyInput.StartAsync();

    // Assert
    spotifyInput.IsActive.Should().BeTrue();
    spotifyInput.IsPaused.Should().BeFalse();
  }

  [Fact]
  public async Task Start_WhenNotAvailable_ThrowsException()
  {
    // Arrange
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(false);
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await spotifyInput.StartAsync());
  }

  [Fact]
  public async Task Stop_WhenPlaying_StopsPlayback()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();
    await spotifyInput.StartAsync();

    // Act
    await spotifyInput.StopAsync();

    // Assert
    spotifyInput.IsActive.Should().BeFalse();
  }

  [Fact]
  public async Task Pause_WhenPlaying_PausesPlayback()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();
    await spotifyInput.StartAsync();

    // Act
    await spotifyInput.PauseAsync();

    // Assert
    spotifyInput.IsPaused.Should().BeTrue();
  }

  [Fact]
  public async Task Pause_WhenNotActive_ThrowsException()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await spotifyInput.PauseAsync());
  }

  [Fact]
  public async Task Resume_WhenPaused_ResumesPlayback()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();
    await spotifyInput.StartAsync();
    await spotifyInput.PauseAsync();

    // Act
    await spotifyInput.ResumeAsync();

    // Assert
    spotifyInput.IsPaused.Should().BeFalse();
  }

  [Fact]
  public async Task Resume_WhenNotActive_ThrowsException()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await spotifyInput.ResumeAsync());
  }

  [Fact]
  public async Task SetVolume_WithValidValue_SetsVolume()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act
    await spotifyInput.SetVolumeAsync(0.5);

    // Assert - no exception should be thrown
    spotifyInput.IsAvailable.Should().BeTrue();
  }

  [Fact]
  public async Task SetVolume_WithInvalidValue_ThrowsException()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
      async () => await spotifyInput.SetVolumeAsync(1.5));
  }

  #endregion

  #region Favorite Songs Tests

  [Fact]
  public async Task GetFavoriteSongs_InSimulationMode_ReturnsSimulatedTracks()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act
    var favorites = await spotifyInput.GetFavoriteSongsAsync();

    // Assert
    favorites.Should().NotBeNull();
    favorites.Should().NotBeEmpty();
    favorites.Should().AllSatisfy(track =>
    {
      track.Name.Should().NotBeEmpty();
      track.Artist.Should().NotBeEmpty();
      track.Album.Should().NotBeEmpty();
      track.DurationMs.Should().BeGreaterThan(0);
    });
  }

  [Fact]
  public async Task GetFavoriteSongs_WithLimit_ReturnsLimitedResults()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act
    var favorites = await spotifyInput.GetFavoriteSongsAsync(limit: 2);

    // Assert
    favorites.Should().HaveCountLessThanOrEqualTo(2);
  }

  [Fact]
  public async Task GetFavoriteSongs_WhenNotInitialized_ThrowsException()
  {
    // Arrange
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(false);
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await spotifyInput.GetFavoriteSongsAsync());
  }

  #endregion

  #region Owned Playlists Tests

  [Fact]
  public async Task GetOwnedPlaylists_InSimulationMode_ReturnsSimulatedPlaylists()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act
    var playlists = await spotifyInput.GetOwnedPlaylistsAsync();

    // Assert
    playlists.Should().NotBeNull();
    playlists.Should().NotBeEmpty();
    playlists.Should().AllSatisfy(playlist =>
    {
      playlist.Name.Should().NotBeEmpty();
      playlist.TrackCount.Should().BeGreaterThan(0);
    });
  }

  [Fact]
  public async Task GetOwnedPlaylists_WithLimit_ReturnsLimitedResults()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act
    var playlists = await spotifyInput.GetOwnedPlaylistsAsync(limit: 1);

    // Assert
    playlists.Should().HaveCountLessThanOrEqualTo(1);
  }

  #endregion

  #region Recently Played Tests

  [Fact]
  public async Task GetRecentlyPlayed_InSimulationMode_ReturnsSimulatedTracks()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act
    var recentTracks = await spotifyInput.GetRecentlyPlayedAsync();

    // Assert
    recentTracks.Should().NotBeNull();
    recentTracks.Should().NotBeEmpty();
    recentTracks.Should().AllSatisfy(track =>
    {
      track.Name.Should().NotBeEmpty();
      track.Artist.Should().NotBeEmpty();
    });
  }

  #endregion

  #region Recommendations Tests

  [Fact]
  public async Task GetGeneralRecommendations_InSimulationMode_ReturnsSimulatedRecommendations()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act
    var recommendations = await spotifyInput.GetGeneralRecommendationsAsync();

    // Assert
    recommendations.Should().NotBeNull();
    recommendations.Should().NotBeEmpty();
    recommendations.Should().AllSatisfy(track =>
    {
      track.Name.Should().Contain("General");
    });
  }

  [Fact]
  public async Task GetAudiobookRecommendations_InSimulationMode_ReturnsSimulatedRecommendations()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act
    var recommendations = await spotifyInput.GetAudiobookRecommendationsAsync();

    // Assert
    recommendations.Should().NotBeNull();
    recommendations.Should().NotBeEmpty();
    recommendations.Should().AllSatisfy(track =>
    {
      track.Name.Should().Contain("Audiobooks");
    });
  }

  #endregion

  #region Search Tests

  [Fact]
  public async Task Search_InSimulationMode_ReturnsSimulatedResults()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act
    var results = await spotifyInput.SearchAsync("test query");

    // Assert
    results.Should().NotBeNull();
    results.Tracks.Should().NotBeEmpty();
    results.Tracks.Should().AllSatisfy(track =>
    {
      track.Name.Should().Contain("test query");
    });
  }

  [Fact]
  public async Task Search_WithEmptyQuery_ThrowsException()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(
      async () => await spotifyInput.SearchAsync(""));
  }

  [Fact]
  public async Task Search_WithLimit_ReturnsLimitedResults()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act
    var results = await spotifyInput.SearchAsync("test", limit: 1);

    // Assert
    results.Tracks.Should().HaveCountLessThanOrEqualTo(1);
  }

  #endregion

  #region Track Playback Tests

  [Fact]
  public async Task PlayTrack_InSimulationMode_StartsPlayback()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act
    await spotifyInput.PlayTrackAsync("spotify:track:test123");

    // Assert
    spotifyInput.IsActive.Should().BeTrue();
  }

  [Fact]
  public async Task PlayTrack_WithEmptyUri_ThrowsException()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(
      async () => await spotifyInput.PlayTrackAsync(""));
  }

  #endregion

  #region Metadata Tests

  [Fact]
  public async Task GetCurrentlyPlaying_InSimulationMode_ReturnsSimulatedTrack()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act
    var currentTrack = await spotifyInput.GetCurrentlyPlayingAsync();

    // Assert
    currentTrack.Should().NotBeNull();
    currentTrack!.Name.Should().Be("Sample Track");
    currentTrack.Artist.Should().Be("Sample Artist");
    currentTrack.Album.Should().Be("Sample Album");
  }

  [Fact]
  public async Task GetCurrentlyPlaying_WhenNotInitialized_ThrowsException()
  {
    // Arrange
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(false);
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await spotifyInput.GetCurrentlyPlayingAsync());
  }

  #endregion

  #region Audio Stream Tests

  [Fact]
  public async Task GetAudioStream_InSimulationMode_ReturnsStream()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();
    await spotifyInput.StartAsync();

    // Act
    var stream = await spotifyInput.GetAudioStreamAsync();

    // Assert
    stream.Should().NotBeNull();
  }

  [Fact]
  public async Task GetAudioStream_WhenNotActive_ReturnsNull()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();

    // Act
    var stream = await spotifyInput.GetAudioStreamAsync();

    // Assert
    stream.Should().BeNull();
  }

  #endregion

  #region Display and Configuration Tests

  [Fact]
  public async Task Display_AfterStart_ShowsMetadata()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    await spotifyInput.InitializeAsync();
    await spotifyInput.StartAsync();

    // Act
    var display = spotifyInput.GetDisplay();
    var metadata = display.GetMetadata();

    // Assert
    metadata.Should().ContainKey("Track");
    metadata.Should().ContainKey("Artist");
    metadata.Should().ContainKey("Album");
  }

  [Fact]
  public void Configuration_CanBeAccessed()
  {
    // Arrange
    var spotifyInput = new SpotifyInput(
      _mockEnvironmentService.Object,
      _mockStorage.Object,
      _mockLogger.Object);

    // Act
    var config = spotifyInput.GetConfiguration();

    // Assert
    config.Should().NotBeNull();
  }

  #endregion
}
