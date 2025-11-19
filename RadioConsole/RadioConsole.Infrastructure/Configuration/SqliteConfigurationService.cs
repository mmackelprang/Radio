using Microsoft.Data.Sqlite;
using RadioConsole.Core.Interfaces;
using RadioConsole.Core.Models;

namespace RadioConsole.Infrastructure.Configuration;

/// <summary>
/// SQLite database-based implementation of the configuration service.
/// </summary>
public class SqliteConfigurationService : IConfigurationService
{
  private readonly string _connectionString;

  /// <summary>
  /// Initializes a new instance of the <see cref="SqliteConfigurationService"/> class.
  /// </summary>
  /// <param name="databasePath">The path to the SQLite database file.</param>
  public SqliteConfigurationService(string databasePath)
  {
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

    var createTableCommand = connection.CreateCommand();
    createTableCommand.CommandText = @"
      CREATE TABLE IF NOT EXISTS ConfigurationItems (
        Id TEXT PRIMARY KEY,
        Key TEXT NOT NULL UNIQUE,
        Value TEXT NOT NULL,
        Category TEXT NOT NULL,
        LastUpdated TEXT NOT NULL
      );
      CREATE INDEX IF NOT EXISTS idx_key ON ConfigurationItems(Key);
      CREATE INDEX IF NOT EXISTS idx_category ON ConfigurationItems(Category);
    ";
    await createTableCommand.ExecuteNonQueryAsync();
  }

  public async Task SaveAsync(ConfigurationItem item)
  {
    using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = @"
      INSERT INTO ConfigurationItems (Id, Key, Value, Category, LastUpdated)
      VALUES ($id, $key, $value, $category, $lastUpdated)
      ON CONFLICT(Key) DO UPDATE SET
        Value = $value,
        Category = $category,
        LastUpdated = $lastUpdated;
    ";

    command.Parameters.AddWithValue("$id", string.IsNullOrEmpty(item.Id) ? Guid.NewGuid().ToString() : item.Id);
    command.Parameters.AddWithValue("$key", item.Key);
    command.Parameters.AddWithValue("$value", item.Value);
    command.Parameters.AddWithValue("$category", item.Category);
    command.Parameters.AddWithValue("$lastUpdated", DateTime.UtcNow.ToString("O"));

    await command.ExecuteNonQueryAsync();
  }

  public async Task<ConfigurationItem?> LoadAsync(string key)
  {
    using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = @"
      SELECT Id, Key, Value, Category, LastUpdated
      FROM ConfigurationItems
      WHERE Key = $key;
    ";
    command.Parameters.AddWithValue("$key", key);

    using var reader = await command.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
      return new ConfigurationItem
      {
        Id = reader.GetString(0),
        Key = reader.GetString(1),
        Value = reader.GetString(2),
        Category = reader.GetString(3),
        LastUpdated = DateTime.Parse(reader.GetString(4))
      };
    }

    return null;
  }

  public async Task<IEnumerable<ConfigurationItem>> LoadAllAsync()
  {
    var items = new List<ConfigurationItem>();

    using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = @"
      SELECT Id, Key, Value, Category, LastUpdated
      FROM ConfigurationItems;
    ";

    using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
      items.Add(new ConfigurationItem
      {
        Id = reader.GetString(0),
        Key = reader.GetString(1),
        Value = reader.GetString(2),
        Category = reader.GetString(3),
        LastUpdated = DateTime.Parse(reader.GetString(4))
      });
    }

    return items;
  }

  public async Task<IEnumerable<ConfigurationItem>> LoadByCategoryAsync(string category)
  {
    var items = new List<ConfigurationItem>();

    using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = @"
      SELECT Id, Key, Value, Category, LastUpdated
      FROM ConfigurationItems
      WHERE Category = $category;
    ";
    command.Parameters.AddWithValue("$category", category);

    using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
      items.Add(new ConfigurationItem
      {
        Id = reader.GetString(0),
        Key = reader.GetString(1),
        Value = reader.GetString(2),
        Category = reader.GetString(3),
        LastUpdated = DateTime.Parse(reader.GetString(4))
      });
    }

    return items;
  }

  public async Task DeleteAsync(string key)
  {
    using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = @"
      DELETE FROM ConfigurationItems
      WHERE Key = $key;
    ";
    command.Parameters.AddWithValue("$key", key);

    await command.ExecuteNonQueryAsync();
  }

  public async Task<bool> ExistsAsync(string key)
  {
    using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = @"
      SELECT COUNT(1)
      FROM ConfigurationItems
      WHERE Key = $key;
    ";
    command.Parameters.AddWithValue("$key", key);

    var result = await command.ExecuteScalarAsync();
    return Convert.ToInt32(result) > 0;
  }
}
