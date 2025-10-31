using RadioConsole.ViewModels;

namespace RadioConsole.Views;

public partial class HistoryPage : ContentPage
{
    public HistoryPage()
    {
        InitializeComponent();
        BindingContext = new HistoryViewModel();
    }
}
