using Bufdio;
using Bufdio.Engines;
using NAudio.Wave;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Modules.Outputs;

/// <summary>
/// Wired soundbar output module
/// 
/// This output uses Bufdio library with PortAudio for direct ALSA audio device access on Linux/Raspberry Pi.
/// Audio streams from the AudioMixer are converted to float samples and sent directly to the soundbar.
/// In simulation mode, audio data is consumed without actual playback.
/// </summary>
public class WiredSoundbarOutput : BaseAudioOutput
{
    private PortAudioEngine? _audioEngine;
    private readonly object _lockObject = new();
    private WaveFileWriter? _waveFileWriter;
    private string? _tempOutputFile;
    private bool _bufdioInitialized = false;

    // Audio format matching AudioMixer output
    private const int SAMPLE_RATE = 44100;
    private const int CHANNELS = 2;
    private const int BITS_PER_SAMPLE = 16;

    public override string Id => "wired_soundbar";
    public override string Name => "Wired Soundbar";
    public override string Description => "Wired Soundbar Connection (Bufdio/PortAudio)";

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
            // Initialize Bufdio PortAudio for actual hardware
            try
            {
                // Initialize PortAudio if not already initialized
                if (!BufdioLib.IsPortAudioInitialized)
                {
                    // Let Bufdio find PortAudio in system libraries
                    BufdioLib.InitializePortAudio(string.Empty);
                }
                _bufdioInitialized = true;
                
                // Check for actual soundbar connection
                IsAvailable = await CheckSoundbarConnectionAsync();
                _display.UpdateStatus(IsAvailable ? "Soundbar Connected" : "Soundbar Not Found");
            }
            catch (Exception ex)
            {
                // If PortAudio initialization fails, fall back to simulation behavior
                IsAvailable = true;
                _bufdioInitialized = false;
                _display.UpdateStatus($"Soundbar (Fallback Mode: {ex.Message})");
            }
        }
    }

    public override Task StartAsync()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Soundbar is not available");

        lock (_lockObject)
        {
            if (!_environmentService.IsSimulationMode && _bufdioInitialized)
            {
                // Initialize Bufdio PortAudioEngine for real audio output
                var options = new AudioEngineOptions(CHANNELS, SAMPLE_RATE, 0.05); // 50ms latency
                _audioEngine = new PortAudioEngine(options);
                
                // Optional: Create a debug WAV file to verify audio data
                var waveFormat = new WaveFormat(SAMPLE_RATE, BITS_PER_SAMPLE, CHANNELS);
                _tempOutputFile = Path.Combine(Path.GetTempPath(), $"soundbar_output_{DateTime.Now:yyyyMMdd_HHmmss}.wav");
                _waveFileWriter = new WaveFileWriter(_tempOutputFile, waveFormat);
            }

            IsActive = true;
            _display.UpdateStatus("Active");
            _display.UpdateMetadata(new Dictionary<string, string>
            {
                ["Volume"] = $"{(int)(_volume * 100)}%",
                ["Connection"] = "Wired",
                ["Engine"] = _bufdioInitialized ? "Bufdio/PortAudio" : "Simulation",
                ["OutputFile"] = _tempOutputFile ?? "None"
            });
        }
        
        return Task.CompletedTask;
    }

    public override Task StopAsync()
    {
        lock (_lockObject)
        {
            IsActive = false;
            
            if (_audioEngine != null)
            {
                _audioEngine.Dispose();
                _audioEngine = null;
            }
            
            if (_waveFileWriter != null)
            {
                _waveFileWriter.Dispose();
                _waveFileWriter = null;
            }

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

        // Read audio data from stream (16-bit PCM from AudioMixer)
        var buffer = new byte[audioStream.Length];
        var bytesRead = audioStream.Read(buffer, 0, buffer.Length);

        if (bytesRead == 0)
            return;

        lock (_lockObject)
        {
            // Write to debug WAV file if enabled
            if (_waveFileWriter != null)
            {
                _waveFileWriter.Write(buffer, 0, bytesRead);
                _waveFileWriter.Flush();
            }

            // Send to Bufdio PortAudioEngine if initialized
            if (_audioEngine != null)
            {
                // Convert 16-bit PCM byte[] to float[] samples for Bufdio
                var floatSamples = ConvertPcm16ToFloat(buffer, bytesRead);
                
                // Send samples to PortAudio
                _audioEngine.Send(floatSamples);
            }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Convert 16-bit PCM byte array to float samples for Bufdio
    /// </summary>
    private float[] ConvertPcm16ToFloat(byte[] pcmData, int byteCount)
    {
        int sampleCount = byteCount / 2; // 16-bit = 2 bytes per sample
        var floatSamples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            // Read 16-bit PCM sample (little endian)
            short pcmSample = (short)(pcmData[i * 2] | (pcmData[i * 2 + 1] << 8));
            
            // Convert to float [-1.0, 1.0]
            floatSamples[i] = pcmSample / 32768.0f;
        }

        return floatSamples;
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
