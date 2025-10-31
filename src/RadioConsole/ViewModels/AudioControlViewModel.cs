using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RadioConsole.Interfaces;
using RadioConsole.Services;
using RadioConsole.Modules.Inputs;
using RadioConsole.Modules.Outputs;
using System.Collections.ObjectModel;

namespace RadioConsole.ViewModels;

public partial class AudioControlViewModel : ObservableObject
{
    private readonly IEnvironmentService _environmentService;
    private readonly IStorage _storage;
    
    [ObservableProperty]
    private ObservableCollection<IAudioInput> _audioInputs = new();
    
    [ObservableProperty]
    private ObservableCollection<IAudioOutput> _audioOutputs = new();
    
    [ObservableProperty]
    private IAudioInput? _selectedInput;
    
    [ObservableProperty]
    private IAudioOutput? _selectedOutput;
    
    [ObservableProperty]
    private string _statusMessage = "Ready";
    
    [ObservableProperty]
    private bool _isPlaying = false;
    
    [ObservableProperty]
    private double _volume = 0.5;

    public AudioControlViewModel(IEnvironmentService environmentService, IStorage storage)
    {
        _environmentService = environmentService;
        _storage = storage;
    }

    public async Task InitializeAsync()
    {
        // Initialize inputs
        var radioInput = new RadioInput(_environmentService, _storage);
        var spotifyInput = new SpotifyInput(_environmentService, _storage);
        
        await radioInput.InitializeAsync();
        await spotifyInput.InitializeAsync();
        
        AudioInputs.Add(radioInput);
        AudioInputs.Add(spotifyInput);
        
        // Initialize outputs
        var soundbarOutput = new WiredSoundbarOutput(_environmentService, _storage);
        var chromecastOutput = new ChromecastOutput(_environmentService, _storage);
        
        await soundbarOutput.InitializeAsync();
        await chromecastOutput.InitializeAsync();
        
        AudioOutputs.Add(soundbarOutput);
        AudioOutputs.Add(chromecastOutput);
        
        // Select first available input and output
        SelectedInput = AudioInputs.FirstOrDefault(i => i.IsAvailable);
        SelectedOutput = AudioOutputs.FirstOrDefault(o => o.IsAvailable);
        
        UpdateStatusMessage();
    }

    [RelayCommand]
    private async Task PlayAsync()
    {
        if (SelectedInput == null || SelectedOutput == null)
        {
            StatusMessage = "Please select input and output";
            return;
        }

        try
        {
            await SelectedOutput.StartAsync();
            await SelectedInput.StartAsync();
            IsPlaying = true;
            StatusMessage = $"Playing {SelectedInput.Name} through {SelectedOutput.Name}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task StopAsync()
    {
        if (SelectedInput != null)
            await SelectedInput.StopAsync();
        
        if (SelectedOutput != null)
            await SelectedOutput.StopAsync();
        
        IsPlaying = false;
        StatusMessage = "Stopped";
    }

    partial void OnVolumeChanged(double value)
    {
        if (SelectedOutput != null)
        {
            _ = SelectedOutput.SetVolumeAsync(value);
        }
    }

    partial void OnSelectedInputChanged(IAudioInput? value)
    {
        UpdateStatusMessage();
    }

    partial void OnSelectedOutputChanged(IAudioOutput? value)
    {
        UpdateStatusMessage();
    }

    private void UpdateStatusMessage()
    {
        if (_environmentService.IsSimulationMode)
        {
            StatusMessage = $"Simulation Mode - {_environmentService.PlatformDescription}";
        }
        else
        {
            StatusMessage = $"Running on {_environmentService.PlatformDescription}";
        }
    }
}
