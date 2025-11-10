using NAudio.Wave;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Modules.Inputs;

/// <summary>
/// Audio input that plays audio from MP3 or WAV files using NAudio
/// </summary>
public class FileAudioInput : BaseAudioInput
{
    private readonly string _filePath;
    private AudioFileReader? _audioFileReader;
    private WaveStream? _waveStream;
    private CancellationTokenSource? _playbackCts;
    private Task? _playbackTask;
    private int _currentRepeatCount = 0;

    public override string Id { get; }
    public override string Name { get; }
    public override string Description { get; }
    public override EventPriority Priority { get; }
    
    private TimeSpan? _duration;
    public override TimeSpan? Duration 
    { 
        get => _duration;
        protected set => _duration = value;
    }

    /// <summary>
    /// Create a FileAudioInput for playing MP3 or WAV files
    /// </summary>
    /// <param name="filePath">Path to the audio file (MP3 or WAV)</param>
    /// <param name="name">Display name for this input</param>
    /// <param name="priority">Priority level for this audio (default: Medium)</param>
    /// <param name="environmentService">Environment service</param>
    /// <param name="storage">Storage service</param>
    public FileAudioInput(
        string filePath,
        string name,
        EventPriority priority,
        IEnvironmentService environmentService,
        IStorage storage) : base(environmentService, storage)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty", nameof(filePath));
        }

        _filePath = filePath;
        Name = name;
        Priority = priority;
        Id = $"file_audio_{Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant().Replace(" ", "_")}";
        Description = $"Audio file: {Path.GetFileName(filePath)}";
    }

    public override async Task InitializeAsync()
    {
        await _configuration.LoadAsync();

        if (_environmentService.IsSimulationMode)
        {
            // In simulation mode, assume file is available
            IsAvailable = true;
            Duration = TimeSpan.FromSeconds(5); // Mock duration
            _display.UpdateStatus($"{Name} (Simulation Mode)");
        }
        else
        {
            // Check if file exists and is a supported format
            if (File.Exists(_filePath))
            {
                try
                {
                    var extension = Path.GetExtension(_filePath).ToLowerInvariant();
                    if (extension == ".mp3" || extension == ".wav")
                    {
                        // Try to load the file to verify it's valid
                        using var reader = new AudioFileReader(_filePath);
                        Duration = reader.TotalTime;
                        IsAvailable = true;
                        _display.UpdateStatus($"{Name} Ready");
                    }
                    else
                    {
                        IsAvailable = false;
                        _display.UpdateStatus($"Unsupported format: {extension}");
                    }
                }
                catch (Exception ex)
                {
                    IsAvailable = false;
                    _display.UpdateStatus($"Error loading file: {ex.Message}");
                }
            }
            else
            {
                IsAvailable = false;
                _display.UpdateStatus("File not found");
            }
        }
    }

    public override async Task StartAsync()
    {
        if (!IsAvailable)
            throw new InvalidOperationException($"{Name} is not available");

        if (IsActive)
            return; // Already playing

        IsActive = true;
        IsPaused = false;
        _currentRepeatCount = 0;

        if (_environmentService.IsSimulationMode)
        {
            _display.UpdateStatus($"Playing {Name}");
            _display.UpdateMetadata(new Dictionary<string, string>
            {
                ["File"] = Path.GetFileName(_filePath),
                ["Status"] = "Playing (Simulation)",
                ["Duration"] = Duration?.ToString(@"mm\:ss") ?? "N/A"
            });
        }
        else
        {
            // Start playback
            await StartPlaybackAsync();
        }
    }

    public override async Task StopAsync()
    {
        IsActive = false;
        IsPaused = false;

        // Cancel playback
        _playbackCts?.Cancel();
        
        // Wait for playback to finish
        if (_playbackTask != null)
        {
            try
            {
                await _playbackTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
            }
        }

        // Cleanup resources
        _audioFileReader?.Dispose();
        _audioFileReader = null;
        _waveStream?.Dispose();
        _waveStream = null;
        _playbackCts?.Dispose();
        _playbackCts = null;
        _playbackTask = null;

        _display.UpdateStatus($"{Name} stopped");
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

        if (_audioFileReader != null)
        {
            return Task.FromResult<Stream?>(_audioFileReader);
        }

        return Task.FromResult<Stream?>(null);
    }

    private async Task StartPlaybackAsync()
    {
        _playbackCts = new CancellationTokenSource();
        _playbackTask = Task.Run(async () => await PlaybackLoopAsync(_playbackCts.Token), _playbackCts.Token);
        await Task.CompletedTask;
    }

    private async Task PlaybackLoopAsync(CancellationToken cancellationToken)
    {
        do
        {
            try
            {
                _audioFileReader = new AudioFileReader(_filePath);
                
                // Set volume
                _audioFileReader.Volume = (float)_volume;

                var buffer = new byte[8192];
                int bytesRead;

                _display.UpdateStatus($"Playing {Name}");

                while ((bytesRead = _audioFileReader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    // Wait if paused
                    while (IsPaused && !cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(100, cancellationToken);
                    }

                    if (cancellationToken.IsCancellationRequested)
                        break;

                    // Fire audio data available event
                    var audioData = new byte[bytesRead];
                    Array.Copy(buffer, audioData, bytesRead);

                    OnAudioDataAvailable(new AudioDataAvailableEventArgs
                    {
                        AudioData = audioData,
                        SampleRate = _audioFileReader.WaveFormat.SampleRate,
                        Channels = _audioFileReader.WaveFormat.Channels,
                        BitsPerSample = _audioFileReader.WaveFormat.BitsPerSample,
                        Timestamp = DateTime.UtcNow
                    });
                }

                _audioFileReader?.Dispose();
                _audioFileReader = null;

                // Handle repeat
                if (_repeatCount == 0) // Infinite repeat
                {
                    continue;
                }
                else if (_repeatCount > 0)
                {
                    _currentRepeatCount++;
                    if (_currentRepeatCount < _repeatCount)
                    {
                        continue;
                    }
                }

                // Playback finished
                break;
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _display.UpdateStatus($"Error: {ex.Message}");
                break;
            }
        } while (!cancellationToken.IsCancellationRequested);

        if (!cancellationToken.IsCancellationRequested)
        {
            IsActive = false;
            _display.UpdateStatus($"{Name} finished");
        }
    }
}
