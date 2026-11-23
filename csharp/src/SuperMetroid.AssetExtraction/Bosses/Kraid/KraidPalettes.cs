using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Kraid;

/// <summary>
/// The shapes of Kraid's five named palettes (#1413): sixteen-colour bands, and the two nine-band health sequences that
/// begin with the hurt flash. Where each palette sits in the original game is the import's knowledge
/// (<c>AssetExtraction/Bosses/KraidPaletteSources</c>).
/// </summary>
public static partial class KraidPalettes
{
    /// <summary>Nine sixteen-colour health bands, including the flash band.</summary>
    public const int HealthBandCount = 9;

    /// <summary>Colours in one Kraid palette band.</summary>
    public const int BandColors = 16;

    /// <summary>The first of a band's colours that are Kraid's eyes, which glow after a hit (#1512).</summary>
    public const int FirstEyeColour = 1;

    /// <summary>
    /// Each of a health sequence's nine bands (#2297): the shade it is installed as, with the colour group of the sequence
    /// that holds its colours, the hurt flash's first and then each health band's, most hurt first.
    /// </summary>
    public static IReadOnlyList<KraidHealthSequenceBand> HealthSequence { get; } =
    [
        new(KraidShades.HurtFlash, 0),
        new(KraidShades.Health(KraidHealthBandDefinitions.BelowQuarter), 1),
        new(KraidShades.Health(KraidHealthBandDefinitions.BelowThreeEighths), 2),
        new(KraidShades.Health(KraidHealthBandDefinitions.BelowHalf), 3),
        new(KraidShades.Health(KraidHealthBandDefinitions.BelowFiveEighths), 4),
        new(KraidShades.Health(KraidHealthBandDefinitions.BelowThreeQuarters), 5),
        new(KraidShades.Health(KraidHealthBandDefinitions.BelowSevenEighths), 6),
        new(KraidShades.Health(KraidHealthBandDefinitions.BelowWhole), 7),
        new(KraidShades.Health(KraidHealthBandDefinitions.Unhurt), 8),
    ];

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
