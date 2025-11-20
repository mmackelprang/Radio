using Microsoft.AspNetCore.SignalR;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Web.Hubs;

namespace RadioConsole.Web.Services;

/// <summary>
/// SignalR-based implementation of IVisualizationService.
/// Broadcasts FFT data to all connected clients via the VisualizerHub.
/// </summary>
public class SignalRVisualizationService : IVisualizationService
{
  private readonly IHubContext<VisualizerHub> _hubContext;
  private readonly ILogger<SignalRVisualizationService> _logger;

  public SignalRVisualizationService(
    IHubContext<VisualizerHub> hubContext,
    ILogger<SignalRVisualizationService> logger)
  {
    _hubContext = hubContext;
    _logger = logger;
  }

  public async Task SendFFTDataAsync(float[] fftData, CancellationToken cancellationToken = default)
  {
    try
    {
      await _hubContext.Clients.All.SendAsync("ReceiveFFTData", fftData, cancellationToken);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to send FFT data via SignalR");
    }
  }
}
