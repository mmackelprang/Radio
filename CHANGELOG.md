# Changelog

All notable changes to the Radio Console project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned
- Hardware integration for Raddy RF320 radio
- Spotify API integration
- Chromecast SDK integration
- Vinyl turntable input support
- MP3 network playback
- Bluetooth input/output support
- Wyze doorbell integration
- Google broadcast integration
- Real-time metadata display
- Enhanced configuration UI per device
- History tracking implementation
- Favorites management system
- Unit and integration tests

## [0.1.0] - 2025-10-31

### Added - Initial Release

#### Core Architecture
- **Interfaces**: IAudioInput, IAudioOutput, IDisplay, IConfiguration, IStorage
- **Base Implementations**: BaseAudioInput, BaseAudioOutput with embedded configuration and display
- **Modular Design**: Easy extensibility for new inputs and outputs

#### Services
- **EnvironmentService**: Automatic platform detection and simulation mode
- **JsonStorageService**: JSON-based persistent storage for configuration and data

#### Input Modules
- **RadioInput**: Raddy RF320 integration (simulation mode ready)
  - FM/AM/SW band support structure
  - Hardware detection with fallback
  
- **SpotifyInput**: Spotify streaming (simulation mode ready)
  - Playback control structure
  - Metadata support

#### Output Modules
- **WiredSoundbarOutput**: Direct audio output (simulation mode ready)
  - Volume control
  - Connection detection
  
- **ChromecastOutput**: Network streaming (simulation mode ready)
  - Device discovery structure
  - Remote casting support

#### User Interface
- **Material Design 3**: Modern, touch-friendly design
- **Navigation Shell**: Flyout menu with multiple pages
- **AudioControlPage**: 
  - Input/output selection
  - Volume slider
  - Play/stop controls
  - Status display
- **HistoryPage**: Playback history view (placeholder)
- **FavoritesPage**: Saved favorites view (placeholder)

#### ViewModels (MVVM)
- **AudioControlViewModel**: Main control logic with CommunityToolkit.Mvvm
- **HistoryViewModel**: History management
- **FavoritesViewModel**: Favorites management

#### Data Models
- **AudioMetadata**: Track/station metadata structure
- **HistoryEntry**: Historical playback records
- **FavoriteEntry**: Saved favorites
- **PlaybackState**: Current playback state

#### Resources
- **Material Design 3 Colors**: Complete color palette
- **Styles**: Consistent UI styling
- **Icons**: App icon and splash screen
- **Value Converters**: IsNotNull and InvertedBool converters

#### Documentation
- **README.md**: Comprehensive project overview, goals, and status
- **PROJECT_PLAN.md**: Detailed development phases
- **ARCHITECTURE.md**: Complete architecture documentation
- **DEVELOPMENT.md**: Development guide and workflow
- **BUILD.md**: Build instructions
- **.gitignore**: Comprehensive .NET MAUI gitignore

#### Platform Support
- **Android**: MainActivity and MainApplication
- **Cross-platform**: .NET 9.0 MAUI framework
- **Raspberry Pi 5**: Target platform (Linux ARM64)
- **Simulation Mode**: Development on any platform

#### Dependencies
- Microsoft.Maui.Controls 9.0.0
- CommunityToolkit.Maui 9.0.0
- CommunityToolkit.Mvvm 8.3.2
- Microsoft.Extensions.Logging.Debug 9.0.0

### Technical Highlights
- Clean separation of concerns with interface-driven design
- MVVM pattern for UI/logic separation
- Template method pattern in base classes
- Observer pattern for display updates
- Dependency injection ready
- Async/await throughout for responsiveness
- Platform-agnostic development with simulation mode

### Known Limitations
- Requires MAUI workload installation to build
- Actual hardware integration not yet implemented
- Font files must be downloaded separately
- No unit tests yet (planned for Phase 8)

---

## Version History Notes

### Version Numbering
- **Major**: Significant architectural changes or breaking changes
- **Minor**: New features, modules, or significant enhancements
- **Patch**: Bug fixes, documentation updates, minor improvements

### Phase Completion Milestones
- v0.1.0: Phase 1-2 Complete (Requirements, Architecture, Basic Setup)
- v0.2.0: Phase 3 Complete (Core Audio I/O with hardware integration)
- v0.3.0: Phase 4 Complete (Advanced inputs)
- v0.4.0: Phase 5 Complete (Output devices)
- v0.5.0: Phase 6 Complete (Enhanced UI)
- v0.6.0: Phase 7 Complete (State management)
- v0.8.0: Phase 8 Complete (Testing and deployment)
- v1.0.0: Phase 9 Complete (Documentation and polish)
