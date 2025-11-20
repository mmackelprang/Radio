using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;
using Sharpcaster;
using Sharpcaster.Models;
using Sharpcaster.Channels;
using Sharpcaster.Models.Media;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Cast audio output implementation that streams audio to Google Cast devices.
/// Does NOT play locally - pipes audio to an HTTP endpoint for casting.
/// </summary>
public class CastAudioOutput : IAudioOutput
{
  private readonly ILogger<CastAudioOutput> _logger;
  private readonly string _streamUrl;
  private ChromecastClient? _chromecastClient;
  private bool _isActive;
  private IAudioPlayer? _audioPlayer;
  private ChromecastReceiver? _selectedDevice;

  public bool IsActive => _isActive;
  public string Name => _selectedDevice != null 
    ? $"Cast to {_selectedDevice.Name}" 
    : "Cast Audio Output (No device selected)";

  /// <summary>
  /// Initializes a new instance of the CastAudioOutput class.
  /// </summary>
  /// <param name="logger">The logger instance.</param>
  /// <param name="streamUrl">The HTTP URL where the audio stream is available.</param>
  public CastAudioOutput(ILogger<CastAudioOutput> logger, string streamUrl)
  {
    _logger = logger;
    _streamUrl = streamUrl;
    _isActive = false;
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
      var devices = await DiscoverDevicesAsync();
      
      if (!devices.Any())
      {
        throw new InvalidOperationException("No Cast devices found on the network");
      }

      // Select the first available device (in a complete implementation, this would be user-selectable)
      _selectedDevice = devices.First();
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
  /// Discovers Google Cast devices on the local network.
  /// </summary>
  /// <param name="timeoutSeconds">Discovery timeout in seconds.</param>
  /// <returns>List of discovered Cast devices.</returns>
  public async Task<IEnumerable<ChromecastReceiver>> DiscoverDevicesAsync(int timeoutSeconds = 5)
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
}
