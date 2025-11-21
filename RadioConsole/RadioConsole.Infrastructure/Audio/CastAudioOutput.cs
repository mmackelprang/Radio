using RadioConsole.Core.Configuration;
using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sharpcaster;
using Sharpcaster.Models;
using Sharpcaster.Channels;
using Sharpcaster.Models.Media;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Cast audio output implementation that streams audio to Google Cast devices.
/// Does NOT play locally - pipes audio to an HTTP endpoint for casting.
/// Supports device selection preferences and automatic reconnection.
/// </summary>
public class CastAudioOutput : IAudioOutput
{
  private readonly ILogger<CastAudioOutput> _logger;
  private readonly CastAudioOptions _options;
  private readonly string _streamUrl;
  private ChromecastClient? _chromecastClient;
  private bool _isActive;
  private IAudioPlayer? _audioPlayer;
  private ChromecastReceiver? _selectedDevice;
  private CancellationTokenSource? _monitoringCts;
  private Task? _monitoringTask;

  public bool IsActive => _isActive;
  public string Name => _selectedDevice != null 
    ? $"Cast to {_selectedDevice.Name}" 
    : "Cast Audio Output (No device selected)";

  /// <summary>
  /// Initializes a new instance of the CastAudioOutput class.
  /// </summary>
  /// <param name="logger">The logger instance.</param>
  /// <param name="streamUrl">The HTTP URL where the audio stream is available.</param>
  /// <param name="options">Cast audio configuration options.</param>
  public CastAudioOutput(
    ILogger<CastAudioOutput> logger, 
    string streamUrl,
    IOptions<CastAudioOptions>? options = null)
  {
    _logger = logger;
    _streamUrl = streamUrl;
    _options = options?.Value ?? new CastAudioOptions();
    _isActive = false;

    _logger.LogDebug("CastAudioOutput initialized with PreferredDevice: {Device}, AutoSelect: {AutoSelect}",
      _options.PreferredDeviceName ?? "none", _options.AutoSelectFirst);
  }

  public async Task InitializeAsync()
  {
    _logger.LogInformation("Initializing cast audio output");
    await Task.CompletedTask;
  }

  public async Task StartAsync(IAudioPlayer audioPlayer)
  {
    if (_isActive)
    {
      _logger.LogWarning("Cast audio output is already active");
      return;
    }

    try
    {
      _logger.LogInformation("Starting cast audio output, discovering devices...");
      _audioPlayer = audioPlayer;

      // Discover Cast devices on the network
      var devices = await DiscoverDevicesAsync(_options.DiscoveryTimeoutSeconds);
      
      if (!devices.Any())
      {
        throw new InvalidOperationException("No Cast devices found on the network");
      }

      // Select device based on preferences
      _selectedDevice = SelectDevice(devices);
      
      if (_selectedDevice == null)
      {
        throw new InvalidOperationException(
          $"Could not select a Cast device. Preferred: '{_options.PreferredDeviceName}', " +
          $"AutoSelect: {_options.AutoSelectFirst}");
      }

      _logger.LogInformation("Selected Cast device: {DeviceName}", _selectedDevice.Name);

      // Create and connect ChromecastClient
      _chromecastClient = new ChromecastClient();
      await _chromecastClient.ConnectChromecast(_selectedDevice);
      _logger.LogInformation("Connected to Cast device");

      // Get the media channel and load media
      var mediaChannel = _chromecastClient.GetChannel<MediaChannel>();
      var media = new Media
      {
        ContentUrl = _streamUrl,
        ContentType = "audio/mpeg",
        StreamType = StreamType.Live
      };
      
      var mediaStatus = await mediaChannel.LoadAsync(media, autoPlay: true);
      
      if (mediaStatus != null)
      {
        _logger.LogInformation("Media loaded and playing on Cast device");
      }

      _isActive = true;
      _logger.LogInformation("Cast audio output started successfully");

      // Start connection monitoring if enabled
      if (_options.EnableReconnection)
      {
        _monitoringCts = new CancellationTokenSource();
        _monitoringTask = MonitorConnectionAsync(_monitoringCts.Token);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to start cast audio output");
      throw;
    }
  }

  public async Task StopAsync()
  {
    if (!_isActive)
    {
      _logger.LogWarning("Cast audio output is not active");
      return;
    }

    try
    {
      _logger.LogInformation("Stopping cast audio output");

      // Stop connection monitoring
      if (_monitoringCts != null)
      {
        _monitoringCts.Cancel();
        if (_monitoringTask != null)
        {
          try
          {
            await _monitoringTask;
          }
          catch (OperationCanceledException)
          {
            // Expected when canceling
          }
        }
        _monitoringCts?.Dispose();
        _monitoringCts = null;
        _monitoringTask = null;
      }

      // Stop playback on the Cast device
      if (_chromecastClient != null)
      {
        try
        {
          var mediaChannel = _chromecastClient.GetChannel<MediaChannel>();
          await mediaChannel.StopAsync();
        }
        catch (Exception ex)
        {
          _logger.LogWarning(ex, "Error stopping media playback on Cast device");
        }

        await _chromecastClient.DisconnectAsync();
        await Task.Run(() => _chromecastClient.Dispose());
        _chromecastClient = null;
      }

      _isActive = false;
      _audioPlayer = null;
      _selectedDevice = null;
      _logger.LogInformation("Cast audio output stopped successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to stop cast audio output");
      throw;
    }
  }

  /// <summary>
  /// Monitors the Cast connection and attempts reconnection if the connection is lost.
  /// Uses exponential backoff for retry attempts.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token to stop monitoring.</param>
  private async Task MonitorConnectionAsync(CancellationToken cancellationToken)
  {
    int retryCount = 0;
    
    _logger.LogInformation("Starting Cast connection monitoring");

    try
    {
      while (!cancellationToken.IsCancellationRequested && retryCount < _options.MaxReconnectionAttempts)
      {
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

        // Check if connection is still active
        if (!IsConnectionHealthy())
        {
          _logger.LogWarning("Cast connection lost, attempting reconnection (attempt {Count}/{Max})", 
            retryCount + 1, _options.MaxReconnectionAttempts);
          
          try
          {
            await ReconnectAsync();
            retryCount = 0; // Reset on successful reconnection
            _logger.LogInformation("Successfully reconnected to Cast device");
          }
          catch (Exception ex)
          {
            retryCount++;
            _logger.LogError(ex, "Reconnection attempt {Count} failed", retryCount);
            
            if (retryCount >= _options.MaxReconnectionAttempts)
            {
              _logger.LogError("Maximum reconnection attempts ({Max}) reached, giving up", 
                _options.MaxReconnectionAttempts);
              _isActive = false;
              break;
            }
            
            // Exponential backoff: delay = baseDelay * 2^retryCount
            var delay = TimeSpan.FromSeconds(_options.ReconnectionBaseDelaySeconds * Math.Pow(2, retryCount - 1));
            _logger.LogInformation("Waiting {Delay}s before next reconnection attempt", delay.TotalSeconds);
            await Task.Delay(delay, cancellationToken);
          }
        }
      }
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Connection monitoring cancelled");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in connection monitoring");
    }
  }

  /// <summary>
  /// Checks if the Cast connection is healthy.
  /// </summary>
  /// <returns>True if connection is healthy, false otherwise.</returns>
  private bool IsConnectionHealthy()
  {
    try
    {
      // Check if client exists and is connected
      return _chromecastClient != null && _isActive;
      
      // Note: In a complete implementation, you could ping the device
      // or check the connection status more thoroughly
    }
    catch
    {
      return false;
    }
  }

  /// <summary>
  /// Attempts to reconnect to the Cast device.
  /// </summary>
  private async Task ReconnectAsync()
  {
    _logger.LogInformation("Attempting to reconnect to Cast device: {DeviceName}", _selectedDevice?.Name);

    // Clean up existing connection
    if (_chromecastClient != null)
    {
      try
      {
        await _chromecastClient.DisconnectAsync();
        await Task.Run(() => _chromecastClient.Dispose());
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Error cleaning up old connection during reconnect");
      }
      _chromecastClient = null;
    }

    // Rediscover devices if we don't have a selected device
    if (_selectedDevice == null)
    {
      var devices = await DiscoverDevicesAsync(_options.DiscoveryTimeoutSeconds);
      _selectedDevice = SelectDevice(devices);
      
      if (_selectedDevice == null)
      {
        throw new InvalidOperationException("Could not find a suitable Cast device for reconnection");
      }
    }

    // Reconnect
    _chromecastClient = new ChromecastClient();
    await _chromecastClient.ConnectChromecast(_selectedDevice);

    // Reload media
    var mediaChannel = _chromecastClient.GetChannel<MediaChannel>();
    var media = new Media
    {
      ContentUrl = _streamUrl,
      ContentType = "audio/mpeg",
      StreamType = StreamType.Live
    };
    
    await mediaChannel.LoadAsync(media, autoPlay: true);
    
    _logger.LogInformation("Reconnection successful");
  }

  /// <summary>
  /// Discovers Google Cast devices on the local network.
  /// </summary>
  /// <param name="timeoutSeconds">Discovery timeout in seconds.</param>
  /// <returns>List of discovered Cast devices.</returns>
  public async Task<IEnumerable<ChromecastReceiver>> DiscoverDevicesAsync(double timeoutSeconds = 5)
  {
    try
    {
      _logger.LogInformation("Discovering Cast devices (timeout: {Timeout}s)...", timeoutSeconds);
      
      var locator = new ChromecastLocator();
      var timeout = TimeSpan.FromSeconds(timeoutSeconds);
      
      var devices = await locator.FindReceiversAsync(timeout);

      foreach (var device in devices)
      {
        _logger.LogInformation("Discovered Cast device: {DeviceName} ({DeviceUri})", 
          device.Name, 
          device.DeviceUri);
      }

      _logger.LogInformation("Discovery complete. Found {Count} Cast device(s)", devices.Count());
      return devices;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during Cast device discovery");
      return Enumerable.Empty<ChromecastReceiver>();
    }
  }

  /// <summary>
  /// Selects a Cast device based on configuration preferences.
  /// </summary>
  /// <param name="devices">Available Cast devices.</param>
  /// <returns>The selected device, or null if no suitable device found.</returns>
  private ChromecastReceiver? SelectDevice(IEnumerable<ChromecastReceiver> devices)
  {
    if (!devices.Any())
    {
      _logger.LogWarning("No devices available for selection");
      return null;
    }

    // Try to find preferred device by name (case-insensitive partial match)
    if (!string.IsNullOrEmpty(_options.PreferredDeviceName))
    {
      var preferred = devices.FirstOrDefault(d => 
        d.Name.Contains(_options.PreferredDeviceName, StringComparison.OrdinalIgnoreCase));
      
      if (preferred != null)
      {
        _logger.LogInformation("Selected preferred Cast device: {DeviceName}", preferred.Name);
        return preferred;
      }

      _logger.LogWarning("Preferred device '{PreferredName}' not found among discovered devices",
        _options.PreferredDeviceName);
    }

    // Fall back to first device if auto-select is enabled
    if (_options.AutoSelectFirst)
    {
      var first = devices.First();
      _logger.LogInformation("Auto-selected first available Cast device: {DeviceName}", first.Name);
      return first;
    }

    _logger.LogWarning("No device selected (auto-select disabled and preferred device not found)");
    return null;
  }
}
