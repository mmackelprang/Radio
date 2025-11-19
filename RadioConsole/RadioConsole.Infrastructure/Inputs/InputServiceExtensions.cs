using Microsoft.Extensions.DependencyInjection;
using RadioConsole.Core.Interfaces.Inputs;

namespace RadioConsole.Infrastructure.Inputs;

/// <summary>
/// Extension methods for registering input services in the DI container.
/// </summary>
public static class InputServiceExtensions
{
  /// <summary>
  /// Add all input services (Raddy Radio, Spotify, Broadcast Receiver) to the service collection.
  /// </summary>
  /// <param name="services">The service collection.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection AddInputServices(this IServiceCollection services)
  {
    services.AddSingleton<IRaddyRadioService, RaddyRadioService>();
    services.AddSingleton<ISpotifyService, SpotifyService>();
    services.AddSingleton<IBroadcastReceiverService, BroadcastReceiverService>();

    return services;
  }

  /// <summary>
  /// Add the Raddy RF320 radio service to the service collection.
  /// </summary>
  /// <param name="services">The service collection.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection AddRaddyRadioService(this IServiceCollection services)
  {
    services.AddSingleton<IRaddyRadioService, RaddyRadioService>();
    return services;
  }

  /// <summary>
  /// Add the Spotify service to the service collection.
  /// </summary>
  /// <param name="services">The service collection.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection AddSpotifyService(this IServiceCollection services)
  {
    services.AddSingleton<ISpotifyService, SpotifyService>();
    return services;
  }

  /// <summary>
  /// Add the Google Broadcast Receiver service to the service collection.
  /// </summary>
  /// <param name="services">The service collection.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection AddBroadcastReceiverService(this IServiceCollection services)
  {
    services.AddSingleton<IBroadcastReceiverService, BroadcastReceiverService>();
    return services;
  }
}
