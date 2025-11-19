using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Factory for creating Text-to-Speech service instances based on provider type.
/// </summary>
public class TextToSpeechFactory
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<TextToSpeechFactory> _logger;

  public TextToSpeechFactory(IServiceProvider serviceProvider, ILogger<TextToSpeechFactory> logger)
  {
    _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <summary>
  /// Create a TTS service instance for the specified provider.
  /// </summary>
  /// <param name="provider">The TTS provider type.</param>
  /// <returns>An ITextToSpeechService instance for the specified provider.</returns>
  public ITextToSpeechService Create(TtsProvider provider)
  {
    _logger.LogInformation("Creating TTS service for provider: {Provider}", provider);

    return provider switch
    {
      TtsProvider.ESpeak => CreateESpeakService(),
      TtsProvider.GoogleCloud => CreateGoogleCloudService(),
      TtsProvider.AzureCloud => CreateAzureCloudService(),
      _ => throw new ArgumentException($"Unsupported TTS provider: {provider}", nameof(provider))
    };
  }

  private ITextToSpeechService CreateESpeakService()
  {
    var audioPlayer = _serviceProvider.GetRequiredService<IAudioPlayer>();
    var priorityService = _serviceProvider.GetRequiredService<IAudioPriorityService>();
    var logger = _serviceProvider.GetRequiredService<ILogger<ESpeakTextToSpeechService>>();
    return new ESpeakTextToSpeechService(audioPlayer, priorityService, logger);
  }

  private ITextToSpeechService CreateGoogleCloudService()
  {
    var audioPlayer = _serviceProvider.GetRequiredService<IAudioPlayer>();
    var priorityService = _serviceProvider.GetRequiredService<IAudioPriorityService>();
    var logger = _serviceProvider.GetRequiredService<ILogger<GoogleCloudTextToSpeechService>>();
    return new GoogleCloudTextToSpeechService(audioPlayer, priorityService, logger);
  }

  private ITextToSpeechService CreateAzureCloudService()
  {
    var audioPlayer = _serviceProvider.GetRequiredService<IAudioPlayer>();
    var priorityService = _serviceProvider.GetRequiredService<IAudioPriorityService>();
    var logger = _serviceProvider.GetRequiredService<ILogger<AzureCloudTextToSpeechService>>();
    return new AzureCloudTextToSpeechService(audioPlayer, priorityService, logger);
  }
}
