using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using RadioConsole.Core.Interfaces;
using RadioConsole.Core.Models;

namespace RadioConsole.Infrastructure.Configuration;

/// <summary>
/// SQLite database-based implementation of the configuration service.
/// Each component is stored in a separate table named Config_{Component}.
/// </summary>
public class SqliteConfigurationService : IConfigurationService
{
  private readonly string _connectionString;
  private readonly string _databasePath;
  private static readonly Regex SecretPattern = new(@"\[SECRET:\[([^,]+),([^,]+),([^\]]+)\]\]", RegexOptions.Compiled);

  /// <summary>
  /// Initializes a new instance of the <see cref="SqliteConfigurationService"/> class.
  /// </summary>
  /// <param name="databasePath">The path to the SQLite database file.</param>
  public SqliteConfigurationService(string databasePath)
  {
    _databasePath = databasePath;
    
    // Ensure the directory exists
    var directory = Path.GetDirectoryName(databasePath);
    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
    {
      Directory.CreateDirectory(directory);
    }

    _connectionString = $"Data Source={databasePath}";
    InitializeDatabase().Wait();
  }

  private async Task InitializeDatabase()
  {
    using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();

    // Create a metadata table to track components
    var createMetadataCommand = connection.CreateCommand();
    createMetadataCommand.CommandText = @"
      CREATE TABLE IF NOT EXISTS ConfigComponents (
        Component TEXT PRIMARY KEY,
        CreatedAt TEXT NOT NULL
      );
    ";
    await createMetadataCommand.ExecuteNonQueryAsync();
  }

  private async Task EnsureComponentTableExistsAsync(string component)
  {
    var tableName = GetTableName(component);
    
    using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();

    // Check if component is registered
    var checkCommand = connection.CreateCommand();
    checkCommand.CommandText = "SELECT COUNT(1) FROM ConfigComponents WHERE Component = $component";
    checkCommand.Parameters.AddWithValue("$component", component);
    var exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;

    if (!exists)
    {
      // Register component
      var registerCommand = connection.CreateCommand();
      registerCommand.CommandText = @"
        INSERT INTO ConfigComponents (Component, CreatedAt)
        VALUES ($component, $createdAt);
      ";
      registerCommand.Parameters.AddWithValue("$component", component);
      registerCommand.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
      await registerCommand.ExecuteNonQueryAsync();

      // Create component table
      var createTableCommand = connection.CreateCommand();
      createTableCommand.CommandText = $@"
        CREATE TABLE IF NOT EXISTS {tableName} (
          Id TEXT PRIMARY KEY,
          Component TEXT NOT NULL,
          Key TEXT NOT NULL,
          Value TEXT NOT NULL,
          Category TEXT NOT NULL,
          LastUpdated TEXT NOT NULL,
          UNIQUE(Key, Category)
        );
        CREATE INDEX IF NOT EXISTS idx_{tableName}_key ON {tableName}(Key);
        CREATE INDEX IF NOT EXISTS idx_{tableName}_category ON {tableName}(Category);
      ";
      await createTableCommand.ExecuteNonQueryAsync();
    }
  }

  public async Task SaveAsync(ConfigurationItem item)
  {
    if (string.IsNullOrWhiteSpace(item.Component))
    {
      throw new ArgumentException("Component cannot be empty", nameof(item));
    }

    await EnsureComponentTableExistsAsync(item.Component);
    var tableName = GetTableName(item.Component);

    using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = $@"
      INSERT INTO {tableName} (Id, Component, Key, Value, Category, LastUpdated)
      VALUES ($id, $component, $key, $value, $category, $lastUpdated)
      ON CONFLICT(Key, Category) DO UPDATE SET
        Value = $value,
        LastUpdated = $lastUpdated;
    ";

    command.Parameters.AddWithValue("$id", string.IsNullOrEmpty(item.Id) ? Guid.NewGuid().ToString() : item.Id);
    command.Parameters.AddWithValue("$component", item.Component);
    command.Parameters.AddWithValue("$key", item.Key);
    command.Parameters.AddWithValue("$value", item.Value);
    command.Parameters.AddWithValue("$category", item.Category);
    command.Parameters.AddWithValue("$lastUpdated", DateTime.UtcNow.ToString("O"));

    await command.ExecuteNonQueryAsync();
  }

  public async Task<ConfigurationItem?> LoadAsync(string component, string key)
  {
    await EnsureComponentTableExistsAsync(component);
    var tableName = GetTableName(component);

    using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = $@"
      SELECT Id, Component, Key, Value, Category, LastUpdated
      FROM {tableName}
      WHERE Key = $key
      LIMIT 1;
    ";
    command.Parameters.AddWithValue("$key", key);

    using var reader = await command.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
      var item = ReadConfigurationItem(reader);
      return await ResolveSecretsAsync(item);
    }

    return null;
  }

  // Legacy method for backward compatibility
  public async Task<ConfigurationItem?> LoadAsync(string key)
  {
    var components = await GetComponentsAsync();
    
    foreach (var component in components)
    {
      var item = await LoadAsync(component, key);
      if (item != null)
      {
        return item;
      }
    }

    return null;
  }

  public async Task<IEnumerable<ConfigurationItem>> LoadAllAsync()
  {
    var components = await GetComponentsAsync();
    var itemsByComponent = await Task.WhenAll(
      components.Select(component => LoadByComponentAsync(component))
    );
    return itemsByComponent.SelectMany(items => items);
  }

  public async Task<IEnumerable<ConfigurationItem>> LoadByComponentAsync(string component)
  {
    await EnsureComponentTableExistsAsync(component);
    var tableName = GetTableName(component);
    var items = new List<ConfigurationItem>();

    using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = $@"
      SELECT Id, Component, Key, Value, Category, LastUpdated
      FROM {tableName};
    ";

    using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
      var item = ReadConfigurationItem(reader);
      items.Add(await ResolveSecretsAsync(item));
    }

    return items;
  }

  public async Task<IEnumerable<ConfigurationItem>> LoadByCategoryAsync(string category)
  {
    var allItems = new List<ConfigurationItem>();
    var components = await GetComponentsAsync();

    foreach (var component in components)
    {
      await EnsureComponentTableExistsAsync(component);
      var tableName = GetTableName(component);

      using var connection = new SqliteConnection(_connectionString);
      await connection.OpenAsync();

      var command = connection.CreateCommand();
      command.CommandText = $@"
        SELECT Id, Component, Key, Value, Category, LastUpdated
        FROM {tableName}
        WHERE Category = $category;
      ";
      command.Parameters.AddWithValue("$category", category);

      using var reader = await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        var item = ReadConfigurationItem(reader);
        allItems.Add(await ResolveSecretsAsync(item));
      }
    }

    return allItems;
  }

  public async Task<IEnumerable<string>> GetComponentsAsync()
  {
    var components = new List<string>();

    using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = "SELECT Component FROM ConfigComponents ORDER BY Component;";

    using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
      components.Add(reader.GetString(0));
    }

    return components;
  }

  public async Task DeleteAsync(string component, string key)
  {
    await EnsureComponentTableExistsAsync(component);
    var tableName = GetTableName(component);

    using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = $@"
      DELETE FROM {tableName}
      WHERE Key = $key;
    ";
    command.Parameters.AddWithValue("$key", key);

    await command.ExecuteNonQueryAsync();
  }

  // Legacy method for backward compatibility
  public async Task DeleteAsync(string key)
  {
    var components = await GetComponentsAsync();

    foreach (var component in components)
    {
      var exists = await ExistsAsync(component, key);
      if (exists)
      {
        await DeleteAsync(component, key);
        break;
      }
    }
  }

  public async Task<bool> ExistsAsync(string component, string key)
  {
    await EnsureComponentTableExistsAsync(component);
    var tableName = GetTableName(component);

    using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = $@"
      SELECT COUNT(1)
      FROM {tableName}
      WHERE Key = $key;
    ";
    command.Parameters.AddWithValue("$key", key);

    var result = await command.ExecuteScalarAsync();
    return Convert.ToInt32(result) > 0;
  }

  // Legacy method for backward compatibility
  public async Task<bool> ExistsAsync(string key)
  {
    var components = await GetComponentsAsync();

    foreach (var component in components)
    {
      if (await ExistsAsync(component, key))
      {
        return true;
      }
    }

    return false;
  }

  public async Task<string> BackupAsync(string? backupDirectory = null)
  {
    backupDirectory ??= "./backup";

    if (!Directory.Exists(backupDirectory))
    {
      Directory.CreateDirectory(backupDirectory);
    }

    var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
    var backupFileName = $"sqlite-{timestamp}.zip";
    var backupPath = Path.Combine(backupDirectory, backupFileName);

    // Export all data to JSON and create zip
    var allItems = await LoadAllAsync();
    var components = await GetComponentsAsync();

    using var archive = ZipFile.Open(backupPath, ZipArchiveMode.Create);
    
    foreach (var component in components)
    {
      var componentItems = allItems.Where(i => i.Component == component).ToList();
      var json = JsonSerializer.Serialize(componentItems, new JsonSerializerOptions { WriteIndented = true });
      
      var entry = archive.CreateEntry($"{component}.json");
      using var writer = new StreamWriter(entry.Open());
      await writer.WriteAsync(json);
    }

    return backupPath;
  }

  public async Task RestoreAsync(string backupPath)
  {
    if (!File.Exists(backupPath))
    {
      throw new FileNotFoundException($"Backup file not found: {backupPath}");
    }

    using var archive = ZipFile.OpenRead(backupPath);

    foreach (var entry in archive.Entries)
    {
      if (entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
      {
        using var stream = entry.Open();
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        
        var items = JsonSerializer.Deserialize<List<ConfigurationItem>>(json);
        if (items != null)
        {
          foreach (var item in items)
          {
            await SaveAsync(item);
          }
        }
      }
    }
  }

  public async Task ReplicateToAsync(IConfigurationService target)
  {
    var allItems = await LoadAllAsync();
    
    foreach (var item in allItems)
    {
      await target.SaveAsync(item);
    }
  }

  private string GetTableName(string component)
  {
    // Sanitize component name for use as table name
    var sanitized = new string(component
      .Where(c => char.IsLetterOrDigit(c) || c == '_')
      .ToArray());
    
    return $"Config_{sanitized}";
  }

  private ConfigurationItem ReadConfigurationItem(SqliteDataReader reader)
  {
    return new ConfigurationItem
    {
      Id = reader.GetString(0),
      Component = reader.GetString(1),
      Key = reader.GetString(2),
      Value = reader.GetString(3),
      Category = reader.GetString(4),
      LastUpdated = DateTime.Parse(reader.GetString(5))
    };
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
      var secretItems = await LoadByComponentAsync("Secrets");
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
