using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Where the cartridge keeps each ending palette role's colours (#1416: moved from Core's palette definitions and
/// logo palette pointer catalog, which no longer name a cartridge source).
/// </summary>
internal static class EndingPaletteSources
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
    /// The last colour of crossfade step <paramref name="step"/>'s background (palette 0) or sprite (palette 1) palette:
    /// for steps 0..15 the background traverses palettes $10..$1 while the sprite traverses $1..$10, and $8B:E58A copies
    /// each backwards from its last colour. Independently checked against all 32 native words for #1165.
    /// </summary>
    public static SnesAddress LogoCrossfadeSource(int step, int palette)
    {
        if ((uint)step >= EndingLogoDefinitions.PaletteSteps)
            throw new ArgumentOutOfRangeException(nameof(step));
        return palette switch
        {
            0 => EndingSourceDefinitions.Palettes.LogoCrossfadeFirstBackgroundEnd.AddWithinBank(-step * PaletteBytes),
            1 => EndingSourceDefinitions.Palettes.LogoCrossfadeFirstSpriteEnd.AddWithinBank(step * PaletteBytes),
            _ => throw new ArgumentOutOfRangeException(nameof(palette)),
        };
    }
}
