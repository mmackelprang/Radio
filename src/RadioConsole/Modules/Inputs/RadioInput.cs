using RadioConsole.Interfaces;

namespace RadioConsole.Modules.Inputs;

/// <summary>
/// Radio input module (Raddy RF320)
/// </summary>
public class RadioInput : BaseAudioInput
{
    public override string Id => "radio";
    public override string Name => "Radio";
    public override string Description => "SW/AM/FM Radio (Raddy RF320)";

    public RadioInput(IEnvironmentService environmentService, IStorage storage) 
        : base(environmentService, storage)
    {
    }

    public override async Task InitializeAsync()
    {
        await _configuration.LoadAsync();
        
        if (_environmentService.IsSimulationMode)
        {
            // Simulation mode - radio is available but mocked
            IsAvailable = true;
            _display.UpdateStatus("Radio (Simulation Mode)");
        }
        else
        {
            // Check for actual radio hardware
            IsAvailable = await CheckRadioHardwareAsync();
            _display.UpdateStatus(IsAvailable ? "Radio Ready" : "Radio Not Found");
        }
    }

    public override async Task StartAsync()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Radio is not available");

        IsActive = true;
        _display.UpdateStatus("Playing");
        
        if (_environmentService.IsSimulationMode)
        {
            // Simulate radio metadata
            _display.UpdateMetadata(new Dictionary<string, string>
            {
                ["Station"] = "FM 101.5",
                ["Signal"] = "Strong",
                ["Band"] = "FM"
            });
        }
        
        await Task.CompletedTask;
    }

    public override Task StopAsync()
    {
        IsActive = false;
        _display.UpdateStatus("Stopped");
        return Task.CompletedTask;
    }

    public override Task<Stream?> GetAudioStreamAsync()
    {
        if (!IsActive)
            return Task.FromResult<Stream?>(null);

        // In simulation mode, return a mock stream
        if (_environmentService.IsSimulationMode)
        {
            return Task.FromResult<Stream?>(new MemoryStream());
        }

        // TODO: Implement actual radio audio stream
        return Task.FromResult<Stream?>(null);
    }

    private Task<bool> CheckRadioHardwareAsync()
    {
        // TODO: Implement actual hardware detection
        return Task.FromResult(false);
    }
}
