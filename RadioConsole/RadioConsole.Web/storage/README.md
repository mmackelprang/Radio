# Storage Directory

This directory contains configuration files for the Radio Console application.

## Configuration Files

Configuration is stored in JSON files, one per component. Each file contains an array of configuration items with the following structure:

```json
{
  "Id": "unique-id",
  "Component": "ComponentName",
  "Category": "CategoryName",
  "Key": "ConfigKey",
  "Value": "ConfigValue",
  "LastUpdated": "2025-01-01T00:00:00Z"
}
```

## Spotify Configuration

The `Spotify.json` file contains the configuration for Spotify integration. You need to obtain these credentials from the Spotify Developer Dashboard:

1. **ClientId**: Your Spotify application's Client ID
   - Get it from: https://developer.spotify.com/dashboard
   - Create a new application if you don't have one

2. **ClientSecret**: Your Spotify application's Client Secret
   - Found in the same location as the Client ID

3. **RefreshToken**: A refresh token for user-level operations (optional for basic search)
   - Required for: playback control, getting currently playing track
   - Not required for: searching tracks and albums
   - To obtain a refresh token, you need to implement OAuth2 authorization flow
   - See: https://developer.spotify.com/documentation/web-api/tutorials/code-flow

### Example Configuration

```json
[
  {
    "Id": "spotify-client-id",
    "Component": "Spotify",
    "Category": "Spotify",
    "Key": "ClientId",
    "Value": "abc123def456",
    "LastUpdated": "2025-01-01T00:00:00Z"
  },
  {
    "Id": "spotify-client-secret",
    "Component": "Spotify",
    "Category": "Spotify",
    "Key": "ClientSecret",
    "Value": "xyz789uvw012",
    "LastUpdated": "2025-01-01T00:00:00Z"
  },
  {
    "Id": "spotify-refresh-token",
    "Component": "Spotify",
    "Category": "Spotify",
    "Key": "RefreshToken",
    "Value": "your-refresh-token-here",
    "LastUpdated": "2025-01-01T00:00:00Z"
  }
]
```

### Notes

- The Spotify service will automatically load these values on startup via `InitializeFromConfigurationAsync()`
- If only ClientId and ClientSecret are provided, the service will use Client Credentials flow (search/browse only)
- If RefreshToken is also provided, the service will use Authorization Code flow (full playback control)
- Values are case-sensitive
- Keep these credentials secure and never commit actual credentials to version control
