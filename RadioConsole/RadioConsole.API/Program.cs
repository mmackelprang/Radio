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

  // Log server configuration
  var serverUrl = builder.Configuration["Kestrel:Endpoints:Http:Url"] ?? "http://localhost:5100";
  Log.Information("===== Radio Console API Starting =====");
  Log.Information("API Server URL: {ServerUrl}", serverUrl);
  Log.Information("Swagger UI: {SwaggerUrl}/swagger", serverUrl);
  Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
  Log.Information("======================================");

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

