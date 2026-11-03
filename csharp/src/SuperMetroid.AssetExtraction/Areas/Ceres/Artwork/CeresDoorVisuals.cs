using SuperMetroid.Core.Assets;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;

/// <summary>
/// Editable Ceres-door colours, which the import pre-colours the doors' and the elevator room's shades from (#1512);
/// actor timing remains engine-owned, and the elevator platform's strips are pictures of the room's floor (#1467).
/// </summary>
public sealed class CeresDoorVisuals
{
    /// <summary>Normal setup colors, including any supplied edits to stock values.</summary>
    internal readonly CeresDoorNormalPaint normal;
    /// <summary>Escape setup colors resolved against the normal palette.</summary>
    internal readonly CeresDoorEscapePaint escape;
    /// <summary>Eight animation steps resolved against the normal palette.</summary>
    internal readonly CeresDoorAnimationPaint animation;
    /// <summary>Retains the recognized door paints.</summary>
    /// <param name="normal">The normal setup paint.</param>
    /// <param name="escape">The escape setup paint, resolved against the normal paint.</param>
    /// <param name="animation">The animation steps, resolved against the normal paint.</param>
    internal CeresDoorVisuals(CeresDoorNormalPaint normal, CeresDoorEscapePaint escape, CeresDoorAnimationPaint animation)
    {
        this.normal = normal;
        this.escape = escape;
        this.animation = animation;
    }

    /// <summary>The door's normal colour <paramref name="color"/>, of its fifteen nontransparent colours.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colour is not one of the fifteen.</exception>
    public RgbColor NormalColor(int color) =>
        normal.ColorAt(Ensure.BetweenInclusive(0, CeresDoorVisualDefinitions.SetupColorCount - 1, color));

    /// <summary>The door's escaping colour <paramref name="color"/>, of its fifteen nontransparent colours.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colour is not one of the fifteen.</exception>
    public RgbColor EscapeColor(int color) =>
        escape.ColorAt(Ensure.BetweenInclusive(0, CeresDoorVisualDefinitions.SetupColorCount - 1, color));

    /// <summary>Colour <paramref name="color"/> of the elevator room's glow at step <paramref name="step"/>, 0–7.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The step or colour is outside the glow.</exception>
    public RgbColor AnimationColor(int step, int color) => animation.ColorAt(
        Ensure.BetweenInclusive(0, CeresDoorVisualDefinitions.AnimationStepCount - 1, step),
        Ensure.BetweenInclusive(0, CeresDoorVisualDefinitions.AnimationColorCount - 1, color));
}
