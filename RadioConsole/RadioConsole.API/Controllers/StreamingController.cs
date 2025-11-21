using Microsoft.AspNetCore.Mvc;
using RadioConsole.API.Services;

namespace RadioConsole.API.Controllers;

/// <summary>
/// Controller for audio streaming endpoints.
/// Provides HTTP streaming of audio in various formats.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StreamingController : ControllerBase
{
  private readonly StreamAudioService _streamService;
  private readonly ILogger<StreamingController> _logger;

  public StreamingController(
    StreamAudioService streamService,
    ILogger<StreamingController> logger)
  {
    _streamService = streamService;
    _logger = logger;
  }

  /// <summary>
  /// Stream audio as MP3 format.
  /// </summary>
  /// <returns>Audio stream in MP3 format</returns>
  [HttpGet("stream.mp3")]
  [Produces("audio/mpeg")]
  public async Task StreamMp3()
  {
    _logger.LogInformation("MP3 stream requested");
    await _streamService.StreamMp3Async(HttpContext);
  }

  /// <summary>
  /// Stream audio as WAV format.
  /// </summary>
  /// <returns>Audio stream in WAV format</returns>
  [HttpGet("stream.wav")]
  [Produces("audio/wav")]
  public async Task StreamWav()
  {
    _logger.LogInformation("WAV stream requested");
    await _streamService.StreamWavAsync(HttpContext);
  }
}
