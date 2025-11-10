using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Modules.Inputs;

/// <summary>
/// Base implementation for audio inputs with common functionality
/// </summary>
public abstract class BaseAudioInput : IAudioInput
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    
    /// <summary>
    /// Type of audio input - defaults to Music for backward compatibility
    /// </summary>
    public virtual AudioInputType InputType => AudioInputType.Music;
    
    public virtual bool IsAvailable { get; protected set; }
    public virtual bool IsActive { get; protected set; }

    /// <summary>
    /// Priority level - defaults to Low for music inputs
    /// </summary>
    public virtual EventPriority Priority => EventPriority.Low;

    /// <summary>
    /// Duration - defaults to null (indefinite) for music inputs
    /// </summary>
    public virtual TimeSpan? Duration { get; protected set; }

    /// <summary>
    /// Whether this input is currently paused
    /// </summary>
    public virtual bool IsPaused { get; protected set; }

    /// <summary>
    /// Current volume level (0.0 to 1.0)
    /// </summary>
    protected double _volume = 1.0;

    /// <summary>
    /// Repeat count (0 = infinite, -1 = no repeat)
    /// </summary>
    protected int _repeatCount = -1;

    /// <summary>
    /// Whether this stream can play concurrently with others
    /// </summary>
    public virtual bool AllowConcurrent { get; set; } = false;

    /// <summary>
    /// Event triggered when PCM audio data is available
    /// </summary>
    public event EventHandler<AudioDataAvailableEventArgs>? AudioDataAvailable;

    protected readonly IEnvironmentService _environmentService;
    protected readonly IStorage _storage;
    protected readonly BaseDisplay _display;
    protected readonly BaseConfiguration _configuration;

    protected BaseAudioInput(IEnvironmentService environmentService, IStorage storage)
    {
        _environmentService = environmentService;
        _storage = storage;
        _display = new BaseDisplay();
        _configuration = new BaseConfiguration(storage, $"input_{Id}");
    }

    public abstract Task InitializeAsync();
    public abstract Task StartAsync();
    public abstract Task StopAsync();
    public abstract Task<Stream?> GetAudioStreamAsync();

    /// <summary>
    /// Pause the audio stream
    /// </summary>
    public virtual Task PauseAsync()
    {
        IsPaused = true;
        _display.UpdateStatus($"{Name} Paused");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Resume the audio stream if paused
    /// </summary>
    public virtual Task ResumeAsync()
    {
        IsPaused = false;
        _display.UpdateStatus($"{Name} Resumed");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Set the preferred volume of the audio stream (0.0 to 1.0)
    /// </summary>
    public virtual Task SetVolumeAsync(double volume)
    {
        if (volume < 0.0 || volume > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be between 0.0 and 1.0");
        }

        _volume = volume;
        _display.UpdateMetadata(new Dictionary<string, string>
        {
            ["Volume"] = $"{(int)(_volume * 100)}%"
        });
        return Task.CompletedTask;
    }

    /// <summary>
    /// Set repeat mode
    /// </summary>
    public virtual void SetRepeat(int repeatCount)
    {
        _repeatCount = repeatCount;
    }

    /// <summary>
    /// Raise the AudioDataAvailable event with PCM data
    /// </summary>
    protected virtual void OnAudioDataAvailable(AudioDataAvailableEventArgs e)
    {
        AudioDataAvailable?.Invoke(this, e);
    }

    public IDeviceConfiguration GetConfiguration() => _configuration;
    public IDisplay GetDisplay() => _display;

    protected class BaseDisplay : IDisplay
    {
        private Dictionary<string, string> _metadata = new();
        private string _statusMessage = "Ready";

        public Dictionary<string, string> GetMetadata() => new(_metadata);
        public string GetStatusMessage() => _statusMessage;
        public event EventHandler? DisplayChanged;

        public void UpdateMetadata(Dictionary<string, string> metadata)
        {
            _metadata = metadata;
            DisplayChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateStatus(string status)
        {
            _statusMessage = status;
            DisplayChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    protected class BaseConfiguration : IDeviceConfiguration
    {
        private readonly Dictionary<string, object> _config = new();
        private readonly IStorage _storage;
        private readonly string _storageKey;

        public BaseConfiguration(IStorage storage, string storageKey)
        {
            _storage = storage;
            _storageKey = storageKey;
        }

        public IEnumerable<string> GetConfigurationKeys() => _config.Keys;

        public T? GetValue<T>(string key)
        {
            if (_config.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return default;
        }

        public void SetValue<T>(string key, T value)
        {
            if (value != null)
                _config[key] = value;
        }

        public async Task SaveAsync()
        {
            await _storage.SaveAsync(_storageKey, _config);
        }

        public async Task LoadAsync()
        {
            var loaded = await _storage.LoadAsync<Dictionary<string, object>>(_storageKey);
            if (loaded != null)
            {
                _config.Clear();
                foreach (var kvp in loaded)
                    _config[kvp.Key] = kvp.Value;
            }
        }
    }
}
