using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Editable Ceres-door RGB5 palettes; actor timing remains engine-owned, and the elevator platform's strips are pictures of the room's floor (#1467).</summary>
public sealed class CeresDoorVisuals
{
    /// <summary>Normal setup colors, including any supplied edits to stock values.</summary>
    internal readonly CeresDoorNormalPaint normal;
    /// <summary>Escape setup colors resolved against the normal palette.</summary>
    internal readonly CeresDoorEscapePaint escape;
    /// <summary>Eight animation steps resolved against the normal palette.</summary>
    internal readonly CeresDoorAnimationPaint animation;
    /// <summary>Retains the door colours the installed visual document supplied.</summary>
    /// <param name="normal">Fifteen normal setup colours.</param>
    /// <param name="escape">Fifteen escape setup colours.</param>
    /// <param name="animation">Eight steps of six animation colours.</param>
    internal CeresDoorVisuals(RgbColor[] normal, RgbColor[] escape, RgbColor[][] animation)
    {
        this.normal = new(normal);
        this.escape = new(escape, this.normal);
        this.animation = new(animation, this.normal);
    }

    /// <summary>Installs fifteen normal nontransparent inks: the completed setup-fade target.</summary>
    /// <param name="destination">The door's colours: an inactive door's, or the active door's.</param>
    public void LoadNormalColors(ColourRun destination)
    {
        for (int color = 0; color < CeresDoorVisualDefinitions.SetupColorCount; color++)
            destination.Write(color, normal.ColorAt(color));
    }

    /// <summary>Installs fifteen escape-state nontransparent inks; escape-state selection remains actor-owned.</summary>
    /// <param name="destination">The active door's colours.</param>
    public void LoadEscapeColors(ColourRun destination)
    {
        for (int color = 0; color < CeresDoorVisualDefinitions.SetupColorCount; color++)
            destination.Write(color, EscapeColor(color));
    }

    /// <summary>Resolves one escape-state ink from the selected palette.</summary>
    private RgbColor EscapeColor(int color) => escape.ColorAt(color);

    /// <summary>Installs the door animation's six colours from glow step 0–7; the actor derives the step from its frame bits 3–5.</summary>
    public void LoadAnimationColors(BackgroundColourSets colours, int step)
    {
        ColourRun animated = CeresDoorColourDefinitions.Animation.In(colours);
        if ((uint)step >= CeresDoorVisualDefinitions.AnimationStepCount) throw new ArgumentOutOfRangeException(nameof(step));
        for (int color = 0; color < CeresDoorVisualDefinitions.AnimationColorCount; color++)
            animated.Write(color, AnimationColor(step, color));
    }

    /// <summary>Resolves one colour of the actor-selected door animation step.</summary>
    private RgbColor AnimationColor(int step, int color) => animation.ColorAt(step, color);
}
