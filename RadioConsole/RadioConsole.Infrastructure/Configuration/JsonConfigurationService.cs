using System.Text.Json;
using RadioConsole.Core.Interfaces;
using RadioConsole.Core.Models;

namespace RadioConsole.Infrastructure.Configuration;

/// <summary>
/// JSON file-based implementation of the configuration service.
/// </summary>
public class JsonConfigurationService : IConfigurationService
{
  private readonly string _filePath;
  private readonly JsonSerializerOptions _jsonOptions;
  private readonly SemaphoreSlim _lock = new(1, 1);

  /// <summary>
  /// Initializes a new instance of the <see cref="JsonConfigurationService"/> class.
  /// </summary>
  /// <param name="filePath">The path to the JSON file for storing configurations.</param>
  public JsonConfigurationService(string filePath)
  {
    _filePath = filePath;
    _jsonOptions = new JsonSerializerOptions
    {
      WriteIndented = true
    };

    // Ensure the directory exists
    var directory = Path.GetDirectoryName(_filePath);
    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
    {
      Directory.CreateDirectory(directory);
    }

    // Initialize file if it doesn't exist
    if (!File.Exists(_filePath))
    {
      File.WriteAllText(_filePath, "[]");
    }
  }

  public async Task SaveAsync(ConfigurationItem item)
  {
    await _lock.WaitAsync();
    try
    {
      var items = await LoadAllInternalAsync();
      var existingItem = items.FirstOrDefault(i => i.Key == item.Key);

      if (existingItem != null)
      {
        existingItem.Value = item.Value;
        existingItem.Category = item.Category;
        existingItem.LastUpdated = DateTime.UtcNow;
      }
      else
      {
        item.Id = Guid.NewGuid().ToString();
        item.LastUpdated = DateTime.UtcNow;
        items.Add(item);
      }

      var json = JsonSerializer.Serialize(items, _jsonOptions);
      await File.WriteAllTextAsync(_filePath, json);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<ConfigurationItem?> LoadAsync(string key)
  {
    await _lock.WaitAsync();
    try
    {
      var items = await LoadAllInternalAsync();
      return items.FirstOrDefault(i => i.Key == key);
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
      return await LoadAllInternalAsync();
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
      var items = await LoadAllInternalAsync();
      return items.Where(i => i.Category == category).ToList();
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task DeleteAsync(string key)
  {
    await _lock.WaitAsync();
    try
    {
      var items = await LoadAllInternalAsync();
      var itemToRemove = items.FirstOrDefault(i => i.Key == key);

      if (itemToRemove != null)
      {
        items.Remove(itemToRemove);
        var json = JsonSerializer.Serialize(items, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
      }
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<bool> ExistsAsync(string key)
  {
    await _lock.WaitAsync();
    try
    {
      var items = await LoadAllInternalAsync();
      return items.Any(i => i.Key == key);
    }
    finally
    {
      _lock.Release();
    }
  }

  private async Task<List<ConfigurationItem>> LoadAllInternalAsync()
  {
    if (!File.Exists(_filePath))
    {
      return new List<ConfigurationItem>();
    }

    var json = await File.ReadAllTextAsync(_filePath);
    if (string.IsNullOrWhiteSpace(json))
    {
      return new List<ConfigurationItem>();
    }

    return JsonSerializer.Deserialize<List<ConfigurationItem>>(json) ?? new List<ConfigurationItem>();
  }
}
