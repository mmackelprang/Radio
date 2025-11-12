using System.Diagnostics;
using System.Text;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Models;

namespace RadioConsole.Api.Services.TtsEngines;

/// <summary>
/// eSpeak-ng Text-to-Speech engine implementation
/// Lightweight, open-source TTS engine available on Linux, Windows, and Mac
/// </summary>
public class EspeakNgTtsEngine : ITtsEngine
{
  private readonly EspeakNgConfig _config;
  private readonly ILogger<EspeakNgTtsEngine> _logger;
  private bool _isAvailable;

  public bool IsAvailable => _isAvailable;
  public string EngineName => "eSpeak-ng";

  public EspeakNgTtsEngine(
    EspeakNgConfig config,
    ILogger<EspeakNgTtsEngine> logger)
  {
    _config = config;
    _logger = logger;
  }

  public async Task<bool> InitializeAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      // Check if eSpeak is available
      if (await TryInitializeESpeakAsync(_config.ExecutablePath, cancellationToken))
      {
        return true;
      }

      // Try fallback to older 'espeak' command
      _logger.LogDebug("Primary eSpeak executable not found, trying fallback to 'espeak'");
      if (await TryInitializeESpeakAsync("espeak", cancellationToken))
      {
        _config.ExecutablePath = "espeak";
        return true;
      }

      // Neither espeak-ng nor espeak are available
      _logger.LogInformation("eSpeak-ng TTS is not available (espeak-ng or espeak not installed)");
      _isAvailable = false;
      return false;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error initializing eSpeak-ng TTS");
      _isAvailable = false;
      return false;
    }
  }

  private async Task<bool> TryInitializeESpeakAsync(string executablePath, CancellationToken cancellationToken)
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
      var output = await testProcess.StandardOutput.ReadToEndAsync(cancellationToken);
      await testProcess.WaitForExitAsync(cancellationToken);

      if (testProcess.ExitCode == 0)
      {
        _isAvailable = true;
        _logger.LogInformation("eSpeak-ng TTS initialized successfully using '{ExecutablePath}': {Version}",
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

  public async Task<Stream> GenerateSpeechAsync(string text, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(text))
    {
      throw new ArgumentException("Text cannot be empty", nameof(text));
    }

    if (!_isAvailable)
    {
      throw new InvalidOperationException("eSpeak-ng TTS is not available");
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

      _logger.LogDebug("Starting eSpeak: {Command} {Args}", _config.ExecutablePath, args);

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
        throw new InvalidOperationException($"eSpeak-ng TTS failed: {error}");
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
}
