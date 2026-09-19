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
    private static ReadOnlySpan<ushort> UnchargedSounds =>
    [
        0x000b, 0x000d, 0x000c, 0x000e,
        0x000f, 0x0012, 0x0010, 0x0011,
        0x0013, 0x0016, 0x0014, 0x0015,
        0x0017, 0x0019, 0x0018, 0x001a,
    ];

    private static ReadOnlySpan<ushort> ChargedSounds =>
    [
        0x0017, 0x0019, 0x0018, 0x001a,
        0x001b, 0x001e, 0x001c, 0x001d,
        0x001f, 0x0022, 0x0020, 0x0021,
        0x0000, 0x0003, 0x0004, 0x0000,
    ];

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

        return (charged ? ChargedSounds : UnchargedSounds)[beamCombination];
    }
}
