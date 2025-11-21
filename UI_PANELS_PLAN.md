# Radio Console UI Panels Architecture & Development Plan

## Document Purpose
This document provides a comprehensive overview of the Radio Console UI panel architecture, current implementation status, and a development plan for future panel creation. It serves as a guide for adding new panels and managing the UI layout following Material Design 3 principles.

---

## 1. Existing UI Panels

### 1.1 Main Layout Panels (Always Visible)

#### GlobalHeader Panel
- **Name:** GlobalHeader
- **Overall Purpose:** Displays system date/time, network status, and system health indicators. Acts as the navigation bar.
- **Components on Panel:**
  - Date display (formatted as "dddd, MMMM dd, yyyy")
  - Time display (formatted as "hh:mm:ss tt")
  - WiFi status icon (green=connected, red=disconnected)
  - System health icon (green=healthy, yellow=warning)
- **Containing File:** `RadioConsole.Web/Components/Shared/GlobalHeader.razor`
- **Location in UI:** Top of screen, full width
- **Status:** ✅ Implemented

#### AudioSetupPanel
- **Name:** AudioSetupPanel (Left Panel)
- **Overall Purpose:** Configure audio devices, input sources, and output destinations. Control transport and audio processing.
- **Components on Panel:**
  - Input Device selector dropdown
  - Output Device selector dropdown
  - Audio Source selector (Radio/Vinyl/Spotify)
  - Transport controls (Play/Pause/Stop)
  - Volume slider
  - Balance slider
  - Equalizer sliders (Bass, Mid, Treble)
  - Google Cast device selector (when Cast output selected)
- **Containing File:** `RadioConsole.Web/Components/Shared/AudioSetupPanel.razor`
- **Location in UI:** Left panel in main layout (1/3 width)
- **Status:** ✅ Implemented

#### NowPlayingPanel
- **Name:** NowPlayingPanel (Center Panel)
- **Overall Purpose:** Display current playback context based on selected input source. Dynamically shows different content for Radio/Spotify/Vinyl.
- **Components on Panel:**
  - **Radio Mode:**
    - Large LED-style frequency display
    - Band indicator (FM/AM)
    - Stereo indicator
    - Signal strength meter
  - **Spotify Mode:**
    - Album art image (250x250px)
    - Track title
    - Artist name
    - Album name
    - Lyrics display (scrollable)
  - **Vinyl Mode:**
    - Vinyl record icon (200px)
    - "Vinyl Turntable" label
    - "Analog Playback" subtitle
- **Containing File:** `RadioConsole.Web/Components/Shared/NowPlayingPanel.razor`
- **Location in UI:** Center panel in main layout (1/3 width)
- **Status:** ✅ Implemented

#### VisualizationPanel
- **Name:** VisualizationPanel (Right Panel)
- **Overall Purpose:** Real-time audio visualization using SignalR and FFT data from the audio stream.
- **Components on Panel:**
  - Visualization type dropdown (Spectrum/Waveform/Level Meter)
  - Canvas element for rendering visualizations
  - SignalR connection for real-time FFT data
- **Containing File:** `RadioConsole.Web/Components/Shared/VisualizationPanel.razor`
- **Location in UI:** Right panel in main layout (1/3 width)
- **Status:** ✅ Implemented

### 1.2 Slide-Out/Hidden Panels

#### SystemTestPanel
- **Name:** SystemTestPanel
- **Overall Purpose:** Testing dashboard for TTS, audio tones, and event simulations. Used during development and troubleshooting.
- **Components on Panel:**
  - TTS test buttons (Male/Female voices)
  - Audio tone test buttons (300Hz, 440Hz)
  - Event simulation buttons (Doorbell, Phone, Notification)
  - Test execution status indicators
- **Containing File:** `RadioConsole.Web/Components/Shared/SystemTestPanel.razor`
- **Location in UI:** Slide-out from right side (activated by gear icon in MainLayout)
- **Status:** ✅ Implemented (partially - slides in from right)

### 1.3 Full Page Panels (Separate Routes)

#### RadioDemo Page
- **Name:** RadioDemo Page
- **Overall Purpose:** Demonstration and documentation page for radio control components, numeric keypad, and touch keyboard.
- **Components on Panel:**
  - Tabbed interface with 4 tabs:
    1. Radio Control tab with RaddyRadioControlPanel
    2. Numeric Keypad tab with NumericKeypad component demo
    3. Touch Keyboard tab with TouchKeyboard component demo
    4. Documentation tab with component usage details
- **Containing File:** `RadioConsole.Web/Components/Pages/RadioDemo.razor`
- **Route:** `/radio-demo`
- **Status:** ✅ Implemented (accessible via NavMenu)

#### SystemPanel Page
- **Name:** SystemPanel Page
- **Overall Purpose:** System configuration and status management page with multiple management interfaces.
- **Components on Panel:**
  - Tabbed interface with 3 tabs:
    1. Configuration Management (ConfigurationManagement component)
    2. Alerts & Notifications (AlertNotificationManagement component)
    3. System Status (SystemStatus component)
- **Containing File:** `RadioConsole.Web/Components/Pages/SystemPanel.razor`
- **Route:** `/system`
- **Status:** ✅ Implemented (accessible via NavMenu)

---

## 2. Reusable UI Components (Building Blocks)

These are not standalone panels but reusable components used within panels:

- **RaddyRadioControlPanel** - Comprehensive radio control interface
- **NumericKeypad** - Touchscreen-optimized numeric input
- **TouchKeyboard** - Full QWERTY keyboard for text input
- **ConfigurationManagement** - CRUD operations for configuration settings
- **AlertNotificationManagement** - TTS and notification settings management
- **SystemStatus** - CPU/Memory/Disk/Network monitoring
- **FileSelector** - File browser component

**Location:** `RadioConsole.Web/Components/Shared/`

---

## 3. Proposed Panel Architecture

### 3.1 Design Principles

Based on the requirements in `RadioPlan_v3.md` section 4 (Blazor User Interface), the panel architecture follows these principles:

1. **Material Design 3** - Using MudBlazor component library for consistent Material Design 3 styling
2. **12.5" x 3.75" Ultra-wide Display** - Optimized for horizontal layout with three main columns
3. **Kiosk Mode** - Full-screen, frameless browser window (hiding URL bar and taskbar)
4. **Responsive Grid System** - Three-column layout for main panels (Left/Center/Right)
5. **Slide-Out/Overlay Pattern** - For system panels and testing tools
6. **Context-Aware Center Panel** - Dynamically changes based on selected audio input
7. **Icon-Driven Navigation** - Header icons trigger panel visibility changes
8. **Real-Time Updates** - SignalR for live data (visualizations, status updates)

### 3.2 Panel Types

#### Type A: Fixed Main Layout Panels
- Always visible in the main layout
- Occupy dedicated screen regions (Left/Center/Right)
- Examples: AudioSetupPanel, NowPlayingPanel, VisualizationPanel

#### Type B: Overlay/Slide-Out Panels
- Hidden by default
- Triggered by header icons or user actions
- Slide in from a direction (right/left/bottom)
- Modal/semi-modal behavior (can dim background)
- Examples: SystemTestPanel, future ConfigurationPanel

#### Type C: Full Page Panels
- Separate routes accessible via navigation
- Replace main layout entirely
- Used for comprehensive settings and demo pages
- Examples: RadioDemo, SystemPanel

#### Type D: Context-Aware Dynamic Panels
- Change content based on application state
- Embedded within Type A panels
- Examples: NowPlayingPanel (shows different UI for Radio/Spotify/Vinyl)

---

## 4. Panel On-Disk Structure

### 4.1 Directory Organization

```
RadioConsole.Web/Components/
├── Pages/                          # Type C: Full Page Panels
│   ├── Home.razor                 # Main entry point
│   ├── RadioDemo.razor            # Demo page for radio components
│   ├── SystemPanel.razor          # System management page
│   └── Error.razor                # Error handling page
│
├── Shared/                        # Type A & B Panels + Reusable Components
│   ├── GlobalHeader.razor         # Type A: Header panel
│   ├── AudioSetupPanel.razor      # Type A: Left panel
│   ├── NowPlayingPanel.razor      # Type A: Center panel (Type D behavior)
│   ├── VisualizationPanel.razor   # Type A: Right panel
│   ├── SystemTestPanel.razor      # Type B: Slide-out testing panel
│   │
│   ├── RaddyRadioControlPanel.razor    # Reusable component
│   ├── NumericKeypad.razor             # Reusable component
│   ├── TouchKeyboard.razor             # Reusable component
│   ├── ConfigurationManagement.razor   # Reusable component
│   ├── AlertNotificationManagement.razor  # Reusable component
│   ├── SystemStatus.razor              # Reusable component
│   └── FileSelector.razor              # Reusable component
│
└── Layout/
    ├── MainLayout.razor           # Main application layout
    └── NavMenu.razor              # Navigation menu
```

### 4.2 Naming Conventions

- **Panels:** End with `Panel` suffix (e.g., `AudioSetupPanel`, `SystemTestPanel`)
- **Pages:** Descriptive names without suffix (e.g., `RadioDemo`, `SystemPanel`)
- **Components:** Descriptive names (e.g., `NumericKeypad`, `TouchKeyboard`)
- **Files:** PascalCase matching the component name (e.g., `AudioSetupPanel.razor`)

---

## 5. Panel Hide/Slide In/Out Mechanism

### 5.1 Current Implementation

The `SystemTestPanel` demonstrates the slide-out pattern:

```razor
<!-- In MainLayout.razor -->
<MudIconButton Icon="@Icons.Material.Filled.Settings" 
               Color="Color.Primary" 
               Class="system-test-toggle"
               OnClick="ToggleSystemTestPanel" />

<div class="system-test-panel @(IsSystemTestPanelOpen ? "open" : "")">
  <SystemTestPanel />
</div>

@code {
  private bool IsSystemTestPanelOpen { get; set; } = false;

  private void ToggleSystemTestPanel()
  {
    IsSystemTestPanelOpen = !IsSystemTestPanelOpen;
  }
}
```

### 5.2 Proposed Enhanced Mechanism

For a more robust panel management system, implement a **PanelService**:

```csharp
// RadioConsole.Web/Services/PanelService.cs
public class PanelService
{
  private readonly Dictionary<string, bool> _panelStates = new();
  public event Action? OnPanelStateChanged;

  public bool IsPanelOpen(string panelName)
  {
    return _panelStates.TryGetValue(panelName, out var isOpen) && isOpen;
  }

  public void TogglePanel(string panelName)
  {
    if (_panelStates.ContainsKey(panelName))
      _panelStates[panelName] = !_panelStates[panelName];
    else
      _panelStates[panelName] = true;
    
    OnPanelStateChanged?.Invoke();
  }

  public void OpenPanel(string panelName)
  {
    _panelStates[panelName] = true;
    OnPanelStateChanged?.Invoke();
  }

  public void ClosePanel(string panelName)
  {
    _panelStates[panelName] = false;
    OnPanelStateChanged?.Invoke();
  }

  public void CloseAllPanels()
  {
    foreach (var key in _panelStates.Keys.ToList())
      _panelStates[key] = false;
    
    OnPanelStateChanged?.Invoke();
  }
}
```

### 5.3 CSS Animation Classes

Define CSS transitions for smooth slide animations:

```css
/* wwwroot/css/panels.css */
.slide-panel {
  position: fixed;
  background-color: #2c2c2c;
  box-shadow: -4px 0 8px rgba(0,0,0,0.3);
  transition: transform 0.3s ease-in-out;
  z-index: 1000;
  overflow-y: auto;
}

/* Slide from right */
.slide-panel-right {
  right: 0;
  top: 0;
  bottom: 0;
  width: 400px;
  transform: translateX(100%);
}

.slide-panel-right.open {
  transform: translateX(0);
}

/* Slide from left */
.slide-panel-left {
  left: 0;
  top: 0;
  bottom: 0;
  width: 400px;
  transform: translateX(-100%);
}

.slide-panel-left.open {
  transform: translateX(0);
}

/* Slide from bottom */
.slide-panel-bottom {
  left: 0;
  right: 0;
  bottom: 0;
  height: 300px;
  transform: translateY(100%);
}

.slide-panel-bottom.open {
  transform: translateY(0);
}

/* Backdrop overlay */
.panel-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  z-index: 999;
  opacity: 0;
  visibility: hidden;
  transition: opacity 0.3s ease-in-out, visibility 0.3s ease-in-out;
}

.panel-backdrop.visible {
  opacity: 1;
  visibility: visible;
}
```

---

## 6. Linkage Between Panels and Main Bar Icons

### 6.1 Current Implementation

The `GlobalHeader` component displays date/time and status icons. Panel controls are currently in `MainLayout.razor`:

```razor
<!-- MainLayout.razor -->
<MudIconButton Icon="@Icons.Material.Filled.Settings" 
               Color="Color.Primary" 
               Class="system-test-toggle"
               OnClick="ToggleSystemTestPanel" />
```

### 6.2 Proposed Enhancement

Move icon controls to `GlobalHeader` for centralized navigation:

```razor
<!-- GlobalHeader.razor (proposed enhancement) -->
<div class="global-header">
  <MudGrid Justify="Justify.SpaceBetween" AlignItems="AlignItems.Center">
    <MudItem xs="6">
      <!-- Date/Time Display -->
    </MudItem>
    
    <MudItem xs="6" Style="text-align: right;">
      <MudStack Row="true" Justify="Justify.FlexEnd" Spacing="2">
        <!-- Status Icons (existing) -->
        <MudTooltip Text="WiFi Status">
          <MudIcon Icon="@Icons.Material.Filled.Wifi" />
        </MudTooltip>
        
        <!-- Panel Control Icons (new) -->
        <MudTooltip Text="System Configuration">
          <MudIconButton Icon="@Icons.Material.Filled.Settings" 
                         Color="Color.Primary" 
                         OnClick="@(() => PanelService.TogglePanel("Configuration"))" />
        </MudTooltip>
        
        <MudTooltip Text="System Test Panel">
          <MudIconButton Icon="@Icons.Material.Filled.Science" 
                         Color="Color.Info" 
                         OnClick="@(() => PanelService.TogglePanel("SystemTest"))" />
        </MudTooltip>
        
        <MudTooltip Text="System Status">
          <MudIconButton Icon="@Icons.Material.Filled.Dashboard" 
                         Color="Color.Success" 
                         OnClick="@(() => PanelService.TogglePanel("SystemStatus"))" />
        </MudTooltip>
      </MudStack>
    </MudItem>
  </MudGrid>
</div>
```

### 6.3 Icon-to-Panel Mapping

| Icon | Panel Name | Panel Type | Purpose |
|------|-----------|------------|---------|
| `Settings` | ConfigurationPanel | Type B (Slide-out) | System configuration management |
| `Science` | SystemTestPanel | Type B (Slide-out) | Testing and diagnostics |
| `Dashboard` | SystemStatusPanel | Type B (Slide-out) | Real-time system metrics |
| `Tune` | RadioControlPanel | Type B (Slide-out) | Advanced radio controls |
| `MusicNote` | PlaylistPanel | Type B (Slide-out) | Spotify/MP3 playlist management |

---

## 7. Panel Template

### 7.1 Standard Panel Template

Use this template when creating a new Type B (slide-out) panel:

```razor
@* File: RadioConsole.Web/Components/Shared/[PanelName]Panel.razor *@
@inject ILogger<[PanelName]Panel> Logger
@inject ISnackbar Snackbar
@inject PanelService PanelService
@implements IDisposable

<MudPaper Class="pa-4" Style="height: 100%; background-color: #2c2c2c;">
  @* Panel Header *@
  <MudStack Row="true" Justify="Justify.SpaceBetween" AlignItems="AlignItems.Center" Class="mb-4">
    <MudText Typo="Typo.h5">
      <MudIcon Icon="@Icons.Material.Filled.[IconName]" Class="mr-2" />
      [Panel Display Name]
    </MudText>
    <MudIconButton Icon="@Icons.Material.Filled.Close" 
                   Color="Color.Default" 
                   OnClick="ClosePanel" />
  </MudStack>

  <MudDivider Class="mb-4" />

  @* Panel Content *@
  <MudStack Spacing="3">
    @* Add your panel content here *@
  </MudStack>
</MudPaper>

@code {
  protected override void OnInitialized()
  {
    Logger.LogInformation("[PanelName]Panel initialized");
  }

  private void ClosePanel()
  {
    PanelService.ClosePanel("[PanelName]");
  }

  public void Dispose()
  {
    // Clean up resources
  }
}
```

### 7.2 Usage in MainLayout

```razor
<!-- MainLayout.razor -->
<div class="slide-panel slide-panel-right @(PanelService.IsPanelOpen("[PanelName]") ? "open" : "")">
  <[PanelName]Panel />
</div>

@if (PanelService.IsPanelOpen("[PanelName]"))
{
  <div class="panel-backdrop visible" @onclick="@(() => PanelService.ClosePanel("[PanelName]"))"></div>
}
```

---

## 8. Phased Development Plan

### Phase 1: Foundation (✅ COMPLETE)
- [x] Basic MainLayout with three-column grid
- [x] GlobalHeader with date/time and status indicators
- [x] AudioSetupPanel (left column)
- [x] NowPlayingPanel (center column, context-aware)
- [x] VisualizationPanel (right column)
- [x] Basic slide-out SystemTestPanel
- [x] RadioDemo and SystemPanel full-page routes

### Phase 2: Panel Management Service (🚧 PROPOSED)
- [ ] Create `PanelService` for centralized panel state management
- [ ] Move panel control icons to `GlobalHeader`
- [ ] Implement CSS animation classes for slide transitions
- [ ] Add backdrop overlay for modal behavior
- [ ] Update `SystemTestPanel` to use new service

### Phase 3: Configuration & Status Panels (🔲 PLANNED)
Following RadioPlan_v3.md requirements:

#### ConfigurationPanel (Type B - Slide-out from right)
- [ ] Comprehensive CRUD for all configuration settings
- [ ] Tab 1: General Settings (audio, display)
- [ ] Tab 2: Device Configuration (USB devices, network)
- [ ] Tab 3: Advanced Settings (logging, performance)
- [ ] Reuse `ConfigurationManagement` component
- [ ] Trigger icon: `Settings` in GlobalHeader

#### SystemStatusPanel (Type B - Slide-out from right)
- [ ] Real-time CPU/Memory/Disk monitoring
- [ ] Network status details
- [ ] Audio device status
- [ ] Process information
- [ ] Reuse `SystemStatus` component
- [ ] Trigger icon: `Dashboard` in GlobalHeader
- [ ] Update interval: 1 second for real-time data

### Phase 4: Rich Audio Panels (🔲 PLANNED)

#### SpotifyPanel (Type D - Replaces NowPlayingPanel content)
- [ ] Album art display (large, high-res)
- [ ] Track metadata (title, artist, album)
- [ ] Playback timeline with seek
- [ ] Playlist browser
- [ ] Search functionality
- [ ] Lyrics display (if available)
- [ ] Status: Currently basic implementation exists in NowPlayingPanel

#### MP3PlayerPanel (Type D - Replaces NowPlayingPanel content)
- [ ] File browser for local MP3 library
- [ ] Playlist management
- [ ] ID3 tag display
- [ ] Album art (from ID3 tags)
- [ ] Playback controls
- [ ] Status: Not yet implemented

#### RadioPanel (Type D - Already in NowPlayingPanel)
- [x] LED-style frequency display
- [x] Band indicator
- [x] Signal strength
- [x] Stereo indicator
- [ ] ENHANCEMENT: Add preset station buttons
- [ ] ENHANCEMENT: Add scan functionality UI

#### PhonoPanel (Type D - Already in NowPlayingPanel)
- [x] Vinyl record animation/icon
- [ ] ENHANCEMENT: Add spinning animation
- [ ] ENHANCEMENT: Add pre-amp settings UI
- [ ] ENHANCEMENT: Add rumble filter controls

### Phase 5: Testing & Notification Panels (🔲 PLANNED)

#### TestingPanel (Type B - Slide-out from right)
- [ ] TTS testing interface with custom text input
- [ ] Audio file playback testing
- [ ] Event simulation (doorbell, phone, notifications)
- [ ] Custom alert creation from audio files
- [ ] Volume ducking tests
- [ ] Status: Partial implementation in SystemTestPanel
- [ ] Trigger icon: `Science` in GlobalHeader

#### AlertManagementPanel (Type B - Slide-out from right)
- [ ] Alert type configuration (Ring, Notify, Alert, Alarm)
- [ ] Audio file assignment for each alert type
- [ ] Volume settings per alert
- [ ] Priority settings
- [ ] Test buttons for each alert
- [ ] Status: Basic implementation in AlertNotificationManagement
- [ ] Could be merged into ConfigurationPanel

### Phase 6: Advanced Features (🔲 FUTURE)

#### PlaylistPanel (Type B - Slide-out from bottom)
- [ ] Spotify playlist browser
- [ ] MP3 playlist editor
- [ ] Favorite stations (radio presets)
- [ ] Recently played history
- [ ] Trigger icon: `MusicNote` in GlobalHeader

#### EqualizerPanel (Type B - Slide-out from bottom)
- [ ] Graphical EQ with frequency bands
- [ ] Preset EQ settings (Rock, Jazz, Classical, etc.)
- [ ] Custom EQ profile management
- [ ] Visual frequency response curve
- [ ] Trigger icon: `GraphicEq` in GlobalHeader

#### CastDevicePanel (Type B - Slide-out from left)
- [ ] Google Cast device discovery
- [ ] Multi-room audio grouping
- [ ] Individual device volume controls
- [ ] Device status indicators
- [ ] Trigger icon: `Cast` in GlobalHeader

---

## 9. Process for Adding a New Panel

### 9.1 Determine Panel Type

First, determine which panel type fits your requirements:

- **Type A (Fixed Main Layout):** Panel is always visible in a dedicated screen region
- **Type B (Slide-Out/Overlay):** Panel is hidden by default, triggered by user action
- **Type C (Full Page):** Panel needs its own route and replaces main layout
- **Type D (Dynamic Content):** Panel changes content based on application state

### 9.2 Create Panel File

1. **For Type A, B, or D panels:**
   - Create file: `RadioConsole.Web/Components/Shared/[PanelName]Panel.razor`
   - Use the panel template from Section 7.1

2. **For Type C panels:**
   - Create file: `RadioConsole.Web/Components/Pages/[PanelName].razor`
   - Add `@page "/route-name"` directive at the top
   - Add `@rendermode InteractiveServer` if needed

### 9.3 Implement Panel Logic

```razor
@code {
  // State properties
  private string ExampleProperty { get; set; } = "";

  // Lifecycle methods
  protected override async Task OnInitializedAsync()
  {
    Logger.LogInformation("[PanelName]Panel initializing");
    await LoadDataAsync();
  }

  // Data loading
  private async Task LoadDataAsync()
  {
    // Load data from API or services
  }

  // Event handlers
  private void HandleButtonClick()
  {
    Snackbar.Add("Action completed", Severity.Success);
  }

  // Cleanup
  public void Dispose()
  {
    // Dispose of resources (timers, subscriptions, etc.)
  }
}
```

### 9.4 Add Panel to Layout

**For Type B (Slide-Out) panels:**

1. Add toggle method to `GlobalHeader.razor`:

```razor
<MudTooltip Text="[Panel Description]">
  <MudIconButton Icon="@Icons.Material.Filled.[IconName]" 
                 Color="Color.Primary" 
                 OnClick="@(() => PanelService.TogglePanel("[PanelName]"))" />
</MudTooltip>
```

2. Add panel container to `MainLayout.razor`:

```razor
<!-- After other slide-out panels -->
<div class="slide-panel slide-panel-right @(PanelService.IsPanelOpen("[PanelName]") ? "open" : "")">
  <[PanelName]Panel />
</div>

@if (PanelService.IsPanelOpen("[PanelName]"))
{
  <div class="panel-backdrop visible" @onclick="@(() => PanelService.ClosePanel("[PanelName]"))"></div>
}
```

**For Type C (Full Page) panels:**

1. Add navigation link to `NavMenu.razor`:

```razor
<div class="nav-item px-3">
  <NavLink class="nav-link" href="[route-name]">
    <span class="bi bi-[icon-name]-nav-menu" aria-hidden="true"></span> [Display Name]
  </NavLink>
</div>
```

### 9.5 Style the Panel

Add custom styles to `wwwroot/css/app.css` or create panel-specific CSS file:

```css
/* Panel-specific styles */
.[panel-name]-panel {
  /* Custom styles */
}
```

### 9.6 Test the Panel

1. Build and run the application
2. Verify panel opens/closes correctly
3. Test all interactive elements
4. Verify responsive behavior
5. Test on target 12.5" x 3.75" display resolution
6. Verify Material Design 3 styling consistency

### 9.7 Document the Panel

1. Add panel entry to this document (Section 1)
2. Update the phased development plan (Section 8)
3. Add any new components to Section 2
4. Update icon-to-panel mapping (Section 6.3)

---

## 10. Material Design 3 Guidelines

### 10.1 Color Palette

The application uses a dark theme optimized for the vintage console aesthetic:

```csharp
PaletteDark = new PaletteDark()
{
  Primary = "#90caf9",        // Light blue
  Secondary = "#ce93d8",      // Light purple
  AppbarBackground = "#1e1e1e",
  Background = "#121212",
  Surface = "#2c2c2c",
  DrawerBackground = "#1e1e1e",
}
```

### 10.2 Typography

- **h2:** Large displays (time, frequency)
- **h3:** Page headers
- **h5:** Panel headers
- **h6:** Section headers
- **body1:** Main text content
- **body2:** Secondary text, metadata

### 10.3 Spacing

- Use MudBlazor `Class="pa-4"` for consistent padding (16px)
- `MudStack Spacing="3"` for vertical spacing between elements
- `mb-4` for margin-bottom on headers

### 10.4 Elevation

- Main panels: `Elevation="2"`
- Cards and paper: `Elevation="4"`
- Dialogs and overlays: `Elevation="8"`

### 10.5 Icons

Use Material Design icons from `Icons.Material.Filled.*`:
- Settings: `Icons.Material.Filled.Settings`
- Audio: `Icons.Material.Filled.VolumeUp`
- Radio: `Icons.Material.Filled.Radio`
- Status: `Icons.Material.Filled.CheckCircle`
- Close: `Icons.Material.Filled.Close`

---

## 11. Accessibility Considerations

1. **Keyboard Navigation:** All interactive elements must be keyboard-accessible
2. **ARIA Labels:** Use `aria-label` on canvas elements and icon buttons
3. **Focus Indicators:** Visible focus states on all interactive elements
4. **Color Contrast:** Minimum 4.5:1 contrast ratio for text
5. **Touch Targets:** Minimum 44x44px touch targets for touchscreen
6. **Screen Reader Support:** Semantic HTML and ARIA roles

---

## 12. Performance Considerations

1. **SignalR Throttling:** Limit visualization updates to 30fps maximum
2. **Lazy Loading:** Consider lazy loading for Type C full-page panels
3. **Dispose Pattern:** Always dispose of timers, subscriptions, and HTTP clients
4. **State Management:** Use PanelService to avoid prop drilling
5. **CSS Animations:** Use GPU-accelerated transforms (translateX/Y) instead of position changes
6. **Image Optimization:** Compress album art and icons, use appropriate formats

---

## 13. Testing Strategy

1. **Unit Tests:** Test panel components in isolation
2. **Integration Tests:** Test panel interactions with services
3. **UI Tests:** Test slide-in/out animations and user interactions
4. **Responsive Tests:** Verify layout on 12.5" x 3.75" display
5. **Performance Tests:** Monitor SignalR message rates and render times
6. **Accessibility Tests:** Automated accessibility audits with tools like axe-core

---

## 14. Known Issues and Future Improvements

### Current Known Issues
1. **UI Click Events Not Received:** Investigation needed for Blazor event handling (mentioned in issue)
2. **MudBlazor Warnings:** Some analyzer warnings for illegal attributes need cleanup

### Future Improvements
1. Implement centralized `PanelService` for state management
2. Move all panel control icons to `GlobalHeader`
3. Add backdrop blur effect for better visual separation
4. Implement panel keyboard shortcuts (ESC to close)
5. Add panel resize capability for certain panels
6. Implement panel position memory (remember last opened state)
7. Add animation preferences (allow users to disable animations)
8. Create panel preview/thumbnail system

---

## 15. Related Documentation

- **RadioPlan_v3.md** - Overall project architecture and requirements
- **UI_VISUALIZATION_GUIDE.md** - Detailed visualization system documentation
- **PHASE5_INTEGRATION_TODO.md** - Current phase tasks and integration checklist

---

## Document Maintenance

**Last Updated:** 2025-11-21  
**Version:** 1.0  
**Author:** Radio Console Development Team  
**Review Frequency:** Update after each major UI change or new panel addition
