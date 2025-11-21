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

  // Parse command line arguments for port and API configuration
  // Priority: 1) Command line args, 2) Config file, 3) Hardcoded defaults
  int? webPortFromArgs = null;
  string? apiBaseUrlFromArgs = null;
  
  for (int i = 0; i < args.Length; i++)
  {
    if ((args[i] == "--port" || args[i] == "--listening-port") && i + 1 < args.Length)
    {
      if (int.TryParse(args[i + 1], out var port))
      {
        webPortFromArgs = port;
        Log.Information("Web UI Port set via command line: {Port}", webPortFromArgs);
      }
      i++;
    }
    else if ((args[i] == "--api-url" || args[i] == "--api-server") && i + 1 < args.Length)
    {
      apiBaseUrlFromArgs = args[i + 1];
      // Ensure the URL has a scheme
      if (!apiBaseUrlFromArgs.StartsWith("http://") && !apiBaseUrlFromArgs.StartsWith("https://"))
      {
        apiBaseUrlFromArgs = "http://" + apiBaseUrlFromArgs;
      }
      Log.Information("API Base URL set via command line: {ApiUrl}", apiBaseUrlFromArgs);
      i++;
    }
  }
  
  // Read port from config if available (before we potentially clear it)
  int? webPortFromConfig = null;
  var kestrelUrl = builder.Configuration["Kestrel:Endpoints:Http:Url"];
  if (!string.IsNullOrEmpty(kestrelUrl))
  {
    try
    {
      var uri = new Uri(kestrelUrl);
      webPortFromConfig = uri.Port;
    }
    catch
    {
      // Invalid URL in config, ignore
    }
  }
  
  // Determine final web port using priority order
  int webPort;
  if (webPortFromArgs.HasValue)
  {
    // Priority 1: Command line argument
    webPort = webPortFromArgs.Value;
  }
  else if (webPortFromConfig.HasValue)
  {
    // Priority 2: Config file
    webPort = webPortFromConfig.Value;
    Log.Information("Web UI Port set from configuration: {Port}", webPort);
  }
  else
  {
    // Priority 3: Hardcoded default
    webPort = 5200;
    Log.Information("Web UI Port set to default: {Port}", webPort);
  }
  
  // Determine API base URL using priority order
  string apiBaseUrl;
  if (!string.IsNullOrEmpty(apiBaseUrlFromArgs))
  {
    // Priority 1: Command line argument
    apiBaseUrl = apiBaseUrlFromArgs;
  }
  else if (!string.IsNullOrEmpty(builder.Configuration["RadioConsole:ApiBaseUrl"]))
  {
    // Priority 2: Config file
    apiBaseUrl = builder.Configuration["RadioConsole:ApiBaseUrl"]!;
    Log.Information("API Base URL set from configuration: {ApiUrl}", apiBaseUrl);
  }
  else
  {
    // Priority 3: Hardcoded default
    apiBaseUrl = "http://localhost:5100";
    Log.Information("API Base URL set to default: {ApiUrl}", apiBaseUrl);
  }
  
  // Override any URL configuration with the determined port
  // Configure Kestrel to listen on the specified port and ignore appsettings.json configuration
  builder.WebHost.ConfigureKestrel((context, serverOptions) =>
  {
    // Clear all endpoints to avoid conflicts
    serverOptions.ConfigurationLoader = null;
    serverOptions.ListenAnyIP(webPort);
  });

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
      client.BaseAddress = new Uri(apiBaseUrl);
      client.Timeout = TimeSpan.FromSeconds(5); // Set a reasonable timeout for health checks
  });

  // Add SignalR for real-time visualizer data
  builder.Services.AddSignalR();

  // Register visualization service (SignalR implementation)
  builder.Services.AddSingleton<IVisualizationService, SignalRVisualizationService>();

  // Add services to the container.
  builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

  var app = builder.Build();

  // Determine the actual listening URLs
  var displayWebUrl = $"http://localhost:{webPort}";
  
  // Log server configuration with prominent info block
  Log.Information("╔═══════════════════════════════════════════════════════════╗");
  Log.Information("║     Radio Console Web UI - Starting Up                   ║");
  Log.Information("╠═══════════════════════════════════════════════════════════╣");
  Log.Information("║  Current Listening Port: {Port,-35} ║", webPort);
  Log.Information("║  Web UI URL:             {Url,-35} ║", displayWebUrl);
  Log.Information("║  Environment:            {Env,-35} ║", app.Environment.EnvironmentName);
  Log.Information("╠═══════════════════════════════════════════════════════════╣");
  Log.Information("║  Required External Ports:                                 ║");
  Log.Information("║    - API Server:         {ApiUrl,-35} ║", apiBaseUrl);
  Log.Information("╚═══════════════════════════════════════════════════════════╝");
  
  // Test API connectivity with health check
  bool apiHealthy = false;
  try
  {
    Log.Information("Checking API health at {ApiUrl}/health...", apiBaseUrl);
    var httpClientFactory = app.Services.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("API");
    var response = await httpClient.GetAsync("/health");
    
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      if (content.Contains("OK"))
      {
        apiHealthy = true;
        Log.Information("✓ API Health Check: SUCCESS - API is healthy and responding");
      }
      else
      {
        Log.Error("✗ API Health Check: FAILED - API responded but health check did not return OK");
        Log.Error("  Response: {Content}", content);
      }
    }
    else
    {
      Log.Error("✗ API Health Check: FAILED - API responded with status {StatusCode}", response.StatusCode);
    }
  }
  catch (Exception ex)
  {
    Log.Error("✗ API Health Check: FAILED - Could not reach API at {ApiUrl}/health", apiBaseUrl);
    Log.Error("  Error: {ErrorType}: {ErrorMessage}", ex.GetType().Name, ex.Message);
  }
  
  // If API health check failed, log error and exit gracefully
  if (!apiHealthy)
  {
    Log.Error("╔═══════════════════════════════════════════════════════════╗");
    Log.Error("║  FATAL ERROR: API Health Check Failed                    ║");
    Log.Error("╠═══════════════════════════════════════════════════════════╣");
    Log.Error("║  The Web UI requires a healthy API connection to run.    ║");
    Log.Error("║  Please ensure the API is running at:                    ║");
    Log.Error("║    {ApiUrl,-54} ║", apiBaseUrl);
    Log.Error("║                                                           ║");
    Log.Error("║  To start the API, run:                                  ║");
    Log.Error("║    dotnet run --project RadioConsole.API                 ║");
    Log.Error("║                                                           ║");
    Log.Error("║  Or use the provided startup scripts in the scripts/     ║");
    Log.Error("║  directory to start both services with correct ports.    ║");
    Log.Error("╚═══════════════════════════════════════════════════════════╝");
    Log.CloseAndFlush();
    Environment.Exit(1);
    return; // This line won't be reached but helps with code analysis
  }
  
  Log.Information("API health check passed. Continuing with Web UI startup...");

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

  Log.Information("Radio Console Web Application started successfully at {WebUrl}", displayWebUrl);
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

