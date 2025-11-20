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

namespace LevelMetering;

internal static class Program
{
    private static void Main(string[] args)
    {
        // Initialize the audio engine.
        using var audioEngine = new MiniAudioEngine();
        
        var defaultDevice = audioEngine.PlaybackDevices.FirstOrDefault(d => d.IsDefault);
        if(defaultDevice.Id == IntPtr.Zero)
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

        // Create a LevelMeterAnalyzer, passing the audio format.
        var levelMeter = new LevelMeterAnalyzer(audioFormat);

        // Attach the analyzer to the player.
        player.AddAnalyzer(levelMeter);

        // Add the player to the device's master mixer.
        device.MasterMixer.AddComponent(player);

        // Start playback.
        device.Start();
        player.Play();

        // Create a timer to periodically display the RMS and peak levels.
        var timer = new System.Timers.Timer(100); // Update every 100 milliseconds
        timer.Elapsed += (sender, e) =>
        {
            Console.WriteLine($"RMS Level: {levelMeter.Rms:F4}, Peak Level: {levelMeter.Peak:F4}");
        };
        timer.Start();

        // Keep the console application running until the user presses a key.
        Console.WriteLine("Playing audio and displaying level meter... Press any key to stop.");
        Console.ReadKey();

        // Stop playback and clean up.
        timer.Stop();
        device.Stop();
    }
}