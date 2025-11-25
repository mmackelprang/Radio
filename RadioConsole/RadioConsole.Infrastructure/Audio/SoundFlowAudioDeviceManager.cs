using RadioConsole.Core.Configuration;
using RadioConsole.Core.Interfaces.Audio;
using SoundFlow.Abstracts;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Structs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Implementation of IAudioDeviceManager using the SoundFlow library.
/// Manages enumeration and selection of audio devices on the system.
/// Provides global audio controls for volume, balance, equalization, and playback.
/// Supports hot-plug detection for USB audio devices.
/// </summary>
public class SoundFlowAudioDeviceManager : IAudioDeviceManager, IDisposable
{
  private readonly ILogger<SoundFlowAudioDeviceManager> _logger;
  private readonly SoundFlowOptions _options;
  private AudioEngine? _engine;
  private string? _currentInputDeviceId;
  private string? _currentOutputDeviceId;
  private Timer? _hotPlugTimer;
  private List<string> _lastKnownDeviceIds = new();
  private bool _disposed;
  
  // Global audio control state
  private float _globalVolume = 1.0f;
  private float _globalBalance = 0.0f;
  private EqualizationSettings _equalization = new() { Bass = 0, Midrange = 0, Treble = 0, Enabled = false };
  private PlaybackState _playbackState = PlaybackState.Stopped;

  /// <summary>
  /// Event raised when a USB audio device is connected.
  /// </summary>
  public event EventHandler<AudioDeviceEventArgs>? DeviceConnected;

  /// <summary>
  /// Event raised when a USB audio device is disconnected.
  /// </summary>
  public event EventHandler<AudioDeviceEventArgs>? DeviceDisconnected;

  /// <summary>
  /// Initializes a new instance of SoundFlowAudioDeviceManager with the specified logger.
  /// Uses default SoundFlowOptions.
  /// </summary>
  /// <param name="logger">Logger for diagnostic output.</param>
  public SoundFlowAudioDeviceManager(ILogger<SoundFlowAudioDeviceManager> logger)
    : this(logger, Options.Create(new SoundFlowOptions()))
  {
  }

  /// <summary>
  /// Initializes a new instance of SoundFlowAudioDeviceManager with the specified logger and options.
  /// </summary>
  /// <param name="logger">Logger for diagnostic output.</param>
  /// <param name="options">SoundFlow configuration options.</param>
  public SoundFlowAudioDeviceManager(
    ILogger<SoundFlowAudioDeviceManager> logger,
    IOptions<SoundFlowOptions> options)
  {
    _logger = logger;
    _options = options.Value;
    InitializeEngine();
    
    if (_options.EnableHotPlug)
    {
      StartHotPlugDetection();
    }
  }

  private void InitializeEngine()
  {
    try
    {
      _logger.LogInformation(
        "Initializing SoundFlow audio engine with settings: SampleRate={SampleRate}, BufferSize={BufferSize}, ExclusiveMode={ExclusiveMode}",
        _options.SampleRate, _options.BufferSize, _options.ExclusiveMode);
      
      // Initialize MiniAudioEngine - the concrete implementation of AudioEngine
      _engine = new MiniAudioEngine();
      
      // Store initial device list for hot-plug detection
      RefreshDeviceList();
      
      _logger.LogInformation("SoundFlow audio device manager initialized successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to initialize audio device manager. Ensure audio drivers are installed and accessible.");
      throw new InvalidOperationException("Failed to initialize SoundFlow audio engine", ex);
    }
  }

  private void StartHotPlugDetection()
  {
    _logger.LogInformation("Starting hot-plug detection with {Interval}ms polling interval", 
      _options.HotPlugPollingIntervalMs);
    
    _hotPlugTimer = new Timer(
      CheckForDeviceChanges,
      null,
      _options.HotPlugPollingIntervalMs,
      _options.HotPlugPollingIntervalMs);
  }

  private void RefreshDeviceList()
  {
    if (_engine == null) return;

    var currentDevices = new List<string>();
    
    var playbackDevices = _engine.PlaybackDevices;
    if (playbackDevices != null)
    {
      currentDevices.AddRange(playbackDevices.Select(d => $"out:{d.Id}"));
    }

    var captureDevices = _engine.CaptureDevices;
    if (captureDevices != null)
    {
      currentDevices.AddRange(captureDevices.Select(d => $"in:{d.Id}"));
    }

    _lastKnownDeviceIds = currentDevices;
  }

  private void CheckForDeviceChanges(object? state)
  {
    if (_disposed || _engine == null) return;

    try
    {
      var currentDevices = new List<string>();
      var deviceInfos = new Dictionary<string, (string Name, bool IsInput)>();

      var playbackDevices = _engine.PlaybackDevices;
      if (playbackDevices != null)
      {
        foreach (var d in playbackDevices)
        {
          var key = $"out:{d.Id}";
          currentDevices.Add(key);
          deviceInfos[key] = (d.Name, false);
        }
      }

      var captureDevices = _engine.CaptureDevices;
      if (captureDevices != null)
      {
        foreach (var d in captureDevices)
        {
          var key = $"in:{d.Id}";
          currentDevices.Add(key);
          deviceInfos[key] = (d.Name, true);
        }
      }

      // Find newly connected devices
      var newDevices = currentDevices.Except(_lastKnownDeviceIds).ToList();
      foreach (var deviceKey in newDevices)
      {
        if (deviceInfos.TryGetValue(deviceKey, out var info))
        {
          _logger.LogInformation("USB audio device connected: {DeviceName}", info.Name);
          
          // Check if this matches our preferred device pattern
          if (!string.IsNullOrEmpty(_options.PreferredUsbDevicePattern) &&
              info.Name.Contains(_options.PreferredUsbDevicePattern, StringComparison.OrdinalIgnoreCase))
          {
            _logger.LogInformation("Detected preferred USB device: {DeviceName} (matches pattern: {Pattern})",
              info.Name, _options.PreferredUsbDevicePattern);
          }

          DeviceConnected?.Invoke(this, new AudioDeviceEventArgs
          {
            DeviceId = deviceKey.Substring(deviceKey.IndexOf(':') + 1),
            DeviceName = info.Name,
            IsInputDevice = info.IsInput
          });
        }
      }

      // Find disconnected devices
      var removedDevices = _lastKnownDeviceIds.Except(currentDevices).ToList();
      foreach (var deviceKey in removedDevices)
      {
        var deviceId = deviceKey.Substring(deviceKey.IndexOf(':') + 1);
        var isInput = deviceKey.StartsWith("in:");
        
        _logger.LogWarning("USB audio device disconnected: {DeviceId}", deviceId);

        // Clear current device if it was disconnected
        if (isInput && _currentInputDeviceId == deviceId)
        {
          _logger.LogWarning("Current input device was disconnected");
          _currentInputDeviceId = null;
        }
        else if (!isInput && _currentOutputDeviceId == deviceId)
        {
          _logger.LogWarning("Current output device was disconnected");
          _currentOutputDeviceId = null;
        }

        DeviceDisconnected?.Invoke(this, new AudioDeviceEventArgs
        {
          DeviceId = deviceId,
          DeviceName = string.Empty,
          IsInputDevice = isInput
        });
      }

      _lastKnownDeviceIds = currentDevices;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during hot-plug device detection");
    }
  }

  /// <inheritdoc/>
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
          var deviceInfo = new AudioDeviceInfo
          {
            Id = device.Id.ToString(),
            Name = device.Name,
            IsDefault = device.IsDefault,
            DeviceType = GetDeviceType(device.Name)
          };
          deviceList.Add(deviceInfo);

          // Log preferred device detection
          if (!string.IsNullOrEmpty(_options.PreferredUsbDevicePattern) &&
              device.Name.Contains(_options.PreferredUsbDevicePattern, StringComparison.OrdinalIgnoreCase))
          {
            _logger.LogDebug("Found preferred input device: {DeviceName} (ID: {DeviceId})",
              device.Name, device.Id);
          }
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

  /// <inheritdoc/>
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
          var deviceInfo = new AudioDeviceInfo
          {
            Id = device.Id.ToString(),
            Name = device.Name,
            IsDefault = device.IsDefault,
            DeviceType = GetDeviceType(device.Name)
          };
          deviceList.Add(deviceInfo);

          // Log preferred device detection
          if (!string.IsNullOrEmpty(_options.PreferredUsbDevicePattern) &&
              device.Name.Contains(_options.PreferredUsbDevicePattern, StringComparison.OrdinalIgnoreCase))
          {
            _logger.LogDebug("Found preferred output device: {DeviceName} (ID: {DeviceId})",
              device.Name, device.Id);
          }
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

  /// <summary>
  /// Finds a USB audio device matching the preferred pattern (e.g., "Raddy" or "SH5").
  /// </summary>
  /// <returns>The matching device info, or null if not found.</returns>
  public async Task<AudioDeviceInfo?> FindPreferredUsbDeviceAsync()
  {
    if (string.IsNullOrEmpty(_options.PreferredUsbDevicePattern))
    {
      _logger.LogDebug("No preferred USB device pattern configured");
      return null;
    }

    var inputDevices = await GetInputDevicesAsync();
    var matchingDevice = inputDevices.FirstOrDefault(d =>
      d.Name.Contains(_options.PreferredUsbDevicePattern, StringComparison.OrdinalIgnoreCase));

    if (matchingDevice != null)
    {
      _logger.LogInformation("Found preferred USB device: {DeviceName} (ID: {DeviceId})",
        matchingDevice.Name, matchingDevice.Id);
    }
    else
    {
      _logger.LogWarning("Preferred USB device with pattern '{Pattern}' not found",
        _options.PreferredUsbDevicePattern);
    }

    return matchingDevice;
  }

  /// <inheritdoc/>
  public async Task<AudioDeviceInfo?> GetCurrentInputDeviceAsync()
  {
    if (string.IsNullOrEmpty(_currentInputDeviceId))
    {
      return null;
    }

    var devices = await GetInputDevicesAsync();
    return devices.FirstOrDefault(d => d.Id == _currentInputDeviceId);
  }

  /// <inheritdoc/>
  public async Task<AudioDeviceInfo?> GetCurrentOutputDeviceAsync()
  {
    if (string.IsNullOrEmpty(_currentOutputDeviceId))
    {
      return null;
    }

    var devices = await GetOutputDevicesAsync();
    return devices.FirstOrDefault(d => d.Id == _currentOutputDeviceId);
  }

  /// <inheritdoc/>
  public async Task SetInputDeviceAsync(string deviceId)
  {
    _logger.LogInformation("Setting input device to: {DeviceId}", deviceId);
    
    // Verify device exists
    var devices = await GetInputDevicesAsync();
    var device = devices.FirstOrDefault(d => d.Id == deviceId);
    
    if (device == null && deviceId != "default")
    {
      _logger.LogWarning("Input device {DeviceId} not found. Device may have been disconnected.", deviceId);
    }
    
    _currentInputDeviceId = deviceId;
    await Task.CompletedTask;
  }

  /// <inheritdoc/>
  public async Task SetOutputDeviceAsync(string deviceId)
  {
    _logger.LogInformation("Setting output device to: {DeviceId}", deviceId);
    
    // Verify device exists
    var devices = await GetOutputDevicesAsync();
    var device = devices.FirstOrDefault(d => d.Id == deviceId);
    
    if (device == null && deviceId != "default")
    {
      _logger.LogWarning("Output device {DeviceId} not found. Device may have been disconnected.", deviceId);
    }
    
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

  /// <summary>
  /// Gets the current SoundFlow configuration options.
  /// </summary>
  /// <returns>The current SoundFlowOptions.</returns>
  public SoundFlowOptions GetOptions() => _options;

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

  /// <summary>
  /// Disposes of the audio device manager and stops hot-plug detection.
  /// </summary>
  public void Dispose()
  {
    if (_disposed) return;
    
    _disposed = true;
    _hotPlugTimer?.Dispose();
    _engine?.Dispose();
    _logger.LogInformation("Audio device manager disposed");
  }
}

/// <summary>
/// Event arguments for audio device connect/disconnect events.
/// </summary>
public class AudioDeviceEventArgs : EventArgs
{
  /// <summary>
  /// The device identifier.
  /// </summary>
  public string DeviceId { get; set; } = string.Empty;

  /// <summary>
  /// The device name.
  /// </summary>
  public string DeviceName { get; set; } = string.Empty;

  /// <summary>
  /// Whether this is an input (capture) device.
  /// </summary>
  public bool IsInputDevice { get; set; }
}
