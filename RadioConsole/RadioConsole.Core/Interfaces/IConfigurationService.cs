using RadioConsole.Core.Models;

namespace RadioConsole.Core.Interfaces;

/// <summary>
/// Interface for configuration service that supports multiple storage types.
/// </summary>
public interface IConfigurationService
{
  /// <summary>
  /// Saves a configuration item.
  /// </summary>
  /// <param name="item">The configuration item to save.</param>
  /// <returns>A task representing the asynchronous operation.</returns>
  Task SaveAsync(ConfigurationItem item);

  /// <summary>
  /// Loads a configuration item by its key.
  /// </summary>
  /// <param name="key">The configuration key.</param>
  /// <returns>The configuration item, or null if not found.</returns>
  Task<ConfigurationItem?> LoadAsync(string key);

  /// <summary>
  /// Loads all configuration items.
  /// </summary>
  /// <returns>A collection of all configuration items.</returns>
  Task<IEnumerable<ConfigurationItem>> LoadAllAsync();

  /// <summary>
  /// Loads all configuration items in a specific category.
  /// </summary>
  /// <param name="category">The category to filter by.</param>
  /// <returns>A collection of configuration items in the specified category.</returns>
  Task<IEnumerable<ConfigurationItem>> LoadByCategoryAsync(string category);

  /// <summary>
  /// Deletes a configuration item by its key.
  /// </summary>
  /// <param name="key">The configuration key.</param>
  /// <returns>A task representing the asynchronous operation.</returns>
  Task DeleteAsync(string key);

  /// <summary>
  /// Checks if a configuration item exists.
  /// </summary>
  /// <param name="key">The configuration key.</param>
  /// <returns>True if the item exists, false otherwise.</returns>
  Task<bool> ExistsAsync(string key);
}
