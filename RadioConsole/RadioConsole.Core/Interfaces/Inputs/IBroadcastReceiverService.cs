namespace RadioConsole.Core.Interfaces.Inputs;

/// <summary>
/// Interface for receiving Google Assistant broadcasts.
/// Listens for incoming 'Broadcast' events from Google Home devices.
/// </summary>
public interface IBroadcastReceiverService
{
  /// <summary>
  /// Initialize the broadcast receiver service and start listening for broadcasts.
  /// </summary>
  Task InitializeAsync();

  /// <summary>
  /// Start listening for broadcast events.
  /// </summary>
  Task StartListeningAsync();

  /// <summary>
  /// Stop listening for broadcast events.
  /// </summary>
  Task StopListeningAsync();

  /// <summary>
  /// Check if the service is currently listening for broadcasts.
  /// </summary>
  bool IsListening { get; }

  /// <summary>
  /// Event raised when a broadcast is received.
  /// </summary>
  event EventHandler<BroadcastReceivedEventArgs>? BroadcastReceived;
}

/// <summary>
/// Event arguments for broadcast received events.
/// Contains the audio data and metadata of the broadcast message.
/// </summary>
public class BroadcastReceivedEventArgs : EventArgs
{
  /// <summary>
  /// The text content of the broadcast message.
  /// </summary>
  public string Message { get; set; } = string.Empty;

  /// <summary>
  /// The audio data of the broadcast message as a stream.
  /// </summary>
  public Stream? AudioData { get; set; }

  /// <summary>
  /// The format of the audio data (e.g., "PCM", "MP3", "WAV").
  /// </summary>
  public string AudioFormat { get; set; } = "PCM";

  /// <summary>
  /// Sample rate of the audio in Hz (e.g., 16000, 44100).
  /// </summary>
  public int SampleRate { get; set; } = 16000;

  /// <summary>
  /// Number of audio channels (1 for mono, 2 for stereo).
  /// </summary>
  public int Channels { get; set; } = 1;

  /// <summary>
  /// Bits per sample (e.g., 16, 24).
  /// </summary>
  public int BitsPerSample { get; set; } = 16;

  /// <summary>
  /// Timestamp when the broadcast was received.
  /// </summary>
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;

  /// <summary>
  /// Unique identifier for this broadcast.
  /// </summary>
  public string BroadcastId { get; set; } = Guid.NewGuid().ToString();
}
