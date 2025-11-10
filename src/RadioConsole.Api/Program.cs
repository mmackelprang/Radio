using RadioConsole.Api.Services;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Modules.Inputs;
using RadioConsole.Api.Modules.Outputs;
using RadioConsole.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Configure eSpeak TTS settings
builder.Services.Configure<ESpeakTtsConfig>(
    builder.Configuration.GetSection("ESpeakTts"));

// Register services
builder.Services.AddSingleton<IEnvironmentService, EnvironmentService>();
builder.Services.AddSingleton<IStorage, JsonStorageService>();
builder.Services.AddSingleton<ITtsService, ESpeakTtsService>();

// Register audio priority manager
builder.Services.AddSingleton<IAudioPriorityManager, AudioPriorityManager>();

// Register music audio input modules
builder.Services.AddSingleton<IAudioInput, RadioInput>();
builder.Services.AddSingleton<IAudioInput, SpotifyInput>();

// Register event audio input modules
builder.Services.AddSingleton<IAudioInput, DoorbellEventInput>();
builder.Services.AddSingleton<IAudioInput, TelephoneRingingEventInput>();
builder.Services.AddSingleton<IAudioInput, GoogleBroadcastEventInput>();
builder.Services.AddSingleton<IAudioInput, TimerExpiredEventInput>();
builder.Services.AddSingleton<IAudioInput, ReminderEventInput>();
builder.Services.AddSingleton<IAudioInput, TtsAudioInput>();

// Register audio output modules
builder.Services.AddSingleton<IAudioOutput, WiredSoundbarOutput>();
builder.Services.AddSingleton<IAudioOutput, ChromecastOutput>();

var app = builder.Build();

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

app.Run();
