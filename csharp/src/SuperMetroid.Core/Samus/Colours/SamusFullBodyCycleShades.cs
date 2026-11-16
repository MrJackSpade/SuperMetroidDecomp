using SuperMetroid.Core.Samus.Artwork;
using static SuperMetroid.Core.Samus.Colours.SamusShadeDefinitions.FullBodyCycles;

namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// Which cycle shade the special full-body suit colourings select for a suit and cycle step. Every cycle shows only its
/// own suit's colours (#1736).
/// </summary>
public static class SamusFullBodyCycleShades
{
    /// <summary>The cycle shade the special Metroid-attachment flash alternates with normal suit colors, the Speed Booster family's last shade.</summary>
    /// <param name="suit">The suit Samus shows.</param>
    internal static SamusFullBodyCycleShade SpeedBoostShade(SamusSuit suit) =>
        new(SamusFullBodyCycleFamily.SpeedBooster, suit, SpeedBoostFlashShade);

    /// <summary>Reads the Screw Attack shade at a step of the suit's own six-step list.</summary>
    /// <param name="suit">The suit Samus shows.</param>
    /// <param name="step">The cycle step, counted from the suit's first entry.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is outside the suit's list.</exception>
    internal static SamusFullBodyCycleShade ReadScrewAttackShade(SamusSuit suit, int step) =>
        TryShade(suit, step, ScrewAttackSteps, SamusFullBodyCycleFamily.ScrewAttack, ShadeCycleShape.PingPong,
            out SamusFullBodyCycleShade palette)
            ? palette
            : throw new ArgumentOutOfRangeException(nameof(step), step, "A Screw Attack step lies within the suit's six-step list.");

    /// <summary>
    /// Reads the active Speed Booster glow at a cycle step. The step is shared with Screw Attack, so it can arrive at
    /// four or five, past the four-step list; such a step shows the list's last shade, the one the cycle holds once reached,
    /// so the suit shows only its own Speed Booster colours (#1736). Gameplay never reads the colours back, and the step
    /// itself, which the suit's colouring advances and clamps, is unchanged.
    /// </summary>
    /// <param name="suit">The suit Samus shows.</param>
    /// <param name="step">The cycle step, counted from the suit's first entry.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is outside the six steps the shared cycle step takes.</exception>
    internal static SamusFullBodyCycleShade ReadActiveSpeedBoosterShade(SamusSuit suit, int step) =>
        new(SamusFullBodyCycleFamily.SpeedBooster, suit,
            Math.Min(Ensure.BetweenInclusive(0, ScrewAttackSteps - 1, step), SpeedBoosterLastStep));

    /// <summary>Resolves a stored-shine cycle step of the suit's own list to a cycle shade.</summary>
    public static bool TryStoredShineShade(SamusSuit suit, int step, out SamusFullBodyCycleShade palette) =>
        TryShade(suit, step, StoredShineSteps, SamusFullBodyCycleFamily.StoredShine, ShadeCycleShape.PingPong,
            out palette);

    /// <summary>Resolves an active-shinespark cycle step of the suit's own list to a cycle shade.</summary>
    public static bool TryActiveShinesparkShade(SamusSuit suit, int step, out SamusFullBodyCycleShade palette) =>
        TryShade(suit, step, ActiveShinesparkSteps, SamusFullBodyCycleFamily.ActiveShinespark, ShadeCycleShape.Linear,
            out palette);

    /// <summary>
    /// Resolves a step within the suit's own list to a family's shade, mirroring later steps back toward the first
    /// shade for ping-pong cycles; false for a step outside the list.
    /// </summary>
    private static bool TryShade(
        SamusSuit suit,
        int step,
        int steps,
        SamusFullBodyCycleFamily family,
        ShadeCycleShape shape,
        out SamusFullBodyCycleShade palette)
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
    private static int Shade(int step, ShadeCycleShape shape) =>
        shape == ShadeCycleShape.PingPong ? Math.Min(step, PingPongSpan - step) : step;
}
