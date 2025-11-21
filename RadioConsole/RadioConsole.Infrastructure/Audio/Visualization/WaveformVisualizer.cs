using RadioConsole.Core.Configuration;
using RadioConsole.Core.Interfaces.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RadioConsole.Infrastructure.Audio.Visualization;

/// <summary>
/// Waveform visualizer displaying audio samples over time.
/// Based on SoundFlow's WaveformVisualizer example.
/// </summary>
public class WaveformVisualizer : IVisualizer
{
  private readonly ILogger<WaveformVisualizer>? _logger;
  private readonly List<float> _waveformBuffer;
  private readonly int _bufferSize;

  public event EventHandler? VisualizationUpdated;
  public VisualizationType Type => VisualizationType.Waveform;

  public WaveformVisualizer(
    ILogger<WaveformVisualizer>? logger = null, 
    IOptions<AudioVisualizationOptions>? options = null)
  {
    _logger = logger;
    _bufferSize = options?.Value.WaveformBufferSize ?? 1024;
    _waveformBuffer = new List<float>(_bufferSize);
  }

  public void ProcessAudioData(ReadOnlySpan<float> audioData)
  {
    if (audioData.IsEmpty)
    {
      _logger?.LogDebug("ProcessAudioData called with empty data");
      return;
    }

    // Add samples to buffer, keeping only the most recent samples
    for (int i = 0; i < audioData.Length; i++)
    {
      _waveformBuffer.Add(audioData[i]);
      if (_waveformBuffer.Count > _bufferSize)
      {
        _waveformBuffer.RemoveAt(0);
      }
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

    if (_waveformBuffer.Count == 0)
    {
      _logger?.LogDebug("Render called with empty waveform buffer");
      return;
    }

    _logger?.LogDebug("Rendering waveform with {Count} samples", _waveformBuffer.Count);

    context.Clear();

    float width = context.Width;
    float height = context.Height;
    float centerY = height / 2;

    // Draw center line
    context.DrawLine(0, centerY, width, centerY, VisualizationColor.Gray, 1f);

    // Draw waveform
    float xStep = width / _waveformBuffer.Count;
    
    for (int i = 0; i < _waveformBuffer.Count - 1; i++)
    {
      float x1 = i * xStep;
      float x2 = (i + 1) * xStep;
      
      // Scale samples to fit in canvas height (-1 to 1 range maps to full height)
      float y1 = centerY - (_waveformBuffer[i] * height * 0.4f);
      float y2 = centerY - (_waveformBuffer[i + 1] * height * 0.4f);

      // Color based on amplitude
      float amplitude = Math.Abs(_waveformBuffer[i]);
      VisualizationColor color = amplitude < 0.5f ? VisualizationColor.Blue :
                                  amplitude < 0.75f ? VisualizationColor.Green :
                                  VisualizationColor.Yellow;

      context.DrawLine(x1, y1, x2, y2, color, 2f);
    }
  }

  public void Dispose()
  {
    _waveformBuffer.Clear();
  }
}
