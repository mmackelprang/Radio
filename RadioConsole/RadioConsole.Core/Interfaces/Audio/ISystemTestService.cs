namespace RadioConsole.Core.Interfaces.Audio;

/// <summary>
/// Service for triggering system test events and audio generation.
/// Used for testing audio priority, TTS, and event handling.
/// </summary>
public interface ISystemTestService
{
  /// <summary>
  /// Trigger a text-to-speech test with a custom phrase.
  /// </summary>
  /// <param name="phrase">The text to speak.</param>
  /// <param name="voiceGender">Optional voice gender ("male" or "female").</param>
  /// <param name="speed">Optional speech speed (0.5 to 2.0).</param>
  Task TriggerTtsAsync(string phrase, string? voiceGender = null, float speed = 1.0f);

  /// <summary>
  /// Generate and play a test tone.
  /// </summary>
  /// <param name="frequency">Frequency of the tone in Hz. Default is 300Hz.</param>
  /// <param name="durationSeconds">Duration of the tone in seconds. Default is 2 seconds.</param>
  Task TriggerTestToneAsync(int frequency = 300, double durationSeconds = 2);

  /// <summary>
  /// Simulate a doorbell event.
  /// This triggers a high-priority audio event to test the ducking behavior.
  /// </summary>
  Task TriggerDoorbellAsync();

  /// <summary>
  /// Check if a test is currently running.
  /// </summary>
  bool IsTestRunning { get; }
}
