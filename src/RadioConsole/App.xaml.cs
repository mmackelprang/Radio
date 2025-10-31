using RadioConsole.Services;
using RadioConsole.ViewModels;
using RadioConsole.Views;

namespace RadioConsole;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}
