using SoundFlow.Abstracts;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Providers;
using SoundFlow.Structs;
using SoundFlow.Visualization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WaveformVisualization;

internal static class Program
{
    private static void Main(string[] args)
    {
        // Standard setup
        using var audioEngine = new MiniAudioEngine();
        var defaultDevice = audioEngine.PlaybackDevices.FirstOrDefault(d => d.IsDefault);
        if(defaultDevice.Id == IntPtr.Zero) return;
        var audioFormat = new AudioFormat { Format = SampleFormat.F32, SampleRate = 48000, Channels = 2 };
        using var device = audioEngine.InitializePlaybackDevice(defaultDevice, audioFormat);
        
        using var dataProvider = new StreamDataProvider(audioEngine, audioFormat, File.OpenRead("path/to/your/audiofile.wav"));
        using var player = new SoundPlayer(audioEngine, audioFormat, dataProvider);

        // Create a LevelMeterAnalyzer or any analyzer you want.
        var levelMeterAnalyzer = new LevelMeterAnalyzer();

        // Create a WaveformVisualizer.
        var waveformVisualizer = new WaveformVisualizer();

        // Add the player to the master mixer.
        Mixer.Master.AddComponent(player);

        // Subscribe to the VisualizationUpdated event to trigger a redraw.
        waveformVisualizer.VisualizationUpdated += (sender, e) =>
        {
            DrawWaveform(waveformVisualizer.Waveform);
        };

        // Connect the player's output to the level meter analyzer's input.
        player.AddAnalyzer(levelMeterAnalyzer);

        // Add the player to the master mixer.
        Mixer.Master.AddComponent(player);

        // Start playback.
        player.Play();

        // Start a timer to update the visualization.
        var timer = new System.Timers.Timer(1000 / 60); // Update at approximately 60 FPS
        timer.Elapsed += (sender, e) =>
        {
            waveformVisualizer.Render(new ConsoleVisualizationContext()); // ConsoleVisualizationContext is just a placeholder
        };
        timer.Start();

        device.MasterMixer.AddComponent(player);
        
        device.Start();
        player.Play();

        Console.WriteLine("Playing audio and displaying waveform... Press any key to stop.");
        Console.ReadKey();

        device.Stop();
        waveformVisualizer.Dispose();
    }

    // Helper method to draw a simple console-based waveform.
    private static void DrawWaveform(IReadOnlyList<float> waveform)
    {
        Console.Clear();
        int consoleWidth = Console.WindowWidth;
        int consoleHeight = Console.WindowHeight;

        if (waveform.Count == 0) return;

        for (int i = 0; i < consoleWidth; i++)
        {
            int waveformIndex = (int)(i * (waveform.Count / (float)consoleWidth));
            waveformIndex = Math.Clamp(waveformIndex, 0, waveform.Count - 1);

            float sampleValue = waveform[waveformIndex];
            int consoleY = (int)((sampleValue + 1) * 0.5 * (consoleHeight - 1));
            consoleY = Math.Clamp(consoleY, 0, consoleHeight - 1);

            if (i < consoleWidth && (consoleHeight - consoleY - 1) < consoleHeight)
            {
                Console.SetCursorPosition(i, consoleHeight - consoleY - 1);
                Console.Write("*");
            }
        }
        Console.SetCursorPosition(0, consoleHeight - 1);
    }
}