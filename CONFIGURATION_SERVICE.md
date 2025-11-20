# Radio Console - Configuration Service Implementation

## Overview
This document provides an overview of the IConfigurationService implementation and how to use it in the Radio Console project. The configuration service now supports component-based organization, secrets management, backup/restore, and replication between storage types.

## Solution Structure

The solution contains six projects following Clean Architecture principles:

- **RadioConsole.Core**: Domain layer with interfaces, models, and enums
- **RadioConsole.Infrastructure**: Implementation layer with concrete services
- **RadioConsole.API**: ASP.NET Core Web API project with Swagger/OpenAPI
- **RadioConsole.Web**: Blazor Server web application
- **RadioConsole.Tests**: xUnit test project
- **RadioConsole.SecretsTool**: Command-line tool for managing secrets

## What's New

### Component-Based Organization
Configuration items are now organized by **Component → Category → Key → Value**:
- **Components** represent system parts (e.g., Spotify, TTS, Audio, Secrets)
- In JSON: Each component is stored in a separate file (e.g., `Spotify.json`, `TTS.json`)
- In SQLite: Each component has its own table (e.g., `Config_Spotify`, `Config_TTS`)

### Secrets Management
- Special "Secrets" component for storing sensitive data
- Secret references: `[SECRET:[Component,Category,Key]]`
- Automatic resolution when loading configuration values
- CLI tool for managing secrets (`RadioConsole.SecretsTool`)

### Backup & Restore
- Create timestamped backups: `BackupAsync()`
- Restore from backups: `RestoreAsync(backupPath)`
- Backups are stored as ZIP files in `./backup` directory

### Replication
- Copy configuration between storage types
- `ReplicateToAsync(target)` enables migration from JSON to SQLite or vice versa

## Configuration

### appsettings.json Configuration

The configuration service storage location and type can be configured in `appsettings.json`:

```json
{
  "ConfigurationStorage": {
    "StoragePath": "./storage",
    "StorageType": "Json",
    "JsonFileName": "config.json",
    "SqliteFileName": "config.db"
  }
}
```

**Configuration Options:**
- `StoragePath`: Directory where configuration files are stored (default: `./storage`)
- `StorageType`: Either `Json` or `SQLite` (default: `Json`)
- `JsonFileName`: Filename for JSON storage (default: `config.json`) - Note: With component-based storage, this is used as the directory path
- `SqliteFileName`: Filename for SQLite database (default: `config.db`)

The storage directory will be automatically created at application startup if it doesn't exist.

### Registration in Dependency Injection

In `Program.cs`, the configuration service is registered using the extension method:

```csharp
builder.Services.AddConfigurationService(builder.Configuration);
```

This reads the settings from `appsettings.json` and creates the appropriate service instance as a singleton.

## IConfigurationService

The configuration service provides a flexible storage mechanism that can switch between JSON and SQLite storage.

### Interface Definition

```csharp
public interface IConfigurationService
{
    // Save/Update operations
    Task SaveAsync(ConfigurationItem item);
    
    // Load operations - Component-based (Recommended)
    Task<ConfigurationItem?> LoadAsync(string component, string key);
    Task<IEnumerable<ConfigurationItem>> LoadByComponentAsync(string component);
    
    // Load operations - Legacy (Backward compatible)
    Task<ConfigurationItem?> LoadAsync(string key);
    Task<IEnumerable<ConfigurationItem>> LoadByCategoryAsync(string category);
    Task<IEnumerable<ConfigurationItem>> LoadAllAsync();
    
    // Component management
    Task<IEnumerable<string>> GetComponentsAsync();
    
    // Delete operations
    Task DeleteAsync(string component, string key);
    Task DeleteAsync(string key); // Legacy
    
    // Existence checks
    Task<bool> ExistsAsync(string component, string key);
    Task<bool> ExistsAsync(string key); // Legacy
    
    // Backup & Restore
    Task<string> BackupAsync(string? backupDirectory = null);
    Task RestoreAsync(string backupPath);
    
    // Replication
    Task ReplicateToAsync(IConfigurationService target);
}
```

### ConfigurationItem Model

```csharp
public class ConfigurationItem
{
    public string Id { get; set; }
    public string Component { get; set; }  // NEW: Component this item belongs to
    public string Key { get; set; }
    public string Value { get; set; }
    public string Category { get; set; }  // For grouping related configuration items
    public DateTime LastUpdated { get; set; }
}
```

## Storage Types

### JSON Storage
- Component-based file storage (one JSON file per component)
- Files named `{Component}.json` (e.g., `Spotify.json`, `TTS.json`)
- Thread-safe operations using SemaphoreSlim
- Automatic directory creation
- Pretty-printed JSON for readability
- Secret resolution on load

### SQLite Storage
- Component-based table storage (one table per component)
- Tables named `Config_{Component}` (e.g., `Config_Spotify`, `Config_TTS`)
- Automatic table creation and indexing
- Optimized queries with indexes on Key and Category columns
- UPSERT operations for efficient updates
- Secret resolution on load

## Usage Examples

### Using Dependency Injection (Recommended)

In your controllers or services, inject `IConfigurationService`:

```csharp
public class MyController : ControllerBase
{
    private readonly IConfigurationService _configService;

    public MyController(IConfigurationService configService)
    {
        _configService = configService;
    }

    public async Task<IActionResult> GetSettings()
    {
        var allSettings = await _configService.LoadAllAsync();
        return Ok(allSettings);
    }
}
```

The service is automatically configured based on your `appsettings.json` settings.

### Manual Creation (For Testing)

```csharp
using RadioConsole.Core.Enums;
using RadioConsole.Infrastructure.Configuration;

// Using JSON storage (provide directory path for component-based storage)
var jsonService = ConfigurationServiceFactory.Create(
    StorageType.Json, 
    "./storage"
);

// Using SQLite storage
var sqliteService = ConfigurationServiceFactory.Create(
    StorageType.SQLite, 
    "./storage/config.db"
);
```

### Saving Configuration (Component-Based)

```csharp
var config = new ConfigurationItem
{
    Component = "Audio",     // NEW: Required component name
    Category = "Settings",
    Key = "Volume",
    Value = "75"
};

await configService.SaveAsync(config);
```

### Loading Configuration

```csharp
// Load by component and key (Recommended)
var volumeConfig = await configService.LoadAsync("Audio", "Volume");
if (volumeConfig != null)
{
    Console.WriteLine($"Volume: {volumeConfig.Value}");
}

// Load all items in a component
var audioConfigs = await configService.LoadByComponentAsync("Audio");

// Load all configurations
var allConfigs = await configService.LoadAllAsync();

// Load by category (across all components)
var settingsConfigs = await configService.LoadByCategoryAsync("Settings");

// Legacy: Load by key only (searches all components)
var config = await configService.LoadAsync("Volume");
```

### Component Management

```csharp
// Get all components
var components = await configService.GetComponentsAsync();
foreach (var component in components)
{
    Console.WriteLine($"Component: {component}");
}
```

### Deleting Configuration

```csharp
// Delete by component and key (Recommended)
await configService.DeleteAsync("Audio", "Volume");

// Legacy: Delete by key only
await configService.DeleteAsync("Volume");
```

### Checking Existence

```csharp
// Check by component and key (Recommended)
bool exists = await configService.ExistsAsync("Audio", "Volume");

// Legacy: Check by key only
bool exists = await configService.ExistsAsync("Volume");
```

## Secrets Management

### Overview

Secrets are stored in a special "Secrets" component and can be referenced in configuration values using the format `[SECRET:[Component,Category,Key]]`. When configuration values are loaded, secret references are automatically resolved.

### Secret Storage Format

Secrets are stored with a concatenated category format:
- **Component**: Always "Secrets"
- **Category**: `{OriginalComponent}_{OriginalCategory}` (e.g., `TTS_Azure`, `Spotify_Auth`)
- **Key**: The secret key name
- **Value**: The actual secret value

### Using the Secrets Tool

The `RadioConsole.SecretsTool` CLI provides commands to manage secrets:

#### Add or Update a Secret

```bash
dotnet run --project RadioConsole/RadioConsole.SecretsTool/RadioConsole.SecretsTool.csproj -- \
  upsert \
  --component TTS \
  --category Azure \
  --key RefreshToken \
  --value "your-secret-token"
```

#### List All Secrets

```bash
dotnet run --project RadioConsole/RadioConsole.SecretsTool/RadioConsole.SecretsTool.csproj -- list
```

#### Delete a Secret

```bash
dotnet run --project RadioConsole/RadioConsole.SecretsTool/RadioConsole.SecretsTool.csproj -- \
  delete \
  --category TTS_Azure \
  --key RefreshToken
```

See [RadioConsole.SecretsTool/README.md](RadioConsole/RadioConsole.SecretsTool/README.md) for complete documentation.

### Programmatic Secret Management

```csharp
// Store a secret
var secret = new ConfigurationItem
{
    Component = "Secrets",
    Category = "TTS_Azure",  // Concatenated format: Component_Category
    Key = "RefreshToken",
    Value = "actual-secret-token"
};
await configService.SaveAsync(secret);

// Reference the secret in a configuration value
var config = new ConfigurationItem
{
    Component = "TTS",
    Category = "Azure",
    Key = "ApiConnection",
    Value = "token=[SECRET:[TTS,Azure,RefreshToken]];endpoint=https://api.example.com"
};
await configService.SaveAsync(config);

// Load the configuration - secret will be automatically resolved
var loaded = await configService.LoadAsync("TTS", "ApiConnection");
// loaded.Value will be: "token=actual-secret-token;endpoint=https://api.example.com"
```

### Secret Reference Format

Use `[SECRET:[Component,Category,Key]]` to reference secrets:

```
[SECRET:[TTS,Azure,RefreshToken]]
[SECRET:[Spotify,Auth,ClientSecret]]
[SECRET:[Audio,Settings,MasterKey]]
```

Multiple secret references can be used in a single value:

```csharp
var connectionString = new ConfigurationItem
{
    Component = "Database",
    Category = "Connection",
    Key = "Primary",
    Value = "Server=localhost;User=[SECRET:[Database,Connection,Username]];Password=[SECRET:[Database,Connection,Password]]"
};
```

## Backup & Restore

### Creating a Backup

```csharp
// Backup to default ./backup directory
var backupPath = await configService.BackupAsync();
Console.WriteLine($"Backup created: {backupPath}");

// Backup to custom directory
var backupPath = await configService.BackupAsync("/path/to/backups");
```

Backup files are named in the format:
- JSON: `json-{timestamp}.zip` (e.g., `json-20251120-143022.zip`)
- SQLite: `sqlite-{timestamp}.zip` (e.g., `sqlite-20251120-143022.zip`)

### Restoring from a Backup

```csharp
// Restore from a backup file
await configService.RestoreAsync("./backup/json-20251120-143022.zip");
```

**Note**: Restoring will overwrite existing configuration data.

## Replication Between Storage Types

Copy configuration data from one storage type to another:

```csharp
// Create source and target services
var jsonService = new JsonConfigurationService("./storage");
var sqliteService = new SqliteConfigurationService("./storage/config.db");

// Replicate from JSON to SQLite
await jsonService.ReplicateToAsync(sqliteService);

// Or replicate from SQLite to JSON
await sqliteService.ReplicateToAsync(jsonService);
```

This is useful for:
- Migrating from JSON to SQLite or vice versa
- Creating synchronized copies
- Testing with different storage backends

## Configuration Categories

The Category property allows you to organize configuration items into logical groups:

- **Auth**: Authentication and authorization settings
- **Settings**: General application settings
- **API**: API keys and endpoints
- **Database**: Database connection information
- **Display**: UI and display preferences

Example organization:

```csharp
// Spotify component with Auth category
var spotifyAuth = new ConfigurationItem
{
    Component = "Spotify",
    Category = "Auth",
    Key = "ClientId",
    Value = "your-client-id"
};

// Audio component with Settings category
var audioSettings = new ConfigurationItem
{
    Component = "Audio",
    Category = "Settings",
    Key = "Volume",
    Value = "75"
};
```

This makes it easy to retrieve all related settings at once using `LoadByComponentAsync()` or `LoadByCategoryAsync()`.

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

The project includes comprehensive unit tests covering:
- Save/Update operations
- Load operations (by component, by key, all, by category)
- Component management (GetComponentsAsync, LoadByComponentAsync)
- Delete operations
- Existence checks
- Timestamp validation
- Factory pattern creation
- Secret resolution (single and multiple references)
- Backup and restore operations
- Replication between storage types
- Legacy method backward compatibility

All tests use xUnit theory attributes to test both JSON and SQLite implementations with the same test cases.

### Running Tests

```bash
dotnet test RadioConsole.sln

# Run only configuration tests
dotnet test --filter "FullyQualifiedName~Configuration"
```

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

### Run the Secrets Tool
```bash
cd RadioConsole/RadioConsole.SecretsTool
dotnet run -- --help
```

### Run Tests
```bash
dotnet test RadioConsole.sln
```

## Migration Guide

### Upgrading from Previous Version

If you have existing configuration data without the Component property, you'll need to migrate it:

```csharp
// Load old configuration
var oldItems = await configService.LoadAllAsync();

// Update each item to include a component
foreach (var item in oldItems)
{
    if (string.IsNullOrEmpty(item.Component))
    {
        // Assign a default component or determine from category/key
        item.Component = "Legacy"; // or determine based on your logic
        await configService.SaveAsync(item);
    }
}
```

### File Structure Changes

**Before (single file):**
```
storage/
  └── config.json  (all configuration in one file)
```

**After (component-based):**
```
storage/
  ├── Spotify.json
  ├── TTS.json
  ├── Audio.json
  ├── Secrets.json
  └── ... (one file per component)
```

### Database Structure Changes

**Before (single table):**
```sql
ConfigurationItems (Id, Key, Value, Category, LastUpdated)
```

**After (component-based):**
```sql
ConfigComponents (Component, CreatedAt)
Config_Spotify (Id, Component, Key, Value, Category, LastUpdated)
Config_TTS (Id, Component, Key, Value, Category, LastUpdated)
Config_Audio (Id, Component, Key, Value, Category, LastUpdated)
Config_Secrets (Id, Component, Key, Value, Category, LastUpdated)
...
```

## Security Considerations

### Secrets Management
- Store secrets only in the "Secrets" component
- Use the RadioConsole.SecretsTool CLI for secrets management
- Avoid hardcoding secrets in application code
- Regularly rotate secrets using the upsert command
- Ensure storage directories have appropriate file permissions

### Backup Security
- Store backups in secure locations
- Encrypt backup files if they contain sensitive data
- Implement backup retention policies
- Test restore procedures regularly

### Access Control
- Restrict access to storage directories
- Use environment-specific storage paths
- Consider encrypting the SQLite database
- Audit configuration access and changes

## Troubleshooting

### Component Not Found
```
Error: Component cannot be empty
```
**Solution**: Ensure all ConfigurationItem objects have the Component property set.

### Secret Not Resolving
```
Value still contains [SECRET:...] after loading
```
**Solution**: Verify the secret exists in the Secrets component with the correct concatenated category format (Component_Category).

### Backup/Restore Issues
```
Error: Backup file not found
```
**Solution**: Check the backup path and ensure the file exists. Use absolute paths when possible.

### Migration Issues
```
Error: Old configuration format detected
```
**Solution**: Run the migration script to add Component properties to existing configuration items.

## Future Enhancements

The IConfigurationService provides a foundation for:
- Encrypted configuration storage
- Remote configuration sources (Azure App Configuration, AWS Parameter Store)
- Configuration change notifications
- Configuration versioning and rollback
- Advanced secret management (Azure Key Vault, HashiCorp Vault integration)
- Configuration validation and schema enforcement
- Role-based access control for configuration changes
