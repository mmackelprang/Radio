using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using RadioConsole.Core.Interfaces;
using RadioConsole.Core.Models;

namespace RadioConsole.Infrastructure.Configuration;

/// <summary>
/// JSON file-based implementation of the configuration service.
/// Each component is stored in a separate JSON file.
/// </summary>
public class JsonConfigurationService : IConfigurationService
{
  private readonly string _storageDirectory;
  private readonly JsonSerializerOptions _jsonOptions;
  private readonly SemaphoreSlim _lock = new(1, 1);
  private static readonly Regex SecretPattern = new(@"\[SECRET:\[([^,]+),([^,]+),([^\]]+)\]\]", RegexOptions.Compiled);

  /// <summary>
  /// Initializes a new instance of the <see cref="JsonConfigurationService"/> class.
  /// </summary>
  /// <param name="storagePath">The directory path for storing component JSON files.</param>
  public JsonConfigurationService(string storagePath)
  {
    // If storagePath is a file, use its directory; otherwise use it as-is
    _storageDirectory = Path.HasExtension(storagePath)
      ? Path.GetDirectoryName(storagePath) ?? "./storage"
      : storagePath;

    _jsonOptions = new JsonSerializerOptions
    {
      WriteIndented = true
    };

    // Ensure the directory exists
    if (!Directory.Exists(_storageDirectory))
    {
      Directory.CreateDirectory(_storageDirectory);
    }
  }

  public async Task SaveAsync(ConfigurationItem item)
  {
    if (string.IsNullOrWhiteSpace(item.Component))
    {
      throw new ArgumentException("Component cannot be empty", nameof(item));
    }

    await _lock.WaitAsync();
    try
    {
      var componentFile = GetComponentFilePath(item.Component);
      var items = await LoadComponentInternalAsync(item.Component);
      var existingItem = items.FirstOrDefault(i => i.Key == item.Key && i.Category == item.Category);

      if (existingItem != null)
      {
        existingItem.Value = item.Value;
        existingItem.LastUpdated = DateTime.UtcNow;
      }
      else
      {
        item.Id = string.IsNullOrEmpty(item.Id) ? Guid.NewGuid().ToString() : item.Id;
        item.LastUpdated = DateTime.UtcNow;
        items.Add(item);
      }

      var json = JsonSerializer.Serialize(items, _jsonOptions);
      await File.WriteAllTextAsync(componentFile, json);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<ConfigurationItem?> LoadAsync(string component, string key)
  {
    await _lock.WaitAsync();
    try
    {
      var items = await LoadComponentInternalAsync(component);
      var item = items.FirstOrDefault(i => i.Key == key);
      return item != null ? await ResolveSecretsAsync(item) : null;
    }
    finally
    {
      _lock.Release();
    }
  }

  // Legacy method for backward compatibility
  public async Task<ConfigurationItem?> LoadAsync(string key)
  {
    await _lock.WaitAsync();
    try
    {
      var allItems = await LoadAllInternalAsync();
      var item = allItems.FirstOrDefault(i => i.Key == key);
      return item != null ? await ResolveSecretsAsync(item) : null;
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<IEnumerable<ConfigurationItem>> LoadAllAsync()
  {
    await _lock.WaitAsync();
    try
    {
      var allItems = await LoadAllInternalAsync();
      var resolvedItems = new List<ConfigurationItem>();
      foreach (var item in allItems)
      {
        resolvedItems.Add(await ResolveSecretsAsync(item));
      }
      return resolvedItems;
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<IEnumerable<ConfigurationItem>> LoadByComponentAsync(string component)
  {
    await _lock.WaitAsync();
    try
    {
      var items = await LoadComponentInternalAsync(component);
      var resolvedItems = new List<ConfigurationItem>();
      foreach (var item in items)
      {
        resolvedItems.Add(await ResolveSecretsAsync(item));
      }
      return resolvedItems;
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<IEnumerable<ConfigurationItem>> LoadByCategoryAsync(string category)
  {
    await _lock.WaitAsync();
    try
    {
      var allItems = await LoadAllInternalAsync();
      var filteredItems = allItems.Where(i => i.Category == category).ToList();
      var resolvedItems = new List<ConfigurationItem>();
      foreach (var item in filteredItems)
      {
        resolvedItems.Add(await ResolveSecretsAsync(item));
      }
      return resolvedItems;
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<IEnumerable<string>> GetComponentsAsync()
  {
    await _lock.WaitAsync();
    try
    {
      var componentFiles = Directory.GetFiles(_storageDirectory, "*.json");
      return componentFiles
        .Select(f => Path.GetFileNameWithoutExtension(f))
        .Where(c => !string.IsNullOrEmpty(c))
        .ToList();
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task DeleteAsync(string component, string key)
  {
    await _lock.WaitAsync();
    try
    {
      var componentFile = GetComponentFilePath(component);
      if (!File.Exists(componentFile))
      {
        return;
      }

      var items = await LoadComponentInternalAsync(component);
      var itemToRemove = items.FirstOrDefault(i => i.Key == key);

      if (itemToRemove != null)
      {
        items.Remove(itemToRemove);
        var json = JsonSerializer.Serialize(items, _jsonOptions);
        await File.WriteAllTextAsync(componentFile, json);
      }
    }
    finally
    {
      _lock.Release();
    }
  }

  // Legacy method for backward compatibility
  public async Task DeleteAsync(string key)
  {
    await _lock.WaitAsync();
    try
    {
      var components = await GetComponentsAsync();
      foreach (var component in components)
      {
        var items = await LoadComponentInternalAsync(component);
        var itemToRemove = items.FirstOrDefault(i => i.Key == key);
        
        if (itemToRemove != null)
        {
          items.Remove(itemToRemove);
          var componentFile = GetComponentFilePath(component);
          var json = JsonSerializer.Serialize(items, _jsonOptions);
          await File.WriteAllTextAsync(componentFile, json);
          break;
        }
      }
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<bool> ExistsAsync(string component, string key)
  {
    await _lock.WaitAsync();
    try
    {
      var items = await LoadComponentInternalAsync(component);
      return items.Any(i => i.Key == key);
    }
    finally
    {
      _lock.Release();
    }
  }

  // Legacy method for backward compatibility
  public async Task<bool> ExistsAsync(string key)
  {
    await _lock.WaitAsync();
    try
    {
      var allItems = await LoadAllInternalAsync();
      return allItems.Any(i => i.Key == key);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<string> BackupAsync(string? backupDirectory = null)
  {
    backupDirectory ??= "./backup";
    
    await _lock.WaitAsync();
    try
    {
      if (!Directory.Exists(backupDirectory))
      {
        Directory.CreateDirectory(backupDirectory);
      }

      var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
      var backupFileName = $"json-{timestamp}.zip";
      var backupPath = Path.Combine(backupDirectory, backupFileName);

      using var archive = ZipFile.Open(backupPath, ZipArchiveMode.Create);
      
      var componentFiles = Directory.GetFiles(_storageDirectory, "*.json");
      foreach (var file in componentFiles)
      {
        var entryName = Path.GetFileName(file);
        archive.CreateEntryFromFile(file, entryName);
      }

      return backupPath;
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task RestoreAsync(string backupPath)
  {
    if (!File.Exists(backupPath))
    {
      throw new FileNotFoundException($"Backup file not found: {backupPath}");
    }

    await _lock.WaitAsync();
    try
    {
      using var archive = ZipFile.OpenRead(backupPath);
      
      foreach (var entry in archive.Entries)
      {
        if (entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
          var destinationPath = Path.Combine(_storageDirectory, entry.FullName);
          entry.ExtractToFile(destinationPath, overwrite: true);
        }
      }
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task ReplicateToAsync(IConfigurationService target)
  {
    await _lock.WaitAsync();
    try
    {
      var allItems = await LoadAllInternalAsync();
      foreach (var item in allItems)
      {
        await target.SaveAsync(item);
      }
    }
    finally
    {
      _lock.Release();
    }
  }

  private string GetComponentFilePath(string component)
  {
    var sanitizedComponent = string.Join("_", component.Split(Path.GetInvalidFileNameChars()));
    return Path.Combine(_storageDirectory, $"{sanitizedComponent}.json");
  }

  private async Task<List<ConfigurationItem>> LoadComponentInternalAsync(string component)
  {
    var componentFile = GetComponentFilePath(component);
    
    if (!File.Exists(componentFile))
    {
      return new List<ConfigurationItem>();
    }

    var json = await File.ReadAllTextAsync(componentFile);
    if (string.IsNullOrWhiteSpace(json))
    {
      return new List<ConfigurationItem>();
    }

    var items = JsonSerializer.Deserialize<List<ConfigurationItem>>(json) ?? new List<ConfigurationItem>();
    
    // Ensure component is set
    foreach (var item in items)
    {
      if (string.IsNullOrEmpty(item.Component))
      {
        item.Component = component;
      }
    }

    return items;
  }

  private async Task<List<ConfigurationItem>> LoadAllInternalAsync()
  {
    var allItems = new List<ConfigurationItem>();
    var componentFiles = Directory.GetFiles(_storageDirectory, "*.json");

    foreach (var file in componentFiles)
    {
      var component = Path.GetFileNameWithoutExtension(file);
      if (!string.IsNullOrEmpty(component))
      {
        var items = await LoadComponentInternalAsync(component);
        allItems.AddRange(items);
      }
    }

    return allItems;
  }

  /// <summary>
  /// Resolves secret references in a configuration item value.
  /// Secret format: [SECRET:[Component,Category,Key]]
  /// </summary>
  private async Task<ConfigurationItem> ResolveSecretsAsync(ConfigurationItem item)
  {
    if (string.IsNullOrEmpty(item.Value) || !item.Value.Contains("[SECRET:["))
    {
      return item;
    }

    var resolvedValue = item.Value;
    var matches = SecretPattern.Matches(item.Value);

    foreach (Match match in matches)
    {
      var secretComponent = match.Groups[1].Value;
      var secretCategory = match.Groups[2].Value;
      var secretKey = match.Groups[3].Value;

      // Look up secret in Secrets component with concatenated category
      var concatenatedCategory = $"{secretComponent}_{secretCategory}";
      var secretItems = await LoadComponentInternalAsync("Secrets");
      var secretItem = secretItems.FirstOrDefault(s => 
        s.Category == concatenatedCategory && s.Key == secretKey);

      if (secretItem != null)
      {
        resolvedValue = resolvedValue.Replace(match.Value, secretItem.Value);
      }
    }

    // Return a new instance with resolved value to avoid modifying cached data
    return new ConfigurationItem
    {
      Id = item.Id,
      Component = item.Component,
      Key = item.Key,
      Value = resolvedValue,
      Category = item.Category,
      LastUpdated = item.LastUpdated
    };
  }
}
