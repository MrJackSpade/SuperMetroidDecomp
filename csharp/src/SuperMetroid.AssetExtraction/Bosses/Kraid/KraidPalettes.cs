using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Kraid;

/// <summary>
/// The shapes of Kraid's five named palettes (#1413): sixteen-colour bands, and the two nine-band health sequences that
/// begin with the hurt flash. Where each palette sits in the original game is the import's knowledge
/// (<c>AssetExtraction/Bosses/KraidPaletteSources</c>).
/// </summary>
public static class KraidPalettes
{
    /// <summary>Nine sixteen-colour health bands, including the flash band.</summary>
    public const int HealthBandCount = 9;

    /// <summary>Colours in one Kraid palette band.</summary>
    public const int BandColors = 16;

    /// <summary>The first of a band's colours that are Kraid's eyes, which glow after a hit (#1512).</summary>
    public const int FirstEyeColour = 1;

    /// <summary>The hurt flash's band among a health sequence's nine: the first (#2297).</summary>
    public const int HurtFlashBand = 0;

    /// <summary>
    /// The shade each of a health sequence's nine bands is installed as, in the order the sequence holds them (#2297): the
    /// hurt flash's, then each health band's, most hurt first.
    /// </summary>
    public static IReadOnlyList<PictureShade> HealthShades { get; } =
        [KraidShades.HurtFlash, .. KraidHealthBandDefinitions.MostHurtFirst.Select(KraidShades.Health)];

    /// <summary>Health band <paramref name="band"/>'s place among a health sequence's nine bands: after the hurt flash's, most hurt first.</summary>
    /// <exception cref="ArgumentException">The band is not one of Kraid's.</exception>
    public static int BandOf(KraidHealthBand band)
    {
        Ensure.NotNull(band);
        for (int place = 0; place < KraidHealthBandDefinitions.MostHurtFirst.Count; place++)
        {
            if (ReferenceEquals(KraidHealthBandDefinitions.MostHurtFirst[place], band))
                return HurtFlashBand + 1 + place;
        }
        throw new ArgumentException($"{band} is not one of Kraid's health bands.", nameof(band));
    }

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
