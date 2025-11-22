# TypeScript Web Application Overview

## Executive Summary

The ConsoleTSWeb application is a modern, touch-optimized React/TypeScript web interface designed for an ultra-wide embedded audio controller display (12.5" x 3.75"). It provides a comprehensive UI for controlling various audio sources (Spotify, USB Radio, Vinyl, File Player, Bluetooth, AUX, Google Cast), managing system configuration, and displaying now-playing information.

## Technology Stack

### Core Framework
- **React 18.3.1** - Modern React with hooks and concurrent features
- **TypeScript** - Type-safe JavaScript development
- **Vite 6.3.5** - Fast build tool and dev server
- **Tailwind CSS** - Utility-first CSS framework for styling

### UI Components
The application uses a comprehensive set of **Radix UI** primitives for accessible, headless components:
- Accordion, Alert Dialog, Aspect Ratio, Avatar
- Checkbox, Collapsible, Context Menu, Dialog
- Dropdown Menu, Hover Card, Label, Menubar
- Navigation Menu, Popover, Progress, Radio Group
- Scroll Area, Select, Separator, Slider
- Switch, Tabs, Toggle, Toggle Group, Tooltip

### Additional Libraries
- **lucide-react** (v0.487.0) - Icon library
- **next-themes** (v0.4.6) - Theme management
- **embla-carousel-react** (v8.6.0) - Carousel component
- **recharts** (v2.15.2) - Charting library
- **react-hook-form** (v7.55.0) - Form state management
- **react-resizable-panels** (v2.1.7) - Resizable layout panels
- **sonner** (v2.0.3) - Toast notifications
- **vaul** (v1.1.2) - Drawer component
- **input-otp** (v1.4.2) - OTP input component

## Application Architecture

### Component Structure

```
src/
├── App.tsx                          # Root application component
├── components/
│   ├── MainBar.tsx                 # Top navigation bar
│   ├── AudioSetup.tsx              # Audio controls (volume, balance, playback)
│   ├── NowPlaying.tsx              # Main content area for current input
│   ├── Playlist.tsx                # Playlist sidebar
│   ├── SystemConfig.tsx            # System configuration interface
│   ├── now-playing/                # Input-specific player components
│   │   ├── SpotifyPlayer.tsx
│   │   ├── RadioPlayer.tsx
│   │   ├── VinylPlayer.tsx
│   │   ├── FilePlayer.tsx
│   │   └── DefaultPlayer.tsx
│   ├── dialogs/                    # Modal dialogs
│   │   └── CastDeviceDialog.tsx
│   └── ui/                         # Reusable UI primitives (40+ components)
│       ├── button.tsx
│       ├── slider.tsx
│       ├── select.tsx
│       └── ... (all Radix UI wrappers)
├── styles/                         # Style definitions
└── guidelines/                     # Design guidelines and attributions
```

### Core Views

#### 1. Main View (`currentView === 'main'`)
The primary interface with two sections:

**Left Section (flex-1):**
- **AudioSetup Component**: Top section containing:
  - Playback controls (shuffle, previous, play/pause, next)
  - Volume slider with icon
  - Balance slider with icons
  - Input device selector (dropdown)
  - Output device selector (dropdown)
  - Cast device button (for Google Cast)

- **NowPlaying Component**: Bottom section (flex-1) displaying:
  - Input-specific player interface based on `currentInput`:
    - `spotify` → SpotifyPlayer (album art, track info, like button)
    - `usb-radio` → RadioPlayer (frequency, band, station controls)
    - `vinyl` → VinylPlayer (turntable visualization, preamp toggle)
    - `file-player` → FilePlayer (file browser, playback info)
    - Others → DefaultPlayer (generic input display)

**Right Section (conditional, w-80):**
- **Playlist Component**: Shown when `showPlaylist === true`
  - Current playlist items
  - Track management
  - Reorder capabilities

#### 2. System Config View (`currentView === 'system-config'`)
Full-width system configuration interface including:
- Component-based configuration editor
- Configuration backup/restore
- System information
- Device visibility settings

### State Management

The application uses React's `useState` hook for local state management in the root `App.tsx`:

```typescript
const [currentView, setCurrentView] = useState<ViewType>('main');
const [currentInput, setCurrentInput] = useState<InputDevice>('spotify');
const [volume, setVolume] = useState(50);
const [balance, setBalance] = useState(0);
const [shuffle, setShuffle] = useState(false);
const [isPlaying, setIsPlaying] = useState(false);
const [showPlaylist, setShowPlaylist] = useState(false);
```

**Note**: Currently, all state is local and ephemeral. The migration will require:
1. API service layer for backend communication
2. State synchronization with RadioConsole.API
3. WebSocket integration for real-time updates (optional but recommended)

## Supported Input Devices

The application supports the following input devices (defined in `AudioSetup.tsx`):

1. **Spotify** - Streaming music service integration
2. **USB Radio** - Raddy RF320 radio via USB
3. **Vinyl Phonograph** - Turntable input via USB ADC
4. **File Player** - Local MP3/FLAC file playback
5. **Bluetooth** - Bluetooth audio input
6. **AUX Input** - Auxiliary line input
7. **Google Cast** - Chromecast/Cast-enabled devices

## Supported Output Devices

Output destinations (defined in `AudioSetup.tsx`):

1. **Built-in Speakers** - Main audio output
2. **Headphones** - Headphone jack output
3. **Bluetooth Output** - Bluetooth speakers/headphones
4. **Line Out** - Line-level output

## UI Design Characteristics

### Ultra-Wide Layout Optimization
- **Target Display**: 12.5" x 3.75" (ultra-wide aspect ratio ~3.33:1)
- **Layout**: Horizontal split with fixed navigation bar at top
- **Color Scheme**: Dark theme (gray-900 background, gray-800 cards)
- **Typography**: White text on dark backgrounds for OLED-friendly display

### Touch Optimization
- Large touch targets with `touch-manipulation` class
- Generous padding on interactive elements
- Clear visual feedback on hover/active states
- Minimal text input (prefer sliders, dropdowns, buttons)

### Responsive Behavior
- Flexbox-based layout adapts to container sizes
- Playlist panel can be toggled on/off to save space
- Scrollable areas for content overflow
- Resizable panels support (via react-resizable-panels)

## Key UI Interactions

### Audio Control Flow
1. User selects input device via dropdown
2. `NowPlaying` component switches to appropriate player
3. Volume/balance adjustments affect currently playing audio
4. Playback controls (play/pause/skip) route to active input

### Navigation Flow
1. **Home Button**: Return to main view
2. **System Config Button**: Open system configuration
3. **Playlist Toggle**: Show/hide playlist sidebar
4. **Close Button** (in config): Return to main view

### Configuration Management
1. User navigates to System Config view
2. Select component to configure
3. Edit configuration items
4. Save changes (would POST to API)
5. Backup/Restore functionality for disaster recovery

## Current Limitations (Require API Integration)

### Data Flow Issues
- **No API Integration**: All state is local, no persistence
- **No Real-Time Updates**: No WebSocket or polling for live data
- **Mock Data**: Player components would need real data from API

### Missing Functionality
- Actual audio playback control (requires API calls)
- Real-time track info from Spotify API
- Radio frequency control and RDS data
- File system browsing for File Player
- Configuration persistence
- System stats monitoring

## Files Requiring API Integration

### Critical Files (Need Extensive Changes)
1. **App.tsx** - Add API service injection, state sync
2. **AudioSetup.tsx** - Wire up volume, balance, device selection APIs
3. **now-playing/SpotifyPlayer.tsx** - Integrate Spotify API calls
4. **now-playing/RadioPlayer.tsx** - Integrate radio control APIs
5. **now-playing/VinylPlayer.tsx** - Integrate vinyl/preamp APIs
6. **now-playing/FilePlayer.tsx** - Integrate file browsing APIs
7. **Playlist.tsx** - Integrate playlist management APIs
8. **SystemConfig.tsx** - Integrate configuration CRUD APIs

### Supporting Files (May Need Changes)
- **MainBar.tsx** - May need system status display
- **dialogs/CastDeviceDialog.tsx** - Cast device discovery API

## Recommended Migration Approach

### Phase 1: API Service Layer
Create TypeScript services/hooks for API communication:
- `useAudioService` - Volume, balance, playback control
- `useDeviceService` - Input/output device management
- `useRadioService` - Radio-specific controls
- `useSpotifyService` - Spotify integration
- `useConfigService` - Configuration management
- `useSystemStatus` - System monitoring

### Phase 2: State Integration
Replace local state with API-backed state:
- Initial load from API on mount
- Optimistic updates with API sync
- Error handling and retry logic
- Loading states for async operations

### Phase 3: Real-Time Features
Add WebSocket or polling for live updates:
- Current playback position
- Radio signal strength
- System stats (CPU, RAM)
- Configuration changes from other clients

### Phase 4: Error Handling & Polish
- Network error recovery
- Offline mode support
- Loading skeletons
- Toast notifications for operations
- Confirmation dialogs for destructive actions

## Build & Development

### Development Server
```bash
npm install
npm run dev
```
Starts Vite dev server (typically on port 5173)

### Production Build
```bash
npm run build
```
Outputs to `dist/` directory, ready for static hosting or embedding in the .NET API

## Integration Points with RadioConsole.API

The TypeScript app will need to:
1. **Configure API Base URL**: Via environment variable or config file
2. **CORS Configuration**: API must allow requests from UI origin
3. **Authentication**: Handle any auth tokens/cookies if required
4. **WebSocket Connection**: For real-time updates (if implemented)
5. **Static Hosting**: API could serve the built UI at root path

## Next Steps

See `MISSING_ENDPOINTS.md` for the complete list of API endpoints required by this UI, and the reconciliation with what's currently available in RadioConsole.API.
