using RadioConsole.Core.Configuration;
using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Examples;

/// <summary>
/// Example demonstrating the comprehensive ducking system.
/// Shows how to configure, trigger, and monitor audio ducking.
/// </summary>
public static class DuckingSystemExample
{
  /// <summary>
  /// Demonstrates basic ducking setup and usage.
  /// </summary>
  public static async Task BasicDuckingExampleAsync()
  {
    Console.WriteLine("=== Basic Ducking Example ===\n");

    // In a real application, these would be injected via DI
    var services = new ServiceCollection();
    ConfigureServices(services);
    using var provider = services.BuildServiceProvider();

    var mixerService = provider.GetRequiredService<IMixerService>();
    var duckingService = provider.GetRequiredService<IDuckingService>();

    // Note: In a real application, you would initialize the mixer with an audio device.
    // This example uses a mock mixer service, so initialization is skipped.
    // Production code: await mixerService.InitializeAsync("default");

    // Create a default ducking configuration
    var config = DuckingConfiguration.CreateDefault();
    Console.WriteLine($"Default preset: {config.ActivePreset}");
    Console.WriteLine($"Voice-Main attack time: {config.ChannelPairSettings["Voice-Main"].Timing.AttackTimeMs}ms");
    Console.WriteLine($"Voice-Main duck level: {config.ChannelPairSettings["Voice-Main"].Timing.DuckLevel * 100}%");

    // Initialize ducking service
    await duckingService.InitializeAsync(config);

    // Subscribe to ducking events
    duckingService.DuckingStateChanged += (sender, args) =>
    {
      Console.WriteLine($"Ducking {(args.DuckingStarted ? "started" : "ended")} on {args.AffectedChannel}");
      Console.WriteLine($"  Trigger: {args.TriggerChannel}, Source: {args.SourceId}");
      Console.WriteLine($"  Duck level: {args.DuckLevel * 100}%");
    };

    Console.WriteLine("\nSimulating TTS announcement...");

    // Trigger ducking (as if TTS started)
    await duckingService.StartDuckingAsync(MixerChannel.Voice, "tts-announcement-1");

    // Check status
    var status = duckingService.GetChannelDuckingStatus(MixerChannel.Main);
    Console.WriteLine($"Main channel ducked: {status.IsDucked}");
    Console.WriteLine($"Current level: {status.CurrentLevel * 100}%");

    // Simulate TTS duration
    await Task.Delay(100);

    // End ducking
    await duckingService.EndDuckingAsync("tts-announcement-1");

    Console.WriteLine("\nDucking complete.");
  }

  /// <summary>
  /// Demonstrates using different ducking presets.
  /// </summary>
  public static void PresetsExample()
  {
    Console.WriteLine("\n=== Ducking Presets Example ===\n");

    var presets = new[]
    {
      DuckingPreset.DJMode,
      DuckingPreset.BackgroundMode,
      DuckingPreset.EmergencyMode,
      DuckingPreset.MusicMode
    };

    foreach (var preset in presets)
    {
      var config = DuckingPresets.CreatePreset(preset);
      var voiceMain = config.ChannelPairSettings["Voice-Main"];

      Console.WriteLine($"{preset}:");
      Console.WriteLine($"  Attack: {voiceMain.Timing.AttackTimeMs}ms");
      Console.WriteLine($"  Release: {voiceMain.Timing.ReleaseTimeMs}ms");
      Console.WriteLine($"  Duck Level: {voiceMain.Timing.DuckLevel * 100}%");
      Console.WriteLine();
    }
  }

  /// <summary>
  /// Demonstrates emergency ducking for alerts.
  /// </summary>
  public static async Task EmergencyDuckingExampleAsync()
  {
    Console.WriteLine("=== Emergency Ducking Example ===\n");

    var services = new ServiceCollection();
    ConfigureServices(services);
    using var provider = services.BuildServiceProvider();

    var duckingService = provider.GetRequiredService<IDuckingService>();

    // Use emergency mode preset
    var config = DuckingPresets.CreateEmergencyMode();
    await duckingService.InitializeAsync(config);

    Console.WriteLine("Triggering emergency alert...");

    // Emergency duck - instant mute
    await duckingService.ApplyEmergencyDuckAsync(MixerChannel.Event, "fire-alarm");

    // Check metrics
    var metrics = duckingService.GetMetrics();
    Console.WriteLine($"Emergency ducks triggered: {metrics.EmergencyDuckCount}");

    // Clean up
    await duckingService.EndDuckingAsync("fire-alarm");
  }

  /// <summary>
  /// Demonstrates crossfade between audio sources.
  /// </summary>
  public static async Task CrossfadeExampleAsync()
  {
    Console.WriteLine("\n=== Crossfade Example ===\n");

    var services = new ServiceCollection();
    ConfigureServices(services);
    using var provider = services.BuildServiceProvider();

    var crossfadeController = provider.GetRequiredService<ICrossfadeController>();

    // Initialize with configuration
    var config = new CrossfadeConfiguration
    {
      Enabled = true,
      DefaultCrossfadeDurationMs = 3000,
      EnableGapless = true
    };
    await crossfadeController.InitializeAsync(config);

    // Subscribe to progress events
    crossfadeController.TransitionProgress += (sender, args) =>
    {
      Console.Write($"\rProgress: {args.Progress * 100:F0}% | Out: {args.OutgoingVolume:F2} | In: {args.IncomingVolume:F2}");
    };

    crossfadeController.TransitionCompleted += (sender, args) =>
    {
      Console.WriteLine($"\nCrossfade completed: {args.TransitionType}");
    };

    Console.WriteLine("Starting crossfade (simulated, 500ms)...");

    // Perform a quick crossfade for demo
    await crossfadeController.CrossfadeAsync("track-1", "track-2", 500);

    Console.WriteLine("Crossfade demo complete.");
  }

  /// <summary>
  /// Demonstrates event-based ducking with automatic triggers.
  /// </summary>
  public static async Task EventBasedDuckingExampleAsync()
  {
    Console.WriteLine("\n=== Event-Based Ducking Example ===\n");

    var services = new ServiceCollection();
    ConfigureServices(services);
    using var provider = services.BuildServiceProvider();

    var duckingService = provider.GetRequiredService<IDuckingService>();
    var mixerService = provider.GetRequiredService<IMixerService>();
    var logger = provider.GetRequiredService<ILogger<EventBasedDucking>>();

    var config = DuckingConfiguration.CreateDefault();
    await duckingService.InitializeAsync(config);

    var eventDucking = new EventBasedDucking(duckingService, mixerService, logger);

    // Subscribe to events
    eventDucking.VoiceActivityDetected += (sender, args) =>
    {
      Console.WriteLine($"Voice activity: {(args.IsVoiceActive ? "detected" : "ended")}");
      Console.WriteLine($"  Level: {args.Level:F2}");
    };

    eventDucking.ManualDuckTriggered += (sender, args) =>
    {
      Console.WriteLine($"Manual duck {(args.IsStart ? "started" : "ended")} for {args.SourceId}");
    };

    // Initialize with automatic ducking disabled (for demo)
    await eventDucking.InitializeAsync(enableAutomatic: false);

    Console.WriteLine("Triggering manual duck...");

    // Manual duck trigger
    await eventDucking.TriggerDuckAsync(MixerChannel.Voice, "user-initiated-tts");
    await Task.Delay(100);
    await eventDucking.ReleaseDuckAsync("user-initiated-tts");

    Console.WriteLine("Event-based ducking demo complete.");

    eventDucking.Dispose();
  }

  /// <summary>
  /// Demonstrates monitoring ducking metrics.
  /// </summary>
  public static async Task MetricsMonitoringExampleAsync()
  {
    Console.WriteLine("\n=== Metrics Monitoring Example ===\n");

    var services = new ServiceCollection();
    ConfigureServices(services);
    using var provider = services.BuildServiceProvider();

    var duckingService = provider.GetRequiredService<IDuckingService>();

    var config = DuckingConfiguration.CreateDefault();
    await duckingService.InitializeAsync(config);

    // Generate some ducking events
    for (int i = 0; i < 5; i++)
    {
      await duckingService.StartDuckingAsync(MixerChannel.Voice, $"tts-{i}");
      await Task.Delay(10);
      await duckingService.EndDuckingAsync($"tts-{i}");
    }

    // Apply an emergency duck
    await duckingService.ApplyEmergencyDuckAsync(MixerChannel.Event, "alert-1");
    await duckingService.EndDuckingAsync("alert-1");

    // Get metrics
    var metrics = duckingService.GetMetrics();

    Console.WriteLine("Ducking Metrics:");
    Console.WriteLine($"  Total ducking events: {metrics.TotalDuckingEvents}");
    Console.WriteLine($"  Average attack time: {metrics.AverageAttackTimeMs:F2}ms");
    Console.WriteLine($"  Max attack time: {metrics.MaxAttackTimeMs:F2}ms");
    Console.WriteLine($"  Emergency ducks: {metrics.EmergencyDuckCount}");
    Console.WriteLine($"  Cascading ducks: {metrics.CascadingDuckCount}");
  }

  /// <summary>
  /// Demonstrates custom ducking configuration.
  /// </summary>
  public static void CustomConfigurationExample()
  {
    Console.WriteLine("\n=== Custom Configuration Example ===\n");

    // Create a custom configuration
    var config = new DuckingConfiguration
    {
      Enabled = true,
      ActivePreset = DuckingPreset.Custom,
      EnableLookAhead = true,
      LookAheadMs = 100,
      DefaultSettings = new DuckingTimingSettings
      {
        AttackTimeMs = 75,
        ReleaseTimeMs = 750,
        HoldTimeMs = 150,
        DuckLevel = 0.25f
      }
    };

    // Add custom channel pair settings
    config.ChannelPairSettings["Voice-Main"] = new ChannelPairDuckingSettings
    {
      TriggerChannel = MixerChannel.Voice,
      TargetChannel = MixerChannel.Main,
      Enabled = true,
      Priority = 10,
      Timing = new DuckingTimingSettings
      {
        AttackTimeMs = 40,
        ReleaseTimeMs = 600,
        HoldTimeMs = 100,
        DuckLevel = 0.15f // Duck to 15%
      }
    };

    // Serialize to JSON for persistence
    var json = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions
    {
      WriteIndented = true
    });

    Console.WriteLine("Custom Configuration JSON:");
    Console.WriteLine(json);
  }

  /// <summary>
  /// Complete integration example showing all components working together.
  /// </summary>
  public static async Task IntegrationExampleAsync()
  {
    Console.WriteLine("\n=== Integration Example ===\n");
    Console.WriteLine("This example shows how the ducking system integrates with MixerService.\n");

    var services = new ServiceCollection();
    ConfigureServices(services);
    using var provider = services.BuildServiceProvider();

    var mixerService = provider.GetRequiredService<IMixerService>();
    var duckingService = provider.GetRequiredService<IDuckingService>();
    var crossfadeController = provider.GetRequiredService<ICrossfadeController>();

    // Subscribe to mixer events (ducking integration)
    mixerService.DuckingStateChanged += (sender, args) =>
    {
      Console.WriteLine($"[Mixer] Ducking {(args.IsDucking ? "active" : "inactive")} - Level: {args.DuckLevel * 100}%");
    };

    // Initialize ducking with DJ mode for aggressive ducking
    var duckingConfig = DuckingPresets.CreateDJMode();
    await duckingService.InitializeAsync(duckingConfig);

    // Initialize crossfade
    await crossfadeController.InitializeAsync(new CrossfadeConfiguration());

    Console.WriteLine("System initialized with DJ Mode ducking.");
    Console.WriteLine("In a real application:");
    Console.WriteLine("  1. Audio sources would be added to the mixer");
    Console.WriteLine("  2. When TTS starts, ducking automatically reduces music volume");
    Console.WriteLine("  3. Crossfades smooth transitions between tracks");
    Console.WriteLine("  4. Emergency alerts instantly mute all other audio");
    Console.WriteLine("\nSee individual examples above for specific use cases.");
  }

  private static void ConfigureServices(IServiceCollection services)
  {
    // Add logging
    services.AddLogging(builder =>
    {
      builder.AddConsole();
      builder.SetMinimumLevel(LogLevel.Warning);
    });

    // Register mock mixer service for demo
    services.AddSingleton<IMixerService, MockMixerService>();

    // Register ducking services
    services.AddSingleton<IDuckingService, DuckingManager>();
    services.AddSingleton<ICrossfadeController, CrossfadeController>();
  }

  /// <summary>
  /// Main entry point for running examples.
  /// </summary>
  public static async Task RunAllExamplesAsync()
  {
    Console.WriteLine("Radio Console - Ducking System Examples");
    Console.WriteLine("========================================\n");

    await BasicDuckingExampleAsync();
    PresetsExample();
    await EmergencyDuckingExampleAsync();
    await CrossfadeExampleAsync();
    await EventBasedDuckingExampleAsync();
    await MetricsMonitoringExampleAsync();
    CustomConfigurationExample();
    await IntegrationExampleAsync();

    Console.WriteLine("\n========================================");
    Console.WriteLine("All examples completed successfully!");
  }
}

/// <summary>
/// Simple mock mixer service for demonstration purposes.
/// </summary>
internal class MockMixerService : IMixerService
{
  private readonly Dictionary<MixerChannel, float> _channelVolumes;
  private readonly Dictionary<string, float> _sourceVolumes;
  private float _masterVolume = 1.0f;
  private float _duckLevel = 0.2f;
  private bool _isDuckingActive;

  public bool IsInitialized => true;
  public float DuckLevel => _duckLevel;
  public bool IsDuckingActive => _isDuckingActive;

  public event EventHandler<MixerSourceEventArgs>? SourceAdded;
  public event EventHandler<MixerSourceEventArgs>? SourceRemoved;
  public event EventHandler<DuckingStateChangedEventArgs>? DuckingStateChanged;
  public event EventHandler<ChannelVolumeChangedEventArgs>? ChannelVolumeChanged;

  public MockMixerService()
  {
    _channelVolumes = new Dictionary<MixerChannel, float>
    {
      [MixerChannel.Main] = 1.0f,
      [MixerChannel.Event] = 1.0f,
      [MixerChannel.Voice] = 1.0f
    };
    _sourceVolumes = new Dictionary<string, float>();
  }

  public Task InitializeAsync(string outputDeviceId, CancellationToken cancellationToken = default) => Task.CompletedTask;

  public Task AddSourceAsync(ISoundFlowAudioSource source, MixerChannel channel, CancellationToken cancellationToken = default)
  {
    _sourceVolumes[source.Id] = 1.0f;
    SourceAdded?.Invoke(this, new MixerSourceEventArgs { SourceId = source.Id, Channel = channel });
    return Task.CompletedTask;
  }

  public Task RemoveSourceAsync(string sourceId, CancellationToken cancellationToken = default)
  {
    _sourceVolumes.Remove(sourceId);
    SourceRemoved?.Invoke(this, new MixerSourceEventArgs { SourceId = sourceId });
    return Task.CompletedTask;
  }

  public IEnumerable<ISoundFlowAudioSource> GetChannelSources(MixerChannel channel) => [];
  public IEnumerable<ISoundFlowAudioSource> GetAllSources() => [];
  public Task MoveSourceToChannelAsync(string sourceId, MixerChannel newChannel, CancellationToken cancellationToken = default) => Task.CompletedTask;

  public float GetChannelVolume(MixerChannel channel) => _channelVolumes.GetValueOrDefault(channel, 1.0f);

  public Task SetChannelVolumeAsync(MixerChannel channel, float volume, int rampDurationMs = 0, CancellationToken cancellationToken = default)
  {
    var oldVolume = _channelVolumes.GetValueOrDefault(channel, 1.0f);
    _channelVolumes[channel] = volume;
    ChannelVolumeChanged?.Invoke(this, new ChannelVolumeChangedEventArgs { Channel = channel, OldVolume = oldVolume, NewVolume = volume });
    return Task.CompletedTask;
  }

  public float GetMasterVolume() => _masterVolume;
  public Task SetMasterVolumeAsync(float volume, int rampDurationMs = 0, CancellationToken cancellationToken = default)
  {
    _masterVolume = volume;
    return Task.CompletedTask;
  }

  public float? GetSourceVolume(string sourceId) => _sourceVolumes.GetValueOrDefault(sourceId, 1.0f);

  public Task SetSourceVolumeAsync(string sourceId, float volume, int rampDurationMs = 0, CancellationToken cancellationToken = default)
  {
    _sourceVolumes[sourceId] = volume;
    return Task.CompletedTask;
  }

  public void SetDuckLevel(float level)
  {
    _duckLevel = level;
    DuckingStateChanged?.Invoke(this, new DuckingStateChangedEventArgs { IsDucking = _isDuckingActive, DuckLevel = level });
  }

  public void Dispose() { }
}
