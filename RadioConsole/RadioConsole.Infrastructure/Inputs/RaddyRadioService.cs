using Microsoft.Extensions.Logging;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Core.Interfaces.Inputs;

namespace RadioConsole.Infrastructure.Inputs;

/// <summary>
/// Implementation of IRaddyRadioService for managing the Raddy RF320 radio.
/// Handles USB Audio device identification and routing to IAudioPlayer.
/// BLE control will be added in a future phase.
/// </summary>
public class RaddyRadioService : IRaddyRadioService
{
  private readonly IAudioDeviceManager _audioDeviceManager;
  private readonly IAudioPlayer _audioPlayer;
  private readonly ILogger<RaddyRadioService> _logger;
  
  private string? _deviceId;
  private bool _isStreaming;
  private int _signalStrength = 0; // 0-6 scale, placeholder for future BLE implementation
  private const string RaddyDeviceIdentifier = "Raddy"; // USB Audio device name contains "Raddy"
  
  public bool IsStreaming => _isStreaming;
  public bool IsDeviceDetected => _deviceId != null;
  public int SignalStrength => _signalStrength;

  public RaddyRadioService(
    IAudioDeviceManager audioDeviceManager,
    IAudioPlayer audioPlayer,
    ILogger<RaddyRadioService> logger)
  {
    _audioDeviceManager = audioDeviceManager ?? throw new ArgumentNullException(nameof(audioDeviceManager));
    _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <inheritdoc/>
  public async Task InitializeAsync()
  {
    _logger.LogInformation("Initializing Raddy RF320 radio service...");
    
    try
    {
      // Enumerate input devices and find the Raddy RF320
      var inputDevices = await _audioDeviceManager.GetInputDevicesAsync();
      var raddyDevice = inputDevices.FirstOrDefault(d => 
        d.Name.Contains(RaddyDeviceIdentifier, StringComparison.OrdinalIgnoreCase) ||
        d.DeviceType.Contains("USB", StringComparison.OrdinalIgnoreCase));

      if (raddyDevice != null)
      {
        _deviceId = raddyDevice.Id;
        _logger.LogInformation("Raddy RF320 USB Audio device detected: {DeviceId} - {DeviceName}", 
          raddyDevice.Id, raddyDevice.Name);
      }
      else
      {
        _logger.LogWarning("Raddy RF320 USB Audio device not found. Ensure the device is connected.");
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error initializing Raddy radio service");
      throw;
    }
  }

  /// <inheritdoc/>
  public async Task StartAsync()
  {
    if (_deviceId == null)
    {
      throw new InvalidOperationException("Raddy device not detected. Call InitializeAsync first.");
    }

    if (_isStreaming)
    {
      _logger.LogWarning("Raddy radio is already streaming.");
      return;
    }

    _logger.LogInformation("Starting Raddy RF320 audio stream from device: {DeviceId}", _deviceId);
    
    try
    {
      // Set the input device for the audio player
      await _audioDeviceManager.SetInputDeviceAsync(_deviceId);
      
      // Initialize audio player with the device if not already initialized
      if (!_audioPlayer.IsInitialized)
      {
        // Use the current output device or default
        var outputDevice = await _audioDeviceManager.GetCurrentOutputDeviceAsync();
        var outputId = outputDevice?.Id ?? "default";
        await _audioPlayer.InitializeAsync(outputId);
      }

      // The audio player now routes audio from the input device to the output device.
      // In a full implementation with hardware capture, this would start capturing
      // audio from the Raddy RF320's USB audio input and routing it through the
      // audio mixer to the configured output device.
      _isStreaming = true;
      
      // Simulate initial signal strength (would come from BLE in future)
      _signalStrength = 4; // Default to "Good" signal
      
      _logger.LogInformation("Raddy RF320 audio stream started successfully.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error starting Raddy radio stream");
      throw;
    }
  }

  /// <inheritdoc/>
  public async Task StopAsync()
  {
    if (!_isStreaming)
    {
      _logger.LogWarning("Raddy radio is not currently streaming.");
      return;
    }

    _logger.LogInformation("Stopping Raddy RF320 audio stream...");
    
    try
    {
      // Stop the audio stream (implementation would stop capturing from input device)
      await _audioPlayer.StopAsync("raddy_radio");
      
      _isStreaming = false;
      _signalStrength = 0; // No signal when stopped
      
      _logger.LogInformation("Raddy RF320 audio stream stopped successfully.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error stopping Raddy radio stream");
      throw;
    }
  }

  /// <inheritdoc/>
  public string? GetDeviceId()
  {
    return _deviceId;
  }

  /// <inheritdoc/>
  public Task<double?> GetFrequencyAsync()
  {
    // Placeholder for future BLE control implementation
    // _logger.LogWarning("GetFrequencyAsync not yet implemented. BLE control will be added in a future phase.");
    return Task.FromResult<double?>(null);
  }

  /// <inheritdoc/>
  public Task SetFrequencyAsync(double frequencyMHz)
  {
    // Placeholder for future BLE control implementation
    _logger.LogWarning("SetFrequencyAsync not yet implemented. BLE control will be added in a future phase. " +
                      "Requested frequency: {Frequency} MHz", frequencyMHz);
    return Task.CompletedTask;
  }
}
