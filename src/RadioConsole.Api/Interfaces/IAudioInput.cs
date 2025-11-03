namespace RadioConsole.Api.Interfaces;

/// <summary>
/// Base interface for all audio input sources
/// </summary>
public interface IAudioInput
{
    /// <summary>
    /// Unique identifier for this input
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Display name for this input
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Description of this input source
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Whether this input is currently available
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Whether this input is currently active
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Initialize the input source
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Start playing audio from this input
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Stop playing audio from this input
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Get the current audio stream
    /// </summary>
    Task<Stream?> GetAudioStreamAsync();

    /// <summary>
    /// Get the configuration interface for this input
    /// </summary>
    IDeviceConfiguration GetConfiguration();

    /// <summary>
    /// Get the display interface for this input
    /// </summary>
    IDisplay GetDisplay();
}
