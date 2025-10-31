# Architecture Documentation

## Overview

The Radio Console application follows a clean, modular architecture that separates concerns and allows for easy extension and testing. The architecture is designed with the following principles:

1. **Interface-Driven Design** - All major components are defined by interfaces
2. **Separation of Concerns** - Clear boundaries between UI, business logic, and hardware
3. **Simulation Support** - Development can happen on any platform
4. **MVVM Pattern** - Clean separation between UI and logic
5. **Extensibility** - Easy to add new inputs and outputs

## Core Architecture Layers

### 1. Interface Layer (`Interfaces/`)

The foundation of the system, defining contracts for all major components:

#### IAudioInput
- Represents any audio input source (radio, streaming, etc.)
- Methods: `InitializeAsync()`, `StartAsync()`, `StopAsync()`, `GetAudioStreamAsync()`
- Properties: `Id`, `Name`, `Description`, `IsAvailable`, `IsActive`
- Provides: `IConfiguration` and `IDisplay` interfaces

#### IAudioOutput
- Represents any audio output device (speakers, streaming, etc.)
- Methods: `InitializeAsync()`, `StartAsync()`, `StopAsync()`, `SendAudioAsync()`, `SetVolumeAsync()`
- Properties: Same as IAudioInput
- Provides: `IConfiguration` and `IDisplay` interfaces

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

### 2. Service Layer (`Services/`)

Core services that support the application:

#### EnvironmentService
- Detects runtime environment (Raspberry Pi vs. other platforms)
- Enables automatic simulation mode
- Platform-agnostic development

#### JsonStorageService
- JSON-based persistent storage
- Stores data in app data directory
- Used for configuration, history, favorites

### 3. Module Layer (`Modules/`)

Concrete implementations of inputs and outputs:

#### Base Classes
- `BaseAudioInput` - Common functionality for all inputs
- `BaseAudioOutput` - Common functionality for all outputs
- Includes embedded `BaseDisplay` and `BaseConfiguration` implementations

#### Input Modules
- **RadioInput** - Raddy RF320 radio integration
  - Supports FM/AM/SW bands
  - Hardware detection with simulation fallback
  
- **SpotifyInput** - Spotify streaming
  - API integration ready
  - Playback control support

#### Output Modules
- **WiredSoundbarOutput** - Direct wired audio
  - Hardware audio output
  - Volume control
  
- **ChromecastOutput** - Network streaming
  - Chromecast device discovery
  - Remote audio casting

### 4. Presentation Layer

#### ViewModels (`ViewModels/`)
Following MVVM pattern with CommunityToolkit.Mvvm:

- **AudioControlViewModel**
  - Manages audio sources and outputs
  - Controls playback state
  - Handles volume control
  - Uses `ObservableObject` and `RelayCommand`

- **HistoryViewModel**
  - Displays playback history
  - Loads from storage

- **FavoritesViewModel**
  - Manages favorite stations/playlists
  - Quick access to saved items

#### Views (`Views/`)
XAML-based Material Design 3 UI:

- **AudioControlPage** - Main control interface
  - Input/output selection
  - Volume slider
  - Play/stop controls
  - Status display

- **HistoryPage** - Playback history
- **FavoritesPage** - Saved favorites

#### Shell (`AppShell.xaml`)
- Navigation framework
- Flyout menu
- Route registration

### 5. Data Model Layer (`Models/`)

Domain models for data structures:

- **AudioMetadata** - Track/station information
- **HistoryEntry** - Historical playback record
- **FavoriteEntry** - Saved favorite item
- **PlaybackState** - Current playback state

## Data Flow

### Playback Flow
1. User selects input and output in AudioControlPage
2. AudioControlViewModel receives selection
3. ViewModel calls `StartAsync()` on output, then input
4. Input generates audio stream via `GetAudioStreamAsync()`
5. Stream is sent to output via `SendAudioAsync()`
6. Display updates show current status and metadata

### Configuration Flow
1. Each module has embedded IConfiguration implementation
2. Configuration saved to storage via IStorage interface
3. Configuration loaded on module initialization
4. Changes persisted automatically

### Simulation Mode
1. EnvironmentService detects platform at startup
2. If not Raspberry Pi, sets IsSimulationMode = true
3. All modules check simulation mode flag
4. Simulated modules provide mock data and behavior
5. Allows full UI testing without hardware

## Extensibility

### Adding a New Input Source

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

Then register in `AudioControlViewModel.InitializeAsync()`.

### Adding a New Output Device

Same pattern as input - inherit from `BaseAudioOutput` and implement abstract methods.

### Adding New UI Pages

1. Create XAML view in `Views/`
2. Create ViewModel in `ViewModels/`
3. Register route in `AppShell.xaml.cs`
4. Add navigation item in `AppShell.xaml`

## Design Patterns Used

1. **Interface Segregation** - Small, focused interfaces
2. **Dependency Injection** - Services registered in MauiProgram
3. **MVVM** - Separation of UI and logic
4. **Template Method** - Base classes with common functionality
5. **Factory Pattern** - Module instantiation in ViewModel
6. **Observer Pattern** - IDisplay.DisplayChanged event
7. **Strategy Pattern** - Different implementations per module

## Platform Support

### Current Target
- Primary: Raspberry Pi 5 (Linux ARM64)
- Development: Any platform with .NET 9.0

### Platform-Specific Code
Located in `Platforms/` directory:
- `Android/` - Android-specific code
- `iOS/` - iOS-specific code (future)
- `MacCatalyst/` - macOS-specific code (future)
- `Windows/` - Windows-specific code (future)

## Dependencies

### Core Dependencies
- Microsoft.Maui.Controls (9.0.0)
- CommunityToolkit.Maui (9.0.0)
- CommunityToolkit.Mvvm (8.3.2)

### Future Dependencies (for full functionality)
- Hardware I/O libraries for Raspberry Pi
- Spotify SDK
- Chromecast SDK
- Audio processing libraries

## Testing Strategy

### Unit Testing
- Test individual modules in isolation
- Mock IEnvironmentService for simulation
- Test configuration and storage services

### Integration Testing
- Test module interactions
- Test ViewModel logic
- Test storage persistence

### Manual Testing
- Use simulation mode for UI testing
- Test on actual Raspberry Pi hardware
- Verify audio quality and performance

## Security Considerations

1. **Configuration Storage** - Sensitive data should be encrypted
2. **API Keys** - Store securely, not in configuration files
3. **Network Communication** - Use secure protocols
4. **Input Validation** - Validate all user inputs
5. **Hardware Access** - Proper permission handling

## Performance Considerations

1. **Audio Streaming** - Low-latency buffering
2. **UI Responsiveness** - Async operations
3. **Memory Management** - Proper disposal of streams
4. **Storage Operations** - Async I/O
5. **Module Initialization** - Lazy loading where appropriate

## Future Enhancements

1. **Plugin System** - Dynamic module loading
2. **Remote Control** - Mobile app or web interface
3. **Multi-room Audio** - Synchronize multiple devices
4. **Audio Processing** - Equalizer, effects
5. **Voice Control** - Integration with voice assistants
6. **Cloud Sync** - Sync favorites and settings across devices
