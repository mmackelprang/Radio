using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio.Visualization;

/// <summary>
/// Level meter visualizer displaying RMS and peak audio levels.
/// Based on SoundFlow's LevelMeterAnalyzer example.
/// </summary>
public class LevelMeterVisualizer : IVisualizer
{
  private readonly ILogger<LevelMeterVisualizer>? _logger;
  private float _rms;
  private float _peak;
  private float _peakHold;
  private DateTime _peakHoldTime;
  private const float PeakHoldDuration = 1.5f; // seconds

  public event EventHandler? VisualizationUpdated;
  public VisualizationType Type => VisualizationType.LevelMeter;

  public LevelMeterVisualizer(ILogger<LevelMeterVisualizer>? logger = null)
  {
    _logger = logger;
    _peakHoldTime = DateTime.UtcNow;
  }

  public void ProcessAudioData(ReadOnlySpan<float> audioData)
  {
    if (audioData.IsEmpty)
    {
      _logger?.LogDebug("ProcessAudioData called with empty data");
      return;
    }

    // Calculate RMS (Root Mean Square) - average loudness
    float sumSquares = 0f;
    float maxSample = 0f;

    for (int i = 0; i < audioData.Length; i++)
    {
      float sample = Math.Abs(audioData[i]);
      sumSquares += sample * sample;
      maxSample = Math.Max(maxSample, sample);
    }

    _rms = (float)Math.Sqrt(sumSquares / audioData.Length);
    _peak = maxSample;

    // Update peak hold
    if (_peak > _peakHold)
    {
      _peakHold = _peak;
      _peakHoldTime = DateTime.UtcNow;
    }
    else if ((DateTime.UtcNow - _peakHoldTime).TotalSeconds > PeakHoldDuration)
    {
      _peakHold = _peak;
    }

    VisualizationUpdated?.Invoke(this, EventArgs.Empty);
  }

  public void Render(IVisualizationContext context)
  {
    if (context == null)
    {
      _logger?.LogWarning("Render called with null context");
      return;
    }

    _logger?.LogDebug("Rendering level meter: RMS={Rms}, Peak={Peak}", _rms, _peak);

    context.Clear();

    float width = context.Width;
    float height = context.Height;
    float centerY = height / 2;
    float barHeight = height * 0.3f;
    float margin = 20f;

    // Draw RMS bar
    float rmsBarWidth = _rms * (width - 2 * margin);
    float rmsY = centerY - barHeight - 10;
    
    // RMS bar color based on level
    VisualizationColor rmsColor = _rms < 0.5f ? VisualizationColor.Green :
                                   _rms < 0.75f ? VisualizationColor.Yellow :
                                   VisualizationColor.Red;

    context.DrawRectangle(margin, rmsY, rmsBarWidth, barHeight, rmsColor);

    // Draw Peak bar
    float peakBarWidth = _peak * (width - 2 * margin);
    float peakY = centerY + 10;
    
    VisualizationColor peakColor = _peak < 0.5f ? VisualizationColor.Green :
                                    _peak < 0.75f ? VisualizationColor.Yellow :
                                    VisualizationColor.Red;

    context.DrawRectangle(margin, peakY, peakBarWidth, barHeight, peakColor);

    // Draw peak hold marker
    if (_peakHold > 0)
    {
      float peakHoldX = margin + _peakHold * (width - 2 * margin);
      context.DrawLine(peakHoldX, peakY, peakHoldX, peakY + barHeight, VisualizationColor.White, 3f);
    }

    // Draw scale markers (0%, 50%, 100%)
    context.DrawLine(margin, 0, margin, height, VisualizationColor.Gray, 1f);
    context.DrawLine(margin + (width - 2 * margin) * 0.5f, 0, margin + (width - 2 * margin) * 0.5f, height, VisualizationColor.Gray, 1f);
    context.DrawLine(width - margin, 0, width - margin, height, VisualizationColor.Gray, 1f);
  }

  public void Dispose()
  {
    // No resources to dispose
  }
}
