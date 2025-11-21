namespace RadioConsole.Web.Services;

/// <summary>
/// Service for managing the state of slide-out panels in the UI.
/// Provides centralized control for opening, closing, and toggling panels.
/// </summary>
public class PanelService
{
  private readonly Dictionary<string, bool> _panelStates = new();

  /// <summary>
  /// Event fired when any panel state changes.
  /// </summary>
  public event Action? OnPanelStateChanged;

  /// <summary>
  /// Checks if a specific panel is currently open.
  /// </summary>
  /// <param name="panelName">The name of the panel to check.</param>
  /// <returns>True if the panel is open, false otherwise.</returns>
  public bool IsPanelOpen(string panelName)
  {
    return _panelStates.TryGetValue(panelName, out var isOpen) && isOpen;
  }

  /// <summary>
  /// Toggles the state of a specific panel (open to closed or vice versa).
  /// </summary>
  /// <param name="panelName">The name of the panel to toggle.</param>
  public void TogglePanel(string panelName)
  {
    if (_panelStates.ContainsKey(panelName))
      _panelStates[panelName] = !_panelStates[panelName];
    else
      _panelStates[panelName] = true;
    
    OnPanelStateChanged?.Invoke();
  }

  /// <summary>
  /// Opens a specific panel.
  /// </summary>
  /// <param name="panelName">The name of the panel to open.</param>
  public void OpenPanel(string panelName)
  {
    _panelStates[panelName] = true;
    OnPanelStateChanged?.Invoke();
  }

  /// <summary>
  /// Closes a specific panel.
  /// </summary>
  /// <param name="panelName">The name of the panel to close.</param>
  public void ClosePanel(string panelName)
  {
    _panelStates[panelName] = false;
    OnPanelStateChanged?.Invoke();
  }

  /// <summary>
  /// Closes all currently open panels.
  /// </summary>
  public void CloseAllPanels()
  {
    foreach (var key in _panelStates.Keys.ToList())
      _panelStates[key] = false;
    
    OnPanelStateChanged?.Invoke();
  }

  /// <summary>
  /// Gets the count of currently open panels.
  /// </summary>
  /// <returns>Number of open panels.</returns>
  public int GetOpenPanelCount()
  {
    return _panelStates.Count(kvp => kvp.Value);
  }

  /// <summary>
  /// Gets a list of all currently open panel names.
  /// </summary>
  /// <returns>List of open panel names.</returns>
  public List<string> GetOpenPanels()
  {
    return _panelStates.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
  }
}
