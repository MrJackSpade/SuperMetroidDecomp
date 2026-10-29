namespace SuperMetroid.Core.Hud;

/// <summary>The HUD's three rows before any of its values is drawn over them (#1466).</summary>
public sealed record HudTemplateImage : HudImage
{
    /// <summary>The template.</summary>
    public static HudTemplateImage Instance { get; } = new();

    /// <inheritdoc/>
    public override string FrameName => "Hud.Template";
}
