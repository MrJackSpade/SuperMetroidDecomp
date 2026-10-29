namespace SuperMetroid.Core.Hud;

/// <summary>The HUD's static top row, above the three rows its values are drawn over (#1466).</summary>
public sealed record HudTopRowImage : HudImage
{
    /// <summary>The top row.</summary>
    public static HudTopRowImage Instance { get; } = new();

    /// <inheritdoc/>
    public override string FrameName => "Hud.TopRow";
}
