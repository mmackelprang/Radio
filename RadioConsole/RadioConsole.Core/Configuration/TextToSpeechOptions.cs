namespace RadioConsole.Core.Configuration;

/// <summary>
/// Configuration options for Text-to-Speech services.
/// </summary>
public class TextToSpeechOptions
{
  /// <summary>
  /// Default TTS provider to use.
  /// Options: "ESpeak" (local, no API key needed), "Azure", "Google"
  /// </summary>
  public string Provider { get; set; } = "ESpeak";

  /// <summary>
  /// Azure TTS configuration.
  /// </summary>
  public AzureTtsOptions Azure { get; set; } = new();

  /// <summary>
  /// Google TTS configuration.
  /// </summary>
  public GoogleTtsOptions Google { get; set; } = new();
}

/// <summary>
/// Azure Text-to-Speech configuration.
/// </summary>
public class AzureTtsOptions
{
  /// <summary>
  /// Azure Cognitive Services API key.
  /// </summary>
  public string ApiKey { get; set; } = string.Empty;

  /// <summary>
  /// Azure region (e.g., "eastus", "westus").
  /// </summary>
  public string Region { get; set; } = "eastus";

  /// <summary>
  /// Default voice to use.
  /// </summary>
  public string DefaultVoice { get; set; } = "en-US-JennyNeural";
}

/// <summary>
/// Google Text-to-Speech configuration.
/// </summary>
public class GoogleTtsOptions
{
  /// <summary>
  /// Google Cloud API key.
  /// </summary>
  public string ApiKey { get; set; } = string.Empty;

  /// <summary>
  /// Default voice to use.
  /// </summary>
  public string DefaultVoice { get; set; } = "en-US-Standard-A";
}
