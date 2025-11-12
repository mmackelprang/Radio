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
            // Sanitize inputId to prevent log injection
            var sanitizedInputId = inputId.Replace("\n", "").Replace("\r", "");
            _logger.LogDebug("Set volume for {InputId} to {Volume}", sanitizedInputId, volume);
        }
        else
        {
            // Sanitize inputId to prevent log injection
            var sanitizedInputId = inputId.Replace("\n", "").Replace("\r", "");
            _logger.LogWarning("Cannot set volume for unknown source: {InputId}", sanitizedInputId);
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
    /// Handles sample rate conversion, bit depth conversion, and channel conversion
    /// </summary>
    private byte[] ConvertAudioData(AudioDataAvailableEventArgs e, int targetRate, int targetChannels, int targetBits)
    {
        // If already in target format, return as-is
        if (e.SampleRate == targetRate && e.Channels == targetChannels && e.BitsPerSample == targetBits)
        {
            return e.AudioData;
        }

        try
        {
            byte[] converted = e.AudioData;

            // Step 1: Convert bit depth if needed
            if (e.BitsPerSample != targetBits)
            {
                converted = ConvertBitDepth(converted, e.Channels, e.BitsPerSample, targetBits);
            }

            // Step 2: Convert sample rate if needed
            if (e.SampleRate != targetRate)
            {
                converted = ConvertSampleRate(converted, e.Channels, targetBits, e.SampleRate, targetRate);
            }

            // Step 3: Convert channels if needed
            if (e.Channels != targetChannels)
            {
                converted = ConvertChannels(converted, e.Channels, targetChannels, targetBits);
            }

            return converted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting audio format. Source: {SampleRate}Hz, {Channels}ch, {Bits}bit. Target: {TargetRate}Hz, {TargetChannels}ch, {TargetBits}bit",
                e.SampleRate, e.Channels, e.BitsPerSample, targetRate, targetChannels, targetBits);
            
            // Return original data as fallback
            return e.AudioData;
        }
    }

    /// <summary>
    /// Convert bit depth of audio data
    /// </summary>
    private byte[] ConvertBitDepth(byte[] audioData, int channels, int sourceBits, int targetBits)
    {
        if (sourceBits == 32 && targetBits == 16)
        {
            // Convert 32-bit float to 16-bit PCM
            int sampleCount = audioData.Length / 4; // 32-bit = 4 bytes per sample
            byte[] converted = new byte[sampleCount * 2]; // 16-bit = 2 bytes per sample

            for (int i = 0; i < sampleCount; i++)
            {
                // Read 32-bit float sample
                float sample = BitConverter.ToSingle(audioData, i * 4);
                
                // Clamp to [-1.0, 1.0] range
                sample = Math.Clamp(sample, -1.0f, 1.0f);
                
                // Convert to 16-bit PCM
                short pcmSample = (short)(sample * short.MaxValue);
                
                // Write 16-bit sample
                converted[i * 2] = (byte)(pcmSample & 0xFF);
                converted[i * 2 + 1] = (byte)((pcmSample >> 8) & 0xFF);
            }

            return converted;
        }
        else if (sourceBits == 16 && targetBits == 32)
        {
            // Convert 16-bit PCM to 32-bit float
            int sampleCount = audioData.Length / 2; // 16-bit = 2 bytes per sample
            byte[] converted = new byte[sampleCount * 4]; // 32-bit = 4 bytes per sample

            for (int i = 0; i < sampleCount; i++)
            {
                // Read 16-bit PCM sample
                short pcmSample = (short)(audioData[i * 2] | (audioData[i * 2 + 1] << 8));
                
                // Convert to float [-1.0, 1.0]
                float sample = pcmSample / (float)short.MaxValue;
                
                // Write 32-bit float sample
                byte[] floatBytes = BitConverter.GetBytes(sample);
                Array.Copy(floatBytes, 0, converted, i * 4, 4);
            }

            return converted;
        }
        else if (sourceBits == 24 && targetBits == 16)
        {
            // Convert 24-bit PCM to 16-bit PCM
            int sampleCount = audioData.Length / 3; // 24-bit = 3 bytes per sample
            byte[] converted = new byte[sampleCount * 2]; // 16-bit = 2 bytes per sample

            for (int i = 0; i < sampleCount; i++)
            {
                // Read 24-bit sample (little endian)
                int sample24 = audioData[i * 3] | (audioData[i * 3 + 1] << 8) | (audioData[i * 3 + 2] << 16);
                
                // Sign extend if negative
                if ((sample24 & 0x800000) != 0)
                {
                    sample24 |= unchecked((int)0xFF000000);
                }
                
                // Convert to 16-bit by dropping the lower 8 bits
                short sample16 = (short)(sample24 >> 8);
                
                // Write 16-bit sample
                converted[i * 2] = (byte)(sample16 & 0xFF);
                converted[i * 2 + 1] = (byte)((sample16 >> 8) & 0xFF);
            }

            return converted;
        }

        // Unsupported conversion, return original
        _logger.LogWarning("Unsupported bit depth conversion: {SourceBits}-bit to {TargetBits}-bit", sourceBits, targetBits);
        return audioData;
    }

    /// <summary>
    /// Convert sample rate using linear interpolation
    /// </summary>
    private byte[] ConvertSampleRate(byte[] audioData, int channels, int bitsPerSample, int sourceRate, int targetRate)
    {
        int bytesPerSample = bitsPerSample / 8;
        int sourceSampleCount = audioData.Length / (bytesPerSample * channels);
        int targetSampleCount = (int)((long)sourceSampleCount * targetRate / sourceRate);
        byte[] converted = new byte[targetSampleCount * bytesPerSample * channels];

        double ratio = (double)sourceRate / targetRate;

        for (int targetSample = 0; targetSample < targetSampleCount; targetSample++)
        {
            double sourceSamplePos = targetSample * ratio;
            int sourceSample1 = (int)sourceSamplePos;
            int sourceSample2 = Math.Min(sourceSample1 + 1, sourceSampleCount - 1);
            double fraction = sourceSamplePos - sourceSample1;

            // Interpolate each channel
            for (int ch = 0; ch < channels; ch++)
            {
                if (bitsPerSample == 16)
                {
                    // 16-bit interpolation
                    int offset1 = (sourceSample1 * channels + ch) * 2;
                    int offset2 = (sourceSample2 * channels + ch) * 2;
                    
                    short sample1 = (short)(audioData[offset1] | (audioData[offset1 + 1] << 8));
                    short sample2 = (short)(audioData[offset2] | (audioData[offset2 + 1] << 8));
                    
                    short interpolated = (short)(sample1 + (sample2 - sample1) * fraction);
                    
                    int targetOffset = (targetSample * channels + ch) * 2;
                    converted[targetOffset] = (byte)(interpolated & 0xFF);
                    converted[targetOffset + 1] = (byte)((interpolated >> 8) & 0xFF);
                }
                else if (bitsPerSample == 32)
                {
                    // 32-bit float interpolation
                    int offset1 = (sourceSample1 * channels + ch) * 4;
                    int offset2 = (sourceSample2 * channels + ch) * 4;
                    
                    float sample1 = BitConverter.ToSingle(audioData, offset1);
                    float sample2 = BitConverter.ToSingle(audioData, offset2);
                    
                    float interpolated = sample1 + (float)((sample2 - sample1) * fraction);
                    
                    int targetOffset = (targetSample * channels + ch) * 4;
                    byte[] floatBytes = BitConverter.GetBytes(interpolated);
                    Array.Copy(floatBytes, 0, converted, targetOffset, 4);
                }
                else if (bitsPerSample == 24)
                {
                    // 24-bit interpolation
                    int offset1 = (sourceSample1 * channels + ch) * 3;
                    int offset2 = (sourceSample2 * channels + ch) * 3;
                    
                    int sample1 = audioData[offset1] | (audioData[offset1 + 1] << 8) | (audioData[offset1 + 2] << 16);
                    int sample2 = audioData[offset2] | (audioData[offset2 + 1] << 8) | (audioData[offset2 + 2] << 16);
                    
                    // Sign extend
                    if ((sample1 & 0x800000) != 0) sample1 |= unchecked((int)0xFF000000);
                    if ((sample2 & 0x800000) != 0) sample2 |= unchecked((int)0xFF000000);
                    
                    int interpolated = (int)(sample1 + (sample2 - sample1) * fraction);
                    
                    int targetOffset = (targetSample * channels + ch) * 3;
                    converted[targetOffset] = (byte)(interpolated & 0xFF);
                    converted[targetOffset + 1] = (byte)((interpolated >> 8) & 0xFF);
                    converted[targetOffset + 2] = (byte)((interpolated >> 16) & 0xFF);
                }
            }
        }

        return converted;
    }

    /// <summary>
    /// Convert number of channels
    /// </summary>
    private byte[] ConvertChannels(byte[] audioData, int sourceChannels, int targetChannels, int bitsPerSample)
    {
        int bytesPerSample = bitsPerSample / 8;
        int frameSizeSource = bytesPerSample * sourceChannels;
        int frameSizeTarget = bytesPerSample * targetChannels;
        int frameCount = audioData.Length / frameSizeSource;
        byte[] converted = new byte[frameCount * frameSizeTarget];

        if (sourceChannels == 1 && targetChannels == 2)
        {
            // Mono to stereo: duplicate the channel
            for (int frame = 0; frame < frameCount; frame++)
            {
                int sourceOffset = frame * frameSizeSource;
                int targetOffset = frame * frameSizeTarget;
                
                // Copy to left channel
                Array.Copy(audioData, sourceOffset, converted, targetOffset, bytesPerSample);
                // Copy to right channel
                Array.Copy(audioData, sourceOffset, converted, targetOffset + bytesPerSample, bytesPerSample);
            }
        }
        else if (sourceChannels == 2 && targetChannels == 1)
        {
            // Stereo to mono: average the channels
            for (int frame = 0; frame < frameCount; frame++)
            {
                int sourceOffset = frame * frameSizeSource;
                int targetOffset = frame * frameSizeTarget;
                
                if (bitsPerSample == 16)
                {
                    short left = (short)(audioData[sourceOffset] | (audioData[sourceOffset + 1] << 8));
                    short right = (short)(audioData[sourceOffset + 2] | (audioData[sourceOffset + 3] << 8));
                    short avg = (short)((left + right) / 2);
                    
                    converted[targetOffset] = (byte)(avg & 0xFF);
                    converted[targetOffset + 1] = (byte)((avg >> 8) & 0xFF);
                }
                else if (bitsPerSample == 32)
                {
                    float left = BitConverter.ToSingle(audioData, sourceOffset);
                    float right = BitConverter.ToSingle(audioData, sourceOffset + 4);
                    float avg = (left + right) / 2.0f;
                    
                    byte[] avgBytes = BitConverter.GetBytes(avg);
                    Array.Copy(avgBytes, 0, converted, targetOffset, 4);
                }
            }
        }
        else
        {
            // Unsupported channel conversion
            _logger.LogWarning("Unsupported channel conversion: {SourceChannels}ch to {TargetChannels}ch", sourceChannels, targetChannels);
            return audioData;
        }

        return converted;
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
