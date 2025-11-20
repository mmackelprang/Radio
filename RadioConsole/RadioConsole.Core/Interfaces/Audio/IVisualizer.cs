namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Interface for audio visualizers.
/// Visualizers process audio data and render visual representations.
/// </summary>
public interface IVisualizer : IDisposable
{
  /// <summary>
  /// Event raised when visualization data has been updated.
  /// UI components should subscribe to this event to trigger redraws.
  /// </summary>
  event EventHandler? VisualizationUpdated;

  /// <summary>
  /// Render the visualization to the provided context.
  /// </summary>
  /// <param name="context">The rendering context</param>
  void Render(IVisualizationContext context);

  /// <summary>
  /// Process audio data for visualization.
  /// </summary>
  /// <param name="audioData">Audio sample data</param>
  void ProcessAudioData(ReadOnlySpan<float> audioData);

  /// <summary>
  /// Get the type of this visualizer.
  /// </summary>
  VisualizationType Type { get; }
}

/// <summary>
/// Types of audio visualizations supported.
/// </summary>
public enum VisualizationType
{
  /// <summary>
  /// Level meter showing RMS and peak levels.
  /// </summary>
  LevelMeter,

  /// <summary>
  /// Waveform display showing audio samples over time.
  /// </summary>
  Waveform,

  /// <summary>
  /// Spectrum analyzer showing frequency distribution.
  /// </summary>
  Spectrum
}
