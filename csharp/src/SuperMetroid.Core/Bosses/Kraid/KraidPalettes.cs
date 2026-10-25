namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The shapes of Kraid's five named palettes (#1413): sixteen-colour bands, and the two nine-band health sequences that
/// begin with the hurt flash. Where each palette sits in the cartridge is the import's knowledge
/// (<c>AssetExtraction/Bosses/KraidPaletteSources</c>).
/// </summary>
public static class KraidPalettes
{
    /// <summary>Nine sixteen-colour health bands, including the flash band.</summary>
    public const int HealthBandCount = 9;

    /// <summary>Colours in one Kraid palette band.</summary>
    public const int BandColors = 16;

    /// <summary>
    /// The colours of one named palette: a single band, or a flash-plus-eight-health sequence.
    /// </summary>
    /// <param name="source">The palette whose complete editable colour extent is required.</param>
    /// <returns>Sixteen colours for a single band or 144 for a flash-plus-eight-health sequence; this count is not in bytes.</returns>
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
