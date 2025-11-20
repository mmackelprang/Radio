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
  /// Loads a configuration item by its component and key.
  /// </summary>
  /// <param name="component">The component name.</param>
  /// <param name="key">The configuration key.</param>
  /// <returns>The configuration item, or null if not found.</returns>
  Task<ConfigurationItem?> LoadAsync(string component, string key);

  /// <summary>
  /// Loads a configuration item by its key (legacy method for backward compatibility).
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
  /// Loads all configuration items in a specific component.
  /// </summary>
  /// <param name="component">The component to filter by.</param>
  /// <returns>A collection of configuration items in the specified component.</returns>
  Task<IEnumerable<ConfigurationItem>> LoadByComponentAsync(string component);

  /// <summary>
  /// Loads all configuration items in a specific category.
  /// </summary>
  /// <param name="category">The category to filter by.</param>
  /// <returns>A collection of configuration items in the specified category.</returns>
  Task<IEnumerable<ConfigurationItem>> LoadByCategoryAsync(string category);

  /// <summary>
  /// Gets a list of all components defined in the configuration.
  /// </summary>
  /// <returns>A collection of component names.</returns>
  Task<IEnumerable<string>> GetComponentsAsync();

  /// <summary>
  /// Deletes a configuration item by its component and key.
  /// </summary>
  /// <param name="component">The component name.</param>
  /// <param name="key">The configuration key.</param>
  /// <returns>A task representing the asynchronous operation.</returns>
  Task DeleteAsync(string component, string key);

  /// <summary>
  /// Deletes a configuration item by its key (legacy method for backward compatibility).
  /// </summary>
  /// <param name="key">The configuration key.</param>
  /// <returns>A task representing the asynchronous operation.</returns>
  Task DeleteAsync(string key);

  /// <summary>
  /// Checks if a configuration item exists.
  /// </summary>
  /// <param name="component">The component name.</param>
  /// <param name="key">The configuration key.</param>
  /// <returns>True if the item exists, false otherwise.</returns>
  Task<bool> ExistsAsync(string component, string key);

  /// <summary>
  /// Checks if a configuration item exists (legacy method for backward compatibility).
  /// </summary>
  /// <param name="key">The configuration key.</param>
  /// <returns>True if the item exists, false otherwise.</returns>
  Task<bool> ExistsAsync(string key);

  /// <summary>
  /// Creates a backup of all configuration data.
  /// </summary>
  /// <param name="backupDirectory">Optional backup directory path. Defaults to ./backup</param>
  /// <returns>The path to the created backup file.</returns>
  Task<string> BackupAsync(string? backupDirectory = null);

  /// <summary>
  /// Restores configuration data from a backup file.
  /// </summary>
  /// <param name="backupPath">The path to the backup file.</param>
  /// <returns>A task representing the asynchronous operation.</returns>
  Task RestoreAsync(string backupPath);

  /// <summary>
  /// Replicates all configuration data to another configuration service.
  /// </summary>
  /// <param name="target">The target configuration service.</param>
  /// <returns>A task representing the asynchronous operation.</returns>
  Task ReplicateToAsync(IConfigurationService target);
}
