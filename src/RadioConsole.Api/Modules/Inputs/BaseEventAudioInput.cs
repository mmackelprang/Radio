using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Modules.Inputs;

/// <summary>
/// Base implementation for event-driven audio inputs
/// </summary>
public abstract class BaseEventAudioInput : BaseAudioInput, IEventAudioInput
{
    public override AudioInputType InputType => AudioInputType.Event;
    
    public abstract EventPriority Priority { get; }
    public abstract TimeSpan? Duration { get; }

    public event EventHandler<EventAudioEventArgs>? AudioEventTriggered;

    protected BaseEventAudioInput(IEnvironmentService environmentService, IStorage storage) 
        : base(environmentService, storage)
    {
    }

    /// <summary>
    /// Trigger an audio event
    /// </summary>
    protected virtual async Task TriggerAudioEventAsync(Dictionary<string, string>? metadata = null)
    {
        try
        {
            var stream = await GetAudioStreamAsync();
            
            var eventArgs = new EventAudioEventArgs
            {
                EventInput = this,
                AudioStream = stream,
                Timestamp = DateTime.UtcNow,
                Metadata = metadata ?? new Dictionary<string, string>()
            };

            AudioEventTriggered?.Invoke(this, eventArgs);
            
            _display.UpdateStatus($"Event triggered at {eventArgs.Timestamp:HH:mm:ss}");
        }
        catch (Exception ex)
        {
            _display.UpdateStatus($"Error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Simulate triggering an event (for testing and simulation mode)
    /// </summary>
    public async Task SimulateTriggerAsync(Dictionary<string, string>? metadata = null)
    {
        if (!IsAvailable)
        {
            throw new InvalidOperationException($"Event input {Name} is not available");
        }

        await TriggerAudioEventAsync(metadata);
    }
}
