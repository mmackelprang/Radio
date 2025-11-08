using RadioConsole.Api.Interfaces;

namespace RadioConsole.Api.Services;

/// <summary>
/// Configuration for audio priority management
/// </summary>
public class AudioPriorityManagerConfig
{
    /// <summary>
    /// Volume reduction level when event audio is playing (0.0 = mute, 1.0 = no change)
    /// </summary>
    public double VolumeReductionLevel { get; set; } = 0.1;
    
    /// <summary>
    /// Whether to mute background audio instead of reducing volume
    /// </summary>
    public bool MuteBackgroundAudio { get; set; } = false;
}

/// <summary>
/// Manages multiple audio sources with priority-based playback on Raspberry Pi 5 with PulseAudio
/// </summary>
public interface IAudioPriorityManager
{
    /// <summary>
    /// Configuration for the audio priority manager
    /// </summary>
    AudioPriorityManagerConfig Config { get; }
    
    /// <summary>
    /// Register an event audio input for monitoring
    /// </summary>
    void RegisterEventInput(IEventAudioInput eventInput);
    
    /// <summary>
    /// Unregister an event audio input
    /// </summary>
    void UnregisterEventInput(IEventAudioInput eventInput);
    
    /// <summary>
    /// Get the current state of all audio sources
    /// </summary>
    Task<AudioPriorityState> GetStateAsync();
}

/// <summary>
/// Represents the current state of the audio priority manager
/// </summary>
public class AudioPriorityState
{
    /// <summary>
    /// Whether an event is currently playing
    /// </summary>
    public bool IsEventPlaying { get; set; }
    
    /// <summary>
    /// Currently playing event (if any)
    /// </summary>
    public IEventAudioInput? CurrentEvent { get; set; }
    
    /// <summary>
    /// Saved volume states of background audio sources
    /// </summary>
    public Dictionary<string, double> SavedVolumeStates { get; set; } = new();
    
    /// <summary>
    /// List of registered event inputs
    /// </summary>
    public List<string> RegisteredEventInputs { get; set; } = new();
}

/// <summary>
/// Implementation of audio priority manager using async/await patterns
/// </summary>
public class AudioPriorityManager : IAudioPriorityManager, IDisposable
{
    private readonly IEnumerable<IAudioOutput> _audioOutputs;
    private readonly ILogger<AudioPriorityManager> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Dictionary<string, IEventAudioInput> _registeredEvents = new();
    private readonly Dictionary<string, double> _savedVolumeStates = new();
    
    private bool _isEventPlaying;
    private IEventAudioInput? _currentEvent;
    private CancellationTokenSource? _currentEventCancellation;

    public AudioPriorityManagerConfig Config { get; }

    public AudioPriorityManager(
        IEnumerable<IAudioOutput> audioOutputs,
        ILogger<AudioPriorityManager> logger,
        AudioPriorityManagerConfig? config = null)
    {
        _audioOutputs = audioOutputs;
        _logger = logger;
        Config = config ?? new AudioPriorityManagerConfig();
    }

    /// <summary>
    /// Register an event audio input for priority handling
    /// </summary>
    public void RegisterEventInput(IEventAudioInput eventInput)
    {
        if (eventInput == null)
            throw new ArgumentNullException(nameof(eventInput));

        if (_registeredEvents.ContainsKey(eventInput.Id))
        {
            _logger.LogWarning("Event input {InputId} is already registered", eventInput.Id);
            return;
        }

        _registeredEvents[eventInput.Id] = eventInput;
        eventInput.AudioEventTriggered += OnAudioEventTriggeredAsync;
        
        _logger.LogInformation("Registered event input: {InputName} ({InputId}) with priority {Priority}", 
            eventInput.Name, eventInput.Id, eventInput.Priority);
    }

    /// <summary>
    /// Unregister an event audio input
    /// </summary>
    public void UnregisterEventInput(IEventAudioInput eventInput)
    {
        if (eventInput == null)
            throw new ArgumentNullException(nameof(eventInput));

        if (_registeredEvents.Remove(eventInput.Id))
        {
            eventInput.AudioEventTriggered -= OnAudioEventTriggeredAsync;
            _logger.LogInformation("Unregistered event input: {InputName} ({InputId})", 
                eventInput.Name, eventInput.Id);
        }
    }

    /// <summary>
    /// Handle high priority audio events
    /// </summary>
    private async void OnAudioEventTriggeredAsync(object? sender, EventAudioEventArgs e)
    {
        try
        {
            await HandleAudioEventAsync(e);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling audio event from {InputName}", e.EventInput.Name);
        }
    }

    /// <summary>
    /// Handle incoming audio event with priority management
    /// </summary>
    private async Task HandleAudioEventAsync(EventAudioEventArgs eventArgs)
    {
        await _semaphore.WaitAsync();
        
        try
        {
            _logger.LogInformation("Handling audio event from {InputName} with priority {Priority}", 
                eventArgs.EventInput.Name, eventArgs.EventInput.Priority);

            // Cancel any currently playing event of lower or equal priority
            if (_isEventPlaying && _currentEvent != null)
            {
                if (eventArgs.EventInput.Priority <= _currentEvent.Priority)
                {
                    _logger.LogInformation("Lower priority event ignored while {CurrentEvent} is playing", 
                        _currentEvent.Name);
                    return;
                }
                
                _logger.LogInformation("Interrupting {CurrentEvent} for higher priority event {NewEvent}", 
                    _currentEvent.Name, eventArgs.EventInput.Name);
                await StopCurrentEventAsync();
            }

            // Save current volume states
            await SaveVolumeStatesAsync();

            // Reduce or mute background audio
            await AdjustBackgroundAudioAsync();

            // Play the event audio
            _isEventPlaying = true;
            _currentEvent = eventArgs.EventInput;
            _currentEventCancellation = new CancellationTokenSource();

            await PlayEventAudioAsync(eventArgs, _currentEventCancellation.Token);

            // Restore volumes after event completes
            await RestoreVolumeStatesAsync();

            _isEventPlaying = false;
            _currentEvent = null;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Event audio playback was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling audio event");
            
            // Ensure we restore volumes even on error
            try
            {
                await RestoreVolumeStatesAsync();
            }
            catch (Exception restoreEx)
            {
                _logger.LogError(restoreEx, "Failed to restore volume states after error");
            }
            
            _isEventPlaying = false;
            _currentEvent = null;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Save current volume states of all active outputs
    /// </summary>
    private async Task SaveVolumeStatesAsync()
    {
        _savedVolumeStates.Clear();

        foreach (var output in _audioOutputs)
        {
            if (output.IsActive)
            {
                try
                {
                    // Get current volume - in real implementation this would query PulseAudio
                    var currentVolume = await GetOutputVolumeAsync(output);
                    _savedVolumeStates[output.Id] = currentVolume;
                    
                    _logger.LogDebug("Saved volume state for {OutputName}: {Volume}", 
                        output.Name, currentVolume);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save volume state for {OutputName}", output.Name);
                }
            }
        }
    }

    /// <summary>
    /// Reduce or mute background audio based on configuration
    /// </summary>
    private async Task AdjustBackgroundAudioAsync()
    {
        var targetVolume = Config.MuteBackgroundAudio ? 0.0 : Config.VolumeReductionLevel;

        foreach (var output in _audioOutputs)
        {
            if (output.IsActive)
            {
                try
                {
                    var volumePercentage = (int)(targetVolume * 100);
                    await output.SetVolumeAsync(volumePercentage);
                    
                    _logger.LogDebug("Adjusted {OutputName} volume to {Volume}%", 
                        output.Name, volumePercentage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to adjust volume for {OutputName}", output.Name);
                }
            }
        }
    }

    /// <summary>
    /// Restore saved volume states
    /// </summary>
    private async Task RestoreVolumeStatesAsync()
    {
        foreach (var output in _audioOutputs)
        {
            if (output.IsActive && _savedVolumeStates.TryGetValue(output.Id, out var savedVolume))
            {
                try
                {
                    var volumePercentage = (int)(savedVolume * 100);
                    await output.SetVolumeAsync(volumePercentage);
                    
                    _logger.LogDebug("Restored {OutputName} volume to {Volume}%", 
                        output.Name, volumePercentage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to restore volume for {OutputName}", output.Name);
                }
            }
        }

        _savedVolumeStates.Clear();
    }

    /// <summary>
    /// Play event audio with optional timeout
    /// </summary>
    private async Task PlayEventAudioAsync(EventAudioEventArgs eventArgs, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Playing event audio from {InputName}", eventArgs.EventInput.Name);

        // Start the event input
        await eventArgs.EventInput.StartAsync();

        // If duration is specified, wait for that duration
        if (eventArgs.EventInput.Duration.HasValue)
        {
            await Task.Delay(eventArgs.EventInput.Duration.Value, cancellationToken);
        }
        else
        {
            // For indefinite duration, wait a reasonable default time (e.g., 5 seconds)
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }

        // Stop the event input
        await eventArgs.EventInput.StopAsync();
        
        _logger.LogInformation("Completed event audio from {InputName}", eventArgs.EventInput.Name);
    }

    /// <summary>
    /// Stop currently playing event
    /// </summary>
    private async Task StopCurrentEventAsync()
    {
        if (_currentEvent != null)
        {
            _currentEventCancellation?.Cancel();
            
            try
            {
                await _currentEvent.StopAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping current event {EventName}", _currentEvent.Name);
            }
        }
    }

    /// <summary>
    /// Get current volume of an output (mock implementation)
    /// In real implementation, this would query PulseAudio
    /// </summary>
    private Task<double> GetOutputVolumeAsync(IAudioOutput output)
    {
        // Mock implementation - in real world, query PulseAudio via pactl or API
        return Task.FromResult(0.7); // 70% volume
    }

    /// <summary>
    /// Get current state of the audio priority manager
    /// </summary>
    public async Task<AudioPriorityState> GetStateAsync()
    {
        await _semaphore.WaitAsync();
        
        try
        {
            return new AudioPriorityState
            {
                IsEventPlaying = _isEventPlaying,
                CurrentEvent = _currentEvent,
                SavedVolumeStates = new Dictionary<string, double>(_savedVolumeStates),
                RegisteredEventInputs = _registeredEvents.Keys.ToList()
            };
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        _currentEventCancellation?.Cancel();
        _currentEventCancellation?.Dispose();
        _semaphore.Dispose();
        
        foreach (var eventInput in _registeredEvents.Values)
        {
            eventInput.AudioEventTriggered -= OnAudioEventTriggeredAsync;
        }
    }
}
