# Radio Console - Configuration Service Implementation

## Overview
This document provides an overview of the IConfigurationService implementation and how to use it in the Radio Console project.

## Solution Structure

The solution contains five projects following Clean Architecture principles:

- **RadioConsole.Core**: Domain layer with interfaces, models, and enums
- **RadioConsole.Infrastructure**: Implementation layer with concrete services
- **RadioConsole.API**: ASP.NET Core Web API project with Swagger/OpenAPI
- **RadioConsole.Web**: Blazor Server web application
- **RadioConsole.Tests**: xUnit test project

## IConfigurationService

The configuration service provides a flexible storage mechanism that can switch between JSON and SQLite storage.

### Interface Definition

```csharp
public interface IConfigurationService
{
    Task SaveAsync(ConfigurationItem item);
    Task<ConfigurationItem?> LoadAsync(string key);
    Task<IEnumerable<ConfigurationItem>> LoadAllAsync();
    Task<IEnumerable<ConfigurationItem>> LoadByCategoryAsync(string category);
    Task DeleteAsync(string key);
    Task<bool> ExistsAsync(string key);
}
```

### ConfigurationItem Model

```csharp
public class ConfigurationItem
{
    public string Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public string Category { get; set; }  // NEW: For grouping related configuration items
    public DateTime LastUpdated { get; set; }
}
```

## Storage Types

### JSON Storage
- File-based storage using System.Text.Json
- Thread-safe operations using SemaphoreSlim
- Automatic directory creation
- Pretty-printed JSON for readability

### SQLite Storage
- Database storage using Microsoft.Data.Sqlite
- Automatic table creation and indexing
- Optimized queries with indexes on Key and Category columns
- UPSERT operations for efficient updates

## Usage Examples

### Creating a Configuration Service

```csharp
using RadioConsole.Core.Enums;
using RadioConsole.Infrastructure.Configuration;

// Using JSON storage
var jsonService = ConfigurationServiceFactory.Create(
    StorageType.Json, 
    "config/settings.json"
);

// Using SQLite storage
var sqliteService = ConfigurationServiceFactory.Create(
    StorageType.SQLite, 
    "config/settings.db"
);
```

### Saving Configuration

```csharp
var config = new ConfigurationItem
{
    Key = "Volume",
    Value = "75",
    Category = "Audio"
};

await configService.SaveAsync(config);
```

### Loading Configuration

```csharp
// Load by key
var volumeConfig = await configService.LoadAsync("Volume");
if (volumeConfig != null)
{
    Console.WriteLine($"Volume: {volumeConfig.Value}");
}

// Load all configurations
var allConfigs = await configService.LoadAllAsync();

// Load by category
var audioConfigs = await configService.LoadByCategoryAsync("Audio");
```

### Deleting Configuration

```csharp
await configService.DeleteAsync("Volume");
```

### Checking Existence

```csharp
bool exists = await configService.ExistsAsync("Volume");
```

## Serilog Integration

Both API and Web projects are configured with Serilog for logging:

### Features
- Console logging for development/debugging
- File logging with daily rolling (logs/api-YYYYMMDD.log, logs/web-YYYYMMDD.log)
- Structured logging with timestamps
- Automatic log rotation

### Log Locations
- API logs: `RadioConsole.API/logs/api-YYYYMMDD.log`
- Web logs: `RadioConsole.Web/logs/web-YYYYMMDD.log`

## Testing

The project includes 20 comprehensive unit tests covering:
- Save/Update operations
- Load operations (by key, all, by category)
- Delete operations
- Existence checks
- Timestamp validation
- Factory pattern creation

All tests use xUnit theory attributes to test both JSON and SQLite implementations with the same test cases.

### Running Tests

```bash
dotnet test RadioConsole.sln
```

## Configuration Categories

The Category property allows you to organize configuration items into logical groups:

- **Audio**: Volume, Balance, Equalization settings
- **Radio**: Frequency, Band, Station presets
- **Spotify**: API keys, User preferences
- **System**: Application settings, Hardware configuration
- **Display**: Theme, Screen settings

This makes it easy to retrieve all related settings at once using `LoadByCategoryAsync()`.

## Building and Running

### Build the solution
```bash
dotnet build RadioConsole.sln
```

### Run the API
```bash
cd RadioConsole/RadioConsole.API
dotnet run
```

### Run the Web Application
```bash
cd RadioConsole/RadioConsole.Web
dotnet run
```

### Run Tests
```bash
dotnet test RadioConsole.sln
```

## Future Enhancements

The IConfigurationService provides a foundation for:
- Encrypted configuration storage
- Remote configuration sources
- Configuration change notifications
- Configuration versioning and rollback
- Import/Export functionality
