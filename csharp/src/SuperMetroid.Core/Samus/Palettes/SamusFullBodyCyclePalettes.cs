using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Which cycle palette the special full-body palette handlers select for a suit and phase.</summary>
public static class SamusFullBodyCyclePalettes
{
    /// <summary>$91:D998: the cycle palette the special Super-palette flash alternates with normal suit colors, the Speed Booster family's last shade.</summary>
    /// <param name="suitByteOffset">Even byte offset 0 (Power), 2 (Varia) or 4 (Gravity) into the native suit table.</param>
    internal static SamusFullBodyCyclePalette SpeedBoostPalette(int suitByteOffset) =>
        new(SamusFullBodyCycleFamily.SpeedBooster, Suit(suitByteOffset), 3);

    /// <summary>
    /// Reads the Screw Attack palette anywhere in the compiled three-suit list allocation, including a restored phase
    /// that runs on into the next suit's list.
    /// </summary>
    internal static SamusFullBodyCyclePalette ReadScrewAttackPalette(
        int suitByteOffset, int phaseByteOffset) =>
        ReadCompiledList(suitByteOffset, phaseByteOffset, phaseCount: 6,
            SamusFullBodyCycleFamily.ScrewAttack, PaletteCycleShape.PingPong);

    /// <summary>
    /// Reads the active Speed Booster palette anywhere in the compiled three-suit list allocation. Gravity phases
    /// eight and ten, inherited from the shared Screw Attack phase word, instead read the instruction bytes after
    /// the lists.
    /// </summary>
    internal static SpeedBoosterPaletteSource ReadActiveSpeedBoosterPalette(
        int suitByteOffset, int phaseByteOffset) =>
        new SpeedBoosterPaletteOffsets(suitByteOffset, phaseByteOffset) switch
        {
            (4, 8) => SpeedBoosterPaletteSource.Of(SpeedBoosterPaletteOverrun.Expansion),
            (4, 10) => SpeedBoosterPaletteSource.Of(SpeedBoosterPaletteOverrun.GrappleCode),
            _ => SpeedBoosterPaletteSource.Of(ReadCompiledList(suitByteOffset, phaseByteOffset, phaseCount: 4,
                SamusFullBodyCycleFamily.SpeedBooster, PaletteCycleShape.Linear)),
        };

    /// <summary>
    /// Resolves the bounded $91:DB10/$91:DB16 stored-shine suit and phase lists to a cycle palette.
    /// </summary>
    public static bool TryStoredShinePalette(
        int suitByteOffset, int phaseByteOffset, out SamusFullBodyCyclePalette palette) =>
        TryPalette(suitByteOffset, phaseByteOffset, 10, SamusFullBodyCycleFamily.StoredShine,
            PaletteCycleShape.PingPong, out palette);

    /// <summary>
    /// Resolves the bounded $91:DB75/$91:DB7B active-shinespark suit and phase lists to a cycle palette.
    /// </summary>
    public static bool TryActiveShinesparkPalette(
        int suitByteOffset, int phaseByteOffset, out SamusFullBodyCyclePalette palette) =>
        TryPalette(suitByteOffset, phaseByteOffset, 6, SamusFullBodyCycleFamily.ActiveShinespark,
            PaletteCycleShape.Linear, out palette);

    /// <summary>
    /// Resolves a valid suit/phase pair to a family's palette, mirroring later phases back toward the first shade
    /// for ping-pong cycles.
    /// </summary>
    private static bool TryPalette(
        int suitByteOffset,
        int phaseByteOffset,
        int lastPhaseByteOffset,
        SamusFullBodyCycleFamily family,
        PaletteCycleShape shape,
        out SamusFullBodyCyclePalette palette)
    {
        if (suitByteOffset > 4 || (suitByteOffset & 1) != 0 ||
            phaseByteOffset > lastPhaseByteOffset || (phaseByteOffset & 1) != 0)
        {
            palette = default;
            return false;
        }

        palette = new(family, suitByteOffset / 2, Shade(phaseByteOffset / 2, shape));
        return true;
    }

    /// <summary>
    /// Reads the selected entry of a family's contiguous three-suit list allocation. The native two-level lookup
    /// adds the full 16-bit phase word to the selected suit's list, so a phase can run on into the next suit's
    /// list; a read that leaves the allocation, or straddles two entries, selects no cycle palette.
    /// </summary>
    private static SamusFullBodyCyclePalette ReadCompiledList(
        int suitByteOffset,
        int phaseByteOffset,
        int phaseCount,
        SamusFullBodyCycleFamily family,
        PaletteCycleShape shape)
    {
        if (suitByteOffset > 4 || (suitByteOffset & 1) != 0)
            throw new ArgumentOutOfRangeException(nameof(suitByteOffset));

        int byteOffset = suitByteOffset / 2 * phaseCount * sizeof(ushort) + phaseByteOffset;
        if (byteOffset + 1 >= 3 * phaseCount * sizeof(ushort))
            throw new InvalidDataException(
                $"{family} palette list read at byte {byteOffset} is outside the compiled list allocation.");
        if ((byteOffset & 1) != 0)
            throw new InvalidDataException(
                $"{family} palette list read at odd byte {byteOffset} straddles two entries and selects no cycle palette.");

        int entry = byteOffset / sizeof(ushort);
        return new(family, entry / phaseCount, Shade(entry % phaseCount, shape));
    }

    /// <summary>The shade a cycle phase shows: linear cycles step through their shades, ping-pong cycles return.</summary>
    private static int Shade(int phase, PaletteCycleShape shape) =>
        shape == PaletteCycleShape.PingPong ? Math.Min(phase, 6 - phase) : phase;

    /// <summary>The suit an even native suit-table byte offset selects.</summary>
    private static int Suit(int suitByteOffset) => suitByteOffset is 0 or 2 or 4
        ? suitByteOffset / 2
        : throw new ArgumentOutOfRangeException(nameof(suitByteOffset));
}
