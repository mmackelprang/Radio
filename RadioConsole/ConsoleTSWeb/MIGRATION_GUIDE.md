# Migration Guide: ConsoleTSWeb to RadioConsole.API Integration

This guide provides detailed phases for integrating the TypeScript web application (`ConsoleTSWeb`) with the RadioConsole.API backend. Each phase includes specific tasks, technical details, and prompts suitable for coding agents.

---

## Prerequisites

Before starting migration:

1. **Read Documentation:**
   - `TYPESCRIPT_APP_OVERVIEW.md` - Understand the UI structure
   - `API_ENDPOINT_RECONCILIATION.md` - See endpoint mapping details
   - `MISSING_ENDPOINTS.md` - Current endpoint status

2. **Ensure API is Running:**
   ```bash
   cd RadioConsole/RadioConsole.API
   dotnet run
   ```
   API should be accessible at `http://localhost:5100`
   Swagger UI at `http://localhost:5100/swagger`

3. **Ensure TypeScript App Dependencies:**
   ```bash
   cd RadioConsole/ConsoleTSWeb
   npm install
   ```

---

## Phase 1: Backend API Enhancement - Missing Core Endpoints

**Goal:** Implement the critical missing endpoints needed for basic UI functionality.

**Estimated Effort:** 3-5 days

### Phase 1.1: Unified Audio Status Controller

**Coding Agent Prompt:**
```
Create a new UnifiedAudioController in RadioConsole.API/Controllers that provides a unified audio status endpoint combining:
- Current input/output devices (from AudioDeviceManager)
- Volume and balance (new properties to add)
- Playback status (playing/paused)
- Shuffle mode status

Implement:
1. GET /api/audio/status - Returns unified status
2. POST /api/audio/volume - Sets master volume (0-100)
3. POST /api/audio/balance - Sets stereo balance (-100 to +100)
4. POST /api/audio/playback - Unified playback control (play/pause/next/previous)
5. POST /api/audio/shuffle - Toggle shuffle mode

Requirements:
- Integrate with IAudioPlayer for volume/playback control
- Store current state (volume, balance, shuffle) in memory or configuration
- Use existing AudioDeviceManager for device info
- Follow existing controller patterns (logging, error handling, swagger docs)
- Return appropriate HTTP status codes (200, 400, 404, 500)
```

### Phase 1.2: Radio Enhancement Controller

**Coding Agent Prompt:**
```
Enhance the existing RaddyRadioController in RadioConsole.API/Controllers to add missing functionality:

Add new endpoints:
1. POST /api/RaddyRadio/tune - Step tune up/down by frequency increment
   - Request: { "direction": "up"|"down", "step": 0.2 }
   - Get current frequency, add/subtract step, call SetFrequency

2. POST /api/RaddyRadio/band - Switch radio band (if hardware supports)
   - Request: { "band": "FM"|"AM"|"SW"|"AIR"|"VHF" }
   - Map band to frequency range and set appropriate frequency

3. POST /api/RaddyRadio/scan - Scan for next station
   - Request: { "direction": "up"|"down" }
   - Implement async scanning using signal strength monitoring
   - Return when signal threshold is met (signalStrength >= 3)
   - Consider long-running operation, may need background task

4. GET /api/RaddyRadio/stations - Get saved stations
   - Load from Configuration service
   - Return collection of saved station presets

5. POST /api/RaddyRadio/save-station - Save current station
   - Request: { "name": "Station Name", "frequency": 101.5, "band": "FM" }
   - Save to Configuration service under "Radio/Stations" component

Requirements:
- Follow existing RaddyRadioController patterns
- Use IRaddyRadioService for hardware interaction
- Use IConfigurationService for station persistence
- Add appropriate error handling and logging
```

### Phase 1.3: File Player Controller

**Coding Agent Prompt:**
```
Create a new FilePlayerController in RadioConsole.API/Controllers for local file playback:

Implement:
1. GET /api/file-player/current - Get currently playing file info
   - Return: songName, fileName, artist, duration, currentTime, albumArtUrl
   - Extract metadata from audio file headers (use TagLib# or similar)

2. GET /api/file-player/browse?path=/music - Browse file system
   - List directories and audio files in specified path
   - Filter for audio formats: .mp3, .flac, .wav, .m4a, .aac, .ogg
   - Security: Restrict browsing to configured base music directories
   - Return: { path, items: [{name, type: "file"|"folder", path}] }

3. POST /api/file-player/select - Select file/folder for playback
   - Request: { "path": "/music/album/song.mp3" }
   - If file: Load into audio player, start playback
   - If folder: Generate playlist from all audio files in folder (recursive)
   - Use IAudioPlayer for playback

Requirements:
- Add configuration for allowed music directories (appsettings.json)
- Validate paths to prevent directory traversal attacks
- Use System.IO for file enumeration
- Consider pagination for large directories
- Integrate with IAudioPlayer from RadioConsole.Core
```

### Phase 1.4: Playlist Controller

**Coding Agent Prompt:**
```
Create a new PlaylistController in RadioConsole.API/Controllers for queue management:

Implement:
1. GET /api/playlist - Get current playlist/queue
   - Return: { items: [{id, songName, artist, duration}] }
   - Maintain in-memory queue or use configuration for persistence

2. POST /api/playlist/add - Add track to playlist
   - Request: { "trackId": "spotify:track:123" or "/path/to/file.mp3" }
   - Support Spotify URIs and local file paths
   - Append to current queue

3. DELETE /api/playlist/{itemId} - Remove track from playlist
   - Remove specified item from queue

4. POST /api/playlist/reorder - Reorder playlist items
   - Request: { "fromIndex": 0, "toIndex": 2 }
   - Move item from one position to another

Requirements:
- Create a PlaylistService to manage queue state
- Use List<PlaylistItem> in memory initially
- Consider thread-safety (lock/concurrent collections)
- Each playlist item needs: id, source (spotify/file/radio), metadata
- Integrate with active audio source for playback
```

### Phase 1.5: Vinyl and Equalization

**Coding Agent Prompt:**
```
Create controllers for Vinyl player and Audio Equalization:

VinylController (RadioConsole.API/Controllers/VinylController.cs):
1. GET /api/vinyl/status - Get vinyl player status
   - Return: { preampEnabled, isPlaying }
   - Read from configuration or hardware state

2. POST /api/vinyl/preamp - Toggle preamp
   - Request: { "enabled": true }
   - Store preference in configuration
   - If hardware control available, trigger GPIO/USB command

EqualizationController (RadioConsole.API/Controllers/EqualizationController.cs):
1. GET /api/equalization/presets - Get available EQ presets
   - Return: ["flat", "bass-boost", "treble-boost", "voice", "classical", "rock"]

2. GET /api/equalization/current - Get current preset
   - Return: { preset: "flat" }

3. POST /api/equalization - Set EQ preset
   - Request: { "preset": "bass-boost" }
   - Apply EQ settings via SoundFlow or audio device
   - Store preference in configuration

Requirements:
- Use IConfigurationService for storing preferences
- Integrate with audio system for actual EQ application
- Add logging and error handling
```

---

## Phase 2: Backend API Enhancement - Spotify & System

**Goal:** Enhance Spotify integration and system management features.

**Estimated Effort:** 2-3 days

### Phase 2.1: Spotify Controller Enhancement

**Coding Agent Prompt:**
```
Enhance Spotify integration in RadioConsole.API:

Option 1: Enhance existing NowPlayingController:
- Add duration, currentTime, and liked fields to SpotifyNowPlaying response
- Requires extending ISpotifyService to track playback position

Option 2: Create dedicated SpotifyController (RadioConsole.API/Controllers/SpotifyController.cs):

Implement:
1. GET /api/spotify/current - Get current track with full details
   - Return: { name, artist, album, duration, currentTime, albumArtUrl, liked }
   - Use ISpotifyService to query Spotify Web API
   - Track playback position (may need background polling or webhook)

2. POST /api/spotify/like - Toggle like status
   - Request: { "liked": true }
   - Call Spotify API to save/remove track from user's library
   - Update ISpotifyService interface if needed

3. POST /api/spotify/play - Start playback
4. POST /api/spotify/pause - Pause playback
5. POST /api/spotify/next - Next track
6. POST /api/spotify/previous - Previous track

Requirements:
- Extend ISpotifyService interface in RadioConsole.Core
- Implement in SpotifyService in RadioConsole.Infrastructure
- Handle Spotify API authentication and token refresh
- Add error handling for API rate limits
- Consider caching track info to reduce API calls
```

### Phase 2.2: System Management Enhancement

**Coding Agent Prompt:**
```
Enhance SystemStatusController in RadioConsole.API/Controllers:

Modifications to existing GET /api/SystemStatus:
1. Add Version and BuildDate properties to SystemStatus model
   - Read from Assembly version or appsettings.json
   - Format: { version: "2.4.1", buildDate: "2024-11-15" }

Add new endpoints:
2. POST /api/system/shutdown - Shutdown system
   - Request: { "confirmed": true }
   - Validation: Require confirmation flag
   - Execute platform-specific shutdown command:
     - Linux/Raspberry Pi: Execute "sudo shutdown -h now"
     - Windows: Execute "shutdown /s /t 0"
   - Return 200 OK before shutdown begins
   - Consider security: Add authentication/authorization

3. GET /api/system/stats - Simplified stats (wrapper endpoint)
   - Return: { cpu: 25, ram: 45, threads: 18 }
   - Map from existing SystemStatus response
   - Convert percentages and simplify format

Requirements:
- Add Version and BuildDate to RadioConsole.Core/Models/SystemStatus.cs
- Implement OS detection for shutdown command
- Add security considerations (require admin role or API key)
- Log shutdown requests for audit trail
```

### Phase 2.3: Prompts Management Controller

**Coding Agent Prompt:**
```
Create PromptsController in RadioConsole.API/Controllers for managing audio prompts (TTS, sound effects):

Implement:
1. GET /api/prompts - Get all prompts
   - Return: { prompts: [{id, name, type: "TTS"|"File", data}] }
   - Load from configuration service

2. POST /api/prompts - Create new prompt
   - Request: { name: "Welcome", type: "TTS", data: "Welcome message" }
   - Validate: name required, type must be TTS or File
   - Save to configuration service
   - Return created prompt with generated ID

3. PUT /api/prompts/{id} - Update prompt
   - Request: { name, type, data }
   - Update in configuration service

4. DELETE /api/prompts/{id} - Delete prompt
   - Remove from configuration service

5. POST /api/prompts/{id}/play - Play prompt
   - Load prompt data
   - If TTS: Convert text to speech, play via audio system
   - If File: Load file path, play via audio system
   - Use AudioPriorityController to trigger high-priority audio (ducking)

Requirements:
- Store prompts in Configuration under "Prompts" component
- For TTS: Integrate with system TTS engine or external API
- For File prompts: Validate file exists and is audio format
- Use IAudioPriorityService to register prompt as high-priority
- Trigger ducking of background music during prompt playback
```

---

## Phase 3: Frontend API Integration Layer

**Goal:** Create TypeScript API client services for backend communication.

**Estimated Effort:** 3-4 days

### Phase 3.1: API Client Infrastructure

**Coding Agent Prompt:**
```
Create API client infrastructure for the TypeScript web app in RadioConsole/ConsoleTSWeb:

Create directory structure:
- src/api/client.ts - Base API client with fetch wrapper
- src/api/types.ts - TypeScript interfaces for API requests/responses
- src/api/hooks/ - React hooks for API calls
- src/api/services/ - Service classes for each API controller

Implement src/api/client.ts:
```typescript
// Base API client with error handling, retries, and timeout
export class ApiClient {
  constructor(baseUrl: string);
  
  async get<T>(path: string, options?: RequestOptions): Promise<T>;
  async post<T>(path: string, data: any, options?: RequestOptions): Promise<T>;
  async put<T>(path: string, data: any, options?: RequestOptions): Promise<T>;
  async delete<T>(path: string, options?: RequestOptions): Promise<T>;
  
  // Interceptors for auth tokens, logging, etc.
  addRequestInterceptor(interceptor: RequestInterceptor): void;
  addResponseInterceptor(interceptor: ResponseInterceptor): void;
}

// Export configured instance
export const apiClient = new ApiClient(getApiBaseUrl());
```

Implement src/api/types.ts:
```typescript
// Define TypeScript interfaces matching API models
export interface AudioStatus {
  volume: number;
  balance: number;
  shuffle: boolean;
  isPlaying: boolean;
  currentInput: string;
  currentOutput: string;
}

export interface RadioStatus {
  frequency: number;
  band: string;
  signalStrength: number;
  volume: number;
  equalization: string;
}

export interface SpotifyTrack {
  name: string;
  artist: string;
  album: string;
  duration: number;
  currentTime: number;
  albumArtUrl: string;
  liked: boolean;
}

// ... etc for all API models
```

Requirements:
- Handle CORS issues (API must allow origins)
- Implement request/response interceptors
- Add error handling with typed error responses
- Add loading states and retry logic
- Support abort signals for cancellation
- Use environment variable for API base URL (default: http://localhost:5100)
```

### Phase 3.2: API Service Hooks

**Coding Agent Prompt:**
```
Create React hooks for API calls in RadioConsole/ConsoleTSWeb/src/api/hooks:

Create custom hooks using React Query or SWR pattern:

src/api/hooks/useAudioStatus.ts:
```typescript
export function useAudioStatus() {
  const [status, setStatus] = useState<AudioStatus | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);
  
  // Fetch audio status
  const refresh = async () => { /* ... */ };
  
  // Update volume
  const setVolume = async (volume: number) => { /* ... */ };
  
  // Update balance
  const setBalance = async (balance: number) => { /* ... */ };
  
  // Control playback
  const play = async () => { /* ... */ };
  const pause = async () => { /* ... */ };
  const next = async () => { /* ... */ };
  const previous = async () => { /* ... */ };
  
  // Toggle shuffle
  const toggleShuffle = async () => { /* ... */ };
  
  useEffect(() => {
    refresh();
    const interval = setInterval(refresh, 5000); // Poll every 5s
    return () => clearInterval(interval);
  }, []);
  
  return { status, isLoading, error, refresh, setVolume, setBalance, play, pause, next, previous, toggleShuffle };
}
```

Create similar hooks for:
- useRadioControl() - Radio frequency, band, tuning, scanning
- useSpotifyPlayer() - Current track, playback control, like
- useFilePlayer() - File browsing, current file, selection
- usePlaylist() - Playlist CRUD operations
- useDeviceManager() - Input/output device management
- useSystemStatus() - System stats, uptime, version
- useConfiguration() - Config CRUD, backup/restore

Requirements:
- Use modern React patterns (hooks, async/await)
- Implement optimistic updates for better UX
- Add proper TypeScript typing
- Handle errors gracefully with user-friendly messages
- Support manual refresh and auto-polling
- Cache data appropriately to reduce API calls
```

### Phase 3.3: WebSocket Integration (Optional)

**Coding Agent Prompt:**
```
Add WebSocket support for real-time updates (OPTIONAL - can be Phase 5):

Backend (RadioConsole.API):
1. Add SignalR package to RadioConsole.API.csproj
2. Create Hubs for real-time communication:
   - AudioStatusHub - Broadcast volume, playback state changes
   - RadioHub - Broadcast frequency changes, signal strength
   - SystemStatusHub - Broadcast CPU/RAM stats

3. Configure SignalR in Program.cs
4. Emit events from services when state changes

Frontend (ConsoleTSWeb):
1. Add @microsoft/signalr package to package.json
2. Create src/api/websocket.ts for SignalR connection
3. Update hooks to use WebSocket events instead of polling
4. Add connection state management (connected/disconnected)

Benefits:
- Real-time playback position updates
- Live signal strength changes
- Instant configuration updates from other clients
- Reduced API polling

Requirements:
- Configure CORS for WebSocket connections
- Handle connection dropouts and reconnection
- Fallback to polling if WebSocket fails
```

---

## Phase 4: Frontend Component Integration

**Goal:** Wire up UI components to use API hooks instead of local state.

**Estimated Effort:** 4-5 days

### Phase 4.1: App.tsx Refactoring

**Coding Agent Prompt:**
```
Refactor RadioConsole/ConsoleTSWeb/src/App.tsx to use API-backed state:

Current state (local useState):
```typescript
const [volume, setVolume] = useState(50);
const [balance, setBalance] = useState(0);
const [shuffle, setShuffle] = useState(false);
const [isPlaying, setIsPlaying] = useState(false);
const [currentInput, setCurrentInput] = useState<InputDevice>('spotify');
```

Replace with API hooks:
```typescript
const { status, setVolume, setBalance, toggleShuffle, play, pause, next, previous } = useAudioStatus();
const { devices, currentInput, currentOutput, setInput, setOutput } = useDeviceManager();

// Use status.volume instead of local volume state
// Use status.isPlaying instead of local isPlaying state
// etc.
```

Requirements:
- Remove all local state that should come from API
- Add loading states while fetching initial data
- Add error boundaries for API failures
- Show error toasts (using sonner) for failed operations
- Implement optimistic updates for better UX
- Keep local state only for UI-only concerns (like showPlaylist)
```

### Phase 4.2: AudioSetup Component Integration

**Coding Agent Prompt:**
```
Update RadioConsole/ConsoleTSWeb/src/components/AudioSetup.tsx to use API:

Current props (passed from parent):
```typescript
interface AudioSetupProps {
  volume: number;
  balance: number;
  shuffle: boolean;
  isPlaying: boolean;
  onVolumeChange: (value: number) => void;
  onBalanceChange: (value: number) => void;
  onShuffleToggle: () => void;
  onPlayPauseToggle: () => void;
  currentInput: string;
  onInputChange: (value: string) => void;
}
```

Refactor to use hooks internally:
```typescript
export function AudioSetup() {
  const { status, setVolume, setBalance, toggleShuffle, play, pause, next, previous } = useAudioStatus();
  const { inputDevices, outputDevices, currentInput, currentOutput, setInput, setOutput } = useDeviceManager();
  
  // Handle slider changes with debouncing
  const debouncedSetVolume = useDebouncedCallback(setVolume, 300);
  
  // Optimistic updates for sliders
  const [localVolume, setLocalVolume] = useState(status?.volume ?? 50);
  
  const handleVolumeChange = (value: number) => {
    setLocalVolume(value); // Immediate UI update
    debouncedSetVolume(value); // Debounced API call
  };
  
  // ...
}
```

Requirements:
- Remove props, use hooks instead
- Implement debouncing for sliders (volume, balance)
- Show loading states on buttons during API calls
- Disable controls during loading
- Show error toasts for failures
- Populate input/output dropdowns from API device lists
- Filter out unavailable devices or show as disabled
```

### Phase 4.3: Now Playing Components Integration

**Coding Agent Prompt:**
```
Update now-playing components in RadioConsole/ConsoleTSWeb/src/components/now-playing/:

SpotifyPlayer.tsx:
```typescript
export function SpotifyPlayer({ isPlaying, onPlayPauseToggle }: Props) {
  const { currentTrack, isLoading, error, like, unlike } = useSpotifyPlayer();
  
  if (isLoading) return <LoadingSpinner />;
  if (error) return <ErrorDisplay error={error} />;
  if (!currentTrack) return <NoTrackPlaying />;
  
  return (
    <div className="...">
      <img src={currentTrack.albumArtUrl} alt="Album art" />
      <h2>{currentTrack.name}</h2>
      <p>{currentTrack.artist}</p>
      <ProgressBar current={currentTrack.currentTime} total={currentTrack.duration} />
      <button onClick={() => currentTrack.liked ? unlike() : like()}>
        {currentTrack.liked ? <HeartFilled /> : <HeartOutline />}
      </button>
    </div>
  );
}
```

RadioPlayer.tsx:
```typescript
export function RadioPlayer({ isPlaying }: Props) {
  const { status, setFrequency, tune, scan, saveStation, stations } = useRadioControl();
  
  // Display current frequency, band, signal strength
  // Implement tuning buttons (up/down)
  // Implement scanning functionality
  // Show saved station presets
  // Allow saving current station
}
```

FilePlayer.tsx:
```typescript
export function FilePlayer({ isPlaying }: Props) {
  const { currentFile, browse, select, isLoading } = useFilePlayer();
  const [currentPath, setCurrentPath] = useState('/music');
  const [items, setItems] = useState<FileItem[]>([]);
  
  useEffect(() => {
    browse(currentPath).then(setItems);
  }, [currentPath]);
  
  // Display file browser
  // Show current playing file info
  // Handle file/folder selection
}
```

VinylPlayer.tsx:
```typescript
export function VinylPlayer({ isPlaying }: Props) {
  const { status, togglePreamp } = useVinylPlayer();
  
  // Display turntable visualization
  // Show preamp toggle switch
}
```

Requirements:
- Fetch real data from API hooks
- Show loading skeletons during data fetch
- Handle error states gracefully
- Implement actual playback controls
- Update progress bars in real-time
- Add visual feedback for user actions
```

### Phase 4.4: System Config & Playlist Components

**Coding Agent Prompt:**
```
Update RadioConsole/ConsoleTSWeb/src/components/SystemConfig.tsx:

```typescript
export function SystemConfig({ onClose }: Props) {
  const { components, getComponent, saveComponent, backup, restore } = useConfiguration();
  const { stats, info } = useSystemStatus();
  const [selectedComponent, setSelectedComponent] = useState<string>('');
  const [configItems, setConfigItems] = useState<ConfigItem[]>([]);
  
  // Fetch component list on mount
  useEffect(() => {
    components.refresh();
  }, []);
  
  // Load config when component selected
  useEffect(() => {
    if (selectedComponent) {
      getComponent(selectedComponent).then(setConfigItems);
    }
  }, [selectedComponent]);
  
  // Handle save
  const handleSave = async () => {
    await saveComponent(selectedComponent, configItems);
    toast.success('Configuration saved');
  };
  
  // Handle backup
  const handleBackup = async () => {
    const path = await backup();
    toast.success(`Backup created: ${path}`);
  };
  
  // Handle restore
  const handleRestore = async (file: File) => {
    await restore(file);
    toast.success('Configuration restored');
  };
  
  return (
    <div>
      <ComponentSelector components={components.data} onSelect={setSelectedComponent} />
      <ConfigEditor items={configItems} onChange={setConfigItems} />
      <button onClick={handleSave}>Save</button>
      <BackupRestore onBackup={handleBackup} onRestore={handleRestore} />
      <SystemInfo stats={stats} info={info} />
    </div>
  );
}
```

Update Playlist.tsx similarly to use usePlaylist() hook.

Requirements:
- Replace mock data with API calls
- Implement proper form validation
- Show success/error toasts for operations
- Add confirmation dialogs for destructive actions
- Implement drag-and-drop for playlist reordering
```

---

## Phase 5: Production Readiness & Polish

**Goal:** Prepare for production deployment with proper error handling, testing, and optimization.

**Estimated Effort:** 2-3 days

### Phase 5.1: Error Handling & Resilience

**Coding Agent Prompt:**
```
Implement comprehensive error handling in ConsoleTSWeb:

1. Create error boundary component (src/components/ErrorBoundary.tsx):
```typescript
export class ErrorBoundary extends React.Component<Props, State> {
  // Catch React render errors
  // Display fallback UI
  // Log errors to console/service
}
```

2. Create API error handler (src/api/errors.ts):
```typescript
export class ApiError extends Error {
  constructor(
    public statusCode: number,
    public message: string,
    public details?: any
  ) {}
}

export function handleApiError(error: unknown): ApiError {
  // Convert fetch errors to ApiError
  // Extract error message from response
  // Handle network errors, timeouts
}
```

3. Add retry logic to API client:
```typescript
// Implement exponential backoff for retries
// Retry on 500, 502, 503, 504 errors
// Don't retry on 400, 401, 403, 404
```

4. Add offline detection:
```typescript
// Detect when API is unreachable
// Show "offline" banner
// Queue mutations for when connection restored
```

Requirements:
- Wrap entire app in ErrorBoundary
- Show user-friendly error messages
- Log errors for debugging
- Implement retry logic with exponential backoff
- Add network status indicator
- Handle timeout scenarios (abort long requests)
```

### Phase 5.2: Loading States & Skeletons

**Coding Agent Prompt:**
```
Improve loading experience in ConsoleTSWeb:

1. Create loading skeleton components (src/components/skeletons/):
- PlayerSkeleton.tsx - Skeleton for now-playing area
- ConfigSkeleton.tsx - Skeleton for config editor
- PlaylistSkeleton.tsx - Skeleton for playlist

2. Add loading states to all async operations:
```typescript
// Show skeletons during initial load
if (isLoading && !data) return <PlayerSkeleton />;

// Show spinner during refresh
if (isLoading && data) return <div>{data}<Spinner /></div>;
```

3. Add progress indicators for long operations:
- File scanning progress
- Station scanning progress
- Backup/restore progress

Requirements:
- Use Suspense for code splitting (lazy load components)
- Show skeletons that match actual content layout
- Add smooth transitions between loading and loaded states
- Indicate progress for long-running operations
```

### Phase 5.3: Build Integration & Deployment

**Coding Agent Prompt:**
```
Integrate ConsoleTSWeb build into RadioConsole.API for unified deployment:

1. Update ConsoleTSWeb/vite.config.ts:
```typescript
export default defineConfig({
  build: {
    outDir: '../RadioConsole.API/wwwroot',
    emptyOutDir: true,
  },
  base: '/', // Serve from root
  server: {
    proxy: {
      '/api': 'http://localhost:5100', // Proxy API calls during dev
    },
  },
});
```

2. Update RadioConsole.API/Program.cs:
```csharp
// Enable static file serving
app.UseStaticFiles();

// Fallback to index.html for client-side routing
app.MapFallbackToFile("index.html");
```

3. Create build script (scripts/build-all.sh):
```bash
#!/bin/bash
set -e

echo "Building TypeScript UI..."
cd RadioConsole/ConsoleTSWeb
npm install
npm run build

echo "Building .NET API..."
cd ../RadioConsole.API
dotnet build -c Release

echo "Build complete!"
```

4. Update .gitignore:
```
# Ignore built UI files in API project
RadioConsole/RadioConsole.API/wwwroot/
```

Requirements:
- Configure CORS for development (allow localhost:5173)
- Disable CORS for production (same origin)
- Set API base URL from environment variable
- Create production build configuration
- Test that UI works when served from API
```

### Phase 5.4: Testing & Documentation

**Coding Agent Prompt:**
```
Add testing and update documentation:

1. Add unit tests for API hooks (src/api/hooks/__tests__/):
```typescript
// Test useAudioStatus hook
describe('useAudioStatus', () => {
  it('fetches initial status', async () => {
    // Mock API response
    // Render hook
    // Assert status is loaded
  });
  
  it('handles volume change', async () => {
    // Mock API endpoint
    // Call setVolume
    // Assert API was called with correct value
  });
});
```

2. Add integration tests for components:
- Test user workflows (select input, adjust volume, play music)
- Use React Testing Library
- Mock API responses

3. Update documentation:
- Create DEPLOYMENT.md with deployment instructions
- Update README.md with build/run instructions
- Document environment variables
- Add troubleshooting guide

Requirements:
- Use vitest for testing (already configured with Vite)
- Achieve reasonable code coverage (>70%)
- Test error scenarios
- Document all configuration options
```

---

## Phase 6: Optional Enhancements

These are nice-to-have features that can be implemented after core functionality is working.

### Phase 6.1: Real-Time Updates via WebSocket

- Implement SignalR hubs on backend
- Replace polling with WebSocket subscriptions
- Add connection status indicator
- Handle reconnection logic

### Phase 6.2: Visualization Features

- Integrate spectrum analyzer (useVisualization hook)
- Add audio level meters to now-playing displays
- Create animated visualizations for music playback

### Phase 6.3: Advanced Configuration

- Add UI for audio priority management
- Configure ducking percentage
- Manage device visibility preferences
- Create EQ editor with custom curves

### Phase 6.4: PWA Features

- Add service worker for offline support
- Enable "Add to Home Screen"
- Cache API responses for offline viewing
- Implement background sync for queued operations

---

## Testing Checklist

After completing each phase, verify:

### Functionality Testing
- [ ] All endpoints return expected data
- [ ] CORS is properly configured
- [ ] Authentication works (if implemented)
- [ ] Error responses are handled gracefully
- [ ] Loading states display correctly
- [ ] Optimistic updates work as expected

### UI/UX Testing
- [ ] All buttons/controls are responsive
- [ ] Touch targets are large enough (44x44px minimum)
- [ ] Loading skeletons match content layout
- [ ] Error messages are user-friendly
- [ ] Success feedback is provided (toasts)
- [ ] Layout works on target display (12.5" x 3.75")

### Performance Testing
- [ ] API responses are fast (<200ms for simple queries)
- [ ] UI remains responsive during API calls
- [ ] No memory leaks (check with DevTools)
- [ ] Smooth animations (60fps)
- [ ] Reasonable bundle size (<500KB gzipped)

### Integration Testing
- [ ] UI works with real hardware (Raspberry Pi 5)
- [ ] Spotify integration works with real account
- [ ] Radio control works with USB device
- [ ] File player can browse actual music library
- [ ] System stats reflect actual hardware

### Cross-Browser Testing
- [ ] Works in Chromium (primary target)
- [ ] Works in Firefox
- [ ] Works in Safari (if needed)

---

## Rollback Plan

If issues arise during migration:

1. **Keep existing Blazor UI** - Don't delete RadioConsole.Web until TypeScript UI is proven
2. **Feature flags** - Use config to enable/disable new endpoints
3. **Versioned endpoints** - Consider `/api/v2/` for new endpoints if breaking changes
4. **Gradual migration** - Deploy one feature at a time, test thoroughly
5. **Monitoring** - Add logging/metrics to track API usage and errors

---

## Support & Resources

- **API Documentation**: http://localhost:5100/swagger
- **React Documentation**: https://react.dev
- **Radix UI**: https://www.radix-ui.com
- **Tailwind CSS**: https://tailwindcss.com
- **Vite**: https://vitejs.dev

For questions or issues, refer to the reconciliation document for endpoint mappings and the TypeScript app overview for UI architecture details.
