namespace RadioConsole;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes for navigation
        Routing.RegisterRoute(nameof(Views.AudioControlPage), typeof(Views.AudioControlPage));
        Routing.RegisterRoute(nameof(Views.HistoryPage), typeof(Views.HistoryPage));
        Routing.RegisterRoute(nameof(Views.FavoritesPage), typeof(Views.FavoritesPage));
    }
}
