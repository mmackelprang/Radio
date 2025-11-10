using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Models;

namespace RadioConsole.Api.Services;

/// <summary>
/// Manages registry of configured audio devices
/// </summary>
public class DeviceRegistry : IDeviceRegistry
{
    private const string INPUT_REGISTRY_KEY = "device_registry_inputs";
    private const string OUTPUT_REGISTRY_KEY = "device_registry_outputs";

    private readonly IStorage _storage;
    private readonly IDeviceFactory _deviceFactory;
    private readonly ILogger<DeviceRegistry> _logger;
    
    private readonly Dictionary<string, DeviceConfiguration> _inputConfigs = new();
    private readonly Dictionary<string, DeviceConfiguration> _outputConfigs = new();
    private readonly Dictionary<string, IAudioInput> _loadedInputs = new();
    private readonly Dictionary<string, IAudioOutput> _loadedOutputs = new();

    public DeviceRegistry(
        IStorage storage,
        IDeviceFactory deviceFactory,
        ILogger<DeviceRegistry> logger)
    {
        _storage = storage;
        _deviceFactory = deviceFactory;
        _logger = logger;
    }

    /// <summary>
    /// Load all device configurations from storage
    /// </summary>
    public async Task LoadConfigurationsAsync()
    {
        try
        {
            // Load input configurations
            var inputs = await _storage.LoadAsync<List<DeviceConfiguration>>(INPUT_REGISTRY_KEY);
            if (inputs != null)
            {
                foreach (var config in inputs.Where(c => c.IsEnabled))
                {
                    _inputConfigs[config.Id] = config;
                    
                    try
                    {
                        var device = _deviceFactory.CreateInput(config);
                        await device.InitializeAsync();
                        _loadedInputs[config.Id] = device;
                        _logger.LogInformation("Loaded input device: {Name} ({Type})", config.Name, config.DeviceType);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to load input device: {Name} ({Type})", config.Name, config.DeviceType);
                    }
                }
            }

            // Load output configurations
            var outputs = await _storage.LoadAsync<List<DeviceConfiguration>>(OUTPUT_REGISTRY_KEY);
            if (outputs != null)
            {
                foreach (var config in outputs.Where(c => c.IsEnabled))
                {
                    _outputConfigs[config.Id] = config;
                    
                    try
                    {
                        var device = _deviceFactory.CreateOutput(config);
                        await device.InitializeAsync();
                        _loadedOutputs[config.Id] = device;
                        _logger.LogInformation("Loaded output device: {Name} ({Type})", config.Name, config.DeviceType);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to load output device: {Name} ({Type})", config.Name, config.DeviceType);
                    }
                }
            }

            _logger.LogInformation("Device registry loaded: {InputCount} inputs, {OutputCount} outputs",
                _loadedInputs.Count, _loadedOutputs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading device configurations");
        }
    }

    /// <summary>
    /// Save all device configurations to storage
    /// </summary>
    private async Task SaveConfigurationsAsync()
    {
        await _storage.SaveAsync(INPUT_REGISTRY_KEY, _inputConfigs.Values.ToList());
        await _storage.SaveAsync(OUTPUT_REGISTRY_KEY, _outputConfigs.Values.ToList());
    }

    /// <summary>
    /// Add a new input device configuration
    /// </summary>
    public async Task<DeviceConfiguration> AddInputAsync(DeviceConfiguration config)
    {
        config.Id = Guid.NewGuid().ToString();
        config.CreatedAt = DateTime.UtcNow;
        config.ModifiedAt = DateTime.UtcNow;

        _inputConfigs[config.Id] = config;
        await SaveConfigurationsAsync();

        // Try to load the device
        if (config.IsEnabled)
        {
            try
            {
                var device = _deviceFactory.CreateInput(config);
                await device.InitializeAsync();
                _loadedInputs[config.Id] = device;
                _logger.LogInformation("Added and loaded input device: {Name} ({Type})", config.Name, config.DeviceType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load newly added input device: {Name} ({Type})", config.Name, config.DeviceType);
            }
        }

        return config;
    }

    /// <summary>
    /// Add a new output device configuration
    /// </summary>
    public async Task<DeviceConfiguration> AddOutputAsync(DeviceConfiguration config)
    {
        config.Id = Guid.NewGuid().ToString();
        config.CreatedAt = DateTime.UtcNow;
        config.ModifiedAt = DateTime.UtcNow;

        _outputConfigs[config.Id] = config;
        await SaveConfigurationsAsync();

        // Try to load the device
        if (config.IsEnabled)
        {
            try
            {
                var device = _deviceFactory.CreateOutput(config);
                await device.InitializeAsync();
                _loadedOutputs[config.Id] = device;
                _logger.LogInformation("Added and loaded output device: {Name} ({Type})", config.Name, config.DeviceType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load newly added output device: {Name} ({Type})", config.Name, config.DeviceType);
            }
        }

        return config;
    }

    /// <summary>
    /// Update an existing input device configuration
    /// </summary>
    public async Task<DeviceConfiguration?> UpdateInputAsync(string id, DeviceConfiguration config)
    {
        if (!_inputConfigs.ContainsKey(id))
            return null;

        config.Id = id;
        config.ModifiedAt = DateTime.UtcNow;
        config.CreatedAt = _inputConfigs[id].CreatedAt;

        _inputConfigs[id] = config;
        await SaveConfigurationsAsync();

        // Reload the device
        if (_loadedInputs.ContainsKey(id))
        {
            try
            {
                await _loadedInputs[id].StopAsync();
            }
            catch { }
            _loadedInputs.Remove(id);
        }

        if (config.IsEnabled)
        {
            try
            {
                var device = _deviceFactory.CreateInput(config);
                await device.InitializeAsync();
                _loadedInputs[id] = device;
                _logger.LogInformation("Updated and reloaded input device: {Name} ({Type})", config.Name, config.DeviceType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reload updated input device: {Name} ({Type})", config.Name, config.DeviceType);
            }
        }

        return config;
    }

    /// <summary>
    /// Update an existing output device configuration
    /// </summary>
    public async Task<DeviceConfiguration?> UpdateOutputAsync(string id, DeviceConfiguration config)
    {
        if (!_outputConfigs.ContainsKey(id))
            return null;

        config.Id = id;
        config.ModifiedAt = DateTime.UtcNow;
        config.CreatedAt = _outputConfigs[id].CreatedAt;

        _outputConfigs[id] = config;
        await SaveConfigurationsAsync();

        // Reload the device
        if (_loadedOutputs.ContainsKey(id))
        {
            try
            {
                await _loadedOutputs[id].StopAsync();
            }
            catch { }
            _loadedOutputs.Remove(id);
        }

        if (config.IsEnabled)
        {
            try
            {
                var device = _deviceFactory.CreateOutput(config);
                await device.InitializeAsync();
                _loadedOutputs[id] = device;
                _logger.LogInformation("Updated and reloaded output device: {Name} ({Type})", config.Name, config.DeviceType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reload updated output device: {Name} ({Type})", config.Name, config.DeviceType);
            }
        }

        return config;
    }

    /// <summary>
    /// Remove an input device configuration
    /// </summary>
    public async Task<bool> RemoveInputAsync(string id)
    {
        if (!_inputConfigs.ContainsKey(id))
            return false;

        // Stop and remove the loaded device
        if (_loadedInputs.ContainsKey(id))
        {
            try
            {
                await _loadedInputs[id].StopAsync();
            }
            catch { }
            _loadedInputs.Remove(id);
        }

        _inputConfigs.Remove(id);
        await SaveConfigurationsAsync();
        _logger.LogInformation("Removed input device: {Id}", id);

        return true;
    }

    /// <summary>
    /// Remove an output device configuration
    /// </summary>
    public async Task<bool> RemoveOutputAsync(string id)
    {
        if (!_outputConfigs.ContainsKey(id))
            return false;

        // Stop and remove the loaded device
        if (_loadedOutputs.ContainsKey(id))
        {
            try
            {
                await _loadedOutputs[id].StopAsync();
            }
            catch { }
            _loadedOutputs.Remove(id);
        }

        _outputConfigs.Remove(id);
        await SaveConfigurationsAsync();
        _logger.LogInformation("Removed output device: {Id}", id);

        return true;
    }

    /// <summary>
    /// Get all input configurations
    /// </summary>
    public IEnumerable<DeviceConfiguration> GetAllInputConfigs() => _inputConfigs.Values;

    /// <summary>
    /// Get all output configurations
    /// </summary>
    public IEnumerable<DeviceConfiguration> GetAllOutputConfigs() => _outputConfigs.Values;

    /// <summary>
    /// Get a specific input configuration
    /// </summary>
    public DeviceConfiguration? GetInputConfig(string id) => 
        _inputConfigs.TryGetValue(id, out var config) ? config : null;

    /// <summary>
    /// Get a specific output configuration
    /// </summary>
    public DeviceConfiguration? GetOutputConfig(string id) =>
        _outputConfigs.TryGetValue(id, out var config) ? config : null;

    /// <summary>
    /// Get all loaded input devices
    /// </summary>
    public IEnumerable<IAudioInput> GetAllInputs() => _loadedInputs.Values;

    /// <summary>
    /// Get all loaded output devices
    /// </summary>
    public IEnumerable<IAudioOutput> GetAllOutputs() => _loadedOutputs.Values;

    /// <summary>
    /// Get a specific loaded input device
    /// </summary>
    public IAudioInput? GetInput(string id) =>
        _loadedInputs.TryGetValue(id, out var device) ? device : null;

    /// <summary>
    /// Get a specific loaded output device
    /// </summary>
    public IAudioOutput? GetOutput(string id) =>
        _loadedOutputs.TryGetValue(id, out var device) ? device : null;
}
