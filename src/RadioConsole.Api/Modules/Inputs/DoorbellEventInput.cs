using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Modules.Inputs;

/// <summary>
/// Event audio input for doorbell ringing
/// </summary>
public class DoorbellEventInput : BaseEventAudioInput
{
    public override string Id => "doorbell_event";
    public override string Name => "Doorbell Ring";
    public override string Description => "Doorbell ringing notification";
    public override EventPriority Priority => EventPriority.High;
    public override TimeSpan? Duration => TimeSpan.FromSeconds(3);

    public DoorbellEventInput(IEnvironmentService environmentService, IStorage storage)
        : base(environmentService, storage)
    {
    }

    public override async Task InitializeAsync()
    {
        await _configuration.LoadAsync();
        
        if (_environmentService.IsSimulationMode)
        {
            IsAvailable = true;
            _display.UpdateStatus("Doorbell Event (Simulation Mode)");
        }
        else
        {
            // Check for actual doorbell integration (e.g., Wyze API, webhook endpoint)
            IsAvailable = await CheckDoorbellIntegrationAsync();
            _display.UpdateStatus(IsAvailable ? "Doorbell Ready" : "Doorbell Not Connected");
        }
    }

    public override async Task StartAsync()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Doorbell event input is not available");

        IsActive = true;
        _display.UpdateStatus("Playing doorbell chime");
        
        if (_environmentService.IsSimulationMode)
        {
            _display.UpdateMetadata(new Dictionary<string, string>
            {
                ["Event"] = "Doorbell Ring",
                ["Location"] = "Front Door",
                ["Time"] = DateTime.Now.ToString("HH:mm:ss")
            });
        }
        
        await Task.CompletedTask;
    }

    public override Task StopAsync()
    {
        IsActive = false;
        _display.UpdateStatus("Doorbell chime completed");
        return Task.CompletedTask;
    }

    public override Task<Stream?> GetAudioStreamAsync()
    {
        if (_environmentService.IsSimulationMode)
        {
            // Return a mock stream for simulation
            return Task.FromResult<Stream?>(new MemoryStream());
        }

        // TODO: Return actual doorbell audio file or stream
        // This could be a pre-recorded chime or live audio from the doorbell camera
        return Task.FromResult<Stream?>(null);
    }

    private Task<bool> CheckDoorbellIntegrationAsync()
    {
        // TODO: Check for actual doorbell integration (Wyze API, webhook, etc.)
        return Task.FromResult(false);
    }

    /// <summary>
    /// Simulate a doorbell ring event
    /// </summary>
    public async Task SimulateDoorbellRingAsync(string location = "Front Door")
    {
        var metadata = new Dictionary<string, string>
        {
            ["Event"] = "Doorbell Ring",
            ["Location"] = location,
            ["Time"] = DateTime.Now.ToString("HH:mm:ss")
        };

        await TriggerAudioEventAsync(metadata);
    }
}
