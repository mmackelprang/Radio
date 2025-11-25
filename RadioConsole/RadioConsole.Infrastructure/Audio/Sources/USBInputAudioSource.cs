using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio.Sources;

/// <summary>
/// Audio source for USB audio input devices (Raddy RF320 radio, vinyl phonograph, etc.).
/// Captures live audio from a USB audio device.
/// </summary>
public class USBInputAudioSource : SoundFlowAudioSourceBase
{
  private readonly string _deviceId;
  private readonly string _deviceName;

  /// <inheritdoc/>
  public override AudioSourceType SourceType => AudioSourceType.USBRadio;

  /// <summary>
  /// Gets the USB device identifier.
  /// </summary>
  public string DeviceId => _deviceId;

  /// <summary>
  /// Gets the USB device name.
  /// </summary>
  public string DeviceName => _deviceName;

  /// <summary>
  /// Creates a new USB input audio source.
  /// </summary>
  /// <param name="id">Unique identifier.</param>
  /// <param name="deviceId">USB device identifier.</param>
  /// <param name="deviceName">Human-readable device name.</param>
  /// <param name="channel">Target mixer channel.</param>
  /// <param name="logger">Logger instance.</param>
  public USBInputAudioSource(
    string id,
    string deviceId,
    string deviceName,
    MixerChannel channel,
    ILogger<USBInputAudioSource> logger)
    : base(id, deviceName, channel, logger)
  {
    _deviceId = deviceId ?? throw new ArgumentNullException(nameof(deviceId));
    _deviceName = deviceName ?? throw new ArgumentNullException(nameof(deviceName));

    SetMetadata("DeviceId", deviceId);
    SetMetadata("DeviceName", deviceName);
    SetMetadata("SourceType", "USBInput");
  }

  /// <inheritdoc/>
  public override async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    if (_disposed)
    {
      throw new ObjectDisposedException(nameof(USBInputAudioSource));
    }

    _logger.LogInformation("Initializing USB input audio source: {DeviceName} (ID: {DeviceId})", _deviceName, _deviceId);

    try
    {
      // USB input source doesn't have a stream to initialize
      // The actual audio capture will be set up by the MixerService
      // using SoundFlow's capture device functionality

      Status = AudioSourceStatus.Ready;
      _logger.LogInformation("USB input audio source initialized: {DeviceName}", _deviceName);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to initialize USB input audio source: {DeviceName}", _deviceName);
      Status = AudioSourceStatus.Error;
      throw;
    }

    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public override Stream? GetAudioStream()
  {
    // USB input sources don't have a direct stream
    // Audio data comes from the capture device
    return null;
  }

  /// <inheritdoc/>
  public override void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _logger.LogDebug("Disposing USB input audio source: {DeviceName}", _deviceName);
    base.Dispose();
  }
}
