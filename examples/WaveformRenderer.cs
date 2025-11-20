// In your XAML:
// <Canvas x:Name="VisualizationCanvas"/>

// In your code-behind:
public partial class MainWindow : Window
{
    private readonly WaveformVisualizer _visualizer;

    public MainWindow()
    {
        InitializeComponent();

        // ... Initialize AudioEngine, SoundPlayer, etc. ...

        _visualizer = new WaveformVisualizer();
        _visualizer.VisualizationUpdated += OnVisualizationUpdated;

        // ...
    }

    private void OnVisualizationUpdated(object? sender, EventArgs e)
    {
        // Marshal the update to the UI thread
        Dispatcher.Invoke(() =>
        {
            VisualizationCanvas.Children.Clear(); // Clear previous drawing

            // Create a custom IVisualizationContext that wraps the Canvas
            var context = new WpfVisualizationContext(VisualizationCanvas);

            // Render the visualization
            _visualizer.Render(context);
        });
    }

    // ...
}

// IVisualizationContext implementation for WPF
public class WpfVisualizationContext : IVisualizationContext
{
    private readonly Canvas _canvas;

    public WpfVisualizationContext(Canvas canvas)
    {
        _canvas = canvas;
    }

    public void Clear()
    {
        _canvas.Children.Clear();
    }

    public void DrawLine(float x1, float y1, float x2, float y2, SoundFlow.Interfaces.Color color, float thickness = 1f)
    {
        var line = new Line
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(color.A * 255), (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255))),
            StrokeThickness = thickness
        };
        _canvas.Children.Add(line);
    }

    public void DrawRectangle(float x, float y, float width, float height, SoundFlow.Interfaces.Color color)
    {
        var rect = new Rectangle
        {
            Width = width,
            Height = height,
            Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(color.A * 255), (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255)))
        };
        Canvas.SetLeft(rect, x);
        Canvas.SetTop(rect, y);
        _canvas.Children.Add(rect);
    }
}