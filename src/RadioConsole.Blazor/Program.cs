using RadioConsole.Blazor.Components;
using MudBlazor.Services;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Services;
using RadioConsole.Api.Modules.Inputs;
using RadioConsole.Api.Modules.Outputs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MudBlazor services for Material Design 3
builder.Services.AddMudServices();

// Register core services from RadioConsole.Api
builder.Services.AddSingleton<IEnvironmentService, EnvironmentService>();
builder.Services.AddSingleton<IStorage, JsonStorageService>();

// Register audio input modules
builder.Services.AddSingleton<IAudioInput, SpotifyInput>();

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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Log the URLs the application is listening on
app.Lifetime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var addresses = app.Urls;
    
    logger.LogInformation("======================================");
    logger.LogInformation("Radio Console Blazor UI Started");
    logger.LogInformation("======================================");
    
    foreach (var address in addresses)
    {
        logger.LogInformation("Blazor UI listening on: {Address}", address);
    }
    
    logger.LogInformation("======================================");
    logger.LogInformation("Open your browser to access the Radio Console interface");
    logger.LogInformation("======================================");
});

app.Run();
