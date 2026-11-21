using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>The installed RGB5 animation frames of the auxiliary enemy palettes, kept as installed, independent of enemy mechanics.</summary>
public sealed class EnemyAuxiliaryColorCatalog
{
    /// <summary>
    /// Each family's installed colour frames: the mirrored eight-frame face-block glow sequence, the Sidehopper drain
    /// stages with its corpse colors, and Golden Torizo's eight body and eight rear-belly health bands.
    /// </summary>
    private readonly Dictionary<EnemyAuxiliaryPalette, InstalledColourRows> families;

    /// <summary>Keeps every family's complete installed frames.</summary>
    /// <param name="families">Each family's frames, as installed.</param>
    internal EnemyAuxiliaryColorCatalog(Dictionary<EnemyAuxiliaryPalette, InstalledColourRows> families) => this.families = families;

    /// <summary>Resolves one installed RGB5 word from the face-block glow, sidehopper drain/corpse, or Golden Torizo health bands without advancing timers, choosing health bands, or writing the palette.</summary>
    /// <param name="palette">One of the four installed auxiliary-palette identities.</param>
    /// <param name="frame">Zero-based frame: 0..7 for the face-block glow or Torizo health bands, or 0..6 for sidehopper stages; frames are not necessarily elapsed animation frames.</param>
    /// <param name="color">Zero-based supplied color: 0..3 for face-block, 0..14 for sidehopper, or 0..15 for Torizo; sidehopper entries omit the transparent color zero.</param>
    /// <returns>The installed packed RGB5 word.</returns>
    public RgbColor Resolve(EnemyAuxiliaryPalette palette, int frame, int color) => Family(palette).Colour(frame, color);

    /// <summary>The installed frames of <paramref name="palette"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The palette is not one of the four installed families.</exception>
    internal InstalledColourRows Family(EnemyAuxiliaryPalette palette) => families.TryGetValue(palette, out InstalledColourRows? family)
        ? family
        : throw new ArgumentOutOfRangeException(nameof(palette));
}
