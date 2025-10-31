using RadioConsole.ViewModels;
using RadioConsole.Services;

namespace RadioConsole.Views;

public partial class AudioControlPage : ContentPage
{
    private readonly AudioControlViewModel _viewModel;

    public AudioControlPage()
    {
        InitializeComponent();
        
        // Initialize services and view model
        var environmentService = new EnvironmentService();
        var storage = new JsonStorageService();
        _viewModel = new AudioControlViewModel(environmentService, storage);
        
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
