using RadioConsole.Core.Interfaces.Audio;
using SoundFlow.Abstracts;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Structs;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Implementation of IAudioDeviceManager using the SoundFlow library.
/// Manages enumeration and selection of audio devices on the system.
/// </summary>
public class SoundFlowAudioDeviceManager : IAudioDeviceManager, IDisposable
{
  private readonly ILogger<SoundFlowAudioDeviceManager> _logger;
  private AudioEngine? _engine;
  private string? _currentInputDeviceId;
  private string? _currentOutputDeviceId;

  public SoundFlowAudioDeviceManager(ILogger<SoundFlowAudioDeviceManager> logger)
  {
    _logger = logger;
    InitializeEngine();
  }

  private void InitializeEngine()
  {
    try
    {
      // Initialize MiniAudioEngine - the concrete implementation of AudioEngine
      _engine = new MiniAudioEngine();
      _logger.LogInformation("SoundFlow audio device manager initialized successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to initialize audio device manager");
      throw;
    }
  }

  public async Task<IEnumerable<AudioDeviceInfo>> GetInputDevicesAsync()
  {
    try
    {
      if (_engine == null)
      {
        _logger.LogWarning("Audio engine not initialized, returning empty device list");
        return Enumerable.Empty<AudioDeviceInfo>();
      }

      // Get capture devices from SoundFlow
      var captureDevices = _engine.CaptureDevices;
      var deviceList = new List<AudioDeviceInfo>();

      if (captureDevices != null)
      {
        foreach (var device in captureDevices)
        {
          deviceList.Add(new AudioDeviceInfo
          {
            Id = device.Id.ToString(),
            Name = device.Name,
            IsDefault = device.IsDefault,
            DeviceType = GetDeviceType(device.Name)
          });
        }
      }

      _logger.LogInformation("Found {Count} input devices", deviceList.Count);
      return await Task.FromResult(deviceList);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to get input devices");
      throw;
    }
  }

  public async Task<IEnumerable<AudioDeviceInfo>> GetOutputDevicesAsync()
  {
    try
    {
      if (_engine == null)
      {
        _logger.LogWarning("Audio engine not initialized, returning empty device list");
        return Enumerable.Empty<AudioDeviceInfo>();
      }

      // Get playback devices from SoundFlow
      var playbackDevices = _engine.PlaybackDevices;
      var deviceList = new List<AudioDeviceInfo>();

      if (playbackDevices != null)
      {
        foreach (var device in playbackDevices)
        {
          deviceList.Add(new AudioDeviceInfo
          {
            Id = device.Id.ToString(),
            Name = device.Name,
            IsDefault = device.IsDefault,
            DeviceType = GetDeviceType(device.Name)
          });
        }
      }

      _logger.LogInformation("Found {Count} output devices", deviceList.Count);
      return await Task.FromResult(deviceList);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to get output devices");
      throw;
    }
  }

  public async Task<AudioDeviceInfo?> GetCurrentInputDeviceAsync()
  {
    if (string.IsNullOrEmpty(_currentInputDeviceId))
    {
      return null;
    }

    var devices = await GetInputDevicesAsync();
    return devices.FirstOrDefault(d => d.Id == _currentInputDeviceId);
  }

  public async Task<AudioDeviceInfo?> GetCurrentOutputDeviceAsync()
  {
    if (string.IsNullOrEmpty(_currentOutputDeviceId))
    {
      return null;
    }

    var devices = await GetOutputDevicesAsync();
    return devices.FirstOrDefault(d => d.Id == _currentOutputDeviceId);
  }

  public async Task SetInputDeviceAsync(string deviceId)
  {
    _logger.LogInformation("Setting input device to: {DeviceId}", deviceId);
    _currentInputDeviceId = deviceId;
    await Task.CompletedTask;
  }

  public async Task SetOutputDeviceAsync(string deviceId)
  {
    _logger.LogInformation("Setting output device to: {DeviceId}", deviceId);
    _currentOutputDeviceId = deviceId;
    await Task.CompletedTask;
  }

  private string GetDeviceType(string deviceName)
  {
    // Detect device type based on name patterns
    var nameLower = deviceName.ToLowerInvariant();

    if (nameLower.Contains("usb"))
    {
      return "USB Audio";
    }
    else if (nameLower.Contains("hdmi"))
    {
      return "HDMI";
    }
    else if (nameLower.Contains("analog") || nameLower.Contains("jack"))
    {
      return "Analog";
    }
    else if (nameLower.Contains("bluetooth") || nameLower.Contains("bt"))
    {
      return "Bluetooth";
    }
    else if (nameLower.Contains("default"))
    {
      return "Default";
    }

    return "Unknown";
  }

  public void Dispose()
  {
    _engine?.Dispose();
    _logger.LogInformation("Audio device manager disposed");
  }
}
