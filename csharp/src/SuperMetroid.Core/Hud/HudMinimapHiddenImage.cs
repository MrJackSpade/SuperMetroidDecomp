namespace SuperMetroid.Core.Hud;

/// <summary>The minimap's empty cell, shown where nothing is revealed (#1466).</summary>
public sealed record HudMinimapHiddenImage : HudImage
{
    /// <summary>The empty cell.</summary>
    public static HudMinimapHiddenImage Instance { get; } = new();

    /// <inheritdoc/>
    public override string FrameName => "Hud.Minimap.Hidden";
}
