# Radio Console Secrets Tool

A command-line utility for managing secrets in the Radio Console configuration system.

## Overview

The Secrets Tool provides a simple interface to add, list, and delete secrets that can be referenced in configuration values. Secrets are stored in a special "Secrets" component and can be automatically resolved when configuration values are loaded.

## Installation

Build the tool:

```bash
dotnet build RadioConsole/RadioConsole.SecretsTool/RadioConsole.SecretsTool.csproj
```

## Usage

### Basic Commands

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

### Storage Options

By default, the tool uses JSON storage in `./storage`. You can specify different storage options:

#### JSON Storage (Default)

```bash
dotnet run --project RadioConsole/RadioConsole.SecretsTool/RadioConsole.SecretsTool.csproj -- \
  upsert \
  --storage-type json \
  --storage-path ./custom/path \
  --component Spotify \
  --category Auth \
  --key ClientSecret \
  --value "secret123"
```

#### SQLite Storage

```bash
dotnet run --project RadioConsole/RadioConsole.SecretsTool/RadioConsole.SecretsTool.csproj -- \
  upsert \
  --storage-type sqlite \
  --storage-path ./storage/config.db \
  --component Spotify \
  --category Auth \
  --key ClientSecret \
  --value "secret123"
```

## Secret Format

Secrets are stored in a special "Secrets" component with a concatenated category format:

- **Component**: Always "Secrets"
- **Category**: `{OriginalComponent}_{OriginalCategory}` (e.g., `TTS_Azure`, `Spotify_Auth`)
- **Key**: The secret key name
- **Value**: The actual secret value

## Referencing Secrets in Configuration

To reference a secret in a configuration value, use the following format:

```
[SECRET:[Component,Category,Key]]
```

### Example

If you have stored a secret:

```bash
dotnet run --project RadioConsole/RadioConsole.SecretsTool/RadioConsole.SecretsTool.csproj -- \
  upsert \
  --component TTS \
  --category Azure \
  --key RefreshToken \
  --value "actual-refresh-token"
```

You can reference it in a configuration value:

```csharp
var config = new ConfigurationItem
{
    Component = "TTS",
    Category = "Azure",
    Key = "ApiConnection",
    Value = "token=[SECRET:[TTS,Azure,RefreshToken]];endpoint=https://api.example.com"
};

await configService.SaveAsync(config);

// When loaded, the secret will be automatically resolved:
var loaded = await configService.LoadAsync("TTS", "ApiConnection");
// loaded.Value will be: "token=actual-refresh-token;endpoint=https://api.example.com"
```

## Command Reference

### upsert

Add or update a secret.

**Required Options:**
- `--component <name>`: Component name (e.g., TTS, Spotify, Audio)
- `--category <name>`: Category name (e.g., Azure, Auth, Settings)
- `--key <name>`: Secret key name
- `--value <value>`: Secret value

**Optional Options:**
- `--storage-type <json|sqlite>`: Storage type (default: json)
- `--storage-path <path>`: Storage path

### list

List all secrets (values are masked for security).

**Optional Options:**
- `--storage-type <json|sqlite>`: Storage type (default: json)
- `--storage-path <path>`: Storage path

### delete

Delete a secret.

**Required Options:**
- `--category <name>`: Category name (concatenated format, e.g., TTS_Azure)
- `--key <name>`: Secret key name

**Optional Options:**
- `--storage-type <json|sqlite>`: Storage type (default: json)
- `--storage-path <path>`: Storage path

## Security Considerations

1. **Value Masking**: When listing secrets, values are automatically masked to prevent accidental exposure.
2. **File Permissions**: Ensure that storage directories and files have appropriate permissions to prevent unauthorized access.
3. **Backup Security**: When backing up configuration that includes the Secrets component, ensure backups are stored securely.
4. **Environment-Specific**: Consider using different storage locations for different environments (dev, staging, production).

## Examples

### Complete Workflow

```bash
# 1. Add a secret for Spotify authentication
dotnet run --project RadioConsole/RadioConsole.SecretsTool/RadioConsole.SecretsTool.csproj -- \
  upsert \
  --component Spotify \
  --category Auth \
  --key ClientSecret \
  --value "spotify-client-secret-123"

# 2. Add a secret for TTS service
dotnet run --project RadioConsole/RadioConsole.SecretsTool/RadioConsole.SecretsTool.csproj -- \
  upsert \
  --component TTS \
  --category Azure \
  --key ApiKey \
  --value "azure-tts-api-key-456"

# 3. List all secrets to verify
dotnet run --project RadioConsole/RadioConsole.SecretsTool/RadioConsole.SecretsTool.csproj -- list

# 4. Update an existing secret
dotnet run --project RadioConsole/RadioConsole.SecretsTool/RadioConsole.SecretsTool.csproj -- \
  upsert \
  --component Spotify \
  --category Auth \
  --key ClientSecret \
  --value "new-spotify-client-secret-789"

# 5. Delete a secret
dotnet run --project RadioConsole/RadioConsole.SecretsTool/RadioConsole.SecretsTool.csproj -- \
  delete \
  --category Spotify_Auth \
  --key ClientSecret
```

## See Also

- [Configuration Service Documentation](../../CONFIGURATION_SERVICE.md)
- [Radio Console Architecture](../../README.md)
