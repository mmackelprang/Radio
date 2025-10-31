using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using RadioConsole.Services;
using RadioConsole.Interfaces;

namespace RadioConsole;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register services
        builder.Services.AddSingleton<IEnvironmentService, EnvironmentService>();
        builder.Services.AddSingleton<IStorage, JsonStorageService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
