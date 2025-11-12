using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Models;
using RadioConsole.Api.Modules.Inputs;
using RadioConsole.Api.Modules.Outputs;
using RadioConsole.Api.Services;

namespace RadioConsole.TestApp;

/// <summary>
/// Console application for testing Radio Console audio inputs and outputs on Raspberry Pi
/// </summary>
class Program
{
  static async Task Main(string[] args)
  {
    Console.WriteLine("========================================");
    Console.WriteLine("Radio Console Test Application");
    Console.WriteLine("========================================");
    Console.WriteLine();

    // Setup dependency injection and configuration
    var services = new ServiceCollection();
    ConfigureServices(services);
    var serviceProvider = services.BuildServiceProvider();

    // Get required services
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    var environmentService = serviceProvider.GetRequiredService<IEnvironmentService>();
    var storage = serviceProvider.GetRequiredService<IStorage>();
    var ttsService = serviceProvider.GetRequiredService<ITtsService>();
    var audioMixer = serviceProvider.GetRequiredService<AudioMixer>();

    logger.LogInformation("Starting Radio Console Test Application");
    logger.LogInformation("Environment: {Environment}", 
      environmentService.IsSimulationMode ? "Simulation Mode" : "Hardware Mode");

    // Initialize TTS service
    await ttsService.InitializeAsync();

    // Setup audio system with WiredSoundBar output
    logger.LogInformation("Setting up audio output: WiredSoundBar");
    var soundbarOutput = new WiredSoundbarOutput(environmentService, storage);
    await soundbarOutput.InitializeAsync();

    if (!soundbarOutput.IsAvailable)
    {
      logger.LogWarning("WiredSoundBar output is not available");
    }

    // Start audio mixer
    await audioMixer.StartAsync();

    // Run test scenarios
    await RunTestScenarios(logger, environmentService, storage, ttsService, audioMixer);

    logger.LogInformation("Test application completed. Press any key to exit.");
    Console.ReadKey();
  }

  static void ConfigureServices(IServiceCollection services)
  {
    // Add configuration
    var configuration = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", optional: true)
      .AddEnvironmentVariables()
      .Build();

    services.AddSingleton<IConfiguration>(configuration);

    // Add logging
    services.AddLogging(builder =>
    {
      builder.AddConsole();
      builder.SetMinimumLevel(LogLevel.Information);
    });

    // Configure eSpeak TTS settings
    services.Configure<ESpeakTtsConfig>(options =>
    {
      options.ESpeakExecutablePath = "espeak-ng";
      options.Voice = "en-us";
      options.Speed = 175;
      options.Pitch = 50;
      options.Volume = 100;
      options.WordGap = 0;
      options.SampleRate = 22050;
    });

    // Register core services
    services.AddSingleton<IEnvironmentService, EnvironmentService>();
    services.AddSingleton<IStorage, JsonStorageService>();
    services.AddSingleton<ITtsService, ESpeakTtsService>();
    services.AddSingleton<AudioMixer>();
  }

  static async Task RunTestScenarios(
    ILogger logger,
    IEnvironmentService environmentService,
    IStorage storage,
    ITtsService ttsService,
    AudioMixer audioMixer)
  {
    logger.LogInformation("");
    logger.LogInformation("========================================");
    logger.LogInformation("Running Test Scenarios");
    logger.LogInformation("========================================");

    // Test 1: FileAudioInput with sample MP3 files
    await TestFileAudioInput(logger, environmentService, storage, audioMixer);

    // Test 2: TtsAudioInput with sample text
    await TestTtsAudioInput(logger, environmentService, storage, ttsService, audioMixer);

    // Test 3: CompositeAudioInput with sample text and MP3 files
    await TestCompositeAudioInput(logger, environmentService, storage, ttsService, audioMixer);

    logger.LogInformation("");
    logger.LogInformation("========================================");
    logger.LogInformation("All Test Scenarios Completed");
    logger.LogInformation("========================================");
  }

  static async Task TestFileAudioInput(
    ILogger logger,
    IEnvironmentService environmentService,
    IStorage storage,
    AudioMixer audioMixer)
  {
    logger.LogInformation("");
    logger.LogInformation("--- Test 1: FileAudioInput ---");

    try
    {
      // Create test MP3 file paths (these would need to exist in real hardware testing)
      var testDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");
      Directory.CreateDirectory(testDataPath);

      // In simulation mode, these files don't need to exist
      var mp3File1 = Path.Combine(testDataPath, "test_music1.mp3");
      var mp3File2 = Path.Combine(testDataPath, "test_music2.mp3");

      if (!environmentService.IsSimulationMode)
      {
        // Create placeholder files for testing if they don't exist
        if (!File.Exists(mp3File1))
        {
          logger.LogInformation("Note: Create {File} for actual audio testing", mp3File1);
        }
        if (!File.Exists(mp3File2))
        {
          logger.LogInformation("Note: Create {File} for actual audio testing", mp3File2);
        }
      }

      // Test FileAudioInput 1
      logger.LogInformation("Creating FileAudioInput 1: {File}", Path.GetFileName(mp3File1));
      var fileInput1 = new FileAudioInput(
        mp3File1,
        "Test Music 1",
        EventPriority.Medium,
        environmentService,
        storage);

      await fileInput1.InitializeAsync();
      audioMixer.RegisterSource(fileInput1);

      logger.LogInformation("  Status: {Status}", fileInput1.IsAvailable ? "Available" : "Not Available");

      if (fileInput1.IsAvailable)
      {
        logger.LogInformation("  Starting playback...");
        await fileInput1.StartAsync();
        await Task.Delay(2000); // Play for 2 seconds

        logger.LogInformation("  Pausing playback...");
        await fileInput1.PauseAsync();
        await Task.Delay(1000);

        logger.LogInformation("  Resuming playback...");
        await fileInput1.ResumeAsync();
        await Task.Delay(2000);

        logger.LogInformation("  Stopping playback...");
        await fileInput1.StopAsync();
      }

      // Test FileAudioInput 2
      logger.LogInformation("Creating FileAudioInput 2: {File}", Path.GetFileName(mp3File2));
      var fileInput2 = new FileAudioInput(
        mp3File2,
        "Test Music 2",
        EventPriority.Low,
        environmentService,
        storage);

      await fileInput2.InitializeAsync();
      audioMixer.RegisterSource(fileInput2);

      logger.LogInformation("  Status: {Status}", fileInput2.IsAvailable ? "Available" : "Not Available");

      if (fileInput2.IsAvailable)
      {
        logger.LogInformation("  Starting playback with volume 0.5...");
        await fileInput2.SetVolumeAsync(0.5);
        await fileInput2.StartAsync();
        await Task.Delay(3000); // Play for 3 seconds

        logger.LogInformation("  Stopping playback...");
        await fileInput2.StopAsync();
      }

      logger.LogInformation("FileAudioInput test completed successfully");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "FileAudioInput test failed");
    }
  }

  static async Task TestTtsAudioInput(
    ILogger logger,
    IEnvironmentService environmentService,
    IStorage storage,
    ITtsService ttsService,
    AudioMixer audioMixer)
  {
    logger.LogInformation("");
    logger.LogInformation("--- Test 2: TtsAudioInput ---");

    try
    {
      // Test TTS 1
      logger.LogInformation("Creating TtsAudioInput 1: Welcome message");
      var ttsInput1 = new TtsAudioInput(environmentService, storage, ttsService);
      await ttsInput1.InitializeAsync();
      audioMixer.RegisterSource(ttsInput1);

      logger.LogInformation("  Status: {Status}", ttsInput1.IsAvailable ? "Available" : "Not Available");

      if (ttsInput1.IsAvailable)
      {
        var welcomeText = "Welcome to the Radio Console Test Application. This is a text to speech announcement.";
        logger.LogInformation("  Announcing: {Text}", welcomeText);
        await ttsInput1.AnnounceTextAsync(welcomeText);
        await Task.Delay(1000); // Wait a bit
        await ttsInput1.StopAsync();
      }

      // Test TTS 2
      logger.LogInformation("Creating TtsAudioInput 2: Time announcement");
      var ttsInput2 = new TtsAudioInput(environmentService, storage, ttsService);
      await ttsInput2.InitializeAsync();
      audioMixer.RegisterSource(ttsInput2);

      if (ttsInput2.IsAvailable)
      {
        var timeText = $"The current time is {DateTime.Now:h:mm tt}.";
        logger.LogInformation("  Announcing: {Text}", timeText);
        await ttsInput2.AnnounceTextAsync(timeText);
        await Task.Delay(1000); // Wait a bit
        await ttsInput2.StopAsync();
      }

      logger.LogInformation("TtsAudioInput test completed successfully");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "TtsAudioInput test failed");
    }
  }

  static async Task TestCompositeAudioInput(
    ILogger logger,
    IEnvironmentService environmentService,
    IStorage storage,
    ITtsService ttsService,
    AudioMixer audioMixer)
  {
    logger.LogInformation("");
    logger.LogInformation("--- Test 3: CompositeAudioInput ---");

    try
    {
      var testDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");

      // Test Composite 1: Serial playback
      logger.LogInformation("Creating CompositeAudioInput 1: Serial playback (TTS + File)");
      var compositeInput1 = new CompositeAudioInput(
        "composite_test1",
        "Composite Test 1",
        EventPriority.High,
        playSerially: true,
        environmentService,
        storage);

      // Add TTS intro
      compositeInput1.AddTtsInput(
        "This is a composite audio input combining text to speech and music files.",
        ttsService,
        volume: 1.0);

      // Add file (if available)
      var mp3File = Path.Combine(testDataPath, "test_music1.mp3");
      if (File.Exists(mp3File) || environmentService.IsSimulationMode)
      {
        compositeInput1.AddFileInput(mp3File, volume: 0.8);
      }

      await compositeInput1.InitializeAsync();
      audioMixer.RegisterSource(compositeInput1);

      logger.LogInformation("  Status: {Status}", compositeInput1.IsAvailable ? "Available" : "Not Available");
      logger.LogInformation("  Duration: {Duration}", compositeInput1.Duration?.ToString(@"mm\:ss") ?? "N/A");

      if (compositeInput1.IsAvailable)
      {
        logger.LogInformation("  Starting serial playback...");
        await compositeInput1.StartAsync();
        await Task.Delay(3000); // Let it play

        logger.LogInformation("  Stopping playback...");
        await compositeInput1.StopAsync();
      }

      // Test Composite 2: Concurrent playback
      logger.LogInformation("Creating CompositeAudioInput 2: Concurrent playback (Multiple TTS)");
      var compositeInput2 = new CompositeAudioInput(
        "composite_test2",
        "Composite Test 2",
        EventPriority.Medium,
        playSerially: false,
        environmentService,
        storage);

      // Add multiple TTS inputs
      compositeInput2.AddTtsInput("First announcement.", ttsService, volume: 0.7);
      compositeInput2.AddTtsInput("Second announcement.", ttsService, volume: 0.5);

      await compositeInput2.InitializeAsync();
      audioMixer.RegisterSource(compositeInput2);

      logger.LogInformation("  Status: {Status}", compositeInput2.IsAvailable ? "Available" : "Not Available");

      if (compositeInput2.IsAvailable)
      {
        logger.LogInformation("  Starting concurrent playback...");
        await compositeInput2.StartAsync();
        await Task.Delay(3000); // Let it play

        logger.LogInformation("  Stopping playback...");
        await compositeInput2.StopAsync();
      }

      logger.LogInformation("CompositeAudioInput test completed successfully");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "CompositeAudioInput test failed");
    }
  }
}
