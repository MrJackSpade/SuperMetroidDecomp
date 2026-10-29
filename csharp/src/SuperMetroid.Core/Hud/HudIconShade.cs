namespace SuperMetroid.Core.Hud;

/// <summary>The shade the HUD last gave an item icon's cells when the selection changed (#1466).</summary>
public enum HudIconShade
{
    /// <summary>The selected item's shade.</summary>
    Selected,

    /// <summary>The shade an item returns to when another is selected.</summary>
    Deselected,
}
