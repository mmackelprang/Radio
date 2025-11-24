using Microsoft.Extensions.Logging;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Base implementation for audio processors providing common functionality.
/// </summary>
public abstract class BaseAudioProcessor : IAudioProcessor
{
  protected readonly ILogger _logger;

  protected BaseAudioProcessor(ILogger logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <inheritdoc />
  public abstract AudioFormat SupportedFormat { get; }

  /// <inheritdoc />
  public abstract string ContentType { get; }

  /// <inheritdoc />
  public virtual bool CanProcess(AudioFormat format) => format == SupportedFormat;

  /// <inheritdoc />
  public virtual async Task ProcessAsync(
    Stream inputStream,
    Stream outputStream,
    AudioProcessingOptions? options = null,
    CancellationToken cancellationToken = default)
  {
    options ??= new AudioProcessingOptions();

    var buffer = new byte[options.BufferSize];
    int bytesRead;

    _logger.LogDebug("Processing {Format} audio stream with buffer size {BufferSize}",
      SupportedFormat, options.BufferSize);

    while (!cancellationToken.IsCancellationRequested)
    {
      bytesRead = await inputStream.ReadAsync(buffer, cancellationToken);

      if (bytesRead == 0)
      {
        // No more data, wait briefly and continue (for live streams)
        await Task.Delay(10, cancellationToken);
        continue;
      }

      await outputStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
      await outputStream.FlushAsync(cancellationToken);
    }
  }
}

/// <summary>
/// Audio processor for MP3 format.
/// </summary>
public class Mp3AudioProcessor : BaseAudioProcessor
{
  public Mp3AudioProcessor(ILogger<Mp3AudioProcessor> logger) : base(logger) { }

  /// <inheritdoc />
  public override AudioFormat SupportedFormat => AudioFormat.Mp3;

  /// <inheritdoc />
  public override string ContentType => "audio/mpeg";
}

/// <summary>
/// Audio processor for WAV format.
/// </summary>
public class WavAudioProcessor : BaseAudioProcessor
{
  public WavAudioProcessor(ILogger<WavAudioProcessor> logger) : base(logger) { }

  /// <inheritdoc />
  public override AudioFormat SupportedFormat => AudioFormat.Wav;

  /// <inheritdoc />
  public override string ContentType => "audio/wav";
}

/// <summary>
/// Audio processor for FLAC format.
/// </summary>
public class FlacAudioProcessor : BaseAudioProcessor
{
  public FlacAudioProcessor(ILogger<FlacAudioProcessor> logger) : base(logger) { }

  /// <inheritdoc />
  public override AudioFormat SupportedFormat => AudioFormat.Flac;

  /// <inheritdoc />
  public override string ContentType => "audio/flac";
}

/// <summary>
/// Audio processor for AAC format.
/// </summary>
public class AacAudioProcessor : BaseAudioProcessor
{
  public AacAudioProcessor(ILogger<AacAudioProcessor> logger) : base(logger) { }

  /// <inheritdoc />
  public override AudioFormat SupportedFormat => AudioFormat.Aac;

  /// <inheritdoc />
  public override string ContentType => "audio/aac";
}

/// <summary>
/// Audio processor for OGG Vorbis format.
/// </summary>
public class OggAudioProcessor : BaseAudioProcessor
{
  public OggAudioProcessor(ILogger<OggAudioProcessor> logger) : base(logger) { }

  /// <inheritdoc />
  public override AudioFormat SupportedFormat => AudioFormat.Ogg;

  /// <inheritdoc />
  public override string ContentType => "audio/ogg";
}

/// <summary>
/// Audio processor for OPUS format.
/// </summary>
public class OpusAudioProcessor : BaseAudioProcessor
{
  public OpusAudioProcessor(ILogger<OpusAudioProcessor> logger) : base(logger) { }

  /// <inheritdoc />
  public override AudioFormat SupportedFormat => AudioFormat.Opus;

  /// <inheritdoc />
  public override string ContentType => "audio/opus";
}
