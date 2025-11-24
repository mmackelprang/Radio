using Microsoft.AspNetCore.Mvc;
using RadioConsole.API.Services;

namespace RadioConsole.API.Controllers;

/// <summary>
/// Controller for audio streaming endpoints.
/// Provides HTTP streaming of audio in various formats.
/// Supported formats: WAV, MP3, FLAC, AAC, OGG, OPUS.
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

  /// <summary>
  /// Stream audio as FLAC format.
  /// </summary>
  /// <returns>Audio stream in FLAC format</returns>
  [HttpGet("stream.flac")]
  [Produces("audio/flac")]
  public async Task StreamFlac()
  {
    _logger.LogInformation("FLAC stream requested");
    await _streamService.StreamFlacAsync(HttpContext);
  }

  /// <summary>
  /// Stream audio as AAC format.
  /// </summary>
  /// <returns>Audio stream in AAC format</returns>
  [HttpGet("stream.aac")]
  [Produces("audio/aac")]
  public async Task StreamAac()
  {
    _logger.LogInformation("AAC stream requested");
    await _streamService.StreamAacAsync(HttpContext);
  }

  /// <summary>
  /// Stream audio as OGG format.
  /// </summary>
  /// <returns>Audio stream in OGG format</returns>
  [HttpGet("stream.ogg")]
  [Produces("audio/ogg")]
  public async Task StreamOgg()
  {
    _logger.LogInformation("OGG stream requested");
    await _streamService.StreamOggAsync(HttpContext);
  }

  /// <summary>
  /// Stream audio as OPUS format.
  /// </summary>
  /// <returns>Audio stream in OPUS format</returns>
  [HttpGet("stream.opus")]
  [Produces("audio/opus")]
  public async Task StreamOpus()
  {
    _logger.LogInformation("OPUS stream requested");
    await _streamService.StreamOpusAsync(HttpContext);
  }
}
