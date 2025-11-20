using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces;
using RadioConsole.Core.Models;
using RadioConsole.Infrastructure.Configuration;
using Xunit;
using System.Threading;

namespace RadioConsole.Tests.Configuration;

/// <summary>
/// Unit tests for configuration services (JSON and SQLite implementations).
/// </summary>
// Collection attribute ensures tests in this class do not run in parallel, avoiding
// file locking issues with SQLite temporary databases.
[Collection("ConfigurationServiceTestsCollection")]
public class ConfigurationServiceTests : IDisposable
{
  private readonly string _testDirectory;
  private readonly string _jsonFilePath;
  private readonly string _sqliteFilePath;

  public ConfigurationServiceTests()
  {
    // Create a unique test directory for each test run
    _testDirectory = Path.Combine(Path.GetTempPath(), $"RadioConsole_Tests_{Guid.NewGuid()}");
    Directory.CreateDirectory(_testDirectory);

    _jsonFilePath = Path.Combine(_testDirectory, "test_config.json");
    _sqliteFilePath = Path.Combine(_testDirectory, "test_config.db");
  }

  public void Dispose()
  {
    if (!Directory.Exists(_testDirectory))
    {
      return;
    }

    // Retry deletion to handle transient SQLite file locks due to async finalizers.
    for (var attempt = 0; attempt < 5; attempt++)
    {
      try
      {
        Directory.Delete(_testDirectory, true);
        break;
      }
      catch (IOException)
      {
        if (attempt == 4)
        {
          // Swallow on final attempt to prevent test failure purely due to cleanup.
          break;
        }
        Thread.Sleep(50);
      }
      catch (UnauthorizedAccessException)
      {
        if (attempt == 4)
        {
          break;
        }
        Thread.Sleep(50);
      }
    }
  }

  [Theory]
  [InlineData(StorageType.Json)]
  [InlineData(StorageType.SQLite)]
  public async Task SaveAsync_ShouldSaveConfigurationItem(StorageType storageType)
  {
    // Arrange
    var service = CreateService(storageType);
    var item = new ConfigurationItem
    {
      Component = "TestComponent",
      Key = "TestKey",
      Value = "TestValue",
      Category = "General"
    };

    // Act
    await service.SaveAsync(item);

    // Assert
    var loaded = await service.LoadAsync("TestComponent", "TestKey");
    Assert.NotNull(loaded);
    Assert.Equal("TestComponent", loaded.Component);
    Assert.Equal("TestKey", loaded.Key);
    Assert.Equal("TestValue", loaded.Value);
    Assert.Equal("General", loaded.Category);
  }

  [Theory]
  [InlineData(StorageType.Json)]
  [InlineData(StorageType.SQLite)]
  public async Task SaveAsync_ShouldUpdateExistingItem(StorageType storageType)
  {
    // Arrange
    var service = CreateService(storageType);
    var item = new ConfigurationItem
    {
      Component = "TestComponent",
      Key = "TestKey",
      Value = "OriginalValue",
      Category = "General"
    };
    await service.SaveAsync(item);

    // Act
    var updatedItem = new ConfigurationItem
    {
      Component = "TestComponent",
      Key = "TestKey",
      Value = "UpdatedValue",
      Category = "Advanced"
    };
    await service.SaveAsync(updatedItem);

    // Assert
    var loaded = await service.LoadAsync("TestComponent", "TestKey");
    Assert.NotNull(loaded);
    Assert.Equal("UpdatedValue", loaded.Value);
    Assert.Equal("Advanced", loaded.Category);
  }

  [Theory]
  [InlineData(StorageType.Json)]
  [InlineData(StorageType.SQLite)]
  public async Task LoadAsync_ShouldReturnNullForNonExistentKey(StorageType storageType)
  {
    // Arrange
    var service = CreateService(storageType);

    // Act
    var result = await service.LoadAsync("TestComponent", "NonExistentKey");

    // Assert
    Assert.Null(result);
  }

  [Theory]
  [InlineData(StorageType.Json)]
  [InlineData(StorageType.SQLite)]
  public async Task LoadAllAsync_ShouldReturnAllItems(StorageType storageType)
  {
    // Arrange
    var service = CreateService(storageType);
    var items = new[]
    {
      new ConfigurationItem { Component = "Component1", Key = "Key1", Value = "Value1", Category = "General" },
      new ConfigurationItem { Component = "Component2", Key = "Key2", Value = "Value2", Category = "Advanced" },
      new ConfigurationItem { Component = "Component1", Key = "Key3", Value = "Value3", Category = "General" }
    };

    foreach (var item in items)
    {
      await service.SaveAsync(item);
    }

    // Act
    var allItems = await service.LoadAllAsync();

    // Assert
    Assert.Equal(3, allItems.Count());
  }

  [Theory]
  [InlineData(StorageType.Json)]
  [InlineData(StorageType.SQLite)]
  public async Task LoadByCategoryAsync_ShouldReturnItemsInCategory(StorageType storageType)
  {
    // Arrange
    var service = CreateService(storageType);
    var items = new[]
    {
      new ConfigurationItem { Component = "Component1", Key = "Key1", Value = "Value1", Category = "General" },
      new ConfigurationItem { Component = "Component2", Key = "Key2", Value = "Value2", Category = "Advanced" },
      new ConfigurationItem { Component = "Component1", Key = "Key3", Value = "Value3", Category = "General" },
      new ConfigurationItem { Component = "Component3", Key = "Key4", Value = "Value4", Category = "Audio" }
    };

    foreach (var item in items)
    {
      await service.SaveAsync(item);
    }

    // Act
    var generalItems = await service.LoadByCategoryAsync("General");
    var advancedItems = await service.LoadByCategoryAsync("Advanced");
    var audioItems = await service.LoadByCategoryAsync("Audio");

    // Assert
    Assert.Equal(2, generalItems.Count());
    Assert.Single(advancedItems);
    Assert.Single(audioItems);
    Assert.All(generalItems, item => Assert.Equal("General", item.Category));
    Assert.All(advancedItems, item => Assert.Equal("Advanced", item.Category));
    Assert.All(audioItems, item => Assert.Equal("Audio", item.Category));
  }

  [Theory]
  [InlineData(StorageType.Json)]
  [InlineData(StorageType.SQLite)]
  public async Task DeleteAsync_ShouldRemoveItem(StorageType storageType)
  {
    // Arrange
    var service = CreateService(storageType);
    var item = new ConfigurationItem
    {
      Component = "TestComponent",
      Key = "TestKey",
      Value = "TestValue",
      Category = "General"
    };
    await service.SaveAsync(item);

    // Act
    await service.DeleteAsync("TestComponent", "TestKey");

    // Assert
    var loaded = await service.LoadAsync("TestComponent", "TestKey");
    Assert.Null(loaded);
  }

  [Theory]
  [InlineData(StorageType.Json)]
  [InlineData(StorageType.SQLite)]
  public async Task ExistsAsync_ShouldReturnTrueForExistingKey(StorageType storageType)
  {
    // Arrange
    var service = CreateService(storageType);
    var item = new ConfigurationItem
    {
      Component = "TestComponent",
      Key = "TestKey",
      Value = "TestValue",
      Category = "General"
    };
    await service.SaveAsync(item);

    // Act
    var exists = await service.ExistsAsync("TestComponent", "TestKey");

    // Assert
    Assert.True(exists);
  }

  [Theory]
  [InlineData(StorageType.Json)]
  [InlineData(StorageType.SQLite)]
  public async Task ExistsAsync_ShouldReturnFalseForNonExistentKey(StorageType storageType)
  {
    // Arrange
    var service = CreateService(storageType);

    // Act
    var exists = await service.ExistsAsync("TestComponent", "NonExistentKey");

    // Assert
    Assert.False(exists);
  }

  [Theory]
  [InlineData(StorageType.Json)]
  [InlineData(StorageType.SQLite)]
  public async Task SaveAsync_ShouldSetLastUpdatedTimestamp(StorageType storageType)
  {
    // Arrange
    var service = CreateService(storageType);
    var beforeSave = DateTime.UtcNow;
    var item = new ConfigurationItem
    {
      Component = "TestComponent",
      Key = "TestKey",
      Value = "TestValue",
      Category = "General"
    };

    // Act
    await service.SaveAsync(item);
    await Task.Delay(10); // Small delay to ensure time difference
    var afterSave = DateTime.UtcNow;

    // Assert
    var loaded = await service.LoadAsync("TestComponent", "TestKey");
    Assert.NotNull(loaded);
    var lastUpdatedUtc = loaded.LastUpdated.Kind == DateTimeKind.Utc
      ? loaded.LastUpdated
      : loaded.LastUpdated.ToUniversalTime();
    Assert.InRange(lastUpdatedUtc, beforeSave.AddSeconds(-1), afterSave.AddSeconds(1));
  }

  [Fact]
  public void ConfigurationServiceFactory_ShouldCreateJsonService()
  {
    // Act
    var service = ConfigurationServiceFactory.Create(StorageType.Json, _jsonFilePath);

    // Assert
    Assert.IsType<JsonConfigurationService>(service);
  }

  [Fact]
  public void ConfigurationServiceFactory_ShouldCreateSqliteService()
  {
    // Act
    var service = ConfigurationServiceFactory.Create(StorageType.SQLite, _sqliteFilePath);

    // Assert
    Assert.IsType<SqliteConfigurationService>(service);
  }

  private IConfigurationService CreateService(StorageType storageType)
  {
    return storageType switch
    {
      StorageType.Json => new JsonConfigurationService(_jsonFilePath),
      StorageType.SQLite => new SqliteConfigurationService(_sqliteFilePath),
      _ => throw new ArgumentException($"Unsupported storage type: {storageType}")
    };
  }
}
