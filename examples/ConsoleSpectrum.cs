using SoundFlow.Abstracts;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Interfaces;
using SoundFlow.Providers;
using SoundFlow.Structs;
using SoundFlow.Visualization;
using System;
using System.IO;
using System.Linq;

namespace SpectrumAnalyzerVisualization;

internal static class Program
{
    private static void Main(string[] args)
    {
        // Standard setup.
        using var audioEngine = new MiniAudioEngine();
        var defaultDevice = audioEngine.PlaybackDevices.FirstOrDefault(d => d.IsDefault);
        if(defaultDevice.Id == IntPtr.Zero) return;
        var audioFormat = new AudioFormat { Format = SampleFormat.F32, SampleRate = 48000, Channels = 2 };
        using var device = audioEngine.InitializePlaybackDevice(defaultDevice, audioFormat);
        
        using var dataProvider = new StreamDataProvider(audioEngine, audioFormat, File.OpenRead("path/to/your/audiofile.wav"));
        using var player = new SoundPlayer(audioEngine, audioFormat, dataProvider);

        // Create the SpectrumAnalyzer and SpectrumVisualizer.
        var spectrumAnalyzer = new SpectrumAnalyzer(audioFormat, fftSize: 2048);
        var spectrumVisualizer = new SpectrumVisualizer(spectrumAnalyzer);

        // Attach the analyzer to the player.
        player.AddAnalyzer(spectrumAnalyzer);
        
        device.MasterMixer.AddComponent(player);
        
        device.Start();
        player.Play();

        // Subscribe to the VisualizationUpdated event to trigger a redraw.
        spectrumVisualizer.VisualizationUpdated += (sender, e) =>
        {
            DrawSpectrum(spectrumAnalyzer.SpectrumData);
        };

        // Start a timer to update the visualization.
        var timer = new System.Timers.Timer(1000 / 60); // Update at approximately 60 FPS
        timer.Elapsed += (sender, e) =>
        {
            spectrumVisualizer.ProcessOnAudioData(Array.Empty<float>());
            spectrumVisualizer.Render(new ConsoleVisualizationContext());
        };
        timer.Start();

        // Keep the console application running until the user presses a key.
        Console.WriteLine("Playing audio and displaying spectrum analyzer... Press any key to stop.");
        Console.ReadKey();

        device.Stop();
        spectrumVisualizer.Dispose();
    }

    // Helper method to draw a simple console-based spectrum analyzer.
    private static void DrawSpectrum(ReadOnlySpan<float> spectrumData)
    {
        Console.Clear();
        int consoleWidth = Console.WindowWidth;
        int consoleHeight = Console.WindowHeight -1;

        if (spectrumData.IsEmpty) return;

        for (int i = 0; i < consoleWidth; i++)
        {
            // Logarithmic mapping of frequency bins to console columns for better visualization
            double logIndex = Math.Log10(1 + 9 * ((double)i / consoleWidth));
            int spectrumIndex = (int)(logIndex * (spectrumData.Length - 1));
            
            float magnitude = spectrumData[spectrumIndex];
            int barHeight = (int)(magnitude * consoleHeight);
            barHeight = Math.Clamp(barHeight, 0, consoleHeight);

            for (int j = 0; j < barHeight; j++)
            {
                Console.SetCursorPosition(i, consoleHeight - 1 - j);
                Console.Write("█");
            }
        }
        Console.SetCursorPosition(0, consoleHeight - 1);
    }
}

// Simple IVisualizationContext implementation for console output.
public class ConsoleVisualizationContext : IVisualizationContext
{
    public void Clear() { }
    public void DrawLine(float x1, float y1, float x2, float y2, Color color, float thickness = 1f) { }
    public void DrawRectangle(float x, float y, float width, float height, Color color) { }
}