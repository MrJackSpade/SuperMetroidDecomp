namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge-exact library-one sound routing for ordinary and charged beam producers.
/// </summary>
/// <remarks>
/// The 65C816 indexes these tables with the raw four-bit beam combination. Entries twelve
/// through fifteen therefore observe the neighboring charged and non-beam tables rather
/// than stopping at the twelve authored combinations. Those four bounded overreads are
/// gameplay-visible advanced-technique behavior and are compiled deliberately.
/// </remarks>
public static class SamusProjectileSoundRoutingDefinitions
{
    /// <summary>$90:C28F ProjectileSFX_Uncharged: first library-one request in the three beam families.</summary>
    private const ushort UnchargedPowerRequest = 0x000b;
    /// <summary>$90:C2C1 non-beam missile firing request, reached by charged selector thirteen.</summary>
    private const ushort MissileRequest = 0x0003;
    /// <summary>$90:C2C3 non-beam super-missile firing request, reached by charged selector fourteen.</summary>
    private const ushort SuperMissileRequest = 0x0004;
    /// <summary>The cartridge's explicit no-new-sound result.</summary>
    private const ushort NoSound = 0;

    /// <summary>
    /// Resolves the byte-sized library-one request word for one beam combination.
    /// Zero retains the cartridge's explicit no-new-sound result.
    /// </summary>
    public static ushort Resolve(bool charged, SamusBeamCombination combination) => combination switch
    {
        SamusBeamCombination.Power or SamusBeamCombination.Wave or SamusBeamCombination.Ice or
            SamusBeamCombination.IceWave or SamusBeamCombination.Spazer or SamusBeamCombination.SpazerWave or
            SamusBeamCombination.SpazerIce or SamusBeamCombination.SpazerIceWave or SamusBeamCombination.Plasma or
            SamusBeamCombination.PlasmaWave or SamusBeamCombination.PlasmaIce or SamusBeamCombination.PlasmaIceWave =>
            Authored(charged, combination),
        // The uncharged overread reaches the first charged beam family. The
        // charged overread reaches the non-beam no-sound/missile/super/no-sound row.
        SamusBeamCombination.SpazerPlasma => charged ? NoSound : Authored(true, SamusBeamCombination.Power),
        SamusBeamCombination.SpazerPlasmaWave => charged ? MissileRequest : Authored(true, SamusBeamCombination.Wave),
        SamusBeamCombination.SpazerPlasmaIce => charged ? SuperMissileRequest : Authored(true, SamusBeamCombination.Ice),
        SamusBeamCombination.SpazerPlasmaIceWave => charged ? NoSound : Authored(true, SamusBeamCombination.IceWave),
        _ => throw new ArgumentOutOfRangeException(nameof(combination), combination, "Undefined beam combination."),
    };

    // Library one reserves four adjacent sounds per beam family (Power, Spazer, Plasma),
    // followed by the same three families charged, twelve request IDs later.
    private static ushort Authored(bool charged, SamusBeamCombination combination)
    {
        int family = combination.HasPlasma ? 8 : combination.HasSpazer ? 4 : 0;
        int variant = (combination.HasIce, combination.HasWave) switch
        {
            (false, false) => 0,
            (true, false) => 1,
            (false, true) => family == 0 ? 2 : 3,
            (true, true) => family == 0 ? 3 : 2,
        };
        return (ushort)(UnchargedPowerRequest + family + variant + (charged ? 12 : 0));
    }
}
