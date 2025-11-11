namespace RadioConsole.Api.Interfaces;

/// <summary>
/// Type of audio input
/// </summary>
public enum AudioInputType
{
    /// <summary>
    /// Music/streaming input (radio, spotify, etc.)
    /// </summary>
    Music,
    
    /// <summary>
    /// Event-driven input (doorbell, timer, etc.)
    /// </summary>
    Event
}

/// <summary>
/// Represents the priority level of an audio input
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
/// Event arguments for audio data available events
/// </summary>
public class AudioDataAvailableEventArgs : EventArgs
{
    /// <summary>
    /// PCM audio buffer data
    /// </summary>
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Sample rate of the audio data
    /// </summary>
    public int SampleRate { get; set; }
    
    /// <summary>
    /// Number of channels (1 = mono, 2 = stereo)
    /// </summary>
    public int Channels { get; set; }
    
    /// <summary>
    /// Bits per sample (typically 16)
    /// </summary>
    public int BitsPerSample { get; set; }
    
    /// <summary>
    /// Timestamp when the audio data was generated
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Base interface for all audio input sources
/// </summary>
public interface IAudioInput : IAudioDevice
{
    /// <summary>
    /// Type of this audio input
    /// </summary>
    AudioInputType InputType { get; }

    /// <summary>
    /// Priority level of this input (for event-based inputs)
    /// </summary>
    EventPriority Priority { get; }
    
    /// <summary>
    /// Duration of the audio in seconds (null for indefinite)
    /// </summary>
    TimeSpan? Duration { get; }
    
    /// <summary>
    /// Event triggered when PCM audio data is available
    /// </summary>
    event EventHandler<AudioDataAvailableEventArgs>? AudioDataAvailable;

    /// <summary>
    /// Pause the audio stream
    /// </summary>
    Task PauseAsync();

    /// <summary>
    /// Resume the audio stream if paused
    /// </summary>
    Task ResumeAsync();

    /// <summary>
    /// Set the preferred volume of the audio stream (0.0 to 1.0)
    /// </summary>
    Task SetVolumeAsync(double volume);

    /// <summary>
    /// Set repeat mode. When the stream is complete, replay the audio from the beginning up to N times.
    /// If repeatCount is 0, repeat forever.
    /// </summary>
    /// <param name="repeatCount">Number of times to repeat (0 = infinite)</param>
    void SetRepeat(int repeatCount);

    /// <summary>
    /// Gets or sets whether this stream can play concurrently with other active streams
    /// </summary>
    bool AllowConcurrent { get; set; }

    /// <summary>
    /// Get the current audio stream (legacy method - prefer using AudioDataAvailable event)
    /// </summary>
    Task<Stream?> GetAudioStreamAsync();
}
