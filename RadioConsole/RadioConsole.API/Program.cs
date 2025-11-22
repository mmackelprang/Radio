using RadioConsole.Infrastructure.Configuration;
using RadioConsole.Infrastructure.Audio;
using RadioConsole.Infrastructure.Inputs;
using RadioConsole.API.Services;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
  .MinimumLevel.Information()
  .WriteTo.Console()
  .WriteTo.File("./logs/api-.log", rollingInterval: RollingInterval.Day)
  .CreateLogger();

try
{
  Log.Information("Starting Radio Console API");

  var builder = WebApplication.CreateBuilder(args);

  // Parse command line arguments for port configuration
  // Priority: 1) Command line args, 2) Config file, 3) Hardcoded defaults
  int? apiPortFromArgs = null;
  
  for (int i = 0; i < args.Length; i++)
  {
    if ((args[i] == "--port" || args[i] == "--listening-port") && i + 1 < args.Length)
    {
      if (int.TryParse(args[i + 1], out var port))
      {
        apiPortFromArgs = port;
        Log.Information("API Port set via command line: {Port}", apiPortFromArgs);
      }
      i++;
    }
  }
  
  // Read port from config if available (before we potentially clear it)
  int? apiPortFromConfig = null;
  var kestrelUrl = builder.Configuration["Kestrel:Endpoints:Http:Url"];
  if (!string.IsNullOrEmpty(kestrelUrl))
  {
    try
    {
      var uri = new Uri(kestrelUrl);
      apiPortFromConfig = uri.Port;
    }
    catch
    {
      // Invalid URL in config, ignore
    }
  }
  
  // Determine final API port using priority order
  int apiPort;
  if (apiPortFromArgs.HasValue)
  {
    // Priority 1: Command line argument
    apiPort = apiPortFromArgs.Value;
  }
  else if (apiPortFromConfig.HasValue)
  {
    // Priority 2: Config file
    apiPort = apiPortFromConfig.Value;
    Log.Information("API Port set from configuration: {Port}", apiPort);
  }
  else
  {
    // Priority 3: Hardcoded default
    apiPort = 5100;
    Log.Information("API Port set to default: {Port}", apiPort);
  }
  
  // Swagger runs on the same port as the API
  int swaggerPort = apiPort;
  
  // Override any URL configuration with the determined port
  // Configure Kestrel to listen on the specified port and ignore appsettings.json configuration
  builder.WebHost.ConfigureKestrel((context, serverOptions) =>
  {
    // Clear all endpoints to avoid conflicts
    serverOptions.ConfigurationLoader = null;
    serverOptions.ListenAnyIP(apiPort);
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
  builder.Services.AddSingleton<StreamAudioService>();

  // Add input services (Raddy Radio, Spotify, Broadcast Receiver)
  builder.Services.AddInputServices();

  // Add controllers
  builder.Services.AddControllers();

  // Add services to the container.
  // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
  builder.Services.AddEndpointsApiExplorer();
  builder.Services.AddSwaggerGen();

  var app = builder.Build();

  // Determine the actual listening URLs
  var listeningUrl = $"http://0.0.0.0:{apiPort}";
  var displayUrl = $"http://localhost:{apiPort}";
  
  // Log server configuration with prominent info block
  Log.Information("╔═══════════════════════════════════════════════════════════╗");
  Log.Information("║       Radio Console API - Starting Up                     ║");
  Log.Information("╠═══════════════════════════════════════════════════════════╣");
  Log.Information("║  Current Listening Port: {Port,-35} ║", apiPort);
  Log.Information("║  API Base URL:           {Url,-35} ║", displayUrl);
  Log.Information("║  Swagger UI:             {Url,-35} ║", $"{displayUrl}/swagger");
  Log.Information("║  Environment:            {Env,-35} ║", app.Environment.EnvironmentName);
  Log.Information("╠═══════════════════════════════════════════════════════════╣");
  Log.Information("║  Required External Ports:                                 ║");
  Log.Information("║    - None (API is standalone)                             ║");
  Log.Information("╚═══════════════════════════════════════════════════════════╝");

  // Configure the HTTP request pipeline.
  if (app.Environment.IsDevelopment())
  {
    app.UseSwagger();
    app.UseSwaggerUI();
  }

  app.UseHttpsRedirection();

  // Map controllers
  app.MapControllers();

  app.Run();
}
catch (Exception ex)
{
  Log.Fatal(ex, "API terminated unexpectedly");
}
finally
{
  Log.CloseAndFlush();
}

