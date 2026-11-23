using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Where the cartridge keeps each ending palette role's colours (#1416: moved from Core's palette definitions and
/// logo palette pointer catalog, which no longer name a cartridge source).
/// </summary>
internal static partial class EndingPaletteSources
{
    /// <summary>The bytes of one sixteen-colour palette.</summary>
    private const int PaletteBytes = 16 * sizeof(ushort);

    /// <summary>The contiguous source of a palette role. The crossfade has two interleaved sources per step and no single source.</summary>
    public static int Source(EndingPaletteId id) => id switch
    {
        EndingPaletteId.Escape => EndingSourceDefinitions.Palettes.Escape,
        EndingPaletteId.PostCredits => EndingSourceDefinitions.Palettes.PostCredits,
        EndingPaletteId.Credits => EndingSourceDefinitions.Palettes.Credits,
        EndingPaletteId.Explosion => EndingSourceDefinitions.Palettes.Explosion,
        EndingPaletteId.FinalGunship => EndingSourceDefinitions.Palettes.FinalGunship,
        EndingPaletteId.LogoInitial => EndingSourceDefinitions.Palettes.LogoInitial,
        EndingPaletteId.LogoCrossfade => throw new ArgumentOutOfRangeException(nameof(id)),
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    /// <summary>
    /// The last colour of crossfade step <paramref name="step"/>'s background or sprite palette (<paramref name="layer"/>):
    /// for steps 0..15 the background traverses palettes $10..$1 while the sprite traverses $1..$10, and $8B:E58A copies
    /// each backwards from its last colour. Independently checked against all 32 native words for #1165.
    /// </summary>
    internal static SnesAddress LogoCrossfadeSource(int step, EndingLogoLayer layer)
    {
        if ((uint)step >= EndingLogoDefinitions.CrossfadeSteps)
            throw new ArgumentOutOfRangeException(nameof(step));
        return layer switch
        {
            EndingLogoLayer.Background => EndingSourceDefinitions.Palettes.LogoCrossfadeFirstBackgroundEnd.AddWithinBank(-step * PaletteBytes),
            EndingLogoLayer.Sprites => EndingSourceDefinitions.Palettes.LogoCrossfadeFirstSpriteEnd.AddWithinBank(step * PaletteBytes),
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Undefined ending logo layer."),
        };
    }

    /// <summary>The logo's layers in the order the installed crossfade lists each step's palettes: the background's, then the sprites'.</summary>
    internal static IReadOnlyList<EndingLogoLayer> CrossfadeLayers { get; } = [EndingLogoLayer.Background, EndingLogoLayer.Sprites];
}
