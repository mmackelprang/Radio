using RadioConsole.Infrastructure.Configuration;
using RadioConsole.Web.Components;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
  .MinimumLevel.Information()
  .WriteTo.Console()
  .WriteTo.File("logs/web-.log", rollingInterval: RollingInterval.Day)
  .CreateLogger();

try
{
  Log.Information("Starting Radio Console Web Application");

  var builder = WebApplication.CreateBuilder(args);

  // Add Serilog to the app
  builder.Host.UseSerilog();

  // Add configuration service with settings from appsettings.json
  builder.Services.AddConfigurationService(builder.Configuration);

  // Add services to the container.
  builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

  var app = builder.Build();

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

  Log.Information("Radio Console Web Application started successfully");
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

