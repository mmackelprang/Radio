using RadioConsole.Api.Services;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Modules.Inputs;
using RadioConsole.Api.Modules.Outputs;

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

// Register services
builder.Services.AddSingleton<IEnvironmentService, EnvironmentService>();
builder.Services.AddSingleton<IStorage, JsonStorageService>();

// Register audio modules
builder.Services.AddSingleton<IAudioInput, RadioInput>();
builder.Services.AddSingleton<IAudioInput, SpotifyInput>();
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
