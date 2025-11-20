namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Interface for visualization rendering context.
/// This abstraction allows different UI frameworks (WPF, Blazor, etc.) to implement
/// their own rendering mechanisms while using the same visualization logic.
/// </summary>
public interface IVisualizationContext
{
  /// <summary>
  /// Clear the visualization canvas/drawing surface.
  /// </summary>
  void Clear();

  /// <summary>
  /// Draw a line between two points.
  /// </summary>
  /// <param name="x1">Start X coordinate</param>
  /// <param name="y1">Start Y coordinate</param>
  /// <param name="x2">End X coordinate</param>
  /// <param name="y2">End Y coordinate</param>
  /// <param name="color">Line color</param>
  /// <param name="thickness">Line thickness</param>
  void DrawLine(float x1, float y1, float x2, float y2, VisualizationColor color, float thickness = 1f);

  /// <summary>
  /// Draw a filled rectangle.
  /// </summary>
  /// <param name="x">X coordinate of top-left corner</param>
  /// <param name="y">Y coordinate of top-left corner</param>
  /// <param name="width">Rectangle width</param>
  /// <param name="height">Rectangle height</param>
  /// <param name="color">Fill color</param>
  void DrawRectangle(float x, float y, float width, float height, VisualizationColor color);

  /// <summary>
  /// Get the width of the visualization canvas.
  /// </summary>
  float Width { get; }

  /// <summary>
  /// Get the height of the visualization canvas.
  /// </summary>
  float Height { get; }
}

/// <summary>
/// Color representation for visualizations.
/// Values are normalized between 0 and 1.
/// </summary>
public struct VisualizationColor
{
  public float R { get; set; }
  public float G { get; set; }
  public float B { get; set; }
  public float A { get; set; }

  public VisualizationColor(float r, float g, float b, float a = 1f)
  {
    R = r;
    G = g;
    B = b;
    A = a;
  }

  // Common colors
  public static VisualizationColor Green => new(0.3f, 0.69f, 0.31f, 1f); // #4caf50
  public static VisualizationColor Yellow => new(1f, 0.76f, 0.03f, 1f); // #ffc107
  public static VisualizationColor Red => new(0.96f, 0.26f, 0.21f, 1f); // #f44336
  public static VisualizationColor Blue => new(0.13f, 0.59f, 0.95f, 1f); // #2196f3
  public static VisualizationColor White => new(1f, 1f, 1f, 1f);
  public static VisualizationColor Gray => new(0.5f, 0.5f, 0.5f, 1f);

  /// <summary>
  /// Convert to hex color string for web rendering.
  /// </summary>
  public string ToHex()
  {
    int r = (int)(R * 255);
    int g = (int)(G * 255);
    int b = (int)(B * 255);
    int a = (int)(A * 255);
    return $"#{r:X2}{g:X2}{b:X2}{a:X2}";
  }
}
