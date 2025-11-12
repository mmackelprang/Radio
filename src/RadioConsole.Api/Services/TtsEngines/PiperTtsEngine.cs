using System.Diagnostics;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Models;

namespace RadioConsole.Api.Services.TtsEngines;

/// <summary>
/// Piper Text-to-Speech engine implementation
/// High-quality neural TTS using ONNX models
/// </summary>
public class PiperTtsEngine : ITtsEngine
{
  private readonly PiperConfig _config;
  private readonly ILogger<PiperTtsEngine> _logger;
  private bool _isAvailable;

  public bool IsAvailable => _isAvailable;
  public string EngineName => "Piper";

  public PiperTtsEngine(
    PiperConfig config,
    ILogger<PiperTtsEngine> logger)
  {
    _config = config;
    _logger = logger;
  }

  public async Task<bool> InitializeAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      // Check if Piper executable exists
      if (!await CheckExecutableExistsAsync(cancellationToken))
      {
        _logger.LogInformation("Piper TTS executable not found at: {Path}", _config.ExecutablePath);
        _isAvailable = false;
        return false;
      }

      // Validate model and config paths
      if (string.IsNullOrWhiteSpace(_config.ModelPath) || !File.Exists(_config.ModelPath))
      {
        _logger.LogWarning("Piper TTS model file not found at: {Path}", _config.ModelPath);
        _isAvailable = false;
        return false;
      }

      if (string.IsNullOrWhiteSpace(_config.ConfigPath) || !File.Exists(_config.ConfigPath))
      {
        _logger.LogWarning("Piper TTS config file not found at: {Path}", _config.ConfigPath);
        _isAvailable = false;
        return false;
      }

      _isAvailable = true;
      _logger.LogInformation("Piper TTS initialized successfully with model: {Model}", _config.ModelPath);
      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error initializing Piper TTS");
      _isAvailable = false;
      return false;
    }
  }

  private async Task<bool> CheckExecutableExistsAsync(CancellationToken cancellationToken)
  {
    try
    {
      var testProcess = new Process
      {
        StartInfo = new ProcessStartInfo
        {
          FileName = _config.ExecutablePath,
          Arguments = "--version",
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          UseShellExecute = false,
          CreateNoWindow = true
        }
      };

      testProcess.Start();
      var output = await testProcess.StandardOutput.ReadToEndAsync(cancellationToken);
      await testProcess.WaitForExitAsync(cancellationToken);

      if (testProcess.ExitCode == 0)
      {
        _logger.LogInformation("Piper TTS found: {Version}", output.Split('\n')[0]);
        return true;
      }

      return false;
    }
    catch (Exception)
    {
      // Executable not found or failed to start
      return false;
    }
  }

  public async Task<Stream> GenerateSpeechAsync(string text, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(text))
    {
      throw new ArgumentException("Text cannot be empty", nameof(text));
    }

    if (!_isAvailable)
    {
      throw new InvalidOperationException("Piper TTS is not available");
    }

    try
    {
      var startInfo = new ProcessStartInfo
      {
        FileName = _config.ExecutablePath,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };

      // Build arguments
      var args = new List<string>
      {
        "--model", _config.ModelPath,
        "--config", _config.ConfigPath,
        "--output_raw" // Output raw PCM
      };

      if (_config.SpeakingRate != 1.0)
      {
        args.Add("--length_scale");
        args.Add((1.0 / _config.SpeakingRate).ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
      }

      if (_config.SentenceSilence != 0.2)
      {
        args.Add("--sentence_silence");
        args.Add(_config.SentenceSilence.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
      }

      startInfo.Arguments = string.Join(" ", args);

      _logger.LogDebug("Starting Piper: {Command} {Args}", _config.ExecutablePath, startInfo.Arguments);

      using var process = new Process { StartInfo = startInfo };
      process.Start();

      // Write text to stdin
      await process.StandardInput.WriteLineAsync(text.AsMemory(), cancellationToken);
      await process.StandardInput.FlushAsync(cancellationToken);
      process.StandardInput.Close();

      // Read PCM data from stdout
      var pcmStream = new MemoryStream();
      await process.StandardOutput.BaseStream.CopyToAsync(pcmStream, cancellationToken);

      // Wait for process to complete
      await process.WaitForExitAsync(cancellationToken);

      if (process.ExitCode != 0)
      {
        var error = await process.StandardError.ReadToEndAsync(cancellationToken);
        _logger.LogError("Piper process failed with code {Code}: {Error}", process.ExitCode, error);
        throw new InvalidOperationException($"Piper TTS failed: {error}");
      }

      // Convert PCM to WAV
      var wavStream = ConvertPcmToWav(pcmStream, _config.SampleRate);

      _logger.LogDebug("Generated {Bytes} bytes of audio for text: {Text}", wavStream.Length, text);

      return wavStream;
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

    // Rough estimate: ~3 words per second at normal speed
    var wordCount = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    var baseSeconds = wordCount / 3.0;

    // Adjust for speaking rate
    var seconds = baseSeconds / _config.SpeakingRate;

    // Add small buffer for processing
    return seconds + 0.5;
  }

  private Stream ConvertPcmToWav(MemoryStream pcmStream, int sampleRate)
  {
    pcmStream.Position = 0;
    var pcmData = pcmStream.ToArray();
    var numSamples = pcmData.Length / 2; // 16-bit audio
    var numBytes = pcmData.Length;

    var wavStream = new MemoryStream();
    using var writer = new BinaryWriter(wavStream, System.Text.Encoding.UTF8, leaveOpen: true);

    // Write WAV header
    writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
    writer.Write(36 + numBytes); // File size - 8
    writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
    writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
    writer.Write(16); // Format chunk size
    writer.Write((short)1); // Audio format (PCM)
    writer.Write((short)1); // Number of channels (mono)
    writer.Write(sampleRate); // Sample rate
    writer.Write(sampleRate * 2); // Byte rate (sample rate * channels * bytes per sample)
    writer.Write((short)2); // Block align (channels * bytes per sample)
    writer.Write((short)16); // Bits per sample
    writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
    writer.Write(numBytes); // Data size

    // Write PCM data
    writer.Write(pcmData);

    wavStream.Position = 0;
    return wavStream;
  }
}
