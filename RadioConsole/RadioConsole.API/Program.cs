using RadioConsole.Infrastructure.Configuration;
using RadioConsole.Infrastructure.Audio;
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

  // Add Serilog to the app
  builder.Host.UseSerilog();

  // Add configuration service with settings from appsettings.json
  builder.Services.AddConfigurationService(builder.Configuration);

  // Add audio services
  builder.Services.AddAudioServices();
  builder.Services.AddSingleton<StreamAudioService>();

  // Add services to the container.
  // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
  builder.Services.AddEndpointsApiExplorer();
  builder.Services.AddSwaggerGen();

  var app = builder.Build();

  // Configure the HTTP request pipeline.
  if (app.Environment.IsDevelopment())
  {
    app.UseSwagger();
    app.UseSwaggerUI();
  }

  app.UseHttpsRedirection();

  // Audio streaming endpoints
  app.MapGet("/stream.mp3", async (HttpContext context, StreamAudioService streamService) =>
  {
    await streamService.StreamMp3Async(context);
  })
  .WithName("StreamAudioMp3")
  .WithOpenApi()
  .ExcludeFromDescription(); // Don't show in Swagger as it's a streaming endpoint

  app.MapGet("/stream.wav", async (HttpContext context, StreamAudioService streamService) =>
  {
    await streamService.StreamWavAsync(context);
  })
  .WithName("StreamAudioWav")
  .WithOpenApi()
  .ExcludeFromDescription(); // Don't show in Swagger as it's a streaming endpoint

  var summaries = new[]
  {
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
  };

  app.MapGet("/weatherforecast", () =>
  {
    Log.Information("Weather forecast requested");
    var forecast =  Enumerable.Range(1, 5).Select(index =>
      new WeatherForecast
      (
        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        Random.Shared.Next(-20, 55),
        summaries[Random.Shared.Next(summaries.Length)]
      ))
      .ToArray();
    return forecast;
  })
  .WithName("GetWeatherForecast")
  .WithOpenApi();

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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
  public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

