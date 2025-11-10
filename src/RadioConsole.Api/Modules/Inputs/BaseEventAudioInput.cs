using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Modules.Inputs;

/// <summary>
/// Base implementation for event-driven audio inputs
/// </summary>
public abstract class BaseEventAudioInput : BaseAudioInput
{
    public override AudioInputType InputType => AudioInputType.Event;

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
            
            // Fire audio data available event
            // In a real implementation, this would read from the stream and fire events with PCM data
            if (stream != null)
            {
                _display.UpdateStatus($"Event triggered at {DateTime.UtcNow:HH:mm:ss}");
                
                // For now, we'll just track that the event was triggered
                // Real PCM streaming would be implemented in concrete classes
            }
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
