# Audio Visualization System

This document describes the audio visualization system implemented in the Radio Console application, based on the SoundFlow library examples.

> **📸 UI Screenshots & Visuals:** See [UI Visualization Guide](docs/UI_VISUALIZATION_GUIDE.md) for visual representations and UI mockups of all three visualization types.

## Architecture Overview

The visualization system follows a layered architecture with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer (Web)                 │
│  - VisualizationPanel.razor (UI Component)                  │
│  - BlazorVisualizationContext (JavaScript Interop)          │
│  - visualizer.js (Canvas Rendering)                         │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────┴────────────────────────────────────────┐
│                   Application Layer (Core)                   │
│  - IVisualizer (Visualizer Contract)                        │
│  - IVisualizationContext (Drawing Primitives)               │
│  - IVisualizationService (Data Broadcasting)                │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────┴────────────────────────────────────────┐
│               Infrastructure Layer (Implementation)          │
│  - LevelMeterVisualizer                                      │
│  - WaveformVisualizer                                        │
│  - SpectrumVisualizer                                        │
│  - SignalRVisualizationService                              │
└──────────────────────────────────────────────────────────────┘
```

## Visualization Types

### 1. Level Meter
Displays real-time RMS (Root Mean Square) and peak audio levels.

**Features:**
- Dual bar display (RMS and Peak)
- Color-coded intensity (Green → Yellow → Red)
- Peak hold marker with 1.5 second decay
- Scale markers at 0%, 50%, 100%

**Use Cases:**
- Monitoring audio levels
- Preventing clipping
- Visual feedback for mixing

**Implementation:** `RadioConsole.Infrastructure.Audio.Visualization.LevelMeterVisualizer`

### 2. Waveform
Shows audio samples over time as a continuous waveform.

**Features:**
- Real-time waveform display
- Color-coded amplitude (Blue → Green → Yellow)
- Circular buffer (1024 samples default)
- Smooth line rendering

**Use Cases:**
- Visualizing audio content
- Detecting silence or clipping
- Audio editing reference

**Implementation:** `RadioConsole.Infrastructure.Audio.Visualization.WaveformVisualizer`

### 3. Spectrum Analyzer
Displays frequency distribution using FFT analysis.

**Features:**
- 64 frequency bins (configurable)
- Color gradient based on frequency (Low=Red, Mid=Green, High=Blue)
- Smoothed bar display with exponential moving average
- Glow effect for high magnitudes

**Use Cases:**
- Analyzing frequency content
- EQ adjustment reference
- Music visualization

**Implementation:** `RadioConsole.Infrastructure.Audio.Visualization.SpectrumVisualizer`

**Note:** Current implementation uses a simplified frequency approximation. For production use, integrate a proper FFT library like MathNet.Numerics or Kiss FFT.

## Core Interfaces

### IVisualizationContext
Provides drawing primitives abstracted from the UI framework.

```csharp
public interface IVisualizationContext
{
  void Clear();
  void DrawLine(float x1, float y1, float x2, float y2, VisualizationColor color, float thickness = 1f);
  void DrawRectangle(float x, float y, float width, float height, VisualizationColor color);
  float Width { get; }
  float Height { get; }
}
```

**Implementations:**
- `BlazorVisualizationContext`: Uses JavaScript interop to render on HTML5 canvas

### IVisualizer
Defines the contract for all audio visualizers.

```csharp
public interface IVisualizer : IDisposable
{
  event EventHandler? VisualizationUpdated;
  void Render(IVisualizationContext context);
  void ProcessAudioData(ReadOnlySpan<float> audioData);
  VisualizationType Type { get; }
}
```

### IVisualizationService
Broadcasts visualization data to connected clients (used by audio player).

```csharp
public interface IVisualizationService
{
  Task SendFFTDataAsync(float[] fftData, CancellationToken cancellationToken = default);
}
```

**Implementations:**
- `SignalRVisualizationService`: Broadcasts via SignalR to all connected web clients

## Usage

### In Blazor UI

The `VisualizationPanel` component provides a dropdown to select visualization type:

```razor
<MudSelect T="VisualizationType" Label="Visualization Type" 
           Value="@selectedVisualizationType" 
           ValueChanged="@OnVisualizationTypeChanged">
  <MudSelectItem Value="@VisualizationType.Spectrum">Spectrum</MudSelectItem>
  <MudSelectItem Value="@VisualizationType.Waveform">Waveform</MudSelectItem>
  <MudSelectItem Value="@VisualizationType.LevelMeter">Level Meter</MudSelectItem>
</MudSelect>
```

### Creating a Custom Visualizer

1. Implement the `IVisualizer` interface:

```csharp
public class CustomVisualizer : IVisualizer
{
  public event EventHandler? VisualizationUpdated;
  public VisualizationType Type => VisualizationType.Custom;

  public void ProcessAudioData(ReadOnlySpan<float> audioData)
  {
    // Process audio samples
    // Update internal state
    VisualizationUpdated?.Invoke(this, EventArgs.Empty);
  }

  public void Render(IVisualizationContext context)
  {
    context.Clear();
    // Draw visualization using context methods
    context.DrawLine(...);
    context.DrawRectangle(...);
  }

  public void Dispose() { /* Cleanup */ }
}
```

2. Register with dependency injection (if needed)
3. Integrate with audio player to receive audio samples

## JavaScript Rendering

The `visualizer.js` file handles canvas rendering in the browser:

### Key Functions

- `initializeVisualizer(canvasElement)`: Initialize canvas and start animation loop
- `setVisualizationType(type)`: Switch visualization mode
- `updateVisualizerData(fftData)`: Update FFT data from SignalR
- `renderVisualizationCommands(commands)`: Execute custom drawing commands
- `disposeVisualizer()`: Cleanup resources

### Drawing Commands

Custom visualizers can send drawing commands from C#:

```javascript
{
  type: "line",
  x1: 10, y1: 20,
  x2: 30, y2: 40,
  color: "#FF5722",
  thickness: 2
}

{
  type: "rectangle",
  x: 10, y: 20,
  width: 50, height: 30,
  color: "#4CAF50"
}
```

## Configuration

Visualization settings can be added to `appsettings.json`:

```json
{
  "RadioConsole": {
    "Visualization": {
      "UpdateIntervalMs": 50,
      "SmoothingFactor": 0.3,
      "SpectrumBinCount": 64,
      "WaveformBufferSize": 1024
    }
  }
}
```

## Performance Considerations

1. **Update Rate**: Default 50ms (20 FPS) balances smoothness with CPU usage
2. **Canvas Size**: Automatically resizes to container, optimal for 12.5" x 3.75" display
3. **Smoothing**: Exponential moving average reduces jitter in spectrum analyzer
4. **Buffer Size**: Waveform uses circular buffer to limit memory usage

## Future Enhancements

### Short Term
- [ ] Connect visualizers to real audio player data
- [ ] Implement proper FFT using MathNet.Numerics or Kiss FFT
- [ ] Add configuration options for each visualizer
- [ ] Implement visualizer hot-swapping without restart

### Long Term
- [ ] Add more visualization types (VU meter, oscilloscope, spectrogram)
- [ ] Support for stereo visualization (L/R channels)
- [ ] Customizable color schemes and themes
- [ ] GPU-accelerated rendering for complex visualizations
- [ ] Recording/screenshot capability

## References

- **[UI Visualization Guide](docs/UI_VISUALIZATION_GUIDE.md)** - Visual mockups and UI examples
- **[Code Review](CODE_REVIEW.md)** - Comprehensive code review findings and recommendations
- SoundFlow Documentation: https://lsxprime.github.io/soundflow-docs/
- Example Code: `/examples/ConsoleLevelMeter.cs`, `ConsoleWaveform.cs`, `ConsoleSpectrum.cs`
- WPF Examples: `/examples/WaveformVisualizer.cs`, `WaveformRenderer.cs`
- HTML5 Canvas API: https://developer.mozilla.org/en-US/docs/Web/API/Canvas_API

## Troubleshooting

### Visualization not updating
- Check SignalR connection in browser console
- Verify `AudioPlayer.EnableFftDataGeneration(true)` is called
- Ensure audio is playing

### Performance issues
- Reduce update interval in configuration
- Lower FFT bin count (e.g., 32 instead of 64)
- Check browser console for JavaScript errors

### Colors not displaying correctly
- Verify `VisualizationColor.ToHex()` format is correct
- Check canvas context settings in visualizer.js
- Ensure color values are normalized (0-1 range)
