# Spotify Integration Guide

## Overview

The RadioConsole.Api now includes comprehensive Spotify integration through the SpotifyInput module. This allows you to:

- Access your favorite songs and playlists
- View recently played tracks
- Get personalized recommendations
- Search for songs, artists, and albums
- Control playback (Start/Stop/Pause/Resume)
- Retrieve current track metadata

## Configuration

### Environment Variables

The Spotify integration can be configured using environment variables:

```bash
# Required for all modes
export SPOTIFY_CLIENT_ID="your_spotify_client_id"

# Optional - for user authentication (PKCE flow)
export SPOTIFY_REFRESH_TOKEN="your_spotify_refresh_token"

# Optional - for client credentials flow (limited access)
export SPOTIFY_CLIENT_SECRET="your_spotify_client_secret"

# Optional - custom redirect URI
export SPOTIFY_REDIRECT_URI="http://localhost:5000/callback"
```

### Configuration File

Alternatively, you can configure Spotify through the storage system. The configuration is stored in the device configuration for the `spotify` input:

```json
{
  "ClientId": "your_spotify_client_id",
  "ClientSecret": "your_spotify_client_secret",
  "RefreshToken": "your_spotify_refresh_token",
  "RedirectUri": "http://localhost:5000/callback",
  "UseSimulation": false,
  "MaxItems": 50,
  "Market": "US"
}
```

## Authentication Modes

### 1. PKCE Flow (Recommended for User Data)

This mode provides full access to user-specific data like playlists, favorites, and recently played tracks.

**Requirements:**
- `SPOTIFY_CLIENT_ID`
- `SPOTIFY_REFRESH_TOKEN`

**Obtaining a Refresh Token:**
1. Create a Spotify application at https://developer.spotify.com/dashboard
2. Set up OAuth redirect URI
3. Use the Spotify Authorization Code with PKCE flow to obtain an initial access token
4. The refresh token is included in the initial token response
5. Store the refresh token securely in your environment

### 2. Client Credentials Flow (Fallback)

This mode provides limited access - only public data like search and browse features.

**Requirements:**
- `SPOTIFY_CLIENT_ID`
- `SPOTIFY_CLIENT_SECRET`

**Note:** This mode cannot access user-specific data (playlists, favorites, recently played).

### 3. Simulation Mode (Testing)

For development and testing without Spotify credentials:

```csharp
// Simulation mode is automatically enabled if:
// - Running on non-Raspberry Pi hardware, OR
// - UseSimulation is set to true in configuration
```

## Usage Examples

### Getting Favorite Songs

```csharp
var spotifyInput = serviceProvider.GetRequiredService<SpotifyInput>();
await spotifyInput.InitializeAsync();

var favorites = await spotifyInput.GetFavoriteSongsAsync(limit: 20);
foreach (var track in favorites)
{
    Console.WriteLine($"{track.Artist} - {track.Name}");
}
```

### Searching for Tracks

```csharp
var results = await spotifyInput.SearchAsync("Bohemian Rhapsody", limit: 10);
foreach (var track in results.Tracks)
{
    Console.WriteLine($"{track.Artist} - {track.Name} ({track.Album})");
}
```

### Playback Control

```csharp
// Play a specific track
await spotifyInput.PlayTrackAsync("spotify:track:3n3Ppam7vgaVa1iaRUc9Lp");

// Start playback
await spotifyInput.StartAsync();

// Pause
await spotifyInput.PauseAsync();

// Resume
await spotifyInput.ResumeAsync();

// Stop
await spotifyInput.StopAsync();
```

### Getting Current Track

```csharp
var currentTrack = await spotifyInput.GetCurrentlyPlayingAsync();
if (currentTrack != null)
{
    Console.WriteLine($"Now Playing: {currentTrack.Artist} - {currentTrack.Name}");
    Console.WriteLine($"Album: {currentTrack.Album}");
    Console.WriteLine($"Duration: {TimeSpan.FromMilliseconds(currentTrack.DurationMs):mm\\:ss}");
}
```

### Getting Recommendations

```csharp
// General recommendations based on listening history
var recommendations = await spotifyInput.GetGeneralRecommendationsAsync(limit: 20);

// Audiobook recommendations
var audiobooks = await spotifyInput.GetAudiobookRecommendationsAsync(limit: 10);
```

### Getting Playlists

```csharp
var playlists = await spotifyInput.GetOwnedPlaylistsAsync(limit: 50);
foreach (var playlist in playlists)
{
    Console.WriteLine($"{playlist.Name} ({playlist.TrackCount} tracks)");
    Console.WriteLine($"  {playlist.Description}");
}
```

### Recently Played

```csharp
var recentTracks = await spotifyInput.GetRecentlyPlayedAsync(limit: 20);
foreach (var track in recentTracks)
{
    Console.WriteLine($"{track.Artist} - {track.Name}");
}
```

## Testing

### Console Test Application

Use the RadioConsole.TestApp to test the Spotify integration:

```bash
cd src/RadioConsole.TestApp
dotnet run
```

### Unit Tests

Run the comprehensive unit tests:

```bash
dotnet test --filter "FullyQualifiedName~SpotifyInputTests"
```

All 32 Spotify tests include simulation mode for testing without credentials.

## API Integration Details

The SpotifyInput uses the [SpotifyAPI-NET](https://github.com/JohnnyCrazy/SpotifyAPI-NET) library (v7.2.1) for all Spotify API interactions.

### Supported Features

- ✅ User's saved tracks (favorites)
- ✅ User's playlists
- ✅ Recently played tracks
- ✅ Personalized recommendations
- ✅ Search (tracks, artists, albums, playlists)
- ✅ Playback control
- ✅ Current playback information
- ✅ Track metadata

### Limitations

- Audio streaming is not directly supported by the Spotify Web API
- Playback requires an active Spotify player (app, web player, or Connect device)
- Some features require user authentication (PKCE flow)

## Security Notes

1. **Never commit credentials** - Always use environment variables or secure configuration storage
2. **Refresh tokens are sensitive** - Treat them like passwords
3. **Use HTTPS** in production for OAuth redirect URIs
4. **Rotate credentials** regularly
5. **Monitor API usage** to stay within Spotify's rate limits

## Troubleshooting

### "Spotify credentials not configured"

- Ensure `SPOTIFY_CLIENT_ID` is set
- For user data access, also set `SPOTIFY_REFRESH_TOKEN`
- For client credentials flow, also set `SPOTIFY_CLIENT_SECRET`

### "Failed to authenticate with user credentials"

- Verify your refresh token is valid
- Refresh tokens can expire - you may need to re-authenticate
- Check that your Spotify application has the correct scopes

### "Spotify client not initialized"

- Call `InitializeAsync()` before using any Spotify features
- Check that `IsAvailable` is true after initialization

### API Rate Limits

- Spotify has rate limits on API calls
- The implementation respects the `MaxItems` configuration (default: 50)
- Use appropriate limits in your queries to stay within limits

## Additional Resources

- [Spotify Web API Documentation](https://developer.spotify.com/documentation/web-api/)
- [SpotifyAPI-NET Documentation](https://johnnycrazy.github.io/SpotifyAPI-NET/)
- [OAuth 2.0 PKCE Flow](https://oauth.net/2/pkce/)
