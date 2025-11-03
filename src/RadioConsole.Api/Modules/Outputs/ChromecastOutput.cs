using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Modules.Outputs;

/// <summary>
/// Chromecast output module
/// </summary>
public class ChromecastOutput : BaseAudioOutput
{
    public override string Id => "chromecast";
    public override string Name => "Chromecast";
    public override string Description => "Chromecast Audio Streaming";

    public ChromecastOutput(IEnvironmentService environmentService, IStorage storage) 
        : base(environmentService, storage)
    {
    }

    public override async Task InitializeAsync()
    {
        await _configuration.LoadAsync();
        
        if (_environmentService.IsSimulationMode)
        {
            // Simulation mode - Chromecast is available but mocked
            IsAvailable = true;
            _display.UpdateStatus("Chromecast (Simulation Mode)");
        }
        else
        {
            // Check for Chromecast devices on network
            IsAvailable = await DiscoverChromecastAsync();
            _display.UpdateStatus(IsAvailable ? "Chromecast Found" : "No Chromecast Devices");
        }
    }

    public override Task StartAsync()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Chromecast is not available");

        IsActive = true;
        _display.UpdateStatus("Streaming");
        _display.UpdateMetadata(new Dictionary<string, string>
        {
            ["Volume"] = $"{(int)(_volume * 100)}%",
            ["Device"] = _environmentService.IsSimulationMode ? "Simulated Chromecast" : "Living Room"
        });
        
        return Task.CompletedTask;
    }

    public override Task StopAsync()
    {
        IsActive = false;
        _display.UpdateStatus("Disconnected");
        return Task.CompletedTask;
    }

    public override Task SendAudioAsync(Stream audioStream)
    {
        if (!IsActive)
            throw new InvalidOperationException("Chromecast is not active");

        // In simulation mode, just consume the stream
        if (_environmentService.IsSimulationMode)
        {
            return Task.CompletedTask;
        }

        // TODO: Implement actual Chromecast streaming
        return Task.CompletedTask;
    }

    public override async Task SetVolumeAsync(double volume)
    {
        await base.SetVolumeAsync(volume);
        
        _display.UpdateMetadata(new Dictionary<string, string>
        {
            ["Volume"] = $"{(int)(_volume * 100)}%",
            ["Device"] = _environmentService.IsSimulationMode ? "Simulated Chromecast" : "Living Room"
        });
    }

    private Task<bool> DiscoverChromecastAsync()
    {
        // TODO: Implement actual Chromecast discovery
        return Task.FromResult(false);
    }
}
