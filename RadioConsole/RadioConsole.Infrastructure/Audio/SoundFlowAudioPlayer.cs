using RadioConsole.Core;
using RadioConsole.Core.Interfaces.Audio;
using SoundFlow.Abstracts;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Enums;
using SoundFlow.Structs;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio;

/// <summary>
/// Implementation of IAudioPlayer using the SoundFlow library.
/// Wraps SoundFlow's AudioEngine and AudioPlaybackDevice for cross-platform audio playback.
/// </summary>
public class SoundFlowAudioPlayer : IAudioPlayer, IDisposable
{
    private readonly ILogger<SoundFlowAudioPlayer> _logger;
    private readonly IVisualizationService? _visualizationService;
    private AudioEngine? _engine;
    private AudioPlaybackDevice? _playbackDevice;
    private readonly Dictionary<string, AudioSource> _audioSources;
    private bool _isInitialized;
    private string? _currentDeviceId;
    private Timer? _fftTimer;

    public bool IsInitialized => _isInitialized;

    public SoundFlowAudioPlayer(ILogger<SoundFlowAudioPlayer> logger, IVisualizationService? visualizationService = null)
    {
        _logger = logger;
        _visualizationService = visualizationService;
        _audioSources = new Dictionary<string, AudioSource>();
        _isInitialized = false;
    }

  public async Task InitializeAsync(string deviceId)
  {
    try
    {
      _logger.LogInformation("Initializing SoundFlow audio player with device: {DeviceId}", deviceId);

      // Initialize the MiniAudioEngine
      _engine = new MiniAudioEngine();

      // Create audio format (standard CD quality: 44.1kHz, 2 channels, 16-bit)
      var format = new AudioFormat
      {
        SampleRate = AudioConstants.DefaultSampleRate,
        Channels = AudioConstants.DefaultChannels,
        Format = SampleFormat.S16
      };

      // Find the device by ID
      DeviceInfo? targetDevice = null;
      if (!string.IsNullOrEmpty(deviceId) && deviceId != AudioConstants.DefaultDeviceId)
      {
        var devices = _engine.PlaybackDevices;
        if (devices != null)
        {
          // Convert deviceId string to nint for comparison
          if (nint.TryParse(deviceId, out var deviceIdNint))
          {
            targetDevice = devices.FirstOrDefault(d => d.Id == deviceIdNint);
            if (targetDevice == null)
            {
              _logger.LogWarning("Device {DeviceId} not found, using default device", deviceId);
            }
          }
          else
          {
            _logger.LogWarning("Invalid device ID format: {DeviceId}, using default device", deviceId);
          }
        }
      }

      // Initialize playback device (null means use default device and default config)
      _playbackDevice = _engine.InitializePlaybackDevice(targetDevice, format, null);
      
      _currentDeviceId = deviceId;
      _isInitialized = true;

      _logger.LogInformation("SoundFlow audio player initialized successfully");
      await Task.CompletedTask;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to initialize SoundFlow audio player");
      throw;
    }
  }

  public async Task PlayAsync(string sourceId, Stream audioData)
  {
    if (!_isInitialized || _playbackDevice == null || _engine == null)
    {
      throw new InvalidOperationException("Audio player is not initialized");
    }

    try
    {
      _logger.LogInformation("Starting playback for source: {SourceId}", sourceId);

      // Stop existing source if it exists
      if (_audioSources.ContainsKey(sourceId))
      {
        await StopAsync(sourceId);
      }

      // Create audio format for decoder
      var format = new AudioFormat
      {
        SampleRate = AudioConstants.DefaultSampleRate,
        Channels = AudioConstants.DefaultChannels,
        Format = SampleFormat.S16
      };

      // Create a decoder for the audio stream
      var decoder = _engine.CreateDecoder(audioData, format);
      
      // Store the source information
      var source = new AudioSource
      {
        Id = sourceId,
        Stream = audioData,
        Decoder = decoder as SoundComponent,
        Volume = 1.0f
      };

      _audioSources[sourceId] = source;

      // Connect the decoder to the playback device's master mixer
      if (decoder is SoundComponent component)
      {
        _playbackDevice.MasterMixer.AddComponent(component);
      }
      
      // Start playback if not already started
      if (!_playbackDevice.IsRunning)
      {
        _playbackDevice.Start();
      }

      _logger.LogInformation("Source {SourceId} added to playback", sourceId);
      await Task.CompletedTask;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to play audio source: {SourceId}", sourceId);
      throw;
    }
  }

  public async Task StopAsync(string sourceId)
  {
    if (_audioSources.ContainsKey(sourceId))
    {
      _logger.LogInformation("Stopping playback for source: {SourceId}", sourceId);
      
      var source = _audioSources[sourceId];
      
      // Remove the decoder from the playback device's master mixer
      if (source.Decoder != null && _playbackDevice != null)
      {
        _playbackDevice.MasterMixer.RemoveComponent(source.Decoder);
        source.Decoder.Dispose();
      }
      
      source.Stream?.Dispose();
      _audioSources.Remove(sourceId);

      _logger.LogInformation("Source {SourceId} stopped and removed", sourceId);
    }

    await Task.CompletedTask;
  }

  public async Task SetVolumeAsync(string sourceId, float volume)
  {
    if (_audioSources.TryGetValue(sourceId, out var source))
    {
      source.Volume = Math.Clamp(volume, 0.0f, 1.0f);
      _logger.LogInformation("Volume for source {SourceId} set to {Volume}", sourceId, source.Volume);
    }
    else
    {
      _logger.LogWarning("Attempted to set volume for non-existent source: {SourceId}", sourceId);
    }

    await Task.CompletedTask;
  }

  public Stream GetMixedOutputStream()
  {
    if (!_isInitialized)
    {
      throw new InvalidOperationException("Audio player is not initialized");
    }

    // Return a stream that represents the mixed audio output
    // For now, return a memory stream that will be populated by the audio engine
    // In a complete implementation, this would tap into SoundFlow's output pipeline
    var outputStream = new MemoryStream();
    return outputStream;
  }

  public void Dispose()
    {
        _logger.LogInformation("Disposing SoundFlow audio player");

        _fftTimer?.Dispose();

        foreach (var source in _audioSources.Values)
        {
            source.Decoder?.Dispose();
            source.Stream?.Dispose();
        }
        _audioSources.Clear();

        if (_playbackDevice != null)
        {
            _playbackDevice.Stop();
            _playbackDevice.Dispose();
        }

        _engine?.Dispose();

        _isInitialized = false;
    }

    public void EnableFftDataGeneration(bool enabled)
    {
        if (enabled)
        {
            _fftTimer = new Timer(async _ => await GenerateFftData(), null, 0, 50);
        }
        else
        {
            _fftTimer?.Dispose();
        }
    }

    private async Task GenerateFftData()
    {
        if (_playbackDevice != null && _visualizationService != null)
        {
            var fftData = new float[256]; // Get 256 samples
            // Note: This is a placeholder. A real implementation would need to get the FFT data from SoundFlow.
            // SoundFlow uses MiniAudio, which does not have built-in FFT capabilities.
            // A potential solution is to integrate a library like Kiss FFT.
            // For now, we'll generate some random data to simulate the FFT data.
            var rand = new Random();
            for (int i = 0; i < fftData.Length; i++)
            {
                fftData[i] = (float)rand.NextDouble();
            }
            await _visualizationService.SendFFTDataAsync(fftData);
        }
    }

  private class AudioSource
  {
    public string Id { get; set; } = string.Empty;
    public Stream? Stream { get; set; }
    public SoundComponent? Decoder { get; set; }
    public float Volume { get; set; }
  }
}
