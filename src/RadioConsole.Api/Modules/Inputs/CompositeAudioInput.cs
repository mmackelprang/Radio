using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Modules.Inputs;

/// <summary>
/// Audio input that combines multiple file and TTS inputs to create composite audio
/// Can play items serially or concurrently with volume levels and repeat instructions
/// </summary>
public class CompositeAudioInput : BaseAudioInput
{
    public override AudioInputType InputType => AudioInputType.Event;
    private readonly List<IAudioInput> _inputs = new();
    private readonly bool _playSerially;
    private readonly Dictionary<IAudioInput, double> _volumeLevels = new();
    private CancellationTokenSource? _playbackCts;
    private Task? _playbackTask;

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
    /// Create a CompositeAudioInput from a list of file paths and text
    /// </summary>
    /// <param name="id">Unique identifier</param>
    /// <param name="name">Display name</param>
    /// <param name="priority">Priority level</param>
    /// <param name="playSerially">If true, play items one after another; if false, play concurrently</param>
    /// <param name="environmentService">Environment service</param>
    /// <param name="storage">Storage service</param>
    public CompositeAudioInput(
        string id,
        string name,
        EventPriority priority,
        bool playSerially,
        IEnvironmentService environmentService,
        IStorage storage) : base(environmentService, storage)
    {
        Id = id;
        Name = name;
        Priority = priority;
        _playSerially = playSerially;
        Description = $"Composite audio: {name} ({(playSerially ? "Serial" : "Concurrent")})";
    }

    /// <summary>
    /// Add a file audio input to the composite
    /// </summary>
    public void AddFileInput(string filePath, double volume = 1.0, int repeatCount = -1)
    {
        var fileInput = new FileAudioInput(
            filePath,
            Path.GetFileNameWithoutExtension(filePath),
            EventPriority.Medium,
            _environmentService,
            _storage);

        fileInput.SetRepeat(repeatCount);
        _volumeLevels[fileInput] = volume;
        _inputs.Add(fileInput);
    }

    /// <summary>
    /// Add a TTS audio input to the composite
    /// </summary>
    public void AddTtsInput(string text, ITtsService ttsService, double volume = 1.0, int repeatCount = -1)
    {
        var ttsInput = new TtsAudioInput(
            _environmentService,
            _storage,
            ttsService);

        ttsInput.SetRepeat(repeatCount);
        _volumeLevels[ttsInput] = volume;
        _inputs.Add(ttsInput);
        
        // Trigger TTS generation asynchronously
        _ = Task.Run(async () =>
        {
            try
            {
                await ttsInput.AnnounceTextAsync(text);
            }
            catch
            {
                // Ignore errors during initialization
            }
        });
    }

    public override async Task InitializeAsync()
    {
        await _configuration.LoadAsync();

        // Initialize all child inputs
        var initTasks = _inputs.Select(input => input.InitializeAsync());
        await Task.WhenAll(initTasks);

        // Check if all inputs are available
        IsAvailable = _inputs.All(i => i.IsAvailable);

        // Calculate total duration
        if (_playSerially)
        {
            // Sum of all durations
            Duration = TimeSpan.Zero;
            foreach (var input in _inputs)
            {
                if (input.Duration.HasValue)
                {
                    Duration = Duration.Value.Add(input.Duration.Value);
                }
                else
                {
                    Duration = null; // One indefinite input makes the whole composite indefinite
                    break;
                }
            }
        }
        else
        {
            // Max of all durations
            Duration = _inputs.Max(i => i.Duration);
        }

        _display.UpdateStatus(IsAvailable 
            ? $"{Name} Ready ({_inputs.Count} items)" 
            : $"{Name} Not Ready");
    }

    public override Task StartAsync()
    {
        if (!IsAvailable)
            throw new InvalidOperationException($"{Name} is not available");

        if (IsActive)
            return Task.CompletedTask; // Already playing

        IsActive = true;
        IsPaused = false;

        _display.UpdateStatus($"Playing {Name}");
        _display.UpdateMetadata(new Dictionary<string, string>
        {
            ["Items"] = _inputs.Count.ToString(),
            ["Mode"] = _playSerially ? "Serial" : "Concurrent",
            ["Duration"] = Duration?.ToString(@"mm\:ss") ?? "N/A"
        });

        // Subscribe to audio data from all inputs
        foreach (var input in _inputs)
        {
            input.AudioDataAvailable += OnChildAudioDataAvailable;
        }

        // Start playback
        _playbackCts = new CancellationTokenSource();
        _playbackTask = _playSerially 
            ? PlaySeriallyAsync(_playbackCts.Token) 
            : PlayConcurrentlyAsync(_playbackCts.Token);
        
        return Task.CompletedTask;
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

        // Stop all inputs
        var stopTasks = _inputs.Select(input => input.StopAsync());
        await Task.WhenAll(stopTasks);

        // Unsubscribe from events
        foreach (var input in _inputs)
        {
            input.AudioDataAvailable -= OnChildAudioDataAvailable;
        }

        _playbackCts?.Dispose();
        _playbackCts = null;
        _playbackTask = null;

        _display.UpdateStatus($"{Name} stopped");
    }

    public override async Task PauseAsync()
    {
        if (!IsActive)
            throw new InvalidOperationException($"{Name} is not active");

        IsPaused = true;

        // Pause all active inputs
        var pauseTasks = _inputs.Where(i => i.IsActive).Select(i => i.PauseAsync());
        await Task.WhenAll(pauseTasks);

        _display.UpdateStatus($"{Name} paused");
    }

    public override async Task ResumeAsync()
    {
        if (!IsActive)
            throw new InvalidOperationException($"{Name} is not active");

        IsPaused = false;

        // Resume all paused inputs
        var resumeTasks = _inputs.Where(i => i.IsActive).Select(i => i.ResumeAsync());
        await Task.WhenAll(resumeTasks);

        _display.UpdateStatus($"{Name} resumed");
    }

    public override Task<Stream?> GetAudioStreamAsync()
    {
        // Composite doesn't have a single stream
        return Task.FromResult<Stream?>(null);
    }

    private async Task PlaySeriallyAsync(CancellationToken cancellationToken)
    {
        try
        {
            foreach (var input in _inputs)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                // Set volume
                if (_volumeLevels.TryGetValue(input, out var volume))
                {
                    await input.SetVolumeAsync(volume);
                }

                // Start the input
                await input.StartAsync();

                // Wait for it to finish
                while (input.IsActive && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(100, cancellationToken);
                }

                // Stop the input
                if (input.IsActive)
                {
                    await input.StopAsync();
                }
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                IsActive = false;
                _display.UpdateStatus($"{Name} finished");
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _display.UpdateStatus($"Error: {ex.Message}");
            IsActive = false;
        }
    }

    private async Task PlayConcurrentlyAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Set volumes and start all inputs concurrently
            var tasks = new List<Task>();

            foreach (var input in _inputs)
            {
                if (_volumeLevels.TryGetValue(input, out var volume))
                {
                    await input.SetVolumeAsync(volume);
                }
                tasks.Add(input.StartAsync());
            }

            await Task.WhenAll(tasks);

            // Wait for all inputs to finish
            while (_inputs.Any(i => i.IsActive) && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100, cancellationToken);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                IsActive = false;
                _display.UpdateStatus($"{Name} finished");
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _display.UpdateStatus($"Error: {ex.Message}");
            IsActive = false;
        }
    }

    private void OnChildAudioDataAvailable(object? sender, AudioDataAvailableEventArgs e)
    {
        if (!IsActive || IsPaused)
            return;

        // Forward the audio data event
        // In a real implementation, this would mix the audio if playing concurrently
        OnAudioDataAvailable(e);
    }
}
