using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Modules.Inputs;

/// <summary>
/// Event audio input for telephone ringing
/// </summary>
public class TelephoneRingingEventInput : BaseEventAudioInput
{
    public override string Id => "telephone_event";
    public override string Name => "Telephone Ring";
    public override string Description => "Telephone ringing notification";
    public override EventPriority Priority => EventPriority.High;
    public override TimeSpan? Duration => TimeSpan.FromSeconds(5);

    public TelephoneRingingEventInput(IEnvironmentService environmentService, IStorage storage)
        : base(environmentService, storage)
    {
    }

    public override async Task InitializeAsync()
    {
        await _configuration.LoadAsync();
        
        IsAvailable = _environmentService.IsSimulationMode;
        _display.UpdateStatus(IsAvailable 
            ? "Telephone Event (Simulation Mode)" 
            : "Telephone Not Connected");
    }

    public override async Task StartAsync()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Telephone event input is not available");

        IsActive = true;
        _display.UpdateStatus("Playing telephone ring");
        
        if (_environmentService.IsSimulationMode)
        {
            _display.UpdateMetadata(new Dictionary<string, string>
            {
                ["Event"] = "Telephone Ring",
                ["Caller"] = "Unknown",
                ["Time"] = DateTime.Now.ToString("HH:mm:ss")
            });
        }
        
        await Task.CompletedTask;
    }

    public override Task StopAsync()
    {
        IsActive = false;
        _display.UpdateStatus("Telephone ring stopped");
        return Task.CompletedTask;
    }

    public override Task<Stream?> GetAudioStreamAsync()
    {
        return Task.FromResult<Stream?>(_environmentService.IsSimulationMode 
            ? new MemoryStream() 
            : null);
    }
}
