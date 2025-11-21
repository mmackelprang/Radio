using RadioConsole.Core.Configuration;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Extension methods for registering audio services in the DI container.
/// </summary>
public static class AudioServiceExtensions
{
  /// <summary>
  /// Adds audio services (SoundFlow implementations) to the service collection.
  /// </summary>
  /// <param name="services">The service collection.</param>
  /// <param name="configuration">The configuration instance.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection AddAudioServices(
    this IServiceCollection services,
    IConfiguration? configuration = null)
  {
    // Register audio visualization options if configuration is provided
    if (configuration != null)
    {
      services.Configure<AudioVisualizationOptions>(
        configuration.GetSection("AudioVisualization"));
    }

    // Register audio player as singleton to maintain state across requests
    services.AddSingleton<IAudioPlayer, SoundFlowAudioPlayer>();

    // Register audio device manager as singleton
    services.AddSingleton<IAudioDeviceManager, SoundFlowAudioDeviceManager>();

    // Register audio priority service as singleton
    services.AddSingleton<IAudioPriorityService, AudioPriorityService>();

    // Register TTS factory as singleton
    services.AddSingleton<TextToSpeechFactory>();

    // Register system test service as singleton
    services.AddSingleton<ISystemTestService, SystemTestService>();

    // Register metadata service as singleton
    services.AddSingleton<IMetadataService, TagLibMetadataService>();

    // Register audio outputs as transient (new instance per request)
    services.AddTransient<LocalAudioOutput>();
    services.AddTransient(sp => 
    {
      var logger = sp.GetRequiredService<ILogger<CastAudioOutput>>();
      // Get the stream URL from configuration or use default
      var streamUrl = "http://localhost:5000/stream.mp3"; // TODO: Make configurable
      return new CastAudioOutput(logger, streamUrl);
    });

    return services;
  }
}
