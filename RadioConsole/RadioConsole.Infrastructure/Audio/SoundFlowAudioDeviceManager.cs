using RadioConsole.Core.Interfaces.Audio;
using SoundFlow.Abstracts;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Structs;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Implementation of IAudioDeviceManager using the SoundFlow library.
/// Manages enumeration and selection of audio devices on the system.
/// Provides global audio controls for volume, balance, equalization, and playback.
/// </summary>
public class SoundFlowAudioDeviceManager : IAudioDeviceManager, IDisposable
{
  private readonly ILogger<SoundFlowAudioDeviceManager> _logger;
  private AudioEngine? _engine;
  private string? _currentInputDeviceId;
  private string? _currentOutputDeviceId;
  
  // Global audio control state
  private float _globalVolume = 1.0f;
  private float _globalBalance = 0.0f;
  private EqualizationSettings _equalization = new() { Bass = 0, Midrange = 0, Treble = 0, Enabled = false };
  private PlaybackState _playbackState = PlaybackState.Stopped;

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

  // Global Audio Control Methods

  /// <inheritdoc/>
  public async Task<float> GetGlobalVolumeAsync()
  {
    _logger.LogDebug("Getting global volume: {Volume}", _globalVolume);
    return await Task.FromResult(_globalVolume);
  }

  /// <inheritdoc/>
  public async Task SetGlobalVolumeAsync(float volume)
  {
    var clampedVolume = Math.Clamp(volume, 0.0f, 1.0f);
    _globalVolume = clampedVolume;
    
    _logger.LogInformation("Global volume set to: {Volume}", clampedVolume);
    
    // In a full implementation, this would apply the volume to the MiniAudioEngine's master mixer
    // AudioEngine.MasterMixer.Volume = clampedVolume;
    
    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public async Task<float> GetGlobalBalanceAsync()
  {
    _logger.LogDebug("Getting global balance: {Balance}", _globalBalance);
    return await Task.FromResult(_globalBalance);
  }

  /// <inheritdoc/>
  public async Task SetGlobalBalanceAsync(float balance)
  {
    var clampedBalance = Math.Clamp(balance, -1.0f, 1.0f);
    _globalBalance = clampedBalance;
    
    _logger.LogInformation("Global balance (pan) set to: {Balance}", clampedBalance);
    
    // In a full implementation, this would apply the pan to the MiniAudioEngine's master mixer
    // AudioEngine.MasterMixer.Pan = clampedBalance;
    
    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public async Task<EqualizationSettings> GetEqualizationAsync()
  {
    _logger.LogDebug("Getting equalization settings");
    return await Task.FromResult(_equalization);
  }

  /// <inheritdoc/>
  public async Task SetEqualizationAsync(EqualizationSettings settings)
  {
    if (settings == null)
    {
      throw new ArgumentNullException(nameof(settings));
    }

    // Clamp values to valid range
    _equalization = new EqualizationSettings
    {
      Bass = Math.Clamp(settings.Bass, -12.0f, 12.0f),
      Midrange = Math.Clamp(settings.Midrange, -12.0f, 12.0f),
      Treble = Math.Clamp(settings.Treble, -12.0f, 12.0f),
      Enabled = settings.Enabled
    };

    _logger.LogInformation(
      "Equalization set - Bass: {Bass}dB, Midrange: {Midrange}dB, Treble: {Treble}dB, Enabled: {Enabled}",
      _equalization.Bass, _equalization.Midrange, _equalization.Treble, _equalization.Enabled);

    // In a full implementation, this would apply EQ to the MiniAudioEngine's master mixer
    // using a multi-band equalizer component
    
    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public async Task PauseAsync()
  {
    _playbackState = PlaybackState.Paused;
    _logger.LogInformation("Global audio playback paused");
    
    // In a full implementation: AudioEngine.Pause();
    
    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public async Task PlayAsync()
  {
    _playbackState = PlaybackState.Playing;
    _logger.LogInformation("Global audio playback resumed/started");
    
    // In a full implementation: AudioEngine.Play();
    
    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public async Task StopAsync()
  {
    _playbackState = PlaybackState.Stopped;
    _logger.LogInformation("Global audio playback stopped");
    
    // In a full implementation: AudioEngine.Stop();
    
    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public async Task<PlaybackState> GetPlaybackStateAsync()
  {
    _logger.LogDebug("Getting playback state: {State}", _playbackState);
    return await Task.FromResult(_playbackState);
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
