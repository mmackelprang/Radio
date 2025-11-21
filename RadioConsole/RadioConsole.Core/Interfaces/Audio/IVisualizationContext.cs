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
public readonly struct VisualizationColor
{
  public float R { get; init; }
  public float G { get; init; }
  public float B { get; init; }
  public float A { get; init; }

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
  /// Values are clamped to [0, 1] range before conversion.
  /// </summary>
  public string ToHex()
  {
    int r = (int)(Math.Clamp(R, 0f, 1f) * 255);
    int g = (int)(Math.Clamp(G, 0f, 1f) * 255);
    int b = (int)(Math.Clamp(B, 0f, 1f) * 255);
    int a = (int)(Math.Clamp(A, 0f, 1f) * 255);
    return $"#{r:X2}{g:X2}{b:X2}{a:X2}";
  }

  /// <summary>
  /// Parse a hex color string to create a VisualizationColor.
  /// Supports formats: #RGB, #RGBA, #RRGGBB, #RRGGBBAA
  /// </summary>
  /// <param name="hex">Hex color string (e.g., "#FF0000" or "#FF0000FF")</param>
  /// <returns>Parsed color</returns>
  /// <exception cref="ArgumentException">Thrown if hex format is invalid</exception>
  public static VisualizationColor FromHex(string hex)
  {
    if (string.IsNullOrEmpty(hex) || hex[0] != '#')
      throw new ArgumentException("Invalid hex color format. Must start with #", nameof(hex));

    hex = hex.TrimStart('#');

    if (hex.Length != 6 && hex.Length != 8)
      throw new ArgumentException("Hex color must be 6 or 8 characters (RGB or RGBA)", nameof(hex));

    int r = Convert.ToInt32(hex.Substring(0, 2), 16);
    int g = Convert.ToInt32(hex.Substring(2, 2), 16);
    int b = Convert.ToInt32(hex.Substring(4, 2), 16);
    int a = hex.Length == 8 ? Convert.ToInt32(hex.Substring(6, 2), 16) : 255;

    return new VisualizationColor(r / 255f, g / 255f, b / 255f, a / 255f);
  }
}
