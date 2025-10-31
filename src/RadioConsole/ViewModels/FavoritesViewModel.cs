using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace RadioConsole.ViewModels;

public partial class FavoritesViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<FavoriteItem> _favoriteItems = new();

    public FavoritesViewModel()
    {
        // Load favorite items
        LoadFavorites();
    }

    private void LoadFavorites()
    {
        // TODO: Load from storage
        // For now, add some placeholder items
        FavoriteItems.Add(new FavoriteItem 
        { 
            Title = "FM 101.5 - Classic Rock",
            Source = "Radio",
            Details = "88.0 - 108.0 MHz"
        });
        FavoriteItems.Add(new FavoriteItem 
        { 
            Title = "My Daily Mix",
            Source = "Spotify",
            Details = "Playlist"
        });
    }
}

public class FavoriteItem
{
    public string Title { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}
