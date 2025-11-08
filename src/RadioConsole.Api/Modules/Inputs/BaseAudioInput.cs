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
