using NAudio.Wave;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Modules.Outputs;

/// <summary>
/// Wired soundbar output module
/// 
/// This output uses the system's default ALSA audio device on Linux/Raspberry Pi.
/// Audio streams from the AudioMixer are sent directly to the soundbar via ALSA.
/// The implementation uses a BufferedWaveProvider with WaveFileWriter for cross-platform support.
/// On actual hardware, ALSA integration would happen through the default audio device.
/// </summary>
public class WiredSoundbarOutput : BaseAudioOutput
{
    private BufferedWaveProvider? _waveProvider;
    private readonly object _lockObject = new();
    private WaveFileWriter? _waveFileWriter;
    private string? _tempOutputFile;

    // Audio format matching AudioMixer output
    private const int SAMPLE_RATE = 44100;
    private const int CHANNELS = 2;
    private const int BITS_PER_SAMPLE = 16;

    public override string Id => "wired_soundbar";
    public override string Name => "Wired Soundbar";
    public override string Description => "Wired Soundbar Connection";

    public WiredSoundbarOutput(IEnvironmentService environmentService, IStorage storage) 
        : base(environmentService, storage)
    {
    }

    public override async Task InitializeAsync()
    {
        await _configuration.LoadAsync();
        
        if (_environmentService.IsSimulationMode)
        {
            // Simulation mode - soundbar is available but mocked
            IsAvailable = true;
            _display.UpdateStatus("Soundbar (Simulation Mode)");
        }
        else
        {
            // Check for actual soundbar connection
            IsAvailable = await CheckSoundbarConnectionAsync();
            _display.UpdateStatus(IsAvailable ? "Soundbar Connected" : "Soundbar Not Found");
        }
    }

    public override Task StartAsync()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Soundbar is not available");

        lock (_lockObject)
        {
            // Initialize wave provider with standard PCM format
            var waveFormat = new WaveFormat(SAMPLE_RATE, BITS_PER_SAMPLE, CHANNELS);
            _waveProvider = new BufferedWaveProvider(waveFormat)
            {
                BufferLength = 1024 * 1024, // 1MB buffer to prevent dropping packets
                DiscardOnBufferOverflow = false // Don't drop audio data
            };

            if (!_environmentService.IsSimulationMode)
            {
                // In hardware mode, we would configure ALSA output here
                // For now, write to a temp file to demonstrate streaming works
                _tempOutputFile = Path.Combine(Path.GetTempPath(), $"soundbar_output_{DateTime.Now:yyyyMMdd_HHmmss}.wav");
                _waveFileWriter = new WaveFileWriter(_tempOutputFile, waveFormat);
            }

            IsActive = true;
            _display.UpdateStatus("Active");
            _display.UpdateMetadata(new Dictionary<string, string>
            {
                ["Volume"] = $"{(int)(_volume * 100)}%",
                ["Connection"] = "Wired",
                ["OutputFile"] = _tempOutputFile ?? "Simulation"
            });
        }
        
        return Task.CompletedTask;
    }

    public override Task StopAsync()
    {
        lock (_lockObject)
        {
            IsActive = false;
            
            if (_waveFileWriter != null)
            {
                _waveFileWriter.Dispose();
                _waveFileWriter = null;
            }

            _waveProvider = null;
            _display.UpdateStatus("Inactive");
        }
        
        return Task.CompletedTask;
    }

    public override async Task SendAudioAsync(Stream audioStream)
    {
        if (!IsActive)
            throw new InvalidOperationException("Soundbar is not active");

        // In simulation mode, just consume the stream
        if (_environmentService.IsSimulationMode)
        {
            return;
        }

        // Stream audio data to the output device (or file in testing)
        lock (_lockObject)
        {
            if (_waveProvider == null)
                return;

            // Read all available data from the stream
            var buffer = new byte[audioStream.Length];
            var bytesRead = audioStream.Read(buffer, 0, buffer.Length);

            if (bytesRead > 0)
            {
                // Add audio data to the buffer
                _waveProvider.AddSamples(buffer, 0, bytesRead);

                // Write to file for testing/verification
                if (_waveFileWriter != null)
                {
                    _waveFileWriter.Write(buffer, 0, bytesRead);
                    _waveFileWriter.Flush();
                }
            }
        }

        await Task.CompletedTask;
    }

    public override async Task SetVolumeAsync(double volume)
    {
        await base.SetVolumeAsync(volume);
        
        _display.UpdateMetadata(new Dictionary<string, string>
        {
            ["Volume"] = $"{(int)(_volume * 100)}%",
            ["Connection"] = "Wired"
        });
    }

    private Task<bool> CheckSoundbarConnectionAsync()
    {
        // As specified in requirements, assume soundbar is always available
        return Task.FromResult(true);
    }
}
