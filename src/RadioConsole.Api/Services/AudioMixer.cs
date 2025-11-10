using System.Collections.Concurrent;
using RadioConsole.Api.Interfaces;

namespace RadioConsole.Api.Services;

/// <summary>
/// Audio mixer that combines multiple PCM audio streams into a single output stream
/// Respects volume levels, priority, and handles sample format consistency
/// </summary>
public class AudioMixer : IDisposable
{
    private readonly ILogger<AudioMixer> _logger;
    private readonly IEnumerable<IAudioOutput> _outputs;
    private readonly ConcurrentDictionary<string, AudioSourceState> _activeSources = new();
    private readonly SemaphoreSlim _mixerLock = new(1, 1);
    private CancellationTokenSource? _mixingCts;
    private Task? _mixingTask;
    private bool _isRunning;

    // Standard output format for mixing
    private const int TARGET_SAMPLE_RATE = 44100;
    private const int TARGET_CHANNELS = 2;
    private const int TARGET_BITS_PER_SAMPLE = 16;
    private const int BUFFER_SIZE = 8192;

    public AudioMixer(
        IEnumerable<IAudioOutput> outputs,
        ILogger<AudioMixer> logger)
    {
        _outputs = outputs;
        _logger = logger;
    }

    /// <summary>
    /// Start the audio mixer
    /// </summary>
    public async Task StartAsync()
    {
        if (_isRunning)
        {
            _logger.LogWarning("AudioMixer is already running");
            return;
        }

        _logger.LogInformation("Starting AudioMixer");
        _isRunning = true;
        _mixingCts = new CancellationTokenSource();
        _mixingTask = Task.Run(() => MixingLoopAsync(_mixingCts.Token), _mixingCts.Token);

        await Task.CompletedTask;
    }

    /// <summary>
    /// Stop the audio mixer
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isRunning)
            return;

        _logger.LogInformation("Stopping AudioMixer");
        _isRunning = false;

        // Cancel mixing
        _mixingCts?.Cancel();
        
        if (_mixingTask != null)
        {
            try
            {
                await _mixingTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
            }
        }

        // Clear all sources
        _activeSources.Clear();

        _mixingCts?.Dispose();
        _mixingCts = null;
        _mixingTask = null;
    }

    /// <summary>
    /// Register an audio input source with the mixer
    /// </summary>
    public void RegisterSource(IAudioInput input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        var state = new AudioSourceState
        {
            Input = input,
            Volume = 1.0,
            IsActive = false,
            LastDataTimestamp = DateTime.UtcNow
        };

        if (_activeSources.TryAdd(input.Id, state))
        {
            // Subscribe to audio data events
            input.AudioDataAvailable += OnAudioDataAvailable;
            
            _logger.LogInformation("Registered audio source: {InputName} ({InputId})", 
                input.Name, input.Id);
        }
        else
        {
            _logger.LogWarning("Audio source {InputId} is already registered", input.Id);
        }
    }

    /// <summary>
    /// Unregister an audio input source from the mixer
    /// </summary>
    public void UnregisterSource(IAudioInput input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        if (_activeSources.TryRemove(input.Id, out var state))
        {
            // Unsubscribe from audio data events
            input.AudioDataAvailable -= OnAudioDataAvailable;
            
            _logger.LogInformation("Unregistered audio source: {InputName} ({InputId})", 
                input.Name, input.Id);
        }
    }

    /// <summary>
    /// Set the volume for a specific audio source
    /// </summary>
    public void SetSourceVolume(string inputId, double volume)
    {
        if (volume < 0.0 || volume > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be between 0.0 and 1.0");
        }

        if (_activeSources.TryGetValue(inputId, out var state))
        {
            state.Volume = volume;
            _logger.LogDebug("Set volume for {InputId} to {Volume}", inputId, volume);
        }
        else
        {
            _logger.LogWarning("Cannot set volume for unknown source: {InputId}", inputId);
        }
    }

    /// <summary>
    /// Get the current state of the mixer
    /// </summary>
    public AudioMixerState GetState()
    {
        return new AudioMixerState
        {
            IsRunning = _isRunning,
            ActiveSourceCount = _activeSources.Count(s => s.Value.IsActive),
            TotalSourceCount = _activeSources.Count,
            Sources = _activeSources.Values.Select(s => new SourceInfo
            {
                InputId = s.Input.Id,
                InputName = s.Input.Name,
                Volume = s.Volume,
                IsActive = s.IsActive,
                Priority = s.Input.Priority,
                LastDataTimestamp = s.LastDataTimestamp
            }).ToList()
        };
    }

    /// <summary>
    /// Handle incoming audio data from sources
    /// </summary>
    private void OnAudioDataAvailable(object? sender, AudioDataAvailableEventArgs e)
    {
        if (!_isRunning)
            return;

        if (sender is not IAudioInput input)
            return;

        if (!_activeSources.TryGetValue(input.Id, out var state))
            return;

        try
        {
            // Convert and resample audio data to target format if needed
            var convertedData = ConvertAudioData(e, TARGET_SAMPLE_RATE, TARGET_CHANNELS, TARGET_BITS_PER_SAMPLE);

            // Apply volume
            ApplyVolume(convertedData, state.Volume);

            // Store in buffer for mixing
            state.IsActive = true;
            state.LastDataTimestamp = DateTime.UtcNow;
            state.CurrentBuffer.Enqueue(convertedData);

            // Limit buffer size to prevent memory issues
            while (state.CurrentBuffer.Count > 10)
            {
                state.CurrentBuffer.TryDequeue(out _);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audio data from {InputName}", input.Name);
        }
    }

    /// <summary>
    /// Main mixing loop that combines audio from all sources and sends to outputs
    /// </summary>
    private async Task MixingLoopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("AudioMixer mixing loop started");

        var mixedBuffer = new byte[BUFFER_SIZE];

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _mixerLock.WaitAsync(cancellationToken);

                try
                {
                    // Get all active sources sorted by priority
                    var activeSources = _activeSources.Values
                        .Where(s => s.IsActive && s.CurrentBuffer.Count > 0)
                        .OrderByDescending(s => s.Input.Priority)
                        .ToList();

                    if (activeSources.Count == 0)
                    {
                        // No active sources, wait a bit
                        await Task.Delay(10, cancellationToken);
                        continue;
                    }

                    // Check for concurrent playback rules
                    var sourcesToMix = new List<AudioSourceState>();
                    
                    // Add highest priority source
                    var highestPriority = activeSources.FirstOrDefault();
                    if (highestPriority != null)
                    {
                        sourcesToMix.Add(highestPriority);

                        // Add other sources if they allow concurrent playback
                        foreach (var source in activeSources.Skip(1))
                        {
                            if (source.Input.AllowConcurrent && highestPriority.Input.AllowConcurrent)
                            {
                                sourcesToMix.Add(source);
                            }
                        }
                    }

                    // Mix the audio data
                    Array.Clear(mixedBuffer, 0, mixedBuffer.Length);
                    int mixedLength = MixAudioSources(sourcesToMix, mixedBuffer);

                    if (mixedLength > 0)
                    {
                        // Send mixed audio to all outputs
                        await SendToOutputsAsync(mixedBuffer, mixedLength, cancellationToken);
                    }
                }
                finally
                {
                    _mixerLock.Release();
                }

                // Small delay to prevent tight loop
                await Task.Delay(1, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("AudioMixer mixing loop cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AudioMixer mixing loop");
        }
        finally
        {
            _logger.LogInformation("AudioMixer mixing loop stopped");
        }
    }

    /// <summary>
    /// Mix audio data from multiple sources into a single buffer
    /// </summary>
    private int MixAudioSources(List<AudioSourceState> sources, byte[] outputBuffer)
    {
        if (sources.Count == 0)
            return 0;

        int mixedLength = 0;
        var tempBuffer = new int[outputBuffer.Length / 2]; // 16-bit samples

        foreach (var source in sources)
        {
            if (!source.CurrentBuffer.TryDequeue(out var audioData))
                continue;

            // Convert bytes to samples and add to temp buffer
            int sampleCount = Math.Min(audioData.Length / 2, tempBuffer.Length);
            for (int i = 0; i < sampleCount; i++)
            {
                short sample = (short)(audioData[i * 2] | (audioData[i * 2 + 1] << 8));
                tempBuffer[i] += sample;
            }

            mixedLength = Math.Max(mixedLength, audioData.Length);
        }

        // Convert mixed samples back to bytes with clipping
        for (int i = 0; i < mixedLength / 2; i++)
        {
            int sample = tempBuffer[i];
            
            // Clip to prevent overflow
            if (sample > short.MaxValue)
                sample = short.MaxValue;
            else if (sample < short.MinValue)
                sample = short.MinValue;

            outputBuffer[i * 2] = (byte)(sample & 0xFF);
            outputBuffer[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }

        return mixedLength;
    }

    /// <summary>
    /// Send mixed audio to all active outputs
    /// </summary>
    private async Task SendToOutputsAsync(byte[] audioData, int length, CancellationToken cancellationToken)
    {
        var outputTasks = _outputs
            .Where(o => o.IsActive)
            .Select(async output =>
            {
                try
                {
                    var stream = new MemoryStream(audioData, 0, length);
                    await output.SendAudioAsync(stream);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending audio to output {OutputName}", output.Name);
                }
            });

        await Task.WhenAll(outputTasks);
    }

    /// <summary>
    /// Convert audio data to target format
    /// Note: This is a simplified implementation. Production code would use proper resampling.
    /// </summary>
    private byte[] ConvertAudioData(AudioDataAvailableEventArgs e, int targetRate, int targetChannels, int targetBits)
    {
        // For now, assume data is already in correct format
        // In production, you would use a library like NAudio for proper conversion/resampling
        if (e.SampleRate == targetRate && e.Channels == targetChannels && e.BitsPerSample == targetBits)
        {
            return e.AudioData;
        }

        _logger.LogWarning("Audio format conversion needed but not implemented. " +
            "Source: {SampleRate}Hz, {Channels}ch, {Bits}bit. " +
            "Target: {TargetRate}Hz, {TargetChannels}ch, {TargetBits}bit",
            e.SampleRate, e.Channels, e.BitsPerSample,
            targetRate, targetChannels, targetBits);

        // Return original data for now
        return e.AudioData;
    }

    /// <summary>
    /// Apply volume adjustment to audio data
    /// </summary>
    private void ApplyVolume(byte[] audioData, double volume)
    {
        if (Math.Abs(volume - 1.0) < 0.01)
            return; // No adjustment needed

        // Apply volume to 16-bit PCM samples
        for (int i = 0; i < audioData.Length - 1; i += 2)
        {
            short sample = (short)(audioData[i] | (audioData[i + 1] << 8));
            sample = (short)(sample * volume);
            audioData[i] = (byte)(sample & 0xFF);
            audioData[i + 1] = (byte)((sample >> 8) & 0xFF);
        }
    }

    public void Dispose()
    {
        StopAsync().Wait();
        _mixerLock?.Dispose();
    }
}

/// <summary>
/// State of an audio source in the mixer
/// </summary>
internal class AudioSourceState
{
    public IAudioInput Input { get; set; } = null!;
    public double Volume { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastDataTimestamp { get; set; }
    public ConcurrentQueue<byte[]> CurrentBuffer { get; set; } = new();
}

/// <summary>
/// Current state of the audio mixer
/// </summary>
public class AudioMixerState
{
    public bool IsRunning { get; set; }
    public int ActiveSourceCount { get; set; }
    public int TotalSourceCount { get; set; }
    public List<SourceInfo> Sources { get; set; } = new();
}

/// <summary>
/// Information about an audio source
/// </summary>
public class SourceInfo
{
    public string InputId { get; set; } = string.Empty;
    public string InputName { get; set; } = string.Empty;
    public double Volume { get; set; }
    public bool IsActive { get; set; }
    public EventPriority Priority { get; set; }
    public DateTime LastDataTimestamp { get; set; }
}
