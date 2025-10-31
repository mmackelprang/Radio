namespace RadioConsole.Interfaces;

/// <summary>
/// Base interface for all audio output devices
/// </summary>
public interface IAudioOutput
{
    /// <summary>
    /// Unique identifier for this output
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Display name for this output
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Description of this output device
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Whether this output is currently available
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Whether this output is currently active
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Initialize the output device
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Start playing audio to this output
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Stop playing audio to this output
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Send audio data to this output
    /// </summary>
    Task SendAudioAsync(Stream audioStream);

    /// <summary>
    /// Set the volume level (0.0 to 1.0)
    /// </summary>
    Task SetVolumeAsync(double volume);

    /// <summary>
    /// Get the configuration interface for this output
    /// </summary>
    IConfiguration GetConfiguration();

    /// <summary>
    /// Get the display interface for this output
    /// </summary>
    IDisplay GetDisplay();
}
