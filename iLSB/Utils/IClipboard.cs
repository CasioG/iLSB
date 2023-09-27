namespace iLSB.Utils;

/// <summary>
/// Provides methods to place text on and retrieve text from the system Clipboard.
/// </summary>
public interface IClipboard
{
    /// <summary>
    /// Retrieves text data from the Clipboard.
    /// </summary>
    public string? GetText();

    /// <summary>
    /// Clears the Clipboard and then adds text data to it.
    /// </summary>
    public void SetText(string text);
}
