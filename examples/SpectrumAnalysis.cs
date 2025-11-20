using SoundFlow.Abstracts;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Providers;
using SoundFlow.Structs;
using SoundFlow.Visualization;
using System;
using System.IO;
using System.Linq;

namespace SpectrumAnalysis;

internal static class Program
{
    private static void Main(string[] args)
    {
        // Initialize the audio engine.
        using var audioEngine = new MiniAudioEngine();
        
        var defaultDevice = audioEngine.PlaybackDevices.FirstOrDefault(d => d.IsDefault);
        if (defaultDevice.Id == IntPtr.Zero)
        {
            Console.WriteLine("No default playback device found.");
            return;
        }

        var audioFormat = new AudioFormat
        {
            Format = SampleFormat.F32,
            SampleRate = 48000,
            Channels = 2
        };
        
        using var device = audioEngine.InitializePlaybackDevice(defaultDevice, audioFormat);
        
        // Create a SoundPlayer and load an audio file.
        using var dataProvider = new StreamDataProvider(audioEngine, audioFormat, File.OpenRead("path/to/your/audiofile.wav"));
        using var player = new SoundPlayer(audioEngine, audioFormat, dataProvider);

        // Create a SpectrumAnalyzer with an FFT size of 2048.
        var spectrumAnalyzer = new SpectrumAnalyzer(audioFormat, fftSize: 2048);

        // Attach the spectrum analyzer to the player.
        player.AddAnalyzer(spectrumAnalyzer);

        // Add the player to the device's master mixer.
        device.MasterMixer.AddComponent(player);

        // Start playback.
        device.Start();
        player.Play();

        // Create a timer to periodically display the spectrum data.
        var timer = new System.Timers.Timer(100); // Update every 100 milliseconds
        timer.Elapsed += (sender, e) =>
        {
            // Get the spectrum data from the analyzer.
            var spectrumData = spectrumAnalyzer.SpectrumData;

            // Print the magnitude of the first few frequency bins.
            if (spectrumData.Length > 0)
            {
                Console.Write("Spectrum: ");
                for (int i = 0; i < Math.Min(10, spectrumData.Length); i++)
                {
                    Console.Write($"{spectrumData[i]:F2} ");
                }
                Console.WriteLine();
            }
        };
        timer.Start();

        // Keep the console application running until the user presses a key.
        Console.WriteLine("Playing audio and displaying spectrum data... Press any key to stop.");
        Console.ReadKey();

        // Stop playback and clean up.
        timer.Stop();
        device.Stop();
    }
}