using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Installed RGB5 animation frames independent of stock storage and enemy mechanics.</summary>
public sealed class EnemyAuxiliaryColorCatalog
{
    /// <summary>
    /// Each family's colour frames: the mirrored eight-frame face-block glow sequence, the Sidehopper drain stages with its
    /// separate corpse colors, and Golden Torizo's eight body and eight rear-belly health bands.
    /// </summary>
    internal readonly IReadOnlyDictionary<EnemyAuxiliaryPalette, EnemyAuxiliaryColourFrames> families;

    /// <summary>Keeps each family's frames as the import recognized them.</summary>
    /// <param name="families">Each family's frames, as stock paint, an endpoint interpolation or retained edits.</param>
    internal EnemyAuxiliaryColorCatalog(IReadOnlyDictionary<EnemyAuxiliaryPalette, EnemyAuxiliaryColourFrames> families) =>
        this.families = families;

    /// <summary>Resolves one selected RGB5 word from the face-block glow, sidehopper drain/corpse, or Golden Torizo health bands without advancing timers, choosing health bands, or writing the palette.</summary>
    /// <param name="palette">One of the four installed auxiliary-palette identities.</param>
    /// <param name="frame">Zero-based frame: 0..7 for the face-block glow or Torizo health bands, or 0..6 for sidehopper stages; frames are not necessarily elapsed animation frames.</param>
    /// <param name="color">Zero-based supplied color: 0..3 for face-block, 0..14 for sidehopper, or 0..15 for Torizo; sidehopper entries omit the transparent color zero.</param>
    /// <returns>The packed RGB5 word preserving independently supplied edits.</returns>
    public RgbColor Resolve(EnemyAuxiliaryPalette palette, int frame, int color) => EnemyAuxiliaryColors.Resolve(this, palette, frame, color);
}
