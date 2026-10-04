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
    /// <summary>The complete raw low-nibble selector domain accepted by the cartridge.</summary>
    public const int SelectorCount = 16;

    /// <summary>
    /// Resolves the byte-sized library-one request word for one raw beam combination.
    /// Zero retains the cartridge's explicit no-new-sound result.
    /// </summary>
    public static ushort Resolve(bool charged, int beamCombination)
    {
        if ((uint)beamCombination >= SelectorCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(beamCombination), beamCombination,
                "Beam sound routing accepts the cartridge's four-bit selector domain.");
        }

        // The uncharged overread reaches the first charged beam family. The
        // charged overread reaches the non-beam no-sound/missile/super/no-sound row.
        if (beamCombination >= 12)
            return charged ? (beamCombination - 12) switch { 1 => MissileRequest, 2 => SuperMissileRequest, _ => (ushort)0 }
                : Resolve(true, beamCombination - 12);

        SamusBeamFlags elements = (SamusBeamFlags)beamCombination;
        int family = beamCombination & (int)(SamusBeamFlags.Spazer | SamusBeamFlags.Plasma);
        int variant = (elements & (SamusBeamFlags.Ice | SamusBeamFlags.Wave)) switch
        {
            SamusBeamFlags.None => 0,
            SamusBeamFlags.Ice => 1,
            SamusBeamFlags.Wave => family == 0 ? 2 : 3,
            _ => family == 0 ? 3 : 2,
        };
        // Library one reserves four adjacent sounds per beam family, followed
        // by the same three families charged, twelve request IDs later.
        return (ushort)(UnchargedPowerRequest + family + variant + (charged ? 12 : 0));
    }
}
