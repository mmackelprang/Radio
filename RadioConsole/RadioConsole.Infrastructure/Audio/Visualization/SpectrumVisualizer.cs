using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Infrastructure.Audio.Visualization;

/// <summary>
/// Spectrum visualizer displaying frequency distribution using FFT.
/// Based on SoundFlow's SpectrumAnalyzer example.
/// Note: This is a simplified implementation. For production use, integrate
/// a proper FFT library like MathNet.Numerics or Kiss FFT.
/// </summary>
public class SpectrumVisualizer : IVisualizer
{
  private readonly ILogger<SpectrumVisualizer>? _logger;
  private readonly float[] _spectrumData;
  private readonly int _binCount;

  public event EventHandler? VisualizationUpdated;
  public VisualizationType Type => VisualizationType.Spectrum;

  public SpectrumVisualizer(ILogger<SpectrumVisualizer>? logger = null, int binCount = 64)
  {
    _logger = logger;
    _binCount = binCount;
    _spectrumData = new float[_binCount];
  }

  public void ProcessAudioData(ReadOnlySpan<float> audioData)
  {
    if (audioData.IsEmpty)
    {
      return;
    }

    // NOTE: This is a placeholder implementation
    // For real FFT analysis, integrate a library like:
    // - MathNet.Numerics (managed, cross-platform)
    // - Kiss FFT (native, requires P/Invoke wrapper)
    // - Implement your own FFT algorithm
    
    // For now, we'll simulate spectrum data based on audio energy in different ranges
    // This gives a rough approximation but is NOT a real FFT
    
    int samplesPerBin = Math.Max(1, audioData.Length / _binCount);
    
    for (int i = 0; i < _binCount; i++)
    {
      int startIdx = i * samplesPerBin;
      int endIdx = Math.Min(startIdx + samplesPerBin, audioData.Length);
      
      // Calculate energy in this frequency band (rough approximation)
      float energy = 0f;
      for (int j = startIdx; j < endIdx; j++)
      {
        energy += Math.Abs(audioData[j]);
      }
      
      energy /= (endIdx - startIdx);
      
      // Apply smoothing (exponential moving average)
      float smoothingFactor = 0.3f;
      _spectrumData[i] = (_spectrumData[i] * (1 - smoothingFactor)) + (energy * smoothingFactor);
    }

    VisualizationUpdated?.Invoke(this, EventArgs.Empty);
  }

  public void Render(IVisualizationContext context)
  {
    if (context == null)
    {
      return;
    }

    context.Clear();

    float width = context.Width;
    float height = context.Height;
    
    // Calculate bar dimensions
    float barWidth = (width / _binCount) * 0.8f;
    float barGap = (width / _binCount) * 0.2f;

    // Draw spectrum bars
    for (int i = 0; i < _binCount; i++)
    {
      float magnitude = _spectrumData[i];
      float barHeight = magnitude * height * 0.9f;
      
      float x = i * (barWidth + barGap);
      float y = height - barHeight;

      // Color based on frequency (low = red, mid = green, high = blue)
      VisualizationColor color;
      float normalizedFreq = (float)i / _binCount;
      
      if (normalizedFreq < 0.33f)
      {
        // Low frequencies - Red to Yellow
        color = new VisualizationColor(1f, normalizedFreq * 3f, 0f, 1f);
      }
      else if (normalizedFreq < 0.66f)
      {
        // Mid frequencies - Yellow to Green
        float t = (normalizedFreq - 0.33f) * 3f;
        color = new VisualizationColor(1f - t, 1f, 0f, 1f);
      }
      else
      {
        // High frequencies - Green to Blue
        float t = (normalizedFreq - 0.66f) * 3f;
        color = new VisualizationColor(0f, 1f - t, t, 1f);
      }

      context.DrawRectangle(x, y, barWidth, barHeight, color);

      // Add glow effect for higher magnitudes
      if (magnitude > 0.6f)
      {
        context.DrawRectangle(x, y, barWidth, barHeight * 0.1f, VisualizationColor.White);
      }
    }
  }

  public void Dispose()
  {
    Array.Clear(_spectrumData, 0, _spectrumData.Length);
  }
}
