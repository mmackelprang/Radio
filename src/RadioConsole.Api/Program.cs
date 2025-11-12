using RadioConsole.Api.Services;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Modules.Inputs;
using RadioConsole.Api.Modules.Outputs;
using RadioConsole.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Radio Console API",
        Version = "v1",
        Description = @"Radio Console API for managing audio inputs, outputs, and events.

## Audio Input Types
- **Music**: Traditional streaming sources (Radio, Spotify, USB, Files)
- **Event**: Priority-based notifications (Doorbells, Timers, Alerts)

## Key Features
- Multiple audio inputs with priority management
- Event-driven audio with automatic volume ducking
- Real-time audio mixing
- Text-to-speech announcements (eSpeak-ng, Piper, Google Cloud TTS)
- Composite audio inputs (combine files and TTS)
- File-based audio playback (MP3, WAV)

## Event Priorities
- **Low**: Informational events
- **Medium**: Standard notifications
- **High**: Urgent notifications (doorbell, phone)
- **Critical**: Emergency alerts

See EVENTS_DOCUMENTATION.md for detailed event configuration examples.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Radio Console",
            Url = new Uri("https://github.com/mmackelprang/Radio")
        }
    });
});

// Add SignalR for real-time communication
builder.Services.AddSignalR();

// Add CORS policy for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// Configure TTS settings
builder.Services.Configure<TtsConfig>(
    builder.Configuration.GetSection("Tts"));

// Register services
builder.Services.AddSingleton<IEnvironmentService, EnvironmentService>();
builder.Services.AddSingleton<IStorage, JsonStorageService>();
builder.Services.AddSingleton<ITtsService, TtsService>();

// Register device management services
builder.Services.AddSingleton<IDeviceFactory, DeviceFactory>();
builder.Services.AddSingleton<IDeviceRegistry, DeviceRegistry>();

// Register audio priority manager
builder.Services.AddSingleton<IAudioPriorityManager, AudioPriorityManager>();

// Register audio mixer
builder.Services.AddSingleton<AudioMixer>();

// Register music audio input modules
builder.Services.AddSingleton<IAudioInput, SpotifyInput>();

// Register TTS audio input module
builder.Services.AddSingleton<IAudioInput, TtsAudioInput>();

// Note: RadioInput, DoorbellEventInput, TelephoneRingingEventInput, GoogleBroadcastEventInput,
// TimerExpiredEventInput, and ReminderEventInput have been removed.
// Use UsbAudioInput, FileAudioInput, and CompositeAudioInput instead.
// See AUDIO_INPUT_MIGRATION.md for migration guide.

// Register audio output modules
builder.Services.AddSingleton<IAudioOutput, WiredSoundbarOutput>();
builder.Services.AddSingleton<IAudioOutput, ChromecastOutput>();

var app = builder.Build();

// Initialize device registry
var deviceRegistry = app.Services.GetRequiredService<IDeviceRegistry>();
await deviceRegistry.LoadConfigurationsAsync();

// Initialize all audio modules
var inputs = app.Services.GetServices<IAudioInput>();
var outputs = app.Services.GetServices<IAudioOutput>();

foreach (var input in inputs)
{
    await input.InitializeAsync();
}

foreach (var output in outputs)
{
    await output.InitializeAsync();
}

// Initialize audio mixer
var audioMixer = app.Services.GetRequiredService<AudioMixer>();
await audioMixer.StartAsync();

// Register all inputs with the mixer
foreach (var input in inputs)
{
    audioMixer.RegisterSource(input);
}

// Register event inputs with the priority manager
var priorityManager = app.Services.GetRequiredService<IAudioPriorityManager>();
foreach (var input in inputs)
{
    if (input.InputType == AudioInputType.Event)
    {
        priorityManager.RegisterEventInput(input);
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Map SignalR hubs (will be added later)
// app.MapHub<AudioHub>("/hubs/audio");

// Log the URLs the application is listening on
app.Lifetime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var addresses = app.Urls;
    
    logger.LogInformation("======================================");
    logger.LogInformation("Radio Console API Started");
    logger.LogInformation("======================================");
    
    foreach (var address in addresses)
    {
        logger.LogInformation("API listening on: {Address}", address);
        
        if (app.Environment.IsDevelopment())
        {
            // Log Swagger/OpenAPI URLs
            var httpAddress = address.Replace("https://", "http://");
            logger.LogInformation("  -> Swagger UI: {Address}/swagger", address);
            logger.LogInformation("  -> OpenAPI JSON: {Address}/swagger/v1/swagger.json", address);
        }
    }
    
    logger.LogInformation("======================================");
});

app.Run();
