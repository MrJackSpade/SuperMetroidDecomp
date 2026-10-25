namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Native bank-$A7 Kraid palette addresses and exact source lengths.</summary>
public static class KraidPaletteRomData
{
    /// <summary>$A7:AAA6, initial target colors staged at CGRAM palette eleven.</summary>
    public const int InitialTargetPaletteWords = 0xa7aaa6;

    /// <summary>Nine sixteen-color health bands, including the flash band.</summary>
    public const int HealthBandCount = 9;

    /// <summary>Colors in one native Kraid palette band.</summary>
    public const int BandColors = 16;


    /// <summary>
    /// Three individual sixteen-color bands and two nine-band health sequences.
    /// Exact native source extents are independently checked; no adjacent palette is included.
    /// </summary>
    /// <param name="source">The palette source whose complete editable color extent is required.</param>
    /// <returns>Sixteen RGB5 color words for a single band or 144 for a flash-plus-eight-health sequence; this count is not in bytes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The source is not one of the five supported palette identities.</exception>
    public static int ColorCount(KraidPaletteSource source) => source switch
    {
        KraidPaletteSource.RoomBackdrop or KraidPaletteSource.InitialTarget or
            KraidPaletteSource.DeathArm => BandColors,
        KraidPaletteSource.Health or KraidPaletteSource.Secondary =>
            HealthBandCount * BandColors,
        _ => throw new ArgumentOutOfRangeException(nameof(source)),
    };
}
