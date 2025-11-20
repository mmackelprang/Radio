using RadioConsole.Core.Interfaces.Audio;
using Microsoft.JSInterop;

namespace RadioConsole.Web.Services;

/// <summary>
/// Blazor implementation of IVisualizationContext using JavaScript interop.
/// Sends drawing commands to the JavaScript canvas renderer.
/// </summary>
public class BlazorVisualizationContext : IVisualizationContext
{
  private readonly IJSRuntime _jsRuntime;
  private readonly List<DrawCommand> _commands;
  private float _width;
  private float _height;

  public float Width => _width;
  public float Height => _height;

  public BlazorVisualizationContext(IJSRuntime jsRuntime, float width, float height)
  {
    _jsRuntime = jsRuntime;
    _width = width;
    _height = height;
    _commands = new List<DrawCommand>();
  }

  public void SetDimensions(float width, float height)
  {
    _width = width;
    _height = height;
  }

  public void Clear()
  {
    _commands.Clear();
  }

  public void DrawLine(float x1, float y1, float x2, float y2, VisualizationColor color, float thickness = 1f)
  {
    _commands.Add(new DrawCommand
    {
      Type = "line",
      X1 = x1,
      Y1 = y1,
      X2 = x2,
      Y2 = y2,
      Color = color.ToHex(),
      Thickness = thickness
    });
  }

  public void DrawRectangle(float x, float y, float width, float height, VisualizationColor color)
  {
    _commands.Add(new DrawCommand
    {
      Type = "rectangle",
      X = x,
      Y = y,
      Width = width,
      Height = height,
      Color = color.ToHex()
    });
  }

  /// <summary>
  /// Execute all drawing commands via JavaScript interop.
  /// </summary>
  public async Task ExecuteCommandsAsync()
  {
    if (_commands.Count > 0)
    {
      await _jsRuntime.InvokeVoidAsync("renderVisualizationCommands", _commands);
    }
  }

  private class DrawCommand
  {
    public string Type { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float X1 { get; set; }
    public float Y1 { get; set; }
    public float X2 { get; set; }
    public float Y2 { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public string Color { get; set; } = string.Empty;
    public float Thickness { get; set; }
  }
}
