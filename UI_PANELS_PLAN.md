# Radio Console UI Panels Architecture & Development Plan

## Document Purpose
This document provides a comprehensive overview of the Radio Console UI panel architecture, current implementation status, and a development plan for future panel creation. It serves as a guide for adding new panels and managing the UI layout following Material Design 3 principles.

## Critical Specification Requirement

**⚠️ ALL UI MUST FIT WITHIN THE 3-PANEL LAYOUT ⚠️**

Per the requirements in `RadioPlan_v3.md` section 4 (Blazor User Interface):
- The UI must maintain a **three-column layout** at all times: Left Panel / Center Panel / Right Panel
- The **GlobalHeader** is always visible at the top
- Additional functionality is accessed via **slide-out panels** that overlay the main layout
- **No full-page routes** that replace or navigate away from the main layout
- **No side navigation menu** - all navigation via icons in GlobalHeader

### Current Compliance Status (Updated 2025-11-21)
- ✅ **Compliant:** MainLayout with AudioSetupPanel, NowPlayingPanel, VisualizationPanel
- ✅ **Compliant:** SystemTestPanel (slide-out overlay)
- ✅ **Compliant:** ConfigurationPanel (slide-out overlay) - **NEW**
- ✅ **Compliant:** SystemStatusPanel (slide-out overlay) - **NEW**
- ✅ **Compliant:** AlertManagementPanel (slide-out overlay) - **NEW**
- ✅ **RESOLVED:** RadioDemo.razor removed and archived to `/docs/archived/`
- ✅ **RESOLVED:** SystemPanel.razor removed and archived to `/docs/archived/`
- ✅ **RESOLVED:** NavMenu.razor removed, all navigation via GlobalHeader icons

**Status:** ✅ **FULLY COMPLIANT** - All specification violations have been resolved.

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
- **Location in UI:** Slide-out from right side (activated by Science icon in GlobalHeader)
- **Status:** ✅ Implemented

#### ConfigurationPanel
- **Name:** ConfigurationPanel
- **Overall Purpose:** Configuration management panel for all system settings, audio configuration, and device settings.
- **Components on Panel:**
  - **Tab 1 - General Settings:** Reuses ConfigurationManagement component for CRUD operations
  - **Tab 2 - Device Configuration:** USB device status (Raddy RF320, Phono), network status, IP address
  - **Tab 3 - Advanced Settings:** Logging configuration, performance settings (buffer size, sample rate, FFT rate)
  - Import/Export configuration buttons
- **Containing File:** `RadioConsole.Web/Components/Shared/ConfigurationPanel.razor`
- **Location in UI:** Slide-out from right side (activated by Settings icon in GlobalHeader)
- **Status:** ✅ Implemented (2025-11-21) | ✅ Enhanced with tabs (2025-11-21 Phase 3)

#### SystemStatusPanel
- **Name:** SystemStatusPanel
- **Overall Purpose:** Real-time system monitoring panel with charts showing CPU, memory, disk usage, network status, and device information.
- **Components on Panel:**
  - **Resource Usage Section:**
    - Real-time CPU usage with historical chart (30-point SVG graph)
    - Real-time Memory usage with historical chart (30-point SVG graph)
    - Progress bars with color-coded thresholds
  - **Device Status Section:**
    - Network connection status with visual indicator
    - Audio device count with status indicator
    - USB device count with status indicator
    - Network bandwidth monitoring (download/upload speeds)
  - **Controls:**
    - Refresh button for manual updates
    - Configurable update interval (1s, 5s, 10s, 30s)
  - Reuses existing SystemStatus component for server/resource details
- **Containing File:** `RadioConsole.Web/Components/Shared/SystemStatusPanel.razor`
- **Location in UI:** Slide-out from right side (activated by Dashboard icon in GlobalHeader)
- **Status:** ✅ Implemented (2025-11-21) | ✅ Enhanced with charts (2025-11-21 Phase 3)

#### AlertManagementPanel
- **Name:** AlertManagementPanel
- **Overall Purpose:** Alerts and notifications management panel for configuring TTS settings and notification preferences with testing capabilities.
- **Components on Panel:**
  - **Alert Testing Interface (Left Column):**
    - Four alert types: Ring, Notify, Alert, Alarm
    - Per-alert controls: Play/Test button, Stop button
    - Volume slider with percentage display (0-100%)
    - Real-time volume level indicator (animated bar during playback)
    - Audio file assignment display
    - Priority display (color-coded chips)
  - **Alert Configuration (Right Column):**
    - Reuses AlertNotificationManagement component
    - TTS configuration (engine, voice, speed)
    - Notification settings
    - Alert audio file assignment
  - **Alert History:** Dialog showing last 50 alerts with timestamps and status
- **Containing File:** `RadioConsole.Web/Components/Shared/AlertManagementPanel.razor`
- **Location in UI:** Slide-out from right side (activated by Notifications icon in GlobalHeader)
- **Status:** ✅ Implemented (2025-11-21) | ✅ Enhanced with testing (2025-11-21 Phase 3)

#### RadioControlPanel
- **Name:** RadioControlPanel
- **Overall Purpose:** Advanced radio control interface for the Raddy RF320 USB radio. Provides comprehensive tuning, scanning, and volume controls.
- **Components on Panel:**
  - LED-style frequency display (large, digital font with glow effect)
  - LED-style band indicator (FM/AM/SW/AIR/VHF)
  - Band selector dropdown (5 bands supported)
  - Tuning controls: Tune Up/Down buttons
  - Scan controls: Scan Up/Down buttons
  - Radio volume slider with +/- buttons
  - Set Frequency button (opens numeric keypad)
  - Status indicators: Streaming status, Device detection, Signal strength
  - Reuses RaddyRadioControlPanel component
- **Containing File:** Integrated into `RadioConsole.Web/Components/Layout/MainLayout.razor`
- **Location in UI:** Slide-out from right side (activated by Tune icon in GlobalHeader OR "Advanced Radio Controls" button in NowPlayingPanel Radio Mode)
- **Status:** ✅ Implemented (2025-11-21 Phase 2B) | ✅ Added to GlobalHeader (2025-11-21)

### 1.3 Archived Legacy Panels (REMOVED)

**Note:** These panels have been removed and archived to `/docs/archived/` as they violated the 3-panel layout specification.

#### RadioDemo Page (✅ ARCHIVED)
- **Previous Implementation:** Separate full-page route at `/radio-demo`
- **Archived Location:** `/docs/archived/RadioDemo.razor`
- **Status:** ✅ Removed and archived (2025-11-21)
- **Replacement:** RaddyRadioControlPanel integration pending in NowPlayingPanel

#### SystemPanel Page (✅ ARCHIVED)
- **Previous Implementation:** Separate full-page route at `/system`
- **Archived Location:** `/docs/archived/SystemPanel.razor`
- **Status:** ✅ Removed and archived (2025-11-21)
- **Replacement:** Replaced by ConfigurationPanel, SystemStatusPanel, and AlertManagementPanel
  - Remove NavMenu entry
  - Each panel gets its own icon in GlobalHeader

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

**IMPORTANT:** All UI must fit within the 3-panel layout defined in the specification. No full-page routes that replace the main layout are permitted.

#### Type A: Fixed Main Layout Panels
- Always visible in the main layout
- Occupy dedicated screen regions (Left/Center/Right)
- Cannot be hidden or closed
- Examples: AudioSetupPanel, NowPlayingPanel, VisualizationPanel

#### Type B: Overlay/Slide-Out Panels
- Hidden by default
- Triggered by header icons or user actions
- Slide in from a direction (right/left/bottom)
- Overlay the 3-panel layout without replacing it
- Modal/semi-modal behavior (can dim background)
- Examples: SystemTestPanel, ConfigurationPanel, SystemStatusPanel

#### Type D: Context-Aware Dynamic Panels
- Change content based on application state
- Embedded within Type A panels
- Switch between different views/modes
- Examples: NowPlayingPanel (shows different UI for Radio/Spotify/Vinyl)

**Note:** Type C (Full Page Panels) has been eliminated per specification requirements. All existing full-page panels must be converted to Type B (slide-out) or integrated into Type A/D panels.

---

## 4. Panel On-Disk Structure

### 4.1 Directory Organization

```
RadioConsole.Web/Components/
├── Pages/                          # Minimal - Only essential pages
│   ├── Home.razor                 # Main entry point (renders MainLayout with 3-panel structure)
│   └── Error.razor                # Error handling page
│   
│   # DEPRECATED - TO BE REMOVED:
│   ├── RadioDemo.razor            # ⚠️ TO BE REMOVED - Breaks 3-panel layout
│   └── SystemPanel.razor          # ⚠️ TO BE REMOVED - Breaks 3-panel layout
│
├── Shared/                        # Type A & B Panels + Reusable Components
│   # Type A: Fixed Main Layout Panels (Always Visible)
│   ├── GlobalHeader.razor         # Type A: Header panel
│   ├── AudioSetupPanel.razor      # Type A: Left panel
│   ├── NowPlayingPanel.razor      # Type A: Center panel (Type D behavior)
│   ├── VisualizationPanel.razor   # Type A: Right panel
│   
│   # Type B: Slide-Out Panels (Hidden by Default)
│   ├── SystemTestPanel.razor      # Type B: Testing panel (slides from right)
│   ├── ConfigurationPanel.razor   # Type B: Configuration panel (TO BE CREATED)
│   ├── SystemStatusPanel.razor    # Type B: System status panel (TO BE CREATED)
│   ├── AlertManagementPanel.razor # Type B: Alert/notification panel (TO BE CREATED)
│   │
│   # Reusable Components (Building Blocks)
│   ├── RaddyRadioControlPanel.razor    # Radio control UI
│   ├── NumericKeypad.razor             # Numeric input
│   ├── TouchKeyboard.razor             # Text input
│   ├── ConfigurationManagement.razor   # Config CRUD UI
│   ├── AlertNotificationManagement.razor  # Alert/TTS settings UI
│   ├── SystemStatus.razor              # System metrics UI
│   └── FileSelector.razor              # File browser
│
└── Layout/
    ├── MainLayout.razor           # 3-panel layout + slide-out panel containers
    └── NavMenu.razor              # Navigation menu (TO BE REMOVED - replaced by GlobalHeader icons)
```

### 4.2 Naming Conventions

- **Panels:** End with `Panel` suffix (e.g., `AudioSetupPanel`, `SystemTestPanel`, `ConfigurationPanel`)
- **Pages:** Only `Home.razor` and `Error.razor` should remain in Pages/
- **Components:** Descriptive names (e.g., `NumericKeypad`, `TouchKeyboard`)
- **Files:** PascalCase matching the component name (e.g., `AudioSetupPanel.razor`)

### 4.3 Migration Rules

1. **No new full-page routes** - All new features must use Type A or Type B panels
2. **Existing full-page panels must be converted** - See Phase 2 conversion plan
3. **NavMenu will be deprecated** - All navigation via GlobalHeader icons
4. **Home.razor is minimal** - Just renders MainLayout with 3-panel structure

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

| Icon | Panel Name | Panel Type | Purpose | Status |
|------|-----------|------------|---------|--------|
| `Settings` | ConfigurationPanel | Type B (Slide-out) | System configuration management | ✅ Implemented |
| `Dashboard` | SystemStatusPanel | Type B (Slide-out) | Real-time system metrics | ✅ Implemented |
| `Notifications` | AlertManagementPanel | Type B (Slide-out) | Alert and notification configuration | ✅ Implemented |
| `Science` | SystemTestPanel | Type B (Slide-out) | Testing and diagnostics | ✅ Implemented |
| `Tune` | RadioControlPanel | Type B (Slide-out) | Advanced radio controls | ✅ Implemented |
| `MusicNote` | PlaylistPanel | Type B (Slide-out) | Spotify/MP3 playlist management | 🔲 Planned |
| `GraphicEq` | EqualizerPanel | Type B (Slide-out) | Graphical EQ controls | 🔲 Planned |
| `Cast` | CastDevicePanel | Type B (Slide-out) | Google Cast device management | 🔲 Planned |

**Note:** All implemented panels (✅) are accessible via icons in the GlobalHeader.

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
- [x] RadioDemo and SystemPanel full-page routes (⚠️ TO BE DEPRECATED)

### Phase 2: Panel Management & Full-Page Conversion (✅ COMPLETE)

**Goal:** Eliminate all full-page routes and implement proper panel management within the 3-panel layout.

#### 2A: Panel Management Service (✅ COMPLETE)
- [x] Create `PanelService` for centralized panel state management
- [x] Implement CSS animation classes for slide transitions
- [x] Add backdrop overlay for modal behavior
- [x] Update `SystemTestPanel` to use new service
- [x] Move all panel control icons to `GlobalHeader`
- [x] Remove `NavMenu.razor` (replaced by GlobalHeader icons)

#### 2B: Convert RadioDemo Page to 3-Panel Layout (✅ COMPLETE)
**Current State:** RadioDemo.razor removed, archived at `/docs/archived/`. RaddyRadioControlPanel integrated into NowPlayingPanel.  
**Action Completed:** 
- [x] Remove `/radio-demo` route entirely
- [x] Remove RadioDemo entry from NavMenu.razor
- [x] Integrate RaddyRadioControlPanel into NowPlayingPanel (Radio Mode)
  - [x] Add "Advanced Radio Controls" button in Radio Mode
  - [x] Opens slide-out panel with full radio control interface
  - [x] Wire up to PanelService with "RadioControl" panel name
- [x] Archive RadioDemo.razor to `/docs/archived/` for reference
- [x] Remove demo/documentation tabs (not needed in production UI)
- [x] Update UI_PANELS_PLAN.md to mark completion

**Implementation Summary (2025-11-21):**
- ✅ Added "Advanced Radio Controls" button to Radio Mode in NowPlayingPanel
- ✅ Created RadioControl slide-out panel in MainLayout
- ✅ Integrated RaddyRadioControlPanel component into slide-out panel
- ✅ Panel accessible via button in Radio Mode of NowPlayingPanel
- ✅ Consistent with existing slide-out panel patterns
- ✅ All 176 tests still passing

#### 2C: Convert SystemPanel Page to Slide-Out Panels (✅ COMPLETE)
**Current State:** SystemPanel.razor removed, replaced with three new slide-out panels  

##### Create ConfigurationPanel (Type B - Slide-out) (✅ COMPLETE)
- [x] Create `RadioConsole.Web/Components/Shared/ConfigurationPanel.razor`
- [x] Reuse existing `ConfigurationManagement.razor` component inside panel
- [x] Add close button and panel header
- [x] Wire up to PanelService
- [x] Add Settings icon to GlobalHeader → opens ConfigurationPanel
- [x] Slide-in from right side

##### Create SystemStatusPanel (Type B - Slide-out) (✅ COMPLETE)
- [x] Create `RadioConsole.Web/Components/Shared/SystemStatusPanel.razor`
- [x] Reuse existing `SystemStatus.razor` component inside panel
- [x] Add close button and panel header
- [x] Wire up to PanelService
- [x] Add Dashboard icon to GlobalHeader → opens SystemStatusPanel
- [x] Real-time updates (1 second interval)
- [x] Slide-in from right side

##### Create AlertManagementPanel (Type B - Slide-out) (✅ COMPLETE)
- [x] Create `RadioConsole.Web/Components/Shared/AlertManagementPanel.razor`
- [x] Reuse existing `AlertNotificationManagement.razor` component inside panel
- [x] Add close button and panel header
- [x] Wire up to PanelService
- [x] Add Notifications icon to GlobalHeader → opens AlertManagementPanel
- [x] Slide-in from right side

##### Cleanup (✅ COMPLETE)
- [x] Remove `/system` route entirely
- [x] Remove SystemPanel entry from NavMenu.razor
- [x] Archive SystemPanel.razor to `/docs/archived/` for reference
- [x] Update MainLayout to include new slide-out panel containers
- [x] Verify all three panels work independently with PanelService

**Implementation Summary:**
- ✅ Created `PanelService` with event-driven state management
- ✅ Added comprehensive CSS animations for slide-out panels (right, left, top, bottom)
- ✅ Added backdrop overlay with blur effect
- ✅ Integrated all panel controls into GlobalHeader with icon buttons
- ✅ Removed deprecated NavMenu.razor
- ✅ Created three new Type B slide-out panels (Configuration, SystemStatus, AlertManagement)
- ✅ Archived deprecated full-page routes to `/docs/archived/`
- ✅ All 154 tests passing
- ✅ 3-panel layout specification fully compliant

### Phase 3: Enhanced Panel Features (🚧 IN PROGRESS - 2025-11-21)
Following RadioPlan_v3.md requirements:

#### ConfigurationPanel Enhancements (✅ COMPLETE)
- [x] Added tabbed interface (General, Device Configuration, Advanced Settings)
- [x] Tab 1: General Settings (audio, display) - Reuses ConfigurationManagement component
- [x] Tab 2: Device Configuration (USB devices, network) - Shows Raddy/Phono devices, network status
- [x] Tab 3: Advanced Settings (logging, performance) - Shows log level, buffer size, sample rate
- [x] Added import/export configuration buttons (UI ready, placeholder logic)
- [ ] Add search/filter functionality for settings (deferred to Phase 3B)
- [ ] Add import/export configuration backend (deferred to Phase 3B)

#### SystemStatusPanel Enhancements (✅ COMPLETE)
- [x] Real-time CPU/Memory/Disk charts - SVG mini-charts with 30-point history
- [x] Network bandwidth monitoring - Download/upload speed display
- [x] Audio device status with visual indicators - Count with color-coded status
- [x] USB device status - Device count with visual indicators
- [x] Added refresh button for manual updates
- [x] Added configurable update interval (1s, 5s, 10s, 30s dropdown)
- [ ] Process information and resource usage (deferred to Phase 3B)

#### AlertManagementPanel Enhancements (✅ COMPLETE)
- [x] Preview/test button for each alert type (Ring, Notify, Alert, Alarm)
- [x] Stop button for each alert type
- [x] Volume slider with percentage display per alert
- [x] Real-time volume level indicator - Animated progress bar during playback
- [x] Alert history/log viewer - Dialog showing last 50 alerts with timestamps
- [x] Visual alert type indicators - Icons and color-coding per type
- [ ] Visual waveform display for audio files (deferred to Phase 3B)

### Phase 4: Rich Audio Panels (🚧 IN PROGRESS)

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
- [x] Access to advanced radio controls via button (2025-11-21)
- [ ] ENHANCEMENT: Add preset station buttons (future)
- [ ] ENHANCEMENT: Add scan functionality UI (available in RaddyRadioControlPanel)

#### PhonoPanel (Type D - Already in NowPlayingPanel)
- [x] Vinyl record animation/icon
- [x] **ENHANCEMENT: Add spinning animation** ✅ (2025-11-21)
- [x] RPM indicator chip (33⅓ RPM)
- [x] Analog indicator chip
- [ ] ENHANCEMENT: Add pre-amp settings UI (future)
- [ ] ENHANCEMENT: Add rumble filter controls (future)

**Phase 4 Progress:** Core enhancements to Radio and Phono panels completed. Additional features deferred to future phases.

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

**IMPORTANT:** Type C (Full Page) panels are **not allowed**. All UI must fit within the 3-panel layout.

Choose the appropriate panel type:

- **Type A (Fixed Main Layout):** Panel is always visible in a dedicated screen region (Left/Center/Right)
  - Use only when the panel is a core part of the main UI
  - Examples: AudioSetupPanel, NowPlayingPanel, VisualizationPanel
  
- **Type B (Slide-Out/Overlay):** Panel is hidden by default, triggered by user action
  - Use for configuration, settings, status, testing, and auxiliary features
  - Slides in from right, left, or bottom
  - Examples: SystemTestPanel, ConfigurationPanel, SystemStatusPanel
  
- **Type D (Dynamic Content):** Panel changes content based on application state
  - Embedded within Type A panels
  - Switches between different views/modes
  - Examples: NowPlayingPanel content (Radio/Spotify/Vinyl modes)

**Rule:** If you think you need a full-page panel (Type C), you should be creating a Type B (slide-out) panel instead.

### 9.2 Create Panel File

**For Type A or Type B panels:**
- Create file: `RadioConsole.Web/Components/Shared/[PanelName]Panel.razor`
- Use the panel template from Section 7.1
- End filename with `Panel` suffix

**For Type D dynamic content:**
- Implement within the parent Type A panel
- Use conditional rendering based on state
- See NowPlayingPanel.razor for example

**NEVER create:**
- New files in `Pages/` directory (except Home.razor and Error.razor)
- New `@page` routes
- New NavMenu entries

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

1. Add icon button to `GlobalHeader.razor`:

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

**For Type A (Fixed Layout) panels:**

Not applicable - Type A panels are hardcoded in MainLayout.razor as part of the 3-panel structure. They cannot be added dynamically.

**For Type D (Dynamic Content):**

Implement within the parent Type A panel using conditional rendering:

```razor
@if (CurrentMode == "ModeName")
{
  <!-- Your mode-specific content -->
}
```

**REMOVED:** Instructions for Type C panels - no longer supported.

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
2. **Verify 3-panel layout is maintained** - Main layout should always show Left/Center/Right panels
3. Verify panel opens/closes correctly (for Type B panels)
4. Test all interactive elements
5. Verify responsive behavior
6. Test on target 12.5" x 3.75" display resolution
7. Verify Material Design 3 styling consistency
8. **Ensure no full-page routes** - URL should always remain at `/` (Home)

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

1. **MudBlazor Analyzer Warnings (LOW PRIORITY):**
   - ConfigurationManagement.razor: Illegal attributes `IsVisible` and `IsVisibleChanged` on MudDialog (6 warnings)
   - SystemStatus.razor: Illegal attributes `Checked` and `CheckedChanged` on MudSwitch (2 warnings)
   - **Impact:** Non-blocking analyzer warnings during build
   - **Action Required:** Update to use correct MudBlazor attribute patterns (@bind-Value instead of custom)
   - **Priority:** Low (does not affect functionality)
   - **Status:** Deferred to future maintenance

2. **Radio Control Integration Pending:**
   - RaddyRadioControlPanel not yet integrated into NowPlayingPanel Radio Mode
   - **Action Required:** Add "Advanced Radio Controls" button/option in Radio Mode
   - **Status:** Planned for Phase 3 enhancements

### Recently Resolved Issues (Phase 2)

1. ✅ **Full-Page Panels Break 3-Panel Layout (RESOLVED):**
   - RadioDemo.razor (`/radio-demo` route) - Removed and archived
   - SystemPanel.razor (`/system` route) - Removed and archived
   - Replaced with three new Type B slide-out panels
   - **Resolution Date:** 2025-11-21
   - **Status:** Complete

2. ✅ **NavMenu.razor Deprecated (RESOLVED):**
   - Side navigation menu removed
   - All navigation now via GlobalHeader icons
   - **Resolution Date:** 2025-11-21
   - **Status:** Complete

3. ✅ **Panel Management Service (RESOLVED):**
   - PanelService created with event-driven state management
   - Comprehensive CSS animations implemented
   - Backdrop overlay with blur effect added
   - **Resolution Date:** 2025-11-21
   - **Status:** Complete

### Future Improvements

1. ✅ **Integrate RaddyRadioControlPanel into NowPlayingPanel** - COMPLETED (2025-11-21)
2. **Fix MudBlazor analyzer warnings** - Update attribute bindings in ConfigurationManagement and SystemStatus
3. **Add panel keyboard shortcuts** (ESC to close, Ctrl+P for specific panels)
4. **Add panel resize capability** for certain panels (optional)
5. **Implement panel position memory** - Remember which panels were open
6. **Add animation preferences** - Allow users to disable animations for performance
7. **Create panel preview/thumbnail system** for better UX
8. **Add preset station buttons** to Radio Mode
9. **Add pre-amp settings UI** to Phono Mode
10. **Implement playlist browser** for Spotify and MP3 modes

---

## 15. Related Documentation

- **RadioPlan_v3.md** - Overall project architecture and requirements
- **UI_VISUALIZATION_GUIDE.md** - Detailed visualization system documentation
- **PHASE5_INTEGRATION_TODO.md** - Current phase tasks and integration checklist

---

## Document Maintenance

**Last Updated:** 2025-11-21  
**Version:** 2.2  
**Author:** Radio Console Development Team  
**Review Frequency:** Update after each major UI change or new panel addition

**Recent Updates:**
- **2025-11-21 Phase 4 Implementation:**
  - Added RadioControl panel icon to GlobalHeader (Tune icon)
  - All panels now accessible via meaningful icons in GlobalHeader
  - Added spinning vinyl animation to Phono/Vinyl mode with RPM indicator
  - Enhanced NowPlayingPanel Radio Mode with "Advanced Radio Controls" button
  - Phase 2B (Radio Demo Integration) completed
  - Phase 4 (Rich Audio Panels) partially completed
  - 176 tests passing, build successful
- **2025-11-21 Phase 3 Implementation:**
  - Enhanced ConfigurationPanel with tabbed interface (General, Device, Advanced)
  - Enhanced SystemStatusPanel with real-time charts, network monitoring, device status
  - Enhanced AlertManagementPanel with test buttons, volume controls, history viewer
  - All enhancements include Material Design 3 styling and real-time updates
  - 176 tests passing, build successful with 10 non-blocking analyzer warnings
- **2025-11-21 Phase 2 Completion:**
  - Phase 2 Panel Management & Full-Page Conversion completed
  - PanelService implemented with comprehensive CSS animations
  - Three new Type B slide-out panels created (Configuration, SystemStatus, AlertManagement)
  - NavMenu.razor removed, all navigation moved to GlobalHeader
  - Deprecated full-page routes (RadioDemo, SystemPanel) removed and archived
  - All tests passing (154 tests at completion), 3-panel layout specification fully compliant
