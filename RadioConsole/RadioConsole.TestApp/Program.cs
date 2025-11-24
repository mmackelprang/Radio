using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RadioConsole.Core.Interfaces;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio;
using RadioConsole.Infrastructure.Configuration;
using Serilog;

namespace RadioConsole.TestApp;

/// <summary>
/// Test console application to validate audio functionality.
/// Tests streaming audio formats, global audio controls, and audio source management.
/// </summary>
public class Program
{
  public static async Task Main(string[] args)
  {
    // Configure Serilog
    Log.Logger = new LoggerConfiguration()
      .MinimumLevel.Information()
      .WriteTo.Console()
      .CreateLogger();

    Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
    Console.WriteLine("║       Radio Console - Audio Test Application              ║");
    Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
    Console.WriteLine();

    // Build service provider
    var services = new ServiceCollection();
    ConfigureServices(services);
    var serviceProvider = services.BuildServiceProvider();

    // Main menu loop
    var running = true;
    while (running)
    {
      Console.WriteLine();
      Console.WriteLine("═══════════════════════════════════════════════════════════");
      Console.WriteLine("  Select a test to run:");
      Console.WriteLine("═══════════════════════════════════════════════════════════");
      Console.WriteLine("  1. Test Streaming Audio Formats");
      Console.WriteLine("  2. Test Global Audio Controls");
      Console.WriteLine("  3. Test Audio Source Management (Create Only)");
      Console.WriteLine("  4. Test Audio Source Create & Play");
      Console.WriteLine("  5. Interactive Audio Source Player");
      Console.WriteLine("  6. Test All Features");
      Console.WriteLine("  0. Exit");
      Console.WriteLine("═══════════════════════════════════════════════════════════");
      Console.Write("  Enter choice: ");

      var input = Console.ReadLine()?.Trim();
      Console.WriteLine();

      switch (input)
      {
        case "1":
          await TestStreamingAudioFormats(serviceProvider);
          break;
        case "2":
          await TestGlobalAudioControls(serviceProvider);
          break;
        case "3":
          await TestAudioSourceManagement(serviceProvider);
          break;
        case "4":
          await TestAudioSourceCreateAndPlay(serviceProvider);
          break;
        case "5":
          await InteractiveAudioSourcePlayer(serviceProvider);
          break;
        case "6":
          await TestStreamingAudioFormats(serviceProvider);
          await TestGlobalAudioControls(serviceProvider);
          await TestAudioSourceManagement(serviceProvider);
          await TestAudioSourceCreateAndPlay(serviceProvider);
          break;
        case "0":
          running = false;
          break;
        default:
          Console.WriteLine("Invalid choice. Please try again.");
          break;
      }
    }

    Console.WriteLine("Test application exiting...");
    Log.CloseAndFlush();
  }

  private static void ConfigureServices(IServiceCollection services)
  {
    // Add logging
    services.AddLogging(builder =>
    {
      builder.AddSerilog();
    });

    // Add configuration service (JSON file-based)
    services.AddSingleton<IConfigurationService>(sp =>
    {
      return new JsonConfigurationService("./test-storage");
    });

    // Add audio services
    services.AddAudioServices();
  }

  private static Task TestStreamingAudioFormats(IServiceProvider serviceProvider)
  {
    Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
    Console.WriteLine("║        Test: Streaming Audio Formats                      ║");
    Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
    Console.WriteLine();

    Console.WriteLine("Supported Audio Formats:");
    Console.WriteLine("------------------------");
    
    var formats = new[] { "wav", "mp3", "flac", "aac", "ogg", "opus" };
    foreach (var format in formats)
    {
      var isSupported = RadioConsole.API.Services.StreamAudioService.IsFormatSupported(format);
      var contentType = RadioConsole.API.Services.StreamAudioService.GetContentType(format);
      Console.WriteLine($"  {format.ToUpper(),-6} - Supported: {isSupported,-5} - Content-Type: {contentType}");
    }

    Console.WriteLine();
    Console.WriteLine("Unsupported format test:");
    var invalidFormat = "xyz";
    var invalidSupported = RadioConsole.API.Services.StreamAudioService.IsFormatSupported(invalidFormat);
    Console.WriteLine($"  {invalidFormat.ToUpper(),-6} - Supported: {invalidSupported}");

    Console.WriteLine();
    Console.WriteLine("✓ Streaming audio format tests completed.");
    return Task.CompletedTask;
  }

  private static async Task TestGlobalAudioControls(IServiceProvider serviceProvider)
  {
    Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
    Console.WriteLine("║        Test: Global Audio Controls                        ║");
    Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
    Console.WriteLine();

    var deviceManager = serviceProvider.GetRequiredService<IAudioDeviceManager>();

    // Test Volume
    Console.WriteLine("Volume Control:");
    Console.WriteLine("---------------");
    var initialVolume = await deviceManager.GetGlobalVolumeAsync();
    Console.WriteLine($"  Initial volume: {initialVolume:F2}");

    await deviceManager.SetGlobalVolumeAsync(0.75f);
    var newVolume = await deviceManager.GetGlobalVolumeAsync();
    Console.WriteLine($"  Set volume to 0.75, current: {newVolume:F2}");

    await deviceManager.SetGlobalVolumeAsync(1.5f); // Should be clamped
    var clampedVolume = await deviceManager.GetGlobalVolumeAsync();
    Console.WriteLine($"  Set volume to 1.5 (should clamp to 1.0): {clampedVolume:F2}");

    Console.WriteLine();

    // Test Balance
    Console.WriteLine("Balance (Pan) Control:");
    Console.WriteLine("----------------------");
    var initialBalance = await deviceManager.GetGlobalBalanceAsync();
    Console.WriteLine($"  Initial balance: {initialBalance:F2}");

    await deviceManager.SetGlobalBalanceAsync(-0.5f);
    var leftBalance = await deviceManager.GetGlobalBalanceAsync();
    Console.WriteLine($"  Set balance to -0.5 (left): {leftBalance:F2}");

    await deviceManager.SetGlobalBalanceAsync(0.5f);
    var rightBalance = await deviceManager.GetGlobalBalanceAsync();
    Console.WriteLine($"  Set balance to 0.5 (right): {rightBalance:F2}");

    await deviceManager.SetGlobalBalanceAsync(0.0f);
    var centerBalance = await deviceManager.GetGlobalBalanceAsync();
    Console.WriteLine($"  Set balance to 0.0 (center): {centerBalance:F2}");

    Console.WriteLine();

    // Test Equalization
    Console.WriteLine("Equalization Control:");
    Console.WriteLine("---------------------");
    var initialEq = await deviceManager.GetEqualizationAsync();
    Console.WriteLine($"  Initial EQ - Bass: {initialEq.Bass:F1}dB, Mid: {initialEq.Midrange:F1}dB, Treble: {initialEq.Treble:F1}dB, Enabled: {initialEq.Enabled}");

    await deviceManager.SetEqualizationAsync(new EqualizationSettings
    {
      Bass = 3.0f,
      Midrange = -1.0f,
      Treble = 2.0f,
      Enabled = true
    });
    var newEq = await deviceManager.GetEqualizationAsync();
    Console.WriteLine($"  New EQ - Bass: {newEq.Bass:F1}dB, Mid: {newEq.Midrange:F1}dB, Treble: {newEq.Treble:F1}dB, Enabled: {newEq.Enabled}");

    Console.WriteLine();

    // Test Playback State
    Console.WriteLine("Playback State Control:");
    Console.WriteLine("-----------------------");
    var initialState = await deviceManager.GetPlaybackStateAsync();
    Console.WriteLine($"  Initial state: {initialState}");

    await deviceManager.PlayAsync();
    var playingState = await deviceManager.GetPlaybackStateAsync();
    Console.WriteLine($"  After Play(): {playingState}");

    await deviceManager.PauseAsync();
    var pausedState = await deviceManager.GetPlaybackStateAsync();
    Console.WriteLine($"  After Pause(): {pausedState}");

    await deviceManager.StopAsync();
    var stoppedState = await deviceManager.GetPlaybackStateAsync();
    Console.WriteLine($"  After Stop(): {stoppedState}");

    Console.WriteLine();
    Console.WriteLine("✓ Global audio control tests completed.");
  }

  private static async Task TestAudioSourceManagement(IServiceProvider serviceProvider)
  {
    Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
    Console.WriteLine("║        Test: Audio Source Management (Create Only)        ║");
    Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
    Console.WriteLine();

    var sourceManager = serviceProvider.GetRequiredService<IAudioSourceManager>();

    // Test creating standard audio sources
    Console.WriteLine("Creating Standard Audio Sources:");
    Console.WriteLine("--------------------------------");

    var spotifyId = await sourceManager.CreateSpotifySourceAsync();
    Console.WriteLine($"  Created Spotify source: {spotifyId}");

    var radioId = await sourceManager.CreateRadioSourceAsync();
    Console.WriteLine($"  Created Radio source: {radioId}");

    var vinylId = await sourceManager.CreateVinylRecordSourceAsync();
    Console.WriteLine($"  Created Vinyl Record source: {vinylId}");

    var filePlayerId = await sourceManager.CreateFilePlayerSourceAsync("/path/to/audio.mp3");
    Console.WriteLine($"  Created File Player source: {filePlayerId}");

    Console.WriteLine();

    // Test creating high-priority audio sources
    Console.WriteLine("Creating High-Priority Audio Sources:");
    Console.WriteLine("-------------------------------------");

    var ttsId = await sourceManager.CreateTtsEventSourceAsync("Hello, this is a test", "en-US-Standard-A", 1.0f);
    Console.WriteLine($"  Created TTS Event source: {ttsId}");

    var fileEventId = await sourceManager.CreateFileEventSourceAsync("/sounds/doorbell.wav");
    Console.WriteLine($"  Created File Event source: {fileEventId}");

    Console.WriteLine();

    // Test listing active sources
    Console.WriteLine("Active Audio Sources:");
    Console.WriteLine("---------------------");
    var sources = await sourceManager.GetActiveSourcesAsync();
    foreach (var source in sources)
    {
      Console.WriteLine($"  [{source.Type}] {source.Id} - {source.Name} (Priority: {(source.IsHighPriority ? "HIGH" : "Standard")}, Status: {source.Status})");
    }

    Console.WriteLine();

    // Test getting specific source info
    Console.WriteLine("Source Info:");
    Console.WriteLine("------------");
    var ttsInfo = await sourceManager.GetSourceInfoAsync(ttsId);
    if (ttsInfo != null)
    {
      Console.WriteLine($"  TTS Source Info:");
      Console.WriteLine($"    ID: {ttsInfo.Id}");
      Console.WriteLine($"    Type: {ttsInfo.Type}");
      Console.WriteLine($"    High Priority: {ttsInfo.IsHighPriority}");
      Console.WriteLine($"    Metadata:");
      foreach (var kvp in ttsInfo.Metadata)
      {
        Console.WriteLine($"      {kvp.Key}: {kvp.Value}");
      }
    }

    Console.WriteLine();

    // Test stopping sources
    Console.WriteLine("Stopping Sources:");
    Console.WriteLine("-----------------");
    await sourceManager.StopSourceAsync(ttsId);
    Console.WriteLine($"  Stopped TTS source: {ttsId}");

    await sourceManager.StopSourceAsync(fileEventId);
    Console.WriteLine($"  Stopped File Event source: {fileEventId}");

    var remainingSources = await sourceManager.GetActiveSourcesAsync();
    Console.WriteLine($"  Remaining active sources: {remainingSources.Count()}");

    // Stop all remaining sources
    await sourceManager.StopAllSourcesAsync();
    var finalSources = await sourceManager.GetActiveSourcesAsync();
    Console.WriteLine($"  After StopAllSources: {finalSources.Count()} sources remaining");

    Console.WriteLine();
    Console.WriteLine("✓ Audio source management tests completed.");
  }

  private static async Task TestAudioSourceCreateAndPlay(IServiceProvider serviceProvider)
  {
    Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
    Console.WriteLine("║        Test: Audio Source Create & Play                   ║");
    Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
    Console.WriteLine();

    var sourceManager = serviceProvider.GetRequiredService<IAudioSourceManager>();

    // Create and play a TTS Event source
    Console.WriteLine("Creating and Playing TTS Event Source:");
    Console.WriteLine("--------------------------------------");
    var ttsId = await sourceManager.CreateTtsEventSourceAsync(
      "Hello! This is a test of the text to speech system.",
      "en-US",
      1.0f);
    Console.WriteLine($"  Created TTS Event source: {ttsId}");

    var ttsInfo = await sourceManager.GetSourceInfoAsync(ttsId);
    Console.WriteLine($"  Status before play: {ttsInfo?.Status}");

    Console.WriteLine("  Starting playback...");
    await sourceManager.PlaySourceAsync(ttsId);
    
    ttsInfo = await sourceManager.GetSourceInfoAsync(ttsId);
    Console.WriteLine($"  Status after play: {ttsInfo?.Status}");

    Console.WriteLine("  Pausing...");
    await sourceManager.PauseSourceAsync(ttsId);
    
    ttsInfo = await sourceManager.GetSourceInfoAsync(ttsId);
    Console.WriteLine($"  Status after pause: {ttsInfo?.Status}");

    Console.WriteLine("  Resuming...");
    await sourceManager.ResumeSourceAsync(ttsId);
    
    ttsInfo = await sourceManager.GetSourceInfoAsync(ttsId);
    Console.WriteLine($"  Status after resume: {ttsInfo?.Status}");

    Console.WriteLine("  Stopping...");
    await sourceManager.StopSourceAsync(ttsId);

    Console.WriteLine();

    // Create and play a File Player source
    Console.WriteLine("Creating and Playing File Player Source:");
    Console.WriteLine("-----------------------------------------");
    
    // Create a sample audio file path (user would provide real path)
    Console.Write("  Enter path to an audio file (or press Enter to skip): ");
    var audioPath = Console.ReadLine()?.Trim();

    if (!string.IsNullOrEmpty(audioPath))
    {
      var filePlayerId = await sourceManager.CreateFilePlayerSourceAsync(audioPath);
      Console.WriteLine($"  Created File Player source: {filePlayerId}");

      var fileInfo = await sourceManager.GetSourceInfoAsync(filePlayerId);
      Console.WriteLine($"  Status before play: {fileInfo?.Status}");
      Console.WriteLine($"  File path: {fileInfo?.Metadata.GetValueOrDefault("Path", "N/A")}");

      Console.WriteLine("  Starting playback...");
      await sourceManager.PlaySourceAsync(filePlayerId);

      fileInfo = await sourceManager.GetSourceInfoAsync(filePlayerId);
      Console.WriteLine($"  Status after play: {fileInfo?.Status}");

      Console.WriteLine("  Stopping...");
      await sourceManager.StopSourceAsync(filePlayerId);
    }
    else
    {
      Console.WriteLine("  Skipped file player test.");
    }

    Console.WriteLine();

    // Create and play a File Event source (high priority)
    Console.WriteLine("Creating and Playing File Event Source (High Priority):");
    Console.WriteLine("---------------------------------------------------------");
    
    Console.Write("  Enter path to a notification sound file (or press Enter to skip): ");
    var notificationPath = Console.ReadLine()?.Trim();

    if (!string.IsNullOrEmpty(notificationPath))
    {
      var fileEventId = await sourceManager.CreateFileEventSourceAsync(notificationPath);
      Console.WriteLine($"  Created File Event source (HIGH PRIORITY): {fileEventId}");

      var eventInfo = await sourceManager.GetSourceInfoAsync(fileEventId);
      Console.WriteLine($"  Status before play: {eventInfo?.Status}");
      Console.WriteLine($"  Is High Priority: {eventInfo?.IsHighPriority}");

      Console.WriteLine("  Starting playback (this would duck other audio sources)...");
      await sourceManager.PlaySourceAsync(fileEventId);

      eventInfo = await sourceManager.GetSourceInfoAsync(fileEventId);
      Console.WriteLine($"  Status after play: {eventInfo?.Status}");

      Console.WriteLine("  Stopping...");
      await sourceManager.StopSourceAsync(fileEventId);
    }
    else
    {
      Console.WriteLine("  Skipped file event test.");
    }

    // Clean up any remaining sources
    await sourceManager.StopAllSourcesAsync();

    Console.WriteLine();
    Console.WriteLine("✓ Audio source create & play tests completed.");
  }

  private static async Task InteractiveAudioSourcePlayer(IServiceProvider serviceProvider)
  {
    Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
    Console.WriteLine("║        Interactive Audio Source Player                    ║");
    Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
    Console.WriteLine();

    var sourceManager = serviceProvider.GetRequiredService<IAudioSourceManager>();
    var running = true;

    while (running)
    {
      // Show active sources
      var sources = (await sourceManager.GetActiveSourcesAsync()).ToList();
      Console.WriteLine();
      Console.WriteLine("Active Sources: " + (sources.Count == 0 ? "(none)" : ""));
      for (int i = 0; i < sources.Count; i++)
      {
        var s = sources[i];
        Console.WriteLine($"  [{i + 1}] {s.Id} - {s.Type} - Status: {s.Status} {(s.IsHighPriority ? "(HIGH PRIORITY)" : "")}");
      }

      Console.WriteLine();
      Console.WriteLine("Commands:");
      Console.WriteLine("  1. Create Spotify source");
      Console.WriteLine("  2. Create Radio source");
      Console.WriteLine("  3. Create Vinyl Record source");
      Console.WriteLine("  4. Create File Player source");
      Console.WriteLine("  5. Create TTS Event source (High Priority)");
      Console.WriteLine("  6. Create File Event source (High Priority)");
      Console.WriteLine("  P. Play source");
      Console.WriteLine("  A. Pause source");
      Console.WriteLine("  R. Resume source");
      Console.WriteLine("  S. Stop source");
      Console.WriteLine("  X. Stop all sources");
      Console.WriteLine("  0. Back to main menu");
      Console.Write("Enter command: ");

      var input = Console.ReadLine()?.Trim().ToUpper();
      Console.WriteLine();

      try
      {
        switch (input)
        {
          case "1":
            var spotifyId = await sourceManager.CreateSpotifySourceAsync();
            Console.WriteLine($"Created Spotify source: {spotifyId}");
            break;

          case "2":
            var radioId = await sourceManager.CreateRadioSourceAsync();
            Console.WriteLine($"Created Radio source: {radioId}");
            break;

          case "3":
            var vinylId = await sourceManager.CreateVinylRecordSourceAsync();
            Console.WriteLine($"Created Vinyl Record source: {vinylId}");
            break;

          case "4":
            Console.Write("Enter file path: ");
            var filePath = Console.ReadLine()?.Trim();
            var fileId = await sourceManager.CreateFilePlayerSourceAsync(filePath);
            Console.WriteLine($"Created File Player source: {fileId}");
            break;

          case "5":
            Console.Write("Enter text to speak: ");
            var ttsText = Console.ReadLine()?.Trim() ?? "Hello, this is a test.";
            Console.Write("Enter voice (or press Enter for default): ");
            var ttsVoice = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(ttsVoice)) ttsVoice = "en-US";
            var ttsId = await sourceManager.CreateTtsEventSourceAsync(ttsText, ttsVoice, 1.0f);
            Console.WriteLine($"Created TTS Event source: {ttsId}");
            break;

          case "6":
            Console.Write("Enter notification sound file path: ");
            var eventPath = Console.ReadLine()?.Trim() ?? "/sounds/notification.wav";
            var eventId = await sourceManager.CreateFileEventSourceAsync(eventPath);
            Console.WriteLine($"Created File Event source: {eventId}");
            break;

          case "P":
            Console.Write("Enter source number to play: ");
            if (int.TryParse(Console.ReadLine(), out var playNum) && playNum > 0 && playNum <= sources.Count)
            {
              var sourceToPlay = sources[playNum - 1];
              await sourceManager.PlaySourceAsync(sourceToPlay.Id);
              Console.WriteLine($"Playing: {sourceToPlay.Id}");
            }
            else
            {
              Console.WriteLine("Invalid source number.");
            }
            break;

          case "A":
            Console.Write("Enter source number to pause: ");
            if (int.TryParse(Console.ReadLine(), out var pauseNum) && pauseNum > 0 && pauseNum <= sources.Count)
            {
              var sourceToPause = sources[pauseNum - 1];
              await sourceManager.PauseSourceAsync(sourceToPause.Id);
              Console.WriteLine($"Paused: {sourceToPause.Id}");
            }
            else
            {
              Console.WriteLine("Invalid source number.");
            }
            break;

          case "R":
            Console.Write("Enter source number to resume: ");
            if (int.TryParse(Console.ReadLine(), out var resumeNum) && resumeNum > 0 && resumeNum <= sources.Count)
            {
              var sourceToResume = sources[resumeNum - 1];
              await sourceManager.ResumeSourceAsync(sourceToResume.Id);
              Console.WriteLine($"Resumed: {sourceToResume.Id}");
            }
            else
            {
              Console.WriteLine("Invalid source number.");
            }
            break;

          case "S":
            Console.Write("Enter source number to stop: ");
            if (int.TryParse(Console.ReadLine(), out var stopNum) && stopNum > 0 && stopNum <= sources.Count)
            {
              var sourceToStop = sources[stopNum - 1];
              await sourceManager.StopSourceAsync(sourceToStop.Id);
              Console.WriteLine($"Stopped: {sourceToStop.Id}");
            }
            else
            {
              Console.WriteLine("Invalid source number.");
            }
            break;

          case "X":
            await sourceManager.StopAllSourcesAsync();
            Console.WriteLine("All sources stopped.");
            break;

          case "0":
            running = false;
            await sourceManager.StopAllSourcesAsync();
            break;

          default:
            Console.WriteLine("Invalid command.");
            break;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error: {ex.Message}");
      }
    }

    Console.WriteLine("Exiting interactive player.");
  }
}
