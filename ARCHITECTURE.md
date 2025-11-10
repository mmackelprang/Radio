# Architecture Documentation

## Overview

The Radio Console application follows a clean, modular architecture with a clear separation between backend and frontend. The architecture is designed with the following principles:

1. **API-First Design** - RESTful API with clear endpoint contracts
2. **Separation of Concerns** - Backend API, frontend UI, and hardware abstraction
3. **Simulation Support** - Development can happen on any platform
4. **Modern Web Stack** - ASP.NET Core backend with React/TypeScript frontend
5. **Extensibility** - Easy to add new inputs and outputs
6. **Real-time Updates** - WebSocket/SignalR for live metadata and status

## Core Architecture Layers

### 1. Backend API Layer (ASP.NET Core)

#### Interface Layer (`Interfaces/`)

The foundation of the system, defining contracts for all major components:

#### IAudioInput
- Represents any audio input source (radio, streaming, events, etc.)
- Methods: `InitializeAsync()`, `StartAsync()`, `StopAsync()`, `GetAudioStreamAsync()`, `PauseAsync()`, `ResumeAsync()`, `SetVolumeAsync()`
- Properties: `Id`, `Name`, `Description`, `IsAvailable`, `IsActive`, `InputType`, `Priority`, `Duration`, `AllowConcurrent`
- InputType: Distinguishes between `Music` and `Event` inputs
- Event: `AudioDataAvailable` - fires when PCM audio data is available
- Methods: `SetRepeat()`, `SimulateTriggerAsync()` for event simulation
- Provides: `IDeviceConfiguration` and `IDisplay` interfaces

Note: The `IEventAudioInput` interface has been removed. All event functionality is now integrated into `IAudioInput` with the `InputType` property distinguishing between Music and Event inputs.

#### IAudioOutput
- Represents any audio output device (speakers, streaming, etc.)
- Methods: `InitializeAsync()`, `StartAsync()`, `StopAsync()`, `SendAudioAsync()`, `SetVolumeAsync()`
- Properties: Same as IAudioInput
- Provides: `IDeviceConfiguration` and `IDisplay` interfaces

#### IDisplay
- Provides metadata and status information for UI display
- Methods: `GetMetadata()`, `GetStatusMessage()`
- Events: `DisplayChanged` event for real-time updates

#### IConfiguration
- Manages device-specific configuration
- Methods: `GetValue<T>()`, `SetValue<T>()`, `SaveAsync()`, `LoadAsync()`
- Allows flexible configuration per device

#### IStorage
- Abstracts data persistence
- Methods: `SaveAsync<T>()`, `LoadAsync<T>()`, `DeleteAsync()`, `ExistsAsync()`
- Currently implemented with JSON file storage

### 2. API Controllers (`Controllers/`)

RESTful API endpoints for client interaction:

#### AudioController
- `GET /api/audio/inputs` - List available audio inputs
- `GET /api/audio/outputs` - List available audio outputs
- `POST /api/audio/start` - Start playback with selected input/output
- `POST /api/audio/stop` - Stop current playback
- `PUT /api/audio/volume` - Adjust volume level
- `GET /api/audio/status` - Get current playback status

#### HistoryController
- `GET /api/history` - Retrieve playback history
- `DELETE /api/history/{id}` - Remove history entry

#### FavoritesController
- `GET /api/favorites` - Retrieve saved favorites
- `POST /api/favorites` - Add new favorite
- `DELETE /api/favorites/{id}` - Remove favorite

#### ConfigurationController
- `GET /api/config/{moduleId}` - Get module configuration
- `PUT /api/config/{moduleId}` - Update module configuration

### 3. Real-time Communication (`Hubs/`)

SignalR hubs for push updates:

#### AudioHub
- Broadcasts metadata changes
- Sends status updates
- Notifies clients of audio events

### 4. Service Layer (`Services/`)

Core services that support the application:

#### EnvironmentService
- Detects runtime environment (Raspberry Pi vs. other platforms)
- Enables automatic simulation mode
- Platform-agnostic development

#### AudioPriorityManager
- Manages multiple audio sources with priority-based playback
- Monitors registered event audio inputs
- Handles high-priority audio event interruption:
  - Saves current volume states
  - Reduces/mutes background audio during events
  - Plays event audio
  - Restores original volumes
- Uses async/await patterns for all operations
- Provides comprehensive exception handling
- Configurable volume reduction level (0.0 = mute, 1.0 = no change)
- Designed for Raspberry Pi 5 with PulseAudio

#### JsonStorageService
- JSON-based persistent storage
- Stores data in app data directory
- Used for configuration, history, favorites

### 5. Module Layer (`Modules/`)

Concrete implementations of inputs and outputs:

#### Base Classes
- `BaseAudioInput` - Common functionality for all audio inputs (music and events)
  - Includes event management via `TriggerAudioEventAsync()` and `SimulateTriggerAsync()`
  - Playback control (pause, resume, volume, repeat)
  - Embedded `BaseDisplay` and `BaseConfiguration` implementations
- `BaseAudioOutput` - Common functionality for all outputs
  - Includes embedded `BaseDisplay` and `BaseConfiguration` implementations

Note: The `BaseEventAudioInput` class has been removed. All functionality is now in `BaseAudioInput`.

#### Music Input Modules
- **RadioInput** - Raddy RF320 radio integration
  - Supports FM/AM/SW bands
  - Hardware detection with simulation fallback
  
- **SpotifyInput** - Spotify streaming
  - API integration ready
  - Playback control support

#### Event Input Modules
- **DoorbellEventInput** - Doorbell ring notifications
  - High priority (interrupts music)
  - 3 second duration
  - Integration ready for Wyze doorbell

- **TelephoneRingingEventInput** - Phone ring notifications
  - High priority
  - 5 second duration

- **GoogleBroadcastEventInput** - Google Home broadcasts
  - Medium priority
  - Variable duration

- **TimerExpiredEventInput** - Timer notifications
  - Medium priority
  - 3 second duration

- **ReminderEventInput** - Calendar/scheduled reminders
  - Medium priority
  - 2 second duration

#### Output Modules
- **WiredSoundbarOutput** - Direct wired audio
  - Hardware audio output
  - Volume control
  
- **ChromecastOutput** - Network streaming
  - Chromecast device discovery
  - Remote audio casting

## Frontend Architecture (React/TypeScript)

### 1. Component Structure (`src/components/`)

React components organized by feature:

#### Layout Components
- **AppLayout** - Main application shell with navigation
- **ThemeToggle** - Dark/light mode switcher
- **Navigation** - Side drawer or app bar navigation

#### Feature Components
- **AudioControl** - Main audio control interface
  - Input selector dropdown
  - Output selector dropdown
  - Volume slider
  - Play/stop buttons
  - Status display
  
- **History** - Playback history list
  - History items with timestamp
  - Clear history functionality
  
- **Favorites** - Saved favorites list
  - Add to favorites button
  - Quick access to favorite items

### 2. State Management (`src/contexts/`)

React Context API for global state:

#### AudioContext
- Current input/output selection
- Playback state
- Volume level
- Available inputs and outputs

#### ThemeContext
- Current theme mode (dark/light)
- Theme toggle function

### 3. API Services (`src/services/`)

TypeScript clients for backend communication:

#### ApiService
- Centralized HTTP client configuration
- Error handling and retry logic
- Token management (if authentication added)

#### AudioService
- Wraps audio-related API calls
- Manages WebSocket connection for real-time updates

### 4. Theming (`src/theme/`)

Material-UI theme configuration:

#### theme.ts
- Material Design 3 color palette
- Dark mode theme definition
- Light mode theme definition
- Typography settings
- Component style overrides

### 5. Custom Hooks (`src/hooks/`)

Reusable React hooks:

- **useAudio** - Audio control and status
- **useTheme** - Theme mode management
- **useSignalR** - WebSocket connection management

## Data Flow

### API Request Flow
1. User interacts with React component
2. Component calls API service function
3. Service makes HTTP request to ASP.NET Core API
4. Controller receives request and calls appropriate service
5. Service interacts with modules (inputs/outputs)
6. Response returned through controller to frontend
7. Component updates UI based on response

### Real-time Update Flow
1. Backend module triggers event (e.g., metadata change)
2. Event published to SignalR hub
3. Hub broadcasts to connected clients
4. Frontend SignalR client receives update
5. React state updated via context
6. UI automatically re-renders with new data

### Playback Flow
1. User selects input and output in AudioControl component
2. Component calls AudioService.startPlayback()
3. API receives POST request to /api/audio/start
4. AudioController calls service to start playback
5. Service calls StartAsync() on output, then input
6. Input generates audio stream via GetAudioStreamAsync()
7. Stream is sent to output via SendAudioAsync()
8. Status updated and broadcast via SignalR
9. Frontend receives update and displays current status

### Configuration Flow
1. Each module has embedded IConfiguration implementation
2. Configuration exposed via API endpoints
3. Frontend requests configuration via GET /api/config/{moduleId}
4. User modifies settings in UI
5. Changes sent via PUT /api/config/{moduleId}
6. Configuration saved to storage via IStorage interface
7. Configuration loaded on module initialization

### Simulation Mode
1. EnvironmentService detects platform at startup
2. If not Raspberry Pi, sets IsSimulationMode = true
3. All modules check simulation mode flag
4. Simulated modules provide mock data and behavior
5. Allows full UI testing without hardware

## Extensibility

### Adding a New Input Source

Backend (C#):
```csharp
public class NewInput : BaseAudioInput
{
    public override string Id => "new_input";
    public override string Name => "New Input";
    public override string Description => "New Input Description";

    public NewInput(IEnvironmentService env, IStorage storage) 
        : base(env, storage) { }

    public override async Task InitializeAsync()
    {
        // Initialization logic
        IsAvailable = true; // or check hardware
    }

    // Implement other abstract methods
}
```

Then register in the API's dependency injection container.

Frontend (TypeScript):
- No changes needed - new input will appear in the inputs list automatically

### Adding a New Output Device

Same pattern as input - inherit from `BaseAudioOutput` and register in DI container.

### Adding New UI Pages

1. Create new React component in `src/components/`
2. Add route in `App.tsx`
3. Add navigation item to drawer/menu
4. Create any necessary API endpoints in backend

## Design Patterns Used

1. **RESTful API** - Standard HTTP methods and status codes
2. **Repository Pattern** - Storage abstraction
3. **Dependency Injection** - Services registered in ASP.NET Core DI
4. **Component-Based Architecture** - React components
5. **Template Method** - Base classes with common functionality
6. **Factory Pattern** - Module instantiation in DI container
7. **Observer Pattern** - SignalR real-time updates
8. **Strategy Pattern** - Different implementations per module
9. **Context API** - React state management

## Platform Support

### Current Target
- Primary: Raspberry Pi 5 (Linux ARM64) with Chromium browser
- Development: Any platform with .NET 9.0 and Node.js

### Browser Requirements
- Modern browsers with WebSocket support
- Chromium/Chrome (recommended for Raspberry Pi)
- Firefox, Safari, Edge also supported

## Dependencies

### Backend Dependencies (ASP.NET Core)
- Microsoft.AspNetCore.App (9.0.0)
- Microsoft.AspNetCore.SignalR (9.0.0)
- System.Text.Json (built-in)

### Frontend Dependencies (React)
- React 18.x
- React Router 6.x
- Material-UI (MUI) v5
- TypeScript 5.x
- Axios or Fetch API
- @microsoft/signalr (for WebSocket)

### Future Dependencies (for full functionality)
- Hardware I/O libraries for Raspberry Pi
- Spotify SDK integration
- Chromecast SDK integration
- Audio processing libraries

## Testing Strategy

### Backend Testing
- Unit test controllers and services
- Mock IEnvironmentService for simulation
- Test API endpoints with integration tests
- Test SignalR hub functionality

### Frontend Testing
- Component testing with React Testing Library
- Integration testing with Mock Service Worker
- E2E testing with Playwright or Cypress
- Visual regression testing

### Manual Testing
- Use simulation mode for development
- Test on actual Raspberry Pi hardware
- Verify audio quality and performance
- Test dark/light mode switching
- Verify responsive design on touchscreen

## Security Considerations

1. **API Security** - Consider adding authentication/authorization for production
2. **CORS Configuration** - Properly configure allowed origins
3. **Configuration Storage** - Sensitive data should be encrypted
4. **API Keys** - Store securely in environment variables or Azure Key Vault
5. **Input Validation** - Validate all user inputs on both frontend and backend
6. **HTTPS** - Use HTTPS in production
7. **WebSocket Security** - Secure SignalR connections

## Performance Considerations

1. **Audio Streaming** - Low-latency buffering
2. **API Responsiveness** - Async operations throughout
3. **WebSocket Efficiency** - Throttle updates to avoid overwhelming clients
4. **Frontend Performance** - React optimization (memoization, lazy loading)
5. **Memory Management** - Proper disposal of streams
6. **Storage Operations** - Async I/O
7. **Module Initialization** - Lazy loading where appropriate
8. **Bundle Size** - Code splitting in frontend

## Future Enhancements

1. **Plugin System** - Dynamic module loading via API
2. **Mobile Apps** - Native iOS/Android apps consuming the same API
3. **Progressive Web App (PWA)** - Installable web app
4. **Multi-room Audio** - Synchronize multiple devices
5. **Audio Processing** - Equalizer, effects via Web Audio API
6. **Voice Control** - Integration with voice assistants
7. **Cloud Sync** - Sync favorites and settings across devices
8. **Authentication** - User accounts and personalization
