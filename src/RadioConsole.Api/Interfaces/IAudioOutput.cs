namespace RadioConsole.Api.Interfaces;

/// <summary>
/// Base interface for all audio output devices
/// </summary>
public interface IAudioOutput : IAudioDevice
{
    /// <summary>
    /// Send audio data to this output
    /// </summary>
    Task SendAudioAsync(Stream audioStream);

    /// <summary>
    /// Set the volume level (0.0 to 1.0)
    /// </summary>
    Task SetVolumeAsync(double volume);
}
