using Microsoft.Extensions.Logging;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Factory for creating audio processors based on detected format.
/// Uses dependency injection to resolve the appropriate processor.
/// </summary>
/// <remarks>
/// <para>
/// The factory maintains a dictionary of all registered processors and can:
/// <list type="bullet">
/// <item><description>Return a specific processor for a given format</description></item>
/// <item><description>List all available processors</description></item>
/// <item><description>List all supported formats</description></item>
/// </list>
/// </para>
/// <para>
/// To extend with new formats:
/// <list type="number">
/// <item><description>Create a new processor implementing IAudioProcessor</description></item>
/// <item><description>Register it in the DI container</description></item>
/// <item><description>The factory will automatically pick it up via constructor injection</description></item>
/// </list>
/// </para>
/// </remarks>
public class AudioProcessorFactory : IAudioProcessorFactory
{
  private readonly ILogger<AudioProcessorFactory> _logger;
  private readonly Dictionary<AudioFormat, IAudioProcessor> _processors;

  /// <summary>
  /// Initializes a new instance of the AudioProcessorFactory class.
  /// </summary>
  /// <param name="processors">Collection of audio processors from DI.</param>
  /// <param name="logger">Logger instance.</param>
  public AudioProcessorFactory(
    IEnumerable<IAudioProcessor> processors,
    ILogger<AudioProcessorFactory> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    _processors = new Dictionary<AudioFormat, IAudioProcessor>();

    foreach (var processor in processors)
    {
      if (!_processors.ContainsKey(processor.SupportedFormat))
      {
        _processors[processor.SupportedFormat] = processor;
        _logger.LogDebug("Registered audio processor for format {Format}", processor.SupportedFormat);
      }
      else
      {
        _logger.LogWarning(
          "Duplicate processor registration for format {Format}, keeping first registered",
          processor.SupportedFormat);
      }
    }

    _logger.LogInformation(
      "AudioProcessorFactory initialized with {Count} processors: {Formats}",
      _processors.Count,
      string.Join(", ", _processors.Keys));
  }

  /// <inheritdoc />
  public IAudioProcessor? GetProcessor(AudioFormat format)
  {
    if (_processors.TryGetValue(format, out var processor))
    {
      _logger.LogDebug("Returning processor for format {Format}", format);
      return processor;
    }

    _logger.LogWarning("No processor registered for format {Format}", format);
    return null;
  }

  /// <inheritdoc />
  public IEnumerable<IAudioProcessor> GetAllProcessors() => _processors.Values;

  /// <inheritdoc />
  public IEnumerable<AudioFormat> GetSupportedFormats() => _processors.Keys;
}
