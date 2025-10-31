using System.Text.Json;
using RadioConsole.Interfaces;

namespace RadioConsole.Services;

/// <summary>
/// Simple JSON-based storage implementation
/// </summary>
public class JsonStorageService : IStorage
{
    private readonly string _storagePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonStorageService()
    {
        _storagePath = Path.Combine(FileSystem.AppDataDirectory, "storage");
        Directory.CreateDirectory(_storagePath);
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task SaveAsync<T>(string key, T data)
    {
        var filePath = GetFilePath(key);
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<T?> LoadAsync<T>(string key)
    {
        var filePath = GetFilePath(key);
        if (!File.Exists(filePath))
            return default;

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public Task DeleteAsync(string key)
    {
        var filePath = GetFilePath(key);
        if (File.Exists(filePath))
            File.Delete(filePath);
        
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        var filePath = GetFilePath(key);
        return Task.FromResult(File.Exists(filePath));
    }

    public Task<IEnumerable<string>> GetKeysAsync()
    {
        var files = Directory.GetFiles(_storagePath, "*.json");
        var keys = files.Select(f => Path.GetFileNameWithoutExtension(f));
        return Task.FromResult(keys);
    }

    private string GetFilePath(string key)
    {
        var fileName = $"{key}.json";
        return Path.Combine(_storagePath, fileName);
    }
}
