using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Installed RGB5 animation rows independent of cartridge storage and enemy mechanics.</summary>
public sealed class EnemyAuxiliaryColorCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => EnemyAuxiliaryColors.ContentIdentity(this);

    /// <summary>The mirrored eight-row face-block glow sequence.</summary>
    internal readonly EnemyAuxiliaryPaletteRows faceBlock;
    /// <summary>Sidehopper drain stages and its separate corpse colors.</summary>
    internal readonly EnemyAuxiliaryPaletteRows deadSidehopper;
    /// <summary>Golden Torizo's eight body health-band palettes.</summary>
    internal readonly EnemyAuxiliaryPaletteRows torizoBody;
    /// <summary>Golden Torizo's eight rear-belly health-band palettes.</summary>
    internal readonly EnemyAuxiliaryPaletteRows torizoBelly;
    /// <summary>Recognizes each supplied family's stock paint relationship while retaining unmatched edits.</summary>
    internal EnemyAuxiliaryColorCatalog(Dictionary<EnemyAuxiliaryPalette, RgbColor[][]> frames) =>
        (faceBlock, deadSidehopper, torizoBody, torizoBelly) =
            (new(frames[EnemyAuxiliaryPalette.FaceBlock], AuxiliaryPaint.FaceGlow),
                new(frames[EnemyAuxiliaryPalette.DeadSidehopper], AuxiliaryPaint.SidehopperDrain),
                new(frames[EnemyAuxiliaryPalette.GoldenTorizoBody], AuxiliaryPaint.TorizoFront),
                new(frames[EnemyAuxiliaryPalette.GoldenTorizoBelly], AuxiliaryPaint.TorizoRear));

    /// <summary>Resolves one selected RGB5 word from the face-block glow, sidehopper drain/corpse, or Golden Torizo health rows without advancing timers, choosing health bands, or writing CGRAM.</summary>
    /// <param name="palette">One of the four installed auxiliary-palette identities.</param>
    /// <param name="frame">Zero-based row: 0..7 for face-block or Torizo palettes, or 0..6 for sidehopper stages; rows are not necessarily elapsed animation frames.</param>
    /// <param name="color">Zero-based supplied color: 0..3 for face-block, 0..14 for sidehopper, or 0..15 for Torizo; sidehopper entries omit the native row's color zero.</param>
    /// <returns>The packed RGB5 word preserving independently supplied edits.</returns>
    public RgbColor Resolve(EnemyAuxiliaryPalette palette, int frame, int color) => EnemyAuxiliaryColors.Resolve(this, palette, frame, color);
}
