namespace SuperMetroid.Core.Assets;

/// <summary>Compiled controller binding represented by a panel's safe button-glyph slot.</summary>
public enum GameplayMessagePanelButtonBinding : byte
{
    /// <summary>The message has no large-panel controller-glyph slot.</summary>
    None,
    /// <summary>The panel displays the configured shoot-button glyph.</summary>
    Shoot,
    /// <summary>The panel displays the configured run-button glyph.</summary>
    Run,
}
