using RadioConsole.ViewModels;

namespace RadioConsole.Views;

public partial class FavoritesPage : ContentPage
{
    public FavoritesPage()
    {
        InitializeComponent();
        BindingContext = new FavoritesViewModel();
    }
}
