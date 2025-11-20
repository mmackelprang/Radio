namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Service for broadcasting FFT visualization data to connected clients.
/// This abstraction allows the Infrastructure layer to push visualization data
/// without depending on the Web layer's SignalR implementation.
/// </summary>
public interface IVisualizationService
{
  /// <summary>
  /// Sends FFT data to all connected clients for visualization.
  /// </summary>
  /// <param name="fftData">Array of FFT magnitude values (normalized 0-1)</param>
  /// <param name="cancellationToken">Cancellation token</param>
  Task SendFFTDataAsync(float[] fftData, CancellationToken cancellationToken = default);
}
