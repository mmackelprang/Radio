using Microsoft.AspNetCore.Mvc;
using RadioConsole.Core.Interfaces.Inputs;
using RadioConsole.Core.Interfaces.Audio;

namespace RadioConsole.API.Controllers;

/// <summary>
/// API controller for retrieving Now Playing information from various audio sources.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NowPlayingController : ControllerBase
{
  private readonly IRaddyRadioService _radioService;
  private readonly ISpotifyService _spotifyService;
  private readonly IAudioPlayer _audioPlayer;
  private readonly ILogger<NowPlayingController> _logger;

  public NowPlayingController(
    IRaddyRadioService radioService,
    ISpotifyService spotifyService,
    IAudioPlayer audioPlayer,
    ILogger<NowPlayingController> logger)
  {
    _radioService = radioService ?? throw new ArgumentNullException(nameof(radioService));
    _spotifyService = spotifyService ?? throw new ArgumentNullException(nameof(spotifyService));
    _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <summary>
  /// Get now playing information for all sources.
  /// </summary>
  /// <returns>Now playing information from all available sources.</returns>
  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<NowPlayingInfo>> Get()
  {
    try
    {
      var info = new NowPlayingInfo
      {
        Radio = await GetRadioInfoAsync(),
        Spotify = await GetSpotifyInfoAsync(),
        IsPlayerInitialized = _audioPlayer.IsInitialized
      };

      return Ok(info);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving now playing information");
      return StatusCode(500, new { error = "Failed to retrieve now playing information", details = ex.Message });
    }
  }

  /// <summary>
  /// Get now playing information for the radio.
  /// </summary>
  /// <returns>Radio now playing information.</returns>
  [HttpGet("radio")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<RadioNowPlaying>> GetRadio()
  {
    try
    {
      var info = await GetRadioInfoAsync();
      return Ok(info);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving radio now playing information");
      return StatusCode(500, new { error = "Failed to retrieve radio information", details = ex.Message });
    }
  }

  /// <summary>
  /// Get now playing information for Spotify.
  /// </summary>
  /// <returns>Spotify now playing information.</returns>
  [HttpGet("spotify")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<SpotifyNowPlaying>> GetSpotify()
  {
    try
    {
      var info = await GetSpotifyInfoAsync();
      return Ok(info);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving Spotify now playing information");
      return StatusCode(500, new { error = "Failed to retrieve Spotify information", details = ex.Message });
    }
  }

  private async Task<RadioNowPlaying> GetRadioInfoAsync()
  {
    var frequency = await _radioService.GetFrequencyAsync();
    return new RadioNowPlaying
    {
      IsStreaming = _radioService.IsStreaming,
      IsDeviceDetected = _radioService.IsDeviceDetected,
      Frequency = frequency,
      Band = DetermineBand(frequency),
      DeviceId = _radioService.GetDeviceId()
    };
  }

  private async Task<SpotifyNowPlaying> GetSpotifyInfoAsync()
  {
    var track = await _spotifyService.GetCurrentlyPlayingAsync();
    return new SpotifyNowPlaying
    {
      TrackName = track?.Name,
      Artist = track?.Artist,
      Album = track?.Album,
      AlbumArtUrl = track?.AlbumArtUrl,
      IsPlaying = track != null
    };
  }

  private static string DetermineBand(double? frequency)
  {
    if (!frequency.HasValue) return "Unknown";

    // Determine band based on frequency ranges
    if (frequency >= 87.5 && frequency <= 108.0)
      return "FM";
    else if (frequency >= 0.53 && frequency <= 1.71)
      return "AM";
    else if (frequency >= 1.71 && frequency <= 30.0)
      return "SW";
    else if (frequency >= 108.0 && frequency <= 137.0)
      return "AIR";
    else if (frequency >= 30.0 && frequency <= 300.0)
      return "VHF";
    else
      return "Unknown";
  }
}

/// <summary>
/// Combined now playing information from all sources.
/// </summary>
public record NowPlayingInfo
{
  /// <summary>
  /// Radio now playing information.
  /// </summary>
  public RadioNowPlaying Radio { get; init; } = new();

  /// <summary>
  /// Spotify now playing information.
  /// </summary>
  public SpotifyNowPlaying Spotify { get; init; } = new();

  /// <summary>
  /// Whether the audio player is initialized.
  /// </summary>
  public bool IsPlayerInitialized { get; init; }
}

/// <summary>
/// Radio now playing information.
/// </summary>
public record RadioNowPlaying
{
  /// <summary>
  /// Whether the radio is currently streaming.
  /// </summary>
  public bool IsStreaming { get; init; }

  /// <summary>
  /// Whether the radio device is detected.
  /// </summary>
  public bool IsDeviceDetected { get; init; }

  /// <summary>
  /// Current frequency in MHz.
  /// </summary>
  public double? Frequency { get; init; }

  /// <summary>
  /// Current band (FM, AM, SW, AIR, VHF).
  /// </summary>
  public string Band { get; init; } = "Unknown";

  /// <summary>
  /// USB Audio device ID.
  /// </summary>
  public string? DeviceId { get; init; }
}

/// <summary>
/// Spotify now playing information.
/// </summary>
public record SpotifyNowPlaying
{
  /// <summary>
  /// Track name.
  /// </summary>
  public string? TrackName { get; init; }

  /// <summary>
  /// Artist name.
  /// </summary>
  public string? Artist { get; init; }

  /// <summary>
  /// Album name.
  /// </summary>
  public string? Album { get; init; }

  /// <summary>
  /// Album art URL.
  /// </summary>
  public string? AlbumArtUrl { get; init; }

  /// <summary>
  /// Whether a track is currently playing.
  /// </summary>
  public bool IsPlaying { get; init; }
}
