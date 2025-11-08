using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Modules.Inputs;

/// <summary>
/// Event audio input for Google Broadcast messages
/// </summary>
public class GoogleBroadcastEventInput : BaseEventAudioInput
{
    public override string Id => "google_broadcast_event";
    public override string Name => "Google Broadcast";
    public override string Description => "Google Home broadcast message";
    public override EventPriority Priority => EventPriority.Medium;
    public override TimeSpan? Duration => null; // Variable duration based on message length

    public GoogleBroadcastEventInput(IEnvironmentService environmentService, IStorage storage)
        : base(environmentService, storage)
    {
    }

    public override async Task InitializeAsync()
    {
        await _configuration.LoadAsync();
        
        IsAvailable = _environmentService.IsSimulationMode;
        _display.UpdateStatus(IsAvailable 
            ? "Google Broadcast (Simulation Mode)" 
            : "Google Broadcast Not Connected");
    }

    public override async Task StartAsync()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Google Broadcast event input is not available");

        IsActive = true;
        _display.UpdateStatus("Playing Google Broadcast");
        
        if (_environmentService.IsSimulationMode)
        {
            _display.UpdateMetadata(new Dictionary<string, string>
            {
                ["Event"] = "Google Broadcast",
                ["Message"] = "Sample broadcast message",
                ["Time"] = DateTime.Now.ToString("HH:mm:ss")
            });
        }
        
        await Task.CompletedTask;
    }

    public override Task StopAsync()
    {
        IsActive = false;
        _display.UpdateStatus("Google Broadcast completed");
        return Task.CompletedTask;
    }

    public override Task<Stream?> GetAudioStreamAsync()
    {
        return Task.FromResult<Stream?>(_environmentService.IsSimulationMode 
            ? new MemoryStream() 
            : null);
    }
}
