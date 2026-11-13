using SuperMetroid.Core.Samus.Artwork;
using static SuperMetroid.Core.Samus.Palettes.SamusPaletteDefinitions.FullBodyCycles;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// Which cycle palette the special full-body suit colourings select for a suit and cycle step. Every cycle shows only its
/// own suit's colours (#1736).
/// </summary>
public static class SamusFullBodyCyclePalettes
{
    /// <summary>The cycle palette the special Super-palette flash alternates with normal suit colors, the Speed Booster family's last shade.</summary>
    /// <param name="suit">The suit Samus shows.</param>
    internal static SamusFullBodyCyclePalette SpeedBoostPalette(SamusSuit suit) =>
        new(SamusFullBodyCycleFamily.SpeedBooster, suit, SpeedBoostFlashShade);

    /// <summary>Reads the Screw Attack palette at a step of the suit's own six-step list.</summary>
    /// <param name="suit">The suit Samus shows.</param>
    /// <param name="step">The cycle step, counted from the suit's first entry.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is outside the suit's list.</exception>
    internal static SamusFullBodyCyclePalette ReadScrewAttackPalette(SamusSuit suit, int step) =>
        TryPalette(suit, step, ScrewAttackSteps, SamusFullBodyCycleFamily.ScrewAttack, PaletteCycleShape.PingPong,
            out SamusFullBodyCyclePalette palette)
            ? palette
            : throw new ArgumentOutOfRangeException(nameof(step), step, "A Screw Attack step lies within the suit's six-step list.");

    /// <summary>
    /// Reads the active Speed Booster palette at a cycle step. The step is shared with Screw Attack, so it can arrive at
    /// four or five, past the four-step list; such a step shows the list's last shade, the one the cycle holds once reached,
    /// so the suit shows only its own Speed Booster colours (#1736). Gameplay never reads the colours back, and the step
    /// itself, which the caller advances and clamps, is unchanged.
    /// </summary>
    /// <param name="suit">The suit Samus shows.</param>
    /// <param name="step">The cycle step, counted from the suit's first entry.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is outside the six steps the shared cycle step takes.</exception>
    internal static SamusFullBodyCyclePalette ReadActiveSpeedBoosterPalette(SamusSuit suit, int step) =>
        new(SamusFullBodyCycleFamily.SpeedBooster, suit,
            Math.Min(Ensure.BetweenInclusive(0, ScrewAttackSteps - 1, step), SpeedBoosterLastStep));

    /// <summary>Resolves a stored-shine cycle step of the suit's own list to a cycle palette.</summary>
    public static bool TryStoredShinePalette(SamusSuit suit, int step, out SamusFullBodyCyclePalette palette) =>
        TryPalette(suit, step, StoredShineSteps, SamusFullBodyCycleFamily.StoredShine, PaletteCycleShape.PingPong,
            out palette);

    /// <summary>Resolves an active-shinespark cycle step of the suit's own list to a cycle palette.</summary>
    public static bool TryActiveShinesparkPalette(SamusSuit suit, int step, out SamusFullBodyCyclePalette palette) =>
        TryPalette(suit, step, ActiveShinesparkSteps, SamusFullBodyCycleFamily.ActiveShinespark, PaletteCycleShape.Linear,
            out palette);

    /// <summary>
    /// Resolves a step within the suit's own list to a family's palette, mirroring later steps back toward the first
    /// shade for ping-pong cycles; false for a step outside the list.
    /// </summary>
    private static bool TryPalette(
        SamusSuit suit,
        int step,
        int steps,
        SamusFullBodyCycleFamily family,
        PaletteCycleShape shape,
        out SamusFullBodyCyclePalette palette)
    {
        if ((uint)step >= (uint)steps)
        {
            palette = default;
            return false;
        }

        palette = new(family, suit, Shade(step, shape));
        return true;
    }

    /// <summary>The shade a cycle step shows: linear cycles step through their shades, ping-pong cycles return.</summary>
    private static int Shade(int step, PaletteCycleShape shape) =>
        shape == PaletteCycleShape.PingPong ? Math.Min(step, PingPongSpan - step) : step;
}
