using RadioConsole.Infrastructure.Configuration;
using RadioConsole.Infrastructure.Audio;
using RadioConsole.Infrastructure.Inputs;
using RadioConsole.Web.Components;
using RadioConsole.Web.Services;
using RadioConsole.Core.Interfaces.Audio;
using Serilog;
using MudBlazor.Services;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
  .MinimumLevel.Information()
  .WriteTo.Console()
  .WriteTo.File("./logs/web-.log", rollingInterval: RollingInterval.Day)
  .CreateLogger();

try
{
  Log.Information("Starting Radio Console Web Application");

  var builder = WebApplication.CreateBuilder(args);

  // Check for appsettings.json and log its location
  var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
  if (File.Exists(appSettingsPath))
  {
    Log.Information("Found appsettings.json at: {Path}", appSettingsPath);
  }
  else
  {
    Log.Warning("appsettings.json not found at expected location: {Path}", appSettingsPath);
    Log.Warning("Looking for configuration in: {BaseDir}", AppDomain.CurrentDomain.BaseDirectory);
    Log.Warning("Current directory: {CurrentDir}", Directory.GetCurrentDirectory());
  }

  // Add Serilog to the app
  builder.Host.UseSerilog();

  // Add configuration service with settings from appsettings.json
  builder.Services.AddConfigurationService(builder.Configuration);

  // Add audio services
  builder.Services.AddAudioServices(builder.Configuration);

  // Add input services (Raddy Radio, Spotify, Broadcast Receiver)
  builder.Services.AddInputServices();

  // Add MudBlazor services
  builder.Services.AddMudServices();

  // Add HttpClient for making API calls
  builder.Services.AddHttpClient("API", client =>
  {
      var apiBaseUrl = builder.Configuration["RadioConsole:ApiBaseUrl"] ?? "http://localhost:5100";
      client.BaseAddress = new Uri(apiBaseUrl);
  });

  // Add SignalR for real-time visualizer data
  builder.Services.AddSignalR();

  // Register visualization service (SignalR implementation)
  builder.Services.AddSingleton<IVisualizationService, SignalRVisualizationService>();

  // Add services to the container.
  builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

  var app = builder.Build();

  // Log server configuration and API connection
  var webUrl = builder.Configuration["Kestrel:Endpoints:Http:Url"] ?? "http://localhost:5200";
  var apiBaseUrl = builder.Configuration["RadioConsole:ApiBaseUrl"] ?? "http://localhost:5100";
  Log.Information("===== Radio Console Web Application Starting =====");
  Log.Information("Web Server URL: {WebUrl}", webUrl);
  Log.Information("Expected API URL: {ApiUrl}", apiBaseUrl);
  Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
  
  // Test API connectivity
  try
  {
    var httpClientFactory = app.Services.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("API");
    var response = await httpClient.GetAsync("/api/configuration/components", HttpCompletionOption.ResponseHeadersRead);
    if (response.IsSuccessStatusCode)
    {
      Log.Information("✓ API Connection: SUCCESS - API is reachable at {ApiUrl}", apiBaseUrl);
    }
    else
    {
      Log.Warning("⚠ API Connection: API responded with status {StatusCode}", response.StatusCode);
    }
  }
  catch (Exception ex)
  {
    Log.Warning("✗ API Connection: FAILED - Could not reach API at {ApiUrl}. Error: {Error}", apiBaseUrl, ex.Message);
    Log.Warning("  The Web UI will function with limited features. Please ensure the API is running.");
  }
  Log.Information("==================================================");

  // Configure the HTTP request pipeline.
  if (!app.Environment.IsDevelopment())
  {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
  }

  app.UseHttpsRedirection();

  app.UseStaticFiles();
  app.UseAntiforgery();

  app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

  // Map SignalR hub for visualizer
  app.MapHub<RadioConsole.Web.Hubs.VisualizerHub>("/visualizerhub");

  Log.Information("Radio Console Web Application started successfully at {WebUrl}", webUrl);
  app.Run();
}
catch (Exception ex)
{
  Log.Fatal(ex, "Web application terminated unexpectedly");
}
finally
{
  Log.CloseAndFlush();
}

