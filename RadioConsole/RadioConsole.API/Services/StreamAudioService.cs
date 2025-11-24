using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;
using System.IO.Pipelines;

namespace RadioConsole.API.Services;

/// <summary>
/// Service that exposes the current audio mix as a continuous HTTP stream.
/// This enables casting audio to Google Cast devices and other streaming clients.
/// </summary>
public class StreamAudioService
{
  private readonly ILogger<StreamAudioService> _logger;
  private readonly IAudioPlayer _audioPlayer;

  /// <summary>
  /// Supported audio formats based on SoundFlow library capabilities.
  /// </summary>
  public static readonly string[] SupportedFormats = { "wav", "mp3", "flac", "aac", "ogg", "opus" };

  public StreamAudioService(ILogger<StreamAudioService> logger, IAudioPlayer audioPlayer)
  {
    _logger = logger;
    _audioPlayer = audioPlayer;
  }

  /// <summary>
  /// Determines if the specified format is supported.
  /// </summary>
  /// <param name="format">The audio format to check.</param>
  /// <returns>True if the format is supported, false otherwise.</returns>
  public static bool IsFormatSupported(string format)
  {
    return SupportedFormats.Contains(format.ToLower());
  }

  /// <summary>
  /// Gets the content type for the specified audio format.
  /// </summary>
  /// <param name="format">The audio format.</param>
  /// <returns>The MIME content type for the format.</returns>
  public static string GetContentType(string format)
  {
    return format.ToLower() switch
    {
      "wav" => "audio/wav",
      "mp3" => "audio/mpeg",
      "flac" => "audio/flac",
      "aac" => "audio/aac",
      "ogg" => "audio/ogg",
      "opus" => "audio/opus",
      _ => "audio/mpeg"
    };
  }

  /// <summary>
  /// Streams the mixed audio output to the HTTP response.
  /// </summary>
  /// <param name="context">The HTTP context.</param>
  /// <param name="format">The audio format (wav, mp3, flac, aac, ogg, or opus).</param>
  public async Task StreamAudioAsync(HttpContext context, string format = "mp3")
  {
    _logger.LogInformation("Starting audio stream in {Format} format", format);

    try
    {
      // Set appropriate content type
      context.Response.ContentType = GetContentType(format);

      // Disable buffering for streaming
      context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
      context.Response.Headers["Pragma"] = "no-cache";
      context.Response.Headers["Expires"] = "0";
      context.Response.Headers["Connection"] = "keep-alive";

      // Get the mixed audio output stream from the audio player
      var audioStream = _audioPlayer.GetMixedOutputStream();
      
      // Stream the audio data to the response
      var buffer = new byte[4096];
      int bytesRead;

      _logger.LogInformation("Streaming audio to client...");

      while (!context.RequestAborted.IsCancellationRequested)
      {
        bytesRead = await audioStream.ReadAsync(buffer, 0, buffer.Length, context.RequestAborted);
        
        if (bytesRead == 0)
        {
          // No more data available, wait a bit and try again
          await Task.Delay(10, context.RequestAborted);
          continue;
        }

        await context.Response.Body.WriteAsync(buffer, 0, bytesRead, context.RequestAborted);
        await context.Response.Body.FlushAsync(context.RequestAborted);
      }

      _logger.LogInformation("Audio streaming ended (client disconnected)");
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Audio streaming cancelled by client");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during audio streaming");
      throw;
    }
  }

  /// <summary>
  /// Streams audio as WAV format (uncompressed).
  /// </summary>
  public async Task StreamWavAsync(HttpContext context)
  {
    await StreamAudioAsync(context, "wav");
  }

  /// <summary>
  /// Streams audio as MP3 format (compressed).
  /// </summary>
  public async Task StreamMp3Async(HttpContext context)
  {
    await StreamAudioAsync(context, "mp3");
  }

  /// <summary>
  /// Streams audio as FLAC format (lossless compressed).
  /// </summary>
  public async Task StreamFlacAsync(HttpContext context)
  {
    await StreamAudioAsync(context, "flac");
  }

  /// <summary>
  /// Streams audio as AAC format.
  /// </summary>
  public async Task StreamAacAsync(HttpContext context)
  {
    await StreamAudioAsync(context, "aac");
  }

  /// <summary>
  /// Streams audio as OGG format.
  /// </summary>
  public async Task StreamOggAsync(HttpContext context)
  {
    await StreamAudioAsync(context, "ogg");
  }

  /// <summary>
  /// Streams audio as OPUS format.
  /// </summary>
  public async Task StreamOpusAsync(HttpContext context)
  {
    await StreamAudioAsync(context, "opus");
  }
}
