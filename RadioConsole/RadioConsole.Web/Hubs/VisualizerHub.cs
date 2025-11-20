using Microsoft.AspNetCore.SignalR;

namespace RadioConsole.Web.Hubs;

/// <summary>
/// SignalR Hub for pushing real-time FFT audio visualization data to clients.
/// The audio player pushes FFT data every 50ms to connected clients.
/// </summary>
public class VisualizerHub : Hub
{
  private readonly ILogger<VisualizerHub> _logger;

  public VisualizerHub(ILogger<VisualizerHub> logger)
  {
    _logger = logger;
  }

  public override async Task OnConnectedAsync()
  {
    _logger.LogInformation("Client connected to VisualizerHub: {ConnectionId}", Context.ConnectionId);
    await base.OnConnectedAsync();
  }

  public override async Task OnDisconnectedAsync(Exception? exception)
  {
    _logger.LogInformation("Client disconnected from VisualizerHub: {ConnectionId}", Context.ConnectionId);
    if (exception != null)
    {
      _logger.LogError(exception, "Client disconnected with error");
    }
    await base.OnDisconnectedAsync(exception);
  }

  /// <summary>
  /// Broadcasts FFT data to all connected clients.
  /// This method is called by the audio player service.
  /// </summary>
  /// <param name="fftData">Array of FFT magnitude values (frequency bins).</param>
  public async Task SendFFTData(float[] fftData)
  {
    await Clients.All.SendAsync("ReceiveFFTData", fftData);
  }
}
