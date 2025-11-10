using NAudio.Wave;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Modules.Inputs;

/// <summary>
/// Audio input that captures audio from a USB audio device
/// This can be used for radio receivers, turntables, or other USB audio sources
/// </summary>
public class UsbAudioInput : BaseAudioInput
{
    private readonly int _deviceNumber;
    private WaveInEvent? _waveIn;
    private CancellationTokenSource? _captureCts;

    public override string Id { get; }
    public override string Name { get; }
    public override string Description { get; }

    /// <summary>
    /// Create a UsbAudioInput for capturing audio from a USB device
    /// </summary>
    /// <param name="deviceNumber">Device number (use -1 for default device)</param>
    /// <param name="name">Display name for this input</param>
    /// <param name="description">Description of the input device</param>
    /// <param name="environmentService">Environment service</param>
    /// <param name="storage">Storage service</param>
    public UsbAudioInput(
        int deviceNumber,
        string name,
        string description,
        IEnvironmentService environmentService,
        IStorage storage) : base(environmentService, storage)
    {
        _deviceNumber = deviceNumber;
        Name = name;
        Description = description;
        Id = $"usb_audio_{name.ToLowerInvariant().Replace(" ", "_")}";
    }

    public override async Task InitializeAsync()
    {
        await _configuration.LoadAsync();

        if (_environmentService.IsSimulationMode)
        {
            // In simulation mode, assume device is available
            IsAvailable = true;
            _display.UpdateStatus($"{Name} (Simulation Mode)");
        }
        else
        {
            // Check if the USB audio device is available
            try
            {
                var deviceCount = WaveInEvent.DeviceCount;
                
                if (_deviceNumber == -1)
                {
                    // Use default device
                    IsAvailable = deviceCount > 0;
                }
                else if (_deviceNumber >= 0 && _deviceNumber < deviceCount)
                {
                    var capabilities = WaveInEvent.GetCapabilities(_deviceNumber);
                    IsAvailable = true;
                    _display.UpdateStatus($"{Name} Ready: {capabilities.ProductName}");
                }
                else
                {
                    IsAvailable = false;
                    _display.UpdateStatus($"Device {_deviceNumber} not found (available: 0-{deviceCount - 1})");
                }
            }
            catch (Exception ex)
            {
                IsAvailable = false;
                _display.UpdateStatus($"Error checking device: {ex.Message}");
            }
        }
    }

    public override async Task StartAsync()
    {
        if (!IsAvailable)
            throw new InvalidOperationException($"{Name} is not available");

        if (IsActive)
            return; // Already capturing

        IsActive = true;
        IsPaused = false;

        if (_environmentService.IsSimulationMode)
        {
            _display.UpdateStatus($"Capturing from {Name}");
            _display.UpdateMetadata(new Dictionary<string, string>
            {
                ["Device"] = $"USB Device {_deviceNumber}",
                ["Status"] = "Capturing (Simulation)",
                ["Format"] = "16-bit PCM, 44.1kHz, Stereo"
            });
        }
        else
        {
            // Start capturing
            await StartCaptureAsync();
        }
    }

    public override async Task StopAsync()
    {
        IsActive = false;
        IsPaused = false;

        // Stop capture
        _captureCts?.Cancel();
        
        if (_waveIn != null)
        {
            try
            {
                _waveIn.StopRecording();
                _waveIn.DataAvailable -= OnDataAvailable;
                _waveIn.RecordingStopped -= OnRecordingStopped;
                _waveIn.Dispose();
                _waveIn = null;
            }
            catch (Exception)
            {
                // Ignore errors during cleanup
            }
        }

        _captureCts?.Dispose();
        _captureCts = null;

        _display.UpdateStatus($"{Name} stopped");
        await Task.CompletedTask;
    }

    public override Task PauseAsync()
    {
        if (!IsActive)
            throw new InvalidOperationException($"{Name} is not active");

        IsPaused = true;
        _display.UpdateStatus($"{Name} paused");
        return Task.CompletedTask;
    }

    public override Task ResumeAsync()
    {
        if (!IsActive)
            throw new InvalidOperationException($"{Name} is not active");

        IsPaused = false;
        _display.UpdateStatus($"{Name} resumed");
        return Task.CompletedTask;
    }

    public override Task<Stream?> GetAudioStreamAsync()
    {
        if (_environmentService.IsSimulationMode)
        {
            return Task.FromResult<Stream?>(new MemoryStream());
        }

        // USB audio is a live stream, doesn't have a traditional Stream interface
        return Task.FromResult<Stream?>(null);
    }

    private async Task StartCaptureAsync()
    {
        _captureCts = new CancellationTokenSource();

        try
        {
            _waveIn = new WaveInEvent
            {
                DeviceNumber = _deviceNumber == -1 ? 0 : _deviceNumber,
                WaveFormat = new WaveFormat(44100, 16, 2) // 44.1kHz, 16-bit, stereo
            };

            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.RecordingStopped += OnRecordingStopped;

            _waveIn.StartRecording();

            _display.UpdateStatus($"Capturing from {Name}");
            _display.UpdateMetadata(new Dictionary<string, string>
            {
                ["Device"] = $"Device {_deviceNumber}",
                ["Status"] = "Capturing",
                ["Format"] = $"{_waveIn.WaveFormat.SampleRate}Hz, {_waveIn.WaveFormat.BitsPerSample}-bit, {_waveIn.WaveFormat.Channels}ch"
            });
        }
        catch (Exception ex)
        {
            IsAvailable = false;
            IsActive = false;
            _display.UpdateStatus($"Error starting capture: {ex.Message}");
            throw;
        }

        await Task.CompletedTask;
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (IsPaused || !IsActive)
            return;

        // Apply volume
        var buffer = new byte[e.BytesRecorded];
        Array.Copy(e.Buffer, buffer, e.BytesRecorded);

        // Simple volume adjustment (multiply PCM samples)
        if (_volume != 1.0)
        {
            for (int i = 0; i < buffer.Length; i += 2)
            {
                if (i + 1 < buffer.Length)
                {
                    short sample = (short)(buffer[i] | (buffer[i + 1] << 8));
                    sample = (short)(sample * _volume);
                    buffer[i] = (byte)(sample & 0xFF);
                    buffer[i + 1] = (byte)((sample >> 8) & 0xFF);
                }
            }
        }

        // Fire audio data available event
        OnAudioDataAvailable(new AudioDataAvailableEventArgs
        {
            AudioData = buffer,
            SampleRate = _waveIn!.WaveFormat.SampleRate,
            Channels = _waveIn.WaveFormat.Channels,
            BitsPerSample = _waveIn.WaveFormat.BitsPerSample,
            Timestamp = DateTime.UtcNow
        });
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            _display.UpdateStatus($"Error during capture: {e.Exception.Message}");
        }
    }
}
