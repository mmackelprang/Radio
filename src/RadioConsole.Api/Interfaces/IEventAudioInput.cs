namespace RadioConsole.Api.Interfaces;

/// <summary>
/// Represents the priority level of an event audio input
/// </summary>
public enum EventPriority
{
    /// <summary>
    /// Low priority - informational events
    /// </summary>
    Low,
    
    /// <summary>
    /// Medium priority - standard notifications
    /// </summary>
    Medium,
    
    /// <summary>
    /// High priority - urgent notifications (doorbell, phone ring)
    /// </summary>
    High,
    
    /// <summary>
    /// Critical priority - emergency alerts
    /// </summary>
    Critical
}

/// <summary>
/// Interface for event-based audio inputs (doorbell, timer, etc.)
/// </summary>
public interface IEventAudioInput : IAudioInput
{
    /// <summary>
    /// Priority level of this event
    /// </summary>
    EventPriority Priority { get; }
    
    /// <summary>
    /// Duration of the event audio in seconds (null for indefinite)
    /// </summary>
    TimeSpan? Duration { get; }
    
    /// <summary>
    /// Event triggered when a new audio event occurs
    /// </summary>
    event EventHandler<EventAudioEventArgs>? AudioEventTriggered;
}

/// <summary>
/// Event arguments for audio events
/// </summary>
public class EventAudioEventArgs : EventArgs
{
    /// <summary>
    /// The event input that triggered the audio
    /// </summary>
    public IEventAudioInput EventInput { get; set; } = null!;
    
    /// <summary>
    /// Audio stream for the event
    /// </summary>
    public Stream? AudioStream { get; set; }
    
    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Additional metadata about the event
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}
