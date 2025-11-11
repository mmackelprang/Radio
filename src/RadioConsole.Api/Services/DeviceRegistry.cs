using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Models;

namespace RadioConsole.Api.Services;

/// <summary>
/// Category of audio device for generic operations
/// </summary>
internal enum AudioDeviceCategory
{
  Input,
  Output
}

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
        bool inputsHadDuplicates = await LoadDeviceConfigurationsAsync(
          INPUT_REGISTRY_KEY,
          AudioDeviceCategory.Input,
          _inputConfigs,
          _loadedInputs);

        // Load output configurations
        bool outputsHadDuplicates = await LoadDeviceConfigurationsAsync(
          OUTPUT_REGISTRY_KEY,
          AudioDeviceCategory.Output,
          _outputConfigs,
          _loadedOutputs);

        // If we found and removed duplicates, save the cleaned configurations
        if (inputsHadDuplicates || outputsHadDuplicates)
        {
          _logger.LogWarning("Duplicate device names detected and removed during configuration load. Saving cleaned configurations.");
          await SaveConfigurationsAsync();
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
    /// Helper method to load device configurations for a specific category
    /// </summary>
    private async Task<bool> LoadDeviceConfigurationsAsync<TDevice>(
      string storageKey,
      AudioDeviceCategory category,
      Dictionary<string, DeviceConfiguration> configDict,
      Dictionary<string, TDevice> deviceDict) where TDevice : IAudioDevice
    {
      var configs = await _storage.LoadAsync<List<DeviceConfiguration>>(storageKey);
      if (configs == null)
        return false;

      bool hadDuplicates = false;
      var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      foreach (var config in configs.Where(c => c.IsEnabled))
      {
        // Check for duplicate names
        if (seenNames.Contains(config.Name))
        {
          _logger.LogWarning("Skipping duplicate {Category} device name: {Name} (ID: {Id})",
            category, config.Name, config.Id);
          hadDuplicates = true;
          continue;
        }

        seenNames.Add(config.Name);
        configDict[config.Id] = config;

        try
        {
          TDevice device = category == AudioDeviceCategory.Input
            ? (TDevice)(object)_deviceFactory.CreateInput(config)
            : (TDevice)(object)_deviceFactory.CreateOutput(config);

          await device.InitializeAsync();
          deviceDict[config.Id] = device;
          _logger.LogInformation("Loaded {Category} device: {Name} ({Type})",
            category.ToString().ToLower(), config.Name, config.DeviceType);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to load {Category} device: {Name} ({Type})",
            category.ToString().ToLower(), config.Name, config.DeviceType);
        }
      }

      return hadDuplicates;
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
      return await AddDeviceAsync(config, AudioDeviceCategory.Input, _inputConfigs, _loadedInputs);
    }

    /// <summary>
    /// Add a new output device configuration
    /// </summary>
    public async Task<DeviceConfiguration> AddOutputAsync(DeviceConfiguration config)
    {
      return await AddDeviceAsync(config, AudioDeviceCategory.Output, _outputConfigs, _loadedOutputs);
    }

    /// <summary>
    /// Helper method to add a device configuration
    /// </summary>
    private async Task<DeviceConfiguration> AddDeviceAsync<TDevice>(
      DeviceConfiguration config,
      AudioDeviceCategory category,
      Dictionary<string, DeviceConfiguration> configDict,
      Dictionary<string, TDevice> deviceDict) where TDevice : IAudioDevice
    {
      // Check for duplicate names across both inputs and outputs
      if (IsNameDuplicate(config.Name, null))
      {
        throw new InvalidOperationException($"A device with the name '{config.Name}' already exists.");
      }

      config.Id = Guid.NewGuid().ToString();
      config.CreatedAt = DateTime.UtcNow;
      config.ModifiedAt = DateTime.UtcNow;

      configDict[config.Id] = config;
      await SaveConfigurationsAsync();

      // Try to load the device
      if (config.IsEnabled)
      {
        try
        {
          TDevice device = category == AudioDeviceCategory.Input
            ? (TDevice)(object)_deviceFactory.CreateInput(config)
            : (TDevice)(object)_deviceFactory.CreateOutput(config);

          await device.InitializeAsync();
          deviceDict[config.Id] = device;
          _logger.LogInformation("Added and loaded {Category} device: {Name} ({Type})",
            category.ToString().ToLower(), config.Name, config.DeviceType);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to load newly added {Category} device: {Name} ({Type})",
            category.ToString().ToLower(), config.Name, config.DeviceType);
        }
      }

      return config;
    }

    /// <summary>
    /// Check if a device name is a duplicate
    /// </summary>
    private bool IsNameDuplicate(string name, string? excludeId)
    {
      return _inputConfigs.Values.Any(c => c.Id != excludeId && 
        string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)) ||
        _outputConfigs.Values.Any(c => c.Id != excludeId && 
        string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Update an existing input device configuration
    /// </summary>
    public async Task<DeviceConfiguration?> UpdateInputAsync(string id, DeviceConfiguration config)
    {
      return await UpdateDeviceAsync(id, config, AudioDeviceCategory.Input, _inputConfigs, _loadedInputs);
    }

    /// <summary>
    /// Update an existing output device configuration
    /// </summary>
    public async Task<DeviceConfiguration?> UpdateOutputAsync(string id, DeviceConfiguration config)
    {
      return await UpdateDeviceAsync(id, config, AudioDeviceCategory.Output, _outputConfigs, _loadedOutputs);
    }

    /// <summary>
    /// Helper method to update a device configuration
    /// </summary>
    private async Task<DeviceConfiguration?> UpdateDeviceAsync<TDevice>(
      string id,
      DeviceConfiguration config,
      AudioDeviceCategory category,
      Dictionary<string, DeviceConfiguration> configDict,
      Dictionary<string, TDevice> deviceDict) where TDevice : IAudioDevice
    {
      if (!configDict.ContainsKey(id))
        return null;

      // Check for duplicate names (excluding the current device)
      if (IsNameDuplicate(config.Name, id))
      {
        throw new InvalidOperationException($"A device with the name '{config.Name}' already exists.");
      }

      config.Id = id;
      config.ModifiedAt = DateTime.UtcNow;
      config.CreatedAt = configDict[id].CreatedAt;

      configDict[id] = config;
      await SaveConfigurationsAsync();

      // Reload the device
      if (deviceDict.ContainsKey(id))
      {
        try
        {
          await deviceDict[id].StopAsync();
        }
        catch { }
        deviceDict.Remove(id);
      }

      if (config.IsEnabled)
      {
        try
        {
          TDevice device = category == AudioDeviceCategory.Input
            ? (TDevice)(object)_deviceFactory.CreateInput(config)
            : (TDevice)(object)_deviceFactory.CreateOutput(config);

          await device.InitializeAsync();
          deviceDict[id] = device;
          _logger.LogInformation("Updated and reloaded {Category} device: {Name} ({Type})",
            category.ToString().ToLower(), config.Name, config.DeviceType);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to reload updated {Category} device: {Name} ({Type})",
            category.ToString().ToLower(), config.Name, config.DeviceType);
        }
      }

      return config;
    }

    /// <summary>
    /// Remove an input device configuration
    /// </summary>
    public async Task<bool> RemoveInputAsync(string id)
    {
      return await RemoveDeviceAsync(id, AudioDeviceCategory.Input, _inputConfigs, _loadedInputs);
    }

    /// <summary>
    /// Remove an output device configuration
    /// </summary>
    public async Task<bool> RemoveOutputAsync(string id)
    {
      return await RemoveDeviceAsync(id, AudioDeviceCategory.Output, _outputConfigs, _loadedOutputs);
    }

    /// <summary>
    /// Helper method to remove a device configuration
    /// </summary>
    private async Task<bool> RemoveDeviceAsync<TDevice>(
      string id,
      AudioDeviceCategory category,
      Dictionary<string, DeviceConfiguration> configDict,
      Dictionary<string, TDevice> deviceDict) where TDevice : IAudioDevice
    {
      if (!configDict.ContainsKey(id))
        return false;

      // Stop and remove the loaded device
      if (deviceDict.ContainsKey(id))
      {
        try
        {
          await deviceDict[id].StopAsync();
        }
        catch { }
        deviceDict.Remove(id);
      }

      configDict.Remove(id);
      await SaveConfigurationsAsync();
      _logger.LogInformation("Removed {Category} device: {Id}", category.ToString().ToLower(), id);

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
