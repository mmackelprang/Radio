namespace RadioConsole.Core.Configuration;

/// <summary>
/// Configuration options for audio visualization.
/// </summary>
public class AudioVisualizationOptions
{
  /// <summary>
  /// Buffer size for waveform visualizer (number of samples to display).
  /// </summary>
  public int WaveformBufferSize { get; set; } = 1024;

  /// <summary>
  /// FFT size for spectrum analyzer (must be power of 2).
  /// </summary>
  public int SpectrumFftSize { get; set; } = 512;

  /// <summary>
  /// Number of frequency bands to display in spectrum visualizer.
  /// </summary>
  public int SpectrumBands { get; set; } = 64;

  /// <summary>
  /// Visualization update frequency in Hz.
  /// </summary>
  public int UpdateFrequencyHz { get; set; } = 30;
}
