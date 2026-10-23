using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Ports the nonnegative <c>samus_special_super_palette_flags</c> branch of
/// <c>HandleMiscSamusPalette</c> at <c>$91:D8A5</c>. Ordinary Metroid attachment is the
/// retail producer: odd calls load the speed-boost palette, even calls restore the normal
/// suit, and the complete word increments until the Metroid releases Samus.
/// </summary>
public static class SamusSpecialSuperPalette
{
    /// <summary>
    /// The first phase the cartridge's signed test reads as negative: the drained/rainbow branch, which this
    /// alternation neither draws nor advances.
    /// </summary>
    private const int FirstDrainedBranchPhase = 32768;

    /// <returns>True when the special branch owned—and wrote—the visible Samus palette.</returns>
    public static bool Update(
        SamusColours colours,
        SamusState samus)
    {
        Ensure.NotNull(colours);
        Ensure.NotNull(samus);

        int phase = samus.SpecialSuperPaletteFlags;
        if (phase is 0 or >= FirstDrainedBranchPhase)
            return false;

        if ((phase & 1) == 0)
            SamusNormalSuitPalette.Load(colours.Suit, samus.EquippedItems, samus.Presentation.SuitColors);
        else
            (samus.Presentation.FullBodyCycleColors ?? throw new InvalidOperationException(
                "Metroid-attachment palette requires installed Samus full-body cycle colors."))
                .Apply(colours, SamusPaletteRomData.FullBodyCycles.SpeedBoostPalette(
                    samus.EquippedItems.GetSuitPaletteTableOffset()));
        // The phase is below the drained branch here, so advancing it never wraps.
        samus.SpecialSuperPaletteFlags = phase + 1;
        return true;
    }
}
