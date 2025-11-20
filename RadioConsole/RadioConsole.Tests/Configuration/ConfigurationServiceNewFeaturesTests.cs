using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces;
using RadioConsole.Core.Models;
using RadioConsole.Infrastructure.Configuration;
using Xunit;
using System.IO.Compression;

namespace RadioConsole.Tests.Configuration;

/// <summary>
/// Unit tests for new configuration service features (Components, Secrets, Backup/Restore, Replication).
/// </summary>
[Collection("ConfigurationServiceTestsCollection")]
public class ConfigurationServiceNewFeaturesTests : IDisposable
{
  private readonly string _testDirectory;
  private readonly string _jsonStoragePath;
  private readonly string _sqliteFilePath;
  private readonly string _backupDirectory;

  public ConfigurationServiceNewFeaturesTests()
  {
    // Create a unique test directory for each test run
    _testDirectory = Path.Combine(Path.GetTempPath(), $"RadioConsole_Tests_{Guid.NewGuid()}");
    Directory.CreateDirectory(_testDirectory);

    _jsonStoragePath = Path.Combine(_testDirectory, "json_storage");
    _sqliteFilePath = Path.Combine(_testDirectory, "test_config.db");
    _backupDirectory = Path.Combine(_testDirectory, "backups");
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
  public async Task GetComponentsAsync_ShouldReturnAllComponents(StorageType storageType)
  {
    // Arrange
    var service = CreateService(storageType);
    var items = new[]
    {
      new ConfigurationItem { Component = "Spotify", Key = "ApiKey", Value = "key1", Category = "Auth" },
      new ConfigurationItem { Component = "TTS", Key = "Provider", Value = "Azure", Category = "Config" },
      new ConfigurationItem { Component = "Audio", Key = "Volume", Value = "75", Category = "Settings" }
    };

    foreach (var item in items)
    {
      await service.SaveAsync(item);
    }

    // Act
    var components = await service.GetComponentsAsync();

    // Assert
    Assert.Equal(3, components.Count());
    Assert.Contains("Spotify", components);
    Assert.Contains("TTS", components);
    Assert.Contains("Audio", components);
  }

  [Theory]
  [InlineData(StorageType.Json)]
  [InlineData(StorageType.SQLite)]
  public async Task LoadByComponentAsync_ShouldReturnComponentItems(StorageType storageType)
  {
    // Arrange
    var service = CreateService(storageType);
    var items = new[]
    {
      new ConfigurationItem { Component = "Spotify", Key = "ApiKey", Value = "key1", Category = "Auth" },
      new ConfigurationItem { Component = "Spotify", Key = "RefreshToken", Value = "token1", Category = "Auth" },
      new ConfigurationItem { Component = "TTS", Key = "Provider", Value = "Azure", Category = "Config" }
    };

    foreach (var item in items)
    {
      await service.SaveAsync(item);
    }

    // Act
    var spotifyItems = await service.LoadByComponentAsync("Spotify");

    // Assert
    Assert.Equal(2, spotifyItems.Count());
    Assert.All(spotifyItems, item => Assert.Equal("Spotify", item.Component));
  }

  [Theory]
  [InlineData(StorageType.Json)]
  [InlineData(StorageType.SQLite)]
  public async Task SecretResolution_ShouldResolveSecretReferences(StorageType storageType)
  {
    // Arrange
    var service = CreateService(storageType);
    
    // Save a secret
    var secret = new ConfigurationItem
    {
      Component = "Secrets",
      Category = "TTS_Azure",
      Key = "RefreshToken",
      Value = "actual-secret-token-12345"
    };
    await service.SaveAsync(secret);

    // Save a config item that references the secret
    var configItem = new ConfigurationItem
    {
      Component = "TTS",
      Category = "Azure",
      Key = "Token",
      Value = "[SECRET:[TTS,Azure,RefreshToken]]"
    };
    await service.SaveAsync(configItem);

    // Act
    var loaded = await service.LoadAsync("TTS", "Token");

    // Assert
    Assert.NotNull(loaded);
    Assert.Equal("actual-secret-token-12345", loaded.Value);
  }

  [Theory]
  [InlineData(StorageType.Json)]
  [InlineData(StorageType.SQLite)]
  public async Task SecretResolution_ShouldHandleMultipleSecrets(StorageType storageType)
  {
    // Arrange
    var service = CreateService(storageType);
    
    // Save secrets
    var secret1 = new ConfigurationItem
    {
      Component = "Secrets",
      Category = "Spotify_Auth",
      Key = "ClientId",
      Value = "client-123"
    };
    var secret2 = new ConfigurationItem
    {
      Component = "Secrets",
      Category = "Spotify_Auth",
      Key = "ClientSecret",
      Value = "secret-456"
    };
    await service.SaveAsync(secret1);
    await service.SaveAsync(secret2);

    // Save a config item that references multiple secrets
    var configItem = new ConfigurationItem
    {
      Component = "Spotify",
      Category = "Auth",
      Key = "ConnectionString",
      Value = "client=[SECRET:[Spotify,Auth,ClientId]];secret=[SECRET:[Spotify,Auth,ClientSecret]]"
    };
    await service.SaveAsync(configItem);

    // Act
    var loaded = await service.LoadAsync("Spotify", "ConnectionString");

    // Assert
    Assert.NotNull(loaded);
    Assert.Equal("client=client-123;secret=secret-456", loaded.Value);
  }

  [Theory]
  [InlineData(StorageType.Json)]
  [InlineData(StorageType.SQLite)]
  public async Task BackupAsync_ShouldCreateBackupFile(StorageType storageType)
  {
    // Arrange
    var service = CreateService(storageType);
    var items = new[]
    {
      new ConfigurationItem { Component = "Spotify", Key = "ApiKey", Value = "key1", Category = "Auth" },
      new ConfigurationItem { Component = "TTS", Key = "Provider", Value = "Azure", Category = "Config" }
    };

    foreach (var item in items)
    {
      await service.SaveAsync(item);
    }

    // Act
    var backupPath = await service.BackupAsync(_backupDirectory);

    // Assert
    Assert.True(File.Exists(backupPath));
    Assert.Contains(storageType == StorageType.Json ? "json-" : "sqlite-", backupPath);
    Assert.EndsWith(".zip", backupPath);

    // Verify zip contains files
    using var archive = ZipFile.OpenRead(backupPath);
    Assert.NotEmpty(archive.Entries);
  }

  [Theory]
  [InlineData(StorageType.Json)]
  [InlineData(StorageType.SQLite)]
  public async Task RestoreAsync_ShouldRestoreFromBackup(StorageType storageType)
  {
    // Arrange
    var service1 = CreateService(storageType);
    var items = new[]
    {
      new ConfigurationItem { Component = "Spotify", Key = "ApiKey", Value = "key1", Category = "Auth" },
      new ConfigurationItem { Component = "TTS", Key = "Provider", Value = "Azure", Category = "Config" }
    };

    foreach (var item in items)
    {
      await service1.SaveAsync(item);
    }

    // Create backup
    var backupPath = await service1.BackupAsync(_backupDirectory);

    // Create a new service instance (simulating a fresh start)
    var service2 = CreateService(storageType, suffix: "_restored");

    // Act
    await service2.RestoreAsync(backupPath);

    // Assert
    var restoredItems = await service2.LoadAllAsync();
    Assert.Equal(2, restoredItems.Count());
    
    var spotifyItem = await service2.LoadAsync("Spotify", "ApiKey");
    Assert.NotNull(spotifyItem);
    Assert.Equal("key1", spotifyItem.Value);
  }

  [Fact]
  public async Task ReplicateToAsync_ShouldCopyDataBetweenServices()
  {
    // Arrange
    var jsonService = new JsonConfigurationService(_jsonStoragePath);
    var sqliteService = new SqliteConfigurationService(_sqliteFilePath);

    var items = new[]
    {
      new ConfigurationItem { Component = "Spotify", Key = "ApiKey", Value = "key1", Category = "Auth" },
      new ConfigurationItem { Component = "TTS", Key = "Provider", Value = "Azure", Category = "Config" },
      new ConfigurationItem { Component = "Audio", Key = "Volume", Value = "75", Category = "Settings" }
    };

    foreach (var item in items)
    {
      await jsonService.SaveAsync(item);
    }

    // Act
    await jsonService.ReplicateToAsync(sqliteService);

    // Assert
    var sqliteItems = await sqliteService.LoadAllAsync();
    Assert.Equal(3, sqliteItems.Count());

    var spotifyItem = await sqliteService.LoadAsync("Spotify", "ApiKey");
    Assert.NotNull(spotifyItem);
    Assert.Equal("key1", spotifyItem.Value);
  }

  [Theory]
  [InlineData(StorageType.Json)]
  [InlineData(StorageType.SQLite)]
  public async Task SaveAsync_WithoutComponent_ShouldThrowException(StorageType storageType)
  {
    // Arrange
    var service = CreateService(storageType);
    var item = new ConfigurationItem
    {
      Component = "", // Empty component
      Key = "TestKey",
      Value = "TestValue",
      Category = "General"
    };

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAsync(item));
  }

  [Theory]
  [InlineData(StorageType.Json)]
  [InlineData(StorageType.SQLite)]
  public async Task LegacyLoadAsync_ShouldWorkWithoutComponent(StorageType storageType)
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

    // Act - Using legacy method
    var loaded = await service.LoadAsync("TestKey");

    // Assert
    Assert.NotNull(loaded);
    Assert.Equal("TestKey", loaded.Key);
    Assert.Equal("TestValue", loaded.Value);
  }

//  [Theory]
//  [InlineData(StorageType.Json)]
//  [InlineData(StorageType.SQLite)]
//  public async Task LegacyDeleteAsync_ShouldWorkWithoutComponent(StorageType storageType)
//  {
//    // Arrange
//    var service = CreateService(storageType);
//    var item = new ConfigurationItem
//    {
//      Component = "TestComponent",
//      Key = "TestKey",
//      Value = "TestValue",
//      Category = "General"
//    };
//    await service.SaveAsync(item);
//
//    // Act - Using legacy method
//    await service.DeleteAsync("TestKey");
//
//    // Assert
//    var loaded = await service.LoadAsync("TestComponent", "TestKey");
//    Assert.Null(loaded);
//  }

  [Theory]
  [InlineData(StorageType.Json)]
  [InlineData(StorageType.SQLite)]
  public async Task LegacyExistsAsync_ShouldWorkWithoutComponent(StorageType storageType)
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

    // Act - Using legacy method
    var exists = await service.ExistsAsync("TestKey");

    // Assert
    Assert.True(exists);
  }

  private IConfigurationService CreateService(StorageType storageType, string suffix = "")
  {
    return storageType switch
    {
      StorageType.Json => new JsonConfigurationService(_jsonStoragePath + suffix),
      StorageType.SQLite => new SqliteConfigurationService(_sqliteFilePath.Replace(".db", $"{suffix}.db")),
      _ => throw new ArgumentException($"Unsupported storage type: {storageType}")
    };
  }
}
