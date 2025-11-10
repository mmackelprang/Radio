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
    void RegisterEventInput(IAudioInput eventInput);
    
    /// <summary>
    /// Unregister an event audio input
    /// </summary>
    void UnregisterEventInput(IAudioInput eventInput);
    
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
    public IAudioInput? CurrentEvent { get; set; }
    
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
    private readonly Dictionary<string, IAudioInput> _registeredEvents = new();
    private readonly Dictionary<string, double> _savedVolumeStates = new();
    
    private bool _isEventPlaying;
    private IAudioInput? _currentEvent;
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
    public void RegisterEventInput(IAudioInput eventInput)
    {
        if (eventInput == null)
            throw new ArgumentNullException(nameof(eventInput));

        if (_registeredEvents.ContainsKey(eventInput.Id))
        {
            _logger.LogWarning("Event input {InputId} is already registered", eventInput.Id);
            return;
        }

        _registeredEvents[eventInput.Id] = eventInput;
        // Note: AudioDataAvailable event would be used for actual audio streaming
        // For event triggering, the old AudioEventTriggered pattern is replaced
        // by direct audio data streaming via AudioDataAvailable
        
        _logger.LogInformation("Registered event input: {InputName} ({InputId}) with priority {Priority}", 
            eventInput.Name, eventInput.Id, eventInput.Priority);
    }

    /// <summary>
    /// Unregister an event audio input
    /// </summary>
    public void UnregisterEventInput(IAudioInput eventInput)
    {
        if (eventInput == null)
            throw new ArgumentNullException(nameof(eventInput));

        if (_registeredEvents.Remove(eventInput.Id))
        {
            // Note: Event handler cleanup not needed with new AudioDataAvailable pattern
            _logger.LogInformation("Unregistered event input: {InputName} ({InputId})", 
                eventInput.Name, eventInput.Id);
        }
    }

    /// <summary>
    /// Handle high priority audio events (deprecated - now uses direct AudioDataAvailable streaming)
    /// </summary>
    [Obsolete("This method is deprecated. Audio priority handling now uses direct AudioDataAvailable streaming.")]
    private async void OnAudioEventTriggeredAsync(object? sender, AudioDataAvailableEventArgs e)
    {
        // This method is no longer used in the new architecture
        // Audio priority is now managed through the AudioMixer
        await Task.CompletedTask;
    }

    /// <summary>
    /// Handle incoming audio event with priority management (deprecated)
    /// </summary>
    [Obsolete("This method is deprecated. Audio priority handling now uses AudioMixer.")]
    private async Task HandleAudioEventAsync(AudioDataAvailableEventArgs eventArgs)
    {
        // This method is no longer used in the new architecture
        // Priority management is now handled by AudioMixer based on Priority property
        await Task.CompletedTask;
    }

    /// <summary>
    /// Legacy event handling - to be removed
    /// This functionality is now handled by AudioMixer
    /// </summary>
    [Obsolete]
    private async Task HandleAudioEventAsync_Legacy(IAudioInput eventInput)
    {
        await _semaphore.WaitAsync();
        
        try
        {
            _logger.LogInformation("Handling audio event from {InputName} with priority {Priority}", 
                eventInput.Name, eventInput.Priority);

            // Cancel any currently playing event of lower or equal priority
            if (_isEventPlaying && _currentEvent != null)
            {
                if (eventInput.Priority <= _currentEvent.Priority)
                {
                    _logger.LogInformation("Lower priority event ignored while {CurrentEvent} is playing", 
                        _currentEvent.Name);
                    return;
                }
                
                _logger.LogInformation("Interrupting {CurrentEvent} for higher priority event {NewEvent}", 
                    _currentEvent.Name, eventInput.Name);
                await StopCurrentEventAsync();
            }

            // Save current volume states
            await SaveVolumeStatesAsync();

            // Reduce or mute background audio
            await AdjustBackgroundAudioAsync();

            // Play the event audio
            _isEventPlaying = true;
            _currentEvent = eventInput;
            _currentEventCancellation = new CancellationTokenSource();

            // In the new architecture, this would be handled by AudioMixer
            // For now, just start the input and wait for duration
            await eventInput.StartAsync();
            if (eventInput.Duration.HasValue)
            {
                await Task.Delay(eventInput.Duration.Value, _currentEventCancellation.Token);
            }
            await eventInput.StopAsync();

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
    /// Play event audio (legacy - now handled by AudioMixer)
    /// </summary>
    [Obsolete]
    private async Task PlayEventAudioAsync(AudioDataAvailableEventArgs eventArgs, CancellationToken cancellationToken)
    {
        // This method is deprecated and should not be called in the new architecture
        // Audio mixing is now handled by AudioMixer
        await Task.CompletedTask;
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
        
        // Note: No need to unsubscribe from events as AudioEventTriggered no longer exists
        _registeredEvents.Clear();
    }
}
