namespace RadioConsole.Interfaces;

/// <summary>
/// Interface for data storage and persistence
/// </summary>
public interface IStorage
{
    /// <summary>
    /// Save data to storage
    /// </summary>
    Task SaveAsync<T>(string key, T data);

    /// <summary>
    /// Load data from storage
    /// </summary>
    Task<T?> LoadAsync<T>(string key);

    /// <summary>
    /// Delete data from storage
    /// </summary>
    Task DeleteAsync(string key);

    /// <summary>
    /// Check if a key exists in storage
    /// </summary>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Get all keys in storage
    /// </summary>
    Task<IEnumerable<string>> GetKeysAsync();
}
