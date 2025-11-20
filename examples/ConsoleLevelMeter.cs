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

namespace LevelMeterVisualization;

internal static class Program
{
    private static void Main(string[] args)
    {
        // Standard engine and device setup.
        using var audioEngine = new MiniAudioEngine();
        var defaultDevice = audioEngine.PlaybackDevices.FirstOrDefault(d => d.IsDefault);
        if(defaultDevice.Id == IntPtr.Zero) return;
        var audioFormat = new AudioFormat { Format = SampleFormat.F32, SampleRate = 48000, Channels = 2 };
        using var device = audioEngine.InitializePlaybackDevice(defaultDevice, audioFormat);
        
        // Create a player for an audio file.
        using var dataProvider = new StreamDataProvider(audioEngine, audioFormat, File.OpenRead("path/to/your/audiofile.wav"));
        using var player = new SoundPlayer(audioEngine, audioFormat, dataProvider);

        // Create the LevelMeterAnalyzer and LevelMeterVisualizer.
        // The visualizer is linked to the analyzer.
        var levelMeterAnalyzer = new LevelMeterAnalyzer(audioFormat);
        var levelMeterVisualizer = new LevelMeterVisualizer(levelMeterAnalyzer);
        
        // Attach the analyzer to the player. The analyzer will automatically
        // pass its data to the linked visualizer.
        player.AddAnalyzer(levelMeterAnalyzer);

        // Add the player to the mixer.
        device.MasterMixer.AddComponent(player);
        
        // Start playback.
        device.Start();
        player.Play();

        // Subscribe to the VisualizationUpdated event to trigger a redraw.
        levelMeterVisualizer.VisualizationUpdated += (sender, e) =>
        {
            DrawLevelMeter(levelMeterAnalyzer.Rms, levelMeterAnalyzer.Peak);
        };

        // Start a timer to update the visualization.
        var timer = new System.Timers.Timer(1000 / 60); // Update at approximately 60 FPS
        timer.Elapsed += (sender, e) =>
        {
            levelMeterVisualizer.ProcessOnAudioData(System.Array.Empty<float>());
            levelMeterVisualizer.Render(new ConsoleVisualizationContext());
        };
        timer.Start();

        // Keep the console application running until the user presses a key.
        Console.WriteLine("Playing audio and displaying level meter... Press any key to stop.");
        Console.ReadKey();

        device.Stop();
        levelMeterVisualizer.Dispose();
    }

    // Helper method to draw a simple console-based level meter.
    private static void DrawLevelMeter(float rms, float peak)
    {
        int barLength = (int)(rms * 40); 
        int peakMarkerPos = (int)(peak * 40);

        Console.SetCursorPosition(0, 0);
        Console.Write("RMS:  [");
        Console.Write(new string('#', barLength));
        Console.Write(new string(' ', 40 - barLength));
        Console.Write("]\n");

        Console.SetCursorPosition(0, 1);
        Console.Write("Peak: [");
        Console.Write(new string(' ', 40));
        Console.Write("]\r"); // Carriage return to move back
        Console.Write("Peak: [");
        if(peakMarkerPos < 40) Console.SetCursorPosition(7 + peakMarkerPos, 1);
        Console.Write("|");
        
        Console.SetCursorPosition(0, 3);
    }
}

// Simple IVisualizationContext implementation for console output.
public class ConsoleVisualizationContext : IVisualizationContext
{
    public void Clear() { }
    public void DrawLine(float x1, float y1, float x2, float y2, Color color, float thickness = 1) { }
    public void DrawRectangle(float x, float y, float width, float height, Color color) { }
}