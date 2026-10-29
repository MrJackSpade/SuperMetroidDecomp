namespace SuperMetroid.Core.Hud;

/// <summary>A minimap cell nothing reveals, or one beyond the map's rows: the HUD shows its empty cell (#1466).</summary>
public sealed record MinimapHiddenCell : MinimapCell
{
    /// <summary>The hidden cell.</summary>
    public static MinimapHiddenCell Instance { get; } = new();
}
