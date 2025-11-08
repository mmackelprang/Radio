using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Modules.Inputs;

/// <summary>
/// Event audio input for reminder notifications
/// </summary>
public class ReminderEventInput : BaseEventAudioInput
{
    public override string Id => "reminder_event";
    public override string Name => "Reminder";
    public override string Description => "Reminder notification";
    public override EventPriority Priority => EventPriority.Medium;
    public override TimeSpan? Duration => TimeSpan.FromSeconds(2);

    public ReminderEventInput(IEnvironmentService environmentService, IStorage storage)
        : base(environmentService, storage)
    {
    }

    public override async Task InitializeAsync()
    {
        await _configuration.LoadAsync();
        
        if (_environmentService.IsSimulationMode)
        {
            IsAvailable = true;
            _display.UpdateStatus("Reminder Event (Simulation Mode)");
        }
        else
        {
            // Check for actual reminder integration (e.g., Google Calendar, local scheduler)
            IsAvailable = await CheckReminderIntegrationAsync();
            _display.UpdateStatus(IsAvailable ? "Reminder Ready" : "Reminder Not Connected");
        }
    }

    public override async Task StartAsync()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Reminder event input is not available");

        IsActive = true;
        _display.UpdateStatus("Playing reminder notification");
        
        if (_environmentService.IsSimulationMode)
        {
            _display.UpdateMetadata(new Dictionary<string, string>
            {
                ["Event"] = "Reminder",
                ["Message"] = "Sample reminder",
                ["Time"] = DateTime.Now.ToString("HH:mm:ss")
            });
        }
        
        await Task.CompletedTask;
    }

    public override Task StopAsync()
    {
        IsActive = false;
        _display.UpdateStatus("Reminder notification completed");
        return Task.CompletedTask;
    }

    public override Task<Stream?> GetAudioStreamAsync()
    {
        if (_environmentService.IsSimulationMode)
        {
            // Return a mock stream for simulation
            return Task.FromResult<Stream?>(new MemoryStream());
        }

        // TODO: Return actual reminder audio (TTS or pre-recorded notification sound)
        return Task.FromResult<Stream?>(null);
    }

    private Task<bool> CheckReminderIntegrationAsync()
    {
        // TODO: Check for actual reminder integration
        return Task.FromResult(false);
    }

    /// <summary>
    /// Simulate a reminder event
    /// </summary>
    public async Task SimulateReminderAsync(string message, DateTime? dueTime = null)
    {
        var metadata = new Dictionary<string, string>
        {
            ["Event"] = "Reminder",
            ["Message"] = message,
            ["Time"] = (dueTime ?? DateTime.Now).ToString("HH:mm:ss")
        };

        await TriggerAudioEventAsync(metadata);
    }
}
