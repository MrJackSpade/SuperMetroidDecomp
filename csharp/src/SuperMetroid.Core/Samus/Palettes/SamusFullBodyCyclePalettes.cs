using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using static SuperMetroid.Core.Samus.Palettes.SamusPaletteDefinitions.FullBodyCycles;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Which cycle palette the special full-body palette handlers select for a suit and cycle step.</summary>
public static class SamusFullBodyCyclePalettes
{
    /// <summary>$91:D998: the cycle palette the special Super-palette flash alternates with normal suit colors, the Speed Booster family's last shade.</summary>
    /// <param name="suit">The suit Samus shows.</param>
    internal static SamusFullBodyCyclePalette SpeedBoostPalette(SamusSuit suit) =>
        new(SamusFullBodyCycleFamily.SpeedBooster, suit, SpeedBoostFlashShade);

    /// <summary>
    /// Reads the Screw Attack palette anywhere in the three suits' consecutive lists, including a restored step that
    /// runs on into the next suit's list.
    /// </summary>
    /// <param name="suit">The suit Samus shows.</param>
    /// <param name="step">The cycle step, counted from the suit's first entry.</param>
    internal static SamusFullBodyCyclePalette ReadScrewAttackPalette(SamusSuit suit, int step) =>
        ReadCompiledList(suit, step, ScrewAttackSteps, SamusFullBodyCycleFamily.ScrewAttack, PaletteCycleShape.PingPong);

    /// <summary>
    /// Reads the active Speed Booster palette anywhere in the three suits' consecutive lists. Gravity's steps four and
    /// five, inherited from the shared Screw Attack step, instead copy the colours read past the lists.
    /// </summary>
    /// <param name="suit">The suit Samus shows.</param>
    /// <param name="step">The cycle step, counted from the suit's first entry.</param>
    internal static SpeedBoosterPaletteSource ReadActiveSpeedBoosterPalette(SamusSuit suit, int step) =>
        new SpeedBoosterPaletteSelection(suit, step) switch
        {
            (SamusSuit.Gravity, GravityExpansionOverrunStep) => SpeedBoosterPaletteSource.Of(SpeedBoosterPaletteOverrun.FirstStepPastCycle),
            (SamusSuit.Gravity, GravityGrappleCodeOverrunStep) => SpeedBoosterPaletteSource.Of(SpeedBoosterPaletteOverrun.SecondStepPastCycle),
            _ => SpeedBoosterPaletteSource.Of(ReadCompiledList(suit, step, SpeedBoosterSteps,
                SamusFullBodyCycleFamily.SpeedBooster, PaletteCycleShape.Linear)),
        };

    /// <summary>Resolves a stored-shine cycle step of the suit's own list ($91:DB10/$91:DB16) to a cycle palette.</summary>
    public static bool TryStoredShinePalette(SamusSuit suit, int step, out SamusFullBodyCyclePalette palette) =>
        TryPalette(suit, step, StoredShineSteps, SamusFullBodyCycleFamily.StoredShine, PaletteCycleShape.PingPong,
            out palette);

    /// <summary>Resolves an active-shinespark cycle step of the suit's own list ($91:DB75/$91:DB7B) to a cycle palette.</summary>
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

    /// <summary>
    /// Reads the selected entry of a family's three consecutive suit lists. The cartridge adds the whole step to the
    /// selected suit's list, so a step can run on into the next suit's list; a read that leaves the three lists selects
    /// no cycle palette.
    /// </summary>
    private static SamusFullBodyCyclePalette ReadCompiledList(
        SamusSuit suit,
        int step,
        int steps,
        SamusFullBodyCycleFamily family,
        PaletteCycleShape shape)
    {
        int entry = suit.ListPlace() * steps + step;
        if ((uint)entry >= (uint)(SamusSuitDefinitions.ListOrder.Length * steps))
            throw new InvalidDataException(
                $"{family} palette list read at entry {entry} is outside the compiled lists.");

        return new(family, SamusSuits.AtListPlace(entry / steps), Shade(entry % steps, shape));
    }

    /// <summary>The shade a cycle step shows: linear cycles step through their shades, ping-pong cycles return.</summary>
    private static int Shade(int step, PaletteCycleShape shape) =>
        shape == PaletteCycleShape.PingPong ? Math.Min(step, PingPongSpan - step) : step;
}
