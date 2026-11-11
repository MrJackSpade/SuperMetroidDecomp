using SuperMetroid.Core.Assets;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;

/// <summary>
/// Editable Ceres-door colours, which the import pre-colours the doors' and the elevator room's shades from (#1512);
/// actor timing remains engine-owned, and the elevator platform's strips are pictures of the room's floor (#1467).
/// Every colour is kept as installed.
/// </summary>
public sealed class CeresDoorVisuals
{
    /// <summary>The fifteen installed normal setup colours.</summary>
    private readonly RgbColor[] normal;
    /// <summary>The fifteen installed escape setup colours.</summary>
    private readonly RgbColor[] escape;
    /// <summary>The installed elevator-room glow: eight steps of six colours.</summary>
    private readonly RgbColor[][] animation;

    /// <summary>Retains the door's complete installed colours.</summary>
    /// <param name="normal">The fifteen normal setup colours.</param>
    /// <param name="escape">The fifteen escape setup colours.</param>
    /// <param name="animation">The glow's eight steps of six colours.</param>
    /// <exception cref="InvalidDataException">A set does not hold every colour the door shows.</exception>
    internal CeresDoorVisuals(RgbColor[] normal, RgbColor[] escape, RgbColor[][] animation)
    {
        Ensure.NotNull(normal); Ensure.NotNull(escape); Ensure.NotNull(animation);
        if (normal.Length != CeresDoorVisualDefinitions.SetupColorCount)
            throw new InvalidDataException($"Ceres-door normal colours hold {normal.Length}; expected {CeresDoorVisualDefinitions.SetupColorCount}.");
        if (escape.Length != CeresDoorVisualDefinitions.SetupColorCount)
            throw new InvalidDataException($"Ceres-door escape colours hold {escape.Length}; expected {CeresDoorVisualDefinitions.SetupColorCount}.");
        if (animation.Length != CeresDoorVisualDefinitions.AnimationStepCount ||
            animation.Any(step => step is null || step.Length != CeresDoorVisualDefinitions.AnimationColorCount))
            throw new InvalidDataException(
                $"Ceres-door glow requires {CeresDoorVisualDefinitions.AnimationStepCount} steps of {CeresDoorVisualDefinitions.AnimationColorCount} colours.");
        this.normal = [.. normal];
        this.escape = [.. escape];
        this.animation = [.. animation.Select(step => (RgbColor[])[.. step])];
    }

    /// <summary>The door's normal colour <paramref name="color"/>, of its fifteen nontransparent colours.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colour is not one of the fifteen.</exception>
    public RgbColor NormalColor(int color) =>
        normal[Ensure.BetweenInclusive(0, CeresDoorVisualDefinitions.SetupColorCount - 1, color)];

    /// <summary>The door's escaping colour <paramref name="color"/>, of its fifteen nontransparent colours.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colour is not one of the fifteen.</exception>
    public RgbColor EscapeColor(int color) =>
        escape[Ensure.BetweenInclusive(0, CeresDoorVisualDefinitions.SetupColorCount - 1, color)];

    /// <summary>Colour <paramref name="color"/> of the elevator room's glow at step <paramref name="step"/>, 0–7.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The step or colour is outside the glow.</exception>
    public RgbColor AnimationColor(int step, int color) => animation
        [Ensure.BetweenInclusive(0, CeresDoorVisualDefinitions.AnimationStepCount - 1, step)]
        [Ensure.BetweenInclusive(0, CeresDoorVisualDefinitions.AnimationColorCount - 1, color)];
}
