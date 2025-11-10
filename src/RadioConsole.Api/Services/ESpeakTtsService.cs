using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Models;

namespace RadioConsole.Api.Services;

/// <summary>
/// Text-to-Speech service using eSpeak/eSpeak-ng engine
/// eSpeak is a lightweight, open-source TTS engine available on Linux, Windows, and Mac
/// </summary>
public class ESpeakTtsService : ITtsService
{
    private readonly ESpeakTtsConfig _config;
    private readonly IEnvironmentService _environmentService;
    private readonly ILogger<ESpeakTtsService> _logger;
    private bool _isAvailable;

    public bool IsAvailable => _isAvailable;

    public ESpeakTtsService(
        IOptions<ESpeakTtsConfig> config,
        IEnvironmentService environmentService,
        ILogger<ESpeakTtsService> logger)
    {
        _config = config.Value;
        _environmentService = environmentService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (_environmentService.IsSimulationMode)
            {
                _logger.LogInformation("eSpeak TTS initialized in simulation mode");
                _isAvailable = true;
                return;
            }

            // Check if eSpeak is available
            if (await TryInitializeESpeakAsync(_config.ESpeakExecutablePath))
            {
                return;
            }

            // Try fallback to older 'espeak' command
            _logger.LogDebug("Primary eSpeak executable not found, trying fallback to 'espeak'");
            if (await TryInitializeESpeakAsync("espeak"))
            {
                _config.ESpeakExecutablePath = "espeak";
                return;
            }

            // Neither espeak-ng nor espeak are available
            _logger.LogInformation("eSpeak TTS is not available (espeak-ng or espeak not installed). Text-to-speech features will be disabled. See ESPEAK_TTS_SETUP.md for installation instructions.");
            _isAvailable = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error initializing eSpeak TTS");
            _isAvailable = false;
        }
    }

    private async Task<bool> TryInitializeESpeakAsync(string executablePath)
    {
        try
        {
            var testProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            testProcess.Start();
            var output = await testProcess.StandardOutput.ReadToEndAsync();
            await testProcess.WaitForExitAsync();

            if (testProcess.ExitCode == 0)
            {
                _isAvailable = true;
                _logger.LogInformation("eSpeak TTS initialized successfully using '{ExecutablePath}': {Version}", 
                    executablePath, output.Split('\n')[0]);
                return true;
            }

            return false;
        }
        catch (Exception)
        {
            // Executable not found or failed to start - this is expected when not installed
            return false;
        }
    }

    public async Task<Stream?> GenerateSpeechAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be empty", nameof(text));
        }

        if (_environmentService.IsSimulationMode)
        {
            return await GenerateSimulatedAudioAsync(text, cancellationToken);
        }

        if (!_isAvailable)
        {
            throw new InvalidOperationException("eSpeak TTS is not available");
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _config.ESpeakExecutablePath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Build arguments
            var args = new StringBuilder();
            args.Append("--stdout"); // Output WAV to stdout
            args.Append($" -v {_config.Voice}");
            args.Append($" -s {_config.Speed}");
            args.Append($" -p {_config.Pitch}");
            args.Append($" -a {_config.Volume}");
            
            if (_config.WordGap > 0)
            {
                args.Append($" -g {_config.WordGap}");
            }

            startInfo.Arguments = args.ToString();

            _logger.LogDebug("Starting eSpeak: {Command} {Args}", _config.ESpeakExecutablePath, args);

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            // Write text to stdin
            await process.StandardInput.WriteLineAsync(text.AsMemory(), cancellationToken);
            await process.StandardInput.FlushAsync(cancellationToken);
            process.StandardInput.Close();

            // Read WAV data from stdout
            var audioStream = new MemoryStream();
            await process.StandardOutput.BaseStream.CopyToAsync(audioStream, cancellationToken);
            audioStream.Position = 0;

            // Wait for process to complete
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync(cancellationToken);
                _logger.LogError("eSpeak process failed with code {Code}: {Error}", process.ExitCode, error);
                throw new InvalidOperationException($"eSpeak TTS failed: {error}");
            }

            _logger.LogDebug("Generated {Bytes} bytes of audio for text: {Text}", audioStream.Length, text);

            return audioStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating speech for text: {Text}", text);
            throw;
        }
    }

    public double EstimateDuration(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        // Estimate based on configured speed (words per minute)
        var wordCount = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        var minutes = wordCount / (double)_config.Speed;
        var seconds = minutes * 60.0;

        // Add small buffer for processing
        return seconds + 0.5;
    }

    private async Task<Stream> GenerateSimulatedAudioAsync(string text, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Simulating TTS for: {Text}", text);

        // Generate a simple WAV header with silence
        var duration = EstimateDuration(text);
        var sampleRate = _config.SampleRate;
        var numSamples = (int)(duration * sampleRate);
        var numBytes = numSamples * 2; // 16-bit audio

        var stream = new MemoryStream();
        await using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        // Write WAV header
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + numBytes); // File size - 8
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16); // Format chunk size
        writer.Write((short)1); // Audio format (PCM)
        writer.Write((short)1); // Number of channels
        writer.Write(sampleRate); // Sample rate
        writer.Write(sampleRate * 2); // Byte rate
        writer.Write((short)2); // Block align
        writer.Write((short)16); // Bits per sample
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(numBytes); // Data size

        // Write silence (zeros)
        var silence = new byte[numBytes];
        await stream.WriteAsync(silence, cancellationToken);

        stream.Position = 0;
        return stream;
    }
}
