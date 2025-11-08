using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Modules.Inputs;

/// <summary>
/// Event audio input for timer expired notifications
/// </summary>
public class TimerExpiredEventInput : BaseEventAudioInput
{
    public override string Id => "timer_event";
    public override string Name => "Timer Expired";
    public override string Description => "Timer expiration notification";
    public override EventPriority Priority => EventPriority.Medium;
    public override TimeSpan? Duration => TimeSpan.FromSeconds(3);

    public TimerExpiredEventInput(IEnvironmentService environmentService, IStorage storage)
        : base(environmentService, storage)
    {
    }

    public override async Task InitializeAsync()
    {
        await _configuration.LoadAsync();
        
        IsAvailable = _environmentService.IsSimulationMode;
        _display.UpdateStatus(IsAvailable 
            ? "Timer Event (Simulation Mode)" 
            : "Timer Not Connected");
    }

    public override async Task StartAsync()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Timer event input is not available");

        IsActive = true;
        _display.UpdateStatus("Playing timer notification");
        
        if (_environmentService.IsSimulationMode)
        {
            _display.UpdateMetadata(new Dictionary<string, string>
            {
                ["Event"] = "Timer Expired",
                ["Name"] = "Kitchen Timer",
                ["Time"] = DateTime.Now.ToString("HH:mm:ss")
            });
        }
        
        await Task.CompletedTask;
    }

    public override Task StopAsync()
    {
        IsActive = false;
        _display.UpdateStatus("Timer notification completed");
        return Task.CompletedTask;
    }

    public override Task<Stream?> GetAudioStreamAsync()
    {
        return Task.FromResult<Stream?>(_environmentService.IsSimulationMode 
            ? new MemoryStream() 
            : null);
    }
}
